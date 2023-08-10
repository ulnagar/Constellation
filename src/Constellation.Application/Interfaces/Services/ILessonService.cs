namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Threading.Tasks;

public interface ILessonService
{
    Task RemoveStudentFromFutureRollsForCourse(string studentId, int offeringId);
    Task AddStudentToFutureRollsForCourse(string studentId, string schoolCode, int offeringId);
}
