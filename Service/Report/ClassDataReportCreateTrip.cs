
using System.Text.Json;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using top.ebiz.service.Service.Traveler_Profile;
using top.ebiz.service.Models.Traveler_Profile;
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Service.Report
{
    public class ClassDataReportCreateTrip
    {
        #region structure 


        public class Rootobject
        {
            public string? token_login { get; set; }
            public string? id { get; set; }
            public bool? user_admin { get; set; }
            public string? data_type { get; set; }
            public string? requested_by { get; set; }
            public string? on_behalf_of { get; set; }
            public string? org_unit_req { get; set; }
            public string? org_unit_on_behalf { get; set; }
            public string? date_to_requested { get; set; }
            public string? document_number { get; set; }
            public string? document_status { get; set; }
            public string? company { get; set; }
            public string? travel_type { get; set; }
            public string? travel_with { get; set; }
            public Travel_Details[]? travel_details { get; set; }
            public Traveler_Summary[]? traveler_summary { get; set; }
            public Estimate_Expense[]? estimate_expense { get; set; }
            public Estimate_Expense_Details[]? estimate_expense_details { get; set; }
            public Approval_By[]? approval_by { get; set; }
            public Approval_Details[]? approval_details { get; set; }

            [NotMapped]
            public After_Trip after_trip { get; set; } = new After_Trip();
        }

        public class After_Trip
        {
            public string? opt1 { get; set; }
            public Opt2? opt2 { get; set; }
            public Opt3? opt3 { get; set; }
        }

        public class Opt2
        {
            public string? status { get; set; }
            public string? remark { get; set; }
        }

        public class Opt3
        {
            public string? status { get; set; }
            public string? remark { get; set; }
        }

        public class Travel_Details
        {
            public string? no { get; set; }
            public string? travel_topic { get; set; }
            public string? continent { get; set; }
            public string? country { get; set; }
            public string? city { get; set; }
            public string? province { get; set; }
            public string? location { get; set; }
            public string? business_date { get; set; }
            public string? travel_date { get; set; }
            public string? travel_duration { get; set; }
            public string? traveling_objective { get; set; }
            public string? to_submit { get; set; }
            public string? to_share { get; set; }
            public string? to_share_remark { get; set; }
            public string? other { get; set; }
            public string? other_remark { get; set; }
        }

        public class Traveler_Summary
        {
            public string? no { get; set; }
            public string? emp_id { get; set; }
            public string? emp_name { get; set; }
            public string? org_unit { get; set; }
            public string? country_city { get; set; }
            public string? province { get; set; }
            public string? location { get; set; }
            public string? business_date { get; set; }
            public string? travel_date { get; set; }
            public string? budget_account { get; set; }
        }

        public class Estimate_Expense
        {
            public string exchange_rates_as_of { get; set; }
            public string grand_total_expenses { get; set; }
        }

        public class Estimate_Expense_Details
        {
            public string no { get; set; }
            public string emp_id { get; set; }
            public string emp_name { get; set; }
            public string org_unit { get; set; }
            public string country_city { get; set; }
            public string province { get; set; }
            public string location { get; set; }
            public string business_date { get; set; }
            public string travel_date { get; set; }
            public string budget_account { get; set; }
            public string air_ticket { get; set; }
            public string accommodation { get; set; }
            public string allowance { get; set; }
            public string transportation { get; set; }
            public string passport { get; set; }
            public string passport_valid { get; set; }
            public string visa_fee { get; set; }
            public string others { get; set; }
            public string luggage_clothing { get; set; }
            public string luggage_clothing_valid { get; set; }
            public string insurance { get; set; }
            public string total_expenses { get; set; }
            public string remark { get; set; }
        }

        public class Approval_By
        {
            public string the_budget { get; set; }
            public string shall_seek { get; set; }
            public string remark { get; set; }
        }

        public class Approval_Details
        {
            public string no { get; set; }
            public string emp_id { get; set; }
            public string emp_name { get; set; }
            public string org_unit { get; set; }
            public string line_approval { get; set; }
            public string cap_approval { get; set; }
            public string org_unit_line { get; set; }
            public string org_unit_cap { get; set; }
            public string approved_date_line { get; set; }
            public string approved_date_cap { get; set; }
        }


        #endregion structure 

        public DataTable OBApprovalReport(string token, string doc_id, string state)
        {
            // สร้างตารางข้อมูลสำหรับเก็บผลลัพธ์
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");                     // สถานะการทำงาน
            dtResult.Columns.Add("file_system_path");           // ที่อยู่ไฟล์ในระบบ
            dtResult.Columns.Add("file_outbound_path");         // ที่อยู่ไฟล์สำหรับดาวน์โหลด
            dtResult.Columns.Add("file_outbound_name");         // ชื่อไฟล์
            dtResult.Rows.Add(dtResult.NewRow());               // เพิ่มแถวเปล่า

            try
            {
                // สร้างโมเดลข้อมูลสำหรับค้นหา
                var value = new ApprovalFormModel
                {
                    token_login = token,
                    doc_id = doc_id,
                };

                // เรียกใช้บริการค้นหาข้อมูล
                searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
                ApprovalFormOutModel dataX = service.SearchApprovalForm(value);

                if (dataX != null)
                {
                    // เตรียมข้อมูลสำหรับชื่อไฟล์
                    var datetime2 = "";
                    var country = "";
                    var city = "";
                    var bussinessdate = "";

                    try
                    {
                        datetime2 = DateTime.Now.ToString("dMMMyyyy");
                        // ตรวจสอบว่ามีข้อมูล travel_details และมีข้อมูลอย่างน้อย 1 รายการ
                        if (dataX.travel_details != null && dataX.travel_details.Count > 0)
                        {
                            country = dataX.travel_details[0]?.country?.ToString()?.Replace("/", "_") ?? "";
                            city = dataX.travel_details[0]?.city?.ToString()?.Replace("/", "_") ?? "";
                            bussinessdate = dataX.travel_details[0]?.business_date?.ToString()?.Replace(" ", "") ?? "";
                        }
                    }
                    catch (Exception ex)
                    {
                        // ควรบันทึกข้อผิดพลาดไว้สำหรับการแก้ไขปัญหา
                    }

                    // สร้างชื่อไฟล์
                    string filesname = $"APPROVAL_FORM_{doc_id.ToUpper().Trim()}_{bussinessdate.Trim()}_{country}_{city}.xlsx";

                    // สร้างเส้นทางไฟล์
                    var output = ClassPathReport.genFilePath("temp", filesname);
                    var outputUrl = ClassPathReport.genFullPath("temp", filesname);
                    var pathTempplate = ClassPathReport.genFilePath("template", "2021_OB_Approval_Form.xlsx");

                    // ตรวจสอบว่า template ไฟล์มีอยู่จริง
                    //if (!File.Exists(pathTempplate))
                    //{
                    //    dtResult.Rows[0]["status"] = $"OBApprovalReport Error:{ex.Message.ToString()}";
                    //    return dtResult;
                    //}

                    // สร้างไฟล์ชั่วคราวจาก template
                    FileInfo template = new FileInfo(pathTempplate);
                    using (var package = new ExcelPackage(template))
                    {
                        package.SaveAs(new FileInfo(output));
                    }

                    var status = "false";
                    var msg = "";

                    try
                    {
                        FileInfo fileTemp = new FileInfo(output);
                        using (var ExcelPkg = new ExcelPackage(template))
                        {
                            ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.First();

                            //DevFix 20250129 0000 set style
                            worksheet.Cells.Style.Font.Name = "Arial";   // เปลี่ยนเป็นฟอนต์ที่ต้องการ
                            worksheet.Cells.Style.Font.Size = 11;       // ขนาดตัวอักษร
                            //worksheet.Cells.Style.Font.Bold = true;     // ทำให้ตัวหนา (ถ้าต้องการ)
                            //worksheet.Cells.Style.Font.Italic = true;   // ทำให้เป็นตัวเอียง (ถ้าต้องการ)


                            #region Header detail

                            if (state.ToLower().Trim() == "overseatraining")
                            {
                                worksheet.Cells["C6"].Value = "OVERSEAS TRAINING TRIP";
                            }

                            worksheet.Cells["F8"].Value = dataX.requested_by;
                            worksheet.Cells["L8"].Value = dataX.org_unit_req;
                            worksheet.Cells["S8"].Value = dataX.on_behalf_of;
                            worksheet.Cells["Y8"].Value = dataX.org_unit_on_behalf;
                            worksheet.Cells["AI8"].Value = dataX.date_to_requested;


                            worksheet.Cells["G10"].Value = dataX.document_number;
                            worksheet.Cells["N10"].Value = dataX.document_status;
                            worksheet.Cells["T10"].Value = dataX.company;
                            worksheet.Cells["AB10"].Value = dataX.travel_type;
                            worksheet.Cells["AF10"].Value = dataX.travel_with;

                            #endregion Header detail

                            #region PART I : TRAVEL DETAILS
                            worksheet.Cells["F15"].Value = dataX.travel_details[0].travel_topic;
                            worksheet.Cells["X15"].Value = dataX.travel_details[0].continent;
                            worksheet.Cells["AC15"].Value = dataX.travel_details[0].country;
                            worksheet.Cells["AI15"].Value = dataX.travel_details[0].city;

                            worksheet.Cells["I17"].Value = dataX.travel_details[0].business_date;
                            worksheet.Cells["W17"].Value = dataX.travel_details[0].travel_date;
                            worksheet.Cells["AI17"].Value = dataX.travel_details[0].travel_duration;
                            string traveling_objectiveDef = dataX.travel_details[0].traveling_objective;
                            string traveling_objectiveRow1 = "";
                            string traveling_objectiveRow2 = "";
                            string traveling_objectiveRow3 = "";
                            string traveling_objectiveRow4 = "";
                            // traveling_objectiveDef = @"H&R is our current clients for most of TLB products and also the partnership and joint venture for TDAE production unit and marketing arm. The main objective to visit H&R head quarter is to ensure the healthy business relationship and also explore the opportunity with H&R in technological aspects. During the visiting plan, the discussion topics will combinded for both market information, contract renewal technical information and visit specialties plant. ";

                            if (traveling_objectiveDef.Length > 120)
                            {
                                worksheet.Cells["C20:AK20"].Merge = true;
                                worksheet.Cells["C20"].Style.WrapText = true;
                                worksheet.SelectedRange["C20"].Value = traveling_objectiveDef;
                                worksheet.Row(20).Height = 30 * 4;

                            }
                            else
                            {
                                worksheet.Cells["I19"].Value = traveling_objectiveDef;
                            }

                            int count_summary = dataX.traveler_summary.Count;
                            worksheet.InsertRow(26, (count_summary - 1), 26); // insert row travel summary

                            int firstRow_summary = 26;

                            foreach (var dr in dataX.traveler_summary)
                            {
                                worksheet.Cells["V" + firstRow_summary.ToString() + ":Y" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["V" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells["Z" + firstRow_summary.ToString() + ":AD" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["Z" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells["AE" + firstRow_summary.ToString() + ":AL" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["AE" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                worksheet.Cells["C" + firstRow_summary.ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + firstRow_summary.ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + firstRow_summary.ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + firstRow_summary.ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + firstRow_summary.ToString()].Value = dr.country_city;
                                worksheet.Cells["V" + firstRow_summary.ToString()].Value = dr.business_date;
                                worksheet.Cells["Z" + firstRow_summary.ToString()].Value = dr.travel_date;
                                worksheet.Cells["AE" + firstRow_summary.ToString()].Value = dr.budget_account;

                                firstRow_summary++;
                            }
                            // To approve the business trip according to the objective and description proposed. After the mission completed, the staff should
                            worksheet.Cells["C" + (firstRow_summary + 1).ToString() + ":AL" + (firstRow_summary + 1).ToString()].Merge = true;

                            worksheet.Cells["C" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            // To submit a report to Line Manager within 30 days after the trip is finished. 
                            worksheet.Cells["D" + (firstRow_summary + 2).ToString() + ":AL" + (firstRow_summary + 2).ToString()].Merge = true;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Value = dataX.travel_details[0].to_submit == "Y" ? "\u2611" : "\u2610"; // ☑ or ☐

                            worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            // To share knowledge to concerned person using methods 
                            worksheet.Cells["D" + (firstRow_summary + 3).ToString() + ":M" + (firstRow_summary + 3).ToString()].Merge = true;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Value = dataX.travel_details[0].to_share == "Y" ? "\u2611" : "\u2610";
                            worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            worksheet.Cells["N" + (firstRow_summary + 3).ToString()].Value = dataX.travel_details[0].to_share_remark;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Value = dataX.travel_details[0].other == "Y" ? "\u2611" : "\u2610";
                            worksheet.Cells["E" + (firstRow_summary + 4).ToString()].Value = dataX.travel_details[0].other_remark;

                            #endregion PART I : TRAVEL DETAILS

                            #region PART II : ESTIMATE EXPENSE

                            int firstRow_expense = firstRow_summary + 7;
                            int firstRow_expense_detail = firstRow_summary + 9;
                            int betweenRow = 6;
                            int countExpenseDeatil = dataX.estimate_expense_details.Count;
                            //int rowCopy = 1;

                            worksheet.Cells["AK" + firstRow_expense.ToString()].Value = "Exchange Rates as of " + dataX.estimate_expense[0].exchange_rates_as_of + " THB/USD";

                            int x1 = 0;
                            int x2 = 0;

                            for (int i = 0; i < dataX.estimate_expense_details.Count - 1; i++)
                            {
                                int insRow = (firstRow_expense_detail) + betweenRow;
                                worksheet.InsertRow(insRow, betweenRow, firstRow_expense_detail);

                                int x3 = (((firstRow_expense_detail) + betweenRow));
                                int x4 = (x3 + betweenRow) - 1;
                                if (i == 0)
                                {
                                    x1 = (firstRow_expense_detail);
                                    x2 = ((firstRow_expense_detail) + betweenRow - 1);
                                    //x3 = (((firstRow_expense_detail * rowCopy) + betweenRow) + 1);
                                    //x4 = (((firstRow_expense_detail * rowCopy) + betweenRow + betweenRow) + 1);
                                }
                                worksheet.Cells["A" + (x1).ToString() + ":AL" + (x2).ToString()].Copy(worksheet.Cells["A" + (x3).ToString() + ":AL" + (x4).ToString()]);

                                //worksheet.Cells["A37:AL43"].Copy(worksheet.Cells["A44:AL50"]);
                                firstRow_expense_detail = firstRow_expense_detail + betweenRow;

                            }

                            firstRow_expense_detail = firstRow_summary + 9;
                            int countRow = 0;
                            foreach (var dr in dataX.estimate_expense_details)
                            {
                                countRow++;

                                worksheet.Cells["C" + (firstRow_expense_detail).ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + (firstRow_expense_detail).ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + (firstRow_expense_detail).ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + (firstRow_expense_detail).ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + (firstRow_expense_detail).ToString()].Value = dr.country_city;
                                worksheet.Cells["V" + (firstRow_expense_detail).ToString()].Value = dr.business_date;//business date
                                worksheet.Cells["Z" + (firstRow_expense_detail).ToString()].Value = dr.travel_date;
                                worksheet.Cells["AE" + (firstRow_expense_detail).ToString()].Value = dr.budget_account;

                                worksheet.Cells["J" + (firstRow_expense_detail + 1).ToString()].Value = dr.air_ticket;
                                worksheet.Cells["R" + (firstRow_expense_detail + 1).ToString()].Value = dr.accommodation;
                                worksheet.Cells["AA" + (firstRow_expense_detail + 1).ToString()].Value = dr.allowance;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 1).ToString()].Value = dr.transportation;

                                worksheet.Cells["J" + (firstRow_expense_detail + 2).ToString()].Value = dr.passport;
                                worksheet.Cells["P" + (firstRow_expense_detail + 2).ToString()].Value = dr.passport_valid;
                                worksheet.Cells["AA" + (firstRow_expense_detail + 2).ToString()].Value = dr.visa_fee;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 2).ToString()].Value = dr.others;

                                worksheet.Cells["J" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing;
                                worksheet.Cells["P" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing_valid;
                                worksheet.Cells["AA" + (firstRow_expense_detail + 3).ToString()].Value = dr.insurance;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 3).ToString()].Value = dr.total_expenses;

                                worksheet.Cells["F" + (firstRow_expense_detail + 4).ToString()].Value = dr.remark;

                                if (countRow == dataX.estimate_expense_details.Count)
                                {
                                    worksheet.Cells["AG" + (firstRow_expense_detail + 7).ToString()].Value = dataX.estimate_expense[0].grand_total_expenses;
                                }
                                //if (countRow != dataX.estimate_expense_details.Length)
                                //{
                                firstRow_expense_detail = firstRow_expense_detail + betweenRow;
                                //}
                            }

                            //worksheet.Cells["A36:AL43"].Copy(worksheet.Cells["A66:AK71"]);

                            #endregion PART II : ESTIMATE EXPENSE

                            #region PART III : APPROVAL BY

                            int count_approval = dataX.approval_details.Count;
                            int firstRow_approval = firstRow_expense_detail + 7;

                            worksheet.InsertRow(firstRow_approval, (count_approval - 1), firstRow_approval); // insert row approval by

                            foreach (var dr in dataX.approval_details)
                            {

                                worksheet.Cells["C" + firstRow_approval.ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + firstRow_approval.ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + firstRow_approval.ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + firstRow_approval.ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + firstRow_approval.ToString()].Value = dr.line_approval;
                                //worksheet.Cells["W" + firstRow_approval.ToString()].Value = dr.org_unit_line;
                                worksheet.Cells["Z" + firstRow_approval.ToString()].Value = dr.approved_date_line;
                                worksheet.Cells["AB" + firstRow_approval.ToString()].Value = dr.cap_approval;
                                //worksheet.Cells["AG" + firstRow_approval.ToString()].Value = dr.org_unit_cap;
                                worksheet.Cells["AK" + firstRow_approval.ToString()].Value = dr.approved_date_cap;

                                firstRow_approval++;
                            }

                            worksheet.Cells["C" + (firstRow_approval + 1).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_approval + 1).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_approval + 1).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_approval + 1).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_approval + 1).ToString()].Value = dataX.approval_by[0].the_budget == "Y" ? "\u2611" : "\u2610";
                            worksheet.Cells["C" + (firstRow_approval + 2).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_approval + 2).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_approval + 2).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_approval + 2).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_approval + 2).ToString()].Value = dataX.approval_by[0].shall_seek == "Y" ? "\u2611" : "\u2610";


                            worksheet.Cells["D" + (firstRow_approval + 3).ToString()].Value = dataX.approval_by[0].remark;

                            #endregion PART III : APPROVAL BY


                            //worksheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                            ExcelWorksheet datasheet = ExcelPkg.Workbook.Worksheets["data_sheet"];
                            ////to_submit
                            ////to_share
                            ////other
                            ////the_budget
                            ////shall_seek
                            datasheet.Cells["B1"].Value = dataX.travel_details[0].to_submit == "Y" ? "\u2611" : "\u2610"; // ☑ or ☐
                            datasheet.Cells["B2"].Value = dataX.travel_details[0].to_share == "Y" ? "\u2611" : "\u2610";
                            datasheet.Cells["B3"].Value = dataX.travel_details[0].other == "Y" ? "\u2611" : "\u2610";
                            datasheet.Cells["B4"].Value = dataX.approval_by[0].the_budget == "Y" ? "\u2611" : "\u2610";
                            datasheet.Cells["B5"].Value = dataX.approval_by[0].shall_seek == "Y" ? "\u2611" : "\u2610";


                            // ตั้งค่าให้ฟอนต์รองรับเครื่องหมาย checkbox และจัดกึ่งกลาง
                            for (int i = 1; i <= 5; i++)
                            {
                                var cell = datasheet.Cells[$"B{i}"];
                                cell.Style.Font.Name = "Segoe UI Symbol"; // หรือ Arial Unicode MS
                                cell.Style.Font.Size = 11;
                                cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }

                            //datasheet.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;

                            worksheet.Name = doc_id;

                            ExcelPkg.SaveAs(new FileInfo(output));
                        }

                        status = "true";
                    }
                    catch (Exception ex) { msg = ex.Message.ToString(); }

                    dtResult.Rows[0]["status"] = status;
                    dtResult.Rows[0]["file_system_path"] = output;
                    dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                    dtResult.Rows[0]["file_outbound_name"] = (string.IsNullOrEmpty(msg) ? filesname : msg);
                }
                else
                {
                    dtResult.Rows[0]["status"] = $"OBApprovalReport dataX is null ";
                    dtResult.Rows[0]["file_system_path"] = "";
                    dtResult.Rows[0]["file_outbound_path"] = "";
                    dtResult.Rows[0]["file_outbound_name"] = "";
                }
            }
            catch (Exception ex)
            {
                dtResult.Rows[0]["status"] = $"OBApprovalReport Error:{ex.Message.ToString()}";
                dtResult.Rows[0]["file_system_path"] = "";
                dtResult.Rows[0]["file_outbound_path"] = "";
                dtResult.Rows[0]["file_outbound_name"] = "";
            }

            return dtResult;
        }

        public DataTable OBApprovalReportV1(string token, string doc_id, string state)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");
            dtResult.Rows.Add(dtResult.NewRow());

            try
            {
      
                //string jsonResult = PostCallAPI(doc_id, token).GetAwaiter().GetResult();
                var value = new ApprovalFormModel
                {
                    token_login = token,
                    doc_id = doc_id,
                };
                searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
                ApprovalFormOutModel dataX = service.SearchApprovalForm(value);

                if (dataX != null)
                {
                    //var datetime1 = DateTime.Now.ToString("yyyyMMddHHmmssff");
                    //string datenow = datetime1;
                    //string fileName = $"APPROVAL_FORM_{doc_id.ToUpper().Trim()}_{datetime1}.xlsx";
                    var datetime2 = "";
                    var country = "";
                    var city = "";
                    var bussinessdate = "";
                    try
                    {
                        datetime2 = DateTime.Now.ToString("dMMMyyyy");
                        country = dataX.travel_details[0].country?.ToString().Replace("/", "_"); ;
                        city = dataX.travel_details[0].city?.ToString().Replace("/", "_");
                        bussinessdate = dataX.travel_details[0].business_date?.ToString().Replace(" ", "");
                    }
                    catch { }


                    string filesname = $"APPROVAL_FORM_{doc_id.ToUpper().Trim()}_{bussinessdate.Trim()}_{country}_{city}.xlsx";

                    var output = ClassPathReport.genFilePath("temp", filesname);// Path.Combine(HttpContext.Current.Server.MapPath("~/temp"), fileName);
                    var outputUrl = ClassPathReport.genFullPath("temp", filesname);

                    var pathTempplate = ClassPathReport.genFilePath("template", "2021_OB_Approval_Form.xlsx");
                    FileInfo template = new FileInfo(pathTempplate);// new FileInfo(HttpContext.Current.Server.MapPath(@"~/template/2021_OB_Approval_Form.xlsx"));
                    using (var package = new ExcelPackage(template))
                    {
                        // save file temp ก่อนแล้วค่อยเขียนลง file temp
                        package.SaveAs(new FileInfo(output));
                    }

                    var status = "false";
                    var msg = "";
                    try
                    {
                        FileInfo fileTemp = new FileInfo(output);
                        using (var ExcelPkg = new ExcelPackage(template))
                        {
                            ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.First();

                            //DevFix 20250129 0000 set style
                            worksheet.Cells.Style.Font.Name = "Arial";   // เปลี่ยนเป็นฟอนต์ที่ต้องการ
                            worksheet.Cells.Style.Font.Size = 12;       // ขนาดตัวอักษร
                            //worksheet.Cells.Style.Font.Bold = true;     // ทำให้ตัวหนา (ถ้าต้องการ)
                            //worksheet.Cells.Style.Font.Italic = true;   // ทำให้เป็นตัวเอียง (ถ้าต้องการ)


                            #region Header detail

                            if (state.ToLower().Trim() == "overseatraining")
                            {
                                worksheet.Cells["C6"].Value = "OVERSEAS TRAINING TRIP";
                            }

                            worksheet.Cells["F8"].Value = dataX.requested_by;
                            worksheet.Cells["L8"].Value = dataX.org_unit_req;
                            worksheet.Cells["S8"].Value = dataX.on_behalf_of;
                            worksheet.Cells["Y8"].Value = dataX.org_unit_on_behalf;
                            worksheet.Cells["AI8"].Value = dataX.date_to_requested;


                            worksheet.Cells["G10"].Value = dataX.document_number;
                            worksheet.Cells["N10"].Value = dataX.document_status;
                            worksheet.Cells["T10"].Value = dataX.company;
                            worksheet.Cells["AB10"].Value = dataX.travel_type;
                            worksheet.Cells["AF10"].Value = dataX.travel_with;

                            #endregion Header detail

                            #region PART I : TRAVEL DETAILS
                            worksheet.Cells["F15"].Value = dataX.travel_details[0].travel_topic;
                            worksheet.Cells["X15"].Value = dataX.travel_details[0].continent;
                            worksheet.Cells["AC15"].Value = dataX.travel_details[0].country;
                            worksheet.Cells["AI15"].Value = dataX.travel_details[0].city;

                            worksheet.Cells["I17"].Value = dataX.travel_details[0].business_date;
                            worksheet.Cells["W17"].Value = dataX.travel_details[0].travel_date;
                            worksheet.Cells["AI17"].Value = dataX.travel_details[0].travel_duration;
                            string traveling_objectiveDef = dataX.travel_details[0].traveling_objective;
                            string traveling_objectiveRow1 = "";
                            string traveling_objectiveRow2 = "";
                            string traveling_objectiveRow3 = "";
                            string traveling_objectiveRow4 = "";
                            // traveling_objectiveDef = @"H&R is our current clients for most of TLB products and also the partnership and joint venture for TDAE production unit and marketing arm. The main objective to visit H&R head quarter is to ensure the healthy business relationship and also explore the opportunity with H&R in technological aspects. During the visiting plan, the discussion topics will combinded for both market information, contract renewal technical information and visit specialties plant. ";

                            if (traveling_objectiveDef.Length > 120)
                            {
                                worksheet.Cells["C20:AK20"].Merge = true;
                                worksheet.Cells["C20"].Style.WrapText = true;
                                worksheet.SelectedRange["C20"].Value = traveling_objectiveDef;
                                worksheet.Row(20).Height = 30 * 4;

                            }
                            else
                            {
                                worksheet.Cells["I19"].Value = traveling_objectiveDef;
                            }

                            int count_summary = dataX.traveler_summary.Count;
                            worksheet.InsertRow(26, (count_summary - 1), 26); // insert row travel summary

                            int firstRow_summary = 26;

                            foreach (var dr in dataX.traveler_summary)
                            {
                                worksheet.Cells["V" + firstRow_summary.ToString() + ":Y" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["V" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells["Z" + firstRow_summary.ToString() + ":AD" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["Z" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells["AE" + firstRow_summary.ToString() + ":AL" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["AE" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                worksheet.Cells["C" + firstRow_summary.ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + firstRow_summary.ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + firstRow_summary.ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + firstRow_summary.ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + firstRow_summary.ToString()].Value = dr.country_city;
                                worksheet.Cells["V" + firstRow_summary.ToString()].Value = dr.business_date;
                                worksheet.Cells["Z" + firstRow_summary.ToString()].Value = dr.travel_date;
                                worksheet.Cells["AE" + firstRow_summary.ToString()].Value = dr.budget_account;

                                firstRow_summary++;
                            }
                            // To approve the business trip according to the objective and description proposed. After the mission completed, the staff should
                            worksheet.Cells["C" + (firstRow_summary + 1).ToString() + ":AL" + (firstRow_summary + 1).ToString()].Merge = true;
                            worksheet.Cells["C" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            // To submit a report to Line Manager within 30 days after the trip is finished. 
                            worksheet.Cells["D" + (firstRow_summary + 2).ToString() + ":AL" + (firstRow_summary + 2).ToString()].Merge = true;
                            worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            // To share knowledge to concerned person using methods 
                            worksheet.Cells["D" + (firstRow_summary + 3).ToString() + ":M" + (firstRow_summary + 3).ToString()].Merge = true;
                            worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            worksheet.Cells["N" + (firstRow_summary + 3).ToString()].Value = dataX.travel_details[0].to_share_remark;
                            worksheet.Cells["E" + (firstRow_summary + 4).ToString()].Value = dataX.travel_details[0].other_remark;

                            #endregion PART I : TRAVEL DETAILS

                            #region PART II : ESTIMATE EXPENSE

                            int firstRow_expense = firstRow_summary + 7;
                            int firstRow_expense_detail = firstRow_summary + 9;
                            int betweenRow = 6;
                            int countExpenseDeatil = dataX.estimate_expense_details.Count;
                            //int rowCopy = 1;

                            worksheet.Cells["AK" + firstRow_expense.ToString()].Value = "Exchange Rates as of " + dataX.estimate_expense[0].exchange_rates_as_of + " THB/USD";

                            int x1 = 0;
                            int x2 = 0;

                            for (int i = 0; i < dataX.estimate_expense_details.Count - 1; i++)
                            {
                                int insRow = (firstRow_expense_detail) + betweenRow;
                                worksheet.InsertRow(insRow, betweenRow, firstRow_expense_detail);

                                int x3 = (((firstRow_expense_detail) + betweenRow));
                                int x4 = (x3 + betweenRow) - 1;
                                if (i == 0)
                                {
                                    x1 = (firstRow_expense_detail);
                                    x2 = ((firstRow_expense_detail) + betweenRow - 1);
                                    //x3 = (((firstRow_expense_detail * rowCopy) + betweenRow) + 1);
                                    //x4 = (((firstRow_expense_detail * rowCopy) + betweenRow + betweenRow) + 1);
                                }
                                worksheet.Cells["A" + (x1).ToString() + ":AL" + (x2).ToString()].Copy(worksheet.Cells["A" + (x3).ToString() + ":AL" + (x4).ToString()]);

                                //worksheet.Cells["A37:AL43"].Copy(worksheet.Cells["A44:AL50"]);
                                firstRow_expense_detail = firstRow_expense_detail + betweenRow;

                            }

                            firstRow_expense_detail = firstRow_summary + 9;
                            int countRow = 0;
                            foreach (var dr in dataX.estimate_expense_details)
                            {
                                countRow++;

                                worksheet.Cells["C" + (firstRow_expense_detail).ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + (firstRow_expense_detail).ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + (firstRow_expense_detail).ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + (firstRow_expense_detail).ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + (firstRow_expense_detail).ToString()].Value = dr.country_city;
                                worksheet.Cells["V" + (firstRow_expense_detail).ToString()].Value = dr.business_date;//business date
                                worksheet.Cells["Z" + (firstRow_expense_detail).ToString()].Value = dr.travel_date;
                                worksheet.Cells["AE" + (firstRow_expense_detail).ToString()].Value = dr.budget_account;

                                worksheet.Cells["J" + (firstRow_expense_detail + 1).ToString()].Value = dr.air_ticket;
                                worksheet.Cells["R" + (firstRow_expense_detail + 1).ToString()].Value = dr.accommodation;
                                worksheet.Cells["AA" + (firstRow_expense_detail + 1).ToString()].Value = dr.allowance;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 1).ToString()].Value = dr.transportation;

                                worksheet.Cells["J" + (firstRow_expense_detail + 2).ToString()].Value = dr.passport;
                                worksheet.Cells["P" + (firstRow_expense_detail + 2).ToString()].Value = dr.passport_valid;
                                worksheet.Cells["AA" + (firstRow_expense_detail + 2).ToString()].Value = dr.visa_fee;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 2).ToString()].Value = dr.others;

                                worksheet.Cells["J" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing;
                                worksheet.Cells["P" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing_valid;
                                worksheet.Cells["AA" + (firstRow_expense_detail + 3).ToString()].Value = dr.insurance;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 3).ToString()].Value = dr.total_expenses;

                                worksheet.Cells["F" + (firstRow_expense_detail + 4).ToString()].Value = dr.remark;

                                if (countRow == dataX.estimate_expense_details.Count)
                                {
                                    worksheet.Cells["AG" + (firstRow_expense_detail + 7).ToString()].Value = dataX.estimate_expense[0].grand_total_expenses;
                                }
                                //if (countRow != dataX.estimate_expense_details.Length)
                                //{
                                firstRow_expense_detail = firstRow_expense_detail + betweenRow;
                                //}
                            }

                            //worksheet.Cells["A36:AL43"].Copy(worksheet.Cells["A66:AK71"]);

                            #endregion PART II : ESTIMATE EXPENSE

                            #region PART III : APPROVAL BY

                            int count_approval = dataX.approval_details.Count;
                            int firstRow_approval = firstRow_expense_detail + 7;

                            worksheet.InsertRow(firstRow_approval, (count_approval - 1), firstRow_approval); // insert row approval by

                            foreach (var dr in dataX.approval_details)
                            {

                                worksheet.Cells["C" + firstRow_approval.ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + firstRow_approval.ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + firstRow_approval.ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + firstRow_approval.ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + firstRow_approval.ToString()].Value = dr.line_approval;
                                //worksheet.Cells["W" + firstRow_approval.ToString()].Value = dr.org_unit_line;
                                worksheet.Cells["Z" + firstRow_approval.ToString()].Value = dr.approved_date_line;
                                worksheet.Cells["AB" + firstRow_approval.ToString()].Value = dr.cap_approval;
                                //worksheet.Cells["AG" + firstRow_approval.ToString()].Value = dr.org_unit_cap;
                                worksheet.Cells["AK" + firstRow_approval.ToString()].Value = dr.approved_date_cap;

                                firstRow_approval++;
                            }

                            worksheet.Cells["D" + (firstRow_approval + 3).ToString()].Value = dataX.approval_by[0].remark;

                            #endregion PART III : APPROVAL BY


                            //worksheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                            ExcelWorksheet datasheet = ExcelPkg.Workbook.Worksheets["data_sheet"];
                            //to_submit
                            //to_share
                            //other
                            //the_budget
                            //shall_seek
                            datasheet.Cells["B1"].Value = dataX.travel_details[0].to_submit == "Y" ? true : false;
                            datasheet.Cells["B2"].Value = dataX.travel_details[0].to_share == "Y" ? true : false;
                            datasheet.Cells["B3"].Value = dataX.travel_details[0].other == "Y" ? true : false;
                            datasheet.Cells["B4"].Value = dataX.approval_by[0].the_budget == "Y" ? true : false;
                            datasheet.Cells["B5"].Value = dataX.approval_by[0].shall_seek == "Y" ? true : false;

                            datasheet.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;

                            worksheet.Name = doc_id;

                            ExcelPkg.SaveAs(new FileInfo(output));
                        }

                        status = "true";
                    }
                    catch (Exception ex) { msg = ex.Message.ToString(); }

                    dtResult.Rows[0]["status"] = status;
                    dtResult.Rows[0]["file_system_path"] = output;
                    dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                    dtResult.Rows[0]["file_outbound_name"] = (string.IsNullOrEmpty(msg) ? filesname : msg);
                }
                else
                {
                    dtResult.Rows[0]["status"] = $"OBApprovalReport dataX is null ";
                    dtResult.Rows[0]["file_system_path"] = "";
                    dtResult.Rows[0]["file_outbound_path"] = "";
                    dtResult.Rows[0]["file_outbound_name"] = "";
                }
            }
            catch (Exception ex)
            {
                dtResult.Rows[0]["status"] = $"OBApprovalReport Error:{ex.Message.ToString()}";
                dtResult.Rows[0]["file_system_path"] = "";
                dtResult.Rows[0]["file_outbound_path"] = "";
                dtResult.Rows[0]["file_outbound_name"] = "";
            }

            return dtResult;
        }

        public DataTable LBApprovalReport(string token, string doc_id, string state)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");
            dtResult.Rows.Add(dtResult.NewRow());

            try
            {

                //string jsonResult = PostCallAPI(doc_id, token).GetAwaiter().GetResult();
                var value = new ApprovalFormModel
                {
                    token_login = token,
                    doc_id = doc_id,
                };
                searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
                ApprovalFormOutModel dataX = service.SearchApprovalForm(value);

                if (dataX != null)
                {
                    //var datetime1 = DateTime.Now.ToString("yyyyMMddHHmmssff");
                    //string datenow = datetime1;
                    //string fileName = $"APPROVAL_FORM_{doc_id.ToUpper().Trim()}_{datetime1}.xlsx";
                    var datetime2 = "";
                    var province = "";
                    var city = "";
                    var bussinessdate = "";
                    try
                    {
                        datetime2 = DateTime.Now.ToString("dMMMyyyy");
                        province = dataX.travel_details[0].province?.ToString().Replace("/", "_"); ;
                        city = dataX.travel_details[0].city?.ToString().Replace("/", "_");
                        bussinessdate = dataX.travel_details[0].business_date?.ToString().Replace(" ", "");
                    }
                    catch { }


                    string filesname = $"APPROVAL_FORM_{doc_id.ToUpper().Trim()}_{bussinessdate.Trim()}_{province}_{city}.xlsx";

                    var output = ClassPathReport.genFilePath("temp", filesname);// Path.Combine(HttpContext.Current.Server.MapPath("~/temp"), fileName);
                    var outputUrl = ClassPathReport.genFullPath("temp", filesname);

                    var pathTempplate = ClassPathReport.genFilePath("template", "2021_LB_Approval_Form.xlsx");
                    FileInfo template = new FileInfo(pathTempplate);// new FileInfo(HttpContext.Current.Server.MapPath(@"~/template/2021_OB_Approval_Form.xlsx"));
                    using (var package = new ExcelPackage(template))
                    {
                        // save file temp ก่อนแล้วค่อยเขียนลง file temp
                        package.SaveAs(new FileInfo(output));
                    }

                    var status = "false";
                    var msg = "";
                    try
                    {

                        FileInfo fileTemp = new FileInfo(output);
                        using (var ExcelPkg = new ExcelPackage(template))
                        {
                            ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.First();
                            //DevFix 20250129 0000 set style
                            worksheet.Cells.Style.Font.Name = "Arial";   // เปลี่ยนเป็นฟอนต์ที่ต้องการ
                            worksheet.Cells.Style.Font.Size = 11;       // ขนาดตัวอักษร

                            #region Header detail
                            if (state.ToLower().Trim() == "localtraining")
                            {
                                worksheet.Cells["C6"].Value = "LOCAL TRAINING TRIP";
                            }
                            worksheet.Cells["F8"].Value = dataX.requested_by;
                            worksheet.Cells["L8"].Value = dataX.org_unit_req;
                            worksheet.Cells["S8"].Value = dataX.on_behalf_of;
                            worksheet.Cells["Y8"].Value = dataX.org_unit_on_behalf;
                            worksheet.Cells["AI8"].Value = dataX.date_to_requested;


                            worksheet.Cells["G10"].Value = dataX.document_number;
                            worksheet.Cells["N10"].Value = dataX.document_status;
                            worksheet.Cells["T10"].Value = dataX.company;
                            worksheet.Cells["AB10"].Value = dataX.travel_type;
                            worksheet.Cells["AF10"].Value = dataX.travel_with;

                            #endregion Header detail

                            #region PART I : TRAVEL DETAILS
                            worksheet.Cells["F14"].Value = dataX.travel_details[0].travel_topic;
                            worksheet.Cells["V14"].Value = dataX.travel_details[0].province;
                            worksheet.Cells["AD14"].Value = dataX.travel_details[0].location;
                            //worksheet.Cells["AI15"].Value = dataX.travel_details[0].city;

                            worksheet.Cells["I15"].Value = dataX.travel_details[0].business_date;
                            worksheet.Cells["V15"].Value = dataX.travel_details[0].travel_date;
                            worksheet.Cells["AE15"].Value = dataX.travel_details[0].travel_duration;

                            worksheet.Cells["J16"].Value = dataX.travel_details[0].traveling_objective;

                            int count_summary = dataX.traveler_summary.Count;
                            int firstRow_summary = 22;
                            if (count_summary > 1)
                                worksheet.InsertRow(firstRow_summary + 1, (count_summary - 1), 22); // insert row travel summary



                            foreach (var dr in dataX.traveler_summary)
                            {
                                worksheet.Cells["T" + firstRow_summary.ToString() + ":Y" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["T" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells["Z" + firstRow_summary.ToString() + ":AD" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["Z" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells["AE" + firstRow_summary.ToString() + ":AL" + firstRow_summary.ToString()].Merge = true;
                                worksheet.Cells["AE" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                worksheet.Cells["C" + firstRow_summary.ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + firstRow_summary.ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + firstRow_summary.ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + firstRow_summary.ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + firstRow_summary.ToString()].Value = dr.province;
                                worksheet.Cells["T" + firstRow_summary.ToString()].Value = dr.location;
                                worksheet.Cells["Z" + firstRow_summary.ToString()].Value = dr.travel_date;
                                worksheet.Cells["AE" + firstRow_summary.ToString()].Value = dr.budget_account;

                                firstRow_summary++;
                            }
                            // To approve the business trip according to the objective and description proposed. After the mission completed, the staff should
                            worksheet.Cells["C" + (firstRow_summary + 1).ToString() + ":AL" + (firstRow_summary + 1).ToString()].Merge = true;

                            worksheet.Cells["C" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            // To submit a report to Line Manager within 30 days after the trip is finished. 
                            worksheet.Cells["D" + (firstRow_summary + 2).ToString() + ":AL" + (firstRow_summary + 2).ToString()].Merge = true;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 2).ToString()].Value = dataX.travel_details[0].to_submit == "Y" ? "\u2611" : "\u2610"; // ☑ or ☐

                            worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            // To share knowledge to concerned person using methods 
                            worksheet.Cells["D" + (firstRow_summary + 3).ToString() + ":M" + (firstRow_summary + 3).ToString()].Merge = true;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 3).ToString()].Value = dataX.travel_details[0].to_share == "Y" ? "\u2611" : "\u2610";
                            worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            worksheet.Cells["N" + (firstRow_summary + 3).ToString()].Value = dataX.travel_details[0].to_share_remark;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.Font.Name = "Arial Unicode MS";
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.Font.Size = 11;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Cells["C" + (firstRow_summary + 4).ToString()].Value = dataX.travel_details[0].other == "Y" ? "\u2611" : "\u2610";
                            worksheet.Cells["E" + (firstRow_summary + 4).ToString()].Value = dataX.travel_details[0].other_remark;

                            #endregion PART I : TRAVEL DETAILS

                            #region PART II : ESTIMATE EXPENSE

                            int firstRow_expense = firstRow_summary + 7;
                            int firstRow_expense_detail = firstRow_summary + 9;
                            int betweenRow = 4;
                            int countExpenseDeatil = dataX.estimate_expense_details.Count;
                            //int rowCopy = 1;

                            worksheet.Cells["AK" + firstRow_expense.ToString()].Value = "Exchange Rates as of " + dataX.estimate_expense[0].exchange_rates_as_of + " THB/USD";

                            int x1 = 0;
                            int x2 = 0;

                            for (int i = 0; i < dataX.estimate_expense_details.Count - 1; i++)
                            {
                                int insRow = (firstRow_expense_detail) + betweenRow;
                                worksheet.InsertRow(insRow, betweenRow, firstRow_expense_detail);

                                int x3 = (((firstRow_expense_detail) + betweenRow));
                                int x4 = (x3 + betweenRow) - 1;
                                if (i == 0)
                                {
                                    x1 = (firstRow_expense_detail);
                                    x2 = ((firstRow_expense_detail) + betweenRow - 1);
                                }
                                worksheet.Cells["A" + (x1).ToString() + ":AL" + (x2).ToString()].Copy(worksheet.Cells["A" + (x3).ToString() + ":AL" + (x4).ToString()]);

                                firstRow_expense_detail = firstRow_expense_detail + betweenRow;

                            }

                            firstRow_expense_detail = firstRow_summary + 9;
                            int countRow = 0;
                            foreach (var dr in dataX.estimate_expense_details)
                            {
                                countRow++;

                                worksheet.Cells["C" + (firstRow_expense_detail).ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + (firstRow_expense_detail).ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + (firstRow_expense_detail).ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + (firstRow_expense_detail).ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + (firstRow_expense_detail).ToString()].Value = dr.province;
                                worksheet.Cells["T" + (firstRow_expense_detail).ToString()].Value = dr.location;
                                worksheet.Cells["Z" + (firstRow_expense_detail).ToString()].Value = dr.travel_date;
                                worksheet.Cells["AE" + (firstRow_expense_detail).ToString()].Value = dr.budget_account;

                                //worksheet.Cells["G" + (firstRow_expense_detail + 1).ToString()].Value = dr.allowance;
                                //worksheet.Cells["O" + (firstRow_expense_detail + 1).ToString()].Value = dr.accommodation;
                                //worksheet.Cells["X" + (firstRow_expense_detail + 1).ToString()].Value = dr.transportation;
                                //worksheet.Cells["AG" + (firstRow_expense_detail + 1).ToString()].Value = dr.others;

                                worksheet.Cells["J" + (firstRow_expense_detail + 1).ToString()].Value = dr.allowance;
                                worksheet.Cells["R" + (firstRow_expense_detail + 1).ToString()].Value = dr.accommodation;
                                worksheet.Cells["AA" + (firstRow_expense_detail + 1).ToString()].Value = dr.transportation;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 1).ToString()].Value = dr.others;

                                worksheet.Cells["G" + (firstRow_expense_detail + 2).ToString()].Value = dr.remark;
                                worksheet.Cells["AJ" + (firstRow_expense_detail + 2).ToString()].Value = dr.total_expenses;

                                //worksheet.Cells["H" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing;
                                //worksheet.Cells["P" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing_valid;
                                //worksheet.Cells["X" + (firstRow_expense_detail + 3).ToString()].Value = dr.insurance;
                                //worksheet.Cells["AG" + (firstRow_expense_detail + 3).ToString()].Value = dr.total_expenses;

                                //worksheet.Cells["F" + (firstRow_expense_detail + 4).ToString()].Value = dr.remark;

                                if (countRow == dataX.estimate_expense_details.Count)
                                {
                                    worksheet.Cells["AG" + (firstRow_expense_detail + 5).ToString()].Value = dataX.estimate_expense[0].grand_total_expenses;
                                }

                                firstRow_expense_detail = firstRow_expense_detail + betweenRow;

                            }

                            ////worksheet.Cells["A36:AL43"].Copy(worksheet.Cells["A66:AK71"]);

                            #endregion PART II : ESTIMATE EXPENSE

                            #region PART III : APPROVAL BY

                            int count_approval = dataX.approval_details.Count;
                            int firstRow_approval = firstRow_expense_detail + 7;

                            if (count_approval > 1)
                                worksheet.InsertRow(firstRow_approval + 1, (count_approval - 1), firstRow_approval); // insert row approval by

                            foreach (var dr in dataX.approval_details)
                            {

                                worksheet.Cells["C" + firstRow_approval.ToString()].Value = dr.no + ".";
                                worksheet.Cells["D" + firstRow_approval.ToString()].Value = dr.emp_id;
                                worksheet.Cells["G" + firstRow_approval.ToString()].Value = dr.emp_name;
                                worksheet.Cells["M" + firstRow_approval.ToString()].Value = dr.org_unit;
                                worksheet.Cells["Q" + firstRow_approval.ToString()].Value = dr.line_approval;
                                worksheet.Cells["W" + firstRow_approval.ToString()].Value = dr.org_unit_line;
                                worksheet.Cells["Z" + firstRow_approval.ToString()].Value = dr.approved_date_line;
                                worksheet.Cells["AB" + firstRow_approval.ToString()].Value = dr.cap_approval;
                                worksheet.Cells["AG" + firstRow_approval.ToString()].Value = dr.org_unit_cap;
                                worksheet.Cells["AK" + firstRow_approval.ToString()].Value = dr.approved_date_cap;

                                firstRow_approval++;
                            }

                            worksheet.Cells["D" + (firstRow_approval).ToString()].Value = dataX.approval_by[0].remark;

                            #endregion PART III : APPROVAL BY


                            ExcelWorksheet datasheet = ExcelPkg.Workbook.Worksheets["data_sheet"];
                            //to_submit
                            //to_share
                            //other
                            //the_budget
                            //shall_seek
                            datasheet.Cells["B1"].Value = dataX.travel_details[0].to_submit == "Y" ? true : false;
                            datasheet.Cells["B2"].Value = dataX.travel_details[0].to_share == "Y" ? true : false;
                            datasheet.Cells["B3"].Value = dataX.travel_details[0].other == "Y" ? true : false;
                            //datasheet.Cells["B4"].Value = dataX.approval_by[0].the_budget == "Y" ? true : false;
                            //datasheet.Cells["B5"].Value = dataX.approval_by[0].shall_seek == "Y" ? true : false;

                            datasheet.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;

                            worksheet.Name = doc_id;
                            ExcelPkg.SaveAs(new FileInfo(output));
                            status = "true";
                        }

                    }
                    catch (Exception ex) { msg = ex.Message.ToString(); }

                    dtResult.Rows[0]["status"] = status;
                    dtResult.Rows[0]["file_system_path"] = output;
                    dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                    dtResult.Rows[0]["file_outbound_name"] = (string.IsNullOrEmpty(msg) ? filesname : msg);
                }
                else
                {
                    dtResult.Rows[0]["status"] = $"LBApprovalReport dataX is null ";
                    dtResult.Rows[0]["file_system_path"] = "";
                    dtResult.Rows[0]["file_outbound_path"] = "";
                    dtResult.Rows[0]["file_outbound_name"] = "";
                }
            }
            catch (Exception ex)
            {
                dtResult.Rows[0]["status"] = $"LBApprovalReport Error:{ex.Message.ToString()}";
                dtResult.Rows[0]["file_system_path"] = "";
                dtResult.Rows[0]["file_outbound_path"] = "";
                dtResult.Rows[0]["file_outbound_name"] = "";
            }

            return dtResult;
        }

        public DataTable LBApprovalReportV1(string token, string doc_id, string state)
        {

            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");
            dtResult.Rows.Add(dtResult.NewRow());

            
            var value = new ApprovalFormModel
            {
                token_login = token,
                doc_id = doc_id,
            };
            searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
            ApprovalFormOutModel dataX = service.SearchApprovalForm(value);

            if (dataX != null)
            {

                var datetime2 = DateTime.Now.ToString("dMMMyyyy");
                var provice = dataX.travel_details[0].province?.ToString().Replace(" ", "_");
                var city = dataX.travel_details[0].city?.ToString();
                var bussinessdate = dataX.travel_details[0].business_date?.ToString().Replace(" ", "");

                string filesname = $"APPROVAL_FORM_{doc_id.ToUpper().Trim()}_{bussinessdate?.Trim()}_{provice}_{city}.xlsx";
                //var datetime1 = DateTime.Now.ToString("yyyyMMddHHmmssff");
                //string datenow = datetime1;
                //string fileName = $"APPROVAL_FORM_{doc_id.ToUpper().Trim()}_{datetime1}.xlsx";

                var output = ClassPathReport.genFilePath("temp", filesname);
                var outputUrl = ClassPathReport.genFullPath("temp", filesname);

                var pathTempplate = ClassPathReport.genFilePath("template", "2021_LB_Approval_Form.xlsx");
                FileInfo template = new FileInfo(pathTempplate);
                using (var package = new ExcelPackage(template))
                {
                    // save file temp ก่อนแล้วค่อยเขียนลง file temp
                    package.SaveAs(new FileInfo(output));
                }

                var status = "false";
                var msg = "";
                try
                {

                    FileInfo fileTemp = new FileInfo(output);
                    using (var ExcelPkg = new ExcelPackage(template))
                    {
                        ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.First();
                        //DevFix 20250129 0000 set style
                        worksheet.Cells.Style.Font.Name = "Arial";   // เปลี่ยนเป็นฟอนต์ที่ต้องการ
                        worksheet.Cells.Style.Font.Size = 12;       // ขนาดตัวอักษร

                        #region Header detail
                        if (state.ToLower().Trim() == "localtraining")
                        {
                            worksheet.Cells["C6"].Value = "LOCAL TRAINING TRIP";
                        }
                        worksheet.Cells["F8"].Value = dataX.requested_by;
                        worksheet.Cells["L8"].Value = dataX.org_unit_req;
                        worksheet.Cells["S8"].Value = dataX.on_behalf_of;
                        worksheet.Cells["Y8"].Value = dataX.org_unit_on_behalf;
                        worksheet.Cells["AI8"].Value = dataX.date_to_requested;


                        worksheet.Cells["G10"].Value = dataX.document_number;
                        worksheet.Cells["N10"].Value = dataX.document_status;
                        worksheet.Cells["T10"].Value = dataX.company;
                        worksheet.Cells["AB10"].Value = dataX.travel_type;
                        worksheet.Cells["AF10"].Value = dataX.travel_with;

                        #endregion Header detail

                        #region PART I : TRAVEL DETAILS
                        worksheet.Cells["F14"].Value = dataX.travel_details[0].travel_topic;
                        worksheet.Cells["V14"].Value = dataX.travel_details[0].province;
                        worksheet.Cells["AD14"].Value = dataX.travel_details[0].location;
                        //worksheet.Cells["AI15"].Value = dataX.travel_details[0].city;

                        worksheet.Cells["I15"].Value = dataX.travel_details[0].business_date;
                        worksheet.Cells["V15"].Value = dataX.travel_details[0].travel_date;
                        worksheet.Cells["AE15"].Value = dataX.travel_details[0].travel_duration;

                        worksheet.Cells["J16"].Value = dataX.travel_details[0].traveling_objective;

                        int count_summary = dataX.traveler_summary.Count;
                        worksheet.InsertRow(22, (count_summary - 1), 22); // insert row travel summary

                        int firstRow_summary = 22;

                        foreach (var dr in dataX.traveler_summary)
                        {
                            worksheet.Cells["T" + firstRow_summary.ToString() + ":Y" + firstRow_summary.ToString()].Merge = true;
                            worksheet.Cells["T" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells["Z" + firstRow_summary.ToString() + ":AD" + firstRow_summary.ToString()].Merge = true;
                            worksheet.Cells["Z" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells["AE" + firstRow_summary.ToString() + ":AL" + firstRow_summary.ToString()].Merge = true;
                            worksheet.Cells["AE" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            worksheet.Cells["C" + firstRow_summary.ToString()].Value = dr.no + ".";
                            worksheet.Cells["D" + firstRow_summary.ToString()].Value = dr.emp_id;
                            worksheet.Cells["G" + firstRow_summary.ToString()].Value = dr.emp_name;
                            worksheet.Cells["M" + firstRow_summary.ToString()].Value = dr.org_unit;
                            worksheet.Cells["Q" + firstRow_summary.ToString()].Value = dr.province;
                            worksheet.Cells["T" + firstRow_summary.ToString()].Value = dr.location;
                            worksheet.Cells["Z" + firstRow_summary.ToString()].Value = dr.travel_date;
                            worksheet.Cells["AE" + firstRow_summary.ToString()].Value = dr.budget_account;

                            firstRow_summary++;
                        }
                        // To approve the business trip according to the objective and description proposed. After the mission completed, the staff should
                        worksheet.Cells["C" + (firstRow_summary + 1).ToString() + ":AL" + (firstRow_summary + 1).ToString()].Merge = true;
                        worksheet.Cells["C" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        // To submit a report to Line Manager within 30 days after the trip is finished. 
                        worksheet.Cells["D" + (firstRow_summary + 2).ToString() + ":AL" + (firstRow_summary + 2).ToString()].Merge = true;
                        worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        // To share knowledge to concerned person using methods 
                        worksheet.Cells["D" + (firstRow_summary + 3).ToString() + ":M" + (firstRow_summary + 3).ToString()].Merge = true;
                        worksheet.Cells["D" + firstRow_summary.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        worksheet.Cells["N" + (firstRow_summary + 3).ToString()].Value = dataX.travel_details[0].to_share_remark;
                        worksheet.Cells["E" + (firstRow_summary + 4).ToString()].Value = dataX.travel_details[0].other_remark;

                        #endregion PART I : TRAVEL DETAILS

                        #region PART II : ESTIMATE EXPENSE

                        int firstRow_expense = firstRow_summary + 7;
                        int firstRow_expense_detail = firstRow_summary + 9;
                        int betweenRow = 4;
                        int countExpenseDeatil = dataX.estimate_expense_details.Count;
                        //int rowCopy = 1;

                        worksheet.Cells["AK" + firstRow_expense.ToString()].Value = "Exchange Rates as of " + dataX.estimate_expense[0].exchange_rates_as_of + " THB/USD";

                        int x1 = 0;
                        int x2 = 0;

                        for (int i = 0; i < dataX.estimate_expense_details.Count - 1; i++)
                        {
                            int insRow = (firstRow_expense_detail) + betweenRow;
                            worksheet.InsertRow(insRow, betweenRow, firstRow_expense_detail);

                            int x3 = (((firstRow_expense_detail) + betweenRow));
                            int x4 = (x3 + betweenRow) - 1;
                            if (i == 0)
                            {
                                x1 = (firstRow_expense_detail);
                                x2 = ((firstRow_expense_detail) + betweenRow - 1);
                            }
                            worksheet.Cells["A" + (x1).ToString() + ":AL" + (x2).ToString()].Copy(worksheet.Cells["A" + (x3).ToString() + ":AL" + (x4).ToString()]);

                            firstRow_expense_detail = firstRow_expense_detail + betweenRow;

                        }

                        firstRow_expense_detail = firstRow_summary + 9;
                        int countRow = 0;
                        foreach (var dr in dataX.estimate_expense_details)
                        {
                            countRow++;

                            worksheet.Cells["C" + (firstRow_expense_detail).ToString()].Value = dr.no + ".";
                            worksheet.Cells["D" + (firstRow_expense_detail).ToString()].Value = dr.emp_id;
                            worksheet.Cells["G" + (firstRow_expense_detail).ToString()].Value = dr.emp_name;
                            worksheet.Cells["M" + (firstRow_expense_detail).ToString()].Value = dr.org_unit;
                            worksheet.Cells["Q" + (firstRow_expense_detail).ToString()].Value = dr.province;
                            worksheet.Cells["T" + (firstRow_expense_detail).ToString()].Value = dr.location;
                            worksheet.Cells["Z" + (firstRow_expense_detail).ToString()].Value = dr.travel_date;
                            worksheet.Cells["AE" + (firstRow_expense_detail).ToString()].Value = dr.budget_account;

                            //worksheet.Cells["G" + (firstRow_expense_detail + 1).ToString()].Value = dr.allowance;
                            //worksheet.Cells["O" + (firstRow_expense_detail + 1).ToString()].Value = dr.accommodation;
                            //worksheet.Cells["X" + (firstRow_expense_detail + 1).ToString()].Value = dr.transportation;
                            //worksheet.Cells["AG" + (firstRow_expense_detail + 1).ToString()].Value = dr.others;

                            worksheet.Cells["J" + (firstRow_expense_detail + 1).ToString()].Value = dr.allowance;
                            worksheet.Cells["R" + (firstRow_expense_detail + 1).ToString()].Value = dr.accommodation;
                            worksheet.Cells["AA" + (firstRow_expense_detail + 1).ToString()].Value = dr.transportation;
                            worksheet.Cells["AJ" + (firstRow_expense_detail + 1).ToString()].Value = dr.others;

                            worksheet.Cells["G" + (firstRow_expense_detail + 2).ToString()].Value = dr.remark;
                            worksheet.Cells["AJ" + (firstRow_expense_detail + 2).ToString()].Value = dr.total_expenses;

                            //worksheet.Cells["H" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing;
                            //worksheet.Cells["P" + (firstRow_expense_detail + 3).ToString()].Value = dr.luggage_clothing_valid;
                            //worksheet.Cells["X" + (firstRow_expense_detail + 3).ToString()].Value = dr.insurance;
                            //worksheet.Cells["AG" + (firstRow_expense_detail + 3).ToString()].Value = dr.total_expenses;

                            //worksheet.Cells["F" + (firstRow_expense_detail + 4).ToString()].Value = dr.remark;

                            if (countRow == dataX.estimate_expense_details.Count)
                            {
                                worksheet.Cells["AG" + (firstRow_expense_detail + 5).ToString()].Value = dataX.estimate_expense[0].grand_total_expenses;
                            }

                            firstRow_expense_detail = firstRow_expense_detail + betweenRow;

                        }

                        ////worksheet.Cells["A36:AL43"].Copy(worksheet.Cells["A66:AK71"]);

                        #endregion PART II : ESTIMATE EXPENSE

                        #region PART III : APPROVAL BY

                        int count_approval = dataX.approval_details.Count;
                        int firstRow_approval = firstRow_expense_detail + 7;

                        worksheet.InsertRow(firstRow_approval, (count_approval - 1), firstRow_approval); // insert row approval by

                        foreach (var dr in dataX.approval_details)
                        {

                            worksheet.Cells["C" + firstRow_approval.ToString()].Value = dr.no + ".";
                            worksheet.Cells["D" + firstRow_approval.ToString()].Value = dr.emp_id;
                            worksheet.Cells["G" + firstRow_approval.ToString()].Value = dr.emp_name;
                            worksheet.Cells["M" + firstRow_approval.ToString()].Value = dr.org_unit;
                            worksheet.Cells["Q" + firstRow_approval.ToString()].Value = dr.line_approval;
                            worksheet.Cells["W" + firstRow_approval.ToString()].Value = dr.org_unit_line;
                            worksheet.Cells["Z" + firstRow_approval.ToString()].Value = dr.approved_date_line;
                            worksheet.Cells["AB" + firstRow_approval.ToString()].Value = dr.cap_approval;
                            worksheet.Cells["AG" + firstRow_approval.ToString()].Value = dr.org_unit_cap;
                            worksheet.Cells["AK" + firstRow_approval.ToString()].Value = dr.approved_date_cap;

                            firstRow_approval++;
                        }

                        worksheet.Cells["D" + (firstRow_approval).ToString()].Value = dataX.approval_by[0].remark;

                        #endregion PART III : APPROVAL BY


                        ExcelWorksheet datasheet = ExcelPkg.Workbook.Worksheets["data_sheet"];
                        //to_submit
                        //to_share
                        //other
                        //the_budget
                        //shall_seek
                        datasheet.Cells["B1"].Value = dataX.travel_details[0].to_submit == "Y" ? true : false;
                        datasheet.Cells["B2"].Value = dataX.travel_details[0].to_share == "Y" ? true : false;
                        datasheet.Cells["B3"].Value = dataX.travel_details[0].other == "Y" ? true : false;
                        //datasheet.Cells["B4"].Value = dataX.approval_by[0].the_budget == "Y" ? true : false;
                        //datasheet.Cells["B5"].Value = dataX.approval_by[0].shall_seek == "Y" ? true : false;

                        datasheet.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;

                        worksheet.Name = doc_id;
                        ExcelPkg.SaveAs(new FileInfo(output));
                        status = "true";
                    }

                }
                catch (Exception ex) { msg = ex.Message.ToString(); }

                dtResult.Rows[0]["status"] = status;
                dtResult.Rows[0]["file_system_path"] = output;
                dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                dtResult.Rows[0]["file_outbound_name"] = (string.IsNullOrEmpty(msg) ? filesname : msg);
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
