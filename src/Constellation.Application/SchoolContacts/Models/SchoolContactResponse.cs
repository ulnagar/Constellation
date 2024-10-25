﻿namespace Constellation.Application.SchoolContacts.Models;

using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;

public sealed record SchoolContactResponse(
    SchoolContactId Id,
    SchoolContactRoleId AssignmentId,
    Name Name,
    EmailAddress EmailAddress,
    PhoneNumber PhoneNumber,
    bool IsDirectNumber,
    string Role,
    string SchoolName,
    bool IsActivePartnerSchool,
    string Note,
    bool IsSelfRegistered);