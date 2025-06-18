
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class TravelRecordModel
    { 
        public string? token_login { get; set; }
        public string? doc_id { get; set; }
        public string? country { get; set; }
        public string? date_from { get; set; }
        public string? date_to { get; set; }
         
        public string? travel_type { get; set; } 

        public string? emp_id { get; set; } 
        public string? section { get; set; }
        public string? department { get; set; }
        public string? function { get; set; }

        public List<traveltypeList>? travel_list { get; set; } = new List<traveltypeList>();

    }

    public class TravelRecordOutModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public Boolean user_admin { get; set; }

        public List<travelrecordList> travelrecord { get; set; } = new List<travelrecordList>();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();

    }
    public class travelrecordList
    {
        public string? no { get; set; }
        public string? emp_id { get; set; }
        public string? emp_title { get; set; }
        public string? emp_name { get; set; }
        public string? category { get; set; }
        public string? section { get; set; }
        public string? department { get; set; }
        public string? function { get; set; }

        public string? travel_status { get; set; }
        public string? in_house { get; set; }
        public string? travel_topic { get; set; }
        public string? country { get; set; }
        public string? city_province { get; set; }
        public string? date_from { get; set; }
        public string? date_to { get; set; }
        public string? duration { get; set; }
        public string? estimate_expense { get; set; }
        public string? gl_account { get; set; }
        public string? cost_center { get; set; }
        public string? order_wbs { get; set; }

        public string? accommodation { get; set; }
        public string? air_ticket { get; set; }
        public string? allowance_day { get; set; }
        public string? allowance_night { get; set; }
        public string? clothing_luggage { get; set; }
        public string? course_fee { get; set; }
        public string? instruction_fee { get; set; }
        public string? miscellaneous { get; set; }
        public string? passport { get; set; }
        public string? transportation { get; set; }
        public string? visa_fee { get; set; }
        public string? total { get; set; }
        public string? travel_insurance { get; set; }
        public string? doc_id { get; set; }
        public string? countryid { get; set; }
        public string? travel_type { get; set; }
    }
    public class traveltypeList
    {
        public string? id { get; set; } 
    }
}