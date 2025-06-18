using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class EstimateExpenseModel
    {

    }

    public class EstExpInputModel
    {
        public string token_login { get; set; }
        public string doc_no { get; set; }
        public string emp_id { get; set; }
        public string test_travel_date { get; set; }
    }

    public class EstExpOutModel
    {
        public string PassportDate { get; set; } = "";
        public string PassportExpense { get; set; } = "";
        public string CLDate { get; set; } = "";
        public string CLExpense { get; set; } = "";
    }

    public class EstExpTravelDateModel
    {
        public DateTime travelDate { get; set; }
    }

    public class EstExpSAPModel
    {
        public string type { get; set; }
        public decimal from_date { get; set; }
        public decimal to_date { get; set; }
        public DateTime? to_date_date { get; set; }
    }

}