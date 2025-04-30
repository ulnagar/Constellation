namespace Constellation.Application.Extensions;

using System;

public static class ObjectExtensions
{
    public static TResult IfNotNull<TObject, TResult>(this TObject obj, Func<TObject, TResult> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        if (obj != null)
        {
            return action(obj);
        }

        return default(TResult);
    }
}