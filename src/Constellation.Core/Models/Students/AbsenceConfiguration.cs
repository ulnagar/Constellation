namespace Constellation.Core.Models.Students;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using System;

public class AbsenceConfiguration: IAuditableEntity
{
    private AbsenceConfiguration()
    {
            
    }

    public StudentAbsenceConfigurationId Id { get; private set; }
    public string StudentId { get; private set; }
    public int CalendarYear { get; private set; }
    public AbsenceType AbsenceType { get; private set; }
    public DateOnly ScanStartDate { get; private set; }
    public DateOnly ScanEndDate { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }

    public bool IsDeleted { get; private set; }

    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }


}
