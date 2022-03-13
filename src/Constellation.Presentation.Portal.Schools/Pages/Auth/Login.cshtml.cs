using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
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
        private readonly IUnitOfWork _unitOfWork;

        public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
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
                var result = new LoginResult();

#if DEBUG
                var user = await _userManager.FindByEmailAsync(Input.Email);

                var claimList = new List<Claim>();
                claimList.Add(new Claim("Schools", "8146,8155"));

                await _signInManager.SignInWithClaimsAsync(user, false, claimList);

                return LocalRedirect(returnUrl);
#else
                var user = await _userManager.FindByEmailAsync(Input.Email);
#pragma warning disable CA1416 // Validate platform compatibility
                var context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
                var success = context.ValidateCredentials(Input.Email, Input.Password);
                
                if (success)
                {
                    var linkedSchools = new List<string>();

                    var userAccount = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, Input.Email);
                    using (DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry)
                    {
                        // detAttribute12 is the staff attribute that lists linked school codes
                        // detAttribute3 is the staff attribute that contains the employee number

                        try
                        {
                            // Get list of users linked schools from AD
                            var adAttributeValue = adAccount.Properties["detAttribute12"].Value.ToString();
                            var adSchoolList = JsonConvert.DeserializeObject<List<string>>(adAttributeValue);

                            // Check each school against the DB to ensure it is an active partner school with students
                            // Add any matching entries to the user claims
                            foreach (var code in adSchoolList)
                            {
                                if (await _unitOfWork.Schools.IsPartnerSchoolWithStudents(code) && linkedSchools.All(entry => entry != code))
                                    linkedSchools.Add(code);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
#pragma warning restore CA1416 // Validate platform compatibility

                    // Get list of users linked schools from DB
                    var dbUser = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(Input.Email);
                    if (dbUser != null)
                    {
                        var dbUserSchools = dbUser.Assignments.Where(assignment => !assignment.IsDeleted).Select(assignment => assignment.SchoolCode).ToList();

                        // Check each school against the DB to ensure it is an active partner school with students
                        // Add any matching entries to the user claims
                        foreach (var code in dbUserSchools)
                        {
                            if (await _unitOfWork.Schools.IsPartnerSchoolWithStudents(code) && linkedSchools.All(entry => entry != code))
                                linkedSchools.Add(code);
                        }
                    }

                    var claimList = new List<Claim>();
                    claimList.Add(new Claim("Schools", string.Join(",", linkedSchools)));

                    await _signInManager.SignInWithClaimsAsync(user, false, claimList);

                    return LocalRedirect(returnUrl);
                }
#endif
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
