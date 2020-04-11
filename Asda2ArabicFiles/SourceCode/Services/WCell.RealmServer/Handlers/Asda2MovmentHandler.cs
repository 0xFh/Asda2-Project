using System;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Asda2BattleGround;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Handlers
{
    class Asda2MovmentHandler
    {
        public static void SendMonstMoveOrAtackResponse(Int16 sessIdTarget, NPC movingNpc, Int32 dmg, Vector3 toPos, bool isAtack)
        {
            if(!movingNpc.IsAreaActive)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.MonstMove)) //4015
            {
                packet.WriteInt16(sessIdTarget); //{sessIdTarget}default value : 29 Len : 2
                packet.WriteInt16((short) movingNpc.Entry.NPCId);
               // packet.WriteSkip(Stab8); //value name : stab8 default value : stab8Len : 2
                packet.WriteInt16(movingNpc.UniqIdOnMap); //{monstrId}default value : 129 Len : 2
                packet.WriteByte(isAtack ? 3 : movingNpc.Movement.MoveType == Constants.NPCs.AIMoveType.Walk ? 2 : 5); //{setPositionMode}default value : 2 Len : 1 2 - run 1 - instant 3 - atack
                packet.WriteInt16((short) movingNpc.Asda2X); //{xStart}default value : 91 Len : 2
                packet.WriteInt16((short) movingNpc.Asda2Y); //{yStart}default value : 255 Len : 2
                packet.WriteInt16(isAtack ? 0 : (short) toPos.X); //{xStop}default value : 0 Len : 2
                packet.WriteInt16(isAtack ? 0 : (short) toPos.Y); //{yStop}default value : 0 Len : 2
                var speed = movingNpc.Movement.MoveType == Constants.NPCs.AIMoveType.Walk ? movingNpc.WalkSpeed : movingNpc.RunSpeed;
                packet.WriteInt16(isAtack ? 0 : (short) (1000f/speed));
                //{movingSpeed}default value : 0 Len : 2
                //Todo Asda2 calc animation speed
                packet.WriteInt16(isAtack ? 0 : 10000); //{animationSpeed}default value : 0 Len : 2
                packet.WriteInt16(10000); //{animationSpeedOfAtack}default value : 10000 Len : 2
                packet.WriteInt32(dmg); //{dmg}default value : -2 Len : 4
                packet.WriteInt16(movingNpc.Health); //{hp}default value : 10 Len : 2
                packet.WriteInt16(movingNpc.Health <= 0 ? -1 : 0);
                //{isAlive}default value : 0 Len : 2 monstr dead = -1; alive = 0
                packet.WriteSkip(Stab35); //value name : stab35 default value : stab35Len : 8
                movingNpc.SendPacketToArea(packet,false,true);
            }
        }

        private static readonly byte[] Stab8 = new byte[] { 0x01, 0x00 };
        private static readonly byte[] Stab35 = new byte[] { 0x02, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00 };
        [PacketHandler(RealmServerOpCode.EndMove)]//4008
        public static void EndMoveRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 3;//nk6 default : 1000Len : 8
            var x = packet.ReadFloat();//default : 1192383353Len : 4
            var y = packet.ReadFloat();//default : 1195875328Len : 4
            var x0 = packet.ReadFloat();//default : 1192383353Len : 4
            var y0 = packet.ReadFloat();//default : 1195875328Len : 4
            //Util.NLog.LogUtil.WarnException("{0} {1} {2} {3}",x,y,x0,y0);
            packet.Position += 4;
            var target = packet.ReadInt16();
            client.ActiveCharacter.IsMoving = false;
            SendEndMoveCommonResponse(client,x,y,x0,y0,target);
            /*  try
            {
            //(byte;70)z(single;0)x(single;1)y(single;1)
           /* packet.ReadByte();
            packet.ReadSingle();
            var x = packet.ReadSingle();
            var y = packet.ReadSingle();#1#
            var chr = client.ActiveCharacter;
                if(chr.IsMoving)
                {
                    chr.IsMoving = false;
                    //SendEndMoveCommonResponse(chr);
                    return;
                }
            if ((chr.EndMoveCount+1) % 6 == 0)
            {
                SendStartMoveCommonToOneClienResponset(chr, chr.Client,true);
            }
                if (chr.EndMoveCount == 40)
            {
                //chr.TeleportToBindLocation();
                Asda2CharacterHandler.SendResurectResponse(chr);
            }
            chr.EndMoveCount++;
            }
            catch (System.IO.EndOfStreamException) { }*/
        }


        public static void SendEndMoveCommonResponse(IRealmClient client,float x,float y,float x1,float y1,short target)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.EndMoveCommon))//4009
            {
                packet.WriteByte(0);//{status}default value : 0 Len : 1
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 4 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : -968818636 Len : 4
                packet.WriteInt16(0);//value name : unk2 default value : 0Len : 2
                packet.WriteFloat(x);//{x}default value : 9195,591 Len : 4
                packet.WriteFloat(y);//{y}default value : 39978,21 Len : 4
                packet.WriteFloat(x1);//{x1}default value : 9195,591 Len : 4
                packet.WriteFloat(y1);//{y1}default value : 39978,21 Len : 4
                packet.WriteInt32(0);//value name : unk13 default value : 0Len : 4
                packet.WriteInt16(target);
                client.Send(packet);
            }
        }

        public static void SendEndMoveByFastInstantRegularMoveResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.StartMoveCommon))//4007
            {
                packet.WriteByte(1); //value name : _
                packet.WriteInt16(chr.SessionId); //default value : 0
                packet.WriteInt32(chr.Account.AccountId);//{accId}default value : 0 Len : 4
                packet.WriteInt16(2); //value name : _
                packet.WriteFloat(chr.Asda2X*100); //default value : 1
                packet.WriteFloat(chr.Asda2Y*100); //default value : 1
                packet.WriteFloat(chr.Asda2X * 100); //default value : 1
                packet.WriteFloat(chr.Asda2Y * 100); //default value : 1
                packet.WriteFloat(5); //default value : 1
                packet.WriteInt16(-1);
                chr.SendPacketToArea(packet, true,true);
            }
        }

        
        [PacketHandler(RealmServerOpCode.StartMove)]//4006
        public static void StartMoveRequest(IRealmClient client, RealmPacketIn packet)
        {
            if(!client.ActiveCharacter.CanMove)
            {
                SendStartComonMovePacketError(client.ActiveCharacter, false, Asda2StartMovementStatus.CantMoveInThisCondition);
                return;
            }
            if (client.ActiveCharacter.IsAggred)
            {
                if (client.ActiveCharacter.ArggredDateTime < DateTime.Now)
                {
                    client.ActiveCharacter.IsAggred = false;
                    client.ActiveCharacter.UpdateSpeedFactor();
                }
                else
                {
                    client.ActiveCharacter.SendInfoMsg("You are aggred.");
                    SendStartComonMovePacketError(client.ActiveCharacter, false, Asda2StartMovementStatus.CantMoveInThisCondition);
                    return;
                }
            }
            packet.Position -= 24;// default : 70
            Single x;
            Single y;
            Single x1;
            Single y1;
            short targetId; 
            try
            {
                packet.Position += 16;//default : md5Len : 16
                packet.Position += 5;//tab22 default : stab22Len : 5
                x = packet.ReadSingle()/100;//default : 8574,58Len : 4
                y = packet.ReadSingle()/100;//default : 17935,74Len : 4
                x1 = packet.ReadSingle() / 100;//default : 8236,86Len : 4
                y1 = packet.ReadSingle() / 100;//default : 18713,6Len : 4
                packet.Position += 4;//nk10 default : 0Len : 4
                targetId = packet.ReadInt16();//default : 713Len : 2                
            }
            catch(System.IO.EndOfStreamException)
            {
                return;
            }
            var chr = client.ActiveCharacter;
            
            if (chr.IsAsda2BattlegroundInProgress &&
                (!chr.CurrentBattleGround.IsStarted &&
                 new Vector3(chr.Map.Offset + x1, chr.Map.Offset+y1, 0).GetDistance(chr.CurrentBattleGround.GetBasePosition(chr)) > 40 ||
                 new Vector3(chr.Map.Offset + x1, chr.Map.Offset+y1, 0).GetDistance(chr.CurrentBattleGround.GetForeigLocation(chr)) < 40))
            {
                SendStartComonMovePacketError(client.ActiveCharacter, false,
                                              Asda2StartMovementStatus.YouCantMoveToOtherSideOfRevivalArea);
                return;
            }
            if (chr.IsFirstMoveAfterAtack)
            {
                chr.IsFirstMoveAfterAtack = false;
               // return;
            }
            if (chr.Target is NPC && chr.Target.IsDead)
            {
                chr.Target = null;
                chr.IsFighting = false;
            }
            if(targetId == -1)
            {
                chr.Target = null;
            }
            else
            {
                if(chr.Target == null || !(chr.Target is Character))
                    chr.Target = chr.Map.GetNpcByUniqMapId((ushort)targetId);
            }
            if(chr.Target == null || !(chr.Target is NPC))
                chr.IsFighting = false;
            chr.EndMoveCount = 0;
            
            if(chr.IsFighting && chr.MainWeapon.IsRanged && chr.Target!=null)
                if (new Vector3(x1, y1, 0).GetDistance(chr.Target.Asda2Position) < 3)
                {
                    SendStartMoveCommonToAreaResponse(chr,false);
                    return;
                }
            OnMoveRequest(client, y1, x1);
        }

        public static void OnMoveRequest(IRealmClient client, float y, float x)
        {
            var newPos = new Vector3(x, y, 0);
            var chr = client.ActiveCharacter;
            if(chr.Asda2X == x && chr.Asda2Y == y)
                return;

            
            var mover = chr.MoveControl.Mover as Unit;

            if (mover == null ||
                !mover.UnitFlags.HasFlag(UnitFlags.PlayerControlled) ||
                mover.UnitFlags.HasFlag(UnitFlags.Influenced) || chr.IsDead)
            {
                // don't accept Player input while not under the Player's control
                return;
            }
            if (chr.CurrentMovingVector != Vector2.Zero) //check real coord
                CalculateAndSetRealPos(chr, 0);
            mover.CancelEmote();
            chr.LastNewPosition = newPos;
            chr.CurrentMovingVector = newPos.XY - chr.Asda2Position.XY;
            chr.IsMoving = true;
            SendStartMoveCommonToAreaResponse(chr, false);
        }

        public static void MoveToSelectedTargetAndAttack(Character chr)
        {
            if(chr.Target==null)
                return;
            var xy = chr.Target.Asda2Position.XY;
            var newPos = new Vector3(xy.X, xy.Y, 0);
            chr.LastNewPosition = newPos;
            chr.CurrentMovingVector = newPos.XY - chr.Asda2Position.XY;
            chr.IsMoving = true;
            SendStartMoveCommonToAreaResponse(chr, false);
        }
        [NotVariable] private const int DefaultMoveUpdateTime = 3000;


        public static void CalculateAndSetRealPos(Character chr, int dt)
        {
            if (!chr.IsMoving)
            {
                chr.CurrentMovingVector = Vector2.Zero;
                return;
            }
            SetPosition(chr);
            chr.TimeFromLastPositionUpdate += dt;
            if(chr.TimeFromLastPositionUpdate>DefaultMoveUpdateTime)
            {
                chr.TimeFromLastPositionUpdate = 0;
                NotifyMoving(chr);
            }
        }

        private static void SetPosition(Character chr)
        {
            var pathDist = chr.CurrentMovingVector.Length();
            var now = Environment.TickCount;
            var timeFromLastMove = now - chr.LastMoveTime;
            var avalibleDist = chr.RunSpeed*timeFromLastMove/100;
            chr.OnMove();
            if (avalibleDist >= pathDist)
            {
                chr.SetPosition(new Vector3(chr.LastNewPosition.X + chr.Map.Offset, chr.LastNewPosition.Y + chr.Map.Offset));
                chr.CurrentMovingVector = new Vector2(0, 0);
                chr.IsMoving = false;
                //chr.SendSystemMessage("End moving on {0},{1}", chr.Position.X, chr.Position.Y);
            }
            else
            {
                /*var cos = chr.CurrentMovingVector.X/chr.CurrentMovingVector.Length();
                var angle = Math.Acos(cos);
                var difX = avalibleDist*Math.Cos(angle);
                var difY = avalibleDist*Math.Sin(angle);
                var newX = Convert.ToSingle(chr.Position.X + difX);
                var newY = Convert.ToSingle(chr.Position.Y + (chr.CurrentMovingVector.Y < 0 ? -difY : difY));*/
                var realPos = new Vector3(FindRealPosition(chr.CurrentMovingVector, avalibleDist, chr.Asda2Position.XY), 0);
                //newX, newY, 0);
                chr.SetPosition(new Vector3(realPos.X + chr.Map.Offset, realPos.Y + chr.Map.Offset));
                //chr.SendSystemMessage("seting real pos {0},{1}", chr.Position.X, chr.Position.Y);
                chr.CurrentMovingVector = chr.LastNewPosition.XY - chr.Asda2Position.XY;
            }
        }

        private static void NotifyMoving(Character chr)
        {
            
                                              if (chr.IsInGroup)
                                                  Asda2GroupHandler.SendPartyMemberPositionInfoResponse(chr);
                                              if (chr.IsSoulmated)
                                                  Asda2SoulmateHandler.SendSoulmatePositionResponse(chr.Client);
                                              if (chr.IsAsda2BattlegroundInProgress)
                                                  Asda2BattlegroundHandler.SendCharacterPositionInfoOnWarResponse(chr);
                                        
        }

        static Vector2 FindRealPosition(Vector2 movingVector, float distance,Vector2 startPosition)
        {
            var cos = movingVector.X / movingVector.Length();
            var angle = Math.Acos(cos);
            var difX = distance * Math.Cos(angle);
            var difY = distance * Math.Sin(angle);
            var newX = Convert.ToSingle(startPosition.X + difX);
            var newY = Convert.ToSingle(startPosition.Y + (movingVector.Y < 0 ? -difY : difY));
            return new Vector2(newX,newY);
        }

        public static void SendStartMoveCommonToAreaResponse(Character chr, bool instant,bool includeSelf = true)
        {
            using (var packet = CreateStartComonMovePacket(chr,instant))
                chr.SendPacketToArea(packet, includeSelf);

        }
        public static void SendStartMoveCommonToOneClienResponset(Character movingChr, IRealmClient recievingClient,bool instant)
        {
            using (var packet = CreateStartComonMovePacket(movingChr, instant))
               recievingClient.Send(packet, addEnd: true);
        }

        public static RealmPacketOut CreateStartComonMovePacket(Character chr, bool instant)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.StartMoveCommon);//4007
            packet.WriteByte(1); //value name : _
            packet.WriteInt16(chr.SessionId); //default value : 0
            packet.WriteInt32(chr.Account.AccountId);//{accId}default value : 0 Len : 4
            packet.WriteInt16(2); //value name : _
            packet.WriteFloat(chr.Asda2X*100); //default value : 1
            packet.WriteFloat(chr.Asda2Y*100); //default value : 1
            packet.WriteFloat(instant?chr.Asda2X*100:chr.LastNewPosition.X*100); //default value : 1
            packet.WriteFloat(instant ? chr.Asda2Y * 100 : chr.LastNewPosition.Y * 100); //default value : 1
            packet.WriteFloat(instant?5 : chr.RunSpeed); //default value : 1
            var target = chr.Target as NPC;
            packet.WriteInt16(target == null?-1:target.UniqIdOnMap);
            return packet;
        }
        public static void SendStartComonMovePacketError(Character chr, bool instant,Asda2StartMovementStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.StartMoveCommon)) //4007
            {
                packet.WriteByte((byte) status); //value name : _
                packet.WriteInt16(chr.SessionId); //default value : 0
                packet.WriteInt32((int) chr.Account.AccountId); //{accId}default value : 0 Len : 4
                packet.WriteInt16(2); //value name : _
                packet.WriteFloat(chr.Asda2X*100); //default value : 1
                packet.WriteFloat(chr.Asda2Y*100); //default value : 1
                packet.WriteFloat(instant ? chr.Asda2X*100 : chr.LastNewPosition.X*100); //default value : 1
                packet.WriteFloat(instant ? chr.Asda2Y*100 : chr.LastNewPosition.Y*100); //default value : 1
                packet.WriteFloat(instant ? 5 : chr.RunSpeed); //default value : 1
                var target = chr.Target as NPC;
                packet.WriteInt16(target == null ? 0 : target.UniqIdOnMap);
                chr.Send(packet, addEnd: true);
            }
        }

    }
   
    internal enum Asda2StartMovementStatus
    {
        Ok =1,
        UnavalibleArea =2,
        CantMoveSoFar =3,
        InstantTeleport = 5,
        CantMoveInThisCondition =6,
        WeightLimitHiger90YouCantFigth =7,
        CantMoveBeforeWarStarted =8,
        YouCantMoveToOtherSideOfRevivalArea =9
    }
}
