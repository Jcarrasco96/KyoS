using ClosedXML.Excel;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                    if (item.DeniedBill == true)
                    {
                        worksheet.Cell(currentRow, 12).Value = "Denied";
                        worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.Red);
                    }
                    else
                    {
                        if (item.BilledDate != null && item.PaymentDate != null)
                        {
                            worksheet.Cell(currentRow, 12).Value = "Paid";
                            worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.Green);
                        }
                        else
                        {
                            if (item.BilledDate != null && item.PaymentDate == null)
                            {
                                worksheet.Cell(currentRow, 12).Value = "Pending";
                                worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.BabyBlue);
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 12).Value = "Not Billed";
                                worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                            }
                        }
                    }


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
                    worksheet.Cell(currentRow, 4).Value = "Medicaid Id";
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
                        if (product.DeniedBill == true)
                        {
                            worksheet.Cell(currentRow, 12).Value = "Denied";
                            worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.Red);
                        }
                        else
                        {
                            if (product.BilledDate != null && product.PaymentDate != null)
                            {
                                worksheet.Cell(currentRow, 12).Value = "Paid";
                                worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.Green);
                            }
                            else
                            {
                                if (product.BilledDate != null && product.PaymentDate == null)
                                {
                                    worksheet.Cell(currentRow, 12).Value = "Pending";
                                    worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.BabyBlue);
                                }
                                else
                                {
                                    worksheet.Cell(currentRow, 12).Value = "Not Billed";
                                    worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                                }
                            }
                        }

                        IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 12).Address);
                        rangeCurrent.Style.Font.FontSize = 11;
                        rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        rangeCurrent.Style.Font.Bold = false;

                        worksheet.Cell(5, 1).Value = codes.Count() + " Client";
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
                worksheet.Cell(currentRow, 13).Value = "Telephone";
                worksheet.Cell(currentRow, 14).Value = "Race";
                worksheet.Cell(currentRow, 15).Value = "Preferred Language";
                worksheet.Cell(currentRow, 16).Value = "Date Close";


                worksheet.Style.Font.Bold = true;
                IXLRange range = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 16).Address);
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
                    worksheet.Cell(currentRow, 13).Value = item.Telephone;
                    worksheet.Cell(currentRow, 14).Value = item.Race;
                    worksheet.Cell(currentRow, 15).Value = item.PreferredLanguage;
                    worksheet.Cell(currentRow, 16).Value = item.DateOfClose.ToShortDateString();

                    if (item.DateOfClose.DayOfYear == 1 && item.Status == Common.Enums.StatusType.Close)
                    {
                        worksheet.Cell(currentRow, 16).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    }

                    IXLRange rangeCurrent = worksheet.Range(worksheet.Cell(currentRow, 1).Address, worksheet.Cell(currentRow, 16).Address);
                    rangeCurrent.Style.Font.FontSize = 11;
                    rangeCurrent.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeCurrent.Style.Font.Bold = false;

                }

                worksheet.ColumnsUsed().AdjustToContents();
                IXLRange range1 = worksheet.Range(worksheet.Cell(1, 1).Address, worksheet.Cell(1, 16).Address);
                range1.Style.Font.FontSize = 18;
                range1.Style.Font.Bold = false;
                range1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range1.Merge();
                IXLRange range2 = worksheet.Range(worksheet.Cell(2, 1).Address, worksheet.Cell(2, 16).Address);
                range2.Style.Font.FontSize = 16;
                range2.Style.Font.Bold = false;
                range2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range2.Merge();
                IXLRange range3 = worksheet.Range(worksheet.Cell(3, 1).Address, worksheet.Cell(3, 16).Address);
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
