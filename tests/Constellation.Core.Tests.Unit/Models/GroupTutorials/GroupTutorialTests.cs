﻿namespace Constellation.Core.Tests.Unit.Models.GroupTutorials;

using Abstractions.Clock;
using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Enums;
using ValueObjects;

public class GroupTutorialTests
{
    private IDateTimeProvider _dateTimeProvider = new DateTimeProvider();

    private School School => new School() { Code = "1234", Name = "Imaginarium Public School" };

    private Student Student => Student.Create(
            StudentReferenceNumber.FromValue("123456789"),
            Name.Create("John", "Johnny", "Doe").Value,
            EmailAddress.Create("john.doe3@education.nsw.gov.au").Value,
            Grade.Y09,
            School,
            Gender.NonBinary,
            _dateTimeProvider)
        .Value;

    private StaffMember Teacher => StaffMember.Create(
            EmployeeId.FromValue("1234567"),
            Name.Create("John", string.Empty, "Tester").Value,
            EmailAddress.Create("john.tester@det.nsw.edu.au").Value,
            School,
            Gender.Male,
            false,
            _dateTimeProvider)
        .Value;

    [Fact]
    public void AddTeacher_ShouldReturnFailure_WhenTutorialHasExpired()
    {
        // Arrange
        var sut = GroupTutorial.Create(
            new GroupTutorialId(),
            "Stage 4 Mathematics",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));

        // Act
        var result = sut.AddTeacher(Teacher);

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

        // Act
        var result = sut.AddTeacher(Teacher);

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

        var initialResult = sut.AddTeacher(Teacher);

        TutorialTeacherId? existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : null;

        // Act
        var result = sut.AddTeacher(Teacher);

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

        var initialResult = sut.AddTeacher(Teacher);

        TutorialTeacherId? existingRecord = initialResult.IsSuccess ? initialResult.Value.Id : null;

        sut.RemoveTeacher(Teacher);

        // Act
        var result = sut.AddTeacher(Teacher);

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

        sut.ClearDomainEvents();

        // Act
        var result = sut.AddTeacher(Teacher, effectiveTo);
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

        // Act
        var result = sut.RemoveTeacher(Teacher);
        var teachers = sut.Teachers.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        teachers.Any(member => member.StaffId == Teacher.Id).Should().BeFalse();
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

        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(Teacher);
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

        var initialResult = sut.AddTeacher(Teacher);
        sut.RemoveTeacher(Teacher);

        // Act
        var result = sut.RemoveTeacher(Teacher);
        var teachers = sut.Teachers.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        teachers.Any(member => member.StaffId == Teacher.Id && !member.IsDeleted).Should().BeFalse();
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

        var initialResult = sut.AddTeacher(Teacher);
        sut.RemoveTeacher(Teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(Teacher);
        var teachers = sut.Teachers.ToList();
        var events = sut.GetDomainEvents();

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(0);
        teachers.Any(member => member.StaffId == Teacher.Id && !member.IsDeleted).Should().BeFalse();
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

        var initialResult = sut.AddTeacher(Teacher);

        // Act
        var result = sut.RemoveTeacher(Teacher);
        var teachers = sut.Teachers.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        teachers.Any(member => member.StaffId == Teacher.Id && !member.IsDeleted).Should().BeFalse();
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
        
        sut.AddTeacher(Teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(Teacher);
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
        
        var takesEffectOn = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        sut.AddTeacher(Teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(Teacher, takesEffectOn);
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

        var takesEffectOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

        sut.AddTeacher(Teacher);
        sut.ClearDomainEvents();

        // Act
        var result = sut.RemoveTeacher(Teacher, takesEffectOn);
        var events = sut.GetDomainEvents();
        var teacherEntry = sut.Teachers.First(member => member.StaffId == Teacher.Id);

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
            _dateTimeProvider.Yesterday,
            _dateTimeProvider.Yesterday);

        var student = Student;

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
            _dateTimeProvider.FirstDayOfYear,
            _dateTimeProvider.LastDayOfYear);

        sut.Delete();

        var student = Student;

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

        var student = Student;

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

        var student = Student;

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

        var student = Student;
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

        Student student = Student;

        // Act
        var result = sut.UnenrolStudent(student);
        var enrolments = sut.Enrolments.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        enrolments.Any(member => member.StudentId == student.Id).Should().BeFalse();
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

        var student = Student;
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

        var student = Student;

        var initialResult = sut.EnrolStudent(student);
        sut.UnenrolStudent(student);

        // Act
        var result = sut.UnenrolStudent(student);
        var enrolments = sut.Enrolments.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        enrolments.Any(member => member.StudentId == student.Id && !member.IsDeleted).Should().BeFalse();
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

        var student = Student;

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
        enrolments.Any(member => member.StudentId == student.Id && !member.IsDeleted).Should().BeFalse();
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

        var student = Student;

        var initialResult = sut.EnrolStudent(student);

        // Act
        var result = sut.UnenrolStudent(student);
        var enrolments = sut.Enrolments.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        enrolments.Any(member => member.StudentId == student.Id && !member.IsDeleted).Should().BeFalse();
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

        var student = Student;

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

        var student = Student;

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

        var student = Student;

        var takesEffectOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

        sut.EnrolStudent(student);
        sut.ClearDomainEvents();

        // Act
        var result = sut.UnenrolStudent(student, takesEffectOn);
        var events = sut.GetDomainEvents();
        var studentEntry = sut.Enrolments.First(member => member.StudentId == student.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<StudentRemovedFromGroupTutorialDomainEvent>();
        studentEntry.EffectiveTo.Should().BeNull();
    }
}
