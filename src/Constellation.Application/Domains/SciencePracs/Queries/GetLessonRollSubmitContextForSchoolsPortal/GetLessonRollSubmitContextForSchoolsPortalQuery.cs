namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollSubmitContextForSchoolsPortal;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetLessonRollSubmitContextForSchoolsPortalQuery(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : IQuery<ScienceLessonRollForSubmit>;