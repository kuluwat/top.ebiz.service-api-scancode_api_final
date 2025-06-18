using Microsoft.Exchange.WebServices.Data;

using System.Net;
namespace top.ebiz.service.Service
{
    public class ClassMail443
    {
        public String send_mail()
        {
            String msg_mail = "";
            string mail_server = "webmail.thaioilgroup.com";
            string mail_from = "e-biztravel@thaioilgroup.com";
            string mail_user = "e-biztravel";
            string mail_password = "W6shNUY%k@!T3GTJ3BxXkFrSs";
            


            ExchangeService service = new ExchangeService();
            service.Credentials = new WebCredentials(mail_user, mail_password);
            service.TraceEnabled = true;

            try
            {
                // Autodiscover URL สำหรับ Microsoft Exchange Server
                service.AutodiscoverUrl(mail_from, RedirectionUrlValidationCallback);

                // สร้างอีเมล์ใหม่
                EmailMessage email = new EmailMessage(service);
                email.From = new EmailAddress("Mail Display", mail_from);

                // เพิ่มผู้รับ (ในที่นี้ส่งให้ตัวเอง)
                email.ToRecipients.Add("kuluwat@adb-thailand.com");

                // ตั้งค่าหัวข้อและเนื้อหาอีเมล์
                email.Subject = "Test Email";
                email.Body = new MessageBody(BodyType.HTML, "testmail");

                // ส่งอีเมล์
                email.Send();
                msg_mail = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                msg_mail = "Failed to send email: " + ex.ToString();
            }

            return msg_mail;
        }
      

        // เมธอดสำหรับบันทึก log
        private void WriteLog(string step, string message)
        {
            // ตัวอย่างการบันทึก log ลงในไฟล์
            string logFilePath = @"C:\Logs\SendMailLog.txt";
            string logMessage = $"{DateTime.Now}: {step} - {message}";

            try
            {
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to write log: " + ex.ToString());
            }
        }
        public string SendMail_Normal()
        {
            string msg_mail = "";
            string mail_user = "zkuluwat@thaioilgroup.com";
            string mail_from = "zkuluwat@thaioilgroup.com";
            string mail_pass = "Initial012345;";

            // Force TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ExchangeService service = new ExchangeService();
            service.Credentials = new WebCredentials(mail_user, mail_pass);
            service.TraceEnabled = true;

            try
            {
                // Set the Autodiscover URL
                service.AutodiscoverUrl(mail_from, RedirectionUrlValidationCallback);

                string MailDisplay = $"Thaioil e-Business Travel";
                EmailMessage email = new EmailMessage(service);
                email.From = new EmailAddress(MailDisplay, mail_from);
                email.ToRecipients.Add("kuluwat@adb-thailand.com");
                email.Subject = "Test Email";
                email.Body = "This is a test email.";
                email.Send();

                msg_mail = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                msg_mail = "Error: " + ex.Message;

                // Log InnerException if available
                if (ex.InnerException != null)
                {
                    msg_mail += " Inner Exception: " + ex.InnerException.Message;
                }
            }

            return msg_mail;
        }

        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            Uri uri = new Uri(redirectionUrl);
            return uri.Scheme == "https" && uri.Host.Contains("thaioilgroup.com");
        }

    }
}
