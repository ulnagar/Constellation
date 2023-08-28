using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IStudentService
    {
        Task<ServiceOperationResult<Student>> CreateStudent(StudentDto studentResource);
        Task<ServiceOperationResult<Student>> UpdateStudent(string studentId, StudentDto studentResource);
        Task ReinstateStudent(string studentId);
    }
}
