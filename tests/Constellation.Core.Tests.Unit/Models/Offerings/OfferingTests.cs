namespace Constellation.Core.Tests.Unit.Models.Offerings;

using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Subjects;

public class OfferingTests
{

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfSessionsIsNull()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfAllSessionsAreDeleted()
    {
        // Arrange
        var sut = new Offering();
        sut.AddSession(1);

        sut.RemoveAllSessions();

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsInThePastAndEndDateIsInTheFuture()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        sut.AddSession(1);

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsTodayAndEndDateIsInTheFuture()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        sut.AddSession(1);

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsInThePastAndEndDateIsToday()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        };

        sut.AddSession(1);

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsTodayAndEndDateIsToday()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        };

        sut.AddSession(1);

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeTrue();
    }
}
