namespace AzureStorageBinding.TestingContract
{
    using System.ServiceModel;

    [ServiceContract]
    public interface IEcho
    {
        [OperationContract]
        string Echo(string content);
    }
}