namespace Constellation.Application.Tests.Unit.Features.Contacts;

using Constellation.Application.Domains.Contacts.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Core.ValueObjects;

public class ContactResponseTests
{
    [Fact]
    public void ContactResponse_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var studentId = StudentId.FromValue(Guid.NewGuid());
        var name = Name.Create("Leslie", string.Empty, "Higgins").Value;
        var email = EmailAddress.Create("parent@example.com").Value;

        var first = new ContactResponse(
            StudentReferenceNumber.Empty,
            name,
            Grade.Y07,
            "Test School",
            ContactCategory.ResidentialFamily,
            studentId,
            "Leslie Higgins",
            email,
            null,
            string.Empty);

        var second = new ContactResponse(
            StudentReferenceNumber.Empty,
            name,
            Grade.Y07,
            "Test School",
            ContactCategory.ResidentialFamily,
            studentId,
            "Leslie Higgins",
            EmailAddress.Create("parent@example.com").Value, // separate instance, same email
            null,
            string.Empty);

        // Act & Assert
        first.Should().Be(second);
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void Distinct_ShouldCollapseContactResponses_WithSameEmail()
    {
        // Arrange
        var studentId = StudentId.FromValue(Guid.NewGuid());
        var name = Name.Create("Leslie", string.Empty, "Higgins").Value;

        List<ContactResponse> recipients =
        [
            new(
                StudentReferenceNumber.Empty,
                name,
                Grade.Y07,
                "Test School",
                ContactCategory.ResidentialFamily,
                studentId,
                "Leslie Higgins",
                EmailAddress.Create("parent@example.com").Value,
                null,
                string.Empty),
            new(
                StudentReferenceNumber.Empty,
                name,
                Grade.Y07,
                "Test School",
                ContactCategory.ResidentialFamily,
                studentId,
                "Leslie Higgins",
                EmailAddress.Create("parent@example.com").Value,
                null,
                string.Empty)
        ];

        // Act
        List<ContactResponse> result = recipients.Distinct().ToList();

        // Assert
        result.Should().HaveCount(1);
    }
}