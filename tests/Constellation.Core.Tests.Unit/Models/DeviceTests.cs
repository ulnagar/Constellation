using Constellation.Core.Enums;
using Constellation.Core.Models;
using FluentAssertions;

namespace Constellation.Core.Tests.Unit.Models;

public class DeviceTests
{

	[Fact]
	public void IsAllocated_ShouldReturnTrue_WhenNotDeletedAllocationExists()
	{
		// Arrange
		var sut = new Device();

		var allocation = new DeviceAllocation
		{
			IsDeleted = false
		};

		sut.Allocations.Add(allocation);

		// Act
		var result = sut.IsAllocated();

		// Assert
		result.Should().BeTrue();
	}

    [Fact]
    public void IsAllocated_ShouldReturnFalse_WhenDeletedAllocationExists()
    {
        // Arrange
        var sut = new Device();

        var allocation = new DeviceAllocation
        {
            IsDeleted = true
        };

        sut.Allocations.Add(allocation);

        // Act
        var result = sut.IsAllocated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAllocated_ShouldReturnFalse_WhenNoAllocationExists()
    {
        // Arrange
        var sut = new Device();

        // Act
        var result = sut.IsAllocated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAllocate_ShouldReturnFalse_WhenAlreadyAllocated()
    {
		// Arrange
		var sut = new Device();

        var allocation = new DeviceAllocation
        {
            IsDeleted = true
        };

        sut.Allocations.Add(allocation);

        // Act
        var isAllocated = sut.IsAllocated();
        var result = sut.CanAllocate();

        // Assert
        isAllocated.Should().BeFalse();
        result.Should().BeTrue();
	}

    [Theory]
    [InlineData(Status.Unknown, false)]
    [InlineData(Status.New, true)]
    [InlineData(Status.Allocated, false)]
    [InlineData(Status.Ready, true)]
    [InlineData(Status.RepairingReturning, false)]
    [InlineData(Status.RepairingChecking, false)]
    [InlineData(Status.RepairingInternal, false)]
    [InlineData(Status.RepairingVendor, false)]
    [InlineData(Status.OnHold, false)]
    [InlineData(Status.WrittenOffWithdrawn, false)]
    [InlineData(Status.WrittenOffReplaced, false)]
    [InlineData(Status.WrittenOffDamaged, false)]
    [InlineData(Status.WrittenOffLost, false)]
    public void CanAllocate_ShouldReturnValue_DependingOnStatus(Status status, bool expected)
    {
        // Arrange
        var sut = new Device();

        sut.Status = status;

        // Act
        var result = sut.CanAllocate();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Status.Unknown, true)]
    [InlineData(Status.New, true)]
    [InlineData(Status.Allocated, true)]
    [InlineData(Status.Ready, true)]
    [InlineData(Status.RepairingReturning, true)]
    [InlineData(Status.RepairingChecking, true)]
    [InlineData(Status.RepairingInternal, true)]
    [InlineData(Status.RepairingVendor, true)]
    [InlineData(Status.OnHold, true)]
    [InlineData(Status.WrittenOffWithdrawn, false)]
    [InlineData(Status.WrittenOffReplaced, false)]
    [InlineData(Status.WrittenOffDamaged, false)]
    [InlineData(Status.WrittenOffLost, false)]
    public void CanUpdateStatus_ShouldReturnValue_DependingOnStatus(Status status, bool expected)
    {
        // Arrange
        var sut = new Device();

        sut.Status = status;

        // Act
        var result = sut.CanUpdateStatus();

        // Assert
        result.Should().Be(expected);
    }
}
