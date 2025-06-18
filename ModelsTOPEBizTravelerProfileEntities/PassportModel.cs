
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class PassportModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class PassportOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public bool user_admin { get; set; }
        public bool user_request { get; set; }

        public string remark { get; set; }
        public string pdpa_wording { get; set; }

        public List<passportList>? passport_detail { get; set; } = new List<passportList>();
        public List<EmpListOutModel>? emp_list { get; set; } = new List<EmpListOutModel>();
        public List<ImgList>? img_list { get; set; } = new List<ImgList>();

       [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class passportList
    {
        public string? doc_id { get; set; }
        public string? emp_id { get; set; }
        public string? user_id { get; set; }
        public string? id { get; set; }
        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
        public string? doc_status { get; set; }

        public string? passport_no { get; set; }
        public string? passport_date_issue { get; set; }
        public string? passport_date_expire { get; set; }
        public string? passport_title { get; set; }
        public string? passport_name { get; set; }
        public string? passport_surname { get; set; }
        public string? passport_date_birth { get; set; }
        public Boolean? accept_type { get; set; }
        public string? action_type { get; set; }
        public string? action_change { get; set; }
        public string? default_type { get; set; }
        public string? default_action_change { get; set; }
        public string? sort_by { get; set; }
        public string? ref_id { get; set; }
        public string? sap_status { get; set; }
    }

}