namespace AzureStorageBinding.Table.Channel
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    using AzureStorageBinding.Table.DTO;
    using AzureStorageBinding.Table.Utils;
    using AzureStorageBinding.Utils;
    using AzureStorageBinding.Utils.AzureStorage;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    internal abstract class TableChannel : ChannelBase
    {
        private const int MaxBufferSize = 64 * 1024;

        private static readonly TimeSpan PullInterval = TimeSpan.FromSeconds(1);

        private readonly BufferManager bufferManager;

        private readonly MessageEncoder encoder;

        protected TableChannel(
            ChannelManagerBase channelManager,
            CloudTableClient tableClient,
            string tableName,
            string targetPartition,
            BufferManager bufferManager,
            EndpointAddress address,
            MessageEncoder encoder) : base(channelManager)
        {
            this.CloudTableClient = tableClient;
            this.TargetPartition = targetPartition;
            this.bufferManager = bufferManager;
            this.RemoteAddress = address;
            this.encoder = encoder;
            this.TableName = tableName;
        }

        public EndpointAddress RemoteAddress { get; }

        internal string TargetPartition { get; }

        protected bool ChannelClosed { get; private set; }

        protected CloudTableClient CloudTableClient { get; }

        protected string RequestTableName => TableNameGenerator.GetRequestTableName(this.TableName);

        protected string ResponseTableName => TableNameGenerator.GetResponseTableName(this.TableName);

        protected string TableName { get; }

        private bool TargetAllPartition => string.IsNullOrEmpty(this.TargetPartition) || this.TargetPartition.Equals(TableConstants.TargetAllPartitionToken, StringComparison.OrdinalIgnoreCase);

        protected void CloseChannel()
        {
            this.ChannelClosed = true;
        }

        protected async Task<WcfTableEntity> PopEntryFromTableAsync(string tableName)
        {
            WcfTableEntity entity = null;
            try
            {
                entity = await this.GetTableEntityAsync(this.BuildTableQuery());
                return entity;
            }
            finally
            {
                if (entity != null)
                {
                    await this.RemoveTableEntityAsync(tableName, entity);
                }
            }
        }

        protected async Task<Message> PopMessageFromTableAsync(string tableName, TableQuery<WcfTableEntity> tableQuery)
        {
            WcfTableEntity entity = null;
            try
            {
                entity = await this.GetTableEntityAsync(tableQuery);
                return this.ReadMessageFromTableEntity(entity);
            }
            finally
            {
                if (entity != null)
                {
                    await this.RemoveTableEntityAsync(tableName, entity);
                }
            }
        }

        protected Task<Message> PopMessageFromTableAsync(string tableName) =>
            this.PopMessageFromTableAsync(tableName, new TableQuery<WcfTableEntity> { FilterString = this.GetFilterString(), TakeCount = 1 });

        protected Message ReadMessageFromTableEntity(WcfTableEntity entity)
        {
            // TODO: stackalloc
            byte[] data = null;
            var byteLength = Encoding.UTF8.GetByteCount(entity.Message);

            data = this.bufferManager.TakeBuffer(byteLength);
            Encoding.UTF8.GetBytes(entity.Message, 0, entity.Message.Length, data, 0);
            var buffer = new ArraySegment<byte>(data, 0, byteLength);
            return this.DecodeMessage(buffer);
        }

        /// <summary>
        ///     Try to peek a request
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected Task<(bool, WcfTableEntity)> TryGetTableEntityAsync(string tableName, TimeSpan timeout) => this.TryGetTableEntityAsync(tableName, this.BuildTableQuery(), timeout);

        /// <summary>
        ///     Try to peek a request
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableQuery"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected async Task<(bool, WcfTableEntity)> TryGetTableEntityAsync(string tableName, TableQuery<WcfTableEntity> tableQuery, TimeSpan timeout)
        {
            if (this.ChannelClosed)
            {
                return (false, null);
            }

            this.ThrowIfDisposedOrNotOpen();
            try
            {
                var cloudTable = this.CloudTableClient.GetTableReference(tableName);

                DateTime end = timeout.AfterTimeSpanFromNow();

                while (!this.ChannelClosed && DateTime.Now < end)
                {
                    var res = await cloudTable.ExecuteQueryAsync(tableQuery).ConfigureAwait(false);

                    if (res != null && res.Any())
                    {
                        return (true, res.First());
                    }

                    await Task.Delay(PullInterval).ConfigureAwait(false);
                }

                return (false, null);
            }
            catch (StorageException exception)
            {
                throw new CommunicationException(exception.Message, exception);
            }
        }

        protected async Task<(bool, WcfTableEntity)> TryPopEntityFromTableAsync(string tableName, TimeSpan timeout)
        {
            WcfTableEntity entity = null;
            try
            {
                var (succeed, res) = await this.TryGetTableEntityAsync(tableName, this.BuildTableQuery(), timeout);
                if (succeed)
                {
                    entity = res;
                }

                return (succeed, res);
            }
            finally
            {
                if (entity != null)
                {
                    await this.RemoveTableEntityAsync(tableName, entity);
                }
            }
        }

        protected async Task<(bool, Message)> TryPopMessageFromTableAsync(string tableName, TableQuery<WcfTableEntity> tableQuery, TimeSpan timeout)
        {
            WcfTableEntity entity = null;
            try
            {
                var (succeed, res) = await this.TryGetTableEntityAsync(tableName, tableQuery, timeout);
                if (succeed)
                {
                    entity = res;
                    return (true, this.ReadMessageFromTableEntity(entity));
                }
            }
            finally
            {
                if (entity != null)
                {
                    await this.RemoveTableEntityAsync(tableName, entity);
                }
            }

            return (false, null);
        }

        protected Task<(bool, Message)> TryPopMessageFromTableAsync(string tableName, TimeSpan timeout) => this.TryPopMessageFromTableAsync(tableName, this.BuildTableQuery(), timeout);

        protected async Task WriteMessageAsync(string tableName, Message message, string partitionKey, string requestId)
        {
            ArraySegment<byte> buffer = default;

            try
            {
                var entity = new WcfTableEntity(partitionKey, requestId);
                this.RemoteAddress.ApplyTo(message);
                buffer = this.EncodeMessage(message);

                entity.Message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);

                var cloudTable = this.CloudTableClient.GetTableReference(tableName);
                await cloudTable.ExecuteAsync(TableOperation.Insert(entity)).ConfigureAwait(false);
            }
            finally
            {
                message.Close();
                if (buffer.Array != null)
                {
                    this.bufferManager.ReturnBuffer(buffer.Array);
                }
            }
        }

        private TableQuery<WcfTableEntity> BuildTableQuery()
        {
            if (this.TargetAllPartition)
            {
                return new TableQuery<WcfTableEntity>().Take(1);
            }

            return new TableQuery<WcfTableEntity>().Where(TableQuery.GenerateFilterCondition(TableConstants.PartitionKeyPropertyName, QueryComparisons.Equal, this.TargetPartition)).Take(1);
        }

        private Message DecodeMessage(ArraySegment<byte> buffer)
        {
            if (buffer.Array == null)
            {
                return null;
            }

            return this.encoder.ReadMessage(buffer, this.bufferManager);
        }

        private ArraySegment<byte> EncodeMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return this.encoder.WriteMessage(message, MaxBufferSize, this.bufferManager);
        }

        private string GetFilterString()
        {
            {
                if (this.TargetAllPartition)
                {
                    return string.Empty;
                }

                return $@"PartitionKey = ""{this.TargetPartition}""";
            }
        }

        private async Task<WcfTableEntity> GetTableEntityAsync(TableQuery<WcfTableEntity> tableQuery)
        {
            var cloudTable = this.CloudTableClient.GetTableReference(this.TableName);
            var result = await cloudTable.ExecuteQueryAsync(tableQuery);
            Debug.Assert(result.Any());
            return result.First();
        }

        private async Task RemoveTableEntityAsync(string tableName, WcfTableEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            // TODO: need reliability
            var cloudTable = this.CloudTableClient.GetTableReference(tableName);
            await cloudTable.ExecuteAsync(TableOperation.Delete(entity)).ConfigureAwait(false);
        }
    }
}