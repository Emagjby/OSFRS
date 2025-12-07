using FluentAssertions;
using OSFRS.Backend.Helpers.Usage;

namespace OSFRS.UnitTests.Helpers.Usage;

public class UsageEventBuilderTests
{
    [Fact]
    public void Create_ShouldSetCorrectEventType()
    {
        var dto = UsageEventBuilder.Create("ReservationCreated");

        dto.EventType.Should().Be("ReservationCreated");
    }

    [Fact]
    public void Create_ShouldSetCorrectUserId()
    {
        var dto = UsageEventBuilder.Create("TestEvent", userId: 42);

        dto.UserId.Should().Be(42);
    }

    [Fact]
    public void Create_ShouldAssignCurrentUtcTimestamp()
    {
        var before = DateTime.UtcNow;
        var dto = UsageEventBuilder.Create("TestEvent");
        var after = DateTime.UtcNow;

        dto.Timestamp.Should().BeOnOrAfter(before);
        dto.Timestamp.Should().BeOnOrBefore(after);
        dto.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void CreateWithMetadata_ShouldConvertMetadataValuesToStrings()
    {
        var metadataObj = new Dictionary<string, object>
        {
            ["a"] = 123,
            ["b"] = true,
            ["c"] = null!,
        };

        var dto = UsageEventBuilder.CreateWithMetadata("TestMeta", metadataObj, userId: 7);

        dto.Metadata.Should().ContainKey("a").WhoseValue.Should().Be("123");
        dto.Metadata.Should().ContainKey("b").WhoseValue.Should().Be("True");
        dto.Metadata.Should().ContainKey("c").WhoseValue.Should().Be(string.Empty);
        dto.UserId.Should().Be(7);
    }

    [Fact]
    public void CreateWithMetadata_ShouldAssignTimestamp()
    {
        var before = DateTime.UtcNow;
        var dto = UsageEventBuilder.CreateWithMetadata("Test", new());
        var after = DateTime.UtcNow;

        dto.Timestamp.Should().BeOnOrAfter(before);
        dto.Timestamp.Should().BeOnOrBefore(after);
    }
}
