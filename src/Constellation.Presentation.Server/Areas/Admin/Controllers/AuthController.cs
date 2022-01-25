using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Areas.Admin.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthController(IUnitOfWork unitOfWork, IAuthService authService, 
            UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, 
            SignInManager<AppUser> signInManager)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;

            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { @area = "" });
            }

            return RedirectToPage("/Admin/Login");
        }

        //public async Task<IActionResult> Logout()
        //{
        //    return RedirectToPage("/Admin/Logout");
        //}

        public async Task TestPasswordReset()
        {
            var user = await _userManager.FindByEmailAsync("noemail@here.com");
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _userManager.ResetPasswordAsync(user, resetToken, "P@ssw0rd");

            return;
        }

        public async Task<IActionResult> TokenLogin(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return NotFound();
            }

            var appAccessToken = _unitOfWork.AppAccessTokens.WithDetails(accessToken);
            if (appAccessToken == null)
            {
                return NotFound();
            }

            if (!appAccessToken.IsCurrent)
            {
                return NotFound();
            }

            var user = await _userManager.FindByNameAsync(appAccessToken.MapToUser);
            if (user == null)
            {
                return NotFound();
            }

            await _signInManager.SignInAsync(user, true);

            if (string.IsNullOrWhiteSpace(appAccessToken.RedirectTo))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                return RedirectToLocal(appAccessToken.RedirectTo);
            }
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> AuthSettings()
        {
            var viewModel = await CreateViewModel<Auth_SettingsViewModel>();
            viewModel.Users = _userManager.Users.OrderBy(u => u.LastName).ToList();
            viewModel.Tokens = _unitOfWork.AppAccessTokens.All().OrderBy(t => t.Expiry).ToList();
            viewModel.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();

            return View(viewModel);
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> AuditAccounts()
        {
            await _authService.AuditSchoolContactUsers();
            await _unitOfWork.CompleteAsync();

            return RedirectToLocal("");
        }

        public async Task RepairStaffAccounts()
        {
            await _authService.RepairStaffUserAccounts();
        }

        [Authorize]
        public async Task<IActionResult> UpdateUser(string userId)
        {
            AppUser user;

            user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var viewModel = await CreateViewModel<Auth_UpdateUserViewModel>();
            viewModel.Id = user.Id.ToString();
            viewModel.FirstName = user.FirstName;
            viewModel.LastName = user.LastName;
            viewModel.Username = user.UserName;
            viewModel.Email = user.Email;

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(Auth_UpdateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);

                return View(viewModel);
            }

            var oldUser = await _userManager.FindByIdAsync(viewModel.Id);

            var newUserTemplate = new UserTemplateDto
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                Username = viewModel.Username
            };

            var results = await _authService.UpdateUser(oldUser.UserName, newUserTemplate);
            if (results != IdentityResult.Success)
            {
                AddErrors(results);
                await UpdateViewModel(viewModel);

                return View("UpdateUser", viewModel);
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> NewToken()
        {
            var viewModel = await CreateViewModel<Auth_CreateTokenViewModel>();
            return View("CreateToken", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = AuthRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewToken(Auth_CreateTokenViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                return View("CreateToken", viewModel);
            }

            var user = await _userManager.FindByNameAsync(viewModel.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "That user could not be found in the system!");
                await UpdateViewModel(viewModel);

                return View("CreateToken", viewModel);
            }

            if (!string.IsNullOrWhiteSpace(viewModel.RedirectTo) && !Url.IsLocalUrl(viewModel.RedirectTo))
            {
                ModelState.AddModelError("", "The Redirect Link does not exist!");
                await UpdateViewModel(viewModel);

                return View("CreateToken", viewModel);
            }

            if (viewModel.Expiry < DateTime.Today)
            {
                ModelState.AddModelError("", "The Expiry date cannot be in the past!");
                await UpdateViewModel(viewModel);

                return View("CreateToken", viewModel);
            }

            var token = new AppAccessToken
            {
                MapToUser = viewModel.Username,
                Expiry = viewModel.Expiry,
                RedirectTo = viewModel.RedirectTo
            };

            _unitOfWork.AppAccessTokens.Add(token);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("AuthSettings");
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> RemoveToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return RedirectToAction("AuthSettings");
            }

            var token = _unitOfWork.AppAccessTokens.WithFilter(u => u.AccessToken.ToString() == accessToken);

            if (token == null)
            {
                return RedirectToAction("AuthSettings");
            }

            _unitOfWork.Remove(token);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("AuthSettings");
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> NewUser()
        {
            var viewModel = await CreateViewModel<Auth_CreateUserViewModel>();

            return View("CreateUser", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = AuthRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewUser(Auth_CreateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                viewModel.Password = "";
                viewModel.ConfirmPassword = "";

                return View("CreateUser", viewModel);
            }

            var user = new UserTemplateDto
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Username = viewModel.Email,
                Email = viewModel.Email,
                Password = viewModel.Password
            };
            var result = await _authService.CreateUser(user);

            if (result.Succeeded)
            {
                return RedirectToAction("AuthSettings");
            }

            AddErrors(result);
            await UpdateViewModel(viewModel);
            viewModel.Password = "";
            viewModel.ConfirmPassword = "";

            return View("CreateUser", viewModel);
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("AuthSettings");
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> NewRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "You must specify a name for the new role!");
            }

            if (_roleManager.Roles.Any(r => r.Name == roleName))
            {
                ModelState.AddModelError("", "This role already exists!");
            }

            var role = new AppRole(roleName);
            await _roleManager.CreateAsync(role);

            var showViewModel = await CreateViewModel<Auth_SettingsViewModel>();
            showViewModel.Users = _userManager.Users.ToList();
            showViewModel.Roles = _roleManager.Roles.ToList();

            return View("AuthSettings", showViewModel);
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> RemoveRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }

            return RedirectToAction("AuthSettings");
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> ManageRoles()
        {
            var viewModel = await CreateViewModel<Auth_ManageRolesViewModel>();
            viewModel.Users = _userManager.Users.ToList();
            viewModel.Roles = _roleManager.Roles.ToList();

            return View(viewModel);
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            {
                // Error!
            }

            var user = await _userManager.FindByEmailAsync(userId);

            await _userManager.AddToRoleAsync(user, roleName);

            return RedirectToAction("ManageRoles");
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            {
                // Error!
            }

            var user = await _userManager.FindByEmailAsync(userId);

            await _userManager.RemoveFromRoleAsync(user, roleName);

            return RedirectToAction("ManageRoles");
        }

        [Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> CreateUsersFromStaff()
        {
            var staff = _unitOfWork.Staff.AllActive();

            foreach (var staffMember in staff)
            {
                var username = staffMember.PortalUsername + "@DETNSW";
                var result = await _userManager.FindByEmailAsync(staffMember.EmailAddress);
                if (result != null)
                {
                    continue;
                }

                var newUser = new UserTemplateDto
                {
                    FirstName = staffMember.FirstName,
                    LastName = staffMember.LastName,
                    Email = staffMember.EmailAddress,
                    Username = username,
                    Password = "".GeneratePassword(8, 2)
                };

                await _authService.CreateUser(newUser);
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}