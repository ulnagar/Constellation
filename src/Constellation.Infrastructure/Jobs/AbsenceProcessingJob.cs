namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;
using Microsoft.Extensions.Options;
using Serilog.Context;
using System.Drawing;
using System.Threading;

public class AbsenceProcessingJob : IAbsenceProcessingJob
{
    private readonly ISentralGateway _sentralService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IUnitOfWork _unitOfWork;

    private Student _student;
    private List<DateTime> _excludedDates = new(); 
    private AppConfiguration _configuration;
    private Guid JobId { get; set; }

    public AbsenceProcessingJob(
        IOptions<AppConfiguration> configuration,
        ICourseOfferingRepository offeringRepository,
        ITimetablePeriodRepository periodRepository,
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IUnitOfWork unitOfWork, 
        ISentralGateway sentralService,
        IEmailService emailService, 
        ILogger logger)
    {
        _configuration = configuration.Value;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _unitOfWork = unitOfWork;
        _sentralService = sentralService;
        _emailService = emailService;

        _logger = logger.ForContext<IAbsenceMonitorJob>();
        LogContext.PushProperty(nameof(JobId), JobId);
    }

    public async Task<List<Absence>> StartJob(Guid jobId, Student student, CancellationToken cancellationToken)
    {
        JobId = jobId;
        _student = student;

        _logger.Information("{id}: Scanning student {student} ({grade})", JobId, student.DisplayName, student.CurrentGrade.AsName());

        List<Absence> returnAbsences = new();

        if (student.AbsenceNotificationStartDate is null)
        {
            _logger.Information("{id}: - Student absence scanning disabled. Skipping.", JobId);
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

            double absenceLength = attendAbsence.EndTime
                .Subtract(attendAbsence.StartTime)
                .TotalMinutes;

            attendAbsence.MinutesAbsent = Convert.ToInt32(absenceLength);
        }

        // If there are no pxpabsences, return empty list
        if (pxpAbsences == null || pxpAbsences.Count == 0)
            return returnAbsences;

        var groupedPxpAbsences = pxpAbsences.GroupBy(a => a.Date.Date);

        foreach (var group in groupedPxpAbsences)
        {
            if (cancellationToken.IsCancellationRequested)
                return returnAbsences;

            // If the date of this group of absences is before the cut-off date(s), skip
            if (group.Key.Date < GetEarliestDate())
                continue;

            // Get the timetable for the day
            // Given the date, figure out what day of the cycle we are looking at.
            int cycleDay = group.Key.Date.GetDayNumber();

            // Get all enrolments for this student that were active on that date using the day of the cycle we identified above
            List<CourseOffering> enrolledOfferings = await _offeringRepository
                .GetCurrentEnrolmentsFromStudentForDate(
                    student.StudentId, 
                    group.Key.Date, 
                    cycleDay, 
                    cancellationToken);
            
            foreach (CourseOffering enrolledOffering in enrolledOfferings)
            {
                if (cancellationToken.IsCancellationRequested)
                    return returnAbsences;

                if (!enrolledOffering.Name.Contains(group.First().ClassName))
                {
                    // The PxP absence is for a different class than the courseEnrolment
                    // therefore it should not be processed here.
                    
                    _logger.Warning($"-- Enrolment Class {enrolledOffering.Name} does not match PxP Absence Class {group.First().ClassName}");
                    continue;
                }

                // Get list of periods for this class on this day
                List<TimetablePeriod> periods = await _periodRepository.GetForOfferingOnDay(enrolledOffering.Id, group.Key.Date, cycleDay, cancellationToken);
                    
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
                    if (absencesToProcess.First().Date.Date == DateTime.Today.Date)
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

                    // If the absence is too early, skip
                    if (absencesToProcess.First().Date < GetEarliestDate())
                        continue;

                    // If there are no AttendanceAbsences to match to, Partials cannot be created, Skip
                    if (attendanceAbsences.Count == 0)
                        continue;

                    foreach (SentralPeriodAbsenceDto absence in absencesToProcess)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return returnAbsences;

                        Absence? absenceRecord = ProcessPartialAbsence(absence, attendanceAbsences, enrolledOffering.Id, periodGroup.ToList());

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
                                    var responses = await _responseRepository.GetAllForAbsence(existingAbsence.Id, cancellationToken);
                                    if (responses.All(response => response.Type != ResponseType.System))
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

    private DateTime GetEarliestDate()
    {
        if (_student.AbsenceNotificationStartDate.HasValue && _student.AbsenceNotificationStartDate.Value > _configuration.Absences.AbsenceScanStartDate)
        {
            return _student.AbsenceNotificationStartDate.Value;
        }
        else
        {
            return _configuration.Absences.AbsenceScanStartDate;
        }
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

    private static void CalculateWebAttendAbsencePeriod(SentralPeriodAbsenceDto absence, IList<TimetablePeriod> periodGroup)
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
            _logger.LogInformation("{id}: Student {student} ({grade}): Found absence below Sentral threshold on {Date} - {Period} - {Reason}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absence.Date.ToShortDateString(), absence.Period, absence.Reason);
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

    private async Task<Absence> ProcessPartialAbsence(
        SentralPeriodAbsenceDto absence, 
        List<SentralPeriodAbsenceDto> webAttendAbsences, 
        int courseEnrolmentId, 
        List<TimetablePeriod> periodGroup,
        CancellationToken cancellationToken)
    {
        // Can we figure out when the (PxP) absence starts and ends?
        CalculatePxPAbsenceTimes(absence, periodGroup);

        // If we did figure this out, is there an (Attendance) absence that either exactly matches, or covers this timeframe?
        SentralPeriodAbsenceDto attendanceAbsence = SelectBestWebAttendEntryForPartialAbsence(absence, webAttendAbsences);
        
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
                absence.ExternalExplanationSource,
                absence.ExternalExplanation);
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
                if (existingAbsences.All(innerabsence => !innerabsence.Explained) && _configuration.Absences.DiscountedPartialReasons.Contains(absenceRecord.AbsenceReason))
                {
                    _logger.Information("{id}: Student {student} ({grade}): Found external explaination for {Type} absence on {Date} - {PeriodName}", JobId, _student.DisplayName, _student.CurrentGrade.AsName(), absenceRecord.Type, absenceRecord.Date.ToShortDateString(), absenceRecord.PeriodName);

                    // Is there an existing SYSTEM response? If not, create one
                    List<Response> responses = await _responseRepository.GetAllForAbsence(
                        existingAbsence.Id,
                        cancellationToken);

                    if (responses.All(response => response.Type != ResponseType.System))
                    {
                        existingAbsence.AddResponse(
                            ResponseType.System,
                            absence.ExternalExplanationSource,
                            absence.ExternalExplanation);
                    }

                    responses = null;
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
        int courseEnrolmentId, 
        List<TimetablePeriod> periodGroup, 
        int totalAbsenceTime, 
        CancellationToken cancellationToken)
    {
        if (absencesToProcess.First().Date < GetEarliestDate())
            return null;

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
                        List<Response> responses = await _responseRepository.GetAllForAbsence(
                            existingAbsence.Id, 
                            cancellationToken);

                        if (responses.All(response => response.Type != ResponseType.System))
                        {
                            SentralPeriodAbsenceDto absenceDto = absencesToProcess
                                .First(absence => AbsenceReason.FromValue(absence.Reason) == reason);

                            existingAbsence.AddResponse(
                                ResponseType.System,
                                absenceDto.ExternalExplanationSource,
                                absenceDto.ExternalExplanation);
                        }

                        responses = null;
                    }
                }

                existingAbsences = null;
            }

            absenceRecord = null;

            return null;
        }

        // If all the parts of this whole absence have explained statuses, then record the explanation
        if (absencesToProcess.All(absence =>
                _configuration.Absences.DiscountedWholeReasons.Contains(AbsenceReason.FromValue(absence.Reason))))
        {
            // This absence has been externally explained
            SentralPeriodAbsenceDto absenceDto = absencesToProcess.First(absence => 
                AbsenceReason.FromValue(absence.Reason) == reason);

            absenceRecord.AddResponse(
                ResponseType.System,
                absenceDto.ExternalExplanationSource,
                absenceDto.ExternalExplanation);
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
                    List<Response> responses = await _responseRepository.GetAllForAbsence(
                        existingAbsence.Id, 
                        cancellationToken);

                    if (responses.All(response => response.Type != ResponseType.System))
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
        int courseEnrolmentId, 
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
                startTime = TimeOnly.FromTimeSpan(absencesToProcess.First().StartTime);
                endTime = TimeOnly.FromTimeSpan(absencesToProcess.First().EndTime);
            }
            else
            {
                // This Attendance entry correlates to the partial.
                if (startTime != TimeOnly.FromTimeSpan(attendanceAbsence.StartTime) || endTime != TimeOnly.FromTimeSpan(attendanceAbsence.EndTime))
                {
                    // TIMES DO NOT MATCH!
                    startTime = (TimeOnly.FromTimeSpan(attendanceAbsence.StartTime) > startTime) ? TimeOnly.FromTimeSpan(attendanceAbsence.StartTime) : startTime;
                    endTime = (TimeOnly.FromTimeSpan(attendanceAbsence.EndTime) < endTime) ? TimeOnly.FromTimeSpan(attendanceAbsence.EndTime) : endTime;
                }
            }
        }

        var absence = Absence.Create(
            type,
            _student.StudentId,
            courseEnrolmentId,
            DateOnly.FromDateTime(absencesToProcess.First().Date),
            periodName,
            periodTimeframe,
            reason,
            startTime,
            endTime);

        return absence;
    }
}
