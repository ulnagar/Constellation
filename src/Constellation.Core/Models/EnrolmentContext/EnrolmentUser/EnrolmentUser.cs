namespace Constellation.Core.Models.EnrolmentContext.EnrolmentUser;

using Errors;
using Microsoft.AspNetCore.Identity;
using Primitives;
using Shared;
using System;
using ValueObjects;

public sealed class EnrolmentUser : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];


    public EnrolmentUser()
    { }

    public string PreferredName { get; private set; } = string.Empty;

    public EmailAddress PendingEmail { get; private set; } = EmailAddress.None;

    public PhoneNumber PendingPhone { get; private set; } = Core.ValueObjects.PhoneNumber.Empty;

    public Result UpdatePreferredName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(EnrolmentUserErrors.PreferredNameRequired);

        PreferredName = name.Trim();
        return Result.Success();
    }

    public Result RequestEmailChange(EmailAddress newEmail)
    {
        if (newEmail == EmailAddress.None)
            return Result.Failure(EnrolmentUserErrors.EmailRequired);

        if (string.Equals(newEmail.Email, Email, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(EnrolmentUserErrors.EmailUnchanged);

        PendingEmail = newEmail;
        return Result.Success();
    }

    public Result ConfirmEmailChange()
    {
        if (PendingEmail != EmailAddress.None)
            PendingEmail = EmailAddress.None;

        return Result.Success();
    }

    public Result RequestPhoneChange(PhoneNumber newPhone)
    {
        if (newPhone == Core.ValueObjects.PhoneNumber.Empty)
            return Result.Failure(EnrolmentUserErrors.PhoneRequired);

        if (string.Equals(newPhone.Value, PhoneNumber, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(EnrolmentUserErrors.PhoneUnchanged);

        PendingPhone = newPhone;
        return Result.Success();
    }

    public Result ConfirmPhoneChange()
    {
        if (PendingPhone != Core.ValueObjects.PhoneNumber.Empty)
            PendingPhone = Core.ValueObjects.PhoneNumber.Empty;

        return Result.Success();
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
        => _domainEvents.ToList();

    public void ClearDomainEvents() 
        => _domainEvents.Clear();

    public void RaiseDomainEvent(IDomainEvent domainEvent) 
        => _domainEvents.Add(domainEvent);
}
