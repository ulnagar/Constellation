﻿namespace Constellation.Application.Features.API.Operations.Queries;

using Constellation.Core.Abstractions.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTeamIdForOfferingQueryHandler : IRequestHandler<GetTeamIdForOfferingQuery, string>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamIdForOfferingQueryHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<string> Handle(GetTeamIdForOfferingQuery request, CancellationToken cancellationToken)
    {
        var id = await _teamRepository
            .GetIdByOffering(request.ClassName, request.Year, cancellationToken);

        if (id is null || id == Guid.Empty)
        {
            return null;
        }

        return id.ToString();
    }
}