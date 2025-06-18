using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class CompanyModel
    {
        public string token_login { get; set; }
    }

    public class CompanyResultModel
    {
        public string com_id { get; set; }
        public string com_name { get; set; }
        public int com_sort_by { get; set; }
    }

}