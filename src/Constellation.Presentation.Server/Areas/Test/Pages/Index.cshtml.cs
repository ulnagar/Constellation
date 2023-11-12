namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs;
using Application.Interfaces.Gateways;
using Application.Interfaces.Services;
using BaseModels;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IO;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ISentralGateway _sentralGateway;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IExcelService _excelService;

    public IndexModel(
        ISender mediator,
        ISentralGateway sentralGateway,
        IAttendanceRepository attendanceRepository,
        IExcelService excelService)
    {
        _mediator = mediator;
        _sentralGateway = sentralGateway;
        _attendanceRepository = attendanceRepository;
        _excelService = excelService;
    }

    public List<SentralIncidentDetails> Data { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);


    }

    public async Task<IActionResult> OnPost()
    {
        List<AttendanceValue> values = await _attendanceRepository.GetForReportWithTitle("Term 4, Week 5, 2023");

        MemoryStream result = await _excelService.CreateStudentAttendanceReport(values);

        byte[] fileData = result.ToArray();
        string fileName = "Attendance Report.xlsx";
        string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(fileData, fileType, fileName);
    }
}