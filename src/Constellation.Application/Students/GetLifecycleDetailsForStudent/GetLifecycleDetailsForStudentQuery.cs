namespace Constellation.Application.Students.GetLifecycleDetailsForStudent;

using Abstractions.Messaging;

public sealed record GetLifecycleDetailsForStudentQuery(
    string StudentId)
    : IQuery<RecordLifecycleDetailsResponse>;