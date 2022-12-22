namespace Constellation.Application.Students.GetCurrentStudentsAsDictionary;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentStudentsAsDictionaryQuery
    : IQuery<Dictionary<string, string>>;