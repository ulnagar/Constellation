namespace Constellation.Core.Models.Attachments.Errors;

using Shared;
using System;
using ValueObjects;

public static class AttachmentErrors
{
    public static readonly Func<AttachmentType, string, Error> NotFound = (type, linkId) => new(
        "Attachments.NotFound",
        $"Could not find an Attachment of type {type.Value} with LinkId {linkId}");

    public static readonly Func<AttachmentType, string, Error> NotFoundOnDisk = (type, linkId) => new(
        "Attachments.NotFoundOnDisk",
        $"Could not find the file for Attachment of type {type.Value} with LinkId {linkId}");

    // Continuing would orphan the old filepath value, leaving a file on disk that is no longer managed by the db.
    public static readonly Error FilePathExists = new(
        "Attachments.FilePathExists",
        "A file path already exists for this Attachment");

    // Continuing would overwrite the old filedata value
    public static readonly Error FileDataExists = new(
        "Attachments.FileDataExists",
        "A stored value already exists for this Attachment");
}
