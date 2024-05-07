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
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
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
        Student checkStudent = await _unitOfWork.Students.GetForExistCheck(studentId);
        AdobeConnectRoom checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStudent == null || checkRoom == null)
            return;

        StudentAdobeConnectOperation operation = new()
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
        Staff checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);
        AdobeConnectRoom checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStaff == null || checkRoom == null)
            return;

        TeacherAdobeConnectOperation operation = new()
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
        Student checkStudent = await _unitOfWork.Students.GetForExistCheck(studentId);
        AdobeConnectRoom checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStudent == null || checkRoom == null)
            return;

        StudentAdobeConnectOperation operation = new()
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
        Staff checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);
        AdobeConnectRoom checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

        if (checkStaff == null || checkRoom == null)
            return;

        TeacherAdobeConnectOperation operation = new()
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
        Staff checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (checkStaff == null)
            return;

        TeacherAdobeConnectGroupOperation operation = new()
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
        Staff checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (checkStaff == null)
            return;

        TeacherAdobeConnectGroupOperation operation = new()
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
        AdobeConnectOperation operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

        if (operation == null)
            return;

        if (operation.IsCompleted || operation.IsDeleted)
            return;

        operation.IsCompleted = true;
    }

    public async Task MarkAdobeConnectOperationCompleteAsync(int id)
    {
        // Validate entries
        AdobeConnectOperation operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

        if (operation == null)
            return;

        if (operation.IsCompleted || operation.IsDeleted)
            return;

        operation.IsCompleted = true;
    }

    public async Task CancelAdobeConnectOperation(int id)
    {
        // Validate entries
        AdobeConnectOperation operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

        if (operation == null)
            return;

        CancelAdobeConnectOperation(operation);
    }

    public void CancelAdobeConnectOperation(AdobeConnectOperation operation)
    {
        if (operation is null || operation.IsCompleted || operation.IsDeleted)
            return;

        operation.IsDeleted = true;
    }

    public async Task CreateStudentMSTeamMemberAccess(string studentId, OfferingId offeringId, DateTime schedule)
    {
        // Validate entries
        Student student = await _unitOfWork.Students.GetForExistCheck(studentId);
        Offering offering = await _offeringRepository.GetById(offeringId);

        if (student == null || offering == null)
            return;

        StudentMSTeamOperation operation = new()
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
        Staff staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
        Offering offering = await _offeringRepository.GetById(offeringId);

        if (staffMember == null || offering == null)
            return;

        // Create Operation
        TeacherMSTeamOperation operation = new()
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
        Staff staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
        Offering offering = await _offeringRepository.GetById(offeringId);

        if (staffMember == null || offering == null)
            return;

        // Create Operation
        TeacherMSTeamOperation operation = new()
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
        Offering offering = await _offeringRepository.GetById(offeringId);

        if (offering is null)
            return;

        Course course = await _courseRepository.GetById(offering.CourseId);

        if (course is null)
            return;

        // Create Operation
        GroupMSTeamOperation operation = new()
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
        Student student = await _unitOfWork.Students.GetForExistCheck(studentId);

        if (student == null)
            return;

        StudentEnrolledMSTeamOperation operation = new()
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
        Student student = await _unitOfWork.Students.GetForExistCheck(studentId);

        if (student == null)
            return;

        StudentEnrolledMSTeamOperation operation = new()
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
        Staff staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (staffMember == null)
            return;

        // Create Operation
        TeacherEmployedMSTeamOperation studentTeamOperation = new()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Add,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _unitOfWork.Add(studentTeamOperation);

        TeacherEmployedMSTeamOperation schoolTeamOperation = new()
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
        Staff staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);

        if (staffMember == null)
            return;

        // Create Operation
        TeacherEmployedMSTeamOperation studentTeamOperation = new()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = _dateTime.Now,
        };

        _unitOfWork.Add(studentTeamOperation);

        TeacherEmployedMSTeamOperation schoolTeamOperation = new()
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
        Student student = await _unitOfWork.Students.GetForExistCheck(studentId);
        Offering offering = await _offeringRepository.GetById(offeringId);

        if (student == null || offering == null)
            return;

        StudentMSTeamOperation operation = new()
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
        Staff staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
        Offering offering = await _offeringRepository.GetById(offeringId);

        if (staffMember == null || offering == null)
            return;

        // Create Operation
        TeacherMSTeamOperation operation = new()
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
        MSTeamOperation operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

        if (operation == null)
            return;

        operation.IsCompleted = true;
    }

    public async Task CancelMSTeamOperation(int id)
    {
        // Validate entries
        MSTeamOperation operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

        if (operation == null)
            return;

        operation.IsDeleted = true;
    }

    public Task CreateCanvasUserFromStudent(Student student)
    {
        if (student is null)
            return Task.CompletedTask;

        CreateUserCanvasOperation operation = new(
            student.StudentId,
            student.FirstName,
            student.LastName,
            student.PortalUsername,
            student.EmailAddress);

        _unitOfWork.Add(operation);

        return Task.CompletedTask;
    }

    public Task CreateCanvasUserFromStaff(Staff staff)
    {
        if (staff is null)
            return Task.CompletedTask;

        CreateUserCanvasOperation operation = new(
            staff.StaffId,
            staff.FirstName,
            staff.LastName,
            staff.PortalUsername,
            staff.EmailAddress);

        _unitOfWork.Add(operation);

        return Task.CompletedTask;
    }

    public async Task EnrolStudentInCanvasCourse(Student student, Offering offering, DateTime? scheduledFor = null)
    {
        if (student is null || offering is null)
            return;

        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                student.StudentId,
                resource.CourseId.ToString(),
                CanvasAction.Add,
                CanvasUserType.Student,
                scheduledFor);

            _unitOfWork.Add(operation);
        }

        await _unitOfWork.CompleteAsync();
    }

    public Task EnrolStaffInCanvasCourse(Staff staff, Offering offering, DateTime? scheduledFor = null)
    {
        if (staff is null || offering is null)
            return Task.CompletedTask;

        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                staff.StaffId,
                resource.CourseId.ToString(),
                CanvasAction.Add,
                CanvasUserType.Teacher,
                scheduledFor);
            
            _unitOfWork.Add(operation);
        }

        return Task.CompletedTask;
    }

    public Task UnenrolStudentFromCanvasCourse(Student student, Offering offering, DateTime? scheduledFor = null)
    {
        if (student is null || offering is null)
            return Task.CompletedTask;

        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                student.StudentId,
                resource.CourseId.ToString(),
                CanvasAction.Remove,
                CanvasUserType.Student,
                scheduledFor);

            _unitOfWork.Add(operation);
        }

        return Task.CompletedTask;
    }

    public Task UnenrolStaffFromCanvasCourse(Staff staff, Offering offering, DateTime? scheduledFor = null)
    {
        if (staff is null || offering is null)
            return Task.CompletedTask;

        List<CanvasCourseResource> resources = offering
            .Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                staff.StaffId,
                resource.CourseId.ToString(),
                CanvasAction.Remove,
                CanvasUserType.Teacher,
                scheduledFor);

            _unitOfWork.Add(operation);
        }

        return Task.CompletedTask;
    }

    public Task DisableCanvasUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.CompletedTask;

        DeleteUserCanvasOperation operation = new(userId);

        _unitOfWork.Add(operation);

        return Task.CompletedTask;
    }
}