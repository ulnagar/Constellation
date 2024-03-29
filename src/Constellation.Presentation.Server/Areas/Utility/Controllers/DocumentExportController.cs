﻿using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.Templates.Views.Documents.Attendance;
using Constellation.Presentation.Server.Areas.Utility.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Mime;

namespace Constellation.Presentation.Server.Areas.Utility.Controllers
{
    using Application.Helpers;

    [Area("Utility")]
    [Authorize]
    public class DocumentExportController : Controller
    {
        private readonly IRazorViewToStringRenderer _renderService;
        private readonly IPDFService _pdfService;
        private readonly IExcelService _excelService;

        public DocumentExportController(
            IUnitOfWork unitOfWork, 
            IRazorViewToStringRenderer renderService,
            IPDFService pdfService, 
            IExcelService excelService,
            IMediator mediator)
        {
            _renderService = renderService;
            _pdfService = pdfService;
            _excelService = excelService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExportAttendanceReport(string title)
        {
            var data = JsonConvert.DeserializeObject<AttendanceReportViewModel>((string)TempData["data"]);

            var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", data);
            var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", data);

            var pdfStream = _pdfService.StringToPdfStream(htmlString, headerString);
            var fs = new FileStreamResult(pdfStream, MediaTypeNames.Application.Pdf)
            {
                FileDownloadName = title
            };

            return fs;
        }

        public async Task<IActionResult> Awards()
        {
            var viewModel = new Export_Awards_ViewModel();

            return View("Awards", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<FileResult> Awards(Export_Awards_ViewModel viewModel)
        {
            if (viewModel.UploadedFile.Length == 0)
                return null;

            using var stream = new MemoryStream();
            await viewModel.UploadedFile.CopyToAsync(stream);

            var resultStream = await _excelService.CreateAwardsCalculationFile(stream);

            var fileName = $"Awards {DateTime.Now:yyyy-MM-dd}.xlsx";
            var contentType = FileContentTypes.ExcelModernFile;

            return File(resultStream, contentType, fileName);
        }
    }
}
