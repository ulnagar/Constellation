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

public class ProcessOutboxMessagesJob : IProcessOutboxMessagesJob, IHangfireJob
{
    private readonly AppDbContext _context;
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;

    public ProcessOutboxMessagesJob(AppDbContext context, IPublisher publisher, Serilog.ILogger logger)
    {
        _context = context;
        _publisher = publisher;
        _logger = logger.ForContext<IProcessOutboxMessagesJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        var messages = await _context
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .Take(20)
            .ToListAsync(token);

        foreach (OutboxMessage message in messages)
        {
            var domainEvent = JsonConvert
                .DeserializeObject<IDomainEvent>(
                    message.Content,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

            if (domainEvent is null)
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

            PolicyResult result = await policy.ExecuteAndCaptureAsync(() => 
                _publisher.Publish(domainEvent, token));

            if (result.FinalException is not null)
            {
                // TODO: Log error or report somewhere
                _logger.Warning("Failed to process job {@job} with error {@error}", domainEvent, result.FinalException);

                message.Error = result.FinalException.ToString();
            }

            message.ProcessedOn = DateTime.Now;
        }

        await _context.SaveChangesAsync(token);
    }
}
