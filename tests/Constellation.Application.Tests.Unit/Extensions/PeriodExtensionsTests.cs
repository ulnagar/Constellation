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
    public void GetPeriodDescriptor_ShouldReturnFullNumberPrefixedWithS_WhenPeriodIsInPrimaryGridAndNumberIsTwoDigits()
    {
        // Arrange
        var sut = new TimetablePeriod
        {
            Timetable = "PRIMARY",
            Name = "12"
        };

        // Act
        var result = sut.GetPeriodDescriptor();

        // Assert
        result.Should().Be("S12");
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

    [Fact]
    public void GetPeriodDescriptor_ShouldReturnFullNumber_WhenPeriodNumberIsTwoDigits()
    {
        // Arrange
        var sut = new TimetablePeriod
        {
            Timetable = "SECONDARY",
            Name = "12"
        };

        // Act
        var result = sut.GetPeriodDescriptor();

        // Assert
        result.Should().Be("12");
    }
}
