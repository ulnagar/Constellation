namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Common.PresentationModels;
using Application.Domains.Rollover.Commands.ProcessRolloverDecisions;
using Application.Domains.Students.Queries.GetCurrentStudentsFromGrade;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models.Rollover;
using Constellation.Core.Models.Rollover.Repositories;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Models.Rollover.Enums;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[HasPermission(AuthPermission.Admin_Rollover_Edit_Value)]
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

    [ViewData] public string ActivePage => Models.ActivePage.Rollover;
    [ViewData] public string PageTitle => "Annual Rollover";

    [BindProperty(SupportsGet = true)]
    public Grade CurrentGrade { get; set; } = Grade.Empty;

    public List<RolloverDecision> Statuses { get; set; } = new();
    public List<RolloverResult> ProcessResults { get; set; } = new();
    
    public record RolloverRequest(
        string CurrentGrade, 
        Dictionary<Guid, string> Statuses);

    public async Task<IActionResult> OnGet()
    {
        if (CurrentGrade == Grade.Empty)
        {
            _rolloverRepository.Reset();

            return RedirectToPage(routeValues: new { CurrentGrade = Grade.Y12.Value });
        }

        Result result = await GetStudents(CurrentGrade);

        if (result.IsFailure)
            ModalContent = ErrorDisplay.Create(result.Error);

        return Page();
    }

    public async Task<IActionResult> OnPost([FromBody] RolloverRequest request)
    {
        if (!ModelState.IsValid)
            return Page();

        Result<Grade> currentGrade = Grade.FromValue(request.CurrentGrade);

        if (currentGrade.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(currentGrade.Error);
            return Page();
        }

        if (request.Statuses.Any())
        {
            Result<List<StudentResponse>> attempt = await _mediator.Send(new GetCurrentStudentsFromGradeQuery(currentGrade.Value));

            if (attempt.IsFailure)
                return BadRequest(attempt.Error.Code + ": " + attempt.Error.Message);

            foreach (KeyValuePair<Guid, string> kvp in request.Statuses)
            {
                StudentId studentId = StudentId.FromValue(kvp.Key);
                bool successParse = Enum.TryParse(typeof(RolloverStatus), kvp.Value, out object? status);

                if (!successParse)
                    return BadRequest($"Invalid rollover status: {kvp.Value}");

                StudentResponse? student = attempt.Value.FirstOrDefault(entry => entry.StudentId == studentId);

                if (student is null)
                    continue;

                RolloverDecision decision = new(
                    student.StudentId,
                    student.StudentReferenceNumber,
                    student.Name.DisplayName,
                    currentGrade.Value,
                    student.School)
                {
                    Decision = (RolloverStatus)status!
                };

                Result registerAttempt = _rolloverRepository.RegisterDecision(decision);

                if (registerAttempt.IsFailure)
                    return BadRequest(registerAttempt.Error.Code + ": " + registerAttempt.Error.Message);
            }

            Statuses = new();
        }

        string? nextGrade = currentGrade.Value.Value switch
        {
            "Y12" => Grade.Y11.Value,
            "Y11" => Grade.Y10.Value,
            "Y10" => Grade.Y09.Value,
            "Y09" => Grade.Y08.Value,
            "Y08" => Grade.Y07.Value,
            "Y07" => Grade.Y06.Value,
            "Y06" => Grade.Y05.Value,
            "Y05" => null,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new JsonResult(new { nextGrade });
    }

    private async Task<Result> GetStudents(Grade grade)
    {
        Result<List<StudentResponse>> attempt = await _mediator.Send(new GetCurrentStudentsFromGradeQuery(grade));

        if (attempt.IsFailure)
            return attempt;

        foreach (StudentResponse entry in attempt.Value.OrderBy(student => student.Name.SortOrder))
        {
            Statuses.Add(new(
                entry.StudentId,
                entry.StudentReferenceNumber,
                entry.Name.DisplayName,
                grade,
                entry.School));
        }

        CurrentGrade = grade;

        return Result.Success();
    }

    public async Task<IActionResult> OnPostFinalise()
    {
        Result<List<RolloverResult>> results = await _mediator.Send(new ProcessRolloverDecisionsCommand());

        if (results.IsFailure)
            return BadRequest(results.Error.Code + ": " + results.Error.Message);

        ProcessResults = results.Value;

        return Page();
    }
}
