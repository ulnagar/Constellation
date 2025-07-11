namespace Constellation.Infrastructure.Jobs;

using Application.Domains.Covers.Queries.GetClassCoversSummaryByDateAndOffering;
using Application.Domains.Faculties.Queries.GetFacultyManagers;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.Offerings;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RollMarkingReportJob : IRollMarkingReportJob
{
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISentralGateway _sentralService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly AppConfiguration _configuration;

    public RollMarkingReportJob(
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ISentralGateway sentralService, 
        IEmailService emailService, 
        ILogger logger, 
        IMediator mediator,
        IOptions<AppConfiguration> configuration)
    {
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _sentralService = sentralService;
        _emailService = emailService;
        _logger = logger.ForContext<IRollMarkingReportJob>();
        _mediator = mediator;
        _configuration = configuration.Value;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        DateOnly date = DateOnly.FromDateTime(DateTime.Today);

        if (date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
            return;

        _logger.Information("{id}: Checking Date: {date}", jobId, date.ToShortDateString());
        ICollection<RollMarkReportDto> entries = await _sentralService.GetRollMarkingReportAsync(date);

        List<RollMarkReportDto> unsubmitted = entries.Where(entry => !entry.Submitted).ToList();
        AppConfiguration.AbsencesConfiguration absenceSettings = _configuration.Absences;
        Dictionary<string, string> recipients;

        if (unsubmitted.Count == 0)
        {
            string infoString = $"{date.ToShortDateString()} - No Unsubmitted Rolls found!";

            _logger.Information("{id}: {infoString}", jobId, infoString);

            recipients = new();
            foreach (string entry in absenceSettings.SendRollMarkingReportTo)
            {
                if (recipients.All(recipient => recipient.Value != entry))
                    recipients.Add(entry, entry);
            }
            
            await _emailService.SendNoRollMarkingReport(date, recipients);

            return;
        }

        List<RollMarkingEmailDto> emailList = new();

        foreach (RollMarkReportDto entry in unsubmitted)
        {
            if (token.IsCancellationRequested)
                return;

            if (entry.Teacher.Contains(','))
                entry.Teacher = null;

            RollMarkingEmailDto emailDto = new() { Period = entry.Period };

            entry.ClassName = entry.ClassName.PadLeft(7, '0');

            Offering offering = await _offeringRepository.GetFromYearAndName(date.Year, entry.ClassName, token);

            StaffMember teacher = null;

            if (!string.IsNullOrWhiteSpace(entry.Teacher))
            {
                teacher = await _staffRepository.GetFromName(entry.Teacher);
            }

            if (offering is not null && teacher is null)
            {
                List<TeacherAssignment> assignments = offering
                    .Teachers
                    .Where(assignment =>
                        !assignment.IsDeleted &&
                        assignment.Type == AssignmentType.ClassroomTeacher)
                    .ToList();

                foreach (TeacherAssignment assignment in assignments)
                {
                    teacher = await _staffRepository.GetById(assignment.StaffId, token);

                    if (teacher is not null)
                        entry.Teacher = teacher.Name.DisplayName;
                }
            }

            bool covered = false;
            string coveredBy = string.Empty;
            string coverType = string.Empty;

            if (offering is null && teacher is null)
            {
                // Add note that Constellation does not understand this entry and send only to Scott.
                emailDto.Notes.Add($"Could not identify the class or the teacher from Constellation records.");
                emailDto.Notes.Add($"No email sent to classroom teacher or head teacher.");
            }

            if (teacher is not null)
            {
                emailDto.Teachers.Add(teacher.EmailAddress.Email, teacher.Name.DisplayName);
                emailDto.TeacherName = teacher.Name.SortOrder;
            }

            if (offering is not null)
            {
                Course course = await _courseRepository.GetById(offering.CourseId, token);

                if (course is null)
                    continue;

                Result<List<StaffMember>> headTeachersRequest = await _mediator.Send(new GetFacultyManagersQuery(course.FacultyId), token);
                if (headTeachersRequest.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), headTeachersRequest.Error, true)
                        .Warning("Failed to determine Head Teachers for Faculty");
                }
                else
                {
                    emailDto.HeadTeachers = headTeachersRequest.Value
                        .ToDictionary(member => member.EmailAddress.Email, member => member.Name.DisplayName);
                }

                Result<List<ClassCoverSummaryByDateAndOfferingResponse>> coversRequest = await _mediator.Send(new GetClassCoversSummaryByDateAndOfferingQuery(date, offering.Id), token);

                if (coversRequest.IsSuccess && coversRequest.Value.Count > 0)
                {
                    ClassCoverSummaryByDateAndOfferingResponse classCover = coversRequest.Value.OrderBy(cover => cover.CreatedAt).Last();

                    covered = true;
                    coveredBy = classCover.TeacherName;
                    coverType = classCover.CoverType.Name;
                }
            }

            string infoString = $"{entry.Date.ToShortDateString()} - Unsubmitted Roll for {entry.ClassName}";
            if (!string.IsNullOrWhiteSpace(entry.Teacher))
                infoString += $" by {entry.Teacher}";
            infoString += $" in Period {entry.Period}";
            if (covered)
            {
                infoString += $" covered by {coveredBy} ({coverType})";
            }

            _logger.Information("{id}: {infoString}", jobId, infoString);
            emailDto.RollInformation = infoString;
            emailList.Add(emailDto);
        }

        // Send emails to teachers
        // get flattened list of teachers
        List<KeyValuePair<string, string>> teachers = emailList.SelectMany(list => list.Teachers).Distinct().ToList();

        foreach (KeyValuePair<string, string> teacher in teachers)
        {
            recipients = new();

            List<RollMarkingEmailDto> emails = emailList.Where(item => item.Teachers.ContainsKey(teacher.Key)).ToList();

            if (token.IsCancellationRequested)
                return;

            recipients.Add(teacher.Value, teacher.Key);

            // Email the relevant person (as outlined in the EmailSentTo field
            if (recipients.Count > 0)
                await _emailService.SendDailyRollMarkingReport(emails, date, recipients);
        }

        // Send emails to head teachers
        recipients = new();
        teachers = emailList.SelectMany(list => list.HeadTeachers).Distinct().ToList();

        foreach (KeyValuePair<string, string> teacher in teachers)
        {
            recipients = new();

            List<RollMarkingEmailDto> emails = emailList.Where(item => item.HeadTeachers.ContainsKey(teacher.Key)).ToList();

            if (token.IsCancellationRequested)
                return;

            recipients.Add(teacher.Value, teacher.Key);

            // Email the relevant person (as outlined in the EmailSentTo field
            if (recipients.Count > 0)
                await _emailService.SendDailyRollMarkingReport(emails, date, recipients);
        }

        // Send emails to absence administrator
        if (token.IsCancellationRequested)
            return;
        
        recipients = new();
        foreach (string entry in absenceSettings.SendRollMarkingReportTo)
        {
            if (recipients.All(recipient => recipient.Value != entry))
                recipients.Add(entry, entry);
        }

        if (recipients.Count > 0)
            await _emailService.SendDailyRollMarkingReport(emailList, date, recipients);
    }
}
