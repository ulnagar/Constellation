namespace Constellation.Core.Models;

using Constellation.Core.Enums;

public abstract class AdobeConnectOperation
{
    public int Id { get; set; }
    public string GroupSco { get; set; } = string.Empty;
    public string ScoId { get; set; } = string.Empty; 
    public virtual AdobeConnectRoom? Room { get; set; }
    public string PrincipalId { get; set; } = string.Empty;
    public virtual AdobeConnectOperationAction? Action { get; set; }
    public DateTime DateScheduled { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDeleted { get; set; }
    public int? CoverId { get; set; }
    public virtual ClassCover? Cover { get; set; }

    public void Delete()
    {
        IsDeleted = true;
    }
}

public class StudentAdobeConnectOperation : AdobeConnectOperation
{
    public string StudentId { get; set; } = string.Empty;
    public virtual Student? Student { get; set; }
}

public class CasualAdobeConnectOperation : AdobeConnectOperation
{
    public int? CasualId { get; set; }
    public virtual Casual? Casual { get; set; }
}

public class TeacherAdobeConnectOperation : AdobeConnectOperation
{
    public string StaffId { get; set; } = string.Empty;
    public virtual Staff? Teacher { get; set; }
}

public class TeacherAdobeConnectGroupOperation : AdobeConnectOperation
{
    public string TeacherId { get; set; } = string.Empty;
    public virtual Staff? Teacher { get; set; }
    public string GroupName { get; set; } = string.Empty;
}