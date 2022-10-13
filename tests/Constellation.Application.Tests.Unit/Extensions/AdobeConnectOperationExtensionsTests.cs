using Constellation.Application.Extensions;
using Constellation.Core.Models;

namespace Constellation.Application.Tests.Unit.Extensions;

public class AdobeConnectOperationExtensionsTests
{
    [Fact]
    public void IsOutstanding_ShouldReturnFalse_IfDateScheduledIsInFuture()
    {
        // Arrange
        var sut = new StudentAdobeConnectOperation
        {
            IsDeleted = false,
            IsCompleted = false,
            DateScheduled = DateTime.Today.AddDays(4)
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeFalse();
	}

    [Fact]
    public void IsOutstanding_ShouldReturnTrue_IfDateScheduledIsToday()
    {
        // Arrange
        var sut = new StudentAdobeConnectOperation
        {
            IsDeleted = false,
            IsCompleted = false,
            DateScheduled = DateTime.Today
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOutstanding_ShouldReturnFalse_IfMarkedDeleted()
    {
        // Arrange
        var sut = new StudentAdobeConnectOperation
        {
            IsDeleted = true,
            IsCompleted = false,
            DateScheduled = DateTime.Today
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOutstanding_ShouldReturnFalse_IfMarkedCompleted()
    {
        // Arrange
        var sut = new StudentAdobeConnectOperation
        {
            IsDeleted = false,
            IsCompleted = true,
            DateScheduled = DateTime.Today
        };

        // Act
        var result = sut.IsOutstanding();

        // Assert
        result.Should().BeFalse();
    }
}
