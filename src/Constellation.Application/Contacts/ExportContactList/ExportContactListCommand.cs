namespace Constellation.Application.Contacts.ExportContactList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record ExportContactListCommand(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<ContactCategory> ContactCateogries)
    : ICommand<FileDto>;
