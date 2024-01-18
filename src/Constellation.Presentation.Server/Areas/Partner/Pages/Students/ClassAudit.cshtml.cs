namespace Constellation.Presentation.Server.Areas.Partner.Pages.Students;

using Application.Models.Auth;
using Application.Students.GetCurrentStudentsWithCurrentOfferings;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public sealed class ClassAuditModel : BasePageModel
{
    private readonly ISender _mediator;

    public ClassAuditModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public List<StudentWithOfferingsResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<StudentWithOfferingsResponse>> studentRequest = await _mediator.Send(new GetCurrentStudentsWithCurrentOfferingsQuery());

        if (studentRequest.IsFailure)
        {
            Error = new()
            {
                Error = studentRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Students = studentRequest.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();
    }
}