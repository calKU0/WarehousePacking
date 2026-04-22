using System.ServiceProcess;

namespace WarehousePacking.PrintService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PrintingService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}