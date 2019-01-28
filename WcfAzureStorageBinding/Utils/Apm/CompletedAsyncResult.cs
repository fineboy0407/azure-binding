namespace AzureStorageBinding.Utils.Apm
{
    using System;
    using System.Threading;

    internal class CompletedAsyncResult : IAsyncResult
    {
        public CompletedAsyncResult(object state) => this.AsyncState = state;

        public object AsyncState { get; set; }

        public WaitHandle AsyncWaitHandle => new ManualResetEvent(true);

        public bool CompletedSynchronously => true;

        public bool IsCompleted => true;

        public static CompletedAsyncResult Create(AsyncCallback callback, object state)
        {
            var result = new CompletedAsyncResult(state);
            callback?.Invoke(result);
            return result;
        }
    }
}