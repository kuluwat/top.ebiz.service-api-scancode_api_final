
using System.Text.Json;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using top.ebiz.service.Service.Traveler_Profile;
using top.ebiz.service.Models.Traveler_Profile;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static top.ebiz.service.Service.Report.ClassDataReportCreateTrip;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;


namespace top.ebiz.service.Service.Report
{
    public class ClassDataReportTravelerProfile
    {
        public string ReportTravelRecords(DataTable dtParam, string strParam)
        {
            string JSONresult = "";
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");
            dtResult.Rows.Add(dtResult.NewRow());

            var value = new TravelRecordModel
            {
                token_login = "",
                doc_id = "",
                country = "",
                date_from = "",
                date_to = "",
                travel_type = "",
                emp_id = "",
                section = "",
                department = "",
                function = "",
                travel_list = new List<traveltypeList>()
            };

            if (dtParam != null && dtParam.Rows.Count > 0)
            {
                var dr = dtParam.Rows[0];

                // Map basic properties

                value.token_login = dr["token_login"]?.ToString() ?? "";
                value.doc_id = dr["doc_id"]?.ToString() ?? "";
                value.country = dr["country"]?.ToString() ?? "";
                value.date_from = dr["date_from"]?.ToString() ?? "";
                value.date_to = dr["date_to"]?.ToString() ?? "";
                value.travel_type = dr["travel_type"]?.ToString() ?? "";
                value.emp_id = dr["emp_id"]?.ToString() ?? "";
                value.section = dr["section"]?.ToString() ?? "";
                value.department = dr["department"]?.ToString() ?? "";
                value.function = dr["function"]?.ToString() ?? "";

                // Handle travel_list
                if (dr["travel_list"] != null && !string.IsNullOrEmpty(dr["travel_list"].ToString()))
                {
                    try
                    {
                        var travelListJson = dr["travel_list"].ToString();
                        value.travel_list = JsonConvert.DeserializeObject<List<traveltypeList>>(travelListJson)
                                            ?? new List<traveltypeList>();
                    }
                    catch
                    {
                        value.travel_list = new List<traveltypeList>();
                    }
                }

            }

            searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
            TravelRecordOutModel dataX = service.SearchTravelRecord(value);

            if (dataX != null && dataX.travelrecord != null && dataX.travelrecord.Count > 0)
            {
                try
                {
                    string ret = "";

                    ExcelPackage ExcelPkg = new ExcelPackage();
                    ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.Add("Travel Record (E-BIZ)");

                    // Hide gridline
                    worksheet.View.ShowGridLines = false;
                    string datetime = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

                    #region Set column width & text wrap
                    worksheet.Column(1).Width = 8;
                    worksheet.Column(2).Width = 10;
                    worksheet.Column(3).Width = 8;
                    worksheet.Column(4).Width = 33;
                    worksheet.Column(5).Width = 10;
                    worksheet.Column(6).Width = 10;
                    worksheet.Column(7).Width = 10;
                    worksheet.Column(8).Width = 13;
                    worksheet.Column(9).Width = 13;
                    worksheet.Column(10).Width = 40;
                    worksheet.Column(11).Width = 15;
                    worksheet.Column(12).Width = 13;
                    worksheet.Column(13).Width = 13;
                    worksheet.Column(14).Width = 13;
                    worksheet.Column(15).Width = 13;
                    worksheet.Column(16).Width = 13;
                    worksheet.Column(17).Width = 13;
                    worksheet.Column(18).Width = 13;
                    worksheet.Column(19).Width = 13;
                    worksheet.Column(20).Width = 16;
                    worksheet.Column(21).Width = 16;
                    worksheet.Column(22).Width = 16;
                    worksheet.Column(23).Width = 16;
                    worksheet.Column(24).Width = 16;
                    worksheet.Column(25).Width = 16;
                    worksheet.Column(26).Width = 16;
                    worksheet.Column(27).Width = 16;
                    worksheet.Column(28).Width = 16;
                    worksheet.Column(29).Width = 16;
                    worksheet.Column(30).Width = 16;
                    worksheet.Column(31).Width = 16;
                    worksheet.Column(32).Width = 16;
                    for (int col = 1; col <= 32; col++)
                    {
                        worksheet.Column(col).Style.WrapText = true;
                    }
                    #endregion

                    Color colFirstHeaderHex = System.Drawing.ColorTranslator.FromHtml("#808080");
                    string fontName = "Cordia New";

                    string date_type = dtParam.Rows[0]["travel_type"].ToString() == "ob" || dtParam.Rows[0]["travel_type"].ToString() == "lb"
                        ? "Business date"
                        : dtParam.Rows[0]["travel_type"].ToString() == "ot" || dtParam.Rows[0]["travel_type"].ToString() == "lt"
                            ? "Training date"
                            : "Date";

                    // Header
                    worksheet.Cells["A1"].Value = "Travel Record";
                    worksheet.Cells["A1"].Style.Font.Size = 24;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.Font.Name = fontName;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(colFirstHeaderHex);
                    worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells["A1:AF1"].Merge = true;
                    worksheet.Row(1).Height = 40;

                    worksheet.Cells["A2"].Value = "Print date : " + datetime;
                    worksheet.Cells["A2"].Style.Font.Size = 10;
                    worksheet.Cells["A2"].Style.Font.Bold = true;
                    worksheet.Cells["A2"].Style.Font.Name = fontName;
                    worksheet.Cells["A2:AF2"].Merge = true;

                    worksheet.Cells["A3"].Value = "Travel Type : " + dtParam.Rows[0]["travel_type"].ToString() +
                        ", Country : " + dtParam.Rows[0]["country"].ToString() +
                        ", " + date_type + " : " + dtParam.Rows[0]["date_from"].ToString() +
                        " - " + dtParam.Rows[0]["date_to"].ToString() +
                        ", Employee : " + dtParam.Rows[0]["emp_id"].ToString() +
                        ", Section : " + dtParam.Rows[0]["section"].ToString() +
                        ", Department : " + dtParam.Rows[0]["department"].ToString() +
                        ", Function : " + dtParam.Rows[0]["function"].ToString();
                    worksheet.Cells["A3"].Style.Font.Size = 10;
                    worksheet.Cells["A3"].Style.Font.Bold = true;
                    worksheet.Cells["A3"].Style.Font.Name = fontName;
                    worksheet.Cells["A3:AF3"].Merge = true;

                    Color colEmpHex = System.Drawing.ColorTranslator.FromHtml("#1F4E78");
                    Color colTravelHex = System.Drawing.ColorTranslator.FromHtml("#FFD966");
                    Color colTrainingHex = System.Drawing.ColorTranslator.FromHtml("#375623");
                    Color colBorderHex = System.Drawing.ColorTranslator.FromHtml("#D9D9D9");
                    int fontSize = 12;

                    #region HEADER TABLE
                    // Employee Information Header
                    worksheet.Cells["A4"].Value = "Employee Information";
                    worksheet.Cells["A4"].Style.Font.Size = fontSize;
                    worksheet.Cells["A4"].Style.Font.Bold = true;
                    worksheet.Cells["A4"].Style.Font.Name = fontName;
                    worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["A4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#D6DCE4"));
                    worksheet.Cells["A4"].Style.Font.Color.SetColor(Color.Black);
                    worksheet.Cells["A4:G4"].Merge = true;

                    // Travel Details Header
                    worksheet.Cells["H4"].Value = "Travel Details";
                    worksheet.Cells["H4"].Style.Font.Size = fontSize;
                    worksheet.Cells["H4"].Style.Font.Bold = true;
                    worksheet.Cells["H4"].Style.Font.Name = fontName;
                    worksheet.Cells["H4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["H4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["H4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["H4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFF2CC"));
                    worksheet.Cells["H4"].Style.Font.Color.SetColor(Color.Black);
                    worksheet.Cells["H4:S4"].Merge = true;

                    // Training Expense Header
                    worksheet.Cells["T4"].Value = "Training Expense";
                    worksheet.Cells["T4"].Style.Font.Size = fontSize;
                    worksheet.Cells["T4"].Style.Font.Bold = true;
                    worksheet.Cells["T4"].Style.Font.Name = fontName;
                    worksheet.Cells["T4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["T4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["T4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["T4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#A9D08E"));
                    worksheet.Cells["T4"].Style.Font.Color.SetColor(Color.Black);
                    worksheet.Cells["T4:AF4"].Merge = true;

                    // Column Headers
                    string[] headers = {
                "No", "Emp ID", "Title", "Name", "Section", "Department", "Function",
                "Travel Status", "In-House", "Travel Topic", "Country", "City / Province",
                "Business / Training Date [From]", "Business / Training Date[To]", "Duration (day)",
                "Estimate Expense (BHT)", "GL Account", "Cost Center", "Order / WBS",
                "Accommodation", "Air Ticket", "Allowance_Day", "Allowance_Night",
                "Clothing & Luggage", "Course Fee", "Instruction Fee", "Miscellaneous",
                "Passport", "Transportation", "Visa (Fee)", "Travel Insurance", "Total"
            };

                    for (int col = 1; col <= headers.Length; col++)
                    {
                        using (ExcelRange Rng = worksheet.Cells[5, col])
                        {
                            Rng.Value = headers[col - 1];
                            Rng.Style.Font.Size = fontSize;
                            Rng.Style.Font.Name = fontName;
                            Rng.Style.WrapText = true;
                            Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                            if (col <= 7)
                            {
                                Rng.Style.Font.Color.SetColor(Color.White);
                                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);
                            }
                            else if (col <= 19)
                            {
                                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);
                            }
                            else
                            {
                                Rng.Style.Font.Color.SetColor(Color.White);
                                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);
                            }
                        }
                    }

                    worksheet.Cells["A5:AF5"].AutoFilter = true;
                    #endregion

                    int startRow = 6;
                    int recordCount = dataX.travelrecord.Count;

                    for (int i = 0; i < recordCount; i++)
                    {
                        var drX = dataX.travelrecord[i];

                        // Employee Information
                        worksheet.Cells[startRow + i, 1].Value = i + 1;
                        worksheet.Cells[startRow + i, 2].Value = drX.emp_id?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 3].Value = drX.emp_title?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 4].Value = drX.emp_name?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 5].Value = drX.section?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 6].Value = drX.department?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 7].Value = drX.function?.ToString() ?? "";

