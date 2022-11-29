﻿namespace Constellation.Application.Features.MandatoryTraining.Commands;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GenerateStaffReportCommand(string StaffId) : IRequest<ReportDto> { }

public class GenerateStaffReportCommandHandler : IRequestHandler<GenerateStaffReportCommand, ReportDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExcelService _excelService;

    public GenerateStaffReportCommandHandler(IAppDbContext context, IMapper mapper, IExcelService excelService)
    {
        _context = context;
        _mapper = mapper;
        _excelService = excelService;
    }

    public async Task<ReportDto> Handle(GenerateStaffReportCommand request, CancellationToken cancellationToken)
    {
        var data = new StaffCompletionListDto();

        // - Get all modules
        var modules = await _context.MandatoryTraining.Modules
            .Where(module => !module.IsDeleted)
            .ToListAsync(cancellationToken);

        // - Get all staff
        var staff = await _context.Staff
            .Include(staff => staff.Faculties)
            .ThenInclude(member => member.Faculty)
            .ThenInclude(faculty => faculty.Members)
            .ThenInclude(member => member.Staff)
            .Include(staff => staff.School)
            .ThenInclude(school => school.StaffAssignments)
            .ThenInclude(role => role.SchoolContact)
            .Where(staff => !staff.IsDeleted && staff.StaffId == request.StaffId)
            .ToListAsync(cancellationToken);

        // - Get all completions
        var records = await _context.MandatoryTraining.CompletionRecords
            .Where(record => !record.IsDeleted && record.StaffId == request.StaffId)
            .ToListAsync(cancellationToken);

        data.StaffId = request.StaffId;
        data.Name = staff.First().DisplayName;
        data.SchoolName = staff.First().School.Name;
        data.EmailAddress = staff.First().EmailAddress;
        data.Faculties = staff.First().Faculties.Where(member => !member.IsDeleted).Select(member => member.Faculty.Name).ToList();

        // - Build a collection of completions by each staff member for each module
        foreach (var record in records)
        {
            var entry = new CompletionRecordExtendedDetailsDto();
            entry.AddRecordDetails(record);

            entry.AddModuleDetails(record.Module);

            entry.AddStaffDetails(record.Staff);

            data.Modules.Add(entry);
        }

        // - If a staff member has not completed a module, create a blank entry for them
        foreach (var module in modules)
        {
            var staffIds = staff.Select(staff => staff.StaffId).ToList();
            var staffRecords = data.Modules.Where(record => record.ModuleId == module.Id).Select(record => record.StaffId).ToList();

            var missingStaffIds = staffIds.Except(staffRecords).ToList();

            foreach (var staffId in missingStaffIds)
            {
                var entry = new CompletionRecordExtendedDetailsDto();
                entry.AddModuleDetails(module);

                var staffMember = staff.First(member => member.StaffId == staffId);
                entry.AddStaffDetails(staffMember);

                entry.RecordEffectiveDate = null;
                entry.RecordNotRequired = false;

                data.Modules.Add(entry);
            }
        }

        // - Remove all superceded entries
        foreach (var record in data.Modules)
        {
            var duplicates = data.Modules.Where(entry =>
                    entry.ModuleId == record.ModuleId &&
                    entry.StaffId == record.StaffId &&
                    entry.RecordId != record.RecordId)
                .ToList();

            if (duplicates.All(entry => entry.RecordEffectiveDate < record.RecordEffectiveDate))
            {
                record.IsLatest = true;
                record.CalculateExpiry();
            }
        }

        // Generate CSV/XLSX file
        var fileData = await _excelService.CreateTrainingModuleStaffReportFile(data);

        // Wrap data in return object
        var reportDto = new ReportDto
        {
            FileData = fileData.ToArray(),
            FileName = $"Mandatory Training Report - {data.Name}.xlsx",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        // Return for download
        return reportDto;
    }
}