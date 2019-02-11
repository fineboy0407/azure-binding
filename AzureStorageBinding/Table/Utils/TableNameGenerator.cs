namespace AzureStorageBinding.Table.Utils
{
    internal static class TableNameGenerator
    {
        public static string GetRequestTableName(string baseName) => baseName + TableConstants.RequestTableNameSuffix;

        public static string GetResponseTableName(string baseName) => baseName + TableConstants.ResponseTableNameSuffix;
    }
}