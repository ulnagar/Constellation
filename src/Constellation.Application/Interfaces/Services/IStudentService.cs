using Constellation.Application.DTOs;
using Constellation.Core.Models.Students;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IStudentService
    {
        Task<ServiceOperationResult<Student>> UpdateStudent(string studentId, StudentDto studentResource);
    }
}
