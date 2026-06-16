namespace Constellation.Core.Helpers;

using Constellation.Core.Primitives;

public static class GuidStronglyTypedIdExtensions
{
    extension<TSelf>(TSelf) where TSelf : IStronglyTypedId<TSelf, Guid>
    {
        public static bool TryParse(string? raw, out TSelf result)
        {
            if (Guid.TryParse(raw, out var guid))
            {
                result = TSelf.FromValue(guid);
                return true;
            }
            result = TSelf.Empty;
            return false;
        }
    }
}

public static class StringStronglyTypedIdExtensions
{
    extension<TSelf>(TSelf) where TSelf : IStronglyTypedId<TSelf, string>
    {
        public static bool TryParse(string? raw, out TSelf result)
        {
            result = raw is not null
                ? TSelf.FromValue(raw)
                : TSelf.Empty;
            return raw is not null;
        }
    }
}