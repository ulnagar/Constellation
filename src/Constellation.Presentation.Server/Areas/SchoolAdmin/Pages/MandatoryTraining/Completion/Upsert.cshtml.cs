namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models;
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
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService _authorizationService;

    public UpsertModel(IMediator mediator, IDateTimeProvider dateTimeProvider,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "You must select a staff member")]
    public string StaffId { get; set; }

    [BindProperty, DataType(DataType.Date)]
    public DateTime CompletedDate { get; set; } = DateTime.Today;

    [BindProperty]
    [Required(ErrorMessage = "You must select a training module")]
    public Guid? TrainingModuleId { get; set; }

    [AllowExtensions(FileExtensions: "pdf", ErrorMessage = "You can only upload PDF files")]
    [BindProperty]
    public IFormFile FormFile { get; set; }

    public Dictionary<string, string> StaffOptions { get; set; } = new();
    public Dictionary<Guid, string> ModuleOptions { get; set; } = new();
    public KeyValuePair<string, string> SoloStaffMember { get; set; } = new();

    public bool CanEditRecords { get; set; }
    public CompletionRecordCertificateDto UploadedCertificate { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        await GetClasses(_mediator);

        // Is this user a staff member created a record for themselves?
        await SetUpForm();

        var staffIdClaim = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields

            var entity = await _mediator.Send(new GetCompletionRecordEditContextQuery { Id = Id.Value });

            //TODO: Check that the return value is not null
            // If it is, redirect and show error message?

            StaffId = entity.StaffId;
            CompletedDate = entity.CompletedDate;
            TrainingModuleId = entity.TrainingModuleId;

            UploadedCertificate = await _mediator.Send(new GetUploadedTrainingCertificateMetadataQuery { LinkType = StoredFile.TrainingCertificate, LinkId = Id.Value.ToString() });

            if (!CanEditRecords && StaffId != staffIdClaim)
            {
                // User is not the staff member listed on the record and does not have permission to edit records
                return RedirectToPage("Index");
            }
        }
        else if (!CanEditRecords)
        {
            if (SoloStaffMember.Key == null)
            {
                // This staff member is not on the list of staff. Something has gone wrong here.

                return RedirectToPage("Index");
            }

            StaffId = staffIdClaim;
        }

        
        return Page();
    }

    private async Task SetUpForm()
    {
        var canEditTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);
        CanEditRecords = canEditTest.Succeeded;

        var staffIdClaim = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        StaffOptions = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        ModuleOptions = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());

        if (!CanEditRecords)
        {
            SoloStaffMember = StaffOptions.FirstOrDefault(member => member.Key == staffIdClaim);

            StaffId = staffIdClaim;
        }
    }


    private async Task<FileDto> GetUploadedFile()
    {
        if (FormFile is not null)
        {
            var staffMember = await _mediator.Send(new GetStaffMemberNameByIdQuery { StaffId = StaffId });
            var trainingModule = await _mediator.Send(new GetTrainingModuleEditContextQuery { Id = TrainingModuleId.Value });

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
                // Error uploading file
            }

            return file;
        }

        return null;
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            await SetUpForm();

            if (!CanEditRecords)
            {
                if (SoloStaffMember.Key == null)
                {
                    // This staff member is not on the list of staff. Something has gone wrong here.

                    return RedirectToPage("Index");
                }
            }

            return Page();
        }

        if (Id.HasValue)
        {
            // Update existing entry

            var command = new UpdateTrainingCompletionCommand
            {
                Id = Id.Value,
                StaffId = StaffId,
                TrainingModuleId = TrainingModuleId.Value,
                CompletedDate = CompletedDate,
                ModifiedBy = Request.HttpContext.User.Identity?.Name,
                ModifiedAt = _dateTimeProvider.Now,
                File = await GetUploadedFile()
            };

            await _mediator.Send(command);
        }
        else
        {
            // Create new entry

            var command = new CreateTrainingCompletionCommand
            {
                StaffId = StaffId,
                TrainingModuleId = TrainingModuleId.Value,
                CompletedDate = CompletedDate,
                CreatedBy = Request.HttpContext.User.Identity?.Name,
                CreatedAt = _dateTimeProvider.Now,
                File = await GetUploadedFile()
            };

            await _mediator.Send(command);
        }

        return RedirectToPage("Index");
    }
}
