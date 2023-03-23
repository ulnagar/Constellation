namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.MandatoryTraining.CreateTrainingCompletion;
using Constellation.Application.MandatoryTraining.DoesModuleAllowNotRequiredResponse;
using Constellation.Application.MandatoryTraining.GetCompletionRecordEditContext;
using Constellation.Application.MandatoryTraining.GetTrainingModuleEditContext;
using Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificationMetadata;
using Constellation.Application.MandatoryTraining.UpdateTrainingCompletion;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
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
    public string StaffId { get; set; }

    [BindProperty, DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime CompletedDate { get; set; } = DateTime.Today;

    [BindProperty]
    public bool NotRequired { get; set; }

    [AllowExtensions(FileExtensions: "pdf", ErrorMessage = "You can only upload PDF files")]
    [BindProperty]
    public IFormFile FormFile { get; set; }

    public Dictionary<string, string> StaffOptions { get; set; } = new();
    public Dictionary<Guid, string> ModuleOptions { get; set; } = new();
    public KeyValuePair<string, string> SoloStaffMember { get; set; } = new();
    public KeyValuePair<Guid, string> SoloModule { get; set; } = new();

    public bool CanEditRecords { get; set; }
    public CompletionRecordCertificateDto UploadedCertificate { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        await GetClasses(_mediator);

        var canEditTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);
        CanEditRecords = canEditTest.Succeeded;

        var staffIdClaim = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        // Does the current user have permissons for the selected mode?
        if (Mode == ModeOptions.FULL && !CanEditRecords)
        {
            // Editor mode selected without edit access
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Completion/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        } 
        else if (Mode == ModeOptions.SOLOMODULE && !CanEditRecords)
        {
            // Editor insert mode selected without edit access
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Details", values: new { area = "SchoolAdmin", Id = ModuleId.Value })
            };

            return Page();
        }

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields
            var entityRequest = await _mediator.Send(new GetCompletionRecordEditContextQuery(TrainingCompletionId.FromValue(Id.Value)));
            
            if (entityRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = entityRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Details", values: new { area = "SchoolAdmin", Id = ModuleId.Value })
                };

                return Page();
            }

            var entity = entityRequest.Value;

            StaffId = entity.StaffId;
            CompletedDate = entity.CompletedDate;
            ModuleId = entity.TrainingModuleId.Value;
            NotRequired = entity.NotRequired;

            var certificateRequest = await _mediator.Send(new GetUploadedTrainingCertificateMetadataQuery(StoredFile.TrainingCertificate, Id.Value.ToString()));

            if (certificateRequest.IsSuccess)
            {
                UploadedCertificate = certificateRequest.Value;
            }

            if (!CanEditRecords && StaffId != staffIdClaim)
            {
                // User is not the staff member listed on the record and does not have permission to edit records
                Error = new ErrorDisplay
                {
                    Error = DomainErrors.Permissions.Unauthorised,
                    RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Completion/Index", values: new { area = "SchoolAdmin" })
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
        StaffOptions = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        ModuleOptions = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());

        // Insert only mode allowing staff to create new records for themselves only
        if (Mode == ModeOptions.SOLOSTAFF)
        {
            var staffIdClaim = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;
            SoloStaffMember = StaffOptions.FirstOrDefault(member => member.Key == staffIdClaim);
            StaffId = staffIdClaim;
        }

        // Insert only mode allowing editors to pre-select the module
        if (Mode == ModeOptions.SOLOMODULE)
        {
            SoloModule = ModuleOptions.FirstOrDefault(member => member.Key == ModuleId.Value);
            ModuleId = SoloModule.Key;
        }

        // Edit only mode allowing staff to upload certificate for existing records
        if (Mode == ModeOptions.CERTUPLOAD)
        {
            SoloStaffMember = StaffOptions.FirstOrDefault(member => member.Key == StaffId);
            SoloModule = ModuleOptions.FirstOrDefault(member => member.Key == ModuleId.Value);
        }
    }

    private async Task<FileDto> GetUploadedFile()
    {
        if (FormFile is not null)
        {
            var staffMember = await _mediator.Send(new GetStaffMemberNameByIdQuery { StaffId = StaffId });

            var moduleRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(TrainingModuleId.FromValue(ModuleId.Value)));
            
            if (moduleRequest.IsFailure)
            {
                return null;
            }

            var trainingModule = moduleRequest.Value;

            var file = new FileDto
            {
                FileName = $"{staffMember} - {CompletedDate:yyyy-MM-dd} - {trainingModule.Name}.pdf",
                FileType = FormFile.ContentType
            };

            try
            {
                await using var target = new MemoryStream();
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
        // Check if the Module allows not required if the not required has been selected.
        if (ModuleId is not null && NotRequired)
        {
            var notRequiredRequest = await _mediator.Send(new DoesModuleAllowNotRequiredResponseQuery(TrainingModuleId.FromValue(ModuleId.Value)));

            if (notRequiredRequest.IsFailure || notRequiredRequest.Value == false)
            {
                ModelState.AddModelError("NotRequired", "This Training Module does not allow Not Required responses.");
            }
        }

        if (!ModelState.IsValid)
        {
            await SetUpForm();

            return Page();
        }

        if (Id.HasValue)
        {
            // Update existing entry

            var command = new UpdateTrainingCompletionCommand(
                TrainingCompletionId.FromValue(Id.Value),
                StaffId,
                TrainingModuleId.FromValue(ModuleId.Value),
                CompletedDate,
                NotRequired,
                await GetUploadedFile());
            
            await _mediator.Send(command);
        }
        else
        {
            // Create new entry

            var command = new CreateTrainingCompletionCommand(
                StaffId,
                TrainingModuleId.FromValue(ModuleId.Value),
                CompletedDate,
                NotRequired,
                await GetUploadedFile());

            await _mediator.Send(command);
        }

        if (Mode == ModeOptions.SOLOMODULE)
        {
            return RedirectToPage("/MandatoryTraining/Modules/Details", new { Id = ModuleId.Value });
        }

        return RedirectToPage("Index");
    }

    public enum ModeOptions
    {
        FULL,
        SOLOSTAFF,
        SOLOMODULE,
        CERTUPLOAD
    }
}
