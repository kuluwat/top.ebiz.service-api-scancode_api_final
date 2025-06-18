
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class PortalModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class OpenDocOutModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; } 
        public List<upcomingplanList> up_coming_plan { get; set; } = new List<upcomingplanList>();
          
        public string action_type { get; set; }
        public string action_change { get; set; }
        [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class PortalOutModel
    {
        public string token_login { get; set; } 
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }

        public List<imgportalList> img_list { get; set; } = new List<imgportalList>();
        public List<upcomingplanList> up_coming_plan { get; set; } = new List<upcomingplanList>();

        public List<practice_areasList> practice_areas { get; set; } = new List<practice_areasList>();

        public string text_title { get; set; }
        public string text_desc { get; set; }

        public string text_contact_title { get; set; }
        public string text_contact_email { get; set; }
        public string text_contact_tel { get; set; }

        //get in touch
        public string text_name { get; set; }
        public string text_subject { get; set; }
        public string text_message { get; set; }
         
        //ระบุ field img ที่ต้องการแก้ไข
        public string action_change_imgname { get; set; }

        //url นโยบายการคุ้มครอง​ข้อมูลส่วนบุคคล (Privacy Policy)
        public string url_employee_privacy_center { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; } 
        [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class upcomingplanList
    {
        //date: "25/04/2021", //25 APR 21
        //topic_of_traveling: "Retest Oversea // ไม่จำกัดวงเงิน Multiple (many country)",
        //country: "China, India",
        //business_date: "01 Aug 2021 - 20 Aug 2021",
        //button_status: "7"

        public string doc_id { get; set; }
        public string date { get; set; }
        public string topic_of_traveling { get; set; }
        public string country { get; set; }
        public string business_date { get; set; }
        public string button_status { get; set; }
         
        public string tab_no { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class imgportalList
    {
        public string img_header { get; set; }  
        public string img_personal_profile { get; set; } 

        public string img_banner_1 { get; set; }
        public string url_banner_1 { get; set; }

        public string img_banner_2 { get; set; }
        public string url_banner_2 { get; set; }

        public string img_banner_3 { get; set; }
        public string url_banner_3 { get; set; }

        public string img_practice_areas { get; set; }

    }
    public class practice_areasList
    {
        public string url_approval { get; set; }
        public string url_employee_payment { get; set; }
        public string url_transportation { get; set; }
        public string url_others { get; set; }
    }
    //1. APPROVAL 
    //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/1%20APPROVAL

    //2. EMPLOYEE PAYMENT
    //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/2%20EMPLOYEE%20PAYMENT

    //3. TRANSPORTATION
    //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/3%20TRANSPORTATION

    //4. OTHERS
    //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/4%20OTHERS

}