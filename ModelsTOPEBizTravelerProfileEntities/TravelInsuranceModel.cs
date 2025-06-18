
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class TravelInsuranceModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class TravelInsuranceOutModel
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
         
        public string pdpa_wording { get; set; }
        public string remark { get; set; }

        public List<travelinsuranceList> travelInsurance_detail { get; set; } = new List<travelinsuranceList>();
        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();
        public List<ImgList> img_list { get; set; } = new List<ImgList>();
        public List<ImgList> img_list_cert { get; set; } = new List<ImgList>();
        public List<mailselectList> mail_list { get; set; } = new List<mailselectList>();


        public List<MStatusModel> m_broker { get; set; } = new List<MStatusModel>();
        public List<MStatusModel> m_insurance_plan { get; set; } = new List<MStatusModel>();

       [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class travelinsuranceList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }
        public string doc_status { get; set; }
         
        public string ins_emp_id { get; set; }
        public string ins_emp_name { get; set; }
        public string ins_emp_org { get; set; }
        public string ins_emp_passport { get; set; }
        public string ins_emp_occupation { get; set; }
        public string ins_emp_address { get; set; } 
        public string ins_emp_age { get; set; } 
        public string ins_emp_tel { get; set; }
        public string ins_emp_fax { get; set; }
        public string ins_plan { get; set; }
        public string ins_broker { get; set; }
      
        public string name_beneficiary { get; set; }
        public string relationship { get; set; }
        public string period_ins_dest { get; set; }
        public string period_ins_from { get; set; }
        public string period_ins_to { get; set; }
        public string destination { get; set; }
        public string date_expire { get; set; }
        public string duration { get; set; }
        public string billing_charge { get; set; }

        public string certificates_no { get; set; }
        public string certificates_total { get; set; }
        public string remark { get; set; }

        public string agent_type { get; set; }
        public string broker_type { get; set; }
        public string travel_agent_type { get; set; }
        public string insurance_company { get; set; }

        public string file_outbound_path { get; set; }
        public string file_outbound_name { get; set; }
        public string file_system_path { get; set; }


        public string file_coverage_path { get; set; }
        public string file_coverage_name { get; set; }  

        public string sort_by { get; set; } 

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
}