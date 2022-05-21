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
                FaxNo = model.FaxNo,
                ClinicalDirector = model.ClinicalDirector
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
                FaxNo = clinicEntity.FaxNo,
                ClinicalDirector = clinicEntity.ClinicalDirector
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
                SignaturePath = supervisorEntity.SignaturePath
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
                Active = isNew ? true : model.Active,
                AdditionalRecommended = model.AdditionalRecommended,
                AdmissionDateMTP = model.AdmissionDateMTP,
                ClientLimitation = model.ClientLimitation,
                ClientStrengths = model.ClientStrengths,
                DateOfUpdate = model.DateOfUpdate,
                Family = model.Family,
                FamilyCode = model.FamilyCode,
                FamilyDuration = model.FamilyDuration,
                FamilyFrecuency = model.FamilyFrecuency,
                FamilyUnits = model.FamilyUnits,
                Group = model.Group,
                GroupCode = model.GroupCode,
                GroupDuration = model.GroupDuration,
                GroupFrecuency = model.GroupFrecuency,
                GroupUnits = model.GroupUnits,
                Health = model.Health,
                HealthWhere = model.HealthWhere,
                Individual = model.Individual,
                IndividualCode = model.IndividualCode,
                IndividualDuration = model.IndividualDuration,
                IndividualFrecuency = model.IndividualFrecuency,
                IndividualUnits = model.IndividualUnits,
                Legal = model.Legal,
                LegalWhere = model.LegalWhere,
                Medication = model.Medication,
                MedicationCode = model.MedicationCode,
                MedicationDuration = model.MedicationDuration,
                MedicationFrecuency = model.MedicationFrecuency,
                MedicationUnits = model.MedicationUnits,
                Other = model.Other,
                OtherWhere = model.OtherWhere,
                Paint = model.Paint,
                PaintWhere = model.PaintWhere,
                Psychosocial = model.Psychosocial,
                PsychosocialCode = model.PsychosocialCode,
                PsychosocialDuration = model.PsychosocialDuration,
                PsychosocialFrecuency = model.PsychosocialFrecuency,
                PsychosocialUnits = model.PsychosocialUnits,
                RationaleForUpdate = model.RationaleForUpdate,
                Setting = model.Setting,
                Substance = model.Substance,
                SubstanceWhere = model.SubstanceWhere

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
                Active = mtpEntity.Active,
                AdditionalRecommended = mtpEntity.AdditionalRecommended,
                AdmissionDateMTP = mtpEntity.AdmissionDateMTP,
                ClientLimitation = mtpEntity.ClientLimitation,
                ClientStrengths = mtpEntity.ClientStrengths,
                DateOfUpdate = mtpEntity.DateOfUpdate,
                Family = mtpEntity.Family,
                FamilyCode = mtpEntity.FamilyCode,
                FamilyDuration = mtpEntity.FamilyDuration,
                FamilyFrecuency = mtpEntity.FamilyFrecuency,
                FamilyUnits = mtpEntity.FamilyUnits,
                Group = mtpEntity.Group,
                GroupCode = mtpEntity.GroupCode,
                GroupDuration = mtpEntity.GroupDuration,
                GroupFrecuency = mtpEntity.GroupFrecuency,
                GroupUnits = mtpEntity.GroupUnits,
                Health = mtpEntity.Health,
                HealthWhere = mtpEntity.HealthWhere,
                Individual = mtpEntity.Individual,
                IndividualCode = mtpEntity.IndividualCode,
                IndividualDuration = mtpEntity.IndividualDuration,
                IndividualFrecuency = mtpEntity.IndividualFrecuency,
                IndividualUnits = mtpEntity.IndividualUnits,
                Legal = mtpEntity.Legal,
                LegalWhere = mtpEntity.LegalWhere,
                Medication = mtpEntity.Medication,
                MedicationCode = mtpEntity.MedicationCode,
                MedicationDuration = mtpEntity.MedicationDuration,
                MedicationFrecuency = mtpEntity.MedicationFrecuency,
                MedicationUnits = mtpEntity.MedicationUnits,
                Other = mtpEntity.Other,
                OtherWhere = mtpEntity.OtherWhere,
                Paint = mtpEntity.Paint,
                PaintWhere = mtpEntity.PaintWhere,
                Psychosocial = mtpEntity.Psychosocial,
                PsychosocialCode = mtpEntity.PsychosocialCode,
                PsychosocialDuration = mtpEntity.PsychosocialDuration,
                PsychosocialFrecuency = mtpEntity.PsychosocialFrecuency,
                PsychosocialUnits = mtpEntity.PsychosocialUnits,
                RationaleForUpdate = mtpEntity.RationaleForUpdate,
                Substance = mtpEntity.Substance,
                SubstanceWhere = mtpEntity.SubstanceWhere,
                Client = mtpEntity.Client
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
                Service = ServiceUtils.GetServiceByIndex(model.IdService),
                Adendum = await _context.Adendums.FindAsync(model.IdAdendum),
                Compliment = model.Compliment,
                Compliment_Date = model.Compliment_Date,
                Compliment_Explain = model.Compliment_Explain,
                Compliment_IdMTPReview = model.Compliment_IdMTPReview,
                IdMTPReview = model.IdMTPReview

            };
        }

        public GoalViewModel ToGoalViewModel(GoalEntity goalEntity)
        {
            GoalViewModel model;
            model = new GoalViewModel
            {
                Id = goalEntity.Id,
                Number = goalEntity.Number,
                MTP = goalEntity.MTP,
                IdMTP = goalEntity.MTP.Id,
                Name = goalEntity.Name,
                AreaOfFocus = goalEntity.AreaOfFocus,
                IdService = Convert.ToInt32(goalEntity.Service),
                Services = _combosHelper.GetComboServices(),
                Compliment = goalEntity.Compliment,
                Compliment_Date = goalEntity.Compliment_Date,
                Compliment_Explain = goalEntity.Compliment_Explain,
                Compliment_IdMTPReview = goalEntity.Compliment_IdMTPReview,
                IdMTPReview = goalEntity.IdMTPReview,

                
            };
            if (goalEntity.Adendum != null)
            {
                model.IdAdendum = goalEntity.Adendum.Id;
               
            }

            return model;
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
                Goal = await _context.Goals.FindAsync(model.IdGoal),
                Compliment = model.Compliment,
                Compliment_Date = model.Compliment_Date,
                Compliment_Explain = model.Compliment_Explain,
                Compliment_IdMTPReview = model.Compliment_IdMTPReview,
                IdMTPReview = model.IdMTPReview
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
                Intervention = objectiveEntity.Intervention,
                Compliment = objectiveEntity.Compliment,
                Compliment_Date = objectiveEntity.Compliment_Date,
                Compliment_Explain = objectiveEntity.Compliment_Explain,
                Compliment_IdMTPReview = objectiveEntity.Compliment_IdMTPReview,
                IdMTPReview = objectiveEntity.IdMTPReview
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

                AdmissionedFor = model.AdmissionedFor,
                

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
                
                AdmissionedFor = model.AdmissionedFor,

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

        public async Task<DischargeEntity> ToDischargeEntity(DischargeViewModel model, bool isNew, string userId)
        {
            return new DischargeEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
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
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                Others_Explain = model.Others_Explain,
                Status = model.Status,
                Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == model.IdSupervisor),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public DischargeViewModel ToDischargeViewModel(DischargeEntity model)
        {
            DischargeViewModel salida;
            salida = new DischargeViewModel
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
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                Others_Explain = model.Others_Explain,
                Status = model.Status,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn

            };

            if (model.Supervisor != null)
                salida.IdSupervisor = model.Supervisor.Id;
            else
                salida.IdSupervisor = 0;
            return salida;
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
                Prescriber = model.Prescriber
                
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
                Prescriber = model.Prescriber
            };

        }

        public async Task<FarsFormEntity> ToFarsFormEntity(FarsFormViewModel model, bool isNew, string userId)
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
                Status = model.Status,
                Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == model.IdSupervisor),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public FarsFormViewModel ToFarsFormViewModel(FarsFormEntity model)
        {
            FarsFormViewModel salida;
            salida = new FarsFormViewModel
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
                Status = model.Status,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn

            };
            
            if (model.Supervisor != null)
                salida.IdSupervisor = model.Supervisor.Id;
            else
                salida.IdSupervisor = 0;
            return salida;

        }

        public async Task<BioEntity> ToBioEntity(BioViewModel model, bool isNew)
        {
            return new BioEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
                AdultCurrentExperience = model.AdultCurrentExperience,
                Affect_Angry = model.Affect_Angry,
                Affect_Anxious = model.Affect_Anxious,
                Affect_Appropriate = model.Affect_Appropriate,
                Affect_Blunted = model.Affect_Blunted,
                Affect_Constricted = model.Affect_Constricted,
                Affect_Expansive = model.Affect_Expansive,
                Affect_Flat = model.Affect_Flat,
                Affect_labile = model.Affect_labile,
                Affect_Other = model.Affect_Other,
                Affect_Tearful_Sad = model.Affect_Tearful_Sad,
                AlternativeDiagnosis = model.AlternativeDiagnosis,
                Appearance_Bizarre = model.Appearance_Bizarre,
                Appearance_Cleaned = model.Appearance_Cleaned,
                Appearance_Disheveled = model.Appearance_Disheveled,
                Appearance_FairHygiene = model.Appearance_FairHygiene,
                Appearance_WellGroomed = model.Appearance_WellGroomed,
                Appetite = BioType.GetBioAppetiteByIndex(model.IdIfSexuallyActive),
                ApproximateDateReport = model.ApproximateDateReport,
                ApproximateDateReport_Where = model.ApproximateDateReport_Where,
                AReferral = model.AReferral,
                AReferral_Services = model.AReferral_Services,
                AReferral_When = model.AReferral_When,
                AReferral_Where = model.AReferral_Where,
                BioH0031HN = model.BioH0031HN,
                IDAH0031HO = model.IDAH0031HO,
                CanClientFollow = model.CanClientFollow,
                Children = model.Children,
                ClientAssessmentSituation = model.ClientAssessmentSituation,
                ClientFamilyAbusoTrauma = model.ClientFamilyAbusoTrauma,
                CMH = model.CMH,
                Comments = model.Comments,
                DateAbuse = model.DateAbuse,
                DateBio = model.DateBio,
                DateSignatureLicensedPractitioner = model.DateSignatureLicensedPractitioner,
                DateSignaturePerson = model.DateSignaturePerson,
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                DateSignatureUnlicensedTherapist = model.DateSignatureUnlicensedTherapist,
                Details = model.Details,
                DoesClient = model.DoesClient,
                DoesClientRequired = model.DoesClientRequired,
                DoesClientRequired_Where = model.DoesClientRequired_Where,
                DoesNotAlways = model.DoesNotAlways,
                DoesTheClientExperience = model.DoesTheClientExperience,
                DoesTheClientExperience_Where = model.DoesTheClientExperience_Where,
                DoYouHaveAnyPhysical = model.DoYouHaveAnyPhysical,
                DoYouHaveAnyReligious = model.DoYouHaveAnyReligious,
                DoYouHaveAnyVisual = model.DoYouHaveAnyVisual,
                DoYouOwn = model.DoYouOwn,
                DoYouOwn_Explain = model.DoYouOwn_Explain,
                EastAlone = model.EastAlone,
                EastFew = model.EastFew,
                EastFewer = model.EastFewer,
                FamilyAssessmentSituation = model.FamilyAssessmentSituation,
                FamilyEmotional = model.FamilyEmotional,
                GeneralDescription = model.GeneralDescription,
                Has3OrMore = model.Has3OrMore,
                HasAnIllnes = model.HasAnIllnes,
                HasClientBeenTreatedPain = model.HasClientBeenTreatedPain,
                HasClientBeenTreatedPain_Ifnot = model.HasClientBeenTreatedPain_Ifnot,
                HasClientBeenTreatedPain_PleaseIncludeService = model.HasClientBeenTreatedPain_PleaseIncludeService,
                HasClientBeenTreatedPain_Where = model.HasClientBeenTreatedPain_Where,
                HasTheClient = model.HasTheClient,
                HasTheClientVisitedPhysician = model.HasTheClientVisitedPhysician,
                HasTheClientVisitedPhysician_Date = model.HasTheClientVisitedPhysician_Date,
                HasTheClientVisitedPhysician_Reason = model.HasTheClientVisitedPhysician_Reason,
                HasTheClient_Explain = model.HasTheClient_Explain,
                HasTooth = model.HasTooth,
                HaveYouEverBeen = model.HaveYouEverBeen,
                HaveYouEverBeen_Explain = model.HaveYouEverBeen_Explain,
                HaveYouEverThought = model.HaveYouEverThought,
                HaveYouEverThought_Explain = model.HaveYouEverThought_Explain,
                HigHestEducation = model.HigHestEducation,
                Hydration = BioType.GetBioHydrationByIndex(model.IdHydratation),
                IConcurWhitDiagnistic = model.IConcurWhitDiagnistic,
                If6_Date = model.If6_Date,
                If6_ReferredTo = model.If6_ReferredTo,
                IfForeing_AgeArrival = model.IfForeing_AgeArrival,
                IfForeing_Born = model.IfForeing_Born,
                IfForeing_YearArrival = model.IfForeing_YearArrival,
                IfMarried = model.IfMarried,
                IfSeparated = model.IfSeparated,
                IfSexuallyActive = BioType.GetBioIfSexuallyActiveByIndex(model.IdIfSexuallyActive),
                Insight_Fair = model.Insight_Fair,
                Insight_Good = model.Insight_Good,
                Insight_Other = model.Insight_Other,
                Insight_Poor = model.Insight_Poor,
                Judgment_Fair = model.Judgment_Fair,
                Judgment_Good = model.Judgment_Good,
                Judgment_Other = model.Judgment_Other,
                Judgment_Poor = model.Judgment_Poor,
                Lacking_Location = model.Lacking_Location,
                Lacking_Person = model.Lacking_Person,
                Lacking_Place = model.Lacking_Place,
                Lacking_Time = model.Lacking_Time,
                LegalAssessment = model.LegalAssessment,
                LegalHistory = model.LegalHistory,
                LicensedPractitioner = model.LicensedPractitioner,
                MaritalStatus = model.MaritalStatus,
                Mood_Angry = model.Mood_Angry,
                Mood_Anxious = model.Mood_Anxious,
                Mood_Depressed = model.Mood_Depressed,
                Mood_Euphoric = model.Mood_Euphoric,
                Mood_Euthymic = model.Mood_Euthymic,
                Mood_Maniac = model.Mood_Maniac,
                Mood_Other = model.Mood_Other,
                Motor_Agitated = model.Motor_Agitated,
                Motor_Akathisia = model.Motor_Akathisia,
                Motor_Normal = model.Motor_Normal,
                Motor_Other = model.Motor_Other,
                Motor_RestLess = model.Motor_RestLess,
                Motor_Retardation = model.Motor_Retardation,
                Motor_Tremor = model.Motor_Tremor,
                NotAlwaysPhysically = model.NotAlwaysPhysically,
                ObtainRelease = model.ObtainRelease,
                ObtainReleaseInformation = model.ObtainReleaseInformation,
                ObtainReleaseInformation7 = model.ObtainReleaseInformation7,
                Oriented_FullOriented = model.Oriented_FullOriented,
                Outcome = model.Outcome,
                PersonalFamilyPsychiatric = model.PersonalFamilyPsychiatric,
                PersonInvolved = model.PersonInvolved,
                PleaseProvideGoal = model.PleaseProvideGoal,
                PleaseRatePain = model.PleaseRatePain,
                PresentingProblem = model.PresentingProblem,
                PrimaryLocation = model.PrimaryLocation,
                Priv = model.Priv,
                ProvideIntegratedSummary = model.ProvideIntegratedSummary,
                RecentWeight = BioType.GetBioRecentWeightChangeByIndex(model.IdRecentWeight),
                RelationShips = model.RelationShips,
                RelationshipWithFamily = model.RelationshipWithFamily,
                RiskToOther_Chronic = model.RiskToOther_Chronic,
                RiskToOther_High = model.RiskToOther_High,
                RiskToOther_Low = model.RiskToOther_Low,
                RiskToOther_Medium = model.RiskToOther_Medium,
                RiskToSelf_Chronic = model.RiskToSelf_Chronic,
                RiskToSelf_High = model.RiskToSelf_High,
                RiskToSelf_Low = model.RiskToSelf_Low,
                RiskToSelf_Medium = model.RiskToSelf_Medium,
                SafetyPlan = model.SafetyPlan,
                Setting = model.Setting,
                Speech_Impoverished = model.Speech_Impoverished,
                Speech_Loud = model.Speech_Loud,
                Speech_Mumbled = model.Speech_Mumbled,
                Speech_Normal = model.Speech_Normal,
                Speech_Other = model.Speech_Other,
                Speech_Pressured = model.Speech_Pressured,
                Speech_Rapid = model.Speech_Rapid,
                Speech_Slow = model.Speech_Slow,
                Speech_Slurred = model.Speech_Slurred,
                Speech_Stutters = model.Speech_Stutters,
                SubstanceAbuse = model.SubstanceAbuse,
                Takes3OrMore = model.Takes3OrMore,
                ThoughtContent_Delusions = model.ThoughtContent_Delusions,
                ThoughtContent_Delusions_Type = model.ThoughtContent_Delusions_Type,
                ThoughtContent_Hallucinations = model.ThoughtContent_Hallucinations,
                ThoughtContent_Hallucinations_Type = model.ThoughtContent_Hallucinations_Type,
                ThoughtContent_RealityBased = model.ThoughtContent_RealityBased,
                ThoughtContent_Relevant = model.ThoughtContent_Relevant,
                ThoughtProcess_Blocking = model.ThoughtProcess_Blocking,
                ThoughtProcess_Circumstantial = model.ThoughtProcess_Circumstantial,
                ThoughtProcess_Disorganized = model.ThoughtProcess_Disorganized,
                ThoughtProcess_FightIdeas = model.ThoughtProcess_FightIdeas,
                ThoughtProcess_GoalDirected = model.ThoughtProcess_GoalDirected,
                ThoughtProcess_Irrational = model.ThoughtProcess_Irrational,
                ThoughtProcess_LooseAssociations = model.ThoughtProcess_LooseAssociations,
                ThoughtProcess_Obsessive = model.ThoughtProcess_Obsessive,
                ThoughtProcess_Organized = model.ThoughtProcess_Organized,
                ThoughtProcess_Other = model.ThoughtProcess_Other,
                ThoughtProcess_Preoccupied = model.ThoughtProcess_Preoccupied,
                ThoughtProcess_Rigid = model.ThoughtProcess_Rigid,
                ThoughtProcess_Tangential = model.ThoughtProcess_Tangential,
                TreatmentNeeds = model.TreatmentNeeds,
                Treatmentrecomendations = model.Treatmentrecomendations,
                UnlicensedTherapist = model.UnlicensedTherapist,
                WhatIsTheClient = model.WhatIsTheClient,
                WhatIsYourLanguage = model.WhatIsYourLanguage,
                WhereRecord = model.WhereRecord,
                WhereRecord_When = model.WhereRecord_When,
                WhereRecord_Where = model.WhereRecord_Where,
                WithoutWanting = model.WithoutWanting,
                Client_FK = model.Client_FK,
                ClientDenied = model.ClientDenied,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                ForHowLong = model.ForHowLong
                
            };
        }

        public BioViewModel ToBioViewModel(BioEntity model)
        {
            return new BioViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                AdultCurrentExperience = model.AdultCurrentExperience,
                Affect_Angry = model.Affect_Angry,
                Affect_Anxious = model.Affect_Anxious,
                Affect_Appropriate = model.Affect_Appropriate,
                Affect_Blunted = model.Affect_Blunted,
                Affect_Constricted = model.Affect_Constricted,
                Affect_Expansive = model.Affect_Expansive,
                Affect_Flat = model.Affect_Flat,
                Affect_labile = model.Affect_labile,
                Affect_Other = model.Affect_Other,
                Affect_Tearful_Sad = model.Affect_Tearful_Sad,
                AlternativeDiagnosis = model.AlternativeDiagnosis,
                Appearance_Bizarre = model.Appearance_Bizarre,
                Appearance_Cleaned = model.Appearance_Cleaned,
                Appearance_Disheveled = model.Appearance_Disheveled,
                Appearance_FairHygiene = model.Appearance_FairHygiene,
                Appearance_WellGroomed = model.Appearance_WellGroomed,
                Appetite = model.Appetite,
                ApproximateDateReport = model.ApproximateDateReport,
                ApproximateDateReport_Where = model.ApproximateDateReport_Where,
                AReferral = model.AReferral,
                AReferral_Services = model.AReferral_Services,
                AReferral_When = model.AReferral_When,
                AReferral_Where = model.AReferral_Where,
                BioH0031HN = model.BioH0031HN,
                IDAH0031HO = model.IDAH0031HO,
                CanClientFollow = model.CanClientFollow,
                Children = model.Children,
                ClientAssessmentSituation = model.ClientAssessmentSituation,
                ClientFamilyAbusoTrauma = model.ClientFamilyAbusoTrauma,
                CMH = model.CMH,
                Comments = model.Comments,
                DateAbuse = model.DateAbuse,
                DateBio = model.DateBio,
                DateSignatureLicensedPractitioner = model.DateSignatureLicensedPractitioner,
                DateSignaturePerson = model.DateSignaturePerson,
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                DateSignatureUnlicensedTherapist = model.DateSignatureUnlicensedTherapist,
                Details = model.Details,
                DoesClient = model.DoesClient,
                DoesClientRequired = model.DoesClientRequired,
                DoesClientRequired_Where = model.DoesClientRequired_Where,
                DoesNotAlways = model.DoesNotAlways,
                DoesTheClientExperience = model.DoesTheClientExperience,
                DoesTheClientExperience_Where = model.DoesTheClientExperience_Where,
                DoYouHaveAnyPhysical = model.DoYouHaveAnyPhysical,
                DoYouHaveAnyReligious = model.DoYouHaveAnyReligious,
                DoYouHaveAnyVisual = model.DoYouHaveAnyVisual,
                DoYouOwn = model.DoYouOwn,
                DoYouOwn_Explain = model.DoYouOwn_Explain,
                EastAlone = model.EastAlone,
                EastFew = model.EastFew,
                EastFewer = model.EastFewer,
                FamilyAssessmentSituation = model.FamilyAssessmentSituation,
                FamilyEmotional = model.FamilyEmotional,
                GeneralDescription = model.GeneralDescription,
                Has3OrMore = model.Has3OrMore,
                HasAnIllnes = model.HasAnIllnes,
                HasClientBeenTreatedPain = model.HasClientBeenTreatedPain,
                HasClientBeenTreatedPain_Ifnot = model.HasClientBeenTreatedPain_Ifnot,
                HasClientBeenTreatedPain_PleaseIncludeService = model.HasClientBeenTreatedPain_PleaseIncludeService,
                HasClientBeenTreatedPain_Where = model.HasClientBeenTreatedPain_Where,
                HasTheClient = model.HasTheClient,
                HasTheClientVisitedPhysician = model.HasTheClientVisitedPhysician,
                HasTheClientVisitedPhysician_Date = model.HasTheClientVisitedPhysician_Date,
                HasTheClientVisitedPhysician_Reason = model.HasTheClientVisitedPhysician_Reason,
                HasTheClient_Explain = model.HasTheClient_Explain,
                HasTooth = model.HasTooth,
                HaveYouEverBeen = model.HaveYouEverBeen,
                HaveYouEverBeen_Explain = model.HaveYouEverBeen_Explain,
                HaveYouEverThought = model.HaveYouEverThought,
                HaveYouEverThought_Explain = model.HaveYouEverThought_Explain,
                HigHestEducation = model.HigHestEducation,
                Hydration = model.Hydration,
                IConcurWhitDiagnistic = model.IConcurWhitDiagnistic,
                If6_Date = model.If6_Date,
                If6_ReferredTo = model.If6_ReferredTo,
                IfForeing_AgeArrival = model.IfForeing_AgeArrival,
                IfForeing_Born = model.IfForeing_Born,
                IfForeing_YearArrival = model.IfForeing_YearArrival,
                IfMarried = model.IfMarried,
                IfSeparated = model.IfSeparated,
                IfSexuallyActive = model.IfSexuallyActive,
                Insight_Fair = model.Insight_Fair,
                Insight_Good = model.Insight_Good,
                Insight_Other = model.Insight_Other,
                Insight_Poor = model.Insight_Poor,
                Judgment_Fair = model.Judgment_Fair,
                Judgment_Good = model.Judgment_Good,
                Judgment_Other = model.Judgment_Other,
                Judgment_Poor = model.Judgment_Poor,
                Lacking_Location = model.Lacking_Location,
                Lacking_Person = model.Lacking_Person,
                Lacking_Place = model.Lacking_Place,
                Lacking_Time = model.Lacking_Time,
                LegalAssessment = model.LegalAssessment,
                LegalHistory = model.LegalHistory,
                LicensedPractitioner = model.LicensedPractitioner,
                MaritalStatus = model.MaritalStatus,
                Mood_Angry = model.Mood_Angry,
                Mood_Anxious = model.Mood_Anxious,
                Mood_Depressed = model.Mood_Depressed,
                Mood_Euphoric = model.Mood_Euphoric,
                Mood_Euthymic = model.Mood_Euthymic,
                Mood_Maniac = model.Mood_Maniac,
                Mood_Other = model.Mood_Other,
                Motor_Agitated = model.Motor_Agitated,
                Motor_Akathisia = model.Motor_Akathisia,
                Motor_Normal = model.Motor_Normal,
                Motor_Other = model.Motor_Other,
                Motor_RestLess = model.Motor_RestLess,
                Motor_Retardation = model.Motor_Retardation,
                Motor_Tremor = model.Motor_Tremor,
                NotAlwaysPhysically = model.NotAlwaysPhysically,
                ObtainRelease = model.ObtainRelease,
                ObtainReleaseInformation = model.ObtainReleaseInformation,
                ObtainReleaseInformation7 = model.ObtainReleaseInformation7,
                Oriented_FullOriented = model.Oriented_FullOriented,
                Outcome = model.Outcome,
                PersonalFamilyPsychiatric = model.PersonalFamilyPsychiatric,
                PersonInvolved = model.PersonInvolved,
                PleaseProvideGoal = model.PleaseProvideGoal,
                PleaseRatePain = model.PleaseRatePain,
                PresentingProblem = model.PresentingProblem,
                PrimaryLocation = model.PrimaryLocation,
                Priv = model.Priv,
                ProvideIntegratedSummary = model.ProvideIntegratedSummary,
                RecentWeight = model.RecentWeight,
                RelationShips = model.RelationShips,
                RelationshipWithFamily = model.RelationshipWithFamily,
                RiskToOther_Chronic = model.RiskToOther_Chronic,
                RiskToOther_High = model.RiskToOther_High,
                RiskToOther_Low = model.RiskToOther_Low,
                RiskToOther_Medium = model.RiskToOther_Medium,
                RiskToSelf_Chronic = model.RiskToSelf_Chronic,
                RiskToSelf_High = model.RiskToSelf_High,
                RiskToSelf_Low = model.RiskToSelf_Low,
                RiskToSelf_Medium = model.RiskToSelf_Medium,
                SafetyPlan = model.SafetyPlan,
                Setting = model.Setting,
                Speech_Impoverished = model.Speech_Impoverished,
                Speech_Loud = model.Speech_Loud,
                Speech_Mumbled = model.Speech_Mumbled,
                Speech_Normal = model.Speech_Normal,
                Speech_Other = model.Speech_Other,
                Speech_Pressured = model.Speech_Pressured,
                Speech_Rapid = model.Speech_Rapid,
                Speech_Slow = model.Speech_Slow,
                Speech_Slurred = model.Speech_Slurred,
                Speech_Stutters = model.Speech_Stutters,
                SubstanceAbuse = model.SubstanceAbuse,
                Takes3OrMore = model.Takes3OrMore,
                ThoughtContent_Delusions = model.ThoughtContent_Delusions,
                ThoughtContent_Delusions_Type = model.ThoughtContent_Delusions_Type,
                ThoughtContent_Hallucinations = model.ThoughtContent_Hallucinations,
                ThoughtContent_Hallucinations_Type = model.ThoughtContent_Hallucinations_Type,
                ThoughtContent_RealityBased = model.ThoughtContent_RealityBased,
                ThoughtContent_Relevant = model.ThoughtContent_Relevant,
                ThoughtProcess_Blocking = model.ThoughtProcess_Blocking,
                ThoughtProcess_Circumstantial = model.ThoughtProcess_Circumstantial,
                ThoughtProcess_Disorganized = model.ThoughtProcess_Disorganized,
                ThoughtProcess_FightIdeas = model.ThoughtProcess_FightIdeas,
                ThoughtProcess_GoalDirected = model.ThoughtProcess_GoalDirected,
                ThoughtProcess_Irrational = model.ThoughtProcess_Irrational,
                ThoughtProcess_LooseAssociations = model.ThoughtProcess_LooseAssociations,
                ThoughtProcess_Obsessive = model.ThoughtProcess_Obsessive,
                ThoughtProcess_Organized = model.ThoughtProcess_Organized,
                ThoughtProcess_Other = model.ThoughtProcess_Other,
                ThoughtProcess_Preoccupied = model.ThoughtProcess_Preoccupied,
                ThoughtProcess_Rigid = model.ThoughtProcess_Rigid,
                ThoughtProcess_Tangential = model.ThoughtProcess_Tangential,
                TreatmentNeeds = model.TreatmentNeeds,
                Treatmentrecomendations = model.Treatmentrecomendations,
                UnlicensedTherapist = model.UnlicensedTherapist,
                WhatIsTheClient = model.WhatIsTheClient,
                WhatIsYourLanguage = model.WhatIsYourLanguage,
                WhereRecord = model.WhereRecord,
                WhereRecord_When = model.WhereRecord_When,
                WhereRecord_Where = model.WhereRecord_Where,
                WithoutWanting = model.WithoutWanting,
                Client_FK = model.Client_FK,
                ClientDenied = model.ClientDenied,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                ForHowLong = model.ForHowLong,
                IdAppetite = Convert.ToInt32(model.Appetite) + 1,
                Appetite_Status = _combosHelper.GetComboBio_Appetite(),
                IdHydratation = Convert.ToInt32(model.Hydration) + 1,
                Hydratation_Status = _combosHelper.GetComboBio_Hydration(),
                IdIfSexuallyActive = Convert.ToInt32(model.IfSexuallyActive) + 1,
                IfSexuallyActive_Status = _combosHelper.GetComboBio_IfSexuallyActive(),
                IdRecentWeight = Convert.ToInt32(model.RecentWeight) + 1,
                RecentWeight_Status = _combosHelper.GetComboBio_RecentWeight()

            };

        }
        public async Task<Bio_BehavioralHistoryEntity> ToBio_BehaviorEntity(Bio_BehavioralHistoryViewModel model, bool isNew)
        {
            return new Bio_BehavioralHistoryEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
                Date = model.Date,
                Problem = model.Problem
            };
        }

        public Bio_BehavioralHistoryViewModel ToBio_BehaviorViewModel(Bio_BehavioralHistoryEntity model)
        {
            return new Bio_BehavioralHistoryViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Date = model.Date,
                Problem = model.Problem
            };

        }

        public async Task<AdendumEntity> ToAdendumEntity(AdendumViewModel model, bool isNew, string userId)
        {
            return new AdendumEntity
            {
                Id = isNew ? 0 : model.Id,
                Dateidentified = model.Dateidentified,
                Duration = model.Duration,
                Frecuency = model.Frecuency,
                ProblemStatement = model.ProblemStatement,
                Status = model.Status,
                Unit = model.Unit,
                Mtp = await _context.MTPs
                                    .Include(m => m.Client)
                                    .ThenInclude(c => c.Clients_Diagnostics)
                                    .ThenInclude(cd => cd.Diagnostic)
                                    .FirstOrDefaultAsync(c => c.Id == model.IdMTP),
                
                Goals = model.Goals,
                Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == model.IdSupervisor),
                Facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.Id == model.IdFacilitator),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)

            };
        }

        public AdendumViewModel ToAdendumViewModel(AdendumEntity model)
        {
            AdendumViewModel salida;
            salida = new AdendumViewModel
            {
                Id = model.Id,
                Dateidentified = model.Dateidentified,
                Duration = model.Duration,
                Frecuency = model.Frecuency,
                ProblemStatement = model.ProblemStatement,
                Status = model.Status,
                Unit = model.Unit,
                IdMTP = model.Mtp.Id,
                Facilitator = model.Facilitator,
                Goals = model.Goals,
                Mtp = model.Mtp,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn

            };
            if (model.Facilitator != null)
                salida.IdFacilitator = model.Facilitator.Id;
            else
                salida.IdFacilitator = 0;
            if (model.Supervisor != null)
                salida.IdSupervisor = model.Supervisor.Id;
            else
                salida.IdSupervisor = 0;
            return salida;
        }

        public async Task<MTPReviewEntity> ToMTPReviewEntity(MTPReviewViewModel model, bool isNew, string userId)
        {
            MTPReviewEntity salida;
            salida = new MTPReviewEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
             
                ACopy = model.ACopy,
                DateClinicalDirector = model.DateClinicalDirector,
                DateLicensedPractitioner = model.DateLicensedPractitioner,
                DateSignaturePerson = model.DateSignaturePerson,
                DateTherapist = model.DateTherapist,
                DescribeAnyGoals = model.DescribeAnyGoals,
                DescribeClient = model.DescribeClient,
                IfCurrent = model.IfCurrent,
                TheTreatmentPlan = model.TheTreatmentPlan,
                MTP_FK = model.MTP_FK,
                NumberUnit = model.NumberUnit,
                ProviderNumber = model.ProviderNumber,
                ReviewedOn = model.ReviewedOn,
                ServiceCode = model.ServiceCode,
                SpecifyChanges = model.SpecifyChanges,
                SummaryOfServices = model.SummaryOfServices,
                TheConsumer = model.TheConsumer,
                ClinicalDirector = model.ClinicalDirector,
                Documents = model.Documents,
                LicensedPractitioner = model.LicensedPractitioner,
                Therapist = model.Therapist,
                Status = model.Status,
                Mtp = await _context.MTPs
                                    .Include(m => m.Client)
                                    .ThenInclude(c => c.Clients_Diagnostics)
                                    .ThenInclude(cd => cd.Diagnostic)
                                    .FirstOrDefaultAsync(c => c.Id == model.IdMTP),
                EndTime = model.EndTime,
                Frecuency = model.Frecuency,
                MonthOfTreatment = model.MonthOfTreatment,
                Setting = model.Setting,
                StartTime = model.StartTime,
                DataOfService = model.DataOfService

            };
            
            return salida;
        }

        public MTPReviewViewModel ToMTPReviewViewModel(MTPReviewEntity model)
        {
            return new MTPReviewViewModel
            {
                Id = model.Id,
                Mtp = model.Mtp,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                IdMTP = model.MTP_FK,
                MTP_FK = model.MTP_FK,

                ACopy = model.ACopy,
                DateClinicalDirector = model.DateClinicalDirector,
                DateLicensedPractitioner = model.DateLicensedPractitioner,
                DateSignaturePerson = model.DateSignaturePerson,
                DateTherapist = model.DateTherapist,
                DescribeAnyGoals = model.DescribeAnyGoals,
                DescribeClient = model.DescribeClient,
                IfCurrent = model.IfCurrent,
                TheTreatmentPlan = model.TheTreatmentPlan,
                NumberUnit = model.NumberUnit,
                ProviderNumber = model.ProviderNumber,
                ReviewedOn = model.ReviewedOn,
                ServiceCode = model.ServiceCode,
                SpecifyChanges = model.SpecifyChanges,
                SummaryOfServices = model.SummaryOfServices,
                TheConsumer = model.TheConsumer,
                ClinicalDirector = model.ClinicalDirector,
                Documents = model.Documents,
                LicensedPractitioner = model.LicensedPractitioner,
                Therapist = model.Therapist,
                Status = model.Status,
                EndTime = model.EndTime,
                Frecuency = model.Frecuency,
                MonthOfTreatment = model.MonthOfTreatment,
                Setting = model.Setting,
                StartTime = model.StartTime,
                DataOfService = model.DataOfService

            };
           
        }
    }
}
