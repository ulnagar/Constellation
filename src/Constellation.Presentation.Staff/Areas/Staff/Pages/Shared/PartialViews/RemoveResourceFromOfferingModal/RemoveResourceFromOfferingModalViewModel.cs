﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveResourceFromOfferingModal;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveResourceFromOfferingModalViewModel(
    OfferingId OfferingId,
    ResourceId ResourceId,
    string ResourceName,
    string ResourceType,
    string CourseName,
    string OfferingName);