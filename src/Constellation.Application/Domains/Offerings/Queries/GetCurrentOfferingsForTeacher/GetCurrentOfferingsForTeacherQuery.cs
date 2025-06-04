namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public record GetCurrentOfferingsForTeacherQuery(
    StaffId StaffId,
    string EmailAddress = null) 
    : IQuery<List<TeacherOfferingResponse>>;