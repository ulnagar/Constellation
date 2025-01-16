namespace Constellation.Core.Tests.Unit.Models.Offerings;

using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Identifiers;
using Core.Models.Timetables.Identifiers;

public class OfferingTests
{
    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfSessionsIsNull()
    {
        // Arrange
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
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
            new(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        );

        sut.AddSession(new());

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
            new(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        );

        sut.AddSession(new());

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
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        );

        sut.AddSession(new());

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
            new(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddSession(new());

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
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddSession(new());

        // Act
        var result = sut.IsCurrent;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AddTeacher_ShouldReturnSuccess_WhenEntryDoesNotAlreadyExist()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Act
        var result = sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddTeacher_ShouldCreateEntry_WhenEntryDoesNotAlreadyExist()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Act
        var result = sut.AddTeacher("1", AssignmentType.ClassroomTeacher);
        var entry = sut.Teachers.FirstOrDefault(assignment => assignment.StaffId == "1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        entry.Should().NotBeNull();
    }

    [Fact]
    public void AddTeacher_ShouldReturnFailure_WhenNonDeletedEntryAlreadyExists()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Arrange
        sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        // Act
        var result = sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(OfferingErrors.AddTeacher.AlreadyExists);
    }

    [Fact]
    public void AddTeacher_ShouldReturnSuccess_WhenDeletedEntryAlreadyExists()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        sut.RemoveTeacher("1", AssignmentType.ClassroomTeacher);

