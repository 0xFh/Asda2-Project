using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Paths;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Taxi
{
    /// <summary>
    /// 
    /// TODO: Cancel flight
    /// TODO: Save Character's route to DB
    /// 
    /// Static helper and srcCont class for Taxi-related information (Flight-Paths, Flight-Masters etc)
    /// </summary>
    public static class TaxiMgr
    {
        /// <summary>
        /// The delay in millis between position updates of Units that are on Taxis.
        /// </summary>
        [Variable("TaxiInterpolationMillis")] public static int InterpolationDelayMillis = 800;

        private static int airSpeed = 32;
        [NotVariable] public static PathNode[] PathNodesById = new PathNode[340];
        [NotVariable] public static TaxiPath[] PathsById = new TaxiPath[1200];

        /// <summary>A TaxiNode Mask with all existing nodes activated.</summary>
        public static TaxiNodeMask AllActiveMask = new TaxiNodeMask();

        public static MappedDBCReader<PathVertex, DBCTaxiPathNodeConverter> TaxiVertexReader;

        /// <summary>
        /// The speed of Units travelling on Taxis in yards/second - Default: 16.
        /// (The average speed on foot is 7 y/s)
        /// </summary>
        [Variable("TaxiAirSpeed")]
        public static int AirSpeed
        {
            get { return TaxiMgr.airSpeed; }
            set { TaxiMgr.airSpeed = value; }
        }

        public static void Initialize()
        {
        }

        public static PathNode GetNode(uint id)
        {
            return TaxiMgr.PathNodesById.Get<PathNode>(id);
        }

        /// <summary>
        /// Returns the TaxiNode closest to the given position (within 10 yards)
        /// </summary>
        /// <param name="pos">A position given in world coordinates</param>
        /// <returns>The closest TaxiNode within 10 yards, or null.</returns>
        public static PathNode GetNearestTaxiNode(Vector3 pos)
        {
            PathNode pathNode1 = (PathNode) null;
            float num1 = float.MaxValue;
            foreach (PathNode pathNode2 in TaxiMgr.PathNodesById)
            {
                if (pathNode2 != null)
                {
                    float num2 = pathNode2.Position.DistanceSquared(ref pos);
                    if ((double) num2 < (double) num1)
                    {
                        num1 = num2;
                        pathNode1 = pathNode2;
                    }
                }
            }

            return pathNode1;
        }

        public static PathVertex GetVertex(int id)
        {
            PathVertex pathVertex;
            TaxiMgr.TaxiVertexReader.Entries.TryGetValue(id, out pathVertex);
            return pathVertex;
        }

        /// <summary>Sends the given Character on the given Path.</summary>
        /// <param name="chr">The Character to fly around.</param>
        /// <param name="destinations">An array of destination TaxiNodes.</param>
        /// <returns>Whether the client was sent on its way.</returns>
        internal static bool TryFly(Character chr, NPC vendor, PathNode[] destinations)
        {
            IRealmClient client = chr.Client;
            if (vendor == null && chr.Role.IsStaff)
            {
                PathNode pathNode = ((IEnumerable<PathNode>) destinations).LastOrDefault<PathNode>();
                if (pathNode == null)
                    return false;
                chr.TeleportTo((IWorldLocation) pathNode);
                return true;
            }

            if (vendor == null || !vendor.CheckVendorInteraction(chr))
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.NotAvailable);
            else if (TaxiMgr.PreFlightCheatChecks(client, destinations) &&
                     TaxiMgr.PreFlightValidPathCheck(client, destinations) &&
                     (client.ActiveCharacter.GodMode || TaxiMgr.PreFlightMoneyCheck(client)))
            {
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.Ok);
                chr.UpdatePvPState(false, true);
                TaxiMgr.FlyUnit((Unit) chr, true);
                return true;
            }

            return false;
        }

        /// <summary>Check various character states that disallow flights.</summary>
        /// <param name="client">The IRealmClient requesting the flight.</param>
        /// <param name="destinations">An array of destination TaxiNodes.</param>
        /// <returns>True if flight allowed.</returns>
        private static bool PreFlightCheatChecks(IRealmClient client, PathNode[] destinations)
        {
            Character activeCharacter = client.ActiveCharacter;
            PathNode destination = destinations[0];
            if (destinations.Length < 2)
                return false;
            if (activeCharacter.IsMounted)
            {
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.PlayerAlreadyMounted);
                return false;
            }

            if (activeCharacter.ShapeshiftForm != ShapeshiftForm.Normal &&
                activeCharacter.ShapeshiftForm != ShapeshiftForm.BattleStance &&
                (activeCharacter.ShapeshiftForm != ShapeshiftForm.BerserkerStance &&
                 activeCharacter.ShapeshiftForm != ShapeshiftForm.DefensiveStance) &&
                activeCharacter.ShapeshiftForm != ShapeshiftForm.Shadow)
            {
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.PlayerShapeShifted);
                return false;
            }

            if (activeCharacter.IsLoggingOut)
            {
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.PlayerShapeShifted);
                return false;
            }

            if (!activeCharacter.CanMove)
            {
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.PlayerMoving);
                return false;
            }

            if (activeCharacter.IsUsingSpell)
            {
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.PlayerBusy);
                return false;
            }

            if (destination.MapId != activeCharacter.Map.Id)
            {
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.NoPathNearby);
                return false;
            }

            if (activeCharacter.TradeWindow == null)
                return true;
            TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.PlayerBusy);
            return false;
        }

        /// <summary>
        /// Check that a valid path exists between the destinations.
        /// Also sets the characters TaxiPaths queue with the sequence of valid
        /// paths to the final destination.
        /// </summary>
        /// <param name="client">The IRealmClient requesting the flight.</param>
        /// <param name="destinations">An array of destination TaxiNodes.</param>
        /// <returns>True if a valid path exists.</returns>
        private static bool PreFlightValidPathCheck(IRealmClient client, PathNode[] destinations)
        {
            Character activeCharacter = client.ActiveCharacter;
            activeCharacter.TaxiPaths.Clear();
            for (uint index = 0; (long) index < (long) (destinations.Length - 1); ++index)
            {
                TaxiPath pathTo = destinations[index].GetPathTo(destinations[index + 1U]);
                if (pathTo == null)
                {
                    activeCharacter.TaxiPaths.Clear();
                    TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.InvalidChoice);
                    return false;
                }

                activeCharacter.TaxiPaths.Enqueue(pathTo);
            }

            return true;
        }

        /// <summary>
        /// Check that the character has enough money to cover the cost of the flight(s).
        /// Also deducts the cost of the flight from the character.
        /// </summary>
        /// <param name="client">The IRealmClient requesting the flight.</param>
        /// <returns>An array of destination TaxiNodes.</returns>
        private static bool PreFlightMoneyCheck(IRealmClient client)
        {
            Character activeCharacter = client.ActiveCharacter;
            uint amount = 0;
            foreach (TaxiPath taxiPath in activeCharacter.TaxiPaths)
            {
                if (taxiPath != null)
                {
                    amount += taxiPath.Price;
                }
                else
                {
                    activeCharacter.TaxiPaths.Clear();
                    TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.InvalidChoice);
                    return false;
                }
            }

            if (activeCharacter.Money < amount)
            {
                activeCharacter.TaxiPaths.Clear();
                TaxiHandler.SendActivateTaxiReply((IPacketReceiver) client, TaxiActivateResponse.InsufficientFunds);
                return false;
            }

            client.ActiveCharacter.SubtractMoney(amount);
            client.ActiveCharacter.Achievements.CheckPossibleAchievementUpdates(
                AchievementCriteriaType.GoldSpentForTravelling, amount, 0U, (Unit) null);
            client.ActiveCharacter.Achievements.CheckPossibleAchievementUpdates(
                AchievementCriteriaType.FlightPathsTaken, 1U, 0U, (Unit) null);
            return true;
        }

        /// <summary>
        /// Client-side taxi interpolation gets fishy when exceeding certain speed limits
        /// </summary>
        internal static bool IsNormalSpeed
        {
            get { return TaxiMgr.AirSpeed <= 32; }
        }

        /// <summary>Send character down the next leg of a multi-hop trip.</summary>
        internal static void ContinueFlight(Unit unit)
        {
            if (unit.LatestTaxiPathNode == null)
                return;
            PathVertex pathVertex = unit.LatestTaxiPathNode.Value;
            PathNode to = pathVertex.Path.To;
            if (unit.m_TaxiMovementTimer.IsRunning && !unit.IsInRadius(to.Position, (float) TaxiMgr.AirSpeed))
                return;
            bool flag = false;
            if (unit.TaxiPaths.Count < 2)
            {
                flag = true;
            }
            else
            {
                if (unit.TaxiPaths.Dequeue().To != to)
                {
                    unit.CancelTaxiFlight();
                    return;
                }

                TaxiPath taxiPath = unit.TaxiPaths.Peek();
                if (to != taxiPath.From)
                {
                    unit.CancelTaxiFlight();
                    return;
                }
            }

            if (!flag)
            {
                TaxiMgr.FlyUnit(unit, false);
            }
            else
            {
                if (TaxiMgr.IsNormalSpeed)
                    unit.Map.MoveObject((WorldObject) unit, pathVertex.Pos);
                else
                    unit.TeleportTo(pathVertex.Pos);
                unit.CancelTaxiFlight();
            }
        }

        public static void FlyUnit(Unit chr, bool startFlight)
        {
            TaxiMgr.FlyUnit(chr, startFlight, (LinkedListNode<PathVertex>) null);
        }

        public static void FlyUnit(Unit unit, bool startFlight, LinkedListNode<PathVertex> startNode)
        {
            if (unit.TaxiPaths.Count < 1)
                throw new InvalidOperationException("Tried to fly Unit without Path given.");
            TaxiPath taxiPath = unit.TaxiPaths.Peek();
            unit.IsInCombat = false;
            unit.Stealthed = 0;
            if (startFlight)
            {
                NPCEntry entry = NPCMgr.GetEntry(unit.Faction.IsAlliance
                    ? taxiPath.From.AllianceMountId
                    : taxiPath.From.HordeMountId);
                if (entry != null)
                {
                    uint displayId = entry.GetRandomModel().DisplayId;
                    unit.Mount(displayId);
                    if (unit is Character)
                        unit.PushFieldUpdateToPlayer((Character) unit, (UpdateFieldId) UnitFields.MOUNTDISPLAYID,
                            displayId);
                }

                unit.OnTaxiStart();
            }

            unit.LatestTaxiPathNode = startNode ?? taxiPath.Nodes.First;
            if (unit.LatestTaxiPathNode == taxiPath.Nodes.First)
            {
                unit.taxiTime = 0;
                MovementHandler.SendMoveToPacket<PathVertex>(unit, taxiPath.PathTime,
                    MonsterMoveFlags.Flag_0x2000_FullPoints_1, (IEnumerable<PathVertex>) taxiPath.Nodes);
            }
            else
            {
                unit.taxiTime = startNode.Previous.Value.TimeFromStart +
                                (int) (1000.0 * (double) startNode.Value.Pos.GetDistance(unit.Position) /
                                       (double) TaxiMgr.AirSpeed);
                MovementHandler.SendMoveToPacket<PathVertex>(unit, TaxiMgr.AirSpeed,
                    MonsterMoveFlags.Flag_0x2000_FullPoints_1, startNode);
            }
        }

        /// <summary>
        /// Interpolates the position of the given Unit along the Path given the elapsed flight time.
        /// </summary>
        /// <param name="elapsedTime">Time that elapsed since the given unit passed by the last PathVertex</param>
        internal static void InterpolatePosition(Unit unit, int elapsedTime)
        {
            LinkedListNode<PathVertex> linkedListNode = unit.LatestTaxiPathNode;
            unit.taxiTime += elapsedTime;
            if (linkedListNode.Next == null)
            {
                unit.CancelTaxiFlight();
            }
            else
            {
                while (linkedListNode.Next.Value.TimeFromStart <= unit.taxiTime)
                {
                    linkedListNode = linkedListNode.Next;
                    unit.LatestTaxiPathNode = linkedListNode;
                    if (linkedListNode.Next == null)
                    {
                        if (TaxiMgr.IsNormalSpeed)
                        {
                            unit.m_TaxiMovementTimer.Stop();
                            return;
                        }

                        TaxiMgr.ContinueFlight(unit);
                        return;
                    }
                }

                PathVertex pathVertex1 = linkedListNode.Value;
                PathVertex pathVertex2 = linkedListNode.Next.Value;
                int num = unit.taxiTime - linkedListNode.Value.TimeFromStart;
                Vector3 newPos = pathVertex1.Pos + (pathVertex2.Pos - pathVertex1.Pos) * (float) num /
                                 (float) pathVertex2.TimeFromPrevious;
                unit.Map.MoveObject((WorldObject) unit, ref newPos);
            }
        }
    }
}