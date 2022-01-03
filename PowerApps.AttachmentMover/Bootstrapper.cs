namespace AttachmentMover
{
    /// <summary>
    ///   Application Launcher
    /// </summary>
    public static class Bootstrapper
    {
        static void Main(string[] args)
        {
            BootstrapperWorker bootstrapper_Worker = new BootstrapperWorker();
            bootstrapper_Worker.GatewayProcess();
        }
    }
}