using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class RequestTypeModel
    {
        public string token_login { get; set; }

    }

    public class RequestTypeResultModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }

}