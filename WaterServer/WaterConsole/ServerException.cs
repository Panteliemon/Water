using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WaterConsole;

public class ServerException : Exception
{
    public ServerException(string message): base(message)
    {
    }

    public static void ThrowIfError(HttpResponseMessage httpResponse)
    {
        if (!httpResponse.IsSuccessStatusCode)
        {
            string content = httpResponse.Content.ReadAsStringAsync().Result;
            string[] lines = content.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                const string exceptionMarker = "Exception: ";
                int exceptionMarkerIndex = lines[0].IndexOf(exceptionMarker);
                if (exceptionMarkerIndex >= 0)
                {
                    throw new ServerException(lines[0].Substring(exceptionMarkerIndex + exceptionMarker.Length));
                }
                else
                {
                    throw new ServerException(content);
                }
            }
            else
            {
                throw new ServerException(httpResponse.StatusCode.ToString());
            }
        }
    }
}
