
using Newtonsoft.Json;
using System.Data;
using static top.ebiz.service.Service.Report.ClassReportModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
 


namespace top.ebiz.service.Service.Report
{
    public class ClassReportOpenXml
    {
        public (DataTable, string) ReportInsurance(ReportParamModel value)
        {
            var param = value.param ?? "";
            var method = value.method ?? "";

            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(param)) { return (dt, "IsNullOrEmpty Param."); }

            if (string.IsNullOrEmpty(method)) { return (dt, "IsNullOrEmpty Method."); }
             
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
                        string datetime = DateTime.Now.ToString("ddMMyyhhmmss");
                        string reportDate = DateTime.Now.ToString("dd MMMM yyyy");
                        var template = ClassPathReport.genFilePath("template", "Starr_BTA_Application_form_original.docx");

                        // ค่าที่ดึงจาก DataTable
                        string policyHolder = dt.Rows[0]["policyHolder"].ToString();
                        string passportNo = dt.Rows[0]["passportNo"].ToString();
                        string companyName = dt.Rows[0]["companyName"].ToString();
                        string address = dt.Rows[0]["address"].ToString();
                        string occupation = dt.Rows[0]["occupation"].ToString();
                        string age = dt.Rows[0]["age"].ToString();
                        string tel = dt.Rows[0]["tel"].ToString();
                        string fax = dt.Rows[0]["fax"].ToString();
                        string nameOfBeneficiary = dt.Rows[0]["nameOfBeneficiary"].ToString();
                        string relationship = dt.Rows[0]["relationship"].ToString();
                        string pdateFrom = dt.Rows[0]["pdateFrom"].ToString();
                        string pdateTo = dt.Rows[0]["pdateTo"].ToString();
                        string duration = dt.Rows[0]["duration"].ToString();
                        string durationValue = duration;
                        string insPlan = dt.Rows[0]["insPlan"].ToString();
                        string destination = dt.Rows[0]["destination"].ToString();
                        string broker = dt.Rows[0]["broker"].ToString();

                        string safePolicyHolder = policyHolder.Trim().Replace(" ", "_").Replace(".", "");
                        string safeDestination = destination.Trim().Replace(" ", "_");
                        string safeDatetime = datetime.Trim().Replace(" ", "_");

                        string fileName = $"{safePolicyHolder}_{safeDestination}_{safeDatetime}.docx";
                        var output = ClassPathReport.genFilePath("temp", fileName);
                        var outputUrl = ClassPathReport.genFullPath("temp", fileName);

                        File.Copy(template, output, true); // copy template ก่อนแก้ไข

                        // แก้ไข placeholder ด้วย Open XML SDK
                        using (WordprocessingDocument doc = WordprocessingDocument.Open(output, true))
                        {
                            var body = doc.MainDocumentPart.Document.Body;
                            var texts = body.Descendants<Text>();

                            void ReplaceText(string placeholder, string value)
                            {
                                foreach (var t in texts.Where(t => t.Text.Contains(placeholder)))
                                {
                                    t.Text = t.Text.Replace(placeholder, value);
                                }
                            }

                            // แทนที่ทุก field
                            ReplaceText("<<policyHolder>>", policyHolder);
                            ReplaceText("<<passportNo>>", passportNo);
                            ReplaceText("<<companyName>>", companyName);
                            ReplaceText("<<address>>", address);
                            ReplaceText("<<occupation>>", occupation);
                            ReplaceText("<<age>>", age);
                            ReplaceText("<<tel>>", tel);
                            ReplaceText("<<fax>>", fax);
                            ReplaceText("<<nameOfBeneficiary>>", nameOfBeneficiary);
                            ReplaceText("<<relationship>>", relationship);
                            ReplaceText("<<durationValue>>", duration);
                            ReplaceText("<<pdateFrom>>", pdateFrom);
                            ReplaceText("<<pdateTo>>", pdateTo);
                            ReplaceText("<<duration>>", duration);
                            ReplaceText("<<insPlan>>", insPlan);
                            ReplaceText("<<destination>>", destination);
                            ReplaceText("<<reportDate>>", reportDate);
                            ReplaceText("<<broker>>", broker);

                            doc.MainDocumentPart.Document.Save();
                        }

                        // update datatable
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

            ds.Tables.Add(dt.Copy());

            return (dt, "");
        }
        public (DataTable, string) ReportEmployeeLetter(ReportParamModel value)
        {

            var param = value.param ?? "";
            var method = value.method ?? "";
            string msg = "";
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(param)) { return (dt, "IsNullOrEmpty Param."); }

