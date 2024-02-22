namespace Constellation.Core.Models.SciencePracs;

using Identifiers;

public sealed class SciencePracAttendance
{
    public SciencePracAttendance(
        SciencePracRollId sciencePracRollId,
        string studentId)
    {
        Id = new();

        SciencePracRollId = sciencePracRollId;
        StudentId = studentId;
    }

    public SciencePracAttendanceId Id { get; private set; }
    public SciencePracRollId SciencePracRollId { get; private set; }
    public string StudentId { get; private set; }
    public bool Present { get; private set; }

    public void UpdateAttendance(bool present) => Present = present;
}
