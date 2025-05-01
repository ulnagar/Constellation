namespace Constellation.Application.Domains.Students.Queries.CountStudentsWithoutSentralId;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountStudentsWithoutSentralIdQueryHandler
: IQueryHandler<CountStudentsWithoutSentralIdQuery, int>
{
    private readonly IStudentRepository _studentRepository;

    public CountStudentsWithoutSentralIdQueryHandler(
        IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<int>> Handle(CountStudentsWithoutSentralIdQuery request, CancellationToken cancellationToken)
    {
        int count = await _studentRepository.GetCountCurrentStudentsWithoutSentralId(cancellationToken);

        return count;
    }
}
