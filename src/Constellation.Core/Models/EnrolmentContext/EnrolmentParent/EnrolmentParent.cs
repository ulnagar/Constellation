namespace Constellation.Core.Models.EnrolmentContext.EnrolmentParent;

using Constellation.Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using ValueObjects;

public sealed class EnrolmentParent
{
    private EnrolmentParent()
    {
        
    }

    public EnrolmentParentId Id { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
}
