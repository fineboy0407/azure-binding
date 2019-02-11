namespace AzureStorageBinding.TestingService
{
    using System;

    using AzureStorageBinding.TestingContract;

    internal class EchoService : IEcho
    {
        public string Echo(string content) => $"Echo: {content}, {DateTimeOffset.UtcNow.ToString()}";
    }
}