﻿namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using System;
using System.Threading.Tasks;

public interface IOperationService
{
    Task CreateStudentMSTeamMemberAccess(string studentId, OfferingId offeringId, DateTime schedule);
    Task CreateTeacherMSTeamMemberAccess(string staffId, OfferingId offeringId, DateTime scheduled, Guid? coverId);
    Task CreateTeacherMSTeamOwnerAccess(string staffId, OfferingId offeringId, DateTime scheduled, Guid? coverId);
    Task CreateClassroomMSTeam(OfferingId offeringId, DateTime scheduled);
    Task RemoveStudentMSTeamAccess(string studentId, OfferingId offeringId, DateTime schedule);
    Task RemoveTeacherMSTeamAccess(string staffId, OfferingId offeringId, DateTime scheduled, Guid? coverId);

    Task MarkMSTeamOperationComplete(int id);
    Task CancelMSTeamOperation(int id);

    Task CreateStudentEnrolmentMSTeamAccess(string studentId);
    Task RemoveStudentEnrollmentMSTeamAccess(string studentId);
    Task CreateTeacherEmployedMSTeamAccess(string staffId);
    Task RemoveTeacherEmployedMSTeamAccess(string staffId);
    
    Task CreateCanvasUserFromStudent(Student student);
    Task CreateCanvasUserFromStaff(Staff staff);
    Task EnrolStudentInCanvasCourse(Student student, Offering offering, DateTime? scheduledFor = null);
    Task EnrolStaffInCanvasCourse(Staff staff, Offering offering, DateTime? scheduledFor = null);
    Task UnenrolStudentFromCanvasCourse(Student student, Offering offering, DateTime? scheduledFor = null);
    Task UnenrolStaffFromCanvasCourse(Staff staff, Offering offering, DateTime? scheduledFor = null);
    Task DisableCanvasUser(string userId);
}