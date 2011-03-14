using System;
using System.ServiceModel;
using log4net;

namespace RestServiceLauncher
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof(XplaneServices.RestService));
            host.Open();
            Log.Debug("Started Service.");
            Console.WriteLine("Service running.  Press any key to exit.");
            Console.ReadLine();
            host.Close();
        }
    }
}
