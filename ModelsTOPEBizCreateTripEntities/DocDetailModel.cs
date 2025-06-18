using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace top.ebiz.service.Models.Create_Trip
{
    public class DocDetailModel
    {
        public buttonModel button { get; set; } = new buttonModel();
        public string type { get; set; }
        public string document_status { get; set; }
        public string doc_status { get; set; }
        public DocBehalf behalf { get; set; } = new DocBehalf();
        public string id_company { get; set; }
        public string company_name { get; set; }
        public DocTravelType type_of_travel { get; set; } = new DocTravelType();
        public string topic_of_travel { get; set; }
        public string travel { get; set; }
        public List<dataListModel> continent { get; set; } = new List<dataListModel>();
        //public string contient_name { get; set; }
        public List<DocCountry> country { get; set; } = new List<DocCountry>();
        public List<DocProvince> province { get; set; } = new List<DocProvince>();
        public string city { get; set; }
        public DateModel business_date { get; set; } = new DateModel();
        public DateModel travel_date { get; set; } = new DateModel();
        public string travel_objective_expected { get; set; }
        public List<Traveler> summary_table { get; set; } = new List<Traveler>();
        public List<DocFileModel> doc_file_table { get; set; } = new List<DocFileModel>();
        public initiatorModel initiator { get; set; } = new initiatorModel();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
        public string remark { get; set; }
        public actionModel action { get; set; } = new actionModel();


        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string type_flow { get; set; }
        //DevFix 20210622 0000 เพิ่มข้อมูล ประเภทพนักงาน 1:Employee, 2:Contract
        public string user_type { get; set; }
        public string request_user_type { get; set; }

        //DevFix 20210806 0000 เพิ่มข้อมูล requester name
        public string requester_emp_name { get; set; }

    }

    public class dataListModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class buttonModel
    {
        public string part_i { get; set; }
        public string part_ii { get; set; }
        public string part_iii { get; set; }
        public string part_iiii { get; set; }
        public string part_cap { get; set; }
        public string submit { get; set; }
        public string save { get; set; }
        public string cancel { get; set; }
        public string reject { get; set; }
        public string revise { get; set; }
        public string approve { get; set; }
    }

    public class DocDetailSearchModel
    {
        public string token_login { get; set; }
        public string id_doc { get; set; }
    }

    public class ContinentDocModel
    {
        public string DH_CODE { get; set; }
        public decimal CTN_ID { get; set; }
        public string CTN_NAME { get; set; }
    }
    public class CountryDocModel
    {
        public string contry_id { get; set; }
        public string country_name { get; set; }
        public string continent_id { get; set; }
        public string continent_name { get; set; }
    }
    public class ProvinceDocModel
    {
        public string province_id { get; set; }
        public string province_name { get; set; }
    }

    public class StatusDocModel
    {
        public string? dh_code { get; set; }
        public string dh_doc_status { get; set; }
    }

    public class TravelerDocModel
    {
        public string? emp_id { get; set; }
        public string? emp_name { get; set; }
        public string? emp_organization { get; set; }
        public string? continent_id { get; set; }
        public string? continent { get; set; }
        public string? country_id { get; set; }
        public string? country_name { get; set; }
        public string? province_id { get; set; }
        public string? province_name { get; set; }
        public string? city { get; set; }
        public string? business_date_start { get; set; }
        public string? business_date_stop { get; set; }
        public string? travel_date_start { get; set; }
        public string? travel_date_stop { get; set; }
        public string? gl_account { get; set; }
        public string? cost { get; set; }
        public string? order_wbs { get; set; }
        public string? remark { get; set; }

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
        //0 กับ 3 แก้ไขได้
        public string? approve_status { get; set; }
        public string? approve_remark { get; set; }
        public string? remark_opt { get; set; }
        public string? remark_cap { get; set; }

        //DevFix 20210817 เพิ่ม key เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
        public string? traveler_ref_id { get; set; }
        public string? exchange_rate { get; set; }
        public string? exchange_date { get; set; }
        public string? exchange_currency { get; set; }
        public string? allowance { get; set; }
        

    }

    public class DocHeadModel
    {
        public string document_status { get; set; }
        public string DH_CODE { get; set; }
        public decimal? DH_VERSION { get; set; }
        public decimal? DH_DOC_STATUS { get; set; }
        public DateTime? DH_DATE { get; set; }
        public string DH_COM_CODE { get; set; }
        public string DH_TYPE { get; set; }
        public string DH_BEHALF_EMP_ID { get; set; }
        public string ENNAME { get; set; }
        public string COMPANYCODE { get; set; }
        public string COMPANYNAME { get; set; }
        public string DH_TOPIC { get; set; }
        public string DH_TRAVEL { get; set; }
        public DateTime? DH_BUS_FROMDATE { get; set; }
        public string bus_start { get; set; }
        public DateTime? DH_BUS_TODATE { get; set; }
        public string bus_stop { get; set; }
        public DateTime? DH_TRAVEL_FROMDATE { get; set; }
        public string travel_start { get; set; }
        public DateTime? DH_TRAVEL_TODATE { get; set; }
        public string travel_stop { get; set; }
        public decimal? DH_TRAVEL_DAYS { get; set; }
        public string DH_TRAVEL_OBJECT { get; set; }
        public string DH_INITIATOR_EMPID { get; set; }
        public string INITIATOR_NAME { get; set; }
        public string INITIATOR_COM { get; set; }
        public string DH_INITIATOR_REMARK { get; set; }
        public string DH_AFTER_TRIP_OPT1 { get; set; }
        public string DH_AFTER_TRIP_OPT2 { get; set; }
        public string DH_AFTER_TRIP_OPT3 { get; set; }
        public string DH_AFTER_TRIP_OPT2_REMARK { get; set; }
        public string DH_REMARK { get; set; }
        public string DH_AFTER_TRIP_OPT3_REMARK { get; set; }
        public string DH_CITY { get; set; }
        public decimal? DH_TOTAL_PERSON { get; set; }
        public DateTime? DH_CREATE_DATE { get; set; }


        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string DH_TYPE_FLOW { get; set; }

        //DevFix 20210622 0000 เพิ่มข้อมูล ประเภทพนักงาน 1:Employee, 2:Contract
        public string REQUEST_USER_TYPE { get; set; }
        public string REQUEST_USER_NAME { get; set; }
    }

    public class DocEmailDetailsSearchModel
    {
        public string? token_login { get; set; }
        public string? id_doc { get; set; }
        public string? id { get; set; }
    }
}