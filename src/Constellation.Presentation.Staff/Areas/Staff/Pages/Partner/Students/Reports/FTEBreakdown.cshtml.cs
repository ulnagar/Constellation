namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Models.Auth;
using Constellation.Application.Enrolments.GetFTETotalByGrade;
using Constellation.Core.Shared;
using Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class FTEBreakdownModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public FTEBreakdownModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;

    public List<GradeEntry> Grades { get; set; } = new();
    public int TotalMaleEnrolments { get; set; }
    public decimal TotalMaleEnrolmentFTE { get; set; }
    public int TotalFemaleEnrolments { get; set; }
    public decimal TotalFemaleEnrolmentFTE { get; set; }
    public int TotalEnrolments { get; set; }
    public decimal TotalEnrolmentFTE { get; set; }

    public class GradeEntry
    {
        public string Grade { get; set; }
        public int MaleEnrolments { get; set; }
        public decimal MaleEnrolmentFTE { get; set; }
        public int FemaleEnrolments { get; set; }
        public decimal FemaleEnrolmentFTE { get; set; }

        public int TotalEnrolments => MaleEnrolments + FemaleEnrolments;
        public decimal TotalEnrolmentFTE => MaleEnrolmentFTE + FemaleEnrolmentFTE;
    }

    public async Task OnGet()
    {
        Result<List<GradeFTESummaryResponse>> request = await _mediator.Send(new GetFTETotalByGradeQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Reports/Index", values: new { area = "Staff" })
            };

            return;
        }

        foreach (GradeFTESummaryResponse entry in request.Value)
        {
            Grades.Add(new()
            {
                Grade = entry.Grade.AsName(),
                MaleEnrolments = entry.MaleEnrolments,
                MaleEnrolmentFTE = entry.MaleEnrolmentFTE,
                FemaleEnrolments = entry.FemaleEnrolments,
                FemaleEnrolmentFTE = entry.FemaleEnrolmentFTE
            });
        }

        TotalMaleEnrolments = request.Value.Sum(grade => grade.MaleEnrolments);
        TotalMaleEnrolmentFTE = request.Value.Sum(grade => grade.MaleEnrolmentFTE);
        TotalFemaleEnrolments = request.Value.Sum(grade => grade.FemaleEnrolments);
        TotalFemaleEnrolmentFTE = request.Value.Sum(grade => grade.FemaleEnrolmentFTE);

        TotalEnrolments = request.Value.Sum(grade => grade.TotalEnrolments);
        TotalEnrolmentFTE = request.Value.Sum(grade => grade.TotalEnrolmentFTE);
    }
}