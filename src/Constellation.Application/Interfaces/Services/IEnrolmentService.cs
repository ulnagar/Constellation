using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IEnrolmentService
    {
        Task CreateEnrolment(string studentId, OfferingId offeringId, DateTime dateCreated);
        Task RemoveEnrolment(int enrolmentId);
    }
}
