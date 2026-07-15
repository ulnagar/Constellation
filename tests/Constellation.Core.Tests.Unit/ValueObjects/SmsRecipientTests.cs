namespace Constellation.Core.Tests.Unit.ValueObjects;

using Core.ValueObjects;

public class SmsRecipientTests
{
    [Fact]
    public void SmsRecipient_WithSameNumber_ShouldBeEqual()
    {
        // Arrange
        var first = SmsRecipient.Create("Leslie Higgins", "0400111222").Value;
        var second = SmsRecipient.Create("Leslie Higgins", "0400111222").Value;

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
        first.Equals(second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void SmsRecipient_WithSameNumberButDifferentName_ShouldBeEqual()
    {
        // Arrange
        var first = SmsRecipient.Create("Leslie Higgins", "0400111222").Value;
        var second = SmsRecipient.Create("L. Higgins", "0400111222").Value;

        // Act & Assert
        (first == second).Should().BeTrue();
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void SmsRecipient_WithDifferentNumber_ShouldNotBeEqual()
    {
        // Arrange
        var first = SmsRecipient.Create("Leslie Higgins", "0400111222").Value;
        var second = SmsRecipient.Create("Leslie Higgins", "0400333444").Value;

        // Act & Assert
        (first == second).Should().BeFalse();
        (first != second).Should().BeTrue();
        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void SmsRecipient_Number_ShouldReturnUnderlyingValue()
    {
        // Arrange
        var sut = SmsRecipient.Create("Leslie Higgins", "0400111222").Value;

        // Act & Assert
        sut.Number.Should().Be("0400111222");
    }
}