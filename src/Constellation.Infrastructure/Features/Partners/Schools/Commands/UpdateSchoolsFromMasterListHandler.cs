using Constellation.Application.Features.Partners.Schools.Commands;
using Constellation.Application.Interfaces.Gateways;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Schools.Commands
{
    public class UpdateSchoolsFromMasterListHandler : IRequestHandler<UpdateSchoolsFromMasterList>
    {
        private readonly IMediator _mediator;
        private readonly ICeseGateway _ceseGateway;

        public UpdateSchoolsFromMasterListHandler(IMediator mediator, ICeseGateway ceseGateway)
        {
            _mediator = mediator;
            _ceseGateway = ceseGateway;
        }

        public async Task<Unit> Handle(UpdateSchoolsFromMasterList request, CancellationToken cancellationToken)
        {
            var jsonObjects = await _ceseGateway.GetSchoolsFromMasterData();

            foreach (var jsonObject in jsonObjects)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Unit.Value;

                var command = jsonObject.ConvertToCommand();
                await _mediator.Send(command, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
