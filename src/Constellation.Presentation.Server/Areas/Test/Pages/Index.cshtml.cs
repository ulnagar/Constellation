namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Absences.ExportUnexplainedPartialAbsencesReport;
using BaseModels;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICaseRepository _repository;

    public IndexModel(
        IMediator mediator,
        ICaseRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    public List<Case> Cases { get; set; }


    public async Task OnGet()
    {
        Cases = await _repository.GetAll();
    }

    public async Task OnGetCreateCase()
    {
        var item = new Case();
    }

}