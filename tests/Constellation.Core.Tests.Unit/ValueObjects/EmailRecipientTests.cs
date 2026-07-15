namespace Constellation.Core.Tests.Unit.ValueObjects;

using Core.ValueObjects;

public class EmailRecipientTests
{
    [Fact]
    public void EmailRecipient_WithSameEmail_ShouldBeEqual()
    {
        // Arrange
        var first = EmailRecipient.Create("Leslie Higgins", "parent@example.com").Value;
        var second = EmailRecipient.Create("Leslie Higgins", "parent@example.com").Value;

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
        first.Equals(second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void EmailRecipient_WithSameEmailButDifferentName_ShouldBeEqual()
    {
        // Arrange
        // Equality is keyed off the email address only, matching PhoneNumber's behaviour
        var first = EmailRecipient.Create("Leslie Higgins", "parent@example.com").Value;
        var second = EmailRecipient.Create("L. Higgins", "parent@example.com").Value;

        // Act & Assert
        (first == second).Should().BeTrue();
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void EmailRecipient_WithDifferentEmail_ShouldNotBeEqual()
    {
        // Arrange
        var first = EmailRecipient.Create("Leslie Higgins", "parent@example.com").Value;
        var second = EmailRecipient.Create("Leslie Higgins", "other.parent@example.com").Value;

        // Act & Assert
        (first == second).Should().BeFalse();
        (first != second).Should().BeTrue();
        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void EmailRecipient_Email_ShouldReturnUnderlyingValue()
    {
        // Arrange
        var sut = EmailRecipient.Create("Leslie Higgins", "parent@example.com").Value;

        // Act & Assert
        sut.Email.Should().Be("parent@example.com");
    }
}