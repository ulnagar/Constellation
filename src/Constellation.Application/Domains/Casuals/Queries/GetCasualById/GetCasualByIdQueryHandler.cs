﻿namespace Constellation.Application.Domains.Casuals.Queries.GetCasualById;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Shared;
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
            casual.Name.FirstName,
            casual.Name.LastName,
            casual.EmailAddress.Email,
            casual.SchoolCode,
            casual.EdvalTeacherId);
    }
}
