namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Students.Models;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.DTOs;
using Constellation.Application.Students.GetCurrentStudentsFromSchool;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Net.Mime;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ReportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ReportModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ReportModel>()
            .ForContext("APPLICATION", "Schools Portal");

        StartDate = dateTime.Today;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Absences;

    public List<StudentResponse> Students { get; set; } = new();

    [BindProperty]
    public List<StudentId> SelectedStudents { get; set; } = new();

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly StartDate { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
        
        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                studentsRequest.Error,
                _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        var endDate = StartDate.AddDays(12);

        if (SelectedStudents.Count > 1)
        {
            List<FileDto> reports = new();

            // We need to loop each student id and collate the report into a zip file.
            foreach (StudentId studentId in SelectedStudents)
            {
                Result<FileDto> reportRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(studentId, StartDate, endDate));

                if (reportRequest.IsSuccess)
                    reports.Add(reportRequest.Value);
            }

            // Zip all reports
            using MemoryStream memoryStream = new MemoryStream();
            using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create))
            {
                foreach (FileDto file in reports)
                {
                    ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.FileName);
                    await using StreamWriter streamWriter = new(zipArchiveEntry.Open());
                    byte[] fileData = file.FileData;
                    await streamWriter.BaseStream.WriteAsync(fileData, 0, fileData.Length);
                }
            }

            MemoryStream attachmentStream = new(memoryStream.ToArray());

            return File(attachmentStream.ToArray(), MediaTypeNames.Application.Zip, "Attendance Reports.zip");
        }
         
        // We only have one student, so just download that file.
        Result<FileDto> fileRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(SelectedStudents.First(), StartDate, endDate));

        if (fileRequest.IsFailure)
            return BadRequest();

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}