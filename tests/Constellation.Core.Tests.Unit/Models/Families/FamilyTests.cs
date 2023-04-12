namespace Constellation.Core.Tests.Unit.Models.Families;

using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;

public class FamilyTests
{
    private const string FamilyName = "Mr L Higgins";
    private const string SentralId = "100";
    private const string FamilyAddressLine1 = "123 Fake Street";
    private const string FamilyAddressLine2 = "Unit 3";
    private const string FamilyAddressTown = "Nowhere";
    private const string FamilyAddressPostCode = "1234";
    private const string InvalidEmail = "not.valid@";
    private const string ValidEmail = "test@here.com";
    private const string OtherValidEmail = "test2@here.com";
    private const string ParentTitle = "Mr";
    private const string ParentFirstName = "Leslie";
    private const string ParentLastName = "Higgins";
    private const string ParentMobile = "0400111222";
    private const string StudentId = "123456789";

    [Fact]
    public void LinkFamilyToSentralDetails_ShouldReturnFailure_WhenEmptySentralIdProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.LinkFamilyToSentralDetails(string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void LinkFamilyToSentralDetails_ShouldReturnSuccess_WhenValidSentralIdProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.LinkFamilyToSentralDetails(SentralId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.SentralId.Should().Be(SentralId);
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnFailure_WhenTitleIsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            string.Empty,
            FamilyAddressLine1,
            FamilyAddressLine2,
            FamilyAddressTown,
            FamilyAddressPostCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnFailure_WhenLine1IsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            FamilyName,
            string.Empty,
            FamilyAddressLine2,
            FamilyAddressTown,
            FamilyAddressPostCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnFailure_WhenTownIsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            FamilyName,
            FamilyAddressLine1,
            FamilyAddressLine2,
            string.Empty,
            FamilyAddressPostCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnFailure_WhenPostCodeIsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            FamilyName,
            FamilyAddressLine1,
            FamilyAddressLine2,
            FamilyAddressTown,
            string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnSuccess_WhenLine2IsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            FamilyName,
            FamilyAddressLine1,
            string.Empty,
            FamilyAddressTown,
            FamilyAddressPostCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.FamilyTitle.Should().Be(FamilyName);
        sut.AddressLine1.Should().Be(FamilyAddressLine1);
        sut.AddressTown.Should().Be(FamilyAddressTown);
        sut.AddressPostCode.Should().Be(FamilyAddressPostCode);
        sut.AddressLine2.Should().Be(string.Empty);
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnReplaceLine2_WhenLine2IsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        sut.UpdateFamilyAddress(
            FamilyName,
            FamilyAddressLine1,
            FamilyAddressLine2,
            FamilyAddressTown,
            FamilyAddressPostCode);

        // Act
        var result = sut.UpdateFamilyAddress(
            "Another Name",
            "Another Address Line 1",
            string.Empty,
            "Another Town",
            "PostCode");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.FamilyTitle.Should().Be("Another Name");
        sut.AddressLine1.Should().Be("Another Address Line 1");
        sut.AddressTown.Should().Be("Another Town");
        sut.AddressPostCode.Should().Be("PostCode");
        sut.AddressLine2.Should().Be(string.Empty);
    }

    [Fact]
    public void UpdateFamilyEmail_ShouldReturnFailure_WhenInvalidEmailIsProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyEmail(InvalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateFamilyEmail_ShouldReturnFailure_WhenProvidedEmailIsBlank()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyEmail(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateFamilyEmail_ShouldRaiseDomainEvent_WhenEmailIsChanged()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateFamilyEmail(ValidEmail);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<FamilyEmailAddressChangedDomainEvent>();
    }

    [Fact]
    public void AddParent_ShouldReturnFailure_WhenInvalidEmailIsProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            InvalidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void AddParent_ShouldReturnFailure_WhenParentAlreadyExists()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Act 
        var result = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Should().BeEquivalentTo(DomainErrors.Families.Parents.AlreadyExists);
    }

    [Fact]
    public void AddParent_ShouldRaiseDomainEvent_WhenParentIsAdded()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<ParentAddedToFamilyDomainEvent>();
    }

