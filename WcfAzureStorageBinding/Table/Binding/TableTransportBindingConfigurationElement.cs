namespace AzureStorageBinding.Table.Binding
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    using AzureStorageBinding.Table.Utils;

    public class TableTransportBindingConfigurationElement : StandardBindingElement
    {
        protected override void OnApplyConfiguration(Binding binding)
        {
            if (binding == null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            if (binding is TableTransportBinding tableBinding)
            {
                tableBinding.ApplySetting(this.ConnectionString, this.TargetPartitionKey);
            }
        }

        protected override Type BindingElementType { get; }

        [ConfigurationProperty(TableConstants.ConnectionStringConfigureName)]
        public string ConnectionString
        {
            get => (string)base[TableConstants.ConnectionStringConfigureName];
            set => base[TableConstants.ConnectionStringConfigureName] = value;
        }

        [ConfigurationProperty(TableConstants.TargetPartitionKeyConfigureName)]
        public string TargetPartitionKey
        {
            get => (string)base[TableConstants.TargetPartitionKeyConfigureName];
            set => base[TableConstants.TargetPartitionKeyConfigureName] = value;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                var properties = base.Properties;
                properties.Add(new ConfigurationProperty(TableConstants.ConnectionStringConfigureName, typeof(string)));
                properties.Add(new ConfigurationProperty(TableConstants.TargetPartitionKeyConfigureName, typeof(string)));
                return properties;
            }
        }
    }
}