using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Presentation.Server.Areas.Reports.Models;
using Constellation.Presentation.Server.Areas.Utility.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Utility.Controllers
{
    [Area("Utility")]
    [Authorize]
    public class DocumentExportController : BaseController
    {
        private readonly IRazorViewToStringRenderer _renderService;
        private readonly IPDFService _pdfService;
        private readonly IExcelService _excelService;

        public DocumentExportController(IUnitOfWork unitOfWork, IRazorViewToStringRenderer renderService,
            IPDFService pdfService, IExcelService excelService)
            : base(unitOfWork)
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
            var fs = new FileStreamResult(pdfStream, MediaTypeNames.Application.Pdf);
            fs.FileDownloadName = title;

            return fs;
        }

        [Route("Awards")]
        public async Task<ActionResult> AwardsUpload()
        {
            var viewModel = CreateViewModel<Export_Awards_ViewModel>();

            return View("Awards", viewModel);
        }

        [HttpPost]
        [Route("Awards")]
        [ValidateAntiForgeryToken]
        public async Task<FileResult> DownloadAwardsFile(Export_Awards_ViewModel viewModel)
        {
            if (viewModel.uploadedFile.Length == 0)
                return null;

            using var stream = new MemoryStream();
            await viewModel.uploadedFile.CopyToAsync(stream);

            var resultStream = await _excelService.CreateAwardsCalculationFile(stream);

            var fileName = $"Awards {DateTime.Now.ToString("yyyy-MM-dd")}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(resultStream, contentType, fileName);
        }
    }
}
