namespace Constellation.Core.Tests.Unit.Models;

using Abstractions.Clock;
using Constellation.Core.Models;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Enums;
using ValueObjects;

public class SchoolTests
{
    private IDateTimeProvider _dateTimeProvider = new DateTimeProvider();

    private School School => new School() { Code = "1234", Name = "Imaginarium Public School" };

    private Student Student => Student.Create(
        StudentReferenceNumber.FromValue("123456789"),
        Name.Create("John", "Johnny", "Doe").Value, 
        EmailAddress.Create("john.doe3@education.nsw.gov.au").Value,
        Grade.Y09,
        School,
        2024,
        Gender.NonBinary, 
        _dateTimeProvider)
        .Value;

    [Fact]
    public void HasStudents_ShouldReturnFalse_WhenNoStudentsExist()
    {
        // Arrange
        var sut = new School();

        // Act
        var result = sut.HasStudents;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasStudents_ShouldReturnFalse_WhenNoCurrentStudentsExist()
    {
		// Arrange
		var sut = new School();

        var student = Student;
        
		student.Withdraw(_dateTimeProvider);

		sut.Students.Add(student);

		// Act
		var result = sut.HasStudents;

		// Assert
		result.Should().BeFalse();
	}

    [Fact]
    public void HasStudents_ShouldReturnTrue_WhenCurrentStudentsExist()
    {
        // Arrange
        var sut = new School();

        var student = Student;

        sut.Students.Add(student);

        // Act
        var result = sut.HasStudents;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasStaff_ShouldReturnFalse_WhenNoStaffExist()
    {
        // Arrange
        var sut = new School();

        // Act
        var result = sut.HasStaff;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasStaff_ShouldReturnFalse_WhenNoCurrentStaffExist()
    {
        // Arrange
        var sut = new School();

        var staff = new Staff();
        staff.IsDeleted = true;

        sut.Staff.Add(staff);

        // Act
        var result = sut.HasStaff;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasStaff_ShouldReturnTrue_WhenCurrentStaffExist()
    {
        // Arrange
        var sut = new School();

        var staff = new Staff();
        staff.IsDeleted = false;

        sut.Staff.Add(staff);

        // Act
        var result = sut.HasStaff;

        // Assert
        result.Should().BeTrue();
    }
}
