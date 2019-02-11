namespace AzureStorageBinding.TestingClient
{
    using System;
    using System.Threading.Tasks;

    public class Program
    {
        private static async Task Main()
        {
            var client = new EchoClient();

            while (true)
            {
                var content = Console.ReadLine();
                var res = await client.EchoAsync(content);
                Console.WriteLine(res);
            }
        }
    }
}