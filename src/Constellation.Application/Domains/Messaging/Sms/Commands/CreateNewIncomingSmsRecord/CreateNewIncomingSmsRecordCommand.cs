namespace Constellation.Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;

using Abstractions.Messaging;
using Dtos;

public sealed record CreateNewIncomingSmsRecordCommand(
    IncomingSms IncomingSms)
    : ICommand;