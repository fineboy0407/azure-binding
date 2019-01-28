namespace AzureStorageBinding.Table.Binding
{
    using System;
    using System.ServiceModel.Channels;

    using AzureStorageBinding.Table.Channel;

    public class TableTransportBindingElement : TransportBindingElement
    {
        public TableTransportBindingElement()
        {
        }

        public override BindingElement Clone() => new TableTransportBindingElement { ConnectionString = this.ConnectionString, TargetPartitionKey = this.TargetPartitionKey };

        public string TargetPartitionKey { get; set; }

        public string ConnectionString { get; set; }

        public override string Scheme { get; } = "az.table";

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IRequestChannel);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IReplyChannel);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw new ArgumentException($"Expected {nameof(IRequestChannel)}, got {typeof(TChannel).Name}.");
            }

            return (IChannelFactory<TChannel>)new TableRequestChannelFactory(context, this);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw new ArgumentException($"Expected {nameof(IRequestChannel)}, got {typeof(TChannel).Name}.");
            }

            return (IChannelListener<TChannel>)new TableReplyChannelListener(context, this);
        }
    }
}