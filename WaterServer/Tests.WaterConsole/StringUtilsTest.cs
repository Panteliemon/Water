using System;
using WaterConsole;

namespace Tests.WaterConsole;

public class StringUtilsTest
{
    [Fact]
    public void ParseCommand_Empty()
    {
        InputCommand cmd = StringUtils.ParseCommand("");

        Assert.NotNull(cmd);
        Assert.Equal("", cmd.NameLower);
        Assert.NotNull(cmd.Parameters);
    }

    [Fact]
    public void ParseCommand_1()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test");

        Assert.Equal("test", cmd.NameLower);
    }

    [Fact]
    public void ParseCommand_2()
    {
        InputCommand cmd = StringUtils.ParseCommand("   Test");

        Assert.Equal("test", cmd.NameLower);
    }

    [Fact]
    public void ParseCommand_3()
    {
        InputCommand cmd = StringUtils.ParseCommand("   Test ");

        Assert.Equal("test", cmd.NameLower);
    }

    [Fact]
    public void ParseCommand_4()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test   ");

        Assert.Equal("test", cmd.NameLower);
    }

    [Fact]
    public void ParseCommand_1Param1()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test Param");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Param", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_1Param2()
    {
        InputCommand cmd = StringUtils.ParseCommand("   Test  Param");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Param", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_1Param3()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test  Param    ");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Param", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_1Param4()
    {
        InputCommand cmd = StringUtils.ParseCommand("  Test  Param   ");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Param", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_1Param5()
    {
        InputCommand cmd = StringUtils.ParseCommand("  Test  \"Spaces 2\"");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Spaces 2", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_1Param6()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test  \"Spaces 2\"   ");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Spaces 2", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_1Param7()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test  \'Spaces 2\'");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Spaces 2", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_1Param8()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test  \'Spaces 2\'   ");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("Spaces 2", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseCommand_2Params1()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test  First Second   ");

        Assert.Equal("test", cmd.NameLower);
        Assert.Equal(2, cmd.Parameters.Count);
        Assert.Equal("First", cmd.Parameters[0]);
        Assert.Equal("Second", cmd.Parameters[1]);
    }

    [Fact]
    public void ParseCommand_2Params2()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test First    Second");

        Assert.Equal("test", cmd.NameLower);
        Assert.Equal(2, cmd.Parameters.Count);
        Assert.Equal("First", cmd.Parameters[0]);
        Assert.Equal("Second", cmd.Parameters[1]);
    }

    [Fact]
    public void ParseCommand_2Params3()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test \"First \" Second");

        Assert.Equal("test", cmd.NameLower);
        Assert.Equal(2, cmd.Parameters.Count);
        Assert.Equal("First ", cmd.Parameters[0]);
        Assert.Equal("Second", cmd.Parameters[1]);
    }

    [Fact]
    public void ParseCommand_2Params4()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test First \"Second\"");

        Assert.Equal("test", cmd.NameLower);
        Assert.Equal(2, cmd.Parameters.Count);
        Assert.Equal("First", cmd.Parameters[0]);
        Assert.Equal("Second", cmd.Parameters[1]);
    }

    [Fact]
    public void ParseCommand_2Params5()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test \'First \' Second");

        Assert.Equal("test", cmd.NameLower);
        Assert.Equal(2, cmd.Parameters.Count);
        Assert.Equal("First ", cmd.Parameters[0]);
        Assert.Equal("Second", cmd.Parameters[1]);
    }

    [Fact]
    public void ParseCommand_2Params6()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test First \'Second\'");

        Assert.Equal("test", cmd.NameLower);
        Assert.Equal(2, cmd.Parameters.Count);
        Assert.Equal("First", cmd.Parameters[0]);
        Assert.Equal("Second", cmd.Parameters[1]);
    }

    [Fact]
    public void ParseCommand_Unclosed1()
    {
        InputCommand cmd = StringUtils.ParseCommand("Test \"   ");

        Assert.Equal("test", cmd.NameLower);
        Assert.Single(cmd.Parameters);
        Assert.Equal("   ", cmd.Parameters[0]);
    }

    [Fact]
    public void ParseTaskTime_A()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("A", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 5, 6, 0, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 5, 7, 0, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_B()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("B", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 5, 5, 0, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 5, 6, 0, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("15/6", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 15, 0, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 6, 16, 0, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("15/6/24", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2024, 6, 15, 0, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2024, 6, 16, 0, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_a_hh_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("a 12-13", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 5, 6, 12, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 5, 6, 13, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_a_hhmm_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("a 12:20-13:30", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 5, 6, 12, 20, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 5, 6, 13, 30, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_b_hh_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("b 12-13", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 5, 5, 12, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 5, 5, 13, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_b_hhmm_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("b 12:20-13:30", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 5, 5, 12, 20, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 5, 5, 13, 30, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hh_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("15/6 8-11", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 15, 8, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 6, 15, 11, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hhmm_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("15/6 8 : 10 - 11 : 05", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 15, 8, 10, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 6, 15, 11, 5, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hh_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("15   /6/24 8  - 12 ", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2024, 6, 15, 8, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2024, 6, 15, 12, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hhmm_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("15   /6/24 8 : 10 - 12 :25", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2024, 6, 15, 8, 10, 0), parsed.Item1);
        Assert.Equal(new DateTime(2024, 6, 15, 12, 25, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hh_ddMM_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/3 6 - 23/3 6", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 6, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hh_ddMM_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6 - 23/3 6", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 6, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hh_ddMMyyyy_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03 6 - 23/3/23 6", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 6, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hh_ddMMyyyy_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6 - 23/3/23 8", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 8, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hh_ddMM_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/3 6 - 23/3 21:10", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 21, 10, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hh_ddMM_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6 - 23/3 21:10", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 21, 10, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hh_ddMMyyyy_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03 6 - 23/3/23 21:10", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 21, 10, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hh_ddMMyyyy_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6 - 23/3/23 21:10", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 21, 10, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hhmm_ddMM_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/3 6:15 - 23/3 6", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 6, 15, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 6, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hhmm_ddMM_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6:22 - 23/3 7", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 22, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 7, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hhmm_ddMMyyyy_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03 11:12 - 23/3/23 6", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 11, 12, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 6, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hhmm_ddMMyyyy_hh()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6:00 - 23/3/23 8", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 0, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 8, 0, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hhmm_ddMM_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/3 6:15 - 23/3 5:55", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 6, 15, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 5, 55, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hhmm_ddMM_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6:22 - 23/3 7:7", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 22, 0), parsed.Item1);
        Assert.Equal(new DateTime(2025, 3, 23, 7, 7, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMM_hhmm_ddMMyyyy_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03 11:12 - 23/3/23 6:56", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 3, 22, 11, 12, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 6, 56, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_ddMMyyyy_hhmm_ddMMyyyy_hhmm()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("22/03/2023 6:01 - 23/3/23 8:01", new DateTime(2025, 5, 5));
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2023, 3, 22, 6, 1, 0), parsed.Item1);
        Assert.Equal(new DateTime(2023, 3, 23, 8, 1, 0), parsed.Item2);
    }

    [Fact]
    public void ParseTaskTime_Invalid1()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("a 8:00 - b 8:00", new DateTime(2025, 5, 5));
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseTaskTime_Invalid2()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("a 8:00 - 23/3 8:00", new DateTime(2025, 5, 5));
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseTaskTime_Invalid3()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("23/3 8 - a 6", new DateTime(2025, 5, 5));
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseTaskTime_Invalid4()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("a 16:00", new DateTime(2025, 5, 5));
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseTaskTime_Invalid5()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("5/5 16", new DateTime(2025, 5, 5));
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseTaskTime_Invalid6()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("30/2", new DateTime(2025, 5, 5));
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseTaskTime_Invalid7()
    {
        Tuple<DateTime, DateTime> parsed = StringUtils.ParseTaskTime("a 3:00 - 29:60", new DateTime(2025, 5, 5));
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseStdMultiplier_XY1()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("2/4");
        Assert.NotNull(parsed);
        Assert.Equal(2.0, parsed.Item1);
        Assert.Equal(4.0, parsed.Item2);
    }

    [Fact]
    public void ParseStdMultiplier_XY2()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("  2 / 4  ");
        Assert.NotNull(parsed);
        Assert.Equal(2.0, parsed.Item1);
        Assert.Equal(4.0, parsed.Item2);
    }

    [Fact]
    public void ParseStdMultiplier_XY3()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("  -2 /-4  ");
        Assert.NotNull(parsed);
        Assert.Equal(-2.0, parsed.Item1);
        Assert.Equal(-4.0, parsed.Item2);
    }

    [Fact]
    public void ParseStdMultiplier_XY4()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("  333.333 /-10.5  ");
        Assert.NotNull(parsed);
        Assert.Equal(333.333, parsed.Item1);
        Assert.Equal(-10.5, parsed.Item2);
    }

    [Fact]
    public void ParseStdMultiplier_X1()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("42");
        Assert.NotNull(parsed);
        Assert.Equal(42.0, parsed.Item1);
        Assert.Equal(1.0, parsed.Item2);
    }

    [Fact]
    public void ParseStdMultiplier_X2()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("  43.33 ");
        Assert.NotNull(parsed);
        Assert.Equal(43.33, parsed.Item1);
        Assert.Equal(1.0, parsed.Item2);
    }

    [Fact]
    public void ParseStdMultiplier_Fail1()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("w");
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseStdMultiplier_Fail2()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("1/0");
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseStdMultiplier_Fail3()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("2//3");
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseStdMultiplier_Fail4()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier(" 1 2");
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseStdMultiplier_Fail5()
    {
        Tuple<double, double> parsed = StringUtils.ParseStdMultiplier("5/6 + +");
        Assert.Null(parsed);
    }

    [Fact]
    public void ParseDateTime_DShort()
    {
        DateTime? parsed = StringUtils.ParseDateTime(" 11/6", new DateTime(2025, 1, 1));

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 11, 0, 0, 0), parsed.Value);
    }

    [Fact]
    public void ParseDateTime_DLong1()
    {
        DateTime? parsed = StringUtils.ParseDateTime("11 / 6 / 25", new DateTime(2024, 1, 1));

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 11, 0, 0, 0), parsed.Value);
    }

    [Fact]
    public void ParseDateTime_DLong2()
    {
        DateTime? parsed = StringUtils.ParseDateTime("11/6/2025", new DateTime(2024, 1, 1));

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 11, 0, 0, 0), parsed.Value);
    }

    [Fact]
    public void ParseDateTime_DTShort()
    {
        DateTime? parsed = StringUtils.ParseDateTime("11/6 10", new DateTime(2025, 1, 1));

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 11, 10, 0, 0), parsed.Value);
    }

    [Fact]
    public void ParseDateTime_DTLong()
    {
        DateTime? parsed = StringUtils.ParseDateTime("11/6 10:30", new DateTime(2025, 1, 1));

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2025, 6, 11, 10, 30, 0), parsed.Value);
    }

    [Fact]
    public void ParseDateTime_DLongTShort()
    {
        DateTime? parsed = StringUtils.ParseDateTime("11/6/24 10", new DateTime(2025, 1, 1));

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2024, 6, 11, 10, 0, 0), parsed.Value);
    }

    [Fact]
    public void ParseDateTime_DLongTLong()
    {
        DateTime? parsed = StringUtils.ParseDateTime("11/6/2024 10:30", new DateTime(2025, 1, 1));

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2024, 6, 11, 10, 30, 0), parsed.Value);
    }
}