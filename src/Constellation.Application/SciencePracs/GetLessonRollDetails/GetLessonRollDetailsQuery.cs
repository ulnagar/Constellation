namespace Constellation.Application.SciencePracs.GetLessonRollDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetLessonRollDetailsQuery(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : IQuery<LessonRollDetailsResponse>;