namespace Constellation.Application.Extensions;

using System;
using System.IO;

public static class StreamExtensions
{
    public static bool IsExcelFile(this Stream stream)
    {
        const string XLS_Signature = "D0-CF-11-E0-A1-B1-1A-E1";

        byte[] buffer = new byte[8];
        int length = stream.Read(buffer, 0, 8);

        if (length < 8)
            return false;

        string hex = BitConverter.ToString(buffer);
        // HTML = "3C-21-44-4F-43-54-59-50"
        // XLS = "D0-CF-11-E0-A1-B1-1A-E1"
        stream.Position = 0;

        return hex == XLS_Signature;
    }
}