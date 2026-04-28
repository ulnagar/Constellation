namespace Constellation.Core.Tests.Unit.Models.Awards;

using Core.Models.Awards;
using Core.Models.Awards.Identifiers;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using Enums;
using Extensions;

public class NominationTests
{
    [Fact]
    public void Delete_ShouldMarkNominationAsDeleted()
    {
        // Arrange
        Nomination nomination = FirstInSubjectNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), Grade.Y10, "Mathematics").Value;

        // Act
        nomination.Delete();

        // Assert
        nomination.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void FirstInSubjectNomination_GetDescription_ShouldIncludeGradeAndCourseName_WhenBothFlagsAreTrue()
    {
        // Arrange
        FirstInSubjectNomination sut = (FirstInSubjectNomination)FirstInSubjectNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), Grade.Y10, "Mathematics").Value;

        // Act
        string result = sut.GetDescription(showGrade: true, showClass: true);

        // Assert
        result.Should().Be($"First in Course {Grade.Y10.AsName()} Mathematics");
    }

    [Fact]
    public void FirstInSubjectNomination_GetDescription_ShouldExcludeGrade_WhenShowGradeIsFalse()
    {
        // Arrange
        FirstInSubjectNomination sut = (FirstInSubjectNomination)FirstInSubjectNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), Grade.Y10, "Mathematics").Value;

        // Act
        string result = sut.GetDescription(showGrade: false, showClass: true);

        // Assert
        result.Should().Be("First in Course Mathematics");
    }

    [Fact]
    public void FirstInSubjectNomination_GetDescription_ShouldExcludeCourseName_WhenShowClassIsFalse()
    {
        // Arrange
        FirstInSubjectNomination sut = (FirstInSubjectNomination)FirstInSubjectNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), Grade.Y10, "Mathematics").Value;

        // Act
        string result = sut.GetDescription(showGrade: true, showClass: false);

        // Assert
        result.Should().Be($"First in Course {Grade.Y10.AsName()}");
    }

    [Fact]
    public void FirstInSubjectNomination_GetDescription_ShouldReturnBaseLabel_WhenBothFlagsAreFalse()
    {
        // Arrange
        FirstInSubjectNomination sut = (FirstInSubjectNomination)FirstInSubjectNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), Grade.Y10, "Mathematics").Value;

        // Act
        string result = sut.GetDescription(showGrade: false, showClass: false);

        // Assert
        result.Should().Be("First in Course");
    }

    [Fact]
    public void AcademicExcellenceNomination_GetDescription_ShouldIncludeClassName_WhenShowClassIsTrue()
    {
        // Arrange
        AcademicExcellenceNomination sut = (AcademicExcellenceNomination)AcademicExcellenceNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), "Mathematics", Grade.Y10, new OfferingId(), "10MAAT1").Value;

        // Act
        string result = sut.GetDescription(showClass: true);

        // Assert
        result.Should().Be("Academic Excellence Mathematics - 10MAAT1");
    }

    [Fact]
    public void AcademicExcellenceNomination_GetDescription_ShouldExcludeClassName_WhenShowClassIsFalse()
    {
        // Arrange
        AcademicExcellenceNomination sut = (AcademicExcellenceNomination)AcademicExcellenceNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), "Mathematics", Grade.Y10, new OfferingId(), "10MAAT1").Value;

        // Act
        string result = sut.GetDescription(showClass: false);

        // Assert
        result.Should().Be("Academic Excellence Mathematics");
    }

    [Fact]
    public void AcademicAchievementNomination_GetDescription_ShouldIncludeClassName_WhenShowClassIsTrue()
    {
        // Arrange
        AcademicAchievementNomination sut = (AcademicAchievementNomination)AcademicAchievementNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), new CourseId(), "Mathematics", Grade.Y10, new OfferingId(), "10MAAT1").Value;

        // Act
        string result = sut.GetDescription(showClass: true);

        // Assert
        result.Should().Be("Academic Achievement Mathematics - 10MAAT1");
    }

    [Fact]
    public void PrincipalsAwardNomination_GetDescription_ShouldReturnCorrectLabel()
    {
        // Arrange
        PrincipalsAwardNomination sut = (PrincipalsAwardNomination)PrincipalsAwardNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), Grade.Y10).Value;

        // Act
        string result = sut.GetDescription();

        // Assert
        result.Should().Be("Principals Award");
    }

    [Fact]
    public void GalaxyMedalNomination_GetDescription_ShouldReturnCorrectLabel()
    {
        // Arrange
        GalaxyMedalNomination sut = (GalaxyMedalNomination)GalaxyMedalNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), Grade.Y10).Value;

        // Act
        string result = sut.GetDescription();

        // Assert
        result.Should().Be("Galaxy Medal");
    }

    [Fact]
    public void UniversalAchieverNomination_GetDescription_ShouldReturnCorrectLabel()
    {
        // Arrange
        UniversalAchieverNomination sut = (UniversalAchieverNomination)UniversalAchieverNomination.Create(
            new AwardNominationPeriodId(), new StudentId(), Grade.Y10).Value;

        // Act
        string result = sut.GetDescription();

        // Assert
        result.Should().Be("Universal Achiever Award");
    }
}