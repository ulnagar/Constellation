using Constellation.Application.Features.Partners.Schools.Commands;
using FluentAssertions;

namespace Constellation.Application.Tests.Unit.Features.Partners.Schools.Commands;

public class UpsertSchoolValidatorTests
{
    private UpsertSchoolValidator _sut;

    public UpsertSchoolValidatorTests()
    {
        _sut = new UpsertSchoolValidator();
    }

    [Fact]
    public void UpsertSchoolValidator_ShouldFailToValidate_WhenNumberIsLessThanTenDigitsInLength()
    {
        // Arrange
        var model = new UpsertSchool();
        model.Code = "1111";
        model.EmailAddress = "someschool@det.nsw.edu.au";
        model.PhoneNumber = "012345";

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("PhoneNumber");
    }

    [Fact]
    public void UpsertSchoolValidator_ShouldFailToValidate_WhenNumberStartsWithUnknownPrefix()
    {
        // Arrange
        var model = new UpsertSchool();
        model.Code = "1111";
        model.EmailAddress = "someschool@det.nsw.edu.au";
        model.PhoneNumber = "0123456789";

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("PhoneNumber");
    }

    [Fact]
    public void UpsertSchoolValidator_ShouldValidate_WhenNumberIsValidPhoneNumber()
    {
        // Arrange
        var model = new UpsertSchool();
        model.Code = "1111";
        model.EmailAddress = "someschool@det.nsw.edu.au";
        model.PhoneNumber = "0212345678";

        // Act
        var result = _sut.Validate(model).IsValid;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UpsertSchoolValidator_ShouldFailToValidate_WhenSchoolCodeIsEmpty()
    {
        // Arrange
        var model = new UpsertSchool();
        model.Code = "";
        model.EmailAddress = "someschool@det.nsw.edu.au";
        model.PhoneNumber = "0212345678";

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("Code");
    }

    [Fact]
    public void UpsertSchoolValidator_ShouldFailToValidate_WhenEmailIsNotValid()
    {
        // Arrange
        var model = new UpsertSchool();
        model.Code = "1111";
        model.EmailAddress = "someschooldet.nsw.edu.au";
        model.PhoneNumber = "0212345678";

        // Act
        var result = _sut.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().PropertyName.Should().Be("EmailAddress");
    }
}
