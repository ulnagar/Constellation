namespace Constellation.Core.Tests.Unit.Models.Messaging;

using Constellation.Core.Models.Messaging.Drafts;
using Core.ValueObjects;

public class MessageRecipientTests
{
    [Fact]
    public void MessageRecipient_WithSameEmail_ShouldBeEqual()
    {
        // Arrange
        var email = EmailAddress.Create("parent@example.com").Value;

        var first = new MessageRecipient(EmailAddress.Create("parent@example.com").Value, "Leslie Higgins");
        var second = new MessageRecipient(EmailAddress.Create("parent@example.com").Value, "Leslie Higgins");

        // Act & Assert
        first.Equals(second).Should().BeTrue();
        (first == second).Should().BeFalse(); // MessageRecipient does not override ==, only Equals/GetHashCode
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void MessageRecipient_WithDifferentEmail_ShouldNotBeEqual()
    {
        // Arrange
        var first = new MessageRecipient(EmailAddress.Create("parent@example.com").Value, "Leslie Higgins");
        var second = new MessageRecipient(EmailAddress.Create("other.parent@example.com").Value, "Leslie Higgins");

        // Act & Assert
        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void MessageDraft_RemoveRecipient_ShouldRemoveMatchingRecipient_ByValueNotReference()
    {
        // Arrange
        var draft = new MessageDraft(Guid.NewGuid());
        var recipient = new MessageRecipient(EmailAddress.Create("parent@example.com").Value, "Leslie Higgins");
        draft.AddRecipient(recipient);

        // A freshly-constructed instance with the same email, not the same reference
        var duplicateInstance = new MessageRecipient(EmailAddress.Create("parent@example.com").Value, "Leslie Higgins");

        // Act
        draft.RemoveRecipient(duplicateInstance);

        // Assert
        draft.Recipients.Should().BeEmpty();
    }
}