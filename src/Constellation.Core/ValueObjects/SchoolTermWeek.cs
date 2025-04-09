namespace Constellation.Core.ValueObjects;

using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using System.Collections.Generic;

public sealed class SchoolTermWeek : ValueObject
{
    public SchoolTerm Term { get; init; }
    public SchoolWeek Week { get; init; }

    public SchoolTermWeek(SchoolTerm term, SchoolWeek week)
        => (Term, Week) = (term, week);

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Term;
        yield return Week;
    }
}