    [Fact]
    public void AddParent_ShouldIncludeMobileNumber_WhenMobileNumberIsValid()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        result.Value.MobileNumber.Should().Be(ParentMobile);
    }

    [Fact]
    public void AddParent_ShouldHaveEmptyMobileNumber_WhenMobileNumberIsInvalid()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            "1",
            ValidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        result.Value.MobileNumber.Should().Be(string.Empty);
    }

    [Fact]
    public void UpdateParent_ShouldReturnFailure_WhenInvalidEmailProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);
        
        var parent = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            InvalidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void UpdateParent_ShouldReturnFailure_WhenParentDoesNotExist()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.UpdateParent(
            new ParentId(),
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void UpdateParent_ShouldRaiseDomainEvent_WhenParentEmailChanges()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        var parent = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        sut.ClearDomainEvents();

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            OtherValidEmail,
            Parent.SentralReference.None);

        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<ParentEmailAddressChangedDomainEvent>();
    }

    [Fact]
    public void UpdateParent_ShouldNotRaiseDomainEvent_WhenParentEmailDoesNotChange()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        var parent = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        sut.ClearDomainEvents();

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().BeNullOrEmpty();
    }

    [Fact]
    public void UpdateParent_ShouldIncludeMobileNumber_WhenMobileNumberIsValid()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        var parent = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            string.Empty,
            ValidEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        result.Value.MobileNumber.Should().Be(ParentMobile);
    }

    [Fact]
    public void UpdateParent_ShouldHaveEmptyMobileNumber_WhenMobileNumberIsInvalid()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        var parent = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            string.Empty,
            ValidEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        result.Value.MobileNumber.Should().Be(string.Empty);
    }

    [Fact]
    public void RemoveParent_ShouldReturnFailure_WhenParentDoesNotExist()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        var otherFamily = Family.Create(
            new FamilyId(),
            FamilyName);

        var parent = otherFamily.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.RemoveParent(parent.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void RemoveParent_ShouldRaiseDomainEvent_WhenParentIsRemoved()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        var parent = sut.AddParent(
            ParentTitle,
            ParentFirstName,
            ParentLastName,
            ParentMobile,
            ValidEmail,
            Parent.SentralReference.None);

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveParent(parent.Value);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<ParentRemovedFromFamilyDomainEvent>();
    }

    [Fact]
    public void AddStudent_ShouldRaiseDomainEvent_WhenStudentIsAdded()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.AddStudent(StudentId, false);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentAddedToFamilyDomainEvent>();
    }

    [Fact]
    public void AddStudent_ShouldNotRaiseDomainEvent_WhenStudentAlreadyExistsAndResidentialStatusUnchanged()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        sut.AddStudent(StudentId, false);
        sut.ClearDomainEvents();

        // Act
        var result = sut.AddStudent(StudentId, false);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().BeNullOrEmpty();
    }

    [Fact]
    public void AddStudent_ShouldRaiseDomainEvent_WhenStudentAlreadyExistsAndResidentialStatusChangedToResidential()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        sut.AddStudent(StudentId, false);
        sut.ClearDomainEvents();

        // Act
        var result = sut.AddStudent(StudentId, true);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentResidentialFamilyChangedDomainEvent>();
    }

    [Fact]
    public void RemoveStudent_ShouldReturnSuccessWithoutRaisingDomainEvent_WhenStudentNotFound()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        // Act
        var result = sut.RemoveStudent(StudentId);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().BeNullOrEmpty();
    }

    [Fact]
    public void RemoveStudent_ShouldReturnSuccessAndRaiseDomainEvent_WhenStudentIsRemoved()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            FamilyName);

        sut.AddStudent(StudentId, false);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveStudent(StudentId);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentRemovedFromFamilyDomainEvent>();
    }
}
