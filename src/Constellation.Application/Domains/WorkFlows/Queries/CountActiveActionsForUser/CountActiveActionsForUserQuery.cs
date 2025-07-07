namespace Constellation.Application.Domains.WorkFlows.Queries.CountActiveActionsForUser;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record CountActiveActionsForUserQuery(
    StaffId StaffId)
    : IQuery<int>;