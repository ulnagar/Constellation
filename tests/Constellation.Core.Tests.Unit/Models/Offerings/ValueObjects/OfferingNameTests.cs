namespace Constellation.Core.Tests.Unit.Models.Offerings.ValueObjects;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;

public class OfferingNameTests
{

    [Fact]
    public void Create_ShouldReturnSuccess_WhenClassValuesAreValid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Code = "ENG",
            Grade = Grade.Y07,
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            course,
            "P",
            '1');

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenTutorialValuesAreValid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Code = "TUT",
            Grade = Grade.Y07,
            Name = "Tutorials"
        };

        // Act
        var sut = OfferingName.Create(
            course,
            "MG");

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenClassCourseGradeIsInvalid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Code = "ENG",
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            course,
            "P",
            '1');

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeFalse();
        sut.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTutorialCourseGradeIsInvalid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Code = "TUT",
            Name = "Tutorial"
        };

        // Act
        var sut = OfferingName.Create(
            course,
            "MG");

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeFalse();
        sut.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenClassCourseCodeIsInvalid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Grade = Grade.Y07,
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            course,
            "P",
            '1');

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeFalse();
        sut.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTutorialCourseCodeIsInvalid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Grade = Grade.Y07,
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            course,
            "MG");

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeFalse();
        sut.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData("ENG", Grade.Y07, "P", '1', "07ENGP1")]
    [InlineData("eng", Grade.Y07, "P", '1', "07ENGP1")]
    [InlineData("MAT", Grade.Y08, "G", '3', "08MATG3")]
    [InlineData("PHY", Grade.Y11, "2", '1', "11PHY21")]
    [InlineData("YDM", Grade.Y06, "A", '1', "06YDMA1")]
    [InlineData("ENG", Grade.Y07, "P", 'A', "07ENGPA")]
    [InlineData("STE", Grade.Y05, "B", '3', "05STEB3")]
    public void ToString_ShouldReturnValidName_WhenClassValuesAreValid(string code, Grade grade, string line, char sequence, string expected)
    {
        // Arrange
        var course = new Course()
        {
            Grade = grade,
            Code = code
        };

        var sut = OfferingName.Create(
            course,
            line,
            sequence);

        // Act
        var result = sut.Value.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("TUT", Grade.Y07, "MG", "07TUTMG")]
    [InlineData("TUT", Grade.Y08, "KR", "08TUTKR")]
    [InlineData("TUT", Grade.Y09, "NE", "09TUTNE")]
    [InlineData("TUT", Grade.Y10, "LS", "10TUTLS")]
    [InlineData("TUT", Grade.Y11, "PU", "11TUTPU")]
    [InlineData("TUT", Grade.Y12, "MU", "12TUTMU")]
    [InlineData("TUT", Grade.Y05, "LW", "05TUTLW")]
    [InlineData("TUT", Grade.Y06, "KP", "06TUTKP")]
    public void ToString_ShouldReturnValidName_WhenTutorialValuesAreValid(string code, Grade grade, string initials, string expected)
    {
        // Arrange
        var course = new Course()
        {
            Grade = grade,
            Code = code
        };

        var sut = OfferingName.Create(
            course,
            initials);

        // Act
        var result = sut.Value.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("07ENGP1", "07", "ENG", "P", '1')]
    [InlineData("08MATG4", "08", "MAT", "G", '4')]
    [InlineData("09SCIPA", "09", "SCI", "P", 'A')]
    [InlineData("10ENGG1", "10", "ENG", "G", '1')]
    [InlineData("11MXX41", "11", "MXX", "4", '1')]
    [InlineData("12SCX23", "12", "SCX", "2", '3')]
    [InlineData("05STEA1", "05", "STE", "A", '1')]
    [InlineData("06YDMC1", "06", "YDM", "C", '1')]
    public void FromValue_ShouldReturnValidObject_WhenClassNameIsValid(string className, string grade, string course, string line, char sequence)
    {
        // Arrange


        // Act
        var sut = OfferingName.FromValue(className);

        // Assert
        sut.Should().NotBeNull();
        sut.Grade.Should().Be(grade);
        sut.Course.Should().Be(course);
        sut.Line.Should().Be(line);
        sut.Sequence.Should().Be(sequence);
    }

    [Theory]
    [InlineData("07TUTMG", "07", "TUT", "MG")]
    [InlineData("08TUTKR", "08", "TUT", "KR")]
    [InlineData("09TUTNE", "09", "TUT", "NE")]
    [InlineData("10TUTLS", "10", "TUT", "LS")]
    [InlineData("11TUTPU", "11", "TUT", "PU")]
    [InlineData("12TUTMU", "12", "TUT", "MU")]
    [InlineData("05TUTLW", "05", "TUT", "LW")]
    [InlineData("06TUTKP", "06", "TUT", "KP")]
    public void FromValue_ShouldReturnValidObject_WhenTutorialNameIsValid(string className, string grade, string course, string initials)
    {
        // Arrange


        // Act
        var sut = OfferingName.FromValue(className);

        // Assert
        sut.Should().NotBeNull();
        sut.Grade.Should().Be(grade);
        sut.Course.Should().Be(course);
        sut.Initials.Should().Be(initials);
    }

    [Fact]
    public void FromValue_ShouldReturnValidObject_WhenLegacyNameIsValid(string name)
    {
        // Arrange


        // Act
        var sut = OfferingName.FromValue("07ENG1");

        // Assert
        sut.Should().NotBeNull();
        sut.Grade.Should().Be("07");
        sut.Course.Should().Be("ENG");
        sut.Sequence.Should().Be('1');
    }
}