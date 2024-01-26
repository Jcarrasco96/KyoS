using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KyoS.Web.Helpers
{
    public class OverlapindHelper : IOverlapindHelper
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IUserHelper _userHelper;

        public OverlapindHelper(DataContext context, ICombosHelper combosHelper, IUserHelper userHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _userHelper = userHelper;
        }

        public string OverlapingDocumentsAssistant(int idDocumentAssistant, DateTime initialTime, DateTime endTime, int idDocument, DocumentDescription typeDocument)
        {
            DocumentsAssistantEntity documentAssistant = _context.DocumentsAssistant.FirstOrDefault(n => n.Id == idDocumentAssistant);

            if (documentAssistant != null)
            {
                if (typeDocument == DocumentDescription.Bio)
                {
                    BioEntity bio = _context.Bio
                                            .Include(n => n.Client)
                                            .FirstOrDefault(n => (n.DocumentsAssistant.Id == idDocumentAssistant)
                                                               && n.Id != idDocument
                                                               && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                                || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                                || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                                || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (bio != null)
                    {
                        return "BIO - " + bio.Client.Name + " - " + bio.StartTime + " - " + bio.EndTime;
                    }
                }
                else
                {
                    BioEntity bio = _context.Bio
                                            .Include(n => n.Client)
                                            .FirstOrDefault(n => (n.DocumentsAssistant.Id == idDocumentAssistant)
                                                 && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                 || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                 || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                 || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (bio != null)
                    {
                        return "BIO - " + bio.Client.Name + " - " + bio.StartTime + " - " + bio.EndTime;
                    }
                }
                //if(typeDocument == DocumentDescription.B)falta poner el brief en los tipod de documentos
               
                if (typeDocument == DocumentDescription.MTP)
                {
                    MTPEntity mtp = _context.MTPs
                                            .Include(n => n.Client)
                                            .FirstOrDefault(n => (n.DocumentAssistant.Id == idDocumentAssistant)
                                             && n.Id != idDocument
                                             && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                             || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                             || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                             || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (mtp != null)
                    {
                        return "MTP - " + mtp.Client.Name + " - " + mtp.StartTime + " - " + mtp.EndTime;
                    }
                }
                else
                {
                    MTPEntity mtp = _context.MTPs
                                            .Include(n => n.Client)
                                            .FirstOrDefault(n => (n.DocumentAssistant.Id == idDocumentAssistant)
                                             && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                             || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                             || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                             || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (mtp != null)
                    {
                        return "MTP - " + mtp.Client.Name + " - " + mtp.StartTime + " - " + mtp.EndTime;
                    }
                }
                if (typeDocument == DocumentDescription.Fars)
                {
                    FarsFormEntity fars = _context.FarsForm
                                                  .Include(n => n.Client)
                                                  .FirstOrDefault(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                                     && n.Id != idDocument
                                                                     && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                                      || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                                      || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                                      || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (fars != null)
                    {
                        return "FARS - " + fars.Client.Name + " - " + fars.StartTime + " - " + fars.EndTime;
                    }
                }
                else
                {
                    FarsFormEntity fars = _context.FarsForm
                                                  .Include(n => n.Client)
                                                  .FirstOrDefault(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                                   && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                                    || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                                    || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                                    || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (fars != null)
                    {
                        return "FARS - " + fars.Client.Name + " - " + fars.StartTime + " - " + fars.EndTime;
                    }
                }
                // el medical history esta comentado porque no se bilea
                /*if (typeDocument == DocumentDescription.MedicalHistory)
                {
                    if (_context.IntakeMedicalHistory.Where(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                         && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                          || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                          || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                          || (n.StartTime <= endTime && n.EndTime >= endTime))).Count() > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_context.IntakeMedicalHistory.Where(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                         && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                          || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                          || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                          || (n.StartTime <= endTime && n.EndTime >= endTime))).Count() > 0)
                    {
                        return false;
                    }
                }
                */
                if (typeDocument == DocumentDescription.MTP_review)
                {
                    MTPReviewEntity mtpr = _context.MTPReviews
                                                   .Include(n => n.Mtp)
                                                   .ThenInclude(n => n.Client)
                                                   .FirstOrDefault(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                         && n.Id != idDocument
                                                         && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                          || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                          || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                          || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (mtpr != null)
                    {
                        return "MTPR - " + mtpr.Mtp.Client.Name + " - " + mtpr.Mtp.StartTime + " - " + mtpr.Mtp.EndTime;
                    }
                }
                else
                {
                    MTPReviewEntity mtpr = _context.MTPReviews
                                                   .Include(n => n.Mtp)
                                                   .ThenInclude(n => n.Client)
                                                   .FirstOrDefault(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                                    && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                                     || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                                     || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                                     || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (mtpr != null)
                    {
                        return "MTPR - " + mtpr.Mtp.Client.Name + " - " + mtpr.Mtp.StartTime + " - " + mtpr.Mtp.EndTime;
                    }
                }
                // el brief esta puesto en tipo de documento como others
                if (typeDocument == DocumentDescription.Others)
                {
                    BriefEntity brief = _context.Brief
                                                .Include(n => n.Client)
                                                .FirstOrDefault(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                                   && n.Id != idDocument
                                                                   && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                                    || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                                    || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                                    || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (brief != null)
                    {
                        return "BRIEFS - " + brief.Client.Name + " - " + brief.StartTime + " - " + brief.EndTime;
                    }
                }
                else
                {
                    BriefEntity brief = _context.Brief
                                                .Include(n => n.Client)
                                                .FirstOrDefault(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                                 && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                                  || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                                  || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                                  || (n.StartTime <= endTime && n.EndTime >= endTime)));
                    if (brief != null)
                    {
                        return "BRIEFS - " + brief.Client.Name + " - " + brief.StartTime + " - " + brief.EndTime;
                    }
                }
            }

            return string.Empty;
        }

    }
}
