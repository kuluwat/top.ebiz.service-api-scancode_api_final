
using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using System.Net.Mail;
using System.Net;
using top.ebiz.helper;
using Microsoft.AspNetCore.Hosting.Server;
using System.Web.Services.Description;
using top.ebiz.service.Constants.Configulations;
using System.Security;

namespace ebiz.webservice.service.Controllers
{
    public class MailNotiController : ControllerBase
    {
        public MailNotiController()
        {
        }
        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            return redirectionUrl.ToLower().StartsWith("https://");
        }
        public class emailReheckConfigList
        {
            public string? mail_user { get; set; }
            public string? mail_pass { get; set; }
        }

        [ValidateAntiForgeryToken]
        [HttpPost("EmailRecheckExchangeService", Name = "EmailRecheckExchangeService")]
        public IActionResult Post()
        {
            string msg = "";
            try
            {
                //ปิดเพื่อเทสก่อน
                // "epha@thaioilgroup.com";
                //"Esfr78**St"; 
                string emai_test = "";
                String emai_cc_admin = "";

                string MailTo = emai_test;
                string MailCc = emai_test;

                emai_test = "kuluwat@adb-thailand.com";
                emai_test = "zkuluwat@thaioilgroup.com";


                //Mail ที่ส่งได้ตอนนี้เป็น mail ของ Carservice , epha ส่งไม่ได้
                string mail_user = "carservice@thaioilgroup.com";
                string mail_pass = "mV6SE$vp80APdztC#V68MEcTe";

                //mail_user = "epha@thaioilgroup.com";
                //mail_pass = "Esd57@3#aa";

                ExchangeService service = new ExchangeService();
                service.Credentials = new WebCredentials(mail_user, mail_pass);
                //service.Credentials = new WebCredentials("kuluwat@adb-thailand.com", "i8LtZdw:w~9TcV7");
                service.TraceEnabled = true;

                //< environmentVariable name = "SenderEmail" value = "carservice@thaioilgroup.com" />
                //< environmentVariable name = "SenderPassword" value = "mV6SE$vp80APdztC#V68MEcTe" />

                EmailMessage message = new EmailMessage(service);
                service.AutodiscoverUrl(mail_user, RedirectionUrlValidationCallback);
                message.From = new EmailAddress($"Thaioil workflow system [Do not reply][Local]", mail_user);
                message.Subject = "Test";
                message.Body = "Test body";
                message.ToRecipients.Add(emai_test);

                try
                {
                    message.SendAndSaveCopy();
                }
                catch (Exception ex1)
                {
                    message.Send();
                    msg = ex1.ToString();
                }

            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }

            return Ok(msg);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("SendMail587", Name = "SendMail587")]
        public async Task<bool> SendMailX(string smtpClientPort, [FromServices] AppSettings appSettings)
        {
            string _Server = "";
            string _MailFrom = appSettings.mailSettings.MailUser; //"
            string _MailFromPass = appSettings.mailSettings.MailPassword;
            string _emai_admin = appSettings.mailSettings.MailUser;

            string emai_test = appSettings.mailSettings.MailUser;

            string msg_mail = "";
            string MailDisplay = $"Thaioil workflow system [Do not reply][{_Server}]";
            string MailFrom = _MailFrom;
            string MailFromPass = _MailFromPass;
            string MailTo = appSettings.mailSettings.MailUser;
            string MailCC = "";
            string MailSubject = "x2";
            string MailBody = "x2";
            string s_mail_attachments = @$"C:\Users\2bLove\source\repos\top.ebiz\top.ebiz.service\bin\Debug\net8.0\wwwroot\Image\OB25010055\details\455866223_1043795144093465_3346434879178522390_n-20250120143929.jpg";
            try
            {

                string smtpServer = "webmail.thaioilgroup.com";// AppSettings.Value.EmailConfig.SMTPServer;
                int smtpPort = Convert.ToInt32(smtpClientPort); // AppSettings.Value.EmailConfig.Port;
                string senderEmail = MailFrom;
                string senderPassword = _MailFromPass;


                using (SmtpClient smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;
                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress(senderEmail, MailDisplay),
                        Subject = MailSubject,
                        Body = MailBody,
                        IsBodyHtml = true
                    };
                    if (emai_test != "")
                    {

                        var email_to = emai_test.Split(';');

                        for (int i = 0; i < email_to.Length; i++)
                        {
                            if (email_to[i].ToString() != "")
                            {
                                mail.To.Add(email_to[i]);
                            }
                        }
                        MailBody += " Mail to : " + MailTo;
                        MailBody += " Mail cc : " + MailCC;
                    }
                    else
                    {
                        String emai_admin = _emai_admin;

                        var email_to = MailTo.Split(';');

                        for (int i = 0; i < email_to.Length; i++)
                        {
                            if (email_to[i].ToString() != "")
                            {
                                mail.To.Add(email_to[i]);
                            }
                        }

                        var email_cc = MailCC.Split(';');

                        for (int i = 0; i < email_cc.Length; i++)
                        {
                            if (email_cc[i].ToString() != "")
                            {
                                mail.CC.Add(email_cc[i]);
                            }
                        }

                        var email_bcc = emai_admin.Split(';');

                        for (int i = 0; i < email_bcc.Length; i++)
                        {
                            if (email_bcc[i].ToString() != "")
                            {
                                mail.Bcc.Add(email_bcc[i]);
                            }
                        }

                    }

                    mail.Body = MailBody;

                    #region Attachments
                    try
                    {
                        // Attachments
                        if (!string.IsNullOrEmpty(s_mail_attachments))
                        {
                            // แยกไฟล์แนบออกจากตัวแบ่ง '|'
                            string[] attachments = s_mail_attachments.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                            // เพิ่มไฟล์แนบ
                            if (attachments != null && attachments.Length > 0)
                            {
                                foreach (var filePath in attachments)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                                        {
                                            try
                                            {
                                                // เพิ่มไฟล์แนบลงใน MailMessage
                                                mail.Attachments.Add(new System.Net.Mail.Attachment(filePath));
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"Error adding attachment: {filePath}, {ex.Message}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error adding attachment: {filePath}, {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //write log email
                    }
                    #endregion Attachments

                    await smtpClient.SendMailAsync(mail);
                }

                return true;
            }
            catch (Exception ex)
            {

                // throw new CarServicesException(ex.HResult, ex.Message);
                LoggerFile.write(ex);

            }

            return true;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("SendMail587X3", Name = "SendMail587X3")]
        public bool SendMailX3(string smtpClientPort, [FromServices] AppSettings appSettings)
        {
            string _Server = "";
            string _MailFrom = appSettings.mailSettings.MailUser; //"zkuluwat@thaioilgroup.com";
            string _MailFromPass = appSettings.mailSettings.MailPassword;
            string _emai_admin = _MailFrom;

            string emai_test = _MailFrom;

            string msg_mail = "";
            string MailDisplay = $"Thaioil workflow system [Do not reply][{_Server}]";
            string MailFrom = _MailFrom;
            string MailFromPass = _MailFromPass;
            string MailTo = _MailFrom;
            string MailCC = "";
            string MailSubject = "x3";
            string MailBody = "x3";

            string s_mail_attachments = @$"C:\Users\2bLove\source\repos\top.ebiz\top.ebiz.service\bin\Debug\net8.0\wwwroot\Image\OB25010055\details\455866223_1043795144093465_3346434879178522390_n-20250120143929.jpg";

            try
            {

                string smtpServer = "webmail.thaioilgroup.com";// AppSettings.Value.EmailConfig.SMTPServer;
                int smtpPort = Convert.ToInt32(smtpClientPort); // AppSettings.Value.EmailConfig.Port;
                string senderEmail = MailFrom;
                string senderPassword = _MailFromPass;


                using (SmtpClient smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;
                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress(senderEmail, MailDisplay),
                        Subject = MailSubject,
                        Body = MailBody,
                        IsBodyHtml = true
                    };
                    if (emai_test != "")
                    {

                        var email_to = emai_test.Split(';');

                        for (int i = 0; i < email_to.Length; i++)
                        {
                            if (email_to[i].ToString() != "")
                            {
                                mail.To.Add(email_to[i]);
                            }
                        }
                        MailBody += " Mail to : " + MailTo;
                        MailBody += " Mail cc : " + MailCC;
                    }
                    else
                    {
                        String emai_admin = _emai_admin;

                        var email_to = MailTo.Split(';');

                        for (int i = 0; i < email_to.Length; i++)
                        {
                            if (email_to[i].ToString() != "")
                            {
                                mail.To.Add(email_to[i]);
                            }
                        }

                        var email_cc = MailCC.Split(';');

                        for (int i = 0; i < email_cc.Length; i++)
                        {
                            if (email_cc[i].ToString() != "")
                            {
                                mail.CC.Add(email_cc[i]);
                            }
                        }

                        var email_bcc = emai_admin.Split(';');

                        for (int i = 0; i < email_bcc.Length; i++)
                        {
                            if (email_bcc[i].ToString() != "")
                            {
                                mail.Bcc.Add(email_bcc[i]);
                            }
                        }

                    }

                    mail.Body = MailBody;

                    #region Attachments
                    try
                    {
                        // Attachments
                        if (!string.IsNullOrEmpty(s_mail_attachments))
                        {
                            // แยกไฟล์แนบออกจากตัวแบ่ง '|'
                            string[] attachments = s_mail_attachments.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                            // เพิ่มไฟล์แนบ
                            if (attachments != null && attachments.Length > 0)
                            {
                                foreach (var filePath in attachments)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                                        {
                                            try
                                            {
                                                // เพิ่มไฟล์แนบลงใน MailMessage
                                                mail.Attachments.Add(new System.Net.Mail.Attachment(filePath));
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"Error adding attachment: {filePath}, {ex.Message}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error adding attachment: {filePath}, {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //write log email
                    }
                    #endregion Attachments

                    smtpClient.SendMailAsync(mail);
                }

                return true;
            }
            catch (Exception ex)
            {

                // throw new CarServicesException(ex.HResult, ex.Message);
                LoggerFile.write(ex);

            }

            return true;
        }


        [ValidateAntiForgeryToken]
        [HttpPost("SendMailRecheckSmtp", Name = "SendMailRecheckSmtp")]
        public async Task<IActionResult> SendMailRecheckSmtp([FromBody] emailReheckConfigList value)
        {
            // เรียกใช้งานฟังก์ชัน SendMailX
            bool result = await SendMailX(value.mail_user, value.mail_pass);

            if (result)
            {
                return Ok("Email sent successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to send email.");
            }
        }

        public async Task<bool> SendMailX(string _MailFrom, string _MailFromPass)
        {
            string _Server = "Local";
            string emai_test = ""; // ทดสอบ หากไม่ใช้ อาจปล่อยว่าง
            string msg_mail = "";
            string MailTo = _MailFrom;
            string MailCC = "";
            string MailDisplay = $"Thaioil workflow system [Do not reply][{_Server}]";
            string MailSubject = $"x{_MailFrom}";
            string MailBody = $"MailBody {_MailFrom}, {_MailFromPass}";
            try
            {
                // กำหนดค่าของ SMTP Server
                string smtpServer = "smtp.thaioilgroup.com";
                int smtpPort = 443; // ตามที่คุณต้องการใช้
                string senderEmail = _MailFrom;
                string senderPassword = _MailFromPass;

                using (SmtpClient smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort; // ใช้ Port 443
                    SecureString securePassword = new SecureString();
                    foreach (char c in senderPassword)
                    {
                        securePassword.AppendChar(c);
                    }
                    securePassword.MakeReadOnly();
                    smtpClient.Credentials = new NetworkCredential(senderEmail, securePassword);
                    smtpClient.EnableSsl = true; // อาจต้องตรวจสอบว่า Server ต้องการ SSL หรือไม่

                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress(senderEmail, MailDisplay),
                        Subject = MailSubject,
                        Body = MailBody,
                        IsBodyHtml = true
                    };

                    // เพิ่มผู้รับ (To)
                    if (!string.IsNullOrEmpty(MailTo))
                    {
                        var email_to = MailTo.Split(';');
                        foreach (var email in email_to)
                        {
                            if (!string.IsNullOrEmpty(email))
                                mail.To.Add(email.Trim());
                        }
                    }

                    // เพิ่มผู้รับสำเนา (CC)
                    if (!string.IsNullOrEmpty(MailCC))
                    {
                        var email_cc = MailCC.Split(';');
                        foreach (var email in email_cc)
                        {
                            if (!string.IsNullOrEmpty(email))
                                mail.CC.Add(email.Trim());
                        }
                    }

                    // ส่งอีเมล
                    await smtpClient.SendMailAsync(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                // บันทึก Log
                LoggerFile.write(ex);
                return false; // ส่งอีเมลไม่สำเร็จ
            }
        }
        // public bool SendMailX2(string _MailFrom, string _MailFromPass)
        // {
        //     string _Server = "Local";
        //     string emai_test = ""; // ทดสอบ หากไม่ใช้ อาจปล่อยว่าง
        //     string msg_mail = "";
        //     string MailTo = _MailFrom;
        //     string MailCC = "";
        //     string MailDisplay = $"Thaioil workflow system [Do not reply][{_Server}]";
        //     string MailSubject = $"x{_MailFrom}";
        //     string MailBody = $"MailBody {_MailFrom}, {_MailFromPass}";
        //     try
        //     {
        //         // กำหนดค่าของ SMTP Server
        //         string smtpServer = "smtp.thaioilgroup.com";
        //         int smtpPort = 443; // ตามที่คุณต้องการใช้
        //         string senderEmail = _MailFrom;
        //         string senderPassword = _MailFromPass;

        //         using (SmtpClient smtpClient = new SmtpClient(smtpServer))
        //         {
        //             smtpClient.Port = smtpPort; // ใช้ Port 443
        //             smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
        //             smtpClient.EnableSsl = true; // อาจต้องตรวจสอบว่า Server ต้องการ SSL หรือไม่

        //             MailMessage mail = new MailMessage
        //             {
        //                 From = new MailAddress(senderEmail, MailDisplay),
        //                 Subject = MailSubject,
        //                 Body = MailBody,
        //                 IsBodyHtml = true
        //             };

        //             // เพิ่มผู้รับ (To)
        //             if (!string.IsNullOrEmpty(MailTo))
        //             {
        //                 var email_to = MailTo.Split(';');
        //                 foreach (var email in email_to)
        //                 {
        //                     if (!string.IsNullOrEmpty(email))
        //                         mail.To.Add(email.Trim());
        //                 }
        //             }

        //             // เพิ่มผู้รับสำเนา (CC)
        //             if (!string.IsNullOrEmpty(MailCC))
        //             {
        //                 var email_cc = MailCC.Split(';');
        //                 foreach (var email in email_cc)
        //                 {
        //                     if (!string.IsNullOrEmpty(email))
        //                         mail.CC.Add(email.Trim());
        //                 }
        //             }

        //             // ส่งอีเมล
        //             smtpClient.SendMailAsync(mail);
        //         }

        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         // บันทึก Log
        //         LoggerFile.write(ex);
        //         return false; // ส่งอีเมลไม่สำเร็จ
        //     }
        // }
    }
}
