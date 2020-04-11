using WCell.Util.Variables;

namespace WCell.RealmServer.Interaction
{
    public static class WhoList
    {
        [Variable("MaxWhoListResultCount")] public static uint MaxResultCount = 50;
    }
}