        // Act
        var result = sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddTeacher_ShouldReturnSuccess_WhenDifferentAssignmentTypeEntryAlreadyExists()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        // Act
        var result = sut.AddTeacher("1", AssignmentType.Supervisor);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }


    [Fact]
    public void AddTeacher_ShouldRaiseDomainEvent_WhenNewEntryIsCreated()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.ClearDomainEvents();

        // Act
        var result = sut.AddTeacher("1", AssignmentType.ClassroomTeacher);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Count().Should().Be(1);
        events.First().Should().BeOfType<TeacherAddedToOfferingDomainEvent>();
    }

    [Fact]
    public void AddTeacher_ShouldNotRaiseDomainEvent_WhenSecondEntryIsCreated()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.Supervisor);

        sut.ClearDomainEvents();

        // Act
        var result = sut.AddTeacher("1", AssignmentType.ClassroomTeacher);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Count().Should().Be(0);
    }

    [Fact]
    public void RemoveTeacher_ShouldReturnTrue_WhenMatchingEntryExists()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher("1", AssignmentType.ClassroomTeacher);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemoveTeacher_ShouldDeleteEntry_WhenMatchingEntryExists()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.ClassroomTeacher);
        sut.ClearDomainEvents();

        var assignment = sut.Teachers.FirstOrDefault(entry => entry.StaffId == "1");

        // Act
        var result = sut.RemoveTeacher("1", AssignmentType.ClassroomTeacher);

        // Assert
        result.IsSuccess.Should().BeTrue();
        assignment.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void RemoveTeacher_ShouldRaiseDomainEvent_WhenEntryDeleted()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.ClassroomTeacher);

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher("1", AssignmentType.ClassroomTeacher);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Count().Should().Be(1);
        events.First().Should().BeOfType<TeacherRemovedFromOfferingDomainEvent>();
    }

    [Fact]
    public void RemoveTeacher_ShouldReturnFailure_WhenEntryNotFound()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.Supervisor);

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher("1", AssignmentType.ClassroomTeacher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(OfferingErrors.RemoveTeacher.NotFound);
    }

    [Fact]
    public void RemoveTeacher_ShouldNotRaiseDomainEvent_WhenAnotherEntryExistsForTeacher()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddTeacher("1", AssignmentType.ClassroomTeacher);
        sut.AddTeacher("1", AssignmentType.Supervisor);

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher("1", AssignmentType.ClassroomTeacher);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Count().Should().Be(0);
    }

    [Fact]
    public void AddSession_ShouldReturnSuccess_WhenEntryDoesNotAlreadyExist()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Act
        var result = sut.AddSession(new());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddSession_ShouldReturnFailure_WhenEntryAlreadyExists()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddSession(new());
        sut.ClearDomainEvents();

        // Act
        var result = sut.AddSession(new());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(OfferingErrors.AddSession.AlreadyExists);
    }

    [Fact]
    public void AddSession_ShouldCreateEntry_WhenEntryDoesNotAlreadyExist()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Act
        var result = sut.AddSession(new());

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.Sessions.Should().HaveCount(1);
        sut.Sessions.First().PeriodId.Should().Be(1);
    }

    [Fact]
    public void RemoveSession_ShouldReturnSuccess_WhenEntryIsDeleted()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddSession(new());
        var session = sut.Sessions.First();

        // Act
        var result = sut.RemoveSession(session.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void RemoveSession_ShouldReturnFailure_WhenEntryIsNotFound()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Act
        var result = sut.RemoveSession(new SessionId());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(OfferingErrors.RemoveSession.NotFound);
    }

    [Fact]
    public void AddResource_ShouldReturnSuccess_WhenResourceDoesNotExist()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Act
        var result = sut.AddResource(
            ResourceType.MicrosoftTeam, 
            Guid.NewGuid().ToString(), 
            "Microsoft Team", 
            "https://teams.microsoft.com/");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddResource_ShouldReturnSuccessWithoutRaisingDomainEvent_WhenResourceAlreadyExists()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        var resourceId = Guid.NewGuid().ToString();

        sut.AddResource(
            ResourceType.MicrosoftTeam,
            resourceId,
            "Microsoft Team",
            "https://teams.microsoft.com/");

        sut.ClearDomainEvents();

        // Act
        var result = sut.AddResource(
            ResourceType.MicrosoftTeam,
            resourceId,
            "Microsoft Team",
            "https://teams.microsoft.com/");
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Count().Should().Be(0);
    }

    [Fact]
    public void AddResource_ShouldReturnFailure_WhenResourceTypeIsInvalid()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        // Act
        var result = sut.AddResource(
            ResourceType.FromValue("Blackboard Course"),
            Guid.NewGuid().ToString(),
            "BlackBoard",
            "https://teams.microsoft.com/");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ResourceErrors.InvalidType("Blackboard Course"));
    }

    [Fact]
    public void AddResource_ShouldRaiseDomainEvent_WhenResourceDoesNotExist()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.ClearDomainEvents();

        // Act
        var result = sut.AddResource(
            ResourceType.MicrosoftTeam,
            Guid.NewGuid().ToString(),
            "Microsoft Team",
            "https://teams.microsoft.com/");
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Count().Should().Be(1);
        events.First().Should().BeOfType<ResourceAddedToOfferingDomainEvent>();
    }

    [Fact]
    public void RemoveResource_ShouldReturnSuccess_WhenResourceIsDeleted()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        var resourceId = Guid.NewGuid().ToString();

        sut.AddResource(
            ResourceType.MicrosoftTeam,
            resourceId,
            "Microsoft Team",
            "https://teams.microsoft.com/");

        sut.ClearDomainEvents();

        var resource = sut.Resources.First();

        var result = sut.RemoveResource(resource.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemoveResource_ShouldDeleteEntry_WhenResourceIsFound()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        var resourceId = Guid.NewGuid().ToString();

        sut.AddResource(
            ResourceType.MicrosoftTeam,
            resourceId,
            "Microsoft Team",
            "https://teams.microsoft.com/");

        sut.ClearDomainEvents();

        var resource = sut.Resources.First();

        var result = sut.RemoveResource(resource.Id);

        result.IsSuccess.Should().BeTrue();
        sut.Resources.Count().Should().Be(0);
    }

    [Fact]
    public void RemoveResource_ShouldRaiseDomainEvent_WhenResourceIsDeleted()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        var resourceId = Guid.NewGuid().ToString();

        sut.AddResource(
            ResourceType.MicrosoftTeam,
            resourceId,
            "Microsoft Team",
            "https://teams.microsoft.com/");

        sut.ClearDomainEvents();

        var resource = sut.Resources.First();

        var result = sut.RemoveResource(resource.Id);
        var events = sut.GetDomainEvents();

        result.IsSuccess.Should().BeTrue();
        events.Count().Should().Be(1);
        events.First().Should().BeOfType<ResourceRemovedFromOfferingDomainEvent>();
    }

    [Fact]
    public void RemoveResource_ShouldReturnFailure_WhenResourceIsNotFound()
    {
        var name = OfferingName.FromValue("07SCI1");

        var sut = new Offering(
            name.Value,
            new(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today)
        );

        sut.AddResource(
            ResourceType.MicrosoftTeam,
            Guid.NewGuid().ToString(),
            "Microsoft Team",
            "https://teams.microsoft.com/");

        sut.ClearDomainEvents();

        var resource = sut.Resources.First();
        var newResourceId = new ResourceId();

        var result = sut.RemoveResource(newResourceId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ResourceErrors.NotFound(newResourceId));
    }
}
