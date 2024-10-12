namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Students.Identifiers;
using Identifiers;

public sealed class SciencePracAttendance
{
    public SciencePracAttendance(
        SciencePracRollId sciencePracRollId,
        StudentId studentId)
    {
        Id = new();

        SciencePracRollId = sciencePracRollId;
        StudentId = studentId;
    }

    public SciencePracAttendanceId Id { get; private set; }
    public SciencePracRollId SciencePracRollId { get; private set; }
    public StudentId StudentId { get; private set; }
    public bool Present { get; private set; }

    public void UpdateAttendance(bool present) => Present = present;
}
