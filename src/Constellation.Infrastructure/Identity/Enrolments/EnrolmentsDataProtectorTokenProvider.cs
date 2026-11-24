namespace Constellation.Infrastructure.Identity.Enrolments;

using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class EnrolmentsDataProtectorTokenProvider<TUser> 
    : DataProtectorTokenProvider<EnrolmentUser>
{
    public EnrolmentsDataProtectorTokenProvider(
        IDataProtectionProvider dataProtectionProvider, 
        IOptions<EnrolmentsTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<EnrolmentUser>> logger)
        : base(dataProtectionProvider, options, logger) { }
}