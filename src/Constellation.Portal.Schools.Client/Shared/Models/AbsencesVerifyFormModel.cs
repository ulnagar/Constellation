namespace Constellation.Portal.Schools.Client.Shared.Models;

public sealed class AbsencesVerifyFormModel
{
    public Guid AbsenceId { get; set; }
    public Guid ResponseId { get; set; }
    public string Comment { get; set; }
    public string Username { get; set; }
}
