﻿#nullable enable
namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.ImportAssetsFromFile;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.StaffMembers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ImportAssetsFromFileCommandHandler
: ICommandHandler<ImportAssetsFromFileCommand, List<ImportAssetStatusDto>>
{
    private readonly IExcelService _excelService;
    private readonly IAssetRepository _assetRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public ImportAssetsFromFileCommandHandler(
        IExcelService excelService,
        IAssetRepository assetRepository,
        ISchoolRepository schoolRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _excelService = excelService;
        _assetRepository = assetRepository;
        _schoolRepository = schoolRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<ImportAssetsFromFileCommand>();
    }

    public async Task<Result<List<ImportAssetStatusDto>>> Handle(ImportAssetsFromFileCommand request, CancellationToken cancellationToken)
    {
        List<ImportAssetStatusDto> response = new();

        List<ImportAssetDto> importedAssets = await _excelService.ImportAssetsFromFile(request.ImportFile, cancellationToken);

        List<School> schools = await _schoolRepository.GetAll(cancellationToken);

        List<StaffMember> staff = await _staffRepository.GetAll(cancellationToken);

        List<Student> students = await _studentRepository.GetAll(cancellationToken);

        foreach (ImportAssetDto importAsset in importedAssets)
        {
            Result<AssetNumber> assetNumber = AssetNumber.Create(importAsset.AssetNumber);

            if (assetNumber.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportAssetDto), importAsset, true)
                    .ForContext(nameof(Error), assetNumber.Error, true)
                    .Warning("Failed to import Asset");

                response.Add(new (importAsset.RowNumber, false, assetNumber.Error));

                continue;
            }

            AssetStatus status = importAsset.Status switch
            {
                "Active" => AssetStatus.Active,
                "Disposed" => AssetStatus.Disposed,
                "Lost" => AssetStatus.Lost,
                "Stolen" => AssetStatus.Stolen,
                "Pending Disposal" => AssetStatus.PendingDisposal,
                _ => AssetStatus.Active
            };

            AssetCategory category = importAsset.Category switch
            {
                "Student" => AssetCategory.Student,
                "Staff" => AssetCategory.Staff,
                _ => AssetCategory.Student
            };

            LocationCategory locationCategory = importAsset.LocationCategory switch
            {
                "Community" or "Private Residence" => LocationCategory.PrivateResidence,
                "Coordinating Office" or "Storage Shed" => LocationCategory.CoordinatingOffice,
                "Corporate Office" or "State Office" => LocationCategory.CorporateOffice,
                "Public School" => LocationCategory.PublicSchool,
                _ => LocationCategory.CoordinatingOffice
            };

            Result<Asset> asset = await Asset.Create(
                assetNumber.Value,
                importAsset.SerialNumber ?? string.Empty,
                importAsset.SapNumber ?? string.Empty,
                importAsset.Manufacturer ?? string.Empty,
                importAsset.ModelNumber ?? string.Empty,
                importAsset.ModelDescription ?? string.Empty,
                category,
                importAsset.PurchaseDate ?? _dateTime.Today,
                string.Empty,
                importAsset.PurchaseCost,
                importAsset.WarrantyEndDate ?? DateOnly.MinValue,
                _assetRepository);

            if (asset.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportAssetDto), importAsset, true)
                    .ForContext(nameof(Error), asset.Error, true)
                    .Warning("Failed to import Asset");

                response.Add(new(importAsset.RowNumber, false, asset.Error));
                
                continue;
            }

            if (!status.Equals(AssetStatus.Active))
            {
                Result statusUpdate = asset.Value.UpdateStatus(status);

                if (statusUpdate.IsFailure)
                {
                    _logger
                        .ForContext(nameof(ImportAssetDto), importAsset, true)
                        .ForContext(nameof(Error), statusUpdate.Error, true)
                        .Warning("Failed to import Asset");

                response.Add(new(importAsset.RowNumber, false, statusUpdate.Error));

                    continue;
                }
            }

            School? locationSchool = schools.FirstOrDefault(entry => entry.Name == importAsset.LocationSite);

            if (locationSchool is null && locationCategory.Equals(LocationCategory.PublicSchool))
            {
                _logger
                    .ForContext(nameof(ImportAssetDto), importAsset, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(""), true)
                    .Warning("Failed to import Asset");

                response.Add(new(importAsset.RowNumber, false, DomainErrors.Partners.School.NotFound("")));

                continue;
            }

            Result<Location> location = locationCategory switch
            {
                _ when locationCategory.Equals(LocationCategory.CoordinatingOffice) =>
                    Location.CreateOfficeLocationRecord(asset.Value.Id, importAsset.LocationRoom ?? string.Empty, true, _dateTime.Today),
                _ when locationCategory.Equals(LocationCategory.CorporateOffice) =>
                    Location.CreateCorporateOfficeLocationRecord(asset.Value.Id, importAsset.LocationSite ?? string.Empty, true, _dateTime.Today),
                _ when locationCategory.Equals(LocationCategory.PrivateResidence) =>
                    Location.CreatePrivateResidenceLocationRecord(asset.Value.Id, true, _dateTime.Today),
                _ when locationCategory.Equals(LocationCategory.PublicSchool) =>
                    Location.CreatePublicSchoolLocationRecord(asset.Value.Id, locationSchool!.Name, locationSchool!.Code, true, _dateTime.Today)
            };

            if (location.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportAssetDto), importAsset, true)
                    .ForContext(nameof(Error), location.Error, true)
                    .Warning("Failed to import Asset");

                response.Add(new(importAsset.RowNumber, false, location.Error));
                
                continue;
            }

            asset.Value.AddLocation(location.Value);

            if (!string.IsNullOrWhiteSpace(importAsset.ResponsibleOfficer))
            {
                School? allocationSchool = schools.FirstOrDefault(entry => entry.Code == importAsset.ResponsibleOfficer);
                StaffMember? staffMember = staff.FirstOrDefault(entry => entry.Id.ToString() == importAsset.ResponsibleOfficer);
                Student? student = students.FirstOrDefault(entry => entry.StudentReferenceNumber.ToString() == importAsset.ResponsibleOfficer);

                if (allocationSchool is not null)
                {
                    Result<Allocation> allocation = Allocation.Create(
                        asset.Value.Id,
                        allocationSchool,
                        _dateTime.Today);

                    if (allocation.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(ImportAssetDto), importAsset, true)
                            .ForContext(nameof(Error), allocation.Error, true)
                            .Warning("Failed to import Asset");

                        response.Add(new(importAsset.RowNumber, false, allocation.Error));

                        continue;
                    }

                    asset.Value.AddAllocation(allocation.Value);
                }

                if (staffMember is not null)
                {
                    Result<Allocation> allocation = Allocation.Create(
                        asset.Value.Id,
                        staffMember,
                        _dateTime.Today);

                    if (allocation.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(ImportAssetDto), importAsset, true)
                            .ForContext(nameof(Error), allocation.Error, true)
                            .Warning("Failed to import Asset");

                        response.Add(new(importAsset.RowNumber, false, allocation.Error));
                        
                        continue;
                    }

                    asset.Value.AddAllocation(allocation.Value);
                }

                if (student is not null)
                {
                    Result<Allocation> allocation = Allocation.Create(
                        asset.Value.Id,
                        student,
                        _dateTime.Today);

                    if (allocation.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(ImportAssetDto), importAsset, true)
                            .ForContext(nameof(Error), allocation.Error, true)
                            .Warning("Failed to import Asset");

                        response.Add(new(importAsset.RowNumber, false, allocation.Error));

                        continue;
                    }

                    asset.Value.AddAllocation(allocation.Value);
                }
            }

            Result<Note> note = Note.Create(
                asset.Value.Id,
                "Imported from file");

            if (note.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportAssetDto), importAsset, true)
                    .ForContext(nameof(Error), note.Error, true)
                .Warning("Failed to import Asset");

                response.Add(new(importAsset.RowNumber, false, note.Error));
                
                continue;
            }

            asset.Value.AddNote(note.Value);

            if (!string.IsNullOrWhiteSpace(importAsset.Notes))
            {
                Result<Note> oldNote = Note.Create(
                    asset.Value.Id,
                    importAsset.Notes);

                if (oldNote.IsFailure)
                {
                    _logger
                        .ForContext(nameof(ImportAssetDto), importAsset, true)
                        .ForContext(nameof(Error), oldNote.Error, true)
                        .Warning("Failed to import Asset");

                    response.Add(new(importAsset.RowNumber, false, oldNote.Error));

                    continue;
                }

                asset.Value.AddNote(oldNote.Value);
            }

            if (importAsset.LastSighted.HasValue && importAsset.LastSighted != DateOnly.MinValue)
            {
                Result<Sighting> sighting = Sighting.Create(
                    asset.Value.Id,
                    "Legacy Import",
                    importAsset.LastSighted.Value.ToDateTime(TimeOnly.MinValue),
                    string.Empty,
                    _dateTime);

                if (sighting.IsFailure)
                {
                    _logger
                        .ForContext(nameof(ImportAssetDto), importAsset, true)
                        .ForContext(nameof(Error), sighting.Error, true)
                        .Warning("Failed to import Asset");

                    response.Add(new(importAsset.RowNumber, false, sighting.Error));
                    
                    continue;
                }

                asset.Value.AddSighting(sighting.Value);
            }

            _assetRepository.Insert(asset.Value);
            await _unitOfWork.CompleteAsync(cancellationToken);

            response.Add(new(importAsset.RowNumber, true, null));
        }

        return response;
    }
}
