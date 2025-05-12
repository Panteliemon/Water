namespace WaterServer.Utils;

public static class Parse
{
    /// <summary>
    /// Read the subj from the substring
    /// </summary>
    /// <param name="str"></param>
    /// <param name="pos">Must be within range always.
    /// Initial position: where the first digit of an integer is supposed to be.
    /// Final position if success: on the last digit of the integer.
    /// Final position if failed: not defined.</param>
    /// <param name="value"></param>
    /// <returns>True if parsed succesfully</returns>
    public static bool ReadPositiveInteger(string str, ref int pos, out int value)
    {
        value = str[pos] - '0';
        if ((value > 9) || (value < 0))
            return false;

        while (true)
        {
            int nextPos = pos + 1;
            if (nextPos < str.Length)
            {
                char nextChar = str[nextPos];
                if ((nextChar >= '0') && (nextChar <= '9'))
                {
                    // 2147483647
                    if (value > 214748364)
                        return false;
                    int digit = nextChar - '0';
                    if ((value == 214748364) && (digit > 7))
                        return false;

                    value = value * 10 + digit;
                    pos = nextPos;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
