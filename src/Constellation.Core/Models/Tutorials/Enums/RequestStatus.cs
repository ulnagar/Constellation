namespace Constellation.Core.Models.Tutorials.Enums;

using Core.Common;
using System.Collections.Generic;

public sealed class RequestStatus : StringEnumeration<RequestStatus>
{
    public static readonly RequestStatus Requested = new("Requested", "Requested");
    public static readonly RequestStatus Approved = new("Approved", "Approved");
    public static readonly RequestStatus Scheduled = new("Scheduled", "Scheduled");
    public static readonly RequestStatus Rejected = new("Rejected", "Rejected");
    
    private RequestStatus(string value, string name)
        : base(value, name) { }

    public static IEnumerable<RequestStatus> GetOptions => GetEnumerable;
}