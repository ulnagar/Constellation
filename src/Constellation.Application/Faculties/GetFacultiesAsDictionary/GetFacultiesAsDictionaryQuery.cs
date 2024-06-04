namespace Constellation.Application.Faculties.GetFacultiesAsDictionary;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;
using System.Collections.Generic;

public sealed record GetFacultiesAsDictionaryQuery()
    : IQuery<Dictionary<FacultyId, string>>;