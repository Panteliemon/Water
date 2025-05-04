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
}