namespace Constellation.Application.ClassCovers.GetCoverWithDetails;

using Constellation.Application.Helpers;
using Constellation.Core.Models.Identifiers;
using System;
using System.ComponentModel.DataAnnotations;

public sealed record CoverWithDetailsResponse(
    ClassCoverId Id,
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