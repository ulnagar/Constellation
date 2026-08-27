namespace Constellation.Application.Interfaces.Services.Csv;

using Core.Shared;
using System;
using System.Collections.Generic;

public interface ICsvReader<T>
{
    Result<List<T>> Read(Stream stream, Func<string[], Result<T>> rowMapper, CsvReadOptions? options = null);
}