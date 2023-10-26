namespace Constellation.Core.Tests.Unit.Models.GroupTutorials;

using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students;

public class GroupTutorialTests
{
    [Fact]
    public void AddTeacher_ShouldReturnFailure_WhenTutorialHasExpired()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.GroupTutorials.GroupTutorial.TutorialHasExpired.Code);
    }

    [Fact]
    public void AddTeacher_ShouldReturnFailure_WhenTutorialHasBeenDeleted()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        sut.Delete();

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.GroupTutorials.GroupTutorial.TutorialHasExpired.Code);
    }

    [Fact]
    public void AddTeacher_ShouldReturnSuccessWithSameId_WhenTeacherIsAlreadyAdded()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var initialResult = sut.AddTeacher(teacher);

        TutorialTeacherId? existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : null;

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(existingRecord);
    }

    [Fact]
    public void AddTeacher_ShouldReturnSuccessWithNewId_WhenTeacherWasPreviouslyRemoved()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var initialResult = sut.AddTeacher(teacher);

        TutorialTeacherId? existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : null;

        sut.RemoveTeacher(teacher);

        // Act
        var result = sut.AddTeacher(teacher);

        // Assert
        existingRecord.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(existingRecord);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("2022-12-01")]
    public void AddTeacher_ShouldRaiseDomainEvent_WhenTeacherSucessfullyAdded(string effectiveToDate)
    {
        // Arrange
        DateOnly? effectiveTo = string.IsNullOrWhiteSpace(effectiveToDate) ? null : DateOnly.Parse(effectiveToDate);

        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        sut.ClearDomainEvents();

        // Act
        var result = sut.AddTeacher(teacher, effectiveTo);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<TeacherAddedToGroupTutorialDomainEvent>();
    }

    [Fact]
    public void RemoveTeacher_ShouldReturnSuccess_WhenTeacherIsNotAdded()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        // Act
        var result = sut.RemoveTeacher(teacher);
        var teachers = sut.Teachers.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        teachers.Any(member => member.StaffId == teacher.StaffId).Should().BeFalse();
    }

    [Fact]
    public void RemoveTeacher_ShouldNotRaiseDomainEvent_WhenTeacherIsNotAdded()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(teacher);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(0);
    }

    [Fact]
    public void RemoveTeacher_ShouldReturnSuccess_WhenTeacherHasAlreadyBeenRemoved()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var initialResult = sut.AddTeacher(teacher);
        sut.RemoveTeacher(teacher);

        // Act
        var result = sut.RemoveTeacher(teacher);
        var teachers = sut.Teachers.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        teachers.Any(member => member.StaffId == teacher.StaffId && !member.IsDeleted).Should().BeFalse();
    }

    [Fact]
    public void RemoveTeacher_ShouldNotRaiseDomainEvent_WhenTeacherHasAlreadyBeenRemoved()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var initialResult = sut.AddTeacher(teacher);
        sut.RemoveTeacher(teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(teacher);
        var teachers = sut.Teachers.ToList();
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(0);
        teachers.Any(member => member.StaffId == teacher.StaffId && !member.IsDeleted).Should().BeFalse();
    }

    [Fact]
    public void RemoveTeacher_ShouldReturnSuccess_WhenTeacherIsRemoved()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var initialResult = sut.AddTeacher(teacher);

        // Act
        var result = sut.RemoveTeacher(teacher);
        var teachers = sut.Teachers.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        teachers.Any(member => member.StaffId == teacher.StaffId && !member.IsDeleted).Should().BeFalse();
    }

    [Fact]
    public void RemoveTeacher_ShouldRaiseDomainEvent_WhenTeacherIsRemoved()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        sut.AddTeacher(teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(teacher);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<TeacherRemovedFromGroupTutorialDomainEvent>();
    }

    [Fact]
    public void RemoveTeacher_ShouldNotRaiseDomainEvent_WhenTakesEffectOnIsSpecified()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var takesEffectOn = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        sut.AddTeacher(teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(teacher, takesEffectOn);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(0);
    }

    [Fact]
    public void RemoveTeacher_ShouldTakeEffectToday_WhenTakesEffectOnIsInThePast()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var teacher = new Staff
        {
            StaffId = "123456789"
        };

        var takesEffectOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

        sut.AddTeacher(teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(teacher, takesEffectOn);
        var events = sut.GetDomainEvents();
        var teacherEntry = sut.Teachers.First(member => member.StaffId == teacher.StaffId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<TeacherRemovedFromGroupTutorialDomainEvent>();
        teacherEntry.EffectiveTo.Should().BeNull();
    }

    [Fact]
    public void EnrolStudent_ShouldReturnFailure_WhenTutorialHasExpired()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        // Act
        var result = sut.EnrolStudent(student);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.GroupTutorials.GroupTutorial.TutorialHasExpired.Code);
    }

    [Fact]
    public void EnrolStudent_ShouldReturnFailure_WhenTutorialHasBeenDeleted()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        sut.Delete();

        var student = new Student
        {
            StudentId = "123456789"
        };

        // Act
        var result = sut.EnrolStudent(student);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.GroupTutorials.GroupTutorial.TutorialHasExpired.Code);
    }

    [Fact]
    public void EnrolStudent_ShouldReturnSuccessWithSameId_WhenStudentIsAlreadyEnrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        var initialResult = sut.EnrolStudent(student);

        TutorialEnrolmentId? existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : null;

        // Act
        var result = sut.EnrolStudent(student);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(existingRecord);
    }

    [Fact]
    public void EnrolStudent_ShouldReturnSuccessWithNewId_WhenStudentWasPreviouslyEnrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        var initialResult = sut.EnrolStudent(student);

        TutorialEnrolmentId? existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : null;

        sut.UnenrolStudent(student);

        // Act
        var result = sut.EnrolStudent(student);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(existingRecord);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("2022-12-01")]
    public void EnrolStudent_ShouldRaiseDomainEvent_WhenStudentSucessfullyEnrolled(string effectiveToDate)
    {
        // Arrange
        DateOnly? effectiveTo = string.IsNullOrWhiteSpace(effectiveToDate) ? null : DateOnly.Parse(effectiveToDate);

        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        sut.ClearDomainEvents();

        // Act
        var result = sut.EnrolStudent(student, effectiveTo);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentAddedToGroupTutorialDomainEvent>();
    }

    [Fact]
    public void UnenrolStudent_ShouldReturnSuccess_WhenStudentIsNotEnrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        // Act
        var result = sut.UnenrolStudent(student);
        var enrolments = sut.Enrolments.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        enrolments.Any(member => member.StudentId == student.StudentId).Should().BeFalse();
    }

    [Fact]
    public void UnenrolStudent_ShouldNotRaiseDomainEvent_WhenStudentIsNotEnrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        sut.ClearDomainEvents();

        // Act
        var result = sut.UnenrolStudent(student);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(0);
    }

    [Fact]
    public void UnenrolStudent_ShouldReturnSuccess_WhenStudentHasAlreadyBeenUnenrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        var initialResult = sut.EnrolStudent(student);
        sut.UnenrolStudent(student);

        // Act
        var result = sut.UnenrolStudent(student);
        var enrolments = sut.Enrolments.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        enrolments.Any(member => member.StudentId == student.StudentId && !member.IsDeleted).Should().BeFalse();
    }

    [Fact]
    public void UnenrolStudent_ShouldNotRaiseDomainEvent_WhenStudentHasAlreadyBeenUnenrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        var initialResult = sut.EnrolStudent(student);
        sut.UnenrolStudent(student);
        sut.ClearDomainEvents();

        // Act
        var result = sut.UnenrolStudent(student);
        var enrolments = sut.Enrolments.ToList();
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(0);
        enrolments.Any(member => member.StudentId == student.StudentId && !member.IsDeleted).Should().BeFalse();
    }

    [Fact]
    public void UnenrolStudent_ShouldReturnSuccess_WhenStudentIsUnenrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        var initialResult = sut.EnrolStudent(student);

        // Act
        var result = sut.UnenrolStudent(student);
        var enrolments = sut.Enrolments.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        enrolments.Any(member => member.StudentId == student.StudentId && !member.IsDeleted).Should().BeFalse();
    }

    [Fact]
    public void UnenrolStudent_ShouldRaiseDomainEvent_WhenStudentIsUnenrolled()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        sut.EnrolStudent(student);
        sut.ClearDomainEvents();

        // Act
        var result = sut.UnenrolStudent(student);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentRemovedFromGroupTutorialDomainEvent>();
    }

    [Fact]
    public void UnenrolStudent_ShouldNotRaiseDomainEvent_WhenTakesEffectOnIsSpecified()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        var takesEffectOn = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        sut.EnrolStudent(student);
        sut.ClearDomainEvents();

        // Act
        var result = sut.UnenrolStudent(student, takesEffectOn);
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(0);
    }

    [Fact]
    public void UnenrolStudent_ShouldTakeEffectToday_WhenTakesEffectOnIsInThePast()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        var student = new Student
        {
            StudentId = "123456789"
        };

        var takesEffectOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

        sut.EnrolStudent(student);
        sut.ClearDomainEvents();

        // Act
        var result = sut.UnenrolStudent(student, takesEffectOn);
        var events = sut.GetDomainEvents();
        var studentEntry = sut.Enrolments.First(member => member.StudentId == student.StudentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentRemovedFromGroupTutorialDomainEvent>();
        studentEntry.EffectiveTo.Should().BeNull();
    }
}
