namespace Constellation.Core.Models.Auth.Enums;

using Constellation.Core.Common;

public sealed class LoginStatus : StringEnumeration<LoginStatus>
{
    public static readonly LoginStatus None = new("");

    public static readonly LoginStatus Success = new("Success");
    public static readonly LoginStatus Failed = new("Failed");
    public static readonly LoginStatus Started = new("Started");
    public static readonly LoginStatus SingleSignOn = new("SSO Success");
    
    private LoginStatus(string value)
        : base(value, value) { }

    public static IEnumerable<LoginStatus> GetOptions => GetEnumerable;
}