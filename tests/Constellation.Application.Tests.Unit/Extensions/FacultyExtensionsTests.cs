using Constellation.Application.Extensions;
using Constellation.Core.Enums;

namespace Constellation.Application.Tests.Unit.Extensions;

public class FacultyExtensionsTests
{
    [Fact]
    public void AsList_ShouldReturnListOfStringWithOneFacultyName_WhenProvidedFacultyWithSingleValue()
    {
		// Arrange
		var sut = Faculty.Administration;

		// Act
		var result = sut.AsList();

		// Assert
		result.Count.Should().Be(1);
		result.Should().Contain("Admin");
	}

    [Fact]
    public void AsList_ShouldReturnListOfStringWithMultipleFacultyNames_WhenProvidedFacultyWithMultipleValues()
    {
        // Arrange
        var sut = Faculty.Administration;
        sut |= Faculty.Executive;

        // Act
        var result = sut.AsList();

        // Assert
        result.Count.Should().Be(2);
        result.Should().Contain("Admin");
        result.Should().Contain("Exec");
    }
}
