namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Identifiers;

public sealed record SciencePracLessonOffering(
    SciencePracLessonId LessonId,
    int OfferingId);