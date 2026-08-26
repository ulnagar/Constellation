namespace Constellation.Application.Domains.Auth.Commands.UpdateUserNotificationPreferences;

using Abstractions.Messaging;
using Core.Models.Auth.Enums;
using System;

public sealed record UpdateUserNotificationPreferencesCommand(
    Guid UserId,
    List<NotificationType> Types)
    : ICommand;