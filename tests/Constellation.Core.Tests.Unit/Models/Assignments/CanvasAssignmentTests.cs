namespace Constellation.Core.Tests.Unit.Models.Assignments;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Assignments;
using Core.Models.Students.Identifiers;

public class CanvasAssignmentTests
{
    private readonly StudentId _studentId = StudentId.FromValue(new Guid("27864b85-a672-48cb-a93a-ad671ba72d24"));

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
            _studentId, 
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
            _studentId, 
            "test@email.com");

        // Act
        var result = sut.AddSubmission(
            _studentId, 
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
            _studentId,
            "test@email.com");

        // Act
        var result = sut.GetDomainEvents();

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().BeOfType<AssignmentAttemptSubmittedDomainEvent>();
    }
}
