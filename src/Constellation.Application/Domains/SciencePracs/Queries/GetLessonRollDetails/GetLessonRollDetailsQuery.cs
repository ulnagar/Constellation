namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollDetails;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetLessonRollDetailsQuery(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : IQuery<LessonRollDetailsResponse>;