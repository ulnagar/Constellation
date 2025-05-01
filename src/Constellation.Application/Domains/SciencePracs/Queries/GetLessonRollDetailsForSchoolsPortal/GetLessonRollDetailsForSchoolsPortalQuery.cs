namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollDetailsForSchoolsPortal;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetLessonRollDetailsForSchoolsPortalQuery(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : IQuery<ScienceLessonRollDetails>;