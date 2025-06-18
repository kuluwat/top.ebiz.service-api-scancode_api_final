
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class AirTicketModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string data_db { get; set; }
    }

    public class AirTicketOutModel
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

        public List<airticketList> airticket_detail { get; set; } = new List<airticketList>();

        public List<airticketbookList> airticket_booking { get; set; } = new List<airticketbookList>();
        public List<ImgList> img_list { get; set; } = new List<ImgList>();
        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();

        public List<MStatusModel> m_book_status { get; set; } = new List<MStatusModel>();
        public List<MStatusModel> m_book_type { get; set; } = new List<MStatusModel>();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class airticketbookList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string id { get; set; }
        public string data_type { get; set; }
        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
        public string doc_status { get; set; }

        public string ask_booking { get; set; }
        public string search_air_ticket { get; set; }
        public string as_company_recommend { get; set; }
        public string already_booked { get; set; }
        public string already_booked_other { get; set; }
        public string already_booked_id { get; set; }

        public string booking_ref { get; set; }
        public string booking_status { get; set; }
        public string additional_request { get; set; }

        public string data_type_allowance { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class airticketList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string id { get; set; }

        public string airticket_date { get; set; }
        public string airticket_route_from { get; set; }
        public string airticket_route_to { get; set; }
        public string airticket_flight { get; set; }

        public string airticket_departure_time { get; set; }
        public string airticket_arrival_time { get; set; }

        public string airticket_departure_date { get; set; }
        public string airticket_arrival_date { get; set; }

        public string check_my_trip { get; set; }
        public string airticket_root { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class ImgList
    {
        public string? doc_id { get; set; }
        public string? emp_id { get; set; }
        public string? id { get; set; }
        public string? id_level_1 { get; set; }
        public string? id_level_2 { get; set; } 
        public string? path { get; set; }
        public string? filename { get; set; }
        public string? fullname { get; set; }
        public string? pagename { get; set; }
        public string? actionname { get; set; }
        public string? status { get; set; }
        public string? active_type { get; set; } 

        public string? modified_date { get; set; }
        public string? modified_by { get; set; }

        public string? action_type { get; set; }
        public string? action_change { get; set; }

        public string? remark { get; set; }
    }
}