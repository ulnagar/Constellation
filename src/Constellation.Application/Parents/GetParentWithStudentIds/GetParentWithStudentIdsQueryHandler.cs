namespace Constellation.Application.Parents.GetParentWithStudentIds;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Core.Models.Students.Identifiers;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetParentWithStudentIdsQueryHandler
    : IQueryHandler<GetParentWithStudentIdsQuery, List<StudentId>>
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

    public async Task<Result<List<StudentId>>> Handle(GetParentWithStudentIdsQuery request, CancellationToken cancellationToken)
    {
        var studentIds = await _studentFamilyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        if (!studentIds.Any())
        {
            _logger.Information("");

            return Result.Failure<List<StudentId>>(new Error("Partner.Students.Family.NotFound", "Family could not be found"));
        }

        return studentIds.Keys.ToList();
    }
}
