namespace Constellation.Core.Tests.Unit.Models.Assignments;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Identifiers;

public class CanvasAssignmentTests
{
    [Fact]
    public void AddSubmission_ShouldSetAttemptToOne_WhenNoPreviousAttemptExists()
    {
        // Arrange
        var sut = CanvasAssignment.Create(
            new(),
            "Test Assignment",
            1,
            DateTime.Today,
            DateTime.Today.AddMonths(1),
            DateTime.Today,
            false,
            null,
            3);

        // Act
        var result = sut.AddSubmission(
            "StudentId", 
            "test@email.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<CanvasAssignmentSubmission>();
        result.Value.Attempt.Should().Be(1);
    }

    [Fact]
    public void AddSubmission_ShouldIncrementAttempts_WhenPreviousAttemptExists()
    {
        // Arrange
        var sut = CanvasAssignment.Create(
            new(),
            "Test Assignment",
            1,
            DateTime.Today,
            DateTime.Today.AddMonths(1),
            DateTime.Today,
            false,
            null,
            3);

        sut.AddSubmission(
            "StudentId", 
            "test@email.com");

        // Act
        var result = sut.AddSubmission(
            "StudentId", 
            "test@email.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<CanvasAssignmentSubmission>();
        result.Value.Attempt.Should().Be(2);
    }

    [Fact]
    public void AddSubmission_ShouldRaiseDomainEvent_WhenSubmissionProvided()
    {
        // Arrange
        var sut = CanvasAssignment.Create(
            new(),
            "Test Assignment",
            1,
            DateTime.Today,
            DateTime.Today.AddMonths(1),
            DateTime.Today,
            false,
            null,
            3);

        sut.AddSubmission(
            "StudentId",
            "test@email.com");

        // Act
        var result = sut.GetDomainEvents();

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().BeOfType<AssignmentAttemptSubmittedDomainEvent>();
    }

    [Fact]
    public void ReUploadSubmissionToCanvas_ShouldReturnFailure_WhenSubmissionDoesNotPreviouslyExist()
    {
        // Arrange
        var sut = CanvasAssignment.Create(
            new(),
            "Test Assignment",
            1,
            DateTime.Today,
            DateTime.Today.AddMonths(1),
            DateTime.Today,
            false,
            null,
            3);

        var submission = sut.AddSubmission(
            "StudentId",
            "test@email.com");

        var domainEvent = sut.GetDomainEvents().First();

        sut.ClearDomainEvents();

        // Act
        var result = sut.ReUploadSubmissionToCanvas(new AssignmentSubmissionId());
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        events.Should().HaveCount(0);
    }

    [Fact]
    public void ReUploadSubmissionToCanvas_ShouldRaiseNewDomainEvent_WhenSubmissionPreviouslyExists()
    {
        // Arrange
        var sut = CanvasAssignment.Create(
            new(),
            "Test Assignment",
            1,
            DateTime.Today,
            DateTime.Today.AddMonths(1),
            DateTime.Today,
            false,
            null,
            3);

        var submission = sut.AddSubmission(
            "StudentId",
            "test@email.com");

        var domainEvent = sut.GetDomainEvents().First();

        sut.ClearDomainEvents();

        // Act
        var result = sut.ReUploadSubmissionToCanvas(submission.Value.Id);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<AssignmentAttemptSubmittedDomainEvent>();
        events.First().Id.Should().NotBeEquivalentTo(domainEvent.Id);
    }
}
