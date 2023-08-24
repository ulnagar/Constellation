namespace Constellation.Application.Casuals.GetCasualById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCasualByIdQueryHandler
    : IQueryHandler<GetCasualByIdQuery, CasualResponse>
{
    private readonly ICasualRepository _casualRepository;

    public GetCasualByIdQueryHandler(
        ICasualRepository casualRepository)
    {
        _casualRepository = casualRepository;
    }

    public async Task<Result<CasualResponse>> Handle(GetCasualByIdQuery request, CancellationToken cancellationToken)
    {
        var casual = await _casualRepository.GetById(request.CasualId, cancellationToken);

        if (casual is null)
        {
            return Result.Failure<CasualResponse>(DomainErrors.Casuals.Casual.NotFound(request.CasualId));
        }

        return new CasualResponse(
            casual.Id,
            casual.FirstName,
            casual.LastName,
            casual.EmailAddress,
            casual.SchoolCode,
            casual.AdobeConnectId);
    }
}
