namespace Constellation.Application.Domains.Students.Queries.ExportLastLoggedInDateForCurrentStudents;

using Constellation.Application.Abstractions.Messaging;

public sealed record ExportLastLoggedInDateForCurrentStudentsQuery()
    : IQuery<byte[]>;