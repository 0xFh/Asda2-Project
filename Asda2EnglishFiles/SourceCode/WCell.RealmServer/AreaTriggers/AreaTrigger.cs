using NLog;
using System;
using WCell.Constants.AreaTriggers;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AreaTriggers
{
    /// <summary>
    /// AreaTriggers are invisible areas ingame that are always known by the client.
    /// An AreaTrigger is triggered when a Character steps on it.
    /// </summary>
    public class AreaTrigger
    {
        private const float tollerance = 55f;
        public readonly uint Id;
        public readonly AreaTriggerId ATId;
        public readonly MapId MapId;
        public Vector3 Position;
        public readonly float Radius;
        public readonly float BoxLength;
        public readonly float BoxWidth;
        public readonly float BoxHeight;
        public readonly float BoxYaw;
        [NotPersistent] public readonly float MaxDistSq;
        [NotPersistent] public ATTemplate Template;

        public event AreaTrigger.ATUseHandler Triggered;

        public AreaTrigger(uint id, MapId mapId, float x, float y, float z, float radius, float boxLength,
            float boxWidth, float boxHeight, float boxYaw)
        {
            this.Id = id;
            this.ATId = (AreaTriggerId) this.Id;
            this.MapId = mapId;
            this.Position.X = x;
            this.Position.Y = y;
            this.Position.Z = z;
            this.Radius = radius;
            this.BoxLength = boxLength;
            this.BoxWidth = boxWidth;
            this.BoxHeight = boxHeight;
            this.BoxYaw = boxYaw;
            this.MaxDistSq = (float) (((double) this.Radius + 55.0) * ((double) this.Radius + 55.0));
        }

        /// <summary>
        /// Returns whether the given object is within the bounds of this AreaTrigger.
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public bool IsInArea(Character chr)
        {
            if (chr.Map.Id != this.MapId)
                return false;
            if ((double) this.Radius > 0.0)
            {
                float distanceSq = chr.GetDistanceSq(this.Position);
                if ((double) distanceSq > (double) this.MaxDistSq)
                {
                    LogManager.GetCurrentClassLogger()
                        .Warn("Character {0} tried to trigger {1} while being {2} yards away.", (object) chr,
                            (object) this, (object) Math.Sqrt((double) distanceSq));
                    return false;
                }
            }
            else
            {
                float num1 = 6.283185f - this.BoxYaw;
                double num2 = Math.Sin((double) num1);
                double num3 = Math.Cos((double) num1);
                float num4 = chr.Position.X - this.Position.X;
                float num5 = chr.Position.Y - this.Position.Y;
                if (Math.Abs((double) this.Position.X + (double) num4 * num3 - (double) num5 * num2 -
                             (double) this.Position.X) > (double) this.BoxLength / 2.0 + 55.0 ||
                    Math.Abs((double) this.Position.Y + (double) num5 * num3 + (double) num4 * num2 -
                             (double) this.Position.Y) > (double) this.BoxWidth / 2.0 + 55.0 ||
                    (double) Math.Abs(chr.Position.Z - this.Position.Z) > (double) this.BoxHeight / 2.0 + 55.0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Does general checks, for whether the given Character may trigger this and sends
        /// an error response if not.
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public bool CheckTrigger(Character chr)
        {
            if (!this.IsInArea(chr) || chr.IsOnTaxi)
                return false;
            if (this.Template == null || (long) chr.Level >= (long) this.Template.RequiredLevel)
                return true;
            WCell.RealmServer.Handlers.AreaTriggerHandler.SendAreaTriggerMessage((IPacketReceiver) chr.Client,
                "You need at least level " + (object) this.Template.RequiredLevel + ".");
            return false;
        }

        /// <summary>Triggers this trigger</summary>
        /// <remarks>Requires map context.</remarks>
        public bool Trigger(Character chr)
        {
            if (!this.CheckTrigger(chr))
                return false;
            this.NotifyTriggered(this, chr);
            if (this.Template != null)
                return this.Template.Handler(chr, this);
            return true;
        }

        internal void NotifyTriggered(AreaTrigger at, Character chr)
        {
            AreaTrigger.ATUseHandler triggered = this.Triggered;
            if (triggered == null)
                return;
            triggered(at, chr);
        }

        public override string ToString()
        {
            if (this.Template != null)
                return this.Template.Name + " (Id: " + (object) this.Id + " [" + (object) this.ATId + "])";
            return ((int) this.ATId).ToString() + " (Id: " + (object) this.Id + ")";
        }

        public void Write(IndentTextWriter writer)
        {
            writer.WriteLine((object) this);
            ++writer.IndentLevel;
            writer.WriteLine("MapId: " + (object) this.MapId);
            writer.WriteLine("Position: " + (object) this.Position);
            writer.WriteLineNotDefault<float>(this.Radius, "Radius: " + (object) this.Radius);
            writer.WriteLineNotDefault<float>(
                (float) ((double) this.BoxLength + (double) this.BoxWidth + (double) this.BoxHeight +
                         (double) this.BoxYaw),
                "Box Length: " + (object) this.BoxLength + ", Width: " + (object) this.BoxWidth + ", Height: " +
                (object) this.BoxHeight + ", Yaw: " + (object) this.BoxYaw);
            if (this.Template != null)
                this.Template.Write(writer);
            --writer.IndentLevel;
        }

        public delegate void ATUseHandler(AreaTrigger at, Character triggerer);
    }
}