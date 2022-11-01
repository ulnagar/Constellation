namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<CompletionRecordDto> CompletionRecords { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        CompletionRecords = await _mediator.Send(new GetListOfCompletionRecordsQuery());
    }
}
