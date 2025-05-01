namespace Constellation.Application.Domains.WorkFlows.Queries.CountActiveActionsForUser;

using Abstractions.Messaging;

public sealed record CountActiveActionsForUserQuery(
    string StaffId)
    : IQuery<int>;