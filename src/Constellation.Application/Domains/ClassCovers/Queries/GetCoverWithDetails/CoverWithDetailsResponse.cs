﻿namespace Constellation.Application.Domains.ClassCovers.Queries.GetCoverWithDetails;

using Core.Models.Identifiers;
using Helpers;
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