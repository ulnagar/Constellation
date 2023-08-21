using Constellation.Application.Extensions;
using Constellation.Core.Models.Subjects;

namespace Constellation.Application.Tests.Unit.Extensions;

public class CourseOfferingExtensionsTests
{

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfSessionsIsNull()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(1)
        };

        // Act
        var result = sut.IsCurrent();

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
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsInThePastAndEndDateIsInTheFuture()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(1)
        };

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsTodayAndEndDateIsInTheFuture()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsInThePastAndEndDateIsToday()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsTodayAndEndDateIsToday()
    {
        // Arrange
        var sut = new Offering
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today
        };

        var session = new Session
        {
            IsDeleted = false
        };

        sut.Sessions.Add(session);

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }
}
