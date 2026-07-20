namespace Constellation.Application.Tests.Unit.Extensions;

using Constellation.Core.Enums;
using Core.Extensions;

public class GradeExtensionsTests
{

    [Fact]
    public void AsName_ShouldReturnZeroFilledString_WhenProvidedWithGradeWithSingleDigit()
    {
		// Arrange
		var sut = Grade.Y06;

		// Act
		var result = sut.AsName();

		// Assert
		result.Should().Be("Year 06");
	}
}
