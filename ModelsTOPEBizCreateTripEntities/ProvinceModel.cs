using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class ProvinceModel
    {
        public string token_login { get; set; }
        public string country_id { get; set; }
    }

    public class ProvinceResultModel
    {
        public string id { get; set; }
        public string province { get; set; }
    }
}