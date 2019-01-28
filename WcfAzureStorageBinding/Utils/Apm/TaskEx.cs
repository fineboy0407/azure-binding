namespace AzureStorageBinding.Utils.Apm
{
    using System;
    using System.Threading.Tasks;

    internal static class TaskEx
    {
        public static IAsyncResult AsApm<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var tcs = new TaskCompletionSource<T>(state);
            task.ContinueWith(
                t =>
                    {
                        if (t.IsFaulted)
                        {
                            tcs.TrySetException(t.Exception.InnerExceptions);
                        }
                        else if (t.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else
                        {
                            tcs.TrySetResult(t.Result);
                        }

                        callback?.Invoke(tcs.Task);
                    },
                TaskScheduler.Default);
            return tcs.Task;
        }

        public static IAsyncResult AsApm(this Task task, AsyncCallback callback, object state)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var tcs = new TaskCompletionSource<object>(state);
            task.ContinueWith(
                t =>
                    {
                        if (t.IsFaulted)
                        {
                            tcs.TrySetException(t.Exception.InnerExceptions);
                        }
                        else if (t.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else
                        {
                            tcs.TrySetResult(null);
                        }

                        callback?.Invoke(tcs.Task);
                    },
                TaskScheduler.Default);
            return tcs.Task;
        }

        public static T GetApmTaskResult<T>(this IAsyncResult result)
        {
            if (result is Task<T> apmTask)
            {
                return apmTask.Result;
            }

            throw new InvalidOperationException($"{nameof(GetApmTaskResult)} can only get result type {nameof(T)} on {nameof(Task<T>)}");
        }

        public static void WaitApmTask(this IAsyncResult result)
        {
            if (result is Task apmTask)
            {
                apmTask.Wait();
            }
            else
            {
                throw new InvalidOperationException($"{nameof(WaitApmTask)} can only wait on {nameof(Task)}");
            }
        }
    }
}