namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Infrastructure.DependencyInjection;

public class SessionService : ISessionService, IScopedService
{
    private readonly IOperationService _operationService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IOfferingSessionsRepository _sessionsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SessionService(
        IOfferingRepository offeringRepository,
        IOfferingSessionsRepository sessionsRepository,
        IUnitOfWork unitOfWork, 
        IOperationService operationService,
        IDateTimeProvider dateTime)
    {
        _offeringRepository = offeringRepository;
        _sessionsRepository = sessionsRepository;
        _unitOfWork = unitOfWork;
        _operationService = operationService;
        _dateTime = dateTime;
    }

    public async Task<ServiceOperationResult<Session>> CreateSession(SessionDto sessionResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<Session>();

        // Validate entries
        var checkSession = await _sessionsRepository.GetById(sessionResource.Id);
        if (checkSession != null)
        {
            result.Success = false;
            result.Errors.Add($"A session with that ID already exists!");
            return result;
        }

        var checkOffering = await _offeringRepository.GetById(sessionResource.OfferingId);
        if (checkOffering == null)
        {
            result.Success = false;
            result.Errors.Add($"An offering with that Id could not be found!");
            return result;
        }

        if (checkOffering.EndDate < _dateTime.Today)
        {
            result.Success = false;
            result.Errors.Add($"The selected offering has already expired!");
            return result;
        }

        var checkStaff = await _unitOfWork.Staff.GetForExistCheck(sessionResource.StaffId);
        if (checkStaff == null)
        {
            result.Success = false;
            result.Errors.Add($"A staff member with that Id could not be found!");
            return result;
        }

        if (checkStaff.IsDeleted)
        {
            result.Success = false;
            result.Errors.Add($"The selected staff member is not available!");
            return result;
        }

        var checkPeriod = await _unitOfWork.Periods.ForEditAsync(sessionResource.PeriodId);
        if (checkPeriod == null)
        {
            result.Success = false;
            result.Errors.Add($"A period with that Id could not be found!");
            return result;
        }

        if (checkPeriod.IsDeleted)
        {
            result.Success = false;
            result.Errors.Add($"The selected period is not available!");
            return result;
        }

        var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(sessionResource.RoomId);
        if (checkRoom == null)
        {
            result.Success = false;
            result.Errors.Add($"A room with that Id could not be found!");
            return result;
        }

        if (checkRoom.IsDeleted)
        {
            result.Success = false;
            result.Errors.Add($"The selected room is not available!");
            return result;
        }

        // Create new entity
        var session = new Session
        {
            OfferingId = sessionResource.OfferingId,
            StaffId = sessionResource.StaffId,
            PeriodId = sessionResource.PeriodId,
            RoomId = sessionResource.RoomId,
            DateCreated = DateTime.Today
        };

        _unitOfWork.Add(session);

        // If this is the first session for this Offering with this Teacher, add the Teacher to the TEAM.
        if (!await _sessionsRepository.AnyCurrentForOfferingAndTeacher(sessionResource.OfferingId, sessionResource.StaffId))
        {
            await _operationService.CreateTeacherMSTeamOwnerAccess(sessionResource.StaffId, sessionResource.OfferingId, DateTime.Now, null);

            await _operationService.EnrolStaffInCanvasCourse(checkStaff, checkOffering);
        }

        // If this is the first session for this Offering with this Room, add the Students to the ROOM.\
        if (!await _sessionsRepository.AnyCurrentForOfferingAndRoom(sessionResource.OfferingId, sessionResource.RoomId))
        {
            var students = checkOffering.Enrolments.Where(e => !e.IsDeleted).Select(e => e.Student).Distinct().ToList();

            foreach (var student in students)
            {
                await _operationService.CreateStudentMSTeamMemberAccess(student.StudentId, sessionResource.OfferingId, DateTime.Now);
                await _operationService.CreateStudentAdobeConnectAccess(student.StudentId, sessionResource.RoomId, DateTime.Now);
            }
        }

        result.Success = true;
        result.Entity = session;
        return result;
    }

