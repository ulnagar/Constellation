namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Models.Auth;
using Application.Schools.GetSchoolById;
using Application.Schools.GetSchoolForEdit;
using Application.Schools.UpsertSchool;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditSchools)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;

    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; } = null;

    [BindProperty] 
    public string SchoolCode { get; set; } = string.Empty;

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Address { get; set; } = string.Empty;

    [BindProperty]
    public string Town { get; set; } = string.Empty;

    [BindProperty]
    public string State { get; set; } = string.Empty;

    [BindProperty]
    public string PostCode { get; set; } = string.Empty;

    [BindProperty]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    public string? FaxNumber { get; set; } = string.Empty;

    [BindProperty]
    public string EmailAddress { get; set; } = string.Empty;

    [BindProperty]
    public string? Division { get; set; } = string.Empty;

    [BindProperty]
    public bool HeatSchool { get; set; }

    [BindProperty]
    public string? Electorate { get; set; } = string.Empty;

    [BindProperty]
    public string? PrincipalNetwork { get; set; } = string.Empty;

    public async Task OnGet()
    {
        if (Id is null)
            return;

        Result<SchoolEditResponse> school = await _mediator.Send(new GetSchoolForEditQuery(Id));

        if (school.IsFailure)
        {
            Error = new()
            {
                Error = school.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Schools/Index", values: new { area = "Staff" })
            };

            return;
        }

        SchoolCode = school.Value.SchoolCode;
        Name = school.Value.Name;
        Address = school.Value.Address;
        Town = school.Value.Town;
        State = school.Value.State;
        PostCode = school.Value.PostCode;
        PhoneNumber = school.Value.PhoneNumber;
        FaxNumber = school.Value.FaxNumber;
        EmailAddress = school.Value.EmailAddress;
        Division = school.Value.Division;
        HeatSchool = school.Value.HeatSchool;
        Electorate = school.Value.Electorate;
        PrincipalNetwork = school.Value.PrincipalNetwork;
    }

    public async Task<IActionResult> OnPost()
    {
        string phoneNumber = PhoneNumber
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace(" ", string.Empty)
            .Trim();

        string faxNumber = FaxNumber?
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace(" ", string.Empty)
            .Trim()
            ?? string.Empty;

        UpsertSchoolCommand command = new()
        {
            Code = SchoolCode,
            Name = Name,
            Address = Address,
            Town = Town,
            State = State,
            PostCode = PostCode,
            PhoneNumber = phoneNumber,
            FaxNumber = faxNumber,
            EmailAddress = EmailAddress,
            Division = Division,
            LateOpening = HeatSchool,
            Electorate = Electorate,
            PrincipalNetwork = PrincipalNetwork
        };

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/Partner/Schools/Index", new { area = "Staff" });
    }
}