namespace Constellation.Application.SciencePracs.GetLessonDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetLessonDetailsQuery(
    SciencePracLessonId LessonId)
    : IQuery<LessonDetailsResponse>;