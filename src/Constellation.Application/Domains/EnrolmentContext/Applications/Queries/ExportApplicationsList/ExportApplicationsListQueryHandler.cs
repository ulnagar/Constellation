namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.ExportApplicationsList;

using Abstractions.Messaging;
using Application.Interfaces.Services.Excel;
using Core.Extensions;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Serilog;
using System.Globalization;

internal sealed class ExportApplicationsListQueryHandler
: IQueryHandler<ExportApplicationsListQuery, byte[]>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IExcelWriter _writer;
    private readonly ILogger _logger;

    public ExportApplicationsListQueryHandler(
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentApplicationRepository applicationRepository,
        IExcelWriter writer,
        ILogger logger)
    {
        _periodRepository = periodRepository;
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
            ("Application Reference", a => a.ApplicationReference),
            ("Student Family Name", a => a.StudentName.LastName),
            ("Student Given Names", a => a.StudentName.FirstName),
            ("Student Preferred Name", a => a.StudentName.PreferredName),
            ("Full Name", a => a.StudentName.DisplayName),
            ("DoB", a => a.DateOfBirth?.ToString("d", CultureInfo.InvariantCulture) ?? string.Empty),
            ("Gender", a => a.StudentGender.Name),
            ("Cohort", a => a.Grade.AsNumber()),
            ("Current School Code", a => a.CurrentSchoolCode),
            ("Current School", a => a.CurrentSchool),
            ("Destination School Code", a => a.DestinationSchoolCode),
            ("Destination School", a => a.DestinationSchool),
            ("Parent First Name", a => a.ParentName?.FirstName),
            ("Parent Last Name", a => a.ParentName?.LastName),
            ("Street Address", a => a.MailingAddress?.Street),
            ("Town", a => a.MailingAddress?.Town),
            ("State", a => a.MailingAddress?.State),
            ("PostCode", a => a.MailingAddress?.Postcode),
            ("Phone Number", a => a.ParentPhoneNumber),
            ("Email", a => a.ParentEmailAddress),
            ("Status", a => a.Status.ToString()));

        byte[] file = _writer.GetAsByteArray(workbook);

        return file;
    }
}
