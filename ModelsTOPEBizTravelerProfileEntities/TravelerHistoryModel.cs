using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using top.ebiz.service.Models.Create_Trip;


namespace top.ebiz.service.Models.Traveler_Profile
{
    public class TravelerHistoryModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string type_data { get; set; }
    }

    public class TravelerOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean show_button { get; set; }

        public string travel_topic { get; set; }
        public string travel_topic_sub { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string country_city { get; set; }

        public List<travelerEmpList> traveler_emp { get; set; } = new List<travelerEmpList>();
        public List<travelerVisaList> traveler_visa { get; set; } = new List<travelerVisaList>();

    }
    public class travelerEmpList
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean show_button { get; set; }

        public string age { get; set; }
        public string org_unit { get; set; }
        public string titlename { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string userDisplay { get; set; }
        public string userName { get; set; }
        public string division { get; set; }
        public string idNum { get; set; }
        public string userEmail { get; set; }
        public string userPhone { get; set; }
        public string userTel { get; set; }
        public Boolean isEdit { get; set; }
        public string imgpath { get; set; }
        public string imgprofilename { get; set; }

        public string passportno { get; set; }
        public string dateofissue { get; set; }
        public string dateofbirth { get; set; }
        public string dateofexpire { get; set; }
        public string passport_img { get; set; }
        public string passport_img_name { get; set; }

        public string travel_topic { get; set; }
        public string travel_topic_sub { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string country_city { get; set; }

        public string remark { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }

    public class travelerVisaList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string seq { get; set; }
        public string country { get; set; }
        public string icon { get; set; }
        public string datefrom { get; set; }
        public string dateto { get; set; }
    }
    public class travelerHistoryList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string seq { get; set; }
        public string country { get; set; }
        public string icon { get; set; }
        public string datefrom { get; set; }
        public string dateto { get; set; }


        public string nationality { get; set; }
        public string valid_unit { get; set; }
        public string visa_type { get; set; }
        public string visa_entry { get; set; }
        public string full_path { get; set; }//url visap page
        public string file_name { get; set; } //file name visap page

    }

    public class TravelerHistoryOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean user_request { get; set; }
        public Boolean show_button { get; set; }


        public string travel_topic { get; set; }
        public string travel_topic_sub { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string country_city { get; set; }

        public string pdpa_wording { get; set; }
        public string remark { get; set; }

        public List<travelerEmpList> traveler_emp { get; set; } = new List<travelerEmpList>();
        public List<travelerHistoryList> arrTraveler { get; set; } = new List<travelerHistoryList>();
        [NotMapped]
        public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }


    public class EmpIdCheckModel
    {
        public string? emp_id { get; set; }
        public string? check_type { get; set; }
    } 
    public class EmpListOutModel
    {
        public string? token_login { get; set; }
        public string? doc_id { get; set; }
        public string? emp_id { get; set; }
        public string? id { get; set; }

        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
        public string? doc_status_id { get; set; }
        public string? doc_status_text { get; set; }

        public string? status_trip_cancelled { get; set; }

        public Boolean? user_admin { get; set; }
        public Boolean? show_button { get; set; }

        public string? hiredate { get; set; }
        public string? titlename { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? age { get; set; }
        public string? org_unit { get; set; }

        public string? userDisplay { get; set; }
        public string? userName { get; set; }
        public string? division { get; set; }
        public string? idNum { get; set; }
        public string? userEmail { get; set; }
        public string? userPhone { get; set; }
        public string? userTel { get; set; }

        //userCompany,userPosition,userGender,userJoinDate,dateOfDeparture
        public string? userCompany { get; set; }
        public string? userPosition { get; set; }
        public string? userGender { get; set; }
        public string? userJoinDate { get; set; }
        public string? dateOfDeparture { get; set; }

        public string? sap_from_date { get; set; }
        public string? sap_to_date { get; set; }
        public string? def_location_id { get; set; }

        public Boolean? isEdit { get; set; }
        public string? imgpath { get; set; }
        public string? imgprofilename { get; set; }

        public string? passportno { get; set; }
        public string? dateofissue { get; set; }
        public string? dateofbirth { get; set; }
        public string? dateofexpire { get; set; }
        public string? passport_img { get; set; }
        public string? passport_img_name { get; set; }

        public string? travel_topic { get; set; }
        public string? travel_topic_sub { get; set; }
        public string? business_date { get; set; }
        public string? travel_date { get; set; }
        public string? country_city { get; set; }

        public string? continent_id { get; set; }
        public string? country_id { get; set; }
        //gl_account,cost_center,io_wbs
        public string? gl_account { get; set; }
        public string? cost_center { get; set; }
        public string? io_wbs { get; set; }

        public string? remark { get; set; }

        public string? mail_status { get; set; }
        public string? mail_remark { get; set; }
        public string? type_send_to_broker { get; set; }

        public string? send_to_sap { get; set; }
        public List<travelerEmpList>? traveler_emp { get; set; } = new List<travelerEmpList>();
        public List<travelerHistoryList>? arrTraveler { get; set; } = new List<travelerHistoryList>();

    }

}