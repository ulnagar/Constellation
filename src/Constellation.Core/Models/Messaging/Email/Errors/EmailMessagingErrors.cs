namespace Constellation.Core.Models.Messaging.Email.Errors;

using Enums;
using Shared;
using System.Collections.Generic;

public static class EmailMessagingErrors
{
    public static Error DuplicateRecipient(string email) => new(
        "EmailMessage.DuplicateRecipient",
        $"The email address '{email}' has already been added to this message.");

    public static Error DuplicateRecipient(IEnumerable<string> emails) => new(
        "EmailMessage.DuplicateRecipient",
        $"The following email addresses have already been added to this message: {string.Join(", ", emails)}.");

    public static Error DuplicateRecipientInBatch(IEnumerable<string> emails) => new(
        "EmailMessage.DuplicateRecipientInBatch",
        $"The following email addresses appear more than once in the recipient list: {string.Join(", ", emails)}.");

    public static Error InvalidStatusTransition(EmailStatus current, EmailStatus attempted) => new(
        "EmailMessage.InvalidStatusTransition",
            $"Cannot transition email status from '{current}' to '{attempted}'.")
            ;
    public static Error SendFailed(string error) => new(
        "EmailMessage.SendFailed", 
        error);
}
