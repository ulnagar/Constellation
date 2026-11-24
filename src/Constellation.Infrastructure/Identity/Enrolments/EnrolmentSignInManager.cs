namespace Constellation.Infrastructure.Identity.Enrolments;

using Application.Domains.EnrolmentContext.Helpers;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class EnrolmentSignInManager : SignInManager<EnrolmentUser>
{
    public EnrolmentSignInManager(
        UserManager<EnrolmentUser> userManager, 
        IHttpContextAccessor contextAccessor, 
        IUserClaimsPrincipalFactory<EnrolmentUser> claimsFactory, 
        IOptions<IdentityOptions> optionsAccessor, 
        ILogger<SignInManager<EnrolmentUser>> logger, 
        IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<EnrolmentUser> confirmation) 
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        AuthenticationScheme = EnrolmentsIdentityConstants.AuthenticationScheme;
    }
}