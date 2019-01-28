namespace AzureStorageBinding.Table.Channel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using AzureStorageBinding.Utils.Apm;

    // TODO: we are not really honer the timeout here
    internal class TableRequestContext : RequestContext
    {
        private object stateLock = new object();

        private Func<Message, string, string, Task> WriteReplyMessageAsyncDelegate { get;}

        private string partitionKey;

        private string requestId;

        private bool aborted = false;

        private CommunicationState state = CommunicationState.Opened;

        private TimeSpan sendTimeout;

        public TableRequestContext(Message requestMessage, string partitionKey, string requestId, Func<Message, string, string, Task> writeReplyMessageAsyncDelegate, TimeSpan sendTimeout)
        {
            this.RequestMessage = requestMessage;
            this.partitionKey = partitionKey;
            this.requestId = requestId;
            this.WriteReplyMessageAsyncDelegate = writeReplyMessageAsyncDelegate;
            this.sendTimeout = sendTimeout;
        }

        public override void Abort()
        {
            lock (this.stateLock)
            {
                if (this.aborted)
                {
                    return;
                }

                this.aborted = true;
                this.state = CommunicationState.Faulted;
            }
        }

        private void ThrowIfClosedOrFaulted()
        {
            if (this.aborted)
            {
                throw new CommunicationObjectAbortedException();
            }

            if (this.state == CommunicationState.Faulted)
            {
                throw new CommunicationObjectFaultedException();
            }

            if (this.state == CommunicationState.Closed)
            {
                throw new CommunicationException($"{nameof(TableRequestContext)} closed.");
            }
        }

        public override void Close()
        {
            this.Close(TimeSpan.MaxValue);
        }

        public override void Close(TimeSpan timeout)
        {
            lock (this.stateLock)
            {
                this.state = CommunicationState.Closed;
            }
        }

        public override void Reply(Message message) => this.Reply(message, this.sendTimeout);

        public override void Reply(Message message, TimeSpan timeout)
        {
            this.WriteReplyMessageAsync(message).GetAwaiter().GetResult();
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state) => this.BeginReply(message, this.sendTimeout, callback, state);

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state) => this.WriteReplyMessageAsync(message).AsApm(callback, state);
        

        public override void EndReply(IAsyncResult result)
        {
            result.WaitApmTask();
        }

        private Task WriteReplyMessageAsync(Message message)
        {
            this.ThrowIfClosedOrFaulted();
            return this.WriteReplyMessageAsyncDelegate(message, this.partitionKey, this.requestId);
        }

        public override Message RequestMessage { get; }
    }
}