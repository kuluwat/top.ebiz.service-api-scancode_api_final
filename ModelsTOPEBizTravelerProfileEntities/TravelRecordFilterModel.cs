
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class TravelRecordFilterModel
    {
        public string token_login { get; set; }
    }

    public class TravelRecordFilterOutModel
    {
        public string token_login { get; set; }   
        public Boolean user_admin { get; set; }

        public List<MStatusModel> m_traveltype { get; set; } = new List<MStatusModel>();
        public List<MasterNormalModel> m_continent { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel> m_country { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel> m_province { get; set; } = new List<MasterNormalModel>();
        public List<MasterSectionModel> m_section  { get; set; } = new List<MasterSectionModel>();
        public List<emplistModel> emp_list { get; set; } = new List<emplistModel>();

       [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
}