using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.Util.Variables;

namespace WCell.RealmServer.Asda2_Items
{
    public class Asda2CraftMgr
    {
        [NotVariable] public static Asda2RecipeTemplate[] RecipeTemplates = new Asda2RecipeTemplate[700];

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, "Craft")]
        public static void Init()
        {
            ContentMgr.Load<Asda2RecipeTemplate>();
        }

        public static Asda2RecipeTemplate GetRecipeTemplate(int id)
        {
            if (id < 1 || id >= Asda2CraftMgr.RecipeTemplates.Length)
                return (Asda2RecipeTemplate) null;
            return Asda2CraftMgr.RecipeTemplates[id];
        }
    }
}