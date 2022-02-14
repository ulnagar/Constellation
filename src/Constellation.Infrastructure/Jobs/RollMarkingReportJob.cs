﻿using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class RollMarkingReportJob : IRollMarkingReportJob, IScopedService, IHangfireJob
    {
        private readonly ISentralGateway _sentralService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<IRollMarkingReportJob> _logger;

        public RollMarkingReportJob(ISentralGateway sentralService, IUnitOfWork unitOfWork, 
            IEmailService emailService, ILogger<IRollMarkingReportJob> logger)
        {
            _sentralService = sentralService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task StartJob()
        {
            var date = DateTime.Today;

            if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                return;

            _logger.LogInformation($"Checking Date: {date.ToShortDateString()}");
            var entries = await _sentralService.GetRollMarkingReportAsync(date);

            var unsubmitted = entries.Where(entry => !entry.Submitted).ToList();

            if (unsubmitted.Any())
            {
                foreach (var entry in unsubmitted.OrderBy(item => item.ClassName).ThenBy(item => item.Period))
                {
                    var offeringName = entry.ClassName.PadLeft(6, '0');
                    entry.ClassName = offeringName;

                    var offering = await _unitOfWork.CourseOfferings.GetFromYearAndName(date.Year, offeringName);
                    var covers = new List<ClassCover>();

                    if (offering != null)
                    {
                        var casualCovers = await _unitOfWork.CasualClassCovers.ForClassworkNotifications(date, offering.Id);
                        var teacherCovers = await _unitOfWork.TeacherClassCovers.ForClassworkNotifications(date, offering.Id);

                        foreach (var cover in casualCovers)
                            covers.Add(cover);
                        foreach (var cover in teacherCovers)
                            covers.Add(cover);
                    }

                    if (covers.Any())
                    {
                        entry.Covered = true;
                        entry.HeadTeacher = offering.Course.HeadTeacher.DisplayName;
                        entry.EmailSentTo = offering.Course.HeadTeacher.EmailAddress;

                        foreach (var cover in covers)
                        {
                            if (cover.GetType().Name == nameof(CasualClassCover))
                            {
                                entry.Covered = true;
                                entry.CoveredBy = ((CasualClassCover)cover).Casual.DisplayName;
                                entry.CoverType = "Casual";
                            }

                            if (cover.GetType().Name == nameof(TeacherClassCover))
                            {
                                entry.Covered = true;
                                entry.CoveredBy = ((TeacherClassCover)cover).Staff.DisplayName;
                                entry.CoverType = "Teacher";
                            }

                            var infoString = $"{entry.Date.ToShortDateString()} - Unsubmitted Roll for {entry.ClassName} ({entry.Teacher}) in Period {entry.Period} covered by {entry.CoveredBy} ({entry.CoverType})";

                            _logger.LogInformation(infoString);
                            entry.Description = infoString;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Teacher))
                        {
                            var teacher = await _unitOfWork.Staff.GetFromName(entry.Teacher);
                            if (teacher != null)
                                entry.EmailSentTo = teacher.EmailAddress;
                            else if (offering != null)
                                entry.EmailSentTo = offering.Course.HeadTeacher.EmailAddress;
                            else
                                entry.EmailSentTo = "auroracoll-h.school@det.nsw.edu.au";
                        }
                        else if (offering != null)
                            entry.EmailSentTo = offering.Course.HeadTeacher.EmailAddress;
                        else
                            entry.EmailSentTo = "auroracoll-h.school@det.nsw.edu.au";

                        var infoString = $"{entry.Date.ToShortDateString()} - Unsubmitted Roll for {entry.ClassName}";
                        if (!string.IsNullOrWhiteSpace(entry.Teacher))
                            infoString += $" by {entry.Teacher}";
                        infoString += $" in Period {entry.Period}";

                        _logger.LogInformation(infoString);
                        entry.Description = infoString;
                    }
                }

                var groupedEntries = unsubmitted.GroupBy(item => item.EmailSentTo);

                foreach (var group in groupedEntries)
                {
                    // Email the relevant person (as outlined in the EmailSentTo field
                    var orderedEntries = group.OrderBy(item => item.Date).ThenBy(item => item.ClassName).ThenBy(item => item.Period).ToList();

                    await _emailService.SendDailyRollMarkingReport(orderedEntries, false);
                }
            }
            else
            {
                var infoString = $"{date.ToShortDateString()} - No Unsubmitted Rolls found!";

                _logger.LogInformation(infoString);
            }

            if (unsubmitted.Any())
                await _emailService.SendDailyRollMarkingReport(unsubmitted, true);
        }
    }
}
