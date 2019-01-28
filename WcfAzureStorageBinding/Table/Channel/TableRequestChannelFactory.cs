namespace AzureStorageBinding.Table.Channel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using AzureStorageBinding.Table.Binding;
    using AzureStorageBinding.Utils.Apm;

    using Microsoft.WindowsAzure.Storage;

    internal class TableRequestChannelFactory : ChannelFactoryBase<IRequestChannel>
    {
        private string targetPartitionKey;

        private MessageEncoderFactory encoderFactory;

        private BufferManager bufferManager;

        private CloudStorageAccount storageAccount;

        public TableRequestChannelFactory(BindingContext context, TableTransportBindingElement element):base(context.Binding)
        {
            this.encoderFactory = context.BindingParameters.Remove<MessageEncodingBindingElement>().CreateMessageEncoderFactory();
            this.bufferManager = BufferManager.CreateBufferManager(element.MaxBufferPoolSize, int.MaxValue);
            this.targetPartitionKey = element.TargetPartitionKey;
            this.storageAccount = CloudStorageAccount.Parse(element.ConnectionString);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create( callback, state);

        protected override void OnEndOpen(IAsyncResult result)
        {
        }

        protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new TableRequestChannel(this, this.storageAccount.CreateCloudTableClient(), this.targetPartitionKey, this.bufferManager, address, this.encoderFactory.CreateSessionEncoder(), via);
        }
    }
}