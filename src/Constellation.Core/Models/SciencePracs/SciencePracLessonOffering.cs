namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Offerings.Identifiers;
using Identifiers;

public sealed record SciencePracLessonOffering(
    SciencePracLessonId LessonId,
    OfferingId OfferingId);