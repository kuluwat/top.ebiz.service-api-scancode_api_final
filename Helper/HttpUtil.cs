using System.Text;
using Microsoft.AspNetCore.Mvc;

public class HttpUtil
{
    public static async Task<string> RequestText(HttpRequest req)
    {
        string requestText = null;
        using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
        {
            requestText = await reader.ReadToEndAsync();
        }
        return requestText;
    }


}

