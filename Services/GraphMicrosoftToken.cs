using System;
using System.Text.Json.Serialization;

namespace top.ebiz.service.Service.AzureAD
{

    public class GraphMicrosoftToken
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }

        public string access_token { get; set; }
        private DateTime? ExpireTime;
        public void setExpireTime(int? expires_in)
        {
            DateTime now = DateTime.UtcNow;
            if (expires_in != null)
            {
                ExpireTime = now.AddSeconds((double)expires_in);
            }
        }
        public bool isExpire()
        {
            if (ExpireTime == null) return true;
            DateTime now = DateTime.UtcNow;
            return now > ExpireTime;
        }



    }

}


