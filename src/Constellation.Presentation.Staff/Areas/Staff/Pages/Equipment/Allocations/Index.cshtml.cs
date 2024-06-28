namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Allocations;

using Application.Assets.GetAllocationList;
using Application.Models.Auth;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Allocations;
    [ViewData] public string PageTitle => "Assets by Allocation";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StringEnumerableBinder))]
    public AllocationType AllocationType { get; set; }
    public List<AllocationListItem> Allocations { get; set; }

    public async Task OnGet()
    {
        AllocationType ??= AllocationType.Student;

        Result<List<AllocationListItem>> result = AllocationType switch
        {
            _ when AllocationType.Equals(AllocationType.Student) => await _mediator.Send(new GetStudentAllocationListQuery()),
            _ when AllocationType.Equals(AllocationType.Staff) => await _mediator.Send(new GetStaffAllocationListQuery()),
            _ when AllocationType.Equals(AllocationType.School) => await _mediator.Send(new GetSchoolAllocationListQuery()),
            _ when AllocationType.Equals(AllocationType.CommunityMember) => await _mediator.Send(new GetCommunityMemberAllocationListQuery()),
            _ => Result.Failure<List<AllocationListItem>>(AllocationErrors.UnknownType)
        };

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return;
        }

        Allocations = result.Value;
    }
}
