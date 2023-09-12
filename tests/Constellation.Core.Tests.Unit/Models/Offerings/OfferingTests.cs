namespace Constellation.Core.Tests.Unit.Models.Offerings;

using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects;

public class OfferingTests
{

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfSessionsIsNull()
    {
        // Arrange
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            1,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        );

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfAllSessionsAreDeleted()
    {
        // Arrange
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            1,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        );

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
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            1,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        );

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
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        );

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
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            1,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.Today)
        );

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
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddSession(1);

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeTrue();
    }
}
