namespace Constellation.Application.Students.GetLifecycleDetailsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record GetLifecycleDetailsForStudentQuery(
    StudentId StudentId)
    : IQuery<RecordLifecycleDetailsResponse>;