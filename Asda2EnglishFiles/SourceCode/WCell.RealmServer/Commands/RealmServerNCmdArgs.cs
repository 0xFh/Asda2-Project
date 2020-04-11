namespace WCell.RealmServer.Commands
{
    public class RealmServerNCmdArgs : RealmServerCmdArgs
    {
        public uint N;

        public RealmServerNCmdArgs(RealmServerCmdArgs args, uint n)
            : base(args)
        {
            this.N = n;
        }
    }
}