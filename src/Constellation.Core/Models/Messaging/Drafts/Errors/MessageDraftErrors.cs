namespace Constellation.Core.Models.Messaging.Drafts.Errors;

using Shared;

public static class MessageDraftErrors
{
    public static readonly Error NotFound = new(
        "MessageDraft.NotFound",
        "Could not find a draft for the user");

    public static readonly Error InvalidSender = new(
        "MessageDraft.InvalidSender",
        "The sender provided is not valid");

    public static class AddRecipient
    {
        public static readonly Error DuplicateEmailFound = new(
            "MessageDraft.AddRecipient.DuplicateEmailFound",
            "A recipient already exists with the same email");

        public static readonly Error DuplicatePhoneNumberFound = new(
            "MessageDraft.AddRecipient.DuplicatePhoneNumberFound",
            "A recipient already exists with the same phone number");
    }

    public static class RemoveRecipient
    {
        public static readonly Error NotFound = new(
            "MessageDraft.RemoveRecipient.NotFound",
            "Recipient was not found in the draft to remove");
    }
}
