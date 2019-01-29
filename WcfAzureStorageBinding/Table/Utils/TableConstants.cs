namespace AzureStorageBinding.Table.Utils
{
    internal static class TableConstants
    {
        public const string ClientId = "clientId";

        public const string ConnectionStringConfigureName = "connectionString";

        public const string TargetAllPartitionToken = "all";

        public const string TargetPartitionKeyConfigureName = "targetPartitionKey";

        internal const string RequestTableNameSuffix = "Request";

        internal const string ResponseTableNameSuffix = "Response";

        public static string PartitionKeyPropertyName => "PartitionKey";

        public static string RowKeyPropertyName => "RowKey";
    }
}