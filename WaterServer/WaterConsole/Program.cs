using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    const ConsoleColor FORMAT = ConsoleColor.Red;

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
                            ExecuteAddTask(cmd.Parameters.Skip(1).ToList());
                        }
                    });
                }
                else if ((cmd.NameLower == "edit") || (cmd.NameLower == "update"))
                {
                    CompoundCmdWrapper(cmd.Parameters, new string[] { "plant", "task" }, parameterLower =>
                    {
                        if (parameterLower == "plant")
                        {
                            ExecuteEditPlant(cmd.Parameters.Skip(1).ToList());
                        }
                        else if (parameterLower == "task")
                        {
                            ExecuteEditTask(cmd.Parameters.Skip(1).ToList());
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
                            ExecuteDeleteTask(cmd.Parameters.Skip(1).ToList());
                        }
                    });
                }
                else if ((cmd.NameLower == "pull") || (cmd.NameLower == "reload"))
                {
                    ExecutePull();
                }
                else if ((cmd.NameLower == "pl") || (cmd.NameLower == "plants"))
                {
                    ExecuteViewPlantList(cmd.Parameters);
                }
                else if ((cmd.NameLower == "ts") || (cmd.NameLower == "tasks"))
                {
                    ExecuteViewTaskList(cmd.Parameters);
                }
                else if (cmd.NameLower == "nt")
                {
                    ExecuteAddTask(cmd.Parameters);
                }
#if DEBUG
                else if (cmd.NameLower == "fail")
                {
                    throw new Exception("NOOOOOOOOO!!!");
                }
