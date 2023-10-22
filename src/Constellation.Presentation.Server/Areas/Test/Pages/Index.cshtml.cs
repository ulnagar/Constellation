namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Students.GetCurrentStudentsFromGrade;
using Application.Students.Models;
using Constellation.Presentation.Server.BaseModels;
using Core.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Services;
using Shared.Models;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly RolloverService _rolloverService;

    public IndexModel(
        ISender mediator,
        RolloverService rolloverService)
    {
        _mediator = mediator;
        _rolloverService = rolloverService;
    }

    [BindProperty] 
    public List<RolloverDecision> Statuses { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Grade CurrentGrade { get; set; }

    // Withdraw old Year 12 students
    // Move Year 11 to Year 12
    // Withdraw leaving Year 10 students
    // Move remaining Year 10 to Year 11
    // Move Year 9 to Year 10
    // Move Year 8 to Year 9
    // Move Year 7 to Year 8
    // Withdraw leaving Year 6 students
    // Move remaining Year 6 to Year 7
    // Move Year 5 to Year 6
    // Enrol new Year 11 students
    // Enrol new Year 7 students
    // Enrol new Year 5 students

    public async Task OnGetAsync()
    {
        await GetClasses(_mediator);

        await GetStudents(Grade.Y12);
    }

    public async Task<IActionResult> OnPost()
    {
        switch (CurrentGrade)
        {
            case Grade.Y12:
                await GetStudents(Grade.Y11);
                break;
            case Grade.Y11:
                await GetStudents(Grade.Y10);
                break;
            case Grade.Y10:
                await GetStudents(Grade.Y09);
                break;
            case Grade.Y09:
                await GetStudents(Grade.Y08);
                break;
            case Grade.Y08:
                await GetStudents(Grade.Y07);
                break;
            case Grade.Y07:
                await GetStudents(Grade.Y06);
                break;
            case Grade.Y06:
                await GetStudents(Grade.Y05);
                break;
            case Grade.Y05:
                await Finalise();
                break;
        }

        return Page();
    }

    public async Task GetStudents(Grade grade)
    {
        if (Statuses.Any())
        {
            _rolloverService.RolloverDecisions.AddRange(Statuses);

            Statuses = new();
        }

        Result<List<StudentResponse>> attempt = await _mediator.Send(new GetCurrentStudentsFromGradeQuery(grade));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return;
        }

        foreach (StudentResponse entry in attempt.Value.OrderBy(student => student.DisplayName))
        {
            if (_rolloverService.RolloverDecisions.Any(record => record.StudentId == entry.StudentId))
                continue;

            Statuses.Add(new()
            {
                StudentId = entry.StudentId,
                StudentName = entry.DisplayName,
                Grade = grade,
                SchoolName = entry.School,
                Decision = RolloverDecision.Status.Unknown
            });
        }

        CurrentGrade = grade;
    }

    public async Task Finalise()
    {
        var entriesByGrade = _rolloverService.RolloverDecisions.GroupBy(entry => entry.Grade);
    }
}
