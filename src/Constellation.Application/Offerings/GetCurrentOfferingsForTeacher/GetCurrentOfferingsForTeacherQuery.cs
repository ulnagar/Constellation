namespace Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public record GetCurrentOfferingsForTeacherQuery(
    string StaffId = null,
    string EmailAddress = null) 
    : IQuery<List<TeacherOfferingResponse>>;