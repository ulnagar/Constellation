namespace Constellation.Core.Models.Awards;

using Enums;
using Errors;
using Identifiers;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class NominationPeriod
{
    private List<Nomination> _nominations = new();
    private List<NominationPeriodGrade> _grades = new();

    private NominationPeriod() { }

    private NominationPeriod(
        string name,
        DateOnly lockoutDate,
        List<Grade> includedGrades)
    {
        Id = new();

        foreach (Grade grade in includedGrades)
            AddGrade(grade);

        Name = name;
        LockoutDate = lockoutDate;
    }

    public AwardNominationPeriodId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IReadOnlyList<NominationPeriodGrade> IncludedGrades => _grades;
    public DateOnly LockoutDate { get; private set; }
    public IReadOnlyList<Nomination> Nominations => _nominations;

    public static Result<NominationPeriod> Create(
        string name,
        List<Grade> grades,
        DateOnly lockoutDate)
    {
        if (lockoutDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure<NominationPeriod>(AwardNominationPeriodErrors.PastDate);

        return Result.Success(new NominationPeriod(name, lockoutDate, grades));
    }

    public Result AddNomination(Nomination nomination)
    {
        if (HasDuplicateEntry(nomination))
            return Result.Failure(AwardNominationErrors.DuplicateFound);

        _nominations.Add(nomination);
        return Result.Success();
    }

    public void UpdateName(string name) => Name = name;
    public void UpdateLockoutDate(DateOnly date) => LockoutDate = date;
    private void AddGrade(Grade grade) => _grades.Add(new(Id, grade));

    private bool HasDuplicateEntry(Nomination newAward) =>
        newAward switch
        {
            FirstInSubjectNomination fis =>
                Nominations
                    .OfType<FirstInSubjectNomination>()
                    .Any(entry =>
                        entry.StudentId == fis.StudentId &&
                        entry.CourseId == fis.CourseId &&
                        !entry.IsDeleted),

            FirstInSubjectMathematicsNomination fism =>
                Nominations
                    .OfType<FirstInSubjectMathematicsNomination>()
                    .Any(entry =>
                        entry.StudentId == fism.StudentId &&
                        entry.CourseId == fism.CourseId &&
                        !entry.IsDeleted),

            FirstInSubjectScienceTechnologyNomination fisst =>
                Nominations
                    .OfType<FirstInSubjectScienceTechnologyNomination>()
                    .Any(entry =>
                        entry.StudentId == fisst.StudentId &&
                        entry.CourseId == fisst.CourseId &&
                        !entry.IsDeleted),

            AcademicExcellenceNomination ae =>
                Nominations
                    .OfType<AcademicExcellenceNomination>()
                    .Any(entry =>
                        entry.StudentId == ae.StudentId &&
                        entry.CourseId == ae.CourseId &&
                        entry.OfferingId == ae.OfferingId &&
                        !entry.IsDeleted),

            AcademicExcellenceMathematicsNomination aem =>
                Nominations
                    .OfType<AcademicExcellenceMathematicsNomination>()
                    .Any(entry =>
                        entry.StudentId == aem.StudentId &&
                        entry.CourseId == aem.CourseId &&
                        entry.OfferingId == aem.OfferingId &&
                        !entry.IsDeleted),

            AcademicExcellenceScienceTechnologyNomination aest =>
                Nominations
                    .OfType<AcademicExcellenceScienceTechnologyNomination>()
                    .Any(entry =>
                        entry.StudentId == aest.StudentId &&
                        entry.CourseId == aest.CourseId &&
                        entry.OfferingId == aest.OfferingId &&
                        !entry.IsDeleted),

            AcademicAchievementNomination aa =>
                Nominations
                    .OfType<AcademicAchievementNomination>()
                    .Any(entry =>
                        entry.StudentId == aa.StudentId &&
                        entry.CourseId == aa.CourseId &&
                        entry.OfferingId == aa.OfferingId &&
                        !entry.IsDeleted),

            AcademicAchievementMathematicsNomination aam =>
                Nominations
                    .OfType<AcademicAchievementMathematicsNomination>()
                    .Any(entry =>
                        entry.StudentId == aam.StudentId &&
                        entry.CourseId == aam.CourseId &&
                        entry.OfferingId == aam.OfferingId &&
                        !entry.IsDeleted),

            AcademicAchievementScienceTechnologyNomination aast =>
                Nominations
                    .OfType<AcademicAchievementScienceTechnologyNomination>()
                    .Any(entry =>
                        entry.StudentId == aast.StudentId &&
                        entry.CourseId == aast.CourseId &&
                        entry.OfferingId == aast.OfferingId &&
                        !entry.IsDeleted),

            // There is no reason that this award cannot be given twice to the same person
            PrincipalsAwardNomination pa => false,
            GalaxyMedalNomination ga => false,
            UniversalAchieverNomination ua => false,

            _ => throw new NotImplementedException()
        };
}
