namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record SciencePracLessonOffering(
    SciencePracLessonId LessonId,
    OfferingId OfferingId);