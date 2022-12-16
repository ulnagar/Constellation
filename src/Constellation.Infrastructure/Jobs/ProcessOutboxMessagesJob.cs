namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Core.Primitives;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Outbox;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

public class ProcessOutboxMessagesJob : IProcessOutboxMessagesJob, IHangfireJob
{
    private readonly AppDbContext _context;
    private readonly IPublisher _publisher;

    public ProcessOutboxMessagesJob(AppDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
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
                .DeserializeObject<IDomainEvent>(message.Content);

            if (domainEvent is null)
            {
                // TODO: Handle properly
                continue;
            }

            AsyncRetryPolicy policy = Policy
                .Handle<Exception>()
                .RetryAsync(3);

            PolicyResult result = await policy.ExecuteAndCaptureAsync(() => 
                _publisher.Publish(domainEvent, token));

            if (result.FinalException is not null)
            {
                // TODO: Log error or report somewhere
                message.Error = result.FinalException.ToString();
            }

            message.ProcessedOn = DateTime.Now;
        }

        await _context.SaveChangesAsync(token);
    }
}
