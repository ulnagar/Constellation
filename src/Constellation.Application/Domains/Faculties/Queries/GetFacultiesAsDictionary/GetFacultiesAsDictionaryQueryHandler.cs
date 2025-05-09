﻿namespace Constellation.Application.Domains.Faculties.Queries.GetFacultiesAsDictionary;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFacultiesAsDictionaryQueryHandler
: IQueryHandler<GetFacultiesAsDictionaryQuery, Dictionary<FacultyId, string>>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetFacultiesAsDictionaryQueryHandler(
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _logger = logger;
    }

    public async Task<Result<Dictionary<FacultyId, string>>> Handle(GetFacultiesAsDictionaryQuery request, CancellationToken cancellationToken)
    {
        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        return faculties.ToDictionary(key => key.Id, value => value.Name);
    }
}

