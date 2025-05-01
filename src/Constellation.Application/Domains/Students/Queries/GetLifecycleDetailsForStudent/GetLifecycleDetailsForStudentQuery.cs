namespace Constellation.Application.Domains.Students.Queries.GetLifecycleDetailsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record GetLifecycleDetailsForStudentQuery(
    StudentId StudentId)
    : IQuery<RecordLifecycleDetailsResponse>;