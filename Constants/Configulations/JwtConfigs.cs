namespace top.ebiz.service.Constants.Configulations;
public class JwtConfigs
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; }

    public void setSecretKey(string SecretKey)
    {
        this.SecretKey = SecretKey;
    }
}