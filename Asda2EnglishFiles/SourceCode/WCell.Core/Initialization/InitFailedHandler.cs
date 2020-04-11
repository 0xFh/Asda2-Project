namespace WCell.Core.Initialization
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mgr"></param>
    /// <param name="step"></param>
    /// <returns>Whether or not to continue</returns>
    public delegate bool InitFailedHandler(InitMgr mgr, InitializationStep step);
}