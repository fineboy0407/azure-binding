namespace AzureStorageBinding.TestingContract
{
    using System.ServiceModel;
    using System.Threading.Tasks;

    [ServiceContract(Name = "IEcho")]
    public interface IEchoClient
    {
        [OperationContract]
        Task<string> EchoAsync(string content);
    }
}