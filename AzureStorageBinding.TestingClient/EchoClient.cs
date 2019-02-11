namespace AzureStorageBinding.TestingClient
{
    using System.ServiceModel;
    using System.Threading.Tasks;

    using AzureStorageBinding.TestingContract;

    internal class EchoClient : ClientBase<IEchoClient>
    {
        public Task<string> EchoAsync(string content) => this.Channel.EchoAsync(content);
    }
}