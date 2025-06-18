
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;


namespace top.ebiz.service.Models.Traveler_Profile
{
    public class EmployeeListModel
    {
        public string token_login { get; set; }
        public string filter_value { get; set; } 
    }

    public class EmployeeListOutModel
    { 
        public string token_login { get; set; } 
        public List<emplistModel> emp_list { get; set; } = new List<emplistModel>();


        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    } 

    public class emplistModel
    {
        public string id { get; set; } 
        public string emp_id { get; set; }
        public string username { get; set; }
        public string titlename { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string displayname { get; set; }
        public string idicator { get; set; }
        public string telephone { get; set; }
         
        public string category { get; set; }
        public string sections { get; set; }
        public string department { get; set; }
        public string function { get; set; } 

    } 

}