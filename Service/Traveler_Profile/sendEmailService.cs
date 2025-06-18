
using System.Data;
using Microsoft.Exchange.WebServices.Data;
using top.ebiz.service.Models.Traveler_Profile;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Net;
using top.ebiz.helper;
using top.ebiz.service.Models.Create_Trip;

using System.Diagnostics;
using System.Web;
using static top.ebiz.service.Service.Traveler_Profile.SetDocService;
using top.ebiz.service.Service.Create_Trip;
using _documentService = top.ebiz.service.Service.Create_Trip.documentService;
using System.Security;
namespace top.ebiz.service.Service.Traveler_Profile
{
    public class SendEmailServiceTravelerProfile
    {
        SetDocService sws = new SetDocService();
        searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();

        ClassConnectionDb conn;
        string sqlstr = "";
        string sqlstr_all = "";
        string ret = "";
        DataTable dt;
        Boolean user_admin = false;
        string user_id = "";
        string user_role = "";

        string Environments = top.ebiz.helper.AppEnvironment.GeteServerFolder() ?? "";
        
        #region  emailconfig  
        public async Task<string> send_mail(String doc_id, String step_flow, String s_mail_to, String s_mail_cc, String s_subject, String s_mail_body, string s_mail_attachments, string s_mail_show_case = "", bool resend = true)
        {
            try
            { 
                    return SendMail_443(doc_id, step_flow, s_mail_to, s_mail_cc, s_subject, s_mail_body, s_mail_attachments, s_mail_show_case, resend).GetAwaiter().GetResult(); ;
              
            }
            catch (Exception e2)
            {
                return e2.Message.ToString();
            } 
        }

