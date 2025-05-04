using System;

namespace WaterConsole;

internal class Program
{
    const ConsoleColor USERINPUT = ConsoleColor.Gray;
    const ConsoleColor INFO = ConsoleColor.Cyan;
    const ConsoleColor WARNING = ConsoleColor.Yellow;
    const ConsoleColor ERROR = ConsoleColor.Red;

    static void Main(string[] args)
    {
        try
        {
            PrintHello();

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
                else
                {
                    InColor(WARNING, () =>
                    {
                        Console.WriteLine($"Unknown command: {cmd.NameLower.ToUpper()}");
                        InColor(USERINPUT, () => Console.Write("h"));
                        Console.Write(", ");
                        InColor(USERINPUT, () => Console.Write("help"));
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
                InColor(WARNING, () =>
                {
                    Console.WriteLine(ex.StackTrace);
                });
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
            InColor(USERINPUT, () => Console.Write("  h"));
            Console.Write(", ");
            InColor(USERINPUT, () => Console.Write("help"));
            Console.WriteLine(" - get help");
            InColor(USERINPUT, () => Console.Write("  q"));
            Console.Write(", ");
            InColor(USERINPUT, () => Console.Write("exit"));
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
}
