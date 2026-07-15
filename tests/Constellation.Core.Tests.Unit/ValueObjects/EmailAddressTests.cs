namespace Constellation.Core.Tests.Unit.ValueObjects;

using Core.ValueObjects;

public class EmailAddressTests
{
    [Fact]
    public void EmailAddress_WithSameValue_ShouldBeEqual()
    {
        var first = EmailAddress.Create("parent@example.com").Value;
        var second = EmailAddress.Create("parent@example.com").Value;

        (first == second).Should().BeTrue();
        first.Equals(second).Should().BeTrue();
    }
}