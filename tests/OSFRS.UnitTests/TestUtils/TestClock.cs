namespace OSFRS.UnitTests.TestUtils;

public class TestClock
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;

    public void AdvanceHours(int h) => UtcNow = UtcNow.AddHours(h);
}
