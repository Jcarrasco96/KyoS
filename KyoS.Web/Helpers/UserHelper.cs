using KyoS.Common.Enums;
using KyoS.Web.Data.Entities;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<UserEntity> _signInManager;

        public UserHelper(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager, SignInManager<UserEntity> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> AddUserAsync(UserEntity user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleAsync(UserEntity user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task<UserEntity> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<UserEntity> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public IEnumerable<Users_in_Role_ViewModel> GetUsers()
        {
            var model = (from user in _userManager.Users
                         select new
                         {
                             UserId = user.Id,
                             Username = user.UserName,
                             FullName = user.FullName,
                             RoleNames = user.UserType.ToString(),
                             Clinic = user.Clinic
                         }).ToList().Select(p => new Users_in_Role_ViewModel()
                         {
                             UserId = p.UserId,
                             Username = p.Username,
                             Email = p.Username,
                             Fullname = p.FullName,
                             Role = p.RoleNames,
                             Clinic = p.Clinic
                         });
            return model;
        }

        public async Task<bool> IsUserInRoleAsync(UserEntity user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ChangePasswordAsync(UserEntity user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task<IdentityResult> DeleteUserAsync(UserEntity user)
        {
            if (await this.IsUserInRoleAsync(user, UserType.Admin.ToString()))
                await _userManager.RemoveFromRoleAsync(user, UserType.Admin.ToString());
            if (await this.IsUserInRoleAsync(user, UserType.Mannager.ToString()))
                await _userManager.RemoveFromRoleAsync(user, UserType.Mannager.ToString());
            if (await this.IsUserInRoleAsync(user, UserType.Facilitator.ToString()))
                await _userManager.RemoveFromRoleAsync(user, UserType.Facilitator.ToString());
            if (await this.IsUserInRoleAsync(user, UserType.Supervisor.ToString()))
                await _userManager.RemoveFromRoleAsync(user, UserType.Supervisor.ToString());
            return await _userManager.DeleteAsync(user);
        }
    }
}
