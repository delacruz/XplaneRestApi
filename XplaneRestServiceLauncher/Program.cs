using System;
using System.ServiceModel;

namespace RestServiceLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof(XplaneServices.RestService));
            host.Open();
            Console.WriteLine("Service running.  Press any key to exit.");
            Console.ReadLine();
            host.Close();
        }
    }
}
