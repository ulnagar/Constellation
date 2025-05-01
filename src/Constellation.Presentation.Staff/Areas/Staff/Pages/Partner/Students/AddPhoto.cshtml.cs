namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Common.PresentationModels;
using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Commands.SetStudentPhoto;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class AddPhotoModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddPhotoModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddPhotoModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;
    [ViewData] public string PageTitle => "Upload Student Photo";

    [BindProperty]
    public StudentId Id { get; set; } = StudentId.Empty;

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    public SelectList Students { get; set; }

    public async Task OnGet()
    {
        Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        Students = new SelectList(students.Value, "Key", "Value");
    }

    public async Task<IActionResult> OnPost()
    {
        if (UploadFile is null)
        {
            ModalContent = new ErrorDisplay(Error.NullValue);

            Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

            Students = new SelectList(students.Value, "Key", "Value");

            return Page();
        }

        await using MemoryStream target = new();
        await UploadFile!.CopyToAsync(target);

        SetStudentPhotoCommand command = new(
            Id,
            target.ToArray());

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update student photo by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

            Students = new SelectList(students.Value, "Key", "Value");

            return Page();
        }

        return RedirectToPage();
    }
}