namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    private readonly IMediator _mediator;

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
        Dictionary<string, OfferingId> response = new();

        var username = User.Identity?.Name;

        if (username is null)
            return new Dictionary<string, OfferingId>();

        var query = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(null, username));

        if (query.IsFailure)
            return response;

        foreach (var entry in query.Value)
        {
            response.Add(entry.OfferingName, entry.OfferingId);
        }

        return response;
    }
}