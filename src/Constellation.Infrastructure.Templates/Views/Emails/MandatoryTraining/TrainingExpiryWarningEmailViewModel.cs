namespace Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;

using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public abstract class TrainingExpiryWarningEmailViewModel : EmailLayoutBaseViewModel
{
    public Dictionary<string, string> Courses { get; set; } = new();

    public abstract string WarningText { get; }
}
