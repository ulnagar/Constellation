namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;

public class ResponseVerificationStatus : StringEnumeration<ResponseVerificationStatus>
{
    public static ResponseVerificationStatus NotRequired = new("NR", "Not Required");
    public static ResponseVerificationStatus Pending = new("Pending", "Pending");
    public static ResponseVerificationStatus Rejected = new("Rejected", "Rejected");
    public static ResponseVerificationStatus Verified = new("Verified", "Verified");

    public ResponseVerificationStatus(string value, string name)
        : base(value, name) { }
}