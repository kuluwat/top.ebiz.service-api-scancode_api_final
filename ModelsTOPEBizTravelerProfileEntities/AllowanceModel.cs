
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip; 

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class AllowanceModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class AllowanceOutModel
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

        public string gl_account { get; set; }
        public string cost_center { get; set; }
        public string io_wbs { get; set; }

        public List<allowanceList> allowance_main { get; set; } = new List<allowanceList>();
        public List<allowancedetailList> allowance_detail { get; set; } = new List<allowancedetailList>();
        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();
        public List<mailselectList> mail_list { get; set; } = new List<mailselectList>();

        public List<ImgList> img_list { get; set; } = new List<ImgList>();

        public List<MStatusModel> m_allowance_type { get; set; } = new List<MStatusModel>(); 
        public List<ExchangeRateList> m_exchangerate { get; set; } = new List<ExchangeRateList>();
        public List<CurrencyList> m_currency { get; set; } = new List<CurrencyList>();
        public List<emailList> m_empmail_list { get; set; } = new List<emailList>();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class AllowanceMasterOutModel
    {
        public string token_login { get; set; } 

        public List<MStatusModel> m_allowance_type { get; set; } = new List<MStatusModel>();

        public List<ExchangeRateList> m_exchangerate { get; set; } = new List<ExchangeRateList>();
        public List<CurrencyList> m_currency { get; set; } = new List<CurrencyList>();
        public List<emailList> m_empmail_list { get; set; } = new List<emailList>();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class allowanceList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }
        public string doc_status { get; set; }

        public string grand_total { get; set; }
        public string luggage_clothing { get; set; }
        public string remark { get; set; }

        //Auwat 20210705 เพิ่ม file Passport, Passport Valid Until, Luggage Clothing Valid Until
        public string passport { get; set; }
        public string passport_date { get; set; }
        public string luggage_clothing_date { get; set; }


        public string exchange_rate { get; set; }
        public string currency { get; set; }//unit
        public string as_of { get; set; } //date

        public string sendmail_to_traveler { get; set; }
         
        public string file_travel_report { get; set; }
        public string file_report { get; set; }
         
        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class allowancedetailList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string allowance_days { get; set; }
        public string allowance_date { get; set; }
        public string allowance_hrs { get; set; }
        public string allowance_low { get; set; }
        public string allowance_mid { get; set; }
        public string allowance_hight { get; set; }
        public string allowance_total { get; set; }
        public string allowance_unit { get; set; }

        public string allowance_remark { get; set; }
        public string allowance_type_id { get; set; }
        public string allowance_values_def { get; set; }

        public string exchange_rate { get; set; }
        public string currency { get; set; }//unit
        public string as_of { get; set; } //date

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class allowancemailList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string mail_emp_id { get; set; }
        public string mail_to { get; set; }
        public string mail_cc { get; set; }
        public string mail_bcc { get; set; }
        public string mail_status { get; set; }
        public string mail_remark { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
}