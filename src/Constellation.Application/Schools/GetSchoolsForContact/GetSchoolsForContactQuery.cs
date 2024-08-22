﻿namespace Constellation.Application.Schools.GetSchoolsForContact;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;
using System.Collections.Generic;

public sealed record GetSchoolsForContactQuery(
    SchoolContactId ContactId,
    bool IsAdmin = false)
    : IQuery<List<SchoolResponse>>;