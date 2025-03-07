namespace Constellation.Application.SchoolContacts.ExportContactsBySchool;

using Abstractions.Messaging;
using DTOs;

public sealed record ExportContactsBySchoolQuery(
    bool IncludeRestrictedContacts = false)
    : IQuery<FileDto>;