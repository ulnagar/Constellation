namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentStudentsAsDictionaryQuery
    : IQuery<Dictionary<StudentId, string>>;