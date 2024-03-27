namespace Constellation.Application.SciencePracs.GetLessonRollDetailsForSchoolsPortal;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetLessonRollDetailsForSchoolsPortalQuery(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : IQuery<ScienceLessonRollDetails>;