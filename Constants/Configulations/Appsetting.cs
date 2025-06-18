namespace top.ebiz.service.Constants.Configulations;
public class AppSettings
{
    public ConnectionStrings connectionStrings { get; set; } = new ConnectionStrings();
    public MailSettings mailSettings { get; set; } = new MailSettings();
    public JwtConfigs JwtConfigs { get; set; } = new JwtConfigs();
}