namespace Constellation.Application.Domains.EmergencyConsole.Commands.UpdateEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateEmergencyConsoleMessageTemplateCommandHandler
: ICommandHandler<UpdateEmergencyConsoleMessageTemplateCommand>
{
    public UpdateEmergencyConsoleMessageTemplateCommandHandler()
    {
        
    }

    public async Task<Result> Handle(UpdateEmergencyConsoleMessageTemplateCommand request, CancellationToken cancellationToken)
    {
        
    }
}
