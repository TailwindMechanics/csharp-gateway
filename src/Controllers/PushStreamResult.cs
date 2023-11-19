//path: src\Controllers\PushStreamResult.cs

using Microsoft.AspNetCore.Mvc;

namespace Neurocache.Controllers.Agent
{
    public class OperationChannel(
        Func<Stream, HttpContext, Task> onStreaming,
        string contentType
    ) : IActionResult
    {
        readonly Func<Stream, HttpContext, Task> onStreaming = onStreaming;
        readonly string contentType = contentType;

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = contentType;

            await using var stream = response.Body;
            await onStreaming(stream, context.HttpContext);
        }
    }
}
