using System;
using System.Collections.Generic;
using System.Linq;
using WaterServer.ModelSimple;

namespace WaterServer.Models;

public class ClientTaskResult
{
    public int TaskId { get; }
    public IReadOnlyList<ClientTaskResultSegment> Segments { get; }

    public ClientTaskResult(int taskId, IReadOnlyList<ClientTaskResultSegment> segments)
    {
        TaskId = taskId;
        Segments = segments;
    }

    public static bool TryParse(string requestStr, out ClientTaskResult clientTaskResult)
    {
        clientTaskResult = null;
        if (string.IsNullOrEmpty(requestStr))
            return false;

        int? taskId = null;
        List<ClientTaskResultSegment> segments = new();

        int pos = 0;
        while (pos < requestStr.Length)
        {
            char c = requestStr[pos];
            if (c == 'T')
            {
                // Not allowed to repeat
                if (taskId.HasValue)
                    return false;

                if (pos + 1 < requestStr.Length)
                {
                    pos++;
                    if (!ReadPositiveInteger(requestStr, ref pos, out int parsedTaskId))
                        return false;

                    taskId = parsedTaskId;
                    pos++;
                }
                else
                {
                    return false;
                }
            }
            else if (c == 'I')
            {
                if (!ReadSegment(requestStr, ref pos, out ClientTaskResultSegment segment))
                    return false;

                // We allow plant index duplicates in list.

                segments.Add(segment);
                pos++;
            }
            else
            {
                // Roll over until the next known control char
                pos++;
            }
        }

        if (!taskId.HasValue)
            return false;
        if (segments.Count == 0)
            return false;

        clientTaskResult = new ClientTaskResult(taskId.Value, segments);
        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="str"></param>
    /// <param name="pos">Must be within range always.
    /// Initial position: on the 'I' (not checked).
    /// Final position if success: on the last symbol which still belongs to this segment.
    /// Final position if failed: not defined.</param>
    /// <param name="segment"></param>
    private static bool ReadSegment(string str, ref int pos, out ClientTaskResultSegment segment)
    {
        segment = default;

        if (pos + 1 < str.Length)
        {
            pos++;
            if (!ReadPositiveInteger(str, ref pos, out int parsedIndex))
                return false;
            if (parsedIndex > SPlant.MAX_INDEX)
                return false;

            STaskStatus? taskStatus = null;
            if (pos + 1 < str.Length)
            {
                pos++;
                while (pos < str.Length)
                {
                    char c = str[pos];
                    if (c == 'R')
                    {
                        // Not allowed to repeat
                        if (taskStatus.HasValue)
                            return false;

                        if (pos + 1 < str.Length)
                        {
                            pos++;
                            if (!ReadPositiveInteger(str, ref pos, out int parsedStatusInt))
                                return false;

                            STaskStatus taskStatusValue = (STaskStatus)parsedStatusInt;
                            if (!Enum.IsDefined(typeof(STaskStatus), taskStatusValue))
                                taskStatusValue = STaskStatus.Unknown;

                            // Client really cannot report "not started".
                            // "In progress" however is allowed - who knows how clients could work.
                            if (taskStatusValue == STaskStatus.NotStarted)
                                return false;

                            taskStatus = taskStatusValue;
                            pos++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if ((c == 'I') || (c == 'T'))
                    {
                        // Exit by encountering root-level control character.
                        // Move back (according to spec)
                        pos--;

                        if (taskStatus.HasValue)
                        {
                            segment = new ClientTaskResultSegment(parsedIndex, taskStatus.Value);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Skip until known control character
                        pos++;
                    }
                }

                if (taskStatus.HasValue)
                {
                    // Exit by reading to the end
                    segment = new ClientTaskResultSegment(parsedIndex, taskStatus.Value);
                    return true;
                }
                else
                {
                    // Status is mandatory.
                    return false;
                }
            }
            else
            {
                // Need some attributes related to parsed plant index.
                return false;
            }
        }
        else
        {
            // Need plant index after 'I'
            return false;
        }
    }

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
    private static bool ReadPositiveInteger(string str, ref int pos, out int value)
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
