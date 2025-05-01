namespace Constellation.Infrastructure.Jobs;

using Application.Domains.Auth.Commands.AuditAllUsers;
using Constellation.Application.Interfaces.Jobs;
using Core.Shared;

internal sealed class UserManagerJob : IUserManagerJob
{
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public UserManagerJob(
        ISender mediator,
        ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<IUserManagerJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.Information("{id}: Starting job", jobId);

        Result result = await _mediator.Send(new AuditAllUsersCommand(), token);

        _logger.Information("{id}: Stopping job", jobId);
    }
}