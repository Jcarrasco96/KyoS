using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;

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

        public ClinicEntity ToClinicEntity(ClinicViewModel model, string path, bool isNew, string pathSignatureClinical)
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
                ClinicalDirector = model.ClinicalDirector,
                ProviderMedicaidId = model.ProviderMedicaidId,
                ProviderTaxId = model.ProviderTaxId,
                SignaturePath = pathSignatureClinical,
                CodeGroupTherapy = model.CodeGroupTherapy,
                CodeIndTherapy = model.CodeIndTherapy,
                CodePSRTherapy = model.CodePSRTherapy,
                CodeMTP = model.CodeMTP,
                CodeBIO = model.CodeBIO,
                CodeMTPR = model.CodeMTPR,
                CodeFARS = model.CodeFARS,
                CPTCode_TCM = model.CPTCode_TCM,
                Initials = model.Initials

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
                ClinicalDirector = clinicEntity.ClinicalDirector,
                ProviderMedicaidId = clinicEntity.ProviderMedicaidId,
                ProviderTaxId = clinicEntity.ProviderTaxId,
                SignaturePath = clinicEntity.SignaturePath,
                CodeGroupTherapy = clinicEntity.CodeGroupTherapy,
                CodeIndTherapy = clinicEntity.CodeIndTherapy,
                CodePSRTherapy = clinicEntity.CodePSRTherapy,
                CodeMTP = clinicEntity.CodeMTP,
                CodeBIO = clinicEntity.CodeBIO,
                CodeMTPR = clinicEntity.CodeMTPR,
                CodeFARS = clinicEntity.CodeFARS,
                CPTCode_TCM = clinicEntity.CPTCode_TCM,
                Initials = clinicEntity.Initials

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
                Name = model.Name,
                Service = ThemeUtils.GetThemeByIndex(model.IdService)
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
                Clinics = _combosHelper.GetComboClinics(),
                IdService = Convert.ToInt32(themeEntity.Service),
                Services = _combosHelper.GetComboThemeType(),
                Service = themeEntity.Service
            };
        }

        public async Task<ActivityEntity> ToActivityEntity(ActivityViewModel model, bool isNew, string userId)
        {
            return new ActivityEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Status = ActivityStatus.Pending,
                Theme = await _context.Themes.FindAsync(model.IdTheme),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
            };

        }

        public ActivityViewModel ToActivityViewModel(ActivityEntity activityEntity)
        {
            return new ActivityViewModel
            {
                Id = activityEntity.Id,
                Name = activityEntity.Name,
                Themes = _combosHelper.GetComboThemes(),
                IdTheme = activityEntity.Theme.Id,
                Theme = activityEntity.Theme,
                CreatedBy = activityEntity.CreatedBy,
                CreatedOn = activityEntity.CreatedOn
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
                SignaturePath = signaturePath,
                RaterEducation = model.RaterEducation,
                RaterFMHCertification = model.RaterFMHCertification
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
                SignaturePath = facilitatorEntity.SignaturePath,
                RaterEducation = facilitatorEntity.RaterEducation,
                RaterFMHCertification = facilitatorEntity.RaterFMHCertification
            };
        }

        public async Task<CaseMannagerEntity> ToCaseMannagerEntity(CaseMannagerViewModel model, string signaturePath, bool isNew)
        {
            return new CaseMannagerEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                ProviderNumber = model.ProviderNumber,
                Credentials = model.Credentials,
                Name = isNew ? model.FirstName + ' ' + model.MiddleName + ' ' + model.LastName : model.Name,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                SignaturePath = signaturePath,
                Email = model.Email,
                Phone = model.Phone,
                Money = model.Money,
                TCMSupervisor = await _context.TCMSupervisors.FindAsync(model.IdTCMsupervisor),
                RaterEducation = model.RaterEducation,
                RaterFMHCertification = model.RaterFMHCertification,
                Address = model.Address,
                City = model.City,
                DateOfBirth = model.DateOfBirth,
                FirstName = model.FirstName,
                PaymentMethod = PaymentMethodUtils.GetPaymentMethodByIndex(model.IdPaymentMethod),
                Gender = GenderUtils.GetGenderByIndex(model.IdGender),
                LastName = model.LastName,
                MiddleName = model.MiddleName,
                PH = model.PH,
                State = model.State,
                ZipCode = model.ZipCode,
                AccountNumber = model.AccountNumber,
                AccountType = AccountTypeUtils.GetAccountTypeByIndex(model.IdAccountType),
                CAQH = model.CAQH,
                CompanyEIN = model.CAQH,
                CompanyName = model.CompanyName,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                CredentialExpirationDate = model.CredentialExpirationDate,
                DEALicense = model.DEALicense,
                FinancialInstitutionsName = model.FinancialInstitutionsName,
                HiringDate = model.HiringDate,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                MedicaidProviderID = model.MedicaidProviderID,
                MedicareProviderID = model.MedicareProviderID,
                NPI = model.NPI,
                Routing = model.Routing,
                SSN = model.SSN
            };
        }

        public CaseMannagerViewModel ToCaseMannagerViewModel(CaseMannagerEntity caseMannagerEntity, int idClinic)
        {
            return new CaseMannagerViewModel
            {
                Id = caseMannagerEntity.Id,
                Name = caseMannagerEntity.Name,
                ProviderNumber = caseMannagerEntity.ProviderNumber,
                Credentials = caseMannagerEntity.Credentials,
                IdClinic = caseMannagerEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                IdStatus = (caseMannagerEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus(),
                IdUser = _userHelper.GetIdByUserName(caseMannagerEntity.LinkedUser),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, idClinic),
                SignaturePath = caseMannagerEntity.SignaturePath,
                Email = caseMannagerEntity.Email,
                Phone = caseMannagerEntity.Phone,
                Money = caseMannagerEntity.Money,
                IdTCMsupervisor = caseMannagerEntity.TCMSupervisor == null? 0 : caseMannagerEntity.TCMSupervisor.Id,
                TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(idClinic),
                RaterEducation = caseMannagerEntity.RaterEducation,
                RaterFMHCertification = caseMannagerEntity.RaterFMHCertification,
                Address = caseMannagerEntity.Address,
                City = caseMannagerEntity.City,
                DateOfBirth = caseMannagerEntity.DateOfBirth,
                FirstName = caseMannagerEntity.FirstName,
                PaymentMethod = caseMannagerEntity.PaymentMethod,
                Gender = caseMannagerEntity.Gender,
                LastName = caseMannagerEntity.LastName,
                MiddleName = caseMannagerEntity.MiddleName,
                PH = caseMannagerEntity.PH,
                State = caseMannagerEntity.State,
                ZipCode = caseMannagerEntity.ZipCode,
                AccountNumber = caseMannagerEntity.AccountNumber,
                AccountType = caseMannagerEntity.AccountType,
                CAQH = caseMannagerEntity.CAQH,
                CompanyEIN = caseMannagerEntity.CAQH,
                CompanyName = caseMannagerEntity.CompanyName,
                CreatedBy = caseMannagerEntity.CreatedBy,
                CreatedOn = caseMannagerEntity.CreatedOn,
                CredentialExpirationDate = caseMannagerEntity.CredentialExpirationDate,
                DEALicense = caseMannagerEntity.DEALicense,
                FinancialInstitutionsName = caseMannagerEntity.FinancialInstitutionsName,
                HiringDate = caseMannagerEntity.HiringDate,
                LastModifiedBy = caseMannagerEntity.LastModifiedBy,
                LastModifiedOn = caseMannagerEntity.LastModifiedOn,
                MedicaidProviderID = caseMannagerEntity.MedicaidProviderID,
                MedicareProviderID = caseMannagerEntity.MedicareProviderID,
                NPI = caseMannagerEntity.NPI,
                Routing = caseMannagerEntity.Routing,
                SSN = caseMannagerEntity.SSN,
                IdGender = (caseMannagerEntity.Gender == GenderType.Female) ? 1 : 2,
                GenderList = _combosHelper.GetComboGender(),
                IdAccountType = (caseMannagerEntity.AccountType == AccountType.Personal_Checking) ? 1 : (caseMannagerEntity.AccountType == AccountType.Personal_Saving) ? 2 : (caseMannagerEntity.AccountType == AccountType.Company_Checking) ? 3 : (caseMannagerEntity.AccountType == AccountType.Company_Saving) ? 4 : 0,
                AccountTypeList = _combosHelper.GetComboAccountType(),
                IdPaymentMethod = (caseMannagerEntity.PaymentMethod == PaymentMethod.Check) ? 1 : (caseMannagerEntity.PaymentMethod == PaymentMethod.Direct_Deposit) ? 2 : (caseMannagerEntity.PaymentMethod == PaymentMethod.Zelle) ? 3 :  0,
                PaymentMethodList = _combosHelper.GetComboPaymentMethod(),
                TCMCertifications = caseMannagerEntity.TCMCertifications
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
                //Referred = await _context.Referreds.FirstOrDefaultAsync(r => r.Id == model.IdReferred),
                LegalGuardian = await _context.LegalGuardians.FirstOrDefaultAsync(lg => lg.Id == model.IdLegalGuardian),
                EmergencyContact = await _context.EmergencyContacts.FirstOrDefaultAsync(ec => ec.Id == model.IdEmergencyContact),
                RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(model.IdRelationshipEC),
                RelationShipOfLegalGuardian = RelationshipUtils.GetRelationshipByIndex(model.IdRelationship),
                IndividualTherapyFacilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.Id == model.IdFacilitatorIT),
                Service = ServiceUtils.GetServiceByIndex(model.IdService),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                IdFacilitatorPSR = model.IdFacilitatorPSR,
                IdFacilitatorGroup = model.IdFacilitatorGroup,
                OtherLanguage_Read = model.OtherLanguage_Read,
                OtherLanguage_Speak = model.OtherLanguage_Speak,
                OtherLanguage_Understand = model.OtherLanguage_Understand,
                MedicareId = model.MedicareId,
                DateOfClose = model.DateOfClose,
                OnlyTCM = model.OnlyTCM,
                Annotations = model.Annotations,
                DocumentsAssistant = await _context.DocumentsAssistant.FirstOrDefaultAsync(d => d.Id == model.IdDocumentsAssistant)
            };
        }

        public async Task<ClientViewModel> ToClientViewModel(ClientEntity clientEntity, string userId)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.Id == userId);
            TCMClientEntity tcmclient = _context.TCMClient.FirstOrDefault(n => n.Client.Id == clientEntity.Id);
            
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
                //IdReferred = (clientEntity.Client_Referred != null) ? clientEntity.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Id : 0,
                //Referreds = _combosHelper.GetComboReferredsByClinic(userId),                
                IdEmergencyContact = (clientEntity.EmergencyContact != null) ? clientEntity.EmergencyContact.Id : 0,
                EmergencyContacts = _combosHelper.GetComboEmergencyContactsByClinic(userId),
                IdDoctor = (clientEntity.Doctor != null) ? clientEntity.Doctor.Id : 0,
                Doctors = _combosHelper.GetComboDoctorsByClinic(userId),
                IdPsychiatrist = (clientEntity.Psychiatrist != null) ? clientEntity.Psychiatrist.Id : 0,
                Psychiatrists = _combosHelper.GetComboPsychiatristsByClinic(userId),
                IdLegalGuardian = (clientEntity.LegalGuardian != null) ? clientEntity.LegalGuardian.Id : 0,
                LegalsGuardians = _combosHelper.GetComboLegalGuardiansByClinic(userId),
                DiagnosticTemp = _context.DiagnosticsTemp.Where(n => n.UserName == user_logged.UserName && n.IdClient == clientEntity.Id),
                ReferredTemp = _context.ReferredsTemp.Where(n => n.CreatedBy == user_logged.UserName && n.IdClient == clientEntity.Id),
                DocumentTemp = _context.DocumentsTemp.Where(n => n.UserName == user_logged.UserName && n.IdClient == clientEntity.Id),
                IdService = Convert.ToInt32(clientEntity.Service), 
                Services = _combosHelper.GetComboServices(),
                IdFacilitatorIT = (clientEntity.IndividualTherapyFacilitator != null) ? clientEntity.IndividualTherapyFacilitator.Id : 0,
                ITFacilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, true),
                IdFacilitatorPSR = clientEntity.IdFacilitatorPSR,
                IdFacilitatorGroup = clientEntity.IdFacilitatorGroup,
                OtherLanguage_Read = clientEntity.OtherLanguage_Read,
                OtherLanguage_Speak = clientEntity.OtherLanguage_Speak,
                OtherLanguage_Understand = clientEntity.OtherLanguage_Understand,
                MedicareId = clientEntity.MedicareId,
                DateOfClose = clientEntity.DateOfClose,
                Documents = clientEntity.Documents,
                OnlyTCM = clientEntity.OnlyTCM,
                HealthInsuranceTemp = _context.HealthInsuranceTemp.Where(n => n.UserName == user_logged.UserName && n.IdClient == clientEntity.Id),
                Clients_HealthInsurances = clientEntity.Clients_HealthInsurances,
                Annotations = clientEntity.Annotations,
                Doctor = clientEntity.Doctor,
                Psychiatrist = clientEntity.Psychiatrist,
                LegalGuardian = clientEntity.LegalGuardian,
                EmergencyContact = clientEntity.EmergencyContact,
                IdDocumentsAssistant = (clientEntity.DocumentsAssistant != null) ? clientEntity.DocumentsAssistant.Id : 0,
                DocumentsAssistant = clientEntity.DocumentsAssistant,
                DocumentsAssistants = _combosHelper.GetComboDocumentsAssistantByClinic(user_logged.Clinic.Id,false,false),
                IdTCMClient = (tcmclient != null)? tcmclient.Id : 0 

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
                SignaturePath = signaturePath,
                RaterEducation = model.RaterEducation,
                RaterFMHCertification = model.RaterFMHCertification
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
                RaterEducation = supervisorEntity.RaterEducation,
                RaterFMHCertification = supervisorEntity.RaterFMHCertification
            };
        }

        public async Task<TCMSupervisorEntity> ToTCMsupervisorEntity(TCMSupervisorViewModel model, string signaturePath, bool isNew, string userId)
        {
            return new TCMSupervisorEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Code = model.Code,
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                Name = isNew ? model.FirstName + " " + model.MiddleName + " " + model.LastName : model.Name,
                SignaturePath = signaturePath,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                RaterEducation = model.RaterEducation,
                RaterFMHCertification = model.RaterFMHCertification,
                Email = model.Email,
                Phone = model.Phone,
                Money = model.Money,
                Address = model.Address,
                City = model.City,
                DateOfBirth = model.DateOfBirth,
                FirstName = model.FirstName,
                PaymentMethod = PaymentMethodUtils.GetPaymentMethodByIndex(model.IdPaymentMethod),
                Gender = GenderUtils.GetGenderByIndex(model.IdGender),
                LastName = model.LastName,
                MiddleName = model.MiddleName,
                PH = model.PH,
                State = model.State,
                ZipCode = model.ZipCode,
                AccountNumber = model.AccountNumber,
                AccountType = AccountTypeUtils.GetAccountTypeByIndex(model.IdAccountType),
                CAQH = model.CAQH,
                CompanyEIN = model.CompanyEIN,
                CompanyName = model.CompanyName,
                CredentialExpirationDate = model.CredentialExpirationDate,
                DEALicense = model.DEALicense,
                FinancialInstitutionsName = model.FinancialInstitutionsName,
                HiringDate = model.HiringDate,
                MedicaidProviderID = model.MedicaidProviderID,
                MedicareProviderID = model.MedicareProviderID,
                NPI = model.NPI,
                Routing = model.Routing,
                SSN = model.SSN, 
                Credentials = model.Credentials
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
                StatusList = _combosHelper.GetComboClientStatus(),
                RaterEducation = TCMSupervisorEntity.RaterEducation,
                RaterFMHCertification = TCMSupervisorEntity.RaterFMHCertification,
                Email = TCMSupervisorEntity.Email,
                Phone = TCMSupervisorEntity.Phone,
                Money = TCMSupervisorEntity.Money,
               
                Address = TCMSupervisorEntity.Address,
                City = TCMSupervisorEntity.City,
                DateOfBirth = TCMSupervisorEntity.DateOfBirth,
                FirstName = TCMSupervisorEntity.FirstName,
                PaymentMethod = TCMSupervisorEntity.PaymentMethod,
                Gender = TCMSupervisorEntity.Gender,
                LastName = TCMSupervisorEntity.LastName,
                MiddleName = TCMSupervisorEntity.MiddleName,
                PH = TCMSupervisorEntity.PH,
                State = TCMSupervisorEntity.State,
                ZipCode = TCMSupervisorEntity.ZipCode,
                AccountNumber = TCMSupervisorEntity.AccountNumber,
                AccountType = TCMSupervisorEntity.AccountType,
                CAQH = TCMSupervisorEntity.CAQH,
                CompanyEIN = TCMSupervisorEntity.CompanyEIN,
                CompanyName = TCMSupervisorEntity.CompanyName,
                CreatedBy = TCMSupervisorEntity.CreatedBy,
                CreatedOn = TCMSupervisorEntity.CreatedOn,
                CredentialExpirationDate = TCMSupervisorEntity.CredentialExpirationDate,
                DEALicense = TCMSupervisorEntity.DEALicense,
                FinancialInstitutionsName = TCMSupervisorEntity.FinancialInstitutionsName,
                HiringDate = TCMSupervisorEntity.HiringDate,
                LastModifiedBy = TCMSupervisorEntity.LastModifiedBy,
                LastModifiedOn = TCMSupervisorEntity.LastModifiedOn,
                MedicaidProviderID = TCMSupervisorEntity.MedicaidProviderID,
                MedicareProviderID = TCMSupervisorEntity.MedicareProviderID,
                NPI = TCMSupervisorEntity.NPI,
                Routing = TCMSupervisorEntity.Routing,
                SSN = TCMSupervisorEntity.SSN,
                IdGender = (TCMSupervisorEntity.Gender == GenderType.Female) ? 1 : 2,
                GenderList = _combosHelper.GetComboGender(),
                IdAccountType = (TCMSupervisorEntity.AccountType == AccountType.Personal_Checking) ? 1 : (TCMSupervisorEntity.AccountType == AccountType.Personal_Saving) ? 2 : (TCMSupervisorEntity.AccountType == AccountType.Company_Checking) ? 3 : (TCMSupervisorEntity.AccountType == AccountType.Company_Saving) ? 4 : 0,
                AccountTypeList = _combosHelper.GetComboAccountType(),
                IdPaymentMethod = (TCMSupervisorEntity.PaymentMethod == PaymentMethod.Check) ? 1 : (TCMSupervisorEntity.PaymentMethod == PaymentMethod.Direct_Deposit) ? 2 : (TCMSupervisorEntity.PaymentMethod == PaymentMethod.Zelle) ? 3 : 0,
                PaymentMethodList = _combosHelper.GetComboPaymentMethod(),
                TCMCertifications = TCMSupervisorEntity.TCMCertifications,
                Credentials = TCMSupervisorEntity.Credentials
            };
        }

        public async Task<MTPEntity> ToMTPEntity(MTPViewModel model, bool isNew, string userId)
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
                Modality = "PSR",
                Frecuency = model.PsychosocialFrecuency,
                NumberOfMonths = model.PsychosocialDuration,
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
                SubstanceWhere = model.SubstanceWhere,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Messages = _context.Messages
                                      .Where(n => n.Mtp.Id == model.Id)
                                      .ToList(),
                Status = model.Status,
                SupervisorDate = model.SupervisorDate,
                DocumentAssistant = await _context.DocumentsAssistant.FindAsync(model.IdDocumentAssistant),
                CodeBill = model.CodeBill,
                Units = model.Units

            };
        }

        public MTPViewModel ToMTPViewModel(MTPEntity mtpEntity)
        {
            MTPViewModel model;
            model = new MTPViewModel
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
                Client = mtpEntity.Client,
                Goals = mtpEntity.Goals,
                CreatedBy = mtpEntity.CreatedBy,
                CreatedOn = mtpEntity.CreatedOn,
                LastModifiedBy = mtpEntity.LastModifiedBy,
                LastModifiedOn = mtpEntity.LastModifiedOn,
                Status = mtpEntity.Status,
                AdmissionedFor = mtpEntity.AdmissionedFor,
                CodeBill = mtpEntity.CodeBill,
                Units = mtpEntity.Units
                
            };

            if (mtpEntity.DocumentAssistant == null)
            {
                model.IdDocumentAssistant = 0;
            }
            else
            {
                model.IdDocumentAssistant = mtpEntity.DocumentAssistant.Id;
            }

            return model;
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
                Compliment_Date = goalEntity.Compliment_Date.Day > 1 ? goalEntity.Compliment_Date : DateTime.Now,
                Compliment_Explain = goalEntity.Compliment_Explain,
                Compliment_IdMTPReview = goalEntity.Compliment_IdMTPReview,
                IdMTPReview = goalEntity.IdMTPReview                
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
                Service = model.Service,
                SharedSession = model.SharedSession,
                Schedule = await _context.Schedule.FindAsync(model.IdSchedule)
            };
        }

        public GroupViewModel ToGroupViewModel(GroupEntity groupEntity)
        {
            FacilitatorEntity facilitator = _context.Facilitators
                                                    .Include(c => c.Clinic)
                                                    .FirstOrDefault(n => n.Id == groupEntity.Facilitator.Id);

            GroupViewModel model = new GroupViewModel();
            model = new GroupViewModel
            {
                Id = groupEntity.Id,
                Facilitator = groupEntity.Facilitator,
                IdFacilitator = groupEntity.Facilitator.Id,
                Facilitators = _combosHelper.GetComboFacilitators(),
                Am = groupEntity.Am,
                Pm = groupEntity.Pm,
                Clients = groupEntity.Clients,
                SharedSession = groupEntity.SharedSession,
                Schedules = _combosHelper.GetComboSchedulesByClinic(facilitator.Clinic.Id, groupEntity.Service)
            };
            if (groupEntity.Schedule != null)
            {
                model.Schedule = groupEntity.Schedule;
                model.IdSchedule = groupEntity.Schedule.Id;
                
            }
            return model;
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

                Schema = model.Schema,
                Setting = model.Setting
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
                Other_Intervention = model.Other_Intervention,
                SubSchedule = await _context.SubSchedule.FindAsync(model.IdSubSchedule),
                Setting = model.Setting
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
                Other_Intervention = model.Other_Intervention,
                Setting = model.Setting
            };
        }        

        public Workday_ClientViewModel ToWorkdayClientViewModel(Workday_Client model, bool indTherapy = false)
        {
            Workday_ClientViewModel salida = new Workday_ClientViewModel();
            
            salida = new Workday_ClientViewModel
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

            if (model.Schedule == null)
            {
                if (indTherapy == false)
                {
                    salida.IdSchedule = 0;
                    salida.Schedules = _combosHelper.GetComboSchedulesForFacilitatorForDay(model.Facilitator.Id, model.Workday.Id, model.Client.Id, model.Id);
                }
               
            }
            else
            {
                if (indTherapy == false)
                {
                    salida.IdSchedule = model.Schedule.Id;
                    salida.Schedules = _combosHelper.GetComboSchedulesForFacilitatorForDay(model.Facilitator.Id, model.Workday.Id, model.Client.Id,model.Id);
                }
                else
                {
                    salida.IdSchedule = model.Schedule.Id;
                    salida.Schedules = _combosHelper.GetComboSubSchedulesForFacilitatorForDay(model.Facilitator.Id, model.Workday.Id, model.Schedule.Id, model.Client.Id,model.Id);
                }
                
            }
            return salida;
        }

        public async Task<MessageEntity> ToMessageEntity(MessageViewModel model, bool isNew)
        {

            return new MessageEntity
            {
                Id = isNew ? 0 : model.Id,
                Workday_Client = (model.IdWorkdayClient != 0) ? await _context.Workdays_Clients
                                                                              .Include(wc => wc.Facilitator)
                                                                              .FirstOrDefaultAsync(wc => wc.Id == model.IdWorkdayClient) : null,
                FarsForm = (model.IdFarsForm != 0) ? await _context.FarsForm
                                                                   .FirstOrDefaultAsync(f => f.Id == model.IdFarsForm) : null,
                MTPReview = (model.IdMTPReview != 0) ? await _context.MTPReviews
                                                                     .FirstOrDefaultAsync(m => m.Id == model.IdMTPReview) : null,
                Addendum = (model.IdAddendum != 0) ? await _context.Adendums
                                                                   .FirstOrDefaultAsync(a => a.Id == model.IdAddendum) : null,
                Discharge = (model.IdDischarge != 0) ? await _context.Discharge
                                                                     .FirstOrDefaultAsync(d => d.Id == model.IdDischarge) : null,
                Mtp = (model.IdMtp != 0) ? await _context.MTPs
                                                         .FirstOrDefaultAsync(d => d.Id == model.IdMtp) : null,
                Brief = (model.IdBrief != 0) ? await _context.Brief
                                                         .FirstOrDefaultAsync(d => d.Id == model.IdBrief) : null,
                Bio = (model.IdBio != 0) ? await _context.Bio
                                                         .FirstOrDefaultAsync(d => d.Id == model.IdBio) : null,
                Title = model.Title,
                Text = model.Text,
                DateCreated = DateTime.Now,
                Status = MessageStatus.NotRead,
                Notification = model.Notification
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
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                FaxNumber = model.FaxNumber,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode
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
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                FaxNumber = model.FaxNumber,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode
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
                AdressLine2 = model.AdressLine2,
                SignPath = model.SignPath
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
               // ReferredNote = model.ReferredNote,
                Name = model.Name,
                Address = model.Address,
                Telephone = model.Telephone,
                Email = model.Email,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Agency = model.Agency,
                Title = model.Title,
                City = model.City,
                State = model.State,
                ZidCode = model.ZidCode
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
                CreatedOn = model.CreatedOn,
                FaxNumber = model.FaxNumber,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode
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
                FaxNumber = model.FaxNumber,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
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
                //ReferredNote = model.ReferredNote,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                Agency = model.Agency,
                Title = model.Title,
                City = model.City,
                State = model.State,
                ZidCode = model.ZidCode
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
                AdressLine2 = model.AdressLine2,
                SignPath = model.SignPath
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
                SolvedDate = !isNew && (StatusUtils.GetIncidentStatusByIndex(model.IdStatus) == IncidentsStatus.Solved) ? DateTime.Now : model.SolvedDate,
                client = await _context.Clients.FindAsync(model.IdClient),
                UserAsigned = await _context.Users.FindAsync(model.IdUserAssigned)
            };
        }

        public IncidentViewModel ToIncidentViewModel(IncidentEntity model)
        {
            string idUserAssigned = "";
            if (model.UserAsigned != null)
            {
                idUserAssigned = model.UserAsigned.Id;
            }

            int idClient = 0;
            if (model.client != null)
            {
                idClient = model.client.Id;
            }

            return new IncidentViewModel
            {
                Id = model.Id,
                Description = model.Description,
                CreatedDate = model.CreatedDate,
                IdStatus = (model.Status == IncidentsStatus.Pending) ? 0 : (model.Status == IncidentsStatus.Solved) ? 1 : (model.Status == IncidentsStatus.NotValid) ? 2 : 0,
                StatusList = _combosHelper.GetComboIncidentsStatus(),
                IdUserCreatedBy = model.UserCreatedBy.Id,
                UserCreatedBy = model.UserCreatedBy,
                IdClient = idClient,
                IdUserAssigned = idUserAssigned,
                Clients = _combosHelper.GetComboClientsAdmissionByClinic(model.UserCreatedBy.Clinic.Id),
                Users = _combosHelper.GetComboUserNamesByClinic(model.UserCreatedBy.Clinic.Id)

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
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                NeedAuthorization = model.NeedAuthorization
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
                LastModifiedOn = model.LastModifiedOn,
                NeedAuthorization = model.NeedAuthorization
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
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                MemberId = model.MemberId,
                AuthorizationNumber = model.AuthorizationNumber
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
                Client = model.Client,
                IdHealthInsurance = model.HealthInsurance.Id,
                HealthInsurances = _combosHelper.GetComboActiveInsurancesByClinic(idClinic),
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                MemberId = model.MemberId,
                AuthorizationNumber = model.AuthorizationNumber,
                IdAgencyService = (model.Agency == ServiceAgency.CMH) ? 0 : 1,
                AgencyServices = _combosHelper.GetComboServiceAgency(),
                Agency = model.Agency
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
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                MHProblems = model.MHProblems,
                TCMSupervisorEdit = model.TCMSupervisorEdit,
                BillSemanalMH = model.BillSemanalMH,
                IndNoteForAppointment = model.IndNoteForAppointment,
                TCMInitialTime = model.TCMInitialTime,
                TCMEndTime = model.TCMEndTime,
                LockTCMNoteForUnits = model.LockTCMNoteForUnits,
                UnitsForDayForClient = model.UnitsForDayForClient,
                DischargeJoinCommission = model.DischargeJoinCommission,
                CreateNotesTCMWithServiceplanInEdition = model.CreateNotesTCMWithServiceplanInEdition,
                SupervisorEdit = model.SupervisorEdit,
                TCMSupervisionTimeWithCaseManager = model.TCMSupervisionTimeWithCaseManager,
                DocumentAssisstant_Intake = model.DocumentAssisstant_Intake,
                CreateTCMNotesWithoutDomain = model.CreateTCMNotesWithoutDomain,
                TCMPayStub_Filtro = StatusUtils.GetFiltroTCMPayStubByIndex(model.IdFiltroPayStub),
                MTPmultipleSignatures = model.MTPmultipleSignatures,
                TCMLockCreateNote = model.TCMLockCreateNote,
                DashBoardPrincipal = DashboardUtils.GetDashboardTypeByIndex(model.IdDashboard)
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
                LastModifiedOn = model.LastModifiedOn,
                MHProblems = model.MHProblems,
                TCMSupervisorEdit = model.TCMSupervisorEdit,
                BillSemanalMH = model.BillSemanalMH,
                IndNoteForAppointment = model.IndNoteForAppointment,
                TCMInitialTime = model.TCMInitialTime,
                TCMEndTime = model.TCMEndTime,
                LockTCMNoteForUnits = model.LockTCMNoteForUnits,
                UnitsForDayForClient = model.UnitsForDayForClient,
                DischargeJoinCommission = model.DischargeJoinCommission,
                CreateNotesTCMWithServiceplanInEdition = model.CreateNotesTCMWithServiceplanInEdition,
                SupervisorEdit = model.SupervisorEdit,
                TCMSupervisionTimeWithCaseManager = model.TCMSupervisionTimeWithCaseManager,
                DocumentAssisstant_Intake = model.DocumentAssisstant_Intake,
                CreateTCMNotesWithoutDomain = model.CreateTCMNotesWithoutDomain,
                IdFiltroPayStub = (model.TCMPayStub_Filtro == TCMPayStubFiltro.Created) ? 0 : (model.TCMPayStub_Filtro == TCMPayStubFiltro.Approved) ? 1 : (model.TCMPayStub_Filtro == TCMPayStubFiltro.Billed) ? 2 : 3,
                FiltroPayStubs = _combosHelper.GetComboFiltroTCMPayStubByClinic(),
                MTPmultipleSignatures = model.MTPmultipleSignatures,
                TCMLockCreateNote = model.TCMLockCreateNote,
                IdDashboard = (model.DashBoardPrincipal == DashboardType.MH) ? 0 : 1,
                Dashboards = _combosHelper.GetComboDashboardType()
            };
        }

        public async Task<TCMServiceEntity> ToTCMServiceEntity(TCMServiceViewModel model, bool isNew, string userId)
        {
            return new TCMServiceEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
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

        public async Task<TCMStageEntity> ToTCMStageEntity(TCMStageViewModel model, bool isNew, string userId)
        {
            return new TCMStageEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
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
                tCMservice = TcmStageEntity.tCMservice,
                CreatedBy = TcmStageEntity.CreatedBy,
                CreatedOn = TcmStageEntity.CreatedOn
            };
        }

        public TCMClientEntity ToTCMClientEntity(TCMClientViewModel model, bool isNew, string userId)
        {
            return new TCMClientEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Casemanager = model.Casemanager,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                CaseNumber = model.CaseNumber,
                DataOpen = model.DataOpen,
                DataClose = model.DataClose,
                Period = model.Period,
                Client = model.Client,
                Younger = model.Younger
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
                Period = tcmClientEntity.Period,
                CreatedOn = tcmClientEntity.CreatedOn,
                CreatedBy = tcmClientEntity.CreatedBy,
                Younger = tcmClientEntity.Younger
            };
        }

        public TCMServicePlanViewModel ToTCMServicePlanViewModel(TCMServicePlanEntity TcmServicePlanEntity)
        {
            return new TCMServicePlanViewModel
            {
                Id = TcmServicePlanEntity.Id,
                ID_TcmClient = TcmServicePlanEntity.TcmClient.Id,
                DateIntake = TcmServicePlanEntity.DateIntake,
                DateServicePlan = TcmServicePlanEntity.DateServicePlan,
                DateAssessment = TcmServicePlanEntity.DateAssessment,
                DateCertification = TcmServicePlanEntity.DateCertification,
                DischargerCriteria = TcmServicePlanEntity.DischargerCriteria,
                Strengths = TcmServicePlanEntity.Strengths,
                Weakness = TcmServicePlanEntity.Weakness,
                CaseNumber = TcmServicePlanEntity.TcmClient.CaseNumber,
                ID_Status = (TcmServicePlanEntity.Status == StatusType.Open) ? 1 : 2 ,
                DateSupervisorSignature = TcmServicePlanEntity.DateSupervisorSignature,
                DateTcmSignature = TcmServicePlanEntity.DateTcmSignature
            };
        }

        public async Task<TCMServicePlanEntity> ToTCMServicePlanEntity(TCMServicePlanViewModel model, bool isNew, string userId)
        {
            return new TCMServicePlanEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DateIntake = model.DateIntake,
                DateServicePlan = model.DateServicePlan,
                DateAssessment = model.DateAssessment,
                DateCertification = model.DateCertification,
                TcmClient = await _context.TCMClient
                                          .Include(n => n.Client)
                                          .FirstOrDefaultAsync(n => n.Id == model.ID_TcmClient),
                DischargerCriteria = model.DischargerCriteria,
                Weakness = model.Weakness,
                Strengths = model.Strengths,
                Status = StatusUtils.GetStatusByIndex(model.ID_Status),
                Approved = model.Approved,
                TCMMessages = _context.TCMMessages
                                      .Where(n => n.TCMServicePlan.Id == model.Id)
                                      .ToList(),
                DateSupervisorSignature = model.DateSupervisorSignature,
                DateTcmSignature = model.DateTcmSignature
                
            };
        }

        public TCMDomainViewModel ToTCMDomainViewModel(TCMDomainEntity TcmDomainEntity)
        {
            return new TCMDomainViewModel
            {
                Id = TcmDomainEntity.Id,
                DateIdentified = TcmDomainEntity.DateIdentified,
                Needs_Identified = TcmDomainEntity.NeedsIdentified,
                Long_Term = TcmDomainEntity.LongTerm,
                Id_ServicePlan = TcmDomainEntity.TcmServicePlan.Id,
                Code = TcmDomainEntity.Code,
                Name = TcmDomainEntity.Name,
                TcmServicePlan = TcmDomainEntity.TcmServicePlan,
                Services = _combosHelper.GetComboServicesNotUsed(TcmDomainEntity.TcmServicePlan.Id),
                Origin = TcmDomainEntity.Origin,
                CreatedBy = TcmDomainEntity.CreatedBy,
                CreatedOn = TcmDomainEntity.CreatedOn,
                LastModifiedBy = TcmDomainEntity.LastModifiedBy,
                LastModifiedOn = TcmDomainEntity.LastModifiedOn,
                Id_SubService = TcmDomainEntity.IdSubService,
                NameSubService = TcmDomainEntity.NameSubService,
                DateAccomplished = (TcmDomainEntity.DateAccomplished.Year != 1) ? TcmDomainEntity.DateAccomplished : DateTime.Now.Date
            };
        }

        public TCMDomainEntity ToTCMDomainEntity(TCMDomainViewModel model, bool isNew, string origin = "Service Plan Review", string userId = "")
        {
           
            return new TCMDomainEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DateIdentified = model.DateIdentified,
                NeedsIdentified = model.Needs_Identified,
                LongTerm = model.Long_Term,
                TCMObjetive = model.TCMObjetive,
                Code = model.Code,
                Name = model.Name,
                TcmServicePlan = model.TcmServicePlan,
                Origin = origin,
                NameSubService =(model.Id_SubService == 0)? string.Empty: _context.TCMSubServices.FirstOrDefault(n => n.Id == model.Id_SubService).Name,
                IdSubService = model.Id_SubService,
                Status = model.Status,
                DateAccomplished = model.DateAccomplished
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
                DateSignatureEmployee = model.DateSignatureEmployee
                
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
                DateSignatureEmployee = model.DateSignatureEmployee

            };
           
        }

        public IntakeConsentForTreatmentEntity ToIntakeConsentForTreatmentEntity(IntakeConsentForTreatmentViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor
                

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
                AdmissionedFor = model.AdmissionedFor

            };
            
        }

        public IntakeConsentForReleaseEntity ToIntakeConsentForReleaseEntity(IntakeConsentForReleaseViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor

            };

        }

        public IntakeConsumerRightsEntity ToIntakeConsumerRightsEntity(IntakeConsumerRightsViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor

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
                AdmissionedFor = model.AdmissionedFor

            };

        }

        public IntakeAcknowledgementHippaEntity ToIntakeAcknoewledgementHippaEntity(IntakeAcknoewledgementHippaViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor

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
                AdmissionedFor = model.AdmissionedFor
               
            };

        }
        
        public IntakeAccessToServicesEntity ToIntakeAccessToServicesEntity(IntakeAccessToServicesViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor
                
            };

        }

        public IntakeOrientationChecklistEntity ToIntakeOrientationChecklistEntity(IntakeOrientationCheckListViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor

            };
        }

        public async Task<TCMObjetiveEntity> ToTCMObjetiveEntity(TCMObjetiveViewModel model, bool isNew, int origin, string userId)
        {
            string valor = "Service Plan";
            if (origin == 1)
                valor = "Addendum";
            if (origin == 2)
                valor = "Service Plan Review";

            return new TCMObjetiveEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmDomain = await _context.TCMDomains.FindAsync(model.Id_Domain),
                IdObjetive = model.IdObjetive,
                //Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Name = model.name,
                Task = model.task,
                //Long_Term = model.long_Term,
                StartDate = model.StartDate,
                
                TargetDate = model.TargetDate,
                EndDate = model.EndDate,
                Finish = model.Finish,                
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                Responsible = model.Responsible,
                Origin = valor
            };
        }

        public TCMObjetiveViewModel ToTCMObjetiveViewModel(TCMObjetiveEntity TcmObjetiveEntity)
        {
            TCMObjetiveViewModel salida;

            salida = new TCMObjetiveViewModel
            {
                Id = TcmObjetiveEntity.Id,
                Id_Domain = TcmObjetiveEntity.TcmDomain.Id,
                IdObjetive= TcmObjetiveEntity.IdObjetive,
                //IdClinic = TcmObjetiveEntity.Clinic.Id,
                //Clinics = _combosHelper.GetComboClinics(),
                name = TcmObjetiveEntity.Name,
                task = TcmObjetiveEntity.Task,
                Status = TcmObjetiveEntity.Status,
                StartDate = TcmObjetiveEntity.StartDate,
                TargetDate = TcmObjetiveEntity.TargetDate,
                EndDate = TcmObjetiveEntity.EndDate,
                Finish = TcmObjetiveEntity.Finish,
                TcmDomain = TcmObjetiveEntity.TcmDomain,
                Stages = _combosHelper.GetComboStagesNotUsed(TcmObjetiveEntity.TcmDomain),
                Responsible = TcmObjetiveEntity.Responsible,
                IdStatus = (TcmObjetiveEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus(),
                IdServicePlanReview = TcmObjetiveEntity.TcmDomain.TcmServicePlan.TCMServicePlanReview.Id,
                Idd = TcmObjetiveEntity.Id,
                CreatedBy = TcmObjetiveEntity.CreatedBy,
                CreatedOn = TcmObjetiveEntity.CreatedOn,
                LastModifiedBy = TcmObjetiveEntity.LastModifiedBy,
                LastModifiedOn = TcmObjetiveEntity.LastModifiedOn,
                Name = TcmObjetiveEntity.Name,
                Task = TcmObjetiveEntity.Task                
            };

            if (TcmObjetiveEntity.Origin == "Service Plan")
                salida.Origi = 0;
            if (TcmObjetiveEntity.Origin == "Addendum")
                salida.Origi = 1;
            if (TcmObjetiveEntity.Origin == "Service Plan Review")
                salida.Origi = 2;

            return salida;
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
                AdmissionedFor = model.AdmissionedFor

            };

        }

        public IntakeTransportationEntity ToIntakeTransportationEntity(IntakeTransportationViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor

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
                AdmissionedFor = model.AdmissionedFor

            };

        }

        public IntakeConsentPhotographEntity ToIntakeConsentPhotographEntity(IntakeConsentPhotographViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor
            };
        }

        public async Task<TCMAdendumEntity> ToTCMAdendumEntity(TCMAdendumViewModel model, bool isNew, string userId)
        {

            return new TCMAdendumEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DateAdendum = model.Date_Identified,
                TcmServicePlan = await _context.TCMServicePlans
                                               .Include(n => n.TcmClient)
                                               .ThenInclude(n => n.Client)
                                               .FirstOrDefaultAsync(n => n.Id == model.ID_TcmServicePlan),
                TcmDomain = model.TcmDomain,
                LongTerm = model.Long_term,
                NeedsIdentified = model.Needs_Identified,
                TCMMessages = _context.TCMMessages
                                      .Where(n => n.TCMAddendum.Id == model.Id)
                                      .ToList(),
                Approved = model.Approved,
                DateTCMSign = model.DateTCMSign,
                DateTCMSupervisorSign = model.DateTCMSupervisorSign

            };
        }

        public TCMAdendumViewModel ToTCMAdendumViewModel(TCMAdendumEntity TcmAdendumEntity)
        {
            return new TCMAdendumViewModel
            {
                Id = TcmAdendumEntity.Id,
                TcmServicePlan = TcmAdendumEntity.TcmServicePlan,
                ListTcmServicePlan = _combosHelper.GetComboServicesPlan(TcmAdendumEntity.TcmServicePlan.TcmClient.Casemanager.Clinic.Id, TcmAdendumEntity.TcmServicePlan.TcmClient.Casemanager.Id, TcmAdendumEntity.TcmServicePlan.TcmClient.CaseNumber),
                TcmDominio = _combosHelper.GetComboTCMServices(),
                TcmDomain = TcmAdendumEntity.TcmDomain,
                DateAdendum = TcmAdendumEntity.DateAdendum,
                LongTerm = TcmAdendumEntity.LongTerm,
                NeedsIdentified = TcmAdendumEntity.NeedsIdentified,
                CreatedOn = TcmAdendumEntity.CreatedOn,
                CreatedBy = TcmAdendumEntity.CreatedBy,
                DateTCMSign = TcmAdendumEntity.DateTCMSign,
                DateTCMSupervisorSign = TcmAdendumEntity.DateTCMSupervisorSign
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
                Approved = TcmServicePlanReviewEntity.Approved,
                CreatedBy = TcmServicePlanReviewEntity.CreatedBy,
                CreatedOn = TcmServicePlanReviewEntity.CreatedOn,
                ClientContinue = TcmServicePlanReviewEntity.ClientContinue,
                ClientHasBeen1 = TcmServicePlanReviewEntity.ClientHasBeen1,
                ClientHasBeen2 = TcmServicePlanReviewEntity.ClientHasBeen2,
                ClientNoLonger1 = TcmServicePlanReviewEntity.ClientNoLonger1,
                ClientNoLonger2 = TcmServicePlanReviewEntity.ClientNoLonger2,
                ClientWillContinue = TcmServicePlanReviewEntity.ClientWillContinue,
                ClientWillHave = TcmServicePlanReviewEntity.ClientWillHave,
                DateTCMCaseManagerSignature = (TcmServicePlanReviewEntity.DateTCMCaseManagerSignature.Year != 1) ? TcmServicePlanReviewEntity.DateTCMCaseManagerSignature 
                                                                                                                 : DateTime.Today.Date,
                DateTCMSupervisorSignature = (TcmServicePlanReviewEntity.DateTCMSupervisorSignature.Year != 1) ? TcmServicePlanReviewEntity.DateTCMSupervisorSignature
                                                                                                                 : DateTime.Today.Date,
                HasBeenExplained = TcmServicePlanReviewEntity.HasBeenExplained,
                TheExpertedReviewDate = TcmServicePlanReviewEntity.TheExpertedReviewDate
            };
        }

        public async Task<TCMServicePlanReviewEntity> ToTCMServicePlanReviewEntity(TCMServicePlanReviewViewModel model, bool isNew, string userId)
        {
            TCMServicePlanEntity tcmServicePlan = await _context.TCMServicePlans
                                                                .Include(n => n.TcmClient)
                                                                .ThenInclude(n => n.Client)
                                                                .FirstOrDefaultAsync(n => n.Id == model.IdServicePlan);
            return new TCMServicePlanReviewEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DateOpending = tcmServicePlan.DateIntake,
                DateServicePlanReview = model.DateServicePlanReview,
                Recomendation = model.Recomendation,
                SummaryProgress = model.SummaryProgress,
                TcmServicePlan = tcmServicePlan,
                TCMServicePlanRevDomain = model.TCMServicePlanRevDomain,
                TCMMessages = _context.TCMMessages
                                      .Where(n => n.TCMServicePlan.TCMServicePlanReview.Id == model.Id)
                                      .ToList(),
                Approved = model.Approved,
                ClientContinue = model.ClientContinue,
                ClientHasBeen1 = model.ClientHasBeen1,
                ClientHasBeen2 = model.ClientHasBeen2,
                ClientNoLonger1 = model.ClientNoLonger1,
                ClientNoLonger2 = model.ClientNoLonger2,
                ClientWillContinue = model.ClientWillContinue,
                ClientWillHave = model.ClientWillHave,
                DateTCMCaseManagerSignature = model.DateTCMCaseManagerSignature,
                DateTCMSupervisorSignature = model.DateTCMSupervisorSignature,
                HasBeenExplained = model.HasBeenExplained,
                TheExpertedReviewDate = model.TheExpertedReviewDate

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
                status = _combosHelper.GetComboClientStatus()
              
            };
        }

        public TCMServicePlanReviewDomainEntity ToTCMServicePlanReviewDomainEntity(TCMServicePlanReviewDomainViewModel model, bool isNew, string userId)
        {
            return new TCMServicePlanReviewDomainEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmDomain = model.TcmDomain,
                ChangesUpdate = model.ChangesUpdate
               
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
                StaffingDate = TcmDischargeEntity.StaffingDate,
                StaffSignatureDate = TcmDischargeEntity.StaffSignatureDate,
                SupervisorSignatureDate = TcmDischargeEntity.SupervisorSignatureDate,
                TcmDischargeFollowUp = TcmDischargeEntity.TcmDischargeFollowUp,
                TcmDischargeServiceStatus = TcmDischargeEntity.TcmDischargeServiceStatus,
                TcmServices = TcmDischargeEntity.TcmServicePlan.TCMService,
                Approved = TcmDischargeEntity.Approved,
                CreatedBy = TcmDischargeEntity.CreatedBy,
                CreatedOn = TcmDischargeEntity.CreatedOn,
                TCMSupervisor = TcmDischargeEntity.TCMSupervisor

            };
        }

        public async Task<TCMDischargeEntity> ToTCMDischargeEntity(TCMDischargeViewModel model, bool isNew, string userId)
        {
            return new TCMDischargeEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
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
                TcmServicePlan = await _context.TCMServicePlans
                                               .Include(n => n.TcmClient)
                                               .ThenInclude(n => n.Client)
                                               .FirstOrDefaultAsync(m => m.Id == model.IdServicePlan),
                Approved = model.Approved,
                TCMMessages = _context.TCMMessages
                                      .Where(n => n.TCMDischarge.Id == model.Id)
                                      .ToList()

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
                AdmissionedFor = model.AdmissionedFor


            };

        }

        public IntakeFeeAgreementEntity ToIntakeFeeAgreementEntity(IntakeFeeAgreementViewModel model, bool isNew)
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
                AdmissionedFor = model.AdmissionedFor

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
                AdmissionedFor = model.AdmissionedFor                

            };

        }

        public IntakeTuberculosisEntity ToIntakeTuberculosisEntity(IntakeTuberculosisViewModel model, bool isNew)
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

        public IntakeMedicalHistoryEntity ToIntakeMedicalHistoryEntity(IntakeMedicalHistoryViewModel model, bool isNew, string userId)
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
                InformationProvided = model.InformationProvided,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                StartTime = model.StartTime,
                EndTime = model.EndTime
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

                AddressPhysician = (model.Client.Doctor == null) ? string.Empty : model.Client.Doctor.Address,
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
                City = (model.Client.Doctor == null) ? string.Empty : model.Client.Doctor.City,
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
                PrimaryCarePhysician = (model.Client.Doctor == null) ? string.Empty: model.Client.Doctor.Name,
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
                State = (model.Client.Doctor == null) ? string.Empty : model.Client.Doctor.State,
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
                ZipCode = (model.Client.Doctor == null)? string.Empty : model.Client.Doctor.ZipCode,

                AgeOfFirstMenstruation = model.AgeOfFirstMenstruation,
                DateOfLastBreastExam = model.DateOfLastBreastExam,
                DateOfLastPelvic = model.DateOfLastPelvic,
                DateOfLastPeriod = model.DateOfLastPeriod,
                UsualDurationOfPeriods = model.UsualDurationOfPeriods,
                UsualIntervalBetweenPeriods = model.UsualIntervalBetweenPeriods,

                AdmissionedFor = model.AdmissionedFor,
                InformationProvided = model.InformationProvided,
                IdDoctor = (model.Client.Doctor == null) ? 0 : model.Client.Doctor.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                StartTime = model.StartTime,
                EndTime = model.EndTime
                
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
                Administrative = model.Administrative,
                ClientTransferred = model.ClientTransferred,
                ClinicalCoherente = model.ClinicalCoherente,
                ClinicalIncoherente = model.ClinicalIncoherente,
                ClinicalInRemission = model.ClinicalInRemission,
                ClinicalStable = model.ClinicalStable,
                ClinicalUnpredictable = model.ClinicalUnpredictable,
                ClinicalUnstable = model.ClinicalUnstable,
                CompletedTreatment = model.CompletedTreatment,
                DateDischarge = model.DateDischarge,
                DateReport = model.DateReport,
                DischargeDiagnosis = model.DischargeDiagnosis,
                LeftBefore = model.LeftBefore,
                NonCompliant = model.NonCompliant,
                Other = model.Other,
                Other_Explain = model.Other_Explain,
                Planned = model.Planned,
                PrognosisFair = model.PrognosisFair,
                PrognosisGood = model.PrognosisGood,
                PrognosisGuarded = model.PrognosisGuarded,
                PrognosisPoor = model.PrognosisPoor,
                ProgramClubHouse = model.ProgramClubHouse,
                ProgramGroup = model.ProgramGroup,
                ProgramInd = model.ProgramInd,
                ProgramPSR = model.ProgramPSR,
                ReferralsTo = model.ReferralsTo,
                Termination = model.Termination,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignaturePerson = model.DateSignaturePerson,
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                Status = model.Status,
                Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == model.IdSupervisor),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TypeService = model.TypeService,
                DateAdmissionService = model.DateAdmissionService,
                Messages = !isNew ? await _context.Messages.Where(m => m.Discharge.Id == model.Id).ToListAsync() : null,
                ClientMoveOutArea = model.ClientMoveOutArea,
                ExtendedHospitalization = model.ExtendedHospitalization,
                Follow_up = model.Follow_up,
                MinimalProgress = model.MinimalProgress,
                ModerateProgress = model.ModerateProgress,
                NoProgress = model.NoProgress,
                PlanCompletePartially = model.PlanCompletePartially,
                Regression = model.Regression,
                SignificantProgress = model.SignificantProgress,
                SummaryOfPresentingProblems = model.SummaryOfPresentingProblems,
                TreatmentSummary = model.TreatmentSummary,
                UnableToDetermine = model.UnableToDetermine,
                JoinCommission = model.JoinCommission
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
                DateDischarge = model.DateDischarge,
                DateReport = model.DateReport,
                Administrative = model.Administrative,
                ClientTransferred = model.ClientTransferred,
                ClinicalCoherente = model.ClinicalCoherente,
                ClinicalIncoherente = model.ClinicalIncoherente,
                ClinicalInRemission = model.ClinicalInRemission,
                ClinicalStable = model.ClinicalStable,
                ClinicalUnpredictable = model.ClinicalUnpredictable,
                ClinicalUnstable = model.ClinicalUnstable,
                CompletedTreatment = model.CompletedTreatment,
                DischargeDiagnosis = model.DischargeDiagnosis,
                LeftBefore = model.LeftBefore,
                Messages = model.Messages,
                NonCompliant = model.NonCompliant,
                Other = model.Other,
                Other_Explain = model.Other_Explain,
                Planned = model.Planned,
                PrognosisFair = model.PrognosisFair,
                PrognosisGood = model.PrognosisGood,
                PrognosisGuarded = model.PrognosisGuarded,
                PrognosisPoor = model.PrognosisPoor,
                ProgramClubHouse = model.ProgramClubHouse,
                ProgramGroup = model.ProgramGroup,
                ProgramInd = model.ProgramInd,
                ProgramPSR = model.ProgramPSR,
                ReferralsTo = model.ReferralsTo,
                Termination = model.Termination,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignaturePerson = model.DateSignaturePerson,
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                Status = model.Status,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TypeService = model.TypeService,
                DateAdmissionService = model.DateAdmissionService,
                ClientMoveOutArea = model.ClientMoveOutArea,
                ExtendedHospitalization = model.ExtendedHospitalization,
                Follow_up = model.Follow_up,
                MinimalProgress = model.MinimalProgress,
                ModerateProgress = model.ModerateProgress,
                NoProgress = model.NoProgress,
                PlanCompletePartially = model.PlanCompletePartially,
                Regression = model.Regression,
                SignificantProgress = model.SignificantProgress,
                SummaryOfPresentingProblems = model.SummaryOfPresentingProblems,
                TreatmentSummary = model.TreatmentSummary,
                UnableToDetermine = model.UnableToDetermine,
                JoinCommission = model.JoinCommission

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
                Prescriber = model.Prescriber,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                ReasonPurpose = model.ReasonPurpose,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,

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
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                ReasonPurpose = model.ReasonPurpose
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
                Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == model.IdSupervisor),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Messages = !isNew ? await _context.Messages.Where(m => m.FarsForm.Id == model.Id).ToListAsync() : null,
                Type = FARSUtils.GetypeByIndex(model.IdType),
                CodeBill = model.CodeBill,
                Units = model.Units,
                StartTime = model.StartTime,
                EndTime = model.EndTime
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
                LastModifiedOn = model.LastModifiedOn,
                Type = model.Type,
                IdType = Convert.ToInt32(model.Type),
                FarsType = _combosHelper.GetComboFARSType(),
                CodeBill = model.CodeBill,
                Units = model.Units,
                StartTime = model.StartTime,
                EndTime = model.EndTime

            };
            
            if (model.Supervisor != null)
                salida.IdSupervisor = model.Supervisor.Id;
            else
                salida.IdSupervisor = 0;
            return salida;

        }

        public async Task<BioEntity> ToBioEntity(BioViewModel model, bool isNew, string userId)
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
                Appetite = BioType.GetBioAppetiteByIndex(model.IdAppetite),
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
                Supervisor = model.Supervisor,
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
                DocumentsAssistant = model.DocumentsAssistant,
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
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                AdmissionedFor = model.AdmissionedFor,
                Messages = _context.Messages
                                   .Where(n => n.Bio.Id == model.Id)
                                   .ToList(),
                Status = model.Status,
                CodeBill = model.CodeBill,
                Units = model.Units,
                AnyEating = model.AnyEating,
                AnyFood = model.AnyFood,
                MilitaryServiceHistory = model.MilitaryServiceHistory,
                MilitaryServiceHistory_Explain = model.MilitaryServiceHistory_Explain,
                VocationalAssesment = model.VocationalAssesment,
                Code90791 = model.Code90791

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
                Supervisor = model.Supervisor,
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
                DocumentsAssistant = model.DocumentsAssistant,
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
                RecentWeight_Status = _combosHelper.GetComboBio_RecentWeight(),
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                AdmissionedFor = model.AdmissionedFor,
                Status = model.Status,
                CodeBill = model.CodeBill,
                Units = model.Units,
                AnyEating = model.AnyEating,
                AnyFood = model.AnyFood,
                MilitaryServiceHistory = model.MilitaryServiceHistory,
                MilitaryServiceHistory_Explain = model.MilitaryServiceHistory_Explain,
                VocationalAssesment = model.VocationalAssesment,
                Code90791 = model.Code90791

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
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Messages = !isNew ? await _context.Messages.Where(m => m.Addendum.Id == model.Id).ToListAsync() : null,
                DateOfApprove = model.DateOfApprove,
                DocumentAssisstant = await _context.DocumentsAssistant.FirstOrDefaultAsync(n => n.Id == model.IdDocumentAssisstant)
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
                LastModifiedOn = model.LastModifiedOn,
                DateOfApprove = model.DateOfApprove

            };
            if (model.Facilitator != null)
                salida.IdFacilitator = model.Facilitator.Id;
            else
                salida.IdFacilitator = 0;
            if (model.Supervisor != null)
                salida.IdSupervisor = model.Supervisor.Id;
            else
                salida.IdSupervisor = 0;
            if (model.DocumentAssisstant != null)
                salida.IdDocumentAssisstant = model.DocumentAssisstant.Id;
            else
                salida.IdDocumentAssisstant = 0;
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
                DataOfService = model.DataOfService,
                Messages = !isNew ? await _context.Messages.Where(m => m.MTPReview.Id == model.Id).ToListAsync() : null,
                CodeBill = model.CodeBill,
                Units = model.Units,
                DateIndFacilitator = model.DateIndFacilitator,
                IndFacilitator = (model.IdIndFacilitator == 0) ? null : await _context.Facilitators
                                               .FindAsync(model.IdIndFacilitator),
                SignIndTherapy = model.SignIndTherapy,
                SignTherapy = model.SignTherapy,
                FacilitatorId = (model.IdFacilitator == 0)? 0: _context.Facilitators
                                        .FirstOrDefault(c => c.Id == model.IdFacilitator).Id
                                              

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
                DataOfService = model.DataOfService,
                CodeBill = model.CodeBill,
                Units = model.Units,
                IdIndFacilitator = (model.IndFacilitator != null) ? model.IndFacilitator.Id : 0,
                DateIndFacilitator = model.DateIndFacilitator,
                IndFacilitator = model.IndFacilitator,
                SignIndTherapy = model.SignIndTherapy,
                SignTherapy = model.SignTherapy,
                IdFacilitator = model.FacilitatorId,
                FacilitatorId = model.FacilitatorId

            };           
        }

        public async Task<TCMIntakeFormEntity> ToTCMIntakeFormEntity(TCMIntakeFormViewModel model, bool isNew, string userId)
        {
            TCMIntakeFormEntity salida;
            salida = new TCMIntakeFormEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),

                TcmClient_FK = model.TcmClient_FK,
                TcmClient = await _context.TCMClient
                                          .Include(m => m.Client)
                                          .ThenInclude(m => m.Clients_Diagnostics)
                                          .ThenInclude(m => m.Diagnostic)
                                          .Include(m => m.Client)
                                          .ThenInclude(m => m.Clients_HealthInsurances)
                                          .ThenInclude(m => m.HealthInsurance)
                                          .Include(m => m.Client.LegalGuardian)
                                          .Include(n => n.Client.EmergencyContact)
                                          .Include(n => n.Client.Client_Referred)
                                          .Include(n => n.Client.Doctor)
                                          .Include(n => n.Client.Psychiatrist)
                                          .FirstOrDefaultAsync(n => n.Id == model.IdTCMClient),
                Agency = model.Agency,
                CaseManagerNotes = model.CaseManagerNotes,
                Elibigility = model.Elibigility,
                EmploymentStatus = EmployedUtils.GetEmployedByIndex(model.IdEmployedStatus),
                Grade = model.Grade,
                IntakeDate = model.IntakeDate,
                IsClientCurrently = model.IsClientCurrently,
                LTC = model.LTC,
                MMA = model.MMA,
                MonthlyFamilyIncome = model.MonthlyFamilyIncome,
                NeedSpecial = model.NeedSpecial,
                NeedSpecial_Specify = model.NeedSpecial_Specify,
                Other = model.Other,
                Other_Address = model.Other_Address,
                Other_City = model.Other_City,
                Other_Phone = model.Other_Phone,
                PrimarySourceIncome = model.PrimarySourceIncome,
                ResidentialStatus = ResidentialUtils.GetResidentialByIndex1(model.IdResidentialStatus),
                School = model.School,
                School_EBD = model.School_EBD,
                School_ESE = model.School_ESE,
                School_ESOL = model.School_ESOL,
                School_HHIP = model.School_HHIP,
                School_Other = model.School_Other,
                School_Regular = model.School_Regular,
                SecondaryContact = model.SecondaryContact,
                SecondaryContact_Phone = model.SecondaryContact_Phone,
                SecondaryContact_RelationShip = model.SecondaryContact_RelationShip,
                TeacherCounselor_Name = model.TeacherCounselor_Name,
                TeacherCounselor_Phone = model.TeacherCounselor_Phone,
                TitlePosition = model.TitlePosition,
                EducationLevel = model.EducationLevel,
                InsuranceOther = model.InsuranceOther,
                ReligionOrEspiritual = model.ReligionOrEspiritual,
                CountryOfBirth = model.CountryOfBirth,
                EmergencyContact = model.EmergencyContact,
                StatusOther = model.StatusOther,
                StatusOther_Explain = model.StatusOther_Explain,
                StatusResident = model.StatusResident,
                StausCitizen = model.StausCitizen,
                YearEnterUsa = model.YearEnterUsa,
                PCP_Name = model.PCP_Name,
                PCP_Address = model.PCP_Address,
                PCP_Phone = model.PCP_Phone,
                PCP_CityStateZip = model.PCP_CityStateZip,
                PCP_Place = model.PCP_Place,
                PCP_FaxNumber = model.PCP_FaxNumber,
                Psychiatrist_Name = model.Psychiatrist_Name,
                Psychiatrist_Address = model.Psychiatrist_Address,
                Psychiatrist_Phone = model.Psychiatrist_Phone,
                Psychiatrist_CityStateZip = model.Psychiatrist_CityStateZip


            };

            return salida;
        }

        public TCMIntakeFormViewModel ToTCMIntakeFormViewModel(TCMIntakeFormEntity model)
        {
            return new TCMIntakeFormViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                IdTCMClient = model.TcmClient.Id,
                IdEmployedStatus = Convert.ToInt32(model.EmploymentStatus),
                EmployedStatus = _combosHelper.GetComboEmployed(),
                TcmClient_FK = model.TcmClient_FK,
                TcmClient = model.TcmClient,
                Agency = model.Agency,
                CaseManagerNotes = model.CaseManagerNotes,
                Elibigility = model.Elibigility,
                EmploymentStatus = model.EmploymentStatus,
                Grade = model.Grade,
                IntakeDate = model.IntakeDate,
                IsClientCurrently = model.IsClientCurrently,
                LTC = model.LTC,
                MMA = model.MMA,
                MonthlyFamilyIncome = model.MonthlyFamilyIncome,
                NeedSpecial = model.NeedSpecial,
                NeedSpecial_Specify = model.NeedSpecial_Specify,
                Other = model.Other,
                Other_Address = model.Other_Address,
                Other_City = model.Other_City,
                Other_Phone = model.Other_Phone,
                PrimarySourceIncome = model.PrimarySourceIncome,
                School = model.School,
                School_EBD = model.School_EBD,
                School_ESE = model.School_ESE,
                School_ESOL = model.School_ESOL,
                School_HHIP = model.School_HHIP,
                School_Other = model.School_Other,
                School_Regular = model.School_Regular,
                SecondaryContact = model.SecondaryContact,
                SecondaryContact_Phone = model.SecondaryContact_Phone,
                SecondaryContact_RelationShip = model.SecondaryContact_RelationShip,
                TeacherCounselor_Name = model.TeacherCounselor_Name,
                TeacherCounselor_Phone = model.TeacherCounselor_Phone,
                TitlePosition = model.TitlePosition,
                EducationLevel = model.EducationLevel,
                InsuranceOther = model.InsuranceOther,
                ReligionOrEspiritual = model.ReligionOrEspiritual,
                CountryOfBirth = model.CountryOfBirth,
                EmergencyContact = model.EmergencyContact,
                StatusOther = model.StatusOther,
                StatusOther_Explain = model.StatusOther_Explain,
                StatusResident = model.StatusResident,
                StausCitizen = model.StausCitizen,
                YearEnterUsa = model.YearEnterUsa,
                IdResidentialStatus = Convert.ToInt32(model.ResidentialStatus),
                ResidentialStatusList = _combosHelper.GetComboResidential(),
                PCP_Name = model.PCP_Name,
                PCP_Address = model.PCP_Address,
                PCP_Phone = model.PCP_Phone,
                PCP_CityStateZip = model.PCP_CityStateZip,
                PCP_Place = model.PCP_Place,
                PCP_FaxNumber = model.PCP_FaxNumber,
                Psychiatrist_Name = model.Psychiatrist_Name,
                Psychiatrist_Address = model.Psychiatrist_Address,
                Psychiatrist_Phone = model.Psychiatrist_Phone,
                Psychiatrist_CityStateZip = model.Psychiatrist_CityStateZip
            };
        }

        public TCMIntakeConsentForTreatmentEntity ToTCMIntakeConsentForTreatmentEntity(TCMIntakeConsentForTreatmentViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeConsentForTreatmentEntity
            {
                Id = isNew ? 0 : model.Id,
                TcmClient = model.TcmClient,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
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
                AdmissionedFor = model.AdmissionedFor
            };
        }

        public TCMIntakeConsentForTreatmentViewModel ToTCMIntakeConsentForTreatmentViewModel(TCMIntakeConsentForTreatmentEntity model)
        {
            return new TCMIntakeConsentForTreatmentViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
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
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn

            };

        }

        public async Task<TCMIntakeConsentForReleaseEntity> ToTCMIntakeConsentForReleaseEntity(TCMIntakeConsentForReleaseViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeConsentForReleaseEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = await _context.TCMClient.FirstOrDefaultAsync(c => c.Id == model.TcmClient_FK),
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                Discharge = model.Discharge,
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
                NameOfFacility = model.NameOfFacility,
                Address = model.Address,
                CityStateZip = model.CityStateZip,
                PhoneNo = model.PhoneNo,
                FaxNo = model.FaxNo,
                ConsentType = ConsentUtils.GetTypeByIndex(model.Idtype),
                OtherAutorizedInformation = model.OtherAutorizedInformation,
                OtherPurposeRequest = model.OtherPurposeRequest,
                InForm_Electronic = model.InForm_Electronic,
                InForm_AllofTheAbove = model.InForm_AllofTheAbove
                
            };
        }

        public TCMIntakeConsentForReleaseViewModel ToTCMIntakeConsentForReleaseViewModel(TCMIntakeConsentForReleaseEntity model)
        {
            return new TCMIntakeConsentForReleaseViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
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
                Discharge = model.Discharge,
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
                NameOfFacility = model.NameOfFacility,
                Address = model.Address,
                CityStateZip = model.CityStateZip,
                PhoneNo = model.PhoneNo,
                FaxNo = model.FaxNo,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                ConsentType = model.ConsentType,
                OtherAutorizedInformation = model.OtherAutorizedInformation,
                OtherPurposeRequest = model.OtherPurposeRequest,
                ConsentList = _combosHelper.GetComboConsentType(),
                Idtype = (model.ConsentType == ConsentType.HURRICANE) ? 1 : (model.ConsentType == ConsentType.PCP) ? 2 : (model.ConsentType == ConsentType.PSYCHIATRIST) ? 3 : (model.ConsentType == ConsentType.EMERGENCY_CONTACT) ? 4 : (model.ConsentType == ConsentType.DCF) ? 5 : (model.ConsentType == ConsentType.SSA) ? 6 : (model.ConsentType == ConsentType.BANK) ? 7 : (model.ConsentType == ConsentType.HOUSING_OFFICES) ? 8 : (model.ConsentType == ConsentType.POLICE_STATION) ? 9 : (model.ConsentType == ConsentType.PHARMACY) ? 10
                : (model.ConsentType == ConsentType.MEDICAL_INSURANCE) ? 11 : (model.ConsentType == ConsentType.CAC) ? 12 : (model.ConsentType == ConsentType.LIFELINESS_PROVIDERS) ? 13 : (model.ConsentType == ConsentType.TAG_AGENCY) ? 14 : (model.ConsentType == ConsentType.STS) ? 15 : (model.ConsentType == ConsentType.DONATION_CENTERS) ? 16 : (model.ConsentType == ConsentType.LTC) ? 17 : (model.ConsentType == ConsentType.INTERNET_SERVICES) ? 18 : (model.ConsentType == ConsentType.USCIS) ? 19 : 0,
                InForm_Electronic = model.InForm_Electronic,
                InForm_AllofTheAbove = model.InForm_AllofTheAbove
            };
        }

        public TCMIntakeConsumerRightsEntity ToTCMIntakeConsumerRightsEntity(TCMIntakeConsumerRightsViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeConsumerRightsEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                ServedOf = model.ServedOf,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor
                

            };
        }

        public TCMIntakeConsumerRightsViewModel ToTCMIntakeConsumerRightsViewModel(TCMIntakeConsumerRightsEntity model)
        {
            return new TCMIntakeConsumerRightsViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                ServedOf = model.ServedOf,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn

            };

        }

        public TCMIntakeAcknowledgementHippaEntity ToTCMIntakeAcknoewledgementHippaEntity(TCMIntakeAcknoewledgementHippaViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeAcknowledgementHippaEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor

            };
        }

        public TCMIntakeAcknoewledgementHippaViewModel ToTCMIntakeAcknoewledgementHippaViewModel(TCMIntakeAcknowledgementHippaEntity model)
        {
            return new TCMIntakeAcknoewledgementHippaViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public TCMIntakeOrientationChecklistEntity ToTCMIntakeOrientationChecklistEntity(TCMIntakeOrientationCheckListViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeOrientationChecklistEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
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
                AdmissionedFor = model.AdmissionedFor

            };
        }

        public TCMIntakeOrientationCheckListViewModel ToTCMIntakeOrientationChecklistViewModel(TCMIntakeOrientationChecklistEntity model)
        {
            return new TCMIntakeOrientationCheckListViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTcmClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
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
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public TCMIntakeAdvancedDirectiveEntity ToTCMIntakeAdvancedDirectiveEntity(TCMIntakeAdvancedDirectiveViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeAdvancedDirectiveEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                IHave = model.IHave,
                IHaveNot = model.IHaveNot

            };
        }

        public TCMIntakeAdvancedDirectiveViewModel ToTCMIntakeAdvancedDirectiveViewModel(TCMIntakeAdvancedDirectiveEntity model)
        {
            return new TCMIntakeAdvancedDirectiveViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                IHave = model.IHave,
                IHaveNot = model.IHaveNot,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };

        }

        public TCMIntakeForeignLanguageEntity ToTCMIntakeForeignLanguageEntity(TCMIntakeForeignLanguageViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeForeignLanguageEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor

            };
        }

        public TCMIntakeForeignLanguageViewModel ToTCMIntakeForeignLanguageViewModel(TCMIntakeForeignLanguageEntity model)
        {
            return new TCMIntakeForeignLanguageViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public TCMIntakeWelcomeEntity ToTCMIntakeWelcomeEntity(TCMIntakeWelcomeViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeWelcomeEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date
            };
        }

        public TCMIntakeWelcomeViewModel ToTCMIntakeWelcomeViewModel(TCMIntakeWelcomeEntity model)
        {
            return new TCMIntakeWelcomeViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };

        }

        public TCMIntakeNonClinicalLogEntity ToTCMIntakeNonClinicalLogEntity(TCMIntakeNonClinicalLogViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeNonClinicalLogEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date,
                DateActivity = model.DateActivity
            };
        }

        public TCMIntakeNonClinicalLogViewModel ToTCMIntakeNonClinicalLogViewModel(TCMIntakeNonClinicalLogEntity model)
        {
            return new TCMIntakeNonClinicalLogViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date,
                DateActivity = model.DateActivity,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };

        }

        public TCMIntakeMiniMentalEntity ToTCMIntakeMiniMenatalEntity(TCMIntakeMiniMentalViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeMiniMentalEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date,
                Attention = model.Attention,
                LanguageCopy = model.LanguageCopy,
                LanguageFollow = model.LanguageFollow,
                LanguageName = model.LanguageName,
                LanguageRead = model.LanguageRead,
                LanguageRepeat = model.LanguageRepeat,
                LanguageWrite = model.LanguageWrite,
                OrientationWhat = model.OrientationWhat,
                OrientationWhere = model.OrientationWhere,
                Recall = model.Recall,
                RegistrationName = model.RegistrationName,
                TotalScore = model.TotalScore,
                Trials = model.Trials
            };
        }

        public TCMIntakeMiniMentalViewModel ToTCMIntakeMiniMenatalViewModel(TCMIntakeMiniMentalEntity model)
        {
            return new TCMIntakeMiniMentalViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date,
                Attention = model.Attention,
                LanguageCopy = model.LanguageCopy,
                LanguageFollow = model.LanguageFollow,
                LanguageName = model.LanguageName,
                LanguageRead = model.LanguageRead,
                LanguageRepeat = model.LanguageRepeat,
                LanguageWrite = model.LanguageWrite,
                OrientationWhat = model.OrientationWhat,
                OrientationWhere = model.OrientationWhere,
                Recall = model.Recall,
                RegistrationName = model.RegistrationName,
                TotalScore = model.TotalScore,
                Trials = model.Trials,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public TCMIntakeCoordinationCareEntity ToTCMIntakeCoordinationCareEntity(TCMIntakeCoordinationCareViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeCoordinationCareEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date,
                DateSignatureEmployee =model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                IAuthorize = model.IAuthorize,
                InformationAllBefore = model.InformationAllBefore,
                InformationElectronic = model.InformationElectronic,
                InformationFascimile = model.InformationFascimile,
                InformationNonKnown = model.InformationNonKnown,
                InformationToRelease = model.InformationToRelease,
                InformationTorequested = model.InformationTorequested,
                InformationVerbal = model.InformationVerbal,
                InformationWrited = model.InformationWrited,
                IRefuse = model.IRefuse,
                PCP = model.PCP,
                Specialist = model.Specialist,
                SpecialistText = model.SpecialistText
            };
        }

        public TCMIntakeCoordinationCareViewModel ToTCMIntakeCoordinationCareViewModel(TCMIntakeCoordinationCareEntity model)
        {
            return new TCMIntakeCoordinationCareViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Date = model.Date,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                IAuthorize = model.IAuthorize,
                InformationAllBefore = model.InformationAllBefore,
                InformationElectronic = model.InformationElectronic,
                InformationFascimile = model.InformationFascimile,
                InformationNonKnown = model.InformationNonKnown,
                InformationToRelease = model.InformationToRelease,
                InformationTorequested = model.InformationTorequested,
                InformationVerbal = model.InformationVerbal,
                InformationWrited = model.InformationWrited,
                IRefuse = model.IRefuse,
                PCP = model.PCP,
                Specialist = model.Specialist,
                SpecialistText = model.SpecialistText,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };

        }

        public async Task<TCMDischargeFollowUpEntity> ToTCMDischargeFollowUpEntity(TCMDischargeFollowUpViewModel model, bool isNew, string userId)
        {
            TCMDischargeFollowUpEntity salida;
            salida  = new TCMDischargeFollowUpEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Address_Location = model.Address_Location,
                NextAppt = model.NextAppt,
                PhoneNumber = model.PhoneNumber,
                ProviderAgency = model.ProviderAgency,
                TcmDischarge = await _context.TCMDischarge.FirstAsync(n => n.Id == model.IdTCMDischarge),
                TypeService = model.TypeService

            };

            return salida;
        }

        public TCMDischargeFollowUpViewModel ToTCMDischargeFollowUpViewModel(TCMDischargeFollowUpEntity model)
        {
            return new TCMDischargeFollowUpViewModel
            {
                Id = model.Id,
                Address_Location = model.Address_Location,
                NextAppt = model.NextAppt,
                PhoneNumber = model.PhoneNumber,
                ProviderAgency = model.ProviderAgency,
                TcmDischarge = model.TcmDischarge,
                TypeService = model.TypeService,
                IdTCMDischarge = model.TcmDischarge.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public async Task<TCMIntakeAppendixJEntity> ToTCMIntakeAppendixJEntity(TCMIntakeAppendixJViewModel model, bool isNew, string userId)
        {
            TCMIntakeAppendixJEntity salida;
            salida = new TCMIntakeAppendixJEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = await _context.TCMClient.Include(n => n.Casemanager).ThenInclude(n => n.TCMSupervisor).FirstAsync(n => n.Id == model.IdTCMClient),
                AdmissionedFor = model.AdmissionedFor,
                Approved = model.Approved,
                Date = model.Date,
                HasBeen = model.HasBeen,
                HasHad = model.HasHad,
                IsAt = model.IsAt,
                IsAwaiting = model.IsAwaiting,
                IsExperiencing = model.IsExperiencing,
                SupervisorSignatureDate = model.SupervisorSignatureDate,
                TcmClient_FK = model.TcmClient_FK,
                TcmSupervisor = model.TcmSupervisor,
                HasAMental2 = model.HasAMental2,
                HasAMental6 = model.HasAMental6,
                HasRecolated = model.HasRecolated,
                IsEnrolled = model.IsEnrolled,
                IsNotReceiving = model.IsNotReceiving,
                Lacks = model.Lacks,
                Meets = model.Meets,
                RequiresOngoing = model.RequiresOngoing,
                RequiresServices = model.RequiresServices,
                AppendixType = AppendixJUtils.GetTypeByIndex(model.IdType),
                Active = model.Active,
                DateExpired = model.DateExpired
            };

            return salida;
        }

        public TCMIntakeAppendixJViewModel ToTCMIntakeAppendixJViewModel(TCMIntakeAppendixJEntity model)
        {
            return new TCMIntakeAppendixJViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Approved = model.Approved,
                Date = model.Date,
                HasBeen = model.HasBeen,
                HasHad = model.HasHad,
                IsAt = model.IsAt,
                IsAwaiting = model.IsAwaiting,
                IsExperiencing = model.IsExperiencing,
                SupervisorSignatureDate = model.SupervisorSignatureDate,
                TcmClient_FK = model.TcmClient_FK,
                TcmSupervisor = model.TcmSupervisor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                HasAMental2 = model.HasAMental2,
                HasAMental6 = model.HasAMental6,
                HasRecolated = model.HasRecolated,
                IsEnrolled = model.IsEnrolled,
                IsNotReceiving = model.IsNotReceiving,
                Lacks = model.Lacks,
                Meets = model.Meets,
                RequiresOngoing = model.RequiresOngoing,
                RequiresServices = model.RequiresServices,
                AppendixType = model.AppendixType,
                Active = model.Active,
                DateExpired = model.DateExpired,
                IdType = (model.AppendixType == AppendixJType.Initial) ? 0 : (model.AppendixType == AppendixJType.Review) ? 1 : 2,
                AppendixJTypes = _combosHelper.GetComboAppendixJType()
            };

        }

        public TCMIntakeInterventionLogEntity ToTCMIntakeInterventionLogEntity(TCMIntakeInterventionLogViewModel model, bool isNew, string userId)
        {
            TCMIntakeInterventionLogEntity salida;
            salida = new TCMIntakeInterventionLogEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient_FK = model.TcmClient_FK,
                InterventionList = new System.Collections.Generic.List<TCMIntakeInterventionEntity>()                
            };

            return salida;
        }

        public TCMIntakeInterventionLogViewModel ToTCMIntakeInterventionLogViewModel(TCMIntakeInterventionLogEntity model)
        {
            return new TCMIntakeInterventionLogViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient_FK,
                TcmClient_FK = model.TcmClient_FK,
                InterventionList = model.InterventionList,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };

        }

        public async Task<TCMIntakeInterventionEntity> ToTCMIntakeInterventionEntity(TCMIntakeInterventionViewModel model, bool isNew, string userId)
        {
            TCMIntakeInterventionEntity salida;
            salida = new TCMIntakeInterventionEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Activity = model.Activity,
                Date = model.Date,
                TcmInterventionLog = await _context.TCMIntakeInterventionLog.FirstOrDefaultAsync(n => n.Id == model.IdInterventionLog)
        };

            return salida;
        }

        public TCMIntakeInterventionViewModel ToTCMIntakeInterventionViewModel(TCMIntakeInterventionEntity model)
        {
            return new TCMIntakeInterventionViewModel
            {
                Id = model.Id,
                Activity = model.Activity,
                Date = model.Date,
                TcmInterventionLog = model.TcmInterventionLog,
                IdInterventionLog = model.TcmInterventionLog.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };

        }

        public async Task<TCMFarsFormEntity> ToTCMFarsFormEntity(TCMFarsFormViewModel model, bool isNew, string userId)
        {
            return new TCMFarsFormEntity
            {
                Id = isNew ? 0 : model.Id,
                TCMClient = await _context.TCMClient
                                          .Include(n => n.Client)  
                                          .FirstOrDefaultAsync(c => c.Id == model.IdTCMClient),
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
                TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.Id == model.IdTCMSupervisor),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmMessages = _context.TCMMessages
                                      .Where(n => n.TCMFarsForm.Id == model.Id)
                                      .ToList()
            };
        }

        public TCMFarsFormViewModel ToTCMFarsFormViewModel(TCMFarsFormEntity model)
        {
            TCMFarsFormViewModel salida;
            salida = new TCMFarsFormViewModel
            {
                Id = model.Id,
                TCMClient = model.TCMClient,
                IdTCMClient = model.TCMClient.Id,
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

            if (model.TCMSupervisor != null)
                salida.IdTCMSupervisor = model.TCMSupervisor.Id;
            else
                salida.IdTCMSupervisor = 0;
            return salida;

        }

        public async Task<TCMAssessmentEntity> ToTCMAssessmentEntity(TCMAssessmentViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentEntity
            {
                Id = isNew ? 0 : model.Id,
                TcmClient = await _context.TCMClient
                                          .Include(n => n.Client)
                                          .FirstOrDefaultAsync(c => c.Id == model.TcmClient_FK),
                
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Approved = model.Approved,
                AreChild = YesNoNAUtils.GetYesNoNaByIndex(model.IdYesNoNAAreChild),
                AreChildAddress = model.AreChildAddress,
                TcmClient_FK = model.TcmClient_FK,
                AreChildCity = model.AreChildCity,
                AreChildName = model.AreChildName,
                AreChildPhone = model.AreChildPhone,
                Caregiver = model.Caregiver,
                ChildFather = model.ChildFather,
                ChildMother = model.ChildMother,
                ClientInput = model.ClientInput,
                DateAssessment = model.DateAssessment,
                Divorced = model.Divorced,
                Family = model.Family,
                Married = model.Married,
                MayWe = YesNoNAUtils.GetYesNoNaByIndex(model.IdYesNoNAWe),
                NeverMarried = model.NeverMarried,
                Other = model.Other,
                OtherExplain = model.OtherExplain,
                PresentingProblems = model.PresentingProblems,
                Referring = model.Referring,
                Review = model.Review,
                School = model.School,
                Separated = model.Separated,
                Treating = model.Treating,
                
                MedicationList = model.MedicationList,
                PastCurrentServiceList = model.PastCurrentServiceList,
                AnyOther = model.AnyOther,
                DateOfOnSetPresentingProblem = model.DateOfOnSetPresentingProblem,
                HasTheClient = model.HasTheClient,
                HouseCompositionList = model.HouseCompositionList,
                HowDoesByFollowing = model.HowDoesByFollowing,
                HowDoesCalendar = model.HowDoesCalendar,
                HowDoesDaily = model.HowDoesDaily,
                HowDoesElectronic = model.HowDoesElectronic,
                HowDoesFamily = model.HowDoesFamily,
                HowDoesKeeping = model.HowDoesKeeping,
                HowDoesOther = model.HowDoesOther,
                HowDoesOtherExplain = model.HowDoesOtherExplain,
                HowDoesPill = model.HowDoesPill,
                HowDoesRNHHA = model.HowDoesRNHHA,
                HowWeelEnable = model.HowWeelEnable,
                HowWeelWithALot = model.HowWeelWithALot,
                HowWeelWithNo = model.HowWeelWithNo,
                HowWeelWithSome = model.HowWeelWithSome,
                IndividualAgencyList = model.IndividualAgencyList,
                PharmacyPhone = model.PharmacyPhone,
                PresentingProblemPrevious = model.PresentingProblemPrevious,
                WhatPharmacy = model.WhatPharmacy,

                HospitalList = model.HospitalList,
                AbuseViolence = model.AbuseViolence,
                Allergy = model.Allergy,
                AllergySpecify = model.AllergySpecify,
                AreAllImmunization = model.AreAllImmunization,
                AreAllImmunizationExplain = model.AreAllImmunizationExplain,
                AreYouPhysician = model.AreYouPhysician,
                AreYouPhysicianSpecify = model.AreYouPhysicianSpecify,
                DateMostRecent = model.DateMostRecent,
                DescribeAnyOther = model.DescribeAnyOther,
                DescribeAnyRisk = model.DescribeAnyRisk,
                DoesAggressiveness = model.DoesAggressiveness,
                DoesAnxiety = model.DoesAnxiety,
                DoesDelusions = model.DoesDelusions,
                DoesDepression = model.DoesDepression,
                DoesFearfulness = model.DoesFearfulness,
                DoesHallucinations = model.DoesHallucinations,
                DoesHelplessness = model.DoesHelplessness,
                DoesHopelessness = model.DoesHopelessness,
                DoesHyperactivity = model.DoesHyperactivity,
                DoesImpulsivity = model.DoesImpulsivity,
                DoesIrritability = model.DoesIrritability,
                DoesLoss = model.DoesLoss,
                DoesLow = model.DoesLow,
                DoesMood = model.DoesMood,
                DoesNegative = model.DoesNegative,
                DoesNervousness = model.DoesNervousness,
                DoesObsessive = model.DoesObsessive,
                DoesPanic = model.DoesPanic,
                DoesParanoia = model.DoesParanoia,
                DoesPoor = model.DoesPoor,
                DoesSadness = model.DoesSadness,
                DoesSelfNeglect = model.DoesSelfNeglect,
                DoesSheUnderstand = model.DoesSheUnderstand,
                DoesSleep = model.DoesSleep,
                DoesTheClientFeel = model.DoesTheClientFeel,
                DoesWithdrawal = model.DoesWithdrawal,
                DrugList = model.DrugList,
                HasClientUndergone = model.HasClientUndergone,
                HasDifficultySeeingLevel = model.HasDifficultySeeingLevel,
                HasDifficultySeeingObjetive = model.HasDifficultySeeingObjetive,
                HasNoImpairment = model.HasNoImpairment,
                HasNoUsefull = model.HasNoUsefull,
                HaveYouEverBeenToAny = model.HaveYouEverBeenToAny,
                HaveYouEverUsedAlcohol = model.HaveYouEverUsedAlcohol,
                HearingDifficulty = model.HearingDifficulty,
                HearingImpairment = model.HearingImpairment,
                HearingNotDetermined = model.HearingNotDetermined,
                Hears = model.Hears,
                Homicidal = model.Homicidal,
                HowActive = FrecuencyActiveUtils.GetFrecuencyActiveByIndex(model.IdFrecuencyActive),
                HowManyTimes = model.HowManyTimes,
                IsClientCurrently = model.IsClientCurrently,
                IsClientPregnancy = YesNoNAUtils.GetYesNoNaByIndex(model.IdYesNoNAPregnancy),
                IsSheReceiving = model.IsSheReceiving,
                Issues = model.Issues,
                LegalDecisionAddress = model.LegalDecisionAddress,
                LegalDecisionAdLitem = model.LegalDecisionAdLitem,
                LegalDecisionAttomey = model.LegalDecisionAttomey,
                LegalDecisionCityStateZip = model.LegalDecisionCityStateZip,
                LegalDecisionLegal = model.LegalDecisionLegal,
                LegalDecisionName = model.LegalDecisionName,
                LegalDecisionNone = model.LegalDecisionNone,
                LegalDecisionOther = model.LegalDecisionOther,
                LegalDecisionOtherExplain = model.LegalDecisionOtherExplain,
                LegalDecisionParent = model.LegalDecisionParent,
                LegalDecisionPhone = model.LegalDecisionPhone,
                MedicalProblemList = model.MedicalProblemList,
                MentalHealth = model.MentalHealth,
                NeedOfSpecial = model.NeedOfSpecial,
                NeedOfSpecialSpecify = model.NeedOfSpecialSpecify,
                NoHearing = model.NoHearing,
                NoUseful = model.NoUseful,
                Outcome = model.Outcome,
                Provider = model.Provider,
                Suicidal = model.Suicidal,
                SurgeryList = model.SurgeryList,
                TypeOfAssessmentAnnual = model.TypeOfAssessmentAnnual,
                TypeOfAssessmentInitial = model.TypeOfAssessmentInitial,
                TypeOfAssessmentOther = model.TypeOfAssessmentOther,
                TypeOfAssessmentOtherExplain = model.TypeOfAssessmentOtherExplain,
                TypeOfAssessmentSignificant = model.TypeOfAssessmentSignificant,
                VisionImpairment = model.VisionImpairment,
                VisionNotDetermined = model.VisionNotDetermined,
                WhenWas = model.WhenWas,

                AcademicEelementary = model.AcademicEelementary,
                AcademicHigh = model.AcademicHigh,
                AcademicMiddle = model.AcademicMiddle,
                AcademicPreSchool = model.AcademicPreSchool,
                AdditionalInformation = model.AdditionalInformation,
                AdditionalInformationMigration = model.AdditionalInformationMigration,
                AHomeVisit = model.AHomeVisit,
                AHomeVisitOn = model.AHomeVisitOn,
                Appliances = model.Appliances,
                AttendanceEelementary = model.AttendanceEelementary,
                AttendanceHigh = model.AttendanceHigh,
                AttendanceMiddle = model.AttendanceMiddle,
                AttendancePreSchool = model.AttendancePreSchool,
                BathingAssistive = model.BathingAssistive,
                BathingIndependent = model.BathingIndependent,
                BathingPhysical = model.BathingPhysical,
                BathingSupervision = model.BathingSupervision,
                BathingTotal = model.BathingTotal,
                Bathtub = model.Bathtub,
                BehaviorEelementary = model.BehaviorEelementary,
                BehaviorHigh = model.BehaviorHigh,
                BehaviorMiddle = model.BehaviorMiddle,
                BehaviorPreSchool = model.BehaviorPreSchool,
                Briefly = model.Briefly,
                CaseManagerWas = model.CaseManagerWas,
                CaseManagerWasDueTo = model.CaseManagerWasDueTo,
                Citizen = model.Citizen,
                ColonCancer = model.ColonCancer,
                CongredatedHowOften = model.CongredatedHowOften,
                CongredatedProvider = model.CongredatedProvider,
                CongredatedReceive = model.CongredatedReceive,
                ContinueToLive = model.ContinueToLive,
                ContinueToLiveOnly = model.ContinueToLiveOnly,
                CookingAssistive = model.CookingAssistive,
                CookingIndependent = model.CookingIndependent,
                CookingPhysical = model.CookingPhysical,
                CookingSupervision = model.CookingSupervision,
                CookingTotal = model.CookingTotal,
                CountryOfBirth = model.CountryOfBirth,
                CurrentEmployer = model.CurrentEmployer,
                DentalExam = model.DentalExam,
                DescribeAnySchool = model.DescribeAnySchool,
                DescribeClientCultural = model.DescribeClientCultural,
                DescribeClientEducation = model.DescribeClientEducation,
                DescribeClientLiving = model.DescribeClientLiving,
                DescribeClientRelationship = model.DescribeClientRelationship,
                DescribeNeighborhood = model.DescribeNeighborhood,
                DescribeOtherNeedConcerns = model.DescribeOtherNeedConcerns,
                DoesClientBasicNeed = model.DoesClientBasicNeed,
                DoesClientCurrently = model.DoesClientCurrently,
                DoesClientCurrentlyExplain = model.DoesClientCurrentlyExplain,
                DoesClientFeel = model.DoesClientFeel,
                DoesClientFeelExplain = model.DoesClientFeelExplain,
                DoesClientNeedAssistance = model.DoesClientNeedAssistance,
                DoesClientNeedAssistanceEducational = model.DoesClientNeedAssistanceEducational,
                DoesClientNeedAssistanceEducationalExplain = model.DoesClientNeedAssistanceEducationalExplain,
                DoesClientNeedAssistanceExplain = model.DoesClientNeedAssistanceExplain,
                DoesNotKnow = model.DoesNotKnow,
                DoingAssistive = model.DoingAssistive,
                DoingIndependent = model.DoingIndependent,
                DoingPhysical = model.DoingPhysical,
                DoingSupervision = model.DoingSupervision,
                DoingTotal = model.DoingTotal,
                DressingAssistive = model.DressingAssistive,
                DressingIndependent = model.DressingIndependent,
                DressingPhysical = model.DressingPhysical,
                DressingSupervision = model.DressingSupervision,
                DressingTotal = model.DressingTotal,
                Drives = model.Drives,
                Electrical = model.Electrical,
                EmployerAddress = model.EmployerAddress,
                EmployerCityState = model.EmployerCityState,
                EmployerContactPerson = model.EmployerContactPerson,
                EmployerPhone = model.EmployerPhone,
                EmploymentStatus = EmployedUtils.GetEmployedByIndex(model.IdEmploymentStatus),
                ExcessiveCluter = model.ExcessiveCluter,
                FailToEelementary = model.FailToEelementary,
                FailToHigh = model.FailToHigh,
                FailToMiddle = model.FailToMiddle,
                FailToPreSchool = model.FailToPreSchool,
                FeedingAssistive = model.FeedingAssistive,
                FeedingIndependent = model.FeedingIndependent,
                FeedingPhysical = model.FeedingPhysical,
                FeedingSupervision = model.FeedingSupervision,
                FeedingTotal = model.FeedingTotal,
                FireHazards = model.FireHazards,
                Flooring = model.Flooring,
                FoodPantryHowOften = model.FoodPantryHowOften,
                FoodPantryProvider = model.FoodPantryProvider,
                FoodPantryReceive = model.FoodPantryReceive,
                FoodStampHowOften = model.FoodStampHowOften,
                FoodStampProvider = model.FoodStampProvider,
                FoodStampReceive = model.FoodStampReceive,
                FriendOrFamily = model.FriendOrFamily,
                GroomingAssistive = model.GroomingAssistive,
                GroomingIndependent = model.GroomingIndependent,
                GroomingPhysical = model.GroomingPhysical,
                GroomingSupervision = model.GroomingSupervision,
                GroomingTotal = model.GroomingTotal,
                HasClientEverArrest = model.HasClientEverArrest,
                HasClientEverArrestLastTime = model.HasClientEverArrestLastTime,
                HasClientEverArrestManyTime = model.HasClientEverArrestManyTime,
                HomeDeliveredHowOften = model.HomeDeliveredHowOften,
                HomeDeliveredProvider = model.HomeDeliveredProvider,
                HomeDeliveredReceive = model.HomeDeliveredReceive,
                IfThereAnyHousing = model.IfThereAnyHousing,
                IfYesWereCriminal = model.IfYesWereCriminal,
                IfYesWhatArea = model.IfYesWhatArea,
                ImmigrationOther = model.ImmigrationOther, 
                ImmigrationOtherExplain = model.ImmigrationOtherExplain,
                Insect = model.Insect,
                IsClientCurrentlyEmployed = model.IsClientCurrentlyEmployed,
                IsClientCurrentlySchool = model.IsClientCurrentlySchool,
                IsClientCurrentlySchoolExplain = model.IsClientCurrentlySchoolExplain,
                IsClientInterested = model.IsClientInterested,
                IsClientInvolved = model.IsClientInvolved,
                IsClientInvolvedSpecify = model.IsClientInvolvedSpecify,
                IsTheClientAbleWork = model.IsTheClientAbleWork,
                IsTheClientAbleWorkLimitation = model.IsTheClientAbleWorkLimitation,
                IsTheClientHavingFinancial = model.IsTheClientHavingFinancial,
                IsTheClientHavingFinancialExplain = model.IsTheClientHavingFinancialExplain,
                IsThereAnyAide = model.IsThereAnyAide,
                IsThereAnyAideName = model.IsThereAnyAideName,
                IsThereAnyAidePhone = model.IsThereAnyAidePhone,
                IsThereAnyCurrentLegalProcess = model.IsThereAnyCurrentLegalProcess,
                LabWorks = model.LabWorks,
                LearningEelementary = model.LearningEelementary,
                LearningHigh = model.LearningHigh,
                LearningMiddle = model.LearningMiddle,
                LearningPreSchool = model.LearningPreSchool,
                ListAnyNeed = model.ListAnyNeed,
                ListClientCurrentPotencialStrngths = model.ListClientCurrentPotencialStrngths,
                ListClientCurrentPotencialWeakness = model.ListClientCurrentPotencialWeakness,
                MakingAssistive = model.MakingAssistive,
                MakingIndependent = model.MakingIndependent,
                MakingPhysical = model.MakingPhysical,
                MakingSupervision = model.MakingSupervision,
                MakingTotal = model.MakingTotal,
                Mammogram = model.Mammogram,
                MayWeLeaveSend = model.MayWeLeaveSend,
                MonthlyFamilyIncome = model.MonthlyFamilyIncome,
                NoAirCondition = model.NoAirCondition,
                NoTelephone = model.NoTelephone,
                NotHot = model.NotHot,
                NumberOfBedrooms = model.NumberOfBedrooms,
                NumberOfPersonLiving = model.NumberOfPersonLiving,
                OtherFinancial = model.OtherFinancial,
                OtherHowOften = model.OtherHowOften,
                OtherProvider = model.OtherProvider,
                OtherReceive = model.OtherReceive,
                PapAndHPV = model.PapAndHPV,
                ParticipationEelementary = model.ParticipationEelementary,
                ParticipationHigh = model.ParticipationHigh,
                ParticipationMiddle = model.ParticipationMiddle,
                ParticipationPreSchool = model.ParticipationPreSchool,
                PersonPorBedrooms = model.PersonPorBedrooms,
                PhysicalExam = model.PhysicalExam,
                PhysicalOther = model.PhysicalOther,
                PreferToLive = model.PreferToLive,
                Poor = model.Poor,
                ProbationOfficer = model.ProbationOfficer,
                ProbationOfficerName = model.ProbationOfficerName,
                ProbationOfficerPhone = model.ProbationOfficerPhone,
                RecommendedActivities = model.RecommendedActivities,
                RecommendedBasicNeed = model.RecommendedBasicNeed,
                RecommendedEconomic = model.RecommendedEconomic,
                RecommendedHousing = model.RecommendedHousing,
                RecommendedLegalImmigration = model.RecommendedLegalImmigration,
                RecommendedMentalHealth = model.RecommendedMentalHealth,
                RecommendedOther = model.RecommendedOther,
                RecommendedOtherSpecify = model.RecommendedOtherSpecify,
                RecommendedPhysicalHealth = model.RecommendedPhysicalHealth,
                RecommendedRecreational = model.RecommendedRecreational,
                RecommendedSchool = model.RecommendedSchool,
                RecommendedTransportation = model.RecommendedTransportation,
                RecommendedVocation = model.RecommendedVocation,
                RelationshipEelementary = model.RelationshipEelementary,
                RelationshipHigh =model.RelationshipHigh,
                RelationshipMiddle = model.RelationshipMiddle,
                RelationshipPreSchool = model.RelationshipPreSchool,
                Resident = model.Resident,
                ResidentStatus = ResidentialUtils.GetResidentialByIndex1(model.IdResidentStatus),
                SchoolAddress = model.SchoolAddress,
                SchoolCityState = model.SchoolCityState,
                SchoolDistrict = model.SchoolDistrict,
                SchoolGrade = model.SchoolGrade,
                SchoolName = model.SchoolName,
                SchoolProgramEBD = model.SchoolProgramEBD,
                SchoolProgramESE = model.SchoolProgramESE,
                SchoolProgramESOL = model.SchoolProgramESOL,
                SchoolProgramHHIP = model.SchoolProgramHHIP,
                SchoolProgramOther = model.SchoolProgramOther,
                SchoolProgramRegular = model.SchoolProgramRegular,
                SchoolProgramTeacherName = model.SchoolProgramTeacherName,
                SchoolProgramTeacherPhone = model.SchoolProgramTeacherPhone,
                ShoppingAssistive = model.ShoppingAssistive,
                ShoppingIndependent = model.ShoppingIndependent,
                ShoppingPhysical = model.ShoppingPhysical,
                ShoppingSupervision = model.ShoppingSupervision,
                ShoppingTotal = model.ShoppingTotal,
                Staff = model.Staff,
                Stairs = model.Stairs,
                Structural = model.Structural,
                TakesABus = model.TakesABus,
                TransferringAssistive = model.TransferringAssistive,
                TransferringIndependent = model.TransferringIndependent,
                TransferringPhysical = model.TransferringPhysical,
                TransferringSupervision = model.TransferringSupervision,
                TransferringTotal = model.TransferringTotal,
                TransportationOther = model.TransportationOther,
                TransportationOtherExplain = model.TransportationOtherExplain,
                Tripping = model.Tripping,
                Unsanitary = model.Unsanitary,
                VocationalEmployment = model.VocationalEmployment,
                Walks = model.Walks,
                WhatActivityThings = model.WhatActivityThings,
                WhatIsCollegeGraduated = model.WhatIsCollegeGraduated,
                WhatIsElementary = model.WhatIsElementary,
                WhatIsGED = model.WhatIsGED,
                WhatIsGraduated = model.WhatIsGraduated,
                WhatIsGraduatedDegree = model.WhatIsGraduatedDegree,
                WhatIsHighSchool = model.WhatIsHighSchool,
                WhatIsMiddle = model.WhatIsMiddle,
                WhatIsNoSchool = model.WhatIsNoSchool,
                WhatIsSomeCollege = model.WhatIsSomeCollege,
                WhatIsSomeHigh = model.WhatIsSomeHigh,
                WhatIsTheMainSource = model.WhatIsTheMainSource,
                WhatIsTradeSchool = model.WhatIsTradeSchool,
                WhatIsUnknown = model.WhatIsUnknown,
                WouldLikeObtainJob = model.WouldLikeObtainJob,
                WouldLikeObtainJobNotAtThisTime = model.WouldLikeObtainJobNotAtThisTime,
                YearEnteredUsa = model.YearEnteredUsa,

                CantDoItAtAll = model.CantDoItAtAll,
                DateSignatureCaseManager = model.DateSignatureCaseManager,
                DateSignatureTCMSupervisor = model.DateSignatureTCMSupervisor,
                DoesClientTranspotation = model.DoesClientTranspotation,
                DoesClientTranspotationExplain = model.DoesClientTranspotationExplain,
                HoweverOn = model.HoweverOn,
                HoweverVisitScheduler = model.HoweverVisitScheduler,
                NeedALot = model.NeedALot,
                NeedNoHelp = model.NeedNoHelp,
                NeedSome = model.NeedSome,
                OtherReceiveExplain = model.OtherReceiveExplain,
                Status = TCMDocumentStatus.Approved,
                TCMSupervisor = model.TCMSupervisor,
                TcmMessages = _context.TCMMessages
                                      .Where(n => n.TCMAssessment.Id == model.Id)
                                      .ToList()
            
            };
        }

        public TCMAssessmentViewModel ToTCMAssessmentViewModel(TCMAssessmentEntity model)
        {
            TCMAssessmentViewModel salida;
            salida = new TCMAssessmentViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Approved = model.Approved,
                IdYesNoNAAreChild = (model.AreChild == YesNoNAType.Yes) ? 0 : (model.AreChild == YesNoNAType.No) ? 1 : 2,
                AreChild = model.AreChild,
                AreChildAddress = model.AreChildAddress,
                TcmClient_FK = model.TcmClient_FK,
                AreChildCity = model.AreChildCity,
                AreChildName = model.AreChildName,
                AreChildPhone = model.AreChildPhone,
                Caregiver = model.Caregiver,
                ChildFather = model.ChildFather,
                ChildMother = model.ChildMother,
                ClientInput = model.ClientInput,
                DateAssessment = model.DateAssessment,
                Divorced = model.Divorced,
                Family = model.Family,
                Married = model.Married,
                IdYesNoNAWe = (model.MayWe == YesNoNAType.Yes) ? 0 : (model.MayWe == YesNoNAType.No) ? 1 : 2,
                YesNoNAs = _combosHelper.GetComboYesNoNA(),
                MayWe = model.MayWe,
                NeverMarried = model.NeverMarried,
                Other = model.Other,
                OtherExplain = model.OtherExplain,
                PresentingProblems = model.PresentingProblems,
                Referring = model.Referring,
                Review = model.Review,
                School = model.School,
                Separated = model.Separated,
                Treating = model.Treating,
                MedicationList = model.MedicationList,
                PastCurrentServiceList = model.PastCurrentServiceList,
                AnyOther = model.AnyOther,
                DateOfOnSetPresentingProblem = (model.DateOfOnSetPresentingProblem.Year == 1) ? DateTime.Now : model.DateOfOnSetPresentingProblem,
                HasTheClient = model.HasTheClient,
                HouseCompositionList = model.HouseCompositionList,
                HowDoesByFollowing = model.HowDoesByFollowing,
                HowDoesCalendar = model.HowDoesCalendar,
                HowDoesDaily = model.HowDoesDaily,
                HowDoesElectronic = model.HowDoesElectronic,
                HowDoesFamily = model.HowDoesFamily,
                HowDoesKeeping = model.HowDoesKeeping,
                HowDoesOther = model.HowDoesOther,
                HowDoesOtherExplain = model.HowDoesOtherExplain,
                HowDoesPill = model.HowDoesPill,
                HowDoesRNHHA = model.HowDoesRNHHA,
                HowWeelEnable = model.HowWeelEnable,
                HowWeelWithALot = model.HowWeelWithALot,
                HowWeelWithNo = model.HowWeelWithNo,
                HowWeelWithSome = model.HowWeelWithSome,
                IndividualAgencyList = model.IndividualAgencyList,
                PharmacyPhone = model.PharmacyPhone,
                PresentingProblemPrevious = model.PresentingProblemPrevious,
                WhatPharmacy = model.WhatPharmacy,
                HospitalList = model.HospitalList,
                AbuseViolence = model.AbuseViolence,
                Allergy = model.Allergy,
                AllergySpecify = model.AllergySpecify,
                AreAllImmunization = model.AreAllImmunization,
                AreAllImmunizationExplain = model.AreAllImmunizationExplain,
                AreYouPhysician = model.AreYouPhysician,
                AreYouPhysicianSpecify = model.AreYouPhysicianSpecify,
                DateMostRecent = model.DateMostRecent,
                DescribeAnyOther = model.DescribeAnyOther,
                DescribeAnyRisk = model.DescribeAnyRisk,
                DoesAggressiveness = model.DoesAggressiveness,
                DoesAnxiety = model.DoesAnxiety,
                DoesDelusions = model.DoesDelusions,
                DoesDepression = model.DoesDepression,
                DoesFearfulness = model.DoesFearfulness,
                DoesHallucinations = model.DoesHallucinations,
                DoesHelplessness = model.DoesHelplessness,
                DoesHopelessness = model.DoesHopelessness,
                DoesHyperactivity = model.DoesHyperactivity,
                DoesImpulsivity = model.DoesImpulsivity,
                DoesIrritability = model.DoesIrritability,
                DoesLoss = model.DoesLoss,
                DoesLow = model.DoesLow,
                DoesMood = model.DoesMood,
                DoesNegative = model.DoesNegative,
                DoesNervousness = model.DoesNervousness,
                DoesObsessive = model.DoesObsessive,
                DoesPanic = model.DoesPanic,
                DoesParanoia = model.DoesParanoia,
                DoesPoor = model.DoesPoor,
                DoesSadness = model.DoesSadness,
                DoesSelfNeglect = model.DoesSelfNeglect,
                DoesSheUnderstand = model.DoesSheUnderstand,
                DoesSleep = model.DoesSleep,
                DoesTheClientFeel = model.DoesTheClientFeel,
                DoesWithdrawal = model.DoesWithdrawal,
                DrugList = model.DrugList,
                HasClientUndergone = model.HasClientUndergone,
                HasDifficultySeeingLevel = model.HasDifficultySeeingLevel,
                HasDifficultySeeingObjetive = model.HasDifficultySeeingObjetive,
                HasNoImpairment = model.HasNoImpairment,
                HasNoUsefull = model.HasNoUsefull,
                HaveYouEverBeenToAny = model.HaveYouEverBeenToAny,
                HaveYouEverUsedAlcohol = model.HaveYouEverUsedAlcohol,
                HearingDifficulty = model.HearingDifficulty,
                HearingImpairment = model.HearingImpairment,
                HearingNotDetermined = model.HearingNotDetermined,
                Hears = model.Hears,
                Homicidal = model.Homicidal,
                HowActive = model.HowActive,
                HowManyTimes = model.HowManyTimes,
                IsClientCurrently = model.IsClientCurrently,
                IdYesNoNAPregnancy = (model.IsClientPregnancy == YesNoNAType.Yes) ? 0 : (model.IsClientPregnancy == YesNoNAType.No) ? 1 : 2,
                IsSheReceiving = model.IsSheReceiving,
                Issues = model.Issues,
                LegalDecisionAddress = model.LegalDecisionAddress, 
                LegalDecisionAdLitem = model.LegalDecisionAdLitem,
                LegalDecisionAttomey = model.LegalDecisionAttomey, 
                LegalDecisionCityStateZip = model.LegalDecisionCityStateZip, 
                LegalDecisionLegal = model.LegalDecisionLegal, 
                LegalDecisionName = model.LegalDecisionName,
                LegalDecisionNone = model.LegalDecisionNone,
                LegalDecisionOther = model.LegalDecisionOther, 
                LegalDecisionOtherExplain = model.LegalDecisionOtherExplain, 
                LegalDecisionParent = model.LegalDecisionParent,
                LegalDecisionPhone = model.LegalDecisionPhone,
                MedicalProblemList = model.MedicalProblemList,
                MentalHealth = model.MentalHealth,
                NeedOfSpecial = model.NeedOfSpecial,
                NeedOfSpecialSpecify = model.NeedOfSpecialSpecify,
                NoHearing = model.NoHearing,
                NoUseful = model.NoUseful,
                Outcome = model.Outcome,
                Provider = model.Provider,
                Suicidal = model.Suicidal,
                SurgeryList = model.SurgeryList,
                TypeOfAssessmentAnnual = model.TypeOfAssessmentAnnual,
                TypeOfAssessmentInitial = model.TypeOfAssessmentInitial,
                TypeOfAssessmentOther = model.TypeOfAssessmentOther,
                TypeOfAssessmentOtherExplain = model.TypeOfAssessmentOtherExplain,
                TypeOfAssessmentSignificant = model.TypeOfAssessmentSignificant,
                VisionImpairment = model.VisionImpairment,
                VisionNotDetermined = model.VisionNotDetermined,
                WhenWas = model.WhenWas,
                AcademicEelementary = model.AcademicEelementary,
                AcademicHigh = model.AcademicHigh,
                AcademicMiddle = model.AcademicMiddle,
                AcademicPreSchool = model.AcademicPreSchool,
                AdditionalInformation = model.AdditionalInformation,
                AdditionalInformationMigration = model.AdditionalInformationMigration,
                AHomeVisit = model.AHomeVisit,
                AHomeVisitOn = model.AHomeVisitOn,
                Appliances = model.Appliances,
                AttendanceEelementary = model.AttendanceEelementary,
                AttendanceHigh = model.AttendanceHigh,
                AttendanceMiddle = model.AttendanceMiddle,
                AttendancePreSchool = model.AttendancePreSchool,
                BathingAssistive = model.BathingAssistive,
                BathingIndependent = model.BathingIndependent,
                BathingPhysical = model.BathingPhysical,
                BathingSupervision = model.BathingSupervision,
                BathingTotal = model.BathingTotal,
                Bathtub = model.Bathtub,
                BehaviorEelementary = model.BehaviorEelementary,
                BehaviorHigh = model.BehaviorHigh,
                BehaviorMiddle = model.BehaviorMiddle,
                BehaviorPreSchool = model.BehaviorPreSchool,
                Briefly = model.Briefly,
                CaseManagerWas = model.CaseManagerWas,
                CaseManagerWasDueTo = model.CaseManagerWasDueTo,
                Citizen = model.Citizen,
                ColonCancer = model.ColonCancer,
                CongredatedHowOften = model.CongredatedHowOften,
                CongredatedProvider = model.CongredatedProvider,
                CongredatedReceive = model.CongredatedReceive,
                ContinueToLive = model.ContinueToLive,
                ContinueToLiveOnly = model.ContinueToLiveOnly,
                CookingAssistive = model.CookingAssistive,
                CookingIndependent = model.CookingIndependent,
                CookingPhysical = model.CookingPhysical,
                CookingSupervision = model.CookingSupervision,
                CookingTotal = model.CookingTotal,
                CountryOfBirth = model.CountryOfBirth,
                CurrentEmployer = model.CurrentEmployer,
                DentalExam = model.DentalExam,
                DescribeAnySchool = model.DescribeAnySchool,
                DescribeClientCultural = model.DescribeClientCultural,
                DescribeClientEducation = model.DescribeClientEducation,
                DescribeClientLiving = model.DescribeClientLiving,
                DescribeClientRelationship = model.DescribeClientRelationship,
                DescribeNeighborhood = model.DescribeNeighborhood,
                DescribeOtherNeedConcerns = model.DescribeOtherNeedConcerns,
                DoesClientBasicNeed = model.DoesClientBasicNeed,
                DoesClientCurrently = model.DoesClientCurrently,
                DoesClientCurrentlyExplain = model.DoesClientCurrentlyExplain,
                DoesClientFeel = model.DoesClientFeel,
                DoesClientFeelExplain = model.DoesClientFeelExplain,
                DoesClientNeedAssistance = model.DoesClientNeedAssistance,
                DoesClientNeedAssistanceEducational = model.DoesClientNeedAssistanceEducational,
                DoesClientNeedAssistanceEducationalExplain = model.DoesClientNeedAssistanceEducationalExplain,
                DoesClientNeedAssistanceExplain = model.DoesClientNeedAssistanceExplain,
                DoesNotKnow = model.DoesNotKnow,
                DoingAssistive = model.DoingAssistive,
                DoingIndependent = model.DoingIndependent,
                DoingPhysical = model.DoingPhysical,
                DoingSupervision = model.DoingSupervision,
                DoingTotal = model.DoingTotal,
                DressingAssistive = model.DressingAssistive,
                DressingIndependent = model.DressingIndependent,
                DressingPhysical = model.DressingPhysical,
                DressingSupervision = model.DressingSupervision,
                DressingTotal = model.DressingTotal,
                Drives = model.Drives,
                Electrical = model.Electrical,
                EmployerAddress = model.EmployerAddress,
                EmployerCityState = model.EmployerCityState,
                EmployerContactPerson = model.EmployerContactPerson,
                EmployerPhone = model.EmployerPhone,
                EmploymentStatus = model.EmploymentStatus,
                IdEmploymentStatus = (model.EmploymentStatus == EmploymentStatus.EmployetFT) ? 0 : (model.EmploymentStatus == EmploymentStatus.EmployetPT) ? 1 : (model.EmploymentStatus == EmploymentStatus.Retired) ? 2 : (model.EmploymentStatus == EmploymentStatus.Disabled) ? 3 : (model.EmploymentStatus == EmploymentStatus.Homemaker) ? 4 : (model.EmploymentStatus == EmploymentStatus.Student) ? 5 : (model.EmploymentStatus == EmploymentStatus.Unemployed) ? 6 : 7,
                EmploymentStatuss = _combosHelper.GetComboEmployed(),
                ExcessiveCluter = model.ExcessiveCluter,
                FailToEelementary = model.FailToEelementary,
                FailToHigh = model.FailToHigh,
                FailToMiddle = model.FailToMiddle,
                FailToPreSchool = model.FailToPreSchool,
                FeedingAssistive = model.FeedingAssistive,
                FeedingIndependent = model.FeedingIndependent,
                FeedingPhysical = model.FeedingPhysical,
                FeedingSupervision = model.FeedingSupervision,
                FeedingTotal = model.FeedingTotal,
                FireHazards = model.FireHazards,
                Flooring = model.Flooring,
                FoodPantryHowOften = model.FoodPantryHowOften,
                FoodPantryProvider = model.FoodPantryProvider,
                FoodPantryReceive = model.FoodPantryReceive,
                FoodStampHowOften = model.FoodStampHowOften,
                FoodStampProvider = model.FoodStampProvider,
                FoodStampReceive = model.FoodStampReceive,
                FriendOrFamily = model.FriendOrFamily,
                GroomingAssistive = model.GroomingAssistive,
                GroomingIndependent = model.GroomingIndependent,
                GroomingPhysical = model.GroomingPhysical,
                GroomingSupervision = model.GroomingSupervision,
                GroomingTotal = model.GroomingTotal,
                HasClientEverArrest = model.HasClientEverArrest,
                HasClientEverArrestLastTime = model.HasClientEverArrestLastTime,
                HasClientEverArrestManyTime = model.HasClientEverArrestManyTime,
                HomeDeliveredHowOften = model.HomeDeliveredHowOften,
                HomeDeliveredProvider = model.HomeDeliveredProvider,
                HomeDeliveredReceive = model.HomeDeliveredReceive,
                IfThereAnyHousing = model.IfThereAnyHousing,
                IfYesWereCriminal = model.IfYesWereCriminal,
                IfYesWhatArea = model.IfYesWhatArea,
                ImmigrationOther = model.ImmigrationOther,
                ImmigrationOtherExplain = model.ImmigrationOtherExplain,
                Insect = model.Insect,
                IsClientCurrentlyEmployed = model.IsClientCurrentlyEmployed,
                IsClientCurrentlySchool = model.IsClientCurrentlySchool,
                IsClientCurrentlySchoolExplain = model.IsClientCurrentlySchoolExplain,
                IsClientInterested = model.IsClientInterested,
                IsClientInvolved = model.IsClientInvolved,
                IsClientInvolvedSpecify = model.IsClientInvolvedSpecify,
                IsTheClientAbleWork = model.IsTheClientAbleWork,
                IsTheClientAbleWorkLimitation = model.IsTheClientAbleWorkLimitation,
                IsTheClientHavingFinancial = model.IsTheClientHavingFinancial,
                IsTheClientHavingFinancialExplain = model.IsTheClientHavingFinancialExplain,
                IsThereAnyAide = model.IsThereAnyAide,
                IsThereAnyAideName = model.IsThereAnyAideName,
                IsThereAnyAidePhone = model.IsThereAnyAidePhone,
                IsThereAnyCurrentLegalProcess = model.IsThereAnyCurrentLegalProcess,
                LabWorks = model.LabWorks,
                LearningEelementary = model.LearningEelementary,
                LearningHigh = model.LearningHigh,
                LearningMiddle = model.LearningMiddle,
                LearningPreSchool = model.LearningPreSchool,
                ListAnyNeed = model.ListAnyNeed,
                ListClientCurrentPotencialStrngths = model.ListClientCurrentPotencialStrngths,
                ListClientCurrentPotencialWeakness = model.ListClientCurrentPotencialWeakness,
                MakingAssistive = model.MakingAssistive,
                MakingIndependent = model.MakingIndependent,
                MakingPhysical = model.MakingPhysical,
                MakingSupervision = model.MakingSupervision,
                MakingTotal = model.MakingTotal,
                Mammogram = model.Mammogram,
                MayWeLeaveSend = model.MayWeLeaveSend,
                MonthlyFamilyIncome = model.MonthlyFamilyIncome,
                NoAirCondition = model.NoAirCondition,
                NoTelephone = model.NoTelephone,
                NotHot = model.NotHot,
                NumberOfBedrooms = model.NumberOfBedrooms,
                NumberOfPersonLiving = model.NumberOfPersonLiving,
                OtherFinancial = model.OtherFinancial,
                OtherHowOften = model.OtherHowOften,
                OtherProvider = model.OtherProvider,
                OtherReceive = model.OtherReceive,
                PapAndHPV = model.PapAndHPV,
                ParticipationEelementary = model.ParticipationEelementary,
                ParticipationHigh = model.ParticipationHigh,
                ParticipationMiddle = model.ParticipationMiddle,
                ParticipationPreSchool = model.ParticipationPreSchool,
                PersonPorBedrooms = model.PersonPorBedrooms,
                PhysicalExam = model.PhysicalExam,
                PhysicalOther = model.PhysicalOther,
                PreferToLive = model.PreferToLive,
                Poor = model.Poor,
                ProbationOfficer = model.ProbationOfficer,
                ProbationOfficerName = model.ProbationOfficerName,
                ProbationOfficerPhone = model.ProbationOfficerPhone,
                RecommendedActivities = model.RecommendedActivities,
                RecommendedBasicNeed = model.RecommendedBasicNeed,
                RecommendedEconomic = model.RecommendedEconomic,
                RecommendedHousing = model.RecommendedHousing,
                RecommendedLegalImmigration = model.RecommendedLegalImmigration,
                RecommendedMentalHealth = model.RecommendedMentalHealth,
                RecommendedOther = model.RecommendedOther,
                RecommendedOtherSpecify = model.RecommendedOtherSpecify,
                RecommendedPhysicalHealth = model.RecommendedPhysicalHealth,
                RecommendedRecreational = model.RecommendedRecreational,
                RecommendedSchool = model.RecommendedSchool,
                RecommendedTransportation = model.RecommendedTransportation,
                RecommendedVocation = model.RecommendedVocation,
                RelationshipEelementary = model.RelationshipEelementary,
                RelationshipHigh = model.RelationshipHigh,
                RelationshipMiddle = model.RelationshipMiddle,
                RelationshipPreSchool = model.RelationshipPreSchool,
                Resident = model.Resident,
                IdResidentStatus = (model.ResidentStatus == ResidentialStatus.LivingAlone)? 0 : (model.ResidentStatus == ResidentialStatus.livingWithRelatives) ? 1 : (model.ResidentStatus == ResidentialStatus.livingWithNoRelatives) ? 2 : (model.ResidentStatus == ResidentialStatus.AsistedLivingFacility) ? 3 : (model.ResidentStatus == ResidentialStatus.FosterCare_GroupHome) ? 4 : (model.ResidentStatus == ResidentialStatus.Hospital_NursingHome) ? 5 : (model.ResidentStatus == ResidentialStatus.ResidentialProgram) ? 6 : (model.ResidentStatus == ResidentialStatus.Correctional_Facility) ? 7 : 8,
                ResidentStatuss = _combosHelper.GetComboResidential(),
                ResidentStatus = model.ResidentStatus,
                SchoolAddress = model.SchoolAddress,
                SchoolCityState = model.SchoolCityState,
                SchoolDistrict = model.SchoolDistrict,
                SchoolGrade = model.SchoolGrade,
                SchoolName = model.SchoolName,
                SchoolProgramEBD = model.SchoolProgramEBD,
                SchoolProgramESE = model.SchoolProgramESE,
                SchoolProgramESOL = model.SchoolProgramESOL,
                SchoolProgramHHIP = model.SchoolProgramHHIP,
                SchoolProgramOther = model.SchoolProgramOther,
                SchoolProgramRegular = model.SchoolProgramRegular,
                SchoolProgramTeacherName = model.SchoolProgramTeacherName,
                SchoolProgramTeacherPhone = model.SchoolProgramTeacherPhone,
                ShoppingAssistive = model.ShoppingAssistive,
                ShoppingIndependent = model.ShoppingIndependent,
                ShoppingPhysical = model.ShoppingPhysical,
                ShoppingSupervision = model.ShoppingSupervision,
                ShoppingTotal = model.ShoppingTotal,
                Staff = model.Staff,
                Stairs = model.Stairs,
                Structural = model.Structural,
                TakesABus = model.TakesABus,
                TransferringAssistive = model.TransferringAssistive,
                TransferringIndependent = model.TransferringIndependent,
                TransferringPhysical = model.TransferringPhysical,
                TransferringSupervision = model.TransferringSupervision,
                TransferringTotal = model.TransferringTotal,
                TransportationOther = model.TransportationOther,
                TransportationOtherExplain = model.TransportationOtherExplain,
                Tripping = model.Tripping,
                Unsanitary = model.Unsanitary,
                VocationalEmployment = model.VocationalEmployment,
                Walks = model.Walks,
                WhatActivityThings = model.WhatActivityThings,
                WhatIsCollegeGraduated = model.WhatIsCollegeGraduated,
                WhatIsElementary = model.WhatIsElementary,
                WhatIsGED = model.WhatIsGED,
                WhatIsGraduated = model.WhatIsGraduated,
                WhatIsGraduatedDegree = model.WhatIsGraduatedDegree,
                WhatIsHighSchool = model.WhatIsHighSchool,
                WhatIsMiddle = model.WhatIsMiddle,
                WhatIsNoSchool = model.WhatIsNoSchool,
                WhatIsSomeCollege = model.WhatIsSomeCollege,
                WhatIsSomeHigh = model.WhatIsSomeHigh,
                WhatIsTheMainSource = model.WhatIsTheMainSource,
                WhatIsTradeSchool = model.WhatIsTradeSchool,
                WhatIsUnknown = model.WhatIsUnknown,
                WouldLikeObtainJob = model.WouldLikeObtainJob,
                WouldLikeObtainJobNotAtThisTime = model.WouldLikeObtainJobNotAtThisTime,
                YearEnteredUsa = model.YearEnteredUsa,               
                CantDoItAtAll = model.CantDoItAtAll,
                DateSignatureCaseManager = (model.DateSignatureCaseManager.Year == 1) ? DateTime.Now : model.DateSignatureCaseManager,                              
                DateSignatureTCMSupervisor = (model.DateSignatureTCMSupervisor.Year == 1) ? DateTime.Now : model.DateSignatureTCMSupervisor,                
                DoesClientTranspotation = model.DoesClientTranspotation,
                DoesClientTranspotationExplain = model.DoesClientTranspotationExplain,
                HoweverOn = model.HoweverOn,
                HoweverVisitScheduler = model.HoweverVisitScheduler,                
                NeedALot = model.NeedALot,
                NeedNoHelp = model.NeedNoHelp,
                NeedSome = model.NeedSome,
                OtherReceiveExplain = model.OtherReceiveExplain,
                Status = TCMDocumentStatus.Edition,
                TCMSupervisor = model.TCMSupervisor,
                Client_Referred_List = model.TcmClient.Client.Client_Referred.ToList(),
                IdFrecuencyActive = (model.HowActive == FrecuencyActive.Daily) ? 0: (model.HowActive == FrecuencyActive.Three_Time_per_week_or_more) ? 1 : (model.HowActive == FrecuencyActive.Three_Time_per_week_or_less) ? 2 : (model.HowActive == FrecuencyActive.Once_per_week) ? 3 : (model.HowActive == FrecuencyActive.Rarely) ? 4 : 5,
                FrecuencyActiveList = _combosHelper.GetComboFrecuencyActive(),
                Psychiatrist_Name = (model.TcmClient.TCMIntakeForm == null)? "": model.TcmClient.TCMIntakeForm.Psychiatrist_Name,
                Psychiatrist_Address = (model.TcmClient.TCMIntakeForm == null) ? "" : model.TcmClient.TCMIntakeForm.Psychiatrist_Address,
                Psychiatrist_Phone = (model.TcmClient.TCMIntakeForm == null) ? "" : model.TcmClient.TCMIntakeForm.Psychiatrist_Phone,
                Psychiatrist_CityStateZip = (model.TcmClient.TCMIntakeForm == null) ? "" : model.TcmClient.TCMIntakeForm.Psychiatrist_CityStateZip,
                PCP_Name = (model.TcmClient.TCMIntakeForm == null) ? "" : model.TcmClient.TCMIntakeForm.PCP_Name,
                PCP_Address = (model.TcmClient.TCMIntakeForm == null) ? "": model.TcmClient.TCMIntakeForm.PCP_Address,
                PCP_Phone = (model.TcmClient.TCMIntakeForm == null) ? "" : model.TcmClient.TCMIntakeForm.PCP_Phone,
                PCP_CityStateZip = (model.TcmClient.TCMIntakeForm == null) ? "" : model.TcmClient.TCMIntakeForm.PCP_CityStateZip                
            };
           
            return salida;

        }

        public async Task<TCMAssessmentHouseCompositionEntity> ToTCMAssessmentHouseCompositionEntity(TCMAssessmentHouseCompositionViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentHouseCompositionEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Age = model.Age,
                Name = model.Name,
                RelationShip = model.RelationShip,
                Supporting = model.Supporting,
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment)

            };
        }

        public TCMAssessmentHouseCompositionViewModel ToTCMAssessmentHouseCompositionViewModel(TCMAssessmentHouseCompositionEntity model)
        {
            TCMAssessmentHouseCompositionViewModel salida;
            salida = new TCMAssessmentHouseCompositionViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Age = model.Age,
                Name = model.Name,
                RelationShip = model.RelationShip,
                Supporting = model.Supporting,
                TcmAssessment = model.TcmAssessment,
                IdTCMAssessment = model.TcmAssessment.Id
            };

            return salida;

        }

        public async Task<TCMAssessmentIndividualAgencyEntity> ToTCMAssessmenIndividualAgencyEntity(TCMAssessmentIndividualAgencyViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentIndividualAgencyEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Name = model.Name,
                RelationShip = model.RelationShip,
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment),
                Agency = model.Agency

            };
        }

        public TCMAssessmentIndividualAgencyViewModel ToTCMAssessmentIndividualAgencyViewModel(TCMAssessmentIndividualAgencyEntity model)
        {
            TCMAssessmentIndividualAgencyViewModel salida;
            salida = new TCMAssessmentIndividualAgencyViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Name = model.Name,
                RelationShip = model.RelationShip,
                TcmAssessment = model.TcmAssessment,
                Agency = model.Agency,
                IdTCMAssessment = model.TcmAssessment.Id
                
            };

            return salida;
        }

        public async Task<TCMAssessmentMedicationEntity> ToTCMAssessmenMedicationEntity(TCMAssessmentMedicationViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentMedicationEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Name = model.Name,
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment),
                Dosage = model.Dosage,
                Frequency = model.Frequency,
                Prescriber = model.Prescriber,
                ReasonPurpose = model.ReasonPurpose
          
            };
        }

        public TCMAssessmentMedicationViewModel ToTCMAssessmentMedicationViewModel(TCMAssessmentMedicationEntity model)
        {
            TCMAssessmentMedicationViewModel salida;
            salida = new TCMAssessmentMedicationViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Name = model.Name,
                TcmAssessment = model.TcmAssessment,
                Dosage = model.Dosage,
                Frequency = model.Frequency,
                Prescriber = model.Prescriber,
                ReasonPurpose = model.ReasonPurpose,
                IdTCMAssessment = model.TcmAssessment.Id
            };

            return salida;
        }

        public async Task<TCMAssessmentPastCurrentServiceEntity> ToTCMAssessmenPastCurrentServiceEntity(TCMAssessmentPastCurrentServiceViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentPastCurrentServiceEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment),
                DateReceived = model.DateReceived,
                Efectiveness = EffectivenessUtils.GetEffectivenessByIndex(model.IdEffectivess),
                ProviderAgency = model.ProviderAgency,
                TypeService = model.TypeService
            };
        }

        public TCMAssessmentPastCurrentServiceViewModel ToTCMAssessmentPastCurrentServiceViewModel(TCMAssessmentPastCurrentServiceEntity model)
        {
            TCMAssessmentPastCurrentServiceViewModel salida;
            salida = new TCMAssessmentPastCurrentServiceViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TcmAssessment = model.TcmAssessment,
                TypeService = model.TypeService,
                ProviderAgency = model.ProviderAgency,
                Efectiveness = model.Efectiveness,
                DateReceived = model.DateReceived,
                IdTCMAssessment = model.TcmAssessment.Id,
                IdEffectivess = (model.Efectiveness == EffectivenessType.Effective) ? 0 : (model.Efectiveness == EffectivenessType.Highly_effective) ? 1 : (model.Efectiveness == EffectivenessType.Somewhat_effective) ? 2 : (model.Efectiveness == EffectivenessType.slightly_effective) ? 3 : (model.Efectiveness == EffectivenessType.Not_at_all_effective) ? 4 : (model.Efectiveness == EffectivenessType.unable_to_evaluate) ? 5 : (model.Efectiveness == EffectivenessType.In_progress) ? 6 : 7,
                EffectivessList = _combosHelper.GetComboEffectiveness()
            };

            return salida;
        }

        public async Task<TCMAssessmentHospitalEntity> ToTCMAssessmentHospitalEntity(TCMAssessmentHospitalViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentHospitalEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment),
                Date = model.Date,
                Name = model.Name,
                Reason = model.Reason
            };
        }

        public TCMAssessmentHospitalViewModel ToTCMAssessmentHospitalViewModel(TCMAssessmentHospitalEntity model)
        {
            TCMAssessmentHospitalViewModel salida;
            salida = new TCMAssessmentHospitalViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TcmAssessment = model.TcmAssessment,
                Date = model.Date,
                Name = model.Name,
                Reason = model.Reason,
                IdTCMAssessment = model.TcmAssessment.Id
            };

            return salida;
        }

        public async Task<TCMAssessmentDrugEntity> ToTCMAssessmentDrugEntity(TCMAssessmentDrugViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentDrugEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment),
                Age = model.Age,
                DateBegin = model.DateBegin,
                Frequency = model.Frequency,
                LastTimeUsed = model.LastTimeUsed,
                SustanceName = DrugsUtils.GetEffectivenessByIndex(model.IdDrugs)
                
            };
        }

        public TCMAssessmentDrugViewModel ToTCMAssessmentDrugViewModel(TCMAssessmentDrugEntity model)
        {
            TCMAssessmentDrugViewModel salida;
            salida = new TCMAssessmentDrugViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TcmAssessment = model.TcmAssessment,
                Age = model.Age,
                DateBegin = model.DateBegin,
                Frequency = model.Frequency,
                LastTimeUsed = model.LastTimeUsed,
                SustanceName = model.SustanceName,
                IdTCMAssessment = model.TcmAssessment.Id,
                DrugsList = _combosHelper.GetComboDrugs(),
                IdDrugs = (model.SustanceName == DrugsType.Alcohol) ? 0 : (model.SustanceName == DrugsType.Amphetamine_Meth) ? 1 : (model.SustanceName == DrugsType.Barbiturates) ? 2 : (model.SustanceName == DrugsType.Benzodiazepines) ? 3 : (model.SustanceName == DrugsType.Caffeine) ? 4 : (model.SustanceName == DrugsType.Cocaina_Crack) ? 5 : (model.SustanceName == DrugsType.Hallucinogens) ? 6 :
                (model.SustanceName == DrugsType.Heroin) ? 7 : (model.SustanceName == DrugsType.Inhalants_Solvents) ? 8 : (model.SustanceName == DrugsType.LSD) ? 9 : (model.SustanceName == DrugsType.Marijuana_Hashish) ? 10 : (model.SustanceName == DrugsType.MDMA_Ecstasy) ? 11 : (model.SustanceName == DrugsType.Nicotne) ? 12 : (model.SustanceName == DrugsType.Opium) ? 13 : 14

            };

            return salida;
        }

        public async Task<TCMAssessmentMedicalProblemEntity> ToTCMAssessmentMedicalProblemEntity(TCMAssessmentMedicalProblemViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentMedicalProblemEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment),
                Client = model.Client,
                Comments = model.Comments,
                Family = model.Family,
                MedicalProblem = model.MedicalProblem
            };
        }

        public TCMAssessmentMedicalProblemViewModel ToTCMAssessmentMedicalProblemViewModel(TCMAssessmentMedicalProblemEntity model)
        {
            TCMAssessmentMedicalProblemViewModel salida;
            salida = new TCMAssessmentMedicalProblemViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TcmAssessment = model.TcmAssessment,
                Client = model.Client,
                Comments = model.Comments,
                Family = model.Family,
                MedicalProblem = model.MedicalProblem,
                IdTCMAssessment = model.TcmAssessment.Id
            };

            return salida;
        }

        public async Task<TCMAssessmentSurgeryEntity> ToTCMAssessmentSurgeryEntity (TCMAssessmentSurgeryViewModel model, bool isNew, string userId)
        {
            return new TCMAssessmentSurgeryEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmAssessment = await _context.TCMAssessment.FirstOrDefaultAsync(c => c.Id == model.IdTCMAssessment),
                Date = model.Date,
                Hospital =model.Hospital,
                Outcome = model.Outcome,
                TypeSurgery = model.TypeSurgery
            };
        }

        public TCMAssessmentSurgeryViewModel ToTCMAssessmentSurgeryViewModel(TCMAssessmentSurgeryEntity model)
        {
            TCMAssessmentSurgeryViewModel salida;
            salida = new TCMAssessmentSurgeryViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TcmAssessment = model.TcmAssessment,
                Date = model.Date,
                Hospital = model.Hospital,
                Outcome = model.Outcome,
                TypeSurgery = model.TypeSurgery,
                IdTCMAssessment = model.TcmAssessment.Id
            };

            return salida;
        }

        public async Task<TCMNoteEntity> ToTCMNoteEntity(TCMNoteViewModel model, bool isNew, string userId)
        {
            return new TCMNoteEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Outcome = model.Outcome,
               // CaseManager = _context.CaseManagers
                 //                     .FirstOrDefault(n => n.Id == model.IdCaseManager),
                TCMNoteActivity = await _context.TCMNoteActivity.Where(n => n.TCMNote.Id == model.IdTCMNote).ToListAsync(),                
                DateOfService = model.DateOfService,                
                NextStep = model.NextStep,
                ServiceCode = model.ServiceCode,
                Status = model.Status,
                TCMClient = _context.TCMClient
                                    .Include(n => n.Client)
                                    .FirstOrDefault(n => n.Id == model.IdTCMClient),
                TCMMessages = _context.TCMMessages
                                      .Where(n => n.TCMNote.Id == model.Id)
                                      .ToList(),
                Sign = model.Sign,
                BillDms = model.BillDms,
                BilledDate = model.BilledDate,
                PaymentDate = model.PaymentDate,
                CodeBill = model.CodeBill
            };
        }

        public TCMNoteViewModel ToTCMNoteViewModel(TCMNoteEntity model)
        {
            TCMNoteViewModel salida;
            salida = new TCMNoteViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Outcome = model.Outcome,
                //CaseManager = model.CaseManager,
                TCMNoteActivity = model.TCMNoteActivity,                
                DateOfService = model.DateOfService,               
                NextStep = model.NextStep,
                ServiceCode = model.ServiceCode,
                Status = model.Status,
                TCMClient = model.TCMClient,
                IdCaseManager = model.TCMClient.Casemanager.Id,
                IdTCMClient = model.TCMClient.Id,
                IdTCMNote = model.Id,
                Sign = model.Sign,
                BillDms = model.BillDms,
                BilledDate = model.BilledDate,
                PaymentDate = model.PaymentDate,
                CodeBill = model.CodeBill

            };

            return salida;
        }

        public async Task<TCMNoteActivityEntity> ToTCMNoteActivityEntity(TCMNoteActivityViewModel model, bool isNew, string userId)
        {
            return new TCMNoteActivityEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DescriptionOfService = model.DescriptionOfService,
                EndTime = model.StartTime.AddMinutes(model.Minutes),
                Minutes = model.Minutes,
                Setting = ServiceTCMNotesUtils.GetCodeByIndex(model.IdSetting),
                StartTime = model.StartTime,
                TCMDomain = model.TCMDomain,
                TCMNote = _context.TCMNote.FirstOrDefault(n => n.Id == model.IdTCMNote),
                ServiceName = model.ServiceName,
                TCMServiceActivity = await _context.TCMServiceActivity.FirstOrDefaultAsync(n => n.Id == model.IdTCMActivity),
                Billable = model.Billable
            };
        }

        public TCMNoteActivityViewModel ToTCMNoteActivityViewModel(TCMNoteActivityEntity model)
        {
            TCMNoteActivityViewModel salida;
            salida = new TCMNoteActivityViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                DescriptionOfService = model.DescriptionOfService,
                EndTime = model.EndTime,
                Minutes = model.Minutes,
                Setting = model.Setting,
                StartTime = model.StartTime,
                TCMDomain = (model.TCMDomain != null)? model.TCMDomain : new TCMDomainEntity(),
                TCMNote = model.TCMNote,
                IdTCMNote = model.TCMNote.Id,
                IdTCMDomain = (model.TCMDomain != null)? model.TCMDomain.Id : 0,
                IdSetting = ServiceTCMNotesUtils.GetIndexByCode(model.Setting),
                SettingList = _combosHelper.GetComboTCMNoteSetting(),
                DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == model.TCMNote.TCMClient.Id).Id, model.TCMNote.DateOfService),
                IdTCMClient = model.TCMNote.TCMClient.Id,
                ActivityList = (model.TCMDomain != null)? _combosHelper.GetComboTCMNoteActivity(model.TCMDomain.Code) : _combosHelper.GetComboTCMNoteActivity(string.Empty),
                DateOfServiceNote = model.TCMNote.DateOfService,
                ServiceName = model.ServiceName,
                IdTCMActivity = (model.TCMServiceActivity != null)? model.TCMServiceActivity.Id : 0,
                DescriptionTemp = (model.TCMServiceActivity != null)? (model.TCMServiceActivity.Id != 0)? _context.TCMServiceActivity.FirstOrDefault(n => n.Id == model.TCMServiceActivity.Id).Description : string.Empty : string.Empty,
                TimeEnd = model.EndTime.ToShortTimeString(),
                NeedIdentified = model.TCMDomain.NeedsIdentified,
                Billable = model.Billable
        };

            return salida;
        }

        public async Task<DocumentsAssistantEntity> ToDocumentsAssistantEntity(DocumentsAssistantViewModel model, string signaturePath, bool isNew)
        {
            return new DocumentsAssistantEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Code = model.Code,
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                Name = model.Name,
                SignaturePath = signaturePath,
                RaterEducation = model.RaterEducation,
                RaterFMHCertification = model.RaterFMHCertification
            };
        }

        public DocumentsAssistantViewModel ToDocumentsAssistantViewModel(DocumentsAssistantEntity model, int idClinic)
        {
            return new DocumentsAssistantViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code,
                IdClinic = model.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                IdUser = _userHelper.GetIdByUserName(model.LinkedUser),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, idClinic),
                SignaturePath = model.SignaturePath,
                RaterEducation = model.RaterEducation,
                RaterFMHCertification = model.RaterFMHCertification
            };
        }

        public async Task<TCMServiceActivityEntity> ToTCMServiceActivityEntity(TCMServiceActivityViewModel model, bool isNew, string userId)
        {
            return new TCMServiceActivityEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmService = await _context.TCMServices.FindAsync(model.IdService),
                Name = model.Name,
                Description = model.Description,
                Unit = model.Unit,
                Status = model.Status,
                Approved = model.Approved,
                Frecuency = model.Frecuency
                
            };
        }

        public TCMServiceActivityViewModel ToTCMServiceActivityViewModel(TCMServiceActivityEntity TcmStageEntity)
        {
            return new TCMServiceActivityViewModel
            {
                Id = TcmStageEntity.Id,
                CreatedBy = TcmStageEntity.CreatedBy,
                CreatedOn = TcmStageEntity.CreatedOn,
                LastModifiedBy = TcmStageEntity.LastModifiedBy,
                LastModifiedOn = TcmStageEntity.LastModifiedOn,
                Name = TcmStageEntity.Name,
                IdService = TcmStageEntity.TcmService.Id,
                IdClinic = TcmStageEntity.TcmService.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                Description = TcmStageEntity.Description,
                Unit = TcmStageEntity.Unit,
                TcmService = TcmStageEntity.TcmService,
                Status = TcmStageEntity.Status,
                Approved = TcmStageEntity.Approved,
                Frecuency = TcmStageEntity.Frecuency
                
            };
        }

        public TCMNoteActivityTempEntity ToTCMNoteActivityTempEntity(TCMNoteActivityViewModel model, bool isNew, string userId)
        {
            return new TCMNoteActivityTempEntity
            {
                Id = isNew ? 0 : model.Id,
                DescriptionOfService = model.DescriptionOfService,
                EndTime = model.EndTime,
                Minutes = model.Minutes,
                IdSetting = model.IdSetting,
                Setting = ServiceTCMNotesUtils.GetCodeByIndex(model.IdSetting),
                IdTCMDomain = model.IdTCMDomain,
                TCMDomainCode = (model.TCMDomain != null)? model.TCMDomain.Code : string.Empty,
                StartTime = model.StartTime,
                UserName = userId,
                IdTCMClient = model.IdTCMClient,
                DateOfServiceOfNote = model.StartTime,
                ServiceName = model.ServiceName,
                IdTCMServiceActivity = model.IdTCMActivity,
                Billable = model.Billable
            };
        }

        public async Task<TCMMessageEntity> ToTCMMessageEntity(TCMMessageViewModel model, bool isNew)
        {

            return new TCMMessageEntity
            {
                Id = isNew ? 0 : model.Id,
                TCMNote = (model.IdTCMNote != 0) ? await _context.TCMNote
                                                                 .Include(wc => wc.TCMClient.Casemanager)
                                                                 .FirstOrDefaultAsync(wc => wc.Id == model.IdTCMNote) : null,
                TCMFarsForm = (model.IdTCMFarsForm != 0) ? await _context.TCMFarsForm
                                                                         .FirstOrDefaultAsync(a => a.Id == model.IdTCMFarsForm) : null,
                TCMServicePlan = (model.IdTCMServiceplan != 0) ? await _context.TCMServicePlans
                                                                               .FirstOrDefaultAsync(a => a.Id == model.IdTCMServiceplan) : null,
                TCMServicePlanReview = (model.IdTCMServiceplanReview != 0) ? await _context.TCMServicePlanReviews
                                                                                           .FirstOrDefaultAsync(a => a.Id == model.IdTCMServiceplanReview) : null,
                TCMAssessment = (model.IdTCMAssessment != 0) ? await _context.TCMAssessment
                                                                             .FirstOrDefaultAsync(a => a.Id == model.IdTCMAssessment) : null,
                TCMAddendum = (model.IdTCMAddendum != 0) ? await _context.TCMAdendums
                                                                         .FirstOrDefaultAsync(a => a.Id == model.IdTCMAddendum) : null,
                TCMDischarge = (model.IdTCMDischarge != 0) ? await _context.TCMDischarge
                                                                           .FirstOrDefaultAsync(d => d.Id == model.IdTCMDischarge) : null,
                Title = model.Title,
                Text = model.Text,
                DateCreated = DateTime.Now,
                Status = MessageStatus.NotRead,
                Notification = model.Notification
            };
        }

        public GoalsTempEntity ToGoalTempEntity(GoalsTempViewModel model, bool isNew)
        {
            return new GoalsTempEntity
            {
                Id = isNew ? 0 : model.Id,
                Number = model.Number,
                Name = model.Name,
                AreaOfFocus = model.AreaOfFocus,
                Service = ServiceUtils.GetServiceByIndex(model.IdService),
                IdClient = model.IdClient,
                UserName = model.UserName,
                numberMonths = model.numberMonths,
                AdmissionDate = model.AdmissionDate,
                TypeDocument = model.TypeDocument
            };
        }

        public GoalsTempViewModel ToGoalTempViewModel(GoalsTempEntity goalEntity)
        {
            GoalsTempViewModel model;
            model = new GoalsTempViewModel
            {
                Id = goalEntity.Id,
                Number = goalEntity.Number,
                Name = goalEntity.Name,
                AreaOfFocus = goalEntity.AreaOfFocus,
                IdService = Convert.ToInt32(goalEntity.Service),
                Services = _combosHelper.GetComboServices(),
                IdClient = goalEntity.IdClient,
                UserName = goalEntity.UserName,
                numberMonths = goalEntity.numberMonths,
                AdmissionDate = goalEntity.AdmissionDate,
                TypeDocument = goalEntity.TypeDocument
            };
           
            return model;
        }

        public async Task<ObjectiveTempEntity> ToObjectiveTempEntity(ObjectiveTempViewModel model, bool isNew)
        {
            return new ObjectiveTempEntity
            {
                Id = isNew ? 0 : model.Id,
                Objetive = model.Objetive,
                DateOpened = model.DateOpened,
                DateTarget = model.DateTarget,
                DateResolved = model.DateResolved,
                Description = model.Description,
                Intervention = model.Intervention,
                GoalTemp = await _context.GoalsTemp.FindAsync(model.IdGoal)
                
            };
        }

        public ObjectiveTempViewModel ToObjectiveTempViewModel(ObjectiveTempEntity objectiveEntity)
        {
            return new ObjectiveTempViewModel
            {
                Id = objectiveEntity.Id,
                Objetive = objectiveEntity.Objetive,
                GoalTemp = objectiveEntity.GoalTemp,
                IdGoal = objectiveEntity.GoalTemp.Id,
                DateOpened = objectiveEntity.DateOpened,
                DateResolved = objectiveEntity.DateResolved,
                DateTarget = objectiveEntity.DateTarget,
                Description = objectiveEntity.Description,
                Intervention = objectiveEntity.Intervention
               
            };
        }

        public async Task<BriefEntity> ToBriefEntity(BriefViewModel model, bool isNew, string userId)
        {
            return new BriefEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient),
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
                BioH0031HN = model.BioH0031HN,
                IDAH0031HO = model.IDAH0031HO,
                ClientAssessmentSituation = model.ClientAssessmentSituation,
                CMH = model.CMH,
                Comments = model.Comments,
                DateBio = model.DateBio,
                DateSignatureLicensedPractitioner = model.DateSignatureLicensedPractitioner,
                DateSignaturePerson = model.DateSignaturePerson,
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                DateSignatureUnlicensedTherapist = model.DateSignatureUnlicensedTherapist,
                DoesClient = model.DoesClient,
                DoYouOwn = model.DoYouOwn,
                DoYouOwn_Explain = model.DoYouOwn_Explain,
                FamilyAssessmentSituation = model.FamilyAssessmentSituation,
                FamilyEmotional = model.FamilyEmotional,
                HasTheClient = model.HasTheClient,
                HasTheClient_Explain = model.HasTheClient_Explain,
                HaveYouEverBeen = model.HaveYouEverBeen,
                HaveYouEverBeen_Explain = model.HaveYouEverBeen_Explain,
                HaveYouEverThought = model.HaveYouEverThought,
                HaveYouEverThought_Explain = model.HaveYouEverThought_Explain,
                IConcurWhitDiagnistic = model.IConcurWhitDiagnistic,
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
                Supervisor = model.Supervisor,
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
                Oriented_FullOriented = model.Oriented_FullOriented,
                PresentingProblem = model.PresentingProblem,
                Priv = model.Priv,
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
                Treatmentrecomendations = model.Treatmentrecomendations,
                DocumentsAssistant = model.DocumentsAssistant,
                Client_FK = model.Client_FK,
                ClientDenied = model.ClientDenied,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                AdmissionedFor = model.AdmissionedFor,
                Messages = _context.Messages
                                   .Where(n => n.Brief.Id == model.Id)
                                   .ToList(),
                Status = model.Status,
                SumanrOfFindings = model.SumanrOfFindings,
                Code90791 = model.Code90791

            };
        }

        public BriefViewModel ToBriefViewModel(BriefEntity model)
        {
            return new BriefViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
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
                BioH0031HN = model.BioH0031HN,
                IDAH0031HO = model.IDAH0031HO,
                ClientAssessmentSituation = model.ClientAssessmentSituation,
                CMH = model.CMH,
                Comments = model.Comments,
                DateBio = model.DateBio,
                DateSignatureLicensedPractitioner = model.DateSignatureLicensedPractitioner,
                DateSignaturePerson = model.DateSignaturePerson,
                DateSignatureSupervisor = model.DateSignatureSupervisor,
                DateSignatureUnlicensedTherapist = model.DateSignatureUnlicensedTherapist,
                DoesClient = model.DoesClient,
                DoYouOwn = model.DoYouOwn,
                DoYouOwn_Explain = model.DoYouOwn_Explain,
                FamilyAssessmentSituation = model.FamilyAssessmentSituation,
                FamilyEmotional = model.FamilyEmotional,
                HasTheClient = model.HasTheClient,
                HasTheClient_Explain = model.HasTheClient_Explain,
                HaveYouEverBeen = model.HaveYouEverBeen,
                HaveYouEverBeen_Explain = model.HaveYouEverBeen_Explain,
                HaveYouEverThought = model.HaveYouEverThought,
                HaveYouEverThought_Explain = model.HaveYouEverThought_Explain,
                IConcurWhitDiagnistic = model.IConcurWhitDiagnistic,
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
                Supervisor = model.Supervisor,
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
                Oriented_FullOriented = model.Oriented_FullOriented,
                PresentingProblem = model.PresentingProblem,
                Priv = model.Priv,
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
                Treatmentrecomendations = model.Treatmentrecomendations,
                DocumentsAssistant = model.DocumentsAssistant,
                Client_FK = model.Client_FK,
                ClientDenied = model.ClientDenied,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                AdmissionedFor = model.AdmissionedFor,
                Status = model.Status,
                SumanrOfFindings = model.SumanrOfFindings,
                Code90791 = model.Code90791

            };

        }

        public async Task<GroupNote2Entity> ToGroupNote2Entity(GroupNote2ViewModel model, bool isNew)
        {
            GroupNote2Entity entity = await _context.GroupNotes2.FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
            return new GroupNote2Entity
            {
                Id = isNew ? 0 : entity.Id,
                Workday_Cient = await _context.Workdays_Clients.FindAsync(model.Id),
                Status = NoteStatus.Edition,
                Other = model.Other,
                Impaired = model.Impaired,
                Euthymic = model.Euthymic,
                Depressed = model.Depressed,
                Anxious = model.Anxious,
                Irritable = model.Irritable,
                Guarded = model.Guarded,
                Withdrawn = model.Withdrawn,
                Hostile = model.Hostile,
                Setting = model.Setting,

                Adequated = model.Adequated,
                Assigned = model.Assigned,
                AssignedTopicOf = model.AssignedTopicOf,
                Congruent = model.Congruent,
                Descompensating = model.Descompensating,
                Developing = model.Developing,
                Dramatic = model.Dramatic,
                Euphoric = model.Euphoric,
                Expressing = model.Expressing,
                Facilitated = model.Facilitated,
                Fair = model.Fair,
                FairAttitude = model.FairAttitude,
                Faulty = model.Faulty,
                Getting = model.Getting,
                GroupLeaderFacilitator = model.GroupLeaderFacilitator,
                GroupLeaderFacilitatorAbout = model.GroupLeaderFacilitatorAbout,
                GroupLeaderProviderPsychoeducation = model.GroupLeaderProviderPsychoeducation,
                GroupLeaderProviderSupport = model.GroupLeaderProviderSupport,
                Inadequated = model.Inadequated,
                InsightAdequate = model.InsightAdequate,
                Involved = model.Involved,
                Kept = model.Kept,
                LearningAbout = model.LearningAbout,
                LearningFrom = model.LearningFrom,
                Limited = model.Limited,
                MildlyImpaired = model.MildlyImpaired,
                MinimalProgress = model.MinimalProgress,
                ModerateProgress = model.ModerateProgress,
                Motivated = model.Motivated,
                Negativistic = model.Negativistic,
                NoProgress = model.NoProgress,
                Normal = model.Normal,
                NotToPerson = model.NotToPerson,
                NotToPlace = model.NotToPlace,
                NotToTime = model.NotToTime,
                Optimistic = model.Optimistic,
                Oriented = model.Oriented,
                OtherExplain = model.OtherExplain,
                Providing = model.Providing,
                Received = model.Received,
                Regression = model.Regression,
                SevereryImpaired = model.SevereryImpaired,
                Sharing = model.Sharing,
                Short = model.Short,
                SignificantProgress = model.SignificantProgress,
                UnableToDetermine = model.UnableToDetermine,
                Unmotivated = model.Unmotivated,
                MTPId = model.MTPId,
                Schema = model.Schema

            };
        }

        public async Task<GroupNote2Entity> ToGroupNote3Entity(GroupNote3ViewModel model, bool isNew)
        {
            GroupNote2Entity entity = await _context.GroupNotes2.FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
            return new GroupNote2Entity
            {
                Id = isNew ? 0 : entity.Id,
                Workday_Cient = await _context.Workdays_Clients.FindAsync(model.Id),
                Status = NoteStatus.Edition,
                Other = model.Other,
                Impaired = model.Impaired,
                Euthymic = model.Euthymic,
                Depressed = model.Depressed,
                Anxious = model.Anxious,
                Irritable = model.Irritable,
                Guarded = model.Guarded,
                Withdrawn = model.Withdrawn,
                Hostile = model.Hostile,
                Setting = model.Setting,

                Adequated = model.Adequated,
                Assigned = model.Assigned,
                AssignedTopicOf = model.AssignedTopicOf,
                Congruent = model.Congruent,
                Descompensating = model.Descompensating,
                Developing = model.Developing,
                Dramatic = model.Dramatic,
                Euphoric = model.Euphoric,
                Expressing = model.Expressing,
                Facilitated = model.Facilitated,
                Fair = model.Fair,
                FairAttitude = model.FairAttitude,
                Faulty = model.Faulty,
                Getting = model.Getting,
                GroupLeaderFacilitator = model.GroupLeaderFacilitator,
                GroupLeaderFacilitatorAbout = model.GroupLeaderFacilitatorAbout,
                GroupLeaderProviderPsychoeducation = model.GroupLeaderProviderPsychoeducation,
                GroupLeaderProviderSupport = model.GroupLeaderProviderSupport,
                Inadequated = model.Inadequated,
                InsightAdequate = model.InsightAdequate,
                Involved = model.Involved,
                Kept = model.Kept,
                LearningAbout = model.LearningAbout,
                LearningFrom = model.LearningFrom,
                Limited = model.Limited,
                MildlyImpaired = model.MildlyImpaired,
                MinimalProgress = model.MinimalProgress,
                ModerateProgress = model.ModerateProgress,
                Motivated = model.Motivated,
                Negativistic = model.Negativistic,
                NoProgress = model.NoProgress,
                Normal = model.Normal,
                NotToPerson = model.NotToPerson,
                NotToPlace = model.NotToPlace,
                NotToTime = model.NotToTime,
                Optimistic = model.Optimistic,
                Oriented = model.Oriented,
                OtherExplain = model.OtherExplain,
                Providing = model.Providing,
                Received = model.Received,
                Regression = model.Regression,
                SevereryImpaired = model.SevereryImpaired,
                Sharing = model.Sharing,
                Short = model.Short,
                SignificantProgress = model.SignificantProgress,
                UnableToDetermine = model.UnableToDetermine,
                Unmotivated = model.Unmotivated,
                MTPId = model.MTPId,
                Schema = model.Schema,
                Workday_Client_FK = model.Workday_Client_FK

            };
        }

        public ScheduleViewModel ToScheduleViewModel(ScheduleEntity model)
        {
            return new ScheduleViewModel
            {
                Id = model.Id,
                InitialTime = model.InitialTime,
                EndTime = model.EndTime,
                Clinic = model.Clinic,
                Description = model.Description,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                //IdSession = (model.Session == SessionType.AM) ? 0 : (model.Session == SessionType.PM) ? 1 : 0,
                Services = _combosHelper.GetComboServices(),
                IdService = Convert.ToInt32(model.Service),
                Sessions = _combosHelper.GetComboSession(),
                IdSession = (model.Session == "AM") ? 0 : 1,
            };
        }

        public ScheduleEntity ToScheduleEntity(ScheduleViewModel model, bool isNew, UserEntity user)
        {
            ScheduleEntity schedule;
            schedule = new ScheduleEntity()
            {
                Id = isNew ? 0 : model.Id,
                InitialTime = model.InitialTime,
                EndTime = model.EndTime,
                Clinic = user.Clinic,
                Service = ServiceUtils.GetServiceByIndex(model.IdService),
               
                Description = model.Description,
                CreatedBy = isNew ? user.UserName : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? user.UserName : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
            if (model.IdSession == 0)
            {
                schedule.Session = "AM";
            }
            if (model.IdSession == 1)
            {
                schedule.Session = "PM";
            }

            return schedule;
        }

        public SubScheduleViewModel ToSubScheduleViewModel(SubScheduleEntity model)
        {
            return new SubScheduleViewModel
            {
                Id = model.Id,
                InitialTime = model.InitialTime,
                EndTime = model.EndTime,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                IdSchedule = model.Schedule.Id
               
            };
        }

        public SubScheduleEntity ToSubScheduleEntity(SubScheduleViewModel model, bool isNew, UserEntity user)
        {
            SubScheduleEntity subSchedule;
            subSchedule = new SubScheduleEntity()
            {
                Id = isNew ? 0 : model.Id,
                InitialTime = model.InitialTime,
                EndTime = model.EndTime,
                CreatedBy = isNew ? user.UserName : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? user.UserName : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Schedule = _context.Schedule.FirstOrDefault(n => n.Id == model.IdSchedule)
            };
            
            return subSchedule;
        }

        public async Task<ManagerEntity> ToManagerEntity(ManagerViewModel model, string signaturePath, bool isNew)
        {
            return new ManagerEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FindAsync(model.IdClinic),
                Name = model.Name,
                Status = StatusUtils.GetStatusByIndex(model.IdStatus),
                LinkedUser = _userHelper.GetUserNameById(model.IdUser),
                SignaturePath = signaturePath
                
            };
        }

        public ManagerViewModel ToManagerViewModel(ManagerEntity managerEntity)
        {
            return new ManagerViewModel
            {
                Id = managerEntity.Id,
                Name = managerEntity.Name,
                IdClinic = managerEntity.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                IdStatus = (managerEntity.Status == StatusType.Open) ? 1 : 2,
                StatusList = _combosHelper.GetComboClientStatus(),
                IdUser = _userHelper.GetIdByUserName(managerEntity.LinkedUser),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager,0),
                SignaturePath = managerEntity.SignaturePath
               
            };
        }

        public ClientAuxiliarViewModel ToClientAUXViewModel(ClientEntity clientEntity, List<MTPReviewEntity> mtpr)
        {
            return new ClientAuxiliarViewModel
            {
                Id = clientEntity.Id,
                Name = clientEntity.Name,
                Code = clientEntity.Code,
                MedicaidID = clientEntity.MedicaidID,
                DateOfBirth = clientEntity.DateOfBirth,
                AdmisionDate = clientEntity.AdmisionDate,
                PlaceOfBirth = clientEntity.PlaceOfBirth,
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
                OtherLanguage = clientEntity.OtherLanguage,
                PhotoPath = clientEntity.PhotoPath,
                SignPath = clientEntity.SignPath,
                //IdReferred = (clientEntity.Client_Referred != null) ? clientEntity.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Id : 0,
                //Referreds = _combosHelper.GetComboReferredsByClinic(userId),                
                IdFacilitatorPSR = clientEntity.IdFacilitatorPSR,
                IdFacilitatorGroup = clientEntity.IdFacilitatorGroup,
                OtherLanguage_Read = clientEntity.OtherLanguage_Read,
                OtherLanguage_Speak = clientEntity.OtherLanguage_Speak,
                OtherLanguage_Understand = clientEntity.OtherLanguage_Understand,
                MedicareId = clientEntity.MedicareId,
                DateOfClose = clientEntity.DateOfClose,
                Documents = clientEntity.Documents,
                OnlyTCM = clientEntity.OnlyTCM,
                Clients_HealthInsurances = clientEntity.Clients_HealthInsurances,
                MTPs = clientEntity.MTPs,
                Bio = clientEntity.Bio,
                MTPRList = mtpr,
                Workdays_Clients = clientEntity.Workdays_Clients,
                Clients_Diagnostics = clientEntity.Clients_Diagnostics,
                Clinic = clientEntity.Clinic,
                FarsFormList = clientEntity.FarsFormList
                
            };
        }

        public TCMIntakeClientSignatureVerificationEntity ToTCMIntakeClientSignatureVerificationEntity(TCMIntakeClientSignatureVerificationViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeClientSignatureVerificationEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardianOrClient = model.DateSignatureLegalGuardianOrClient,
                AdmissionedFor = model.AdmissionedFor                
               
            };
        }

        public TCMIntakeClientSignatureVerificationViewModel ToTCMIntakeClientSignatureVerificationViewModel(TCMIntakeClientSignatureVerificationEntity model)
        {
            return new TCMIntakeClientSignatureVerificationViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardianOrClient = model.DateSignatureLegalGuardianOrClient,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn

            };

        }

        public TCMIntakeClientIdDocumentVerificationEntity ToTCMIntakeClientIdDocumentVerificationEntity(TCMIntakeClientIdDocumentVerificationViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeClientIdDocumentVerificationEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardianOrClient = model.DateSignatureLegalGuardianOrClient,
                AdmissionedFor = model.AdmissionedFor,
                HealthPlan = model.HealthPlan,
                Id_DriverLicense = model.Id_DriverLicense,
                MedicaidId = model.MedicaidId,
                MedicareCard = model.MedicareCard,
                Other_Identification = model.Other_Identification,
                Other_Name = model.Other_Name,
                Passport_Resident = model.Passport_Resident,
                Social = model.Social

            };
        }

        public TCMIntakeClientIdDocumentVerificationViewModel ToTCMIntakeClientIdDocumentVerificationViewModel(TCMIntakeClientIdDocumentVerificationEntity model)
        {
            return new TCMIntakeClientIdDocumentVerificationViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardianOrClient = model.DateSignatureLegalGuardianOrClient,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                HealthPlan = model.HealthPlan,
                Id_DriverLicense = model.Id_DriverLicense,
                MedicaidId = model.MedicaidId,
                MedicareCard = model.MedicareCard,
                Other_Identification = model.Other_Identification,
                Other_Name = model.Other_Name,
                Passport_Resident = model.Passport_Resident,
                Social = model.Social

            };
        }

        public TCMIntakePainScreenEntity ToTCMIntakePainScreenEntity(TCMIntakePainScreenViewModel model, bool isNew, string userId)
        {
            return new TCMIntakePainScreenEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                AlwayasThere = model.AlwayasThere,
                ComesAndGoes = model.ComesAndGoes,
                CurrentPainScore = model.CurrentPainScore,
                DidYouUse = model.DidYouUse,
                DoesYourPainEffect = model.DoesYourPainEffect,
                DoYouBelieve = model.DoYouBelieve,
                DoYouFell = model.DoYouFell,
                DoYouSuffer = model.DoYouSuffer,
                WereYourDrugs = model.WereYourDrugs,
                WhatCauses = model.WhatCauses,
                WhereIs = model.WhereIs,
                DateOfReferral = model.DateOfReferral,
                ReferredTo = model.ReferredTo
                
            };
        }

        public TCMIntakePainScreenViewModel ToTCMIntakePainScreenViewModel(TCMIntakePainScreenEntity model)
        {
            return new TCMIntakePainScreenViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                AlwayasThere = model.AlwayasThere,
                ComesAndGoes = model.ComesAndGoes,
                CurrentPainScore = model.CurrentPainScore,
                DidYouUse = model.DidYouUse,
                DoesYourPainEffect = model.DoesYourPainEffect,
                DoYouBelieve = model.DoYouBelieve,
                DoYouFell = model.DoYouFell,
                DoYouSuffer = model.DoYouSuffer,
                WereYourDrugs = model.WereYourDrugs,
                WhatCauses = model.WhatCauses,
                WhereIs = model.WhereIs,
                DateOfReferral = model.DateOfReferral,
                ReferredTo = model.ReferredTo
            };

        }

        public TCMIntakeColumbiaSuicideEntity ToTCMIntakeColumbiaSuicideEntity(TCMIntakeColumbiaSuicideViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeColumbiaSuicideEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                HaveYouActuallyLifeTime = model.HaveYouActuallyLifeTime,
                HaveYouActuallyLifeTime_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouActuallyLifeTime_Value),
                HaveYouActuallyPastMonth = model.HaveYouActuallyPastMonth,
                HaveYouActuallyPastMonth_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouActuallyPastMonth_Value),
                HaveYouBeenLifeTime = model.HaveYouBeenLifeTime,
                HaveYouBeenLifeTime_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouBeenLifeTime_Value),
                HaveYouBeenPastMonth = model.HaveYouBeenPastMonth,
                HaveYouBeenPastMonth_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouBeenPastMonth_Value),
                HaveYouEver = model.HaveYouEver,
                HaveYouEverIfYes = model.HaveYouEverIfYes,
                HaveYouEverIfYes_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouEverIfYes_Value),
                HaveYouEver_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouEver_Value),
                HaveYouHadLifeTime = model.HaveYouHadLifeTime,
                HaveYouHadLifeTime_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouHadLifeTime_Value),
                HaveYouHadPastMonth = model.HaveYouHadPastMonth,
                HaveYouHadPastMonth_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouHadPastMonth_Value),
                HaveYouStartedLifeTime = model.HaveYouStartedLifeTime,
                HaveYouStartedLifeTime_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouStartedLifeTime_Value),
                HaveYouStartedPastMonth = model.HaveYouStartedPastMonth,
                HaveYouStartedPastMonth_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouStartedPastMonth_Value),
                HaveYouWishedLifeTime = model.HaveYouWishedLifeTime,
                HaveYouWishedLifeTime_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouWishedLifeTime_Value),
                HaveYouWishedPastMonth = model.HaveYouWishedPastMonth,
                HaveYouWishedPastMonth_Value = RiskUtils.GetRiskByIndex(model.IdHaveYouWishedPastMonth_Value)

            };
        }

        public TCMIntakeColumbiaSuicideViewModel ToTCMIntakeColumbiaSuicideViewModel(TCMIntakeColumbiaSuicideEntity model)
        {
            return new TCMIntakeColumbiaSuicideViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                HaveYouActuallyLifeTime = model.HaveYouActuallyLifeTime,
                HaveYouActuallyLifeTime_Value = model.HaveYouActuallyLifeTime_Value,
                HaveYouActuallyPastMonth = model.HaveYouActuallyPastMonth,
                HaveYouActuallyPastMonth_Value = model.HaveYouActuallyPastMonth_Value,
                HaveYouBeenLifeTime = model.HaveYouBeenLifeTime,
                HaveYouBeenLifeTime_Value = model.HaveYouBeenLifeTime_Value,
                HaveYouBeenPastMonth = model.HaveYouBeenPastMonth,
                HaveYouBeenPastMonth_Value = model.HaveYouBeenPastMonth_Value,
                HaveYouEver = model.HaveYouEver,
                HaveYouEverIfYes = model.HaveYouEverIfYes,
                HaveYouEverIfYes_Value = model.HaveYouEverIfYes_Value,
                HaveYouEver_Value = model.HaveYouEver_Value,
                HaveYouHadLifeTime = model.HaveYouHadLifeTime,
                HaveYouHadLifeTime_Value = model.HaveYouHadLifeTime_Value,
                HaveYouHadPastMonth = model.HaveYouHadPastMonth,
                HaveYouHadPastMonth_Value = model.HaveYouHadPastMonth_Value,
                HaveYouStartedLifeTime = model.HaveYouStartedLifeTime,
                HaveYouStartedLifeTime_Value = model.HaveYouStartedLifeTime_Value,
                HaveYouStartedPastMonth = model.HaveYouStartedPastMonth,
                HaveYouStartedPastMonth_Value = model.HaveYouStartedPastMonth_Value,
                HaveYouWishedLifeTime = model.HaveYouWishedLifeTime,
                HaveYouWishedLifeTime_Value = model.HaveYouWishedLifeTime_Value,
                HaveYouWishedPastMonth = model.HaveYouWishedPastMonth,
                HaveYouWishedPastMonth_Value = model.HaveYouWishedPastMonth_Value,

                IdHaveYouWishedPastMonth_Value = (model.HaveYouWishedPastMonth_Value == RiskType.Low) ? 0 : (model.HaveYouWishedPastMonth_Value == RiskType.Moderate) ? 1 : (model.HaveYouWishedPastMonth_Value == RiskType.High) ? 2 : 0,
                IdHaveYouWishedLifeTime_Value = (model.HaveYouWishedLifeTime_Value == RiskType.Low) ? 0 : (model.HaveYouWishedLifeTime_Value == RiskType.Moderate) ? 1 : (model.HaveYouWishedLifeTime_Value == RiskType.High) ? 2 : 0,

                IdHaveYouActuallyPastMonth_Value = (model.HaveYouActuallyPastMonth_Value == RiskType.Low) ? 0 : (model.HaveYouActuallyPastMonth_Value == RiskType.Moderate) ? 1 : (model.HaveYouActuallyPastMonth_Value == RiskType.High) ? 2 : 0,
                IdHaveYouActuallyLifeTime_Value = (model.HaveYouActuallyLifeTime_Value == RiskType.Low) ? 0 : (model.HaveYouActuallyLifeTime_Value == RiskType.Moderate) ? 1 : (model.HaveYouActuallyLifeTime_Value == RiskType.High) ? 2 : 0,

                IdHaveYouBeenPastMonth_Value = (model.HaveYouBeenPastMonth_Value == RiskType.Low) ? 0 : (model.HaveYouBeenPastMonth_Value == RiskType.Moderate) ? 1 : (model.HaveYouBeenPastMonth_Value == RiskType.High) ? 2 : 0,
                IdHaveYouBeenLifeTime_Value = (model.HaveYouBeenLifeTime_Value == RiskType.Low) ? 0 : (model.HaveYouBeenLifeTime_Value == RiskType.Moderate) ? 1 : (model.HaveYouBeenLifeTime_Value == RiskType.High) ? 2 : 0,

                IdHaveYouHadPastMonth_Value = (model.HaveYouHadPastMonth_Value == RiskType.Low) ? 0 : (model.HaveYouHadPastMonth_Value == RiskType.Moderate) ? 1 : (model.HaveYouHadPastMonth_Value == RiskType.High) ? 2 : 0,
                IdHaveYouHadLifeTime_Value = (model.HaveYouHadLifeTime_Value == RiskType.Low) ? 0 : (model.HaveYouHadLifeTime_Value == RiskType.Moderate) ? 1 : (model.HaveYouHadLifeTime_Value == RiskType.High) ? 2 : 0,

                IdHaveYouStartedPastMonth_Value = (model.HaveYouStartedPastMonth_Value == RiskType.Low) ? 0 : (model.HaveYouStartedPastMonth_Value == RiskType.Moderate) ? 1 : (model.HaveYouStartedPastMonth_Value == RiskType.High) ? 2 : 0,
                IdHaveYouStartedLifeTime_Value = (model.HaveYouStartedLifeTime_Value == RiskType.Low) ? 0 : (model.HaveYouStartedLifeTime_Value == RiskType.Moderate) ? 1 : (model.HaveYouStartedLifeTime_Value == RiskType.High) ? 2 : 0,

                IdHaveYouEver_Value = (model.HaveYouEver_Value == RiskType.Low) ? 0 : (model.HaveYouEver_Value == RiskType.Moderate) ? 1 : (model.HaveYouEver_Value == RiskType.High) ? 2 : 0,
                IdHaveYouEverIfYes_Value = (model.HaveYouEverIfYes_Value == RiskType.Low) ? 0 : (model.HaveYouEverIfYes_Value == RiskType.Moderate) ? 1 : (model.HaveYouEverIfYes_Value == RiskType.High) ? 2 : 0,

                RiskList = _combosHelper.GetComboRisk(),

            };

        }

        public TCMIntakePersonalWellbeingEntity ToTCMIntakePersonalWellbeingEntity(TCMIntakePersonalWellbeingViewModel model, bool isNew, string userId)
        {
            return new TCMIntakePersonalWellbeingEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                Living = model.Living,
                Community = model.Community,
                Feel = model.Feel,
                Health = model.Health,
                Life = model.Life,
                Relationships = model.Relationships,
                Religion = model.Religion,
                Security = model.Security
                
            };
        }

        public TCMIntakePersonalWellbeingViewModel ToTCMIntakePersonalWellbeingViewModel(TCMIntakePersonalWellbeingEntity model)
        {
            return new TCMIntakePersonalWellbeingViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Living = model.Living,
                Community = model.Community,
                Feel = model.Feel,
                Health = model.Health,
                Life = model.Life,
                Relationships = model.Relationships,
                Religion = model.Religion,
                Security = model.Security

            };

        }

        public TCMIntakeNutritionalScreenEntity ToTCMIntakeNutritionalScreenEntity(TCMIntakeNutritionalScreenViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeNutritionalScreenEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = model.TcmClient,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                ClientAlwaysHungry = model.ClientAlwaysHungry,
                ClientAlwaysHungry_Value = model.ClientAlwaysHungry_Value,
                ClientAlwaysThirsty = model.ClientAlwaysThirsty,
                ClientAlwaysThirsty_Value = model.ClientAlwaysThirsty_Value,
                ClientAppetiteFair = model.ClientAppetiteFair,
                ClientAppetiteFair_Value = model.ClientAppetiteFair_Value,
                ClientAppetiteGood = model.ClientAppetiteGood,
                ClientAppetiteGood_Value = model.ClientAppetiteGood_Value,
                ClientAppetitepoor = model.ClientAppetitepoor,
                ClientAppetitepoor_Value = model.ClientAppetitepoor_Value,
                ClientBinges = model.ClientBinges,
                ClientBinges_Value = model.ClientBinges_Value,
                ClientDiarrhea = model.ClientDiarrhea,
                ClientDiarrhea_Value = model.ClientDiarrhea_Value,
                ClientEatsAlone = model.ClientEatsAlone,
                ClientEatsAlone_Value = model.ClientEatsAlone_Value,
                ClientEatsFew = model.ClientEatsFew,
                ClientEatsFewer = model.ClientEatsFewer,
                ClientEatsFewer_Value = model.ClientEatsFewer_Value,
                ClientEatsFew_Value = model.ClientEatsFew_Value,
                ClientFoodAllergies = model.ClientFoodAllergies,
                ClientFoodAllergies_Value = model.ClientFoodAllergies_Value,
                ClientHasHistory = model.ClientHasHistory,
                ClientHasHistory_Value = model.ClientHasHistory_Value,
                ClientHasIllnes = model.ClientHasIllnes,
                ClientHasIllnes_Value = model.ClientHasIllnes_Value,
                ClientHasTooth = model.ClientHasTooth,
                ClientHasTooth_Value = model.ClientHasTooth_Value,
                ClientTakes = model.ClientTakes,
                ClientTakes_Value = model.ClientTakes_Value,
                ClientVomits = model.ClientVomits,
                ClientVomits_Value = model.ClientVomits_Value,
                DateOfReferral = model.DateOfReferral,
                ReferredTo = model.ReferredTo,
                WithoutWanting = model.WithoutWanting,
                WithoutWanting_Value = model.WithoutWanting_Value

            };
        }

        public TCMIntakeNutritionalScreenViewModel ToTCMIntakeNutritionalScreenViewModel(TCMIntakeNutritionalScreenEntity model)
        {
            return new TCMIntakeNutritionalScreenViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient.Id,
                TcmClient_FK = model.TcmClient_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                ClientAlwaysHungry = model.ClientAlwaysHungry,
                ClientAlwaysHungry_Value = model.ClientAlwaysHungry_Value,
                ClientAlwaysThirsty = model.ClientAlwaysThirsty,
                ClientAlwaysThirsty_Value = model.ClientAlwaysThirsty_Value,
                ClientAppetiteFair = model.ClientAppetiteFair,
                ClientAppetiteFair_Value = model.ClientAppetiteFair_Value,
                ClientAppetiteGood = model.ClientAppetiteGood,
                ClientAppetiteGood_Value = model.ClientAppetiteGood_Value,
                ClientAppetitepoor = model.ClientAppetitepoor,
                ClientAppetitepoor_Value = model.ClientAppetitepoor_Value,
                ClientBinges = model.ClientBinges,
                ClientBinges_Value = model.ClientBinges_Value,
                ClientDiarrhea = model.ClientDiarrhea,
                ClientDiarrhea_Value = model.ClientDiarrhea_Value,
                ClientEatsAlone = model.ClientEatsAlone,
                ClientEatsAlone_Value = model.ClientEatsAlone_Value,
                ClientEatsFew = model.ClientEatsFew,
                ClientEatsFewer = model.ClientEatsFewer,
                ClientEatsFewer_Value = model.ClientEatsFewer_Value,
                ClientEatsFew_Value = model.ClientEatsFew_Value,
                ClientFoodAllergies = model.ClientFoodAllergies,
                ClientFoodAllergies_Value = model.ClientFoodAllergies_Value,
                ClientHasHistory = model.ClientHasHistory,
                ClientHasHistory_Value = model.ClientHasHistory_Value,
                ClientHasIllnes = model.ClientHasIllnes,
                ClientHasIllnes_Value = model.ClientHasIllnes_Value,
                ClientHasTooth = model.ClientHasTooth,
                ClientHasTooth_Value = model.ClientHasTooth_Value,
                ClientTakes = model.ClientTakes,
                ClientTakes_Value = model.ClientTakes_Value,
                ClientVomits = model.ClientVomits,
                ClientVomits_Value = model.ClientVomits_Value,
                DateOfReferral = model.DateOfReferral,
                ReferredTo = model.ReferredTo,
                WithoutWanting = model.WithoutWanting,
                WithoutWanting_Value = model.WithoutWanting_Value

            };

        }

        public async Task<TCMDateBlockedEntity> ToTCMDateBlockedEntity(TCMDateBlockedViewModel model, bool isNew, string userId)
        {
            return new TCMDateBlockedEntity
            {
                Id = isNew ? 0 : model.Id,
                Clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == model.IdClinic),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DateBlocked = model.DateBlocked,
                Description = model.Description
            };
        }

        public TCMDateBlockedViewModel ToTCMDateBlockedViewModel(TCMDateBlockedEntity model)
        {
            return new TCMDateBlockedViewModel
            {
                Id = model.Id,
                IdClinic = model.Clinic.Id,
                Clinics = _combosHelper.GetComboClinics(),
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                DateBlocked = model.DateBlocked,
                Description = model.Description
            };
        }

        public async Task<CiteEntity> ToCiteEntity(CiteViewModel model,bool isNew, string userId)
        {
            return new CiteEntity
            {
                Id = isNew ? 0 : model.Id,               
                Status = CiteUtils.GetCiteByIndex(model.IdStatus),
                Client = await _context.Clients.FindAsync(model.IdClient),
                DateCite = model.DateCite,
                Copay = model.Copay,
                EventNote = model.EventNote,
                PatientNote = model.PatientNote,
                Facilitator = await _context.Facilitators.FindAsync(model.IdFacilitator),
                SubSchedule = await _context.SubSchedule.FindAsync(model.IdSubSchedule),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null)
            };
        }

        public CiteViewModel ToCiteViewModel(CiteEntity model, int idClinic)
        {
            return new CiteViewModel
            {
                Id = model.Id,                
                Worday_CLient = model.Worday_CLient,
                PatientNote = model.PatientNote,
                Copay = model.Copay,
                DateCite = model.DateCite,                
                Client = model.Client,
                EventNote = model.EventNote,
                Facilitator = model.Facilitator,
                IdClient = model.Client.Id,
                IdFacilitator = model.Facilitator.Id,
                Service = "Therapy Private",
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                IdSubSchedule = model.SubSchedule.Id,
                SubSchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(idClinic, ServiceType.Individual, model.Facilitator.Id, model.DateCite),
                SubSchedule = model.SubSchedule,
                IdStatus = (model.Status == CiteStatus.S) ? 1 : (model.Status == CiteStatus.C) ? 2 : (model.Status == CiteStatus.R) ? 3 : (model.Status == CiteStatus.NS) ? 4 : (model.Status == CiteStatus.AR) ? 5 : (model.Status == CiteStatus.CO) ? 6 : (model.Status == CiteStatus.A) ? 7 : (model.Status == CiteStatus.X) ? 8 : 0,
                Status = model.Status,
                StatusList = _combosHelper.GetComboSiteStatus()               
            };
        }

        public BillDmsEntity ToBillDMSEntity(BillDmsViewModel model, bool isNew)
        {
            return new BillDmsEntity
            {
               Id = isNew ? 0 : model.Id,
               Amount = model.Amount,
               BillDmsDetails = model.BillDmsDetails,
               DateBill = model.DateBill,
               DateBillClose = model.DateBillClose,
               DateBillPayment = model.DateBillPayment,
               Different = model.Different,
               StatusBill = StatusBillUtils.GetStatusBillByIndex(model.IdStatus),
               TCMNotes = model.TCMNotes,
               Units = model.Units,
               Workday_Clients = model.Workday_Clients,
               FinishEdition = model.FinishEdition,
               UnitsMH = model.UnitsMH,
               UnitsTCM = model.UnitsTCM
            };
        }

        public BillDmsViewModel ToBillDMSViewModel(BillDmsEntity model)
        {
            return new BillDmsViewModel
            {
                Id = model.Id,
                Amount = model.Amount,
                BillDmsDetails = model.BillDmsDetails,
                DateBill = model.DateBill,
                DateBillClose = model.DateBillClose,
                DateBillPayment = model.DateBillPayment,
                Different = model.Different,
                StatusBill = model.StatusBill,
                TCMNotes = model.TCMNotes,
                Units = model.Units,
                Workday_Clients = model.Workday_Clients,
                IdStatus = (model.StatusBill == StatusBill.Unbilled) ? 1 : (model.StatusBill == StatusBill.Billed) ? 2 : (model.StatusBill == StatusBill.Pending) ? 3 : (model.StatusBill == StatusBill.Paid) ? 4 : 0,
                StatusList = _combosHelper.GetComboBillStatus(),
                AmountCMHNotes = model.BillDmsDetails.Where(n => n.ServiceAgency == ServiceAgency.CMH).Count(),
                AmountTCMNotes = model.BillDmsDetails.Where(n => n.ServiceAgency == ServiceAgency.TCM).Count(),
                FinishEdition = model.FinishEdition,
                UnitsMH = model.UnitsMH,
                UnitsTCM = model.UnitsTCM
            };
        }

        public BillDmsPaidEntity ToBillDMSPaidEntity(BillDmsPaidViewModel model, bool isNew)
        {
            return new BillDmsPaidEntity
            {
                Id = isNew ? 0 : model.Id,
                Amount = model.Amount,
                OrigePaid = PaidOrigiUtils.GetPaidOrigiByIndex(model.IdOrigi),
                DatePaid = model.DatePaid,
                Bill = _context.BillDms.FirstOrDefault(n => n.Id == model.IdBillDms)
            };
        }

        public BillDmsPaidViewModel ToBillDMSPaidModel(BillDmsPaidEntity model)
        {
            return new BillDmsPaidViewModel
            {
                Id = model.Id,
                Amount = model.Amount,
                IdOrigi = (model.OrigePaid == PaidOrigi.Income) ? 1 : (model.OrigePaid == PaidOrigi.Difference) ? 2 : 0,
                OrigiList = _combosHelper.GetComboBillPaid(),
                DatePaid = model.DatePaid,
                Bill = _context.BillDms.FirstOrDefault(n => n.Id == model.Bill.Id),
                IdBillDms = model.Bill.Id
            };
        }

        public async Task<TCMReferralFormEntity> ToTCMReferralFormEntity(TCMReferralFormViewModel model, bool isNew, string userId)
        {
            TCMReferralFormEntity salida;
            salida = new TCMReferralFormEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),

                TcmClient = await _context.TCMClient
                                          .FirstOrDefaultAsync(n => n.Id == model.IdTCMClient),
               Address = model.Address,
               AssignedTo = model.AssignedTo,
               AuthorizedDate = model.AuthorizedDate,
               DateOfBirth = model.DateOfBirth,
               CaseAccepted = model.CaseAccepted,
               CaseNumber = model.CaseNumber,
               Comments = model.Comments,
               DateAssigned = model.DateAssigned,
               Dx = model.Dx,
               Dx_Description = model.Dx_Description,
               ExperatedDate = model.ExperatedDate,
               Gender = model.Gender,
               HMO = model.HMO,
               LegalGuardianName = model.LegalGuardianName,
               LegalGuardianPhone = model.LegalGuardianPhone,
               MedicaidId = model.MedicaidId,
               NameClient = model.NameClient,
               NameSupervisor = model.NameSupervisor,
               PrimaryPhone = model.PrimaryPhone,
               Program = model.Program,
               ReferredBy_Name = model.ReferredBy_Name,
               ReferredBy_Phone = model.ReferredBy_Phone,
               ReferredBy_Title = model.ReferredBy_Title,
               SecondaryPhone = model.SecondaryPhone,
               SSN = model.SSN,
               TCMSign = model.TCMSign,
               TCMSupervisorSign = model.TCMSupervisorSign,
               UnitsApproved = model.UnitsApproved

            };

            return salida;
        }

        public TCMReferralFormViewModel ToTCMReferralFormViewModel(TCMReferralFormEntity model)
        {
            return new TCMReferralFormViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                IdTCMClient = model.TcmClient.Id,
                TcmClient = model.TcmClient,

                Address = model.Address,
                AssignedTo = model.AssignedTo,
                AuthorizedDate = model.AuthorizedDate,
                DateOfBirth = model.DateOfBirth,
                CaseAccepted = model.CaseAccepted,
                CaseNumber = model.CaseNumber,
                Comments = model.Comments,
                DateAssigned = model.DateAssigned,
                Dx = model.Dx,
                Dx_Description = model.Dx_Description,
                ExperatedDate = model.ExperatedDate,
                Gender = model.Gender,
                HMO = model.HMO,
                LegalGuardianName = model.LegalGuardianName,
                LegalGuardianPhone = model.LegalGuardianPhone,
                MedicaidId = model.MedicaidId,
                NameClient = model.NameClient,
                NameSupervisor = model.NameSupervisor,
                PrimaryPhone = model.PrimaryPhone,
                Program = model.Program,
                ReferredBy_Name = model.ReferredBy_Name,
                ReferredBy_Phone = model.ReferredBy_Phone,
                ReferredBy_Title = model.ReferredBy_Title,
                SecondaryPhone = model.SecondaryPhone,
                SSN = model.SSN,
                TCMSign = model.TCMSign,
                TCMSupervisorSign = model.TCMSupervisorSign,
                UnitsApproved = model.UnitsApproved
                
            };
        }

        public async Task<TCMSupervisionTimeEntity> ToTCMSupervisionTimeEntity(TCMSupervisionTimeViewModel model, bool isNew, string userId)
        {
            return new TCMSupervisionTimeEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                CaseManager = await _context.CaseManagers.FindAsync(model.IdCaseManager),
                Description = model.Description,
                DateSupervision = model.DateSupervision,
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                TCMSupervisor = await _context.TCMSupervisors.FindAsync(model.IdTCMSupervisor),
                Present = model.Present
            };
        }

        public TCMSupervisionTimeViewModel ToTCMSupervisionTimeViewModel(TCMSupervisionTimeEntity model, int idClinic)
        {
            return new TCMSupervisionTimeViewModel
            {
                Id = model.Id,
                Description = model.Description,
                CreatedOn = model.CreatedOn,
                CreatedBy = model.CreatedBy,
                IdCaseManager = model.CaseManager.Id,
                CaseManager = model.CaseManager,
                DateSupervision = model.DateSupervision,
                StartTime = model.StartTime,
                EndTime= model.EndTime,
                IdTCMSupervisor = model.TCMSupervisor.Id,
                TCMSupervisor = model.TCMSupervisor,
                Present = model.Present

            };
        }

        public async Task<TCMSubServiceEntity> ToTCMSubServiceEntity(TCMSubServiceViewModel model, bool isNew, string userId)
        {
            return new TCMSubServiceEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmService = await _context.TCMServices.FindAsync(model.Id_TCMService),
                Name = model.Name,
                Description = model.Description,
                Active = model.Active,
                Frecuency = model.Frecuency,
                Units = model.Units
                
            };
        }

        public TCMSubServiceViewModel ToTCMSubServiceViewModel(TCMSubServiceEntity TcmStageEntity)
        {
            return new TCMSubServiceViewModel
            {
                Id = TcmStageEntity.Id,
                Name = TcmStageEntity.Name,
                Id_TCMService = TcmStageEntity.TcmService.Id,
                Description = TcmStageEntity.Description,
                TcmService = TcmStageEntity.TcmService,
                CreatedBy = TcmStageEntity.CreatedBy,
                CreatedOn = TcmStageEntity.CreatedOn,
                Active = TcmStageEntity.Active,
                Frecuency = TcmStageEntity.Frecuency,
                Units = TcmStageEntity.Units

            };
        }

        public TCMTransferEntity ToTCMTransferEntity(TCMTransferViewModel model, bool isNew, string userId)
        {
            return new TCMTransferEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Address = model.Address,
                ChangeInformation = model.ChangeInformation,
                CityStateZip = model.CityStateZip,
                DateAudit = model.DateAudit,
                DateAuditSign = model.DateAuditSign,
                DateLastService = model.DateLastService,
                DateServicePlanORLastSPR = model.DateServicePlanORLastSPR,
                EndTransferDate = model.EndTransferDate,
                HasClientChart = model.HasClientChart,
                LegalGuardianName = model.LegalGuardianName,
                LegalGuardianPhone = model.LegalGuardianPhone,
                OpeningDate = model.OpeningDate,
                OpeningDateAssignedTo = model.OpeningDateAssignedTo,
                OtherPhone = model.OtherPhone,
                PrimaryPhone = model.PrimaryPhone,
                Return = model.Return,
                TCMAssignedFrom = _context.CaseManagers.FirstOrDefault(n => n.Id == model.IdCaseManagerFrom),
                TCMAssignedFromAccept = model.TCMAssignedFromAccept,
                TCMAssignedTo = _context.CaseManagers.FirstOrDefault(n => n.Id == model.IdCaseManagerTo),
                TCMAssignedToAccept = model.TCMAssignedToAccept,
                TCMClient = _context.TCMClient.FirstOrDefault(n => n.Id == model.IdTCMClient),
                TCMSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.Id == model.IdTCMSupervisor),
                TCMSupervisorAccept = model.TCMSupervisorAccept,
                TransferFollow = model.TransferFollow              

            };
        }

        public TCMTransferViewModel ToTCMTransferViewModel(TCMTransferEntity model)
        {
            return new TCMTransferViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                Address = model.Address,
                ChangeInformation = model.ChangeInformation,
                CityStateZip = model.CityStateZip,
                DateAudit = model.DateAudit,
                DateAuditSign = model.DateAuditSign,
                DateLastService = model.DateLastService,
                DateServicePlanORLastSPR = model.DateServicePlanORLastSPR,
                EndTransferDate = model.EndTransferDate,
                HasClientChart = model.HasClientChart,
                LegalGuardianName = model.LegalGuardianName,
                LegalGuardianPhone = model.LegalGuardianPhone,
                OpeningDate = model.OpeningDate,
                OpeningDateAssignedTo = model.OpeningDateAssignedTo,
                OtherPhone = model.OtherPhone,
                PrimaryPhone = model.PrimaryPhone,
                Return = model.Return,
                TCMAssignedFrom = model.TCMAssignedFrom,
                TCMAssignedFromAccept = model.TCMAssignedFromAccept,
                TCMAssignedTo = model.TCMAssignedTo,
                TCMAssignedToAccept = model.TCMAssignedToAccept,
                TCMClient = model.TCMClient,
                TCMSupervisor = model.TCMSupervisor,
                TCMSupervisorAccept = model.TCMSupervisorAccept,
                TransferFollow = model.TransferFollow,
                IdCaseManagerFrom = model.TCMAssignedFrom.Id,
                IdCaseManagerTo = model.TCMAssignedTo.Id,
                IdTCMClient = model.TCMClient.Id,
                IdTCMSupervisor = model.TCMSupervisor.Id,
                TCMsTo = _combosHelper.GetComboCaseManagersByTCMSupervisor(model.TCMSupervisor.LinkedUser, 1)


            };
        }

        public IntakeConsentForTelehealthEntity ToIntakeConsentForTelehealthEntity(IntakeConsentForTelehealthViewModel model, bool isNew)
        {
            return new IntakeConsentForTelehealthEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                AdmissionedFor = model.AdmissionedFor,
                IConsentToReceive = model.IConsentToReceive,
                Documents = model.Documents
            };
        }

        public IntakeConsentForTelehealthViewModel ToIntakeConsentForTelehealthViewModel(IntakeConsentForTelehealthEntity model)
        {
            return new IntakeConsentForTelehealthViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                AdmissionedFor = model.AdmissionedFor,
                IConsentToReceive = model.IConsentToReceive,
                Documents = model.Documents

            };

        }

        public IntakeNoDuplicateServiceEntity ToIntakeNoDuplicateServiceEntity(IntakeNoDuplicateServiceViewModel model, bool isNew)
        {
            return new IntakeNoDuplicateServiceEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                AdmissionedFor = model.AdmissionedFor,
                Documents = model.Documents
            };
        }

        public IntakeNoDuplicateServiceViewModel ToIntakeNoDuplicateServiceViewModel(IntakeNoDuplicateServiceEntity model)
        {
            return new IntakeNoDuplicateServiceViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                AdmissionedFor = model.AdmissionedFor,
                Documents = model.Documents

            };

        }
        public IntakeAdvancedDirectiveEntity ToIntakeAdvancedDirectiveEntity(IntakeAdvancedDirectiveViewModel model, bool isNew, string userId)
        {
            return new IntakeAdvancedDirectiveEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor,
                IHave = model.IHave,
                IHaveNot = model.IHaveNot

            };
        }

        public IntakeAdvancedDirectiveViewModel ToIntakeAdvancedDirectiveViewModel(IntakeAdvancedDirectiveEntity model)
        {
            return new IntakeAdvancedDirectiveViewModel
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
                IHave = model.IHave,
                IHaveNot = model.IHaveNot,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };

        }

        public ReferralFormEntity ToReferralEntity(ReferralFormViewModel model, bool isNew, string userId)
        {
            FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.Id == model.IdFacilitator);
            SupervisorEntity Supervisor = _context.Supervisors.FirstOrDefault(n => n.Id == model.IdSupervisor);
            return new ReferralFormEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),

                Client = _context.Clients
                                 .FirstOrDefault(n => n.Id == model.IdClient),
                Address = model.Address,
                AssignedTo = facilitator.Name,
                AuthorizedDate = model.AuthorizedDate,
                DateOfBirth = model.DateOfBirth,
                CaseAcceptedFacilitator = model.CaseAcceptedFacilitator,
                CaseAcceptedSupervisor = model.CaseAcceptedSupervisor,
                CaseNumber = model.CaseNumber,
                Comments = model.Comments,
                DateAssigned = model.DateAssigned,
                Dx = model.Dx,
                Dx_Description = model.Dx_Description,
                ExperatedDate = model.ExperatedDate,
                Gender = model.Gender,
                HMO = model.HMO,
                LegalGuardianName = model.LegalGuardianName,
                LegalGuardianPhone = model.LegalGuardianPhone,
                MedicaidId = model.MedicaidId,
                NameClient = model.NameClient,
                NameSupervisor = Supervisor.Name,
                PrimaryPhone = model.PrimaryPhone,
                Program = model.Program,
                ReferredBy_Name = model.ReferredBy_Name,
                ReferredBy_Phone = model.ReferredBy_Phone,
                ReferredBy_Title = model.ReferredBy_Title,
                SecondaryPhone = model.SecondaryPhone,
                SSN = model.SSN,
                FacilitatorSign = model.FacilitatorSign,
                SupervisorSign = model.SupervisorSign,
                UnitsApproved = model.UnitsApproved,
                Facilitator = facilitator,
                Supervisor = Supervisor,
                AssignedBy = model.AssignedBy

            };
        }

        public ReferralFormViewModel ToReferralViewModel(ReferralFormEntity model)
        {
            return new ReferralFormViewModel
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                IdClient = model.Client.Id,
                Client = model.Client,

                Address = model.Address,
                AssignedTo = model.AssignedTo,
                AuthorizedDate = model.AuthorizedDate,
                DateOfBirth = model.DateOfBirth,
                CaseAcceptedFacilitator = model.CaseAcceptedFacilitator,
                CaseAcceptedSupervisor = model.CaseAcceptedSupervisor,
                CaseNumber = model.CaseNumber,
                Comments = model.Comments,
                DateAssigned = model.DateAssigned,
                Dx = model.Dx,
                Dx_Description = model.Dx_Description,
                ExperatedDate = model.ExperatedDate,
                Gender = model.Gender,
                HMO = model.HMO,
                LegalGuardianName = model.LegalGuardianName,
                LegalGuardianPhone = model.LegalGuardianPhone,
                MedicaidId = model.MedicaidId,
                NameClient = model.NameClient,
                NameSupervisor = model.NameSupervisor,
                PrimaryPhone = model.PrimaryPhone,
                Program = model.Program,
                ReferredBy_Name = model.ReferredBy_Name,
                ReferredBy_Phone = model.ReferredBy_Phone,
                ReferredBy_Title = model.ReferredBy_Title,
                SecondaryPhone = model.SecondaryPhone,
                SSN = model.SSN,
                FacilitatorSign = model.FacilitatorSign,
                SupervisorSign = model.SupervisorSign,
                UnitsApproved = model.UnitsApproved,
                AssignedBy = model.AssignedBy
            };
        }

        public async Task<TCMIntakeAppendixIEntity> ToTCMIntakeAppendixIEntity(TCMIntakeAppendixIViewModel model, bool isNew, string userId)
        {
            TCMIntakeAppendixIEntity salida;
            salida = new TCMIntakeAppendixIEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmClient = await _context.TCMClient.Include(n => n.Casemanager).ThenInclude(n => n.TCMSupervisor).FirstAsync(n => n.Id == model.IdTCMClient),
                AdmissionedFor = model.AdmissionedFor,
                Approved = model.Approved,
                Date = model.Date,
                SupervisorSignatureDate = model.SupervisorSignatureDate,
                TcmClient_FK = model.TcmClient_FK,
                TcmSupervisor = model.TcmSupervisor,
                HasAmental2 = model.HasAmental2,
                HasAmental6 = model.HasAmental6,
                HasRecolated = model.HasRecolated,
                IsEnrolled = model.IsEnrolled,
                IsInOut = model.IsInOut,
                IsNot = model.IsNot,
                Lacks = model.Lacks,
                RequiresOngoing = model.RequiresOngoing,
                RequiresServices = model.RequiresServices
            };

            return salida;
        }

        public TCMIntakeAppendixIViewModel ToTCMIntakeAppendixIViewModel(TCMIntakeAppendixIEntity model)
        {
            return new TCMIntakeAppendixIViewModel
            {
                Id = model.Id,
                TcmClient = model.TcmClient,
                IdTCMClient = model.TcmClient_FK,
                AdmissionedFor = model.AdmissionedFor,
                Approved = model.Approved,
                Date = model.Date,
                SupervisorSignatureDate = model.SupervisorSignatureDate,
                TcmClient_FK = model.TcmClient_FK,
                TcmSupervisor = model.TcmSupervisor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                HasAmental2 = model.HasAmental2,
                HasAmental6 = model.HasAmental6,
                HasRecolated = model.HasRecolated,
                IsEnrolled = model.IsEnrolled,
                IsInOut = model.IsInOut,
                IsNot = model.IsNot,
                Lacks = model.Lacks,
                RequiresOngoing = model.RequiresOngoing,
                RequiresServices = model.RequiresServices
            };

        }

        public IntakeClientIdDocumentVerificationEntity ToIntakeClientIdDocumentVerificationEntity(IntakeClientIdDocumentVerificationViewModel model, bool isNew, string userId)
        {
            return new IntakeClientIdDocumentVerificationEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardianOrClient = model.DateSignatureLegalGuardianOrClient,
                AdmissionedFor = model.AdmissionedFor,
                HealthPlan = model.HealthPlan,
                Id_DriverLicense = model.Id_DriverLicense,
                MedicaidId = model.MedicaidId,
                MedicareCard = model.MedicareCard,
                Other_Identification = model.Other_Identification,
                Other_Name = model.Other_Name,
                Passport_Resident = model.Passport_Resident,
                Social = model.Social

            };
        }

        public IntakeClientIdDocumentVerificationViewModel ToIntakeClientIdDocumentVerificationViewModel(IntakeClientIdDocumentVerificationEntity model)
        {
            return new IntakeClientIdDocumentVerificationViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardianOrClient = model.DateSignatureLegalGuardianOrClient,
                AdmissionedFor = model.AdmissionedFor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                HealthPlan = model.HealthPlan,
                Id_DriverLicense = model.Id_DriverLicense,
                MedicaidId = model.MedicaidId,
                MedicareCard = model.MedicareCard,
                Other_Identification = model.Other_Identification,
                Other_Name = model.Other_Name,
                Passport_Resident = model.Passport_Resident,
                Social = model.Social

            };
        }

        public IntakeForeignLanguageEntity ToIntakeForeignLanguageEntity(IntakeForeignLanguageViewModel model, bool isNew, string userId)
        {
            return new IntakeForeignLanguageEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignatureLegalGuardian = model.DateSignatureLegalGuardian,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor

            };
        }

        public IntakeForeignLanguageViewModel ToIntakeForeignLanguageViewModel(IntakeForeignLanguageEntity model)
        {
            return new IntakeForeignLanguageViewModel
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
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn
            };
        }

        public async Task<SafetyPlanEntity> ToSafetyPlanEntity(SafetyPlanViewModel model, bool isNew, string userId)
        {
            return new SafetyPlanEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = await _context.Clients.FirstOrDefaultAsync(n => n.Id == model.Client_FK),
                Client_FK = model.IdClient,
                AdviceIwould = model.AdviceIwould,
                DateSignatureClient = model.DateSignatureClient,
                DateSignatureFacilitator = model.DateSignatureFacilitator,
                Documents = model.Documents,
                Facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.Id == model.IdFacilitator),
                PeopleIcanCall = model.PeopleIcanCall,
                ThingsThat = model.ThingsThat,
                WarningSignsOfCrisis = model.WarningSignsOfCrisis,
                WaysToDistract = model.WaysToDistract,
                WaysToKeepmyselfSafe = model.WaysToKeepmyselfSafe,
                Status = model.Status,
                Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == model.IdSupervisor),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DateDocument = model.DateDocument,
                DocumentAssisstant = await _context.DocumentsAssistant.FirstOrDefaultAsync(n => n.Id == model.IdDocumentAssisstant)
                
            };
        }

        public SafetyPlanViewModel ToSafetyPlanViewModel(SafetyPlanEntity model)
        {
            SafetyPlanViewModel salida;
            salida = new SafetyPlanViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                AdviceIwould = model.AdviceIwould,
                DateSignatureClient = model.DateSignatureClient,
                DateSignatureFacilitator = model.DateSignatureFacilitator,
                Documents = model.Documents,
                Facilitator = (model.Facilitator == null) ? new FacilitatorEntity() : model.Facilitator,
                PeopleIcanCall = model.PeopleIcanCall,
                ThingsThat = model.ThingsThat,
                WarningSignsOfCrisis = model.WarningSignsOfCrisis,
                WaysToDistract = model.WaysToDistract,
                WaysToKeepmyselfSafe = model.WaysToKeepmyselfSafe,
                IdFacilitator = (model.Facilitator == null) ? 0 : model.Facilitator.Id,
                IdSupervisor = (model.Supervisor == null) ? 0 : model.Supervisor.Id,
                Supervisor = (model.Supervisor == null)? new SupervisorEntity() : model.Supervisor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                DateDocument = model.DateDocument,
                DocumentAssisstant = (model.DocumentAssisstant == null) ? new DocumentsAssistantEntity() : model.DocumentAssisstant,
                IdDocumentAssisstant = (model.DocumentAssisstant == null) ? 0 : model.DocumentAssisstant.Id,
                Status = model.Status

            };
                        
            return salida;
        }

        public async Task<IncidentReportEntity> ToIncidentReportEntity(IncidentReportViewModel model, bool isNew, string userId)
        {
            return new IncidentReportEntity
            {
                Id = isNew ? 0 : model.Id,
                Client =await _context.Clients.FirstOrDefaultAsync(n => n.Id == model.IdClient),
                DateSignatureEmployee = model.DateSignatureEmployee,
                Facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.Id == model.IdFacilitator),
                Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == model.IdSupervisor),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                DocumentAssisstant = await _context.DocumentsAssistant.FirstOrDefaultAsync(n => n.Id == model.IdDocumentAssisstant),
                //AdmissionFor
                DateIncident = model.DateIncident,
                DateReport = model.DateReport,
                DescriptionIncident = model.DescriptionIncident,
                Injured = model.Injured,
                Injured_Description = model.Injured_Description,
                Location = model.Location,
                TimeIncident = model.TimeIncident,
                Witnesses = model.Witnesses,
                Witnesses_Contact = model.Witnesses_Contact,
                AdmissionFor = model.AdmissionFor

            };
        }

        public IncidentReportViewModel ToIncidentReportViewModel(IncidentReportEntity model)
        {
            IncidentReportViewModel salida;
            salida = new IncidentReportViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Facilitator = (model.Facilitator == null) ? new FacilitatorEntity() : model.Facilitator,
                IdFacilitator = (model.Facilitator == null) ? 0 : model.Facilitator.Id,
                IdSupervisor = (model.Supervisor == null) ? 0 : model.Supervisor.Id,
                Supervisor = (model.Supervisor == null) ? new SupervisorEntity() : model.Supervisor,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                DocumentAssisstant = (model.DocumentAssisstant == null) ? new DocumentsAssistantEntity() : model.DocumentAssisstant,
                IdDocumentAssisstant = (model.DocumentAssisstant == null) ? 0 : model.DocumentAssisstant.Id,
                DateIncident = model.DateIncident,
                DateReport = model.DateReport,
                DescriptionIncident = model.DescriptionIncident,
                Injured = model.Injured,
                Injured_Description = model.Injured_Description,
                Location = model.Location,
                TimeIncident = model.TimeIncident,
                Witnesses = model.Witnesses,
                Witnesses_Contact = model.Witnesses_Contact,
                AdmissionFor = model.AdmissionFor,
                DateSignatureEmployee = model.DateSignatureEmployee
            };

            return salida;
        }

        public MeetingNoteEntity ToMeetingNoteEntity(MeetingNotesViewModel model, bool isNew)
        {
            return new MeetingNoteEntity
            {
                Id = isNew ? 0 : model.Id,
                Date = model.Date,
                Description = model.Description,
                PlanNote = model.PlanNote,
                Status = model.Status,
                Title = model.Title,
                Supervisor = model.Supervisor,
                FacilitatorList = model.FacilitatorList
            };
        }

        public MeetingNotesViewModel ToMeetingNoteViewModel(MeetingNoteEntity entity)
        {

            MeetingNotesViewModel model = new MeetingNotesViewModel();
            model = new MeetingNotesViewModel
            {
                Id = entity.Id,
                Date = entity.Date,
                Description = entity.Description,
                PlanNote = entity.PlanNote,
                Status = entity.Status,
                Title = entity.Title,
                Supervisor = entity.Supervisor,
                FacilitatorList = entity.FacilitatorList
            };
         
            return model;
        }

        public MeetingNotes_Facilitator ToMeetingNoteFacilitatorEntity(MeetingNotesFacilitatorModel model, bool isNew)
        {
            return new MeetingNotes_Facilitator
            {
                Id = isNew ? 0 : model.Id,
                DateSign = model.DateSign,
                Intervention = model.Intervention,
                Sign = model.Sign
                
            };
        }

        public MeetingNotesFacilitatorModel ToMeetingNoteFacilitatorViewModel(MeetingNotes_Facilitator entity)
        {

            MeetingNotesFacilitatorModel model = new MeetingNotesFacilitatorModel();
            model = new MeetingNotesFacilitatorModel
            {
               Id = entity.Id,
               DateSign = entity.DateSign,
               Intervention = entity.Intervention,
               Sign = entity.Sign,
               Facilitator = entity.Facilitator,
               MeetingNoteEntity = entity.MeetingNoteEntity,
               IdSupervisorNote = entity.MeetingNoteEntity.Id
            };

            return model;
        }

        public TCMPayStubEntity ToPayStubEntity(TCMNotePendingByPayStubViewModel model, bool isNew)
        {
            return new TCMPayStubEntity
            {
                Id = isNew ? 0 : model.Id,
                Amount = model.Amount,
                DatePayStub = model.DatePayStub,
                DatePayStubClose = model.DatePayStubClose,
                DatePayStubPayment = model.DatePayStubPayment,
                StatusPayStub = StatusTCMPaystubUtils.GetStatusBillByIndex(model.IdStatus),
                TCMNotes = model.TCMNoteList,
                Units = model.Units,
                CaseMannager = _context.CaseManagers.Find(model.IdCaseManager),
                TCMPayStubDetails = model.TCMPaystubDetails
            };
        }

        public TCMNotePendingByPayStubViewModel ToPayStubViewModel(TCMPayStubEntity model)
        {
            return new TCMNotePendingByPayStubViewModel
            {
                Id = model.Id,
                Amount = model.Amount,
                TCMPaystubDetails = model.TCMPayStubDetails,
                DatePayStub = model.DatePayStub,
                DatePayStubClose = model.DatePayStubClose,
                DatePayStubPayment = model.DatePayStubPayment,
                Units = model.Units,
                IdStatus = (model.StatusPayStub == StatusTCMPaystub.Pending) ? 0 : 1,
                StatusList = _combosHelper.GetComboBillStatus()
                
               // AmountCMHNotes = model.TCMPayStubDetails.Where(n => n.ServiceAgency == ServiceAgency.CMH).Count(),
               // AmountTCMNotes = model.BillDmsDetails.Where(n => n.ServiceAgency == ServiceAgency.TCM).Count(),
                
            };
        }

        public TCMIntakeMedicalHistoryEntity ToTCMIntakeMedicalHistoryEntity(TCMIntakeMedicalHistoryViewModel model, bool isNew, string userId)
        {
            return new TCMIntakeMedicalHistoryEntity
            {
                Id = isNew ? 0 : model.Id,
                TCMClient = _context.TCMClient.Find(model.IdTCMClient),
                TCMClient_FK = model.TCMClient_FK,
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
                InformationProvided = model.InformationProvided,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                
            };
        }

        public TCMIntakeMedicalHistoryViewModel ToTCMIntakeMedicalHistoryViewModel(TCMIntakeMedicalHistoryEntity model)
        {
            return new TCMIntakeMedicalHistoryViewModel
            {
                Id = model.Id,
                TCMClient = model.TCMClient,
                IdTCMClient = model.TCMClient.Id,
                TCMClient_FK = model.TCMClient_FK,
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
                InformationProvided = model.InformationProvided,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                StartTime = model.StartTime,
                EndTime = model.EndTime

            };

        }

        public Workday_Activity_Facilitator ToWorkdayActivityFacilitatorEntity(Workday_Activity_FacilitatorSkillViewModel model)
        {
            return new Workday_Activity_Facilitator
            {
                Id = model.Id,
                activityDailyLiving = model.activities,
                communityResources = model.community,
                copingSkills = model.coping,
                diseaseManagement = model.disease,
                healthyLiving = model.healthy,
                lifeSkills = model.life,
                relaxationTraining = model.relaxation,
                socialSkills = model.social,
                stressManagement = model.stress,
                AM = model.am,
                PM = model.pm
            };
        }

        public Workday_Activity_FacilitatorSkillViewModel ToWorkdayActivityFacilitatorViewModel(Workday_Activity_Facilitator model)
        {
            return new Workday_Activity_FacilitatorSkillViewModel
            {
                Id = model.Id,
                activities = (bool)model.activityDailyLiving,
                community = (bool)model.communityResources,
                disease = (bool)model.diseaseManagement,
                healthy = (bool)model.healthyLiving,
                life = (bool)model.lifeSkills,
                relaxation = (bool)model.relaxationTraining,
                social = (bool)model.socialSkills,
                stress = (bool)model.stressManagement,
                coping = (bool)model.copingSkills,
                am = model.AM,
                pm = model.PM
            };
        }

        public IntakeNoHarmEntity ToIntakeNoHarmEntity(IntakeNoHarmViewModel model, bool isNew)
        {
            return new IntakeNoHarmEntity
            {
                Id = isNew ? 0 : model.Id,
                Client = model.Client,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor

            };
        }

        public IntakeNoHarmViewModel ToIntakeNoHarmViewModel(IntakeNoHarmEntity model)
        {
            return new IntakeNoHarmViewModel
            {
                Id = model.Id,
                Client = model.Client,
                IdClient = model.Client.Id,
                Client_FK = model.Client_FK,
                DateSignatureEmployee = model.DateSignatureEmployee,
                DateSignaturePerson = model.DateSignaturePerson,
                Documents = model.Documents,
                AdmissionedFor = model.AdmissionedFor

            };

        }

        public CourseEntity ToCourseEntity(CourseViewModel model, bool isNew, string userId)
        {
            return new CourseEntity
            {
                Id = isNew ? 0 : model.Id,
                ValidPeriod = model.ValidPeriod,
                Name = model.Name,
                Role = (model.IdRole == 1) ? UserType.Documents_Assistant : (model.IdRole == 2) ? UserType.Facilitator : (model.IdRole == 3) ? UserType.Supervisor : (model.IdRole == 4) ? UserType.CaseManager : (model.IdRole == 5) ? UserType.TCMSupervisor : (model.IdRole == 6) ? UserType.Manager : (model.IdRole == 7) ? UserType.Admin : (model.IdRole == 8) ? UserType.Frontdesk : UserType.Biller,
               // Clinic =  _context.Clinics.Find(model.IdClinic),
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                Description = model.Description,
                Active = model.Active

            };
        }

        public CourseViewModel ToCourseViewModel(CourseEntity model)
        {
            return new CourseViewModel
            {
                Id = model.Id,
                ValidPeriod = model.ValidPeriod,
                Name = model.Name,
                IdRole = (model.Role == UserType.Documents_Assistant) ? 1 : (model.Role == UserType.Facilitator) ? 2 : (model.Role == UserType.Supervisor) ? 3
                        : (model.Role == UserType.CaseManager) ? 4 : (model.Role == UserType.TCMSupervisor) ? 5
                        : (model.Role == UserType.Manager) ? 6 : (model.Role == UserType.Admin) ? 7 : (model.Role == UserType.Frontdesk) ? 8
                        : (model.Role == UserType.Biller) ? 9 : 0,

                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                Roles = _combosHelper.GetComboRoles(),
                Description = model.Description,
                Active = model.Active

            };

        }

        public CaseManagerCertificationEntity ToCaseManagerCertificationEntity(CaseMannagerCertificationViewModel model, bool isNew, string userId)
        {
            CourseEntity course = _context.Courses.FirstOrDefault(n => n.Id == model.IdCourse);
            return new CaseManagerCertificationEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = course.Name,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                CertificateDate = model.CertificateDate,
                CertificationNumber = model.CertificationNumber,
                ExpirationDate = model.ExpirationDate,
                Course = course,
                TCM = _context.CaseManagers.FirstOrDefault(n => n.Id == model.IdTCM)
                
            };
        }

        public CaseMannagerCertificationViewModel ToCaseManagerCertificationViewModel(CaseManagerCertificationEntity model)
        {
            return new CaseMannagerCertificationViewModel
            {
                Id = model.Id,
                Name = model.Name,
                IdCourse = model.Course.Id,
                IdTCM = model.TCM.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TCMs = _combosHelper.GetComboCaseManager(),
                Courses = _combosHelper.GetComboCourseByRole(UserType.CaseManager),
                CertificateDate = model.CertificateDate,
                ExpirationDate= model.ExpirationDate,
                CertificationNumber = model.CertificationNumber,
                TCM = model.TCM,
                Course = model.Course
                
            };

        }

        public TCMSupervisorCertificationEntity ToTCMSupervisorCertificationEntity(TCMSupervisorCertificationViewModel model, bool isNew, string userId)
        {
            CourseEntity course = _context.Courses.FirstOrDefault(n => n.Id == model.IdCourse);
            return new TCMSupervisorCertificationEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = course.Name,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                CertificateDate = model.CertificateDate,
                CertificationNumber = model.CertificationNumber,
                ExpirationDate = model.ExpirationDate,
                Course = course,
                TCMSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.Id == model.IdTCM)

            };
        }

        public TCMSupervisorCertificationViewModel ToTCMSupervisorCertificationViewModel(TCMSupervisorCertificationEntity model, int idClinic)
        {
            return new TCMSupervisorCertificationViewModel
            {
                Id = model.Id,
                Name = model.Name,
                IdCourse = model.Course.Id,
                IdTCM = model.TCMSupervisor.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedOn = model.LastModifiedOn,
                TCMSupervisors = _combosHelper.GetComboTCMSupervisorByClinic(idClinic),
                Courses = _combosHelper.GetComboCourseByRole(UserType.TCMSupervisor),
                CertificateDate = model.CertificateDate,
                ExpirationDate = model.ExpirationDate,
                CertificationNumber = model.CertificationNumber,
                TCMSupervisor = model.TCMSupervisor,
                Course = model.Course

            };

        }

        public PromotionEntity ToPromotionEntity(PromotionViewModel model, bool isNew)
        {
            return new PromotionEntity
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Active = model.Active,
                CloseDate = model.CloseDate,
                Contact = model.Contact,
                Description = model.Description,
                LinkReferred = model.LinkReferred,
                OpenDate = model.OpenDate,
                Precio = model.Precio,
                Photos = model.Photos,
                Location = model.Location,
                Promotion = model.Promotion,
                Room = model.Room
               
            };
        }

        public PromotionViewModel ToPromotionViewModel(PromotionEntity model)
        {
            return new PromotionViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Active = model.Active,
                CloseDate = model.CloseDate,
                Contact = model.Contact,
                Description = model.Description,
                LinkReferred = model.LinkReferred,
                OpenDate = model.OpenDate,
                Precio = model.Precio,
                Photos = model.Photos,
                Location = model.Location,
                Promotion = model.Promotion,
                Room = model.Room

            };

        }

        public PromotionPhotosEntity ToPromotionPhotoEntity(PromotionPhotoViewModel model, bool isNew, string photoPath)
        {
            return new PromotionPhotosEntity
            {
                Id = isNew ? 0 : model.Id,
                Description = model.Description,
                PhotoPath = photoPath,
                Promotion = _context.Promotions.FirstOrDefault(n => n.Id == model.IdPromotion)

            };
        }

        public PromotionPhotoViewModel ToPromotionPhotoViewModel(PromotionPhotosEntity model)
        {
            return new PromotionPhotoViewModel
            {
                Id = model.Id,
                PhotoPath = model.PhotoPath,
                Description = model.Description,
                Promotion = model.Promotion,
                IdPromotion = model.Promotion.Id
            };

        }

        public async Task<TCMSubServiceStepEntity> ToTCMSubServiceStepEntity(TCMSubServiceStepViewModel model, bool isNew, string userId)
        {
            return new TCMSubServiceStepEntity
            {
                Id = isNew ? 0 : model.Id,
                CreatedBy = isNew ? userId : model.CreatedBy,
                CreatedOn = isNew ? DateTime.Now : model.CreatedOn,
                LastModifiedBy = !isNew ? userId : string.Empty,
                LastModifiedOn = !isNew ? DateTime.Now : Convert.ToDateTime(null),
                TcmSubService = await _context.TCMSubServices.FindAsync(model.Id_TCMSubService),
                Name = model.Name,
                Description = model.Description,
                Active = model.Active,
                Orden = model.Orden,
                Units = model.Units

            };
        }

        public TCMSubServiceStepViewModel ToTCMSubServiceStepViewModel(TCMSubServiceStepEntity entity)
        {
            return new TCMSubServiceStepViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Id_TCMService = entity.TcmSubService.TcmService.Id,
                Id_TCMSubService = entity.TcmSubService.Id,
                Description = entity.Description,
                TcmSubService = entity.TcmSubService,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                Active = entity.Active,
                Orden = entity.Orden,
                Units = entity.Units

            };
        }


    }


}
