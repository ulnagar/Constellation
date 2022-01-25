using System;

namespace Constellation.Core.Enums
{
    [Flags]
    public enum Faculty
    {
        Administration = 1,
        Executive = 2,
        English = 4,
        Mathematics = 8,
        Science = 16,
        Support = 32,
        Stage3 = 64
    }
}