using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IAbsenceService
    {
        ServiceOperationResult<StudentWholeAbsence> CreateWholeAbsence(WholeAbsenceDto absenceResource);
        ServiceOperationResult<StudentPartialAbsence> CreatePartialAbsence(PartialAbsenceDto absenceResource);
        Task CreateSingleStudentExplanation(Guid absenceId, string explanation);
        Task CreateSingleCoordinatorExplanation(Guid absenceId, string explanation, string userName);
        Task RecordCoordinatorVerificationOfPartialExplanation(Guid responseId, bool isVerified, string comment, string username);
    }
}