namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.GroupTutorials.AddStudentToTutorial;
using Constellation.Application.GroupTutorials.AddTeacherToTutorial;
using Constellation.Application.GroupTutorials.CreateRoll;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;
using Constellation.Application.GroupTutorials.RemoveStudentFromTutorial;
using Constellation.Application.GroupTutorials.RemoveTeacherFromTutorial;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Components.TutorialRollCreate;
using Shared.Components.TutorialStudentEnrolment;
using Shared.Components.TutorialTeacherAssignment;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanViewGroupTutorials)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public TutorialStudentEnrolmentSelection StudentEnrolment { get; set; }

    [BindProperty]
    public TutorialStudentRemovalSelection? StudentRemoval { get; set; }

    [BindProperty]
    public TutorialTeacherAssignmentSelection TeacherAssignment { get; set; }

    [BindProperty]
    public TutorialTeacherRemovalSelection? TeacherRemoval { get; set; }

    [BindProperty]
    public TutorialRollCreateSelection RollCreate { get; set; }

    public GroupTutorialDetailResponse Tutorial { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await GetPageInformation(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostEnrolStudent()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        if (string.IsNullOrWhiteSpace(StudentEnrolment.StudentId))
        {
            await GetPageInformation();
            return Page();
        }

        DateOnly? effectiveDate = (StudentEnrolment.LimitedTime) ? DateOnly.FromDateTime(StudentEnrolment.EffectiveTo) : null;

        var result = await _mediator.Send(new AddStudentToTutorialCommand(GroupTutorialId.FromValue(Id), StudentEnrolment.StudentId, effectiveDate));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        StudentEnrolment = new();

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Details", new { area = "Staff", Id });
    }

    public async Task<IActionResult> OnPostAssignTeacher()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        if (string.IsNullOrWhiteSpace(TeacherAssignment.StaffId))
        {
            await GetPageInformation();
            return Page();
        }

        DateOnly? effectiveDate = (TeacherAssignment.LimitedTime) ? DateOnly.FromDateTime(TeacherAssignment.EffectiveTo) : null;

        var result = await _mediator.Send(new AddTeacherToTutorialCommand(GroupTutorialId.FromValue(Id), TeacherAssignment.StaffId, effectiveDate));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        TeacherAssignment = new();

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Details", new { area = "Staff", Id });
    }

    public async Task<IActionResult> OnGetRemoveTeacher(Guid teacherId)
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        await GetPageInformation();

        var teacherIdObject = TutorialTeacherId.FromValue(teacherId);

        var teacherRecord = Tutorial.Teachers.FirstOrDefault(teacher => teacher.Id == teacherIdObject);

        if (teacherRecord == null)
        {
            return ShowError(DomainErrors.GroupTutorials.TutorialTeacher.NotFound);
        }

        TeacherRemoval = new()
        {
            Id = teacherId,
            Name = teacherRecord.Name
        };

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveTeacher()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        DateOnly? effectiveDate = (!TeacherRemoval.Immediate) ? DateOnly.FromDateTime(TeacherRemoval.EffectiveOn) : null;

        var result = await _mediator.Send(new RemoveTeacherFromTutorialCommand(GroupTutorialId.FromValue(Id), TutorialTeacherId.FromValue(TeacherRemoval.Id), effectiveDate));

        TeacherRemoval = null;

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Details", new { area = "Staff", Id = Id });
    }

    public async Task<IActionResult> OnGetRemoveStudent(Guid enrolmentId)
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        await GetPageInformation();

        var enrolmentIdObject = TutorialEnrolmentId.FromValue(enrolmentId);

        var enrolmentRecord = Tutorial.Students.FirstOrDefault(enrolment => enrolment.Id == enrolmentIdObject);

        if (enrolmentRecord == null)
        {
            return ShowError(DomainErrors.GroupTutorials.TutorialEnrolment.NotFound);
        }

        StudentRemoval = new()
        {
            Id = enrolmentId,
            Name = enrolmentRecord.Name
        };

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveStudent()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditGroupTutorials);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        DateOnly? effectiveDate = (!StudentRemoval.Immediate) ? DateOnly.FromDateTime(StudentRemoval.EffectiveOn) : null;

        var result = await _mediator.Send(new RemoveStudentFromTutorialCommand(GroupTutorialId.FromValue(Id), TutorialEnrolmentId.FromValue(StudentRemoval.Id), effectiveDate));

        StudentRemoval = null;

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Details", new { area = "Staff", Id = Id });
    }

    public async Task<IActionResult> OnPostCreateRoll()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, Id, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        var result = await _mediator.Send(new CreateRollCommand(GroupTutorialId.FromValue(Id), DateOnly.FromDateTime(RollCreate.RollDate)));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        RollCreate = new();

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Details", new { area = "Staff", Id });
    }

    private async Task GetPageInformation(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTutorialWithDetailsByIdQuery(GroupTutorialId.FromValue(Id)), cancellationToken);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Index", values: new { area = "Staff" }));

            Tutorial = new(
                GroupTutorialId.FromValue(Id),
                null,
                DateOnly.MinValue,
                DateOnly.MinValue,
                new List<TutorialTeacherResponse>(),
                new List<TutorialEnrolmentResponse>(),
                new List<TutorialRollResponse>());
        }
        else
        {
            Tutorial = result.Value;
        }
    }

    private IActionResult ShowError(Error error)
    {
        ModalContent = new ErrorDisplay(
            error,
            _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Details", values: new { area = "Staff", Id = Id }));

        Tutorial = new(
            GroupTutorialId.FromValue(Id),
            null,
            DateOnly.MinValue,
            DateOnly.MinValue,
            new List<TutorialTeacherResponse>(),
            new List<TutorialEnrolmentResponse>(),
            new List<TutorialRollResponse>());

        TeacherRemoval = null;
        StudentRemoval = null;

        return Page();
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        var fileDto = await _mediator.Send(new GenerateTutorialAttendanceReportQuery(GroupTutorialId.FromValue(Id)));

        if (fileDto.IsFailure)
        {
            ShowError(fileDto.Error);
        }

        return File(fileDto.Value.FileData, fileDto.Value.FileType, fileDto.Value.FileName);
    }

    public sealed class TutorialStudentRemovalSelection
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Immediate { get; set; } = true;
        public DateTime EffectiveOn { get; set; } = DateTime.Today;
    }

    public sealed class TutorialTeacherRemovalSelection
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Immediate { get; set; } = true;
        public DateTime EffectiveOn { get; set; } = DateTime.Today;
    }
}
