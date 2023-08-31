namespace Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public record GetCurrentOfferingsForTeacherQuery(
    string Username) 
    : IQuery<Dictionary<string, OfferingId>>;