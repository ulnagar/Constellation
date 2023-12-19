namespace Constellation.Application.Students.CountStudentsWithoutSentralId;

using Abstractions.Messaging;
using Core.Shared;
using Interfaces.Repositories;
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
