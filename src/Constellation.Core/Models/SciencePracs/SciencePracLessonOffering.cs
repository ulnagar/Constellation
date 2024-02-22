namespace Constellation.Core.Models.SciencePracs;

using Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record SciencePracLessonOffering(
    SciencePracLessonId LessonId,
    OfferingId OfferingId);