namespace AzureStorageBinding.Table.Utils
{
    internal static class TableConstants
    {
        public const string ConnectionStringConfigureName = "connectingString";

        public const string TargetPartitionKeyConfigureName = "targetPartitionKey";

        internal const string RequestTableNameSuffix = "-Request";

        internal const string ResponseTableNameSuffix = "-Response";

        public static string TargetAllPartitionToken => "all";

        public static string PartitionKeyPropertyName => "PartitionKey";

        public static string RowKeyPropertyName => "RowKey";
    }
}