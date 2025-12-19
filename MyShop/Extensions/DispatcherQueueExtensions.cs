using System;
using System.Threading.Tasks;

namespace MyShop.Extensions
{
    public static class DispatcherQueueExtensions
    {
        public static Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<Task> function)
        {
            var tcs = new TaskCompletionSource();

            dispatcher.TryEnqueue(async () =>
            {
                try
                {
                    await function();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
