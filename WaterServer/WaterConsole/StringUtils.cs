using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
