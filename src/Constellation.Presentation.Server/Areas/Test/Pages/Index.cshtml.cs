namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Assets.ImportAssetsFromFile;
using Application.Common.PresentationModels;
using Application.Interfaces.Repositories;
using BaseModels;
using Core.Abstractions.Clock;
using Core.Models;
using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Repositories;
using Core.Models.Assets.ValueObjects;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.Attributes;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IAssetRepository _assetRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(
        IMediator mediator,
        IAssetRepository assetRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _assetRepository = assetRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    [AllowExtensions(FileExtensions: "xlsx", ErrorMessage = "You can only upload XLSX files")]
    public IFormFile UploadFile { get; set; }

    public string Message { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task OnGetRunCode()
    {
        Student student = await _studentRepository.GetById("445087432");

        if (student is null)
            return;

        School school = await _schoolRepository.GetById(student.SchoolCode);

        if (school is null)
            return;

        Result<AssetNumber> assetNumber = AssetNumber.Create("AC00001407");

        if (assetNumber.IsFailure)
        {
            Message = assetNumber.Error.Message;

            return;
        }

        Result<Asset> asset = await Asset.Create(
            assetNumber.Value,
            "LR0E0HAW",
            "901584297",
            "20SE-S00300",
            "Lenovo Yoga 11e 6th Gen",
            AssetCategory.Student,
            "",
            1267m,
            DateOnly.Parse("2020-09-24"),
            _dateTime,
            _assetRepository);

        if (asset.IsFailure)
        {
            Message = asset.Error.Message;

            return;
        }

        Location location = Location.CreatePublicSchoolLocationRecord(
            asset.Value.Id,
            school.Name,
            school.Code,
            true,
            DateOnly.Parse("2021-03-01"));

        asset.Value.AddLocation(location);

        Result<Allocation> allocation = Allocation.Create(
            asset.Value.Id,
            student,
            DateOnly.Parse("2021-03-01"));

        if (allocation.IsFailure)
        {
            Message = asset.Error.Message;

            return;
        }

        asset.Value.AddAllocation(allocation.Value);

        Sighting sighting = Sighting.Create(
            asset.Value.Id,
            "Toni Stanton",
            DateTime.Parse("2022-07-20 10:31"),
            "Good condition");

        asset.Value.AddSighting(sighting);

        _assetRepository.Insert(asset.Value);
        await _unitOfWork.CompleteAsync();
    }

    public async Task OnPostImportFile()
    {
        if (UploadFile is not null)
        {
            try
            {
                await using MemoryStream target = new();
                await UploadFile.CopyToAsync(target);

                Result request = await _mediator.Send(new ImportAssetsFromFileCommand(target));

                if (request.IsFailure)
                {
                    Error = new ErrorDisplay
                    {
                        Error = request.Error,
                        RedirectPath = null
                    };
                }
            }
            catch (Exception ex)
            {
                Error = new ErrorDisplay
                {
                    Error = new(ex.Source, ex.Message),
                    RedirectPath = null
                };
            }
        }
    }
}