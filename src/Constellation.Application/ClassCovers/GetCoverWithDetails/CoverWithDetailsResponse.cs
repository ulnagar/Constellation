namespace Constellation.Application.ClassCovers.GetCoverWithDetails;

using Constellation.Application.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

public sealed record CoverWithDetailsResponse(
    Guid Id,
    [Display(Name = DisplayNameDefaults.DisplayName)]
    string UserName,
    [Display(Name = DisplayNameDefaults.SchoolName)]
    string UserSchool,
    [Display(Name = DisplayNameDefaults.ClassName)]
    string OfferingName,
    [Display(Name = DisplayNameDefaults.DateStart)]
    DateOnly StartDate,
    [Display(Name = DisplayNameDefaults.DateEnd)]
    DateOnly EndDate);