namespace Constellation.Presentation.Server.Areas.Utility.Controllers;

using Application.Helpers;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Core.Models.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Utility")]
[Authorize]
public class ExcelExportController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelService _excelService;
    private readonly IExportService _exportService;

    public ExcelExportController(
        IUnitOfWork unitOfWork,
        IExcelService excelService,
        IExportService exportService)
    {
        _unitOfWork = unitOfWork;
        _excelService = excelService;
        _exportService = exportService;
    }

    public async Task<IActionResult> ExportInterviews(InterviewExportSelectionDto filter, CancellationToken cancellationToken)
    {
        List<Student> data = await _unitOfWork.Students.ForInterviewsExportAsync(filter, cancellationToken);

        List<InterviewExportDto> exportDto = await _exportService.CreatePTOExport(data, filter.PerFamily, filter.ResidentialFamilyOnly, cancellationToken);

        MemoryStream stream = await _excelService.CreatePTOFile(exportDto);

        FileStreamResult fs = new(stream, FileContentTypes.ExcelModernFile)
        {
            FileDownloadName = "PTO Export.xlsx"
        };

        return fs;
    }
}