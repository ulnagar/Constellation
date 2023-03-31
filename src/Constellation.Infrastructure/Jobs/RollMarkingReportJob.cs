﻿namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.ClassCovers.GetCoversSummaryByDateAndOffering;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class RollMarkingReportJob : IRollMarkingReportJob, IScopedService, IHangfireJob
{
    private readonly ISentralGateway _sentralService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<IRollMarkingReportJob> _logger;
    private readonly IMediator _mediator;

    public RollMarkingReportJob(ISentralGateway sentralService, IUnitOfWork unitOfWork, 
        IEmailService emailService, ILogger<IRollMarkingReportJob> logger, IMediator mediator)
    {
        _sentralService = sentralService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
            return;

        _logger.LogInformation("{id}: Checking Date: {date}", jobId, date.ToShortDateString());
        var entries = await _sentralService.GetRollMarkingReportAsync(date);

        var unsubmitted = entries.Where(entry => !entry.Submitted).ToList();
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();
        var recipients = new Dictionary<string, string>();

        if (!unsubmitted.Any())
        {
            var infoString = $"{date.ToShortDateString()} - No Unsubmitted Rolls found!";

            _logger.LogInformation("{id}: {infoString}", jobId, infoString);


            recipients = new();
            if (!recipients.Any(recipient => recipient.Value == absenceSettings.ForwardingEmailAbsenceCoordinator))
                recipients.Add(absenceSettings.AbsenceCoordinatorName, absenceSettings.ForwardingEmailAbsenceCoordinator);

            if (!recipients.Any(recipient => recipient.Value == "scott.new@det.nsw.edu.au"))
                recipients.Add("Scott New", "scott.new@det.nsw.edu.au");

            await _emailService.SendNoRollMarkingReport(date, recipients);

            return;
        }

        var emailList = new List<RollMarkingEmailDto>();

        foreach (var entry in unsubmitted)
        {
            if (token.IsCancellationRequested)
                return;

            var emailDto = new RollMarkingEmailDto();

            var offeringName = entry.ClassName.StartsWith("5") || entry.ClassName.StartsWith("6") ? entry.ClassName.PadLeft(7, '0') : entry.ClassName.PadLeft(6, '0');
            offeringName = offeringName.StartsWith("5") || offeringName.StartsWith("6") ? offeringName.PadLeft(8, '0') : offeringName;
            entry.ClassName = offeringName;

            var offering = await _unitOfWork.CourseOfferings.GetFromYearAndName(date.Year, offeringName);

            Staff teacher = null;

            if (string.IsNullOrWhiteSpace(entry.Teacher) && offering is not null)
            {
                var sessions = _unitOfWork.OfferingSessions.AllForOffering(offering.Id);
                teacher = sessions
                    .GroupBy(entry => entry.StaffId)
                    .OrderByDescending(entry => entry.Count())
                    .First()
                    .Select(entry => entry.Teacher)
                    .First();
            }
            else if (!string.IsNullOrWhiteSpace(entry.Teacher))
            {
                teacher = await _unitOfWork.Staff.GetFromName(entry.Teacher);
            }

            var infoString = string.Empty;
            var Covered = false;
            var CoveredBy = string.Empty;
            var CoverType = string.Empty;

            if (offering is null && teacher is null)
            {
                // Add note that Constellation does not understand this entry and send only to Scott.
                emailDto.Notes.Add($"Could not identify the class or the teacher from Constellation records.");
                emailDto.Notes.Add($"No email sent to classroom teacher or head teacher.");
            }

            if (teacher is not null)
            {
                emailDto.Teachers.Add(teacher.EmailAddress, teacher.DisplayName);
            }

            if (offering is not null)
            {
                var headTeachers = await _mediator.Send(new GetListOfFacultyManagersQuery { FacultyId = offering.Course.FacultyId }, token);
                emailDto.HeadTeachers = headTeachers.ToDictionary(member => member.EmailAddress, member => member.DisplayName);

                var coversRequest = await _mediator.Send(new GetCoversSummaryByDateAndOfferingQuery(date, offering.Id));

                if (coversRequest.IsSuccess)
                {
                    var cover = coversRequest.Value.OrderBy(cover => cover.CreatedAt).Last();

                    Covered = true;
                    CoveredBy = cover.TeacherName;
                    CoverType = cover.CoverType;
                }
            }

            infoString = $"{entry.Date.ToShortDateString()} - Unsubmitted Roll for {entry.ClassName}";
            if (!string.IsNullOrWhiteSpace(entry.Teacher))
                infoString += $" by {entry.Teacher}";
            infoString += $" in Period {entry.Period}";
            if (Covered)
            {
                infoString += $"covered by {CoveredBy} ({CoverType})";
            }

            _logger.LogInformation("{id}: {infoString}", jobId, infoString);
            emailDto.RollInformation = infoString;
            emailList.Add(emailDto);
        }

        // Send emails to teachers
        // get flattened list of teachers
        var teachers = emailList.SelectMany(list => list.Teachers).Distinct().ToList();

        foreach (var teacher in teachers)
        {
            recipients = new();

            var emails = emailList.Where(item => item.Teachers.ContainsKey(teacher.Key)).ToList();

            if (token.IsCancellationRequested)
                return;

            recipients.Add(teacher.Value, teacher.Key);

            // Email the relevant person (as outlined in the EmailSentTo field
            await _emailService.SendDailyRollMarkingReport(emails, date, recipients);
        }

        // Send emails to head teachers
        recipients = new();
        teachers = emailList.SelectMany(list => list.HeadTeachers).Distinct().ToList();

        foreach (var teacher in teachers)
        {
            recipients = new();

            var emails = emailList.Where(item => item.HeadTeachers.ContainsKey(teacher.Key)).ToList();

            if (token.IsCancellationRequested)
                return;

            recipients.Add(teacher.Value, teacher.Key);

            // Email the relevant person (as outlined in the EmailSentTo field
            await _emailService.SendDailyRollMarkingReport(emails, date, recipients);
        }

        // Send emails to absence administrator
        if (token.IsCancellationRequested)
            return;
        
        recipients = new();
        if (!recipients.Any(recipient => recipient.Value == absenceSettings.ForwardingEmailAbsenceCoordinator))
            recipients.Add(absenceSettings.AbsenceCoordinatorName, absenceSettings.ForwardingEmailAbsenceCoordinator);

        if (!recipients.Any(recipient => recipient.Value == "scott.new@det.nsw.edu.au"))
            recipients.Add("Scott New", "scott.new@det.nsw.edu.au");

        await _emailService.SendDailyRollMarkingReport(emailList, date, recipients);
    }
}
