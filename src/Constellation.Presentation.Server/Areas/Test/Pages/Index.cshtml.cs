namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using Constellation.Application.Assets.ImportAssetsFromFile;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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