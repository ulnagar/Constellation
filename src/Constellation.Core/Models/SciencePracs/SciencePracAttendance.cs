namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Identifiers;

public sealed class SciencePracAttendance
{
    public SciencePracAttendance(
        SciencePracRollId rollId,
        string studentId)
    {
        Id = new();

        SciencePracRollId = rollId;
        StudentId = studentId;
    }

    public SciencePracAttendanceId Id { get; private set; }
    public SciencePracRollId SciencePracRollId { get; private set; }
    public string StudentId { get; private set; }
    public bool Present { get; private set; }

    public void UpdateAttendance(bool present) => Present = present;
}
