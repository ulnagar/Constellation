﻿namespace Constellation.Application.Casuals.GetActiveCasuals;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Casuals.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetActiveCasualsQueryHandler
    : IQueryHandler<GetActiveCasualsQuery, List<CasualsListResponse>>
{
    private readonly ICasualRepository _casualRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetActiveCasualsQueryHandler(
        ICasualRepository casualRepository,
        ISchoolRepository schoolRepository)
    {
        _casualRepository = casualRepository;
        _schoolRepository = schoolRepository;
    }
    public async Task<Result<List<CasualsListResponse>>> Handle(GetActiveCasualsQuery request, CancellationToken cancellationToken)
    {
        var returnData = new List<CasualsListResponse>();

        var casuals = await _casualRepository.GetAllActive(cancellationToken);

        if (casuals.Count == 0)
        {
            return returnData;
        }

        foreach (var casual in casuals)
        {
            string schoolName = string.Empty;

            if (!string.IsNullOrWhiteSpace(casual.SchoolCode))
            {
                var school = await _schoolRepository.GetById(casual.SchoolCode, cancellationToken);
                if (school is not null)
                {
                    schoolName = school.Name;
                }
            }

            var entry = new CasualsListResponse(
                casual.Id,
                casual.FirstName,
                casual.LastName,
                schoolName,
                casual.EmailAddress,
                !casual.IsDeleted);

            returnData.Add(entry);
        }

        return returnData;
    }
}
