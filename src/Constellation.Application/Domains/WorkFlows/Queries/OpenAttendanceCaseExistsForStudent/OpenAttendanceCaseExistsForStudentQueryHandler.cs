namespace Constellation.Application.Domains.WorkFlows.Queries.OpenAttendanceCaseExistsForStudent;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class OpenAttendanceCaseExistsForStudentQueryHandler
: IQueryHandler<OpenAttendanceCaseExistsForStudentQuery, bool>
{
    private readonly ICaseRepository _caseRepository;

    public OpenAttendanceCaseExistsForStudentQueryHandler(
        ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<bool>> Handle(OpenAttendanceCaseExistsForStudentQuery request, CancellationToken cancellationToken) =>
        await _caseRepository.ExistingOpenAttendanceCaseForStudent(request.StudentId, cancellationToken);
}
