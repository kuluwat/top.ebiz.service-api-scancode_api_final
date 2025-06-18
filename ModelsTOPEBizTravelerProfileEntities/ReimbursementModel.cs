
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class ReimbursementModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class ReimbursementOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean user_request { get; set; }
        public string user_display { get; set; }
        public string travel_topic { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string country_city { get; set; }
         
        public List<reimbursementList> reimbursement_main { get; set; } = new List<reimbursementList>();
        public List<reimbursementdetailList> reimbursement_detail { get; set; } = new List<reimbursementdetailList>();
        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();
        public List<mailselectList> mail_list { get; set; } = new List<mailselectList>();
        public List<ImgList> img_list { get; set; } = new List<ImgList>();

        public List<ExchangeRateList> m_exchangerate { get; set; } = new List<ExchangeRateList>();
        public List<CurrencyList> m_currency { get; set; } = new List<CurrencyList>();  

       [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class reimbursementList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }
         
        public string sendmail_to_traveler { get; set; } 
        public string remark { get; set; }

        public string file_travel_report { get; set; }
        public string file_report { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class reimbursementdetailList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string reimbursement_date { get; set; }
        public string details { get; set; }

        public string exchange_rate { get; set; }
        public string currency { get; set; }//unit
        public string as_of { get; set; } //date

        public string total { get; set; }
        public string grand_total { get; set; }
        public string remark { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    } 
}