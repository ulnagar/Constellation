﻿namespace Constellation.Core.Models.Covers;

using Constellation.Core.Primitives;
using System;

public sealed class Casual : Entity, IAuditableEntity
{
    private Casual(
        Guid id,
        string firstName,
        string lastName,
        string displayName,
        string emailAddress,
        string adobeConnectId,
        string schoolCode)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        EmailAddress = emailAddress;
        AdobeConnectPrincipalId = adobeConnectId;
        SchoolCode = schoolCode;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DisplayName { get; private set; }
    public string EmailAddress { get; private set; }
    public string AdobeConnectPrincipalId { get; private set; }
    public string SchoolCode { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

}