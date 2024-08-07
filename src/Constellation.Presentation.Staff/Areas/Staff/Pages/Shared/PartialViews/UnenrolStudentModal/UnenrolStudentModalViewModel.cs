﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.UnenrolStudentModal;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record UnenrolStudentModalViewModel(
    OfferingId OfferingId,
    string StudentId,
    string StudentName,
    string CourseName,
    string OfferingName);
