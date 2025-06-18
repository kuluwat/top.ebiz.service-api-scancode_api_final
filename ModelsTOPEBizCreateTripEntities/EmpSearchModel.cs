using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class EmpSearchModel
    {
        public string token_login { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
    }

    public class EmpSearchResultModel
    {
        public string empId { get; set; }
        public string empName { get; set; }
        public string deptName { get; set; }
        public string gl_account { get; set; }
        public string cost_center { get; set; }
        public string order_wbs { get; set; }

    }
}