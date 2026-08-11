#nullable enable
namespace Constellation.Presentation.Staff.Areas;

using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

public abstract class BasePageModel : PageModel, IBaseModel
{
    public ModalContent? ModalContent { get; set; }
}

public abstract class PeriodScopedPageModel : BasePageModel
{
    protected readonly ISender _mediator;

    protected PeriodScopedPageModel(
        ISender mediator)
    {
        _mediator = mediator;
    }
    
    [BindProperty(SupportsGet = true)]
    public EnrolmentPeriodId PeriodId { get; set; } = EnrolmentPeriodId.Empty;

    public EnrolmentPeriodResponse Period { get; private set; } = null!;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, 
        PageHandlerExecutionDelegate next)
    {
        if (PeriodId == EnrolmentPeriodId.Empty)
        {
            context.Result = RedirectToPage("/Partner/Enrolments/Periods/Index", new { area = "Staff" });
            return;
        }

        Result<EnrolmentPeriodResponse> result = await _mediator.Send(new GetEnrolmentPeriodByIdQuery(PeriodId));

        if (result.IsFailure)
        {
            context.Result = RedirectToPage("/Partner/Enrolments/Periods/Index", new { area = "Staff" });
            return;
        }

        Period = result.Value;
        await next();
    }
}