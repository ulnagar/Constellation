namespace Constellation.Presentation.Server.Areas.Test.Pages;
using BaseModels;
using Constellation.Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Repositories;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ITrainingRoleRepository _trainingRepository;

    public IndexModel(
        ISender mediator,
        ITrainingRoleRepository trainingRepository)
    {
        _mediator = mediator;
        _trainingRepository = trainingRepository;
    }

    public List<TrainingRole> Roles { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Roles = await _trainingRepository.GetAllRoles();
    }
}