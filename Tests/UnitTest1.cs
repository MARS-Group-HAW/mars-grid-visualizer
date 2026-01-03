namespace MarsGridVisualizer.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.True(true);

        var mapString = Map.ReadInMapFromLines(
                ["1;1;1;1;1;1",
                 "1;0;0;0;0;1",
                 "1;0;0;0;0;1",
                 "1;0;0;0;0;1",
                 "1;0;0;0;0;1",
                 "1;1;1;1;1;1",]).ToString();

        Assert.Equal("HHHHHH\nH    H\nH    H\nH    H\nH    H\nHHHHHH\n", mapString);
    }
}
