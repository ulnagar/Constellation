using Constellation.Application.Extensions;
using Constellation.Core.Models;

namespace Constellation.Application.Tests.Unit.Extensions;

public class ClassCoverExtensionsTests
{
    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfMarkedDeleted()
    {
		// Arrange
		var sut = new TeacherClassCover
		{
			IsDeleted = true
		};

		// Act
		var result = sut.IsCurrent();

		// Assert
		result.Should().BeFalse();
	}

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfEndDateIsBeforeToday()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            EndDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfEndDateIsToday()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            EndDate = DateTime.Today
        };

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfEndDateIsInTheFuture()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            EndDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfStartDateIsInTheFuture()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            EndDate = DateTime.Today.AddDays(2),
            StartDate = DateTime.Today.AddDays(1)
        };

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsToday()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            EndDate = DateTime.Today.AddDays(2),
            StartDate = DateTime.Today
        };

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsInThePast()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            EndDate = DateTime.Today.AddDays(2),
            StartDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFuture_ShouldReturnTrue_IfStartDateIsInTheFutureAndNotMarkedDeleted()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            StartDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFuture_ShouldReturnFalse_IfMarkedDeleted()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = true,
            StartDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFuture_ShouldReturnFalse_IfStartDateIsInThePast()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            StartDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFuture_ShouldReturnFalse_IfStartDateIsToday()
    {
        // Arrange
        var sut = new TeacherClassCover
        {
            IsDeleted = false,
            StartDate = DateTime.Today
        };

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeFalse();
    }
}
