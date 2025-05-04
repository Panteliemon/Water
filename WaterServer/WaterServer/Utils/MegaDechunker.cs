using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WaterServer.Utils;

public static class MegaDechunker // naming so I know that it is neither built-in nor some 3rd party library
{
    public static void IntegrateMegaDechunker(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            Endpoint endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.OfType<NoChunkingPleaseAttribute>().Any() == true)
            {
                Stream originalBodyStream = context.Response.Body;
                using (MemoryStream memoryBodyStream = new())
                {
                    context.Response.Body = memoryBodyStream;
                    long contentLength = 0;

                    context.Response.OnStarting(() =>
                    {
                        context.Response.Headers.ContentLength = contentLength;
                        return Task.CompletedTask;
                    });

                    await next(context);

                    contentLength = context.Response.Body.Length;
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    await context.Response.Body.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                await next(context);
            }
        });
    }
}
