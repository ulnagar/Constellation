using Constellation.Application.Extensions;
using Constellation.Core.Models;

namespace Constellation.Application.Tests.Unit.Extensions;

public class PeriodExtensionsTests
{

    [Fact]
    public void GetPeriodGroup_ShouldReturnDescription_WhenProvidedWithPeriodObject()
    {
		// Arrange
		var sut = new TimetablePeriod
		{
			Timetable = "Secondary",
			Day = 3
		};

		// Act
		var result = sut.GetPeriodGroup();

		// Assert
		result.Should().Be("SEC Week A Wednesday");
	}

	[Fact]
	public void GetPeriodDescriptor_ShouldReturnStringPrefixedWithS_WhenPeriodIsInPrimaryGrid()
	{
		// Arrange
		var sut = new TimetablePeriod
		{
			Timetable = "PRIMARY",
			Name = "2"
		};

		// Act
		var result = sut.GetPeriodDescriptor();

		// Assert
		result.Should().Be("S2");
	}

    [Fact]
    public void GetPeriodDescriptor_ShouldReturnStringPrefixedWithS_WhenPeriodIsInPrimaryGridAndHasPeriodInTheName()
    {
        // Arrange
        var sut = new TimetablePeriod
        {
            Timetable = "PRIMARY",
            Name = "Period 2"
        };

        // Act
        var result = sut.GetPeriodDescriptor();

        // Assert
        result.Should().Be("S2");
    }

    [Fact]
    public void GetPeriodDescriptor_ShouldReturnStringWithoutPrefix_WhenPeriodIsNotInPrimaryGrid()
    {
        // Arrange
        var sut = new TimetablePeriod
        {
            Timetable = "SECONDARY",
            Name = "2"
        };

        // Act
        var result = sut.GetPeriodDescriptor();

        // Assert
        result.Should().Be("2");
    }

    [Fact]
    public void GetPeriodDescriptor_ShouldReturnStringWithoutPrefix_WhenPeriodIsNotInPrimaryGridAndHasPeriodInTheName()
    {
        // Arrange
        var sut = new TimetablePeriod
        {
            Timetable = "SECONDARY",
            Name = "Period 2"
        };

        // Act
        var result = sut.GetPeriodDescriptor();

        // Assert
        result.Should().Be("2");
    }

    [Theory]
    [InlineData("Lunch", "L")]
    [InlineData("Recess", "R")]
    public void GetPeriodDescriptor_ShouldReturnFirstLetter_WhenPeriodIsLunchOrRecess(string period, string expected)
    {
        // Arrange
        var sut = new TimetablePeriod
        {
            Timetable = "SECONDARY",
            Name = period
        };

        // Act
        var result = sut.GetPeriodDescriptor();

        // Assert
        result.Should().Be(expected);
    }
}
