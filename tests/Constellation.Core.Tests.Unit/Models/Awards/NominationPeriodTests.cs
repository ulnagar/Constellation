namespace Constellation.Core.Tests.Unit.Models.Awards;

using Core.Models.Awards;
using Core.Models.Awards.Errors;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using Enums;
using Shared;

public class NominationPeriodTests
{
    [Fact]
    public void Create_ShouldReturnFailure_WhenLockoutDateIsInThePast()
    {
        // Arrange
        DateOnly lockoutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        // Act
        Result<NominationPeriod> result = NominationPeriod.Create("Test Period", [Grade.Y10], lockoutDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(AwardNominationPeriodErrors.PastDate);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenLockoutDateIsToday()
    {
        // Arrange
        DateOnly lockoutDate = DateOnly.FromDateTime(DateTime.Today);

        // Act
        Result<NominationPeriod> result = NominationPeriod.Create("Test Period", [Grade.Y10], lockoutDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenLockoutDateIsInTheFuture()
    {
        // Arrange
        DateOnly lockoutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        // Act
        Result<NominationPeriod> result = NominationPeriod.Create("Test Period", [Grade.Y10], lockoutDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldPopulateIncludedGrades_WhenCreatedWithMultipleGrades()
    {
        // Arrange
        DateOnly lockoutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        List<Grade> grades = new List<Grade> { Grade.Y10, Grade.Y11 };

        // Act
        Result<NominationPeriod> result = NominationPeriod.Create("Test Period", grades, lockoutDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IncludedGrades.Select(g => g.Grade).Should().BeEquivalentTo(grades);
    }

    [Fact]
    public void AddNomination_ShouldReturnSuccess_WhenNominationDoesNotAlreadyExist()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        Nomination nomination = FirstInSubjectNomination.Create(
            sut.Id, new StudentId(), new CourseId(), Grade.Y10, "Mathematics").Value;

        // Act
        Result result = sut.AddNomination(nomination);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddNomination_ShouldAddNominationToCollection_WhenNominationDoesNotAlreadyExist()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        Nomination nomination = FirstInSubjectNomination.Create(
            sut.Id, new StudentId(), new CourseId(), Grade.Y10, "Mathematics").Value;

        // Act
        sut.AddNomination(nomination);

        // Assert
        sut.Nominations.Should().HaveCount(1);
        sut.Nominations.First().Should().Be(nomination);
    }

    [Fact]
    public void AddNomination_ShouldReturnFailure_WhenNonDeletedDuplicateFirstInSubjectNominationExists()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();
        CourseId courseId = new CourseId();

        Nomination firstNomination = FirstInSubjectNomination.Create(
            sut.Id, studentId, courseId, Grade.Y10, "Mathematics").Value;
        sut.AddNomination(firstNomination);

        Nomination duplicateNomination = FirstInSubjectNomination.Create(
            sut.Id, studentId, courseId, Grade.Y10, "Mathematics").Value;

        // Act
        Result result = sut.AddNomination(duplicateNomination);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(AwardNominationErrors.DuplicateFound);
    }

    [Fact]
    public void AddNomination_ShouldReturnSuccess_WhenDeletedDuplicateFirstInSubjectNominationExists()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();
        CourseId courseId = new CourseId();

        Nomination firstNomination = FirstInSubjectNomination.Create(
            sut.Id, studentId, courseId, Grade.Y10, "Mathematics").Value;
        sut.AddNomination(firstNomination);
        firstNomination.Delete();

        Nomination replacementNomination = FirstInSubjectNomination.Create(
            sut.Id, studentId, courseId, Grade.Y10, "Mathematics").Value;

        // Act
        Result result = sut.AddNomination(replacementNomination);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddNomination_ShouldReturnSuccess_WhenFirstInSubjectNominationExistsForDifferentStudent()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        CourseId courseId = new CourseId();

        Nomination firstNomination = FirstInSubjectNomination.Create(
            sut.Id, new StudentId(), courseId, Grade.Y10, "Mathematics").Value;
        sut.AddNomination(firstNomination);

        Nomination secondNomination = FirstInSubjectNomination.Create(
            sut.Id, new StudentId(), courseId, Grade.Y10, "Mathematics").Value;

        // Act
        Result result = sut.AddNomination(secondNomination);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddNomination_ShouldReturnFailure_WhenNonDeletedDuplicateAcademicExcellenceNominationExists()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();
        CourseId courseId = new CourseId();
        OfferingId offeringId = new OfferingId();

        Nomination firstNomination = AcademicExcellenceNomination.Create(
            sut.Id, studentId, courseId, "Mathematics", Grade.Y10, offeringId, "10MAAT1").Value;
        sut.AddNomination(firstNomination);

        Nomination duplicateNomination = AcademicExcellenceNomination.Create(
            sut.Id, studentId, courseId, "Mathematics", Grade.Y10, offeringId, "10MAAT1").Value;

        // Act
        Result result = sut.AddNomination(duplicateNomination);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(AwardNominationErrors.DuplicateFound);
    }

    [Fact]
    public void AddNomination_ShouldReturnSuccess_WhenAcademicExcellenceNominationExistsForDifferentOffering()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();
        CourseId courseId = new CourseId();

        Nomination firstNomination = AcademicExcellenceNomination.Create(
            sut.Id, studentId, courseId, "Mathematics", Grade.Y10, new OfferingId(), "10MAAT1").Value;
        sut.AddNomination(firstNomination);

        Nomination secondNomination = AcademicExcellenceNomination.Create(
            sut.Id, studentId, courseId, "Mathematics", Grade.Y10, new OfferingId(), "10MAAT2").Value;

        // Act
        Result result = sut.AddNomination(secondNomination);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddNomination_ShouldReturnFailure_WhenNonDeletedDuplicateAcademicAchievementNominationExists()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();
        CourseId courseId = new CourseId();
        OfferingId offeringId = new OfferingId();

        Nomination firstNomination = AcademicAchievementNomination.Create(
            sut.Id, studentId, courseId, "Mathematics", Grade.Y10, offeringId, "10MAAT1").Value;
        sut.AddNomination(firstNomination);

        Nomination duplicateNomination = AcademicAchievementNomination.Create(
            sut.Id, studentId, courseId, "Mathematics", Grade.Y10, offeringId, "10MAAT1").Value;

        // Act
        Result result = sut.AddNomination(duplicateNomination);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(AwardNominationErrors.DuplicateFound);
    }

    [Fact]
    public void AddNomination_ShouldReturnSuccess_WhenPrincipalsAwardNominationAlreadyExistsForSameStudent()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();

        Nomination firstNomination = PrincipalsAwardNomination.Create(sut.Id, studentId, Grade.Y10).Value;
        sut.AddNomination(firstNomination);

        Nomination secondNomination = PrincipalsAwardNomination.Create(sut.Id, studentId, Grade.Y10).Value;

        // Act
        Result result = sut.AddNomination(secondNomination);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddNomination_ShouldReturnSuccess_WhenGalaxyMedalNominationAlreadyExistsForSameStudent()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();

        Nomination firstNomination = GalaxyMedalNomination.Create(sut.Id, studentId, Grade.Y10).Value;
        sut.AddNomination(firstNomination);

        Nomination secondNomination = GalaxyMedalNomination.Create(sut.Id, studentId, Grade.Y10).Value;

        // Act
        Result result = sut.AddNomination(secondNomination);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddNomination_ShouldReturnSuccess_WhenUniversalAchieverNominationAlreadyExistsForSameStudent()
    {
        // Arrange
        NominationPeriod sut = NominationPeriod.Create(
            "Test Period",
            [Grade.Y10],
            DateOnly.FromDateTime(DateTime.Today.AddDays(30))).Value;

        StudentId studentId = new StudentId();

        Nomination firstNomination = UniversalAchieverNomination.Create(sut.Id, studentId, Grade.Y10).Value;
        sut.AddNomination(firstNomination);

        Nomination secondNomination = UniversalAchieverNomination.Create(sut.Id, studentId, Grade.Y10).Value;

        // Act
        Result result = sut.AddNomination(secondNomination);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}