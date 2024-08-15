namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Covers;

using Constellation.Application.Casuals.GetCasualsForSelectionList;
using Constellation.Application.ClassCovers.BulkCreateCovers;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.StaffMembers.GetStaffForSelectionList;
using Constellation.Application.StaffMembers.GetStaffLinkedToOffering;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.ValueObjects;
using Constellation.Presentation.Staff.Areas.Staff.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditCovers)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Covers_Index;


    [BindProperty]
    public string CoveringTeacherId { get; set; }
    [BindProperty]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public List<Guid> CoveredClasses { get; set; } = new();

    public List<CoveringTeacherRecord> CoveringTeacherSelectionList { get; set; } = new();
    public List<ClassRecord> ClassSelectionList { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await PreparePage(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostCreate(CancellationToken cancellationToken)
    {
        var pageReady = await PreparePage(cancellationToken);

        if (!pageReady)
        {
            return Page();
        }

        var teacher = CoveringTeacherSelectionList.First(entry => entry.Id == CoveringTeacherId);

        var teacherType = teacher.Category switch
        {
            "Casuals" => CoverTeacherType.Casual,
            "Teachers" => CoverTeacherType.Staff
        };

        List<OfferingId> offeringIds = CoveredClasses.Select(id => OfferingId.FromValue(id)).ToList();

        var command = new BulkCreateCoversCommand(
            Guid.NewGuid(),
            offeringIds,
            StartDate,
            EndDate,
            teacherType,
            teacher.Id);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error.Message);

            return Page();
        }

        return RedirectToPage("Index");
    }

    private async Task<bool> PreparePage(CancellationToken cancellationToken = default)
    {
        var teacherResponse = await _mediator.Send(new GetStaffForSelectionListQuery(), cancellationToken);
        var casualResponse = await _mediator.Send(new GetCasualsForSelectionListQuery(), cancellationToken);

        if (teacherResponse.IsFailure || casualResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                teacherResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Covers/Index", values: new { area = "Staff" }));
            
            return false;
        }

        CoveringTeacherSelectionList.AddRange(teacherResponse
            .Value
            .Select(teacher =>
                new CoveringTeacherRecord(
                    teacher.StaffId,
                    $"{teacher.FirstName} {teacher.LastName}",
                    $"{teacher.LastName}-{teacher.FirstName}",
                    "Teachers"
                ))
            .ToList());

        CoveringTeacherSelectionList.AddRange(casualResponse
            .Value
            .Select(casual =>
                new CoveringTeacherRecord(
                    casual.Id.ToString(),
                    $"{casual.FirstName} {casual.LastName}",
                    $"{casual.LastName}-{casual.FirstName}",
                    "Casuals"
                ))
            .ToList());

        CoveringTeacherSelectionList = CoveringTeacherSelectionList
            .OrderBy(entry => entry.Category)
            .ThenBy(entry => entry.SortName)
            .ToList();

        var classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                classesResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Covers/Index", values: new { area = "Staff" }));

            return false;
        }

        foreach (var course in classesResponse.Value)
        {
            var teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

            if (teachers.Value.Count == 0)
                continue;

            ClassSelectionList.Add(new ClassRecord(
                course.Id,
                course.Name,
                $"{teachers.Value.First().FirstName[..1]} {teachers.Value.First().LastName}",
                $"Year {course.Name[..2]}"));
        }

        return true;
    }
    
    public sealed record CoveringTeacherRecord(
        string Id,
        string DisplayName,
        string SortName,
        string Category);
}
