using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using System;

namespace Constellation.Core.Tests.Unit.Models.MandatoryTraining;

public class TrainingCompletionTests
{
    [Fact]
    public void MarkNotRequired_DoesNothing_WhenModuleDoesNotAllowNotRequiredEntries()
    {
        // Arrange
        var module = TrainingModule.Create(
            new TrainingModuleId(),
            "Test Module",
            TrainingModuleExpiryFrequency.OnceOff,
            string.Empty,
            false);

        var sut = TrainingCompletion.Create(
            new TrainingCompletionId(),
            "1",
            module.Id);

        sut.SetCompletedDate(DateTime.Today);

        // Act
        sut.MarkNotRequired(module);

        // Assert
        sut.NotRequired.Should().Be(false);
        sut.CompletedDate.Should().NotBe(null);
    }

    [Fact]
    public void MarkNotRequired_ShouldUpdateEntity_WhenModuleAllowsNotRequiredEntries()
    {
        // Arrange
        var module = TrainingModule.Create(
            new TrainingModuleId(),
            "Test Module",
            TrainingModuleExpiryFrequency.OnceOff,
            string.Empty,
            true);

        var sut = TrainingCompletion.Create(
            new TrainingCompletionId(),
            "1",
            module.Id);

        sut.SetCompletedDate(DateTime.Today);

        sut.SetCompletedDate(DateTime.Today);

        // Act
        sut.MarkNotRequired(module);

        // Assert
        sut.NotRequired.Should().Be(true);
        sut.CompletedDate.Should().Be(null);
    }

    [Fact]
    public void SetCompletedDate_ShouldUpdateNotRequiredField_WhenSettingANewCompletionDate()
    {
        // Arrange
        var module = TrainingModule.Create(
            new TrainingModuleId(),
            "Test Module",
            TrainingModuleExpiryFrequency.OnceOff,
            string.Empty,
            true);

        var sut = TrainingCompletion.Create(
            new TrainingCompletionId(),
            "1",
            module.Id);

        sut.SetCompletedDate(DateTime.Today);

        sut.MarkNotRequired(module);

        // Act
        sut.SetCompletedDate(DateTime.Today);

        // Assert
        sut.NotRequired.Should().Be(false);
    }
}
