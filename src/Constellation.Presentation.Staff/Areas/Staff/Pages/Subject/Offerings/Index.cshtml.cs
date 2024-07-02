namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetAllOfferingSummaries;
using Constellation.Application.Offerings.Models;
using Constellation.Core.Enums;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _sender;

    public IndexModel(
        ISender sender)
    {
        _sender = sender;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;

    [BindProperty(SupportsGet = true)]
    public GetAllOfferingSummariesQuery.FilterEnum Filter { get; set; } = GetAllOfferingSummariesQuery.FilterEnum.Active;
    [BindProperty(SupportsGet = true)]
    public GradeDto SelectedGrade { get; set; } = GradeDto.All;
    [BindProperty(SupportsGet = true)]
    public string SelectedFaculty { get; set; }

    public List<OfferingSummaryResponse> Offerings { get; set; } = new();

    public List<Grade> Grades { get; set; } = new();
    public List<string> Faculties { get; set;} = new();

    public async Task OnGet()
    {
        Result<List<OfferingSummaryResponse>> request = await _sender.Send(new GetAllOfferingSummariesQuery { Filter = Filter });

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        Offerings = request.Value;
        Grades = Offerings.Select(offering => offering.Grade).Distinct().ToList();
        Faculties = Offerings.Select(offering => offering.Faculty).Distinct().ToList();

        Offerings = SelectedGrade switch
        {
            GradeDto.All => Offerings,
            GradeDto g => Offerings.Where(offering => (int)offering.Grade == (int)g).ToList()
        };

        Offerings = SelectedFaculty switch
        {
            string s when string.IsNullOrWhiteSpace(s) => Offerings,
            string f => Offerings.Where(offering => offering.Faculty == f).ToList(),
            _ => Offerings
        };

        Offerings = Offerings.OrderByDescending(offering => offering.EndDate.Year).ThenBy(offering => offering.Name).ToList();
    }

    public enum GradeDto
    {
        All,
        Y05 = 5,
        Y06 = 6,
        Y07 = 7,
        Y08 = 8,
        Y09 = 9,
        Y10 = 10,
        Y11 = 11,
        Y12 = 12
    }
}
