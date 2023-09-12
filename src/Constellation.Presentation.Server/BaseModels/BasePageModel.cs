#nullable enable
namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BasePageModel : PageModel, IBaseModel
{
    public IDictionary<string, OfferingId> Classes { get; set; }
    public ErrorDisplay? Error { get; set; }

    public BasePageModel()
    {
        Classes = new Dictionary<string, OfferingId>();
    }

    public async Task GetClasses(IMediator mediator)
    {
        var username = User.Identity?.Name;

        if (username is null)
        {
            return;
        }

        var query = await mediator.Send(new GetCurrentOfferingsForTeacherQuery(username));

        if (query.IsFailure)
        {
            return;
        }

        Classes = query.Value;
    }

    public async Task GetClasses(ISender mediator)
    {
        var username = User.Identity?.Name;

        if (username is null)
        {
            return;
        }

        var query = await mediator.Send(new GetCurrentOfferingsForTeacherQuery(username));

        if (query.IsFailure)
        {
            return;
        }

        Classes = query.Value;
    }
}
