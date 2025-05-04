using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace WaterServer.Utils;

public static class WebExtensions
{
    // .NET doesn't have out-of-the-box formatter for text/plain
    // so cannot use model binding o_O
    public static async Task<string> ReadBodyAsString(this HttpRequest request)
    {
        string result;
        using (StreamReader sr = new StreamReader(request.Body, leaveOpen: true))
        {
            result = await sr.ReadToEndAsync();
        }

        return result;
    }
}
