namespace Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;

using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public class TrainingExpiringSoonWarningViewModel : EmailLayoutBaseViewModel
{
    public Dictionary<string, string> Courses { get; set; } = new();
}
