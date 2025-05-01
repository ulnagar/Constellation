namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonDetails;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetLessonDetailsQuery(
    SciencePracLessonId LessonId)
    : IQuery<LessonDetailsResponse>;