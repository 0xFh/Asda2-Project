using Cell.Core;
using System;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Commands
{
    public class DebugCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("Debug");
            this.EnglishDescription = "Provides Debug-capabilities and management of Debug-tools for Devs.";
        }

        public class GCCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("GC");
                this.EnglishDescription =
                    "Don't use this unless you are well aware of the stages and heuristics involved in the GC process!";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                GC.Collect();
                trigger.Reply("Done.");
            }
        }

        public class InfoCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Info");
                this.EnglishDescription = "Shows all available Debug information.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                DebugCommand.BufferPoolCommand.ShowInfo(trigger);
                DebugCommand.ObjectPoolCommand.ShowInfo(trigger);
            }
        }

        public class ObjectPoolCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("ObjectPool", "Obj");
                this.EnglishDescription = "Overviews the Object Pools";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                DebugCommand.ObjectPoolCommand.ShowInfo(trigger);
            }

            public static void ShowInfo(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Reply("There are {0} ObjectPools in use.",
                    (object) WCell.RealmServer.Misc.ObjectPoolMgr.Pools.Count);
                IObjectPool objectPool1 = (IObjectPool) null;
                IObjectPool objectPool2 = (IObjectPool) null;
                foreach (IObjectPool pool in WCell.RealmServer.Misc.ObjectPoolMgr.Pools)
                {
                    if (objectPool1 == null || pool.AvailableCount > objectPool1.AvailableCount)
                        objectPool1 = pool;
                    if (objectPool2 == null || pool.ObtainedCount > objectPool2.ObtainedCount)
                        objectPool2 = pool;
                }

                trigger.Reply("Biggest Pool ({0}): {1} - Pool with most Objects checked out ({2}): {3}",
                    (object) objectPool1.AvailableCount, (object) objectPool1, (object) objectPool2.ObtainedCount,
                    (object) objectPool2);
            }
        }

        public class BufferPoolCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Buffers", "Buf");
                this.EnglishDescription = "Overviews the Buffer Managers.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                DebugCommand.BufferPoolCommand.ShowInfo(trigger);
            }

            public static void ShowInfo(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Reply("Total Buffer Memory: {0} - Pools in use:", (object) BufferManager.GlobalAllocatedMemory);
                foreach (BufferManager manager in BufferManager.Managers)
                {
                    if (manager.InUse)
                        trigger.Reply("{2}k Buffer: {0}/{1}", (object) manager.UsedSegmentCount,
                            (object) manager.TotalSegmentCount,
                            (object) (float) ((double) manager.SegmentSize / 1024.0));
                }
            }
        }
    }
}