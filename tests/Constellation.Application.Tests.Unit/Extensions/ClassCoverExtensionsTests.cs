using Constellation.Application.Extensions;
using Constellation.Core.Models;
using Constellation.Core.Models.Covers;
using Constellation.Core.ValueObjects;

namespace Constellation.Application.Tests.Unit.Extensions;

public class ClassCoverExtensionsTests
{
    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfMarkedDeleted()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.MinValue,
            DateOnly.MaxValue,
            CoverTeacherType.Staff,
            "1");

        sut.Delete();

		// Act
		var result = sut.IsCurrent();

		// Assert
		result.Should().BeFalse();
	}

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfEndDateIsBeforeToday()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.MinValue,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfEndDateIsToday()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.MinValue,
            DateOnly.FromDateTime(DateTime.Today),
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfEndDateIsInTheFuture()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.MinValue,
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnFalse_IfStartDateIsInTheFuture()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsToday()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.MaxValue,
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_IfStartDateIsInThePast()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.MinValue,
            DateOnly.MaxValue,
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsCurrent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFuture_ShouldReturnTrue_IfStartDateIsInTheFutureAndNotMarkedDeleted()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            DateOnly.MaxValue,
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFuture_ShouldReturnFalse_IfMarkedDeleted()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.MinValue,
            DateOnly.MaxValue,
            CoverTeacherType.Staff,
            "1");

        sut.Delete();

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFuture_ShouldReturnFalse_IfStartDateIsInThePast()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.MinValue,
            DateOnly.MaxValue,
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFuture_ShouldReturnFalse_IfStartDateIsToday()
    {
        // Arrange
        var sut = ClassCover.Create(
            Guid.NewGuid(),
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.MaxValue,
            CoverTeacherType.Staff,
            "1");

        // Act
        var result = sut.IsFuture();

        // Assert
        result.Should().BeFalse();
    }
}
