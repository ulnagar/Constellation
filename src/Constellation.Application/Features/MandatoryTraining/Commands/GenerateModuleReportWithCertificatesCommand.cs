﻿namespace Constellation.Application.Features.MandatoryTraining.Commands;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public record GenerateModuleReportWithCertificatesCommand : IRequest<ReportDto>
{
    public Guid Id { get; init; }
}

public class GenerateModuleReportWithCertificatesCommandHandler : IRequestHandler<GenerateModuleReportWithCertificatesCommand, ReportDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExcelService _excelService;

    public GenerateModuleReportWithCertificatesCommandHandler(IAppDbContext context, IMapper mapper,
        IExcelService excelService)
    {
        _context = context;
        _mapper = mapper;
        _excelService = excelService;
    }

    public async Task<ReportDto> Handle(GenerateModuleReportWithCertificatesCommand request, CancellationToken cancellationToken)
    {
        var fileList = new Dictionary<string, byte[]>();

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
            .Include(staff => staff.Faculties)
            .ThenInclude(member => member.Faculty)
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

        // Remove completion records for staff who are no longer active
        var recordStaff = data.Completions.Select(record => record.StaffId).Distinct().ToList();
        foreach (var staffId in recordStaff)
        {
            if (currentStaff.All(member => member.StaffId != staffId))
            {
                data.Completions.RemoveAll(record => record.StaffId == staffId);
            }
        }

        data.Completions = data.Completions.OrderBy(record => record.StaffLastName).ToList();

        // Generate CSV/XLSX file
        var fileData = await _excelService.CreateTrainingModuleReportFile(data);

        fileList.Add($"Mandatory Training Report - {data.Name}.xlsx", fileData.ToArray());

        var certificates = await _context.StoredFiles
            .AsNoTracking()
            .Where(storedFile => storedFile.LinkType == StoredFile.TrainingCertificate && data.Completions.Select(record => record.Id.ToString()).ToList().Contains(storedFile.LinkId))
            .ToListAsync(cancellationToken);

        foreach (var certificate in certificates)
        {
            fileList.Add(certificate.Name, certificate.FileData);
        }

        // Create ZIP file
        using var memoryStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
        {
            foreach (var file in fileList)
            {
                var zipArchiveEntry = zipArchive.CreateEntry(file.Key);
                using var streamWriter = new StreamWriter(zipArchiveEntry.Open());
                streamWriter.BaseStream.Write(file.Value, 0, file.Value.Length);
            }
        }

        var zipFile = new ReportDto
        {
            FileData = memoryStream.ToArray(),
            FileName = $"Mandatory Training Export - {data.Name}.zip",
            FileType = MediaTypeNames.Application.Zip
        };

        return zipFile;
    }
}
