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
      Init("Debug");
      EnglishDescription = "Provides Debug-capabilities and management of Debug-tools for Devs.";
    }

    public class GCCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("GC");
        EnglishDescription =
          "Don't use this unless you are well aware of the stages and heuristics involved in the GC process!";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        GC.Collect();
        trigger.Reply("Done.");
      }
    }

    public class InfoCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Info");
        EnglishDescription = "Shows all available Debug information.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        BufferPoolCommand.ShowInfo(trigger);
        ObjectPoolCommand.ShowInfo(trigger);
      }
    }

    public class ObjectPoolCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("ObjectPool", "Obj");
        EnglishDescription = "Overviews the Object Pools";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        ShowInfo(trigger);
      }

      public static void ShowInfo(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        trigger.Reply("There are {0} ObjectPools in use.",
          (object) Misc.ObjectPoolMgr.Pools.Count);
        IObjectPool objectPool1 = null;
        IObjectPool objectPool2 = null;
        foreach(IObjectPool pool in Misc.ObjectPoolMgr.Pools)
        {
          if(objectPool1 == null || pool.AvailableCount > objectPool1.AvailableCount)
            objectPool1 = pool;
          if(objectPool2 == null || pool.ObtainedCount > objectPool2.ObtainedCount)
            objectPool2 = pool;
        }

        trigger.Reply("Biggest Pool ({0}): {1} - Pool with most Objects checked out ({2}): {3}",
          (object) objectPool1.AvailableCount, (object) objectPool1, (object) objectPool2.ObtainedCount,
          (object) objectPool2);
      }
    }

    public class BufferPoolCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Buffers", "Buf");
        EnglishDescription = "Overviews the Buffer Managers.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        ShowInfo(trigger);
      }

      public static void ShowInfo(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        trigger.Reply("Total Buffer Memory: {0} - Pools in use:", (object) BufferManager.GlobalAllocatedMemory);
        foreach(BufferManager manager in BufferManager.Managers)
        {
          if(manager.InUse)
            trigger.Reply("{2}k Buffer: {0}/{1}", (object) manager.UsedSegmentCount,
              (object) manager.TotalSegmentCount,
              (object) (float) ((double) manager.SegmentSize / 1024.0));
        }
      }
    }
  }
}