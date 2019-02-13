namespace AzureStorageBinding.Table.Binding
{
    using System;
    using System.ServiceModel.Channels;

    public class TableTransportBinding : Binding
    {
        private readonly MessageEncodingBindingElement messageEncodingBindingElement;

        private readonly TableTransportBindingElement transportBindingElement;

        public TableTransportBinding()
        {
            this.transportBindingElement = new TableTransportBindingElement();
            this.messageEncodingBindingElement = new TextMessageEncodingBindingElement();
        }

        public override string Scheme => this.transportBindingElement.Scheme;

        public override BindingElementCollection CreateBindingElements() => new BindingElementCollection { this.messageEncodingBindingElement, this.transportBindingElement };

        internal void ApplySetting(string connectionString, string targetPartitionKey)
        {
            if (this.transportBindingElement == null)
            {
                throw new InvalidOperationException($"{nameof(this.transportBindingElement)} is null.");
            }

            this.transportBindingElement.ConnectionString = connectionString;
            this.transportBindingElement.TargetPartitionKey = targetPartitionKey;
        }

        public string ConnectionString
        {
            get => this.transportBindingElement.ConnectionString;
            set => this.transportBindingElement.ConnectionString = value;
        }

        public string TargetPartitionKey
        {
            get => this.transportBindingElement.TargetPartitionKey;
            set => this.transportBindingElement.TargetPartitionKey = value;
        }
    }
}