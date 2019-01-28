namespace AzureStorageBinding.Table.Binding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    public class TableTransportBinding : Binding
    {
        private TableTransportBindingElement transportBindingElement;

        private MessageEncodingBindingElement messageEncodingBindingElement;

        public TableTransportBinding()
        {
            this.transportBindingElement = new TableTransportBindingElement();
            this.messageEncodingBindingElement = new TextMessageEncodingBindingElement();
        }

        public override BindingElementCollection CreateBindingElements()
        {
            return new BindingElementCollection() { this.transportBindingElement, this.messageEncodingBindingElement };
        }

        public override string Scheme => this.transportBindingElement.Scheme;

        internal void ApplySetting(string connectionString, string targetPartitionKey)
        {
            if (this.transportBindingElement == null)
            {
                throw new InvalidOperationException($"{nameof(this.transportBindingElement)} is null.");
            }

            this.transportBindingElement.ConnectionString = connectionString;
            this.transportBindingElement.TargetPartitionKey = targetPartitionKey;
        }
    }
}