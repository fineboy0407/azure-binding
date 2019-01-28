namespace AzureStorageBinding.Table.Channel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using AzureStorageBinding.Table.DTO;
    using AzureStorageBinding.Utils.Apm;

    using Microsoft.WindowsAzure.Storage.Table;

    internal class TableReplyChannel : TableChannel, IReplyChannel
    {
        public TableReplyChannel(
            ChannelManagerBase channelManager,
            CloudTableClient tableClient,
            string tableName,
            string targetPartition,
            BufferManager bufferManager,
            EndpointAddress address,
            MessageEncoder encoder) : base(channelManager, tableClient, tableName, targetPartition, bufferManager, address, encoder)
        {
        }

        public EndpointAddress LocalAddress { get; }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state) => this.BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state) => this.ReceiveRequestAsync(timeout).AsApm(callback, state);

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state) => this.TryReceiveRequestAsync(timeout).AsApm(callback, state);

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state) => this.WaitForRequestAsync(timeout).AsApm(callback, state);

        public RequestContext EndReceiveRequest(IAsyncResult result) => result.GetApmTaskResult<RequestContext>();

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            var (succeed, ctx) = result.GetApmTaskResult<(bool, TableRequestContext)>();
            context = ctx;
            return succeed;
        }

        public bool EndWaitForRequest(IAsyncResult result) => result.GetApmTaskResult<(bool succeed, WcfTableEntity)>().succeed;

        public RequestContext ReceiveRequest() => this.ReceiveRequest(this.DefaultReceiveTimeout);

        public RequestContext ReceiveRequest(TimeSpan timeout) => this.ReceiveRequestAsync(timeout).GetAwaiter().GetResult();

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            var (succeed, ctx) = this.TryReceiveRequestAsync(timeout).GetAwaiter().GetResult();
            context = ctx;
            return succeed;
        }

        public bool WaitForRequest(TimeSpan timeout) => this.WaitForRequestAsync(timeout).GetAwaiter().GetResult().succeed;

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => CompletedAsyncResult.Create(callback, state);

        protected override void OnClose(TimeSpan timeout)
        {
            this.CloseChannel();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.CloseChannel();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected async Task<(bool succeed, TableRequestContext requestContext)> TryReceiveRequestAsync(TimeSpan timeout)
        {
            var (succeed, entity) = await this.TryPopEntityFromTableAsync(this.RequestTableName, timeout);
            if (succeed)
            {
                return (true, this.BuildTableRequestContext(entity, timeout));
            }

            return (false, null);
        }

        /// <summary>
        ///     Peek a request
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected Task<(bool succeed, WcfTableEntity entity)> WaitForRequestAsync(TimeSpan timeout) => this.TryGetTableEntityAsync(this.RequestTableName, timeout);

        private TableRequestContext BuildTableRequestContext(WcfTableEntity entity, TimeSpan timeout)
        {
            var msg = this.ReadMessageFromTableEntity(entity);
            return new TableRequestContext(msg, entity.PartitionKey, entity.RequestId, this.WriteReplyMessageAsync, timeout);
        }

        private async Task<RequestContext> ReceiveRequestAsync(TimeSpan timeout)
        {
            this.ThrowIfDisposedOrNotOpen();
            var entity = await this.PopEntryFromTableAsync(this.RequestTableName);
            return this.BuildTableRequestContext(entity, timeout);
        }

        private Task WriteReplyMessageAsync(Message message, string partitionKey, string requestId)
        {
            this.ThrowIfDisposedOrNotOpen();
            return this.WriteMessageAsync(this.ResponseTableName, message, partitionKey, requestId);
        }
    }
}