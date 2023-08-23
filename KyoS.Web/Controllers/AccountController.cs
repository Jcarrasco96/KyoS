using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly DataContext _context;

        public AccountController(IUserHelper userHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
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
                if (!_userHelper.GetActiveByUserName(model.Username))
                {
                    ModelState.AddModelError(string.Empty, "User not active.");
                }
                else
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

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "User account locked out.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email or password incorrect.");
                    }
                }
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
                        UserType = (model.RoleId == 1) ? UserType.Documents_Assistant : (model.RoleId == 2) ? UserType.Facilitator : (model.RoleId == 3) ? UserType.Supervisor : (model.RoleId == 4) ? UserType.CaseManager : (model.RoleId == 5) ? UserType.TCMSupervisor : (model.RoleId == 6) ? UserType.Manager : (model.RoleId == 7) ? UserType.Admin : UserType.Frontdesk,
                        Active = model.Active,
                        Clinic = (model.IdClinic != 0) ? _context.Clinics.FirstOrDefault(c => c.Id == model.IdClinic) : null                    
                    };

                    try
                    {
                        await _userHelper.AddUserAsync(user, model.Password);
                        await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());
                        List<UserEntity> userList = _context.Users.Include(n => n.Clinic).ToList();

                        List<Users_in_Role_ViewModel> list_User = new List<Users_in_Role_ViewModel>();
                        foreach (var item in userList)
                        {
                            Users_in_Role_ViewModel temp = new Users_in_Role_ViewModel();
                            temp.Active = item.Active;
                            temp.Clinic = item.Clinic;
                            temp.Email = item.Email;
                            temp.Fullname = item.FullName;
                            temp.Username = item.UserName;
                            temp.UserId = item.Id;
                            temp.Role = item.UserType.ToString();
                            list_User.Add(temp);
                        }

                        ViewBag.StatusMessage = "User has been successfully created";

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewAccounts", list_User) });
                        //return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_Message", "User has been successfully created") });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_Message", "Error. User already exist") });
                }
            }

            model.Clinics = _combosHelper.GetComboClinics();
            model.Roles = _combosHelper.GetComboRoles();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
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
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "ChangePassword") });
            }
            UserEntity user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            IdentityResult result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_Message", "Your password was changed successfully") });
            }
            AddErrors(result);
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "ChangePassword") });
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == string.Empty)
            {
                return RedirectToAction("Home/Error404");
            }
            UserEntity user_to_eliminate = await _userHelper.GetUserByIdAsync(id);
            UserEntity user_in = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user_to_eliminate == user_in)
            {
                return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ErrorDeleting });
            }
            await _userHelper.DeleteUserAsync(user_to_eliminate);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == string.Empty)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user = await _context.Users

                                            .Include(u => u.Clinic)

                                            .FirstOrDefaultAsync(u => u.Id == id);                                           

            if (user == null)
            {
                return RedirectToAction("Home/Error404");
            }

            EditViewModel model = new EditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = (user.UserType == UserType.Documents_Assistant) ? 1 : (user.UserType == UserType.Facilitator) ? 2 : (user.UserType == UserType.Supervisor) ? 3
                                                                     : (user.UserType == UserType.CaseManager) ? 4 : (user.UserType == UserType.TCMSupervisor) ? 5
                                                                     : (user.UserType == UserType.Manager) ? 6 : (user.UserType == UserType.Admin) ? 7 : (user.UserType == UserType.Frontdesk) ? 8 : 0,
                Roles = _combosHelper.GetComboRoles(),
                IdClinic = (user.Clinic != null) ? user.Clinic.Id : 0,
                Clinics = _combosHelper.GetComboClinics(),
                Active = user.Active
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                    UserEntity user = await _userHelper.GetUserByIdAsync(model.Id);

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.PhoneNumber = string.Empty;
                    user.Address = string.Empty;
                    user.Document = string.Empty;
                    user.UserType = (model.RoleId == 1) ? UserType.Documents_Assistant : (model.RoleId == 2) ? UserType.Facilitator : (model.RoleId == 3) ? UserType.Supervisor : (model.RoleId == 4) ? UserType.CaseManager : (model.RoleId == 5) ? UserType.TCMSupervisor : (model.RoleId == 6) ? UserType.Manager : (model.RoleId == 7) ? UserType.Admin : UserType.Frontdesk;
                    user.Active = model.Active;
                    user.Clinic = (model.IdClinic != 0) ? _context.Clinics.FirstOrDefault(c => c.Id == model.IdClinic) : null;
                    
                    try
                    {                                          
                        await _userHelper.EditUserAsync(user);

                        var roles = await _userHelper.GetRolesAsync(user);
                        await _userHelper.RemoveFromRolesAsync(user, roles);
                        await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());
                        
                        List<UserEntity> userList = _context.Users.Include(n => n.Clinic).ToList();
                        
                        List<Users_in_Role_ViewModel> list_User = new List<Users_in_Role_ViewModel>();
                        foreach (var item in userList)
                        {
                            Users_in_Role_ViewModel temp = new Users_in_Role_ViewModel();
                            temp.Active = item.Active;
                            temp.Clinic = item.Clinic;
                            temp.Email = item.Email;
                            temp.Fullname = item.FullName;
                            temp.Username = item.UserName;
                            temp.UserId = item.Id;
                            temp.Role = item.UserType.ToString();
                            list_User.Add(temp);
                        }

                        ViewBag.StatusMessage = "The user has been successfully modified.";

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewAccounts", list_User) });

                    //return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_Message", "The user has been successfully modified.") });
                }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }               
            }

            model.Clinics = _combosHelper.GetComboClinics();
            model.Roles = _combosHelper.GetComboRoles();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", model) });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ResetPassword()
        {
            SetPasswordViewModel model = new SetPasswordViewModel
            {
                Users = _combosHelper.GetComboUserNames()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {            
                UserEntity user = await _userHelper.GetUserByIdAsync(model.IdUser.ToString());
                if (user != null)
                {
                    _userHelper.HardResetPassword(user.Email, model.NewPassword);
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_Message", "User password was updated successfully") });
                }
            }

            model.Users = _combosHelper.GetComboUserNames();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "ResetPassword", model) });
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}