    public async Task<ServiceOperationResult<Session>> UpdateSession(int id, SessionDto sessionResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<Session>();

        // Validate entries
        var session = await _sessionsRepository.GetById(sessionResource.Id);
        if (session == null)
        {
            result.Success = false;
            result.Errors.Add($"A session with that ID could not be found!");
            return result;
        }

        if (sessionResource.OfferingId is null)
        {
            result.Success = false;
            result.Errors.Add("An offering must be provided");
            return result;
        }
        
        var checkOffering = await _offeringRepository.GetById(sessionResource.OfferingId);
        if (checkOffering == null)
        {
            result.Success = false;
            result.Errors.Add($"An offering with that Id could not be found!");
            return result;
        }

        if (checkOffering.EndDate < _dateTime.Today)
        {
            result.Success = false;
            result.Errors.Add($"The selected offering has already expired!");
            return result;
        }

        session.OfferingId = sessionResource.OfferingId;

        if (!string.IsNullOrWhiteSpace(sessionResource.StaffId))
        {
            var checkStaff = await _unitOfWork.Staff.GetForExistCheck(sessionResource.StaffId);
            if (checkStaff == null)
            {
                result.Success = false;
                result.Errors.Add($"A staff member with that Id could not be found!");
                return result;
            }

            if (checkStaff.IsDeleted)
            {
                result.Success = false;
                result.Errors.Add($"The selected staff member is not available!");
                return result;
            }

            session.StaffId = sessionResource.StaffId;
        }

        if (sessionResource.PeriodId != 0)
        {
            var checkPeriod = await _unitOfWork.Periods.ForEditAsync(sessionResource.PeriodId);
            if (checkPeriod == null)
            {
                result.Success = false;
                result.Errors.Add($"A period with that Id could not be found!");
                return result;
            }

            if (checkPeriod.IsDeleted)
            {
                result.Success = false;
                result.Errors.Add($"The selected period is not available!");
                return result;
            }

            session.PeriodId = sessionResource.PeriodId;
        }

        if (!string.IsNullOrWhiteSpace(sessionResource.RoomId))
        {
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(sessionResource.RoomId);
            if (checkRoom == null)
            {
                result.Success = false;
                result.Errors.Add($"A room with that Id could not be found!");
                return result;
            }

            if (checkRoom.IsDeleted)
            {
                result.Success = false;
                result.Errors.Add($"The selected room is not available!");
                return result;
            }

            session.PeriodId = sessionResource.PeriodId;
        }

        result.Success = true;
        result.Entity = session;
        return result;
    }

    public async Task RemoveSession(int id)
    {
        // Validate entries
        var session = await _sessionsRepository.GetById(id);

        if (session == null)
            return;

        session.IsDeleted = true;
        session.DateDeleted = _dateTime.Now;

        // If there are no other active sessions for this Offering with this Teacher, remove teacher access to the Team.
        if (!await _sessionsRepository.AnyCurrentForOfferingAndTeacher(session.OfferingId, session.StaffId))
        {
            await _operationService.RemoveTeacherMSTeamAccess(session.StaffId, session.OfferingId, DateTime.Now, null);
            await _operationService.UnenrolStaffFromCanvasCourse(session.Teacher, session.Offering);
        }

        // If there are no other active sessions for this Offering in this Room, remove student access to the Room.
        if (!await _sessionsRepository.AnyCurrentForOfferingAndRoom(session.OfferingId, session.RoomId))
        {
            var offering = await _offeringRepository.GetById(session.OfferingId);

            foreach (var student in offering.Enrolments.Where(e => !e.IsDeleted).Select(e => e.Student).Distinct().ToList())
                await _operationService.RemoveStudentAdobeConnectAccess(student.StudentId, session.RoomId, DateTime.Now);
        }

        // If there are no other active sessions for this Offering, remove student access to the Team.
        if (!await _sessionsRepository.AnyCurrentForOffering(session.OfferingId))
        {
            var offering = await _offeringRepository.GetById(session.OfferingId);

            foreach (var student in offering.Enrolments.Where(e => !e.IsDeleted).Select(e => e.Student).Distinct().ToList())
                await _operationService.RemoveStudentMSTeamAccess(student.StudentId, session.OfferingId, DateTime.Now);
        }
    }

    public async Task<ServiceOperationResult<TimetablePeriod>> CreatePeriod(PeriodDto periodResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<TimetablePeriod>();

        // Validate entries
        if (periodResource.Id != null)
        {
            var checkPeriod = await _unitOfWork.Periods.ForEditAsync(periodResource.Id.Value);

            if (checkPeriod != null)
            {
                result.Success = false;
                result.Errors.Add($"A period with that Id already exists!");
                return result;
            }
        }

        var period = new TimetablePeriod
        {
            Day = periodResource.Day,
            Timetable = periodResource.Timetable,
            Period = periodResource.Period,
            StartTime = periodResource.StartTime,
            EndTime = periodResource.EndTime,
            Name = periodResource.Name,
            Type = periodResource.Type
        };

        _unitOfWork.Add(period);

        result.Success = true;
        result.Entity = period;

        return result;
    }

    public async Task<ServiceOperationResult<TimetablePeriod>> UpdatePeriod(int? id, PeriodDto periodResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<TimetablePeriod>();

        // Validate entries
        if (periodResource.Id == null)
        {
            result.Success = false;
            result.Errors.Add($"A period with that Id cannot be found!");
            return result;
        }

        var period = await _unitOfWork.Periods.ForEditAsync(periodResource.Id.Value);

        if (periodResource.Day != 0)
            period.Day = periodResource.Day;

        if (periodResource.Period != 0)
            period.Period = periodResource.Period;

        period.Timetable = periodResource.Timetable;

        period.StartTime = periodResource.StartTime;
        period.EndTime = periodResource.EndTime;

        if (!string.IsNullOrWhiteSpace(periodResource.Name))
            period.Name = periodResource.Name;

        if (!string.IsNullOrWhiteSpace(periodResource.Type))
            period.Type = periodResource.Type;

        result.Success = true;
        result.Entity = period;

        return result;
    }

    public async Task RemovePeriod(int id)
    {
        // Validate entries
        var period = await _unitOfWork.Periods.ForEditAsync(id);

        if (period == null)
            return;

        period.IsDeleted = true;
        period.DateDeleted = _dateTime.Now;
    }
}
