#nullable enable
namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Application.Features.Home.Queries;
using Constellation.Application.Interfaces.Repositories;
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

    public async Task GetClasses(IUnitOfWork unitOfWork)
    {
        var username = User.Identity?.Name;

        if (username is null)
        {
            return;
        }

        var teacher = await unitOfWork.Staff.FromEmailForExistCheck(username);

        if (teacher != null)
        {
            var entries = unitOfWork.CourseOfferings.AllForTeacher(teacher.StaffId);

            foreach (var entry in entries)
            {
                Classes.Add(entry.Name, entry.Id);
            }
        }
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
}
