namespace Constellation.Core.Tests.Unit.Models.MandatoryTraining;

using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining;
using Core.Models.MandatoryTraining.Identifiers;

public class TrainingCompletionTests
{
    [Fact]
    public void MarkNotRequired_DoesNothing_WhenModuleDoesNotAllowNotRequiredEntries()
    {
        // Arrange
        TrainingRole role = TrainingRole.Create(
            "Test Role");

        role.AddMember("1");

        TrainingModule? module = TrainingModule.Create(
            new TrainingModuleId(),
            "Test Module",
            TrainingModuleExpiryFrequency.OnceOff,
            string.Empty);

        role.AddModule(module.Id);

        TrainingCompletion? sut = TrainingCompletion.Create(
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
        TrainingRole role = TrainingRole.Create(
            "Test Role");

        role.AddMember("1");

        TrainingModule? module = TrainingModule.Create(
            new TrainingModuleId(),
            "Test Module",
            TrainingModuleExpiryFrequency.OnceOff,
            string.Empty);

        TrainingCompletion? sut = TrainingCompletion.Create(
            "1",
            module.Id);

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
        TrainingModule? module = TrainingModule.Create(
            new TrainingModuleId(),
            "Test Module",
            TrainingModuleExpiryFrequency.OnceOff,
            string.Empty);

        TrainingCompletion? sut = TrainingCompletion.Create(
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
