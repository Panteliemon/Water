using System;

namespace WaterConsole;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            PrintHello();

            while (true)
            {
                InColor(ConsoleColor.Cyan, () => Console.Write("> "));

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
                    InColor(ConsoleColor.Yellow, () =>
                    {
                        Console.WriteLine($"Unknown command: {cmd.NameLower.ToUpper()}");
                        InColor(ConsoleColor.Gray, () => Console.Write("h, help"));
                        Console.WriteLine(" for the list of supported commands.");
                    });
                }
            }
        }
        catch (Exception ex)
        {
            InColor(ConsoleColor.Red, () =>
            {
                Console.WriteLine($"[x] Unhandled exception: {ex.Message}");
                InColor(ConsoleColor.Yellow, () =>
                {
                    Console.WriteLine(ex.StackTrace);
                });
                Console.WriteLine("App terminated");
            });
        }
    }

    static void PrintHello()
    {
        InColor(ConsoleColor.Cyan, () =>
        {
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("Waterer 2.0 Control Console");
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine();
            Console.WriteLine("Quick start: ");
            InColor(ConsoleColor.Gray, () => Console.Write("  tl"));
            Console.WriteLine(" - view list of tasks");
            InColor(ConsoleColor.Gray, () => Console.Write("  nt"));
            Console.WriteLine(" - add new task");
            InColor(ConsoleColor.Gray, () => Console.Write("  h, help"));
            Console.WriteLine(" - get help");
            InColor(ConsoleColor.Gray, () => Console.Write("  q, exit"));
            Console.WriteLine(" - exit");
            Console.WriteLine();
        });
    }

    static void PrintHelp()
    {
        InColor(ConsoleColor.Cyan, () =>
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
