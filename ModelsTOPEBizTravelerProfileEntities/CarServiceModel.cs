
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class CarServiceModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class CarServiceOutModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }

        public string doc_id { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }

       [NotMapped] 
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    } 
}