namespace Constellation.Application.Domains.Affirmations.Queries;

using Abstractions.Messaging;
using Core.Models.StaffMembers.ValueObjects;

public sealed record GetAffirmationQuery(
    EmployeeId EmployeeId)
    : IQuery<string>;