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
        private readonly BufferManager bufferManager;

        private readonly MessageEncoderFactory encoderFactory;

        private readonly CloudStorageAccount storageAccount;

        private readonly string targetPartitionKey;

        public TableRequestChannelFactory(BindingContext context, TableTransportBindingElement element) : base(context.Binding)
        {
            this.encoderFactory = context.BindingParameters.Remove<MessageEncodingBindingElement>().CreateMessageEncoderFactory();
            this.bufferManager = BufferManager.CreateBufferManager(element.MaxBufferPoolSize, int.MaxValue);
            this.targetPartitionKey = element.TargetPartitionKey;
            this.storageAccount = CloudStorageAccount.Parse(element.ConnectionString);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via) =>
            new TableRequestChannel(this, this.storageAccount.CreateCloudTableClient(), this.targetPartitionKey, this.bufferManager, address, this.encoderFactory.CreateSessionEncoder(), via);

        protected override void OnEndOpen(IAsyncResult result)
        {
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }
    }
}