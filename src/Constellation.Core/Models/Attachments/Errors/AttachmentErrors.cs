namespace Constellation.Core.Models.Attachments.Errors;

using Shared;
using System;
using ValueObjects;

public static class AttachmentErrors
{
    public static Func<AttachmentType, string, Error> NotFound = (type, linkId) => new(
        "Attachments.NotFound",
        $"Could not find an Attachment of type {type.Value} with LinkId {linkId}");

    public static Func<AttachmentType, string, Error> NotFoundOnDisk = (type, linkId) => new(
        "Attachments.NotFoundOnDisk",
        $"Could not find the file for Attachment of type {type.Value} with LinkId {linkId}");
}
