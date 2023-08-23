namespace Constellation.Core.Tests.Unit.Models.Subjects;

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
        var session = new Session
        {
            IsDeleted = true
        };

        sut.Sessions.Add(session);

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

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

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

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

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

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

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

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeTrue();
    }
}
