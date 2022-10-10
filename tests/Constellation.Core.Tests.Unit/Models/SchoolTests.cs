using Constellation.Core.Models;
using FluentAssertions;

namespace Constellation.Core.Tests.Unit.Models;

public class SchoolTests
{
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

		var student = new Student();
		student.IsDeleted = true;

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

        var student = new Student();
        student.IsDeleted = false;

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
