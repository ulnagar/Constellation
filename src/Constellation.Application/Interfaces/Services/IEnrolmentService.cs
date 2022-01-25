using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IEnrolmentService
    {
        Task CreateEnrolment(string studentId, int offeringId, DateTime dateCreated);
        Task RemoveEnrolment(int enrolmentId);
    }
}
