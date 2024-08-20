namespace Constellation.Application.DTOs.Canvas;

using System.Collections.Generic;

public sealed record RubricEntry(
    string RubricId,
    string Name,
    double MaxPoints,
    List<RubricEntry.RubricCriterion> Criteria)
{
    public sealed record RubricCriterion(
        string CriterionId,
        double MaxPoints,
        string Name,
        string Description,
        List<RubricCriterionRating> Ratings);

    public sealed record RubricCriterionRating(
        string RatingId,
        double Points,
        string Name,
        string Description);
}