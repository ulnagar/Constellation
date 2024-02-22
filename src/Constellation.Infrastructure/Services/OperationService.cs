namespace Constellation.Infrastructure.Services;

using Application.Enums;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Students;
using System;
using System.Threading.Tasks;

public class OperationService : IOperationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeProvider _dateTime;

    public OperationService(
        IUnitOfWork unitOfWork,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _dateTime = dateTime;
    }

    public async Task CreateStudentAdobeConnectAccess(string studentId, string roomId, DateTime schedule)
    {
        // Verify entries
        var checkStudent = await _unitOfWork.Students.GetForExistCheck(studentId);
        var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStudent == null || checkRoom == null)
            return;

        var operation = new StudentAdobeConnectOperation
        {
            ScoId = roomId,
            StudentId = studentId,
            Action = AdobeConnectOperationAction.Add,
            DateScheduled = schedule
        };

        _unitOfWork.Add(operation);
        await _unitOfWork.CompleteAsync();
    }

    public async Task CreateTeacherAdobeConnectAccess(string staffId, string roomId, DateTime scheduled, Guid coverId)
    {
        // Verify entries
        var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);
        var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStaff == null || checkRoom == null)
            return;

        var operation = new TeacherAdobeConnectOperation
        {
            ScoId = roomId,
            StaffId = staffId,
            Action = AdobeConnectOperationAction.Add,
            DateScheduled = scheduled,
            CoverId = coverId
        };

        _unitOfWork.Add(operation);
    }

    public async Task RemoveStudentAdobeConnectAccess(string studentId, string roomId, DateTime schedule)
    {
        // Verify entries
        var checkStudent = await _unitOfWork.Students.GetForExistCheck(studentId);
        var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStudent == null || checkRoom == null)
            return;

        var operation = new StudentAdobeConnectOperation
        {
            ScoId = roomId,
            StudentId = studentId,
            Action = AdobeConnectOperationAction.Remove,
            DateScheduled = schedule
        };

        _unitOfWork.Add(operation);
    }

    public async Task RemoveTeacherAdobeConnectAccess(string staffId, string roomId, DateTime scheduled, Guid coverId)
    {
        // Verify entries
        var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);
        var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStaff == null || checkRoom == null)
            return;

        var operation = new TeacherAdobeConnectOperation
        {
            ScoId = roomId,
            StaffId = staffId,
            Action = AdobeConnectOperationAction.Remove,
            DateScheduled = scheduled,
            CoverId = coverId
        };

        _unitOfWork.Add(operation);
    }

    public async Task CreateTeacherAdobeConnectGroupMembership(string staffId, AdobeConnectGroup groupName, DateTime scheduled)
    {
        // Verify entries
        var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (checkStaff == null)
            return;

        var operation = new TeacherAdobeConnectGroupOperation
        {
            GroupSco = ((int)groupName).ToString(),
            GroupName = groupName.ToString(),
            TeacherId = staffId,
            Action = AdobeConnectOperationAction.Add,
            DateScheduled = scheduled
        };

        _unitOfWork.Add(operation);
    }

    public async Task RemoveTeacherAdobeConnectGroupMembership(string staffId, AdobeConnectGroup groupName, DateTime scheduled)
    {
        // Verify entries
        var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (checkStaff == null)
            return;

        var operation = new TeacherAdobeConnectGroupOperation
        {
            GroupSco = ((int)groupName).ToString(),
            GroupName = groupName.ToString(),
            TeacherId = staffId,
            Action = AdobeConnectOperationAction.Remove,
            DateScheduled = scheduled
        };

        _unitOfWork.Add(operation);
    }

    public async Task MarkAdobeConnectOperationComplete(int id)
    {
        // Validate entries
        var operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

        if (operation == null)
            return;

        if (operation.IsCompleted || operation.IsDeleted)
            return;

        operation.IsCompleted = true;
    }

    public async Task MarkAdobeConnectOperationCompleteAsync(int id)
    {
        // Validate entries
        var operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

        if (operation == null)
            return;

        if (operation.IsCompleted || operation.IsDeleted)
            return;

        operation.IsCompleted = true;
    }

    public async Task CancelAdobeConnectOperation(int id)
    {
        // Validate entries
        var operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

        if (operation == null)
            return;

        CancelAdobeConnectOperation(operation);
    }

    public void CancelAdobeConnectOperation(AdobeConnectOperation operation)
    {
        if (operation.IsCompleted || operation.IsDeleted)
            return;

        operation.IsDeleted = true;
    }

    public async Task CreateStudentMSTeamMemberAccess(string studentId, OfferingId offeringId, DateTime schedule)
    {
        // Validate entries
        var student = await _unitOfWork.Students.GetForExistCheck(studentId);
        var offering = await _offeringRepository.GetById(offeringId);

        if (student == null || offering == null)
            return;

        var operation = new StudentMSTeamOperation()
        {
            StudentId = studentId,
            OfferingId = offeringId,
            DateScheduled = schedule,
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _unitOfWork.Add(operation);
        await _unitOfWork.CompleteAsync();
    }

    public async Task CreateTeacherMSTeamMemberAccess(string staffId, OfferingId offeringId, DateTime scheduled, Guid? coverId)
    {
        // Validate entries
        var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
        var offering = await _offeringRepository.GetById(offeringId);

        if (staffMember == null || offering == null)
            return;

        // Create Operation
        var operation = new TeacherMSTeamOperation
        {
            OfferingId = offering.Id,
            StaffId = staffMember.StaffId,
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = scheduled
        };

        if (coverId.HasValue)
            operation.CoverId = coverId.Value;

        _unitOfWork.Add(operation);
    }

    public async Task CreateTeacherMSTeamOwnerAccess(string staffId, OfferingId offeringId, DateTime scheduled, Guid? coverId)
    {
        // Validate entries
        var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
        var offering = await _offeringRepository.GetById(offeringId);

        if (staffMember == null || offering == null)
            return;

        // Create Operation
        var operation = new TeacherMSTeamOperation
        {
            OfferingId = offering.Id,
            StaffId = staffMember.StaffId,
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            DateScheduled = scheduled
        };

        if (coverId.HasValue)
            operation.CoverId = coverId.Value;

        _unitOfWork.Add(operation);
    }

    public async Task CreateClassroomMSTeam(OfferingId offeringId, DateTime scheduled)
    {
        // Validate entries
        var offering = await _offeringRepository.GetById(offeringId);

        if (offering is null)
            return;

        var course = await _courseRepository.GetById(offering.CourseId);

        if (course is null)
            return;

        // Create Operation
        var operation = new GroupMSTeamOperation
        {
            FacultyId = course.FacultyId,
            OfferingId = offering.Id,
            Offering = offering,
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Group,
            DateScheduled = scheduled
        };

        _unitOfWork.Add(operation);
    }

    public async Task CreateStudentEnrolmentMSTeamAccess(string studentId)
    {
        // Validate entries
        var student = await _unitOfWork.Students.GetForExistCheck(studentId);

        if (student == null)
            return;

        var operation = new StudentEnrolledMSTeamOperation()
        {
            StudentId = studentId,
            TeamName = MicrosoftTeam.Students,
            DateScheduled = _dateTime.Now,
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _unitOfWork.Add(operation);
    }

    public async Task RemoveStudentEnrollmentMSTeamAccess(string studentId)
    {
        // Validate entries
        var student = await _unitOfWork.Students.GetForExistCheck(studentId);

        if (student == null)
            return;

        var operation = new StudentEnrolledMSTeamOperation()
        {
            StudentId = studentId,
            TeamName = MicrosoftTeam.Students,
            DateScheduled = _dateTime.Now,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _unitOfWork.Add(operation);
    }

    public async Task CreateTeacherEmployedMSTeamAccess(string staffId)
    {
        // Validate entries
        var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (staffMember == null)
            return;

        // Create Operation
        var studentTeamOperation = new TeacherEmployedMSTeamOperation()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Add,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _unitOfWork.Add(studentTeamOperation);

        var schoolTeamOperation = new TeacherEmployedMSTeamOperation()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Staff,
            Action = MSTeamOperationAction.Add,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _unitOfWork.Add(schoolTeamOperation);
    }

    public async Task RemoveTeacherEmployedMSTeamAccess(string staffId)
    {
        // Validate entries
        var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (staffMember == null)
            return;

        // Create Operation
        var studentTeamOperation = new TeacherEmployedMSTeamOperation()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = _dateTime.Now,
        };

        _unitOfWork.Add(studentTeamOperation);

        var schoolTeamOperation = new TeacherEmployedMSTeamOperation()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Staff,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = _dateTime.Now,
        };

        _unitOfWork.Add(schoolTeamOperation);
    }
    
    public async Task RemoveStudentMSTeamAccess(string studentId, OfferingId offeringId, DateTime schedule)
    {
        // Validate entries
        var student = await _unitOfWork.Students.GetForExistCheck(studentId);
        var offering = await _offeringRepository.GetById(offeringId);

        if (student == null || offering == null)
            return;

        var operation = new StudentMSTeamOperation()
        {
            StudentId = studentId,
            OfferingId = offeringId,
            DateScheduled = schedule,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _unitOfWork.Add(operation);
    }

    public async Task RemoveTeacherMSTeamAccess(string staffId, OfferingId offeringId, DateTime scheduled, Guid? coverId)
    {
        // Validate entries
        var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
        var offering = await _offeringRepository.GetById(offeringId);

        if (staffMember == null || offering == null)
            return;

        // Create Operation
        var operation = new TeacherMSTeamOperation
        {
            OfferingId = offering.Id,
            StaffId = staffMember.StaffId,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            DateScheduled = scheduled
        };

        if (coverId.HasValue)
            operation.CoverId = coverId.Value;

        _unitOfWork.Add(operation);
    }

    public async Task MarkMSTeamOperationComplete(int id)
    {
        // Validate entries
        var operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

        if (operation == null)
            return;

        operation.IsCompleted = true;
    }

    public async Task CancelMSTeamOperation(int id)
    {
        // Validate entries
        var operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

        if (operation == null)
            return;

        operation.IsDeleted = true;
    }
    public Task CreateCanvasUserFromStudent(Student student)
    {
        var operation = new CreateUserCanvasOperation()
        {
            UserId = student.StudentId,
            FirstName = student.FirstName,
            LastName = student.LastName,
            PortalUsername = student.PortalUsername,
            EmailAddress = student.EmailAddress
        };

        _unitOfWork.Add(operation);

        return Task.CompletedTask;
    }

    public Task CreateCanvasUserFromStaff(Staff staff)
    {
        var operation = new CreateUserCanvasOperation()
        {
            UserId = staff.StaffId,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            PortalUsername = staff.PortalUsername,
            EmailAddress = staff.EmailAddress
        };

        _unitOfWork.Add(operation);

        return Task.CompletedTask;
    }

    public async Task EnrolStudentInCanvasCourse(Student student, Offering offering, DateTime? scheduledFor = null)
    {
        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                student.StudentId,
                resource.CourseId,
                CanvasAction.Add,
                CanvasUserType.Student,
                scheduledFor);

            _unitOfWork.Add(operation);
        }

        await _unitOfWork.CompleteAsync();
    }

    public Task EnrolStaffInCanvasCourse(Staff staff, Offering offering, DateTime? scheduledFor = null)
    {
        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                staff.StaffId,
                resource.CourseId,
                CanvasAction.Add,
                CanvasUserType.Teacher,
                scheduledFor);
            
            _unitOfWork.Add(operation);
        }

        return Task.CompletedTask;
    }

    public Task UnenrolStudentFromCanvasCourse(Student student, Offering offering, DateTime? scheduledFor = null)
    {
        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                student.StudentId,
                resource.CourseId,
                CanvasAction.Remove,
                CanvasUserType.Student,
                scheduledFor);

            _unitOfWork.Add(operation);
        }

        return Task.CompletedTask;
    }

    public Task UnenrolStaffFromCanvasCourse(Staff staff, Offering offering, DateTime? scheduledFor = null)
    {
        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                staff.StaffId,
                resource.CourseId,
                CanvasAction.Remove,
                CanvasUserType.Teacher,
                scheduledFor);

            _unitOfWork.Add(operation);
        }
        return Task.CompletedTask;
    }

    public Task DisableCanvasUser(string UserId)
    {
        if (string.IsNullOrEmpty(UserId))
            return Task.CompletedTask;

        var operation = new DeleteUserCanvasOperation()
        {
            UserId = UserId
        };

        _unitOfWork.Add(operation);

        return Task.CompletedTask;
    }
}