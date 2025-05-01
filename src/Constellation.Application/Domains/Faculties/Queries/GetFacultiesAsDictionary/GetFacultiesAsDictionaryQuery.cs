namespace Constellation.Application.Domains.Faculties.Queries.GetFacultiesAsDictionary;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;
using System.Collections.Generic;

public sealed record GetFacultiesAsDictionaryQuery()
    : IQuery<Dictionary<FacultyId, string>>;