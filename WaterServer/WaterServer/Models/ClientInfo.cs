using WaterServer.Utils;

namespace WaterServer.Models;

public class ClientInfo
{
    public int CountsPerLiter { get; }

    public ClientInfo(int countsPerLiter)
    {
        CountsPerLiter = countsPerLiter;
    }

    public static bool TryParse(string requestStr, out ClientInfo result)
    {
        result = null;
        if (string.IsNullOrEmpty(requestStr))
            return false;

        int? countsPerLiter = null;

        int pos = 0;
        while (pos < requestStr.Length)
        {
            char c = requestStr[pos];
            if (c == 'C')
            {
                if (countsPerLiter.HasValue)
                    return false; // No duplication.

                if (pos + 1 < requestStr.Length)
                {
                    pos++;
                    if (!Parse.ReadPositiveInteger(requestStr, ref pos, out int parsedCPR))
                        return false;

                    // No validation on server's side.
                    countsPerLiter = parsedCPR;
                    pos++;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Skip through unknown
                pos++;
            }
        }

        if (!countsPerLiter.HasValue)
            return false;

        result = new ClientInfo(countsPerLiter.Value);
        return true;
    }
}
