using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly DataContext _context;
        
        public MessagesController(DataContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Facilitator"))
            {
                ViewBag.CountsMessagesNotes = _context.Messages
                                                      .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                && m.Workday_Client != null))
                                                      .ToString();
                
                ViewBag.CountsMessagesFars = _context.Messages
                                                     .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                               && m.FarsForm != null))
                                                     .ToString();

                ViewBag.CountsMessagesMTPReview = _context.Messages
                                                          .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                    && m.MTPReview != null))
                                                          .ToString();
                
                ViewBag.CountsMessagesAddendum = _context.Messages
                                                         .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                   && m.Addendum != null))
                                                         .ToString();

                ViewBag.CountsMessagesDischarge = _context.Messages
                                                          .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                    && m.Discharge != null))
                                                          .ToString();
            }
            return View();
        }
    }
}