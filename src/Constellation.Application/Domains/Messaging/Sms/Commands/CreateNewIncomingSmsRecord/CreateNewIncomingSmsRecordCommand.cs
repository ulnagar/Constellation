namespace Constellation.Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;

using Abstractions.Messaging;
using Models;

public sealed record CreateNewIncomingSmsRecordCommand(
    IncomingSms IncomingSms)
    : ICommand;