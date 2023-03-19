using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;

namespace Constellation.Core.Tests.Unit.Models.Covers;

public class ClassCoverTests
{
    [Fact]
    public void EditDates_ShouldRaiseCorrectDomainEvent_WhenOnlyStartDateChanged()
    {
        // Arrange
        var sut = ClassCover.Create(
            new ClassCoverId(Guid.NewGuid()),
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            ValueObjects.CoverTeacherType.Staff,
            "1");

        sut.ClearDomainEvents();

        // Act
        sut.EditDates(
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            sut.EndDate);
        var events = sut.GetDomainEvents();

        // Assert
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<CoverStartDateChangedDomainEvent>();
    }

    [Fact]
    public void EditDates_ShouldRaiseCorrectDomainEvent_WhenOnlyEndDateChanged()
    {        
        // Arrange
        var sut = ClassCover.Create(
            new ClassCoverId(Guid.NewGuid()),
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            ValueObjects.CoverTeacherType.Staff,
            "1");

        sut.ClearDomainEvents();

        // Act
        sut.EditDates(
            sut.StartDate,
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var events = sut.GetDomainEvents();

        // Assert
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<CoverEndDateChangedDomainEvent>();
    }

    [Fact]
    public void EditDates_ShouldRaiseCorrectDomainEvent_WhenBothStartAndEndDatesChanged()
    {
        // Arrange
        var sut = ClassCover.Create(
            new ClassCoverId(Guid.NewGuid()),
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            ValueObjects.CoverTeacherType.Staff,
            "1");

        sut.ClearDomainEvents();

        // Act
        sut.EditDates(
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var events = sut.GetDomainEvents();

        // Assert
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<CoverStartAndEndDatesChangedDomainEvent>();
    }
}
