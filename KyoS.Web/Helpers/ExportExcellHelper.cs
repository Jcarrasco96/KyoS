using ClosedXML.Excel;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace KyoS.Web.Helpers
{
    public class ExportExcellHelper : IExportExcellHelper
    {
        public ExportExcellHelper(DataContext context)
        {

        }

        #region Export Excel
        public byte[] ExportFacilitatorHelper(List<FacilitatorEntity> aFacilitator)
        {
            var facilitator = aFacilitator;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("facilitator");
                var currentRow = 4;
                worksheet.Cell(currentRow, 4).Value = "Name";
                worksheet.Cell(currentRow, 5).Value = "Status";
                worksheet.Cell(currentRow, 6).Value = "Link User";
                worksheet.Style.Font.Bold = true;
                IXLRange range = worksheet.Range(worksheet.Cell(4, 4).Address, worksheet.Cell(4, 6).Address);
                range.Style.Fill.SetBackgroundColor(XLColor.DarkGray);
                range.SetAutoFilter();
                foreach (var item in facilitator)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 4).Value = item.Name;
                    worksheet.Cell(currentRow, 5).Value = item.Status;
                    worksheet.Cell(currentRow, 6).Value = item.LinkedUser;

                }

                worksheet.ColumnsUsed().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = ConvertStreamToByteArray(stream);
                    //return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                    return stream.ToArray();
                }
            }
        }

        public byte[] ExportBillForWeekHelper(List<Workday_Client> aworkday_Clients, string Periodo, string ClinicName, string data)
        {
            var workday_Clients = aworkday_Clients;
            int amount = 0;
            int unit_total = 0;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("All Services (" + aworkday_Clients.Count() + " Services)");
                worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                worksheet.Cell(2, 1).Value = ClinicName;
                worksheet.Cell(3, 2).Value = Periodo;
                worksheet.Cell(3, 13).Value = data;
                worksheet.Cell(3, 13).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(3, 13).Style.Font.FontSize = 16;
                worksheet.Cell(3, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(3, 1).Value = "SUPERBILL";
                worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.BlueGray;
                worksheet.Cell(3, 1).Style.Font.FontSize = 16;
                worksheet.Cell(3, 1).Style.Font.Bold = true;
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var currentRow = 4;
                worksheet.Cell(currentRow, 1).Value = "Client Name";
                worksheet.Cell(currentRow, 2).Value = "Case No";
                worksheet.Cell(currentRow, 3).Value = "DOB";
                worksheet.Cell(currentRow, 4).Value = "Medicaid Id";
                worksheet.Cell(currentRow, 5).Value = "Insurance | Member Id";
                worksheet.Cell(currentRow, 6).Value = "Diagnostics";
                worksheet.Cell(currentRow, 7).Value = "Date";
                worksheet.Cell(currentRow, 8).Value = "Service";
                worksheet.Cell(currentRow, 9).Value = "Setting";
                worksheet.Cell(currentRow, 10).Value = "Units";
                worksheet.Cell(currentRow, 11).Value = "Amount";
                worksheet.Cell(currentRow, 12).Value = "Therapist";
                worksheet.Cell(currentRow, 13).Value = "Status Bill";


                worksheet.Style.Font.Bold = true;
                IXLRange range = worksheet.Range(worksheet.Cell(4, 1).Address, worksheet.Cell(4, 13).Address);
                range.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                range.SetAutoFilter();
                currentRow++;

                List<string> codes = new List<string>();
                List<string> facilitators = new List<string>();

                foreach (var item in workday_Clients)
                {
                    if (codes.Contains(item.Client.Code) == false)
                    {
                        codes.Add(item.Client.Code);
                    }
                    if (facilitators.Contains(item.Facilitator.Name) == false)
                    {
                        facilitators.Add(item.Facilitator.Name);
                    }

                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.Client.Name;
                    worksheet.Cell(currentRow, 2).Value = item.Client.Code;
                    worksheet.Cell(currentRow, 3).Value = item.Client.DateOfBirth.ToShortDateString();
                    worksheet.Cell(currentRow, 4).Value = item.Client.MedicaidID;
                    if (item.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
                    {
                        worksheet.Cell(currentRow, 5).Value = item.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name;
                        worksheet.Cell(currentRow, 5).Value += " | " + item.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 5).Value = "-";
                    }
                    if (item.Client.Clients_Diagnostics.Count() > 0)
                    {
                        worksheet.Cell(currentRow, 6).Value = item.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 6).Value = "-";
                    }
                    worksheet.Cell(currentRow, 7).Value = item.Workday.Date.ToShortDateString();
                    if (item.Note != null)
                    {
                        worksheet.Cell(currentRow, 8).Value = item.Note.Setting;
                        worksheet.Cell(currentRow, 9).Value = 16;
                        worksheet.Cell(currentRow, 10).Value = 16 * 9;
                        unit_total += 16;
                        amount += (16 * 9);
                    }
                    else
                    {
                        if (item.NoteP != null)
                        {
                            worksheet.Cell(currentRow, 8).Value = "PSR";
                            worksheet.Cell(currentRow, 9).Value = item.NoteP.Setting;
                            worksheet.Cell(currentRow, 10).Value = item.NoteP.RealUnits;
                            worksheet.Cell(currentRow, 11).Value = item.NoteP.RealUnits * 9;
                            unit_total += item.NoteP.RealUnits;
                            amount += (item.NoteP.RealUnits * 9);
                        }
                        else
                        {
                            if (item.IndividualNote != null)
                            {
                                worksheet.Cell(currentRow, 8).Value = "Ind.";
                                worksheet.Cell(currentRow, 9).Value = "";
                                worksheet.Cell(currentRow, 10).Value = 4;
                                worksheet.Cell(currentRow, 11).Value = 4 * 12;
                                unit_total += 4;
                                amount += (4 * 9);
                            }
                            else
                            {
                                if (item.GroupNote != null)
                                {
                                    worksheet.Cell(currentRow, 8).Value = "Group";
                                    worksheet.Cell(currentRow, 9).Value = "";
                                    worksheet.Cell(currentRow, 10).Value = 8;
                                    worksheet.Cell(currentRow, 11).Value = 8 * 7;
                                    unit_total += 8;
                                    amount += (8 * 7);
                                }
                                else
                                {
                                    if (item.GroupNote2 != null)
                                    {
                                        worksheet.Cell(currentRow, 8).Value = "Group";
                                        worksheet.Cell(currentRow, 9).Value = "";
                                        worksheet.Cell(currentRow, 10).Value = item.GroupNote2.GroupNotes2_Activities.Count();
                                        worksheet.Cell(currentRow, 11).Value = item.GroupNote2.GroupNotes2_Activities.Count() * 7;
                                        unit_total += 8;
                                        amount += (8 * 7);
                                    }
                                    else
                                    {
                                        worksheet.Cell(currentRow, 8).Value = "PSR";
                                        worksheet.Cell(currentRow, 9).Value = "53";
                                        worksheet.Cell(currentRow, 10).Value = 16;
                                        worksheet.Cell(currentRow, 11).Value = 16 * 9;
                                        unit_total += 16;
                                        amount += (16 * 9);
                                    }
                                   
                                }
                            }
                        }
                    }

                    IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 13).Address);
                    rangeCurrent.Style.Font.FontSize = 11;
                    rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeCurrent.Style.Font.Bold = false;

                    worksheet.Cell(5, 1).Value = codes.Count() + " Clients";
                    worksheet.Cell(5, 8).Value = workday_Clients.Count() + " Services";
                    worksheet.Cell(5, 10).Value = unit_total;
                    worksheet.Cell(5, 11).Value = "$ " + amount;
                    worksheet.Cell(5, 12).Value = facilitators.Count() + " Facilitators";
                    IXLRange rangeTotal = worksheet.Range(worksheet.Cell(5, 1).Address, worksheet.Cell(5, 12).Address);
                    rangeTotal.Style.Font.FontSize = 13;
                    rangeTotal.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeTotal.Style.Font.Bold = true;
                    rangeTotal.Style.Font.FontColor = XLColor.GoldenBrown;

                    worksheet.Cell(currentRow, 12).Value = item.Facilitator.Name;
                    if (item.DeniedBill == true)
                    {
                        worksheet.Cell(currentRow, 13).Value = "Denied";
                        worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Red);
                    }
                    else
                    {
                        if (item.BilledDate != null && item.PaymentDate != null)
                        {
                            worksheet.Cell(currentRow, 13).Value = "Paid";
                            worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Green);
                        }
                        else
                        {
                            if (item.BilledDate != null && item.PaymentDate == null)
                            {
                                worksheet.Cell(currentRow, 13).Value = "Pending";
                                worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.BabyBlue);
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 13).Value = "Not Billed";
                                worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                            }
                        }
                    }


                }

                worksheet.ColumnsUsed().AdjustToContents();

                IXLRange range1 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 13).Address);
                range1.Style.Font.FontSize = 18;
                range1.Style.Font.Bold = false;
                range1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range1.Merge();
                IXLRange range2 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 13).Address);
                range2.Style.Font.FontSize = 16;
                range2.Style.Font.Bold = false;
                range2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range2.Merge();
                IXLRange range3 = worksheet.Range(worksheet.Cell(3, 2).Address, worksheet.Cell(3, 13).Address);
                range3.Style.Font.FontSize = 14;
                range3.Style.Font.Bold = false;
                range3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range3.Merge();

                int count = 0;
                foreach (var item in workday_Clients.GroupBy(n => n.Client))
                {
                    if (item.Key.Name.ToString().Length > 24)
                    {
                        count = workbook.Worksheets.Where(n => n.Name.Contains(item.Key.Name.Substring(0, 24).ToString()) == true).Count();
                        if (count > 0)
                        {
                            count++;
                            worksheet = workbook.Worksheets.Add(item.Key.Name.Substring(0, 24).ToString() + '-' + count.ToString() + '-' + item.Key.Workdays_Clients.Count().ToString());
                        }
                        else
                        {
                            worksheet = workbook.Worksheets.Add(item.Key.Name.Substring(0, 24).ToString() + item.Key.Workdays_Clients.Count().ToString());
                        }
                    }
                    else
                    {
                        count = workbook.Worksheets.Where(n => n.Name.Contains(item.Key.Name.ToString()) == true).Count();
                        if (count > 0)
                        {
                            count++;
                            worksheet = workbook.Worksheets.Add(item.Key.Name.ToString() + '-' + count.ToString() + '-' + item.Key.Workdays_Clients.Count().ToString());
                        }
                        else
                        {
                            worksheet = workbook.Worksheets.Add(item.Key.Name.ToString() + item.Key.Workdays_Clients.Count().ToString());
                        }
                        
                    }

                    count = 0;

                    worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                    worksheet.Cell(2, 1).Value = ClinicName;
                    worksheet.Cell(3, 2).Value = Periodo;
                    worksheet.Cell(3, 13).Value = data;
                    worksheet.Cell(3, 13).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(3, 13).Style.Font.FontSize = 16;
                    worksheet.Cell(3, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(3, 1).Value = "SUPERBILL";
                    worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.BlueGray;
                    worksheet.Cell(3, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(3, 1).Style.Font.Bold = true;
                    worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    IXLRange range11 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 13).Address);
                    range11.Style.Font.FontSize = 18;
                    range11.Style.Font.Bold = false;
                    range11.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    range11.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range11.Merge();
                    IXLRange range21 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 13).Address);
                    range21.Style.Font.FontSize = 16;
                    range21.Style.Font.Bold = false;
                    range21.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range21.Merge();
                    IXLRange range31 = worksheet.Range(worksheet.Cell(3, 2).Address, worksheet.Cell(3, 12).Address);
                    range31.Style.Font.FontSize = 14;
                    range31.Style.Font.Bold = false;
                    range31.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range31.Merge();

                    currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Client Name";
                    worksheet.Cell(currentRow, 2).Value = "Case No";
                    worksheet.Cell(currentRow, 3).Value = "DOB";
                    worksheet.Cell(currentRow, 4).Value = "Medicaid Id";
                    worksheet.Cell(currentRow, 5).Value = "Insurance | Member Id";
                    worksheet.Cell(currentRow, 6).Value = "Diagnostics";
                    worksheet.Cell(currentRow, 7).Value = "Date";
                    worksheet.Cell(currentRow, 8).Value = "Setting";
                    worksheet.Cell(currentRow, 9).Value = "Setting";
                    worksheet.Cell(currentRow, 10).Value = "Units";
                    worksheet.Cell(currentRow, 11).Value = "Amount";
                    worksheet.Cell(currentRow, 12).Value = "Therapist";
                    worksheet.Cell(currentRow, 13).Value = "Status Bill";


                    IXLRange range0 = worksheet.Range(worksheet.Cell(4, 1).Address, worksheet.Cell(4, 13).Address);
                    range0.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    range0.SetAutoFilter();
                    range0.Style.Font.Bold = true;
                    currentRow++;
                    unit_total = 0;
                    amount = 0;
                    worksheet.ColumnsUsed().AdjustToContents();

                    codes = new List<string>();
                    facilitators = new List<string>();

                    foreach (var product in item)
                    {
                        if (codes.Contains(product.Client.Code) == false)
                        {
                            codes.Add(product.Client.Code);
                        }
                        if (facilitators.Contains(product.Facilitator.Name) == false)
                        {
                            facilitators.Add(product.Facilitator.Name);
                        }

                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = product.Client.Name;
                        worksheet.Cell(currentRow, 2).Value = product.Client.Code;
                        worksheet.Cell(currentRow, 3).Value = product.Client.DateOfBirth.ToShortDateString();
                        worksheet.Cell(currentRow, 4).Value = product.Client.MedicaidID;
                        if (product.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
                        {
                            worksheet.Cell(currentRow, 5).Value = product.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name;
                            worksheet.Cell(currentRow, 5).Value += " | " + product.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 5).Value = "-";
                        }
                        if (product.Client.Clients_Diagnostics.Count() > 0)
                        {
                            worksheet.Cell(currentRow, 6).Value = product.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 6).Value = "-";
                        }
                        worksheet.Cell(currentRow, 7).Value = product.Workday.Date.ToShortDateString();
                        if (product.Note != null)
                        {
                            worksheet.Cell(currentRow, 8).Value = product.Note.Setting;
                            worksheet.Cell(currentRow, 9).Value = 16;
                            worksheet.Cell(currentRow, 10).Value = 16 * 9;
                            unit_total += 16;
                            amount += (16 * 9);
                        }
                        else
                        {
                            if (product.NoteP != null)
                            {
                                worksheet.Cell(currentRow, 8).Value = product.NoteP.Setting;
                                worksheet.Cell(currentRow, 9).Value = product.NoteP.RealUnits;
                                worksheet.Cell(currentRow, 10).Value = product.NoteP.RealUnits * 9;
                                unit_total += product.NoteP.RealUnits;
                                amount += (product.NoteP.RealUnits * 9);
                            }
                            else
                            {
                                if (product.IndividualNote != null)
                                {
                                    worksheet.Cell(currentRow, 8).Value = "";
                                    worksheet.Cell(currentRow, 9).Value = 4;
                                    worksheet.Cell(currentRow, 10).Value = 4 * 12;
                                    unit_total += 4;
                                    amount += (4 * 9);
                                }
                                else
                                {
                                    if (product.GroupNote != null)
                                    {
                                        worksheet.Cell(currentRow, 8).Value = "";
                                        worksheet.Cell(currentRow, 9).Value = 8;
                                        worksheet.Cell(currentRow, 10).Value = 8 * 7;
                                        unit_total += 8;
                                        amount += (8 * 7);
                                    }
                                    else
                                    {
                                        if (product.GroupNote2 != null)
                                        {
                                            worksheet.Cell(currentRow, 8).Value = "";
                                            worksheet.Cell(currentRow, 9).Value = product.GroupNote2.GroupNotes2_Activities.Count();
                                            worksheet.Cell(currentRow, 10).Value = product.GroupNote2.GroupNotes2_Activities.Count() * 7;
                                            unit_total += 8;
                                            amount += (8 * 7);
                                        }
                                        else
                                        {
                                            worksheet.Cell(currentRow, 8).Value = "53";
                                            worksheet.Cell(currentRow, 9).Value = 16;
                                            worksheet.Cell(currentRow, 10).Value = 16 * 9;
                                            unit_total += 16;
                                            amount += (16 * 9);
                                        }
                                    }
                                }
                            }

                        }

                        worksheet.Cell(currentRow, 12).Value = product.Facilitator.Name;
                        if (product.DeniedBill == true)
                        {
                            worksheet.Cell(currentRow, 13).Value = "Denied";
                            worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Red);
                        }
                        else
                        {
                            if (product.BilledDate != null && product.PaymentDate != null)
                            {
                                worksheet.Cell(currentRow, 13).Value = "Paid";
                                worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Green);
                            }
                            else
                            {
                                if (product.BilledDate != null && product.PaymentDate == null)
                                {
                                    worksheet.Cell(currentRow, 13).Value = "Pending";
                                    worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.BabyBlue);
                                }
                                else
                                {
                                    worksheet.Cell(currentRow, 13).Value = "Not Billed";
                                    worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                                }
                            }
                        }

                        IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 13).Address);
                        rangeCurrent.Style.Font.FontSize = 12;
                        rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeCurrent.Style.Font.Bold = false;

                        worksheet.Cell(5, 1).Value = codes.Count() + " Client";
                        worksheet.Cell(5, 8).Value = item.Count() + " Services";
                        worksheet.Cell(5, 10).Value = unit_total;
                        worksheet.Cell(5, 11).Value = "$ " + amount;
                        worksheet.Cell(5, 12).Value = facilitators.Count() + " Facilitators";
                        IXLRange rangeTotal = worksheet.Range(worksheet.Cell(5, 1).Address, worksheet.Cell(5, 12).Address);
                        rangeTotal.Style.Font.FontSize = 13;
                        rangeTotal.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeTotal.Style.Font.Bold = true;
                        rangeTotal.Style.Font.FontColor = XLColor.GoldenBrown;
                    }
                    worksheet.ColumnsUsed().AdjustToContents();

                }


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = ConvertStreamToByteArray(stream);
                    //return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                    return stream.ToArray();
                }
            }
        }

        public byte[] ExportAllClients(List<ClientEntity> clients, string date = "")
        {
            var Clients = clients;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("All Clients (" + clients.Count() + ")");
                worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                worksheet.Cell(2, 1).Value = clients.ElementAt(0).Clinic.Name;
                worksheet.Cell(3, 1).Value = date;
                var currentRow = 4;
                worksheet.Cell(currentRow, 1).Value = "Client Name";
                worksheet.Cell(currentRow, 2).Value = "Case No";
                worksheet.Cell(currentRow, 3).Value = "DOB";
                worksheet.Cell(currentRow, 4).Value = "Gender";
                worksheet.Cell(currentRow, 5).Value = "Medicaid Id";
                worksheet.Cell(currentRow, 6).Value = "Medicare Id";
                worksheet.Cell(currentRow, 7).Value = "Insurance | Member Id";
                worksheet.Cell(currentRow, 8).Value = "Principal Diagnostics";
                worksheet.Cell(currentRow, 9).Value = "Admission";
                worksheet.Cell(currentRow, 10).Value = "Status";
                worksheet.Cell(currentRow, 11).Value = "Full Address";
                worksheet.Cell(currentRow, 12).Value = "City";
                worksheet.Cell(currentRow, 13).Value = "Zip Code";
                worksheet.Cell(currentRow, 14).Value = "Telephone";
                worksheet.Cell(currentRow, 15).Value = "Race";
                worksheet.Cell(currentRow, 16).Value = "Preferred Language";
                worksheet.Cell(currentRow, 17).Value = "Date Close";


                worksheet.Style.Font.Bold = true;
                IXLRange range = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 17).Address);
                range.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                range.SetAutoFilter();

                foreach (var item in Clients)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.Name;
                    worksheet.Cell(currentRow, 2).Value = item.Code;
                    worksheet.Cell(currentRow, 3).Value = item.DateOfBirth.ToShortDateString();
                    worksheet.Cell(currentRow, 4).Value = item.Gender;
                    worksheet.Cell(currentRow, 5).Value = item.MedicaidID;
                    worksheet.Cell(currentRow, 6).Value = item.MedicareId;

                    if (item.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
                    {
                        worksheet.Cell(currentRow, 7).Value = item.Clients_HealthInsurances.FirstOrDefault(n => n.Active == true).HealthInsurance.Name;
                        worksheet.Cell(currentRow, 7).Value += " | " + item.Clients_HealthInsurances.First(n => n.Active == true).MemberId;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 7).Value = " - ";
                        worksheet.Cell(currentRow, 7).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    }
                    if (item.Clients_Diagnostics.Where(n => n.Principal == true).Count() > 0)
                    {
                        worksheet.Cell(currentRow, 8).Value = item.Clients_Diagnostics.FirstOrDefault(n => n.Principal == true).Diagnostic.Code;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 8).Value = " - ";
                        worksheet.Cell(currentRow, 8).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    }

                    worksheet.Cell(currentRow, 9).Value = item.AdmisionDate;
                    worksheet.Cell(currentRow, 10).Value = item.Status;
                    if (item.Status == Common.Enums.StatusType.Open)
                    {
                        worksheet.Cell(currentRow, 10).Style.Fill.SetBackgroundColor(XLColor.Green);
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 10).Style.Fill.SetBackgroundColor(XLColor.Red);
                    }

                    worksheet.Cell(currentRow, 11).Value = item.FullAddress;
                    worksheet.Cell(currentRow, 12).Value = item.City;
                    worksheet.Cell(currentRow, 13).Value = item.ZipCode;
                    worksheet.Cell(currentRow, 14).Value = item.Telephone;
                    worksheet.Cell(currentRow, 15).Value = item.Race;
                    worksheet.Cell(currentRow, 16).Value = item.PreferredLanguage;
                    worksheet.Cell(currentRow, 17).Value = item.DateOfClose.ToShortDateString();

                    if (item.DateOfClose.DayOfYear == 1 && item.Status == Common.Enums.StatusType.Close)
                    {
                        worksheet.Cell(currentRow, 17).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    }

                    IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 17).Address);
                    rangeCurrent.Style.Font.FontSize = 11;
                    rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeCurrent.Style.Font.Bold = false;

                }

                worksheet.ColumnsUsed().AdjustToContents();
                IXLRange range1 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 17).Address);
                range1.Style.Font.FontSize = 18;
                range1.Style.Font.Bold = false;
                range1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range1.Merge();
                IXLRange range2 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 17).Address);
                range2.Style.Font.FontSize = 16;
                range2.Style.Font.Bold = false;
                range2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range2.Merge();
                IXLRange range3 = worksheet.Range(worksheet.Cell(3, 1).Address, worksheet.Cell(3, 17).Address);
                range3.Style.Font.FontSize = 14;
                range3.Style.Font.Bold = false;
                range3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range3.Merge();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = ConvertStreamToByteArray(stream);
                    //return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                    return stream.ToArray();
                }
            }
        }

        public byte[] ExportBillHoldForWeekHelper(List<Workday_Client> aworkday_Clients, string Periodo, string ClinicName, string data)
        {
            var workday_Clients = aworkday_Clients;
            int amount = 0;
            int unit_total = 0;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("All Clients (" + aworkday_Clients.Count() + " Services)");
                worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                worksheet.Cell(2, 1).Value = ClinicName;
                worksheet.Cell(3, 2).Value = Periodo;
                worksheet.Cell(3, 12).Value = data;
                worksheet.Cell(3, 12).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(3, 12).Style.Font.FontSize = 16;
                worksheet.Cell(3, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(3, 1).Value = "SUPERBILL";
                worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.BlueGray;
                worksheet.Cell(3, 1).Style.Font.FontSize = 16;
                worksheet.Cell(3, 1).Style.Font.Bold = true;
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var currentRow = 4;
                worksheet.Cell(currentRow, 1).Value = "Client Name";
                worksheet.Cell(currentRow, 2).Value = "Case No";
                worksheet.Cell(currentRow, 3).Value = "DOB";
                worksheet.Cell(currentRow, 4).Value = "Medicaid Id";
                worksheet.Cell(currentRow, 5).Value = "Insurance | Member Id";
                worksheet.Cell(currentRow, 6).Value = "Diagnostics";
                worksheet.Cell(currentRow, 7).Value = "Date";
                worksheet.Cell(currentRow, 8).Value = "Setting";
                worksheet.Cell(currentRow, 9).Value = "Units";
                worksheet.Cell(currentRow, 10).Value = "Amount";
                worksheet.Cell(currentRow, 11).Value = "Therapist";
                worksheet.Cell(currentRow, 12).Value = "Status Bill";


                worksheet.Style.Font.Bold = true;
                IXLRange range = worksheet.Range(worksheet.Cell(4, 1).Address, worksheet.Cell(4, 12).Address);
                range.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                range.SetAutoFilter();
                currentRow++;

                List<string> codes = new List<string>();
                List<string> facilitators = new List<string>();

                foreach (var item in workday_Clients)
                {
                    if (codes.Contains(item.Client.Code) == false)
                    {
                        codes.Add(item.Client.Code);
                    }
                    if (facilitators.Contains(item.Facilitator.Name) == false)
                    {
                        facilitators.Add(item.Facilitator.Name);
                    }

                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.Client.Name;
                    worksheet.Cell(currentRow, 2).Value = item.Client.Code;
                    worksheet.Cell(currentRow, 3).Value = item.Client.DateOfBirth.ToShortDateString();
                    worksheet.Cell(currentRow, 4).Value = item.Client.MedicaidID;
                    if (item.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
                    {
                        worksheet.Cell(currentRow, 5).Value = item.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name;
                        worksheet.Cell(currentRow, 5).Value += " | " + item.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 5).Value = "-";
                    }
                    if (item.Client.Clients_Diagnostics.Count() > 0)
                    {
                        worksheet.Cell(currentRow, 6).Value = item.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 6).Value = "-";
                    }
                    worksheet.Cell(currentRow, 7).Value = item.Workday.Date.ToShortDateString();
                    if (item.Note != null)
                    {
                        worksheet.Cell(currentRow, 8).Value = item.Note.Setting;
                        worksheet.Cell(currentRow, 9).Value = 16;
                        worksheet.Cell(currentRow, 10).Value = 16 * 9;
                        unit_total += 16;
                        amount += (16 * 9);
                    }
                    else
                    {
                        if (item.NoteP != null)
                        {
                            worksheet.Cell(currentRow, 8).Value = item.NoteP.Setting;
                            worksheet.Cell(currentRow, 9).Value = item.NoteP.RealUnits;
                            worksheet.Cell(currentRow, 10).Value = item.NoteP.RealUnits * 9;
                            unit_total += item.NoteP.RealUnits;
                            amount += (item.NoteP.RealUnits * 9);
                        }
                        else
                        {
                            if (item.IndividualNote != null)
                            {
                                worksheet.Cell(currentRow, 8).Value = "53";
                                worksheet.Cell(currentRow, 9).Value = 4;
                                worksheet.Cell(currentRow, 10).Value = 4 * 12;
                                unit_total += 4;
                                amount += (4 * 9);
                            }
                            else
                            {
                                if (item.GroupNote != null)
                                {
                                    worksheet.Cell(currentRow, 8).Value = "101";
                                    worksheet.Cell(currentRow, 9).Value = 8;
                                    worksheet.Cell(currentRow, 10).Value = 8 * 7;
                                    unit_total += 8;
                                    amount += (8 * 7);
                                }
                                else
                                {
                                    worksheet.Cell(currentRow, 8).Value = "53";
                                    worksheet.Cell(currentRow, 9).Value = 16;
                                    worksheet.Cell(currentRow, 10).Value = 16 * 9;
                                    unit_total += 16;
                                    amount += (16 * 9);
                                }
                            }
                        }
                    }

                    IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 12).Address);
                    rangeCurrent.Style.Font.FontSize = 11;
                    rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeCurrent.Style.Font.Bold = false;

                    worksheet.Cell(5, 1).Value = codes.Count() + " Clients";
                    worksheet.Cell(5, 7).Value = workday_Clients.Count() + " Services";
                    worksheet.Cell(5, 9).Value = unit_total;
                    worksheet.Cell(5, 10).Value = "$ " + amount;
                    worksheet.Cell(5, 11).Value = facilitators.Count() + " Facilitators";
                    IXLRange rangeTotal = worksheet.Range(worksheet.Cell(5, 1).Address, worksheet.Cell(5, 11).Address);
                    rangeTotal.Style.Font.FontSize = 13;
                    rangeTotal.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeTotal.Style.Font.Bold = true;
                    rangeTotal.Style.Font.FontColor = XLColor.GoldenBrown;

                    worksheet.Cell(currentRow, 11).Value = item.Facilitator.Name;
                    worksheet.Cell(currentRow, 12).Value = "Hold";
                    worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.Red);

                }

                worksheet.ColumnsUsed().AdjustToContents();

                IXLRange range1 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 12).Address);
                range1.Style.Font.FontSize = 18;
                range1.Style.Font.Bold = false;
                range1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range1.Merge();
                IXLRange range2 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 12).Address);
                range2.Style.Font.FontSize = 16;
                range2.Style.Font.Bold = false;
                range2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range2.Merge();
                IXLRange range3 = worksheet.Range(worksheet.Cell(3, 2).Address, worksheet.Cell(3, 11).Address);
                range3.Style.Font.FontSize = 14;
                range3.Style.Font.Bold = false;
                range3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range3.Merge();

                foreach (var item in workday_Clients.GroupBy(n => n.Client))
                {
                    if (item.Key.Name.ToString().Length > 30)
                    {
                        worksheet = workbook.Worksheets.Add(item.Key.Name.Substring(0, 28).ToString());
                    }
                    else
                    {
                        worksheet = workbook.Worksheets.Add(item.Key.Name.ToString());
                    }

                    worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                    worksheet.Cell(2, 1).Value = ClinicName;
                    worksheet.Cell(3, 2).Value = Periodo;
                    worksheet.Cell(3, 12).Value = data;
                    worksheet.Cell(3, 12).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(3, 12).Style.Font.FontSize = 16;
                    worksheet.Cell(3, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(3, 1).Value = "SUPERBILL";
                    worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.BlueGray;
                    worksheet.Cell(3, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(3, 1).Style.Font.Bold = true;
                    worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    IXLRange range11 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 12).Address);
                    range11.Style.Font.FontSize = 18;
                    range11.Style.Font.Bold = false;
                    range11.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    range11.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range11.Merge();
                    IXLRange range21 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 12).Address);
                    range21.Style.Font.FontSize = 16;
                    range21.Style.Font.Bold = false;
                    range21.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range21.Merge();
                    IXLRange range31 = worksheet.Range(worksheet.Cell(3, 2).Address, worksheet.Cell(3, 11).Address);
                    range31.Style.Font.FontSize = 14;
                    range31.Style.Font.Bold = false;
                    range31.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range31.Merge();

                    currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Client Name";
                    worksheet.Cell(currentRow, 2).Value = "Case No";
                    worksheet.Cell(currentRow, 3).Value = "DOB";
                    worksheet.Cell(currentRow, 4).Value = "Member Id";
                    worksheet.Cell(currentRow, 5).Value = "Insurance | Member Id";
                    worksheet.Cell(currentRow, 6).Value = "Diagnostics";
                    worksheet.Cell(currentRow, 7).Value = "Date";
                    worksheet.Cell(currentRow, 8).Value = "Setting";
                    worksheet.Cell(currentRow, 9).Value = "Units";
                    worksheet.Cell(currentRow, 10).Value = "Amount";
                    worksheet.Cell(currentRow, 11).Value = "Therapist";
                    worksheet.Cell(currentRow, 12).Value = "Status Bill";

                    IXLRange range0 = worksheet.Range(worksheet.Cell(4, 1).Address, worksheet.Cell(4, 12).Address);
                    range0.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    range0.SetAutoFilter();
                    range0.Style.Font.Bold = true;
                    currentRow++;
                    unit_total = 0;
                    amount = 0;
                    worksheet.ColumnsUsed().AdjustToContents();
                    codes = new List<string>();
                    facilitators = new List<string>();
                    foreach (var product in item)
                    {
                        if (codes.Contains(product.Client.Code) == false)
                        {
                            codes.Add(product.Client.Code);
                        }

                        if (facilitators.Contains(product.Facilitator.Name) == false)
                        {
                            facilitators.Add(product.Facilitator.Name);
                        }
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = product.Client.Name;
                        worksheet.Cell(currentRow, 2).Value = product.Client.Code;
                        worksheet.Cell(currentRow, 3).Value = product.Client.DateOfBirth.ToShortDateString();
                        worksheet.Cell(currentRow, 4).Value = product.Client.MedicaidID;
                        if (product.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
                        {
                            worksheet.Cell(currentRow, 5).Value = product.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name;
                            worksheet.Cell(currentRow, 5).Value += " | " + product.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 5).Value = "-";
                        }
                        if (product.Client.Clients_Diagnostics.Count() > 0)
                        {
                            worksheet.Cell(currentRow, 6).Value = product.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 6).Value = "-";
                        }
                        worksheet.Cell(currentRow, 7).Value = product.Workday.Date.ToShortDateString();
                        if (product.Note != null)
                        {
                            worksheet.Cell(currentRow, 8).Value = product.Note.Setting;
                            worksheet.Cell(currentRow, 9).Value = 16;
                            worksheet.Cell(currentRow, 10).Value = 16 * 9;
                            unit_total += 16;
                            amount += (16 * 9);
                        }
                        else
                        {
                            if (product.NoteP != null)
                            {
                                worksheet.Cell(currentRow, 8).Value = product.NoteP.Setting;
                                worksheet.Cell(currentRow, 9).Value = product.NoteP.RealUnits;
                                worksheet.Cell(currentRow, 10).Value = product.NoteP.RealUnits * 9;
                                unit_total += product.NoteP.RealUnits;
                                amount += (product.NoteP.RealUnits * 9);
                            }
                            else
                            {
                                if (product.IndividualNote != null)
                                {
                                    worksheet.Cell(currentRow, 8).Value = "53";
                                    worksheet.Cell(currentRow, 9).Value = 4;
                                    worksheet.Cell(currentRow, 10).Value = 4 * 12;
                                    unit_total += 4;
                                    amount += (4 * 9);
                                }
                                else
                                {
                                    if (product.GroupNote != null)
                                    {
                                        worksheet.Cell(currentRow, 8).Value = "101";
                                        worksheet.Cell(currentRow, 9).Value = 8;
                                        worksheet.Cell(currentRow, 10).Value = 8 * 7;
                                        unit_total += 8;
                                        amount += (8 * 7);
                                    }
                                    else
                                    {
                                        worksheet.Cell(currentRow, 8).Value = "53";
                                        worksheet.Cell(currentRow, 9).Value = 16;
                                        worksheet.Cell(currentRow, 10).Value = 16 * 9;
                                        unit_total += 16;
                                        amount += (16 * 9);
                                    }
                                }
                            }

                        }

                        worksheet.Cell(currentRow, 11).Value = product.Facilitator.Name;

                        IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 12).Address);
                        rangeCurrent.Style.Font.FontSize = 11;
                        rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeCurrent.Style.Font.Bold = false;

                        worksheet.Cell(5, 1).Value = codes.Count() + " Clients";
                        worksheet.Cell(5, 7).Value = item.Count() + " Services";
                        worksheet.Cell(5, 9).Value = unit_total;
                        worksheet.Cell(5, 10).Value = "$ " + amount;
                        worksheet.Cell(5, 11).Value = facilitators.Count() + " Facilitators";
                        IXLRange rangeTotal = worksheet.Range(worksheet.Cell(5, 1).Address, worksheet.Cell(5, 11).Address);
                        rangeTotal.Style.Font.FontSize = 13;
                        rangeTotal.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeTotal.Style.Font.Bold = true;
                        rangeTotal.Style.Font.FontColor = XLColor.GoldenBrown;
                    }
                    worksheet.ColumnsUsed().AdjustToContents();

                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = ConvertStreamToByteArray(stream);
                    //return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                    return stream.ToArray();
                }
            }
        }

        public byte[] ExportAllReferreds(List<Client_Referred> clients)
        {
            var Clients = clients;
            var workbook = new XLWorkbook();
            if (clients.Count() > 0)
            {
                
                {
                    var worksheet = workbook.Worksheets.Add("All Clients (" + clients.Count() + ")");
                    worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                    worksheet.Cell(2, 1).Value = clients.ElementAt(0).Client.Clinic.Name;
                    worksheet.Cell(3, 1).Value = "Date Report: " + DateTime.Today.ToLongDateString();
                    var currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Referred Name";
                    worksheet.Cell(currentRow, 2).Value = "Referred Address";
                    worksheet.Cell(currentRow, 3).Value = "Referred type";
                    worksheet.Cell(currentRow, 4).Value = "Client Name";
                    worksheet.Cell(currentRow, 5).Value = "Service";
                    worksheet.Cell(currentRow, 6).Value = "Code";
                    worksheet.Cell(currentRow, 7).Value = "Admission";
                    worksheet.Cell(currentRow, 8).Value = "Status";



                    worksheet.Style.Font.Bold = true;
                    IXLRange range = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 8).Address);
                    range.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    range.SetAutoFilter();

                    foreach (var item in Clients)
                    {
                        currentRow++;

                        worksheet.Cell(currentRow, 1).Value = item.Referred.Name;
                        worksheet.Cell(currentRow, 2).Value = item.Referred.Address;
                        worksheet.Cell(currentRow, 3).Value = item.type;
                        if (item.type == Common.Enums.ReferredType.In)
                        {
                            worksheet.Cell(currentRow, 3).Style.Fill.SetBackgroundColor(XLColor.Green);
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 3).Style.Fill.SetBackgroundColor(XLColor.Gray);
                        }
                        worksheet.Cell(currentRow, 4).Value = item.Service;
                        worksheet.Cell(currentRow, 5).Value = item.Client.Name;
                        worksheet.Cell(currentRow, 6).Value = item.Client.Code;
                        worksheet.Cell(currentRow, 7).Value = item.Client.AdmisionDate;
                        worksheet.Cell(currentRow, 8).Value = item.Client.Status;
                        if (item.Client.Status == Common.Enums.StatusType.Open)
                        {
                            worksheet.Cell(currentRow, 8).Style.Fill.SetBackgroundColor(XLColor.Green);
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 8).Style.Fill.SetBackgroundColor(XLColor.Red);
                        }

                        IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 8).Address);
                        rangeCurrent.Style.Font.FontSize = 11;
                        rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeCurrent.Style.Font.Bold = false;

                    }

                    worksheet.ColumnsUsed().AdjustToContents();
                    IXLRange range1 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 8).Address);
                    range1.Style.Font.FontSize = 18;
                    range1.Style.Font.Bold = false;
                    range1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    range1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range1.Merge();
                    IXLRange range2 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 8).Address);
                    range2.Style.Font.FontSize = 16;
                    range2.Style.Font.Bold = false;
                    range2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range2.Merge();
                    IXLRange range3 = worksheet.Range(worksheet.Cell(3, 1).Address, worksheet.Cell(3, 8).Address);
                    range3.Style.Font.FontSize = 14;
                    range3.Style.Font.Bold = false;
                    range3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range3.Merge();

                    
                }
            }
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = ConvertStreamToByteArray(stream);
                //return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                return stream.ToArray();
            }
        }

        public byte[] ExportBillTCMHelper(List<TCMNoteEntity> aTCMnotes, string Periodo, string ClinicName, string data)
        {
            var tcmNotes = aTCMnotes;
            int amount = 0;
            int unit_total = 0;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("All Services (" + aTCMnotes.Count() + " Notes)");
                worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                worksheet.Cell(2, 1).Value = ClinicName;
                worksheet.Cell(3, 2).Value = Periodo;
                worksheet.Cell(3, 13).Value = data;
                worksheet.Cell(3, 13).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(3, 13).Style.Font.FontSize = 16;
                worksheet.Cell(3, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(3, 1).Value = "SUPERBILL";
                worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.BlueGray;
                worksheet.Cell(3, 1).Style.Font.FontSize = 16;
                worksheet.Cell(3, 1).Style.Font.Bold = true;
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var currentRow = 4;
                worksheet.Cell(currentRow, 1).Value = "Client Name";
                worksheet.Cell(currentRow, 2).Value = "Case No";
                worksheet.Cell(currentRow, 3).Value = "DOB";
                worksheet.Cell(currentRow, 4).Value = "Medicaid Id";
                worksheet.Cell(currentRow, 5).Value = "Insurance | Member Id";
                worksheet.Cell(currentRow, 6).Value = "Diagnostics";
                worksheet.Cell(currentRow, 7).Value = "Date";
                worksheet.Cell(currentRow, 8).Value = "Service";
                worksheet.Cell(currentRow, 9).Value = "Minutes";
                worksheet.Cell(currentRow, 10).Value = "Units";
                worksheet.Cell(currentRow, 11).Value = "Amount";
                worksheet.Cell(currentRow, 12).Value = "CaseManager";
                worksheet.Cell(currentRow, 13).Value = "Status Bill";


                worksheet.Style.Font.Bold = true;
                IXLRange range = worksheet.Range(worksheet.Cell(4, 1).Address, worksheet.Cell(4, 13).Address);
                range.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                range.SetAutoFilter();
                currentRow++;

                List<string> codes = new List<string>();
                List<string> facilitators = new List<string>();

                foreach (var item in tcmNotes)
                {
                    int valor = 0;
                    int residuo = 0;
                    int valorAumentado = 0;

                    if (codes.Contains(item.TCMClient.Client.Code) == false)
                    {
                        codes.Add(item.TCMClient.Client.Code);
                    }
                    if (facilitators.Contains(item.TCMClient.Casemanager.Name) == false)
                    {
                        facilitators.Add(item.TCMClient.Casemanager.Name);
                    }

                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.TCMClient.Client.Name;
                    worksheet.Cell(currentRow, 2).Value = item.TCMClient.Client.Code;
                    worksheet.Cell(currentRow, 3).Value = item.TCMClient.Client.DateOfBirth.ToShortDateString();
                    worksheet.Cell(currentRow, 4).Value = item.TCMClient.Client.MedicaidID;
                    if (item.TCMClient.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
                    {
                        worksheet.Cell(currentRow, 5).Value = item.TCMClient.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name;
                        worksheet.Cell(currentRow, 5).Value += " | " + item.TCMClient.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 5).Value = "-";
                    }
                    if (item.TCMClient.Client.Clients_Diagnostics.Count() > 0)
                    {
                        worksheet.Cell(currentRow, 6).Value = item.TCMClient.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 6).Value = "-";
                    }
                    worksheet.Cell(currentRow, 7).Value = item.DateOfService.ToShortDateString();
                    worksheet.Cell(currentRow, 8).Value = item.TCMNoteActivity.Count();
                    worksheet.Cell(currentRow, 9).Value = item.TCMNoteActivity.Sum(n => n.Minutes);
                    int temp = 0;
                    int temp1 = 0;
                    foreach (var product in item.TCMNoteActivity)
                    {
                        valor = product.Minutes / 15;
                        residuo = product.Minutes % 15;
                        valorAumentado = valor + 1;

                        if (residuo > 7)
                        {
                            temp += valorAumentado;
                            temp1 += (valorAumentado * 12);
                            unit_total = unit_total + valorAumentado;
                            amount = amount + (valorAumentado * 12);

                        }
                        else
                        {
                            temp += valor;
                            temp1 += (valor * 12);
                            unit_total = unit_total + valor;
                            amount = amount + (valor * 12);
                        }

                    }
                    worksheet.Cell(currentRow, 10).Value = temp;
                    worksheet.Cell(currentRow, 11).Value = temp1;

                    IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 13).Address);
                    rangeCurrent.Style.Font.FontSize = 11;
                    rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeCurrent.Style.Font.Bold = false;

                    worksheet.Cell(5, 1).Value = codes.Count() + " Clients";
                    worksheet.Cell(5, 7).Value = tcmNotes.Count() + " Notes";
                    worksheet.Cell(5, 8).Value = tcmNotes.Sum(n => n.TCMNoteActivity.Count());
                    worksheet.Cell(5, 9).Value = tcmNotes.Sum(n => n.TCMNoteActivity.Sum(m => m.Minutes));
                    worksheet.Cell(5, 10).Value = unit_total;
                    worksheet.Cell(5, 11).Value = "$ " + amount;
                    worksheet.Cell(5, 12).Value = facilitators.Count() + " CaseManager";
                    IXLRange rangeTotal = worksheet.Range(worksheet.Cell(5, 1).Address, worksheet.Cell(5, 12).Address);
                    rangeTotal.Style.Font.FontSize = 13;
                    rangeTotal.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeTotal.Style.Font.Bold = true;
                    rangeTotal.Style.Font.FontColor = XLColor.GoldenBrown;

                    worksheet.Cell(currentRow, 12).Value = item.TCMClient.Casemanager.Name;
                    if (item.DeniedBill == true)
                    {
                        worksheet.Cell(currentRow, 13).Value = "Denied";
                        worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Red);
                    }
                    else
                    {
                        if (item.BilledDate != null && item.PaymentDate != null)
                        {
                            worksheet.Cell(currentRow, 13).Value = "Paid";
                            worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Green);
                        }
                        else
                        {
                            if (item.BilledDate != null && item.PaymentDate == null)
                            {
                                worksheet.Cell(currentRow, 13).Value = "Pending";
                                worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.BabyBlue);
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 13).Value = "Not Billed";
                                worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                            }
                        }
                    }


                }

                worksheet.ColumnsUsed().AdjustToContents();

                IXLRange range1 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 13).Address);
                range1.Style.Font.FontSize = 18;
                range1.Style.Font.Bold = false;
                range1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range1.Merge();
                IXLRange range2 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 13).Address);
                range2.Style.Font.FontSize = 16;
                range2.Style.Font.Bold = false;
                range2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range2.Merge();
                IXLRange range3 = worksheet.Range(worksheet.Cell(3, 2).Address, worksheet.Cell(3, 13).Address);
                range3.Style.Font.FontSize = 14;
                range3.Style.Font.Bold = false;
                range3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range3.Merge();

                int count = 0;
                foreach (var item in tcmNotes.GroupBy(n => n.TCMClient))
                {
                    int amount1 = 0;
                    int unit_total1 = 0;
                    if (item.Key.Client.Name.ToString().Length > 24)
                    {
                        count = workbook.Worksheets.Where(n => n.Name.Contains(item.Key.Client.Name.Substring(0, 24).ToString()) == true).Count();
                        if (count > 0)
                        {
                            count++;
                            worksheet = workbook.Worksheets.Add(item.Key.Client.Name.Substring(0, 24).ToString() + '-' + count.ToString() + '-' + item.Key.TCMNote.Sum(m => m.TCMNoteActivity.Count()).ToString());
                        }
                        else
                        {
                            worksheet = workbook.Worksheets.Add(item.Key.Client.Name.Substring(0, 24).ToString() + item.Key.TCMNote.Sum(m => m.TCMNoteActivity.Count()).ToString());
                        }
                    }
                    else
                    {
                        count = workbook.Worksheets.Where(n => n.Name.Contains(item.Key.Client.Name.ToString()) == true).Count();
                        if (count > 0)
                        {
                            count++;
                            worksheet = workbook.Worksheets.Add(item.Key.Client.Name.ToString() + '-' + count.ToString() + '-' + item.Key.TCMNote.Sum(m => m.TCMNoteActivity.Count()).ToString());
                        }
                        else
                        {
                            worksheet = workbook.Worksheets.Add(item.Key.Client.Name.ToString() + item.Key.TCMNote.Sum(m => m.TCMNoteActivity.Count()).ToString());
                        }

                    }

                    count = 0;

                    worksheet.Cells("A1").Value = "COMMUNITY HEALTH THERAPY CENTER. INC";
                    worksheet.Cell(2, 1).Value = ClinicName;
                    worksheet.Cell(3, 2).Value = Periodo;
                    worksheet.Cell(3, 13).Value = data;
                    worksheet.Cell(3, 13).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(3, 13).Style.Font.FontSize = 16;
                    worksheet.Cell(3, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(3, 1).Value = "SUPERBILL";
                    worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.BlueGray;
                    worksheet.Cell(3, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(3, 1).Style.Font.Bold = true;
                    worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    IXLRange range11 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 13).Address);
                    range11.Style.Font.FontSize = 18;
                    range11.Style.Font.Bold = false;
                    range11.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    range11.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range11.Merge();
                    IXLRange range21 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 13).Address);
                    range21.Style.Font.FontSize = 16;
                    range21.Style.Font.Bold = false;
                    range21.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range21.Merge();
                    IXLRange range31 = worksheet.Range(worksheet.Cell(3, 2).Address, worksheet.Cell(3, 12).Address);
                    range31.Style.Font.FontSize = 14;
                    range31.Style.Font.Bold = false;
                    range31.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range31.Merge();

                    currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Client Name";
                    worksheet.Cell(currentRow, 2).Value = "Case No";
                    worksheet.Cell(currentRow, 3).Value = "DOB";
                    worksheet.Cell(currentRow, 4).Value = "Medicaid Id";
                    worksheet.Cell(currentRow, 5).Value = "Insurance | Member Id";
                    worksheet.Cell(currentRow, 6).Value = "Diagnostics";
                    worksheet.Cell(currentRow, 7).Value = "Date";
                    worksheet.Cell(currentRow, 8).Value = "Setting";
                    worksheet.Cell(currentRow, 9).Value = "Minutes";
                    worksheet.Cell(currentRow, 10).Value = "Units";
                    worksheet.Cell(currentRow, 11).Value = "Amount";
                    worksheet.Cell(currentRow, 12).Value = "CaseManager";
                    worksheet.Cell(currentRow, 13).Value = "Status Bill";


                    IXLRange range0 = worksheet.Range(worksheet.Cell(4, 1).Address, worksheet.Cell(4, 13).Address);
                    range0.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    range0.SetAutoFilter();
                    range0.Style.Font.Bold = true;
                    currentRow++;
                    unit_total = 0;
                    amount = 0;
                    worksheet.ColumnsUsed().AdjustToContents();

                    codes = new List<string>();
                    facilitators = new List<string>();

                    foreach (var product in item)
                    {
                        int valor1 = 0;
                        int residuo1 = 0;
                        int valorAumentado1 = 0;

                        foreach (var activity in product.TCMNoteActivity)
                        {
                           

                            if (codes.Contains(product.TCMClient.CaseNumber) == false)
                            {
                                codes.Add(product.TCMClient.CaseNumber);
                            }
                            if (facilitators.Contains(product.TCMClient.Casemanager.Name) == false)
                            {
                                facilitators.Add(product.TCMClient.Casemanager.Name);
                            }

                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = product.TCMClient.Client.Name;
                            worksheet.Cell(currentRow, 2).Value = product.TCMClient.Client.Code;
                            worksheet.Cell(currentRow, 3).Value = product.TCMClient.Client.DateOfBirth.ToShortDateString();
                            worksheet.Cell(currentRow, 4).Value = product.TCMClient.Client.MedicaidID;
                            if (product.TCMClient.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
                            {
                                worksheet.Cell(currentRow, 5).Value = product.TCMClient.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name;
                                worksheet.Cell(currentRow, 5).Value += " | " + product.TCMClient.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId;
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 5).Value = "-";
                            }
                            if (product.TCMClient.Client.Clients_Diagnostics.Count() > 0)
                            {
                                worksheet.Cell(currentRow, 6).Value = product.TCMClient.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code;
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 6).Value = "-";
                            }
                            worksheet.Cell(currentRow, 7).Value = product.DateOfService.ToShortDateString();
                            worksheet.Cell(currentRow, 8).Value = activity.Setting;
                            worksheet.Cell(currentRow, 9).Value = activity.Minutes;

                            valor1 = activity.Minutes / 15;
                            residuo1 = activity.Minutes % 15;
                            valorAumentado1 = valor1 + 1;

                            if (residuo1 > 7)
                            {
                                worksheet.Cell(currentRow, 10).Value = valorAumentado1;
                                worksheet.Cell(currentRow, 11).Value = valorAumentado1 * 12;
                                unit_total1 = unit_total1 + valorAumentado1;
                                amount1 = amount1 + (valorAumentado1 * 12);

                            }
                            else
                            {
                                worksheet.Cell(currentRow, 10).Value = valor1;
                                worksheet.Cell(currentRow, 11).Value = valor1 * 12;
                                unit_total1 = unit_total1 + valor1;
                                amount1 = amount1 + (valor1 * 12);
                            }

                            worksheet.Cell(currentRow, 12).Value = product.TCMClient.Casemanager.Name;
                            if (product.DeniedBill == true)
                            {
                                worksheet.Cell(currentRow, 13).Value = "Denied";
                                worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Red);
                            }
                            else
                            {
                                if (product.BilledDate != null && product.PaymentDate != null)
                                {
                                    worksheet.Cell(currentRow, 13).Value = "Paid";
                                    worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.Green);
                                }
                                else
                                {
                                    if (product.BilledDate != null && product.PaymentDate == null)
                                    {
                                        worksheet.Cell(currentRow, 13).Value = "Pending";
                                        worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.BabyBlue);
                                    }
                                    else
                                    {
                                        worksheet.Cell(currentRow, 13).Value = "Not Billed";
                                        worksheet.Cell(currentRow, 13).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                                    }
                                }
                            }

                        }

                        IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 13).Address);
                        rangeCurrent.Style.Font.FontSize = 11;
                        rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeCurrent.Style.Font.Bold = false;

                        worksheet.Cell(5, 1).Value = codes.Count() + " Client";
                        worksheet.Cell(5, 7).Value = item.Sum(m => m.TCMNoteActivity.Count()) + " Services";
                        worksheet.Cell(5, 9).Value = item.Sum(n => n.TCMNoteActivity.Sum(m => m.Minutes));
                        worksheet.Cell(5, 10).Value = unit_total1;
                        worksheet.Cell(5, 11).Value = "$ " + amount1;
                        worksheet.Cell(5, 12).Value = facilitators.Count() + " CaseManager";
                        IXLRange rangeTotal = worksheet.Range(worksheet.Cell(5, 1).Address, worksheet.Cell(5, 12).Address);
                        rangeTotal.Style.Font.FontSize = 12;
                        rangeTotal.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeTotal.Style.Font.Bold = true;
                        rangeTotal.Style.Font.FontColor = XLColor.GoldenBrown;
                    }
                   
                }
                worksheet.ColumnsUsed().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = ConvertStreamToByteArray(stream);
                    //return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                    return stream.ToArray();
                }
            }
        }

        public byte[] ExportBillDmsHelper(BillDmsEntity abillDms, string Periodo, string ClinicName, string data)
        {
            var billDms = abillDms;
            int amount = 0;
            int unit_total = 0;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DMS Invoice");
                var currentRow = 4;

                worksheet.Cells("A1").Value = "DMS System INVOICE " + billDms.Id;
                worksheet.Cells("A1").Style.Font.Bold = true;
                worksheet.Cells("A1").Style.Font.FontColor = XLColor.Gold;
                worksheet.Cells("A1").Style.Fill.SetBackgroundColor(XLColor.Black);
                worksheet.Cell("A1").Style.Font.FontSize = 20;
                worksheet.Cell("A3").Value = ClinicName;
                worksheet.Cell("A3").Style.Font.FontColor = XLColor.Black;
                worksheet.Cell("A3").Style.Font.Bold = true;
                worksheet.Cell("A3").Style.Font.FontSize = 16;
                worksheet.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("A6").Value = "Invoice Date: " + billDms.DateBill.ToShortDateString();
                worksheet.Cell("A6").Style.Font.FontColor = XLColor.Black;
                worksheet.Cell("A7").Value = Periodo;
                worksheet.Cell("A7").Style.Font.FontColor = XLColor.Black;

                currentRow = 11;
                worksheet.Cell(currentRow, 1).Value = "ITEM DESCRIPTION";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 2).Value = "SERVICES";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 3).Value = "NOTES";
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 4).Value = "UNITS";
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 5).Value = "RATE";
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 6).Value = "AMOUNT";
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                IXLRange range0 = worksheet.Range(worksheet.Cell(11, 1).Address, worksheet.Cell(11, 6).Address);
                range0.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                range0.Style.Font.Bold = true;
                currentRow++;
                unit_total = 0;
                amount = 0;
                worksheet.ColumnsUsed().AdjustToContents();

                worksheet.Cell(currentRow, 1).Value = "(* Units to be invoiced)";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 2).Value = "CMH";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 3).Value = billDms.BillDmsDetails.Where(n => n.ServiceAgency == Common.Enums.ServiceAgency.CMH && n.StatusBill != Common.Enums.StatusBill.Pending).Count();
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 4).Value = billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.CMH && m.StatusBill != Common.Enums.StatusBill.Pending).Sum(n => n.Unit);
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 5).Value = "0.20";
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 6).Value = "$ " + billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.CMH && m.StatusBill != Common.Enums.StatusBill.Pending).Sum(n => n.Amount);
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right ;

                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "(* Units pending to be invoiced)";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 2).Value = "CMH";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 3).Value = billDms.BillDmsDetails.Where(n => n.ServiceAgency == Common.Enums.ServiceAgency.CMH && n.StatusBill == Common.Enums.StatusBill.Pending).Count();
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 4).Value = billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.CMH && m.StatusBill == Common.Enums.StatusBill.Pending).Sum(n => n.Unit);
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 5).Value = "0.20";
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 6).Value = "$ " + billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.CMH && m.StatusBill == Common.Enums.StatusBill.Pending).Sum(n => n.Amount);
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "(* Units to be invoiced)";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 2).Value = "TCM";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 3).Value = billDms.BillDmsDetails.Where(n => n.ServiceAgency == Common.Enums.ServiceAgency.TCM && n.StatusBill != Common.Enums.StatusBill.Pending).Count();
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 4).Value = billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.TCM && m.StatusBill != Common.Enums.StatusBill.Pending).Sum(n => n.Unit);
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 5).Value = "0.20";
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 6).Value = "$ " + billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.TCM && m.StatusBill != Common.Enums.StatusBill.Pending).Sum(n => n.Amount);
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "(* Units pending to be invoiced)";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 2).Value = "TCM";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 3).Value = billDms.BillDmsDetails.Where(n => n.ServiceAgency == Common.Enums.ServiceAgency.TCM && n.StatusBill == Common.Enums.StatusBill.Pending).Count();
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 4).Value = billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.TCM && m.StatusBill == Common.Enums.StatusBill.Pending).Sum(n => n.Unit);
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 5).Value = "0.20";
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 6).Value = "$ " + billDms.BillDmsDetails.Where(m => m.ServiceAgency == Common.Enums.ServiceAgency.TCM && m.StatusBill == Common.Enums.StatusBill.Pending).Sum(n => n.Amount);
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                IXLRange range11 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 6).Address);
                range11.Style.Font.FontSize = 20;
                range11.Style.Font.Bold = true;
                range11.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range11.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range11.Merge();
                IXLRange range21 = worksheet.Range(worksheet.Cell(3, 1).Address, worksheet.Cell(3, 6).Address);
                range21.Style.Font.FontSize = 16;
                range21.Style.Font.Bold = true;
                range21.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range21.Merge();
                IXLRange range31 = worksheet.Range(worksheet.Cell(6, 1).Address, worksheet.Cell(6, 6).Address);
                range31.Style.Font.FontSize = 14;
                range31.Style.Font.Bold = false;
                range31.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                range31.Merge();
                IXLRange range41 = worksheet.Range(worksheet.Cell(7, 1).Address, worksheet.Cell(7, 6).Address);
                range41.Style.Font.FontSize = 14;
                range41.Style.Font.Bold = false;
                range41.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                range41.Merge();

                currentRow++;
                currentRow++;
                currentRow++;
                currentRow++;
                currentRow++;
                currentRow++;
                currentRow++;
                currentRow++;

                worksheet.Cell(currentRow, 3).Value = "SUBTOTAL";
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 3).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 6).Value = "$ " + billDms.BillDmsDetails.Sum(n => n.Amount);
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(currentRow, 6).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

                IXLRange range51 = worksheet.Range(worksheet.Cell(currentRow, 3).Address, worksheet.Cell(currentRow, 5).Address);
                range51.Merge();
                currentRow++;

                worksheet.Cell(currentRow, 3).Value = "TAX";
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 3).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 6).Value = "$ 0.00";
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(currentRow, 6).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

                IXLRange range61 = worksheet.Range(worksheet.Cell(currentRow, 3).Address, worksheet.Cell(currentRow, 5).Address);
                range61.Merge();
                currentRow++;

                worksheet.Cell(currentRow, 3).Value = "TOTAL";
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 3).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 6).Value = "$ " + billDms.BillDmsDetails.Sum(n => n.Amount);
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(currentRow, 6).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 6).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 6).Style.Fill.SetBackgroundColor(XLColor.LightGray);

                IXLRange range71 = worksheet.Range(worksheet.Cell(currentRow, 3).Address, worksheet.Cell(currentRow, 5).Address);
                range71.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                range71.Merge();

                worksheet.Cells("A35").Value = "(*) Total units are calculated based on Medicaid codes. ";
                worksheet.Cells("A36").Value = "Documents that are not notes are not included in the total units.";

                IXLRange range110 = worksheet.Range(worksheet.Cell(35, 1).Address, worksheet.Cell(35, 6).Address);
                range110.Style.Font.FontSize = 10;
                range110.Style.Font.Bold = false;
                range110.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range110.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range110.Merge();

                IXLRange range111 = worksheet.Range(worksheet.Cell(36, 1).Address, worksheet.Cell(36, 6).Address);
                range111.Style.Font.FontSize = 10;
                range111.Style.Font.Bold = false;
                range111.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range111.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range111.Merge();

                //-----------------------------------------------

                worksheet = workbook.Worksheets.Add("Invoice Details");
                worksheet.Cells("A1").Value = "DMS System INVOICE " + billDms.Id;
                worksheet.Cells("A1").Style.Font.Bold = true;
                worksheet.Cells("A1").Style.Font.FontColor = XLColor.Gold;
                worksheet.Cells("A1").Style.Fill.SetBackgroundColor(XLColor.Black);
                worksheet.Cell("A1").Style.Font.FontSize = 20;
                worksheet.Cell("A2").Value = ClinicName;
                worksheet.Cell("A2").Style.Font.FontColor = XLColor.Black;
                worksheet.Cell("A2").Style.Font.Bold = true;
                worksheet.Cell("A2").Style.Font.FontSize = 16;
                worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("A3").Value = "Invoice Date: " + billDms.DateBill.ToShortDateString();
                worksheet.Cell("A3").Style.Font.FontColor = XLColor.Black;
                worksheet.Cell("A4").Value = Periodo;
                worksheet.Cell("A4").Style.Font.FontColor = XLColor.Black;
               
                currentRow = 5;
                worksheet.Cell(currentRow, 1).Value = "Client Name";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 2).Value = "Service Date";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 3).Value = "Service";
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 4).Value = "Units";
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 5).Value = "Amount";
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(currentRow, 6).Value = "Amount";
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                worksheet.Style.Font.Bold = true;
                IXLRange range = worksheet.Range(worksheet.Cell(5, 1).Address, worksheet.Cell(5, 6).Address);
                range.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                range.SetAutoFilter();
                currentRow++;

                //Totales
                worksheet.Cell(currentRow, 1).Value = "CLients (" + billDms.BillDmsDetails.GroupBy(n => n.NameClient).Count() + ")";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 2).Value = "Notes (" + billDms.BillDmsDetails.Count() +")";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 3).Value = "Services (" + billDms.BillDmsDetails.GroupBy(n => n.ServiceAgency).Count() + ")";
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 4).Value = "Units (" + billDms.BillDmsDetails.Sum(n => n.Unit) + ")";
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 5).Value = "$ " + billDms.BillDmsDetails.Where(n => n.StatusBill == Common.Enums.StatusBill.Pending).Sum(n => n.Amount);
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(currentRow, 6).Value = "$ " + billDms.BillDmsDetails.Where(n => n.StatusBill != Common.Enums.StatusBill.Pending).Sum(n => n.Amount);
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                IXLRange rangeTotal = worksheet.Range(worksheet.Cell(6, 1).Address, worksheet.Cell(6, 6).Address);
                rangeTotal.Style.Font.FontColor = XLColor.GoldenBrown;
                rangeTotal.Style.Font.FontSize = 12;
                range.Style.Font.Bold = true;
                currentRow++;

                foreach (var item in billDms.BillDmsDetails.OrderBy(n => n.NameClient).ThenBy(m => m.DateService))
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.NameClient;
                    worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(currentRow, 2).Value = item.DateService;
                    worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(currentRow, 3).Value = item.ServiceAgency;
                    worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(currentRow, 4).Value = item.Unit;
                    worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    if (item.StatusBill == Common.Enums.StatusBill.Pending)
                    {
                        worksheet.Cell(currentRow, 5).Value = "$ " + item.Amount;
                        worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(currentRow, 5).Style.Font.FontColor = XLColor.Red;
                        worksheet.Cell(currentRow, 6).Value = "$ 0.00" ;
                        worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 5).Value = "$ 0.00";
                        worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(currentRow, 6).Value = "$ " + item.Amount;
                        worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    

                    IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 6).Address);
                    rangeCurrent.Style.Font.FontSize = 11;
                    rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeCurrent.Style.Font.Bold = false;
                   
                }

                worksheet.ColumnsUsed().AdjustToContents();

                IXLRange range1 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 6).Address);
                range1.Style.Font.FontSize = 20;
                range1.Style.Font.Bold = true;
                range1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range1.Merge();
                IXLRange range2 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 6).Address);
                range2.Style.Font.FontSize = 16;
                range2.Style.Font.Bold = true;
                range2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range2.Merge();
                IXLRange range3 = worksheet.Range(worksheet.Cell(3, 1).Address, worksheet.Cell(3, 6).Address);
                range3.Style.Font.FontSize = 14;
                range3.Style.Font.Bold = false;
                range3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                range3.Merge();
                IXLRange range4 = worksheet.Range(worksheet.Cell(4, 1).Address, worksheet.Cell(4, 6).Address);
                range4.Style.Font.FontSize = 14;
                range4.Style.Font.Bold = false;
                range4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                range4.Merge();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = ConvertStreamToByteArray(stream);
                    //return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                    return stream.ToArray();
                }
            }
        }

        #endregion

        #region Utils functions
        public byte[] ConvertStreamToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
        #endregion
    }
}
