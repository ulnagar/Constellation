namespace Constellation.Presentation.Server.Areas.ShortTerm.Pages.Covers;

using Constellation.Application.Casuals.GetCasualsForSelectionList;
using Constellation.Application.ClassCovers.BulkCreateCovers;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.StaffMembers.GetStaffForSelectionList;
using Constellation.Application.StaffMembers.GetStaffLinkedToOffering;
using Constellation.Core.ValueObjects;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditCovers)]
public class CreateModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public CreateModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty]
    public string CoveringTeacherId { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public List<int> CoveredClasses { get; set; } = new();

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

        var command = new BulkCreateCoversCommand(
            Guid.NewGuid(),
            CoveredClasses,
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
        await GetClasses(_mediator);

        var teacherResponse = await _mediator.Send(new GetStaffForSelectionListQuery(), cancellationToken);
        var casualResponse = await _mediator.Send(new GetCasualsForSelectionListQuery(), cancellationToken);

        if (teacherResponse.IsFailure || casualResponse.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = teacherResponse.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Covers/Index", values: new { area = "ShortTerm" })
            };

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
            Error = new ErrorDisplay
            {
                Error = classesResponse.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Covers/Index", values: new { area = "ShortTerm" })
            };

            return false;
        }

        foreach (var course in classesResponse.Value)
        {
            var teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

            var frequency = teachers
                .Value
                .GroupBy(x => x.StaffId)
                .Select(group => new { StaffId = group.Key, Count = group.Count() })
                .OrderByDescending(x => x.Count)
                .First();

            var primaryTeacher = teachers.Value.First(teacher => teacher.StaffId == frequency.StaffId);

            ClassSelectionList.Add(new ClassRecord(
                course.Id,
                course.Name,
                $"{primaryTeacher.FirstName[..1]} {primaryTeacher.LastName}",
                $"Year {course.Name[..2]}"));
        }

        return true;
    }
    
    public sealed record CoveringTeacherRecord(
        string Id,
        string DisplayName,
        string SortName,
        string Category);

    public sealed record ClassRecord(
        int Id,
        string Name,
        string Teacher,
        string Grade);
}
