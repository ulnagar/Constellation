namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.MandatoryTrainingEditor)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpsertModel(IMediator mediator, IDateTimeProvider dateTimeProvider)
    {
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }
    [BindProperty]
    public string StaffId { get; set; }
    [BindProperty, DataType(DataType.Date)]
    public DateTime CompletedDate { get; set; } = DateTime.Today;
    [BindProperty]
    public Guid TrainingModuleId { get; set; }

    public Dictionary<string, string> StaffOptions { get; set; } = new();
    public Dictionary<Guid, string> ModuleOptions { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            // Get existing entry from database and populate fields

            var entity = await _mediator.Send(new GetCompletionRecordEditContextQuery { Id = Id.Value });

            //TODO: Check that the return value is not null
            // If it is, redirect and show error message?

            StaffId = entity.StaffId;
            CompletedDate = entity.CompletedDate;
            TrainingModuleId = entity.TrainingModuleId;
        }

        StaffOptions = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        ModuleOptions = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (Id.HasValue)
        {
            // Update existing entry

            var command = new UpdateTrainingCompletionCommand
            {
                Id = Id.Value,
                StaffId = StaffId,
                TrainingModuleId = TrainingModuleId,
                CompletedDate = CompletedDate,
                ModifiedBy = Request.HttpContext.User.Identity?.Name,
                ModifiedAt = _dateTimeProvider.Now,
            };

            await _mediator.Send(command);
        }
        else
        {
            // Create new entry

            var command = new CreateTrainingCompletionCommand
            {
                StaffId = StaffId,
                TrainingModuleId = TrainingModuleId,
                CompletedDate = CompletedDate,
                CreatedBy = Request.HttpContext.User.Identity?.Name,
                CreatedAt = _dateTimeProvider.Now,
            };

            await _mediator.Send(command);
        }

        return RedirectToPage("Index");
    }
}
