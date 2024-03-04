namespace Constellation.Infrastructure.Jobs;

using Application.Extensions;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Core.Extensions;
using Microsoft.Extensions.Options;
using Serilog.Context;
using System.Threading;

internal sealed class AbsenceProcessingJob : IAbsenceProcessingJob
{
    private readonly ISentralGateway _sentralService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IAbsenceRepository _absenceRepository;

    private Student _student;
    private List<DateOnly> _excludedDates = new(); 
    private readonly AppConfiguration _configuration;
    private Guid JobId { get; set; }

    public AbsenceProcessingJob(
        IOptions<AppConfiguration> configuration,
        IOfferingRepository offeringRepository,
        ITimetablePeriodRepository periodRepository,
        IAbsenceRepository absenceRepository,
        ISentralGateway sentralService,
        IEmailService emailService, 
        ILogger logger)
    {
        _configuration = configuration.Value;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _absenceRepository = absenceRepository;
        _sentralService = sentralService;
        _emailService = emailService;

        _logger = logger.ForContext<IAbsenceMonitorJob>();
    }

    public async Task<List<Absence>> StartJob(Guid jobId, Student student, CancellationToken cancellationToken)
    {
        JobId = jobId;
        _student = student;
        LogContext.PushProperty(nameof(JobId), JobId);

        _logger.Information("{id}: Scanning student {student} ({grade})", JobId, student.DisplayName, student.CurrentGrade.AsName());

        List<Absence> returnAbsences = new();

        List<DateOnly> activePartialScanDates = new();
        List<DateOnly> activeWholeScanDates = new();

        foreach (AbsenceConfiguration config in student.AbsenceConfigurations)
        {
            if (config.IsDeleted || config.CalendarYear != DateTime.Today.Year)
                continue;

            if (config.AbsenceType == AbsenceType.Partial)
            {
                List<DateOnly> validDates = config.ScanStartDate.Range(config.ScanEndDate);

                foreach (DateOnly validDate in validDates)
                {
                    if (validDate >= DateOnly.FromDateTime(DateTime.Today))
                        continue;

                    activePartialScanDates.Add(validDate);
                }
            }

            if (config.AbsenceType == AbsenceType.Whole)
            {
                List<DateOnly> validDates = config.ScanStartDate.Range(config.ScanEndDate);

                foreach (DateOnly validDate in validDates)
                {
                    if (validDate >= DateOnly.FromDateTime(DateTime.Today))
                        continue;

                    activeWholeScanDates.Add(validDate);
                }
            }
        }

        if (activePartialScanDates.Count == 0 && activeWholeScanDates.Count == 0)
        {
            _logger.Information("{id}: - Student absence scanning disabled. Skipping.", JobId);

            return returnAbsences;
        }

        string sentralId = student.SentralStudentId;
        if (string.IsNullOrWhiteSpace(sentralId))
        {
            if (cancellationToken.IsCancellationRequested)
                return returnAbsences;

            // This student doesn't have an identified sentral id!
            student.SentralStudentId = await _sentralService
                .GetSentralStudentIdFromSRN(
                    student.StudentId, 
                    ((int)student.CurrentGrade).ToString());

            sentralId = student.SentralStudentId;

            if (string.IsNullOrWhiteSpace(sentralId))
            {
                // Send warning email to Technology Support Team
                _logger.Error("Cannot find Sentral Id for student {student}", student.DisplayName);
                await _emailService.SendAdminAbsenceSentralAlert(student.DisplayName);
                return returnAbsences;
            }
        }

        if (!_excludedDates.Any())
            _excludedDates = await _sentralService.GetExcludedDatesFromCalendar(DateTime.Today.Year.ToString());

        List<SentralPeriodAbsenceDto> pxpAbsences = await _sentralService.GetAbsenceDataAsync(sentralId);
        List<SentralPeriodAbsenceDto> attendanceAbsences = await _sentralService.GetPartialAbsenceDataAsync(sentralId);

        // If the webattend absence is not a whole day absence, calculate the absence length
        foreach (SentralPeriodAbsenceDto attendAbsence in attendanceAbsences.Where(aa => !aa.WholeDay))
        {
            if (cancellationToken.IsCancellationRequested)
                return returnAbsences;

            double absenceLength = (attendAbsence.EndTime - attendAbsence.StartTime).TotalMinutes;

            attendAbsence.MinutesAbsent = Convert.ToInt32(absenceLength);
        }

        // If there are no pxpabsences, return empty list
        if (pxpAbsences == null || pxpAbsences.Count == 0)
            return returnAbsences;

        var groupedPxpAbsences = pxpAbsences.GroupBy(a => a.Date);

        foreach (var group in groupedPxpAbsences)
        {
            if (cancellationToken.IsCancellationRequested)
                return returnAbsences;

            // If the date of this group of absences is not a valid scan date, skip
            if (!activePartialScanDates.Contains(group.Key) && !activeWholeScanDates.Contains(group.Key))
                continue;

            // Get the timetable for the day
            // Given the date, figure out what day of the cycle we are looking at.
            int cycleDay = group.Key.GetDayNumber();

            // Get all enrolments for this student that were active on that date using the day of the cycle we identified above
            List<Offering> enrolledOfferings = await _offeringRepository
                .GetCurrentEnrolmentsFromStudentForDate(
                    student.StudentId, 
                    group.Key, 
                    cycleDay, 
                    cancellationToken);
            
            foreach (Offering enrolledOffering in enrolledOfferings)
            {
                if (cancellationToken.IsCancellationRequested)
                    return returnAbsences;

                if (!enrolledOffering.Name.Value.Contains(group.First().ClassName))
                {
                    // The PxP absence is for a different class than the courseEnrolment
                    // therefore it should not be processed here.
                    
                    _logger
                        .ForContext("Absences", group, true)
                        .Warning($"-- Enrolment Class {enrolledOffering.Name} does not match PxP Absence Class {group.First().ClassName}");
                    continue;
                }

                // Get list of periods for this class on this day
                List<TimetablePeriod> periods = await _periodRepository.GetForOfferingOnDay(enrolledOffering.Id, group.Key, cycleDay, cancellationToken);
                    
                // Find all contiguous periods
                var periodGroups = periods.GroupConsecutive();

                foreach (var periodGroup in periodGroups)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return returnAbsences;

                    List<TimetablePeriod> coursePeriods = periodGroup
                        .OrderBy(period => period.StartTime)
                        .ToList();

                    // Calculate the length of the timetabled lesson
                    int totalBlockMinutes = coursePeriods
                        .Select(period => 
                            (int)period.EndTime
                                .Subtract(period.StartTime)
                                .TotalMinutes)
                        .Sum();

                    // This should be a single contiguous block of periods for the day.
                    // Find all absences from the day (group) that occur during this periodGroup
                    List<SentralPeriodAbsenceDto> absencesToProcess = group
                        .Where(absence => 
                            coursePeriods.Any(period => 
                                period.GetPeriodDescriptor() == absence.Period))
                        .ToList();

                    if (absencesToProcess.Count == 0)
                        continue;

                    // Ignore absences from TODAY only
                    if (absencesToProcess.First().Date == DateOnly.FromDateTime(DateTime.Today))
                        continue;

                    int totalAbsenceTime = 0;

                    foreach (SentralPeriodAbsenceDto absenceTime in absencesToProcess)
                    {
                        if (absenceTime.Type == SentralPeriodAbsenceDto.Whole)
                        {
                            TimetablePeriod period = coursePeriods
                                .First(timetableperiod => 
                                    timetableperiod.GetPeriodDescriptor() == absenceTime.Period);

                            absenceTime.MinutesAbsent = (int)period.EndTime.Subtract(period.StartTime).TotalMinutes;
                        }

                        totalAbsenceTime += absenceTime.MinutesAbsent;
                    }

                    if (totalBlockMinutes == totalAbsenceTime)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return returnAbsences;

                        // If the absence is not on a valid scan date, skip
                        if (!activeWholeScanDates.Contains(absencesToProcess.First().Date))
                            continue;

                        // These absences grouped together form a Whole Absence
                        Absence? absenceRecord = await ProcessWholeAbsence(
                            absencesToProcess, 
                            attendanceAbsences, 
                            enrolledOffering.Id, 
                            periodGroup.ToList(), 
                            totalAbsenceTime, 
                            cancellationToken);

                        if (absenceRecord is not null)
                            returnAbsences.Add(absenceRecord);

                        continue;
                    }

                    // These absences cannot be grouped together, therefore are partial
                    List<Absence> detectedAbsences = new();

                    // If the absence is not on a valid scan date, skip
                    if (!activePartialScanDates.Contains(absencesToProcess.First().Date))
                        continue;

                    // If there are no AttendanceAbsences to match to, Partials cannot be created, Skip
                    if (attendanceAbsences.Count == 0)
                        continue;

                    foreach (SentralPeriodAbsenceDto absence in absencesToProcess)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return returnAbsences;

                        Absence? absenceRecord = await ProcessPartialAbsence(absence, attendanceAbsences, enrolledOffering.Id, periodGroup.ToList(), cancellationToken);

                        if (absenceRecord is not null)
                            detectedAbsences.Add(absenceRecord);
                    }

                    // Detect consecutive absences and combine
                    foreach (Absence resource in detectedAbsences.OrderBy(absence => absence.StartTime).ToList())
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return returnAbsences;

                        // If no other detected absence starts when this one finishes, it is not contiguous with anything else
                        if (detectedAbsences.All(absence => absence.StartTime != resource.EndTime))
                            continue;

                        Absence firstAbsence = resource;
                        Absence secondAbsence = detectedAbsences.First(absence => absence.StartTime == firstAbsence.EndTime);

                        // Ignore if only one of the absences has been detected as explained
                        if ((resource.Explained && !secondAbsence.Explained) || (!resource.Explained && secondAbsence.Explained))
                            continue;

                        // Remove the second absence and update the first to cover the extended time frame.
                        detectedAbsences.Remove(secondAbsence);

                        firstAbsence.MergeAbsence(secondAbsence);
                    }

