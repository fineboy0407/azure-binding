namespace AzureStorageBinding.Table.Channel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    using AzureStorageBinding.Table.Binding;
    using AzureStorageBinding.Table.Utils;
    using AzureStorageBinding.Utils.Apm;

    using Microsoft.WindowsAzure.Storage;

    internal class TableReplyChannelListener : ChannelListenerBase<IReplyChannel>
    {
        private string targetPartitionKey;

        private MessageEncoderFactory encoderFactory;

        private BufferManager bufferManager;

        private CloudStorageAccount storageAccount;

        private TableReplyChannel replyChannel;

        private string TableName => this.Uri.AbsolutePath;

        public TableReplyChannelListener(BindingContext context, TableTransportBindingElement element) : base(context.Binding)
        {
            this.encoderFactory = context.BindingParameters.Remove<MessageEncodingBindingElement>().CreateMessageEncoderFactory();
            this.bufferManager = BufferManager.CreateBufferManager(element.MaxBufferPoolSize, int.MaxValue);
            this.targetPartitionKey = element.TargetPartitionKey;
            this.storageAccount = CloudStorageAccount.Parse(element.ConnectionString);
            this.Uri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
        }

        protected override void OnAbort()
        {
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override void OnOpen(TimeSpan timeout) => this.OnOpenAsync(timeout).GetAwaiter().GetResult();

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => this.OnOpenAsync(timeout).AsApm(callback, state);

        protected override void OnEndOpen(IAsyncResult result) => result.WaitApmTask();

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return true;
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return true;
        }

        public override Uri Uri { get; }

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

        protected override IReplyChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return this.OnAcceptChannel(TimeSpan.MaxValue);
        }

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