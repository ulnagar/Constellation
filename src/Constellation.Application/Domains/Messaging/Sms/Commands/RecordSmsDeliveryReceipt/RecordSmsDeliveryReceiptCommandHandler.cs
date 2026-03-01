namespace Constellation.Application.Domains.Messaging.Sms.Commands.RecordSmsDeliveryReceipt;

using Abstractions.Messaging;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Text;

internal sealed class RecordSmsDeliveryReceiptCommandHandler
: ICommandHandler<RecordSmsDeliveryReceiptCommand>
{
    public RecordSmsDeliveryReceiptCommandHandler()
    {
        
    }

    public async Task<Result> Handle(RecordSmsDeliveryReceiptCommand request, CancellationToken cancellationToken)
    {
        
    }
}
