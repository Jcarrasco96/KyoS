using KyoS.Api.Models.Records;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillsController : Controller
{
    private readonly DataContext _context;

    public BillsController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("{idStatus}")]
    public async Task<IActionResult> NotBilled(int idStatus)    //  0 - Edition, 1 - Pending, 2 - Approved, 3 - all status, 4 - not started
    {
        NoteStatus status = NoteStatus.Approved;
        bool allStatus = false;
        if (idStatus < 2)
        {
            status = (idStatus == 0) ? NoteStatus.Edition : (idStatus == 1) ? NoteStatus.Pending : NoteStatus.Approved;
        }
        else
        {
            if (idStatus == 3)
            {
                allStatus = true;
            }
        }
       
        List<Workday_Client> workday_Client = null!;
        List<TCMNoteEntity> tcmNotesUnBill = null!;
        if (idStatus == 0 || idStatus == 1 || idStatus == 2)
        {
            workday_Client = await _context.Workdays_Clients

                                           .Include(wc => wc.Facilitator)
                                           .Include(c => c.Client)
                                           .ThenInclude(w => w.Clinic)
                                           .ThenInclude(w => w.Setting)
                                           .Include(w => w.Workday)
                                           
                                           .Include(wc => wc.Client)
                                           .ThenInclude(w => w.Clients_Diagnostics)
                                           .ThenInclude(w => w.Diagnostic)

                                           .Include(wc => wc.Client)
                                           .ThenInclude(w => w.Clients_HealthInsurances)
                                           .ThenInclude(w => w.HealthInsurance)

                                           .Include(wc => wc.Note)
                                           .ThenInclude(t => t.Supervisor)

                                           .Include(wc => wc.NoteP)
                                           .ThenInclude(w => w.NotesP_Activities)

                                           .Include(wc => wc.NoteP)
                                           .ThenInclude(t => t.Supervisor)

                                           .Include(wc => wc.IndividualNote)
                                           .ThenInclude(t => t.Supervisor)

                                           .Include(wc => wc.GroupNote)
                                           .ThenInclude(w => w.GroupNotes_Activities)

                                           .Include(wc => wc.GroupNote)
                                           .ThenInclude(w => w.Supervisor)

                                           .Include(wc => wc.GroupNote2)
                                           .ThenInclude(w => w.GroupNotes2_Activities)

                                           .Include(wc => wc.GroupNote2)
                                           .ThenInclude(w => w.Supervisor)

                                           .AsSplitQuery()

                                           .Where(wc => wc.Present == true
                                                     && wc.Client != null
                                                     && wc.BilledDate == null
                                                     && (((int)wc.Note.Status) == idStatus || (int)wc.NoteP.Status == idStatus
                                                                                    || (int)wc.IndividualNote.Status == idStatus
                                                                                    || (int)wc.GroupNote.Status == idStatus
                                                                                    || (int)wc.GroupNote2.Status == idStatus))
                                            .OrderBy(wc => wc.Client.Name)
                                            .ThenBy(wc => wc.Workday.Date)
                                            .ToListAsync();

            tcmNotesUnBill = _context.TCMNote
                                     .Include(t => t.TCMClient)
                                     .ThenInclude(c => c.Client)
                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                     .ThenInclude(cd => cd.Diagnostic)

                                     .Include(t => t.TCMClient)
                                     .ThenInclude(c => c.Client)
                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                     .ThenInclude(cd => cd.HealthInsurance)

                                     .Include(t => t.TCMNoteActivity)

                                     .Include(t => t.TCMClient)
                                     .ThenInclude(t => t.Casemanager)
                                     .ThenInclude(t => t.TCMSupervisor)
                                     
                                     .Include(t => t.TCMClient)
                                     .ThenInclude(c => c.Client)
                                     .ThenInclude(w => w.Clinic)
                                     .ThenInclude(w => w.Setting)

                                     .AsSplitQuery()

                                     .Where(t => t.EOB == null
                                              && t.BilledDate == null
                                              && t.TCMNoteActivity.Where(n => n.Billable == true).Count() > 0
                                              && t.Status == status)
                                              
                                     .ToList();
        }
        else
        {
            if (idStatus == 3)
            {
                workday_Client = await _context.Workdays_Clients

                                               .Include(wc => wc.Facilitator)
                                               .Include(c => c.Client)
                                               .ThenInclude(w => w.Clinic)
                                               .ThenInclude(w => w.Setting)
                                               .Include(w => w.Workday)

                                               .Include(wc => wc.Client)
                                               .ThenInclude(w => w.Clients_Diagnostics)
                                               .ThenInclude(w => w.Diagnostic)

                                               .Include(wc => wc.Client)
                                               .ThenInclude(w => w.Clients_HealthInsurances)
                                               .ThenInclude(w => w.HealthInsurance)

                                               .Include(wc => wc.Note)
                                               .ThenInclude(wc => wc.Supervisor)

                                               .Include(wc => wc.NoteP)
                                               .ThenInclude(w => w.NotesP_Activities)

                                               .Include(wc => wc.NoteP)
                                               .ThenInclude(wc => wc.Supervisor)

                                               .Include(wc => wc.IndividualNote)
                                               .ThenInclude(wc => wc.Supervisor)

                                               .Include(wc => wc.GroupNote)
                                               .ThenInclude(w => w.GroupNotes_Activities)

                                               .Include(wc => wc.GroupNote)
                                               .ThenInclude(w => w.Supervisor)

                                               .Include(wc => wc.GroupNote2)
                                               .ThenInclude(w => w.GroupNotes2_Activities)

                                               .Include(wc => wc.GroupNote2)
                                               .ThenInclude(w => w.Supervisor)

                                               .AsSplitQuery()

                                                .Where(wc => wc.Present == true
                                                            && wc.Client != null
                                                            && wc.BilledDate == null)
                                                .OrderBy(wc => wc.Client.Name)
                                                .ThenBy(wc => wc.Workday.Date)
                                                .ToListAsync();

                tcmNotesUnBill = _context.TCMNote
                                         .Include(t => t.TCMClient)
                                         .ThenInclude(c => c.Client)
                                         .ThenInclude(cl => cl.Clients_Diagnostics)
                                         .ThenInclude(cd => cd.Diagnostic)

                                         .Include(t => t.TCMClient)
                                         .ThenInclude(c => c.Client)
                                         .ThenInclude(cl => cl.Clients_HealthInsurances)
                                         .ThenInclude(cd => cd.HealthInsurance)

                                         .Include(t => t.TCMNoteActivity)

                                         .Include(t => t.TCMClient)
                                         .ThenInclude(t => t.Casemanager)
                                         .ThenInclude(t => t.TCMSupervisor)
                                         
                                         .Include(t => t.TCMClient)
                                         .ThenInclude(c => c.Client)
                                         .ThenInclude(w => w.Clinic)
                                         .ThenInclude(w => w.Setting)

                                         .AsSplitQuery()

                                         .Where(t => t.EOB == null
                                                  && t.BilledDate == null
                                                  && t.TCMNoteActivity.Where(n => n.Billable == true).Count() > 0)

                                         .ToList();
            }
            else
            {
                if (idStatus == 4) //only MH
                {
                    workday_Client = await _context.Workdays_Clients

                                            .Include(wc => wc.Facilitator)
                                            .Include(c => c.Client)
                                            .ThenInclude(w => w.Clinic)
                                            .ThenInclude(w => w.Setting)
                                            .Include(w => w.Workday)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_Diagnostics)
                                            .ThenInclude(w => w.Diagnostic)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_HealthInsurances)
                                            .ThenInclude(w => w.HealthInsurance)

                                            .Include(wc => wc.Note)
                                            .ThenInclude(w => w.Supervisor)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(w => w.NotesP_Activities)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(w => w.Supervisor)

                                            .Include(wc => wc.IndividualNote)
                                            .ThenInclude(w => w.Supervisor)

                                            .Include(wc => wc.GroupNote)
                                            .ThenInclude(w => w.GroupNotes_Activities)

                                            .Include(wc => wc.GroupNote)
                                            .ThenInclude(w => w.Supervisor)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(w => w.GroupNotes2_Activities)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(w => w.Supervisor)

                                            .AsSplitQuery()

                                            .Where(wc => wc.Present == true
                                                        && wc.Client != null
                                                        && wc.BilledDate == null
                                                        && (wc.Note == null && wc.NoteP == null && wc.IndividualNote == null
                                                                             && wc.GroupNote == null && wc.GroupNote2 == null))
                                            .OrderBy(wc => wc.Client.Name)
                                            .ThenBy(wc => wc.Workday.Date)
                                            .ToListAsync();
                }
            }
        }

        List<NotBilled> notBilleds = new List<NotBilled>();
        string insuranceMemberId = string.Empty;
        string service;
        string setting;
        int units;
        decimal amount;
        string supervisor;
        foreach (var item in workday_Client)
        {
            insuranceMemberId = "-";
            if (item.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
            {
                insuranceMemberId = $"{item.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name} | {item.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId}";
            }

            if (item.Note != null)
            {
                service = "PSR";
                setting = item.Note.Setting;
                units = 16;
                amount = 16 * item.Client.Clinic.Setting.PricePSR;
                supervisor = (item.Note.Supervisor != null)? item.Note.Supervisor.Name : string.Empty;
            }
            else
            {
                if (item.NoteP != null)
                {
                    service = "PSR";
                    setting = item.NoteP.Setting;
                    units = item.NoteP.RealUnits;
                    amount = item.NoteP.RealUnits * item.Client.Clinic.Setting.PricePSR;
                    supervisor = (item.NoteP.Supervisor != null) ? item.NoteP.Supervisor.Name : string.Empty;
                }
                else
                {
                    if (item.IndividualNote != null)
                    {
                        service = "Ind.";
                        setting = "";
                        units = item.IndividualNote.RealUnits;
                        amount = item.IndividualNote.RealUnits * item.Client.Clinic.Setting.PriceInd;
                        supervisor = (item.IndividualNote.Supervisor != null) ? item.IndividualNote.Supervisor.Name : string.Empty;
                    }
                    else
                    {
                        if (item.GroupNote != null)
                        {
                            service = "Group";
                            setting = "";
                            units = 8;
                            amount = 8 * item.Client.Clinic.Setting.PriceGroup;
                            supervisor = (item.GroupNote.Supervisor != null) ? item.GroupNote.Supervisor.Name : string.Empty;
                        }
                        else
                        {
                            if (item.GroupNote2 != null)
                            {
                                service = "Group";
                                setting = "";
                                units = item.GroupNote2.GroupNotes2_Activities.Count();
                                amount = item.GroupNote2.GroupNotes2_Activities.Count() * item.Client.Clinic.Setting.PriceGroup;
                                supervisor = (item.GroupNote2.Supervisor != null) ? item.GroupNote2.Supervisor.Name : string.Empty;
                            }
                            else
                            {
                                service = "PSR";
                                setting = "53";
                                units = 16;
                                amount = 16 * item.Client.Clinic.Setting.PricePSR;
                                supervisor = string.Empty;
                            }
                        }
                    }
                }

            }

            NotBilled value = new NotBilled(item.Client.Name, item.Client.Code, item.Client.DateOfBirth.ToShortDateString(), item.Client.MedicaidID,
                                            insuranceMemberId, (item.Client.Clients_Diagnostics.Count() > 0) ? item.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code : "-",
                                            item.Workday.Date.ToShortDateString(), service, setting, units, amount, supervisor, "Not Billed");

            notBilleds.Add(value);
        }

        int Unit = 0;
       // int UnitTotal = 0;
        decimal amountTCM = 0.00m;
       // decimal amountTotal = 0.00m;

        int minutesTCM = 0;
        int residuoTCM = 0;
        int unitTCM = 0;
        //List<int> idTCMs = new List<int>();
        foreach (var item in tcmNotesUnBill)
        {
            minutesTCM = item.TCMNoteActivity.Sum(n => n.Minutes);
            unitTCM = minutesTCM / 15;
            residuoTCM = minutesTCM % 15;
            if (residuoTCM > 7)
            {
                Unit = unitTCM + 1;
                amountTCM = (decimal)(unitTCM + 1) * item.TCMClient.Client.Clinic.Setting.PriceTCM;
            }
            else
            {
                Unit = unitTCM;
                amountTCM = (decimal)(unitTCM * item.TCMClient.Client.Clinic.Setting.PriceTCM);
            }

           // if (idTCMs.Contains(item.TCMClient.Casemanager.Id) == false)
           // {
           //     idTCMs.Add(item.TCMClient.Casemanager.Id);
           // }
           // UnitTotal += Unit;
           // amountTotal += amountTCM;

            NotBilled value = new NotBilled(item.TCMClient.Client.Name, item.TCMClient.Client.Code, item.TCMClient.Client.DateOfBirth.ToShortDateString(), item.TCMClient.Client.MedicaidID,
                                           insuranceMemberId, (item.TCMClient.Client.Clients_Diagnostics.Count() > 0) ? item.TCMClient.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code : "-",
                                           item.DateOfService.ToShortDateString(), "TCM", item.TCMNoteActivity.FirstOrDefault().Setting, Unit, amountTCM, item.TCMClient.Casemanager.TCMSupervisor.Name, "Not Billed");

            notBilleds.Add(value);

        }

        return Ok(notBilleds);
    }
}

