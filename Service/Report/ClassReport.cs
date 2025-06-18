
using Newtonsoft.Json;
using OfficeOpenXml;
using ExcelPackage = OfficeOpenXml.ExcelPackage;
using OfficeOpenXml.Style;
using System.Data;
using System.Drawing;
using top.ebiz.service.Models.Traveler_Profile;
using static top.ebiz.service.Service.Report.ClassReportModel;


namespace top.ebiz.service.Service.Report
{
    public class ClassPathReport
    {
        public static string genFullPath(string page_name, string file_name)
        {
            var folderPath = $"/DocumentFile/{page_name.ToLower()}";
            // Retrieve ServerPathAPI from configuration
            var serverPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI() ?? "";
            // Construct relative path and URL
            var relativePath = $"{folderPath}/{file_name}".Replace("\\", "/");
            var fullUrl = $"{serverPathAPI.TrimEnd('/')}{Uri.EscapeUriString(relativePath)}";

            // Validate URL integrity
            if (!fullUrl.StartsWith(serverPathAPI, StringComparison.OrdinalIgnoreCase))
            { 
                return "";
            }
            else
            {
                return fullUrl;
            }
        }
        public static string genFilePath(string page_name, string file_name)
        {   
            string content_path = $"DocumentFile/{page_name.ToLower()}";
            string content_name = file_name;// data.txt,LETTER_TEMPLATE_TOP.docx ;
            string finalFilePath = "";
            var fileInfo = FileUtil.GetFileInfo($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot/{content_path}/{content_name}");
            finalFilePath = fileInfo?.FullName ?? "";
            if (finalFilePath == "")
            {
                finalFilePath = Path.Combine(
              AppDomain.CurrentDomain.BaseDirectory,
              "wwwroot",
              "DocumentFile",
              page_name.ToLower(),
              file_name
          );
            }
            return finalFilePath.Replace('/', '\\');
        }
    }
    public class ClassReport
    {
        private DataTable empty_document()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("file_system_path");
            dt.Columns.Add("file_outbound_path");
            dt.Columns.Add("file_outbound_name");
            dt.Columns.Add("status");
            dt.Rows.Add(dt.NewRow());

            string fileName = "empty.docx";
            var output = ClassPathReport.genFilePath("temp", fileName);
            var outputUrl = ClassPathReport.genFullPath("temp", fileName);

            dt.Rows[0]["file_system_path"] = output;
            dt.Rows[0]["file_outbound_path"] = outputUrl;
            dt.Rows[0]["file_outbound_name"] = fileName;
            dt.Rows[0]["status"] = "true";

            return dt;
        }
        public string Report(ReportParamModel value)
        {
            var param = value.param ?? "";
            var method = value.method ?? "";
            string msg = "";

            if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; }
         ;
            if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; }
         ;

            DataTable dt = new DataTable();
            try
            {
                dt = new DataTable();
                if (!string.IsNullOrEmpty(param))
                {
                    object obj = JsonConvert.DeserializeObject(param);
                    dt = (DataTable)JsonConvert.DeserializeObject(param, typeof(DataTable));
                    dt.Columns.Add("file_system_path");
                    dt.Columns.Add("file_outbound_path");
                    dt.Columns.Add("file_outbound_name");
                    dt.Columns.Add("status");
                }

                if (dt?.Rows.Count > 0)
                {
                    if (method == "insurance")
                    {
                        ClassReportOpenXml classReport = new ClassReportOpenXml();
                        (dt, msg) = classReport.ReportInsurance(value);

                    }
                    else if (method == "employee_letter")
                    {
                        ClassReportOpenXml classReport = new ClassReportOpenXml();
                        (dt, msg) = classReport.ReportEmployeeLetter(value);

                    }
                    else
                    {
                        string fileName = "empty.docx";
                        var output = ClassPathReport.genFilePath("template", fileName);
                        var outputUrl = ClassPathReport.genFullPath("template", fileName);

                        dt.Rows[0]["file_system_path"] = output;
                        dt.Rows[0]["file_outbound_path"] = outputUrl;
                        dt.Rows[0]["file_outbound_name"] = fileName;
                        dt.Rows[0]["status"] = "true";
                    }
                }
                else
                {
                    dt = new DataTable();
                    dt.Columns.Add("status");
                    dt.NewRow();

                    dt.Rows[0]["file_system_path"] = "";
                    dt.Rows[0]["file_outbound_path"] = "";
                    dt.Rows[0]["file_outbound_name"] = "";
                    dt.Rows[0]["status"] = "No data.";
                }

            }
            catch (Exception ex)
            {
                dt = new DataTable();
                dt.Columns.Add("file_system_path");
                dt.Columns.Add("file_outbound_path");
                dt.Columns.Add("file_outbound_name");
                dt.Columns.Add("status");
                dt.Rows.Add(dt.NewRow());

                dt.Rows[0]["file_system_path"] = "";
                dt.Rows[0]["file_outbound_path"] = "";
                dt.Rows[0]["file_outbound_name"] = "";
                dt.Rows[0]["status"] = ex.ToString();
            }

            dt.TableName = "dtResult";
            DataSet ds = new DataSet();

            ds.Tables.Add(dt);

            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

