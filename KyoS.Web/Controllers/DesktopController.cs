﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class DesktopController : Controller
    {
        private readonly DataContext _context;
        public DesktopController(DataContext context)
        {
            _context = context;           
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
                    ViewBag.ApprovedNotes = _context.Workdays_Clients
                                                   .Include(wc => wc.Note)
                                                   .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && (wc.Note.Status == NoteStatus.Approved || wc.NoteP.Status == NoteStatus.Approved)
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();

                    ViewBag.PendingNotes = _context.Workdays_Clients
                                                      .Include(wc => wc.Note)
                                                      .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                                  && wc.Workday.Service == ServiceType.PSR)).ToString();

                    ViewBag.InProgressNotes = _context.Workdays_Clients
                                                      .Include(wc => wc.Note)
                                                      .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && (wc.Note.Status == NoteStatus.Edition || wc.NoteP.Status == NoteStatus.Edition)
                                                                  && wc.Workday.Service == ServiceType.PSR)).ToString();

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

                    ViewBag.NotPresentNotes = _context.Workdays_Clients
                                                      .Include(wc => wc.Note)
                                                      .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Present == false
                                                                  && wc.Workday.Service == ServiceType.PSR)).ToString();
                }
                else
                    ViewBag.EnabledPSR = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                ViewBag.EnabledInd = "1";
                if (_context.Workdays_Clients.Count(wc => wc.Workday.Service == ServiceType.Individual && wc.Facilitator.LinkedUser == User.Identity.Name) > 0)
                {
                    ViewBag.ApprovedIndNotes = _context.Workdays_Clients
                                                   .Include(wc => wc.IndividualNote)
                                                   .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.IndividualNote.Status == NoteStatus.Approved
                                                              && wc.Workday.Service == ServiceType.Individual)).ToString();

                    ViewBag.PendingIndNotes = _context.Workdays_Clients
                                                      .Include(wc => wc.IndividualNote)
                                                      .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.IndividualNote.Status == NoteStatus.Pending
                                                                  && wc.Workday.Service == ServiceType.Individual)).ToString();

                    ViewBag.InProgressIndNotes = _context.Workdays_Clients
                                                         .Include(wc => wc.IndividualNote)
                                                         .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.IndividualNote.Status == NoteStatus.Edition
                                                                  && wc.Workday.Service == ServiceType.Individual)).ToString();

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
                                                         .Include(wc => wc.IndividualNote)
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
                    ViewBag.ApprovedGroupNotes = _context.Workdays_Clients
                                                         .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                    && wc.GroupNote.Status == NoteStatus.Approved
                                                                    && wc.Workday.Service == ServiceType.Group)).ToString();

                    ViewBag.PendingGroupNotes = _context.Workdays_Clients
                                                        .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                   && wc.GroupNote.Status == NoteStatus.Pending
                                                                   && wc.Workday.Service == ServiceType.Group)).ToString();

                    ViewBag.InProgressGroupNotes = _context.Workdays_Clients
                                                           .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.GroupNote.Status == NoteStatus.Edition
                                                                      && wc.Workday.Service == ServiceType.Group)).ToString();

                    not_started_list = await _context.Workdays_Clients
                                                     .Include(wc => wc.GroupNote)
                                                     .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                && wc.Present == true
                                                                && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                    not_started_list = not_started_list.Where(wc => wc.GroupNote == null).ToList();
                    ViewBag.NotStartedGroupNotes = not_started_list.Count.ToString();

                    notes_review_list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Messages)
                                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                 && wc.GroupNote.Status == NoteStatus.Pending
                                                                 && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                    notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                    ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();

                    ViewBag.NotPresentGroupNotes = _context.Workdays_Clients
                                                           .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.Present == false
                                                                      && wc.Workday.Service == ServiceType.Group)).ToString();
                }
                else
                    ViewBag.EnabledGroup = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                List<MTPEntity> mtps = await _context.MTPs
                                                     .Include(n => n.MtpReviewList)                                                       
                                                     .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                              && m.Active == true && m.Client.Status == StatusType.Open
                                                              && m.Client.Group.Facilitator.Id == facilitator.Id)).ToListAsync();
                int count = 0;
                int month = 0;
                foreach (var item in mtps)
                {
                    foreach (var value in item.MtpReviewList)
                    {
                        month = month + value.MonthOfTreatment;
                    
                    }
                    if (item.NumberOfMonths != null)
                    {
                        if (DateTime.Now > item.MTPDevelopedDate.Date.AddMonths(Convert.ToInt32(item.NumberOfMonths+month)))
                        {
                            count++;
                        }
                    }
                    month = 0;
                }

                ViewBag.ExpiredMTPsFacilitator = count.ToString();

                List<ClientEntity> clientListPSR = await _context.Clients
                                                                  .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.PSR).ToList().Count == 0) 
                                                                    && m.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == StatusType.Close && m.IdFacilitatorPSR == facilitator.Id)).ToListAsync();
                List<ClientEntity> clientListIND = await _context.Clients
                                                              .Include(m => m.DischargeList)

                                                              .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.Individual).ToList().Count == 0)
                                                                    && m.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == StatusType.Close && m.IndividualTherapyFacilitator.Id == facilitator.Id)).ToListAsync();
                foreach (var item in clientListIND)
                {
                    clientListPSR.Add(item);
                }

                ViewBag.ClientDischarge = clientListPSR.Count().ToString();

                List<DischargeEntity> DischargeEdit = await _context.Discharge
                                                                    .Include(f => f.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Where(n => (n.Status == DischargeStatus.Edition
                                                                        && n.Client.Clinic.Id == user_logged.Clinic.Id 
                                                                        && n.CreatedBy == user_logged.UserName))
                                                                    .OrderBy(f => f.Client.Name)
                                                                    .ToListAsync();
                ViewBag.DischargeEdition = DischargeEdit.Count().ToString();

                List<DischargeEntity> DischargePending = await _context.Discharge
                                                                    .Include(f => f.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Where(n => (n.Status == DischargeStatus.Pending
                                                                        && n.Client.Clinic.Id == user_logged.Clinic.Id 
                                                                        && n.CreatedBy == user_logged.UserName))
                                                                    .OrderBy(f => f.Client.Name)
                                                                    .ToListAsync();
                ViewBag.DischargePending = DischargePending.Count().ToString();

                List<MTPReviewEntity> MTPReviewEdit = await _context.MTPReviews
                                                                .Include(n => n.Mtp)
                                                                .ThenInclude(n => n.Client)
                                                                .ThenInclude(n => n.Clinic)
                                                                .Where(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                   && m.Status == AdendumStatus.Edition
                                                                   && (m.Mtp.Client.IdFacilitatorPSR == facilitator.Id ||
                                                                       m.Mtp.Client.IndividualTherapyFacilitator.Id == facilitator.Id))).ToListAsync();
                ViewBag.MTPReviewEdition = MTPReviewEdit.Count().ToString();

                List<MTPReviewEntity> MTPReviewPending = await _context.MTPReviews
                                                                .Include(n => n.Mtp)
                                                                .ThenInclude(n => n.Client)
                                                                .ThenInclude(n => n.Clinic)
                                                                .Where(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                   && m.Status == AdendumStatus.Pending
                                                                   && m.Mtp.Client.Group.Facilitator.Id == facilitator.Id)).ToListAsync();
                ViewBag.MTPReviewPending = MTPReviewPending.Count().ToString();

            }
            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                ViewBag.PendingActivities = _context.Activities
                                                    .Count(a => (a.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                              && a.Status == ActivityStatus.Pending)).ToString();

                ViewBag.PendingNotes = _context.Workdays_Clients
                                               .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                              && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();

                ViewBag.PendingIndNotes = _context.Workdays_Clients
                                                  .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.IndividualNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Individual)).ToString();

                ViewBag.PendingGroupNotes = _context.Workdays_Clients
                                                    .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.GroupNote.Status == NoteStatus.Pending
                                                               && wc.Workday.Service == ServiceType.Group)).ToString();

                ViewBag.PendingBIO = _context.Clients
                                                    .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.Bio == null)).ToString();

                ViewBag.PendingInitialFars = _context.Clients
                                                    .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.FarsFormList.Count == 0)).ToString();

                ViewBag.PendingFars = _context.FarsForm
                                                  .Count(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == FarsStatus.Pending)).ToString();

                ViewBag.PendingAddendum = _context.Adendums
                                                  .Count(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == AdendumStatus.Pending)).ToString();

                ViewBag.PendingMTPReview = _context.MTPReviews
                                                  .Count(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == AdendumStatus.Pending)).ToString();

                ViewBag.PendingDischarge = _context.Discharge
                                                  .Count(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == DischargeStatus.Pending)).ToString();

                List<ClientEntity> client = await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

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
                                                             && wc.GroupNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();

                ViewBag.MedicalHistoryMissing = _context.Clients
                                                       .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                              && wc.IntakeMedicalHistory == null)).ToString();
            }
            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                ViewBag.PendingNotes = _context.Workdays_Clients
                                               .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                         && (wc.Note.Status == NoteStatus.Pending || wc.IndividualNote.Status == NoteStatus.Pending 
                                                             || wc.GroupNote.Status == NoteStatus.Pending
                                                             || wc.NoteP.Status == NoteStatus.Pending))).ToString();

                ViewBag.InProgressNotes = _context.Workdays_Clients
                                                  .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                            && (wc.Note.Status == NoteStatus.Edition || wc.IndividualNote.Status == NoteStatus.Edition
                                                                || wc.GroupNote.Status == NoteStatus.Edition || wc.NoteP.Status == NoteStatus.Edition))).ToString();

                List<Workday_Client> not_started_list = await _context.Workdays_Clients

                                                                      .Include(wc => wc.Note)
                                                                      .Include(wc => wc.IndividualNote)
                                                                      .Include(wc => wc.GroupNote)
                                                                      .Include(wc => wc.NoteP)

                                                                      .Where(wc => wc.Facilitator.Clinic.Id == user_logged.Clinic.Id)
                                                                      .ToListAsync();

                not_started_list = not_started_list.Where(wc => (wc.Note == null && wc.IndividualNote == null && 
                                                                 wc.GroupNote == null && wc.NoteP == null && wc.Present == true)).ToList();
                ViewBag.NotStartedNotes = not_started_list.Count.ToString();

                List<ClientEntity> client = await _context.Clients

                                                          .Include(c => c.MTPs)

                                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                                          .ToListAsync();

                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                       .Include(wc => wc.Messages)
                                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                                                    (wc.Note.Status == NoteStatus.Pending || wc.IndividualNote.Status == NoteStatus.Pending ||
                                                                                     wc.GroupNote.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)))
                                                                       .ToListAsync();

                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();
                
                //-------------clients without documentation--------------//
                client = await _context.Clients
                                       .Where(c => c.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                int clients_without_doc = 0;                
                DocumentEntity doc_pe;
                DocumentEntity doc_intake;
                DocumentEntity doc_bio;
                DocumentEntity doc_mtp;
                DocumentEntity doc_fars;
                DocumentEntity doc_consent;
                foreach (var item in client)
                {
                    doc_pe = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Psychiatrist_evaluation
                                                                        && d.Client.Id == item.Id));
                    doc_intake = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Intake
                                                                        && d.Client.Id == item.Id));
                    doc_bio = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Bio
                                                                        && d.Client.Id == item.Id));
                    doc_mtp = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.MTP
                                                                        && d.Client.Id == item.Id));
                    doc_fars = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Fars
                                                                        && d.Client.Id == item.Id));
                    doc_consent = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Consent
                                                                        && d.Client.Id == item.Id));

                    if (doc_pe == null || doc_intake == null || doc_bio == null || doc_mtp == null || doc_fars == null || doc_consent == null)
                    {
                        clients_without_doc++;
                    }
                }
                ViewBag.ClientsWithoutDoc = clients_without_doc.ToString();

                List<ClientEntity> Clients = await _context.Clients
                                                     .Include(d => d.IntakeAcknowledgementHipa)
                                                     .Include(d => d.IntakeConsentForRelease)
                                                     .Include(d => d.IntakeConsentForTreatment)
                                                     .Include(d => d.IntakeConsumerRights)
                                                     .Include(d => d.IntakeOrientationChecklist)
                                                     .Include(d => d.IntakeAccessToServices)
                                                     .Include(d => d.IntakeConsentPhotograph)
                                                     .Include(d => d.IntakeFeeAgreement)
                                                     .Include(d => d.IntakeMedicalHistory)
                                                     .Include(d => d.IntakeScreening)
                                                     .Include(d => d.IntakeTransportation)
                                                     .Include(d => d.IntakeTuberculosis)
                                                     .Include(d => d.Bio)
                                                     .Include(d => d.MTPs)
                                                     .ThenInclude(d => d.MtpReviewList)
                                                     .Include(d => d.MTPs)
                                                     .ThenInclude(d => d.AdendumList)
                                                     .Include(d => d.MTPs)
                                                     .Include(d => d.DischargeList)
                                                     .Include(d => d.FarsFormList)
                                                     .Where(g => g.Clinic.Id == user_logged.Clinic.Id)
                                                     .ToListAsync();
                int cantDocument = 0;
                foreach (var item in Clients)
                {
                    if (item.IntakeAcknowledgementHipa != null)
                        cantDocument++;
                    if (item.IntakeConsentForRelease != null)
                        cantDocument++;
                    if (item.IntakeConsentForTreatment != null)
                        cantDocument++;
                    if (item.IntakeConsumerRights != null)
                        cantDocument++;
                    if (item.IntakeAccessToServices != null)
                        cantDocument++;
                    if (item.IntakeConsentPhotograph != null)
                        cantDocument++;
                    if (item.IntakeFeeAgreement != null)
                        cantDocument++;
                    if (item.IntakeMedicalHistory != null)
                        cantDocument++;
                    if (item.IntakeScreening != null)
                        cantDocument++;
                    if (item.IntakeTransportation != null)
                        cantDocument++;
                    if (item.IntakeTuberculosis != null)
                        cantDocument++;
                    if (item.Bio != null)
                        cantDocument++;

                    cantDocument += item.FarsFormList.Count();
                    cantDocument += item.DischargeList.Count();
                    cantDocument += item.MTPs.Count();
                    cantDocument += item.MTPs.Sum(n => n.AdendumList.Count());
                    cantDocument += item.MTPs.Sum(n => n.MtpReviewList.Count());
                }

                ViewBag.AllDocumentsForClient = cantDocument.ToString();

                //ViewBag.AllDocumentsForClient = _context.Clients.Count(n => n.Clinic.Id == user_logged.Clinic.Id).ToString();

                ViewBag.ApprovedNotes = _context.Workdays_Clients

                                                .Include(wc => wc.Note)

                                                .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                             (wc.Note.Status == NoteStatus.Approved || wc.IndividualNote.Status == NoteStatus.Approved ||
                                                              wc.GroupNote.Status == NoteStatus.Approved || wc.NoteP.Status == NoteStatus.Approved))).ToString();

                ViewBag.NotPresentNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.Present == false)).ToString();

                List<MTPEntity> mtps = await _context.MTPs
                                                     .Include(n => n.MtpReviewList)
                                                     .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id 
                                                               && m.Active == true && m.Client.Status == StatusType.Open)).ToListAsync();
                int count = 0;
                int month = 0;
                foreach (var item in mtps)
                {
                    foreach (var product in item.MtpReviewList)
                    {
                        month += product.MonthOfTreatment;
                    }
                    if (item.NumberOfMonths != null)
                    {
                        if (DateTime.Now > item.MTPDevelopedDate.Date.AddMonths(Convert.ToInt32(item.NumberOfMonths+month)))
                        {
                            count++;
                        }
                    }
                    month = 0;
                }

                ViewBag.ExpiredMTPs = count.ToString();

                //TCM Dashboard

                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                               .Include(c => c.TcmServicePlan)
                                                               .Include(c => c.Client)
                                                               .Where(c => c.Client.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                tcmClient = tcmClient.Where(wc => wc.TcmServicePlan == null).ToList();
                ViewBag.NotStartedCases = tcmClient.Count.ToString();

                ViewBag.OpenBinder = _context.TCMClient
                                             .Where(g => (g.Status == StatusType.Open
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.CloseCases = _context.TCMClient
                                             .Where(g => (g.Status == StatusType.Close
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.Billing = _context.TCMClient
                                          .Where(g => (g.Status == StatusType.Open
                                             && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ServicePlanEdition = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => (g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanPending = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => (g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 1)).Count().ToString();

                ViewBag.AdendumEdition = _context.TCMClient
                                                 .Include(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMAdendum)
                                                 .Where(g => (g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && g.TcmServicePlan.TCMAdendum.Where(n => n.Approved == 0).Count() > 0)).Count().ToString();

                ViewBag.AdendumPending = _context.TCMClient
                                                 .Include(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMAdendum)
                                                 .Where(g => (g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && g.TcmServicePlan.TCMAdendum.Where(n => n.Approved == 1).Count() > 0)).Count().ToString();

                ViewBag.ServicePlanReviewEdition = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanReviewPending = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 1)).Count().ToString();

                ViewBag.DischargeEdition = _context.TCMClient
                                                   .Include(n => n.TcmServicePlan)
                                                   .ThenInclude(n => n.TCMDischarge)
                                                   .Where(g => (g.Status == StatusType.Open
                                                       && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && g.TcmServicePlan.TCMDischarge.Approved == 0)).Count().ToString();

                ViewBag.DischargePending = _context.TCMClient
                                                   .Include(n => n.TcmServicePlan)
                                                   .ThenInclude(n => n.TCMDischarge)
                                                   .Where(g => (g.Status == StatusType.Open
                                                       && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && g.TcmServicePlan.TCMDischarge.Approved == 1)).Count().ToString();

                ViewBag.TCMCaseManager = _context.CaseManagers
                                                 .Where(g => (g.Status == StatusType.Open
                                                    && g.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.TCMFarsPending = _context.TCMFarsForm
                                              .Where(g => (g.Status == FarsStatus.Pending
                                                        && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AssesmentPending = _context.TCMAssessment
                                                   .Where(g => (g.Approved == 1
                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AllDocuments = _context.TCMClient
                                               .Where(g => g.Client.Clinic.Id == user_logged.Clinic.Id).Count().ToString();

                ViewBag.TCMNotesEdition = _context.TCMNote
                                                  .Where(g => (g.Status == NoteStatus.Edition
                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.TCMNotesPending = _context.TCMNote
                                                  .Where(g => (g.Status == NoteStatus.Pending
                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.TCMNotesApproved = _context.TCMNote
                                                .Where(g => (g.Status == NoteStatus.Approved
                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();
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

                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                .Include(c => c.TcmServicePlan)
                                                                .Include(c => c.Client)
                                                                .Where(c => c.Client.Clinic.Id == user_logged.Clinic.Id
                                                                     && c.Casemanager.Id == caseManager.Id).ToListAsync();
                tcmClient = tcmClient.Where(wc => wc.TcmServicePlan == null).ToList();
                ViewBag.NotStartedCases = tcmClient.Count.ToString();

                ViewBag.OpenBinder = _context.TCMClient
                                             .Where(g => (g.Casemanager.Id == caseManager.Id
                                                && g.Status == StatusType.Open
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.CloseCases = _context.TCMClient
                                             .Where(g => (g.Casemanager.Id == caseManager.Id
                                                && g.Status == StatusType.Close
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.Billing = _context.TCMClient
                                          .Where(g => (g.Casemanager.Id == caseManager.Id
                                             && g.Status == StatusType.Open
                                             && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ServicePlanEdition = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => (g.Casemanager.Id == caseManager.Id
                                                        && g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanPending = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => (g.Casemanager.Id == caseManager.Id
                                                        && g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 1)).Count().ToString();

                ViewBag.AdendumEdition = _context.TCMAdendums
                                                 .Include(n => n.TcmServicePlan)
                                                 .Where(g => (g.Approved == 0
                                                    && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count()
                                                 .ToString();

                ViewBag.AdendumPending = _context.TCMClient
                                                 .Include(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMAdendum)
                                                 .Where(g => (g.Casemanager.Id == caseManager.Id
                                                    && g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && g.TcmServicePlan.TCMAdendum.Where(n => n.Approved == 1).Count() > 0)).Count().ToString();

                ViewBag.ServicePlanReviewEdition = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                && g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanReviewPending = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                && g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 1)).Count().ToString();

                ViewBag.DischargeEdition = _context.TCMDischarge
                                                   .Where(g => (g.Approved == 0
                                                     && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                     && g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id)).Count().ToString();

                ViewBag.DischargePending = _context.TCMDischarge
                                                   .Where(g => (g.Approved == 1
                                                     && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                     && g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id)).Count().ToString();

                ViewBag.NotesEdition = _context.TCMNote
                                               .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                      && g.Status == NoteStatus.Edition
                                                      && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesPending = _context.TCMNote
                                               .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                     && g.Status == NoteStatus.Pending
                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesApproved = _context.TCMNote
                                                .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                    && g.Status == NoteStatus.Approved
                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AppendiceJPending = _context.TCMIntakeAppendixJ
                                                    .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                         && g.Approved == 1
                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.FarsPending = _context.TCMFarsForm
                                              .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                         && g.Status == FarsStatus.Pending
                                                         && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AssesmentPending = _context.TCMAssessment
                                              .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                         && g.Approved == 1
                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AllDocuments = _context.TCMClient
                                               .Where(g => (g.Casemanager.Id == caseManager.Id
                                                         && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ActivityPending = _context.TCMServiceActivity
                                                  .Where(g => (g.Approved < 2
                                                               && g.CreatedBy == user_logged.UserName
                                                               && g.TcmService.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.TCMNoteReview = _context.TCMNote
                                                .Include(wc => wc.CaseManager)
                                                .Include(wc => wc.TCMClient)
                                                .ThenInclude(wc => wc.Client)
                                                .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                .Where(wc => (wc.CaseManager.LinkedUser == user_logged.UserName
                                                       && wc.Status == NoteStatus.Pending
                                                       && wc.TCMMessages.Count() > 0)).Count()
                                                .ToString();
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                List<ClientEntity> client = await _context.Clients
                                                         .Include(c => c.MTPs)
                                                         .Where(c => c.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                ViewBag.PendingBIO = _context.Clients
                                                    .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.Bio == null)).ToString();

                ViewBag.PendingInitialFars = _context.Clients
                                                    .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.FarsFormList.Count == 0)).ToString();

                ViewBag.MedicalHistoryMissing = _context.Clients
                                                        .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.IntakeMedicalHistory == null)).ToString();

            }
            if (User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

              
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                .Include(c => c.TcmServicePlan)
                                                                .Include(c => c.Client)
                                                                .Where(c => c.Client.Clinic.Id == user_logged.Clinic.Id).ToListAsync();

                tcmClient = tcmClient.Where(wc => wc.TcmServicePlan == null).ToList();
                ViewBag.NotStartedCases = tcmClient.Count.ToString();

                ViewBag.OpenBinder = _context.TCMClient
                                             .Where(g => (g.Status == StatusType.Open
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ServicePlanPending = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => ( g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 1)).Count().ToString();

                ViewBag.AdendumPending = _context.TCMAdendums
                                                 .Include(n => n.TcmServicePlan)
                                                 .Where(g => (g.Approved == 1
                                                    && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count()
                                                 .ToString();

                ViewBag.ServicePlanReviewEdition = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanReviewPending = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 1)).Count().ToString();

                ViewBag.DischargePending = _context.TCMClient
                                                   .Include(n => n.TcmServicePlan)
                                                   .ThenInclude(n => n.TCMDischarge)
                                                   .Where(g => (g.Status == StatusType.Open
                                                       && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && g.TcmServicePlan.TCMDischarge.Approved == 1)).Count().ToString();

                ViewBag.TCMFarsPending = _context.TCMFarsForm
                                                 .Include(m =>m.TCMClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.Clinic)
                                                 .Where(g => (g.Status == FarsStatus.Pending
                                                      && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesPending = _context.TCMNote
                                               .Where(g => (g.Status == NoteStatus.Pending
                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesApproved = _context.TCMNote
                                                .Where(g => (g.Status == NoteStatus.Approved
                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ActivityPending = _context.TCMServiceActivity
                                                  .Where(g => (g.Approved < 2
                                                               && g.TcmService.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AppendiceJPending = _context.TCMIntakeAppendixJ
                                                   .Where(g => (g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.Approved == 1
                                                        && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.FarsPending = _context.TCMFarsForm
                                              .Where(g => (g.TCMClient.Casemanager.Id == user_logged.Clinic.Id
                                                         && g.Status == FarsStatus.Pending
                                                         && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AssesmentPending = _context.TCMAssessment
                                              .Where(g => (g.TcmClient.Casemanager.Id == user_logged.Clinic.Id
                                                         && g.Approved == 1
                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.TCMNoteReview =  _context.TCMNote
                                                 .Include(wc => wc.CaseManager)
                                                 .Include(wc => wc.TCMClient)
                                                 .ThenInclude(wc => wc.Client)
                                                 .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                 .Where(wc => (wc.CaseManager.Clinic.Id == user_logged.Clinic.Id
                                                        && wc.Status == NoteStatus.Pending
                                                        && wc.TCMMessages.Count() > 0)).Count()
                                                 .ToString();
            }
            return View();
        }
    }
}