        private void AttachLongPathFile(EmailMessage email, string fullPath)
        {
            string tempFile = null;
            try
            {
                string fileName = Path.GetFileName(fullPath);
                tempFile = Path.Combine(Path.GetTempPath(), fileName);

                // คัดลอกไฟล์ไปยัง TEMP (ใช้ File.Copy ที่รองรับ long path)
                using (var sourceStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                using (var destStream = new FileStream(tempFile, FileMode.Create))
                {
                    sourceStream.CopyTo(destStream);
                }

                email.Attachments.AddFileAttachment(tempFile);
                Console.WriteLine($"Attached via temp file: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to attach long path file: {ex.Message}");
                throw;
            }
            finally
            {
                if (tempFile != null && File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { /* Ignore */ }
                }
            }
        }
        public async Task<string> SendMail_443(String doc_id, String step_flow, String s_mail_to, String s_mail_cc, String s_subject, String s_mail_body, string s_mail_attachments, string s_mail_show_case, bool resend = true)
        {
            string msg_mail = "";
            string msg_mail_file = "";

            string mail_user = "";
            string mail_from = "";
            string mail_password = "";
            string mail_test = "";
            string mail_font = "";
            string mail_fontsize = "";
            try
            {
                //write_log_mail("71-sendMail start", "s_subject:" + s_subject + "  =>email to:" + s_mail_to.ToString() + "  =>email cc:" + s_mail_cc.ToString());



                msg_mail = dataConfigMail(ref mail_user, ref mail_password, ref mail_from, ref mail_test);
                if (!string.IsNullOrEmpty(msg_mail))
                {
                    return "No config mail.";
                }

                Boolean SendAndSaveCopy = false;
                //mail_user = mail_user.Split('@')[0];

                ExchangeService service = new ExchangeService();
                mail_user = mail_user.Split('@')[0];

                service.Credentials = new WebCredentials(mail_user, mail_password);
                service.TraceEnabled = true;


                string MailDisplay = $"[{(Environments == "PROD" ? "" : Environments)}] Thaioil e-Business Travel";
                EmailMessage email = new EmailMessage(service);
                try { service.AutodiscoverUrl(mail_from, RedirectionUrlValidationCallback); }
                catch (Exception ex)
                {
                    msg_mail = ex.ToString();
                    //write_log_mail("AutodiscoverUrl", "error:" + msg_mail);
                }
                email.From = new EmailAddress(MailDisplay, mail_from);


                if (mail_test != "")
                {
                    // Use mail_test for recipients if it is provided
                    var email_to = mail_test.Split(';');
                    for (int i = 0; i < email_to.Length; i++)
                    {
                        string _mail = (email_to[i].ToString()).Trim();
                        if (_mail != "")
                        {
                            email.ToRecipients.Add(_mail);
                        }
                    }

                    try
                    {
                        s_mail_to = s_mail_to.ToLower();
                        //  s_mail_to = s_mail_to.ToUpper();
                        var emailList = s_mail_to.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(email => email.Trim())
                                                  .ToHashSet();
                        if (emailList.Count > 1) { s_mail_to = string.Join(",", emailList); }
                    }
                    catch { }

                    try
                    {
                        s_mail_cc = s_mail_cc.ToLower();
                        var emailList = s_mail_cc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(email => email.Trim())
                                                  .ToHashSet();
                        if (emailList.Count > 1) { s_mail_cc = string.Join(",", emailList); }
                    }
                    catch { }

                    // Construct the email body
                    var MailBodyDef = s_mail_body;
                    s_mail_body = @"<span lang='en-US'><div>";
                    //s_mail_body += $"<div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>To : {s_mail_to}</span></font></div>";
                    //s_mail_body += $"<div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>CC : {s_mail_cc}</span></font></div>";
                    if (!string.IsNullOrEmpty(s_mail_show_case))
                    {
                        s_mail_body += $"<div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>***Details : {s_mail_show_case}</span></font></div>";
                    }
                    if (step_flow != "resend email")
                    {
                        s_mail_body += $"<div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>Subject : {s_subject}</span></font></div>";
                    }
                    s_mail_body += "<br></div></span>";
                    s_mail_body += MailBodyDef;
                }



                else
                {
                    // Use s_mail_to and s_mail_cc for recipients if mail_test is not provided
                    var email_to = s_mail_to.Split(';');
                    for (int i = 0; i < email_to.Length; i++)
                    {
                        string _mail = (email_to[i].ToString()).Trim();
                        if (_mail != "")
                        {
                            email.ToRecipients.Add(_mail);
                        }
                    }

                    var email_cc = s_mail_cc.Split(';');
                    for (int i = 0; i < email_cc.Length; i++)
                    {
                        string _mail = (email_cc[i].ToString()).Trim();
                        if (_mail != "")
                        {
                            email.CcRecipients.Add(_mail);
                        }
                    }
                }
                //Subject
                email.Subject = $"[{(Environments == "PROD" ? "" : Environments)}] {s_subject}";

                //Body
                //เพิ่ม กำหนด font  
                if (mail_font == "") { mail_font = "Cordia New"; }
                if (mail_fontsize == "") { mail_fontsize = "18"; }
                s_mail_body = "<html><div style='font-size:" + mail_fontsize + "px; font-family:" + mail_font + ";'>" + s_mail_body + "</div></html>";
                email.Body = new MessageBody(BodyType.HTML, s_mail_body);

                #region Attachments
                try
                {
                    if (!string.IsNullOrEmpty(s_mail_attachments))
                    {
                        string[] attachments = s_mail_attachments.Split('|', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var filePath in attachments)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(filePath))
                                {

                                    string fullPath = filePath;
                                    // แก้ไขเส้นทางและถอดรหัส URL
                                    // string fullPath = filePath.Replace(@"D:\ebiz\service\", @"D:\ebiz\service\wwwroot\");
                                    bool hasWwwRoot = filePath.IndexOf("wwwroot", StringComparison.OrdinalIgnoreCase) >= 0;

                                    // ถ้าไม่มี wwwroot และ path ขึ้นต้นด้วย "D:\ebiz\service\" ให้ทำการ Replace
                                    if (!hasWwwRoot && filePath.StartsWith(@"D:\ebiz\service\", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fullPath = filePath.Replace(@"D:\ebiz\service\", @"D:\ebiz\service\wwwroot\");
                                    }
                                    fullPath = HttpUtility.UrlDecode(fullPath);
                                   // write_log_mail("Attachment afterReplacefullPathbf", fullPath.ToString());

                                    // แก้ปัญหาเส้นทางยาว
                                    if (fullPath.Length >= 260 && !fullPath.StartsWith(@"\\?\"))
                                    {
                                        fullPath = @"\\?\" + fullPath;
                                    }

                                    if (File.Exists(fullPath))
                                    {
                                        try
                                        {
                                            email.Attachments.AddFileAttachment(fullPath);
                                            Console.WriteLine($"Attached: {fullPath}");
                                            //write_log_mail("Attached fullPathbf", fullPath.ToString());
                                        }
                                        catch (PathTooLongException)
                                        {
                                            AttachLongPathFile(email, fullPath);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"File not found: {fullPath}");
                                        // Debug: ดูรายชื่อไฟล์ในโฟลเดอร์
                                        //write_log_mail("File not found (Server)", fullPath);
                                        string dir = Path.GetDirectoryName(fullPath);
                                        if (Directory.Exists(dir))
                                        {
                                            Console.WriteLine("Files in directory:");
                                            foreach (var f in Directory.GetFiles(dir))
                                            {
                                                Console.WriteLine(Path.GetFileName(f));
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error attaching {filePath}: {ex.Message}");
                                msg_mail_file = ex.ToString();
                                //write_log_mail("Error attaching", $" {filePath}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    msg_mail_file = ex.ToString();
                    //write_log_mail("Attachment Error", ex.ToString());
                }
                #endregion

                try
                {
                    //write_log_mail("72-sendMail send", (msg_mail_file != "" ? "=>msg_mail_file:" + msg_mail_file : ""));
                    if (SendAndSaveCopy == true)
                    {
                        //จะมีใน send box item
                        email.SendAndSaveCopy();
                    }
                    else
                    {
                        email.Send();

                        //ไม่เก็บใน send box item

                    }
                    msg_mail = "";
                   // write_log_mail("79-sendMail end", "");
                }
                catch (Exception ex)
                {
                    msg_mail = ex.ToString();
                    //write_log_mail("78-sendMail443 error ", "error:" + msg_mail + " => msg_mail_file:" + msg_mail_file);
                }
            }
            catch (Exception ex)
            {
                msg_mail = ex.ToString();
               // write_log_mail("80-sendMail end ", "error:" + msg_mail + " => msg_mail_file:" + msg_mail_file);
            }
            try
            {
                string finaldoc_id = !string.IsNullOrEmpty(doc_id)
       ? doc_id
       : s_subject.Split(':')[0].Trim();

                //ข้อมูลเบื้องต้นก่อนปรับ
                Models.Create_Trip.BZ_EMAIL_DETAILS data_mail = new Models.Create_Trip.BZ_EMAIL_DETAILS();
                data_mail.ID = "";
                data_mail.DOC_ID = finaldoc_id;
                data_mail.STEPFLOW = step_flow;
                data_mail.FROMEMAIL = mail_from;
                data_mail.CCRECIPIENTS = "";
                data_mail.BCCRECIPIENTS = "";

                data_mail.TORECIPIENTS = s_mail_to;
                data_mail.CCRECIPIENTS = s_mail_cc;
                data_mail.SUBJECT = s_subject;
                data_mail.BODY = s_mail_body;
                data_mail.ATTACHMENTS = s_mail_attachments;
                data_mail.STATUSSEND = "";
                data_mail.ERRORSEND = "";
                data_mail.ACTIVETYPE = "Y";
                //เพิ่ม Function Resend E-mail, เก็บค่าไว้ใน table log mail
                data_mail.STATUSSEND = (msg_mail == "" ? "true" : "false");
                data_mail.ERRORSEND = msg_mail;

                try
                {
                    if (resend)
                    {
                        insertMailLog(data_mail);
                        //write_log_mail("81-datafunctionresendMail end", "doc_id" + doc_id.ToString());
                    }

                }
                catch (Exception ex)
                {
                    //write_log_mail("82-Error saving email log", ex.ToString());

                }
                //if (resend)
                //{
                //    insertMailLog(data_mail);
                //    write_log_mail("82-datafunctionresendMail end", "");
                //}

            }
            catch (Exception ex)
            {
                msg_mail = ex.ToString();
                //write_log_mail("83-resendMail  ", "error:" + msg_mail);
            }
            return msg_mail;
        }

        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            Uri uri = new Uri(redirectionUrl);
            return uri.Scheme == "https" && uri.Host.Contains("thaioilgroup.com");
        }

        private string dataConfigMail(ref string mail_user, ref string mail_password, ref string mail_from, ref string mail_test)
        {
            DataTable dtConfig = new DataTable();
            #region Execute to Datable 
            // Get test config from the database
            string sqlstr = @"SELECT DISTINCT LOWER(key_name) AS key_name, key_value  FROM BZ_CONFIG_DATA  WHERE STATUS = 1 AND KEY_FILTER = 'EMAIL'";
            try
            {
                conn = new ClassConnectionDb();
                conn.OpenConnection();
                try
                {
                    var command = conn.conn.CreateCommand();
                    command.CommandText = sqlstr;
                    dtConfig = new DataTable();
                    dtConfig = conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dtConfig.AcceptChanges();
                }
                catch { }
                finally { conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute to Datable

            if (dtConfig?.Rows.Count > 0)
            {
                // เก็บคีย์ที่ต้องการตรวจสอบใน array
                string[] keys = { "email_ebiz", "email_ebiz_pass", "email_from", "email_test" };

                // วนลูปผ่านคีย์ต่างๆ และดึงค่าจาก DataTable
                foreach (var key in keys)
                {
                    DataRow[] dr = dtConfig.AsEnumerable().Where(row => row.Field<string>("key_name") == key).ToArray();
                    if (dr?.Length > 0)
                    {
                        string x = dr[0]["key_value"]?.ToString() ?? "";

                        // ตรวจสอบและกำหนดค่าตามคีย์
                        switch (key)
                        {
                            case "email_ebiz":
                                mail_user = x;
                                break;
                            case "email_ebiz_pass":
                                mail_password = x; // ปลดล็อกถ้าจำเป็น: mail_password = Decrypt(value);
                                break;
                            case "email_from":
                                mail_from = x;
                                break;
                            case "email_test":
                                mail_test = x;
                                break;
                        }
                    }
                    else
                    {
                        return $"{key} not found in config.";
                    }
                }
            }
            else
            {
                return "No config mail.";
            }

            return "";
        }
        private List<Models.Traveler_Profile.Users> get_user_special_group(string group_key)
        {

            List<Models.Traveler_Profile.Users> lstADUsers = new List<Models.Traveler_Profile.Users>();
            try
            {
                var parameters = new List<OracleParameter>();
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    sqlstr = @" select a.emp_id, vw.email, vw.userdisplay as displayname, null as memberof
                             from BZ_USER_SPECIAL_GROUP a 
                             inner join BZ_USER_ROLE b on a.role_id = b.usr_id 
                             inner join VW_BZ_USERS vw on a.emp_id = vw.employeeid
                             where lower(b.usr_name) = lower(:group_key) ";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("group_key", group_key, "char"));
                    var result = context.TempEmpSpecialModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList().FirstOrDefault();

                    if (result != null)
                    {
                        var userList = new Models.Traveler_Profile.Users
                        {
                            Email = result?.email ?? "",
                            UserName = result?.displayname ?? "",
                            DisplayName = result?.displayname ?? "",
                        };
                        lstADUsers.Add(userList);
                    }
                }
            }
            catch { }
            return lstADUsers;
        }
        //TOP GROUP PMSV-Business
        public List<Models.Traveler_Profile.Users> ListPMSVBusiness(string group_key_name)
        {
            return get_user_special_group("PMSVBusiness");
        }
        public List<Models.Traveler_Profile.Users> ListSupperAdmin(ref string saduser_email, string group_key_name)
        {
            if (group_key_name == "") { group_key_name = "E-Mail Group E-Biz"; }
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            DataTable dtadmin = new DataTable();
            List<Models.Traveler_Profile.Users> lstADUsers = new List<Models.Traveler_Profile.Users>();

            DataTable dtEmailGroupList = new DataTable();
            sqlstr = @"select key_value as name  from  bz_config_data where trim(lower(key_name)) =trim(lower(:group_key_name)) ";
            p.Add(new("group_key_name", group_key_name));
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dtEmailGroupList, cmd) == "")
            {
                if (dtEmailGroupList.Rows.Count > 0)
                {
                    lstADUsers = new List<Models.Traveler_Profile.Users>();
                    lstADUsers = get_user_special_group("SupperAdmin");
                }
            }
            for (int i = 0; i < lstADUsers.Count; i++)
            {
                saduser_email += lstADUsers[i].Email.ToString() + ";";
            }

            return lstADUsers;
        }
        public List<Models.Traveler_Profile.Users> ListISOSMember(ref string saduser_email, string group_key_name)
        {
            if (group_key_name == "") { group_key_name = "E-Mail Group E-Biz"; }
            DataTable dtadmin = new DataTable();
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            List<Models.Traveler_Profile.Users> lstADUsers = new List<Models.Traveler_Profile.Users>();
            if (true)
            {
                DataTable dtEmailGroupList = new DataTable();
                sqlstr = @"select key_value as name  from  bz_config_data where trim(lower(key_name)) =trim(lower(:group_key_name)) ";
                p.Add(new("group_key_name", group_key_name));
                cmd = new(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                if (SetDocService.conn_ExecuteData(ref dtEmailGroupList, cmd) == "")
                {
                    if (dtEmailGroupList.Rows.Count > 0)
                    {
                        lstADUsers = new List<Models.Traveler_Profile.Users>();
                        lstADUsers = get_user_special_group("ISOSMember");
                    }
                }
                for (int i = 0; i < lstADUsers.Count; i++)
                {
                    saduser_email += lstADUsers[i].Email.ToString() + ";";
                }
            }
            return lstADUsers;
        }

        public EmailModel EmailConfig(EmailModel value)
        {
            var msg_error = "";
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = value.token_login;

            var page_name = value.page_name;
            var action_name = value.action_name;

            if (page_name.ToLower() == "travelinsurance")
            {
                if (action_name.ToLower() == ("NotiTravelInsuranceForm").ToLower())
                {
                    emailNotiTravelInsuranceForm(ref data, ref msg_error);
                }
                else if (action_name.ToLower() == ("NotiTravelInsuranceListPassportInfo").ToLower())
                {
                    emailNotiTravelInsuranceListPassportInfo(ref data, ref msg_error);
                }
                else if (action_name.ToLower() == ("NotiTravelInsuranceCertificates").ToLower())
                {
                    emailNotiTravelInsuranceCertificates(ref data, ref msg_error);
                }
            }
            else if (page_name.ToLower() == "isos")
            {
                if (action_name.ToLower() == ("NotiISOSNewListRuningNoName").ToLower())
                {
                    emailNotiISOSNewListRuningNoName(ref data, ref msg_error);
                }
                else if (action_name.ToLower() == ("NotiISOSNewList").ToLower())
                {
                    emailNotiISOSNewList(ref data, ref msg_error);
                }
            }
            else if (page_name.ToLower() == "visa")
            {
                if (action_name.ToLower() == ("NotiTravellerCheckVisa").ToLower())
                {
                    emailNotiISOSNewListRuningNoName(ref data, ref msg_error);
                }
                else if (action_name.ToLower() == ("NotiAccommodationToAdmin").ToLower())
                {
                    emailNotiISOSNewList(ref data, ref msg_error);
                }
                else if (action_name.ToLower() == ("NotiDetailVisaRequest").ToLower())
                {
                    emailNotiISOSNewList(ref data, ref msg_error);
                }
                else if (action_name.ToLower() == ("NotiRequestPrepareVisa").ToLower())
                {
                    emailNotiISOSNewList(ref data, ref msg_error);
                }
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel
            {
                status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.",
                remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error
            };
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel
            {
                status = "Error msg",
                remark = msg_error
            };

            return data;
        }

        #endregion  emailconfig 

        private void emailNotiTravelInsuranceForm(ref EmailModel data, ref string msg_error)
        {
            var token_login = data.token_login;
            var doc_id = data.doc_id;

            //แจงเตือนเพõ่อใหพนักงานเขายืนยันขอมูลในแบบฟอรม "คําขอเอาประกันภัยการเดินทาง
            //ตางประเทศ" ในระบบ
            //To: ผูเดินทาง
            //Cc: Super Admin โดยการเลือก กลุม PMSV หรéอ PMDV  
            //ระบบสรางกลุม E-Mail Group E-Biz ทั้ง Group PMSV และ PMDV --> Employee / Admin (SV&DV)

            //หาผู้เดินทางของ doc id ที่ CAP Approve แล้ว
            DataTable dtemp = new DataTable();
            swd = new searchDocTravelerProfileServices();
            // sqlstr = swd.sqlstr_data_emp_detail(token_login, doc_id, "ListEmployeeTravelInsurance");
            var cmd = swd.SqlstrDataEmpDetailWithCommand(token_login, doc_id, "ListEmployeeTravelInsurance");
            sqlstr = cmd.CommandText;
            if (SetDocService.conn_ExecuteData(ref dtemp, cmd) == "")
            {
                if (dtemp.Rows.Count == 0)
                {
                    msg_error = "ไม่มีข้อมูลที่จะส่งไป แจ้งเตือนพนักงานเพื่อให้กรอกคำขอเอาประกันภัยการเดินทางต่างประเทศ";
                    return;
                }
                else
                {
                    List<emailList> email_list = new List<emailList>();
                    for (int i = 0; i < dtemp.Rows.Count; i++)
                    {
                        emailList deflist = new emailList
                        {
                            id = (i + 1).ToString(),
                            doc_id = doc_id,
                            emp_id = dtemp.Rows[i]["emp_id"].ToString(),
                            emp_name = dtemp.Rows[i]["emp_name"].ToString(),
                            email = dtemp.Rows[i]["email"].ToString(),
                            email_status = "",
                            email_msg = ""
                        };
                        email_list.Add(deflist);
                    }
                    data.email_list = email_list;
                }
            }

            //หากลุ่ม Super Admin 
            string saduser_email = "";
            data.aduser_list = ListSupperAdmin(ref saduser_email, "");

            //send mail 
            if (true)
            {
                string s_mail_to = "";
                string s_mail_cc = "";
                string s_mail_subject = "";
                string s_mail_body = "";
                string s_mail_attachments = "";

                for (int i = 0; i < data.email_list.Count; i++)
                {
                    s_mail_to += data.email_list[i].email.ToString() + ";";
                }
                s_mail_cc = saduser_email;

                s_mail_subject = @"Email แจ้งกรอกข้อมูล Traveling Insurance Form";
                s_mail_body = @"Admin (User Request) เขาดําเนินการ submit to send Travel Insurance หรéอ คําขอ
                                เอาประกันภัยการเดินทางตางประเทศ ใหผูเดินทางกรอกขอมูล
                                ระบบสราง Travel Insurance เพõ่อนําสงขอมูลทาง E-Mail ใหกับบรæษัทฯ
                                ประกันได ";

                msg_error = send_mail(doc_id, "EmailNotiTravelInsuranceForm", s_mail_to, s_mail_cc, s_mail_subject, s_mail_body, s_mail_attachments, "").GetAwaiter().GetResult();

            }
        }
        private void emailNotiTravelInsuranceListPassportInfo(ref EmailModel data, ref string msg_error)
        {
            var token_login = data.token_login;
            var doc_id = data.doc_id;
            var emp_id = data.emp_id; //user ที่ส่งข้อมูลมา กรณีที่เป็น admin ให้ส่งทั้งหมด  กรณีที่เป็นเจ้าของข้อมูลให้ส่งตัวเองเท่านั้น
            var page_name = "travelinsurance";

            sws = new SetDocService();
            sws.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);


            //TO: Admin(User Request)
            //CC: ผู้เดินทาง 

            //หาผู้เดินทางของ doc id ที่ CAP Approve แล้ว
            DataTable dtemp = new DataTable();
            swd = new searchDocTravelerProfileServices();
            sqlstr = swd.sqlstr_data_emp_detail(token_login, doc_id, "ListEmployeeTravelInsurance");
            if (SetDocService.conn_ExecuteData(ref dtemp, sqlstr) == "")
            {
                if (dtemp.Rows.Count == 0)
                {
                    msg_error = "ไม่มีข้อมูลที่จะส่งไป แจ้งเตือนพนักงานเพื่อให้กรอกคำขอเอาประกันภัยการเดินทางต่างประเทศ";
                    return;
                }
                else
                {
                    List<emailList> email_list = new List<emailList>();
                    for (int i = 0; i < dtemp.Rows.Count; i++)
                    {
                        if (emp_id == dtemp.Rows[i]["emp_id"].ToString() || user_admin == true)
                        {
                            emailList deflist = new emailList
                            {
                                id = (i + 1).ToString(),
                                doc_id = doc_id,
                                emp_id = dtemp.Rows[i]["emp_id"].ToString(),
                                emp_name = dtemp.Rows[i]["emp_name"].ToString(),
                                email = dtemp.Rows[i]["email"].ToString(),
                                email_status = "",
                                email_msg = ""
                            };
                            email_list.Add(deflist);
                        }
                    }
                    data.email_list = email_list;
                }
            }

            //หากลุ่ม Super Admin 
            string saduser_email = "";
            data.aduser_list = ListSupperAdmin(ref saduser_email, "");

            //ส่งข้อมูลให้ทาง admin เพื่อดำเนินการแก้ไขก่อน step ต่อไปจะส่งให้ บริษัทฯประกัน   
            List<ImgList> fileList = new List<ImgList>();

            List<ImgList> imgList = swd.refdata_img_list(doc_id, page_name, "", token_login);
            if (user_admin == false)
            {
                for (int i = 0; i < imgList.Count; i++)
                {
                    if (emp_id == imgList[i].emp_id.ToString() || user_admin == true)
                    {
                        data.file_list.Add(imgList[i]);
                    }
                }
            }

            //send mail 
            if (true)
            {
                string s_mail_to = "";
                string s_mail_cc = "";
                string s_mail_subject = "";
                string s_mail_body = "";
                string s_mail_attachments = "";

                for (int i = 0; i < data.email_list.Count; i++)
                {
                    s_mail_cc += data.email_list[i].email.ToString() + ";";
                }
                s_mail_to = saduser_email;

                for (int i = 0; i < data.file_list.Count; i++)
                {
                    if (s_mail_attachments != "") { s_mail_attachments += "||"; }
                    s_mail_attachments += data.file_list[i].path + data.file_list[i].filename;
                }

                s_mail_subject = @"Email แจ้ง Travel Insurance List + Passport Info";
                s_mail_body = @"1) พนักงานเขายืนยันขอมูลในแบบฟอรม คําขอเอาประกันภัยการเดินทางตางประเทศ หรือบน
                                ระบบ ได้แก่
                                 • ชื่อผูรับผลประโยชน
                                 • ความสัมพันธกับ ผูรับผลประโยชน
                                 • พนักงานสามารถเพิ่มเติม Travel Insurance แบบสวนตัวได
                                2) ขอมูลอื่นๆ ใหทางระบบดึงขอมูลจากฐานขอมูล และระบบ E - B";

                msg_error = send_mail(doc_id, "EmailNotiTravelInsuranceListPassportInfo", s_mail_to, s_mail_cc, s_mail_subject, s_mail_body, s_mail_attachments).GetAwaiter().GetResult();

            }

        }
        private void emailNotiTravelInsuranceCertificates(ref EmailModel data, ref string msg_error)
        {
            var token_login = data.token_login;
            var doc_id = data.doc_id;

            //•  Email แจ้ง Travel Insurance Certificates  
            //• Admin(User Request)ตรวจสอบขอมูลพรอมสง submit to send Travel Insurance
            //หรéอ คําขอเอาประกันภัยการเดินทางตางประเทศ ใหกับบรæษัทประกัน(Attached เปน PDF จาก
            //Insurance Form)
            //TO: บรæษัทฯ ประกัน(คาง E-Mail เดิม Defult คาไว)
            //CC: Super Admin โดยการเลือก กลุม PMSV หรéอ PMDV

            DataTable dtemp = new DataTable();
            string group_key_name = "E-Mail Group Company Insurance";
            sqlstr = @"select key_value, key_email  from  bz_config_data where trim(lower(key_name)) =trim(lower('" + group_key_name + "')) ";
            if (SetDocService.conn_ExecuteData(ref dtemp, sqlstr) == "")
            {
                if (dtemp.Rows.Count == 0)
                {
                    msg_error = "ไม่มีข้อมูลบริษัทฯประกัน";
                    return;
                }

                List<emailList> email_list = new List<emailList>();
                for (int i = 0; i < dtemp.Rows.Count; i++)
                {
                    emailList deflist = new emailList
                    {
                        id = (i + 1).ToString(),
                        doc_id = doc_id,
                        emp_id = "",
                        emp_name = dtemp.Rows[i]["key_value"].ToString(),
                        email = dtemp.Rows[i]["key_email"].ToString(),
                        email_status = "",
                        email_msg = ""
                    };
                    email_list.Add(deflist);
                }
                data.email_list = email_list;
            }
            //หากลุ่ม Super Admin 
            string saduser_email = "";
            data.aduser_list = ListSupperAdmin(ref saduser_email, "");

            //send mail 
            if (true)
            {
                string s_mail_to = "";
                string s_mail_cc = "";
                string s_mail_subject = "";
                string s_mail_body = "";
                string s_mail_attachments = "";

                for (int i = 0; i < data.email_list.Count; i++)
                {
                    s_mail_to += data.email_list[i].email.ToString() + ";";
                }
                s_mail_cc = saduser_email;

                s_mail_subject = @"Email แจ้ง Email แจ้ง Travel Insurance Certificates";
                s_mail_body = @"Admin(User Request)ตรวจสอบขอมูลพรอมสง submit to send Travel Insurance
                                หรéอ คําขอเอาประกันภัยการเดินทางตางประเทศ ใหกับบรæษัทประกัน(Attached เปน PDF จาก
                                Insurance Form)";

                msg_error = send_mail(doc_id, "EmailNotiTravelInsuranceCertificates", s_mail_to, s_mail_cc, s_mail_subject, s_mail_body, s_mail_attachments).GetAwaiter().GetResult();

            }
        }
        public void emailNotiISOSNewListRuningNoName(ref EmailModel data, ref string msg_error)
        {
            var token_login = data.token_login;
            var doc_id = data.doc_id;

            //เมื่อพบการ update กรมธรรมแลว เรçยบรอยแลว ใหระบบแจงเดือน Super Admin กลุมที่
            //requested Travel Insurance หรéอตรวจสอบหลักสูตรวาเปน business หรéอ training แลว
            //ดําเนินการดังนี้
            //1)หากไมพบ Record ใหระบบบันทึกรายชื่อ และ Create Runnin พรอมสงขอมูลใหกับบรæษัท
            //ISOS
            //TO: ISOS
            //CC: Super Admin โดยการเลือก กลุม PMSV หรéอ PMDV
            //ระบบสราง ISOS MEMBER LIST เพõ่อเก็บขอมูลการสมัคร ISOS

            //หาผู้เดินทางของ doc id ที่ CAP Approve แล้ว
            DataTable dtemp = new DataTable();
            swd = new searchDocTravelerProfileServices();
            sqlstr = @" select a.doc_id,a.isos_emp_id as emp_id,a.isos_emp_name ||' '|| a.isos_emp_surname as emp_name
                        ,bu.email as email
                        from bz_doc_isos a
                        left join vw_bz_users bu on a.isos_emp_id = bu.employeeid    
                        inner join (
                            select distinct ta.dh_code, ta.dta_appr_empid as approverid, ta.dta_travel_empid as employeeid
                            from  bz_doc_traveler_approver ta 
                            where lower(ta.dta_remark) = lower('CAP') and  ta.dta_doc_status = 42 and  lower(ta.dh_code) like lower('OB%') 
                         )ta   on a.isos_emp_id  = ta.employeeid and a.doc_id = ta.dh_code  
                        where a.send_mail = 0 and a.doc_id = '" + doc_id + "' order by a.isos_emp_id ";
            if (SetDocService.conn_ExecuteData(ref dtemp, sqlstr) == "")
            {
                if (dtemp.Rows.Count == 0)
                {
                    msg_error = "ไม่มีข้อมูล ISOS Member List";
                    return;
                }
                else
                {
                    List<emailList> email_list = new List<emailList>();
                    for (int i = 0; i < dtemp.Rows.Count; i++)
                    {
                        emailList deflist = new emailList
                        {
                            id = (i + 1).ToString(),
                            doc_id = doc_id,
                            emp_id = dtemp.Rows[i]["emp_id"].ToString(),
                            emp_name = dtemp.Rows[i]["emp_name"].ToString(),
                            email = dtemp.Rows[i]["email"].ToString(),
                            email_status = "",
                            email_msg = ""
                        };
                        email_list.Add(deflist);
                    }
                    data.email_list = email_list;
                }
            }

            //หากลุ่ม Super Admin 
            string saduser_email = "";
            data.aduser_list = ListSupperAdmin(ref saduser_email, "");

            //send mail 
            if (true)
            {
                string s_mail_to = "";
                string s_mail_cc = "";
                string s_mail_subject = "";
                string s_mail_body = "";
                string s_mail_attachments = "";

                for (int i = 0; i < data.email_list.Count; i++)
                {
                    s_mail_to += data.email_list[i].email.ToString() + ";";
                }
                s_mail_cc = saduser_email;

                s_mail_subject = @"Email แจ้ง ISOS New List (Running No + Name)";
                s_mail_body = @" เมื่อพบการ update กรมธรรมแลว เรçยบรอยแลว ใหระบบแจงเดือน Super Admin กลุมที่
                                 requested Travel Insurance หรéอตรวจสอบหลักสูตรวาเปน business หรéอ training แลว
                                 ดําเนินการดังนี้
                                 1)หากไมพบ Record ใหระบบบันทึกรายชื่อ และ Create Runnin พรอมสงขอมูลใหกับบรæษัท
                                 ISOS";

                msg_error = send_mail(doc_id, "EmailNotiISOSNewListRuningNoName", s_mail_to, s_mail_cc, s_mail_subject, s_mail_body, s_mail_attachments).GetAwaiter().GetResult();

            }


        }
        private void emailNotiISOSNewList(ref EmailModel data, ref string msg_error)
        {
            var token_login = data.token_login;
            var doc_id = data.doc_id;

            //หากมีการ Update กรมธรรมแลว ใหดําเนินการแจง E-Mail ใหกับผูเกี่ยวของเพõ่อเขาตรวจสอบ
            //ขอมูล พรอมกับดูตารางกรมธรรม รายละเอียดเพòöมเติมเกี่ยวกับการเบิกคืนคาสินไหมสินไหม และ
            // รายละเอียดการใชและบรæการ ISOS ซึ่งแสดงทั้งบน E-Mail และสามารถเขาไปดูในระบบได
            //TO: ผูเดินทาง
            //CC: Super Admin โดยการเลือก กลุม PMSV หรéอ PMDV

            //หาผู้เดินทางของ doc id ที่ CAP Approve แล้ว
            DataTable dtemp = new DataTable();
            swd = new searchDocTravelerProfileServices();
            sqlstr = swd.sqlstr_data_emp_detail(token_login, doc_id, "ListEmployeeISOSMemberList");
            if (SetDocService.conn_ExecuteData(ref dtemp, sqlstr) == "")
            {
                if (dtemp.Rows.Count == 0)
                {
                    msg_error = "ไม่มีข้อมูลที่จะส่งไป แจ้งเตือนพนักงานเพื่อให้กรอกคำขอเอาประกันภัยการเดินทางต่างประเทศ";
                    return;
                }
                else
                {
                    List<emailList> email_list = new List<emailList>();
                    for (int i = 0; i < dtemp.Rows.Count; i++)
                    {
                        emailList deflist = new emailList
                        {
                            id = (i + 1).ToString(),
                            doc_id = doc_id,
                            emp_id = dtemp.Rows[i]["emp_id"].ToString(),
                            emp_name = dtemp.Rows[i]["emp_name"].ToString(),
                            email = dtemp.Rows[i]["email"].ToString(),
                            email_status = "",
                            email_msg = ""
                        };
                        email_list.Add(deflist);
                    }
                    data.email_list = email_list;
                }
            }

            //หากลุ่ม Super Admin 
            string saduser_email = "";
            data.aduser_list = ListSupperAdmin(ref saduser_email, "");

            //send mail 
            if (true)
            {
                string s_mail_to = "";
                string s_mail_cc = "";
                string s_mail_subject = "";
                string s_mail_body = "";
                string s_mail_attachments = "";

                for (int i = 0; i < data.email_list.Count; i++)
                {
                    s_mail_to += data.email_list[i].email.ToString() + ";";
                }
                s_mail_cc = saduser_email;

                s_mail_subject = @"Email แจ้งISOS New List";
                s_mail_body = @"หากมีการ Update กรมธรรมแลว ใหดําเนินการแจง E-Mail ใหกับผูเกี่ยวของเพõ่อเขาตรวจสอบ
                                ขอมูล พรอมกับดูตารางกรมธรรม รายละเอียดเพòöมเติมเกี่ยวกับการเบิกคืนคาสินไหมสินไหม และ
                                รายละเอียดการใชและบรæการ ISOS ซึ่งแสดงทั้งบน E-Mail และสามารถเขาไปดูในระบบได";

                msg_error = send_mail(doc_id, "EmailNotiISOSNewList", s_mail_to, s_mail_cc, s_mail_subject, s_mail_body, s_mail_attachments).GetAwaiter().GetResult();

            }

        }
        public void emailNotiISOSNewRecord(ref EmailModel data, ref string msg_error)
        {
            var token_login = data.token_login;
            var doc_id = data.doc_id;
            var parameters = new List<OracleParameter>();

            using (TOPEBizTravelerProfileEntitys context = new TOPEBizTravelerProfileEntitys())
            {

                //เมื่อพบการ update กรมธรรมแลว เรçยบรอยแลว ใหระบบแจงเดือน Super Admin กลุมที่
                //requested Travel Insurance หรéอตรวจสอบหลักสูตรวาเปน business หรéอ training แลว
                //ดําเนินการดังนี้
                //1)หากไมพบ Record ใหระบบบันทึกรายชื่อ และ Create Runnin พรอมสงขอมูลใหกับบรæษัท
                //ISOS
                //TO: ISOS
                //CC: Super Admin โดยการเลือก กลุม PMSV หรéอ PMDV
                //ระบบสราง ISOS MEMBER LIST เพõ่อเก็บขอมูลการสมัคร ISOS

                //หาผู้เดินทางของ doc id ที่ CAP Approve แล้ว
                DataTable dtemp = new DataTable();
                swd = new searchDocTravelerProfileServices();
                sqlstr = @" 
                      select a.id, a.doc_id, a.emp_id, bu.email as email,insurance_company_id
                      from bz_doc_isos_record a
                      left join vw_bz_users bu on a.emp_id = bu.employeeid     
                      where nvl(a.send_mail_type,0) = 0 
                      and a.doc_id = '" + doc_id + "'  order by a.emp_id ";
                if (SetDocService.conn_ExecuteData(ref dtemp, sqlstr) == "")
                {
                    if (dtemp.Rows.Count == 0)
                    {
                        msg_error = "ไม่มีข้อมูล ISOS New Record";
                        return;
                    }
                    else
                    {
                        List<emailList> email_list = new List<emailList>();
                        for (int i = 0; i < dtemp.Rows.Count; i++)
                        {
                            emailList deflist = new emailList
                            {
                                id = dtemp.Rows[i]["id"].ToString(),
                                doc_id = dtemp.Rows[i]["doc_id"].ToString(),
                                emp_id = dtemp.Rows[i]["emp_id"].ToString(),
                                email = dtemp.Rows[i]["email"].ToString(),
                                email_status = "",
                                email_msg = ""
                            };
                            email_list.Add(deflist);
                        }
                        data.email_list = email_list;
                    }
                }

                //หากลุ่ม Super Admin 
                string saduser_email = "";
                data.aduser_list = ListSupperAdmin(ref saduser_email, "");

                //send mail 
                if (true)
                {
                    string s_mail_to = "";
                    string s_mail_cc = "";
                    string s_mail_subject = "";
                    string s_mail_body = "";
                    string s_mail_attachments = "";

                    for (int i = 0; i < data.email_list.Count; i++)
                    {
                        s_mail_to += data.email_list[i].email.ToString() + ";";
                    }
                    s_mail_cc = saduser_email;

                    s_mail_subject = @"Email แจ้ง ISOS New Record (Running No + Name)";
                    s_mail_body = @" หากไมพบ Record ใหระบบบันทึกรายชื่อ และ Create Runnin พรอมสงขอมูลใหกับบรæษัท ISOS";

                    msg_error = send_mail(doc_id, "EmailNotiISOSNewRecord", s_mail_to, s_mail_cc, s_mail_subject, s_mail_body, s_mail_attachments).GetAwaiter().GetResult();
                    if (msg_error == "")
                    {
                        string emp_user_active = "";
                        sqlstr = @"
            UPDATE BZ_DOC_ISOS_RECORD 
            SET SEND_MAIL_TYPE = 1, 
                UPDATE_BY = :update_by, 
                UPDATE_DATE = sysdate, 
                TOKEN_UPDATE = :token_update
            WHERE DOC_ID = :doc_id";

                        parameters.Add(new OracleParameter("update_by", emp_user_active));
                        parameters.Add(new OracleParameter("token_update", token_login));
                        parameters.Add(new OracleParameter("doc_id", data.doc_id));

                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()).ToString();
                        ret = SetDocService.execute_data_ex(iret, true);
                    }
                }
            }


        }

        #region In Page
        public string SendMailInPage(ref List<mailselectList> mail_list
            , List<EmpListOutModel> emp_list
            , List<ImgList> img_list
            , string doc_id, string page_name, string module_name)
        {
            try
            {
                page_name = page_name.ToLower();
                module_name = module_name.ToLower();

                //CONFIRMATION LETTER	028_OB/LB/OT/LT : Business Travel Confirmation Letter
                //ตั้ง batch --> Local 3 day, Overse 5 day  เช่น  busses พฤ ให้ส่งวันจันทร์
                //??? ให้ตรวจสอบข้อมูลไฟล์แนบใหม่ให้เกินจำนวน limit
                string super_admin_mail = "";
                string pmsv_admin_mail = "";
                string pmdv_admin_mail = "";
                string requester_mail = "";
                string initiator_mail = "";
                string on_behalf_of_mail = "";
                string traveler_mail = "";
                string line_approver_mail = "";
                string cap_approver_mail = "";
                string requester_name = "";
                string initial_mail = "";
                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                //mail_list จะมีแค่รายการเดียว
                try
                {
                    #region DevFix 20200911 0000 
                    //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/master/###
                    string LinkLoginTravelerhistory = top.ebiz.helper.AppEnvironment.GeteLinkLoginTravelerhistory().ToString();
                    string sDear = "";
                    string sDetail = "";
                    string sTitle = "";
                    string sBusinessDate = "";
                    string sLocation = "";
                    string sTravelerList = "";
                    string sOtherList = "";
                    Boolean SendmailtoBroker = false;
                    DataTable dtresult = new DataTable();
                    string Tel_Services_Team = "";
                    string Tel_Call_Center = "";
                    sqlstr = @" SELECT key_value as tel_services_team from bz_config_data where lower(key_name) = lower('tel_services_team') and status = 1";
                    SetDocService.conn_ExecuteData(ref dtresult, sqlstr);
                    try { Tel_Services_Team = dtresult.Rows[0]["tel_services_team"]?.ToString() ?? ""; } catch { }
                    sqlstr = @" SELECT key_value as tel_call_center from bz_config_data where lower(key_name) = lower('tel_call_center') and status = 1";
                    SetDocService.conn_ExecuteData(ref dtresult, sqlstr);
                    try { Tel_Call_Center = dtresult.Rows[0]["tel_call_center"]?.ToString() ?? ""; } catch { }
                    #endregion DevFix 20200911 0000  

                    string msg_log = "";
                    int iNo = 1;
                    ret = "";
                    for (int i = 0; i < mail_list.Count; i++)
                    {
                        string xhtml = "";
                        string continent_id = "";
                        string country_id = "";
                        string emp_id_select = "";
                        string emp_id = mail_list[i].emp_id.ToLower();
                        string[] xemp_id = emp_id.Split(';');

                        //ตรวจสอบว่ามีการ active เพื่อส่ง mail หรือไม่จาก mail_status = 'true'
                        List<EmpListOutModel> drempcheck = emp_list.Where(a => ((a.emp_id.ToLower() == emp_id) && a.mail_status == "true")).ToList();
                        if (drempcheck.Count == 0)
                        {
                            if (emp_id.IndexOf(";") > -1)
                            {
                                for (int j = 0; j < xemp_id.Length; j++)
                                {
                                    if (page_name == "isos" && module_name == "sendmail_isos_to_broker")
                                    {
                                        //เอาข้อมูลตามที่เลือกจากหน้าบ้านทั้งหมด ให้เป็น mail_status = true
                                        drempcheck = emp_list.Where(a => ((a.emp_id.ToLower() == xemp_id[j].ToString()))).ToList();
                                        if (drempcheck.Count > 0)
                                        {
                                            drempcheck[0].mail_status = "true";
                                            if (emp_id_select != "") { emp_id_select += ","; }
                                            emp_id_select += "'" + xemp_id[j].ToString() + "'";
                                        }
                                    }
                                    else
                                    {
                                        drempcheck = emp_list.Where(a => ((a.emp_id.ToLower() == xemp_id[j].ToString()) && a.mail_status == "true")).ToList();
                                        if (drempcheck.Count > 0)
                                        {
                                            emp_id_select = "'" + xemp_id[j].ToString() + "'"; break;
                                        }
                                    }
                                }
                            }
                            if (page_name == "isos" && module_name == "sendmail_isos_to_broker")
                            {
                                //กรองข้อมูลอีกครั้งเพื่อใช้ในลำดับถัดไป
                                drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                            }
                        }
                        if (drempcheck.Count == 0)
                        {
                            //ตรวจสอบว่ามีการ active เพื่อส่ง mail หรือไม่จาก mail_status = 'true' กรณีที่เป็นการส่งหา admin ให้เอารายละเอียดจากคนแรก
                            drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                            if (drempcheck.Count > 0)
                            {
                                emp_id_select = "'" + drempcheck[0].emp_id + "'";
                            }
                            else { continue; }
                        }
                        if ((page_name == "travelexpense" && module_name == "tripcancelled")
                            || (page_name == "travelexpense" && module_name == "sendmail_to_sap"))
                        {
                            drempcheck = emp_list.Where(a => ("true" == "true")).ToList();
                        }
                        if (mail_list[i].module == "Sendmail to Broker")
                        {
                            SendmailtoBroker = true;
                        }

                        string s_mail_to = (mail_list[i].mail_to + "").ToString();
                        string s_mail_cc = (mail_list[i].mail_cc + "").ToString();
                        string s_subject = "";
                        string s_mail_body = "";
                        string s_mail_to_display = "";
                        string s_mail_body_in_form = "";
                        string s_mail_attachments = "";
                        string s_mail_to_emp_name = "All,";
                        string s_mail_show_case = "";
                        string resMailShowCase = "";
                        try
                        {
                            s_mail_to_display = (mail_list[i].mail_to_display + "").ToString();
                        }
                        catch { }
                        try
                        {
                            s_mail_body_in_form = (mail_list[i].mail_body_in_form + "").ToString();
                        }
                        catch { }
                        try
                        {
                            s_mail_attachments = (mail_list[i].mail_attachments + "").ToString();
                        }
                        catch { }
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            if (doc_id != "")
                            {
                                try
                                {
                                    //                                    var sqlTravel = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                                    //, b.employeeid as name3, b.orgname as name4
                                    //from BZ_DOC_TRAVELER_EXPENSE a left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid 
                                    //left join (select min(dte_id) as id, dh_code, dte_emp_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                    //on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id 
                                    //where a.dh_code = :doc_id and nvl(a.dte_status,0) <> 0 order by s.id";

                                    var sqlTravel = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
, b.employeeid as name3, b.orgname as name4,a.dte_cap_appr_opt
from BZ_DOC_TRAVELER_EXPENSE a left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid 
left join (select min(dte_id) as id, dh_code, dte_emp_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id 
where a.dh_code = :doc_id and a.dte_cap_appr_opt = 'true'
 and nvl(a.dte_status,0) <> 0 order by s.id";

                                    var parametersTravel = new List<OracleParameter>();
                                    parametersTravel.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    var tempTravel = context.TempModelList.FromSqlRaw(sqlTravel, parametersTravel.ToArray()).ToList();
                                    if (tempTravel != null)
                                    {
                                        foreach (var item in tempTravel)
                                        {
                                            traveler_mail += ";" + item.name2;
                                        }
                                    }

                                    string sql = @"SELECT u.EMAIL as initial_mail, '' as approver_mail
                                FROM BZ_DOC_HEAD d
                                JOIN vw_bz_users u ON d.DH_INITIATOR_EMPID = u.EMPLOYEEID
                                WHERE d.DH_CODE = :doc_id";

                                    var parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                                    var result = context.Database.SqlQueryRaw<EmailResult>(sql, parameters.ToArray()).ToList();

                                    if (!string.IsNullOrEmpty(doc_id))
                                        if (result != null && result.Count > 0)
                                        {
                                            initial_mail = result[0].initial_mail;
                                        }
                                }
                                catch (Exception ex)
                                {
                                    // Handle exception
                                }
                            }

                            // สร้างอินสแตนซ์ของคลาส documentService
                            _documentService documentServiceInstance = new _documentService();

                            // ดึงอีเมลของแอดมิน
                            super_admin_mail = documentServiceInstance.mail_group_admin(context, "super_admin");

                            //  super_admin_mail = documentServiceInstance.get_mail_group_admin(context);

                            // ดึงอีเมลของ pmsv_admin และ pmdv_admin
                            pmsv_admin_mail = documentServiceInstance.mail_group_admin(context, "pmsv_admin");
                            if (doc_id.IndexOf("T") > -1)
                            {
                                pmdv_admin_mail += documentServiceInstance.mail_group_admin(context, "pmdv_admin");
                            }

                            // ดึงอีเมลของผู้ขอ
                            documentServiceInstance.get_mail_requester_in_doc(context, doc_id, ref requester_name, ref requester_mail, ref on_behalf_of_mail);

                            // สร้างออบเจ็กต์ dataMail หากจำเป็น
                            Models.Create_Trip.sendEmailModel dataMail = new Models.Create_Trip.sendEmailModel();

                            if (page_name == "airticket" || page_name == "accommodation" || page_name == "travelinsurance")
                            {
                                if (module_name == "admin_confirmed" || module_name == "sendmail_to_traveler" || module_name == "sendmail_isos_to_broker")
                                {
                                    //s_mail_to = traveler_mail;
                                    //s_mail_cc = (super_admin_mail + pmsv_admin_mail);
                                    dataMail.mail_to = s_mail_to.ToString();
                                    // dataMail.mail_to =  traveler_mail;
                                    dataMail.mail_cc = (super_admin_mail + pmsv_admin_mail);
                                }
                                else if (module_name == "admin_not_confirmed")
                                {
                                    dataMail.mail_to = (super_admin_mail + pmsv_admin_mail);
                                    dataMail.mail_cc = s_mail_to.ToString();
                                }

                                else
                                {

                                    //s_mail_to = traveler_mail;
                                    //s_mail_cc = (super_admin_mail + pmsv_admin_mail);
                                    //dataMail.mail_to = traveler_mail;
                                    dataMail.mail_to = (super_admin_mail + pmsv_admin_mail);
                                    dataMail.mail_cc = s_mail_cc.ToString();


                                }

                            }
                            else
                            {
                                if (module_name == "sendmail_isos_to_broker")
                                {
                                    //s_mail_to = traveler_mail;
                                    //s_mail_cc = (super_admin_mail + pmsv_admin_mail);
                                    dataMail.mail_to = s_mail_to.ToString();
                                    // dataMail.mail_to =  traveler_mail;
                                    dataMail.mail_cc = (super_admin_mail + pmsv_admin_mail);
                                }
                                else
                                {
                                    //s_mail_to = (super_admin_mail + pmsv_admin_mail);
                                    //s_mail_cc = requester_mail + on_behalf_of_mail + initial_mail + traveler_mail;
                                    dataMail.mail_to = (super_admin_mail + pmsv_admin_mail);
                                    dataMail.mail_cc = requester_mail + on_behalf_of_mail + initial_mail + traveler_mail;
                                }

                            }



                            if (page_name == "travelinsurance" || page_name == "transportation")
                            {
                                if (module_name == "sendmail_to_insurance")
                                {
                                    if (!string.IsNullOrEmpty(s_mail_to))
                                    {
                                        resMailShowCase += $"{s_mail_to?.TrimStart(';').ToLower()} <span style='color:#666;'>(Insurance)</span> ";
                                    }
                                    resMailShowCase += $"<br>Cc: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span>";
                                }
                                else
                                {
                                    resMailShowCase = $"<div>To: ";
                                    //if (!string.IsNullOrEmpty(requester_mail))
                                    //{
                                    //    resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                    //}
                                    //if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                    //{
                                    //    resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                    //}
                                    //if (!string.IsNullOrEmpty(initial_mail))
                                    //{
                                    //    resMailShowCase += $"{initial_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Initiator)</span> ";
                                    //}
                                    //if (!string.IsNullOrEmpty(traveler_mail))
                                    //{
                                    //    resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span>";
                                    //}
                                    if (!string.IsNullOrEmpty(s_mail_to))
                                    {
                                        resMailShowCase += $"{s_mail_to?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span> ";
                                    }
                                    resMailShowCase += $"<br>Cc: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span>";
                                }
                                resMailShowCase += "</div>";

                                s_mail_show_case = resMailShowCase;
                            }
                            else if (page_name == "airticket" || page_name == "accommodation")
                            {
                                if (module_name == "admin_confirmed")
                                {
                                    resMailShowCase = $"<div>To: ";

                                    if (!string.IsNullOrEmpty(s_mail_to))
                                    {
                                        resMailShowCase += $"{s_mail_to?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span>";
                                    }
                                    resMailShowCase += $"<br>Cc: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span>";

                                    resMailShowCase += "</div>";

                                    s_mail_show_case = resMailShowCase;
                                }

                                else
                                {
                                    resMailShowCase = $"<div>To: ";

                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span>";
                                    resMailShowCase += $"<br>Cc: ";

                                    if (!string.IsNullOrEmpty(s_mail_cc))
                                    {
                                        resMailShowCase += $"{s_mail_cc?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span>";
                                    }
                                    resMailShowCase += "</div>";

                                    s_mail_show_case = resMailShowCase;
                                }

                            }
                            else
                            {
                                resMailShowCase = $"<div>To: ";
                                resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span>";
                                resMailShowCase += $"<br>Cc: ";
                                //if (!string.IsNullOrEmpty(requester_mail))
                                //{
                                //    resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                //}
                                //if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                //{
                                //    resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                //}
                                //if (!string.IsNullOrEmpty(initial_mail))
                                //{
                                //    resMailShowCase += $"{initial_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Initiator)</span> ";
                                //}
                                //if (!string.IsNullOrEmpty(traveler_mail))
                                //{
                                //    resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span>";
                                //}
                                if (!string.IsNullOrEmpty(s_mail_cc))
                                {
                                    resMailShowCase += $"{s_mail_cc?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span>";
                                }

                                resMailShowCase += "</div>";

                                s_mail_show_case = resMailShowCase;
                            }
                            // จัดรูปแบบการแสดงผู้รับอีเมล

                        }

                        if (page_name == "isos" || page_name == "isosrecord")
                        {
                        }
                        else
                        {
                            continent_id = drempcheck[0].continent_id;
                            country_id = drempcheck[0].country_id;
                            try
                            {
                                string[] xemp = s_mail_to.Split(';');
                                if ((xemp.Length == 1) || (xemp[1].ToString() == ""))
                                {
                                    string smail = xemp[0].ToString();
                                    //กรณีที่มีเพียง 1 คน --> xxx@thaioilgroup.com;
                                    List<EmpListOutModel> dremp = emp_list.Where(a => (a.userEmail.ToLower() == smail.ToLower())).ToList();
                                    if (dremp.Count > 0)
                                    {
                                        s_mail_to_emp_name = dremp[0].userDisplay;
                                    }
                                }
                            }
                            catch { }
                        }

                        s_subject = "E-Biz : Test Send E-Mail Data (" + doc_id + ")";

                        #region ข้อมูลที่ต้องส่งใน mail ของแต่ละ module
                        if (page_name == "airticket")
                        {
                            //009_OB/LB/OT/LT  --> traveler กด และ status = confirmed กับถ้าเป็นการเลือก Ask Booking by Company ต้องมี Already Booked = true
                            //010_OB/LB/OT/LT  --> admin กด และ status <> confirmed 
                            //011_OB/LB/OT/LT  --> admin กด และ status = confirmed กับถ้าเป็นการเลือก Ask Booking by Company ต้องมี Already Booked = true
                            if (module_name == "traveler_request")
                            {
                                //กรณีที่ ทาง traveler submit ส่ง 009_OB/LB/OT/LT : Please book an Air Ticket - [Title_Name of traveler]  ต้องการให้ทาง admin จองตั๋ว
                                s_subject = doc_id + " : Please book an Air Ticket for " + s_mail_to_display;
                                sDear = @"Dear Admin,";
                                sDetail = "Traveler has been request company to book an Air Ticket. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                            else if (module_name == "admin_not_confirmed")
                            {
                                //admin กดส่ง 010_OB/LB/OT/LT : Please review an Air Ticket - [Title_Name of traveler] ให้ traveler มากรอก
                                //009_OB/LB/OT/LT : Please book an Air Ticket - [Title_Name of traveler] 
                                s_subject = doc_id + " : Please review an Air Ticket for " + s_mail_to_display;
                                sDear = @"Dear Admin,";
                                sDetail = "Traveler has been booked an Air Ticket. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                            else if (module_name == "admin_confirmed")
                            {
                                //admin ตรวจสเสร็จ กดส่ง 011_OB/LB/OT/LT : Air Ticket has been confirmed - [Title_Name of traveler] ให้ traveler ทราบ
                                s_subject = doc_id + " : Air Ticket has been confirmed for " + s_mail_to_display;
                                sDear = @"Dear " + s_mail_to_emp_name;
                                sDetail = "Your Air Ticket has been confirmed, To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                                sDetail += "<br>";
                                sDetail += "Please check and notify us as soon as possible if any information is incorrect or if there is an additional requirement.";
                            }
                            else if (module_name == "traveler_review")
                            {
                                //กรณีที่ ทาง traveler submit ส่ง 032_OB/LB/OT/LT : Please review an Air Ticket as booked by [Title_Name of traveler] 
                                s_subject = doc_id + " : Please review an Air Ticket as booked by " + s_mail_to_display;
                                sDear = @"Dear Admin,";
                                sDetail = "Traveler has been booked an Air Ticket. To check details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                        }
                        else if (page_name == "accommodation")
                        {
                            if (module_name == "traveler_request")
                            {
                                //012_OB/LB/OT/LT : Please book an Acommodation - [Title_Name of traveler]
                                s_subject = doc_id + " : Please book an Accommodation for " + s_mail_to_emp_name;
                                sDear = @"Dear Admin,";
                                sDetail = "Traveler has been request company to book an Accommodation. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                            else if (module_name == "admin_not_confirmed")
                            {
                                //013_OB/LB/OT/LT : Please review an Accommodation - [Title_Name of traveler]
                                s_subject = doc_id + " : Please review an Accommodation for " + s_mail_to_display;
                                sDear = @"Dear Admin,";
                                sDetail = "Traveler has been booked an Accommodation. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                            else if (module_name == "admin_confirmed")
                            {
                                //014_OB/LB/OT/LT : Acommodation has been confirmed - [Title_Name of traveler]
                                s_subject = doc_id + " : Accommodation has been confirmed for " + s_mail_to_display;
                                sDear = @"Dear " + s_mail_to_emp_name;
                                sDetail = "Your Accommodation has been confirmed, To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                                sDetail += "<br>";
                                sDetail += "Please check and notify us as soon as possible if any information is incorrect or if there is an additional requirement.";
                            }
                            else if (module_name == "traveler_review")
                            {
                                //กรณีที่ ทาง traveler submit ส่ง 032_OB/LB/OT/LT : Please review an Accommodation as booked by [Title_Name of traveler] 
                                s_subject = doc_id + " : Please review an Accommodation as booked by " + s_mail_to_display;
                                sDear = @"Dear Admin,";
                                sDetail = "Traveler has been booked an Accommodation. To check details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                        }
                        else if (page_name == "visa")
                        {
                            if (module_name == "sendmail_visa_requisition")
                            {
                                s_subject = doc_id + " : VISA Requisition for " + s_mail_to_display;
                                //Attachment : VISA Application (If any)
                                sDear = @"Dear All,";
                                sDetail = "You are require to submit VISA documents as follow. To view the details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";

                                //เพิ่มกรณีที่เดินทางไปมากกว่า 1 ประเทศ
                                sqlstr = @" select distinct te.dte_emp_id as emp_id, te.ct_id as country_id, ct.ct_name as country_name, ct.ctn_id as continent_id   
                                 from bz_doc_traveler_expense te
                                 inner join  bz_master_country ct on te.ct_id = ct.ct_id
                                 where te.dh_code = '" + doc_id + "' and te.dte_emp_id = '" + drempcheck[0].emp_id + "'";
                                if ((mail_list[i].country_in_doc + "") != "")
                                {
                                    sqlstr += @" and te.ct_id in (" + mail_list[i].country_in_doc + ")";
                                }

                                DataTable dtcountry = new DataTable();
                                SetDocService.conn_ExecuteData(ref dtcountry, sqlstr);
                                if (dtcountry.Rows.Count > 0)
                                {

                                    Boolean bcheckrows_white = true;
                                    for (int j = 0; j < dtcountry.Rows.Count; j++)
                                    {
                                        iNo = 1;
                                        int iItem = 1;

                                        sOtherList += " <div>";
                                        if (j == 0)
                                        {
                                            sOtherList += "<table width='1080' border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse;width:648.45pt;margin-left:35.7pt;'>";
                                        }
                                        else
                                        {
                                            sOtherList += "<table width='1080' border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse;width:648.45pt;margin-left:35.7pt;'>";
                                        }
                                        sOtherList += @"
                                                <tbody><tr height='33' style='height:19.8pt;'>
                                                <td width='87' style='width:52.25pt;height:19.8pt;background-color:#1F4E78;padding:0 5.4pt;border-style:solid dotted none solid;border-top-width:1pt;border-right-width:1pt;border-left-width:1pt;border-top-color:windowtext;border-right-color:#1F4E78;border-left-color:windowtext;'>
                                                <span style='background-color:#1F4E78;'>
                                                <div style='text-indent:15.5pt;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='white'><span style='font-size:15pt;'><b>Item</b></span></font></span></font></div>
                                                </span></td>
                                                <td width='812' style='width:487.75pt;height:19.8pt;background-color:#1F4E79;padding:0 5.4pt;border-style:solid dotted none none;border-top-width:1pt;border-right-width:1pt;border-top-color:windowtext;border-right-color:#1F4E78;'>
                                                <span style='background-color:#1F4E79;'>
                                                <div style='text-indent:15.5pt;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='white'><span style='font-size:15pt;'><b>
                                                Document List(" + dtcountry.Rows[j]["country_name"].ToString() + ")</b></span></font></span></font></div>" +
                                                 @" </span></td>
                                                <td width='180' style='width:108.45pt;height:19.8pt;background-color:#1F4E79;padding:0 5.4pt;border-style:solid solid none none;border-top-width:1pt;border-right-width:1pt;border-top-color:windowtext;border-right-color:windowtext;'>
                                                <span style='background-color:#1F4E79;'>
                                                <div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='white'><span style='font-size:15pt;'><b>by</b></span></font></span></font></div>
                                                </span></td>
                                                </tr>";

                                        string sdocument_text = "";
                                        string sdocument_by = "";
                                        string sborder_bt = "none";

                                        continent_id = dtcountry.Rows[j]["continent_id"].ToString();
                                        country_id = dtcountry.Rows[j]["country_id"].ToString();

                                        swd = new searchDocTravelerProfileServices();
                                        DataTable dtdesc = new DataTable();
                                        DataTable dtdocountries = swd.refdata_visa_docountries(continent_id, country_id, ref dtdesc);
                                        if (dtdocountries.Rows.Count > 0)
                                        {
                                            for (int k = 0; k < dtdocountries.Rows.Count; k++)
                                            {
                                                if (s_mail_attachments != "") { s_mail_attachments += "|"; }

                                                string file_name = @"Image\master visa docountries\mtvisacountries\" + dtdocountries.Rows[k]["filename"].ToString();
                                                string _FolderMailAttachments = top.ebiz.helper.AppEnvironment.GeteServerPathAPI();
                                                string mail_attachments = _FolderMailAttachments + file_name;
                                                s_mail_attachments += mail_attachments;
                                            }
                                        }
                                        if (dtdesc.Rows.Count > 0)
                                        {
                                            string border_bottom_color = "";
                                            for (int k = 0; k < dtdesc.Rows.Count; k++)
                                            {
                                                if (k == (dtdesc.Rows.Count - 1)) { border_bottom_color = "border-bottom-color:windowtext;"; sborder_bt = "solid"; }
                                                //s_mail_body_in_form += "<br>" + (k + 1) + " " + dtdesc.Rows[k]["docountries_name"].ToString();
                                                sdocument_text = dtdesc.Rows[k]["docountries_name"].ToString();
                                                sdocument_by = dtdesc.Rows[k]["preparing_by"].ToString();

                                                sOtherList += @"<tr height='24' style='height:14.4pt;'>";
                                                if (j == dtcountry.Rows.Count) { sborder_bt = "solid"; }
                                                if (bcheckrows_white == true)
                                                {
                                                    bcheckrows_white = false;
                                                    sOtherList += @" <td width='87' style='width:52.25pt;height:14.4pt;padding:0 5.4pt;" + border_bottom_color + " border-style:none dotted " + sborder_bt + " solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>" +
                                                                  @" <div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'>" +
                                                                  @"" + iItem + "</span></font></span></font></div></td>" +
                                                                  @" <td width='812' style='width:487.75pt;height:14.4pt;padding:0 5.4pt;" + border_bottom_color + "border-style:none dotted " + sborder_bt + " none;border-right-width:1pt;border-right-color:#1F4E78;'> " +
                                                                  @" <div style='margin:0 0 0 0.85pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'>" +
                                                                  @"" + sdocument_text + " </span></font></span></font></div></td>" +
                                                                  @" <td width='180' valign='bottom' nowrap='' style='width:108.45pt;height:14.4pt;padding:0 5.4pt;" + border_bottom_color + "border-style:none solid " + sborder_bt + " none;border-right-width:1pt;border-right-color:windowtext;'>" +
                                                                  @"<div style='margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>" +
                                                                  @"" + sdocument_by + " </span></font></div></td>";
                                                }
                                                else
                                                {
                                                    bcheckrows_white = true;
                                                    sOtherList += @" <td width='87' style='background-color:#DDEBF7;width:52.25pt;height:14.4pt;padding:0 5.4pt;" + border_bottom_color + " border-style:none dotted " + sborder_bt + " solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>" +
                                                             @"<div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'>" +
                                                             @"" + iItem + "</span></font></span></font></div></td>" +
                                                             @" <td width='812' style='background-color:#DDEBF7;width:487.75pt;height:14.4pt;padding:0 5.4pt;" + border_bottom_color + " border-style:none dotted " + sborder_bt + " none;border-right-width:1pt;border-right-color:#1F4E78;'>" +
                                                             @" <div style='margin:0 0 0 0.85pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'>" +
                                                             @"" + sdocument_text + "</span></font></span></font></div></td>" +
                                                             @" <td width='180' valign='bottom' nowrap='' style='background-color:#DDEBF7;width:108.45pt;height:14.4pt;padding:0 5.4pt;" + border_bottom_color + " border-style:none solid " + sborder_bt + " none;border-right-width:1pt;border-right-color:windowtext;'>" +
                                                             @" <div style='margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>" +
                                                             @"" + sdocument_by + "</span></font></div></td>";
                                                }
                                                sOtherList += @"</tr>";
                                                iItem++;

                                            }
                                        }

                                        sOtherList += "</table><br>";
                                        sOtherList += "</div>";

                                    }

                                }

                            }
                            else if (module_name == "sendmail_visa_employee_letter")
                            {
                                //016_OB/LB/OT/LT : Please review the Employee Letter - [Title_Name of traveler] 
                                //เป็น step ของ traveler update ข้อมูล ในหน้า visa
                                s_subject = doc_id + " : Please review the Employee Letter for " + s_mail_to_display;
                                sDear = @"Dear Admin,";
                                sDetail = "Please review the Employee Letter that has been requested as attached. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";

                            }
                        }
                        else if (page_name == "passport")
                        {
                            //017_OB/LB/OT/LT : Please update Passport information - [Title_Name of traveler]
                            //ส่งตอน CAP Approve แล้วและตรวจสอบได้ว่าไม่มี valid passport เเละให้ส่ง E-Mail  
                        }
                        else if (page_name == "allowance" || page_name == "reimbursement")
                        {
                            if (page_name == "allowance")
                            {
                                //018_OB/LB/OT/LT : Please submit an i-Petty Cash in Allowance - [Title_Name of traveler] 
                                s_subject = doc_id + " : Please create Allowance in i-Petty Cash for " + s_mail_to_display;
                                //Attachment : Allowance Payment Form
                                sDear = @"Dear All,";
                                sDetail = "Please create Allowance in i-Petty Cash for Traveler as attached. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                            else if (page_name == "reimbursement")
                            {
                                //019_OB/LB/OT/LT : Please create Reimbursement in i-Petty Cash for [Title_Name of traveler]
                                s_subject = doc_id + " : Please create Reimbursement in i-Petty Cash for " + s_mail_to_display;
                                //Attachment : Allowance Payment Form
                                sDear = @"Dear All,";
                                sDetail = "Please create Reimbursement in i-Petty Cash for Traveler as attached. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }

                            msg_log += "Send Mail Page : Configuration";
                        }
                        else if (page_name == "travelinsurance" || (page_name == "isos" && module_name == "sendmail_isos_to_traveler"))
                        {
                            if (module_name == "claim form requisition")
                            {
                                //023_OB/LB/OT/LT : Travel Insurance Claim Form Requested - [Title_Name of traveler]
                                s_subject = doc_id + " : Travel Insurance Claim Form Requested - " + s_mail_to_emp_name;

                                sDear = @"Dear " + s_mail_to_emp_name + ",";
                                sDetail = "Please submit the following documents to receive further reimbursement. To view travel details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";

                                sOtherList = @"Procedures and forms for Travel Insurance Claim :";
                                sOtherList += @"<br>";
                                sOtherList += @"1. Complete and sign the Travel Insurance Claim Form (attached).";
                                sOtherList += @"<br>";
                                sOtherList += @"2. Original medical certificate and receipt";
                                sOtherList += @"<br>";
                                sOtherList += @"3. A copy of your ID, a copy of your passport, and a copy of your departure/arrival Thailand boarding pass. All are required to sign each paper.";
                                sOtherList += @"<br>";
                                sOtherList += @"4. Make a copy of the first page of the bank account. All are required to sign each paper. (In the case of employees who have been paid in advance)";

                                //string file_name = @"DocumentFile\Travelinsurance\Claim form Requisition.pdf";
                                string file_name = @"DocumentFile\Travelinsurance\New Travel Claim Form (003).pdf";
                                string _FolderMailAttachments = top.ebiz.helper.AppEnvironment.GeteServerPathAPI();
                                string mail_attachments = _FolderMailAttachments + file_name;
                                s_mail_attachments += mail_attachments;

                            }
                            else
                            {
                                //รายละเอียดไฟล์แนบมาจาก SetTravelInsurance โดยเเยกเป็น module = "Sendmail to Broker" กับ module = "Sendmail to Traveler";
                                if (module_name == "sendmail_to_traveler" || module_name == "sendmail_isos_to_traveler")
                                {
                                    //020_OB/LB/OT/LT : Please complete Travel Insurance form - [Title_Name of traveler]
                                    s_subject = doc_id + " : Please complete Travel Insurance form - " + s_mail_to_emp_name;

                                    sDear = @"Dear " + s_mail_to_emp_name + ",";
                                    sDetail = "Your require to complete a Travel Insurance form. To view travel details, click ";
                                    sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";


                                    sOtherList = @"Note : Your also require to update passport for Travel Insurance application. To view travel details, click ";
                                    sOtherList += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                                }
                                else if (module_name == "sendmail_to_insurance")
                                {
                                    //021_OB/LB/OT/LT : Please submit Travel Insurance Certificate - [Title_Name of traveler]
                                    s_subject = doc_id + " : Please submit Travel Insurance Certificate - " + s_mail_to_display;
                                    //Attachment : Travel Insurance Form and Passport

                                    sDear = @"Dear " + s_mail_to_display + ",";//Dear [Title_Name of Broker],
                                    sDetail = "To requested Travel Insurance Certificate, please find a Travel Insurance form and a copy passport as attached.";

                                    #region เพิ่มแนบไฟล์ Passport    
                                    sqlstr = @"
                                select distinct null as doc_id  ,a.emp_id
                                , a.id 
                                , a.id_level_1, a.id_level_2
                                , a.path
                                , a.file_name as filename
                                , a.page_name as pagename
                                , a.action_name as actionname
                                , a.path || a.file_name as fullname
                                , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) as modified_by
                                , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
                                , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                                , nvl(active_type,'false') as active_type
                                from bz_doc_img a
                                left join vw_bz_users u on (case when a.update_by is null then a.create_by else a.update_by end) = u.employeeid 
                                where a.status = 1 and lower(a.page_name) = lower('passport')  
                                and (a.emp_id,a.id_level_1) in (select emp_id,id from bz_doc_passport ad  where  (ad.default_type is not null and ad.default_type = 'true')    )
                                and a.emp_id = '" + emp_id + "'";
                                    sqlstr += " order by a.id ";
                                    DataTable dtimg = new DataTable();

                                    if (SetDocService.conn_ExecuteData(ref dtimg, sqlstr) == "")
                                    {
                                        //xxxx.jpg | vvvv.jpg
                                        if (dtimg.Rows.Count > 0)
                                        {
                                            //s_mail_body_in_form += "<br>Passport file : ";
                                            for (int k = 0; k < dtimg.Rows.Count; k++)
                                            {

                                                string fullPath = dtimg.Rows[k]["fullname"].ToString();
                                                //write_log_mail("Attached fullPathk", fullPath.ToString());
                                                string emp_id_def = dtimg.Rows[k]["emp_id"].ToString();
                                                if (s_mail_attachments != "") { s_mail_attachments += "|"; }
                                                if (fullPath.Contains(@"/personal/personal/passport/"))
                                                {
                                                    // แทนที่ \personal\personal\passport\ ด้วย \personal\passport\

                                                    string doc_folder = "personal";
                                                    //string file_name = @"Image\" + doc_folder + @"\passport\" + emp_id_def + @"\" + dtimg.Rows[k]["filename"].ToString();
                                                    string file_name = doc_folder + @"\personal" + @"\passport\" + emp_id_def + @"\" + dtimg.Rows[k]["filename"].ToString();
                                                    string _FolderMailAttachments = top.ebiz.helper.AppEnvironment.GeteServerFolder();
                                                    _FolderMailAttachments = EnsureTrailingBackslash(_FolderMailAttachments);
                                                    string mail_attachments = _FolderMailAttachments + file_name;
                                                    s_mail_attachments += mail_attachments;
                                                }
                                                else
                                                {
                                                    string doc_folder = "personal";
                                                    //string file_name = @"Image\" + doc_folder + @"\passport\" + emp_id_def + @"\" + dtimg.Rows[k]["filename"].ToString();
                                                    string file_name = doc_folder + @"\" + doc_id + @"\passport\" + emp_id_def + @"\" + dtimg.Rows[k]["filename"].ToString();
                                                    string _FolderMailAttachments = top.ebiz.helper.AppEnvironment.GeteServerFolder();
                                                    _FolderMailAttachments = EnsureTrailingBackslash(_FolderMailAttachments);
                                                    string mail_attachments = _FolderMailAttachments + file_name;
                                                    s_mail_attachments += mail_attachments;
                                                }

                                                //write_log_mail("Attached s_mail_attachments", s_mail_attachments.ToString());


                                                //s_mail_body_in_form += "<br>Emp ID: " + dtimg.Rows[k]["emp_id"].ToString() + " <br>File Name: " + dtimg.Rows[k]["filename"].ToString();
                                            }
                                        }


                                    }
                                    #endregion เพิ่มแนบไฟล์ Passport   
                                }
                                else if (module_name == "sendmail_to_been_completed")
                                {
                                    //022_OB/LB/OT/LT : Travel Insurance Certificate has been completed - [Title_Name of traveler]
                                    s_subject = doc_id + " : Travel Insurance Certificate has been completed - " + s_mail_to_emp_name;
                                    //Attachment : Travel Insurance Certificate
                                    sDear = @"Dear " + s_mail_to_emp_name + ",";
                                    sDetail = "A Travel Insurance Certificate has been granted. To view Insurance Coverage or travel details, click ";
                                    sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";

                                    sOtherList = @"Thai Oil Public Company Limited partners with International SOS, the leading medical assistance, international healthcare and security assistance company.";
                                    sOtherList += @"<br>";
                                    sOtherList += @"So if you need a medical referral, lose your medication, seek pre-travel advice or experience a medical or security crisis or";
                                    sOtherList += @"<br>";
                                    sOtherList += @"prepare yourself by browsing through their various medical and security online tools and signing up for our alerts as follow;";
                                    sOtherList += @"<br><br>";
                                    sOtherList = @"<b>";
                                    sOtherList += @"Thai Oil Public Company limited - MEMBERSHIP ID: 03AYCA096535";
                                    sOtherList += @"</b>";
                                    sOtherList += @"<br>";
                                    sOtherList += @"Website : <a href='https://www.internationalsos.com/Members_Home/login/clientaccess.cfm?custno=03AYCA096535'>https://www.internationalsos.com/Members_Home/login/clientaccess.cfm?custno=03AYCA096535</a>";
                                    sOtherList += @"<br>";
                                    sOtherList += @"Application : Download the International SOS Assistance App via iOS App Store, Google Play";
                                    sOtherList += @"<br>";
                                    sOtherList += @"To view more information about International SOS, click ";
                                    sOtherList += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id) + @"/" + "isos" + "'>" + doc_id + "</a>";

                                }
                            }
                        }
                        else if (page_name == "isos")
                        {
                            if (module_name == "sendmail_isos_to_broker")
                            {
                                //024_OB/LB/OT/LT : Please update traveler list of International SOS Record  
                                s_subject = doc_id + " : Please update traveler list of International SOS Record";

                                sDear = @"Dear " + s_mail_to_display + ",";
                                sDetail = "Please update traveler list of International SOS Record as follow ;";

                                #region ISOS New Record
                                sqlstr = @" select distinct a.id as no
                                     , a.emp_id
                                     , case when nvl(b.userid,'') = '' then (nvl(b.entitle, '')|| ' ' || b.enfirstname || ' ' || b.enlastname)  else (nvl(a.isos_emp_title, '')|| ' ' || a.isos_emp_name || ' ' || a.isos_emp_surname) end userdisplay 
                                     , case when nvl(b.userid,'') = '' then nvl(b.orgname, '') else (nvl(a.isos_emp_section, '')|| '/' || a.isos_emp_department || '/' || a.isos_emp_function) end division 
                                    from bz_doc_isos_record a 
                                    left join vw_bz_users b on a.emp_id = b.userid
                                    where a.emp_id in (" + emp_id_select + " ) and  substr(a.year,3,2) = substr('" + doc_id + "',3,2) order by a.id ";
                                DataTable dtisos_record = new DataTable();
                                SetDocService.conn_ExecuteData(ref dtisos_record, sqlstr);
                                if (dtisos_record.Rows.Count > 0)
                                {
                                    sOtherList = @"<table border='1' cellspacing='0' cellpadding='0' style='border-collapse:collapse;margin-left:35.7pt;border-style:none;'>
                                            <tbody><tr height='37' style='height:22.7pt;'>
                                            <td width='135' style='width:81pt;height:22.7pt;background-color:#D5DCE4;padding:0 5.4pt;border:1pt solid #BFBFBF;'>
                                            <span style='background-color:#D5DCE4;'>
                                            <div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'><b>Running </b></span></font><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'><b>No.</b></span></font></span></font></div>
                                            </span></td>
                                            <td width='405' style='width:243pt;height:22.7pt;background-color:#D5DCE4;padding:0 5.4pt;border-width:1pt;border-style:solid solid solid none;border-color:#BFBFBF;'>
                                            <span style='background-color:#D5DCE4;'>
                                            <div style='margin:0 0 0 36pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'><b>Name of Traveler</b></span></font></span></font></div>
                                            </span></td>
                                            <td width='255' valign='top' style='width:153pt;height:22.7pt;background-color:#D5DCE4;padding:0;border-width:1pt;border-style:solid solid solid none;border-color:#BFBFBF;'>
                                            <span style='background-color:#D5DCE4;'>
                                            <div align='center' style='text-align:center;margin:0 0 0 4.5pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'><b>Employee ID</b></span></font></span></font></div>
                                            </span></td>
                                            <td width='255' valign='top' style='width:153pt;height:22.7pt;background-color:#D5DCE4;padding:0;border-width:1pt;border-style:solid solid solid none;border-color:#BFBFBF;'>
                                            <span style='background-color:#D5DCE4;'>
                                            <div style='margin:0 0 0 36pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='black'><span style='font-size:15pt;'><b>Organization Unit</b></span></font></span></font></div>
                                            </span></td>
                                            </tr>";
                                    for (int m = 0; m < dtisos_record.Rows.Count; m++)
                                    {
                                        sOtherList += @" <tr height='4' style='height:2.85pt;'>";
                                        sOtherList += @" <td width='135' style='width:81pt;height:2.85pt;padding:0 5.4pt;border-width:1pt;border-style:none solid solid solid;border-color:#BFBFBF;'>
                                                <div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>
                                                " + dtisos_record.Rows[m]["no"] + "</span></font></span></font></div></td>";//1) //Running No.
                                        sOtherList += @" <td width='405' style='width:243pt;height:2.85pt;padding:0 5.4pt;border-style:none solid solid none;border-right-width:1pt;border-bottom-width:1pt;border-right-color:#BFBFBF;border-bottom-color:#BFBFBF;'>
                                                <div style='margin:0 0 0 36pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>
                                                " + dtisos_record.Rows[m]["userdisplay"] + "</span></font></span></font></div></td>";//Name of Traveler
                                        sOtherList += @" <td width='135' style='width:81pt;height:2.85pt;padding:0 5.4pt;border-width:1pt;border-style:none solid solid solid;border-color:#BFBFBF;'>
                                                <div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>
                                                " + dtisos_record.Rows[m]["emp_id"] + "</span></font></span></font></div></td>";//Employee ID 
                                        sOtherList += @" <td width='135' style='width:81pt;height:2.85pt;padding:0 5.4pt;border-width:1pt;border-style:none solid solid solid;border-color:#BFBFBF;'>
                                                <div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>
                                                " + dtisos_record.Rows[m]["division"] + "</span></font></span></font></div></td>";//Organization Unit
                                        sOtherList += @" </tr>";
                                    }
                                    sOtherList += "</body>";
                                    sOtherList += "</table>";
                                }
                                #endregion ISOS New Record
                            }
                        }
                        else if (page_name == "transportation")
                        {
                            //#E-MAIL : 027_OB/LB/OT/LT : Private Car Requisition  - [Title_Name of traveler]
                            s_subject = doc_id + " : Private Car Requisition - " + s_mail_to_emp_name;

                            sDear = @"Dear " + s_mail_to_emp_name + ",";

                            sDetail = @"As you requested to use a Private Car to business travel, Please complete as follows step;";
                            sDetail += @"To view travel details, click";
                            sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";

                            sOtherList = @"<div style='margin:0 0 0 36pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'>";
                            sOtherList += @"<span style='font-size:15pt;'>Traveler Name : " + drempcheck[0].userDisplay + "</span>";
                            sOtherList += @"<span style='font-size:15pt;margin:0 0 0 36pt;'>" + drempcheck[0].emp_id + "</span>";
                            sOtherList += @"<span style='font-size:15pt;margin:0 0 0 36pt;'>" + drempcheck[0].division + "</span>";
                            sOtherList += @"</font></span></font></div>";
                            sOtherList += @"<br>";
                            sOtherList += @"<div style='margin:0 0 0 36pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'>
                                        <span style='font-size:15pt;'>The steps as follows in requesting a private car for business travel; </span></font></span></font></div>";
                            sOtherList += @"<div style='text-indent:-18pt;margin:0 0 0 58.5pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>1)</span></font><font face='Browallia New,sans-serif'><span style='font-size:;'>&nbsp;&nbsp;&nbsp;&nbsp;
                                        </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'><u>Complete a Personal car application form</u></span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'> and seeking </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'><u>approval
                                        from Vice President</u></span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'> with signature before travel date with </span></font></span></font></div>";
                            sOtherList += @"<div style='text-indent:-18pt;margin:0 0 0 58.5pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>2)</span></font><font face='Browallia New,sans-serif'><span style='font-size:;'>&nbsp;&nbsp;&nbsp;&nbsp;
                                        </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>Attached (1) a copy of </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'><u>(insurance) policy or the motor insurance schedule</u></span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>
                                        </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;' lang='th'>สำเนากรมธรรม์ประกันภัยชั้น </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>1 </span></font></span></font></div>";
                            sOtherList += @"<div style='margin:0 0 0 58.5pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>and (2) a </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'><u>copy
                                        of Employee or Spouse of Car Registration</u></span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'> (</span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;' lang='th'>สำเนาทะเบียนรถ ชื่อผู้เข้าอบรมหรือคู่สมรส</span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>)
                                        (2)</span></font></span></font></div>";
                            sOtherList += @"<div style='text-indent:-18pt;margin:0 0 0 58.5pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>3)</span></font><font face='Browallia New,sans-serif'><span style='font-size:;'>&nbsp;&nbsp;&nbsp;&nbsp;
                                        </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'><u>Process to reimbursement via I-Petty</u></span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'> cash with attached documents 1)
                                        &amp; 2)</span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;' lang='th'> </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>with follow procedures; </span></font></span></font></div>
                                        <div style='text-indent:-18pt;margin:0 0 0 76.5pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Wingdings' size='4'><span style='font-size:15pt;'>§</span></font><font face='Wingdings'><span style='font-size:;'>&nbsp; </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>7.8
                                        THB/Kilometers for total distance or</span></font></span></font></div>
                                        <div style='text-indent:-18pt;margin:0 0 0 76.5pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Wingdings' size='4'><span style='font-size:15pt;'>§</span></font><font face='Wingdings'><span style='font-size:;'>&nbsp; </span></font><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>1,950
                                        THB pay for Roundtrip to Bangkok Metropolis and Vicinity.</span></font></span></font></div>";
                            sOtherList += @"";
                        }
                        else if (page_name == "travelexpense")
                        {
                            if (module_name == "sendmail_to_sap")
                            {
                                //029_OB/LB : Business Travel Expenses has been updated and sent to SAP
                                s_subject = doc_id + " : Business Travel Expenses has been updated and sent to SAP.";

                                sDear = @"Dear All,";
                                sDetail = "Business Travel Expenses has been updated and sent to SAP. To view details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                            else if (module_name == "tripcancelled")
                            {
                                //030_OB/LB : The request for business travel has been cancelled
                                s_subject = doc_id + " : The request for business travel has been cancelled.";
                                //Attached : Approval / Output form

                                sDear = @"Dear All,";
                                sDetail = "The request for business travel has been cancelled. To view the approval details, click ";
                                sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id).Replace("/travelerhistory", "") + "/" + page_name + "'>" + doc_id + "</a>";
                            }
                        }
                        else if (page_name == "feedback")
                        {
                            //031_OB/LB/OT/LT : Please complete Business Travel Feedback 
                            s_subject = doc_id + " : Please complete Business/Training Travel Feedback.";
                            //Attached : Approval / Output form

                            sDear = @"Dear " + s_mail_to_emp_name + ",";

                            sDetail = @"Your experience is important to us, please complete this feedback survey, click ";
                            sDetail += @"<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id) + @"/" + page_name + "'>" + doc_id + "</a>";
                            sDetail += @"<br>";
                            sDetail += @"All surveys are confidential and are used for improving our service only.";
                        }
                        #endregion ข้อมูลที่ต้องส่งใน mail ของแต่ละ module 

                        #region set detail mail attachments
                        if ((page_name == "airticket" && module_name == "admin_confirmed")
                             || (page_name == "accommodation" && module_name == "admin_confirmed")
                             || (page_name == "travelinsurance" && module_name == "sendmail_to_been_completed")
                             || page_name == "allowance" || page_name == "reimbursement"
                             || page_name == "airticket" || page_name == "accommodation"
                             || page_name == "transportation")
                        {
                            //e-mail ต้องส่งไฟล์แนบกับรายละเอียด ของ  Allowance/Reimbursement และเอกสาร approved 
                            // แนบไฟล์ excel โดยไฟล์จะถูก genareate จาก ws Ebiz_App   
                            //d:\Ebiz2\EBiz_Webservice\
                            // string _FilePathServerWebservice = top.ebiz.helper.AppEnvironment.GeteServerPathAPI();
                            //string _FilePathServerWebservice = top.ebiz.helper.AppEnvironment.GeteServerFolder();
                            // ดึงค่าเส้นทางจาก config
                            string folderPath = top.ebiz.helper.AppEnvironment.GeteServerFolder();

                            // ทำให้แน่ใจว่ามี \ ปิดท้าย
                            string _FilePathServerWebservice = EnsureTrailingBackslash(folderPath);
                            string _ServerPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI().ToString();


                            msg_log += "Send Mail Page : end Configuration";
                            var xerrorfile = "";
                            try
                            {
                                string xfile = mail_list[i].mail_attachments.ToString();
                                if (xfile != "")
                                {
                                    s_mail_attachments = "";
                                    string[] xsplit_file = xfile.Split(';');
                                    for (int k = 0; k < xsplit_file.Length; k++)
                                    {
                                        string xname = "";
                                        string xpath = xsplit_file[k].ToString();
                                        if (xpath == "") { continue; }
                                        string[] xsplit_path = xpath.Split('/');
                                        if (xsplit_path.Length > 4)
                                        {
                                            //"file_report": "http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/ExportFile/OB20090026/allowance/00001109/Allowance_Payment_Test.xlsx",
                                            //"file_travel_report": "http://tbkc-dapps-05.thaioil.localnet/Ebiz2/temp/EBIZ_TRAVEL_REPORT_202106151443.xlsx",


                                            //<add key = "FilePathServerWebservice" value = "d:\Ebiz2\EBiz_Webservice\" />
                                            //<add key = "ServerPathAPI" value = "d:\Ebiz2\Ebiz_App\" />
                                            //มีแค่ 2 ไฟล์ fix ได้
                                            if (page_name == "allowance" || page_name == "reimbursement")
                                            {
                                                xname = _FilePathServerWebservice;
                                                //if (k == 0)
                                                //{
                                                //    xname = _FilePathServerWebservice;
                                                //}
                                                //else if (k == 1)
                                                //{
                                                //    xname = _ServerPathAPI;
                                                //}
                                                //else { xname = _FilePathServerWebservice; }
                                            }
                                            else { xname = _FilePathServerWebservice; }
                                            for (int m = 4; m < xsplit_path.Length; m++)
                                            {
                                                if (m > 4) { xname += @"\"; }
                                                xname += xsplit_path[m].ToString();
                                            }
                                            //ตรวจสอบ file บน server
                                            if (File.Exists(xname))
                                            {
                                                if (s_mail_attachments != "") { s_mail_attachments += "|"; }
                                                s_mail_attachments += xname;
                                            }
                                            else
                                            {
                                                //กรณีหลุดจากมีแค่ 2 ไฟล์ fix ได้
                                                xname = _FilePathServerWebservice;
                                                for (int m = 4; m < xsplit_path.Length; m++)
                                                {
                                                    if (m > 4) { xname += @"\"; }
                                                    xname += xsplit_path[m].ToString();
                                                }
                                                //ตรวจสอบ file บน server
                                                if (File.Exists(xname))
                                                {
                                                    if (s_mail_attachments != "") { s_mail_attachments += "|"; }
                                                    s_mail_attachments += xname;
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            catch (Exception exfile) { xerrorfile = exfile.Message.ToString() + "-->irow :" + i + "-->file :" + s_mail_attachments; }

                            msg_log += "Send Mail Page : mail_attachments " + xerrorfile;
                        }

                        else if (page_name == "visa" && module_name == "sendmail_visa_employee_letter")
                        {
                            try
                            {
                                //string xfile = mail_list[i].mail_attachments.ToString();
                                //s_mail_attachments = "";
                                //string[] xsplit_file = xfile.Split(';');
                                //for (int k = 0; k < xsplit_file.Length; k++)
                                //{
                                //    string xname = xsplit_file[k].ToString();
                                //    //ตรวจสอบ file บน server
                                //    if (File.Exists(xname))
                                //    {
                                //        if (s_mail_attachments != "") { s_mail_attachments += "|"; }
                                //        s_mail_attachments += xname;
                                //    }
                                //}
                            }
                            catch { }
                        }
                        #endregion set detail mail attachments

                        #region set detail mail 2
                        sTitle = drempcheck[0].travel_topic + "";
                        sBusinessDate = drempcheck[0].business_date + "";
                        sLocation = drempcheck[0].country_city + "";


                        //Traveler List
                        if (page_name == "isos" || page_name == "transportation") { }
                        else
                        {
                            iNo = 1;
                            sTravelerList = "<table style='width: auto; border-collapse: collapse; font-family: Aptos, Arial, sans-serif; font-size: 15px; text-align: left;'>";

                            // หัวตาราง
                            sTravelerList += @"
    <thead>
        <tr style='background-color: #A7D0F0; '>
            <th style='padding: 8px; border: 1px solid #ccc;'>No.</th>
            <th style='padding: 8px; border: 1px solid #ccc;'>Name</th>
            <th style='padding: 8px; border: 1px solid #ccc;'>Employee ID</th>
            <th style='padding: 8px; border: 1px solid #ccc;'>Division</th>
        </tr>
    </thead>
    <tbody>
";

                            foreach (var item in drempcheck)
                            {
                                sTravelerList += "    <tr>";
                                sTravelerList += $"        <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                sTravelerList += $"        <td style='padding: 8px; border: 1px solid #ccc;'>{item.userDisplay}</td>";
                                sTravelerList += $"        <td style='padding: 8px; border: 1px solid #ccc;'>{item.emp_id}</td>";
                                sTravelerList += $"        <td style='padding: 8px; border: 1px solid #ccc;'>{item.division}</td>";
                                sTravelerList += "    </tr>";
                                iNo++;
                            }

                            sTravelerList += "    </tbody></table>";

                        }

                        #endregion set detail mail 2

                        //#region set mail 
                        //s_mail_body = @"<span lang='en-US'>";
                        //s_mail_body += "<div style='font-family: Aptos, sans-serif; font-size: 16px; line-height: 1.5;'>";
                        //s_mail_body += "     <div style='margin:0;'><span style='font-weight: bold;'>";
                        //s_mail_body += "     " + sDear + "</span></div>";
                        //s_mail_body += "     <br>";
                        //s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //s_mail_body += "     " + sDetail + "</div>";
                        //s_mail_body += "     <br>";
                        //s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //s_mail_body += "     " + sTitle + "</div>";
                        //s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //s_mail_body += "     " + sBusinessDate + "</div>";
                        //s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //s_mail_body += "     " + sLocation + "</div>";
                        //s_mail_body += "     <br>";

                        //if (sTravelerList != "")
                        //{
                        //    s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //    s_mail_body += "     <span style='font-weight: bold;'>Traveler Name :</span> " + sTravelerList + "</div>";
                        //    s_mail_body += "     <br>";
                        //}

                        //if (sOtherList != "")
                        //{
                        //    if (page_name == "isos" || (page_name == "visa" && module_name == "sendmail_visa_requisition"))
                        //    {
                        //        s_mail_body += "     <div style='margin:0 0 0 0;'>";
                        //    }
                        //    else
                        //    {
                        //        s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //    }
                        //    s_mail_body += "     " + sOtherList + "</div>";
                        //    s_mail_body += "     <br>";
                        //}

                        //if (module_name == "tripcancelled")
                        //{
                        //    if (s_mail_body_in_form != "")
                        //    {
                        //        s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //        s_mail_body += "     " + s_mail_body_in_form + "</div>";
                        //        s_mail_body += "     <br>";
                        //    }
                        //}

                        //s_mail_body += "     <div style='margin:0 0 0 36pt;'>";
                        //s_mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                        //s_mail_body += "     <br>";
                        //s_mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                        //s_mail_body += "     </div>";

                        //s_mail_body += "     <div style='margin:0;'>";
                        //s_mail_body += "     <br>";
                        //s_mail_body += "     Best Regards,";
                        //s_mail_body += "     <br>";
                        //s_mail_body += "     Business Travel Services Team (PMSV)";
                        //s_mail_body += "     </div>";

                        //s_mail_body += "</div>";
                        //s_mail_body += "</span>";
                        //#endregion set mail

                        #region set mail
                        s_mail_body = @"<table cellpadding='0' cellspacing='0' width='100%' style='font-family: Aptos, Arial, sans-serif; font-size: 15px; color: #333; text-align: left;'>";

                        // Header
                        s_mail_body += @"<tr><td style='padding: 0 0 10px 0; font-weight: bold;' align='left'>" + sDear + @"</td></tr>";
                        s_mail_body += @"<tr><td style='padding: 0 0 10px 0;' align='left'>" + sDetail + @"</td></tr>";

                        // Info Box
                        s_mail_body += @"<tr><td align='left'>
    <table width='60%' cellpadding='5' cellspacing='0' style='background-color: #F3DDFF; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ddd; text-align: left;'>
        <tr><td><strong>Title:</strong> " + sTitle + @"</td></tr>
        <tr><td><strong>Business Date:</strong> " + sBusinessDate + @"</td></tr>
        <tr><td><strong>Location:</strong> " + sLocation + @"</td></tr>
    </table>
</td></tr>";

                        // Traveler List
                        if (!string.IsNullOrEmpty(sTravelerList))
                        {
                            s_mail_body += @"<tr><td style='padding: 10px 0 10px 0;' align='left'><strong>Traveler Name:</strong><br/>" + sTravelerList + @"</td></tr>";
                        }

                        // Other Info
                        if (!string.IsNullOrEmpty(sOtherList))
                        {
                            s_mail_body += @"<tr><td style='padding: 0 0 10px 0;' align='left'>" + sOtherList + @"</td></tr>";
                        }

                        // Trip Cancelled Form
                        if (module_name == "tripcancelled" && !string.IsNullOrEmpty(s_mail_body_in_form))
                        {
                            s_mail_body += @"<tr><td style='padding: 0 0 10px 0;' align='left'>" + s_mail_body_in_form + @"</td></tr>";
                        }

                        // Footer
                        s_mail_body += @"<tr><td style='padding: 10px 0 10px 0;' align='left'>
    If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + @").<br/>
    For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + @").
</td></tr>";

                        s_mail_body += @"<tr><td style='padding: 10px 0 0 0;' align='left'>
    Best Regards,<br/>
    <strong>Business Travel Services Team (PMSV)</strong>
</td></tr>";

                        s_mail_body += @"</table>";
                        #endregion



                        msg_log += "Send Mail Page :esw.send_mail";
                        SendEmailServiceTravelerProfile esw = new SendEmailServiceTravelerProfile();
                        ret = esw.send_mail(doc_id, page_name, s_mail_to, s_mail_cc, s_subject, s_mail_body, s_mail_attachments, s_mail_show_case).GetAwaiter().GetResult();

                        msg_log += "Send Mail Page : ret : " + ret;

                        if (ret == "")
                        {
                            mail_list[i].mail_status = "true";
                            mail_list[i].mail_remark = "Send mail Success.";
                            mail_list[i].mail_remark += msg_log;
                            ret = "true";
                        }
                        else
                        {
                            mail_list[i].mail_status = "false";
                            mail_list[i].mail_remark = "Send mail Error." + "to:" + s_mail_to + " cc:" + s_mail_cc;
                            mail_list[i].mail_remark += msg_log;
                            ret = "false";
                        }

                    }
                }
                catch (Exception ex) { ret += "Send Mail Page error Message:" + ex.Message.ToString(); }

            }
            catch (Exception ex_mail) { ret = ex_mail.Message.ToString(); }

            return ret;
        }
        public static string EnsureTrailingBackslash(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var fullPath = FileUtil.GetDirectoryInfo(path)?.FullName ?? "";// Path.GetFullPath(path).TrimEnd('\\');

            // ตรวจสอบว่าใน path มีคำว่า "wwwroot" หรือไม่ (ไม่สนใจตัวพิมพ์เล็ก-ใหญ่)
            bool hasWwwRoot = fullPath.IndexOf("wwwroot", StringComparison.OrdinalIgnoreCase) >= 0;

            // ถ้าไม่มี wwwroot ให้เพิ่มเข้าไป
            if (!hasWwwRoot)
            {
                fullPath = Path.Combine(fullPath, "wwwroot");
            }

            return fullPath + "\\";
        }

        public string SendMailInContact(ref List<mailselectList> mail_list)
        {

            string s_mail_to = (mail_list[0].mail_to + "").ToString();
            string s_mail_cc = (mail_list[0].mail_cc + "").ToString();
            string s_subject = "";
            string s_mail_body = "";
            string s_mail_body_in_form = (mail_list[0].mail_body_in_form + "").ToString();
            string s_mail_attachments = "";
            string s_mail_to_emp_name = "All,";
            string module = mail_list[0].module;

            s_subject = "E-Biz : Test Send E-Mail Contact As";
            s_mail_body = @"Dear " + s_mail_to_emp_name;
            s_mail_body += @"<br><br>";
            s_mail_body += s_mail_body_in_form;
            s_mail_body += @"<br><br>";
            s_mail_body += @"<br><br>Regards, 
                                <br>
                                <br>System Administration Officer 
                                <br><br>Tel : 038-359000  Ext 20104";

            SendEmailServiceTravelerProfile esw = new SendEmailServiceTravelerProfile();
            ret = esw.send_mail("TEST", "SendMailInContact", s_mail_to, s_mail_cc, s_subject, s_mail_body, s_mail_attachments).GetAwaiter().GetResult();
            if (ret == "") { ret = "true"; }
            return ret;
        }


        // Change the method `write_log_mail` to be static since it is being called in a static context.  
        //public static void write_log_mail(string step, string data_log)
        //{
        //    try
        //    {
        //        logService.logModel mLog = new logService.logModel();
        //        mLog.module = "E-MAIL";
        //        mLog.tevent = step; // step  
        //        mLog.ref_id = 0;
        //        mLog.data_log = data_log;
        //        logService.insertLog(mLog);
        //    }
        //    catch (Exception ex_write)
        //    {
        //        logService.logModel mLog = new logService.logModel();
        //        mLog.module = "E-MAIL";
        //        mLog.tevent = "write log Send Mail Service error"; // step  
        //        mLog.ref_id = 0;
        //        mLog.data_log = ex_write.Message.ToString();
        //        logService.insertLog(mLog);
        //    }
        //}
        #endregion auwat 20221026 1435 เพิ่มเก็บ log การส่ง mail => เนื่องจากมีกรณที่กดปุ่มแล้ว mail ไม่ไป

        #region Insert Details E-Mail
        public static string SendMailFlowTrip(sendEmailModel value, bool resend = true)
        {

            String s_mail_to = value.mail_to ?? "";
            String s_mail_cc = value.mail_cc ?? "";
            String s_subject = value.mail_subject ?? "";
            String s_mail_body = value.mail_body ?? "";
            string s_mail_attachments = value.mail_attachments ?? "";
            string s_mail_show_case = value.mail_show_case ?? "";

            String doc_id = value.doc_id ?? "";
            String step_flow = value.step_flow ?? "";

            try
            {
                top.ebiz.service.Service.Traveler_Profile.SendEmailServiceTravelerProfile clsmail = new Traveler_Profile.SendEmailServiceTravelerProfile();
                return clsmail.send_mail(doc_id, step_flow, s_mail_to, s_mail_cc, s_subject, s_mail_body, s_mail_attachments, s_mail_show_case, resend).GetAwaiter().GetResult();
            }
            catch (Exception exMail) { return exMail.Message.ToString(); }
        }
        public  string SendMail23FlowTrip(sendEmailModel value, bool resend = true)
        {

            String s_mail_to = value.mail_to ?? "";
            String s_mail_cc = value.mail_cc ?? "";
            String s_subject = value.mail_subject ?? "";
            String s_mail_body = value.mail_body ?? "";
            string s_mail_attachments = value.mail_attachments ?? "";
            string s_mail_show_case = value.mail_show_case ?? "";

            String doc_id = value.doc_id ?? "";
            String step_flow = value.step_flow ?? "";

            try
            {
                top.ebiz.service.Service.Traveler_Profile.SendEmailServiceTravelerProfile clsmail = new Traveler_Profile.SendEmailServiceTravelerProfile();
                return clsmail.send_mail(doc_id, step_flow, s_mail_to, s_mail_cc, s_subject, s_mail_body, s_mail_attachments, s_mail_show_case, resend).GetAwaiter().GetResult();
            }
            catch (Exception exMail) { return exMail.Message.ToString(); }
        }

        public ResultModel updateEmailDetail(DocEmailDetailsSearchModel value)
        {
            var data = new ResultModel();
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var token_login = value.token_login;
                    var id_doc = value.id_doc;
                    var id = value.id;

                    //ค้นหาตาม ID ที่ส่งเข้ามา 
                    var item = context.BZ_EMAIL_DETAILS.Where(p => p.ID == id).FirstOrDefaultAsync();
                    if (item != null)
                    {
                        //Function Resend Mail
                        string ret = "";

                        sendEmailModel dataMail = new sendEmailModel();

                        dataMail.mail_from = item.Result?.FROMEMAIL ?? "";
                        dataMail.mail_to = item.Result?.TORECIPIENTS ?? "";
                        dataMail.mail_cc = item.Result?.CCRECIPIENTS ?? "";
                        dataMail.mail_subject = item.Result?.SUBJECT ?? "";
                        dataMail.mail_attachments = item.Result?.ATTACHMENTS ?? "";

                        //ส่งเข้าไป re-send mail และเก็บ log
                        //ret = sendMail(dataMail);
                        String s_mail_to = dataMail.mail_to ?? "";
                        String s_mail_cc = dataMail.mail_cc ?? "";
                        String s_subject = dataMail.mail_subject ?? "";
                        String s_mail_body = dataMail.mail_body ?? "";
                        string s_mail_attachments = dataMail.mail_attachments ?? "";

                        String doc_id = dataMail.doc_id ?? "";
                        String step_flow = dataMail.step_flow ?? "";

                        try
                        {
                            ret = send_mail(doc_id, step_flow, s_mail_to, s_mail_cc, s_subject, s_mail_body, s_mail_attachments, "", false).GetAwaiter().GetResult();
                        }
                        catch (Exception exMail) { ret = exMail.Message.ToString(); }

                        //update รายการที่เลือก โดยให้ activetype = N เพื่อเก็บเป็นรายการเก่า ส่วนรายการใหม่สร้างตอน sendMail
                        item.Result.ACTIVETYPE = "N";

                        if (string.IsNullOrEmpty(ret))
                        {
                            item.Result.STATUSSEND = "true";
                            item.Result.ERRORSEND = "";
                            context.SaveChanges();

                            data.status = "S";
                            data.message = "";
                        }
                        else
                        {
                            item.Result.STATUSSEND = "false";
                            item.Result.ERRORSEND = ret;

                            data.status = "E";
                            data.message = ret;
                        }
                        context.SaveChanges();


                    }
                }
            }
            catch (Exception ex)
            {
                LoggerFile.write(ex);
                data.status = "E";
                data.message = ex.Message.ToString();
            }

            return data;
        }
        public static int insertMailLog(BZ_EMAIL_DETAILS value)
        {
            int iResult = -1;
            try
            {
                // ใช้ EF Core DbContext
                using (var context = new TOPEBizCreateTripEntities())
                {
                    // เปิดการเชื่อมต่อกับฐานข้อมูล
                    using (var connection = context.Database.GetDbConnection())
                    {
                        connection.Open();

                        // สร้าง DbCommand เพื่อเรียก stored procedure
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "BZ_SP_INSERT_MAIL_LOG";
                            cmd.CommandType = CommandType.StoredProcedure;

                            // เพิ่มพารามิเตอร์สำหรับ Stored Procedure    

                            // เพิ่มพารามิเตอร์สำหรับ Stored Procedure 
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_DocId", value?.DOC_ID, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_StepFlow", value?.STEPFLOW, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_FromEmail", value?.FROMEMAIL, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_ToRecipients", value?.TORECIPIENTS, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_BccRecipients", value?.BCCRECIPIENTS, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_CcRecipients", value?.CCRECIPIENTS, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_Subject", value?.SUBJECT, "char"));

                            cmd.Parameters.Add(context.ConvertTypeParameter("p_Body", value?.BODY?.ToString() ?? "", "clob"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_Attachments", value?.ATTACHMENTS?.ToString() ?? "", "clob"));

                            cmd.Parameters.Add(context.ConvertTypeParameter("p_StatusSend", value?.STATUSSEND, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_ErrorSend", value?.ERRORSEND, "char"));
                            cmd.Parameters.Add(context.ConvertTypeParameter("p_ActiveType", "Y", "char"));


                            // เรียก ExecuteNonQuery เพื่อดำเนินการ stored procedure
                            iResult = cmd.ExecuteNonQuery();
                            // บันทึก log เมื่อสำเร็จ
                            //write_log_mail("insertMailLog Success",
                            //    $"Inserted mail log successfully. Rows affected: {iResult}. " +
                            //    $"DocId: {value?.DOC_ID}, Subject: {value?.SUBJECT}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // บันทึก log เมื่อเกิด error
                //write_log_mail("insertMailLog Error",
                //    $"Error inserting mail log. DocId: {value?.DOC_ID}, " +
                //    $"Error: {ex.Message}");

                iResult = -1; // กำหนดค่าเมื่อเกิด error
            }

            return iResult;
        }

        #endregion Insert Details E-Mail
    }
}