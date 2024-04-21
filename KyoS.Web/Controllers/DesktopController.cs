using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class DesktopController : Controller
    {
        private readonly DataContext _context;
        public IConfiguration Configuration { get; }
        public DesktopController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Facilitator"))
            {
                List<Workday_Client> not_started_list;
                List<Workday_Client> notes_review_list;

                ViewBag.EnabledPSR = "1";
                if (_context.Workdays_Clients.Count(wc => (wc.Workday.Service == ServiceType.PSR && wc.Facilitator.LinkedUser == User.Identity.Name)) > 0)
                {
                    ViewBag.ApprovedNotes = await _context.Workdays_Clients                                                   
                                                    .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                            && (wc.Note.Status == NoteStatus.Approved || wc.NoteP.Status == NoteStatus.Approved)
                                                             && wc.Workday.Service == ServiceType.PSR));

                    ViewBag.PendingNotes = await _context.Workdays_Clients                                                      
                                                   .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                           && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                            && wc.Workday.Service == ServiceType.PSR));

                    ViewBag.InProgressNotes = await _context.Workdays_Clients                                                      
                                                      .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && (wc.Note.Status == NoteStatus.Edition || wc.NoteP.Status == NoteStatus.Edition)
                                                               && wc.Workday.Service == ServiceType.PSR));

                   not_started_list = await _context.Workdays_Clients
                                                    .Include(wc => wc.Note)
                                                    .Include(wc => wc.NoteP)
                                                    .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                               && wc.Present == true
                                                               && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                    not_started_list = not_started_list.Where(wc => (wc.Note == null && wc.NoteP == null)).ToList();
                    ViewBag.NotStartedNotes = not_started_list.Count.ToString();

                    notes_review_list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Messages)
                                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                                && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                    notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                    ViewBag.NotesWithReview = notes_review_list.Count.ToString();

                    ViewBag.NotPresentNotes = await _context.Workdays_Clients                                                      
                                                            .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                            && wc.Present == false
                                                                            && wc.Workday.Service == ServiceType.PSR));
                }
                else
                    ViewBag.EnabledPSR = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                ViewBag.EnabledInd = "1";
                if (_context.Workdays_Clients.Count(wc => wc.Workday.Service == ServiceType.Individual && wc.Facilitator.LinkedUser == User.Identity.Name) > 0)
                {
                    ViewBag.ApprovedIndNotes = await _context.Workdays_Clients                                                   
                                                             .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                             && wc.IndividualNote.Status == NoteStatus.Approved
                                                                             && wc.Workday.Service == ServiceType.Individual));

                    ViewBag.PendingIndNotes = await _context.Workdays_Clients                                                      
                                                            .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                            && wc.IndividualNote.Status == NoteStatus.Pending
                                                                            && wc.Workday.Service == ServiceType.Individual));

                    ViewBag.InProgressIndNotes = await _context.Workdays_Clients                                                         
                                                               .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                               && wc.IndividualNote.Status == NoteStatus.Edition
                                                                               && wc.Workday.Service == ServiceType.Individual));

                    not_started_list = await _context.Workdays_Clients
                                                     .Include(wc => wc.IndividualNote)
                                                     .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                          && wc.Present == true
                                                                          && wc.Workday.Service == ServiceType.Individual)).ToListAsync();
                    not_started_list = not_started_list.Where(wc => wc.IndividualNote == null).ToList();
                    ViewBag.NotStartedIndNotes = not_started_list.Count.ToString();

                    notes_review_list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Messages)
                                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                                && wc.IndividualNote.Status == NoteStatus.Pending
                                                                                && wc.Workday.Service == ServiceType.Individual)).ToListAsync();
                    notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                    ViewBag.IndNotesWithReview = notes_review_list.Count.ToString();

                    ViewBag.NotPresentIndNotes = _context.Workdays_Clients                                                         
                                                         .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Present == false
                                                                  && wc.Workday.Service == ServiceType.Individual)).ToString();

                }
                else
                    ViewBag.EnabledInd = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                ViewBag.EnabledGroup = "1";
                if (_context.Workdays_Clients.Count(wc => wc.Workday.Service == ServiceType.Group && wc.Facilitator.LinkedUser == User.Identity.Name) > 0)
                {
                    ViewBag.ApprovedGroupNotes = await _context.Workdays_Clients
                                                               .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                    && (wc.GroupNote.Status == NoteStatus.Approved
                                                                    || wc.GroupNote2.Status == NoteStatus.Approved)
                                                                    && wc.Workday.Service == ServiceType.Group));

                    ViewBag.PendingGroupNotes = await _context.Workdays_Clients
                                                              .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                   && (wc.GroupNote.Status == NoteStatus.Pending
                                                                    || wc.GroupNote2.Status == NoteStatus.Pending)
                                                                   && wc.Workday.Service == ServiceType.Group));

                    ViewBag.InProgressGroupNotes = await _context.Workdays_Clients
                                                                 .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && (wc.GroupNote.Status == NoteStatus.Edition
                                                                    || wc.GroupNote2.Status == NoteStatus.Edition)
                                                                      && wc.Workday.Service == ServiceType.Group));

                    not_started_list = await _context.Workdays_Clients
                                                     .Include(wc => wc.GroupNote)
                                                     .Include(wc => wc.GroupNote2)
                                                     .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                && wc.Present == true
                                                                && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                    not_started_list = not_started_list.Where(wc => wc.GroupNote == null && wc.GroupNote2 == null).ToList();
                    ViewBag.NotStartedGroupNotes = not_started_list.Count.ToString();

                    notes_review_list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Messages)
                                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                 && (wc.GroupNote.Status == NoteStatus.Pending
                                                                 || wc.GroupNote2.Status == NoteStatus.Pending)
                                                                 && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                    notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                    ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();

                    ViewBag.NotPresentGroupNotes = await _context.Workdays_Clients
                                                                 .CountAsync(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                                 && wc.Present == false
                                                                                 && wc.Workday.Service == ServiceType.Group));
                }
                else
                    ViewBag.EnabledGroup = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                ViewBag.ExpiredMTPsFacilitator = await _context.MTPs
                                                               .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                              && m.Client.Status == StatusType.Open
                                                                              && m.Active == true
                                                                              && ((m.Client.IdFacilitatorPSR == facilitator.Id && m.Client.Service == ServiceType.PSR)
                                                                                    && m.Goals.Where(n => n.Objetives.Where(o => o.DateResolved.Date > DateTime.Today.Date
                                                                                    && o.Goal.Service == m.Client.Service
                                                                                    && o.Compliment == false).Count() > 0
                                                                                    ).Count() == 0
                                                                              || (m.Client.IdFacilitatorGroup == facilitator.Id && m.Client.Service == ServiceType.Group)
                                                                               && m.Goals.Where(n => n.Objetives.Where(o => o.DateResolved.Date > DateTime.Today.Date
                                                                               && o.Goal.Service == m.Client.Service
                                                                               && o.Compliment == false).Count() > 0
                                                                          ).Count() == 0)));

                int clientListPSR = await _context.Clients
                                                  .CountAsync(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                 && m.Status == StatusType.Close
                                                                 && m.IdFacilitatorPSR == facilitator.Id
                                                                 && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.PSR).Count() > 0
                                                                 && m.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() == 0));

                int clientListIND = await _context.Clients
                                                  .CountAsync(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                 && m.Status == StatusType.Close
                                                                 && m.IndividualTherapyFacilitator.Id == facilitator.Id
                                                                 && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Individual).Count() > 0
                                                                 && m.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() == 0));

                int clientListGroup = await _context.Clients
                                                    .CountAsync(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                && m.Status == StatusType.Close
                                                                && m.IdFacilitatorGroup == facilitator.Id
                                                                && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Group).Count() > 0
                                                                && m.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() == 0));                                                                   

                ViewBag.ClientDischarge = (clientListPSR + clientListIND + +clientListGroup).ToString();

                ViewBag.DischargeEdition = await _context.Discharge
                                                         .CountAsync(n => (n.Status == DischargeStatus.Edition
                                                                        && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && n.CreatedBy == user_logged.UserName));
                
                ViewBag.DischargePending = await _context.Discharge
                                                         .CountAsync(n => (n.Status == DischargeStatus.Pending
                                                                        && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && n.CreatedBy == user_logged.UserName));

                ViewBag.MTPReviewEdition = await _context.MTPReviews                                                               
                                                         .CountAsync(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && m.Status == AdendumStatus.Edition
                                                                        && m.CreatedBy == user_logged.UserName
                                                                        && m.SignTherapy == false)
                                                                      ||
                                                                          (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && m.Status == AdendumStatus.Edition
                                                                        && m.IndFacilitator.LinkedUser == user_logged.UserName
                                                                        && m.SignIndTherapy == false));                
                ViewBag.MTPReviewPending = await _context.MTPReviews
                                                         .CountAsync(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && (m.Status == AdendumStatus.Pending || (m.SignTherapy == true && m.Status == AdendumStatus.Edition))
                                                                        && m.CreatedBy == user_logged.UserName)
                                                                        ||
                                                                          (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && (m.Status == AdendumStatus.Pending)
                                                                        && m.IndFacilitator.LinkedUser == user_logged.UserName));

                ViewBag.ClientWithoutFARS = await _context.Clients                                                    
                                                          .CountAsync(n => n.Clinic.Id == user_logged.Clinic.Id
                                                             && (((n.IdFacilitatorPSR == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id) && n.MTPs.FirstOrDefault(m => m.Active == true).MtpReviewList.Where(r => r.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.MtpReview).Count() == 0)
                                                                || (n.DischargeList.Where(d => d.TypeService == ServiceType.PSR && d.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_PSR).Count() == 0)
                                                                || (n.DischargeList.Where(d => d.TypeService == ServiceType.Group && d.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Group).Count() == 0)
                                                                || (n.DischargeList.Where(d => d.TypeService == ServiceType.Individual && d.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Ind).Count() == 0)
                                                                || (n.MTPs.Where(m => m.AdendumList.Where(a => a.CreatedBy == user_logged.UserName).Count() > n.FarsFormList.Where(f => f.Type == FARSType.Addendums && f.CreatedBy == user_logged.UserName).Count()).Count() > 0))
                                                                && n.OnlyTCM == false);

                ViewBag.AddemdunEdition = await _context.Adendums
                                                        .CountAsync(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                       && m.Status == AdendumStatus.Edition
                                                                       && m.CreatedBy == user_logged.UserName));
                ViewBag.AddemdunPending = await _context.Adendums
                                                        .CountAsync(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                       && m.Status == AdendumStatus.Pending
                                                                       && m.CreatedBy == user_logged.UserName));
                ViewBag.ReferralPending = await _context.ReferralForms
                                                        .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                       && m.Facilitator.Id == facilitator.Id
                                                                       && m.FacilitatorSign == false));
                ViewBag.SafetyPlan = await _context.Clients
                                                   .CountAsync(c => (c.Clinic.Id == user_logged.Clinic.Id && c.OnlyTCM == false
                                                                  && c.SafetyPlanList.Count() == 0
                                                                  && c.IndividualTherapyFacilitator.Id == facilitator.Id));
                ViewBag.PendingMeetingSign = await _context.MeetingNotes
                                                           .CountAsync(m => (m.Supervisor.Clinic.Id == user_logged.Clinic.Id
                                                                          && (m.FacilitatorList.Where(n => n.Facilitator.LinkedUser == user_logged.UserName
                                                                                                        && n.Sign == false).Count() > 0)
                                                                          && m.Status != NoteStatus.Approved));
            }
            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                ViewBag.PendingActivities = await _context.Activities
                                                    .CountAsync(a => (a.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                   && a.Status == ActivityStatus.Pending));

                ViewBag.PendingNotes = await _context.Workdays_Clients
                                                     .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                             && wc.Workday.Service == ServiceType.PSR));

                ViewBag.PendingIndNotes = await _context.Workdays_Clients
                                                        .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                 && wc.IndividualNote.Status == NoteStatus.Pending
                                                                 && wc.Workday.Service == ServiceType.Individual));

                ViewBag.PendingGroupNotes = await _context.Workdays_Clients
                                                          .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                         && (wc.GroupNote.Status == NoteStatus.Pending
                                                                         || wc.GroupNote2.Status == NoteStatus.Pending)
                                                                         && wc.Workday.Service == ServiceType.Group));

                ViewBag.ClientWithoutBIO = await _context.Clients
                                                         .CountAsync(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                         && wc.Bio == null && wc.Brief == null
                                                                         && wc.OnlyTCM == false));

                ViewBag.PendingInitialFars = await _context.Clients
                                                           .CountAsync(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                           && (wc.FarsFormList.Where(n => n.Type == FARSType.Initial).Count() == 0)
                                                                           && (wc.Bio.CreatedBy == User.Identity.Name
                                                                            || wc.Bio == null)
                                                                           && wc.OnlyTCM == false));

                ViewBag.PendingFars = await _context.FarsForm
                                              .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                             && m.Status == FarsStatus.Pending));

                ViewBag.PendingAddendum = await _context.Adendums
                                                  .CountAsync(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                 && m.Status == AdendumStatus.Pending));

                ViewBag.PendingMTPReview = await _context.MTPReviews
                                                         .CountAsync(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && m.Status == AdendumStatus.Pending));

                ViewBag.PendingDischarge = await _context.Discharge
                                                         .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && m.Status == DischargeStatus.Pending));

                List<ClientEntity> client = await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                    && c.OnlyTCM == false)).ToListAsync();
                ViewBag.MTPMissing = client.Where(wc => wc.MTPs.Count == 0).Count().ToString();
                
                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                       .Include(wc => wc.Messages)
                                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                               && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                                               && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.IndividualNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Individual)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.IndNotesWithReview = notes_review_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && (wc.GroupNote.Status == NoteStatus.Pending
                                                             || wc.GroupNote2.Status == NoteStatus.Pending)
                                                             && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();

                ViewBag.MedicalHistoryMissing = await _context.Clients
                                                              .CountAsync(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                              && wc.IntakeMedicalHistory == null
                                                                              && (wc.Bio.CreatedBy == User.Identity.Name
                                                                               || wc.Bio == null)
                                                                              && wc.OnlyTCM == false));

                ViewBag.PendingMtp = await _context.MTPs
                                                   .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                  && m.Status == MTPStatus.Pending));

                ViewBag.PendingBio = await _context.Bio
                                                   .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                  && m.Status == BioStatus.Pending));

                ViewBag.PendingBrief = await _context.Brief
                                                     .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == BioStatus.Pending));

                ViewBag.ReferralPending = await _context.ReferralForms
                                                        .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                      && m.Supervisor.LinkedUser == user_logged.UserName
                                                                      && m.SupervisorSign == false));

                ViewBag.PendingSupervisorNotes = await _context.MeetingNotes
                                                               .CountAsync(m => (m.Supervisor.Clinic.Id == user_logged.Clinic.Id
                                                                              && m.Supervisor.LinkedUser == user_logged.UserName
                                                                              && m.Status != NoteStatus.Approved));

                ViewBag.ApprovedNotes = await _context.Workdays_Clients
                                                      .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                                        (wc.Note.Status == NoteStatus.Approved
                                                                      || wc.IndividualNote.Status == NoteStatus.Approved
                                                                      || wc.GroupNote.Status == NoteStatus.Approved
                                                                      || wc.GroupNote2.Status == NoteStatus.Approved
                                                                      || wc.NoteP.Status == NoteStatus.Approved)));

                ViewBag.PendingSafetyPlans = await _context.SafetyPlan
                                                           .CountAsync(m => (m.Status == SafetyPlanStatus.Pending));
            }
            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = await _context.Users

                                                       .Include(u => u.Clinic)
                                                            .ThenInclude(c => c.Setting)
                                                       .AsSplitQuery()

                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                
                //CMH Dashboard
                if (user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    Task<int> PendingNotes = MHPendingNotes(user_logged.Clinic.Id);
                    Task<int> InProgressNotes = MHInProgressNotes(user_logged.Clinic.Id);
                    Task<int> NotStartedNotes = MHNotStartedNotes(user_logged.Clinic.Id);
                    Task<int> MTPMissing = MHMTPMissing(user_logged.Clinic.Id);
                    Task<int> NotesWithReview = MHNotesWithReview(user_logged.Clinic.Id);                    
                    Task<int> ApprovedNotes = MHApprovedNotes(user_logged.Clinic.Id);
                    Task<int> NotPresentNotes = MHNotPresentNotes(user_logged.Clinic.Id);
                    Task<int> ExpiredMTPs = MHExpiredMTPs(user_logged.Clinic.Id);
                    Task<int> PendingBIO = MHPendingBIO(user_logged.Clinic.Id);
                    Task<int> PendingInitialFars = MHPendingInitialFars(user_logged.Clinic.Id);
                    Task<int> MedicalHistoryMissing = MHMedicalHistoryMissing(user_logged.Clinic.Id);
                    Task<int> IntakeMissing = MHIntakeMissing(user_logged.Clinic.Id);
                    Task<int> FarsMissing = MHFarsMissing(user_logged.Clinic.Id);
                    Task<int> ClientAuthorization = MHClientAuthorization(user_logged.Clinic.Id);
                    Task<int> ClientBirthday = MHClientBirthday(user_logged.Clinic.Id);
                    Task<int> ClientEligibility = MHClientEligibility(user_logged.Clinic.Id);
                    Task<int> SafetyPlan = MHSafetyPlan(user_logged.Clinic.Id);
                    
                    await Task.WhenAll(PendingNotes, InProgressNotes, NotStartedNotes, MTPMissing, NotesWithReview,
                                        ApprovedNotes, NotPresentNotes, ExpiredMTPs, PendingBIO, PendingInitialFars, MedicalHistoryMissing,
                                        IntakeMissing, FarsMissing, ClientAuthorization, ClientBirthday, ClientEligibility,
                                        SafetyPlan);

                    ViewBag.PendingNotes = await PendingNotes;
                    ViewBag.InProgressNotes = await InProgressNotes;
                    ViewBag.NotStartedNotes = await NotStartedNotes;
                    ViewBag.MTPMissing = await MTPMissing;
                    ViewBag.NotesWithReview = await NotesWithReview;                    
                    ViewBag.ApprovedNotes = await ApprovedNotes;
                    ViewBag.NotPresentNotes = await NotPresentNotes;
                    ViewBag.ExpiredMTPs = await ExpiredMTPs;
                    ViewBag.PendingBIO = await PendingBIO;
                    ViewBag.PendingInitialFars = await PendingInitialFars;
                    ViewBag.MedicalHistoryMissing = await MedicalHistoryMissing;
                    ViewBag.IntakeMissing = await IntakeMissing;
                    ViewBag.FarsMissing = await FarsMissing;
                    ViewBag.ClientAuthorization = await ClientAuthorization;
                    ViewBag.ClientBirthday = await ClientBirthday;
                    ViewBag.ClientEligibility = await ClientEligibility;
                    ViewBag.SafetyPlan = await SafetyPlan;                    
                }          

                //TCM Dashboard
                if (user_logged.Clinic.Setting.TCMClinic)
                {
                    Task<int> NotStartedCases = TCMNotStartedCases(user_logged.Clinic.Id);
                    Task<int> OpenBinder = TCMOpenBinder(user_logged.Clinic.Id);
                    Task<int> ServicePlanPending = TCMServicePlanPending(user_logged.Clinic.Id);
                    Task<int> AdendumPending = TCMAdendumPending(user_logged.Clinic.Id);
                    Task<int> ServicePlanReviewPending = TCMServicePlanReviewPending(user_logged.Clinic.Id);
                    Task<int> DischargePending = TCMDischargePending(user_logged.Clinic.Id);
                    Task<int> CaseManager = TCMCaseManager(user_logged.Clinic.Id);
                    Task<int> FarsPending = TCMFarsPending(user_logged.Clinic.Id);
                    Task<int> AssesmentPending = TCMAssesmentPending(user_logged.Clinic.Id);
                    Task<int> NotesEdition = TCMNotesEdition(user_logged.Clinic.Id);
                    Task<int> NotesPending = TCMNotesPending(user_logged.Clinic.Id);
                    Task<int> NotesApproved = TCMNotesApproved(user_logged.Clinic.Id);
                    Task<int> NotesWithReview = TCMNotesWithReview(user_logged.Clinic.Id);
                    Task<int> Supervisor = TCMSupervisor(user_logged.Clinic.Id);
                    Task<int> Billing = TCMBilling(user_logged.Clinic.Id);
                    Task<int> ClientAuthorizationTCM = TCMClientAuthorization(user_logged.Clinic.Id);


                    await Task.WhenAll(NotStartedCases, OpenBinder, ServicePlanPending, AdendumPending, ServicePlanReviewPending,
                                        DischargePending, CaseManager, FarsPending, AssesmentPending, NotesEdition,
                                        NotesPending, NotesApproved, NotesWithReview, Supervisor, Billing, ClientAuthorizationTCM);

                    ViewBag.NotStartedCases = await NotStartedCases;
                    ViewBag.OpenBinder = await OpenBinder;
                    ViewBag.ServicePlanPending = await ServicePlanPending;
                    ViewBag.AdendumPending = await AdendumPending;
                    ViewBag.ServicePlanReviewPending = await ServicePlanReviewPending;
                    ViewBag.DischargePending = await DischargePending;
                    ViewBag.TCMCaseManager = await CaseManager;
                    ViewBag.TCMFarsPending = await FarsPending;
                    ViewBag.AssesmentPending = await AssesmentPending;                   
                    ViewBag.TCMNotesEdition = await NotesEdition;
                    ViewBag.TCMNotesPending = await NotesPending;
                    ViewBag.TCMNotesApproved = await NotesApproved;
                    ViewBag.TCMNotesWithReview = await NotesWithReview;
                    ViewBag.TCMSupervisor = await Supervisor;
                    ViewBag.Billing = await Billing;
                    ViewBag.TCMClientAuthorization = await ClientAuthorizationTCM;
                }
            }
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index), "Incidents");
            }
            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

                ViewBag.NotStartedCases = await _context.TCMClient
                                                                
                                                        .CountAsync(g => (g.Casemanager.Id == caseManager.Id
                                                                && g.Status == StatusType.Open
                                                                && (g.TcmServicePlan == null
                                                                    || g.TCMAssessment == null
                                                                    || (g.TcmIntakeAppendixJ == null
                                                                     && g.TcmIntakeAppendixI == null))));        

                ViewBag.OpenBinder = await _context.TCMClient
                                                   .CountAsync(g => (g.Casemanager.Id == caseManager.Id
                                                                  && g.Status == StatusType.Open
                                                                  && g.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.CloseCases = await _context.TCMClient
                                                   .CountAsync(g => (g.Casemanager.Id == caseManager.Id
                                                                  && g.Status == StatusType.Close
                                                                  && g.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.Billing = await _context.TCMClient
                                                .CountAsync(g => (g.Casemanager.Id == caseManager.Id
                                                               && g.Status == StatusType.Open
                                                               && g.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.AssesmentEdition = await _context.TCMAssessment
                                                         .CountAsync(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                        && g.Approved == 0));

                ViewBag.AssesmentPending = await _context.TCMAssessment
                                                         .CountAsync(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                        && g.Approved == 1));

                ViewBag.ServicePlanEdition = await _context.TCMServicePlans                                                     
                                                           .CountAsync(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                          && g.Approved == 0));

                ViewBag.ServicePlanPending = await _context.TCMServicePlans
                                                           .CountAsync(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                          && g.Approved == 1));

                ViewBag.AdendumEdition = await _context.TCMAdendums                                                       
                                                       .CountAsync(g => (g.Approved == 0
                                                                      && g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id));

                ViewBag.AdendumPending = await _context.TCMAdendums
                                                       .CountAsync(g => (g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                                      && g.Approved == 1));

                ViewBag.ServicePlanReviewEdition = await _context.TCMServicePlanReviews                                                           
                                                                 .CountAsync(g => (g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                                          && g.Approved == 0));

                ViewBag.ServicePlanReviewPending = await _context.TCMServicePlanReviews
                                                                 .CountAsync(g => (g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                                          && g.Approved == 1));

                ViewBag.DischargeEdition = await _context.TCMDischarge
                                                         .CountAsync(g => (g.Approved == 0
                                                                        && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id));

                ViewBag.DischargePending = await _context.TCMDischarge
                                                         .CountAsync(g => (g.Approved == 1
                                                                        && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id));

                ViewBag.NotesEdition = await _context.TCMNote
                                                     .CountAsync(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                                    && g.Status == NoteStatus.Edition
                                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.NotesPending = await _context.TCMNote
                                                     .CountAsync(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                                    && g.Status == NoteStatus.Pending
                                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.NotesApproved = await _context.TCMNote
                                                      .CountAsync(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                                     && g.Status == NoteStatus.Approved
                                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.AppendiceJPending = await _context.TCMIntakeAppendixJ
                                                          .CountAsync(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                         && g.Approved == 1
                                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.FarsPending = await _context.TCMFarsForm
                                                    .CountAsync(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                                   && g.Status == FarsStatus.Pending
                                                                   && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.AllDocuments = await _context.TCMClient
                                                     .CountAsync(g => (g.Casemanager.Id == caseManager.Id
                                                                    && g.Client.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.ActivityPending = await _context.TCMServiceActivity
                                                        .CountAsync(g => (g.Approved < 2
                                                                       && g.CreatedBy == user_logged.UserName
                                                                       && g.TcmService.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.TCMNoteReview = _context.TCMNote
                                                .Include(wc => wc.TCMClient)
                                                .ThenInclude(wc => wc.Client)
                                                .Include(wc => wc.TCMClient.Casemanager)
                                                .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                .Where(wc => (wc.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                       && wc.Status == NoteStatus.Pending
                                                       && wc.TCMMessages.Count() > 0)).Count()
                                                .ToString();
                ViewBag.AppendiceIPending = await _context.TCMIntakeAppendixI
                                                          .CountAsync(g => (g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                         && g.Approved == 1
                                                                         && g.TcmClient.Casemanager.LinkedUser == user_logged.UserName));
                ViewBag.ClientWithoutDischarge = await _context.TCMServicePlans
                                                               .CountAsync(g => (g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                              && g.TcmClient.Status == StatusType.Close
                                                                              && g.TCMDischarge == null
                                                                              && g.TcmClient.Casemanager.LinkedUser == user_logged.UserName));
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                List<ClientEntity> client = await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Include(c => c.DocumentsAssistant)
                                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                    && c.OnlyTCM == false)).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0 
                                        && ((wc.DocumentsAssistant != null && wc.DocumentsAssistant.LinkedUser == user_logged.UserName
                                            ||wc.DocumentsAssistant == null)))
                               .ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                ViewBag.PendingBIO = await _context.Clients
                                                   .CountAsync(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                  && (wc.Bio == null && wc.Brief == null)
                                                                  && wc.OnlyTCM == false)
                                                                  && ((wc.DocumentsAssistant != null && wc.DocumentsAssistant.LinkedUser == user_logged.UserName)
                                                                     ||wc.DocumentsAssistant == null));

                ViewBag.PendingInitialFars = await _context.Clients
                                                           .CountAsync(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                           && (wc.FarsFormList.Where(n => n.Type == FARSType.Initial).Count() == 0)
                                                                           && (wc.Bio.CreatedBy == User.Identity.Name
                                                                            || wc.Bio == null)
                                                                           && wc.OnlyTCM == false)
                                                                           && ((wc.DocumentsAssistant != null && wc.DocumentsAssistant.LinkedUser == user_logged.UserName)
                                                                             || wc.DocumentsAssistant == null));

                ViewBag.MedicalHistoryMissing = await _context.Clients
                                                              .CountAsync(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                              && wc.IntakeMedicalHistory == null
                                                                              && (wc.Bio.CreatedBy == User.Identity.Name
                                                                               || wc.Bio == null)
                                                                              && wc.OnlyTCM == false)
                                                                              && ((wc.DocumentsAssistant != null && wc.DocumentsAssistant.LinkedUser == user_logged.UserName)
                                                                                || wc.DocumentsAssistant == null));

                List<MTPEntity> MtpPending = await _context.MTPs                                                           
                                                           .Where(n => (n.Status == MTPStatus.Pending
                                                                     && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                     && n.CreatedBy == user_logged.UserName))                                                           
                                                           .ToListAsync();
                ViewBag.MtpPending = MtpPending.Count().ToString();

                List<BioEntity> BioPending = await _context.Bio                                                           
                                                           .Where(n => (n.Status == BioStatus.Pending
                                                                     && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                     && n.CreatedBy == user_logged.UserName))                                                           
                                                           .ToListAsync();
                ViewBag.BioPending = BioPending.Count().ToString();

                List<BriefEntity> BriefPending = await _context.Brief                                                               
                                                               .Where(n => (n.Status == BioStatus.Pending
                                                                    && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && n.CreatedBy == user_logged.UserName))                                                               
                                                               .ToListAsync();
                ViewBag.BriefPending = BriefPending.Count().ToString();

                List<MTPReviewEntity> MTPrPending = await _context.MTPReviews
                                                                  .Where(n => (n.Status == AdendumStatus.Pending
                                                                            && n.CreatedBy == user_logged.UserName))
                                                                  .ToListAsync();
                ViewBag.MTPrPending = MTPrPending.Count().ToString();

                ViewBag.ExpiredMTPs = await _context.MTPs
                                                       .CountAsync(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                      && m.Client.Status == StatusType.Open
                                                                      && m.Active == true
                                                                      && m.Goals.Where(n => n.Objetives.Where(o => o.DateResolved.Date > DateTime.Today.Date
                                                                                                           && o.Goal.Service == m.Client.Service
                                                                                                           && o.Compliment == false).Count() > 0).Count() == 0));

                List<MTPEntity> MtpWithReview = await _context.MTPs
                                                              .Include(wc => wc.Messages)
                                                              .Where(wc => (wc.DocumentAssistant.LinkedUser == User.Identity.Name
                                                                         && wc.Status == MTPStatus.Pending))
                                                              .ToListAsync();
                MtpWithReview = MtpWithReview.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.MtpWithReview = MtpWithReview.Count.ToString();

                List<BioEntity> BioWithReview = await _context.Bio
                                                              .Include(wc => wc.Messages)
                                                              .Where(wc => (wc.DocumentsAssistant.LinkedUser == User.Identity.Name
                                                                         && wc.Status == BioStatus.Pending))
                                                              .ToListAsync();
                BioWithReview = BioWithReview.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.BioWithReview = BioWithReview.Count.ToString();

                List<BriefEntity> BriefWithReview = await _context.Brief
                                                                  .Include(wc => wc.Messages)
                                                                  .Where(wc => (wc.DocumentsAssistant.LinkedUser == User.Identity.Name
                                                                     && wc.Status == BioStatus.Pending))
                                                                  .ToListAsync();
                BriefWithReview = BriefWithReview.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.BriefWithReview = BriefWithReview.Count.ToString();

                List<MTPReviewEntity> MTPrWithReview = await _context.MTPReviews
                                                                     .Include(wc => wc.Messages)
                                                                     .Where(wc => (wc.CreatedBy == User.Identity.Name
                                                                                && wc.Status == AdendumStatus.Pending))
                                                                     .ToListAsync();
                MTPrWithReview = MTPrWithReview.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.MTPrWithReview = MTPrWithReview.Count.ToString();

                ViewBag.IntakeMissing = await _context.Clients
                                                          .CountAsync(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                          && wc.IntakeScreening == null
                                                                          && wc.IntakeConsentForTreatment == null
                                                                          && wc.IntakeConsentForRelease == null
                                                                          && wc.IntakeConsumerRights == null
                                                                          && wc.IntakeAcknowledgementHipa == null
                                                                          && wc.IntakeAccessToServices == null
                                                                          && wc.IntakeOrientationChecklist == null
                                                                          && wc.IntakeTransportation == null
                                                                          && wc.IntakeConsentPhotograph == null
                                                                          && wc.IntakeFeeAgreement == null
                                                                          && wc.IntakeTuberculosis == null
                                                                          && wc.OnlyTCM == false)
                                                                          && ((wc.DocumentsAssistant != null && wc.DocumentsAssistant.LinkedUser == user_logged.UserName)
                                                                             || wc.DocumentsAssistant == null));
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);


                ViewBag.NotStartedCases = await _context.TCMClient                                                                 
                                                        .CountAsync(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && s.Status == StatusType.Open
                                                                        && s.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                        && (s.TcmServicePlan == null
                                                                            
                                                                            || s.TCMAssessment == null
                                                                            || (s.TcmIntakeAppendixJ == null

                                                                            && s.TcmIntakeAppendixI == null))));

                ViewBag.OpenBinder = await _context.TCMClient
                                                   .CountAsync(g => (g.Status == StatusType.Open
                                                                  && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                  && g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.ServicePlanPending = await _context.TCMClient                                                     
                                                           .CountAsync(g => ( g.Status == StatusType.Open
                                                                           && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                           && g.TcmServicePlan.Approved == 1
                                                                           && g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.AdendumPending = await _context.TCMAdendums                                                 
                                                       .CountAsync(g => (g.Approved == 1
                                                                      && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                      && g.TcmServicePlan.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.ServicePlanReviewEdition = await _context.TCMClient                                                           
                                                                 .CountAsync(g => (g.Status == StatusType.Open
                                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 0
                                                                                && g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.ServicePlanReviewPending = await _context.TCMClient                                                           
                                                                 .CountAsync(g => (g.Status == StatusType.Open
                                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 1
                                                                                && g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.DischargePending = await _context.TCMClient                                                   
                                                         .CountAsync(g => (g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && g.TcmServicePlan.TCMDischarge.Approved == 1
                                                                        && g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.TCMFarsPending = await _context.TCMFarsForm                                                 
                                                       .CountAsync(g => (g.Status == FarsStatus.Pending
                                                                      && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                      && g.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.NotesPending = await _context.TCMNote
                                                     .CountAsync(g => (g.Status == NoteStatus.Pending
                                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && g.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.NotesApproved = await _context.TCMNote
                                                      .CountAsync(g => (g.Status == NoteStatus.Approved
                                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                     && g.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.ActivityPending = await _context.TCMServiceActivity
                                                        .CountAsync(g => (g.Approved < 2
                                                                       && g.TcmService.Clinic.Id == user_logged.Clinic.Id));

                ViewBag.AppendiceJPending = await _context.TCMIntakeAppendixJ
                                                          .CountAsync(g => (g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                         && g.Approved == 1
                                                                         && g.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.FarsPending = await _context.TCMFarsForm
                                                    .CountAsync(g => (g.TCMClient.Casemanager.Id == user_logged.Clinic.Id
                                                                   && g.Status == FarsStatus.Pending
                                                                   && g.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.AssesmentPending = await _context.TCMAssessment
                                                         .CountAsync(g => (g.Approved == 1
                                                                        && g.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.TCMNoteReview = _context.TCMNote
                                                .Include(wc => wc.TCMClient)
                                                .ThenInclude(wc => wc.Client)
                                                .Include(wc => wc.TCMClient.Casemanager)
                                                .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                .Where(wc => (wc.TCMClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                           && wc.Status == NoteStatus.Pending
                                                           && wc.TCMMessages.Count() > 0
                                                           && wc.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)).Count()
                                                .ToString();

                ViewBag.TCMCaseManager = await _context.CaseManagers
                                                       .CountAsync(g => (g.Status == StatusType.Open
                                                                      && g.Clinic.Id == user_logged.Clinic.Id
                                                                      && g.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                      && g.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.OpenBinderClose = await _context.TCMClient
                                                  .CountAsync(g => (g.Status == StatusType.Close
                                                                 && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                 && g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.Billing = await _context.TCMClient
                                         .CountAsync(g => (g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                        && g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id));
                ViewBag.AppendiceEPending = await _context.TCMIntakeAppendixI
                                                          .CountAsync(g => (g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                         && g.Approved == 1
                                                                         && g.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));

                ViewBag.ClientWithoutDischarge = await _context.TCMClient
                                                               .CountAsync(g => (g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                         && g.TcmServicePlan != null
                                                                         && g.TcmServicePlan.TCMDischarge == null
                                                                         && g.Status == StatusType.Close
                                                                         && g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName));
            }
            if (User.IsInRole("Frontdesk"))
            {
                return RedirectToAction("Schedule", "Calendar");
            }
            if (User.IsInRole("Biller"))
            {
               
            }
            return View();
        }

        private async Task<int> AmountOfDocumentsCMH(int idClinic)
        {
            int total = 0;
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                total += await db.IntakeAcknowledgement
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeConsentForRelease
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeConsentForTreatment
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeConsumerRights
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeOrientationCheckList
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeAccessToServices
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeConsentPhotograph
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeFeeAgreement
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeMedicalHistory
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeScreenings
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.IntakeTransportation
                                 .AsNoTracking()
                                 .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
                total += await db.Bio
                                 .AsNoTracking()
                                 .CountAsync(b => (b.Client.Clinic.Id == idClinic && b.Client.OnlyTCM == false));
                total += await db.MTPs
                                 .AsNoTracking()
                                 .CountAsync(m => (m.Client.Clinic.Id == idClinic && m.Client.OnlyTCM == false));
                total += await db.MTPReviews
                                 .AsNoTracking()
                                 .CountAsync(m => (m.Mtp.Client.Clinic.Id == idClinic && m.Mtp.Client.OnlyTCM == false));
                total += await db.Adendums
                                 .AsNoTracking()
                                 .CountAsync(a => (a.Mtp.Client.Clinic.Id == idClinic && a.Mtp.Client.OnlyTCM == false));
                total += await db.Discharge
                                 .AsNoTracking()
                                 .CountAsync(d => (d.Client.Clinic.Id == idClinic && d.Client.OnlyTCM == false));
                total += await db.FarsForm
                                 .AsNoTracking()
                                 .CountAsync(f => (f.Client.Clinic.Id == idClinic && f.Client.OnlyTCM == false));
            }
            return total;
        }

        private async Task<int> MHPendingNotes(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Workdays_Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Facilitator.Clinic.Id == clinicId
                                                && (wc.Note.Status == NoteStatus.Pending
                                                || wc.IndividualNote.Status == NoteStatus.Pending
                                                || wc.GroupNote.Status == NoteStatus.Pending
                                                || wc.GroupNote2.Status == NoteStatus.Pending
                                                || wc.NoteP.Status == NoteStatus.Pending)));
            }
        }

        private async Task<int> MHInProgressNotes(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Workdays_Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Facilitator.Clinic.Id == clinicId
                                                && (wc.Note.Status == NoteStatus.Edition
                                                || wc.IndividualNote.Status == NoteStatus.Edition
                                                || wc.GroupNote.Status == NoteStatus.Edition
                                                || wc.GroupNote2.Status == NoteStatus.Edition
                                                || wc.NoteP.Status == NoteStatus.Edition)));
            }
        }

        private async Task<int> MHNotStartedNotes(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Workdays_Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Facilitator.Clinic.Id == clinicId
                                                && wc.Note == null
                                                && wc.IndividualNote == null
                                                && wc.GroupNote == null
                                                && wc.GroupNote2 == null
                                                && wc.NoteP == null
                                                && wc.Present == true));
            }
        }

        private async Task<int> MHMTPMissing(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(c => (c.Clinic.Id == clinicId && c.OnlyTCM == false
                                              && c.MTPs.Count == 0));
            }
        }

        private async Task<int> MHNotesWithReview(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Workdays_Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Facilitator.Clinic.Id == clinicId &&
                                                 (wc.Note.Status == NoteStatus.Pending
                                                 || wc.IndividualNote.Status == NoteStatus.Pending
                                                 || wc.GroupNote.Status == NoteStatus.Pending
                                                 || wc.GroupNote2.Status == NoteStatus.Pending
                                                 || wc.NoteP.Status == NoteStatus.Pending)
                                                 && wc.Messages.Count() > 0));
            }
        }

        private async Task<int> MHApprovedNotes(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Workdays_Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Facilitator.Clinic.Id == clinicId &&
                                                 (wc.Note.Status == NoteStatus.Approved
                                                 || wc.IndividualNote.Status == NoteStatus.Approved
                                                 || wc.GroupNote.Status == NoteStatus.Approved
                                                 || wc.GroupNote2.Status == NoteStatus.Approved
                                                 || wc.NoteP.Status == NoteStatus.Approved)));
            }
        }

        private async Task<int> MHNotPresentNotes(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Workdays_Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Facilitator.Clinic.Id == clinicId &&
                                                  wc.Present == false));
            }
        }

        private async Task<int> MHExpiredMTPs(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.MTPs
                               .AsNoTracking()
                               .CountAsync(m => (m.Client.Clinic.Id == clinicId
                                                && m.Client.Status == StatusType.Open
                                                && m.Active == true
                                                && m.Goals.Where(n => n.Objetives.Where(o => o.DateResolved.Date > DateTime.Today.Date
                                                                                     && o.Goal.Service == m.Client.Service
                                                                                     && o.Compliment == false).Count() > 0).Count() == 0));
            }
        }

        private async Task<int> MHPendingBIO(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Clinic.Id == clinicId
                                                 && wc.Bio == null && wc.Brief == null
                                                 && wc.OnlyTCM == false));
            }
        }

        private async Task<int> MHPendingInitialFars(int clinicId) 
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Clinic.Id == clinicId
                                                 && wc.FarsFormList.Where(n => n.Type == FARSType.Initial).Count() == 0
                                                 && wc.OnlyTCM == false));
            }
        }

        private async Task<int> MHMedicalHistoryMissing(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Clinic.Id == clinicId
                                          && wc.IntakeMedicalHistory == null
                                          && wc.OnlyTCM == false));
            }
        }

        private async Task<int> MHIntakeMissing(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(wc => (wc.Clinic.Id == clinicId
                                                 && wc.IntakeScreening == null
                                                 && wc.IntakeConsentForTreatment == null
                                                 && wc.IntakeConsentForRelease == null
                                                 && wc.IntakeConsumerRights == null
                                                 && wc.IntakeAcknowledgementHipa == null
                                                 && wc.IntakeAccessToServices == null
                                                 && wc.IntakeOrientationChecklist == null
                                                 && wc.IntakeTransportation == null
                                                 && wc.IntakeConsentPhotograph == null
                                                 && wc.IntakeFeeAgreement == null
                                                 && wc.IntakeTuberculosis == null
                                                 && wc.OnlyTCM == false));
            }
        }

        private async Task<int> MHFarsMissing(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(n => n.Clinic.Id == clinicId
                                             && ((n.MTPs.FirstOrDefault(m => m.Active == true).MtpReviewList.Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.MtpReview).Count() == 0)
                                             || (n.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_PSR).Count() == 0)
                                             || (n.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Ind).Count() == 0)
                                             || (n.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Group).Count() == 0)
                                             || (n.MTPs.Where(m => m.AdendumList.Count() > n.FarsFormList.Where(f => f.Type == FARSType.Addendums).Count()).Count() > 0))
                                             && n.OnlyTCM == false);
            }
        }

        private async Task<int> MHClientDischarge(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                int clientListPSR = await db.Clients
                                            .AsNoTracking()
                                            .CountAsync(m => (m.Clinic.Id == clinicId
                                                             && m.Status == StatusType.Close
                                                             && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.PSR).Count() > 0
                                                             && m.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() == 0));
                int clientListIND = await db.Clients
                                            .AsNoTracking()
                                            .CountAsync(m => (m.Clinic.Id == clinicId
                                                                 && m.Status == StatusType.Close
                                                                 && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Individual).Count() > 0
                                                                 && m.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() == 0));
                int clientListGroup = await db.Clients
                                              .AsNoTracking()
                                              .CountAsync(m => (m.Clinic.Id == clinicId
                                                                   && m.Status == StatusType.Close
                                                                   && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Group).Count() > 0
                                                                   && m.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() == 0));

                return clientListPSR + clientListIND + clientListGroup;
            }
        }

        private async Task<int> MHClientAuthorization(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(n => n.Status == StatusType.Open
                                               // && n.Service == ServiceType.PSR
                                               && n.Clinic.Id == clinicId
                                               && n.OnlyTCM == false
                                               && (n.Clients_HealthInsurances.Where(g => g.Agency == ServiceAgency.CMH).Count() == 0
                                                || n.Clients_HealthInsurances.Where(m => m.Active == true
                                                                                      && m.ExpiredDate > DateTime.Today.AddDays(30)
                                                                                      && m.Agency == ServiceAgency.CMH
                                                                                      && m.HealthInsurance.NeedAuthorization == true).Count() == 0)) ;
            }
        }

        private async Task<int> MHClientBirthday(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(n => n.DateOfBirth.Month == DateTime.Today.Month && n.Status == StatusType.Open);
            }
        }

        private async Task<int> MHClientEligibility(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(n => n.EligibilityList.Where(m => m.EligibilityDate.Year == DateTime.Today.Year
                                                                            && m.EligibilityDate.Month == DateTime.Today.Month).Count() == 0
                                                                            && n.Status == StatusType.Open
                                                                            && n.Clinic.Id == clinicId);
            }
        }

        private async Task<int> MHSafetyPlan(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.Clients
                               .AsNoTracking()
                               .CountAsync(c => (c.Clinic.Id == clinicId && c.OnlyTCM == false
                                                && c.SafetyPlanList.Count() == 0));
            }
        }

        private async Task<int> TCMNotStartedCases(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(s => (s.Client.Clinic.Id == clinicId
                                                    && s.Status == StatusType.Open
                                                    && (s.TcmServicePlan == null
                                                        || s.TCMAssessment == null
                                                        || (s.TcmIntakeAppendixJ == null
                                                            && s.TcmIntakeAppendixI == null))));
            }
        }

        private async Task<int> TCMOpenBinder(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMServicePlanPending(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == clinicId
                                                    && g.TcmServicePlan.Approved == 1));
            }
        }

        private async Task<int> TCMAdendumPending(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == clinicId
                                                    && g.TcmServicePlan.TCMAdendum.Where(n => n.Approved == 1).Count() > 0));
            }
        }

        private async Task<int> TCMServicePlanReviewPending(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                && g.Client.Clinic.Id == clinicId
                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 1));
            }
        }

        private async Task<int> TCMDischargePending(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                && g.Client.Clinic.Id == clinicId
                                                && g.TcmServicePlan.TCMDischarge.Approved == 1));
            }
        }

        private async Task<int> TCMCaseManager(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.CaseManagers
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                    && g.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMFarsPending(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMFarsForm
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == FarsStatus.Pending
                                                    && g.TCMClient.Client.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMAssesmentPending(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMAssessment
                               .AsNoTracking()
                               .CountAsync(g => (g.Approved == 1
                                                && g.TcmClient.Client.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMAllDocuments(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(g => g.Client.Clinic.Id == clinicId);
            }
        }

        private async Task<int> TCMNotesEdition(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMNote
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == NoteStatus.Edition
                                                    && g.TCMClient.Client.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMNotesPending(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMNote
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == NoteStatus.Pending
                                                    && g.TCMClient.Client.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMNotesApproved(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMNote
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == NoteStatus.Approved
                                                && g.TCMClient.Client.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMNotesWithReview(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMNote
                               .AsNoTracking()
                               .CountAsync(wc => (wc.TCMClient.Casemanager.Clinic.Id == clinicId &&
                                                        wc.Status == NoteStatus.Pending &&
                                                        wc.TCMMessages.Count() > 0));
            }
        }

        private async Task<int> TCMSupervisor(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMSupervisors
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                    && g.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMBilling(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(g => (g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == clinicId));
            }
        }

        private async Task<int> TCMClientAuthorization(int clinicId)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                return await db.TCMClient
                               .AsNoTracking()
                               .CountAsync(n => n.Status == StatusType.Open
                                               // && n.Service == ServiceType.PSR
                                               && n.Client.Clinic.Id == clinicId
                                               && (n.Client.Clients_HealthInsurances == null
                                               || n.Client.Clients_HealthInsurances.Where(m => m.Active == true
                                                                && m.Agency == ServiceAgency.TCM
                                                                && m.ExpiredDate > DateTime.Today.AddDays(30)).Count() == 0));
            }
        }
    }
}