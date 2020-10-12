using KyoS.Common.Enums;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using System.Threading.Tasks;

namespace KyoS.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckRolesAsync();
            await CheckUserAsync("84070314209", "Oscar", "Hernández Baute", "elpuya84@gmail.com", "230 939 2747", "Ave 54", UserType.Admin);
            await CheckUserAsync("81110214209", "Yoanky", "Madrazo", "ymadrazovalladares@gmail.com", "230 349 2747", "Fort Laurdale", UserType.Admin);
            await CheckUserAsync("91110214209", "Leyanis", "Albo", "leyanis.albo@gmail.com", "230 349 2747", "Ave 60", UserType.Operator);
        }

        private async Task<UserEntity> CheckUserAsync(string document, string firstName, string lastName, string email, string phone, string address, UserType userType)
        {
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
            {
                user = new UserEntity
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Address = address,
                    Document = document,
                    UserType = userType
                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());
            }
            return user;
        }

        private async Task CheckRolesAsync()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.Operator.ToString());
            await _userHelper.CheckRoleAsync(UserType.Facilitator.ToString());
        }       
    }
}
