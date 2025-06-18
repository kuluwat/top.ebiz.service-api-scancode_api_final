

using System.ComponentModel.DataAnnotations.Schema;

namespace top.ebiz.service.Models.Create_Trip
{
    public class NormalModel
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? text { get; set; }
    }
    public class DocModel
    {
        public string token_login { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public DocBehalf behalf { get; set; }
        public string id_company { get; set; }
        public DocTravelType type_of_travel { get; set; }
        public string topic_of_travel { get; set; }
        public string travel { get; set; }
        public List<idModel> continent { get; set; }
        public List<DocCountry> country { get; set; }
        public List<DocProvince> province { get; set; }
        public string city { get; set; }
        public DateModel business_date { get; set; }
        public DateModel travel_date { get; set; }
        public string travel_objective_expected { get; set; }
        public List<Traveler> summary_table { get; set; }
        public List<DocFileModel>? doc_file_table { get; set; } = new List<DocFileModel>();
        public initiatorModel initiator { get; set; }

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
        public string remark { get; set; }
        public actionModel action { get; set; }

        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string type_flow { get; set; }
    }

    public class idModel
    {
        public string id { get; set; }
    }

    public class actionTypeModel
    {
        public string? type { get; set; }
    }
    public class actionModel
    {
        public string? type { get; set; }
        public string? remark { get; set; }
    }
    public class afterTripModel
    {
        public string? opt1 { get; set; }

        [NotMapped]
        public subAfterTripModel? opt2 { get; set; } = new Models.Create_Trip.subAfterTripModel();
        [NotMapped]
        public subAfterTripModel? opt3 { get; set; } = new Models.Create_Trip.subAfterTripModel();
    }
    public class subAfterTripModel
    {
        public string? status { get; set; }
        public string? remark { get; set; }
    }
    public class initiatorModel
    {
        public string status { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_organization { get; set; }
        public string remark { get; set; }
    }
    public class Traveler
    {
        public string dh_code { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_organization { get; set; }
        public string continent_id { get; set; }
        public string continent { get; set; }
        public string country_id { get; set; }
        public string country_name { get; set; }
        public string province_id { get; set; }
        public string province_name { get; set; }
        public string city { get; set; }
        public DateModel business_date { get; set; }
        public DateModel travel_date { get; set; }
        public string travel_duration { get; set; }
        public string gl_account { get; set; }
        public string cost { get; set; }
        public string order { get; set; }
        public string remark { get; set; }

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
        //0 กับ 3 แก้ไขได้
        public string approve_status { get; set; }
        public string approve_remark { get; set; }
        //DevFix 20210719 0000 เพิ่ม field OPT
        public string approve_opt { get; set; }
        public string remark_opt { get; set; }
        public string remark_cap { get; set; }

        //DevFix 20210817 เพิ่ม key เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
        public string traveler_ref_id { get; set; }

    }

    public class DocFileModel
    {
        public string? doc_id { get; set; }
        public Nullable<decimal> id { get; set; }
        public string? path { get; set; }
        public string? filename { get; set; }
        public string? fullname { get; set; }
        public string? action_type { get; set; }
        public string? pagename { get; set; }
        public string? active_type { get; set; }
        public string? action_change { get; set; }
    }

    public class DateModel
    {
        public string start { get; set; }
        public string stop { get; set; }
    }
    public class DocProvince
    {
        public string province_id { get; set; }
        public string province_name { get; set; }
    }
    public class DocCountry
    {
        public string contry_id { get; set; }
        public string country_id { get; set; }
        public string country_name { get; set; }
        public string continent_id { get; set; }
        public string continent_name { get; set; }
    }

    public class DocBehalf
    {
        public string status { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_organization { get; set; }
    }
    public class DocTravelType
    {
        public string meeting { get; set; }
        public string siteVisite { get; set; }
        public string workshop { get; set; }
        public string roadshow { get; set; }
        public string conference { get; set; }
        public string other { get; set; }
        public string other_detail { get; set; }

        //DevFix 20220805 --> after go-live เพิ่ม Tick box = Training
        public string training { get; set; }
    }

    public class genDocNoModel
    {
        public string token_login { get; set; }
        public string doc_type { get; set; }
    }

}