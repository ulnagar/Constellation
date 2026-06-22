namespace Constellation.Core.Models.EnrolmentContext.Application;

using Identifiers;
using ValueObjects;

public sealed class Parent
{
    public ParentId Id { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress? EmailAddress { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
}