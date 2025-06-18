using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using top.ebiz.service.Models.Traveler_Profile;

namespace top.ebiz.service.Models.Create_Trip
{
    public class DocDetail3Model
    {
        public string token { get; set; }
        public string id_doc { get; set; }
    }

    public class DocDetail3OutModel
    {
        public buttonModel button { get; set; } = new buttonModel();
        public string document_status { get; set; }
        public string topic { get; set; }
        public string continent { get; set; }
        public string country { get; set; }
        public string province { get; set; }
        public string travel_date { get; set; }
        public string business_date { get; set; }
        public string total_travel { get; set; }
        public string grand_total { get; set; }

        public string checkbox_1 { get; set; }
        public string checkbox_2 { get; set; }
        public string remark { get; set; }

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();

        public List<travelerList> traveler_list { get; set; } = new List<travelerList>();
        public List<travelerSummaryList> traveler_summary { get; set; } = new List<travelerSummaryList>();

        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string type_flow { get; set; }

        //DevFix 20210527 0000 file
        public List<DocFileListOutModel> docfile { get; set; } = new List<DocFileListOutModel>();

        public List<DocFileModel>? doc_file_table { get; set; } = new List<DocFileModel>();
        public string msg_remark { get; set; }

    }
    public class DocDetail3HeadModel
    {
        public string? emp_id { get; set; }
        public string? emp_name { get; set; }
        public string? emp_org { get; set; }
        public string? appr_emp_id { get; set; }
        public string? appr_emp_name { get; set; }
        public string? appr_emp_org { get; set; }
        public string? continent { get; set; }
        public string? country { get; set; }
        public string? province { get; set; }
        public string? city_text { get; set; }
        public string? bus_date { get; set; }
        public string? travel_date { get; set; }
        public string? action_status { get; set; }
        public string? ref_id { get; set; }
        public string? take_action { get; set; }
        public string? appr_status { get; set; }
        public string? appr_remark { get; set; }
        public string? total { get; set; }

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
        //0 กับ 3 แก้ไขได้
        public string? approve_status { get; set; }
        public string? approve_remark { get; set; }
        //DevFix 20210719 0000 เพิ่ม field OPT
        public string? approve_opt { get; set; }
        public string? remark_opt { get; set; }
        public string? remark_cap { get; set; }

        //DevFix 20210721 0000 เพิ่ม field เพื่อนำไปเช็คผู้อนุมัติใน tab 3
        public string? approve_id { get; set; }
        public string? approve_role_type { get; set; }

        //DevFix 20211013 0000 เพิ่ม key เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
        public string? traveler_ref_id { get; set; }


        //DevFix 20221121 0000 กรณีที่ traverler 1 มีมากกว่า 1 cap ให้ใช้ ค่าใช้จ่าย รายการเดียวพอ --> ใช้ dte_id รหัสข้อมูลรายการ เป็น key 
        public decimal? dte_id { get; set; }
        public decimal? dta_appr_level { get; set; }


        //DevFix 2025038 0000 กรกณีที่ต้องการเรียง approver
        public decimal? dta_id { get; set; }
        public List<DocFileModel>? doc_file_table { get; set; } = new List<DocFileModel>();

    } 
     
    public class ProvinceResult
    {
        public string? PROVINCE { get; set; }
        public decimal? DTE_ID { get; set; }
    }
    public class CityResult
    {
        public string? PROVINCE { get; set; }
        public string? CITY_TEXT { get; set; }
        public decimal? DTE_ID { get; set; }


    }
    public class DateTravelResult
    {
        public string? BUS_DATE { get; set; }
        public string? TRAVEL_DATE { get; set; }


    }
    public class DocDetail3HeadVModel
    {

        public string? CONTINENT { get; set; }
        public string? PROVINCE { get; set; }
        public string? COUNTRY { get; set; }
        public string? CITY_TEXT { get; set; }
        public string? ACTION_STATUS { get; set; }
        public string? TAKE_ACTION { get; set; }
        public string? APPR_STATUS { get; set; }
        public string? APPR_REMARK { get; set; }
        public string? BUS_DATE { get; set; }
        public string? TRAVEL_DATE { get; set; }
        public string? EMP_ID { get; set; }
        public string? EMP_NAME { get; set; }
        public string? EMP_ORG { get; set; }
        public string? APPR_EMP_ID { get; set; }
        public string? APPR_EMP_NAME { get; set; }
        public string? APPR_EMP_ORG { get; set; }
        public string? REF_ID { get; set; }
        public string? TOTAL { get; set; }
        public string? DTE_ID { get; set; } 

        public string? DTA_APPR_LEVEL { get; set; }
       
        public string? APPROVE_STATUS { get; set; }
        public string? APPROVE_REMARK { get; set; }
        public string? APPROVE_OPT { get; set; }
        public string? REMARK_OPT { get; set; }
        public string? REMARK_CAP { get; set; }
        public string? TRAVELER_REF_ID { get; set; }
         

        //DevFix 2025038 0000 กรกณีที่ต้องการเรียง approver
        public decimal? dta_id { get; set; }
    } 
    
