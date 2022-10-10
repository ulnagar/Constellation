using Constellation.Core.Models;
using FluentAssertions;

namespace Constellation.Core.Tests.Unit.Models;

public class AbsenceTests
{

    [Fact]
    public void Explained_ShouldReturnTrue_WhenParentResponseExists()
    {
		// Arrange
		var sut = new Absence();

		var response = new AbsenceResponse
		{
			Type = AbsenceResponse.Parent
		};

		sut.Responses.Add(response);

		// Act
		var result = sut.Explained;

		// Assert
		result.Should().Be(true);
	}

	[Fact]
	public void Explained_ShouldReturnTrue_WhenVerifiedStudentResponseExists()
	{
        // Arrange
        var sut = new Absence();

        var response = new AbsenceResponse
        {
            Type = AbsenceResponse.Student,
            VerificationStatus = AbsenceResponse.Verified
        };

        sut.Responses.Add(response);

        // Act
        var result = sut.Explained;

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Explained_ShouldReturnFalse_WhenUnverifiedStudentResponseExists()
    {
        // Arrange
        var sut = new Absence();

        var response = new AbsenceResponse
        {
            Type = AbsenceResponse.Student,
            VerificationStatus = AbsenceResponse.Pending
        };

        sut.Responses.Add(response);

        // Act
        var result = sut.Explained;

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Explained_ShouldReturnTrue_WhenCoordinatorResponseExists()
    {
        // Arrange
        var sut = new Absence();

        var response = new AbsenceResponse
        {
            Type = AbsenceResponse.Coordinator
        };

        sut.Responses.Add(response);

        // Act
        var result = sut.Explained;

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Explained_ShouldReturnTrue_WhenExternallyExplainedFlagTrue()
    {
        // Arrange
        var sut = new Absence
        {
            ExternallyExplained = true
        };

        // Act
        var result = sut.Explained;

        // Assert
        result.Should().Be(true);
    }
}
