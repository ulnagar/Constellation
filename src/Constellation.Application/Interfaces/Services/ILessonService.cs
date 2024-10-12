namespace Constellation.Application.Interfaces.Services;

using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using System.Threading.Tasks;

public interface ILessonService
{
    Task RemoveStudentFromFutureRollsForCourse(StudentId studentId, OfferingId offeringId);
    Task AddStudentToFutureRollsForCourse(StudentId studentId, string schoolCode, OfferingId offeringId);
}
