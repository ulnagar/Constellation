﻿namespace Constellation.Application.Families.GetFamilyDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetFamilyDetailsByIdQuery(
    FamilyId FamilyId)
    : IQuery<FamilyDetailsResponse>;
