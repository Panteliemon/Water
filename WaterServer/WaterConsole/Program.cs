using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WaterServer.ModelSimple;
using WaterServer.Xml;

namespace WaterConsole;

internal class Program
{
    const ConsoleColor USERINPUT = ConsoleColor.Gray;
    const ConsoleColor INFO = ConsoleColor.Cyan;
    const ConsoleColor WARNING = ConsoleColor.Yellow;
    const ConsoleColor ERROR = ConsoleColor.Red;
    const ConsoleColor REQUEST = ConsoleColor.Green;

    static Connector connector;
    static SModel model;

    static void Main(string[] args)
    {
        try
        {
            PrintHello();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["Debug"] =
#if DEBUG
                    "1"
#else
                    "0"
#endif
                })
                .AddJsonFile("config.json")
                .Build();
            connector = new Connector(configuration);

            // Main cycle
            while (true)
            {
                InColor(INFO, () => Console.Write("> "));

                string commandRaw = Console.ReadLine();
                InputCommand cmd = StringUtils.ParseCommand(commandRaw);
                
                if ((cmd.NameLower == "q") || (cmd.NameLower == "exit"))
                {
                    return;
                }
                else if ((cmd.NameLower == "h") || (cmd.NameLower == "help"))
                {
                    PrintHelp();
                }
                else if (cmd.NameLower == "cls")
                {
                    Console.Clear();
                }
                else if ((cmd.NameLower == "add") || (cmd.NameLower == "new"))
                {
                    CompoundCmdWrapper(cmd.Parameters, new string[] { "plant", "task" }, parameterLower =>
                    {
                        if (parameterLower == "plant")
                        {
                            ExecuteAddPlant();
                        }
                        else if (parameterLower == "task")
                        {

                        }
                    });
                }
                else if ((cmd.NameLower == "edit") || (cmd.NameLower == "update"))
                {
                    CompoundCmdWrapper(cmd.Parameters, new string[] { "plant", "task" }, parameterLower =>
                    {
                        if (parameterLower == "plant")
                        {
                            ExecuteUpdatePlant(cmd.Parameters.Skip(1).ToList());
                        }
                        else if (parameterLower == "task")
                        {

                        }
                    });
                }
                else if (cmd.NameLower == "delete")
                {
                    CompoundCmdWrapper(cmd.Parameters, new string[] { "plant", "task" }, parameterLower =>
                    {
                        if (parameterLower == "plant")
                        {
                            ExecuteDeletePlant(cmd.Parameters.Skip(1).ToList());
                        }
                        else if (parameterLower == "task")
                        {

                        }
                    });
                }
                else if ((cmd.NameLower == "pull") || (cmd.NameLower == "reload"))
                {
                    ExecutePull();
                }
                else if ((cmd.NameLower == "pl") || (cmd.NameLower == "plants"))
                {
                    ExecutePlantList(cmd.Parameters);
                }
                else if ((cmd.NameLower == "tl") || (cmd.NameLower == "tasks"))
                {

                }
                else if (cmd.NameLower == "status")
                {

                }
                else if (cmd.NameLower == "nt")
                {

                }
                else if (cmd.NameLower == "fail")
                {
                    throw new Exception("NOOOOOOOOO!!!");
                }
                else
                {
                    InColor(WARNING, () =>
                    {
                        Console.WriteLine($"Unknown command: {cmd.NameLower.ToUpper()}");
                        PrintCommaSeparated(new[] { "h", "help" }, ", ", USERINPUT);
                        Console.WriteLine(" for the list of supported commands.");
                    });
                }
            }
        }
        catch (Exception ex)
        {
            InColor(ERROR, () =>
            {
                Console.WriteLine($"[x] Unhandled exception: {ex.Message}");
                InColorLn(WARNING, ex.StackTrace);
                Console.WriteLine("App terminated");
            });
        }
    }

    static void PrintHello()
    {
        InColor(INFO, () =>
        {
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("Waterer 2.0 Control Console");
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine();
            Console.WriteLine("Quick start: ");
            InColor(USERINPUT, () => Console.Write("  tl"));
            Console.WriteLine(" - view list of tasks");
            InColor(USERINPUT, () => Console.Write("  nt"));
            Console.WriteLine(" - add new task");
            PrintCommaSeparated(new[] { "  h", "help" }, " or ", USERINPUT);
            Console.WriteLine(" - get help");
            PrintCommaSeparated(new[] { "  q", "exit" }, " or ", USERINPUT);
            Console.WriteLine(" - exit");
            Console.WriteLine();
        });
    }

    static void PrintHelp()
    {
        InColor(INFO, () =>
        {
            Console.WriteLine("TODO...");
        });
    }

    #region Commands

    static bool ExecutePull()
    {
        return NetworkingWrapper("Fetching data from server...", () =>
        {
            SModel newModel = connector.Pull();
            model = newModel;
        });
    }

    static void ExecutePlantList(IReadOnlyList<string> parameters)
    {
        if (!AutoPull())
            return;

        bool[] flags = ReadSimpleFlags(new char[] { 'x' }, parameters);
        bool isXml = flags[0];

        if (isXml)
        {
            for (int i = 0; i < model.Plants.Count; i++)
            {
                InColorLn(INFO, ModelXml.PlantToStr(model.Plants[i]));
            }
        }
        else
        {
            GridBuilder gb = new();
            gb[0, 0] = "Valve No";
            gb[0, 1] = "Type";
            gb.ColumnSeparator = "   ";
            gb.HeaderRowSeparator = '-';

            for (int i = 0; i < model.Plants.Count; i++)
            {
                gb[i + 1, 0] = model.Plants[i].ValveNo.ToString();
                gb[i + 1, 1] = model.Plants[i].PlantType.ToString();
            }

            InColorLn(INFO, gb.ToString());
        }
    }

    static void ExecuteAddPlant()
    {
        if (!AutoPull())
            return;

        if (model.Plants.Count >= 8)
        {
            InColorLn(WARNING, "Plant list full! Cannot add more plants");
            return;
        }

        bool[] isIndexOccupied = new bool[SPlant.MAX_COUNT];
        foreach (SPlant plant in model.Plants)
        {
            if ((plant.Index >= 0) && (plant.Index < isIndexOccupied.Length))
                isIndexOccupied[plant.Index] = true;
        }
        List<int> availableIndicies = new();
        for (int i = 0; i < isIndexOccupied.Length; i++)
        {
            if (!isIndexOccupied[i])
                availableIndicies.Add(i);
        }

        int plantIndex = -1;
        if (availableIndicies.Count == 1)
        {
            plantIndex = availableIndicies[0];
            InColorLn(INFO, $"Valve Number is automatically set to {plantIndex + 1} as the only available option.");
        }
        else
        {
            plantIndex = QueryInt("Enter plant's Valve Number", availableIndicies.Select(x => x + 1)) - 1;
        }

        SPlantType plantType = QueryEnum("Enter plant's Type:",
            Enum.GetValues<SPlantType>().Where(x => x != SPlantType.Unused));

        InColorLn(INFO, $"Valve Number: {plantIndex + 1}");
        InColorLn(INFO, $"Type: {plantType}");

        bool save = AskYesNo("Save to server?");
        if (save)
        {
            SPlant plant = new()
            {
                Index = plantIndex,
                PlantType = plantType
            };

            if (NetworkingWrapper("Sending data...", () =>
                {
                    connector.AddPlant(plant);
                }))
            {
                ExecutePull();
            }
            else
            {
                InColorLn(ERROR, "Add plant aborted");
            }
        }
        else
        {
            InColorLn(INFO, "Add plant aborted");
        }
    }

    static void ExecuteUpdatePlant(IReadOnlyList<string> truncatedParameters)
    {
        if (!AutoPull())
            return;

        SPlant plant = SelectPlantForUpdate(truncatedParameters);
        if (plant == null)
            return;

        SPlantType newType = QueryEnum("Enter new Type:",
            Enum.GetValues<SPlantType>().Where(x => x != SPlantType.Unused));

        InColorLn(INFO, $"Valve Number: {plant.ValveNo}");
        InColorLn(INFO, $"Type: {plant.PlantType} -> {newType}");

        if (AskYesNo("Save to server?"))
        {
            plant.PlantType = newType;
            if (NetworkingWrapper("Sending data...", () =>
                {
                    connector.UpdatePlant(plant);
                }))
            {
                ExecutePull();
            }
            else
            {
                InColorLn(ERROR, "Update plant aborted");
            }
        }
        else
        {
            InColorLn(INFO, "Update plant aborted");
        }
    }

    static void ExecuteDeletePlant(IReadOnlyList<string> truncatedParameters)
    {
        if (!AutoPull())
            return;

        SPlant plant = SelectPlantForUpdate(truncatedParameters);
        if (plant == null)
            return;

        if (AskYesNo("Delete?"))
        {
            if (model.Tasks.Any(t => t.Items.Any(item => item.Plant == plant)))
            {
                InColorLn(WARNING, "This plant is used in tasks.");
                InColorLn(WARNING, "Deleting this plant will delete its entries from all tasks.");
            }

            if (AskYesNo("[!] DELETE PLANT", WARNING))
            {
                if (NetworkingWrapper("Sending delete request...", () =>
                    {
                        connector.DeletePlant(plant.Index);
                    }))
                {
                    ExecutePull();
                }
            }
        }
    }

    static SPlant SelectPlantForUpdate(IReadOnlyList<string> truncatedParameters)
    {
        if (model.Plants.Count == 0)
        {
            InColorLn(INFO, "There are no plants");
            return null;
        }

        SPlant plant = null;
        if (truncatedParameters.Count > 0)
        {
            if (int.TryParse(truncatedParameters[0].Trim(), out int valveNo))
            {
                plant = model.Plants.FirstOrDefault(x => x.ValveNo == valveNo);
                if (plant == null)
                {
                    InColorLn(WARNING, $"Plant with Valve Number {valveNo} doesn't exist");
                }
            }
            else
            {
                InColorLn(WARNING, $"Invalid parameter: {truncatedParameters[0]}");
            }
        }

        if (plant == null)
        {
            if (model.Plants.Count == 1)
            {
                plant = model.Plants[0];
                InColorLn(INFO, $"Plant {plant.ValveNo} was automatically selected as the only available option.");
            }
            else
            {
                int valveNo = QueryInt("Enter plant's Valve Number", model.Plants.Select(x => x.ValveNo));
                plant = model.Plants.First(x => x.ValveNo == valveNo);
            }
        }

        InColorLn(INFO, $"Valve Number: {plant.ValveNo}");
        InColorLn(INFO, $"Type: {plant.PlantType}");
        return plant;
    }

    #endregion

    #region Input "Dialogs"

    static int QueryInt(string prompt, IEnumerable<int> options)
    {
        while (true)
        {
            InColor(REQUEST, () =>
            {
                Console.Write(prompt);
                Console.Write(" (");
                PrintCommaSeparated(options.Select(x => x.ToString()).ToArray(), ", ", USERINPUT);
                Console.WriteLine("):");
                Console.Write("> ");
            });

            string str = Console.ReadLine().Trim();
            if (int.TryParse(str, out int parsed))
            {
                if (options.Contains(parsed))
                {
                    return parsed;
                }
                else
                {
                    InColorLn(WARNING, "Please only use suggested values");
                }
            }
            else
            {
                InColorLn(WARNING, "Please only use suggested integer numbers");
            }
        }
    }

    static T QueryEnum<T>(string prompt, IEnumerable<T> options)
        where T: struct
    {
        if (!typeof(T).IsSubclassOf(typeof(Enum)))
            throw new InvalidOperationException();
        if (!options.Any())
            throw new InvalidOperationException();

        InColorLn(REQUEST, prompt);

        while (true)
        {
            InColor(REQUEST, () =>
            {
                Console.WriteLine("Options:");
                foreach (T option in options)
                {
                    Console.Write("  ");
                    InColor(USERINPUT, Enum.Format(typeof(T), option, "d"));
                    Console.Write(" / ");
                    InColorLn(USERINPUT, Enum.GetName(typeof(T), option));
                }

                Console.Write("> ");
            });

            string str = Console.ReadLine().Trim();
            if (int.TryParse(str, out int parsed))
            {
                if (Enum.IsDefined(typeof(T), parsed))
                {
                    return (T)Enum.ToObject(typeof(T), parsed);
                }
                else
                {
                    InColorLn(WARNING, "Please only use suggested integer or text values");
                }
            }
            else
            {
                List<T> matchedExactly = new();
                List<T> matchedInDifferentCase = new();
                string strLower = str.ToLower();
                foreach (T option in options)
                {
                    if (Enum.GetName(typeof(T), option).ToLower() == strLower)
                    {
                        matchedInDifferentCase.Add(option);
                        if (Enum.GetName(typeof(T), option) == str)
                        {
                            matchedExactly.Add(option);
                        }
                    }
                }

                if (matchedExactly.Count == 1)
                {
                    return matchedExactly[0];
                }
                else if (matchedInDifferentCase.Count == 1)
                {
                    return matchedInDifferentCase[0];
                }
                else if (matchedInDifferentCase.Count > 1)
                {
                    InColorLn(WARNING, "Please use exact lowercase/uppercase spelling because several values match your input.");
                }
                else
                {
                    InColorLn(WARNING, "Please only use suggested integer or text values");
                }
            }
        }
    }

    static bool AskYesNo(string question, ConsoleColor color = REQUEST)
    {
        while (true)
        {
            InColor(color, () =>
            {
                Console.Write(question);
                Console.Write(" (");
                InColor(USERINPUT, "y");
                Console.Write("/");
                InColor(USERINPUT, "N");
                Console.Write("): ");
            });

            string answer = Console.ReadLine().Trim();
            string answerLower = answer.ToLower();
            if (answerLower == "y")
            {
                return true;
            }
            else if (answerLower == "n")
            {
                return false;
            }
            else
            {
                InColorLn(WARNING, "Please only use Y or N");
            }
        }
    }

    static bool[] ReadSimpleFlags(IReadOnlyList<char> lowerCaseFlags, IReadOnlyList<string> parameters)
    {
        bool[] result = new bool[lowerCaseFlags.Count];

        if (parameters.Count > 0)
        {
            if (parameters[0].Length > 1)
            {
                // All flags concatenated from the first parameter
                string lower = parameters[0].ToLower();
                HashSet<char> unknowns = new();
                for (int i = 0; i < lower.Length; i++)
                {
                    char flagCharLower = lower[i];
                    int index = IndexOfChar(lowerCaseFlags, flagCharLower);
                    if (index >= 0)
                    {
                        result[index] = true;
                    }
                    else
                    {
                        if (!unknowns.Contains(flagCharLower))
                        {
                            InColorLn(INFO, $"Unsupported flag: {flagCharLower}");
                            unknowns.Add(flagCharLower);
                        }
                    }
                }
            }
            else
            {
                // Flags in parameters individually
                for (int i = 0; i < parameters.Count; i++)
                {
                    string parameter = parameters[i];
                    if (parameter.Length > 0)
                    {
                        if (parameter.Length == 1)
                        {
                            char flagCharLower = parameter.ToLower()[0];
                            int index = IndexOfChar(lowerCaseFlags, flagCharLower);
                            if (index >= 0)
                            {
                                result[index] = true;
                            }
                            else
                            {
                                InColorLn(INFO, $"Unsupported flag: {parameter}");
                            }
                        }
                        else
                        {
                            InColorLn(INFO, $"Unknown command parameter: {parameter}");
                        }
                    }
                }
            }
        }

        return result;

        static int IndexOfChar(IReadOnlyList<char> list, char c)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == c)
                    return i;
            }

            return -1;
        }
    }

    #endregion

    #region Helpers for Commands

    static bool AutoPull()
    {
        if (model == null)
        {
            if (!ExecutePull())
            {
                InColorLn(ERROR, "Command execution aborted due to failed contact with server.");
                return false;
            }
        }

        return true;
    }

    static void CompoundCmdWrapper(IReadOnlyList<string> parameters,
        string[] supportedParametersLower, Action<string> action)
    {
        if (parameters.Count > 0)
        {
            string parameterLower = parameters[0].ToLower();
            if (supportedParametersLower.Contains(parameterLower))
            {
                action(parameterLower);
            }
            else
            {
                InColor(WARNING, () =>
                {
                    Console.WriteLine($"Unknown command parameter: {parameters[0]}");
                    Console.Write("Supported parameters: ");
                    PrintCommaSeparated(supportedParametersLower, ", ", USERINPUT);
                    Console.WriteLine();
                });
            }
        }
        else
        {
            InColor(WARNING, () =>
            {
                Console.WriteLine("Missing command parameter.");
                Console.Write("Supported parameters: ");
                PrintCommaSeparated(supportedParametersLower, ", ", USERINPUT);
                Console.WriteLine();
            });
        }
    }

    static bool NetworkingWrapper(string infoMessage, Action networkingAction)
    {
        try
        {
            InColor(INFO, infoMessage);
            networkingAction();
            InColorLn(INFO, " [ OK ]");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            InColorLn(ERROR, $"[x] Error: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Color Helpers

    /// <summary>
    /// Executes action while console font is having specified color.
    /// Then returns previous color of the console font.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="action"></param>
    static void InColor(ConsoleColor color, Action action)
    {
        ConsoleColor previousColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            action();
        }
        finally
        {
            Console.ForegroundColor = previousColor;
        }
    }

    static void InColor(ConsoleColor color, string str)
    {
        InColor(color, () => Console.Write(str));
    }

    static void InColorLn(ConsoleColor color, string str)
    {
        InColor(color, () => Console.WriteLine(str));
    }

    static void PrintCommaSeparated(string[] items, string separator, ConsoleColor itemColor)
    {
        if ((items != null) && (items.Length > 0))
        {
            for (int i = 0; i < items.Length - 1; i++)
            {
                InColor(itemColor, items[i]);
                Console.Write(separator);
            }

            InColor(itemColor, items[items.Length - 1]);
        }
    }

    #endregion
}
