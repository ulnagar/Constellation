using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operation
    public class SentralService : ISentralService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISentralGateway _sentralGateway;

        public SentralService(IUnitOfWork unitOfWork, ISentralGateway sentralGateway)
        {
            _unitOfWork = unitOfWork;
            _sentralGateway = sentralGateway;
        }

        public async Task<string> UpdateSentralStudentId(string studentId)
        {
            var student = await _unitOfWork.Students.GetForExistCheck(studentId);

            if (!string.IsNullOrWhiteSpace(student.SentralStudentId))
                return student.SentralStudentId;

            var newId = await _sentralGateway.GetSentralStudentIdFromSRN(student.StudentId, ((int)student.CurrentGrade).ToString());

            if (student.SentralStudentId != newId)
            {
                student.SentralStudentId = newId;
                await _unitOfWork.CompleteAsync();
            }

            return string.IsNullOrWhiteSpace(student.SentralStudentId) ? null : student.SentralStudentId;
        }
    }
}
