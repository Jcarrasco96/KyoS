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

        public bool OverlapingDocumentsAssistant(int idDocumentAssistant, DateTime initialTime, DateTime endTime)
        {
            DocumentsAssistantEntity documentAssistant = _context.DocumentsAssistant.FirstOrDefault(n => n.Id == idDocumentAssistant);

            if (documentAssistant != null)
            {
                if (_context.Bio.Where(n => (n.DocumentsAssistant.Id == idDocumentAssistant)
                                         && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                         || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                         || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                         || (n.StartTime <= endTime && n.EndTime >= endTime))).Count() > 0)
                {
                    return false;
                }
                if (_context.Brief.Where(n => (n.DocumentsAssistant.Id == idDocumentAssistant)
                                        && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                        || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                        || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                        || (n.StartTime <= endTime && n.EndTime >= endTime))).Count() > 0)
                {
                    return false;
                }
                if (_context.MTPs.Where(n => (n.DocumentAssistant.Id == idDocumentAssistant)
                                         && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                         || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                         || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                         || (n.StartTime <= endTime && n.EndTime >= endTime))).Count() > 0)
                {
                    return false;
                }
                if (_context.FarsForm.Where(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                         && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                         || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                         || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                         || (n.StartTime <= endTime && n.EndTime >= endTime))).Count() > 0)
                {
                    return false;
                }
                if (_context.IntakeMedicalHistory.Where(n => (n.CreatedBy == documentAssistant.LinkedUser)
                                                         && ((n.StartTime >= initialTime && n.StartTime <= endTime)
                                                          || (n.EndTime >= initialTime && n.EndTime <= endTime)
                                                          || (n.StartTime <= initialTime && n.EndTime >= initialTime)
                                                          || (n.StartTime <= endTime && n.EndTime >= endTime))).Count() > 0)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
