namespace Constellation.Infrastructure.Services;

using Constellation.Application.Abstractions;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.StaffMembers.GetStaffById;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Documents.Covers;
using System.Net.Mail;

internal sealed class EmailAttachmentService : IEmailAttachmentService
{
    private readonly ISender _mediator;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IPDFService _pdfService;

    public EmailAttachmentService(
        ISender mediator,
        IRazorViewToStringRenderer razorService,
        IPDFService pdfService)
    {
        _mediator = mediator;
        _razorService = razorService;
        _pdfService = pdfService;
    }

    public async Task<Attachment> GenerateClassRollDocument(Offering offering, List<Student> students, CancellationToken cancellationToken = default)
    {
        var model = new CoverRollViewModel();
        model.ClassName = offering.Name;

        foreach (var student in students)
        {
            var entry = new CoverRollViewModel.EnrolledStudent();
            entry.Name = student.Name.DisplayName;
            entry.Gender = student.Gender.Name;
            entry.School = student.CurrentEnrolment?.SchoolName;
            entry.OrderName = student.Name.SortOrder;

            model.Students.Add(entry);
        }

        var rollString = await _razorService.RenderViewToStringAsync("/Views/Documents/Covers/CoverRoll.cshtml", model);
        var rollAttachment = _pdfService.StringToPdfAttachment(rollString, $"{offering.Name} Roll.pdf");

        return rollAttachment;
    }

    public async Task<Attachment> GenerateClassTimetableDocument(Offering offering, List<TimetablePeriod> relevantPeriods, CancellationToken cancellationToken = default)
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

            if (offering.Sessions.Any(session => session.PeriodId == period.Id))
            {
                entry.ClassName = offering.Name;

                List<TeacherAssignment> assignments = offering
                    .Teachers
                    .Where(assignment => 
                        assignment.Type == AssignmentType.ClassroomTeacher && 
                        !assignment.IsDeleted)
                    .ToList();

                foreach (TeacherAssignment assignment in assignments)
                {
                    Result<StaffResponse> teacher = await _mediator.Send(new GetStaffByIdQuery(assignment.StaffId), cancellationToken);

                    if (teacher.IsFailure)
                        continue;

                    entry.ClassTeacher = teacher.Value.Name.DisplayName;
                }
            }

            timetableData.Timetables.Add(entry);
        }

        var timetableHeaderString = await _razorService.RenderViewToStringAsync("/Views/Documents/Timetable/ClassTimetableHeader.cshtml", timetableData);
        var timetableBodyString = await _razorService.RenderViewToStringAsync("/Views/Documents/Timetable/Timetable.cshtml", timetableData);

        var timetableAttachment = _pdfService.StringToPdfAttachment(timetableBodyString, timetableHeaderString, $"{offering.Name} Timetable.pdf");

        return timetableAttachment;
    }
}
