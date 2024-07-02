namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ScienceRolls;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.SciencePracs.GetLessonRollDetailsForSchoolsPortal;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.ModelBinders;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class RollModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public RollModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.ScienceRolls;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public SciencePracLessonId LessonId { get; set; }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public SciencePracRollId RollId { get; set; }


    public ScienceLessonRollDetails MarkedRoll { get; set; }
    public bool Editable { get; set; }

    public async Task OnGet()
    {
        Result<ScienceLessonRollDetails> request = await _mediator.Send(new GetLessonRollDetailsForSchoolsPortalQuery(LessonId, RollId));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        MarkedRoll = request.Value;

        if (MarkedRoll.LessonDate > DateTime.Today.AddDays(-14))
        {
            Editable = true;
        }
    }

}