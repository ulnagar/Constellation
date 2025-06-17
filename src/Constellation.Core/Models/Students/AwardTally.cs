using Constellation.Core.Models.Students.Identifiers;

namespace Constellation.Core.Models.Students;

public sealed class AwardTally
{
    public AwardTally(
        StudentId studentId)
    {
        StudentId = studentId;

        Astras = 0;
        Stellars = 0;
        GalaxyMedals = 0;
        UniversalAchievers = 0;
    }

    public StudentId StudentId { get; private set; }
    public int Astras { get; private set; }
    public int Stellars { get; private set; }
    public int GalaxyMedals { get; private set; }
    public int UniversalAchievers { get; private set; }

    public int CalculatedStellars => Astras / 5;
    public int CalculatedGalaxyMedals => Astras / 25;
    public int CalculatedUniversalAchievers => Astras / 125;

    public void AddAstra() => Astras++;
    public void RemoveAstra() => Astras--;
    public void AddStellar() => Stellars++;
    public void AddGalaxyMedal() => GalaxyMedals++;
    public void AddUniversalAchiever() => UniversalAchievers++;
}