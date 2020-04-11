using WCell.RealmServer.Content;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class CacheCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Cache");
            this.EnglishDescription = "Provides commands to manage the server-side cache of static data.";
        }

        public class PurgeCacheCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Purge");
                this.EnglishDescription = "Removes all cache-files.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                ContentMgr.PurgeCache();
                trigger.Reply("Done.");
            }
        }
    }
}