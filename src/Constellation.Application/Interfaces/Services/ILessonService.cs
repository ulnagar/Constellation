namespace Constellation.Application.Interfaces.Services;

using Constellation.Core.Models.Offerings.Identifiers;
using System.Threading.Tasks;

public interface ILessonService
{
    Task RemoveStudentFromFutureRollsForCourse(string studentId, OfferingId offeringId);
    Task AddStudentToFutureRollsForCourse(string studentId, string schoolCode, OfferingId offeringId);
}
