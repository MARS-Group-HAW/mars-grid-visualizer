using mmvp;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.True(true);

        Assert.Equal(" ", Map.ReadInMapFromLines(
                ["1;1;1;1;1;1",
                 "1;0;0;0;0;1",
                 "1;0;0;0;0;1",
                 "1;0;0;0;0;1",
                 "1;0;0;0;0;1",
                 "1;1;1;1;1;1",]).ToString());
    }
}
