namespace Constellation.Application.Parents.GetParentWithStudentIds;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetParentWithStudentIdsQueryHandler
    : IQueryHandler<GetParentWithStudentIdsQuery, List<string>>
{
    private readonly IFamilyRepository _studentFamilyRepository;
    private readonly Serilog.ILogger _logger;

    public GetParentWithStudentIdsQueryHandler(
        IFamilyRepository studentFamilyRepository,
        Serilog.ILogger logger)
    {
        _studentFamilyRepository = studentFamilyRepository;
        _logger = logger.ForContext<GetParentWithStudentIdsQuery>();
    }

    public async Task<Result<List<string>>> Handle(GetParentWithStudentIdsQuery request, CancellationToken cancellationToken)
    {
        var studentIds = await _studentFamilyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        if (studentIds is null)
        {
            _logger.Information("");

            return Result.Failure<List<string>>(new Error("Partner.Students.Family.NotFound", "Family could not be found"));
        }

        return studentIds;
    }
}
