
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class ISOSModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class ISOSOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean user_request { get; set; }
        public string doc_status { get; set; }

        public string user_display { get; set; }
        public string travel_topic { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string country_city { get; set; }

        public string html_content { get; set; }
        public string video_url { get; set; }
        public string action_type { get; set; }
        public string action_change { get; set; }

        //zbuddhamonk@thaioilgroup.com;znarumolj@thaioilgroup.com
        public string broker_mail { get; set; }

        public List<MMasterNomalModel> business_type_list { get; set; } = new List<MMasterNomalModel>();
        public List<isosList> isos_detail { get; set; } = new List<isosList>();
        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();
        public List<ImgList> img_list { get; set; } = new List<ImgList>();

        public List<MMasterInsurancebrokerModel> m_broker { get; set; } = new List<MMasterInsurancebrokerModel>();

 
        [NotMapped] 
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class isosList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string isos_type_of_travel { get; set; }
        public string isos_emp_id { get; set; }
        public string isos_emp_title { get; set; }
        public string isos_emp_name { get; set; }
        public string isos_emp_surname { get; set; }
        public string isos_emp_section { get; set; }
        public string isos_emp_department { get; set; }
        public string isos_emp_function { get; set; }

        public string business_type_id { get; set; }

        public string remark { get; set; }

        //ใช้ในตาราง SetISOSRecord 
        public string send_mail_type { get; set; }

        public string insurance_company_id { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }


}