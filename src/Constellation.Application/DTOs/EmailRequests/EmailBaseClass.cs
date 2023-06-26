namespace Constellation.Application.DTOs.EmailRequests;

using Constellation.Core.ValueObjects;
using System.Collections.Generic;

public abstract class EmailBaseClass
{
    public List<EmailRecipient> Recipients { get; set; } = new();
}
