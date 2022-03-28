using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class AbsenceProcessingJob : IAbsenceProcessingJob, IScopedService, IHangfireJob
    {
        private readonly ISentralGateway _sentralService;
        private readonly IEmailService _emailService;
        private readonly ILogger<IAbsenceMonitorJob> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private Student _student;
        private List<DateTime> _excludedDates = new List<DateTime>(); 
        private AppSettings.AbsencesModule _appSettings;

        public AbsenceProcessingJob(IUnitOfWork unitOfWork, ISentralGateway sentralService,
            IEmailService emailService, ILogger<IAbsenceMonitorJob> logger)
        {
            _unitOfWork = unitOfWork;
            _sentralService = sentralService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ICollection<Absence>> StartJob(Student student)
        {
            _student = student;
            _appSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            _logger.LogInformation(" Scanning student {student} ({grade})", student.DisplayName, student.CurrentGrade);

            var returnAbsences = new List<Absence>();

            var sentralId = student.SentralStudentId;
            if (string.IsNullOrWhiteSpace(sentralId))
            {
                // This student doesn't have an identified sentral id!
                student.SentralStudentId = await _sentralService.GetSentralStudentIdFromSRN(student.StudentId, ((int)student.CurrentGrade).ToString());

                sentralId = student.SentralStudentId;

                if (string.IsNullOrWhiteSpace(sentralId))
                {
                    // Send warning email to Technology Support Team
                    await _emailService.SendAdminAbsenceSentralAlert(student);
                    return returnAbsences;
                }
            }

            _excludedDates = await _sentralService.GetExcludedDatesFromCalendar(DateTime.Today.Year.ToString());

            var pxpAbsences = await _sentralService.GetAbsenceDataAsync(sentralId);
            var attendanceAbsences = await _sentralService.GetPartialAbsenceDataAsync(sentralId);

            foreach (var attendAbsence in attendanceAbsences.Where(aa => !aa.WholeDay))
            {
                var absenceLength = attendAbsence.EndTime.Subtract(attendAbsence.StartTime).TotalMinutes;

                attendAbsence.MinutesAbsent = Convert.ToInt32(absenceLength);
            }

            if (pxpAbsences == null || pxpAbsences.Count == 0)
                return returnAbsences;

            var groupedPxpAbsences = pxpAbsences.GroupBy(a => a.Date.Date);

            foreach (var group in groupedPxpAbsences)
            {
                if (group.Key.Date < GetEarliestDate())
                    continue;

                // Get the timetable for the day
                // Given the date, figure out what day of the cycle we are looking at.
                var cycleDay = group.Key.Date.GetDayNumber();

                // Get all enrolments for this student that were active on that date using the day of the cycle we identified above
                var courseEnrolments = student.Enrolments
                    .Where(enrol =>
                        enrol.DateCreated < group.Key.Date &&
                        (!enrol.IsDeleted || enrol.DateDeleted.Value.Date > group.Key.Date) &&
                        enrol.Offering.EndDate > group.Key.Date &&
                        enrol.Offering.Sessions.Any(session =>
                            session.DateCreated < group.Key.Date &&
                            (!session.IsDeleted || session.DateDeleted.Value.Date > group.Key.Date) &&
                            session.Period.Day == cycleDay))
                    .ToList();

                foreach (var courseEnrolment in courseEnrolments)
                {
                    if (!courseEnrolment.Offering.Name.Contains(group.First().ClassName))
                    {
                        // The PxP absence is for a different class than the courseEnrolment
                        // therefore it should not be processed here.
                        //_logger.LogInformation($"-- Enrolment Class {courseEnrolment.Offering.Name} does not match PxP Absence Class {group.First().ClassName}");

                        continue;
                    }

                    // Get list of periods for this class on this day
                    var periods = courseEnrolment.Offering
                        .Sessions
                        .Where(session => session.DateCreated < group.Key.Date &&
                                          (!session.IsDeleted || session.DateDeleted.Value.Date > group.Key.Date) &&
                                          session.Period.Day == cycleDay)
                        .Select(session => session.Period)
                        .OrderBy(period => period.StartTime)
                        .ToList();

                    // Find all contiguous periods
                    var periodGroups = periods.GroupConsecutive();

                    foreach (var periodGroup in periodGroups)
                    {
                        var coursePeriods = periodGroup.OrderBy(period => period.StartTime).ToList();

                        var totalBlockMinutes = coursePeriods.Select(period => (int)period.EndTime.Subtract(period.StartTime).TotalMinutes).Sum();

                        // This should be a single contiguous block of periods for the day.
                        // Find all absences from the day (group) that occur during this periodGroup
                        var absencesToProcess = group.Where(absence => coursePeriods.Any(period => period.GetPeriodDescriptor() == absence.Period)).ToList();

                        if (absencesToProcess.Count == 0)
                            continue;

                        // Ignore absences from TODAY only
                        if (absencesToProcess.First().Date.Date == DateTime.Today.Date)
                            continue;

                        var totalAbsenceTime = 0;

                        foreach (var absenceTime in absencesToProcess.ToList())
                        {
                            if (absenceTime.Type == SentralPeriodAbsenceDto.Whole)
                            {
                                var period = coursePeriods.First(timetableperiod => timetableperiod.GetPeriodDescriptor() == absenceTime.Period);
                                absenceTime.MinutesAbsent = (int)period.EndTime.Subtract(period.StartTime).TotalMinutes;
                            }

                            totalAbsenceTime += absenceTime.MinutesAbsent;
                        }

                        if (totalBlockMinutes == totalAbsenceTime)
                        {
                            // These absences grouped together form a Whole Absence
                            var absenceRecord = ProcessWholeAbsence(absencesToProcess, attendanceAbsences, courseEnrolment, periodGroup.ToList(), totalAbsenceTime);

                            if (absenceRecord != null)
                            {
                                returnAbsences.Add(absenceRecord);
                            }

                            continue;
                        }

                        // These absences cannot be grouped together, therefore are partial
                        var detectedAbsences = new List<Absence>();

                        // If the absence is too early, skip
                        if (absencesToProcess.First().Date < GetEarliestDate())
                            continue;

                        // If there are no AttendanceAbsences to match to, Partials cannot be created, Skip
                        if (attendanceAbsences.Count == 0)
                            continue;

                        foreach (var absence in absencesToProcess)
                        {
                            var absenceRecord = ProcessPartialAbsence(absence, attendanceAbsences, courseEnrolment, periodGroup.ToList());

                            if (absenceRecord != null)
                                detectedAbsences.Add(absenceRecord);
                        }

                        // Detect consecutive absences and combine
                        foreach (var resource in detectedAbsences.OrderBy(absence => absence.StartTime).ToList())
                        {
                            // If no other detected absence starts when this one finishes, it is not contiguous with anything else
                            if (detectedAbsences.All(absence => absence.StartTime != resource.EndTime))
                                continue;

                            var firstAbsence = resource;
                            var secondAbsence = detectedAbsences.First(absence => absence.StartTime == firstAbsence.EndTime);

                            // Ignore if only one of the absences has been detected as explained
                            if ((resource.Explained && !secondAbsence.Explained) || (!resource.Explained && secondAbsence.Explained))
                                continue;

                            // Remove the second absence and update the first to cover the extended time frame.
                            detectedAbsences.Remove(secondAbsence);

                            firstAbsence.AbsenceReason = FindWorstAbsenceReason(new List<string> { firstAbsence.AbsenceReason, secondAbsence.AbsenceReason });

                            firstAbsence.EndTime = secondAbsence.EndTime;
                            firstAbsence.AbsenceTimeframe = $"{firstAbsence.StartTime.As12HourTime()} - {secondAbsence.EndTime.As12HourTime()}";
                            firstAbsence.AbsenceLength += secondAbsence.AbsenceLength;
                        }

                        foreach (var newAbsence in detectedAbsences.Where(absence => absence.AbsenceLength > _appSettings.PartialLengthThreshold).ToList())
                        {
                            // Check database for all matching absences already known
                            if (IsNewAbsence(newAbsence, student, _appSettings.DiscountedPartialReasons))
                            {
                                // Add the absence to the database
                                if (newAbsence.Explained)
                                    _logger.LogInformation("  Found new externally explained {Type} absence on {Date} - {PeriodName}", newAbsence.Type, newAbsence.Date.ToShortDateString(), newAbsence.PeriodName);
                                else
                                    _logger.LogInformation("  Found new unexplained {Type} absence on {Date} - {PeriodName} - {AbsenceReason}", newAbsence.Type, newAbsence.Date.ToShortDateString(), newAbsence.PeriodName, newAbsence.AbsenceReason);

                                returnAbsences.Add(newAbsence);
                            }
                        }
                    }
                }
            }

            pxpAbsences = null;
            attendanceAbsences = null;

            return returnAbsences;
        }

        private DateTime GetEarliestDate()
        {
            if (_student.AbsenceNotificationStartDate.HasValue && _student.AbsenceNotificationStartDate.Value > _appSettings.AbsenceScanStartDate)
            {
                return _student.AbsenceNotificationStartDate.Value;
            }
            else
            {
                return _appSettings.AbsenceScanStartDate;
            }
        }

        private void CalculatePxPAbsenceTimes(SentralPeriodAbsenceDto absence, IList<TimetablePeriod> periodGroup)
        {
            var period = (absence.Period.Contains('S'))
                ? periodGroup.FirstOrDefault(pg => pg.Name.Contains(absence.Period.Remove(0, 1)))
                : periodGroup.FirstOrDefault(pg => pg.Name.Contains(absence.Period));

            var absenceLength = new TimeSpan(0, absence.MinutesAbsent, 0);

            switch (absence.PartialType)
            {
                case "Late Arrival":
                case null:
                    absence.StartTime = period.StartTime;
                    absence.EndTime = period.StartTime.Add(absenceLength);

                    break;
                case "Early Leaver":
                    absence.EndTime = period.EndTime;
                    absence.StartTime = period.EndTime.Subtract(absenceLength);

                    break;
                default:
                    break;
            }
        }

        private void CalculateWebAttendAbsencePeriod(SentralPeriodAbsenceDto absence, IList<TimetablePeriod> periodGroup)
        {
            foreach (var period in periodGroup)
            {
                if (period.StartTime <= absence.StartTime && period.EndTime >= absence.EndTime)
                {
                    absence.Period = period.Name;

                    if (absence.MinutesAbsent != period.EndTime.Subtract(period.StartTime).TotalMinutes)
                    {
                        if (absence.StartTime == period.StartTime)
                        {
                            absence.PartialType = "Late Arrival";
                        }
                        else if (absence.EndTime == period.EndTime)
                        {
                            absence.PartialType = "Early Leaver";
                        }
                        else
                        {
                            absence.PartialType = "Timed";
                        }
                    }
                }
            }
        }

        private SentralPeriodAbsenceDto SelectBestWebAttendEntryForPartialAbsence(SentralPeriodAbsenceDto absence, IList<SentralPeriodAbsenceDto> webAttendAbsences)
        {
            // Excluded absences (either dates that are not valid, or length below thresholds):
            if (absence.MinutesAbsent == 5)
            {
                _logger.LogInformation("  Found absence below Sentral threshold on {Date} - {Period} - {Reason}", absence.Date.ToShortDateString(), absence.Period, absence.Reason);
                return null;
            }

            if (absence.Date.Date > DateTime.Today)
            {
                // This absence is in the future. Likely the PxP roll has been opened and an unavilability automatically created,
                // but the roll has not been submitted, meaning no correlating entry in WebAttend.
                return null;
            }

            if (_excludedDates.Contains(absence.Date.Date))
            {
                return null;
            }

            // Check for a whole day WebAttendance Absence entry first
            var wholeDayAttendanceAbsence = webAttendAbsences
                .Where(aa => aa.Date.Date == absence.Date.Date && aa.WholeDay)
                .ToList();

            if (wholeDayAttendanceAbsence.Count == 1)
                return wholeDayAttendanceAbsence.First();

            if (absence.StartTime != new TimeSpan())
            {
                var exactAttendanceAbsence = webAttendAbsences
                    .Where(aa => aa.Date.Date == absence.Date.Date && aa.StartTime == absence.StartTime && aa.MinutesAbsent == absence.MinutesAbsent)
                    .ToList();

                if (exactAttendanceAbsence.Count == 1)
                    return exactAttendanceAbsence.First();

                var approxAttendanceAbsence = webAttendAbsences
                    .Where(aa => aa.Date.Date == absence.Date.Date && aa.StartTime <= absence.StartTime && aa.EndTime >= absence.EndTime)
                    .ToList();

                if (approxAttendanceAbsence.Count == 1)
                    return approxAttendanceAbsence.First();
            }

            // This is a timed partial?
            if (absence.PartialType == "Timed")
            {
                var bestGuessWebAttendAbsences = webAttendAbsences.Where(aa =>
                        aa.Date.Date == absence.Date.Date &&
                        aa.Reason == absence.Reason &&
                        aa.MinutesAbsent == absence.MinutesAbsent)
                    .ToList();

                if (bestGuessWebAttendAbsences.Count == 1)
                    return bestGuessWebAttendAbsences.First();

                if (bestGuessWebAttendAbsences.Count == 0)
                {
                    var nextBestGuessWebAttendAbsences = webAttendAbsences.Where(aa =>
                            aa.Date.Date == absence.Date.Date &&
                            aa.MinutesAbsent == absence.MinutesAbsent)
                        .ToList();

                    if (nextBestGuessWebAttendAbsences.Count == 1)
                        return nextBestGuessWebAttendAbsences.First();
                }

                if (bestGuessWebAttendAbsences.Count > 1)
                {
                    // How do we tell these apart?
                    // Can we match the period for the WebAttend absences to the PxP absence entry?

                    var timetable = (_student.CurrentGrade == Grade.Y05 || _student.CurrentGrade == Grade.Y06)
                        ? "PRIMARY"
                        : "SECONDARY";

                    var periods = _unitOfWork.Periods.AllWithFilter(p => !p.IsDeleted && p.Timetable == timetable).ToList();

                    foreach (var webAttendAbsence in bestGuessWebAttendAbsences)
                    {
                        CalculateWebAttendAbsencePeriod(webAttendAbsence, periods);
                    }

                    var lastAttemptWebAttendAbsences = bestGuessWebAttendAbsences.Where(aa =>
                            aa.Period.Contains(absence.Period) &&
                            aa.PartialType == absence.PartialType)
                        .ToList();

                    if (lastAttemptWebAttendAbsences.Count == 1)
                        return lastAttemptWebAttendAbsences.First();
                }
            }

            // UNABLE TO MATCH ABSENCE FROM PXP TO ATTENDANCE (NO RECORD FOUND)
            // This could be because: 
            //      The PxP roll has not actually been submitted, making the absences detected there not accurate yet.
            //      The PxP absence has been incorrectly created (e.g. creating a Timed absence that should be a Late Arrival or Early Leaver)
            //      The Attendance absence has been deleted for some reason
            return null;
        }

        private SentralPeriodAbsenceDto SelectBestWebAttendEntryForWholeAbsence(IList<SentralPeriodAbsenceDto> absencesToProcess, IList<SentralPeriodAbsenceDto> webAttendAbsences, Absence absence)
        {
            if (absence.Date.Date > DateTime.Today)
            {
                // This absence is in the future. Likely the PxP roll has been opened and an unavilability automatically created,
                // but the roll has not been submitted, meaning no correlating entry in WebAttend.
                return null;
            }

            if (_excludedDates.Contains(absence.Date.Date))
            {
                return null;
            }

            var fromDayAttendanceAbsences = webAttendAbsences
                .Where(aa => aa.Date.Date == absence.Date.Date)
                .ToList();

            // Check for a whole day WebAttendance Absence entry first
            var wholeDayAttendanceAbsence = fromDayAttendanceAbsences
                .Where(aa => aa.WholeDay)
                .ToList();

            if (wholeDayAttendanceAbsence.Count == 1)
                return wholeDayAttendanceAbsence.First();

            // Check for exact match on start and end times
            var forBlockAttendanceAbsences = fromDayAttendanceAbsences
                .Where(aa => aa.StartTime == absence.StartTime && aa.EndTime == absence.EndTime)
                .ToList();

            if (forBlockAttendanceAbsences.Count == 1)
                return forBlockAttendanceAbsences.First();

            // Check for a larger webAttendAbsence that covers this absence
            var largerBlockAttendanceAbsences = fromDayAttendanceAbsences
                .Where(aa => aa.StartTime <= absence.StartTime && aa.EndTime >= absence.EndTime)
                .ToList();

            if (largerBlockAttendanceAbsences.Count == 1)
                return largerBlockAttendanceAbsences.First();

            // Check for multiple webAttendAbsences that combine to cover this absence
            var combinedBlockAttendanceAbsences = fromDayAttendanceAbsences
                .Where(aa => (aa.StartTime >= absence.StartTime && aa.StartTime < absence.EndTime) ||
                    (aa.StartTime <= absence.StartTime && aa.EndTime > absence.StartTime))
                .ToList();

            if (combinedBlockAttendanceAbsences.Count > 0)
            {
                var adjustedAbsenceLength = absencesToProcess.Where(ab => ab.MinutesAbsent > 5).Sum(ab => ab.MinutesAbsent);

                var webAttendStartTime = combinedBlockAttendanceAbsences.Min(aa => aa.StartTime);
                var webAttendEndTime = combinedBlockAttendanceAbsences.Max(aa => aa.EndTime);

                if ((webAttendStartTime == absence.StartTime && webAttendEndTime == absence.EndTime && combinedBlockAttendanceAbsences.Sum(aa => aa.MinutesAbsent) == absencesToProcess.Sum(ab => ab.MinutesAbsent)) ||
                    combinedBlockAttendanceAbsences.Sum(aa => aa.MinutesAbsent) == adjustedAbsenceLength)
                {
                    return new SentralPeriodAbsenceDto
                    {
                        ExternalExplanation = combinedBlockAttendanceAbsences.FirstOrDefault(aa => !string.IsNullOrWhiteSpace(aa.ExternalExplanation))?.ExternalExplanation,
                        ExternalExplanationSource = combinedBlockAttendanceAbsences.FirstOrDefault(aa => !string.IsNullOrWhiteSpace(aa.ExternalExplanation))?.ExternalExplanationSource,
                    };
                }
            }

            return new SentralPeriodAbsenceDto
            {
                ExternalExplanation = "No Attendance entry found: suspect deleted and no longer valid",
                ExternalExplanationSource = "ACOS SYSTEM",
            };
        }

        private Absence ProcessPartialAbsence(SentralPeriodAbsenceDto absence, IList<SentralPeriodAbsenceDto> webAttendAbsences, Enrolment courseEnrolment, IList<TimetablePeriod> periodGroup)
        {
            // Can we figure out when the (PxP) absence starts and ends?
            CalculatePxPAbsenceTimes(absence, periodGroup);

            // If we did figure this out, is there an (Attendance) absence that either exactly matches, or covers this timeframe?
            var attendanceAbsence = SelectBestWebAttendEntryForPartialAbsence(absence, webAttendAbsences);
            if (attendanceAbsence == null)
                return null;

            // Create an object to save this data to the database.
            var absenceRecord = CreateAbsence(new List<SentralPeriodAbsenceDto> { absence }, courseEnrolment, Absence.Partial, absence.MinutesAbsent, absence.Reason, periodGroup);

            if (attendanceAbsence.Timeframe == "Whole Day")
            {
                // This Attendance entry explains the partial. Should create an explained entry.
                var startTime = absence.StartTime;
                var endTime = absence.EndTime;

                absenceRecord.StartTime = startTime;
                absenceRecord.EndTime = endTime;
                absenceRecord.AbsenceTimeframe = $"{startTime.As12HourTime()} - {endTime.As12HourTime()}";
            }
            else
            {
                // This Attendance entry correlates to the partial.
                if ((absence.StartTime != new TimeSpan() && absence.StartTime != attendanceAbsence.StartTime) || (absence.EndTime != new TimeSpan() && absence.EndTime != attendanceAbsence.EndTime))
                {
                    // TIMES DO NOT MATCH!
                    absenceRecord.StartTime = (attendanceAbsence.StartTime > absence.StartTime) ? attendanceAbsence.StartTime : absence.StartTime;
                    absenceRecord.EndTime = (attendanceAbsence.EndTime < absence.EndTime) ? attendanceAbsence.EndTime : absence.EndTime;
                    absenceRecord.AbsenceTimeframe = $"{absenceRecord.StartTime.As12HourTime()} - {absenceRecord.EndTime.As12HourTime()}";
                }
                else
                {
                    absenceRecord.StartTime = attendanceAbsence.StartTime;
                    absenceRecord.EndTime = attendanceAbsence.EndTime;
                    absenceRecord.AbsenceTimeframe = $"{absenceRecord.StartTime.As12HourTime()} - {absenceRecord.EndTime.As12HourTime()}";
                }
            }

            // If this absence was explained using an Accepted Partial Absence Reason, then the absence should be created as explained
            if (_appSettings.DiscountedPartialReasons.Contains(absence.Reason))
            {
                // This absence has been externally explained
                if (attendanceAbsence != null)
                {
                    absenceRecord.ExternalExplanation = attendanceAbsence.ExternalExplanation;
                    absenceRecord.ExternalExplanationSource = attendanceAbsence.ExternalExplanationSource;
                }

                absenceRecord.ExternallyExplained = true;
            }

            return absenceRecord;
        }

        private Absence ProcessWholeAbsence(IList<SentralPeriodAbsenceDto> absencesToProcess, IList<SentralPeriodAbsenceDto> webAttendAbsences, Enrolment courseEnrolment, IList<TimetablePeriod> periodGroup, int totalAbsenceTime)
        {
            if (absencesToProcess.First().Date < GetEarliestDate())
                return null;

            // Calculate acceptable reason
            var reasons = absencesToProcess.Select(absence => absence.Reason).Distinct().ToList();
            var reason = (reasons.Count() == 1) ? reasons.First() : FindWorstAbsenceReason(reasons);

            // Create an object to save this data to the database.
            var absenceRecord = CreateAbsence(absencesToProcess, courseEnrolment, Absence.Whole, totalAbsenceTime, reason, periodGroup);

            // Find a webAttend absence that covers this set
            var attendanceAbsence = SelectBestWebAttendEntryForWholeAbsence(absencesToProcess, webAttendAbsences, absenceRecord);
            if (attendanceAbsence == null)
            {
                // The webAttend entry may have been deleted for some reason. This absence needs to be checked against the database to make sure
                // it doesn't exist in there as an UNEXPLAINED absence, which will trigger a reminder email later.

                // We don't actually care IF it is a new absence, only that existing absences will be updated with the explanation if appropriate.
                IsNewAbsence(absenceRecord, _student, _appSettings.DiscountedWholeReasons);

                return null;
            }

            // If any part of this whole class absence was explained using an Accepted Whole Absence Reason, then the absence should not be processed as whole.
            if (absencesToProcess.All(absence => absence.Reason == "Shared Enrolment") || absencesToProcess.Any(absence => _appSettings.DiscountedWholeReasons.Contains(absence.Reason)))
            {
                // This absence has been externally explained
                if (attendanceAbsence != null)
                {
                    absenceRecord.ExternalExplanation = attendanceAbsence.ExternalExplanation;
                    absenceRecord.ExternalExplanationSource = attendanceAbsence.ExternalExplanationSource;
                }

                absenceRecord.ExternallyExplained = true;
            }

            // Detect duplicates in database and process accordingly
            if (IsNewAbsence(absenceRecord, _student, _appSettings.DiscountedWholeReasons))
            {
                if (absenceRecord.Explained)
                    _logger.LogInformation("  Found new externally explained {Type} absence on {Date} - {PeriodName}", absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName);
                else
                    _logger.LogInformation("  Found new unexplained {Type} absence on {Date} - {PeriodName} - {AbsenceReason}", absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName, absenceRecord.AbsenceReason);

                return absenceRecord;
            }

            return null;
        }

        private static string FindWorstAbsenceReason(ICollection<string> reasons)
        {
            if (reasons.Any(reason => reason == "Unjustified"))
                return "Unjustified";

            if (reasons.Any(reason => reason == "Absent"))
                return "Absent";

            if (reasons.Any(reason => reason == "Suspended"))
                return "Suspended";

            if (reasons.Any(reason => reason == "Exempt"))
                return "Exempt";

            if (reasons.Any(reason => reason == "Leave"))
                return "Leave";

            if (reasons.Any(reason => reason == "Flexible"))
                return "Flexible";

            if (reasons.Any(reason => reason == "Sick"))
                return "Sick";

            if (reasons.Any(reason => reason == "School Business"))
                return "School Business";

            if (reasons.Any(reason => reason == "Shared Enrolment"))
                return "Shared Enrolment";

            return "Unknown";
        }

        private Absence CreateAbsence(IList<SentralPeriodAbsenceDto> absencesToProcess, Enrolment courseEnrolment, string type, int length, string reason, IList<TimetablePeriod> periodGroup)
        {
            var absenceRecord = new Absence
            {
                Date = absencesToProcess.First().Date,
                OfferingId = courseEnrolment.OfferingId,
                StudentId = courseEnrolment.StudentId,
                Type = type,
                AbsenceLength = length,
                AbsenceReason = reason,
                DateScanned = DateTime.Today,
                LastSeen = DateTime.Today
            };

            if (periodGroup.Count() > 1)
            {
                absenceRecord.PeriodName = $"{periodGroup.First().Name} - {periodGroup.Last().Name}";
                absenceRecord.PeriodTimeframe = $"{periodGroup.First().StartTime.As12HourTime()} - {periodGroup.Last().EndTime.As12HourTime()}";
                absenceRecord.AbsenceTimeframe = absenceRecord.PeriodTimeframe;
                absenceRecord.StartTime = periodGroup.First().StartTime;
                absenceRecord.EndTime = periodGroup.Last().EndTime;
            }
            else
            {
                absenceRecord.PeriodName = periodGroup.First().Name;
                absenceRecord.PeriodTimeframe = $"{periodGroup.First().StartTime.As12HourTime()} - {periodGroup.Last().EndTime.As12HourTime()}";
                absenceRecord.AbsenceTimeframe = absenceRecord.PeriodTimeframe;
                absenceRecord.StartTime = periodGroup.First().StartTime;
                absenceRecord.EndTime = periodGroup.Last().EndTime;
            }

            return absenceRecord;
        }

        private bool IsNewAbsence(Absence absence, Student student, string acceptedReasons)
        {
            var existingAbsences = student.Absences
                .Where(existingAbsence =>
                    existingAbsence.Date == absence.Date &&
                    existingAbsence.OfferingId == absence.OfferingId &&
                    existingAbsence.AbsenceTimeframe == absence.AbsenceTimeframe)
                .ToList();

            // If there are any
            if (existingAbsences.Count > 0)
            {
                foreach (var existingAbsence in existingAbsences)
                {
                    // Update the last seen property with today's date
                    existingAbsence.LastSeen = DateTime.Now;

                    // If the new absence is explained with an accepted reason, and the existing absences are not, then update them to signify they were changed on Sentral
                    if (existingAbsences.All(innerabsence => !innerabsence.Explained) && acceptedReasons.Contains(absence.AbsenceReason))
                    {
                        _logger.LogInformation("  Found external explaination for {Type} absence on {Date} - {PeriodName}", absence.Type, absence.Date.ToShortDateString(), absence.PeriodName);
                        existingAbsence.ExternallyExplained = true;
                        existingAbsence.ExternalExplanation = absence.ExternalExplanation;
                        existingAbsence.ExternalExplanationSource = absence.ExternalExplanationSource;
                    }
                }

                // Since it already exists, we don't want to add it to the database, get the next absence instead!
                return false;
            }

            return true;
        }
    }
}
