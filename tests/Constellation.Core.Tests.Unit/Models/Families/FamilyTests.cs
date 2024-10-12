namespace Constellation.Core.Tests.Unit.Models.Families;

using Constellation.Core.Errors;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Families.Events;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;

public class FamilyTests
{
    private const string _familyName = "Mr L Higgins";
    private const string _sentralId = "100";
    private const string _familyAddressLine1 = "123 Fake Street";
    private const string _familyAddressLine2 = "Unit 3";
    private const string _familyAddressTown = "Nowhere";
    private const string _familyAddressPostCode = "1234";
    private const string _invalidEmail = "not.valid@";
    private const string _validEmail = "test@here.com";
    private const string _otherValidEmail = "test2@here.com";
    private const string _parentTitle = "Mr";
    private const string _parentFirstName = "Leslie";
    private const string _parentLastName = "Higgins";
    private const string _parentMobile = "0400111222";
    private readonly StudentId _studentId = StudentId.FromValue(new Guid("27864b85-a672-48cb-a93a-ad671ba72d24")) ;
    private readonly StudentReferenceNumber _studentReferenceNumber = StudentReferenceNumber.FromValue("123456789");

    [Fact]
    public void LinkFamilyToSentralDetails_ShouldReturnFailure_WhenEmptySentralIdProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

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
            _familyName);

        // Act
        var result = sut.LinkFamilyToSentralDetails(_sentralId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.SentralId.Should().Be(_sentralId);
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnFailure_WhenTitleIsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            string.Empty,
            _familyAddressLine1,
            _familyAddressLine2,
            _familyAddressTown,
            _familyAddressPostCode);

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
            _familyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            _familyName,
            string.Empty,
            _familyAddressLine2,
            _familyAddressTown,
            _familyAddressPostCode);

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
            _familyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            _familyName,
            _familyAddressLine1,
            _familyAddressLine2,
            string.Empty,
            _familyAddressPostCode);

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
            _familyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            _familyName,
            _familyAddressLine1,
            _familyAddressLine2,
            _familyAddressTown,
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
            _familyName);

        // Act
        var result = sut.UpdateFamilyAddress(
            _familyName,
            _familyAddressLine1,
            string.Empty,
            _familyAddressTown,
            _familyAddressPostCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.FamilyTitle.Should().Be(_familyName);
        sut.AddressLine1.Should().Be(_familyAddressLine1);
        sut.AddressTown.Should().Be(_familyAddressTown);
        sut.AddressPostCode.Should().Be(_familyAddressPostCode);
        sut.AddressLine2.Should().Be(string.Empty);
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnReplaceLine2_WhenLine2IsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

        sut.UpdateFamilyAddress(
            _familyName,
            _familyAddressLine1,
            _familyAddressLine2,
            _familyAddressTown,
            _familyAddressPostCode);

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
            _familyName);

        // Act
        var result = sut.UpdateFamilyEmail(_invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateFamilyEmail_ShouldReturnFailure_WhenProvidedEmailIsBlank()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

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
            _familyName);

        // Act
        var result = sut.UpdateFamilyEmail(_validEmail);
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
            _familyName);

        // Act
        var result = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _invalidEmail,
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
            _familyName);

        sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        // Act 
        var result = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
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
            _familyName);

        // Act
        var result = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
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
            _familyName);

        // Act
        var result = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        result.Value.MobileNumber.Should().Be(_parentMobile);
    }

    [Fact]
    public void AddParent_ShouldHaveEmptyMobileNumber_WhenMobileNumberIsInvalid()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

        // Act
        var result = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            "1",
            _validEmail,
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
            _familyName);
        
        var parent = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _invalidEmail,
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
            _familyName);

        // Act
        var result = sut.UpdateParent(
            new ParentId(),
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
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
            _familyName);

        var parent = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        sut.ClearDomainEvents();

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _otherValidEmail,
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
            _familyName);

        var parent = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        sut.ClearDomainEvents();

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
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
            _familyName);

        var parent = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            string.Empty,
            _validEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        result.Value.MobileNumber.Should().Be(_parentMobile);
    }

    [Fact]
    public void UpdateParent_ShouldHaveEmptyMobileNumber_WhenMobileNumberIsInvalid()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

        var parent = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.UpdateParent(
            parent.Value.Id,
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            string.Empty,
            _validEmail,
            Parent.SentralReference.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Parent>();
        result.Value.MobileNumber.Should().Be(string.Empty);
    }

    [Fact]
    public void RemoveParent_ShouldReturnSuccessWithoutRaisingDomainEvent_WhenParentNotFound()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

        var otherFamily = Family.Create(
            new FamilyId(),
            _familyName);

        var parent = otherFamily.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        // Act
        var result = sut.RemoveParent(parent.Value.Id);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().BeNullOrEmpty();
    }

    [Fact]
    public void RemoveParent_ShouldRaiseDomainEvent_WhenParentIsRemoved()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            _familyName);

        var parent = sut.AddParent(
            _parentTitle,
            _parentFirstName,
            _parentLastName,
            _parentMobile,
            _validEmail,
            Parent.SentralReference.None);

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveParent(parent.Value.Id);
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
            _familyName);

        // Act
        var result = sut.AddStudent(_studentId, _studentReferenceNumber, false);
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
            _familyName);

        sut.AddStudent(_studentId, _studentReferenceNumber, false);
        sut.ClearDomainEvents();

        // Act
        var result = sut.AddStudent(_studentId, _studentReferenceNumber, false);
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
            _familyName);

        sut.AddStudent(_studentId, _studentReferenceNumber, false);
        sut.ClearDomainEvents();

        // Act
        var result = sut.AddStudent(_studentId, _studentReferenceNumber, true);
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
            _familyName);

        // Act
        var result = sut.RemoveStudent(_studentId);
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
            _familyName);

        sut.AddStudent(_studentId, _studentReferenceNumber, false);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveStudent(_studentId);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentRemovedFromFamilyDomainEvent>();
    }
}
