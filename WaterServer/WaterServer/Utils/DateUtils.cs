using System;

namespace WaterServer.Utils;

public static class DateUtils
{
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// For UTC dates only!
    /// </summary>
    public static long UtcToJsMilliseconds(this DateTime dateTime)
    {
        return (long)Math.Round(dateTime.Subtract(Epoch).TotalMilliseconds);
    }
}
