namespace Constellation.Core.Tests.Unit.Models.Awards;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Awards.Enums;
using Constellation.Core.Models.Awards.Identifiers;
using Core.ValueObjects;
using Shared;

public class NominationNotificationTests
{
    [Fact]
    public void Create_ShouldDeduplicateToAddresses_WhenDuplicateEmailsAreProvided()
    {
        // Arrange
        Result<EmailRecipient> from = EmailRecipient.Create("Sender", "sender@example.com");
        Result<EmailRecipient> duplicate = EmailRecipient.Create("Recipient", "to@example.com");
        List<EmailRecipient> toList = new List<EmailRecipient> { duplicate.Value, duplicate.Value };

        // Act
        NominationNotification notification = NominationNotification.Create(
            new AwardNominationPeriodId(),
            AwardNotificationType.Parent,
            [],
            DateTime.Now,
            from.Value,
            toList,
            [],
            string.Empty,
            string.Empty);

        // Assert
        notification.ToAddresses.Should().HaveCount(1);
    }

    [Fact]
    public void Create_ShouldDeduplicateCcAddresses_WhenDuplicateEmailsAreProvided()
    {
        // Arrange
        Result<EmailRecipient> from = EmailRecipient.Create("Sender", "sender@example.com");
        Result<EmailRecipient> duplicate = EmailRecipient.Create("Recipient", "to@example.com");
        List<EmailRecipient> ccList = new List<EmailRecipient> { duplicate.Value, duplicate.Value };

        // Act
        NominationNotification notification = NominationNotification.Create(
            new AwardNominationPeriodId(),
            AwardNotificationType.Parent,
            [],
            DateTime.Now,
            from.Value,
            [],
            ccList,
            string.Empty,
            string.Empty);

        // Assert
        notification.CcAddresses.Should().HaveCount(1);
    }

    [Fact]
    public void Create_ShouldRetainDistinctToAddresses_WhenUniqueEmailsAreProvided()
    {
        // Arrange
        Result<EmailRecipient> from = EmailRecipient.Create("Sender", "sender@example.com");
        Result<EmailRecipient> to1 = EmailRecipient.Create("Recipient One", "one@example.com");
        Result<EmailRecipient> to2 = EmailRecipient.Create("Recipient Two", "two@example.com");
        List<EmailRecipient> toList = new List<EmailRecipient> { to1.Value, to2.Value };

        // Act
        NominationNotification notification = NominationNotification.Create(
            new AwardNominationPeriodId(),
            AwardNotificationType.Parent,
            [],
            DateTime.Now,
            from.Value,
            toList,
            [],
            string.Empty,
            string.Empty);

        // Assert
        notification.ToAddresses.Should().HaveCount(2);
    }

    [Fact]
    public void Create_ShouldIncludeAllNominations()
    {
        // Arrange
        Result<EmailRecipient> from = EmailRecipient.Create("Sender", "sender@example.com");
        List<AwardNominationId> nominations = new List<AwardNominationId> { new(), new(), new() };

        // Act
        NominationNotification notification = NominationNotification.Create(
            new AwardNominationPeriodId(),
            AwardNotificationType.Parent,
            nominations,
            DateTime.Now,
            from.Value,
            [],
            [],
            string.Empty,
            string.Empty);

        // Assert
        notification.Nominations.Should().HaveCount(3);
        notification.Nominations.Should().BeEquivalentTo(nominations);
    }
}