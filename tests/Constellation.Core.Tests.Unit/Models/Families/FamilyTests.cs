namespace Constellation.Core.Tests.Unit.Models.Families;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;

public class FamilyTests
{
    [Fact]
    public void LinkFamilyToSentralDetails_ShouldReturnFailure_WhenEmptySentralIdProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            "Mr B Hillsley");

        // Act
        var result = sut.LinkFamilyToSentralDetails(null);

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
            "Mr B Hillsley");

        // Act
        var result = sut.LinkFamilyToSentralDetails("100");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.SentralId.Should().Be("100");
    }

    [Fact]
    public void UpdateFamilyAddress_ShouldReturnFailure_WhenTitleIsNotProvided()
    {
        // Arrange
        var sut = Family.Create(
            new FamilyId(),
            "Mr B Hillsley");

        // Act
        var result = sut.UpdateFamilyAddress(
            null,
            "123 Fake Street",
            "Unit 1",
            "Nowhere",
            "1234");

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
            "Mr B Hillsley");

        // Act
        var result = sut.UpdateFamilyAddress(
            "Mr B Hillsley",
            null,
            "Unit 1",
            "Nowhere",
            "1234");

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
            "Mr B Hillsley");

        // Act
        var result = sut.UpdateFamilyAddress(
            "Mr B Hillsley",
            "123 Fake Street",
            "Unit 1",
            null,
            "1234");

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
            "Mr B Hillsley");

        // Act
        var result = sut.UpdateFamilyAddress(
            "Mr B Hillsley",
            "123 Fake Street",
            "Unit 1",
            "Nowhere",
            null);

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
            "Mr B Hillsley");

        // Act
        var result = sut.UpdateFamilyAddress(
            "Mr B Hillsley",
            "123 Fake Street",
            null,
            "Nowhere",
            "1234");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.FamilyTitle.Should().Be("Mr B Hillsley");
        sut.AddressLine1.Should().Be("123 Fake Street");
        sut.AddressTown.Should().Be("Nowhere");
        sut.AddressPostCode.Should().Be("1234");
        sut.AddressLine2.Should().Be(null);
    }
}
