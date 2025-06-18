namespace top.ebiz.service.Constants.Configulations;
public class MailSettings
{


    public bool MailSendAndSaveCopy { get; set; }
    public bool MailSMTP443 { get; set; }
    public string MailSMTPServer587 { get; set; } = string.Empty;
    public string MailSMTPServer443 { get; set; } = string.Empty;
    public string MailFrom { get; set; }  = string.Empty;
    public string MailTest { get; set; }  = string.Empty;
    public string MailFont { get; set; }  = string.Empty;
    public string MailFontSize { get; set; } = string.Empty;
    public string MailUser { get; set; }  = string.Empty;
    public string MailPassword { get; set; } = string.Empty;
    public string MailDevPasswordTest { get; set; } = string.Empty;
    public string MailDevTest { get; set; } = string.Empty;
}