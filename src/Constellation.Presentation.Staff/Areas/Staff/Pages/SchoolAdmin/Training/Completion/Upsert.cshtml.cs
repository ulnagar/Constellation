namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Completion;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Features.Common.Queries;
using Application.Models.Auth;
using Constellation.Application.Training.CreateTrainingCompletion;
using Constellation.Application.Training.GetCompletionRecordEditContext;
using Constellation.Application.Training.GetTrainingModuleEditContext;
using Constellation.Application.Training.GetUploadedTrainingCertificationMetadata;
using Constellation.Application.Training.UpdateTrainingCompletion;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Models.Attachments.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.ModelBinders;
using Presentation.Shared.Pages.Shared.Components.UploadTrainingCompletionCertificate;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Completions;
    [ViewData] public string PageTitle => Id.Equals(TrainingCompletionId.Empty) ? "New Training Completion" : "Edit Training Completion";
    
    // Allow mode switching for:
    // "FULL" - Editor access with no forced fields
    // "SOLOSTAFF" - Insert access with forced staff field
    // "SOLOMODULE" - Insert access with pre-populated module field
    [BindProperty(SupportsGet = true)]
    public CompletionPageMode Mode { get; set; }

    [BindProperty(SupportsGet = true)]
    [Required(ErrorMessage = "You must select a training module")]
    [ModelBinder(typeof(StrongIdBinder))]
    public TrainingModuleId ModuleId { get; set; }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public TrainingCompletionId Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "You must select a staff member")]
    public string SelectedStaffId { get; set; }

    [BindProperty, DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime CompletedDate { get; set; } = DateTime.Today;

    [AllowExtensions(FileExtensions: "pdf", ErrorMessage = "You can only upload PDF files")]
    [BindProperty]
    public IFormFile? FormFile { get; set; }

    public Dictionary<string, string> StaffOptions { get; set; } = new();
    public Dictionary<Guid, string> ModuleOptions { get; set; } = new();
    public KeyValuePair<string, string> SoloStaffMember { get; set; }
    public KeyValuePair<Guid, string> SoloModule { get; set; }

    public bool CanEditRecords { get; set; }
    public CompletionRecordCertificateDto UploadedCertificate { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        string? staffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        AuthorizationResult canEditTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);
        CanEditRecords = canEditTest.Succeeded;

        // Does the current user have permissions for the selected mode?
        if (Mode == CompletionPageMode.Full && !CanEditRecords)
        {
            // Editor mode selected without edit access
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Index", values: new { area = "Staff" })
            };

            return Page();
        } 
        
        if (Mode == CompletionPageMode.SoloModule && !CanEditRecords)
        {
            // Editor insert mode selected without edit access
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Details", values: new { area = "Staff", Id = ModuleId })
            };

            return Page();
        }

        if (!Id.Equals(TrainingCompletionId.Empty))
        {
            // Get existing entry from database and populate fields
            Result<CompletionRecordEditContextDto> entityRequest = await _mediator.Send(new GetCompletionRecordEditContextQuery(ModuleId, Id));
            
            if (entityRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = entityRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Details", values: new { area = "Staff", Id = ModuleId.Value })
                };

                return Page();
            }

            CompletionRecordEditContextDto entity = entityRequest.Value;

            SelectedStaffId = entity.StaffId;
            CompletedDate = entity.CompletedDate;
            ModuleId = entity.TrainingModuleId;

            Result<CompletionRecordCertificateDto> certificateRequest = await _mediator.Send(new GetUploadedTrainingCertificateMetadataQuery(AttachmentType.TrainingCertificate, Id.Value.ToString()));

            if (certificateRequest.IsSuccess)
            {
                UploadedCertificate = certificateRequest.Value;
            }

            if (!CanEditRecords && staffId != SelectedStaffId)
            {
                // User is not the staff member listed on the record and does not have permission to edit records
                Error = new ErrorDisplay
                {
                    Error = DomainErrors.Permissions.Unauthorised,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Index", values: new { area = "Staff" })
                };

                return Page();
            }
        }

        await SetUpForm();

        if (!Id.Equals(TrainingCompletionId.Empty) && !CanEditRecords && SoloStaffMember.Key == string.Empty)
        {
            // This staff member is not on the list of staff. Something has gone wrong here.

            return RedirectToPage("Index");
        }

        return Page();
    }

    private async Task SetUpForm()
    {
        string? staffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        StaffOptions = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        ModuleOptions = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());

        // Insert only mode allowing staff to create new records for themselves only
        if (Mode == CompletionPageMode.SoloStaff)
        {
            SoloStaffMember = StaffOptions.FirstOrDefault(member => member.Key == staffId);
            SelectedStaffId = staffId;
        }

        // Insert only mode allowing editors to pre-select the module
        if (Mode == CompletionPageMode.SoloModule)
        {
            SoloModule = ModuleOptions.FirstOrDefault(member => member.Key == ModuleId.Value);
            ModuleId = TrainingModuleId.FromValue(SoloModule.Key);
        }

        // Edit only mode allowing staff to upload certificate for existing records
        if (Mode == CompletionPageMode.CertUpload)
        {
            SoloStaffMember = StaffOptions.FirstOrDefault(member => member.Key == staffId);
            SoloModule = ModuleOptions.FirstOrDefault(member => member.Key == ModuleId.Value);
        }
    }

    private async Task<FileDto> GetUploadedFile()
    {
        if (FormFile is not null)
        {
            string staffMember = await _mediator.Send(new GetStaffMemberNameByIdQuery { StaffId = SelectedStaffId });

            Result<ModuleEditContextDto> moduleRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(ModuleId));
            
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
        DateOnly completedDate = DateOnly.FromDateTime(CompletedDate);

        if (!ModelState.IsValid)
        {
            await SetUpForm();

            return Page();
        }

        if (!Id.Equals(TrainingCompletionId.Empty))
        {
            // Update existing entry

            UpdateTrainingCompletionCommand command = new(
                Id,
                SelectedStaffId,
                ModuleId,
                completedDate,
                await GetUploadedFile());
            
            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Index", values: new { area = "Staff" })
                };

                return Page();
            }
        }
        else
        {
            // Create new entry

            CreateTrainingCompletionCommand command = new(
                SelectedStaffId,
                ModuleId,
                completedDate,
                await GetUploadedFile());

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Index", values: new { area = "Staff" })
                };

                return Page();
            }
        }

        if (Mode == CompletionPageMode.SoloModule)
        {
            return RedirectToPage("/SchoolAdmin/Training/Modules/Details", new { area = "Staff", Id = ModuleId });
        }

        return RedirectToPage("Index");
    }


}
