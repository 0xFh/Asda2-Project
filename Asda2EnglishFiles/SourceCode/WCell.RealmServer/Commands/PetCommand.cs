namespace WCell.RealmServer.Commands
{
    public class PetCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Pet");
            this.EnglishDescription = "A set of commands to manage pets.";
        }
    }
}