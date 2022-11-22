using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

        foreach (var record in data.Completions)
        {
            record.ExpiryCountdown = record.CalculateExpiry();
            record.Status = CompletionRecordDto.ExpiryStatus.Active;

            if (data.Completions.Any(other =>
                other.Id != record.Id &&
                other.ModuleId == record.ModuleId && // true
                other.StaffId == record.StaffId && // true
                ((other.NotRequired && other.CreatedAt > record.CompletedDate) || // false
                (!other.NotRequired && !record.NotRequired && other.CompletedDate > record.CompletedDate) || // false
                (record.NotRequired && record.CreatedAt < other.CompletedDate)))) // false
            {
                record.Status = CompletionRecordDto.ExpiryStatus.Superceded;
            }
        }

        // Remove superceded completion records
        data.Completions = data.Completions.Where(record => record.Status != CompletionRecordDto.ExpiryStatus.Superceded).OrderBy(record => record.StaffLastName).ToList();

        var currentStaff = await _context.Staff
            .AsNoTracking()
            .Where(staff => !staff.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var staff in currentStaff)
        {
            if (data.Completions.Any(record => record.StaffId == staff.StaffId))
                continue;

            var record = new CompletionRecordDto
            {
                StaffId = staff.StaffId,
                StaffFirstName = staff.FirstName,
                StaffLastName = staff.LastName,
                StaffFaculty = String.Join(",", staff.Faculties.Where(member => !member.IsDeleted).Select(member => member.Faculty.Name)),
                CompletedDate = DateTime.MinValue
            };

            data.Completions.Add(record);
        }

        data.Completions = data.Completions.OrderBy(record => record.StaffLastName).ToList();

        // Generate CSV/XLSX file
        var fileData = await _excelService.CreateTrainingModuleReportFile(data);

        // Wrap data in return object
        var reportDto = new ModuleReportDto
        {
            FileData = fileData.ToArray(),
            FileName = $"Mandatory Training Report - {data.Name}.xlsx",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        // Return for download
        return reportDto;
    }
}
