
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class VisaModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class VisaOutModel
    {
        public string token_login { get; set; }
        public string? doc_id { get; set; }
        public string? id { get; set; }
        public Boolean? user_admin { get; set; }
        public Boolean? user_request { get; set; }
        public string? data_type { get; set; }
      

        public string? user_display { get; set; }
        public string? travel_topic { get; set; }
        public string? business_date { get; set; }
        public string? travel_date { get; set; }
        public string? country_city { get; set; }

        public string? remark { get; set; }
        public string? pdpa_wording { get; set; }

        public string? check_action { get; set; }
        public string? visa_nationality_new { get; set; }
        public string? Datefrom { get; set; }
         
        public List<visaList>? visa_detail { get; set; } = new List<visaList>();
        public List<passportList>? passport_list { get; set; } = new List<passportList>();

        public List<ImgList>? img_list { get; set; } = new List<ImgList>(); 
        public List<EmpListOutModel>? emp_list { get; set; } = new List<EmpListOutModel>();
        public List<mailselectList>? mail_list { get; set; } = new List<mailselectList>();

        public List<MasterNormalModel>? m_continent { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel>? m_country { get; set; } = new List<MasterNormalModel>();
        public List<MasterCountryModel>? country_doc { get; set; } = new List<MasterCountryModel>();


       [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class visaList
    {
        public string? doc_id { get; set; }
        public string? emp_id { get; set; }
        public string? id { get; set; }
      
        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
        public string? doc_status { get; set; }

        //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
        public string? visa_active_in_doc { get; set; }

         
        public string? visa_place_issue { get; set; }
        public string? visa_valid_from { get; set; }
        public string? visa_valid_to { get; set; }
        public string? visa_valid_until { get; set; }
        public string? visa_type { get; set; }
        public string? visa_category { get; set; }
        public string? visa_entry { get; set; }
        public string? visa_name { get; set; }
        public string? visa_surname { get; set; }
        public string? visa_date_birth { get; set; }
        public string? visa_nationality { get; set; }
        public string? passport_no { get; set; }
        public string? visa_sex { get; set; }
        public string? visa_authorized_signature { get; set; }
        public string? visa_remark { get; set; }
        public string? visa_card_id { get; set; }
        public string? visa_serial { get; set; }

        public string? default_type { get; set; }
        public string? default_action_change { get; set; }

        public string? action_type { get; set; }
        public string? action_change { get; set; }
    }

}