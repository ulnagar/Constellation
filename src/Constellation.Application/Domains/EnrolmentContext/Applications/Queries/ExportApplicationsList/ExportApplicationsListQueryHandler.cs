namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.ExportApplicationsList;

using Abstractions.Messaging;
using Application.Interfaces.Services.Excel;
using Core.Extensions;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.Students.Enums;
using Core.Shared;
using Serilog;
using System.Globalization;

internal sealed class ExportApplicationsListQueryHandler
: IQueryHandler<ExportApplicationsListQuery, byte[]>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IExcelWriter _writer;
    private readonly ILogger _logger;

    public ExportApplicationsListQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IExcelWriter writer,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _writer = writer;
        _logger = logger
            .ForContext<ExportApplicationsListQuery>();
    }

    public async Task<Result<byte[]>> Handle(ExportApplicationsListQuery request, CancellationToken cancellationToken)
    {
        List<Application> applications = await _applicationRepository.GetListFromIds(request.ApplicationIds, cancellationToken);

        if (applications.Count == 0)
            return Result.Failure<byte[]>(EnrolmentApplicationErrors.NoneFound);

        IExcelWorkbook workbook = _writer.CreateWorkbook();
        IExcelWorksheet sheet = _writer.AddWorksheet(workbook, "Sheet 1");

        _writer.WriteRange(sheet, 2, applications,
            new("Application Reference", a => a.ApplicationReference),
            new("SRN", a => a.StudentReferenceNumber?.Value ?? ""),
            new("Student Family Name", a => a.StudentName.LastName),
            new("Student Given Names", a => a.StudentName.FirstName),
            new("Student Preferred Name", a => a.StudentName.PreferredName),
            new("Full Name", a => a.StudentName.DisplayName),
            new("Student Email", a => a.StudentEmailAddress?.Email ?? ""),
            new("DoB", a => a.DateOfBirth, ExcelColumnFormat.Date),
            new("Gender", a => a.StudentGender?.Name ?? "Unknown"),
            new("Cohort", a => a.Grade.AsNumber(), ExcelColumnFormat.Text),
            new("Courses", a => String.Join("; ", a.SelectedCourses.Where(entry => entry.Status <= CourseSelectionStatus.Approved).Select(entry => entry.Course.Name))),
            new("Current School Code", a => a.CurrentSchoolCode, ExcelColumnFormat.Text),
            new("Current School", a => a.CurrentSchool),
            new("Destination School Code", a => a.DestinationSchoolCode, ExcelColumnFormat.Text),
            new("Destination School", a => a.DestinationSchool),
            new("Parent First Name", a => a.ParentName?.FirstName),
            new("Parent Last Name", a => a.ParentName?.LastName),
            new("Street Address", a => a.MailingAddress?.Street),
            new("Town", a => a.MailingAddress?.Town),
            new("State", a => a.MailingAddress?.State),
            new("PostCode", a => a.MailingAddress?.Postcode, ExcelColumnFormat.Text),
            new("Phone Number", a => a.ParentPhoneNumber),
            new("Parent Email", a => a.ParentEmailAddress),
            new("Status", a => a.Status.ToString()));

        _writer.ApplyHeaderStyle(sheet, 1);
        _writer.AddAutoFilter(sheet);
        _writer.AutoFitColumns(sheet);

        byte[] file = _writer.GetAsByteArray(workbook);

        return file;
    }
}
