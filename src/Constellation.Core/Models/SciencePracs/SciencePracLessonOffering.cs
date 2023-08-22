namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record SciencePracLessonOffering(
    SciencePracLessonId LessonId,
    OfferingId OfferingId);