namespace Constellation.Application.Students.CountStudentsWithPendingAwards;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountStudentsWithPendingAwardsQueryHandler
: IQueryHandler<CountStudentsWithPendingAwardsQuery, int>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public CountStudentsWithPendingAwardsQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CountStudentsWithPendingAwardsQuery request, CancellationToken cancellationToken)
    {
        int count = await _studentRepository.GetCountCurrentStudentsWithPendingAwards(cancellationToken);

        return count;
    }
}
