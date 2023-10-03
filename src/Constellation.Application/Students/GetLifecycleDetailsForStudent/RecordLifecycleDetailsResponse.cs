namespace Constellation.Application.Students.GetLifecycleDetailsForStudent;

using System;

public sealed record RecordLifecycleDetailsResponse(
    string CreatedBy,
    DateTime CreatedAt,
    string ModifiedBy,
    DateTime ModifiedAt,
    string DeletedBy,
    DateTime? DeletedAt);