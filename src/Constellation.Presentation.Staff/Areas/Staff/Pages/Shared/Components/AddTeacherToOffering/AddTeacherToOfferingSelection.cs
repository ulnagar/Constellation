﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddTeacherToOffering;

using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;

public class AddTeacherToOfferingSelection
{
    public OfferingId OfferingId { get; set; }
    public string StaffId { get; set; }

    [ModelBinder(typeof(FromValueBinder))]
    public AssignmentType? AssignmentType { get; set; }

    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    public Dictionary<string, string> Staff { get; set; } = new();
    public Dictionary<string, string> AssignmentTypes { get; set; } = new();
}
