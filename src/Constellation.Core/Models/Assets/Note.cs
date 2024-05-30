#nullable enable
namespace Constellation.Core.Models.Assets;

using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;


public sealed record Note : IAuditableEntity
{
    // Required by EF Core
    private Note() { }

    private Note(
        AssetId assetId,
        string message)
    {
        AssetId = assetId;
        Message = message;
    }

    public NoteId Id { get; private set; } = new();
    public AssetId AssetId { get; private set; }
    public string Message { get; private set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Result<Note> Create(
        AssetId assetId,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure<Note>(NoteErrors.MessageEmpty);

        Note note = new(
            assetId,
            message);

        return note;
    }
}