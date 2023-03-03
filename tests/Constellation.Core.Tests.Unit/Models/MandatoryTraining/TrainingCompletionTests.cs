using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining;

namespace Constellation.Core.Tests.Unit.Models.MandatoryTraining;

public class TrainingCompletionTests
{
    [Fact]
    public void MarkNotRequired_DoesNothing_WhenModuleDoesNotAllowNotRequiredEntries()
    {
        // Arrange
        var module = new TrainingModule(Guid.NewGuid())
        {
            Name = "Test Module",
            Expiry = TrainingModuleExpiryFrequency.OnceOff,
            Url = string.Empty,
            CanMarkNotRequired = false
        };

        var sut = new TrainingCompletion(Guid.NewGuid())
        {
            StaffId = "1",
            TrainingModuleId = module.Id,
            Module = module
        };

        sut.SetCompletedDate(DateTime.Today);

        // Act
        sut.MarkNotRequired();

        // Assert
        sut.NotRequired.Should().Be(false);
        sut.CompletedDate.Should().NotBe(null);
    }

    [Fact]
    public void MarkNotRequired_ShouldUpdateEntity_WhenModuleAllowsNotRequiredEntries()
    {
        // Arrange
        var module = new TrainingModule(Guid.NewGuid())
        {
            Name = "Test Module",
            Expiry = TrainingModuleExpiryFrequency.OnceOff,
            Url = string.Empty,
            CanMarkNotRequired = true
        };

        var sut = new TrainingCompletion(Guid.NewGuid())
        {
            StaffId = "1",
            TrainingModuleId = module.Id,
            Module = module
        };

        sut.SetCompletedDate(DateTime.Today);

        // Act
        sut.MarkNotRequired();

        // Assert
        sut.NotRequired.Should().Be(true);
        sut.CompletedDate.Should().Be(null);
    }

    [Fact]
    public void SetCompletedDate_ShouldUpdateNotRequiredField_WhenSettingANewCompletionDate()
    {
        // Arrange
        var module = new TrainingModule(Guid.NewGuid())
        {
            Name = "Test Module",
            Expiry = TrainingModuleExpiryFrequency.OnceOff,
            Url = string.Empty,
            CanMarkNotRequired = true
        };

        var sut = new TrainingCompletion(Guid.NewGuid())
        {
            StaffId = "1",
            TrainingModuleId = module.Id,
            Module = module
        };

        sut.MarkNotRequired();

        // Act
        sut.SetCompletedDate(DateTime.Today);

        // Assert
        sut.NotRequired.Should().Be(false);
    }
}
