namespace Constellation.Infrastructure.ExternalServices.Sentral;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Core.Models.Students;
using System.Threading.Tasks;

public class Service : ISentralService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISentralGateway _sentralGateway;

    public Service(IUnitOfWork unitOfWork, ISentralGateway sentralGateway)
    {
        _unitOfWork = unitOfWork;
        _sentralGateway = sentralGateway;
    }

    public async Task<string> UpdateSentralStudentId(string studentId)
    {
        Student student = await _unitOfWork.Students.GetForExistCheck(studentId);

        if (!string.IsNullOrWhiteSpace(student.SentralStudentId))
            return student.SentralStudentId;

        string newId = await _sentralGateway.GetSentralStudentIdFromSRN(student.StudentId, ((int)student.CurrentGrade).ToString());

        if (student.SentralStudentId != newId)
        {
            student.SentralStudentId = newId;
            await _unitOfWork.CompleteAsync();
        }

        return string.IsNullOrWhiteSpace(student.SentralStudentId) ? null : student.SentralStudentId;
    }
}
