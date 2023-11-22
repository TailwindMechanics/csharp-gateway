

using Neurocache.Schema;

namespace Neurocache.Client
{
    public static class ClientOperation
    {
        public static async Task OperationChannel(OperationToken operationToken, Stream stream, HttpContext httpContext)
        {
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var taskToken = new TaskCompletionSource<bool>();
            while (!httpContext.RequestAborted.IsCancellationRequested)
            {
                UpdateLoop();

                await taskToken.Task;
            }

            void UpdateLoop()
            {

            }
        }
    }
}
