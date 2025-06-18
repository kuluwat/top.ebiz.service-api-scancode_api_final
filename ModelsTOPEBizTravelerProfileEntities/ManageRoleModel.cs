 
using System.ComponentModel.DataAnnotations.Schema; 
using top.ebiz.service.Models.Create_Trip;


namespace top.ebiz.service.Models.Traveler_Profile
{
    public class ManageRoleModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class ManageRoleOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public string data_type { get; set; }

        public string user_display { get; set; }
        public string travel_topic { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string country_city { get; set; }

        public List<roleList> admin_list { get; set; } = new List<roleList>();


       [NotMapped] 
        public afterTripModel after_trip { get; set; } = new afterTripModel();

        public List<userNewList> after_add_user { get; set; } = new List<userNewList>();
        public string remark { get; set; }
    }
    public class roleList
    {
        public string id { get; set; }
        public string emp_id { get; set; }

        public string username { get; set; }
        public string displayname { get; set; }
        public string email { get; set; }
        public string idicator { get; set; }

        //role
        public string super_admin { get; set; }
        public string pmsv_admin { get; set; }
        public string pmdv_admin { get; set; }
        public string contact_admin { get; set; }

        public string sort_by { get; set; }
        public string status { get; set; }
        public string action_type { get; set; }
        public string action_change { get; set; }
    }

    public class userNewList
    {
        public string username { get; set; } 
        public string status { get; set; }
        public string remark { get; set; }
    }
}