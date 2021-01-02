using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly DataContext _context;

        public AccountController(IUserHelper userHelper, ICombosHelper combosHelper, DataContext context)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(Request.Query["ReturnUrl"].First());
                    }

                    return RedirectToAction("Index", "Desktop");
                }

                ModelState.AddModelError(string.Empty, "Email or password incorrect.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                  message == ManageMessageId.ChangePasswordSuccess ? "Your password was changed successfully"
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occured"
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.ErrorDeleting ? "An error has occured, unabled to delete logged-in current user"
                : "";

            return View(_userHelper.GetUsers());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
            }
            else
            {
                if (id == 2)
                {
                    ViewBag.Creado = "E";
                }
                else
                {
                    ViewBag.Creado = "N";
                }
            }

            RegisterViewModel model = new RegisterViewModel
            {
                Roles = _combosHelper.GetComboRoles(),
                Clinics = _combosHelper.GetComboClinics()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserEntity user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    user = new UserEntity
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        UserName = model.Email,
                        PhoneNumber = string.Empty,
                        Address = string.Empty,
                        Document = string.Empty,
                        UserType = (model.RoleId == 1) ? UserType.Facilitator : (model.RoleId == 2) ? UserType.Supervisor : (model.RoleId == 3) ? UserType.Mannager : UserType.Admin,
                        Clinic = (model.IdClinic != 0) ? _context.Clinics.FirstOrDefault(c => c.Id == model.IdClinic) : null
                    };

                    try
                    {
                        await _userHelper.AddUserAsync(user, model.Password);
                        await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    return RedirectToAction("Create", new { id = 2 });
                }
            }
           
            model.Clinics = _combosHelper.GetComboClinics();
            model.Roles = _combosHelper.GetComboRoles();
            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            UserEntity user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == string.Empty)
            {
                return NotFound();
            }
            var user_to_eliminate = await _userHelper.GetUserByIdAsync(id);
            var user_in = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user_to_eliminate == user_in)
            {
                return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ErrorDeleting });
            }
            await _userHelper.DeleteUserAsync(user_to_eliminate);
            return RedirectToAction(nameof(Index));
        }
    }
}