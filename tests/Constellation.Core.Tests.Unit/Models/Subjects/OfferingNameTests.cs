namespace Constellation.Core.Tests.Unit.Models.Subjects;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Grade = Enums.Grade.Y07,
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            Enums.Grade.Y07,
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
            Grade = Enums.Grade.Y07,
            Name = "Tutorials"
        };

        // Act
        var sut = OfferingName.Create(
            Enums.Grade.Y07,
            course,
            "MG");

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenClassGradeIsInvalid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Code = "ENG",
            Grade = Enums.Grade.Y07,
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            0,
            course,
            "P",
            '1');

        // Assert
        sut.Should().BeOfType<Result<OfferingName>>();
        sut.IsSuccess.Should().BeFalse();
        sut.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTutorialGradeIsInvalid()
    {
        // Arrange
        var course = new Course()
        {
            Id = 1,
            Code = "TUT",
            Grade = Enums.Grade.Y07,
            Name = "Tutorial"
        };

        // Act
        var sut = OfferingName.Create(
            0,
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
            Grade = Enums.Grade.Y07,
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            Enums.Grade.Y07,
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
            Grade = Enums.Grade.Y07,
            Name = "English"
        };

        // Act
        var sut = OfferingName.Create(
            Enums.Grade.Y07,
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
            Code = code
        };

        var sut = OfferingName.Create(
            grade,
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
            Code = code
        };

        var sut = OfferingName.Create(
            grade,
            course,
            initials);

        // Act
        var result = sut.Value.ToString();

        // Assert
        result.Should().Be(expected);
    }
}