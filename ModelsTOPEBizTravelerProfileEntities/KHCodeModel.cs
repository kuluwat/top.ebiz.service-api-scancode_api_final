
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;
namespace top.ebiz.service.Models.Traveler_Profile
{
    public class KHCodeModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class KHCodeOutModel
    {
        public string token_login { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean user_request { get; set; }
        public string data_type { get; set; }

        public List<khcodeList> khcode_list { get; set; } = new List<khcodeList>();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();

        public string remark { get; set; }
    }
    public class khcodeList
    {
        //id, emp_id, user_id, oversea_code, local_code, status, data_for_sap
        public string id { get; set; }
        public string emp_id { get; set; }
        public string user_id { get; set; }
        public string oversea_code { get; set; }
        public string local_code { get; set; }
        public string status { get; set; }

        public string data_for_sap { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }

    public class TemplateKHCodeOutModel
    {
        public string token_login { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean user_request { get; set; }
        public string data_type { get; set; }

        //field ตาม front end
        public string fileName { get; set; }
        public string fileSize { get; set; }
        public string lastUploadDate { get; set; }
        public string url { get; set; }
        public string uploadBy { get; set; }

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();

        public string remark { get; set; } 

    }
}