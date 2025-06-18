using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class CostCenterModel
    {

    }

    public class CCInputModel
    {
        public string token_login { get; set; }
        public string text { get; set; }
    }

    public class CCOutModel
    {
        public string code { get; set; } = "";
    }



}