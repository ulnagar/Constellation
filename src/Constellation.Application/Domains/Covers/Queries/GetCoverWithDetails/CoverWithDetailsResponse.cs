namespace Constellation.Application.Domains.Covers.Queries.GetCoverWithDetails;

using Constellation.Core.Models.Covers.Identifiers;
using Core.Models.Covers.Enums;
using Helpers;
using System;
using System.ComponentModel.DataAnnotations;

public sealed record CoverWithDetailsResponse(
    CoverId Id,
    [Display(Name = DisplayNameDefaults.DisplayName)]
    string UserName,
    [Display(Name = DisplayNameDefaults.SchoolName)]
    string UserSchool,
    [Display(Name = DisplayNameDefaults.ClassName)]
    string OfferingName,
    [Display(Name = DisplayNameDefaults.DateStart)]
    DateOnly StartDate,
    [Display(Name = DisplayNameDefaults.DateEnd)]
    DateOnly EndDate,
    CoverType CoverType);