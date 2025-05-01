namespace Constellation.Application.Domains.Students.Queries.CountStudentsWithAbsenceScanDisabled;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountStudentsWithAbsenceScanDisabledQueryHandler
: IQueryHandler<CountStudentsWithAbsenceScanDisabledQuery, (int Whole, int Partial)>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public CountStudentsWithAbsenceScanDisabledQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger.ForContext<CountStudentsWithAbsenceScanDisabledQuery>();
    }

    public async Task<Result<(int Whole, int Partial)>> Handle(CountStudentsWithAbsenceScanDisabledQuery request, CancellationToken cancellationToken)
    {
        int partial = await _studentRepository.GetCountCurrentStudentsWithPartialAbsenceScanDisabled(cancellationToken);

        int whole = await _studentRepository.GetCountCurrentStudentsWithWholeAbsenceScanDisabled(cancellationToken);

        return (whole, partial);
    }
}
