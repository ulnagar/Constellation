namespace Constellation.Infrastructure.Services;

using Constellation.Application.Abstractions;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.Templates.Views.Documents.Covers;
using System.Net.Mail;

internal sealed class EmailAttachmentService : IEmailAttachmentService
{
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IPDFService _pdfService;

    public EmailAttachmentService(
        IRazorViewToStringRenderer razorService,
        IPDFService pdfService)
    {
        _razorService = razorService;
        _pdfService = pdfService;
    }

    public async Task<Attachment> GenerateClassRollDocument(CourseOffering offering, List<Student> students, CancellationToken cancellationToken = default)
    {
        var model = new CoverRollViewModel();
        model.ClassName = offering.Name;

        foreach (var student in students)
        {
            var entry = new CoverRollViewModel.EnrolledStudent();
            entry.Name = student.DisplayName;
            entry.Gender = student.Gender;
            entry.School = student.School.Name;
            entry.OrderName = $"{student.LastName} {student.FirstName}";

            model.Students.Add(entry);
        }

        var rollString = await _razorService.RenderViewToStringAsync("/Views/Documents/Covers/CoverRoll.cshtml", model);
        var rollAttachment = _pdfService.StringToPdfAttachment(rollString, $"{offering.Name} Roll.pdf");

        return rollAttachment;
    }

    public async Task<Attachment> GenerateClassTimetableDocument(CourseOffering offering, List<OfferingSession> offeringSessions, List<TimetablePeriod> relevantPeriods, CancellationToken cancellationToken = default)
    {
        var timetableData = new ClassTimetableDataDto
        {
            ClassName = offering.Name
        };

        foreach (var period in relevantPeriods)
        {
            if (period.Type == "Other")
                continue;

            var entry = new TimetableDataDto.TimetableData
            {
                Day = period.Day,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                TimetableName = period.Timetable,
                Name = period.Name,
                Period = period.Period,
                Type = period.Type
            };

            if (offeringSessions.Any(session => session.PeriodId == period.Id))
            {
                var relevantSession = offeringSessions.FirstOrDefault(session => session.PeriodId == period.Id);
                entry.ClassName = relevantSession.Offering.Name;
                entry.ClassTeacher = relevantSession.Teacher.DisplayName;
            }

            timetableData.Timetables.Add(entry);
        }

        var timetableHeaderString = await _razorService.RenderViewToStringAsync("/Views/Documents/Timetable/ClassTimetableHeader.cshtml", timetableData);
        var timetableBodyString = await _razorService.RenderViewToStringAsync("/Views/Documents/Timetable/Timetable.cshtml", timetableData);

        var timetableAttachment = _pdfService.StringToPdfAttachment(timetableBodyString, timetableHeaderString, $"{offering.Name} Timetable.pdf");

        return timetableAttachment;
    }
}
