using Scrutor;

namespace Constellation.Infrastructure.Idempotence;

public static class IImplementationTypeSelectorExtensions
{
    public static IImplementationTypeSelector RegisterHandlers(
        this IImplementationTypeSelector selector,
        Type type)
    {
        return selector.AddClasses(c =>
            c.AssignableTo(type)
                .Where(t => t != typeof(IdempotentDomainEventHandler<>)))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsImplementedInterfaces()
            .WithScopedLifetime();
    }
}
