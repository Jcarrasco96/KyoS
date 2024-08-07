﻿using KyoS.Web.Data.Entities;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IUserHelper
    {
        Task<UserEntity> GetUserByEmailAsync(string email);

        Task<UserEntity> GetUserByIdAsync(string id);

        Task<IdentityResult> AddUserAsync(UserEntity user, string password);

        Task<IdentityResult> EditUserAsync(UserEntity user);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(UserEntity user, string roleName);

        Task RemoveFromRolesAsync(UserEntity user, IEnumerable<string> rolesName);

        Task<IList<string>> GetRolesAsync(UserEntity user);

        Task<bool> IsUserInRoleAsync(UserEntity user, string roleName);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

        IEnumerable<Users_in_Role_ViewModel> GetUsers();

        string GetUserNameById(string id);

        string GetIdByUserName(string userName);

        Task<IdentityResult> ChangePasswordAsync(UserEntity user, string oldPassword, string newPassword);

        Task<IdentityResult> DeleteUserAsync(UserEntity user);

        bool GetActiveByUserName(string userName);

        void HardResetPassword(string email, string newPassword);        
    }
}
