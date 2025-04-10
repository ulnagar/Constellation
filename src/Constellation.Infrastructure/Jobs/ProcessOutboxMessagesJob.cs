#pragma warning disable CA2326
namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Core.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence.ConstellationContext;
using Persistence.ConstellationContext.Outbox;
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
        List<OutboxMessage> messages = await _context
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null && m.OccurredOn <= DateTime.Now)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(token);

        foreach (OutboxMessage message in messages)
        {
            IEvent eventItem = JsonConvert
                .DeserializeObject<IEvent>(
                    message.Content,
                    new JsonSerializerSettings
                    {
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                        TypeNameHandling = TypeNameHandling.All
                    });

            if (eventItem is null)
            {
                _logger.Error("Failed to deserialize job: {@message}", message);

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
                _logger.Error("Failed to process job {@job} with error {@error}", eventItem, result.FinalException);

                message.Error = result.FinalException.ToString();
            }

            message.ProcessedOn = DateTime.Now;
        }

        await _context.SaveChangesAsync(token);
    }
}