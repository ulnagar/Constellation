namespace Constellation.Core.Tests.Unit.Models.GroupTutorials;

using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;

public class GroupTutorialTests
{
    [Fact]
    public void AddTeacher_ShouldReturnFailure_WhenTutorialHasExpired()
    {
        // Arrange
        var sut = new GroupTutorial(
            Guid.NewGuid(), 
            "Stage 4 Mathematics", 
            DateTime.Today.AddMonths(-1),
            DateTime.Today.AddDays(-1));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DomainErrors.GroupTutorials.TutorialHasExpired.Code);
    }

    [Fact]
    public void AddTeacher_ShouldReturnFailure_WhenTutorialHasBeenDeleted()
    {
        // Arrange
        var sut = new GroupTutorial(
            Guid.NewGuid(),
            "Stage 4 Mathematics",
            DateTime.Today.AddMonths(-1),
            DateTime.Today.AddMonths(1));

        sut.IsDeleted = true;

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DomainErrors.GroupTutorials.TutorialHasExpired.Code);
    }

    [Fact]
    public void AddTeacher_ShouldReturnSuccessWithSameId_WhenTeacherIsAlreadyAdded()
    {
        // Arrange
        var sut = new GroupTutorial(
            Guid.NewGuid(),
            "Stage 4 Mathematics",
            DateTime.Today.AddMonths(-1),
            DateTime.Today.AddMonths(1));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var initialResult = sut.AddTeacher(teacher);

        Guid existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : Guid.NewGuid();

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        result.IsSuccess.Should().Be(true);
        result.Value.Should().NotBe(null);
        result.Value.Id.Should().Be(existingRecord);
    }

    [Fact]
    public void AddTeacher_ShouldReturnSuccessWithNewId_WhenTeacherWasPreviouslyRemoved()
    {
        // Arrange
        var sut = new GroupTutorial(
            Guid.NewGuid(),
            "Stage 4 Mathematics",
            DateTime.Today.AddMonths(-1),
            DateTime.Today.AddMonths(1));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var initialResult = sut.AddTeacher(teacher);

        Guid existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : Guid.NewGuid();

        sut.RemoveTeacher(teacher);

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        result.IsSuccess.Should().Be(true);
        result.Value.Should().NotBe(null);
        result.Value.Id.Should().NotBe(existingRecord);
    }

    [Fact]
    public void AddTeacher_ShouldRaiseDomainEvent_WhenTeacherSucessfullyAdded()
    {
        // Arrange
        var sut = new GroupTutorial(
            Guid.NewGuid(),
            "Stage 4 Mathematics",
            DateTime.Today.AddMonths(-1),
            DateTime.Today.AddMonths(1));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        // Act
        var result = sut.AddTeacher(teacher);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().Be(true);
        result.Value.Should().NotBe(null);
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<TeacherAddedToGroupTutorialDomainEvent>();
    }
}
