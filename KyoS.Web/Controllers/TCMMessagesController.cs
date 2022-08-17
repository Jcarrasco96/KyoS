using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class TCMMessagesController : Controller
    {
        private readonly DataContext _context;

        public TCMMessagesController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager"))
            {
                ViewBag.CountsMessagesNotes = _context.TCMMessages
                                                      .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead))
                                                      .ToString();
            }
            return View();
        }
    }
}
