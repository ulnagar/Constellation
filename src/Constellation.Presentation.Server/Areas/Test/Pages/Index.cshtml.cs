namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Absences.ExportUnexplainedPartialAbsencesReport;
using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task OnGet() { }

}