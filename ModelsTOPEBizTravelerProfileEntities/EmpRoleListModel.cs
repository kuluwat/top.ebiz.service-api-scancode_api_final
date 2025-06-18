
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class EmpRoleListModel
    {
        public string? token_login { get; set; }
        public string? filter_value { get; set; }
    }

    public class EmpRoleListOutModel
    {
        public string token_login { get; set; }
        public List<emprolelistModel> contactus { get; set; } = new List<emprolelistModel>();
        public List<emprolelistModel> emprole_list { get; set; } = new List<emprolelistModel>();


        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }

    public class emprolelistModel
    {
        public string id { get; set; }
        public string emp_id { get; set; }
        public string username { get; set; }

        public string name_th { get; set; }
        public string name_en { get; set; }
        public string tel { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string img { get; set; }
        public string idicator { get; set; }
        public string role_type { get; set; }

    }

}