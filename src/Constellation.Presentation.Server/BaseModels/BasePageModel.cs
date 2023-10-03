#nullable enable
namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Core.Models.Offerings.Identifiers;
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
        Dictionary<string, OfferingId> response = new();

        var username = User.Identity?.Name;

        if (username is null)
            return;

        var query = await mediator.Send(new GetCurrentOfferingsForTeacherQuery(null, username));

        if (query.IsFailure)
            return;

        foreach (var entry in query.Value)
        {
            response.Add(entry.OfferingName, entry.OfferingId);
        }

        Classes = response;
    }

    public async Task GetClasses(ISender mediator)
    {
        Dictionary<string, OfferingId> response = new();

        var username = User.Identity?.Name;

        if (username is null)
            return;

        var query = await mediator.Send(new GetCurrentOfferingsForTeacherQuery(null, username));

        if (query.IsFailure)
            return;

        foreach (var entry in query.Value)
        {
            response.Add(entry.OfferingName, entry.OfferingId);
        }

        Classes = response;
    }
}
