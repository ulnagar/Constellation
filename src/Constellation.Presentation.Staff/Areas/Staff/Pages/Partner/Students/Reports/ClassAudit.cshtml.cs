namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Students.GetCurrentStudentsWithCurrentOfferings;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public sealed class ClassAuditModel : BasePageModel
{
    private readonly ISender _mediator;

    public ClassAuditModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;

    public List<StudentWithOfferingsResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<StudentWithOfferingsResponse>> studentRequest = await _mediator.Send(new GetCurrentStudentsWithCurrentOfferingsQuery());

        if (studentRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentRequest.Error);

            return;
        }

        Students = studentRequest.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();
    }
}