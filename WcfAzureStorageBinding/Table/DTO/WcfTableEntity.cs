namespace AzureStorageBinding.Table.DTO
{
    using System;

    using Microsoft.WindowsAzure.Storage.Table;

    internal class WcfTableEntity : TableEntity
    {
        public WcfTableEntity(string partitionKey) : this(partitionKey, Guid.NewGuid().ToString())
        {
        }

        public WcfTableEntity(string partitionKey, string requestId)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = requestId;
            this.RequestId = requestId;
        }

        public WcfTableEntity()
        {
        }

        public string Message { get; set; }

        public string RequestId { get; set; }
    }
}