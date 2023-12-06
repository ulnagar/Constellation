﻿namespace Constellation.Application.Training.Modules.GenerateModuleReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Models;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Faculty.Identifiers;
using Core.Models.Training.Repositories;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateModuleReportCommandHandler
    : ICommandHandler<GenerateModuleReportCommand, ReportDto>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;

    public GenerateModuleReportCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IExcelService excelService,
        IDateTimeProvider dateTime)
    {
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _excelService = excelService;
        _dateTime = dateTime;
    }

    public async Task<Result<ReportDto>> Handle(GenerateModuleReportCommand request, CancellationToken cancellationToken)
    {
        ModuleDetailsDto data = new();

        // Get info from database
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        data.Id = module.Id;
        data.Name = module.Name;
        data.Expiry = module.Expiry.GetDisplayName();
        data.Url = string.IsNullOrWhiteSpace(module.Url) ? string.Empty : module.Url;
        data.IsActive = !module.IsDeleted;

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        //TODO: (R1.14) Add a check to see if each staff member is required to complete this module

        foreach (Staff staffMember in staffMembers)
        {
            List<FacultyId> facultyIds = staffMember
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            List<Faculty> faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            List<TrainingCompletion> records = module.Completions
                    .Where(record =>
                        record.StaffId == staffMember.StaffId &&
                        !record.IsDeleted)
                    .ToList();

            TrainingCompletion record = records.MaxBy(record => record.CompletedDate);

            if (record is null)
            {
                CompletionRecordDto entry = new()
                {
                    StaffId = staffMember.StaffId,
                    StaffFirstName = staffMember.FirstName,
                    StaffLastName = staffMember.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = null,
                    ExpiryCountdown = -9999
                };

                data.Completions.Add(entry);
            }
            else
            {
                CompletionRecordDto entry = new()
                {
                    Id = record.Id,
                    ModuleId = record.TrainingModuleId,
                    ModuleName = module.Name,
                    ModuleExpiry = module.Expiry,
                    StaffId = record.StaffId,
                    StaffFirstName = staffMember.FirstName,
                    StaffLastName = staffMember.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = record.CompletedDate,
                    CreatedAt = record.CreatedAt
                };

                entry.ExpiryCountdown = entry.CalculateExpiry(_dateTime);
                entry.Status = CompletionRecordDto.ExpiryStatus.Active;

                data.Completions.Add(entry);
            }
        }

        data.Completions = data.Completions.OrderBy(record => record.StaffLastName).ToList();

        // Generate CSV/XLSX file
        MemoryStream fileData = await _excelService.CreateTrainingModuleReportFile(data);

        if (!request.IncludeCertificates)
        {
            // Wrap data in return object
            ReportDto reportDto = new()
            {
                FileData = fileData.ToArray(),
                FileName = $"Mandatory Training Report - {data.Name}.xlsx",
                FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            // Return for download
            return reportDto;
        }

        List<AttachmentResponse> fileList = new();

        fileList.Add(new(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Mandatory Training Report - {data.Name}.xlsx",
            fileData.ToArray()));

        List<string> recordIds = data
            .Completions
            .Where(record =>
                record.ExpiryCountdown > 0 &&
                !record.NotRequired)
            .Select(record => record.Id.ToString())
            .ToList();

        foreach (string recordId in recordIds)
        {
            Result<AttachmentResponse> attempt = await _attachmentService.GetAttachmentFile(AttachmentType.TrainingCertificate, recordId, cancellationToken);

            if (attempt.IsFailure)
            {
                continue;
            }

            fileList.Add(attempt.Value);
        }

        // Create ZIP file
        using MemoryStream memoryStream = new MemoryStream();
        using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
        {
            foreach (AttachmentResponse file in fileList)
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.FileName);
                await using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
                await streamWriter.BaseStream.WriteAsync(file.FileData, 0, file.FileData.Length, cancellationToken);
            }
        }

        ReportDto zipFile = new()
        {
            FileData = memoryStream.ToArray(),
            FileName = $"Mandatory Training Export - {data.Name}.zip",
            FileType = MediaTypeNames.Application.Zip
        };

        return zipFile;
    }
}