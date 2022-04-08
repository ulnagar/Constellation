using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Constellation.Presentation.Portal.Schools.Pages.Auth
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMediator _mediator;

        public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            IMediator mediator)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        private class LoginResult
        {
            public bool Succeeded { get; set; }
            public bool RequiresTwoFactor { get; set; }
            public bool IsLockedOut { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl ?? Url.Content("~/");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

//#if DEBUG
//                var claimList = new List<Claim>();
//                claimList.Add(new Claim("Schools", "8146,8155,8343"));

//                await _signInManager.SignInWithClaimsAsync(user, false, claimList);

//                return LocalRedirect(returnUrl);
//#else
#pragma warning disable CA1416 // Validate platform compatibility
                var context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
                //var success = context.ValidateCredentials(Input.Email, Input.Password);
#pragma warning restore CA1416 // Validate platform compatibility
                var success = true;
                
                if (success)
                {
                    List<Claim> claimList = await DetermineUserSchools(user, context, Input.Email);

                    await _signInManager.SignInWithClaimsAsync(user, false, claimList);

                    return LocalRedirect(returnUrl);
                }
//#endif
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        private async Task<List<Claim>> DetermineUserSchools(AppUser user, PrincipalContext context, string emailAddress)
        {
            var linkedSchools = new List<string>();

            var claims = await _userManager.GetClaimsAsync(user);
            var dbClaim = claims.FirstOrDefault(claim => claim.Type == "Schools");

#pragma warning disable CA1416 // Validate platform compatibility
            var userAccount = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, emailAddress);
            using (DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry)
            {
                // detAttribute12 is the staff attribute that lists linked school codes
                // detAttribute3 is the staff attribute that contains the employee number

                try
                {
                    // Get list of users linked schools from AD
                    var adAttributeValue = adAccount.Properties["detAttribute12"].Value;
                    var adSchoolList = new List<string>();

                    // If the adAttributeValue is a string, it is a single school link
                    // If the adAttributeValue is an array, it is multiple school links

                    if (adAttributeValue.GetType() == typeof(string))
                        adSchoolList.Add(adAttributeValue as string);
                    else
                    {
                        foreach (var entry in adAttributeValue as Array)
                        {
                            adSchoolList.Add(entry as string);
                        }
                    }

                    // Check each school against the DB to ensure it is an active partner school with students
                    // Add any matching entries to the user claims
                    foreach (var code in adSchoolList)
                    {
                        if (await _mediator.Send(new IsPartnerSchoolWithStudentsQuery { SchoolCode = code }) && linkedSchools.All(entry => entry != code))
                            linkedSchools.Add(code);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
#pragma warning restore CA1416 // Validate platform compatibility

            // Get list of users linked schools from DB
            var dbUserSchools = await _mediator.Send(new GetLinkedSchoolCodesForUserQuery { UserEmail = emailAddress });

            // Check each school against the DB to ensure it is an active partner school with students
            // Add any matching entries to the user claims
            foreach (var code in dbUserSchools)
            {
                if (await _mediator.Send(new IsPartnerSchoolWithStudentsQuery { SchoolCode = code }) && linkedSchools.All(entry => entry != code))
                    linkedSchools.Add(code);
            }

            var claim = new Claim("Schools", string.Join(",", linkedSchools));

            // If the claim is not in the database: add it.
            // If the claim is different from what is in the database: remove and re-add with the correct information
            if (dbClaim == null)
            {
                await _userManager.AddClaimAsync(user, claim);
            } 
            else
            {
                if (dbClaim.Value != claim.Value)
                {
                    await _userManager.RemoveClaimAsync(user, dbClaim);
                    await _userManager.AddClaimAsync(user, claim);
                }
            }

            var claimList = new List<Claim>
            {
                claim
            };

            return claimList;
        }
    }
}
