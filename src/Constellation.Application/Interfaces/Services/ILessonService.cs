namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Threading.Tasks;

public interface ILessonService
{
    Task RemoveStudentFromFutureRollsForCourse(string studentId, OfferingId offeringId);
    Task AddStudentToFutureRollsForCourse(string studentId, string schoolCode, OfferingId offeringId);
}
