using WCell.RealmServer.NPCs.Vehicles;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class VehicleCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Vehicle", "Veh");
            this.EnglishDescription = "Provides commands to manage Vehicles.";
        }

        public class VehicleClearCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Clear", "C");
                this.EnglishDescription = "Removes all passengers from a Vehicle.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Vehicle target = trigger.Args.GetTarget<Vehicle>();
                if (target == null)
                {
                    trigger.Reply("No Vehicle selected.");
                }
                else
                {
                    int passengerCount = target.PassengerCount;
                    target.ClearAllSeats(false);
                    trigger.Reply("Done. - Removed {0} passengers from {1}", (object) passengerCount, (object) target);
                }
            }
        }
    }
}