namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplates;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole;
using System.Collections.Generic;

public sealed record GetEmergencyConsoleMessageTemplatesQuery()
    : IQuery<List<MessageTemplate>>;
