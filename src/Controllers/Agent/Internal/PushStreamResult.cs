//path: src\Controllers\Agent\Internal\PushStreamResult.cs

using Microsoft.AspNetCore.Mvc;

namespace Neurocache.Gateway.Controllers.Agent
{
    public class PushStreamResult : IActionResult
    {
        private readonly Func<Stream, HttpContext, Task> _onStreaming;
        private readonly string _contentType;

        public PushStreamResult(Func<Stream, HttpContext, Task> onStreaming, string contentType)
        {
            _onStreaming = onStreaming;
            _contentType = contentType;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = _contentType;

            await using (var stream = response.Body)
            {
                await _onStreaming(stream, context.HttpContext);
            }
        }
    }
}
