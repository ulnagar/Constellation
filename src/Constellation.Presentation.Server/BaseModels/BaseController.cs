namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Application.Features.Home.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Subjects.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public BaseController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected async Task<T> CreateViewModel<T>() where T : BaseViewModel, new()
    {
        T viewModel = new();
        await UpdateViewModel(viewModel);

        return viewModel;
    }

    protected async Task UpdateViewModel<T>(T viewModel) where T : BaseViewModel
    {
        viewModel.Classes = await GetClasses();
    }

    protected async Task<IDictionary<string, OfferingId>> GetClasses()
    {
        if (_mediator is not null)
        {
            return await GetClassesWithMediator();
        }

        if (_unitOfWork is not null)
        {
            return await GetClassesWithUoW();
        }

        return new Dictionary<string, OfferingId>();
    }

    private async Task<IDictionary<string, OfferingId>> GetClassesWithMediator()
    {
        var username = User.Identity?.Name;

        if (username is null)
        {
            return new Dictionary<string, OfferingId>();
        }

        return await _mediator.Send(new GetUsersClassesQuery { Username = username });
    }

    private async Task<IDictionary<string, OfferingId>> GetClassesWithUoW()
    {
        var result = new Dictionary<string, OfferingId>();

        var username = User.Identity.Name;
        var teacher = await _unitOfWork.Staff.FromEmailForExistCheck(username);

        if (teacher != null)
        {
            var entries = await _unitOfWork.CourseOfferings.AllForTeacherAsync(teacher.StaffId);

            foreach (var entry in entries)
            {
                result.Add(entry.Name, entry.Id);
            }
        }

        return result;
    }
}