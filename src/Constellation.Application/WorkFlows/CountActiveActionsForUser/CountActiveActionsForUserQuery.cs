namespace Constellation.Application.WorkFlows.CountActiveActionsForUser;

using Abstractions.Messaging;

public sealed record CountActiveActionsForUserQuery(
    string StaffId)
    : IQuery<int>;