            return JSONresult;
        }

        //       public string Report(ReportParamModel value)
        //       {
        //           var param = value.param ?? "";
        //           var method = value.method ?? "";

        //           if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; };
        //           if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; };

        //           DataTable dt = new DataTable();
        //           try
        //           {
        //               dt = new DataTable();
        //               if (!string.IsNullOrEmpty(param))
        //               {
        //                   object obj = JsonConvert.DeserializeObject(param);
        //                   dt = (DataTable)JsonConvert.DeserializeObject(param, typeof(DataTable));
        //                   dt.Columns.Add("file_system_path");
        //                   dt.Columns.Add("file_outbound_path");
        //                   dt.Columns.Add("file_outbound_name");
        //                   dt.Columns.Add("status");
        //               }

        //               if (dt?.Rows.Count > 0)
        //               {
        //                   if (method == "insurance")
        //                   {
        //                       string datetime = DateTime.Now.ToString("ddMMyyhhmmss");
        //                       string reportDate = DateTime.Now.ToString("dd MMMM yyyy");
        //                       var template2 = ClassPathReport.genFilePath("template", "Travel_Insurance_Form2.docx");
        //                       var template = ClassPathReport.genFilePath("template", "Starr_BTA_Application_form_original.docx");

        //                       var durationValue = dt.Rows[0]["duration"].ToString();

        //                       string policyHolder = dt.Rows[0]["policyHolder"].ToString();
        //                       string passportNo = dt.Rows[0]["passportNo"].ToString();
        //                       string companyName = dt.Rows[0]["companyName"].ToString();
        //                       string address = dt.Rows[0]["address"].ToString();
        //                       string occupation = dt.Rows[0]["occupation"].ToString();

        //                       string age = dt.Rows[0]["age"].ToString();
        //                       string tel = dt.Rows[0]["tel"].ToString();
        //                       string fax = dt.Rows[0]["fax"].ToString();
        //                       string nameOfBeneficiary = dt.Rows[0]["nameOfBeneficiary"].ToString();
        //                       string relationship = dt.Rows[0]["relationship"].ToString();

        //                       string pdateFrom = dt.Rows[0]["pdateFrom"].ToString();
        //                       string pdateTo = dt.Rows[0]["pdateTo"].ToString();
        //                       string duration = dt.Rows[0]["duration"].ToString();
        //                       string insPlan = dt.Rows[0]["insPlan"].ToString();
        //                       string destination = dt.Rows[0]["destination"].ToString();
        //                       string broker = dt.Rows[0]["broker"].ToString();


        //                       string safePolicyHolder = policyHolder.Trim().Replace(" ", "_").Replace(".", ""); 
        //                       string safeDestination = destination.Trim().Replace(" ", "_");
        //                       string safeDatetime = datetime.Trim().Replace(" ", "_");

        //                       string fileName = $"{safePolicyHolder}_{safeDestination}_{safeDatetime}.docx";
        //                       var output = ClassPathReport.genFilePath("temp", fileName);
        //                       var outputUrl = ClassPathReport.genFullPath("temp", fileName);

        //                       using (Xceed.Words.NET.DocX myDoc = Xceed.Words.NET.DocX.Load(template))
        //                       {
        //                           myDoc.ReplaceText("<<policyHolder>>", policyHolder);
        //                           myDoc.ReplaceText("<<passportNo>>", passportNo);
        //                           myDoc.ReplaceText("<<companyName>>", companyName);
        //                           myDoc.ReplaceText("<<address>>", address);
        //                           myDoc.ReplaceText("<<occupation>>", occupation);

        //                           myDoc.ReplaceText("<<age>>", age);
        //                           myDoc.ReplaceText("<<tel>>", tel);
        //                           myDoc.ReplaceText("<<fax>>", fax);
        //                           myDoc.ReplaceText("<<nameOfBeneficiary>>", nameOfBeneficiary);
        //                           myDoc.ReplaceText("<<relationship>>", relationship);
        //                           myDoc.ReplaceText("<<durationValue>>", durationValue);

        //                           myDoc.ReplaceText("<<pdateFrom>>", pdateFrom);
        //                           myDoc.ReplaceText("<<pdateTo>>", pdateTo);
        //                           myDoc.ReplaceText("<<duration>>", duration);
        //                           myDoc.ReplaceText("<<insPlan>>", insPlan);
        //                           myDoc.ReplaceText("<<destination>>", destination);
        //                           myDoc.ReplaceText("<<reportDate>>", reportDate);
        //                           myDoc.ReplaceText("<<broker>>", broker);

        //                           myDoc.SaveAs(output);


        //                           dt.Rows[0]["file_system_path"] = output;
        //                           dt.Rows[0]["file_outbound_path"] = outputUrl;
        //                           dt.Rows[0]["file_outbound_name"] = fileName;
        //                           dt.Rows[0]["status"] = "true";
        //                       }
        //                   }
        //                   else if (method == "employee_letter")
        //                   {
        //                       string datetime = DateTime.Now.ToString("ddMMyyhhmmss");
        //                       string reportDate = DateTime.Now.ToString("d MMMM yyyy");

        //                       string nameOfEmbassy1 = "...........................";
        //                       string nameOfEmployee = dt.Rows[0]["nameOfEmployee"].ToString();
        //                       string nameOfEmbassy = dt.Rows[0]["nameOfEmbassy"].ToString();
        //                       string gender = dt.Rows[0]["gender"].ToString();

        //                       string heShe1 = gender.ToLower() == "male".ToLower() ? "He" : "She"; //He/She
        //                       string heShe2 = gender.ToLower() == "male".ToLower() ? "he" : "she"; //he/she

        //                       string hisHer1 = gender.ToLower() == "male".ToLower() ? "His" : "Her"; //His/Her
        //                       string hisHer2 = gender.ToLower() == "male".ToLower() ? "his" : "her"; //his/her

        //                       string joinDate = dt.Rows[0]["joinDate"].ToString();

        //                       string position = dt.Rows[0]["position"].ToString();
        //                       string travelTopic = dt.Rows[0]["travelTopic"].ToString();
        //                       string cityCountry = dt.Rows[0]["cityCountry"].ToString();
        //                       string dateOfDeparture = dt.Rows[0]["dateOfDeparture"].ToString();
        //                       string company = dt.Rows[0]["company"].ToString();

        //                       string companyFullName = company.ToUpper() == "TOP" ? "Thai Oil Public Company Limited" : "Thaioil Energy Services Company Limited";
        //                       string nameSig1 = company.ToUpper() == "TOP" ? "Viroj Wongsathirayakhun" : "Peerasud Sritawat Na Ayudhaya";
        //                       string positionSig1 = company.ToUpper() == "TOP" ? "Executive Vice President-Organization Effectiveness" : "Human Resources and Finance Manager";

        //                       string nameSig2 = company.ToUpper() == "TOP" ? "Ratri Chingchitra" : "Sukulya Veeradaechapol";
        //                       string positionSig2 = company.ToUpper() == "TOP" ? "Manager Compensation and Information System" : "HR Services Manager";

        //                       // string templateName = company.ToUpper() == "TOP" ? "EMPLOYEE LETTER_TOP" : "EMPLOYEE LETTER_TES";

        //                       // var template = Path.Combine(Server.MapPath("~/template"), "EMPLOYEE LETTER_TEMPLATE" + ".docx");
        //                       string templateName = company.ToUpper() == "TOP" ? "EMPLOYEE LETTER_TOP" : "EMPLOYEE LETTER_TES";
        //                       string templateNameCompany = company.ToUpper() == "EMPLOYEE LETTER_TEMPLATE_TOP" ? "TOP" : "EMPLOYEE LETTER_TEMPLATE_TES";

        //                      // var template = ClassPathReport.genFilePath("template", $"templateNameCompany.docx");  // Path.Combine(Server.MapPath("~/template"), templateNameCompany + ".docx");


        //var template = ClassPathReport.genFilePath("template", $"EMPLOYEE LETTER_TEMPLATE.docx");  // Path.Combine(Server.MapPath("~/template"), templateNameCompany + ".docx");

        //                       string fileName = $"{templateName}_{nameOfEmployee?.Replace(' ', '_')}_{datetime}.docx";

        //                       var output = ClassPathReport.genFilePath("temp", fileName);
        //                       var outputUrl = ClassPathReport.genFullPath("temp", fileName);

        //                       using (Xceed.Words.NET.DocX myDoc = Xceed.Words.NET.DocX.Load(template))
        //                       {
        //                           myDoc.ReplaceText("<<nameOfEmbassy1>>", nameOfEmbassy1);
        //                           myDoc.ReplaceText("<<nameOfEmbassy>>", nameOfEmbassy);
        //                           myDoc.ReplaceText("<<reportDate>>", reportDate);
        //                           myDoc.ReplaceText("<<nameOfEmployee>>", nameOfEmployee);
        //                           myDoc.ReplaceText("<<companyFullName>>", companyFullName);
        //                           myDoc.ReplaceText("<<heShe1>>", heShe1);
        //                           myDoc.ReplaceText("<<heShe2>>", heShe2);
        //                           myDoc.ReplaceText("<<hisHer1>>", hisHer1);
        //                           myDoc.ReplaceText("<<hisHer2>>", hisHer2);

        //                           myDoc.ReplaceText("<<joinDate>>", joinDate);
        //                           myDoc.ReplaceText("<<position>>", position);
        //                           myDoc.ReplaceText("<<travelTopic>>", travelTopic);
        //                           myDoc.ReplaceText("<<cityCountry>>", cityCountry);
        //                           myDoc.ReplaceText("<<dateOfDeparture>>", dateOfDeparture);

        //                           myDoc.ReplaceText("<<nameSig1>>", nameSig1);
        //                           myDoc.ReplaceText("<<positionSig1>>", positionSig1);
        //                           myDoc.ReplaceText("<<nameSig2>>", nameSig2);
        //                           myDoc.ReplaceText("<<positionSig2>>", positionSig2);


        //                           myDoc.SaveAs(output);

        //                           dt.Rows[0]["file_system_path"] = output;
        //                           dt.Rows[0]["file_outbound_path"] = outputUrl;
        //                           dt.Rows[0]["file_outbound_name"] = fileName;
        //                           dt.Rows[0]["status"] = "true";
        //                       }
        //                   }
        //                   else
        //                   {
        //                       string fileName = "empty.docx";
        //                       var output = ClassPathReport.genFilePath("template", fileName);
        //                       var outputUrl = ClassPathReport.genFullPath("template", fileName);

        //                       dt.Rows[0]["file_system_path"] = output;
        //                       dt.Rows[0]["file_outbound_path"] = outputUrl;
        //                       dt.Rows[0]["file_outbound_name"] = fileName;
        //                       dt.Rows[0]["status"] = "true";
        //                   }
        //               }
        //               else
        //               {
        //                   dt = new DataTable();
        //                   dt.Columns.Add("status");
        //                   dt.NewRow();

        //                   dt.Rows[0]["file_system_path"] = "";
        //                   dt.Rows[0]["file_outbound_path"] = "";
        //                   dt.Rows[0]["file_outbound_name"] = "";
        //                   dt.Rows[0]["status"] = "No data.";
        //               }

        //           }
        //           catch (Exception ex)
        //           {
        //               dt = new DataTable();
        //               dt.Columns.Add("file_system_path");
        //               dt.Columns.Add("file_outbound_path");
        //               dt.Columns.Add("file_outbound_name");
        //               dt.Columns.Add("status");
        //               dt.Rows.Add(dt.NewRow());

        //               dt.Rows[0]["file_system_path"] = "";
        //               dt.Rows[0]["file_outbound_path"] = "";
        //               dt.Rows[0]["file_outbound_name"] = "";
        //               dt.Rows[0]["status"] = ex.ToString();
        //           }

        //           dt.TableName = "dtResult";
        //           DataSet ds = new DataSet();

        //           ds.Tables.Add(dt);

        //           string JSONresult;
        //           JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

        //           return JSONresult;
        //       }

        //public string TravelRecordX(ReportParamxJsonModel value)
        //{
        //    var param = value.param ?? "";
        //    var method = value.method ?? "";
        //    var jsondata = value.jsondata ?? "";

        //    if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; };
        //    if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; };
        //    if (string.IsNullOrEmpty(jsondata)) { return "IsNullOrEmpty JSONData."; };



        //    DataTable dtParam = new DataTable();
        //    if (param != "")
        //    {
        //        var data = JsonConvert.DeserializeObject<ParamTravelRecord>(jsondata);

        //        // 3. Create DataTable with all columns
        //        DataTable dt = new DataTable();

        //        // Add all main columns
        //        dt.Columns.Add("token_login", typeof(string));
        //        dt.Columns.Add("doc_id", typeof(string));
        //        dt.Columns.Add("country", typeof(string));
        //        dt.Columns.Add("date_from", typeof(string));
        //        dt.Columns.Add("date_to", typeof(string));
        //        dt.Columns.Add("travel_type", typeof(string));
        //        dt.Columns.Add("emp_id", typeof(string));
        //        dt.Columns.Add("section", typeof(string));
        //        dt.Columns.Add("department", typeof(string));
        //        dt.Columns.Add("function", typeof(string));
        //        dt.Columns.Add("first_travel_id", typeof(string));

        //        // 4. Add data row with proper null handling
        //        DataRow row = dt.NewRow();

        //        // Helper function to handle null strings
        //        object GetValueOrDBNull(string value) => string.IsNullOrEmpty(value) ? DBNull.Value : (object)value;

        //        row["token_login"] = GetValueOrDBNull(data.token_login);
        //        row["doc_id"] = GetValueOrDBNull(data.doc_id);
        //        row["country"] = GetValueOrDBNull(data.country);
        //        row["date_from"] = GetValueOrDBNull(data.date_from);
        //        row["date_to"] = GetValueOrDBNull(data.date_to);
        //        row["travel_type"] = GetValueOrDBNull(data.travel_type);
        //        row["emp_id"] = GetValueOrDBNull(data.emp_id);
        //        row["section"] = GetValueOrDBNull(data.section);
        //        row["department"] = GetValueOrDBNull(data.department);
        //        row["function"] = GetValueOrDBNull(data.function);
        //        row["first_travel_id"] = data.travel_list?.FirstOrDefault()?.id != null ?
        //                                 (object)data.travel_list.First().id : DBNull.Value;

        //        dt.Rows.Add(row);

        //        // 5. (Optional) Create separate DataTable for travel_list
        //        DataTable travelListTable = new DataTable();
        //        travelListTable.Columns.Add("id", typeof(string));

        //        if (data.travel_list != null)
        //        {
        //            foreach (var item in data.travel_list)
        //            {
        //                travelListTable.Rows.Add(GetValueOrDBNull(item.id));
        //            }
        //        }

        //        ClassDataReportTravelerProfile rp = new ClassDataReportTravelerProfile();


        //        string JSONresult = rp.ReportTravelRecords(dt, param);

        //        return JSONresult;
        //    }
        //    else
        //    {
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("file_system_path");
        //        dt.Columns.Add("file_outbound_path");
        //        dt.Columns.Add("file_outbound_name");
        //        dt.Columns.Add("status");
        //        dt.Rows.Add(dt.NewRow());

        //        dt.Rows[0]["file_system_path"] = "";
        //        dt.Rows[0]["file_outbound_path"] = "";
        //        dt.Rows[0]["file_outbound_name"] = "";
        //        dt.Rows[0]["status"] = "No parameter.";

        //        dt.TableName = "dtResult";
        //        DataSet ds = new DataSet();

        //        ds.Tables.Add(dt);

        //        string JSONresult;
        //        JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

        //        return JSONresult;
        //    }
        //}
        private string CreateErrorResponse(string errorMessage)
        {

            try
            {
                DataTable errorTable = new DataTable();
                errorTable.Columns.Add("error", typeof(string));
                errorTable.Rows.Add(errorMessage);


                DataSet ds = new DataSet();
                ds.Tables.Add(errorTable);
                errorTable.TableName = "dtError";


                return JsonConvert.SerializeObject(ds, Formatting.Indented);
            }
            catch

            {
                return @"{""dtError"":[{""error"":""ไม่สามารถสร้างข้อความผิดพลาดได้""}]}";
            }
        }
        private ParamTravelRecord TryParseJsonData(string jsondata)
        {
            if (string.IsNullOrEmpty(jsondata))
            {
                return null;
            }

            try
            {
                var unescapedJson = JsonConvert.DeserializeObject<string>(jsondata);
                return JsonConvert.DeserializeObject<ParamTravelRecord>(unescapedJson);
            }
            catch
            {

                return null;
            }
        }
        private DataTable CreateMainDataTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("token_login", typeof(string));
            dt.Columns.Add("doc_id", typeof(string));
            dt.Columns.Add("country", typeof(string));
            dt.Columns.Add("date_from", typeof(string));
            dt.Columns.Add("date_to", typeof(string));
            dt.Columns.Add("travel_type", typeof(string));
            dt.Columns.Add("emp_id", typeof(string));
            dt.Columns.Add("section", typeof(string));
            dt.Columns.Add("department", typeof(string));
            dt.Columns.Add("function", typeof(string));
            dt.Columns.Add("first_travel_id", typeof(string));
            dt.Columns.Add("travel_list", typeof(string));
            return dt;
        }
        private object SafeDbValue(string value)
        {
            return string.IsNullOrEmpty(value) ? DBNull.Value : (object)value;
        }

        public string TravelRecordX(ReportParamxJsonModel request)
        {
            try
            {
                // 1. Validate input
                if (request == null)
                {
                    return CreateErrorResponse("ข้อมูลคำขอไม่ถูกต้อง");
                }

                if (request.param == null)
                {
                    return CreateErrorResponse("ข้อมูลพารามิเตอร์ไม่ถูกต้อง");
                }

                // 2. Prepare data
                var paramData = request.param;
                var jsonData = TryParseJsonData(request.jsondata);

                // 3. Create and populate model
                var travelRecord = new TravelRecordModel
                {
                    token_login = jsonData.token_login,
                    doc_id = jsonData.doc_id,
                    country = jsonData.country,
                    date_from = jsonData.date_from,
                    date_to = jsonData.date_to,
                    travel_type = jsonData.travel_type,
                    emp_id = jsonData.emp_id,
                    section = jsonData.section,
                    department = jsonData.department,
                    function = paramData.function,
                    travel_list = new List<traveltypeList>() // Initialize empty list
                };

                // Handle travel_list data
                if (jsonData?.travel_list != null)
                {
                    travelRecord.travel_list = jsonData.travel_list
                        .Select(t => new traveltypeList { id = t.id })
                        .ToList();
                }

                // 4. Create DataTable and add row
                DataTable mainTable = CreateMainDataTable();
                DataRow row = mainTable.NewRow();

                // Set default travel_list if empty
                var travelListToSerialize = travelRecord.travel_list.Any()
                    ? travelRecord.travel_list
                    : new List<traveltypeList> { new traveltypeList { id = "ob" } };

                // Fill data row
                row["token_login"] = SafeDbValue(travelRecord.token_login);
                row["doc_id"] = SafeDbValue(travelRecord.doc_id);
                row["country"] = SafeDbValue(travelRecord.country);
                row["date_from"] = SafeDbValue(travelRecord.date_from);
                row["date_to"] = SafeDbValue(travelRecord.date_to);
                row["travel_type"] = SafeDbValue(
                    travelRecord.travel_type ??
                    travelRecord.travel_list.FirstOrDefault()?.id
                );
                row["emp_id"] = SafeDbValue(travelRecord.emp_id);
                row["section"] = SafeDbValue(travelRecord.section);
                row["department"] = SafeDbValue(travelRecord.department);
                row["function"] = SafeDbValue(travelRecord.function);
                row["first_travel_id"] = SafeDbValue(paramData.travel_type_name);
                row["travel_list"] = SafeDbValue(JsonConvert.SerializeObject(travelListToSerialize));

                // Add the row to the table (only once)
                mainTable.Rows.Add(row);

                // 5. Generate report
                var rp = new ClassDataReportTravelerProfile();
                string reportResult = rp.ReportTravelRecords(mainTable, "TravelRecordX");

                if (string.IsNullOrEmpty(reportResult))
                {
                    throw new Exception("การสร้างรายงานคืนค่าข้อมูลว่างเปล่า");
                }

                return reportResult;
            }
            catch (JsonException jsonEx)
            {
                return CreateErrorResponse($"รูปแบบข้อมูลไม่ถูกต้อง: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}");
            }
        }
        public string TravelRecord(ReportParamJsonModel value)
        {
            var param = value.param ?? "";
            var method = value.method ?? "";
            var jsondata = value.jsondata ?? "";

            if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; }
            ;
            if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; }
            ;
            if (string.IsNullOrEmpty(jsondata)) { return "IsNullOrEmpty JSONData."; }
            ;

            string ret = "";
            DataSet ds = new DataSet();
            try
            {

                DataTable dt = new DataTable();
                if (jsondata != "")
                {
                    dt = (DataTable)JsonConvert.DeserializeObject(jsondata, typeof(DataTable));
                }
                DataTable dtParam = new DataTable();
                if (param != "")
                {
                    //var result = JsonConvert.DeserializeObject<paramTravelRecord>(param);
                    dtParam = (DataTable)JsonConvert.DeserializeObject(param, typeof(DataTable));
                    //Travel_List[] xxx = new Travel_List[1];
                    //xxx[0].id = "ob";
                    //xxx[1].id = "lb";
                    //var paramX = new paramTravelRecord { token_login = "", doc_id = "", country = "", date_from = "1 Jan 2021", date_to = "31 Dec 2021", travel_type = "ob", emp_id = "", section = "", department = "", function = "", travel_list = xxx };
                }


                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.Add("Travel Record (E-BIZ)");

                //hide gridline
                worksheet.View.ShowGridLines = false;
                string datetime = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

                //int columnWidth = 16;

                #region set column width & text warp
                //worksheet.Column(8).Style.WrapText = true;

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

                worksheet.Column(1).Style.WrapText = true;
                worksheet.Column(2).Style.WrapText = true;
                worksheet.Column(3).Style.WrapText = true;
                worksheet.Column(4).Style.WrapText = true;
                worksheet.Column(5).Style.WrapText = true;
                worksheet.Column(6).Style.WrapText = true;
                worksheet.Column(7).Style.WrapText = true;

                worksheet.Column(8).Style.WrapText = true;
                worksheet.Column(9).Style.WrapText = true;
                worksheet.Column(10).Style.WrapText = true;
                worksheet.Column(11).Style.WrapText = true;
                worksheet.Column(12).Style.WrapText = true;
                worksheet.Column(13).Style.WrapText = true;
                worksheet.Column(14).Style.WrapText = true;
                worksheet.Column(15).Style.WrapText = true;
                worksheet.Column(16).Style.WrapText = true;
                worksheet.Column(17).Style.WrapText = true;
                worksheet.Column(18).Style.WrapText = true;
                worksheet.Column(19).Style.WrapText = true;

                worksheet.Column(20).Style.WrapText = true;
                worksheet.Column(21).Style.WrapText = true;
                worksheet.Column(22).Style.WrapText = true;
                worksheet.Column(23).Style.WrapText = true;
                worksheet.Column(24).Style.WrapText = true;
                worksheet.Column(25).Style.WrapText = true;
                worksheet.Column(26).Style.WrapText = true;
                worksheet.Column(27).Style.WrapText = true;
                worksheet.Column(28).Style.WrapText = true;
                worksheet.Column(29).Style.WrapText = true;
                worksheet.Column(30).Style.WrapText = true;
                worksheet.Column(31).Style.WrapText = true;
                #endregion set column width & text warp

                Color colFirstHeaderHex = ColorTranslator.FromHtml("#808080");
                string fontName = "Cordia New";

                string date_type = dtParam.Rows[0]["travel_type"].ToString() == "ob" || dtParam.Rows[0]["travel_type"].ToString() == "lb" ? "Business date" : dtParam.Rows[0]["travel_type"].ToString() == "ot" || dtParam.Rows[0]["travel_type"].ToString() == "lt" ? "Training date" : "Date";

                worksheet.Cells["A1"].Value = "Travel Record";
                worksheet.Cells["A1"].Style.Font.Size = 24;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Name = fontName;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(colFirstHeaderHex);
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells["A1:AE1"].Merge = true;
                worksheet.Row(1).Height = 40;


                worksheet.Cells["A2"].Value = "Print date : " + datetime;
                worksheet.Cells["A2"].Style.Font.Size = 10;
                worksheet.Cells["A2"].Style.Font.Bold = true;
                worksheet.Cells["A2"].Style.Font.Name = fontName;
                worksheet.Cells["A2:AE2"].Merge = true;

                worksheet.Cells["A3"].Value = "Travel Type : " + dtParam.Rows[0]["travel_type_name"].ToString() + ", Country : " + dtParam.Rows[0]["country"].ToString() + ", " + date_type + " : " + dtParam.Rows[0]["date_from"].ToString() + " - " + dtParam.Rows[0]["date_to"].ToString() + ", Employee : " + dtParam.Rows[0]["emp_id"].ToString() + ", Section : " + dtParam.Rows[0]["section"].ToString() + ", Department : " + dtParam.Rows[0]["department"].ToString() + ", Function : " + dtParam.Rows[0]["function"].ToString();
                worksheet.Cells["A3"].Style.Font.Size = 10;
                worksheet.Cells["A3"].Style.Font.Bold = true;
                worksheet.Cells["A3"].Style.Font.Name = fontName;
                worksheet.Cells["A3:AE3"].Merge = true;
                //worksheet.Cells["A2"].Value = "CAR SERVICE REPORT";
                //worksheet.Cells["A2"].Style.Font.Size = 18;
                //worksheet.Cells["A2"].Style.Font.Bold = true;
                //worksheet.Cells["A2"].Style.Font.Name = fontName;
                //worksheet.Cells["A2:C2"].Merge = true;
                //worksheet.Cells["A3"].Value = "Year : " + year + ", Month : " + month_str + ", Car service from : " + carfrom;
                //worksheet.Cells["A3"].Style.Font.Size = 12;
                //worksheet.Cells["A3"].Style.Font.Bold = true;
                //worksheet.Cells["A3"].Style.Font.Name = fontName;
                //worksheet.Cells["A3:C3"].Merge = true;

                Color colEmpHex = ColorTranslator.FromHtml("#1F4E78");
                Color colTravelHex = ColorTranslator.FromHtml("#FFD966");
                Color colTrainingHex = ColorTranslator.FromHtml("#375623");
                Color colBorderHex = ColorTranslator.FromHtml("#D9D9D9");
                //Color fontHeaderColor = Color.White;
                int fontSize = 12;


                #region HEADER TABLE

                worksheet.Cells["A4"].Value = "Employee Information";
                worksheet.Cells["A4"].Style.Font.Size = fontSize;
                worksheet.Cells["A4"].Style.Font.Bold = true;
                worksheet.Cells["A4"].Style.Font.Name = fontName;
                worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A4"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#D6DCE4"));
                worksheet.Cells["A4"].Style.Font.Color.SetColor(Color.Black);
                worksheet.Cells["A4:G4"].Merge = true;
                //worksheet.Cells["A4:G4"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //worksheet.Cells["A4:G4"].Style.Border.Top.Color.SetColor(Color.Black);

                worksheet.Cells["H4"].Value = "Travel Details";
                worksheet.Cells["H4"].Style.Font.Size = fontSize;
                worksheet.Cells["H4"].Style.Font.Bold = true;
                worksheet.Cells["H4"].Style.Font.Name = fontName;
                worksheet.Cells["H4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["H4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["H4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["H4"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFF2CC"));
                worksheet.Cells["H4"].Style.Font.Color.SetColor(Color.Black);
                worksheet.Cells["H4:S4"].Merge = true;

                worksheet.Cells["T4"].Value = "Traning Expense";
                worksheet.Cells["T4"].Style.Font.Size = fontSize;
                worksheet.Cells["T4"].Style.Font.Bold = true;
                worksheet.Cells["T4"].Style.Font.Name = fontName;
                worksheet.Cells["T4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["T4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["T4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["T4"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#A9D08E"));
                worksheet.Cells["T4"].Style.Font.Color.SetColor(Color.Black);
                worksheet.Cells["T4:AE4"].Merge = true;

                worksheet.Cells["A5:AE5"].AutoFilter = true;

                using (ExcelRange Rng = worksheet.Cells[5, 1])
                {
                    Rng.Value = "No";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    //Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //Rng.Style.Border.Top.Color.SetColor(Color.White);
                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(Color.White);
                    //Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //Rng.Style.Border.Bottom.Color.SetColor(Color.White);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(Color.White);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 2])
                {
                    Rng.Value = "Emp ID";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(Color.White);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(Color.White);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 3])
                {
                    Rng.Value = "Title";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(Color.White);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(Color.White);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 4])
                {
                    Rng.Value = "Name";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(Color.White);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(Color.White);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 5])
                {
                    Rng.Value = "Section";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(Color.White);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(Color.White);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 6])
                {
                    Rng.Value = "Department";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(Color.White);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(Color.White);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 7])
                {
                    Rng.Value = "Function";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(Color.White);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(Color.White);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colEmpHex);

                }

                // Travel detail
                using (ExcelRange Rng = worksheet.Cells[5, 8])
                {
                    Rng.Value = "Travel Status";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 9])
                {
                    Rng.Value = "In-House";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 10])
                {
                    Rng.Value = "Travel Topic";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 11])
                {
                    Rng.Value = "Country";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 12])
                {
                    Rng.Value = "City / Province";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 13])
                {
                    Rng.Value = "Business / Training Date [From]";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 14])
                {
                    Rng.Value = "Business / Training Date[To]";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 15])
                {
                    Rng.Value = "Duration (day)";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 16])
                {
                    Rng.Value = "Estimate Expense (BHT)";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 17])
                {
                    Rng.Value = "GL Account";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    // Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 18])
                {
                    Rng.Value = "Cost Center";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 19])
                {
                    Rng.Value = "Order / WBS";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    //Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTravelHex);

                }

                //Travel expense
                using (ExcelRange Rng = worksheet.Cells[5, 20])
                {
                    Rng.Value = "Accommodation";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 21])
                {
                    Rng.Value = "Air Ticket";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 22])
                {
                    Rng.Value = "Allowance_Day";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 23])
                {
                    Rng.Value = "Allowance_Night";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 24])
                {
                    Rng.Value = "Clothing & Luggage";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 25])
                {
                    Rng.Value = "Course Fee";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 26])
                {
                    Rng.Value = "Instruction Fee";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 27])
                {
                    Rng.Value = "Miscellaneous";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 28])
                {
                    Rng.Value = "Passport";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 29])
                {
                    Rng.Value = "Transportation";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 30])
                {
                    Rng.Value = "Visa (Fee)";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }
                using (ExcelRange Rng = worksheet.Cells[5, 31])
                {
                    Rng.Value = "Total";
                    Rng.Style.Font.Size = fontSize;
                    Rng.Style.Font.Name = fontName;
                    Rng.Style.Font.Bold = false;
                    Rng.Style.Font.Color.SetColor(Color.White);
                    Rng.Style.WrapText = true;
                    Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(colTrainingHex);

                }

                #endregion HEADER TABLE

                int startRow = 6;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    #region DETAIL TABLE
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 1])
                    {
                        Rng.Value = i + 1;
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 2])
                    {
                        Rng.Value = dt.Rows[i]["emp_id"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 3])
                    {
                        Rng.Value = dt.Rows[i]["emp_title"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 4])
                    {
                        Rng.Value = dt.Rows[i]["emp_name"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 5])
                    {
                        Rng.Value = dt.Rows[i]["section"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 6])
                    {
                        Rng.Value = dt.Rows[i]["department"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 7])
                    {
                        Rng.Value = dt.Rows[i]["function"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }

                    //Travel details
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 8])
                    {
                        Rng.Value = dt.Rows[i]["travel_status"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 9])
                    {
                        Rng.Value = dt.Rows[i]["in_house"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 10])
                    {
                        Rng.Value = dt.Rows[i]["travel_topic"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 11])
                    {
                        Rng.Value = dt.Rows[i]["country"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 12])
                    {
                        Rng.Value = dt.Rows[i]["city_province"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 13])
                    {
                        Rng.Value = dt.Rows[i]["date_from"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 14])
                    {
                        Rng.Value = dt.Rows[i]["date_to"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 15])
                    {
                        Rng.Value = dt.Rows[i]["duration"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 16])
                    {
                        Rng.Value = dt.Rows[i]["estimate_expense"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 17])
                    {
                        Rng.Value = dt.Rows[i]["gl_account"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 18])
                    {
                        Rng.Value = dt.Rows[i]["cost_center"].ToString().Trim();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 19])
                    {
                        Rng.Value = dt.Rows[i]["order_wbs"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }

                    // Training expense

                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 20])
                    {
                        Rng.Value = dt.Rows[i]["accommodation"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 21])
                    {
                        Rng.Value = dt.Rows[i]["air_ticket"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 22])
                    {
                        Rng.Value = dt.Rows[i]["allowance_day"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 23])
                    {
                        Rng.Value = dt.Rows[i]["allowance_night"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 24])
                    {
                        Rng.Value = dt.Rows[i]["clothing_luggage"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 25])
                    {
                        Rng.Value = dt.Rows[i]["course_fee"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 26])
                    {
                        Rng.Value = dt.Rows[i]["instruction_fee"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 27])
                    {
                        Rng.Value = dt.Rows[i]["miscellaneous"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 28])
                    {
                        Rng.Value = dt.Rows[i]["passport"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 29])
                    {
                        Rng.Value = dt.Rows[i]["transportation"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 30])
                    {
                        Rng.Value = dt.Rows[i]["visa_fee"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }
                    using (ExcelRange Rng = worksheet.Cells[startRow + i, 31])
                    {
                        Rng.Value = dt.Rows[i]["total"].ToString();
                        Rng.Style.WrapText = true;
                        Rng.Style.Font.Size = fontSize;
                        Rng.Style.Font.Name = fontName;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Color.SetColor(colBorderHex);

                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Color.SetColor(colBorderHex);
                    }

                    if (i == dt.Rows.Count - 1)
                    {
                        using (ExcelRange Rng = worksheet.Cells[startRow + i, 1, startRow + i, 31])
                        {
                            Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            Rng.Style.Border.Bottom.Color.SetColor(colBorderHex);
                        }
                    }

                    #endregion DETAIL TABLE

                }

                //worksheet.Cells.AutoFitColumns();

                var datetime1 = DateTime.Now.ToString("yyyyMMddHHmm");
                string datenow = datetime1;
                //worksheet.Protection.IsProtected = true;
                //worksheet.Protection.AllowSelectLockedCells = true;
                string fileName = $"EBIZ_TRAVEL_RECORD_{datetime1}.xlsx";
                var output = ClassPathReport.genFilePath("temp", fileName);// Path.Combine(Server.MapPath("~/temp"), fileName);
                var outputUrl = ClassPathReport.genFullPath("temp", fileName);

                ExcelPkg.SaveAs(new FileInfo(output));

                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("status");
                dtResult.Columns.Add("file_system_path");
                dtResult.Columns.Add("file_outbound_path");
                dtResult.Columns.Add("file_outbound_name");

                dtResult.Rows.Add(dtResult.NewRow());
                dtResult.Rows[0]["status"] = "true";
                dtResult.Rows[0]["file_system_path"] = output;
                dtResult.Rows[0]["file_outbound_path"] = outputUrl;
                dtResult.Rows[0]["file_outbound_name"] = fileName;

                dtResult.TableName = "dtResult";

                ds = new DataSet();
                ds.Tables.Add(dtResult);

            }
            catch (Exception ex)
            {
                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("status");
                dtResult.Columns.Add("file_system_path");
                dtResult.Columns.Add("file_outbound_path");
                dtResult.Columns.Add("file_outbound_name");

                dtResult.Rows.Add(dtResult.NewRow());
                dtResult.Rows[0]["status"] = ex.ToString();
                dtResult.Rows[0]["file_system_path"] = "";
                dtResult.Rows[0]["file_outbound_path"] = "";
                dtResult.Rows[0]["file_outbound_name"] = "";

                dtResult.TableName = "dtResult";

                ds = new DataSet();
                ds.Tables.Add(dtResult);
            }

            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

            return JSONresult;

        }

        public string TravelReport(ReportParamJsonModel value)
        {
            var param = value.param ?? "";
            var method = value.method ?? "";
            var jsondata = value.jsondata ?? "";

            if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; }
            ;
            if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; }
            ;
            if (string.IsNullOrEmpty(jsondata)) { return "IsNullOrEmpty JSONData."; }
            ;

            DataTable dt = new DataTable();
            try
            {
                DataTable dtData = new DataTable();
                if (jsondata != "")
                {
                    dtData = (DataTable)JsonConvert.DeserializeObject(jsondata, typeof(DataTable));
                }
                DataTable dtParam = new DataTable();
                if (param != "")
                {
                    dtParam = (DataTable)JsonConvert.DeserializeObject(param, typeof(DataTable));
                }

                if (method.ToLower().Trim() == "phase1report")
                {
                    //if (dtParam.Rows.Count > 0 && dtParam.Rows[0]["state"].ToString().Trim() != "" && dtParam.Rows[0]["token_login"].ToString().Trim() != "" && dtParam.Rows[0]["doc_id"].ToString().Trim() != "")
                    if (dtData.Rows.Count > 0)
                    {

                        dt = phase1report(dtParam);
                    }
                    else
                    {
                        dt = empty_document();
                    }
                }
                else

                if (method.ToLower().Trim() == "allowance")
                {
                    if (dtData.Rows.Count > 0)
                    {
                        dt = allowanceReport(dtData);
                    }
                    else
                    {
                        dt = empty_document();
                    }
                }
                else if (method.ToLower().Trim() == "reimbursement")
                {
                    if (dtData.Rows.Count > 0)
                    {
                        dt = reimbursementReport(dtData);
                    }
                    else
                    {
                        dt = empty_document();
                    }
                }
                else
                {
                    dt = empty_document();
                }
            }
            catch (Exception ex)
            {
                dt = new DataTable();
                dt.Columns.Add("file_system_path");
                dt.Columns.Add("file_outbound_path");
                dt.Columns.Add("file_outbound_name");
                dt.Columns.Add("status");
                dt.Rows.Add(dt.NewRow());

                dt.Rows[0]["file_system_path"] = "";
                dt.Rows[0]["file_outbound_path"] = "";
                dt.Rows[0]["file_outbound_name"] = "";
                dt.Rows[0]["status"] = ex.ToString();
            }

            dt.TableName = "dtResult";
            DataSet ds = new DataSet();

            ds.Tables.Add(dt);

            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

            return JSONresult;
        }
        public string ApprovalReport(ReportParamJsonModel value)
        {
            var param = value.param ?? "";
            var method = value.method ?? "";
            var jsondata = value.jsondata ?? "";

            if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; }
            ;
            if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; }
            ;
            if (string.IsNullOrEmpty(jsondata)) { return "IsNullOrEmpty JSONData."; }
            ;

            DataTable dt = new DataTable();
            try
            {
                DataTable dtParam = new DataTable();
                if (param != "")
                {
                    try
                    {
                        dtParam = (DataTable)JsonConvert.DeserializeObject(param, typeof(DataTable));
                    }
                    catch (Exception ex) { }
                }

                if (dtParam.Rows.Count > 0 && dtParam.Rows[0]["state"].ToString().Trim() != "" && dtParam.Rows[0]["token_login"].ToString().Trim() != "" && dtParam.Rows[0]["doc_id"].ToString().Trim() != "")
                {
                    dt = phase1report(dtParam);
                }
                else
                {
                    dt = empty_document();
                }
            }
            catch (Exception ex)
            {
                dt = new DataTable();
                dt.Columns.Add("file_system_path");
                dt.Columns.Add("file_outbound_path");
                dt.Columns.Add("file_outbound_name");
                dt.Columns.Add("status");
                dt.Rows.Add(dt.NewRow());

                dt.Rows[0]["file_system_path"] = "";
                dt.Rows[0]["file_outbound_path"] = "";
                dt.Rows[0]["file_outbound_name"] = "";
                dt.Rows[0]["status"] = ex.ToString();
            }

            dt.TableName = "dtResult";
            DataSet ds = new DataSet();

            ds.Tables.Add(dt);

            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

            return JSONresult;
        }

        public DataTable phase1report(DataTable dtData)
        {
            DataTable dtResult = new DataTable();
            ClassDataReportCreateTrip app = new ClassDataReportCreateTrip();

            string token = dtData.Rows[0]["token_login"]?.ToString()?.Trim() ?? "";
            string doc_id = dtData.Rows[0]["doc_id"]?.ToString()?.Trim() ?? "";
            string state = dtData.Rows[0]["state"]?.ToString()?.Trim() ?? "";

            dtResult = state == "oversea" || state == "overseatraining"
                ? app.OBApprovalReport(token, doc_id, state)
                : app.LBApprovalReport(token, doc_id, state);

            return dtResult;
        }

        public string ReportISOSRecord(ReportParamJsonModel value)
        {
            var param = value.jsondata ?? "";
            var method = value.method ?? "";
            var jsondata = value.jsondata ?? "";

            if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; }
            ;
            if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; }
            ;
            if (string.IsNullOrEmpty(jsondata)) { return "IsNullOrEmpty JSONData."; }
            ;

            DataTable dt = new DataTable();

            try
            {
                DataTable dtData = new DataTable();
                if (jsondata != "")
                {
                    dtData = (DataTable)JsonConvert.DeserializeObject(jsondata, typeof(DataTable));
                }
                DataTable dtParam = new DataTable();
                if (param != "")
                {
                    dtParam = (DataTable)JsonConvert.DeserializeObject(param, typeof(DataTable));
                }
                if (dtParam.Rows.Count > 0 && dtParam.Rows[0]["year"].ToString().Trim() != "" && dtParam.Rows[0]["token_login"].ToString().Trim() != "")
                {
                    ClassDataReportTravelerProfile cls = new ClassDataReportTravelerProfile();
                    dt = cls.ReportISOSRecords(dtParam.Rows[0]["token_login"].ToString().Trim(), dtParam.Rows[0]["year"].ToString().Trim());
                }
                else
                {
                    dt = empty_document();
                }

            }
            catch (Exception ex)
            {
                dt = new DataTable();
                dt.Columns.Add("file_system_path");
                dt.Columns.Add("file_outbound_path");
                dt.Columns.Add("file_outbound_name");
                dt.Columns.Add("status");
                dt.Rows.Add(dt.NewRow());

                dt.Rows[0]["file_system_path"] = "";
                dt.Rows[0]["file_outbound_path"] = "";
                dt.Rows[0]["file_outbound_name"] = "";
                dt.Rows[0]["status"] = ex.ToString();
            }

            dt.TableName = "dtResult";
            DataSet ds = new DataSet();

            ds.Tables.Add(dt);

            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

            return JSONresult;

        }

        public string ReportInsuranceRecord(ReportParamJsonModel value)
        {
            var param = value.jsondata ?? "";
            var method = value.method ?? "";
            var jsondata = value.jsondata ?? "";

            if (string.IsNullOrEmpty(param)) { return "IsNullOrEmpty Param."; }
            ;
            if (string.IsNullOrEmpty(method)) { return "IsNullOrEmpty Method."; }
            ;
            if (string.IsNullOrEmpty(jsondata)) { return "IsNullOrEmpty JSONData."; }
            ;

            DataTable dt = new DataTable();

            try
            {
                DataTable dtData = new DataTable();
                if (jsondata != "")
                {
                    dtData = (DataTable)JsonConvert.DeserializeObject(jsondata, typeof(DataTable));
                }
                DataTable dtParam = new DataTable();
                if (param != "")
                {
                    dtParam = (DataTable)JsonConvert.DeserializeObject(param, typeof(DataTable));
                }
                if (dtParam.Rows.Count > 0 && dtParam.Rows[0]["year"].ToString().Trim() != "" && dtParam.Rows[0]["token_login"].ToString().Trim() != "")
                {
                    ClassDataReportTravelerProfile cls = new ClassDataReportTravelerProfile();
                    dt = cls.ReportInsuranceRecords(dtParam.Rows[0]["token_login"].ToString().Trim(), dtParam.Rows[0]["year"].ToString().Trim());
                }
                else
                {
                    dt = empty_document();
                }

            }
            catch (Exception ex)
            {
                dt = new DataTable();
                dt.Columns.Add("file_system_path");
                dt.Columns.Add("file_outbound_path");
                dt.Columns.Add("file_outbound_name");
                dt.Columns.Add("status");
                dt.Rows.Add(dt.NewRow());

                dt.Rows[0]["file_system_path"] = "";
                dt.Rows[0]["file_outbound_path"] = "";
                dt.Rows[0]["file_outbound_name"] = "";
                dt.Rows[0]["status"] = ex.ToString();
            }

            dt.TableName = "dtResult";
            DataSet ds = new DataSet();

            ds.Tables.Add(dt);

            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(ds, Formatting.Indented);

            return JSONresult;

        }

        private DataTable allowanceReport(DataTable dtData)
        {
            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.Add("Allowance (E-BIZ)");

            //hide gridline
            worksheet.View.ShowGridLines = false;
            string datetime = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

            Color colFirstHeaderHex = ColorTranslator.FromHtml("#808080");
            string fontName = "Cordia New";

            worksheet.Cells["A1"].Value = "Allowance Report";
            worksheet.Cells["A1"].Style.Font.Size = 24;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Name = fontName;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(colFirstHeaderHex);
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1:AE1"].Merge = true;
            worksheet.Row(1).Height = 40;


            worksheet.Cells["A2"].Value = "Print date : " + datetime;
            worksheet.Cells["A2"].Style.Font.Size = 10;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Name = fontName;
            worksheet.Cells["A2:AE2"].Merge = true;


            var datetime1 = DateTime.Now.ToString("yyyyMMddHHmm");
            string datenow = datetime1;
            string fileName = $"EBIZ_ALLOWANCE_REPORT_{datetime1}.xlsx";

            var output = ClassPathReport.genFilePath("temp", fileName);
            var outputUrl = ClassPathReport.genFullPath("temp", fileName);


            ExcelPkg.SaveAs(new FileInfo(output));

            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");

            dtResult.Rows.Add(dtResult.NewRow());
            dtResult.Rows[0]["status"] = "true";
            dtResult.Rows[0]["file_system_path"] = output;
            dtResult.Rows[0]["file_outbound_path"] = outputUrl;
            dtResult.Rows[0]["file_outbound_name"] = fileName;


            return dtResult;
        }

        private DataTable reimbursementReport(DataTable dtData)
        {
            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet worksheet = ExcelPkg.Workbook.Worksheets.Add("Reimbursement (E-BIZ)");

            //hide gridline
            worksheet.View.ShowGridLines = false;
            string datetime = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

            Color colFirstHeaderHex = ColorTranslator.FromHtml("#808080");
            string fontName = "Cordia New";

            worksheet.Cells["A1"].Value = "Reimbursement Report";
            worksheet.Cells["A1"].Style.Font.Size = 24;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Name = fontName;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(colFirstHeaderHex);
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1:AE1"].Merge = true;
            worksheet.Row(1).Height = 40;


            worksheet.Cells["A2"].Value = "Print date : " + datetime;
            worksheet.Cells["A2"].Style.Font.Size = 10;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Name = fontName;
            worksheet.Cells["A2:AE2"].Merge = true;


            var datetime1 = DateTime.Now.ToString("yyyyMMddHHmm");
            string datenow = datetime1;
            string fileName = $"EBIZ_REIMBURSEMENT_REPORT_{datetime1}.xlsx";

            var output = ClassPathReport.genFilePath("temp", fileName);
            var outputUrl = ClassPathReport.genFullPath("temp", fileName);

            ExcelPkg.SaveAs(new FileInfo(output));

            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("status");
            dtResult.Columns.Add("file_system_path");
            dtResult.Columns.Add("file_outbound_path");
            dtResult.Columns.Add("file_outbound_name");

            dtResult.Rows.Add(dtResult.NewRow());
            dtResult.Rows[0]["status"] = "true";
            dtResult.Rows[0]["file_system_path"] = output;
            dtResult.Rows[0]["file_outbound_path"] = outputUrl;
            dtResult.Rows[0]["file_outbound_name"] = fileName;

            return dtResult;
        }

    }


}
