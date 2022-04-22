using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IUserHelper _userHelper;

        public ConverterHelper(DataContext context, ICombosHelper combosHelper, IUserHelper userHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _userHelper = userHelper;
        }

        public ClinicEntity ToClinicEntity(ClinicViewModel model, string path, bool isNew)
        {
            return new ClinicEntity
            {
                Id = isNew ? 0 : model.Id,
                LogoPath = path,
                Name = model.Name,
                CEO = model.CEO,
                Address = model.Address,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Phone = model.Phone,
                FaxNo = model.FaxNo
            };
        }

        public ClinicViewModel ToClinicViewModel(ClinicEntity clinicEntity)
        {
            return new ClinicViewModel
            {
                Id = clinicEntity.Id,
                LogoPath = clinicEntity.LogoPath,
                Name = clinicEntity.Name,
                CEO = clinicEntity.CEO,
                Address = clinicEntity.Address,
                City = clinicEntity.City,
                State = clinicEntity.State,
                ZipCode = clinicEntity.ZipCode,
                Phone = clinicEntity.Phone,
                FaxNo = clinicEntity.FaxNo
            };
        }

        public async Task<ThemeEntity> ToThemeEntity(ThemeViewModel model, bool isNew)
        {
            return new ThemeEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Day = (model.DayId == 1) ? DayOfWeekType.Monday : (model.DayId == 2) ? DayOfWeekType.Tuesday :
                                    (model.DayId == 3) ? DayOfWeekType.Wednesday : (model.DayId == 4) ? DayOfWeekType.Thursday :
                                    (model.DayId == 5) ? DayOfWeekType.Friday : DayOfWeekType.Monday,
                Name = model.Name
            };
        }

        public async Task<ThemeEntity> ToTheme3Entity(Theme3ViewModel model, bool isNew)
        {
            return new ThemeEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),                
                Name = model.Name
            };
        }

        public ThemeViewModel ToThemeViewModel(ThemeEntity themeEntity)
        {
            return new ThemeViewModel
            {
                Id = themeEntity.Id,
                Name = themeEntity.Name,
                Day = themeEntity.Day,
                Days = _combosHelper.GetComboDays(),
                DayId = Convert.ToInt32(themeEntity.Day) + 1,
                IdClinic = themeEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics()
            };
        }

        public Theme3ViewModel ToTheme3ViewModel(ThemeEntity themeEntity)
        {
            return new Theme3ViewModel
            {
                Id = themeEntity.Id,
                Name = themeEntity.Name,                
                IdClinic = themeEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics()
            };
        }

        public async Task<ActivityEntity> ToActivityEntity(ActivityViewModel model, bool isNew)
        {
            return new ActivityEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Status = ActivityStatus.Pending,
                Theme = await _context.Themes.FindAsync(model.IdTheme)
            };
        }

        public ActivityViewModel ToActivityViewModel(ActivityEntity activityEntity)
        {
            return new ActivityViewModel
            {
                Id = activityEntity.Id,
                Name = activityEntity.Name,
                Themes = _combosHelper.GetComboThemes(),
                IdTheme = activityEntity.Theme.Id
            };
        }

        public async Task<NotePrototypeEntity> ToNotePrototypeEntity(NotePrototypeViewModel model, bool isNew)
        {
            return new NotePrototypeEntity
            {
                Id = isNew ? 0 : model.Id,
                Activity = await _context.Activities.FindAsync(model.IdActivity),
                AnswerClient = model.AnswerClient,
                AnswerFacilitator = model.AnswerFacilitator
            };
        }

        public NotePrototypeViewModel ToNotePrototypeViewModel(NotePrototypeEntity noteEntity)
        {
            return new NotePrototypeViewModel
            {
                Id = noteEntity.Id,
                AnswerClient = noteEntity.AnswerClient,
                AnswerFacilitator = noteEntity.AnswerFacilitator,
                IdActivity = noteEntity.Activity.Id,
                Activities = _combosHelper.GetComboActivities(),
                Classifications = noteEntity.Classifications,
                Clients = _combosHelper.GetComboClients(),
                Facilitators = _combosHelper.GetComboFacilitators()
            };
        }

        public async Task<FacilitatorEntity> ToFacilitatorEntity(FacilitatorViewModel model, string signaturePath, bool isNew)
        {
            return new FacilitatorEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Codigo = model.Codigo,
                Name = model.Name,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                SignaturePath = signaturePath
            };
        }

        public async Task<CaseMannagerEntity> ToCaseMannagerEntity(CaseMannagerViewModel model, string signaturePath, bool isNew)
        {
            return new CaseMannagerEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Codigo = model.Codigo,
                Name = model.Name,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                SignaturePath = signaturePath
            };
        }

        public FacilitatorViewModel ToFacilitatorViewModel(FacilitatorEntity facilitatorEntity, int idClinic)
        {
            return new FacilitatorViewModel
            {
                Id = facilitatorEntity.Id,
                Name = facilitatorEntity.Name,
                Codigo = facilitatorEntity.Codigo,
                IdClinic = facilitatorEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                IdStatus = (facilitatorEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus(),
                IdUser = _userHelper.GetIdByUserName(facilitatorEntity.LinkedUser),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, idClinic),
                SignaturePath = facilitatorEntity.SignaturePath
            };
        }

        public CaseMannagerViewModel ToCaseMannagerViewModel(CaseMannagerEntity caseMannagerEntity, int idClinic)
        {
            return new CaseMannagerViewModel
            {
                Id = caseMannagerEntity.Id,
                Name = caseMannagerEntity.Name,
                Codigo = caseMannagerEntity.Codigo,
                IdClinic = caseMannagerEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                IdStatus = (caseMannagerEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus(),
                IdUser = _userHelper.GetIdByUserName(caseMannagerEntity.LinkedUser),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, idClinic),
                SignaturePath = caseMannagerEntity.SignaturePath
            };
        }

        public async Task<ClientEntity> ToClientEntity(ClientViewModel model, bool isNew, string photoPath, string signPath, string userId)
        {
            return new ClientEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Gender = GenderUtils.GetGenderByIndex(model.IdGender),
                DateOfBirth = model.DateOfBirth,
                AdmisionDate = model.AdmisionDate,
                PlaceOfBirth = model.PlaceOfBirth,
                Code = model.Code,
                MedicaidID = model.MedicaidID,
                Clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == model.IdClinic),
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                Email = model.Email,
                Telephone = model.Telephone,
                TelephoneSecondary = model.TelephoneSecondary,
                SSN = model.SSN,
                FullAddress = model.FullAddress,
                AlternativeAddress = model.AlternativeAddress,
                Country = model.Country,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Race = RaceUtils.GetRaceByIndex(model.IdRace),
                MaritalStatus = MaritalUtils.GetMaritalByIndex(model.IdMaritalStatus),
                Ethnicity = EthnicityUtils.GetEthnicityByIndex(model.IdEthnicity),
                PreferredLanguage = PreferredLanguageUtils.GetPreferredLanguageByIndex(model.IdPreferredLanguage),
                OtherLanguage = model.OtherLanguage,
                PhotoPath = photoPath,
                SignPath = signPath,
                Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == model.IdDoctor),
                Psychiatrist = await _context.Psychiatrists.FirstOrDefaultAsync(p => p.Id == model.IdPsychiatrist),
                Referred = await _context.Referreds.FirstOrDefaultAsync(r => r.Id == model.IdReferred),
                LegalGuardian = await _context.LegalGuardians.FirstOrDefaultAsync(lg => lg.Id == model.IdLegalGuardian),
                EmergencyContact = await _context.EmergencyContacts.FirstOrDefaultAsync(ec => ec.Id == model.IdEmergencyContact),
                RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(model.IdRelationshipEC),
                RelationShipOfLegalGuardian = RelationshipUtils.GetRelationshipByIndex(model.IdRelationship),
                IndividualTherapyFacilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.Id == model.IdFacilitatorIT),
                Service = isNew ? ServiceType.PSR : ServiceUtils.GetServiceByIndex(model.IdService),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public async Task<ClientViewModel> ToClientViewModel(ClientEntity clientEntity, string userId)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.Id == userId);

            return new ClientViewModel
            {
                Id = clientEntity.Id,
                Name = clientEntity.Name,
                Code = clientEntity.Code,
                MedicaidID = clientEntity.MedicaidID,
                DateOfBirth = clientEntity.DateOfBirth,
                AdmisionDate = clientEntity.AdmisionDate,
                PlaceOfBirth = clientEntity.PlaceOfBirth,
                IdStatus = (clientEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus(),
                IdClinic = (clientEntity.Clinic != null) ? clientEntity.Clinic.Id : 0,
                Clinics = _combosHelper.GetComboClinics(),
                IdGender = (clientEntity.Gender == GenderType.Female) ? 1 : 2,
                GenderList = _combosHelper.GetComboGender(),
                CreatedBy = clientEntity.CreatedBy,
                CreatedOn = clientEntity.CreatedOn,
                LastModifiedBy = clientEntity.LastModifiedBy,
                LastModifiedOn = clientEntity.LastModifiedOn,
                Email = clientEntity.Email,
                Telephone = clientEntity.Telephone,
                TelephoneSecondary = clientEntity.TelephoneSecondary,
                SSN = clientEntity.SSN,
                FullAddress = clientEntity.FullAddress,
                AlternativeAddress = clientEntity.AlternativeAddress,
                Country = clientEntity.Country,
                City = clientEntity.City,
                State = clientEntity.State,
                ZipCode = clientEntity.ZipCode,
                IdRace = Convert.ToInt32(clientEntity.Race),
                Races = _combosHelper.GetComboRaces(),
                IdMaritalStatus = Convert.ToInt32(clientEntity.MaritalStatus),
                Maritals = _combosHelper.GetComboMaritals(),
                IdEthnicity = Convert.ToInt32(clientEntity.Ethnicity),
                Ethnicities = _combosHelper.GetComboEthnicities(),
                IdPreferredLanguage = Convert.ToInt32(clientEntity.PreferredLanguage),
                Languages = _combosHelper.GetComboLanguages(),
                OtherLanguage = clientEntity.OtherLanguage,
                PhotoPath = clientEntity.PhotoPath,
                SignPath = clientEntity.SignPath,
                IdRelationship = Convert.ToInt32(clientEntity.RelationShipOfLegalGuardian),
                Relationships = _combosHelper.GetComboRelationships(),
                IdRelationshipEC = Convert.ToInt32(clientEntity.RelationShipOfEmergencyContact),
                RelationshipsEC = _combosHelper.GetComboRelationships(),
                IdReferred = (clientEntity.Referred != null) ? clientEntity.Referred.Id : 0,
                Referreds = _combosHelper.GetComboReferredsByClinic(userId),                
                IdEmergencyContact = (clientEntity.EmergencyContact != null) ? clientEntity.EmergencyContact.Id : 0,
                EmergencyContacts = _combosHelper.GetComboEmergencyContactsByClinic(userId),
                IdDoctor = (clientEntity.Doctor != null) ? clientEntity.Doctor.Id : 0,
                Doctors = _combosHelper.GetComboDoctorsByClinic(userId),
                IdPsychiatrist = (clientEntity.Psychiatrist != null) ? clientEntity.Psychiatrist.Id : 0,
                Psychiatrists = _combosHelper.GetComboPsychiatristsByClinic(userId),
                IdLegalGuardian = (clientEntity.LegalGuardian != null) ? clientEntity.LegalGuardian.Id : 0,
                LegalsGuardians = _combosHelper.GetComboLegalGuardiansByClinic(userId),
                DiagnosticTemp = _context.DiagnosticsTemp,
                DocumentTemp = _context.DocumentsTemp,
                IdService = Convert.ToInt32(clientEntity.Service),
                Services = _combosHelper.GetComboServices(),
                IdFacilitatorIT = (clientEntity.IndividualTherapyFacilitator != null) ? clientEntity.IndividualTherapyFacilitator.Id : 0,
                ITFacilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, true)
            };
        }

        public async Task<SupervisorEntity> ToSupervisorEntity(SupervisorViewModel model, string signaturePath, bool isNew)
        {
            return new SupervisorEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Code = model.Code,
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                Name = model.Name,
                SignaturePath = signaturePath
            };
        }

        public SupervisorViewModel ToSupervisorViewModel(SupervisorEntity supervisorEntity, int idClinic)
        {
            return new SupervisorViewModel
            {
                Id = supervisorEntity.Id,
                Name = supervisorEntity.Name,
                Code = supervisorEntity.Code,
                IdClinic = supervisorEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                IdUser = _userHelper.GetIdByUserName(supervisorEntity.LinkedUser),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, idClinic),
                SignaturePath = supervisorEntity.SignaturePath,
            };
        }

        public async Task<TCMSupervisorEntity> ToTCMsupervisorEntity(TCMSupervisorViewModel model, string signaturePath, bool isNew)
        {
            return new TCMSupervisorEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Code = model.Code,
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                Name = model.Name,
                SignaturePath = signaturePath,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
            };
        }

        public TCMSupervisorViewModel ToTCMsupervisorViewModel(TCMSupervisorEntity TCMSupervisorEntity, int idClinic)
        {
            return new TCMSupervisorViewModel
            {
                Id = TCMSupervisorEntity.Id,
                Name = TCMSupervisorEntity.Name,
                Code = TCMSupervisorEntity.Code,
                IdClinic = TCMSupervisorEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                IdUser = _userHelper.GetIdByUserName(TCMSupervisorEntity.LinkedUser),
                LinkedUser = TCMSupervisorEntity.LinkedUser,
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, idClinic),
                SignaturePath = TCMSupervisorEntity.SignaturePath,
                IdStatus = (TCMSupervisorEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus()
            };
        }

        public async Task<MTPEntity> ToMTPEntity(MTPViewModel model, bool isNew)
        {
            return new MTPEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FindAsync(model.IdClient),                
                MTPDevelopedDate = model.MTPDevelopedDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                LevelCare = model.LevelCare,
                InitialDischargeCriteria = model.InitialDischargeCriteria,
                Modality = model.Modality,
                Frecuency = model.Frecuency,
                NumberOfMonths = model.NumberOfMonths,
                Active = isNew ? true : model.Active
            };
        }

        public MTPViewModel ToMTPViewModel(MTPEntity mtpEntity)
        {
            return new MTPViewModel
            {
                Id = mtpEntity.Id,
                IdClient = mtpEntity.Client.Id,
                Clients = _combosHelper.GetComboClients(),               
                MTPDevelopedDate = mtpEntity.MTPDevelopedDate,
                StartTime = mtpEntity.StartTime,
                EndTime = mtpEntity.EndTime,
                LevelCare = mtpEntity.LevelCare,
                InitialDischargeCriteria = mtpEntity.InitialDischargeCriteria,
                Modality = mtpEntity.Modality,
                Frecuency = mtpEntity.Frecuency,
                NumberOfMonths = mtpEntity.NumberOfMonths,                
                Setting = mtpEntity.Setting,
                Active = mtpEntity.Active
            };
        }        

        public async Task<GoalEntity> ToGoalEntity(GoalViewModel model, bool isNew)
        {
            return new GoalEntity
            {
                Id = isNew ? 0 : model.Id,
                Number = model.Number,
                Name = model.Name,
                AreaOfFocus = model.AreaOfFocus,
                MTP = await _context.MTPs.FindAsync(model.IdMTP),
                Service = ServiceUtils.GetServiceByIndex(model.IdService)
            };
        }

        public GoalViewModel ToGoalViewModel(GoalEntity goalEntity)
        {
            return new GoalViewModel
            {
                Id = goalEntity.Id,
                Number = goalEntity.Number,
                MTP = goalEntity.MTP,
                IdMTP = goalEntity.MTP.Id,
                Name = goalEntity.Name,
                AreaOfFocus = goalEntity.AreaOfFocus,
                IdService = Convert.ToInt32(goalEntity.Service),
                Services = _combosHelper.GetComboServices()
            };
        }

        public async Task<ObjetiveEntity> ToObjectiveEntity(ObjectiveViewModel model, bool isNew)
        {
            return new ObjetiveEntity
            {
                Id = isNew ? 0 : model.Id,
                Objetive = model.Objetive,
                DateOpened = model.DateOpened,
                DateTarget = model.DateTarget,
                DateResolved = model.DateResolved,
                Description = model.Description,
                Intervention = model.Intervention,
                Goal = await _context.Goals.FindAsync(model.IdGoal)
            };
        }

        public ObjectiveViewModel ToObjectiveViewModel(ObjetiveEntity objectiveEntity)
        {
            return new ObjectiveViewModel
            {
                Id = objectiveEntity.Id,
                Objetive = objectiveEntity.Objetive,
                Goal = objectiveEntity.Goal,
                IdGoal = objectiveEntity.Goal.Id,
                DateOpened = objectiveEntity.DateOpened,
                DateResolved = objectiveEntity.DateResolved,
                DateTarget = objectiveEntity.DateTarget,
                Description = objectiveEntity.Description,
                Classifications = objectiveEntity.Classifications,
                Intervention = objectiveEntity.Intervention
            };
        }

        public async Task<GroupEntity> ToGroupEntity(GroupViewModel model, bool isNew)
        {
            return new GroupEntity
            {
                Id = isNew ? 0 : model.Id,
                Am = model.Am,
                Pm = model.Pm,
                Facilitator = await _context.Facilitators.FindAsync(model.IdFacilitator),
                Service = model.Service
            };
        }

        public GroupViewModel ToGroupViewModel(GroupEntity groupEntity)
        {
            return new GroupViewModel
            {
                Id = groupEntity.Id,
                Facilitator = groupEntity.Facilitator,
                IdFacilitator = groupEntity.Facilitator.Id,
                Facilitators = _combosHelper.GetComboFacilitators(),
                Am = groupEntity.Am,
                Pm = groupEntity.Pm,
                Clients = groupEntity.Clients
            };
        }

        public async Task<PlanEntity> ToPlanEntity(PlanViewModel model, bool isNew)
        {
            return new PlanEntity
            {

            };
        }

        public PlanViewModel ToPlanViewModel(PlanEntity planEntity)
        {
            return new PlanViewModel
            {
                Id = planEntity.Id,
                Text = planEntity.Text,
                Classifications = planEntity.Classifications
            };
        }

        public async Task<NoteEntity> ToNoteEntity(NoteViewModel model, bool isNew)
        {
            NoteEntity entity = await _context.Notes.FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
            return new NoteEntity
            {
                Id = isNew ? 0 : entity.Id,
                Workday_Cient = await _context.Workdays_Clients.FindAsync(model.Id),
                PlanNote = model.PlanNote,
                Status = NoteStatus.Edition,
                OrientedX3 = model.OrientedX3,
                NotTime = model.NotTime,
                NotPlace = model.NotPlace,
                NotPerson = model.NotPerson,
                Present = model.Present,
                Adequate = model.Adequate,
                Limited = model.Limited,
                Impaired = model.Impaired,
                Faulty = model.Faulty,
                Euthymic = model.Euthymic,
                Congruent = model.Congruent,
                Negativistic = model.Negativistic,
                Depressed = model.Depressed,
                Euphoric = model.Euphoric,
                Optimistic = model.Optimistic,
                Anxious = model.Anxious,
                Hostile = model.Hostile,
                Withdrawn = model.Withdrawn,
                Irritable = model.Irritable,
                Dramatized = model.Dramatized,
                AdequateAC = model.AdequateAC,
                Inadequate = model.Inadequate,
                Fair = model.Fair,
                Unmotivated = model.Unmotivated,
                Motivated = model.Motivated,
                Guarded = model.Guarded,
                Normal = model.Normal,
                ShortSpanned = model.ShortSpanned,
                MildlyImpaired = model.MildlyImpaired,
                SeverelyImpaired = model.SeverelyImpaired,
                Schema = model.Schema
            };
        }

        public NoteViewModel ToNoteViewModel(NoteEntity model)
        {
            return new NoteViewModel
            {
                Id = model.Id,
                PlanNote = model.PlanNote,
                Status = model.Status,
                Workday_Cient = model.Workday_Cient
            };
        }

        public async Task<NotePEntity> ToNotePEntity(NotePViewModel model, bool isNew)
        {
            NotePEntity entity = await _context.NotesP.FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
            return new NotePEntity
            {
                Id = isNew ? 0 : entity.Id,
                Workday_Cient = await _context.Workdays_Clients.FindAsync(model.Id),
                PlanNote = model.PlanNote,
                Status = NoteStatus.Edition,
                Title = model.Title,

                //mental client status
                Attentive = model.Attentive,
                Depressed = model.Depressed,
                Inattentive = model.Inattentive,
                Angry = model.Angry,
                Sad = model.Sad,
                FlatAffect = model.FlatAffect,
                Anxious = model.Anxious,
                PositiveEffect = model.PositiveEffect,
                Oriented1x = model.Oriented1x,
                Oriented2x = model.Oriented2x,
                Oriented3x = model.Oriented3x,
                Impulsive = model.Impulsive,
                Labile = model.Labile,
                Withdrawn = model.Withdrawn,
                RelatesWell = model.RelatesWell,
                DecreasedEyeContact = model.DecreasedEyeContact,
                AppropiateEyeContact = model.AppropiateEyeContact,

                //progress
                Minimal = model.Minimal,
                Slow = model.Slow,
                Steady = model.Steady,
                GoodExcelent = model.GoodExcelent,
                IncreasedDifficultiesNoted = model.IncreasedDifficultiesNoted,
                Complicated = model.Complicated,
                DevelopingInsight = model.DevelopingInsight,
                LittleInsight = model.LittleInsight,
                Aware = model.Aware,
                AbleToGenerateAlternatives = model.AbleToGenerateAlternatives,
                Initiates = model.Initiates,
                ProblemSolved = model.ProblemSolved,
                DemostratesEmpathy = model.DemostratesEmpathy,
                UsesSessions = model.UsesSessions,
                Variable = model.Variable,

                Schema = model.Schema
            };
        }

        public NotePViewModel ToNotePViewModel(NotePEntity model)
        {
            throw new NotImplementedException();
        }

        public async Task<IndividualNoteEntity> ToIndividualNoteEntity(IndividualNoteViewModel model, bool isNew)
        {
            IndividualNoteEntity entity = await _context.IndividualNotes.FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
            return new IndividualNoteEntity
            {
                Id = isNew ? 0 : entity.Id,
                Workday_Cient = await _context.Workdays_Clients.FindAsync(model.Id),
                PlanNote = model.PlanNote,
                Status = NoteStatus.Edition,
                SubjectiveData = model.SubjectiveData,
                ObjectiveData = model.ObjectiveData,
                Assessment = model.Assessment,
                Objective = (model.IdObjetive1 != 0) ? await _context.Objetives.FirstOrDefaultAsync(o => o.Id == model.IdObjetive1) : null,
                Groomed = model.Groomed,
                Unkempt = model.Unkempt,
                Disheveled = model.Disheveled,
                Meticulous = model.Meticulous,
                Overbuild = model.Overbuild,
                Other = model.Other,
                Clear = model.Clear,
                Pressured = model.Pressured,
                Slurred = model.Slurred,
                Slow = model.Slow,
                Impaired = model.Impaired,
                Poverty = model.Poverty,
                Euthymic = model.Euthymic,
                Depressed = model.Depressed,
                Anxious = model.Anxious,
                Fearful = model.Fearful,
                Irritable = model.Irritable,
                Labile = model.Labile,
                WNL = model.WNL,
                Guarded = model.Guarded,
                Withdrawn = model.Withdrawn,
                Hostile = model.Hostile,
                Restless = model.Restless,
                Impulsive = model.Impulsive,
                WNL_Cognition = model.WNL_Cognition,
                Blocked = model.Blocked,
                Obsessive = model.Obsessive,
                Paranoid = model.Paranoid,
                Scattered = model.Scattered,
                Psychotic = model.Psychotic,           
                CBT = model.CBT,
                Psychodynamic = model.Psychodynamic,
                BehaviorModification = model.BehaviorModification,
                Other_Intervention = model.Other_Intervention
            };
        }

        public async Task<GroupNoteEntity> ToGroupNoteEntity(GroupNoteViewModel model, bool isNew)
        {
            GroupNoteEntity entity = await _context.GroupNotes.FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
            return new GroupNoteEntity
            {
                Id = isNew ? 0 : entity.Id,
                Workday_Cient = await _context.Workdays_Clients.FindAsync(model.Id),
                PlanNote = model.PlanNote,
                Status = NoteStatus.Edition,
                Groomed = model.Groomed,
                Unkempt = model.Unkempt,
                Disheveled = model.Disheveled,
                Meticulous = model.Meticulous,
                Overbuild = model.Overbuild,
                Other = model.Other,
                Clear = model.Clear,
                Pressured = model.Pressured,
                Slurred = model.Slurred,
                Slow = model.Slow,
                Impaired = model.Impaired,
                Poverty = model.Poverty,
                Euthymic = model.Euthymic,
                Depressed = model.Depressed,
                Anxious = model.Anxious,
                Fearful = model.Fearful,
                Irritable = model.Irritable,
                Labile = model.Labile,
                WNL = model.WNL,
                Guarded = model.Guarded,
                Withdrawn = model.Withdrawn,
                Hostile = model.Hostile,
                Restless = model.Restless,
                Impulsive = model.Impulsive,
                WNL_Cognition = model.WNL_Cognition,
                Blocked = model.Blocked,
                Obsessive = model.Obsessive,
                Paranoid = model.Paranoid,
                Scattered = model.Scattered,
                Psychotic = model.Psychotic,
                CBT = model.CBT,
                Psychodynamic = model.Psychodynamic,
                BehaviorModification = model.BehaviorModification,
                Other_Intervention = model.Other_Intervention
            };
        }        

        public Workday_ClientViewModel ToWorkdayClientViewModel(Workday_Client model)
        {
            return new Workday_ClientViewModel
            {
                Id = model.Id,
                Workday = model.Workday,
                Client = (model.Client != null) ? model.Client : null,
                Facilitator = model.Facilitator,
                Session = model.Session,
                Present = model.Present,
                Note = model.Note,
                IndividualNote = model.IndividualNote
            };
        }

        public async Task<MessageEntity> ToMessageEntity(MessageViewModel model, bool isNew)
        {

            return new MessageEntity
            {
                Id = isNew ? 0 : model.Id,
                Workday_Client = await _context.Workdays_Clients
                                               .Include(wc => wc.Facilitator)
                                               .FirstOrDefaultAsync(wc => wc.Id == model.IdWorkdayClient),
                Title = model.Title,
                Text = model.Text,
                DateCreated = DateTime.Now,
                Status = MessageStatus.NotRead
            };
        }

        public DoctorEntity ToDoctorEntity(DoctorViewModel model, bool isNew, string userId)
        {
            return new DoctorEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public PsychiatristEntity ToPsychiatristEntity(PsychiatristViewModel model, bool isNew, string userId)
        {
            return new PsychiatristEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public LegalGuardianEntity ToLegalGuardianEntity(LegalGuardianViewModel model, bool isNew, string userId)
        {
            return new LegalGuardianEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Country = model.Country,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                TelephoneSecondary = model.TelephoneSecondary,
                AdressLine2 = model.AdressLine2
            };
        }

        public EmergencyContactEntity ToEmergencyContactEntity(EmergencyContactViewModel model, bool isNew, string userId)
        {
            return new EmergencyContactEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Country = model.Country,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                TelephoneSecondary = model.TelephoneSecondary,
                AdressLine2 = model.AdressLine2
            };
        }

        public ReferredEntity ToReferredEntity(ReferredViewModel model, bool isNew, string userId)
        {
            return new ReferredEntity
            {
                Id = isNew ? 0 : model.Id,
                ReferredNote = model.ReferredNote,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public DiagnosticEntity ToDiagnosticEntity(DiagnosticViewModel model, bool isNew, string userId)
        {
            return new DiagnosticEntity
            {
                Id = isNew ? 0 : model.Id,
                Code = model.Code,
                Description = model.Description,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public DoctorViewModel ToDoctorViewModel(DoctorEntity model)
        {
            return new DoctorViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn
            };
        }

        public PsychiatristViewModel ToPsychiatristViewModel(PsychiatristEntity model)
        {
            return new PsychiatristViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn
            };
        }

        public ReferredViewModel ToReferredViewModel(ReferredEntity model)
        {
            return new ReferredViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                ReferredNote = model.ReferredNote,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn
            };
        }

        public LegalGuardianViewModel ToLegalGuardianViewModel(LegalGuardianEntity model)
        {
            return new LegalGuardianViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Country = model.Country,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                TelephoneSecondary = model.TelephoneSecondary,
                AdressLine2 = model.AdressLine2
            };
        }

        public EmergencyContactViewModel ToEmergencyContactViewModel(EmergencyContactEntity model)
        {
            return new EmergencyContactViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Country = model.Country,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                TelephoneSecondary = model.TelephoneSecondary,
                AdressLine2 = model.AdressLine2
            };
        }

        public DiagnosticViewModel ToDiagnosticViewModel(DiagnosticEntity model)
        {
            return new DiagnosticViewModel
            {
                Id = model.Id,
                Code = model.Code,
                Description = model.Description,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public async Task<IncidentEntity> ToIncidentEntity(IncidentViewModel model, bool isNew, string userId)
        {
            return new IncidentEntity
            {
                Id = isNew ? 0 : model.Id,
                Description = model.Description,
                CreatedDate = isNew ? DateTime.Now : model.CreatedDate,
                Status = isNew ? IncidentsStatus.Pending : StatusUtils.GetIncidentStatusByIndex(model.IdStatus),
                UserCreatedBy = isNew ? await _context.Users.FindAsync(userId) : await _context.Users.FindAsync(model.IdUserCreatedBy),
                SolvedBy = isNew ? string.Empty : (StatusUtils.GetIncidentStatusByIndex(model.IdStatus) == IncidentsStatus.Solved) ? userId : string.Empty,
                SolvedDate = !isNew && (StatusUtils.GetIncidentStatusByIndex(model.IdStatus) == IncidentsStatus.Solved) ? DateTime.Now : model.SolvedDate
            };
        }

        public IncidentViewModel ToIncidentViewModel(IncidentEntity model)
        {
            return new IncidentViewModel
            {
                Id = model.Id,
                Description = model.Description,
                CreatedDate = model.CreatedDate,
                IdStatus = (model.Status == IncidentsStatus.Pending) ? 0 : (model.Status == IncidentsStatus.Solved) ? 1 : (model.Status == IncidentsStatus.NotValid) ? 2 : 0,
                StatusList = _combosHelper.GetComboIncidentsStatus(),
                IdUserCreatedBy = model.UserCreatedBy.Id,
                UserCreatedBy = model.UserCreatedBy
            };
        }

        public async Task<HealthInsuranceEntity> ToHealthInsuranceEntity(HealthInsuranceViewModel model, bool isNew, string userId, string documentPath)
        {
            return new HealthInsuranceEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                SignedDate = model.SignedDate,
                DurationTime = model.DurationTime,
                Active = model.Active,
                DocumentPath = documentPath,
                Clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == model.IdClinic),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)                
            };
        }

        public HealthInsuranceViewModel ToHealthInsuranceViewModel(HealthInsuranceEntity model)
        {
            return new HealthInsuranceViewModel
            {
                Id = model.Id,
                Name = model.Name,
                SignedDate = model.SignedDate,
                DurationTime = model.DurationTime,
                Active = model.Active,
                DocumentPath = model.DocumentPath,
                IdClinic = model.Clinic.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public async Task<Client_HealthInsurance> ToClientHealthInsuranceEntity(UnitsAvailabilityViewModel model, bool isNew, string userId)
        {
            return new Client_HealthInsurance
            {
                Id = isNew ? 0 : model.Id,
                ApprovedDate = model.ApprovedDate,
                DurationTime = model.DurationTime,
                Units = model.Units,                
                Active = isNew ? true : model.Active,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
                HealthInsurance = await _context.HealthInsurances.FirstOrDefaultAsync(hi => hi.Id == model.IdHealthInsurance),                
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public UnitsAvailabilityViewModel ToClientHealthInsuranceViewModel(Client_HealthInsurance model, int idClinic)
        {
            return new UnitsAvailabilityViewModel
            {
                Id = model.Id,
                ApprovedDate = model.ApprovedDate,
                DurationTime = model.DurationTime,
                Units = model.Units,                
                Active = model.Active,
                IdClient = model.Client.Id,
                Clients = _combosHelper.GetComboActiveClientsByClinic(idClinic),
                IdHealthInsurance = model.HealthInsurance.Id,
                HealthInsurances = _combosHelper.GetComboActiveInsurancesByClinic(idClinic),
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public async Task<SettingEntity> ToSettingEntity(SettingViewModel model, bool isNew, string userId)
        {
            return new SettingEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == model.IdClinic),
                AvailableCreateNewWorkdays = model.AvailableCreateNewWorkdays,
                MentalHealthClinic = model.MentalHealthClinic,
                TCMClinic = model.TCMClinic,
                MHClassificationOfGoals = model.MHClassificationOfGoals,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public SettingViewModel ToSettingViewModel(SettingEntity model)
        {
            return new SettingViewModel
            {
                Id = model.Id,
                IdClinic = model.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                AvailableCreateNewWorkdays = model.AvailableCreateNewWorkdays,
                MentalHealthClinic = model.MentalHealthClinic,
                TCMClinic = model.TCMClinic,
                MHClassificationOfGoals = model.MHClassificationOfGoals,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public async Task<TCMServiceEntity> ToTCMServiceEntity(TCMServiceViewModel model, bool isNew)
        {
            return new TCMServiceEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Name = model.Name,
                Description = model.Description,
                Code = model.Code
            };
        }

        public TCMServiceViewModel ToTCMServiceViewModel(TCMServiceEntity TcmServiceEntity, int idClinic)
        {
            return new TCMServiceViewModel
            {
                Id = TcmServiceEntity.Id,
                Name = TcmServiceEntity.Name,
                Code = TcmServiceEntity.Code,
                IdClinic = TcmServiceEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                Description = TcmServiceEntity.Description
            };
        }
        public async Task<TCMStageEntity> ToTCMStageEntity(TCMStageViewModel model, bool isNew)
        {
            return new TCMStageEntity
            {
                Id = isNew ? 0 : model.Id,
                tCMservice = await _context.TCMServices.FindAsync(model.Id_TCMService),
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Name = model.Name,
                Description = model.Description,
                Units = model.Units,
                ID_Etapa = model.ID_Etapa
            };
        }

        public TCMStageViewModel ToTCMStageViewModel(TCMStageEntity TcmStageEntity)
        {
            return new TCMStageViewModel
            {
                Id = TcmStageEntity.Id,
                Name = TcmStageEntity.Name,
                Id_TCMService = TcmStageEntity.tCMservice.Id,
                ID_Etapa = TcmStageEntity.ID_Etapa,
                IdClinic = TcmStageEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                Description = TcmStageEntity.Description,
                Units = TcmStageEntity.Units,
                tCMservice = TcmStageEntity.tCMservice
            };
        }
        public async Task<TCMClientEntity> ToTCMClientEntity(TCMClientViewModel model, bool isNew)
        {
            return new TCMClientEntity
            {
                Id = isNew ? 0 : model.Id,
                Casemanager = model.Casemanager,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                CaseNumber = model.CaseNumber,
                DataOpen = model.DataOpen,
                DataClose = model.DataClose,
                Period = model.Period,
                Client = model.Client

            };
        }

        public TCMClientViewModel ToTCMClientViewModel(TCMClientEntity tcmClientEntity)
        {
            return new TCMClientViewModel
            {
                Id = tcmClientEntity.Id,
                Casemanager = tcmClientEntity.Casemanager,
                IdCaseMannager = tcmClientEntity.Casemanager.Id,
                CaseMannagers = _combosHelper.GetComboCaseManager(),
                IdClient = tcmClientEntity.Client.Id,
                Clients = _combosHelper.GetComboClients(),
                IdStatus = (tcmClientEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus(),
                CaseNumber = tcmClientEntity.CaseNumber,
                DataOpen = tcmClientEntity.DataOpen,
                DataClose = tcmClientEntity.DataClose,
                Period = tcmClientEntity.Period
            };
        }

        public TCMServicePlanViewModel ToTCMServicePlanViewModel(TCMServicePlanEntity TcmServicePlanEntity)
        {
            return new TCMServicePlanViewModel
            {
                Id = TcmServicePlanEntity.Id,
                ID_TcmClient = TcmServicePlanEntity.TcmClient.Id,
                Date_Intake = TcmServicePlanEntity.DateIntake,
                Date_ServicePlan = TcmServicePlanEntity.DateServicePlan,
                Date_Assessment = TcmServicePlanEntity.DateAssessment,
                Date_Certification = TcmServicePlanEntity.DateCertification,
                dischargerCriteria = TcmServicePlanEntity.DischargerCriteria,
                strengths = TcmServicePlanEntity.Strengths,
                weakness = TcmServicePlanEntity.Weakness,
                CaseNumber = TcmServicePlanEntity.TcmClient.CaseNumber,
                ID_Status = (TcmServicePlanEntity.Status == StatusType.Open) ? 1 : 2,
                //TcmClients = _combosHelper.GetComboClientsForTCMCaseOpen(TcmServicePlanEntity.TcmClient.Client.Clinic.Id),
                //TCMDomain = TcmServicePlanEntity.TCMDomain

            };
        }

        public async Task<TCMServicePlanEntity> ToTCMServicePlanEntity(TCMServicePlanViewModel model, bool isNew)
        {
            return new TCMServicePlanEntity
            {
                Id = isNew ? 0 : model.Id,
                DateIntake = model.Date_Intake,
                DateServicePlan = model.Date_ServicePlan,
                DateAssessment = model.Date_Assessment,
                DateCertification = model.Date_Certification,
                TcmClient = await _context.TCMClient.FindAsync(model.ID_TcmClient),
                DischargerCriteria = model.dischargerCriteria,
                Weakness = model.weakness,
                Strengths = model.strengths,
                Status = StatusUtils.GetStatusByIndex(model.ID_Status),
               
            };
        }

        public TCMDomainViewModel ToTCMDomainViewModel(TCMDomainEntity TcmDomainEntity)
        {
            return new TCMDomainViewModel
            {
                Id = TcmDomainEntity.Id,
                Date_Identified = TcmDomainEntity.DateIdentified,
                Needs_Identified = TcmDomainEntity.NeedsIdentified,
                Long_Term = TcmDomainEntity.LongTerm,
                Id_ServicePlan = TcmDomainEntity.TcmServicePlan.Id,
                Code = TcmDomainEntity.Code,
                Name = TcmDomainEntity.Name,
                TcmServicePlan = TcmDomainEntity.TcmServicePlan,
                Services = _combosHelper.GetComboServicesNotUsed(TcmDomainEntity.TcmServicePlan.Id)
            };
        }

        public async Task<TCMDomainEntity> ToTCMDomainEntity(TCMDomainViewModel model, bool isNew)
        {
            return new TCMDomainEntity
            {
                Id = isNew ? 0 : model.Id,
                DateIdentified = model.Date_Identified,
                NeedsIdentified = model.Needs_Identified,
                LongTerm = model.Long_Term,
                TCMObjetive = model.TCMObjetive,
                Code = model.Code,
                Name = model.Name,
                TcmServicePlan = model.TcmServicePlan
        public async Task<IntakeScreeningEntity> ToIntakeEntity(IntakeScreeningViewModel model, bool isNew)
        {
            return new IntakeScreeningEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
                DateDischarge = model.DateDischarge,
                DateSignatureClient = model.DateSignatureClient,
                DateSignatureWitness = model.DateSignatureWitness,
                DoesClientKnowHisName = model.DoesClientKnowHisName,
                DoesClientKnowTimeOfDay = model.DoesClientKnowTimeOfDay,
                DoesClientKnowTodayDate = model.DoesClientKnowTodayDate,
                DoesClientKnowWhereIs = model.DoesClientKnowWhereIs,
                Client_FK = model.Client_FK,
                InformationGatheredBy = model.InformationGatheredBy,
                ClientIsStatus = IntakeScreeninigType.GetClientIsByIndex(model.IdClientIs),
                BehaviorIsStatus = IntakeScreeninigType.GetBehaviorIsByIndex(model.IdBehaviorIs),
                SpeechIsStatus = IntakeScreeninigType.GetSpeechIsByIndex(model.IdSpeechIs),
                EmergencyContact = model.EmergencyContact,
                DateSignatureEmployee = model.DateSignatureEmployee,

            };
        }

        public IntakeScreeningViewModel ToIntakeViewModel(IntakeScreeningEntity model)
        {
            return new   IntakeScreeningViewModel
            {
                Id = model.Id,
                Client = model.Client,
                DateDischarge = model.DateDischarge,
                DateSignatureClient = model.DateSignatureClient,
                DateSignatureWitness = model.DateSignatureWitness,
                DoesClientKnowHisName = model.DoesClientKnowHisName,
                DoesClientKnowTimeOfDay = model.DoesClientKnowTimeOfDay,
                DoesClientKnowTodayDate = model.DoesClientKnowTodayDate,
                DoesClientKnowWhereIs = model.DoesClientKnowWhereIs,
                Client_FK = model.Client_FK,
                IdClient = model.Client.Id,
                InformationGatheredBy = model.InformationGatheredBy,
                IdClientIs = Convert.ToInt32(model.ClientIsStatus) + 1,
                ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                IdBehaviorIs = Convert.ToInt32(model.BehaviorIsStatus) + 1,
                BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                IdSpeechIs = Convert.ToInt32(model.SpeechIsStatus) + 1,
                SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
                EmergencyContact = model.EmergencyContact,
                DateSignatureEmployee = model.DateSignatureEmployee,

            };
           
        }

        public async Task<IntakeConsentForTreatmentEntity> ToIntakeConsentForTreatmentEntity(IntakeConsentForTreatmentViewModel model, bool isNew)
        {
            return new IntakeConsentForTreatmentEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                
                Aggre = model.Aggre,
                Aggre1 = model.Aggre1,
                AuthorizeRelease = model.AuthorizeRelease,
                AuthorizeStaff = model.AuthorizeStaff,
                Certify = model.Certify,
                Certify1 = model.Certify1,
                Client_FK = model.Client_FK,//model.Client.Id,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Underestand = model.Underestand,
                AdmissionedFor = model.AdmissionedFor,
                

            };
        }

        public IntakeConsentForTreatmentViewModel ToIntakeConsentForTreatmentViewModel(IntakeConsentForTreatmentEntity model)
        {
            return new IntakeConsentForTreatmentViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Aggre = model.Aggre,
                Aggre1 = model.Aggre1,
                AuthorizeRelease = model.AuthorizeRelease,
                AuthorizeStaff = model.AuthorizeStaff,
                Certify = model.Certify,
                Certify1 = model.Certify1,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Underestand = model.Underestand,
                AdmissionedFor = model.AdmissionedFor,

            };
            
        }

        public async Task<IntakeConsentForReleaseEntity> ToIntakeConsentForReleaseEntity(IntakeConsentForReleaseViewModel model, bool isNew)
        {
            return new IntakeConsentForReleaseEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Discaherge = model.Discaherge,
                ForPurpose_CaseManagement = model.ForPurpose_CaseManagement,
                ForPurpose_Other = model.ForPurpose_Other,
                ForPurpose_OtherExplain = model.ForPurpose_OtherExplain,
                ForPurpose_Treatment = model.ForPurpose_Treatment,
                InForm_Facsimile = model.InForm_Facsimile,
                InForm_VerbalInformation = model.InForm_VerbalInformation,
                InForm_WrittenRecords = model.InForm_WrittenRecords,
                History = model.History,
                HospitalRecord = model.HospitalRecord,
                IncidentReport = model.IncidentReport,
                
                LabWork = model.LabWork,
                Other = model.Other,
                Other_Explain = model.Other_Explain,
                ProgressReports = model.ProgressReports,
                PsychologycalEvaluation = model.PsychologycalEvaluation,
                SchoolRecord = model.SchoolRecord,
                ToRelease = model.ToRelease,
                AdmissionedFor = model.AdmissionedFor,
            };
        }

        public IntakeConsentForReleaseViewModel ToIntakeConsentForReleaseViewModel(IntakeConsentForReleaseEntity model)
        {
            return new IntakeConsentForReleaseViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                ToRelease = model.ToRelease,
                SchoolRecord = model.SchoolRecord,
                PsychologycalEvaluation = model.PsychologycalEvaluation,
                ProgressReports = model.ProgressReports,
                Other = model.Other,
                Other_Explain = model.Other_Explain,
                Discaherge = model.Discaherge,
                LabWork = model.LabWork,
                History = model.History,
                HospitalRecord = model.HospitalRecord,
                IncidentReport = model.IncidentReport,
                ForPurpose_CaseManagement = model.ForPurpose_CaseManagement,
                ForPurpose_Other = model.ForPurpose_Other,
                ForPurpose_OtherExplain = model.ForPurpose_OtherExplain,
                ForPurpose_Treatment = model.ForPurpose_Treatment,
                InForm_Facsimile = model.InForm_Facsimile,
                InForm_VerbalInformation = model.InForm_VerbalInformation,
                InForm_WrittenRecords = model.InForm_WrittenRecords,
                AdmissionedFor = model.AdmissionedFor,

            };

        }

        public async Task<IntakeConsumerRightsEntity> ToIntakeConsumerRightsEntity(IntakeConsumerRightsViewModel model, bool isNew)
        {
            return new IntakeConsumerRightsEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                ServedOf = model.ServedOf,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,

            };
        }

        public IntakeConsumerRightsViewModel ToIntakeConsumerRightsViewModel(IntakeConsumerRightsEntity model)
        {
            return new IntakeConsumerRightsViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                ServedOf = model.ServedOf,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,

            };

        }

        public async Task<IntakeAcknowledgementHippaEntity> ToIntakeAcknoewledgementHippaEntity(IntakeAcknoewledgementHippaViewModel model, bool isNew)
        {
            return new IntakeAcknowledgementHippaEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,

            };
        }

        public IntakeAcknoewledgementHippaViewModel ToIntakeAcknoewledgementHippaViewModel(IntakeAcknowledgementHippaEntity model)
        {
            return new IntakeAcknoewledgementHippaViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
               
            };

        }
        
        public async Task<IntakeAccessToServicesEntity> ToIntakeAccessToServicesEntity(IntakeAccessToServicesViewModel model, bool isNew)
        {
            return new IntakeAccessToServicesEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,

            };
        }

        public IntakeAccessToServicesViewModel ToIntakeAccessToServicesViewModel(IntakeAccessToServicesEntity model)
        {
            return new IntakeAccessToServicesViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                
            };

        }

        public async Task<IntakeOrientationChecklistEntity> ToIntakeOrientationChecklistEntity(IntakeOrientationCheckListViewModel model, bool isNew)
        {
            return new IntakeOrientationChecklistEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Access = model.Access,
                AgencyExpectation = model.AgencyExpectation,
                AgencyPolice = model.AgencyPolice,
                Code = model.Code,
                Confidentiality = model.Confidentiality,
                Discharge = model.Discharge,
                Education = model.Education,
                Explanation = model.Explanation,
                Fire = model.Fire,
                Identification = model.Identification,
                IndividualPlan = model.IndividualPlan,
                Insent = model.Insent,
                Methods = model.Methods,
                PoliceGrievancce = model.PoliceGrievancce,
                PoliceIllicit = model.PoliceIllicit,
                PoliceTobacco = model.PoliceTobacco,
                PoliceWeapons = model.PoliceWeapons,
                Program = model.Program,
                Purpose = model.Purpose,
                Rights = model.Rights,
                Services = model.Services,
                TheAbove = model.TheAbove,
                TourFacility = model.TourFacility,
                AdmissionedFor = model.AdmissionedFor,

            };
        }

        public async Task<TCMObjetiveEntity> ToTCMObjetiveEntity(TCMObjetiveViewModel model, bool isNew)
        {            
            return new TCMObjetiveEntity
            {
                Id = isNew ? 0 : model.Id,
                TcmDomain = await _context.TCMDomains.FindAsync(model.Id_Domain),
                //Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Name = model.name,
                Task = model.task,
                //Long_Term = model.long_Term,
                StartDate = model.Start_Date,
                TargetDate = model.Target_Date,
                EndDate = model.End_Date,
                Finish = model.Finish,
                IdObjetive = model.ID_Objetive,
                Status = model.Status,
                Responsible = model.Responsible
                
            };
        }

        public TCMObjetiveViewModel ToTCMObjetiveViewModel(TCMObjetiveEntity TcmObjetiveEntity)
        {
            return new TCMObjetiveViewModel
            {
                Id = TcmObjetiveEntity.Id,
                Id_Domain = TcmObjetiveEntity.TcmDomain.Id,
                IdObjetive= TcmObjetiveEntity.IdObjetive,
                //IdClinic = TcmObjetiveEntity.Clinic.Id,
                //Clinics = _combosHelper.GetComboClinics(),
                name = TcmObjetiveEntity.Name,
                task = TcmObjetiveEntity.Task,
                Status = TcmObjetiveEntity.Status,
                Start_Date = TcmObjetiveEntity.StartDate,
                Target_Date = TcmObjetiveEntity.TargetDate,
                End_Date = TcmObjetiveEntity.EndDate,
                Finish = TcmObjetiveEntity.Finish,
                TcmDomain = TcmObjetiveEntity.TcmDomain,
                Stages = _combosHelper.GetComboStagesNotUsed(TcmObjetiveEntity.TcmDomain),
                Responsible = TcmObjetiveEntity.Responsible
            };
        }

        public IntakeOrientationCheckListViewModel ToIntakeOrientationChecklistViewModel(IntakeOrientationChecklistEntity model)
        {
            return new IntakeOrientationCheckListViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Access = model.Access,
                AgencyExpectation = model.AgencyExpectation,
                AgencyPolice = model.AgencyPolice,
                Code = model.Code,
                Confidentiality = model.Confidentiality,
                Discharge = model.Discharge,
                Education = model.Education,
                Explanation = model.Explanation,
                Fire = model.Fire,
                Identification = model.Identification,
                IndividualPlan = model.IndividualPlan,
                Insent = model.Insent,
                Methods = model.Methods,
                PoliceGrievancce = model.PoliceGrievancce,
                PoliceIllicit = model.PoliceIllicit,
                PoliceTobacco = model.PoliceTobacco,
                PoliceWeapons = model.PoliceWeapons,
                Program = model.Program,
                Purpose = model.Purpose,
                Rights = model.Rights,
                Services = model.Services,
                TheAbove = model.TheAbove,
                TourFacility = model.TourFacility,
                AdmissionedFor = model.AdmissionedFor,

            };

        }

        public async Task<IntakeTransportationEntity> ToIntakeTransportationEntity(IntakeTransportationViewModel model, bool isNew)
        {
            return new IntakeTransportationEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,

            };
        }

        public IntakeTransportationViewModel ToIntakeTransportationViewModel(IntakeTransportationEntity model)
        {
            return new IntakeTransportationViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,

            };

        }

        public async Task<IntakeConsentPhotographEntity> ToIntakeConsentPhotographEntity(IntakeConsentPhotographViewModel model, bool isNew)
        {
            return new IntakeConsentPhotographEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Photograph = model.Photograph,
                Filmed = model.Filmed,
                VideoTaped = model.VideoTaped,
                Interviwed = model.Interviwed,
                NoneOfTheForegoing = model.NoneOfTheForegoing,
                Other = model.Other,
                Publication = model.Publication,
                Broadcast = model.Broadcast,
                Markrting = model.Markrting,
                ByTODocument = model.ByTODocument,
                AdmissionedFor = model.AdmissionedFor,

            };
        }

        public async Task<TCMAdendumEntity> ToTCMAdendumEntity(TCMAdendumViewModel model, bool isNew)
        {

            return new TCMAdendumEntity
            {
                Id = isNew ? 0 : model.Id,
                DateAdendum = model.Date_Identified,
                TcmServicePlan = model.TcmServicePlan,
                TcmDomain = model.TcmDomain
                
            };
        }

        public TCMAdendumViewModel ToTCMAdendumViewModel(TCMAdendumEntity TcmAdendumEntity)
        {
            return new TCMAdendumViewModel
            {
                Id = TcmAdendumEntity.Id,
                TcmServicePlan = TcmAdendumEntity.TcmServicePlan,
                ListTcmServicePlan = _combosHelper.GetComboServicesPlan(TcmAdendumEntity.TcmServicePlan.TcmClient.Casemanager.Clinic.Id),
                TcmDominio = _combosHelper.GetComboTCMServices(),
                TcmDomain = TcmAdendumEntity.TcmDomain,
                DateAdendum = TcmAdendumEntity.DateAdendum
            };
        }

        public TCMServicePlanReviewViewModel ToTCMServicePlanReviewViewModel(TCMServicePlanReviewEntity TcmServicePlanReviewEntity)
        {
            return new TCMServicePlanReviewViewModel
            {
                Id = TcmServicePlanReviewEntity.Id,
                DateOpending  = TcmServicePlanReviewEntity.DateOpending,
                DateServicePlanReview = TcmServicePlanReviewEntity.DateServicePlanReview,
                Recomendation = TcmServicePlanReviewEntity.Recomendation,
                SummaryProgress = TcmServicePlanReviewEntity.SummaryProgress,
                TcmServicePlan = TcmServicePlanReviewEntity.TcmServicePlan,
                TCMServicePlanRevDomain = TcmServicePlanReviewEntity.TCMServicePlanRevDomain,
                StatusListDomain = _combosHelper.GetComboObjetiveStatus(),
                StatusListObjetive = _combosHelper.GetComboObjetiveStatus(),
                IdServicePlan = TcmServicePlanReviewEntity.TcmServicePlan.Id,
                _TCMServicePlanRevDomain = TcmServicePlanReviewEntity.TCMServicePlanRevDomain,

                //ID_Status = (TcmServicePlanEntity.Status == StatusType.Open) ? 1 : 2,

            };
        }

        public async Task<TCMServicePlanReviewEntity> ToTCMServicePlanReviewEntity(TCMServicePlanReviewViewModel model, bool isNew)
        {
            TCMServicePlanEntity tcmServicePlan = await _context.TCMServicePlans.FirstOrDefaultAsync(n => n.Id == model.IdServicePlan);
            return new TCMServicePlanReviewEntity
            {
                Id = isNew ? 0 : model.Id,
                DateOpending = tcmServicePlan.DateIntake,
                DateServicePlanReview = model.DateServicePlanReview,
                Recomendation = model.Recomendation,
                SummaryProgress = model.SummaryProgress,
                TcmServicePlan = tcmServicePlan,
                TCMServicePlanRevDomain = model.TCMServicePlanRevDomain,
                
            };
        }

        public TCMServicePlanReviewDomainViewModel ToTCMServicePlanReviewDomainViewModel(TCMServicePlanReviewDomainEntity TcmServicePlanReviewDomianEntity)
        {
            return new TCMServicePlanReviewDomainViewModel
            {
                Id = TcmServicePlanReviewDomianEntity.Id,
                ChangesUpdate = TcmServicePlanReviewDomianEntity.ChangesUpdate,
                IdTcmDomain = TcmServicePlanReviewDomianEntity.TcmDomain.Id,
                TcmDomain = TcmServicePlanReviewDomianEntity.TcmDomain,
                status = _combosHelper.GetComboClientStatus(),
              
            };
        }

        public async Task<TCMServicePlanReviewDomainEntity> ToTCMServicePlanReviewDomainEntity(TCMServicePlanReviewDomainViewModel model, bool isNew)
        {
            return new TCMServicePlanReviewDomainEntity
            {
                Id = isNew ? 0 : model.Id,
                TcmDomain = model.TcmDomain,
                ChangesUpdate = model.ChangesUpdate,
               
            };
        }

        public TCMDischargeViewModel ToTCMDischargeViewModel(TCMDischargeEntity TcmDischargeEntity)
        {
            return new TCMDischargeViewModel
            {
                Id = TcmDischargeEntity.Id,
                AdministrativeDischarge = TcmDischargeEntity.AdministrativeDischarge,
                AdministrativeDischarge_Explain = TcmDischargeEntity.AdministrativeDischarge_Explain,
                AllServiceInPlace = TcmDischargeEntity.AllServiceInPlace,
                ClientLeftVoluntarily = TcmDischargeEntity.ClientLeftVoluntarily,
                ClientMovedOutArea = TcmDischargeEntity.ClientMovedOutArea,
                DischargeDate = TcmDischargeEntity.DischargeDate,
                IdServicePlan = TcmDischargeEntity.TcmServicePlan.Id,
                TcmServicePlan = TcmDischargeEntity.TcmServicePlan,
                LackOfProgress = TcmDischargeEntity.LackOfProgress,
                NonComplianceWithAgencyRules = TcmDischargeEntity.NonComplianceWithAgencyRules,
                Other = TcmDischargeEntity.Other,
                Other_Explain = TcmDischargeEntity.Other_Explain,
                PresentProblems = TcmDischargeEntity.PresentProblems,
                ProgressToward = TcmDischargeEntity.ProgressToward,
                Referred = TcmDischargeEntity.Referred,
                StaffingDate = DateTime.Now,
                StaffSignatureDate = DateTime.Now,
                SupervisorSignatureDate = DateTime.Now,
                TcmDischargeFollowUp = TcmDischargeEntity.TcmDischargeFollowUp,
                TcmDischargeServiceStatus = TcmDischargeEntity.TcmDischargeServiceStatus,
                TcmServices = TcmDischargeEntity.TcmServicePlan.TCMService,

            };
        }

        public async Task<TCMDischargeEntity> ToTCMDischargeEntity(TCMDischargeViewModel model, bool isNew)
        {
            return new TCMDischargeEntity
            {
                Id = isNew ? 0 : model.Id,
                StaffingDate = model.StaffingDate,
                DischargeDate = model.DischargeDate,
                PresentProblems = model.PresentProblems,
                ProgressToward = model.ProgressToward,
                TcmDischargeServiceStatus = model.TcmDischargeServiceStatus,
                AllServiceInPlace = model.AllServiceInPlace,
                NonComplianceWithAgencyRules = model.NonComplianceWithAgencyRules,
                Referred = model.Referred,
                ClientMovedOutArea =model.ClientMovedOutArea,
                ClientLeftVoluntarily = model.ClientLeftVoluntarily,
                LackOfProgress = model.LackOfProgress,
                Other = model.Other,
                Other_Explain = model.Other_Explain,
                AdministrativeDischarge = model.AdministrativeDischarge,
                AdministrativeDischarge_Explain = model.AdministrativeDischarge_Explain,
                TcmDischargeFollowUp = model.TcmDischargeFollowUp,
                StaffSignatureDate = model.StaffSignatureDate,
                SupervisorSignatureDate = model.SupervisorSignatureDate,
                TcmServicePlan = _context.TCMServicePlans.Find(model.IdServicePlan),

            };
        }

        public IntakeConsentPhotographViewModel ToIntakeConsentPhotographViewModel(IntakeConsentPhotographEntity model)
        {
            return new IntakeConsentPhotographViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Photograph = model.Photograph,
                Filmed = model.Filmed,
                VideoTaped = model.VideoTaped,
                Interviwed = model.Interviwed,
                NoneOfTheForegoing = model.NoneOfTheForegoing,
                Other = model.Other,
                Publication = model.Publication,
                Broadcast = model.Broadcast,
                Markrting = model.Markrting,
                ByTODocument = model.ByTODocument,
                AdmissionedFor = model.AdmissionedFor,


            };

        }

        public async Task<IntakeFeeAgreementEntity> ToIntakeFeeAgreementEntity(IntakeFeeAgreementViewModel model, bool isNew)
        {
            return new IntakeFeeAgreementEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,

            };
        }

        public IntakeFeeAgreementViewModel ToIntakeFeeAgreementViewModel(IntakeFeeAgreementEntity model)
        {
            return new IntakeFeeAgreementViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                

            };

        }

        public async Task<IntakeTuberculosisEntity> ToIntakeTuberculosisEntity(IntakeTuberculosisViewModel model, bool isNew)
        {
            return new IntakeTuberculosisEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,

                DoYouCurrently = model.DoYouCurrently,
                DoYouBring = model.DoYouBring,
                DoYouCough = model.DoYouCough,
                DoYouSweat = model.DoYouSweat,
                DoYouHaveFever = model.DoYouHaveFever,
                HaveYouLost = model.HaveYouLost,
                DoYouHaveChest = model.DoYouHaveChest,
                If2OrMore = model.If2OrMore,

                HaveYouRecently = model.HaveYouRecently,
                AreYouRecently = model.AreYouRecently,
                IfYesWhich = model.IfYesWhich,
                DoYouOr = model.DoYouOr,
                HaveYouEverBeen = model.HaveYouEverBeen,
                HaveYouEverWorked = model.HaveYouEverWorked,
                HaveYouEverHadOrgan = model.HaveYouEverHadOrgan,
                HaveYouEverConsidered = model.HaveYouEverConsidered,
                HaveYouEverHadAbnormal = model.HaveYouEverHadAbnormal,
                If3OrMore = model.If3OrMore,

                HaveYouEverHadPositive = model.HaveYouEverHadPositive,
                IfYesWhere = model.IfYesWhere,
                When = model.When,
                HaveYoyEverBeenTold = model.HaveYoyEverBeenTold,
                AgencyExpectation = model.AgencyExpectation,
                If1OrMore = model.If1OrMore,

                AdmissionedFor = model.AdmissionedFor
            };
        }

        public IntakeTuberculosisViewModel ToIntakeTuberculosisViewModel(IntakeTuberculosisEntity model)
        {
            return new IntakeTuberculosisViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,

                DoYouCurrently = model.DoYouCurrently,
                DoYouBring = model.DoYouBring,
                DoYouCough = model.DoYouCough,
                DoYouSweat = model.DoYouSweat,
                DoYouHaveFever = model.DoYouHaveFever,
                HaveYouLost = model.HaveYouLost,
                DoYouHaveChest = model.DoYouHaveChest,
                If2OrMore = model.If2OrMore,

                HaveYouRecently = model.HaveYouRecently,
                AreYouRecently = model.AreYouRecently,
                IfYesWhich = model.IfYesWhich,
                DoYouOr = model.DoYouOr,
                HaveYouEverBeen = model.HaveYouEverBeen,
                HaveYouEverWorked = model.HaveYouEverWorked,
                HaveYouEverHadOrgan = model.HaveYouEverHadOrgan,
                HaveYouEverConsidered = model.HaveYouEverConsidered,
                HaveYouEverHadAbnormal = model.HaveYouEverHadAbnormal,
                If3OrMore = model.If3OrMore,

                HaveYouEverHadPositive = model.HaveYouEverHadPositive,
                IfYesWhere = model.IfYesWhere,
                When = model.When,
                HaveYoyEverBeenTold = model.HaveYoyEverBeenTold,
                AgencyExpectation = model.AgencyExpectation,
                If1OrMore = model.If1OrMore,
                
                AdmissionedFor = model.AdmissionedFor
            };

        }

        public async Task<IntakeMedicalHistoryEntity> ToIntakeMedicalHistoryEntity(IntakeMedicalHistoryViewModel model, bool isNew)
        {
            return new IntakeMedicalHistoryEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,

                AddressPhysician = model.AddressPhysician,
                AgeFirstTalked = model.AgeFirstTalked,
                AgeFirstWalked = model.AgeFirstWalked,
                AgeToiletTrained = model.AgeToiletTrained,
                AgeWeaned = model.AgeWeaned,
                Allergies = model.Allergies,
                Allergies_Describe = model.Allergies_Describe,
                AndOrSoiling = model.AndOrSoiling,
                Anemia = model.Anemia,
                AreYouCurrently = model.AreYouCurrently,
                AreYouPhysician = model.AreYouPhysician,
                Arthritis = model.Arthritis,
                AssumingCertainPositions = model.AssumingCertainPositions,
                BackPain = model.BackPain,
                BeingConfused = model.BeingConfused,
                BeingDisorientated = model.BeingDisorientated,
                BirthWeight = model.BirthWeight,
                BlackStools = model.BlackStools,
                BloodInUrine = model.BloodInUrine,
                BloodyStools = model.BloodyStools,
                BottleFedUntilAge = model.BottleFedUntilAge,
                BreastFed = model.BreastFed,
                BurningUrine = model.BurningUrine,
                Calculating = model.Calculating,
                Cancer = model.Cancer,
                ChestPain = model.ChestPain,
                ChronicCough = model.ChronicCough,
                ChronicIndigestion = model.ChronicIndigestion,
                City = model.City,
                Complications = model.Complications,
                Complications_Explain = model.Complications_Explain,
                Comprehending = model.Comprehending,
                Concentrating = model.Concentrating,
                Constipation = model.Constipation,
                ConvulsionsOrFits = model.ConvulsionsOrFits,
                CoughingOfBlood = model.CoughingOfBlood,
                DescriptionOfChild = model.DescriptionOfChild,
                Diabetes = model.Diabetes,
                Diphtheria = model.Diphtheria,
                DoYouSmoke = model.DoYouSmoke,
                DoYouSmoke_PackPerDay = model.DoYouSmoke_PackPerDay,
                DoYouSmoke_Year = model.DoYouSmoke_Year,
                EarInfections = model.EarInfections,
                Epilepsy = model.Epilepsy,
                EyeTrouble = model.EyeTrouble,
                Fainting = model.Fainting,
                FamilyAsthma = model.FamilyAsthma,
                FamilyAsthma_ = model.FamilyAsthma_,
                FamilyCancer = model.FamilyCancer,
                FamilyCancer_ = model.FamilyCancer_,
                FamilyDiabetes = model.FamilyDiabetes,
                FamilyDiabetes_ = model.FamilyDiabetes_,
                FamilyEpilepsy = model.FamilyEpilepsy,
                FamilyEpilepsy_ = model.FamilyEpilepsy_,
                FamilyGlaucoma = model.FamilyGlaucoma,
                FamilyGlaucoma_ = model.FamilyGlaucoma_,
                FamilyHayFever = model.FamilyHayFever,
                FamilyHayFever_ = model.FamilyHayFever_,
                FamilyHeartDisease = model.FamilyHeartDisease,
                FamilyHeartDisease_ = model.FamilyHeartDisease_,
                FamilyHighBloodPressure = model.FamilyHighBloodPressure,
                FamilyHighBloodPressure_ = model.FamilyHighBloodPressure_,
                FamilyKidneyDisease = model.FamilyKidneyDisease,
                FamilyKidneyDisease_ = model.FamilyKidneyDisease_,
                FamilyNervousDisorders = model.FamilyNervousDisorders,
                FamilyNervousDisorders_ = model.FamilyNervousDisorders_,
                FamilyOther = model.FamilyOther,
                FamilyOther_ = model.FamilyOther_,
                FamilySyphilis = model.FamilySyphilis,
                FamilySyphilis_ = model.FamilySyphilis_,
                FamilyTuberculosis = model.FamilyTuberculosis,
                FamilyTuberculosis_ = model.FamilyTuberculosis_,
                FirstYearMedical = model.FirstYearMedical,
                Fractures = model.Fractures,
                FrequentColds = model.FrequentColds,
                FrequentHeadaches = model.FrequentHeadaches,
                FrequentNoseBleeds = model.FrequentNoseBleeds,
                FrequentSoreThroat = model.FrequentSoreThroat,
                FrequentVomiting = model.FrequentVomiting,
                HaveYouEverBeenPregnant = model.HaveYouEverBeenPregnant,
                HaveYouEverHadComplications = model.Complications,
                HaveYouEverHadExcessive = model.HaveYouEverHadExcessive,
                HaveYouEverHadPainful = model.HaveYouEverHadPainful,
                HaveYouEverHadSpotting = model.HaveYouEverHadSpotting,
                HayFever = model.HayFever,
                HeadInjury = model.HeadInjury,
                Hearing = model.Hearing,
                HearingTrouble = model.HearingTrouble,
                HeartPalpitation = model.HeartPalpitation,
                Hemorrhoids = model.Hemorrhoids,
                Hepatitis = model.Hepatitis,
                Hernia = model.Hernia,
                HighBloodPressure = model.HighBloodPressure,
                Hoarseness = model.Hoarseness,
                Immunizations = model.Immunizations,
                InfectiousDisease = model.InfectiousDisease,
                Jaundice = model.Jaundice,
                KidneyStones = model.KidneyStones,
                KidneyTrouble = model.KidneyTrouble,
                Length = model.Length,
                ListAllCurrentMedications = model.ListAllCurrentMedications,
                LossOfMemory = model.LossOfMemory,
                Mumps = model.Mumps,
                Nervousness = model.Nervousness,
                NightSweats = model.NightSweats,
                Normal = model.Normal,
                PainfulJoints = model.PainfulJoints,
                PainfulMuscles = model.PainfulMuscles,
                PainfulUrination = model.PainfulUrination,
                PerformingCertainMotions = model.PerformingCertainMotions,
                Planned = model.Planned,
                Poliomyelitis = model.Poliomyelitis,
                PrimaryCarePhysician = model.PrimaryCarePhysician,
                ProblemWithBedWetting = model.ProblemWithBedWetting,
                Reading = model.Reading,
                RheumaticFever = model.RheumaticFever,
                Rheumatism = model.Rheumatism,
                ScarletFever = model.ScarletFever,
                Seeing = model.Seeing,
                SeriousInjury = model.SeriousInjury,
                ShortnessOfBreath = model.ShortnessOfBreath,
                SkinTrouble = model.SkinTrouble,
                Speaking = model.Speaking,
                State = model.State,
                StomachPain = model.StomachPain,
                Surgery = model.Surgery,
                SwellingOfFeet = model.SwellingOfFeet,
                SwollenAnkles = model.SwollenAnkles,
                Tuberculosis = model.Tuberculosis,
                Unplanned = model.Unplanned,
                VaricoseVeins = model.VaricoseVeins,
                VenerealDisease = model.VenerealDisease,
                VomitingOfBlood = model.VomitingOfBlood,
                Walking = model.Walking,
                WeightLoss = model.WeightLoss,
                WhoopingCough = model.WhoopingCough,
                WritingSentence = model.WritingSentence,
                ZipCode = model.ZipCode,

                AgeOfFirstMenstruation = model.AgeOfFirstMenstruation,
                DateOfLastBreastExam = model.DateOfLastBreastExam,
                DateOfLastPelvic = model.DateOfLastPelvic,
                DateOfLastPeriod = model.DateOfLastPeriod,
                UsualDurationOfPeriods = model.UsualDurationOfPeriods,
                UsualIntervalBetweenPeriods = model.UsualIntervalBetweenPeriods,

                AdmissionedFor = model.AdmissionedFor,
                InformationProvided = model.InformationProvided
            };
        }

        public IntakeMedicalHistoryViewModel ToIntakeMedicalHistoryViewModel(IntakeMedicalHistoryEntity model)
        {
            return new IntakeMedicalHistoryViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,

                AddressPhysician = model.AddressPhysician,
                AgeFirstTalked = model.AgeFirstTalked,
                AgeFirstWalked = model.AgeFirstWalked,
                AgeToiletTrained = model.AgeToiletTrained,
                AgeWeaned = model.AgeWeaned,
                Allergies = model.Allergies,
                Allergies_Describe = model.Allergies_Describe,
                AndOrSoiling = model.AndOrSoiling,
                Anemia = model.Anemia,
                AreYouCurrently = model.AreYouCurrently,
                AreYouPhysician = model.AreYouPhysician,
                Arthritis = model.Arthritis,
                AssumingCertainPositions = model.AssumingCertainPositions,
                BackPain = model.BackPain,
                BeingConfused = model.BeingConfused,
                BeingDisorientated = model.BeingDisorientated,
                BirthWeight = model.BirthWeight,
                BlackStools = model.BlackStools,
                BloodInUrine = model.BloodInUrine,
                BloodyStools = model.BloodyStools,
                BottleFedUntilAge = model.BottleFedUntilAge,
                BreastFed = model.BreastFed,
                BurningUrine = model.BurningUrine,
                Calculating = model.Calculating,
                Cancer = model.Cancer,
                ChestPain = model.ChestPain,
                ChronicCough = model.ChronicCough,
                ChronicIndigestion = model.ChronicIndigestion,
                City = model.City,
                Complications = model.Complications,
                Complications_Explain = model.Complications_Explain,
                Comprehending = model.Comprehending,
                Concentrating = model.Concentrating,
                Constipation = model.Constipation,
                ConvulsionsOrFits = model.ConvulsionsOrFits,
                CoughingOfBlood = model.CoughingOfBlood,
                DescriptionOfChild = model.DescriptionOfChild,
                Diabetes = model.Diabetes,
                Diphtheria = model.Diphtheria,
                DoYouSmoke = model.DoYouSmoke,
                DoYouSmoke_PackPerDay = model.DoYouSmoke_PackPerDay,
                DoYouSmoke_Year = model.DoYouSmoke_Year,
                EarInfections = model.EarInfections,
                Epilepsy = model.Epilepsy,
                EyeTrouble = model.EyeTrouble,
                Fainting = model.Fainting,
                FamilyAsthma = model.FamilyAsthma,
                FamilyAsthma_ = model.FamilyAsthma_,
                FamilyCancer = model.FamilyCancer,
                FamilyCancer_ = model.FamilyCancer_,
                FamilyDiabetes = model.FamilyDiabetes,
                FamilyDiabetes_ = model.FamilyDiabetes_,
                FamilyEpilepsy = model.FamilyEpilepsy,
                FamilyEpilepsy_ = model.FamilyEpilepsy_,
                FamilyGlaucoma = model.FamilyGlaucoma,
                FamilyGlaucoma_ = model.FamilyGlaucoma_,
                FamilyHayFever = model.FamilyHayFever,
                FamilyHayFever_ = model.FamilyHayFever_,
                FamilyHeartDisease = model.FamilyHeartDisease,
                FamilyHeartDisease_ = model.FamilyHeartDisease_,
                FamilyHighBloodPressure = model.FamilyHighBloodPressure,
                FamilyHighBloodPressure_ = model.FamilyHighBloodPressure_,
                FamilyKidneyDisease = model.FamilyKidneyDisease,
                FamilyKidneyDisease_ = model.FamilyKidneyDisease_,
                FamilyNervousDisorders = model.FamilyNervousDisorders,
                FamilyNervousDisorders_ = model.FamilyNervousDisorders_,
                FamilyOther = model.FamilyOther,
                FamilyOther_ = model.FamilyOther_,
                FamilySyphilis = model.FamilySyphilis,
                FamilySyphilis_ = model.FamilySyphilis_,
                FamilyTuberculosis = model.FamilyTuberculosis,
                FamilyTuberculosis_ = model.FamilyTuberculosis_,
                FirstYearMedical = model.FirstYearMedical,
                Fractures = model.Fractures,
                FrequentColds = model.FrequentColds,
                FrequentHeadaches = model.FrequentHeadaches,
                FrequentNoseBleeds = model.FrequentNoseBleeds,
                FrequentSoreThroat = model.FrequentSoreThroat,
                FrequentVomiting = model.FrequentVomiting,
                HaveYouEverBeenPregnant = model.HaveYouEverBeenPregnant,
                HaveYouEverHadComplications = model.Complications,
                HaveYouEverHadExcessive = model.HaveYouEverHadExcessive,
                HaveYouEverHadPainful = model.HaveYouEverHadPainful,
                HaveYouEverHadSpotting = model.HaveYouEverHadSpotting,
                HayFever = model.HayFever,
                HeadInjury = model.HeadInjury,
                Hearing = model.Hearing,
                HearingTrouble = model.HearingTrouble,
                HeartPalpitation = model.HeartPalpitation,
                Hemorrhoids = model.Hemorrhoids,
                Hepatitis = model.Hepatitis,
                Hernia = model.Hernia,
                HighBloodPressure = model.HighBloodPressure,
                Hoarseness = model.Hoarseness,
                Immunizations = model.Immunizations,
                InfectiousDisease = model.InfectiousDisease,
                Jaundice = model.Jaundice,
                KidneyStones = model.KidneyStones,
                KidneyTrouble = model.KidneyTrouble,
                Length = model.Length,
                ListAllCurrentMedications = model.ListAllCurrentMedications,
                LossOfMemory = model.LossOfMemory,
                Mumps = model.Mumps,
                Nervousness = model.Nervousness,
                NightSweats = model.NightSweats,
                Normal = model.Normal,
                PainfulJoints = model.PainfulJoints,
                PainfulMuscles = model.PainfulMuscles,
                PainfulUrination = model.PainfulUrination,
                PerformingCertainMotions = model.PerformingCertainMotions,
                Planned = model.Planned,
                Poliomyelitis = model.Poliomyelitis,
                PrimaryCarePhysician = model.PrimaryCarePhysician,
                ProblemWithBedWetting = model.ProblemWithBedWetting,
                Reading = model.Reading,
                RheumaticFever = model.RheumaticFever,
                Rheumatism = model.Rheumatism,
                ScarletFever = model.ScarletFever,
                Seeing = model.Seeing,
                SeriousInjury = model.SeriousInjury,
                ShortnessOfBreath = model.ShortnessOfBreath,
                SkinTrouble = model.SkinTrouble,
                Speaking = model.Speaking,
                State = model.State,
                StomachPain = model.StomachPain,
                Surgery = model.Surgery,
                SwellingOfFeet = model.SwellingOfFeet,
                SwollenAnkles = model.SwollenAnkles,
                Tuberculosis = model.Tuberculosis,
                Unplanned = model.Unplanned,
                VaricoseVeins = model.VaricoseVeins,
                VenerealDisease = model.VenerealDisease,
                VomitingOfBlood = model.VomitingOfBlood,
                Walking = model.Walking,
                WeightLoss = model.WeightLoss,
                WhoopingCough = model.WhoopingCough,
                WritingSentence = model.WritingSentence,
                ZipCode = model.ZipCode,

                AgeOfFirstMenstruation = model.AgeOfFirstMenstruation,
                DateOfLastBreastExam = model.DateOfLastBreastExam,
                DateOfLastPelvic = model.DateOfLastPelvic,
                DateOfLastPeriod = model.DateOfLastPeriod,
                UsualDurationOfPeriods = model.UsualDurationOfPeriods,
                UsualIntervalBetweenPeriods = model.UsualIntervalBetweenPeriods,

                AdmissionedFor = model.AdmissionedFor,
                InformationProvided = model.InformationProvided
            };

        }

        public async Task<DischargeEntity> ToDischargeEntity(DischargeViewModel model, bool isNew)
        {
            return new DischargeEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
                // Client_FK = isNew ? model.IdClient : model.Client_FK,
                Client_FK = model.IdClient,
                AdmissionedFor = model.AdmissionedFor,
                AgencyDischargeClient = model.AgencyDischargeClient,
                BriefHistory = model.BriefHistory,
                ClientDeceased = model.ClientDeceased,
                ClientDischargeAgainst = model.ClientDischargeAgainst,
                ClientMoved = model.ClientMoved,
                ClientReferred = model.ClientReferred,
                ConditionalDischarge = model.ConditionalDischarge,
                CourseTreatment = model.CourseTreatment,
                DateDischarge = model.DateDischarge,
                DateReport = model.DateReport,
                FollowDischarge = model.FollowDischarge,
                PhysicallyUnstable = model.PhysicallyUnstable,
                Planned = model.Planned,
                ReasonDischarge = model.ReasonDischarge,
                ReferralAgency1 = model.ReferralAgency1,
                ReferralAgency2 = model.ReferralAgency2,
                ReferralContactPersonal1 = model.ReferralContactPersonal1,
                ReferralContactPersonal2 = model.ReferralContactPersonal2,
                ReferralFor1 = model.ReferralFor1,
                ReferralFor2 = model.ReferralFor2,
                ReferralHoursOperation1 = model.ReferralHoursOperation1,
                ReferralHoursOperation2 = model.ReferralHoursOperation2,
                ReferralPhone1 = model.ReferralPhone1,
                ReferralPhone2 = model.ReferralPhone2,
                TreatmentPlanObjCumpl = model.TreatmentPlanObjCumpl,
                Others = model.Others,
                Hospitalization = model.Hospitalization,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignaturePerson = model.DateSignaturePerson,
            };
        }

        public DischargeViewModel ToDischargeViewModel(DischargeEntity model)
        {
            return new DischargeViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                AdmissionedFor = model.AdmissionedFor,
                AgencyDischargeClient = model.AgencyDischargeClient,
                BriefHistory = model.BriefHistory,
                ClientDeceased = model.ClientDeceased,
                ClientDischargeAgainst = model.ClientDischargeAgainst,
                ClientMoved = model.ClientMoved,
                ClientReferred = model.ClientReferred,
                ConditionalDischarge = model.ConditionalDischarge,
                CourseTreatment = model.CourseTreatment,
                DateDischarge = model.DateDischarge,
                DateReport = model.DateReport,
                FollowDischarge = model.FollowDischarge,
                PhysicallyUnstable = model.PhysicallyUnstable,
                Planned = model.Planned,
                ReasonDischarge = model.ReasonDischarge,
                ReferralAgency1 = model.ReferralAgency1,
                ReferralAgency2 = model.ReferralAgency2,
                ReferralContactPersonal1 = model.ReferralContactPersonal1,
                ReferralContactPersonal2 = model.ReferralContactPersonal2,
                ReferralFor1 = model.ReferralFor1,
                ReferralFor2 = model.ReferralFor2,
                ReferralHoursOperation1 = model.ReferralHoursOperation1,
                ReferralHoursOperation2 = model.ReferralHoursOperation2,
                ReferralPhone1 = model.ReferralPhone1,
                ReferralPhone2 = model.ReferralPhone2,
                TreatmentPlanObjCumpl = model.TreatmentPlanObjCumpl,
                Others = model.Others,
                Hospitalization = model.Hospitalization,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignaturePerson = model.DateSignaturePerson,
            };

        }

        public async Task<MedicationEntity> ToMedicationEntity(MedicationViewModel model, bool isNew)
        {
            return new MedicationEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
                Dosage = model.Dosage,
                Name = model.Name,
                Frequency = model.Frequency,
                Prescriber = model.Prescriber,
            };
        }

        public MedicationViewModel ToMedicationViewModel(MedicationEntity model)
        {
            return new MedicationViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Dosage = model.Dosage,
                Name = model.Name,
                Frequency = model.Frequency,
                Prescriber = model.Prescriber,
            };

        }

        public async Task<FarsFormEntity> ToFarsFormEntity(FarsFormViewModel model, bool isNew)
        {
            return new FarsFormEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
                AbilityScale = model.AbilityScale,
                ActivitiesScale = model.ActivitiesScale,
                AdmissionedFor = model.AdmissionedFor,
                AnxietyScale = model.AnxietyScale,
                CognitiveScale  = model.CognitiveScale,
                ContID1 = model.ContID1,
                ContID2 = model.ContID2,
                ContID3 = model.ContID3,
                ContractorID = model.ContractorID,
                Country = model.Country,
                DangerToOtherScale = model.DangerToOtherScale,
                DangerToSelfScale = model.DangerToSelfScale,
                DcfEvaluation = model.DcfEvaluation,
                DepressionScale = model.DepressionScale,
                EvaluationDate = model.EvaluationDate,
                FamilyEnvironmentScale = model.FamilyEnvironmentScale,
                FamilyRelationShipsScale = model.FamilyRelationShipsScale,
                HyperAffectScale = model.HyperAffectScale,
                InterpersonalScale = model.InterpersonalScale,
                MCOID = model.MCOID,
                MedicaidProviderID = model.MedicaidProviderID,
                MedicaidRecipientID = model.MedicaidRecipientID,
                MedicalScale = model.MedicalScale,
                M_GafScore = model.M_GafScore,
                ProgramEvaluation = model.ProgramEvaluation,
                ProviderId = model.ProviderId,
                ProviderLocal = model.ProviderLocal,
                RaterEducation = model.RaterEducation,
                RaterFMHI = model.RaterFMHI,
                SecurityScale = model.SecurityScale,
                SignatureDate = model.SignatureDate,
                SocialScale = model.SocialScale,
                SubstanceAbusoHistory = model.SubstanceAbusoHistory,
                SubstanceScale = model.SubstanceScale,
                ThoughtProcessScale = model.ThoughtProcessScale,
                TraumaticsScale = model.TraumaticsScale,
                WorkScale = model.WorkScale,

            };
        }

        public FarsFormViewModel ToFarsFormViewModel(FarsFormEntity model)
        {
            return new FarsFormViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                AbilityScale = model.AbilityScale,
                ActivitiesScale = model.ActivitiesScale,
                AdmissionedFor = model.AdmissionedFor,
                AnxietyScale = model.AnxietyScale,
                CognitiveScale = model.CognitiveScale,
                ContID1 = model.ContID1,
                ContID2 = model.ContID2,
                ContID3 = model.ContID3,
                ContractorID = model.ContractorID,
                Country = model.Country,
                DangerToOtherScale = model.DangerToOtherScale,
                DangerToSelfScale = model.DangerToSelfScale,
                DcfEvaluation = model.DcfEvaluation,
                DepressionScale = model.DepressionScale,
                EvaluationDate = model.EvaluationDate,
                FamilyEnvironmentScale = model.FamilyEnvironmentScale,
                FamilyRelationShipsScale = model.FamilyRelationShipsScale,
                HyperAffectScale = model.HyperAffectScale,
                InterpersonalScale = model.InterpersonalScale,
                MCOID = model.MCOID,
                MedicaidProviderID = model.MedicaidProviderID,
                MedicaidRecipientID = model.MedicaidRecipientID,
                MedicalScale = model.MedicalScale,
                M_GafScore = model.M_GafScore,
                ProgramEvaluation = model.ProgramEvaluation,
                ProviderId = model.ProviderId,
                ProviderLocal = model.ProviderLocal,
                RaterEducation = model.RaterEducation,
                RaterFMHI = model.RaterFMHI,
                SecurityScale = model.SecurityScale,
                SignatureDate = model.SignatureDate,
                SocialScale = model.SocialScale,
                SubstanceAbusoHistory = model.SubstanceAbusoHistory,
                SubstanceScale = model.SubstanceScale,
                ThoughtProcessScale = model.ThoughtProcessScale,
                TraumaticsScale = model.TraumaticsScale,
                WorkScale = model.WorkScale,
                
            };

        }
    }
}
