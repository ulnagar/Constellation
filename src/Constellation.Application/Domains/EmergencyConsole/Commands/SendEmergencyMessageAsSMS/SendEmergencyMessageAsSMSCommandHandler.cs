namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsSMS;

using Abstractions.Messaging;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal sealed class SendEmergencyMessageAsSMSCommandHandler
: ICommandHandler<SendEmergencyMessageAsSMSCommand>
{
    public SendEmergencyMessageAsSMSCommandHandler()
    {
        
    }

    public async Task<Result> Handle(SendEmergencyMessageAsSMSCommand request, CancellationToken cancellationToken)
    {
        return Result.Success();
    }
}
 