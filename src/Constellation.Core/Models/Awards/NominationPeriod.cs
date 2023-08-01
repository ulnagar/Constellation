namespace Constellation.Core.Models.Awards;

using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class NominationPeriod
{
    private List<Nomination> _nominations = new();
    private List<NominationPeriodGrade> _grades = new();

    private NominationPeriod() { }

    private NominationPeriod(
        DateOnly lockoutDate,
        List<Grade> includedGrades)
    {
        Id = new();

        foreach (Grade grade in includedGrades)
            _grades.Add(new(Id, grade));

        LockoutDate = lockoutDate;
    }

    public AwardNominationPeriodId Id { get; private set; }
    public IReadOnlyList<NominationPeriodGrade> IncludedGrades => _grades;
    public DateOnly LockoutDate { get; private set; }
    public IReadOnlyList<Nomination> Nominations => _nominations;

    public static Result<NominationPeriod> Create(
        List<Grade> grades,
        DateOnly lockoutDate)
    {
        if (lockoutDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure<NominationPeriod>(DomainErrors.Awards.NominationPeriod.PastDate);

        return Result.Success(new NominationPeriod(lockoutDate, grades));
    }

    public void AddNomination(Nomination nomination) =>
        _nominations.Add(nomination);

    public void DeleteNomination(Nomination nomination) =>
        _nominations.Remove(nomination);
}
