using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Commands;

public record GenerateModuleReportCommand : IRequest<ModuleReportDto>
{
    public Guid Id { get; set; }
}

public class GenerateModuleReportCommandHandler : IRequestHandler<GenerateModuleReportCommand, ModuleReportDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExcelService _excelService;

    public GenerateModuleReportCommandHandler(IAppDbContext context, IMapper mapper, IExcelService excelService)
    {
        _context = context;
        _mapper = mapper;
        _excelService = excelService;
    }

    public async Task<ModuleReportDto> Handle(GenerateModuleReportCommand request, CancellationToken cancellationToken)
    {
        // Get info from database
        var data = await _context.MandatoryTraining.Modules
            .ProjectTo<ModuleDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(module => module.Id == request.Id, cancellationToken);

        // Generate CSV/XLSX file
        var fileData = await _excelService.CreateTrainingModuleReportFile(data);

        // Wrap data in return object
        var reportDto = new ModuleReportDto
        {
            FileData = fileData.ToArray(),
            FileName = $"Mandatory Training Report - {data.Name}",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        // Return for download
        return reportDto;
    }
}
