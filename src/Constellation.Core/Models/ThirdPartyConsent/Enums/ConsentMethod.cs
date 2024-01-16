namespace Constellation.Core.Models.ThirdPartyConsent.Enums;

using Common;

public class ConsentMethod : StringEnumeration<ConsentMethod>
{
    public static readonly ConsentMethod Portal = new("Portal", "Parent Portal");
    public static readonly ConsentMethod Email = new("Email", "Email from Parent");
    public static readonly ConsentMethod PhoneCall = new("PhoneCall", "Phone Call from Parent");

    private ConsentMethod(string value, string name)
        : base(value, name)
    { }
}