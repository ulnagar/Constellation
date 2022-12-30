namespace Constellation.Application.AdminDashboards.AuditUser;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record AuditUserCommand(Guid UserId)
    : ICommand;