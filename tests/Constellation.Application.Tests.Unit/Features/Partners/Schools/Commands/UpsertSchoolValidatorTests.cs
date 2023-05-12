namespace Constellation.Application.Tests.Unit.Features.Partners.Schools.Commands;

using Constellation.Application.Schools.UpsertSchool;

public class UpsertSchoolCommandValidatorTests
{
    private readonly UpsertSchoolCommandValidator _sut;

    public UpsertSchoolCommandValidatorTests()
    {
        _sut = new UpsertSchoolCommandValidator();
    }

    [Fact]
    public void MustBeValidPhoneNumber_ShouldFailToValidate_WhenNumberIsLessThanTenDigitsInLength()
    {
        // Arrange
        var model = new UpsertSchoolCommand
        {
            Code = "1111",
            EmailAddress = "someschool@det.nsw.edu.au",
            PhoneNumber = "012345"
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("PhoneNumber");
    }

    [Fact]
    public void MustBeValidPhoneNumber_ShouldFailToValidate_WhenNumberStartsWithUnknownPrefix()
    {
        // Arrange
        var model = new UpsertSchoolCommand
        {
            Code = "1111",
            EmailAddress = "someschool@det.nsw.edu.au",
            PhoneNumber = "0123456789"
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("PhoneNumber");
    }

    [Fact]
    public void MustBeValidPhoneNumber_ShouldValidate_WhenNumberIsValidPhoneNumber()
    {
        // Arrange
        var model = new UpsertSchoolCommand
        {
            Code = "1111",
            EmailAddress = "someschool@det.nsw.edu.au",
            PhoneNumber = "0212345678"
        };

        // Act
        var result = _sut.Validate(model).IsValid;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SchoolCode_ShouldFailToValidate_WhenSchoolCodeIsEmpty()
    {
        // Arrange
        var model = new UpsertSchoolCommand
        {
            Code = "",
            EmailAddress = "someschool@det.nsw.edu.au",
            PhoneNumber = "0212345678"
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("Code");
    }

    [Fact]
    public void EmailAddress_ShouldFailToValidate_WhenEmailIsNotValid()
    {
        // Arrange
        var model = new UpsertSchoolCommand
        {
            Code = "1111",
            EmailAddress = "someschooldet.nsw.edu.au",
            PhoneNumber = "0212345678"
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("EmailAddress");
    }
}