#endif
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
            InColor(USERINPUT, () => Console.Write("  ts"));
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
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Waterer 2.0 Control Console: Help");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Conventions:");
            InColor(USERINPUT, "  this color");
            Console.WriteLine(" - exact user input");
            Console.Write("  ");
            PrintInFormatBraces("textInBraces", USERINPUT);
            Console.WriteLine(" - user input which adheres to specific format. '{' '}' symbols are not typed.");

            Console.WriteLine();
            Console.WriteLine("COMMANDS");
            Console.WriteLine("  All commands are case-insensitive");
            Console.WriteLine();
            Console.WriteLine("General:");

            InColorLn(USERINPUT, "  q");
            InColorLn(USERINPUT, "  exit");
            Console.WriteLine("    Exit from program");
            Console.WriteLine();

            InColorLn(USERINPUT, "  pull");
            InColorLn(USERINPUT, "  reload");
            Console.WriteLine("    Load actual data from server");
            Console.WriteLine();

            InColorLn(USERINPUT, "  cls");
            Console.WriteLine("    Clear the console");
            
            Console.WriteLine();
            PressAnyKey();
            Console.WriteLine();

            Console.WriteLine("Plants:");
            InColorLn(USERINPUT, "  pl");
            InColorLn(USERINPUT, "  plants");
            InColor(USERINPUT, "  pl ");
            PrintInFormatBraces("flags", USERINPUT);
            Console.WriteLine();
            InColor(USERINPUT, "  plants ");
            PrintInFormatBraces("flags", USERINPUT);
            Console.WriteLine();
            Console.WriteLine("    Displays list of plants.");
            Console.Write("    ");
            PrintInFormatBraces("flags", USERINPUT);
            Console.WriteLine(" - single or several additional characters. Only one flag is supported for plants:");
            InColor(USERINPUT, "      x");
            Console.WriteLine(" - display as xml. Keep in mind that values stored in xml can be by design different");
            Console.WriteLine("          from user-friendly values displayed in UIs.");

            Console.WriteLine();
            InColorLn(USERINPUT, "  add plant");
            InColorLn(USERINPUT, "  new plant");
            Console.WriteLine("    Initiates adding new plant. The program will guide you through the steps.");

            Console.WriteLine();
            InColorLn(USERINPUT, "  edit plant");
            InColorLn(USERINPUT, "  update plant");
            InColor(USERINPUT, "  edit plant ");
            PrintInFormatBraces("valveNo", USERINPUT);
            Console.WriteLine();
            InColor(USERINPUT, "  update plant ");
            PrintInFormatBraces("valveNo", USERINPUT);
            Console.WriteLine();
            Console.WriteLine("    Edit existing plant. The program will guide you through the steps.");
            Console.Write("    ");
            PrintInFormatBraces("valveNo", USERINPUT);
            Console.WriteLine(" - integer Valve Number of the plant. If not provided - the program will ask for it.");

            Console.WriteLine();
            InColorLn(USERINPUT, "  delete plant");
            InColor(USERINPUT, "  delete plant ");
            PrintInFormatBraces("valveNo", USERINPUT);
            Console.WriteLine();
            Console.WriteLine("    Delete existing plant.");
            Console.Write("    ");
            PrintInFormatBraces("valveNo", USERINPUT);
            Console.WriteLine(" - integer Valve Number of the plant. If not provided - the program will ask for it.");

            Console.WriteLine();
            PressAnyKey();
            Console.WriteLine();

            Console.WriteLine("Tasks:");
            InColorLn(USERINPUT, "  ts");
            InColorLn(USERINPUT, "  tasks");
            InColor(USERINPUT, "  ts ");
            PrintInFormatBraces("flags", USERINPUT);
            Console.WriteLine();
            InColor(USERINPUT, "  tasks ");
            PrintInFormatBraces("flags", USERINPUT);
            Console.WriteLine();
            Console.WriteLine("    Displays list of task. By default displays only tasks relevant for today and future tasks.");
            Console.Write("    ");
            PrintInFormatBraces("flags", USERINPUT);
            Console.WriteLine(" - single or several additional characters, either all concatenated, or all separated by space.");
            Console.WriteLine("              Supported flags:");
            InColor(USERINPUT, "      a");
            Console.WriteLine(" - display all tasks from server, including archived tasks.");
            InColor(USERINPUT, "      p");
            Console.WriteLine(" - use pagination for long outputs.");
            InColor(USERINPUT, "      x");
            Console.WriteLine(" - display as xml. Keep in mind that values stored in xml can be by design different");
            Console.WriteLine("          from user-friendly values displayed in UIs.");

            Console.WriteLine();
            InColorLn(USERINPUT, "  add task");
            InColor(USERINPUT, "  add task ");
            PrintInFormatBraces("taskDates", USERINPUT);
            Console.WriteLine();
            InColorLn(USERINPUT, "  new task");
            InColor(USERINPUT, "  new task ");
            PrintInFormatBraces("taskDates", USERINPUT);
            Console.WriteLine();
            InColorLn(USERINPUT, "  nt");
            InColor(USERINPUT, "  nt ");
            PrintInFormatBraces("taskDates", USERINPUT);
            Console.WriteLine();
            Console.WriteLine("    Initiates adding new task. Switches to Edit Task mode, which has its separate set of instructions,");
            Console.WriteLine("    different from those available on root level and described here. The program will provide you the info.");
            Console.Write("    ");
            PrintInFormatBraces("taskDates", USERINPUT);
            Console.WriteLine(" - date-time range for the future task, exact syntax is covered on the next page.");
            Console.WriteLine("                  If not provided, program will ask you for task's dates.");

            Console.WriteLine();
            PressAnyKey();
            Console.WriteLine();

            InColorLn(USERINPUT, "  edit task");
            InColorLn(USERINPUT, "  update task");
            InColor(USERINPUT, "  edit task ");
            PrintInFormatBraces("taskId", USERINPUT);
            Console.WriteLine();
            InColor(USERINPUT, "  update task ");
            PrintInFormatBraces("taskId", USERINPUT);
            Console.WriteLine();
            Console.WriteLine("    Edit existing task. Switches to Edit Task mode, which has its separate set of instructions,");
            Console.WriteLine("    different from those available on root level and described here. The program will provide you the info.");
            Console.Write("    ");
            PrintInFormatBraces("taskId", USERINPUT);
            Console.WriteLine(" - ID of the task. If not specified, the program will ask you for task's ID.");

            Console.WriteLine();
            InColorLn(USERINPUT, "  delete task");
            InColor(USERINPUT, "  delete task ");
            PrintInFormatBraces("taskId", USERINPUT);
            Console.WriteLine();
            Console.WriteLine("    Delete existing task.");
            Console.Write("    ");
            PrintInFormatBraces("taskId", USERINPUT);
            Console.WriteLine(" - ID of the task. If not specified, the program will ask you for task's ID.");

            Console.WriteLine();
            Console.Write("TASK DATES (");
            PrintInFormatBraces("taskDates", USERINPUT);
            Console.WriteLine(") SYNTAX");
            Console.Write("  ");
            PrintInFormatBraces("taskDates", USERINPUT);
            Console.Write(" ::= ");
            PrintInFormatBraces("wholeDayFmt", USERINPUT);
            Console.Write(" | ");
            PrintInFormatBraces("rangeFmt", USERINPUT);
            Console.WriteLine();

            Console.WriteLine();
            Console.Write("  ");
            PrintInFormatBraces("wholeDayFmt", USERINPUT);
            Console.WriteLine(" is for the tasks which are supposed to be valid within the whole calendar day regardless of time.");
            Console.WriteLine("  This term can have one of the following values:");
            InColor(USERINPUT, "    a");
            Console.WriteLine(" - shorthand for \'tomorrow\'");
            InColor(USERINPUT, "    b");
            Console.WriteLine(" - shorthand for \'today\'");
            Console.Write("    ");
            PrintInFormatBraces("dd/MM", USERINPUT);
            Console.WriteLine(" - specific date within current year");
            Console.Write("    ");
            PrintInFormatBraces("dd/MM/yy", USERINPUT);
            Console.WriteLine(" - specific date, 2-digit year format");
            Console.Write("    ");
            PrintInFormatBraces("dd/MM/yyyy", USERINPUT);
            Console.WriteLine(" - specific date, 4-digit year format");
            Console.Write("  Examples: ");
            InColor(USERINPUT, "nt a");
            Console.WriteLine(" - add whole-day task for \'tomorrow\'.");
            InColor(USERINPUT, "            add task 15/5");
            Console.WriteLine(" - add whole-day task for May 15th of current year.");

            Console.WriteLine();
            PressAnyKey();
            Console.WriteLine();

            Console.Write("  ");
            PrintInFormatBraces("rangeFmt", USERINPUT);
            Console.WriteLine(" specifies exact start and end time.");
            Console.WriteLine("  Possible values:");
            InColor(USERINPUT, "    a ");
            PrintInFormatBraces("time", USERINPUT);
            InColor(USERINPUT, "-");
            PrintInFormatBraces("time", USERINPUT);
            Console.WriteLine();
            InColor(USERINPUT, "    b ");
            PrintInFormatBraces("time", USERINPUT);
            InColor(USERINPUT, "-");
            PrintInFormatBraces("time", USERINPUT);
            Console.WriteLine();
            Console.Write("    ");
            PrintInFormatBraces("date", USERINPUT);
            Console.Write(" ");
            PrintInFormatBraces("time", USERINPUT);
            InColor(USERINPUT, "-");
            PrintInFormatBraces("time", USERINPUT);
            Console.WriteLine();
            Console.Write("    ");
            PrintInFormatBraces("date", USERINPUT);
            Console.Write(" ");
            PrintInFormatBraces("time", USERINPUT);
            InColor(USERINPUT, "-");
            PrintInFormatBraces("date", USERINPUT);
            Console.Write(" ");
            PrintInFormatBraces("time", USERINPUT);
            Console.WriteLine();

            Console.Write("  Here ");
            PrintInFormatBraces("date", USERINPUT);
            Console.Write(" is either ");
            PrintInFormatBraces("dd/MM", USERINPUT);
            Console.Write(", ");
            PrintInFormatBraces("dd/MM/yy", USERINPUT);
            Console.Write(" or ");
            PrintInFormatBraces("dd/MM/yyyy", USERINPUT);
            Console.Write("; ");
            PrintInFormatBraces("time", USERINPUT);
            Console.Write(" is either ");
            PrintInFormatBraces("hh:mm", USERINPUT);
            Console.Write(" or ");
            PrintInFormatBraces("hh", USERINPUT);
            Console.WriteLine(",");

            Console.Write("  and ");
            InColor(USERINPUT, "a");
            Console.Write(" and ");
            InColor(USERINPUT, "b");
            Console.WriteLine(" are shorthands for \'tomorrow\' and \'today\' respectively.");
            Console.WriteLine("  Seconds are not supported. Hours are in 24h format.");

            Console.Write("  Examples: ");
            InColor(USERINPUT, "nt a 9-13");
            Console.WriteLine(" - add task valid from tomorrow 9:00 am till tomorrow 1:00 pm.");
            InColor(USERINPUT, "            nt 15/5/25 8 - 16/5/2025 5:59");
            Console.WriteLine(" - add task valid from May 15 2025 8:00 till May 16 2025 5:59.");

            Console.WriteLine();
            Console.WriteLine("Colors:");
            InColorLn(INFO, "  info");
            InColorLn(USERINPUT, "  user input");
            InColorLn(REQUEST, "  indicates that specific input is required, and root-level commands are not supported");
            InColorLn(WARNING, "  warning");
            InColorLn(ERROR, "  error");
        });
    }

    static void PressAnyKey()
    {
        Console.Write("=====    Press any key...   =====");
        Console.ReadKey();
        Console.WriteLine();
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

    static void ExecuteViewPlantList(IReadOnlyList<string> parameters)
    {
        if (!AutoPull())
            return;

        if (model.Plants.Count == 0)
        {
            InColorLn(INFO, "< empty >");
            return;
        }

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
            gb[0, 2] = "Std Volume ml";
            gb[0, 3] = "Offset ml";
            gb.ColumnSeparator = "   ";
            gb.HeaderRowSeparator = '-';

            for (int i = 0; i < model.Plants.Count; i++)
            {
                gb[i + 1, 0] = model.Plants[i].ValveNo.ToString();
                gb[i + 1, 1] = model.Plants[i].PlantType.ToString();
                gb[i + 1, 2] = StringUtils.StandardVolumeToStr(model.Plants[i].StandardVolumeMl);
                gb[i + 1, 3] = StringUtils.StandardVolumeToStr(model.Plants[i].OffsetMl);
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
            int? valveNo = QueryInt("Enter plant's Valve Number", availableIndicies.Select(x => x + 1), allowCancel: true);
            if (!valveNo.HasValue)
            {
                InColorLn(INFO, "Operation canceled");
                return;
            }

            plantIndex = valveNo.Value - 1;
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

    static void ExecuteEditPlant(IReadOnlyList<string> truncatedParameters)
    {
        if (!AutoPull())
            return;

        SPlant plant = SelectPlantForUpdate(truncatedParameters);
        if (plant == null)
            return;

        SPlantType newType = QueryEnum("Enter new Type:",
            Enum.GetValues<SPlantType>().Where(x => x != SPlantType.Unused));

        int? stdVolumeMl = QueryGenericInt("Enter Standard Volume ml", 0, SPlant.MAX_STD_VOLUMEML, true);
        if (!stdVolumeMl.HasValue)
            return;
        if (stdVolumeMl == 0)
            stdVolumeMl = null;

        int? offsetMl = QueryGenericInt("Enter Offset ml", -SPlant.MAX_OFFSETML, SPlant.MAX_OFFSETML, true);
        if (!offsetMl.HasValue)
            return;
        if (offsetMl == 0)
            offsetMl = null;

        InColorLn(INFO, $"Valve Number: {plant.ValveNo}");
        InColorLn(INFO, $"Type: {plant.PlantType} -> {newType}");
        InColorLn(INFO, $"Standard Volume ml: {StringUtils.StandardVolumeToStr(plant.StandardVolumeMl)} -> {StringUtils.StandardVolumeToStr(stdVolumeMl)}");
        InColorLn(INFO, $"Offset ml: {StringUtils.StandardVolumeToStr(plant.OffsetMl)} -> {StringUtils.StandardVolumeToStr(offsetMl)}");

        if (AskYesNo("Save to server?"))
        {
            plant.PlantType = newType;
            plant.StandardVolumeMl = stdVolumeMl;
            plant.OffsetMl = offsetMl;
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
                int? valveNo = QueryInt("Enter plant's Valve Number", model.Plants.Select(x => x.ValveNo), allowCancel: true);
                if (!valveNo.HasValue)
                {
                    InColorLn(INFO, "Operation canceled");
                    return null;
                }

                plant = model.Plants.First(x => x.ValveNo == valveNo.Value);
            }
        }

        InColorLn(INFO, $"Valve Number: {plant.ValveNo}");
        InColorLn(INFO, $"Type: {plant.PlantType}");
        return plant;
    }

    static void ExecuteAddTask(IReadOnlyList<string> truncatedParameters)
    {
        if (!AutoPull())
            return;

        STask editTask = STask.Empty();
        if (!EditTaskMode_ExecuteDates(truncatedParameters, editTask))
            return;

        RunEditTaskMode(null, editTask);
    }

    static void ExecuteEditTask(IReadOnlyList<string> truncatedParameters)
    {
        if (!AutoPull())
            return;

        STask editTask = SelectTaskForUpdate(truncatedParameters);
        if (editTask == null)
            return;

        RunEditTaskMode(editTask, editTask.Clone());
    }

    static void ExecuteDeleteTask(IReadOnlyList<string> truncatedParameters)
    {
        if (!AutoPull())
            return;

        STask task = SelectTaskForUpdate(truncatedParameters);
        if (task == null)
            return;

        if (AskYesNo("Delete?"))
        {
            if (NetworkingWrapper("Sending delete request...", () =>
                {
                    connector.DeleteTask(task.Id);
                }))
            {
                ExecutePull();
            }
        }
    }

    static STask SelectTaskForUpdate(IReadOnlyList<string> truncatedParameters)
    {
        if (model.Tasks.Count == 0)
        {
            InColorLn(INFO, "There are no tasks");
            return null;
        }

        STask editTask = null;
        if (truncatedParameters.Count > 0)
        {
            if (int.TryParse(truncatedParameters[0], out int taskId))
            {
                editTask = model.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (editTask == null)
                {
                    InColorLn(WARNING, $"Task with ID {taskId} doesn't exist");
                }
            }
            else
            {
                InColorLn(WARNING, $"Invalid parameter: {truncatedParameters[0]}");
            }
        }

        if (editTask == null)
        {
            // For plants there was auto-select if only one, however for tasks it would be too sloppy.
            // Always select.
            List<int> taskIds = model.Tasks.Select(t => t.Id).OrderBy(x => x).ToList();
            int? taskId = QueryInt("Enter Task ID:", taskIds, false, true);
            if (!taskId.HasValue)
            {
                InColorLn(INFO, "Operation canceled");
                return null;
            }

            editTask = model.Tasks.First(t => t.Id == taskId.Value);
        }

        EditTaskMode_ExecutePrint(null, editTask);
        return editTask;
    }

    static void RunEditTaskMode(STask initial, STask actual)
    {
        EditTaskMode_ExecuteHelp();

        while (true)
        {
            InColor(REQUEST, "T> ");

            string commandRaw = Console.ReadLine();
            InputCommand cmd = StringUtils.ParseCommand(commandRaw);

            if (cmd.NameLower == "p")
            {
                EditTaskMode_ExecutePlant(cmd.Parameters, actual);
            }
            else if (cmd.NameLower == "st")
            {
                EditTaskMode_ExecuteStd(cmd.Parameters, actual);
            }
            else if (cmd.NameLower == "d")
            {
                EditTaskMode_ExecuteDates(cmd.Parameters, actual);
            }
            else if ((cmd.NameLower == "ts") || (cmd.NameLower == "view"))
            {
                EditTaskMode_ExecutePrint(initial, actual);
            }
            else if ((cmd.NameLower == "w") || (cmd.NameLower == "save"))
            {
                if (AskYesNo("Save to server?"))
                {
                    int? newId = null;
                    if (NetworkingWrapper("Sending data...", () =>
                        {
                            if (actual.Id == 0)
                                newId = connector.AddTask(actual);
                            else
                                connector.UpdateTask(actual);
                        }))
                    {
                        if (newId.HasValue)
                            InColorLn(INFO, $"ID: {newId.Value}");

                        ExecutePull();
                        return;
                    }
                    else
                    {
                        InColorLn(ERROR, "Not saved");
                    }
                }
            }
            else if ((cmd.NameLower == "h") || (cmd.NameLower == "help"))
            {
                EditTaskMode_ExecuteHelp();
            }
            else if ((cmd.NameLower == "c") || (cmd.NameLower == "cancel"))
            {
                return;
            }
            else
            {
                InColorLn(WARNING, $"Unknown command: {cmd.NameLower.ToUpper()}");
            }
        }
    }

    static void EditTaskMode_ExecuteHelp()
    {
        InColor(REQUEST, () =>
        {
            Console.WriteLine("Edit Task mode:");
            Console.Write("  ");
            InColor(USERINPUT, "p");
            Console.WriteLine(" - add/edit plant within task. Options:");

            Console.Write("  ");
            InColor(USERINPUT, "p ");
            PrintInFormatBraces("valveNo", USERINPUT);
            Console.WriteLine();

            Console.Write("  ");
            InColor(USERINPUT, "p ");
            PrintInFormatBraces("valveNo", USERINPUT);
            Console.Write(" ");
            PrintInFormatBraces("amount", USERINPUT);
            Console.WriteLine();

            Console.Write("  ");
            InColor(USERINPUT, "st");
            Console.WriteLine(" - set all plants within task to some fraction of their Standard Volume. Options:");

            Console.Write("  ");
            InColor(USERINPUT, "st ");
            PrintInFormatBraces("multiplier", USERINPUT);
            Console.WriteLine();

            Console.Write("  ");
            InColor(USERINPUT, "st ");
            PrintInFormatBraces("numerator", USERINPUT);
            Console.Write("/");
            PrintInFormatBraces("denominator", USERINPUT);
            Console.WriteLine();

            Console.Write("  ");
            InColor(USERINPUT, "d");
            Console.WriteLine(" - change dates. Options:");

            Console.Write("  ");
            InColor(USERINPUT, "d ");
            PrintInFormatBraces("taskDates", USERINPUT);
            Console.WriteLine();

            Console.Write("  ");
            PrintCommaSeparated(new string[] { "ts", "view" }, ", ", USERINPUT);
            Console.WriteLine(" - view task's current state (with all edits)");

            Console.Write("  ");
            PrintCommaSeparated(new string[] { "w", "save" }, ", ", USERINPUT);
            Console.WriteLine(" - save to server");

            Console.Write("  ");
            PrintCommaSeparated(new string[] { "h", "help" }, ", ", USERINPUT);
            Console.WriteLine(" - display this message");

            Console.Write("  ");
            PrintCommaSeparated(new string[] { "c", "cancel" }, ", ", USERINPUT);
            Console.WriteLine(" - cancel edit task");
        });
    }

    static void EditTaskMode_ExecutePlant(IReadOnlyList<string> parameters, STask task)
    {
        SPlant plant = null;
        if (parameters.Count > 0)
        {
            if (int.TryParse(parameters[0], out int valveNo))
            {
                plant = model.Plants.FirstOrDefault(p => p.ValveNo == valveNo);
                if (plant == null)
                {
                    InColorLn(WARNING, $"Plant with Valve Number {valveNo} doesn't exist.");
                }
            }
            else
            {
                InColorLn(WARNING, $"Invalid parameter: {parameters[0]}");
            }
        }

        if (plant == null)
        {
            if (model.Plants.Count == 0)
            {
                InColorLn(INFO, "There are no plants in the system. Cannot attach plant to a task.");
                return;
            }

            int? valveNo = QueryInt("Enter plant's Valve Number", model.Plants.Select(x => x.ValveNo), allowCancel: true);
            if (!valveNo.HasValue)
            {
                InColorLn(INFO, "Operation canceled");
                return;
            }

            plant = model.Plants.First(x => x.ValveNo == valveNo.Value);
        }
        else // plant succesfully obtained from the first parameter
        {
            // May be there is already volumn ml in the second parameter
            if (parameters.Count > 1)
            {
                if (int.TryParse(parameters[1], out int volumeMl))
                {
                    if ((volumeMl < 0) || (volumeMl > STaskItem.MAX_VOLUMEML))
                    {
                        InColorLn(WARNING, $"Value out of range [0; {STaskItem.MAX_VOLUMEML}]");
                    }
                    else
                    {
                        ApplyVolumeToTask(task, plant, volumeMl);

                        InColorLn(INFO, $"Valve No {plant.ValveNo}: {volumeMl} ml");
                        return;
                    }
                }
                else
                {
                    InColorLn(WARNING, $"Invalid parameter: {parameters[1]}");
                }
            }

            InColorLn(INFO, $"Valve No: {plant.ValveNo}");
        }

        // Plant - known, need to know volume ml
        InColorLn(REQUEST, $"Specify Volume ml, 0 to {STaskItem.MAX_VOLUMEML}:");
        InColor(REQUEST, "> ");

        string str = Console.ReadLine().Trim();

        // Here no inner cycles; if mistaken - restart the command.
        if (int.TryParse(str, out int parsedVolumeMl))
        {
            if ((parsedVolumeMl < 0) || (parsedVolumeMl > STaskItem.MAX_VOLUMEML))
            {
                InColorLn(WARNING, $"Value out of range");
            }
            else
            {
                ApplyVolumeToTask(task, plant, parsedVolumeMl);

                InColorLn(INFO, $"Valve No {plant.ValveNo}: {parsedVolumeMl} ml");
                return;
            }
        }
        else
        {
            InColorLn(WARNING, $"Failed to parse integer value");
        }
    }

    static void ApplyVolumeToTask(STask task, SPlant plant, int volumeMl)
    {
        if (volumeMl == 0)
        {
            task.Items.RemoveAll(item => item.Plant == plant);
        }
        else
        {
            STaskItem item = task.Items.FirstOrDefault(x => x.Plant == plant);
            if (item == null)
            {
                item = new STaskItem()
                {
                    Plant = plant,
                    Status = STaskStatus.NotStarted
                };
                task.Items.Add(item);
            }

            item.VolumeMl = volumeMl;
        }
    }

    static void EditTaskMode_ExecuteStd(IReadOnlyList<string> parameters, STask task)
    {
        if (!model.Plants.Any(p => p.HasStandardVolume))
        {
            InColorLn(INFO, $"There are no plants with Standard Volume ml or Offset specified. 'std' command cannot do anything.");
            return;
        }

        Tuple<double, double> ratio = null;
        if (parameters.Count > 0)
        {
            string glued = string.Join(" ", parameters);
            ratio = StringUtils.ParseStdMultiplier(glued);
            if (ratio == null)
            {
                InColorLn(WARNING, $"Invalid multiplier or fraction: {glued}");
            }
        }
        else
        {
            InColorLn(REQUEST, $"Specify multiplier as single number or as a fraction:");
            InColor(REQUEST, "> ");

            string str = Console.ReadLine().Trim();
            ratio = StringUtils.ParseStdMultiplier(str);
            if (ratio == null)
            {
                InColorLn(WARNING, $"Invalid multiplier or fraction: {str}");
            }
        }

        if (ratio != null)
        {
            double multiplier = ratio.Item1 / ratio.Item2;
            foreach (SPlant plant in model.Plants)
            {
                if (plant.HasStandardVolume)
                {
                    int volumeMl = plant.GetStandardVolume(multiplier) ?? 0; // not null when "has standard volume"
                    InColorLn(INFO, $"Valve No {plant.ValveNo}: {volumeMl} ml");
                    ApplyVolumeToTask(task, plant, volumeMl);
                }
                else
                {
                    InColorLn(INFO, $"Valve No {plant.ValveNo}: skipped");
                }
            }
        }
    }

    static bool EditTaskMode_ExecuteDates(IReadOnlyList<string> parameters, STask task)
    {
        Tuple<DateTime, DateTime> datesLocal = GetTaskDatesFromParameters(parameters);
        if (datesLocal == null)
        {
            datesLocal = QueryTaskTimes();
            if (datesLocal == null)
                return false;
        }

        task.UtcValidFrom = datesLocal.Item1.ToUniversalTime();
        task.UtcValidTo = datesLocal.Item2.ToUniversalTime();
        return true;
    }

    static void EditTaskMode_ExecutePrint(STask initial, STask actual)
    {
        if (initial != null)
        {
            InColorLn(INFO, "Before:");
            List<STask> tasksBefore = new List<STask>() { initial };
            PrintTasks(tasksBefore, false, false);
            Console.WriteLine();
            InColorLn(INFO, "After:");
        }

        List<STask> tasksAfter = new List<STask>() { actual };
        PrintTasks(tasksAfter, false, false);
    }

    static void ExecuteViewTaskList(IReadOnlyList<string> parameters)
    {
        bool[] flags = ReadSimpleFlags(new char[] { 'x', 'a', 'p' }, parameters);
        bool isXml = flags[0];
        bool isAll = flags[1];
        bool isPaged = flags[2];

        if (!AutoPull())
            return;

        if (isAll)
        {
            PrintTasks(model.Tasks.OrderBy(t => t.UtcValidFrom).ToList(), isPaged, isXml);
        }
        else
        {
            DateTime localToday = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Local);
            DateTime utcToday = localToday.ToUniversalTime();
            PrintTasks(model.Tasks.Where(t => t.UtcValidTo > utcToday).OrderBy(t => t.UtcValidFrom).ToList(), isPaged, isXml);
        }
    }

    static void PrintTasks(IList<STask> tasks, bool isPaged, bool isXml)
    {
        if (tasks.Count == 0)
        {
            InColorLn(INFO, "< empty >");
            return;
        }

        string stringOutput;
        if (isXml)
        {
            StringBuilder sb = new();
            for (int i = 0; i < tasks.Count; i++)
            {
                if (i > 0)
                    sb.AppendLine();

                sb.Append(ModelXml.TaskToStr(tasks[i]));
            }

            stringOutput = sb.ToString();
        }
        else
        {
            MultilineGridBuilder gb = new();
            gb.HeaderRowSeparator = '=';
            gb.RowSeparator = '-';
            gb.ColumnSeparator = "|";

            for (int j = 0; j < model.Plants.Count; j++)
            {
                SPlant plant = model.Plants[j];
                gb[0, j + 1] = $"Plant {plant.ValveNo}{Environment.NewLine}{plant.PlantType}";
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                STask task = tasks[i];
                gb[i + 1, 0] = $"ID:   {task.Id}{Environment.NewLine}From: {task.UtcValidFrom.ToLocalTime():dd\\/MM\\/yy HH\\:mm}{Environment.NewLine}To:   {task.UtcValidTo.ToLocalTime():dd\\/MM\\/yy HH\\:mm}";

                for (int j = 0; j < model.Plants.Count; j++)
                {
                    SPlant plant = model.Plants[j];
                    STaskItem taskItem = task.Items.FirstOrDefault(item => item.Plant == plant);
                    if (taskItem != null)
                    {
                        gb[i + 1, j + 1] = $"{taskItem.VolumeMl} ml{Environment.NewLine}{StringUtils.TaskStatusToGridStr(taskItem.Status)}";
                    }
                }
            }

            stringOutput = gb.ToString();
        }

        if (isPaged)
        {
            const int pgSize = 28; // divisible by 4 so tasks display nicely when in grid form
            List<string> lines = StringUtils.SplitIntoLines(stringOutput);
            InColor(INFO, () =>
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if ((i > 5) && ((i + 2) % pgSize == 0)) // with shift by 2 it splits at separator
                    {
                        PressAnyKey();
                    }

                    Console.WriteLine(lines[i]);
                }
            });
        }
        else
        {
            InColorLn(INFO, stringOutput);
        }
    }

    #endregion

    #region Input "Dialogs"

    static int? QueryInt(string prompt, IEnumerable<int> options, bool inline = true, bool allowCancel = false)
    {
        while (true)
        {
            InColor(REQUEST, () =>
            {
                Console.Write(prompt);
                if (inline)
                {
                    Console.Write(" (");
                    PrintCommaSeparated(options.Select(x => x.ToString()).ToArray(), ", ", USERINPUT);
                    Console.WriteLine("):");
                }
                else
                {
                    Console.WriteLine();
                    Console.Write("(");
                    PrintCommaSeparated(options.Select(x => x.ToString()).ToArray(), ", ", USERINPUT);
                    Console.WriteLine(")");
                }
                
                Console.Write("> ");
            });

            string str = Console.ReadLine().Trim();
            string lower = str.ToLower();
            if (allowCancel && ((lower == "c") || (lower == "cancel")))
                return null;

            if (int.TryParse(str, out int parsed))
            {
                if (options.Contains(parsed))
                {
                    return parsed;
                }
                else
                {
                    InColor(WARNING, "Please only use suggested values");
                }
            }
            else
            {
                InColor(WARNING, "Please only use suggested integer numbers");
            }

            if (allowCancel)
            {
                InColor(WARNING, ". Or use ");
                InColor(USERINPUT, "c");
                InColor(WARNING, " or ");
                InColor(USERINPUT, "cancel");
                InColorLn(WARNING, " to cancel the operation.");
            }
            else
            {
                Console.WriteLine();
            }
        }
    }

    static int? QueryGenericInt(string prompt, int? min = null, int? max = null, bool allowCancel = false)
    {
        while (true)
        {
            InColor(REQUEST, () =>
            {
                Console.Write(prompt);
                if (min.HasValue)
                {
                    if (max.HasValue)
                        Console.WriteLine($" ({min.Value}..{max.Value}):");
                    else
                        Console.WriteLine($" (from {min.Value}):");
                }
                else if (max.HasValue)
                {
                    Console.WriteLine($" (up to {max.Value}):");
                }
                else
                {
                    Console.WriteLine();
                }

                Console.Write("> ");
            });

            string str = Console.ReadLine().Trim();
            string lower = str.ToLower();
            if (allowCancel && ((lower == "c") || (lower == "cancel")))
                return null;

            if (int.TryParse(str, out int parsed))
            {
                if (min.HasValue && (parsed < min.Value))
                {
                    InColor(WARNING, $"Value should not be less than {min.Value}");
                }
                else
                {
                    if (max.HasValue && (parsed > max.Value))
                    {
                        InColor(WARNING, $"Value should not be greater than {max.Value}");
                    }
                    else
                    {
                        return parsed;
                    }
                }
            }
            else
            {
                InColor(WARNING, "Please enter integer number");
            }

            if (allowCancel)
            {
                InColor(WARNING, ". Or use ");
                InColor(USERINPUT, "c");
                InColor(WARNING, " or ");
                InColor(USERINPUT, "cancel");
                InColorLn(WARNING, " to cancel the operation.");
            }
            else
            {
                Console.WriteLine();
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

    static Tuple<DateTime, DateTime> QueryTaskTimes()
    {
        InColor(REQUEST, () =>
        {
            Console.WriteLine("Enter task start-end time. Format:");
            Console.Write("  ");
            InColor(USERINPUT, "a");
            Console.WriteLine(" - tomorrow, whole day");
            Console.Write("  ");
            InColor(USERINPUT, "b");
            Console.WriteLine(" - today, whole day");

            Console.Write("  ");
            PrintInFormatBraces("dd/MM", USERINPUT);
            Console.Write(" or ");
            PrintInFormatBraces("dd/MM/yy", USERINPUT);
            Console.Write(" or ");
            PrintInFormatBraces("dd/MM/yyyy", USERINPUT);
            Console.WriteLine(" - specific date, whole day");

            Console.Write("  ");
            InColor(USERINPUT, "a ");
            PrintInFormatBraces("hh", USERINPUT);
            InColor(USERINPUT, " - ");
            PrintInFormatBraces("hh", USERINPUT);
            Console.WriteLine(" - tomorrow, time range (from hh:00 to hh:00)");

            Console.Write("  ");
            PrintInFormatBraces("dd/MM", USERINPUT);
            Console.Write(" ");
            PrintInFormatBraces("hh:mm", USERINPUT);
            InColor(USERINPUT, " - ");
            PrintInFormatBraces("dd/MM/yyyy", USERINPUT);
            Console.Write(" ");
            PrintInFormatBraces("hh", USERINPUT);
            Console.WriteLine(" - specify both start/end dates and times with/without minutes and year,");

            Console.WriteLine("and so on. If both dates specified - cannot use shorthands 'a' or 'b'.");
        });

        while (true)
        {
            InColor(REQUEST, "Type ");
            InColor(USERINPUT, "c");
            InColor(REQUEST, " or ");
            InColor(USERINPUT, "cancel");
            InColorLn(REQUEST, " for canceling the operation.");

            InColor(REQUEST, "T> ");

            string str = Console.ReadLine();
            string lower = str.Trim().ToLower();
            if ((lower == "cancel") || (lower == "c"))
                return null;

            Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime(str, DateTime.Today);
            if (parsed == null)
            {
                InColorLn(WARNING, "Didn't parse time range :( Check the values.");
            }
            else
            {
                if (ValidateTaskTimes(parsed))
                {
                    InColorLn(INFO, $"Valid From: {parsed.Item1:dd\\/MM\\/yyyy HH\\:mm}");
                    InColorLn(INFO, $"Valid To:   {parsed.Item2:dd\\/MM\\/yyyy HH\\:mm}");

                    return new Tuple<DateTime, DateTime>(
                        DateTime.SpecifyKind(parsed.Item1, DateTimeKind.Local),
                        DateTime.SpecifyKind(parsed.Item2, DateTimeKind.Local)
                    );
                }
            }
        }
    }

    static Tuple<DateTime, DateTime> GetTaskDatesFromParameters(IReadOnlyList<string> truncatedParameters)
    {
        Tuple<DateTime, DateTime> parsed = null;
        if (truncatedParameters.Count > 0)
        {
            string glued = string.Join(' ', truncatedParameters);
            parsed = StringUtils.ParseTaskTime(glued, DateTime.Today);
            if (parsed == null)
            {
                InColorLn(WARNING, "Task start-end time not parsed from command parameters.");
            }
            else
            {
                if (ValidateTaskTimes(parsed))
                {
                    parsed = new Tuple<DateTime, DateTime>(
                        DateTime.SpecifyKind(parsed.Item1, DateTimeKind.Local),
                        DateTime.SpecifyKind(parsed.Item2, DateTimeKind.Local)
                    );

                    InColorLn(INFO, $"Valid From: {parsed.Item1:dd\\/MM\\/yyyy HH\\:mm}");
                    InColorLn(INFO, $"Valid To:   {parsed.Item2:dd\\/MM\\/yyyy HH\\:mm}");

                    return parsed;
                }
            }
        }

        return null;
    }

    static bool ValidateTaskTimes(Tuple<DateTime, DateTime> parsed)
    {
        if (parsed.Item1 > parsed.Item2)
        {
            InColor(WARNING, "Incorrect values: start time greater than end time.");
            return false;
        }

        return true;
    }

    #endregion

    #region Parsing Input

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

    static void PrintInFormatBraces(string value, ConsoleColor? valueColor = null)
    {
        InColor(FORMAT, "{");
        if (valueColor.HasValue)
            InColor(valueColor.Value, value);
        else
            Console.Write(value);
        InColor(FORMAT, "}");
    }

    #endregion
}
