namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContent)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<CompletionRecordDto> CompletionRecords { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        // If user does not have details view permissions, only show their own records
        if (User.HasClaim(claim => claim.Type == AuthClaimType.Permission && claim.Value == AuthPermissions.MandatoryTrainingDetailsView))
        {
            CompletionRecords = await _mediator.Send(new GetListOfCompletionRecordsQuery(""));
        } else
        {
            var staffId = User.FindFirst(AuthClaimType.StaffEmployeeId).Value;
            CompletionRecords = await _mediator.Send(new GetListOfCompletionRecordsQuery(staffId));
        }
    }
}
