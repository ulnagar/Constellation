﻿namespace Constellation.Application.Families.GetFamilyContactsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Families.Models;
using System.Collections.Generic;

public sealed record GetFamilyContactsForStudentQuery(
    string StudentId)
    : IQuery<List<FamilyContactResponse>>;