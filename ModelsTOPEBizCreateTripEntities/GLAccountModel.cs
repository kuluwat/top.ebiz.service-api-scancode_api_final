using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class GLAccountModel
    {

    }

    public class GLInputModel
    {
        public string token_login { get; set; }
        public string text { get; set; }
    }

    public class GLOutModel
    {
        public string code { get; set; } = "";
    }



}