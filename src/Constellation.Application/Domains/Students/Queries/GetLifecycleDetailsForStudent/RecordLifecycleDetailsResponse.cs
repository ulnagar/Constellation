namespace Constellation.Application.Domains.Students.Queries.GetLifecycleDetailsForStudent;

using System;

public sealed record RecordLifecycleDetailsResponse(
    string CreatedBy,
    DateTime CreatedAt,
    string ModifiedBy,
    DateTime ModifiedAt,
    string DeletedBy,
    DateTime? DeletedAt);