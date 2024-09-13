namespace Constellation.Core.Tests.Unit.Models;

using Constellation.Core.Models;

public class SchoolTests
{
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
