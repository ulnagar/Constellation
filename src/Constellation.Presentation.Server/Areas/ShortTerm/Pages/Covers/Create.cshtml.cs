namespace Constellation.Presentation.Server.Areas.ShortTerm.Pages.Covers;

using Constellation.Application.Casuals.GetCasualsForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Application.Staff.GetForSelectionList;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditCovers)]
public class CreateModel : BasePageModel
{
    private readonly IMediator _mediator;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public string CoveringTeacherId { get; set; }
    [BindProperty]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public List<string> CoveredClasses { get; set; } = new();

    public List<CoveringTeacherRecord> CoveringTeacherSelectionList { get; set; } = new();
    public List<ClassRecord> ClassSelectionList { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        var teacherResponse = await _mediator.Send(new GetStaffForSelectionListQuery(), cancellationToken);
        var casualResponse = await _mediator.Send(new GetCasualsForSelectionListQuery(), cancellationToken);

        if (teacherResponse.IsFailure || casualResponse.IsFailure)
            return RedirectToAction("Index");

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
                    $"{casual.FirstName} {casual.LastName} (Casual)",
                    $"{casual.LastName}-{casual.FirstName}",
                    "Casuals"
                ))
            .ToList());
    }
    
    public sealed record CoveringTeacherRecord(
        string Id,
        string FirstName,
        string LastName,
        string SortName,
        string Category);

    public sealed record ClassRecord(
        int Id,
        string Name,
        string Teacher);
}
