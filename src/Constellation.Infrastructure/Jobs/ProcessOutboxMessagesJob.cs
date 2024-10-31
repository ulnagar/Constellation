namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Core.Primitives;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Outbox;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;

internal sealed class ProcessOutboxMessagesJob : IProcessOutboxMessagesJob
{
    private readonly AppDbContext _context;
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;

    public ProcessOutboxMessagesJob(
        AppDbContext context, 
        IPublisher publisher, 
        ILogger logger)
    {
        _context = context;
        _publisher = publisher;
        _logger = logger.ForContext<IProcessOutboxMessagesJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        var messages = await _context
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null && m.OccurredOn <= DateTime.Now)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(token);

        foreach (OutboxMessage message in messages)
        {
            var eventItem = JsonConvert
                .DeserializeObject<IEvent>(
                    message.Content,
                    new JsonSerializerSettings
                    {
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                        TypeNameHandling = TypeNameHandling.All
                    });

            if (eventItem is null)
            {
                // TODO: Handle properly
                _logger.Warning("Failed to deserialize job: {@message}", message);

                continue;
            }

            AsyncRetryPolicy policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromMilliseconds(50 * attempt));

            // To Prevent Circular Dependency Issues: https://www.davidguida.net/mediatr-how-to-use-decorators-to-add-retry-policies/
            PolicyResult result = await policy.ExecuteAndCaptureAsync(() => 
                _publisher.Publish(eventItem, token));

            if (result.FinalException is not null)
            {
                // TODO: Log error or report somewhere
                _logger.Warning("Failed to process job {@job} with error {@error}", eventItem, result.FinalException);

                message.Error = result.FinalException.ToString();
            }

            message.ProcessedOn = DateTime.Now;
        }

        await _context.SaveChangesAsync(token);
    }
}
