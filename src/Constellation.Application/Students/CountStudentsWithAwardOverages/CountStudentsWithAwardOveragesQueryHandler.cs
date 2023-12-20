namespace Constellation.Application.Students.CountStudentsWithAwardOverages;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountStudentsWithAwardOveragesQueryHandler
    : IQueryHandler<CountStudentsWithAwardOveragesQuery, int>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public CountStudentsWithAwardOveragesQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CountStudentsWithAwardOveragesQuery request, CancellationToken cancellationToken)
    {
        int count = await _studentRepository.GetCountCurrentStudentsWithAwardOverages(cancellationToken);

        return count;
    }
}
