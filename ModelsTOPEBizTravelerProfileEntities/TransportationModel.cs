
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class TransportationModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }
     
    public class TransportationOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean user_request { get; set; }
        public string transportation_type { get; set; }
        public string t_car_id { get; set; }
        public string url_personal_car_document { get; set; }
          
        public string cars_doc_no { get; set; }

        public string user_display { get; set; }

        public string html_content { get; set; }
        public string video_url { get; set; }
        public string action_type { get; set; }
        public string action_change { get; set; }

        public List<transportationCarList> transportation_car { get; set; } = new List<transportationCarList>();
        public List<transportationList> transportation_detail { get; set; } = new List<transportationList>();

        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();
        public List<ImgList> img_list { get; set; } = new List<ImgList>(); 

       [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class transportationCarList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string t_car_id { get; set; }

        public string company_name { get; set; }
        public string driver_name { get; set; }
        public string telephone_no { get; set; }
        public string car_model { get; set; }
        public string car_color { get; set; }
        public string car_registration_no { get; set; }
        public string remark { get; set; }
        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class transportationList
    {
        public string doc_id { get; set; }
        public string id { get; set; }

        public string t_car_id { get; set; }

        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_sname { get; set; }
        public string emp_display { get; set; }
        public string emp_tel { get; set; }
        public string travel_date { get; set; }
        public string travel_time { get; set; }
        public string place_from { get; set; }
        public string place_to { get; set; }
        public string place_from_url { get; set; }
        public string place_to_url { get; set; }
        public string place_type { get; set; }
        public string car_type { get; set; }

        public string transport_desc { get; set; }

        public string remark { get; set; }
        public string action_type { get; set; }
        public string action_change { get; set; }
    }
}