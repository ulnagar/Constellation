namespace Constellation.Core.Models.Attendance.Repositories;

using Checkin;
using Core.Enums;
using Offerings.Identifiers;
using Subjects.Identifiers;

public interface ICheckInRepository
{
    Task<List<CheckInResponse>> GetAll(CancellationToken cancellationToken = default);
    Task<List<CheckInResponse>> GetFromGrade(Grade grade, CancellationToken cancellationToken = default);
    Task<List<CheckInResponse>> GetFromCourse(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<CheckInResponse>> GetFromOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<CheckInResponse>> GetFromSchool(string schoolCode, CancellationToken cancellationToken = default);
    void Insert(CheckInResponse item);
}