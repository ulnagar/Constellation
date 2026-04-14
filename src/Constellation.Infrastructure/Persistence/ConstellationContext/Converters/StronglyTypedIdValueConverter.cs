namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.Primitives;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;
using System.Reflection;

internal sealed class StronglyTypedIdValueConverter<TId, TValue> : ValueConverter<TId, TValue>
    where TId : IStronglyTypedId<TId, TValue>
    where TValue : IEquatable<TValue>
{
    public StronglyTypedIdValueConverter() : base(
        id => id.Value,
        BuildFromValueExpression())
    { }

    private static Expression<Func<TValue, TId>> BuildFromValueExpression()
    {
        // Resolve the concrete static method on TId at converter construction time
        MethodInfo method = typeof(TId).GetMethod(
            nameof(IStronglyTypedId<TId, TValue>.FromValue),
            BindingFlags.Public | BindingFlags.Static,
            new[] { typeof(TValue) })!;

        ParameterExpression param = Expression.Parameter(typeof(TValue), "value");
        MethodCallExpression call = Expression.Call(method, param);

        return Expression.Lambda<Func<TValue, TId>>(call, param);
    }
}

public class StronglyTypedIdConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder builder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (IConventionEntityType entityType in builder.Metadata.GetEntityTypes())
        {
            foreach (IConventionProperty property in entityType.GetProperties())
            {
                Type type = property.ClrType;

                Type? matchingInterface = type
                    .GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IStronglyTypedId<,>));

                if (matchingInterface is null)
                    continue;

                Type[] typeArgs = matchingInterface.GetGenericArguments(); // [TId, TValue]

                Type converterType = typeof(StronglyTypedIdValueConverter<,>)
                    .MakeGenericType(typeArgs);

                ValueConverter converter = (ValueConverter)Activator.CreateInstance(converterType)!;

                property.Builder.HasConversion(converter);
            }
        }
    }
}