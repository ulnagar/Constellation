namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Models.Auth;
using Constellation.Application.Rollover.ProcessRolloverDecisions;
using Constellation.Application.Students.GetCurrentStudentsFromGrade;
using Constellation.Application.Students.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models.Rollover;
using Constellation.Core.Models.Rollover.Repositories;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class RolloverModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IRolloverRepository _rolloverRepository;

    public RolloverModel(
        ISender mediator, 
        IRolloverRepository rolloverRepository)
    {
        _mediator = mediator;
        _rolloverRepository = rolloverRepository;
    }

    [BindProperty] 
    public List<RolloverDecision> Statuses { get; set; } = new();

    public List<RolloverResult> ProcessResults { get; set; } = new();

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

        _rolloverRepository.Reset();

        await GetStudents(Grade.Y12);
    }

    public async Task<IActionResult> OnPost()
    {
        if (Statuses.Any())
        {
            foreach (RolloverDecision decision in Statuses)
            {
                Result registerAttempt = _rolloverRepository.RegisterDecision(decision);

                if (registerAttempt.IsFailure)
                {
                    Error = new()
                    {
                        Error = registerAttempt.Error,
                        RedirectPath = null
                    };

                    return Page();
                }

            }

            Statuses = new();
        }

        return CurrentGrade switch
        {
            Grade.Y12 => await GetStudents(Grade.Y11),
            Grade.Y11 => await GetStudents(Grade.Y10),
            Grade.Y10 => await GetStudents(Grade.Y09),
            Grade.Y09 => await GetStudents(Grade.Y08),
            Grade.Y08 => await GetStudents(Grade.Y07),
            Grade.Y07 => await GetStudents(Grade.Y06),
            Grade.Y06 => await GetStudents(Grade.Y05),
            Grade.Y05 => await Finalise(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IActionResult> GetStudents(Grade grade)
    {
        Result<List<StudentResponse>> attempt = await _mediator.Send(new GetCurrentStudentsFromGradeQuery(grade));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return Page();
        }

        foreach (StudentResponse entry in attempt.Value.OrderBy(student => student.DisplayName))
        {
            Statuses.Add(new(
                entry.StudentId,
                entry.DisplayName,
                grade,
                entry.School));
        }

        CurrentGrade = grade;

        return Page();
    }

    public async Task<IActionResult> Finalise()
    {
        Result<List<RolloverResult>> results = await _mediator.Send(new ProcessRolloverDecisionsCommand());

        if (results.IsFailure)
        {
            Error = new()
            {
                Error = results.Error,
                RedirectPath = null
            };

            return Page();
        }

        ProcessResults = results.Value;

        return Page();
    }
}
