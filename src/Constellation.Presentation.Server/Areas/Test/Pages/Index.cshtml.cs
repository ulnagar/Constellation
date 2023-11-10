namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs;
using Application.Interfaces.Gateways;
using Application.Interfaces.Services;
using BaseModels;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ISentralGateway _sentralGateway;
    private readonly IExcelService _excelService;

    public IndexModel(
        ISender mediator,
        ISentralGateway sentralGateway,
        IExcelService excelService)
    {
        _mediator = mediator;
        _sentralGateway = sentralGateway;
        _excelService = excelService;
    }

    public List<SentralIncidentDetails> Data { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);


    }

    public async Task OnPost()
    {
        Stream file = await _sentralGateway.GetNAwardReport();

        Data = await _excelService.ConvertSentralIncidentReport(file);
    }
}