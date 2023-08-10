namespace Constellation.Application.SciencePracs.GetLessonRollSubmitContextForSchoolsPortal;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetLessonRollSubmitContextForSchoolsPortalQuery(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : IQuery<ScienceLessonRollForSubmit>;