            if (string.IsNullOrEmpty(method)) { return (dt, "IsNullOrEmpty Method."); }


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
                    string datetime = DateTime.Now.ToString("ddMMyyhhmmss");
                    string reportDate = DateTime.Now.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("en-US"));

                    string nameOfEmbassy1 = "...........................";

                    string nameOfEmployee = dt.Rows[0]["nameOfEmployee"].ToString();
                    string nameOfEmbassy = dt.Rows[0]["nameOfEmbassy"].ToString();
                    string gender = dt.Rows[0]["gender"].ToString();

                    string heShe1 = gender.ToLower() == "male" ? "He" : "She";
                    string heShe2 = gender.ToLower() == "male" ? "he" : "she";
                    string hisHer1 = gender.ToLower() == "male" ? "His" : "Her";
                    string hisHer2 = gender.ToLower() == "male" ? "his" : "her";

                    string joinDate = dt.Rows[0]["joinDate"].ToString();
                    string position = dt.Rows[0]["position"].ToString();
                    string travelTopic = dt.Rows[0]["travelTopic"].ToString();
                    string cityCountry = dt.Rows[0]["cityCountry"].ToString();
                    string dateOfDeparture = dt.Rows[0]["dateOfDeparture"].ToString();
                    string company = dt.Rows[0]["company"].ToString();

                    string companyFullName = company.ToUpper() == "TOP" ? "Thai Oil Public Company Limited" : "Thaioil Energy Services Company Limited";
                    string nameSig1 = company.ToUpper() == "TOP" ? "Viroj Wongsathirayakhun" : "Peerasud Sritawat Na Ayudhaya";
                    string positionSig1 = company.ToUpper() == "TOP" ? "Executive Vice President-Organization Effectiveness" : "Human Resources and Finance Manager";
                    string nameSig2 = company.ToUpper() == "TOP" ? "Ratri Chingchitra" : "Sukulya Veeradaechapol";
                    string positionSig2 = company.ToUpper() == "TOP" ? "Manager Compensation and Information System" : "HR Services Manager";

                    string template = ClassPathReport.genFilePath("template", "EMPLOYEE LETTER_TEMPLATE.docx");
                    string fileName = $"{company}_{nameOfEmployee?.Replace(' ', '_')}_{datetime}.docx";

                    var output = ClassPathReport.genFilePath("temp", fileName);
                    var outputUrl = ClassPathReport.genFullPath("temp", fileName);

                    File.Copy(template, output, true);

                    using (WordprocessingDocument doc = WordprocessingDocument.Open(output, true))
                    {
                        var body = doc.MainDocumentPart?.Document?.Body;

                        if (body == null)
                            throw new Exception("Template document body not found.");

                        var texts = body.Descendants<Text>();

                        void Replace(string placeholder, string value)
                        {
                            foreach (var t in texts.Where(t => t.Text.Contains(placeholder)))
                                t.Text = t.Text.Replace(placeholder, value);
                        }

                        // แทนที่ข้อความ
                        Replace("<<nameOfEmbassy1>>", nameOfEmbassy1);
                        Replace("<<nameOfEmbassy>>", nameOfEmbassy);
                        Replace("<<reportDate>>", reportDate);
                        Replace("<<nameOfEmployee>>", nameOfEmployee);
                        Replace("<<companyFullName>>", companyFullName);
                        Replace("<<heShe1>>", heShe1);
                        Replace("<<heShe2>>", heShe2);
                        Replace("<<hisHer1>>", hisHer1);
                        Replace("<<hisHer2>>", hisHer2);
                        Replace("<<joinDate>>", joinDate);
                        Replace("<<position>>", position);
                        Replace("<<travelTopic>>", travelTopic);
                        Replace("<<cityCountry>>", cityCountry);
                        Replace("<<dateOfDeparture>>", dateOfDeparture);
                        Replace("<<nameSig1>>", nameSig1);
                        Replace("<<positionSig1>>", positionSig1);
                        Replace("<<nameSig2>>", nameSig2);
                        Replace("<<positionSig2>>", positionSig2);

                        doc.MainDocumentPart.Document.Save();
                    }

                    dt.Rows[0]["file_system_path"] = output;
                    dt.Rows[0]["file_outbound_path"] = outputUrl;
                    dt.Rows[0]["file_outbound_name"] = fileName;
                    dt.Rows[0]["status"] = "true";


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

            return (dt, "");
        }

    }


}
