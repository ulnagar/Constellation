#pragma warning disable CA2326
namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Core.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence.ConstellationContext;
using Persistence.EnrolmentContext;
using Persistence.Shared.Outbox;
using Polly;
using Polly.Retry;
using Serilog;

internal sealed class ProcessOutboxMessagesJob : IProcessOutboxMessagesJob
{
    private readonly ConstellationDbContext _constellationContext;
    private readonly EnrolmentDbContext _enrolmentContext;
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;

    private const int BatchSizePerContext = 20;

    public ProcessOutboxMessagesJob(
        ConstellationDbContext constellationContext, 
        EnrolmentDbContext enrolmentContext,
        IPublisher publisher, 
        ILogger logger)
    {
        _constellationContext = constellationContext;
        _enrolmentContext = enrolmentContext;
        _publisher = publisher;
        _logger = logger.ForContext<IProcessOutboxMessagesJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        List<OutboxMessage> constellationMessages = await _constellationContext
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSizePerContext)
            .ToListAsync(token);

        List<OutboxMessage> enrolmentMessages = await _enrolmentContext
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSizePerContext)
            .ToListAsync(token);

        if (constellationMessages.Count == 0 && enrolmentMessages.Count == 0)
            return;

        // Merge by OccurredOn so that if both contexts have pending
        // messages, they're dispatched in the order they actually
        // occurred rather than draining one context before the other.
        List<OutboxMessage> merged = constellationMessages
            .Concat(enrolmentMessages)
            .OrderBy(entry => entry.OccurredOn)
            .ToList();

        foreach (OutboxMessage message in merged)
        {
            IEvent? eventItem = JsonConvert
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

        bool constellationDirty = constellationMessages.Count > 0;
        bool enrolmentDirty = enrolmentMessages.Count > 0;

        if (constellationDirty)
            await _constellationContext.SaveChangesAsync(token);

        if (enrolmentDirty)
            await _enrolmentContext.SaveChangesAsync(token);
    }
}