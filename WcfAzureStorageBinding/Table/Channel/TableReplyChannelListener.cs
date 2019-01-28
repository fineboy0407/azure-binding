namespace AzureStorageBinding.Table.Channel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using AzureStorageBinding.Table.Binding;
    using AzureStorageBinding.Table.Utils;
    using AzureStorageBinding.Utils.Apm;

    using Microsoft.WindowsAzure.Storage;

    internal class TableReplyChannelListener : ChannelListenerBase<IReplyChannel>
    {
        private readonly BufferManager bufferManager;

        private readonly MessageEncoderFactory encoderFactory;

        private readonly CloudStorageAccount storageAccount;

        private readonly string targetPartitionKey;

        private TableReplyChannel replyChannel;

        public TableReplyChannelListener(BindingContext context, TableTransportBindingElement element) : base(context.Binding)
        {
            this.encoderFactory = context.BindingParameters.Remove<MessageEncodingBindingElement>().CreateMessageEncoderFactory();
            this.bufferManager = BufferManager.CreateBufferManager(element.MaxBufferPoolSize, int.MaxValue);
            this.targetPartitionKey = element.TargetPartitionKey;
            this.storageAccount = CloudStorageAccount.Parse(element.ConnectionString);
            this.Uri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
        }

        public override Uri Uri { get; }

        private string TableName => this.Uri.AbsolutePath;

        protected override void OnAbort()
        {
        }

        protected override IReplyChannel OnAcceptChannel(TimeSpan timeout)
        {
            if (this.replyChannel == null)
            {
                var address = new EndpointAddress(this.Uri);
                this.replyChannel = new TableReplyChannel(
                    this,
                    this.storageAccount.CreateCloudTableClient(),
                    this.Uri.AbsolutePath,
                    this.targetPartitionKey,
                    this.bufferManager,
                    address,
                    this.encoderFactory.CreateSessionEncoder());
            }

            return this.replyChannel;
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => this.OnOpenAsync(timeout).AsApm(callback, state);

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override IReplyChannel OnEndAcceptChannel(IAsyncResult result) => this.OnAcceptChannel(TimeSpan.MaxValue);

        protected override void OnEndClose(IAsyncResult result)
        {
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            result.WaitApmTask();
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result) => true;

        protected override void OnOpen(TimeSpan timeout)
        {
            this.OnOpenAsync(timeout).GetAwaiter().GetResult();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout) => true;

        private async Task OnOpenAsync(TimeSpan timeout)
        {
            var cloudTableClient = this.storageAccount.CreateCloudTableClient();
            var requestTable = cloudTableClient.GetTableReference(TableNameGenerator.GetRequestTableName(this.TableName));
            await requestTable.CreateIfNotExistsAsync().ConfigureAwait(false);
            var replyTable = cloudTableClient.GetTableReference(TableNameGenerator.GetResponseTableName(this.TableName));
            await replyTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }
    }
}