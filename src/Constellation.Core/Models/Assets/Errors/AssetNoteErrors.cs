using Constellation.Core.Shared;

namespace Constellation.Core.Models.Assets.Errors;

public static class AssetNoteErrors
{
    public static readonly Error MessageEmpty = new(
        "Assets.Note.MessageEmpty",
        "Message must not be empty");
}