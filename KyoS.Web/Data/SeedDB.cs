using KyoS.Common.Enums;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using System;
using System.Threading.Tasks;

namespace KyoS.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IClassificationHelper _classificationHelper;

        public SeedDb(DataContext context, IUserHelper userHelper, IClassificationHelper classificationHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _classificationHelper = classificationHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckRolesAsync();

            await CheckUserAsync("84070314209", "Oscar", "Hernández Baute", "elpuya84@gmail.com", "230 939 2747", "Ave 54", UserType.Admin);
            await CheckUserAsync("81110214209", "Yoanky", "Madrazo", "ymadrazovalladares@gmail.com", "230 349 2747", "Fort Laurdale", UserType.Admin);
            await CheckUserAsync("91110214209", "Leyanis", "Albo", "leyanis.albo@gmail.com", "230 349 2747", "Ave 60", UserType.Admin);

            await CheckClassificationAsync("Depressed");
            await CheckClassificationAsync("Negativistic");
            await CheckClassificationAsync("Sadness");
            await CheckClassificationAsync("Anxious");
            await CheckClassificationAsync("SleepProblems");
            await CheckClassificationAsync("Insomnia");
            await CheckClassificationAsync("Socialization");
            await CheckClassificationAsync("Isolation");
            await CheckClassificationAsync("Community");
            await CheckClassificationAsync("Motivation");
            await CheckClassificationAsync("Irritable");
            await CheckClassificationAsync("SelfEsteem");
            await CheckClassificationAsync("Concentration");
            await CheckClassificationAsync("Memory");
            await CheckClassificationAsync("Independent");
            await CheckClassificationAsync("MedicalManagenent");
            await CheckClassificationAsync("SelfCare");
            await CheckClassificationAsync("PositiveSelfTalk");
            await CheckClassificationAsync("NegativeSelfTalk");
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
            await _userHelper.CheckRoleAsync(UserType.Manager.ToString());
            await _userHelper.CheckRoleAsync(UserType.Supervisor.ToString());
            await _userHelper.CheckRoleAsync(UserType.Facilitator.ToString());
            await _userHelper.CheckRoleAsync(UserType.CaseManager.ToString());
            await _userHelper.CheckRoleAsync(UserType.TCMSupervisor.ToString());
            await _userHelper.CheckRoleAsync(UserType.Documents_Assistant.ToString());
            await _userHelper.CheckRoleAsync(UserType.Frontdesk.ToString());
            await _userHelper.CheckRoleAsync(UserType.Biller.ToString());
        }

        private async Task CheckClassificationAsync(string classification)
        {
            await _classificationHelper.CheckClassificationAsync(classification);
        }
    }
}
