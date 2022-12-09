using Constellation.Application.Extensions;
using Constellation.Core.Models;

namespace Constellation.Application.Tests.Unit.Extensions;

public class MSTeamOperationExtensionsTests
{

    [Fact]
    public void IsOutstanding_ShouldReturnTrue_WhenDateScheduledIsInThePast()
    {
		// Arrange
		var sut = new StudentMSTeamOperation
		{
			DateScheduled = DateTime.Today.AddDays(-1)
		};

		// Act
		var result = sut.IsOutstanding();

		// Assert
		result.Should().BeTrue();
	}

    [Fact]
    public void IsOutstanding_ShouldReturnTrue_WhenDateScheduledIsToday()
    {
        // Arrange
        var sut = new StudentMSTeamOperation
        {
            DateScheduled = DateTime.Today
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOutstanding_ShouldReturnFalse_WhenDateScheduledIsInTheFuture()
    {
        // Arrange
        var sut = new StudentMSTeamOperation
        {
            DateScheduled = DateTime.Today.AddDays(2)
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOutstanding_ShouldReturnFalse_WhenDateScheduledIsInThePastAndMarkedDeleted()
    {
        // Arrange
        var sut = new StudentMSTeamOperation
        {
            DateScheduled = DateTime.Today.AddDays(-1),
            IsDeleted = true
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOutstanding_ShouldReturnFalse_WhenDateScheduledIsInThePastAndMarkedCompleted()
    {
        // Arrange
        var sut = new StudentMSTeamOperation
        {
            DateScheduled = DateTime.Today.AddDays(-1),
            IsCompleted = true
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeFalse();
    }
}
