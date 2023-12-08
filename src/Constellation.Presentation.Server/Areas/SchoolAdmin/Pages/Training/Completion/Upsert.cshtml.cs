namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Completion;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.CreateTrainingCompletion;
using Constellation.Application.Training.Modules.GetCompletionRecordEditContext;
using Constellation.Application.Training.Modules.GetTrainingModuleEditContext;
using Constellation.Application.Training.Modules.GetUploadedTrainingCertificationMetadata;
using Constellation.Application.Training.Modules.UpdateTrainingCompletion;
using Constellation.Core.Errors;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Validation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
[RequestSizeLimit(10485760)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        IMediator mediator, 
        IAuthorizationService authorizationService,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _linkGenerator = linkGenerator;
    }

    // Allow mode switching for:
    // "FULL" - Editor access with no forced fields
    // "SOLOSTAFF" - Insert access with forced staff field
    // "SOLOMODULE" - Insert access with pre-populated module field
    [BindProperty(SupportsGet = true)]
    public ModeOptions Mode { get; set; }

    [BindProperty(SupportsGet = true)]
    [Required(ErrorMessage = "You must select a training module")]
    // Must be nullable to have the default value be null, and therefore trigger required validation rule
    public Guid? ModuleId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "You must select a staff member")]
    public string SelectedStaffId { get; set; }

    [BindProperty, DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime CompletedDate { get; set; } = DateTime.Today;

    [AllowExtensions(FileExtensions: "pdf", ErrorMessage = "You can only upload PDF files")]
    [BindProperty]
    public IFormFile FormFile { get; set; }

    public Dictionary<string, string> StaffOptions { get; set; } = new();
    public Dictionary<Guid, string> ModuleOptions { get; set; } = new();
    public KeyValuePair<string, string> SoloStaffMember { get; set; } = new();
    public KeyValuePair<Guid, string> SoloModule { get; set; } = new();

    public bool CanEditRecords { get; set; }
    public CompletionRecordCertificateDto UploadedCertificate { get; set; } = new();

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Completions;
    [ViewData] public string StaffId { get; set; }

    public async Task<IActionResult> OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        AuthorizationResult canEditTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);
        CanEditRecords = canEditTest.Succeeded;

        // Does the current user have permissons for the selected mode?
        if (Mode == ModeOptions.Full && !CanEditRecords)
        {
            // Editor mode selected without edit access
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        } 
        else if (Mode == ModeOptions.SoloModule && !CanEditRecords)
        {
            // Editor insert mode selected without edit access
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Modules/Details", values: new { area = "SchoolAdmin", Id = ModuleId.Value })
            };

            return Page();
        }

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields
            Result<CompletionRecordEditContextDto> entityRequest = await _mediator.Send(new GetCompletionRecordEditContextQuery(TrainingModuleId.FromValue(ModuleId.Value), TrainingCompletionId.FromValue(Id.Value)));
            
            if (entityRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = entityRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Training/Modules/Details", values: new { area = "SchoolAdmin", Id = ModuleId.Value })
                };

                return Page();
            }

            CompletionRecordEditContextDto entity = entityRequest.Value;

            SelectedStaffId = entity.StaffId;
            CompletedDate = entity.CompletedDate;
            ModuleId = entity.TrainingModuleId.Value;

            Result<CompletionRecordCertificateDto> certificateRequest = await _mediator.Send(new GetUploadedTrainingCertificateMetadataQuery(AttachmentType.TrainingCertificate, Id.Value.ToString()));

            if (certificateRequest.IsSuccess)
            {
                UploadedCertificate = certificateRequest.Value;
            }

            if (!CanEditRecords && StaffId != SelectedStaffId)
            {
                // User is not the staff member listed on the record and does not have permission to edit records
                Error = new ErrorDisplay
                {
                    Error = DomainErrors.Permissions.Unauthorised,
                    RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Index", values: new { area = "SchoolAdmin" })
                };

                return Page();
            }
        }

        await SetUpForm();

        if (!Id.HasValue && !CanEditRecords && SoloStaffMember.Key == null)
        {
            // This staff member is not on the list of staff. Something has gone wrong here.

            return RedirectToPage("Index");
        }

        return Page();
    }

    private async Task SetUpForm()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        StaffOptions = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        ModuleOptions = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());

        // Insert only mode allowing staff to create new records for themselves only
        if (Mode == ModeOptions.SoloStaff)
        {
            SoloStaffMember = StaffOptions.FirstOrDefault(member => member.Key == StaffId);
            SelectedStaffId = StaffId;
        }

        // Insert only mode allowing editors to pre-select the module
        if (Mode == ModeOptions.SoloModule)
        {
            SoloModule = ModuleOptions.FirstOrDefault(member => member.Key == ModuleId.Value);
            ModuleId = SoloModule.Key;
        }

        // Edit only mode allowing staff to upload certificate for existing records
        if (Mode == ModeOptions.CertUpload)
        {
            SoloStaffMember = StaffOptions.FirstOrDefault(member => member.Key == StaffId);
            SoloModule = ModuleOptions.FirstOrDefault(member => member.Key == ModuleId.Value);
        }
    }

    private async Task<FileDto> GetUploadedFile()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (FormFile is not null)
        {
            string staffMember = await _mediator.Send(new GetStaffMemberNameByIdQuery { StaffId = SelectedStaffId });

            Result<ModuleEditContextDto> moduleRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(TrainingModuleId.FromValue(ModuleId.Value)));
            
            if (moduleRequest.IsFailure)
            {
                return null;
            }

            ModuleEditContextDto trainingModule = moduleRequest.Value;

            FileDto file = new()
            {
                FileName = $"{staffMember} - {CompletedDate:yyyy-MM-dd} - {trainingModule.Name}.pdf",
                FileType = FormFile.ContentType
            };

            try
            {
                await using MemoryStream target = new();
                await FormFile.CopyToAsync(target);
                file.FileData = target.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }

            return file;
        }

        return null;
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        DateOnly completedDate = DateOnly.FromDateTime(CompletedDate);

        if (!ModelState.IsValid)
        {
            await SetUpForm();

            return Page();
        }

        if (Id.HasValue)
        {
            // Update existing entry

            UpdateTrainingCompletionCommand command = new(
                TrainingCompletionId.FromValue(Id.Value),
                SelectedStaffId,
                TrainingModuleId.FromValue(ModuleId.Value),
                completedDate,
                await GetUploadedFile());
            
            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Index", values: new { area = "SchoolAdmin" })
                };

                return Page();
            }
        }
        else
        {
            // Create new entry

            CreateTrainingCompletionCommand command = new(
                SelectedStaffId,
                TrainingModuleId.FromValue(ModuleId.Value),
                completedDate,
                await GetUploadedFile());

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Index", values: new { area = "SchoolAdmin" })
                };

                return Page();
            }
        }

        if (Mode == ModeOptions.SoloModule)
        {
            return RedirectToPage("/Training/Modules/Details", new { Id = ModuleId.Value });
        }

        return RedirectToPage("Index");
    }

    public enum ModeOptions
    {
        Full,
        SoloStaff,
        SoloModule,
        CertUpload
    }
}
