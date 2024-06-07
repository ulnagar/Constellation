#nullable enable
namespace Constellation.Application.Assets.ImportAssetsFromFile;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Repositories;
using Core.Models.Assets.ValueObjects;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ImportAssetsFromFileCommandHandler
: ICommandHandler<ImportAssetsFromFileCommand>
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

    public async Task<Result> Handle(ImportAssetsFromFileCommand request, CancellationToken cancellationToken)
    {
        List<ImportAssetDto> importedAssets = await _excelService.ImportAssetsFromFile(request.ImportFile, cancellationToken);

        List<School> schools = await _schoolRepository.GetAll(cancellationToken);

        List<Staff> staff = await _staffRepository.GetAll(cancellationToken);

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

                continue;
            }

            Result statusUpdate = asset.Value.UpdateStatus(status);

            if (asset.IsFailure)
            {
                _logger
                    .ForContext(nameof(ImportAssetDto), importAsset, true)
                    .ForContext(nameof(Error), statusUpdate.Error, true)
                    .Warning("Failed to import Asset");

                continue;
            }

            School? locationSchool = schools.FirstOrDefault(entry => entry.Name == importAsset.LocationSite);

            if (locationSchool is null && locationCategory.Equals(LocationCategory.PublicSchool))
            {
                _logger
                    .ForContext(nameof(ImportAssetDto), importAsset, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(""), true)
                    .Warning("Failed to import Asset");

                continue;
            }

            Location location = locationCategory switch
            {
                _ when locationCategory.Equals(LocationCategory.CoordinatingOffice) =>
                    Location.CreateOfficeLocationRecord(asset.Value.Id, importAsset.LocationRoom ?? string.Empty, true, _dateTime.Today),
                _ when locationCategory.Equals(LocationCategory.CorporateOffice) =>
                    Location.CreateCorporateOfficeLocationRecord(asset.Value.Id, importAsset.LocationSite ?? string.Empty, true, _dateTime.Today),
                _ when locationCategory.Equals(LocationCategory.PrivateResidence) =>
                    Location.CreatePrivateResidenceLocationRecord(asset.Value.Id, true, _dateTime.Today),
                _ when locationCategory.Equals(LocationCategory.PublicSchool) =>
                    Location.CreatePublicSchoolLocationRecord(asset.Value.Id, importAsset.LocationSite ?? locationSchool!.Name, locationSchool!.Code, true, _dateTime.Today)
            };
                
            asset.Value.AddLocation(location);

            if (!string.IsNullOrWhiteSpace(importAsset.ResponsibleOfficer))
            {
                School? allocationSchool = schools.FirstOrDefault(entry => entry.Code == importAsset.ResponsibleOfficer);
                Staff? staffMember = staff.FirstOrDefault(entry => entry.StaffId == importAsset.ResponsibleOfficer);
                Student? student = students.FirstOrDefault(entry => entry.StudentId == importAsset.ResponsibleOfficer);

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

                    continue;
                }

                asset.Value.AddNote(oldNote.Value);
            }

            _assetRepository.Insert(asset.Value);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
