namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System.Collections.Generic;

public class Contact : BaseAuditableEntity
{
    public string FirstName { get; set; }
    public string PreferredName { get; set; }
    public string LastName { get; set; }
    public string DisplayName => $"{(string.IsNullOrWhiteSpace(PreferredName) ? FirstName : PreferredName)} {LastName}";
    public Gender Gender { get; set; }

    public string EmailAddress { get; set; }

    public IList<ContactRole> Roles { get; private set; } = new List<ContactRole>();
