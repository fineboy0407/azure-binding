namespace AzureStorageBinding.TestingService
{
    using System;
    using System.ServiceModel;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceHost = new ServiceHost(typeof(EchoService));

            serviceHost.BeginOpen(serviceHost.EndOpen, null);

            Console.WriteLine("Service running.");
            Console.WriteLine("Press <ENTER> to stop.");
            Console.ReadLine();
        }
    }
}