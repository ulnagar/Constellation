namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IEnrolmentRepository
{
    Task<List<Enrolment>> GetCurrentByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<List<Enrolment>> GetCurrentByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    void Insert(Enrolment enrolment);
}