
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Service.Create_Trip;
 
namespace top.ebiz.service.Models.Traveler_Profile
{
    public class TravelExpenseModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class TravelExpenseOutModel
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

        public List<travelexpenseList> travelexpense_main { get; set; } = new List<travelexpenseList>();
        public List<travelexpensedetailList> travelexpense_detail { get; set; } = new List<travelexpensedetailList>();
        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();
        public List<mailselectList> mail_list { get; set; } = new List<mailselectList>();
        public List<ImgList> img_list { get; set; } = new List<ImgList>();

        public List<ExchangeRateList> m_exchangerate { get; set; } = new List<ExchangeRateList>();
        public List<CurrencyList> m_currency { get; set; } = new List<CurrencyList>();
        public List<MStatusModel> dtm_expense_type { get; set; } = new List<MStatusModel>();
        public List<MStatusModel> dtm_status { get; set; } = new List<MStatusModel>();

        [NotMapped] 
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class travelexpenseList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; } 

        public string status_trip_cancelled { get; set; }
        public string send_to_sap { get; set; }
        public string remark { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class travelexpensedetailList
    {
        public string? doc_id { get; set; }
        public string? emp_id { get; set; }
        public string? id { get; set; }

        public string? expense_type { get; set; }
        public string? data_date { get; set; }
        public string? status { get; set; } 

        public string? exchange_rate { get; set; }
        public string? currency { get; set; }//unit
        public string? as_of { get; set; } //date

        public string? total { get; set; }
        public string? grand_total { get; set; }
        public string? remark { get; set; }

        public string? status_active { get; set; }

        public string? action_type { get; set; }
        public string? action_change { get; set; }
        public string? ref_id { get; set; }
        public string? sap_status { get; set; }
    }
}