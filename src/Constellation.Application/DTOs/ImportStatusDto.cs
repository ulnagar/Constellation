#nullable enable
namespace Constellation.Application.DTOs;

using Core.Shared;

public sealed record ImportStatusDto(
    int RowNumber,
    bool Succeeded,
    Error? Error);
