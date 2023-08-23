#nullable enable
namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Application.Features.Home.Queries;
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

        Classes = await mediator.Send(new GetUsersClassesQuery { Username = username });
    }

    public async Task GetClasses(ISender mediator)
    {
        var username = User.Identity?.Name;

        if (username is null)
        {
            return;
        }

        Classes = await mediator.Send(new GetUsersClassesQuery { Username = username });
    }
}
