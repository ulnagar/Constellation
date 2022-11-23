namespace Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;

using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public abstract class TrainingExpiryWarningEmailViewModel : EmailLayoutBaseViewModel
{
    public Dictionary<string, string> Courses { get; set; } = new();

    public abstract string WarningText { get; }
}

public sealed class TrainingExpiringSoonWarningEmailViewModel : TrainingExpiryWarningEmailViewModel
{
    public override string WarningText => "expire within the next 30 days";
}

public sealed class TrainingExpiringSoonAlertEmailViewModel : TrainingExpiryWarningEmailViewModel
{
    public override string WarningText => "expire within the next 14 days";
}

public sealed class TrainingExpiredAlertEmailViewModel : TrainingExpiryWarningEmailViewModel
{
    public override string WarningText => "have expired";
}