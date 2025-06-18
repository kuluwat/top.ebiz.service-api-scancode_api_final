
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class AccommodationModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class AccommodationOutModel
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

        public List<accommodationList> accommodation_detail { get; set; } = new List<accommodationList>();
        public List<accommodationbookList> accommodation_booking { get; set; } = new List<accommodationbookList>();

        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();
        public List<ImgList> img_list { get; set; } = new List<ImgList>();

        public List<MStatusModel> m_book_status { get; set; } = new List<MStatusModel>();
        public List<MStatusModel> m_book_type { get; set; } = new List<MStatusModel>();
        public List<MStatusModel> m_room_type { get; set; } = new List<MStatusModel>();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class accommodationbookList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }
        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
        public string doc_status { get; set; }

        public string booking { get; set; }
        public string search { get; set; }
        public string recommend { get; set; }
        public string already_booked { get; set; }
        public string already_booked_other { get; set; }
        public string already_booked_id { get; set; }

        public string additional_request { get; set; }
        public string booking_status { get; set; }
        public string place_name { get; set; }
        public string map_url { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class accommodationList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }
        public string country { get; set; }
        public string hotel_name { get; set; }
        public string check_in { get; set; }
        public string check_out { get; set; }
        public string roomtype { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }

}