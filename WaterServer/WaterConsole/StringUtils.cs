using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterServer.ModelSimple;

namespace WaterConsole;

internal static class StringUtils
{
    public static InputCommand ParseCommand(string str)
    {
        if (str == null)
        {
            return new InputCommand()
            {
                NameLower = null,
                Parameters = new List<string>()
            };
        }

        // Meaning of State see in switch-case
        int state = 0;
        StringBuilder sb = null;
        string commandName = string.Empty;
        List<string> parameters = new();

        for (int i=0; i<str.Length; i++)
        {
            char c = str[i];
            switch (state)
            {
                case 0: // before command name
                    if (!char.IsWhiteSpace(c))
                    {
                        sb = new StringBuilder();
                        sb.Append(c);
                        state = 1;
                    }
                    break;

                case 1: // assemblying command name
                    if (char.IsWhiteSpace(c))
                    {
                        commandName = sb.ToString();
                        sb = null;
                        state = 2;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;

                case 2: // expecting next parameter
                    if (!char.IsWhiteSpace(c))
                    {
                        sb = new StringBuilder();
                        if (c == '\"')
                        {
                            state = 4;
                        }
                        else if (c == '\'')
                        {
                            state = 5;
                        }
                        else
                        {
                            sb.Append(c);
                            state = 3;
                        }
                    }
                    break;

                case 3: // assemblying simple parameter
                    if (char.IsWhiteSpace(c))
                    {
                        parameters.Add(sb.ToString());
                        sb = null;
                        state = 2;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;

                case 4: // assemblying "" parameter
                    if (c == '\"')
                    {
                        parameters.Add(sb.ToString());
                        sb = null;
                        state = 2;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;

                case 5: // assemblying '' parameter
                    if (c == '\'')
                    {
                        parameters.Add(sb.ToString());
                        sb = null;
                        state = 2;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }

        // Check final state
        switch (state)
        {
            case 1:
                commandName = sb.ToString();
                break;
            case 3:
            case 4:
            case 5:
                parameters.Add(sb.ToString());
                break;
        }

        return new InputCommand()
        {
            NameLower = commandName.ToLower(),
            Parameters = parameters
        };
    }

    public static Tuple<DateTime, DateTime> ParseTaskTime(string str, DateTime today)
    {
        // <Input> ::= <DA>
        //             | <D>
        //             | <DA> <T> - <T>
        //             | <D> <T> - <T>
        //             | <D> <T> - <D> <T>
        // <DA> ::= 'A'
        //          | 'B'
        // <D> ::= <dd/MM>
        //         | <dd/MM/yyyy>
        // <T> ::= <hh>
        //         | <hh:mm>

        List<string> lexems = ParseTaskTime_Split(str);
        if (lexems.Count == 0)
            return null;

        int pos = 0;

        bool? daResult = ParseTaskTime_TryDA(lexems, ref pos, out DateOnly startDate, DateOnly.FromDateTime(today));
        if (daResult == false)
            return null;
        if (daResult == true)
        {
            // <DA>
            // <DA> <T> - <T>
            if (pos + 1 < lexems.Count)
            {
                pos++;
                bool? tResult1 = ParseTaskTime_TryT(lexems, ref pos, out TimeOnly startTime);
                if (tResult1 != true)
                    return null;

                if (pos + 1 < lexems.Count)
                {
                    pos++;
                    if (lexems[pos] == "-")
                    {
                        if (pos + 1 < lexems.Count)
                        {
                            pos++;
                            bool? tResult2 = ParseTaskTime_TryT(lexems, ref pos, out TimeOnly endTime);
                            if (tResult2 == true)
                            {
                                if (pos + 1 < lexems.Count)
                                    return null;

                                return new Tuple<DateTime, DateTime>(
                                    new DateTime(startDate, startTime),
                                    new DateTime(startDate, endTime)
                                );
                            }
                        }
                    }
                }

                return null;
            }
            else
            {
                return new Tuple<DateTime, DateTime>(
                    new DateTime(startDate, new TimeOnly(0, 0)),
                    new DateTime(startDate.AddDays(1), new TimeOnly(0, 0))
                );
            }
        }
        else
        {
            // <D>
            // <D> <T> - <T>
            // <D> <T> - <D> <T>
            bool? dResult1 = ParseTaskTime_TryD(lexems, ref pos, out startDate, today.Year);
            if (dResult1 != true)
                return null;

            if (pos + 1 < lexems.Count)
            {
                pos++;
                bool? tResult1 = ParseTaskTime_TryT(lexems, ref pos, out TimeOnly startTime);
                if (tResult1 != true)
                    return null;

                if (pos + 1 < lexems.Count)
                {
                    pos++;
                    if (lexems[pos] != "-")
                        return null;

                    if (pos + 1 < lexems.Count)
                    {
                        pos++;
                        bool? dResult2 = ParseTaskTime_TryD(lexems, ref pos, out DateOnly endDate, today.Year);
                        if (dResult2 == false)
                            return null;

                        if (dResult2 == true)
                        {
                            // <D> <T> - <D> <T>
                            if (pos + 1 < lexems.Count)
                            {
                                pos++;
                                bool? tResult2 = ParseTaskTime_TryT(lexems, ref pos, out TimeOnly endTime);
                                if (tResult2 != true)
                                    return null;

                                if (pos + 1 < lexems.Count)
                                    return null;

                                return new Tuple<DateTime, DateTime>(
                                    new DateTime(startDate, startTime),
                                    new DateTime(endDate, endTime)
                                );
                            }

                            return null;
                        }
                        else
                        {
                            // <D> <T> - <T>
                            bool? tResult2 = ParseTaskTime_TryT(lexems, ref pos, out TimeOnly endTime);
                            if (tResult2 != true)
                                return null;

                            if (pos + 1 < lexems.Count)
                                return null;

                            return new Tuple<DateTime, DateTime>(
                                new DateTime(startDate, startTime),
                                new DateTime(startDate, endTime)
                            );
                        }
                    }
                }

                return null;
            }
            else
            {
                return new Tuple<DateTime, DateTime>(
                    new DateTime(startDate, new TimeOnly(0, 0)),
                    new DateTime(startDate.AddDays(1), new TimeOnly(0, 0))
                );
            }
        }
    }

    #region ParseTaskTime

    // Syntax parsing:
    // - pos: initially: on first lexem (potentially) belonging to the syntax node
    //        after successfull call: on last lexem still belonging to the parsed syntax node
    //        after failed call: unchanged
    //        Always must remain within range.
    // - out result: filled if successful
    // - returns: true: parsed successfully
    //            false: recognized but contains error (definitely), impossible to parse
    //            null: not recognized, try parse another syntax node

    private static bool? ParseTaskTime_TryDA(List<string> lexems, ref int pos, out DateOnly result,
        DateOnly today)
    {
        result = DateOnly.MinValue;
        if ((lexems[pos] == "a") || (lexems[pos] == "A"))
        {
            result = today.AddDays(1);
            return true;
        }
        else if ((lexems[pos] == "b") || (lexems[pos] == "B"))
        {
            result = today;
            return true;
        }

        return null;
    }

    private static bool? ParseTaskTime_TryD(List<string> lexems, ref int pos, out DateOnly result,
        int todayYear)
    {
        result = DateOnly.MinValue;
        if ((pos + 2 < lexems.Count)
            && (lexems[pos + 1] == "/"))
        {
            // "Recognized". Now must obey rules.
            if (int.TryParse(lexems[pos], out int day)
                && int.TryParse(lexems[pos + 2], out int month))
            {
                if (pos + 3 < lexems.Count)
                {
                    if (lexems[pos + 3] == "/")
                    {
                        if (pos + 4 < lexems.Count)
                        {
                            if (int.TryParse(lexems[pos + 4], out int year))
                            {
                                if (year < 100)
                                    year += 2000;

                                try
                                {
                                    result = new DateOnly(year, month, day);
                                    pos += 4;
                                    return true;
                                }
                                catch (Exception)
                                {
                                    // To fallback
                                }
                            }
                        }

                        return false;
                    }
                }

                // dd/MM only.
                try
                {
                    result = new DateOnly(todayYear, month, day);
                    pos += 2;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        return null;
    }

    private static bool? ParseTaskTime_TryT(List<string> lexems, ref int pos, out TimeOnly result)
    {
        result = TimeOnly.MinValue;
        if (ParseTaskTime_GetHours(lexems[pos], out int hours))
        {
            if (pos + 1 < lexems.Count)
            {
                if (lexems[pos + 1] == ":")
                {
                    if (pos + 2 < lexems.Count)
                    {
                        if (ParseTaskTime_GetMinutes(lexems[pos + 2], out int minutes))
                        {
                            pos += 2;
                            result = new TimeOnly(hours, minutes);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    result = new TimeOnly(hours, 0);
                    return true;
                }
            }
            else
            {
                result = new TimeOnly(hours, 0);
                return true;
            }
        }

        return null;
    }

    private static bool ParseTaskTime_GetHours(string lexem, out int value)
    {
        if (int.TryParse(lexem, out value))
        {
            if ((value >= 0) && (value < 24))
                return true;
        }

        return false;
    }

    private static bool ParseTaskTime_GetMinutes(string lexem, out int value)
    {
        if (int.TryParse(lexem, out value))
        {
            if ((value >= 0) && (value < 60))
                return true;
        }

        return false;
    }

    private static List<string> ParseTaskTime_Split(string str)
    {
        List<string> result = new();
        StringBuilder sb = null;

        int state = 0;
        for (int i=0; i<str.Length; i++)
        {
            char c = str[i];
            switch (state)
            {
                case 0:
                    if (c == ':')
                    {
                        result.Add(":");
                    }
                    else if (c == '/')
                    {
                        result.Add("/");
                    }
                    else if (c == '-')
                    {
                        result.Add("-");
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        // :)
                    }
                    else
                    {
                        sb = new StringBuilder();
                        sb.Append(c);
                        state = 1;
                    }
                    break;

                case 1:
                    if (c == ':')
                    {
                        result.Add(sb.ToString());
                        sb = null;
                        result.Add(":");
                        state = 0;
                    }
                    else if (c == '/')
                    {
                        result.Add(sb.ToString());
                        sb = null;
                        result.Add("/");
                        state = 0;
                    }
                    else if (c == '-')
                    {
                        result.Add(sb.ToString());
                        sb = null;
                        result.Add("-");
                        state = 0;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        result.Add(sb.ToString());
                        sb = null;
                        state = 0;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }

        if (state == 1)
            result.Add(sb.ToString());

        return result;
    }

    #endregion

    public static List<string> SplitIntoLines(string str)
    {
        List<string> result = new List<string>();
        if (string.IsNullOrEmpty(str))
        {
            result.Add(str);
            return result;
        }

        int state = 0;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            switch (state)
            {
                case 0:
                    if (c == 13)
                    {
                        state = 1;
                    }
                    else if (c == 10)
                    {
                        result.Add(sb.ToString());
                        sb = new StringBuilder();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
                case 1: // after 13
                    if (c == 13)
                    {
                        sb.Append((char)13);
                        // stay in state 1
                    }
                    else if (c == 10)
                    {
                        result.Add(sb.ToString());
                        sb = new StringBuilder();
                        state = 0;
                    }
                    else
                    {
                        sb.Append((char)13);
                        sb.Append(c);
                        state = 0;
                    }
                    break;
            }
        }

        if (state == 1)
        {
            sb.Append((char)13);
        }

        result.Add(sb.ToString());

        return result;
    }

    public static string TaskStatusToGridStr(STaskStatus taskStatus)
    {
        switch (taskStatus)
        {
            case STaskStatus.NotStarted: return "[N/S]";
            case STaskStatus.InProgress: return "[PRG...]";
            case STaskStatus.Success: return "[DONE]";
            case STaskStatus.LowRate: return "[ERR:L]";
            case STaskStatus.NoCounter: return "[ERR:C]";
            case STaskStatus.Error: return "[ERR]";
        }

        return "[???]";
    }
}
