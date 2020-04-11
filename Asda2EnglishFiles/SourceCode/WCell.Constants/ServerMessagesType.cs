namespace WCell.Constants
{
    public enum ServerMessagesType
    {
        ServerShutdownStart = 1,
        ServerRestartStart = 2,
        Custom = 3,
        ServerShutdownCancelled = 4,
        ServerRestartCancelled = 5,
        BattlegroundShutdown = 6,
        BattlegroundRestart = 7,
        InstanceShutdown = 8,
        InstanceRestart = 9,
    }
}