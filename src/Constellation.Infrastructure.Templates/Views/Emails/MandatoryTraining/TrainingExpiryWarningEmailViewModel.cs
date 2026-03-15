namespace Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;

using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public sealed class TrainingExpiryWarningEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/MandatoryTraining/TrainingExpiryWarningEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    
    public Dictionary<string, string> Courses { get; set; } = [];

    public required string WarningText { get; set;  }
}
