namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.ExportOfferList;

using Abstractions.Messaging;
using Application.Interfaces.Services.Excel;
using Constellation.Core.Models.EnrolmentContext.Application.Enums;
using Constellation.Core.Models.EnrolmentContext.Application.Errors;
using Core.Extensions;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Globalization;

internal sealed class ExportOfferListQueryHandler
: IQueryHandler<ExportOfferListQuery, byte[]>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IExcelWriter _writer;
    private readonly ILogger _logger;

    public ExportOfferListQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentOfferRepository offerRepository,
        IExcelWriter writer,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _offerRepository = offerRepository;
        _writer = writer;
        _logger = logger
            .ForContext<ExportOfferListQuery>();
    }

    public async Task<Result<byte[]>> Handle(ExportOfferListQuery request, CancellationToken cancellationToken)
    {
        List<Offer> offers = await _offerRepository.GetListFromIds(request.OfferIds, cancellationToken);

        if (offers.Count == 0)
            return Result.Failure<byte[]>(EnrolmentOfferErrors.NoneFound);

        List<Application> applications =
            await _applicationRepository.GetListFromIds(offers.Select(entry => entry.ApplicationId).ToList(),
                cancellationToken);

        List<EnrolmentOfferExport> rows = [];

        foreach (Offer offer in offers)
        {
            Application? application = applications.FirstOrDefault(entry => entry.Id == offer.ApplicationId);

            if (application is null)
            {
                _logger
                    .ForContext(nameof(Offer), offer, true)
                    .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                    .Warning("Failed to find Enrolment Application linked to Offer");

                continue;
            }

            List<string> notes = [];

            foreach (var note in offer.Notes)
            {
                notes.Add($"{note.CreatedAt.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)} - {note.CreatedBy} - {note.Note}");
            }

            rows.Add(new(
                offer.Id,
                application.Id,
                EnrolmentPeriodId.Empty,
                string.Empty,
                application.StudentReferenceNumber,
                application.StudentName,
                application.StudentEmailAddress,
                application.StudentGender,
                application.ParentName,
                application.ParentEmailAddress,
                application.ParentPhoneNumber,
                application.ApplicationReference ?? string.Empty,
                application.CurrentSchoolCode,
                application.CurrentSchool ?? string.Empty,
                application.DestinationSchoolCode,
                application.DestinationSchool ?? string.Empty,
                application.Program,
                application.Grade,
                application.SelectedCourses.Where(entry => entry.Status == CourseSelectionStatus.Approved).Select(entry => entry.Course).ToList(),
                offer.Status,
                offer.Response,
                offer.OfferedAt?.DateTime,
                offer.RespondBy?.DateTime,
                offer.ReminderSentAt?.DateTime,
                offer.RespondedAt?.DateTime,
                offer.RespondedAt.HasValue ? offer.HasCourtOrders : null,
                offer.RespondedAt.HasValue ? offer.HasHealthConcerns : null,
                offer.RespondedAt.HasValue ? offer.RequestedLaptop : null,
                notes));
        }

        IExcelWorkbook workbook = _writer.CreateWorkbook();
        IExcelWorksheet sheet = _writer.AddWorksheet(workbook, "Sheet 1");

        _writer.WriteRange(sheet, 2, rows,
            new("Application Reference", a => a.ApplicationReference),
            new("SRN", a => a.StudentReferenceNumber?.Value ?? string.Empty),
            new("Student Family Name", a => a.StudentName.LastName),
            new("Student Given Names", a => a.StudentName.FirstName),
            new("Student Preferred Name", a => a.StudentName.PreferredName),
            new("Full Name", a => a.StudentName.DisplayName),
            new("Student Email", a => a.StudentEmail?.Email ?? string.Empty),
            new("Gender", a => a.StudentGender.Name),
            new("Cohort", a => a.Grade.AsNumber(), ExcelColumnFormat.Text),
            new("Program", a => a.Program.Name),
            new("Courses", a => String.Join("; ", a.Courses.Select(entry => entry.Name))),
            new("Current School Code", a => a.CurrentSchoolCode, ExcelColumnFormat.Text),
            new("Current School", a => a.CurrentSchool),
            new("Destination School Code", a => a.DestinationSchoolCode, ExcelColumnFormat.Text),
            new("Destination School", a => a.DestinationSchool),
            new("Parent First Name", a => a.ParentName?.FirstName),
            new("Parent Last Name", a => a.ParentName?.LastName),
            new("Phone Number", a => a.ParentPhoneNumber),
            new("Email", a => a.ParentEmailAddress),
            new("Status", a => a.Status.ToString()),
            new("Offered At", a => a.OfferedAt, ExcelColumnFormat.Date),
            new("Respond By", a => a.RespondBy, ExcelColumnFormat.Date),
            new("Reminder Sent At", a => a.ReminderSentAt, ExcelColumnFormat.Date),
            new("Responded At", a => a.RespondedAt, ExcelColumnFormat.Date),
            new("Response", a => a.Response),
            new("Court Orders", a => a.HasCourtOrders is null ? "N/A" : a.HasCourtOrders.Value ? "Yes" : "No"),
            new("Health Concerns", a => a.HasHealthConcerns is null ? "N/A" : a.HasHealthConcerns.Value ? "Yes" : "No"),
            new ("Laptop Requested", a => a.LaptopRequested is null ? "N/A" : a.LaptopRequested.Value ? "Yes" : "No"),
            new("Notes", a => string.Join("\n", a.Notes), ExcelColumnFormat.List));

        _writer.ApplyHeaderStyle(sheet, 1);
        _writer.AddAutoFilter(sheet);
        _writer.AutoFitColumns(sheet);

        byte[] file = _writer.GetAsByteArray(workbook);

        return file;
    }
}
