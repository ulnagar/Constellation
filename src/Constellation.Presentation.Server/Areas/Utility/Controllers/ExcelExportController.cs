﻿using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Utility.Controllers
{
    using Application.Helpers;

    [Area("Utility")]
    [Authorize]
    public class ExcelExportController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelService _excelService;
        private readonly IExportService _exportService;

        public ExcelExportController(
            IUnitOfWork unitOfWork,
            IExcelService excelService,
            IExportService exportService,
            IMediator mediator)
            :base(mediator)
        {
            _unitOfWork = unitOfWork;
            _excelService = excelService;
            _exportService = exportService;
        }

        //public async Task<IActionResult> ExportAbsences(string title)
        //{
        //    var data = JsonConvert.DeserializeObject<ICollection<AbsenceExportDto>>((string)TempData["data"]);

        //    var stream = await _excelService.CreateAbsencesFile(data, title);

        //    var fs = new FileStreamResult(stream, FileContentTypes.ExcelModernFile)
        //    {
        //        FileDownloadName = "AbsencesExport.xlsx"
        //    };

        //    return fs;
        //}

        public async Task<IActionResult> ExportInterviews(InterviewExportSelectionDto filter, CancellationToken cancellationToken)
        {
            var data = await _unitOfWork.Students.ForInterviewsExportAsync(filter, cancellationToken);

            var exportDto = await _exportService.CreatePTOExport(data, filter.PerFamily, filter.ResidentialFamilyOnly, cancellationToken);

            var stream = await _excelService.CreatePTOFile(exportDto);

            var fs = new FileStreamResult(stream, FileContentTypes.ExcelModernFile)
            {
                FileDownloadName = "PTO Export.xlsx"
            };

            return fs;
        }
    }
}
