namespace Constellation.Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplicationById;

using Abstractions.Messaging;
using Models;
using System;
using ApplicationId = Core.Models.StudentOnboarding.Identifiers.ApplicationId;

public sealed record GetEnrolmentApplicationByIdQuery(
    ApplicationId ApplicationId)
    : IQuery<EnrolmentApplicationResponse>;