    public class DocDetail3HeadTable1Model
    {
        //VW_BZ_TRAVEL_LOCATION_DATA
        public string? CONTINENT { get; set; }
        public string? PROVINCE { get; set; }
        public string? COUNTRY { get; set; }
        public string? CITY_TEXT { get; set; }
        public string? DTE_ID { get; set; }
        public string? DH_CODE { get; set; }
        public string? DTA_APPR_EMPID { get; set; } 
    }
    public class DocDetail3HeadVVModel
    {
        public string? CONTINENT { get; set; }
        public string? PROVINCE { get; set; }
        public string? COUNTRY { get; set; } 
    }
    public class DocList3Model
    {
        public string topic { get; set; }
        public string type { get; set; }
        public string checkbox_1 { get; set; }
        public string checkbox_2 { get; set; }
        public string doc_status { get; set; }
        public string document_status { get; set; }
        public string remark { get; set; }
        public string DH_AFTER_TRIP_OPT1 { get; set; }
        public string DH_AFTER_TRIP_OPT2 { get; set; }
        public string DH_AFTER_TRIP_OPT3 { get; set; }
        public string DH_AFTER_TRIP_OPT2_REMARK { get; set; }
        public string DH_AFTER_TRIP_OPT3_REMARK { get; set; }
        public string person { get; set; }
        public string bus_date { get; set; }
        public string travel_date { get; set; }
        public string continent { get; set; }
        public string country { get; set; }
        public string city_text { get; set; }

        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow$ 2:not flow$ 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string DH_TYPE_FLOW { get; set; }
    }
    public class DocList3VModel
    {
        public string? DH_CODE { get; set; }
        public string? TYPE { get; set; }
        public string? CHECKBOX_1 { get; set; }
        public string? CHECKBOX_2 { get; set; }
        public string? REMARK { get; set; }
        public string? DOC_STATUS { get; set; }
        public string? DOCUMENT_STATUS { get; set; }
        public string? DH_AFTER_TRIP_OPT1 { get; set; }
        public string? DH_AFTER_TRIP_OPT2 { get; set; }
        public string? DH_AFTER_TRIP_OPT3 { get; set; }
        public string? DH_AFTER_TRIP_OPT2_REMARK { get; set; }
        public string? DH_AFTER_TRIP_OPT3_REMARK { get; set; }
        public string? PERSON { get; set; }
        public string? TOPIC { get; set; }
        public string? BUS_DATE { get; set; }
        public string? TRAVEL_DATE { get; set; }
        public string? CITY_TEXT { get; set; }
        public string? COUNTRY { get; set; }
        public string? CONTINENT { get; set; }
        public string? DH_TYPE_FLOW { get; set; }

    }
    public class travelerList
    {
        //public string no { get; set; }
        //public string emp_code { get; set; }
        //public string emp_name { get; set; }
        //public string emp_unit { get; set; }
        public string? text { get; set; }
        public string? emp_id { get; set; }
        public string? country { get; set; }
        public string? businessDate { get; set; }
    }

    public class travelerSummaryList
    {
        public string ref_id { get; set; }
        public string no { get; set; }
        public string sort_by { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_unit { get; set; }
        public string country { get; set; }
        public string province { get; set; }
        public string business_date { get; set; }
        public string traveler_date { get; set; }
        public string total_expenses { get; set; }
        public string take_action { get; set; }
        public string appr_id { get; set; }
        public string appr_name { get; set; }
        public string appr_status { get; set; }
        public string appr_remark { get; set; }

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject
        public string approve_status { get; set; }
        public string approve_remark { get; set; }
        //DevFix 20210719 0000 เพิ่ม field OPT
        public string approve_opt { get; set; }
        public string remark_opt { get; set; }
        public string remark_cap { get; set; }

        //DevFix 20211013 0000 เพิ่ม key เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
        public string traveler_ref_id { get; set; }
    }

    //DevFix 20210527 0000 file
    public class DocFileListInModel
    {
        // DH_CODE, DF_ID, DF_NAME, DF_PATH, DF_REMARK
        public string? DH_CODE { get; set; }
        public string? DF_ID { get; set; }
        public string? DF_NAME { get; set; }
        public string? DF_PATH { get; set; }
        public string? DF_REMARK { get; set; }

    }
    public class DocFileListOutModel
    {
        // DH_CODE, DF_ID, DF_NAME, DF_PATH, DF_REMARK
        public string? DH_CODE { get; set; }
        public string? DF_ID { get; set; }
        public string? DF_NAME { get; set; }
        public string? DF_PATH { get; set; }
        [NotMapped]
        public string? DF_FULL_PATH { get; set; }
        public string? DF_REMARK { get; set; }

        [NotMapped]
        public afterTripModel? after_trip { get; set; } = new afterTripModel();
    } 
    public class DocFileListTravelerhistoryOutModel
    {
        // DH_CODE, DF_ID, DF_NAME, DF_PATH, DF_REMARK
        public string? DH_CODE { get; set; }
        public string? DF_ID { get; set; }
        public string? DF_NAME { get; set; }
        public string? DF_PATH { get; set; }
        [NotMapped]
        public string? DF_FULL_PATH { get; set; }
        public string? DF_REMARK { get; set; }
         
        public ImgList? img_list { get; set; } = new ImgList();

        [NotMapped]
        public afterTripModel? after_trip { get; set; } = new afterTripModel();
    } 
}