                    foreach (Absence newAbsence in detectedAbsences.Where(absence => absence.AbsenceLength > _configuration.Absences.PartialLengthThreshold).ToList())
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return returnAbsences;

                        // Check database for all matching absences already known
                        int absenceCount = await _absenceRepository.GetCountForStudentDateAndOffering(student.StudentId, newAbsence.Date, newAbsence.OfferingId, newAbsence.AbsenceTimeframe, cancellationToken);

                        if (absenceCount > 0)
                        {
                            // There is an existing absence here
                            List<Absence> existingAbsences = await _absenceRepository.GetAllForStudentDateAndOffering(student.StudentId, newAbsence.Date, newAbsence.OfferingId, newAbsence.AbsenceTimeframe, cancellationToken);

                            List<AbsenceReason> acceptedReasons = _configuration.Absences.DiscountedPartialReasons;

                            foreach (Absence existingAbsence in existingAbsences)
                            {
                                // Update the last seen property with today's date
                                existingAbsence.UpdateLastSeen();

                                // If the new absence is explained with an accepted reason, and the existing absences are not, then update them to signify they were changed on Sentral
                                if (existingAbsences.All(innerabsence => !innerabsence.Explained) && newAbsence.Responses.Any())
                                {
                                    _logger.Information("{id}: Student {student} ({grade}): Found external explaination for {Type} absence on {Date} - {PeriodName}", JobId, student.DisplayName, student.CurrentGrade.AsName(), newAbsence.Type, newAbsence.Date.ToShortDateString(), newAbsence.PeriodName);

                                    // Is there an existing SYSTEM response? If not, create one
                                    if (existingAbsence.Responses.All(response => response.Type != ResponseType.System))
                                    {
                                        var newResponse = newAbsence.Responses.First();

                                        existingAbsence.AddResponse(
                                            ResponseType.System,
                                            newResponse.From,
                                            newResponse.Explanation);
                                    }
                                }
                            }

                            existingAbsences = null;
                        }
                        else
                        {
                            // This is a new absence
                            // Add the absence to the database
                            if (newAbsence.Explained)
                                _logger.Information("{id}: Student {student} ({grade}): Found new externally explained {Type} absence on {Date} - {PeriodName}", JobId, student.DisplayName, student.CurrentGrade.AsName(), newAbsence.Type, newAbsence.Date.ToShortDateString(), newAbsence.PeriodName);
                            else
                                _logger.Information("{id}: Student {student} ({grade}): Found new unexplained {Type} absence on {Date} - {PeriodName} - {AbsenceReason}", JobId, student.DisplayName, student.CurrentGrade.AsName(), newAbsence.Type, newAbsence.Date.ToShortDateString(), newAbsence.PeriodName, newAbsence.AbsenceReason);

                            returnAbsences.Add(newAbsence);
                        }
                    }
                }

                periods = null;
            }

            enrolledOfferings = null;
        }

        pxpAbsences = null;
        attendanceAbsences = null;
        _student = null;

        return returnAbsences;
    }

    private static void CalculatePxPAbsenceTimes(
        SentralPeriodAbsenceDto absence, 
        List<TimetablePeriod> periodGroup)
    {
        TimetablePeriod? period = (absence.Period.Contains('S'))
            ? periodGroup.FirstOrDefault(pg => pg.Name.Contains(absence.Period.Remove(0, 1)))
            : periodGroup.FirstOrDefault(pg => pg.Name.Contains(absence.Period));

        if (period is null)
            return;

        TimeSpan absenceLength = new TimeSpan(0, absence.MinutesAbsent, 0);

        switch (absence.PartialType)
        {
            case "Late Arrival":
            case null:
                absence.StartTime = TimeOnly.FromTimeSpan(period.StartTime);
                absence.EndTime = absence.StartTime.Add(absenceLength);

                break;
            case "Early Leaver":
                absence.EndTime = TimeOnly.FromTimeSpan(period.EndTime);
                absence.StartTime = absence.EndTime.Add(-absenceLength);

                break;
            default:
                break;
        }
    }

    private static void CalculateWebAttendAbsencePeriod(
        SentralPeriodAbsenceDto absence, 
        List<TimetablePeriod> periodGroup)
    {
        foreach (var period in periodGroup)
        {
            TimeOnly pStart = TimeOnly.FromTimeSpan(period.StartTime);
            TimeOnly pEnd = TimeOnly.FromTimeSpan(period.EndTime);

            if (pStart <= absence.StartTime && pEnd >= absence.EndTime)
            {
                absence.Period = period.Name;

                if (absence.MinutesAbsent != period.EndTime.Subtract(period.StartTime).TotalMinutes)
                {
                    if (absence.StartTime == pStart)
                    {
                        absence.PartialType = "Late Arrival";
                    }
                    else if (absence.EndTime == pEnd)
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

    private async Task<SentralPeriodAbsenceDto> SelectBestWebAttendEntryForPartialAbsence(
        SentralPeriodAbsenceDto absence, 
        List<SentralPeriodAbsenceDto> webAttendAbsences,
        CancellationToken cancellationToken)
    {
        // Excluded absences (either dates that are not valid, or length below thresholds):
        if (absence.MinutesAbsent == 5)
        {
            _logger.Information("{id}: Student {student} ({grade}): Found absence below Sentral threshold on {Date} - {Period} - {Reason}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absence.Date.ToShortDateString(), absence.Period, absence.Reason);
            return null;
        }

        if (absence.Date > DateOnly.FromDateTime(DateTime.Today))
            // This absence is in the future. Likely the PxP roll has been opened and an unavilability automatically created,
            // but the roll has not been submitted, meaning no correlating entry in WebAttend.
            return null;

        if (_excludedDates.Contains(absence.Date))
            return null;

        // Check for a whole day WebAttendance Absence entry first
        List<SentralPeriodAbsenceDto> wholeDayAttendanceAbsence = webAttendAbsences
            .Where(aa => aa.Date == absence.Date && aa.WholeDay)
            .ToList();

        if (wholeDayAttendanceAbsence.Count == 1)
            return wholeDayAttendanceAbsence.First();

        if (absence.StartTime != TimeOnly.MinValue)
        {
            int exactAttendanceAbsenceCount = webAttendAbsences
                .Count(aa =>
                    aa.Date == absence.Date &&
                    aa.StartTime == absence.StartTime &&
                    aa.MinutesAbsent == absence.MinutesAbsent);

            if (exactAttendanceAbsenceCount == 1)
                return webAttendAbsences.First(aa => 
                    aa.Date == absence.Date &&
                    aa.StartTime == absence.StartTime &&
                    aa.MinutesAbsent == absence.MinutesAbsent);

            int approxAttendanceAbsenceCount = webAttendAbsences
                .Count(aa =>
                    aa.Date == absence.Date &&
                    aa.StartTime <= absence.StartTime &&
                    aa.EndTime >= absence.EndTime);

            if (approxAttendanceAbsenceCount == 1)
                return webAttendAbsences.First(aa =>
                    aa.Date == absence.Date &&
                    aa.StartTime <= absence.StartTime &&
                    aa.EndTime >= absence.EndTime);
        }

        // This is a timed partial?
        if (absence.PartialType == "Timed")
        {
            int bestGuessWebAttendAbsencesCount = webAttendAbsences
                .Count(aa =>
                    aa.Date == absence.Date &&
                    aa.Reason == absence.Reason &&
                    aa.MinutesAbsent == absence.MinutesAbsent);

            if (bestGuessWebAttendAbsencesCount == 1)
                return webAttendAbsences.First(aa =>
                    aa.Date == absence.Date &&
                    aa.Reason == absence.Reason &&
                    aa.MinutesAbsent == absence.MinutesAbsent);

            if (bestGuessWebAttendAbsencesCount == 0)
            {
                int nextBestGuessWebAttendAbsencesCount = webAttendAbsences
                    .Count(aa =>
                        aa.Date == absence.Date &&
                        aa.MinutesAbsent == absence.MinutesAbsent);

                if (nextBestGuessWebAttendAbsencesCount == 1)
                    return webAttendAbsences.First(aa =>
                        aa.Date == absence.Date &&
                        aa.MinutesAbsent == absence.MinutesAbsent);
            }

            if (bestGuessWebAttendAbsencesCount > 1)
            {
                // How do we tell these apart?
                // Can we match the period for the WebAttend absences to the PxP absence entry?

                string timetable = (_student.CurrentGrade == Grade.Y05 || _student.CurrentGrade == Grade.Y06)
                    ? "PRIMARY"
                    : "SECONDARY";

                List<TimetablePeriod> periods = await _periodRepository.GetAllFromTimetable(new List<string> { timetable }, cancellationToken);

                var bestGuessWebAttendAbsences = webAttendAbsences
                    .Where(aa =>
                        aa.Date == absence.Date &&
                        aa.Reason == absence.Reason &&
                        aa.MinutesAbsent == absence.MinutesAbsent);

                foreach (var webAttendAbsence in bestGuessWebAttendAbsences)
                    CalculateWebAttendAbsencePeriod(webAttendAbsence, periods);

                int lastAttemptWebAttendAbsencesCount = bestGuessWebAttendAbsences
                    .Count(aa =>
                        aa.Period.Contains(absence.Period) &&
                        aa.PartialType == absence.PartialType);

                if (lastAttemptWebAttendAbsencesCount == 1)
                    return bestGuessWebAttendAbsences.FirstOrDefault(aa =>
                        aa.Period.Contains(absence.Period) &&
                        aa.PartialType == absence.PartialType);
            }
        }

        // UNABLE TO MATCH ABSENCE FROM PXP TO ATTENDANCE (NO RECORD FOUND)
        // This could be because: 
        //      The PxP roll has not actually been submitted, making the absences detected there not accurate yet.
        //      The PxP absence has been incorrectly created (e.g. creating a Timed absence that should be a Late Arrival or Early Leaver)
        //      The Attendance absence has been deleted for some reason
        return null;
    }

    private SentralPeriodAbsenceDto SelectBestWebAttendEntryForWholeAbsence(
        List<SentralPeriodAbsenceDto> absencesToProcess, 
        List<SentralPeriodAbsenceDto> webAttendAbsences, 
        Absence absence)
    {
        if (absence.Date > DateOnly.FromDateTime(DateTime.Today))
        {
            // This absence is in the future. Likely the PxP roll has been opened and an unavilability automatically created,
            // but the roll has not been submitted, meaning no correlating entry in WebAttend.
            return null;
        }

        if (_excludedDates.Contains(absence.Date))
            return null;

        var fromDayAttendanceAbsences = webAttendAbsences
            .Where(aa => aa.Date == absence.Date)
            .ToList();

        // Check for a whole day WebAttendance Absence entry first
        var wholeDayAttendanceAbsence = fromDayAttendanceAbsences
            .Where(aa => aa.WholeDay)
            .ToList();

        if (wholeDayAttendanceAbsence.Count == 1)
            return wholeDayAttendanceAbsence.First();

        // Check for exact match on start and end times
        var forBlockAttendanceAbsences = fromDayAttendanceAbsences
            .Where(aa => 
                aa.StartTime == absence.StartTime && 
                aa.EndTime == absence.EndTime)
            .ToList();

        if (forBlockAttendanceAbsences.Count == 1)
            return forBlockAttendanceAbsences.First();

        // Check for a larger webAttendAbsence that covers this absence
        var largerBlockAttendanceAbsences = fromDayAttendanceAbsences
            .Where(aa => 
                aa.StartTime <= absence.StartTime && 
                aa.EndTime >= absence.EndTime)
            .ToList();

        if (largerBlockAttendanceAbsences.Count == 1)
            return largerBlockAttendanceAbsences.First();

        // Check for multiple webAttendAbsences that combine to cover this absence
        var combinedBlockAttendanceAbsences = fromDayAttendanceAbsences
            .Where(aa => 
                (aa.StartTime >= absence.StartTime && aa.StartTime < absence.EndTime) ||
                (aa.StartTime <= absence.StartTime && aa.EndTime > absence.StartTime))
            .ToList();

        if (combinedBlockAttendanceAbsences.Count > 0)
        {
            var adjustedAbsenceLength = absencesToProcess.Where(ab => ab.MinutesAbsent > 5).Sum(ab => ab.MinutesAbsent);

            var webAttendStartTime = combinedBlockAttendanceAbsences.Min(aa => aa.StartTime);
            var webAttendEndTime = combinedBlockAttendanceAbsences.Max(aa => aa.EndTime);

            if ((webAttendStartTime == absence.StartTime && 
                webAttendEndTime == absence.EndTime && 
                combinedBlockAttendanceAbsences.Sum(aa => aa.MinutesAbsent) == absencesToProcess.Sum(ab => ab.MinutesAbsent)) ||
                combinedBlockAttendanceAbsences.Sum(aa => aa.MinutesAbsent) == adjustedAbsenceLength)
            {
                return new SentralPeriodAbsenceDto
                {
                    ExternalExplanation = combinedBlockAttendanceAbsences.FirstOrDefault(aa => !string.IsNullOrWhiteSpace(aa.ExternalExplanation))?.ExternalExplanation,
                    ExternalExplanationSource = combinedBlockAttendanceAbsences.FirstOrDefault(aa => !string.IsNullOrWhiteSpace(aa.ExternalExplanation))?.ExternalExplanationSource,
                };
            }
        }

        return null;
    }

    private async Task<Absence> ProcessPartialAbsence(
        SentralPeriodAbsenceDto absence, 
        List<SentralPeriodAbsenceDto> webAttendAbsences, 
        OfferingId courseEnrolmentId, 
        List<TimetablePeriod> periodGroup,
        CancellationToken cancellationToken)
    {
        // Can we figure out when the (PxP) absence starts and ends?
        CalculatePxPAbsenceTimes(absence, periodGroup);

        // If we did figure this out, is there an (Attendance) absence that either exactly matches, or covers this timeframe?
        SentralPeriodAbsenceDto attendanceAbsence = await SelectBestWebAttendEntryForPartialAbsence(absence, webAttendAbsences, cancellationToken);
        
        if (attendanceAbsence is null)
            return null;

        // Create an object to save this data to the database.
        var absenceRecord = CreateAbsence(
            new List<SentralPeriodAbsenceDto> { absence },
            attendanceAbsence,
            courseEnrolmentId, 
            AbsenceType.Partial,
            AbsenceReason.FromValue(absence.Reason),
            periodGroup);

        // If this absence was explained using an Accepted Partial Absence Reason, then the absence should be created as explained
        if (_configuration.Absences.DiscountedPartialReasons.Contains(absenceRecord.AbsenceReason))
        {
            // This absence has been externally explained
            absenceRecord.AddResponse(
                ResponseType.System,
                attendanceAbsence.ExternalExplanationSource,
                attendanceAbsence.ExternalExplanation);
        }

        // Does this absence already exist in the database?
        int absenceCount = await _absenceRepository.GetCountForStudentDateAndOffering(
            _student.StudentId,
            absenceRecord.Date,
            absenceRecord.OfferingId,
            absenceRecord.AbsenceTimeframe,
            cancellationToken);

        if (absenceCount > 0)
        {
            // There is an existing absence here
            List<Absence> existingAbsences = await _absenceRepository.GetAllForStudentDateAndOffering(
                _student.StudentId,
                absenceRecord.Date,
                absenceRecord.OfferingId,
                absenceRecord.AbsenceTimeframe,
                cancellationToken);

            List<AbsenceReason> acceptedReasons = _configuration.Absences.DiscountedPartialReasons;

            foreach (Absence existingAbsence in existingAbsences)
            {
                // Update the last seen property with today's date
                existingAbsence.UpdateLastSeen();

                // If the new absence is explained with an accepted reason, and the existing absences are not, then update them to signify they were changed on Sentral
                if (!existingAbsence.Explained && _configuration.Absences.DiscountedPartialReasons.Contains(absenceRecord.AbsenceReason))
                {
                    _logger.Information("{id}: Student {student} ({grade}): Found external explaination for {Type} absence on {Date} - {PeriodName}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName);

                    // Is there an existing SYSTEM response? If not, create one
                    if (existingAbsence.Responses.All(response => response.Type != ResponseType.System))
                    {
                        existingAbsence.AddResponse(
                            ResponseType.System,
                            attendanceAbsence.ExternalExplanationSource,
                            attendanceAbsence.ExternalExplanation);
                    }
                }
            }

            existingAbsences = null;
            absenceRecord = null;

            return null;
        }

        return absenceRecord;
    }

    private async Task<Absence> ProcessWholeAbsence(
        List<SentralPeriodAbsenceDto> absencesToProcess, 
        List<SentralPeriodAbsenceDto> webAttendAbsences, 
        OfferingId courseEnrolmentId, 
        List<TimetablePeriod> periodGroup, 
        int totalAbsenceTime, 
        CancellationToken cancellationToken)
    {
        // Calculate acceptable reason
        List<AbsenceReason> reasons = absencesToProcess
            .Select(absence => AbsenceReason.FromValue(absence.Reason))
            .Distinct()
            .ToList();
        AbsenceReason reason = (reasons.Count == 1) ? reasons.First() : Absence.FindWorstAbsenceReason(reasons);

        // Create an object to save this data to the database.
        Absence absenceRecord = CreateAbsence(
            absencesToProcess, 
            null,
            courseEnrolmentId, 
            AbsenceType.Whole, 
            reason, 
            periodGroup);

        // Find a webAttend absence that covers this set
        SentralPeriodAbsenceDto attendanceAbsence = SelectBestWebAttendEntryForWholeAbsence(
            absencesToProcess, 
            webAttendAbsences, 
            absenceRecord);

        // Check database for all matching absences already known
        int absenceCount = await _absenceRepository.GetCountForStudentDateAndOffering(
            _student.StudentId, 
            absenceRecord.Date, 
            absenceRecord.OfferingId, 
            absenceRecord.AbsenceTimeframe, 
            cancellationToken);

        if (attendanceAbsence == null)
        {
            // The webAttend entry may have been deleted for some reason. This absence needs to be checked against the database to make sure
            // it doesn't exist in there as an UNEXPLAINED absence, which will trigger a reminder email later.

            // We don't actually care IF it is a new absence, only that existing absences will be updated with the explanation if appropriate.
            // Check database for all matching absences already known
            if (absenceCount > 0)
            {
                // There is an existing absence here
                List<Absence> existingAbsences = await _absenceRepository.GetAllForStudentDateAndOffering(
                    _student.StudentId, 
                    absenceRecord.Date, 
                    absenceRecord.OfferingId, 
                    absenceRecord.AbsenceTimeframe, 
                    cancellationToken);

                List<AbsenceReason> acceptedReasons = _configuration.Absences.DiscountedWholeReasons;

                _logger.Information("{id}: Found absences that have been removed from Sentral WebAttend for student {student} on date {date}", JobId, _student.DisplayName, absenceRecord.Date);

                foreach (Absence existingAbsence in existingAbsences)
                {
                    // Update the last seen property with today's date
                    existingAbsence.UpdateLastSeen();

                    // If the new absence is explained with an accepted reason, and the existing absences are not, then update them to signify they were changed on Sentral
                    if (existingAbsences.All(innerabsence => !innerabsence.Explained) && _configuration.Absences.DiscountedWholeReasons.Contains(reason))
                    {
                        _logger.Information("{id}: Student {student} ({grade}): Found external explaination for {Type} absence on {Date} - {PeriodName}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName);

                        // Is there an existing SYSTEM response? If not, create one
                        if (existingAbsence.Responses.All(response => response.Type != ResponseType.System))
                        {
                            SentralPeriodAbsenceDto absenceDto = absencesToProcess
                                .First(absence => AbsenceReason.FromValue(absence.Reason) == reason);

                            existingAbsence.AddResponse(
                                ResponseType.System,
                                absenceDto.ExternalExplanationSource,
                                absenceDto.ExternalExplanation);
                        }
                    }
                }

                existingAbsences = null;
            }

            absenceRecord = null;

            return null;
        }

        // UPDATED 2024-03-04
        // If "Shared Enrolment" is not included in the DiscountedWholeReasons collection, but a whole absence is made up
        // of multiple partial absences, where one or more is "Shared Enrolment" and all the rest are included in the
        // DiscountedWholeReasons collection, this should still be treated as an explained absence.

        // If all the parts of this whole absence have explained statuses, then record the explanation
        if (absencesToProcess.All(absence =>
                _configuration.Absences.DiscountedWholeReasons.Contains(AbsenceReason.FromValue(absence.Reason)) ||
                absence.Reason.Equals(AbsenceReason.SharedEnrolment)))
        {
            // This absence has been externally explained
            absenceRecord.AddResponse(
                ResponseType.System,
                attendanceAbsence.ExternalExplanationSource,
                attendanceAbsence.ExternalExplanation);
        }

        if (absenceCount > 0)
        {
            // There is an existing absence here
            List<Absence> existingAbsences = await _absenceRepository.GetAllForStudentDateAndOffering(
                _student.StudentId, 
                absenceRecord.Date, 
                absenceRecord.OfferingId, 
                absenceRecord.AbsenceTimeframe, 
                cancellationToken);

            List<AbsenceReason> acceptedReasons = _configuration.Absences.DiscountedPartialReasons;

            foreach (Absence existingAbsence in existingAbsences)
            {
                // Update the last seen property with today's date
                existingAbsence.UpdateLastSeen();

                // If the new absence is explained with an accepted reason, and the existing absences are not, then update them to signify they were changed on Sentral
                if (existingAbsences.All(innerabsence => !innerabsence.Explained) && 
                    absenceRecord.Responses.Any())
                {
                    _logger.Information("{id}: Student {student} ({grade}): Found external explaination for {Type} absence on {Date} - {PeriodName}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName);

                    // Is there an existing SYSTEM response? If not, create one
                    if (existingAbsence.Responses.All(response => response.Type != ResponseType.System))
                    {
                        Response newResponse = absenceRecord.Responses.First();

                        existingAbsence.AddResponse(
                            ResponseType.System,
                            newResponse.From,
                            newResponse.Explanation);
                    }
                }
            }

            existingAbsences = null;
        }
        else
        {
            // This is a new absence
            // Add the absence to the database
            if (absenceRecord.Explained)
                _logger.Information("{id}: Student {student} ({grade}): Found new externally explained {Type} absence on {Date} - {PeriodName}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName);
            else
                _logger.Information("{id}: Student {student} ({grade}): Found new unexplained {Type} absence on {Date} - {PeriodName} - {AbsenceReason}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName, absenceRecord.AbsenceReason);

            return absenceRecord;
        }

        return null;
    }

    private Absence CreateAbsence(
        List<SentralPeriodAbsenceDto> absencesToProcess, 
        SentralPeriodAbsenceDto? attendanceAbsence,
        OfferingId courseEnrolmentId, 
        AbsenceType type, 
        AbsenceReason reason, 
        List<TimetablePeriod> periodGroup)
    {
        string periodName, periodTimeframe = string.Empty;
        TimeOnly startTime, endTime = TimeOnly.MinValue;

        if (periodGroup.Count > 1)
        {
            periodName = $"{periodGroup.First().Name} - {periodGroup.Last().Name}";
            periodTimeframe = $"{periodGroup.First().StartTime.As12HourTime()} - {periodGroup.Last().EndTime.As12HourTime()}";
            startTime = TimeOnly.FromTimeSpan(periodGroup.First().StartTime);
            endTime = TimeOnly.FromTimeSpan(periodGroup.Last().EndTime);
        }
        else
        {
            periodName = periodGroup.First().Name;
            periodTimeframe = $"{periodGroup.First().StartTime.As12HourTime()} - {periodGroup.Last().EndTime.As12HourTime()}";
            startTime = TimeOnly.FromTimeSpan(periodGroup.First().StartTime);
            endTime = TimeOnly.FromTimeSpan(periodGroup.Last().EndTime);
        }

        if (type == AbsenceType.Partial && attendanceAbsence is not null)
        {
            // Update the start time and end time appropriately

            if (attendanceAbsence.Timeframe == "Whole Day")
            {
                // This Attendance entry explains the partial. Should create an explained entry.
                startTime = absencesToProcess.First().StartTime;
                endTime = absencesToProcess.First().EndTime;
            }
            else
            {
                // This Attendance entry correlates to the partial.
                if (startTime != attendanceAbsence.StartTime || endTime != attendanceAbsence.EndTime)
                {
                    // TIMES DO NOT MATCH!
                    startTime = (attendanceAbsence.StartTime > startTime) ? attendanceAbsence.StartTime : startTime;
                    endTime = (attendanceAbsence.EndTime < endTime) ? attendanceAbsence.EndTime : endTime;
                }
            }
        }

        var absence = Absence.Create(
            type,
            _student.StudentId,
            courseEnrolmentId,
            absencesToProcess.First().Date,
            periodName,
            periodTimeframe,
            reason,
            startTime,
            endTime);

        return absence;
    }
}