                        // Travel Details
                        worksheet.Cells[startRow + i, 8].Value = drX.travel_status?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 9].Value = drX.in_house?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 10].Value = drX.travel_topic?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 11].Value = drX.country?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 12].Value = drX.city_province?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 13].Value = drX.date_from?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 14].Value = drX.date_to?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 15].Value = drX.duration?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 16].Value = drX.estimate_expense?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 17].Value = drX.gl_account?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 18].Value = drX.cost_center?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 19].Value = drX.order_wbs?.ToString() ?? "";

                        // Training Expense
                        worksheet.Cells[startRow + i, 20].Value = drX.accommodation?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 21].Value = drX.air_ticket?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 22].Value = drX.allowance_day?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 23].Value = drX.allowance_night?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 24].Value = drX.clothing_luggage?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 25].Value = drX.course_fee?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 26].Value = drX.instruction_fee?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 27].Value = drX.miscellaneous?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 28].Value = drX.passport?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 29].Value = drX.transportation?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 30].Value = drX.visa_fee?.ToString() ?? "";
                        worksheet.Cells[startRow + i, 31].Value = drX.travel_insurance?.ToString() ?? ""; // New column
                        worksheet.Cells[startRow + i, 32].Value = drX.total?.ToString() ?? "";

                        // Apply common styling to all cells in the row
                        for (int col = 1; col <= 32; col++)
                        {
                            using (ExcelRange Rng = worksheet.Cells[startRow + i, col])
                            {
                                Rng.Style.Font.Size = fontSize;
                                Rng.Style.Font.Name = fontName;
                                Rng.Style.WrapText = true;
                                Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                                Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                                if (col <= 7)
                                {
                                    Rng.Style.HorizontalAlignment = col == 4 ? ExcelHorizontalAlignment.Left : ExcelHorizontalAlignment.Center;
                                }
                                else if (col <= 19)
                                {
                                    Rng.Style.HorizontalAlignment = (col == 8 || col == 9 || col == 10) ?
                                        ExcelHorizontalAlignment.Left : ExcelHorizontalAlignment.Center;
                                }
                                else
                                {
                                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                }
                            }
                        }

                        // Add bottom border for the last row
                        if (i == recordCount - 1)
                        {
                            using (ExcelRange Rng = worksheet.Cells[startRow + i, 1, startRow + i, 32])
                            {
                                Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                Rng.Style.Border.Bottom.Color.SetColor(colBorderHex);
                            }
                        }
                    }

                    var datetime1 = DateTime.Now.ToString("yyyyMMddHHmm");
                    string fileName = $"EBIZ_TRAVEL_RECORD_{datetime1}.xlsx";
                    var output = ClassPathReport.genFilePath("temp", fileName);
                    var outputUrl = ClassPathReport.genFullPath("temp", fileName);

                    ExcelPkg.SaveAs(new FileInfo(output));

                    dtResult.Rows[0]["status"] = "true";
                    dtResult.Rows[0]["file_system_path"] = output;
                    dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                    dtResult.Rows[0]["file_outbound_name"] = fileName;
                }
                catch (Exception ex)
                {
                    dtResult.Rows[0]["status"] = "Error: " + ex.Message;
                    dtResult.Rows[0]["file_system_path"] = "";
                    dtResult.Rows[0]["file_outbound_path"] = "";
                    dtResult.Rows[0]["file_outbound_name"] = "";
                }
            }
            else
            {
                dtResult.Rows[0]["status"] = "No travel records found";
                dtResult.Rows[0]["file_system_path"] = "";
                dtResult.Rows[0]["file_outbound_path"] = "";
                dtResult.Rows[0]["file_outbound_name"] = "";
            }

            dtResult.TableName = "dtResult";
            DataSet ds = new DataSet();
            ds.Tables.Add(dtResult);
            JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

            return JSONresult;
        }
        public DataTable ReportISOSRecords(string token, string year)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");
            dtResult.Rows.Add(dtResult.NewRow());

            var value = new ExportRecordModel
            {
                token_login = token,
                year = year,
                after_trip = new afterTripModel()
            };
            ExportReportService service = new ExportReportService();
            ReportISOSRecordOutModel dataX = service.report_isos_member_list_record(value);

            if (dataX != null)
            {
                var datetime1 = DateTime.Now.ToString("yyyyMMddHHmmssff");
                string datenow = datetime1;
                string fileName = $"ISOS_MEMBER_LIST_RECORD_{year.ToUpper().Trim()}_{datetime1}.xlsx";
                var output = ClassPathReport.genFilePath("temp", fileName);
                var outputUrl = ClassPathReport.genFullPath("temp", fileName);

                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
                //HttpContext.Current.Server.MapPath(@"~/template/TEMPLATE_ISOS_MEMBER_LIST_RECORD.xlsx")
                var outputTemplate = ClassPathReport.genFilePath("template", "TEMPLATE_ISOS_MEMBER_LIST_RECORD.xlsx");

                var status = "false";
                var msg = "";
                try
                {
                    FileInfo template = new FileInfo(outputTemplate);
                    if (template != null && template.Exists)
                    {
                        //copy file temp to new file
                        using (var package = new ExcelPackage(template))
                        {
                            // save file temp ก่อนแล้วค่อยเขียนลง file temp
                            package.SaveAs(new FileInfo(output));
                        }

                        template = new FileInfo(output);
                        using (var ExcelPkg = new ExcelPackage(template))
                        {
                            ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.First();
                            string datetime = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

                            #region Header detail

                            worksheet.Cells["C2"].Value = "ISOS MEMBER LIST OF " + year;
                            worksheet.Cells["C3"].Value = "UPDATE " + datetime;

                            #endregion Header detail

                            int count_summary = dataX.details_list.Count;
                            worksheet.InsertRow(7, (count_summary - 1), 7); // insert row travel summary

                            int firstRow_summary = 7;

                            foreach (var dr in dataX.details_list)
                            {
                                worksheet.Cells["C" + firstRow_summary.ToString()].Value = dr.no;
                                worksheet.Cells["D" + firstRow_summary.ToString()].Value = dr.type_of_travel;
                                worksheet.Cells["E" + firstRow_summary.ToString()].Value = dr.emp_id;
                                worksheet.Cells["F" + firstRow_summary.ToString()].Value = dr.emp_display;
                                worksheet.Cells["G" + firstRow_summary.ToString()].Value = dr.emp_section;
                                worksheet.Cells["H" + firstRow_summary.ToString()].Value = dr.emp_department;
                                worksheet.Cells["I" + firstRow_summary.ToString()].Value = dr.emp_function;

                                firstRow_summary++;
                            }

                            worksheet.Name = "ISOS MEMBER LIST OF " + year;
                            ExcelPkg.SaveAs(new FileInfo(output));
                        }
                    }
                    status = "true";
                }
                catch (Exception ex) { msg = ex.Message.ToString(); }

                dtResult.Rows[0]["status"] = status;
                dtResult.Rows[0]["file_system_path"] = output;
                dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                dtResult.Rows[0]["file_outbound_name"] = (string.IsNullOrEmpty(msg) ? fileName : msg);
            }
            else
            {
                dtResult.Rows[0]["status"] = "false";
                dtResult.Rows[0]["file_system_path"] = "";
                dtResult.Rows[0]["file_outbound_path"] = "";
                dtResult.Rows[0]["file_outbound_name"] = "";
            }

            return dtResult;
        }

        public DataTable ReportInsuranceRecords(string token, string year)
        {

            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");
            dtResult.Rows.Add(dtResult.NewRow());


            var value = new ExportRecordModel
            {
                token_login = token,
                year = year,
                after_trip = new afterTripModel()
            };
            ExportReportService service = new ExportReportService();
            ReportInsuranceRecordOutModel dataX = service.report_insurance_list_record(value);


            if (dataX!=null)
            { 
                var datetime1 = DateTime.Now.ToString("yyyyMMddHHmmssff");
                string datenow = datetime1;
                string fileName = $"TRAVEL_INSURANCE_RECORD_{year.ToUpper().Trim()}_{datetime1}.xlsx";

                var output = ClassPathReport.genFilePath("temp", fileName);
                var outputUrl = ClassPathReport.genFullPath("temp", fileName);

                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                //FileInfo template = new FileInfo(HttpContext.Current.Server.MapPath(@"~/template/TEMPLATE_TRAVEL_INSURANCE_RECORD.xlsx"));
                var outputTemplate = ClassPathReport.genFilePath("template", "TEMPLATE_TRAVEL_INSURANCE_RECORD.xlsx");

                var status = "true";
                var msg = "";
                try
                {
                    FileInfo template = new FileInfo(outputTemplate);
                    using (var package = new ExcelPackage(template))
                    {
                        // save file temp ก่อนแล้วค่อยเขียนลง file temp
                        package.SaveAs(new FileInfo(output));
                    }

                    template = new FileInfo(output);
                    using (var ExcelPkg = new ExcelPackage(template))
                    {
                        ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.First();
                        string datetime = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

                        #region Header detail

                        worksheet.Cells["B2"].Value = "TRAVEL INSURANCE RECORD OF " + year;
                        worksheet.Cells["B3"].Value = "UPDATE " + datetime;

                        #endregion Header detail

                        int count_summary = dataX.details_list.Count;
                        worksheet.InsertRow(7, (count_summary - 1), 7); // insert row travel summary

                        int firstRow_summary = 7;

                        foreach (var dr in dataX.details_list)
                        {
                            worksheet.Cells["B" + firstRow_summary.ToString()].Value = dr.doc_id;
                            worksheet.Cells["C" + firstRow_summary.ToString()].Value = dr.emp_id;
                            worksheet.Cells["D" + firstRow_summary.ToString()].Value = dr.emp_passport;
                            worksheet.Cells["E" + firstRow_summary.ToString()].Value = dr.emp_display;
                            worksheet.Cells["F" + firstRow_summary.ToString()].Value = dr.emp_section;
                            worksheet.Cells["G" + firstRow_summary.ToString()].Value = dr.emp_department;
                            worksheet.Cells["H" + firstRow_summary.ToString()].Value = dr.emp_function;
                            worksheet.Cells["I" + firstRow_summary.ToString()].Value = dr.name_beneficiary;
                            worksheet.Cells["J" + firstRow_summary.ToString()].Value = dr.relationship;

                            worksheet.Cells["K" + firstRow_summary.ToString()].Value = dr.certificates_no;
                            worksheet.Cells["L" + firstRow_summary.ToString()].Value = dr.period_ins_from;
                            worksheet.Cells["M" + firstRow_summary.ToString()].Value = dr.period_ins_to;
                            worksheet.Cells["N" + firstRow_summary.ToString()].Value = dr.duration;
                            worksheet.Cells["O" + firstRow_summary.ToString()].Value = dr.country;
                            worksheet.Cells["P" + firstRow_summary.ToString()].Value = dr.billing_charge;

                            //int number = 1234567890;
                            //Convert.ToDecimal(number).ToString("#,##0.00");

                            string total = "";
                            try
                            {
                                total = dr.certificates_total != "" && dr.certificates_total != null ? Convert.ToDecimal(Convert.ToInt32(dr.certificates_total)).ToString("#,##0.00") : "";
                            }
                            catch
                            {
                                total = "";
                            }
                            worksheet.Cells["Q" + firstRow_summary.ToString()].Value = total;

                            firstRow_summary++;
                        }


                        worksheet.Name = "TRAVEL INSURANCE RECORD OF " + year;
                        ExcelPkg.SaveAs(new FileInfo(output));
                    }
                }
                catch (Exception ex) { msg = ex.Message; }
                dtResult.Rows[0]["status"] = status;
                dtResult.Rows[0]["file_system_path"] = output;
                dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                dtResult.Rows[0]["file_outbound_name"] = (string.IsNullOrEmpty(msg) ? fileName : msg);
            }
            else
            {
                dtResult.Rows[0]["status"] = "false";
                dtResult.Rows[0]["file_system_path"] = "";
                dtResult.Rows[0]["file_outbound_path"] = "";
                dtResult.Rows[0]["file_outbound_name"] = "";
            }

            return dtResult;
        }

    }
}
