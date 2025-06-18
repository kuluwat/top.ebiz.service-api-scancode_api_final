using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace top.ebiz.service.Models.Create_Trip
{


    public class DocDetail2Model
    {
        public buttonModel button { get; set; } = new buttonModel();
        public string type { get; set; }
        public string document_status { get; set; }
        public TypeModel oversea { get; set; } = new TypeModel();
        public TypeModel local { get; set; } = new TypeModel();

        //DevFix 20200827 2349 Exchange Rates as of -->ExchangeRatesModel
        public ExchangeRatesModel ExchangeRates { get; set; } = new ExchangeRatesModel();

        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string type_flow { get; set; }

        public string msg_remark { get; set; }

        public List<DocFileModel> doc_file_table { get; set; } = new List<DocFileModel>();


    }
    public class DocList2Model
    {
        public string? type { get; set; }
        public string? checkbox_1 { get; set; }
        public string? checkbox_2 { get; set; }
        public string? doc_status { get; set; }
        public string? document_status { get; set; }
        public string? remark { get; set; } 


        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string? DH_TYPE_FLOW { get; set; }
        public string? DH_VERSION { get; set; }
    }
    public class TypeModel
    {


        public List<employeeDoc2Model> employee { get; set; } = new List<employeeDoc2Model>();
        public List<travelerDoc2Model> traveler { get; set; } = new List<travelerDoc2Model>();
        public List<doc2ApproverModel> approver { get; set; } = new List<doc2ApproverModel>();
        public string checkbox_1 { get; set; }
        public string checkbox_2 { get; set; }
        public string remark { get; set; }
    }
    public class employeeDoc2Model
    {
        public string? ref_id { get; set; } = string.Empty;
        public string? id { get; set; } = string.Empty;
        public string? title { get; set; } = string.Empty;
        public string? name { get; set; } = string.Empty;
        public string? name2 { get; set; } = string.Empty;
        public string? org { get; set; } = string.Empty;
        public string? country_id { get; set; } = string.Empty;
        public string? country { get; set; } = string.Empty;
        public string? province { get; set; } = string.Empty;
        public string? business_date { get; set; } = string.Empty;
        public string? travel_date { get; set; } = string.Empty;
        public string? clothing_expense { get; set; } = string.Empty;
        public string? passport_expense { get; set; } = string.Empty;
        public string? visa_fee { get; set; } = string.Empty;
        public string? remark { get; set; } = string.Empty;

        //DevFix 20210813 0000 เพิ่ม field city
        public string? city { get; set; } = string.Empty;

        public employeeDoc2Model() { }
        public employeeDoc2Model(employeeDoc2V2Model model)
        {
            ref_id = model.ref_id;
            id = model.employeeid;
            // employeeid = model.employeeid;
            name = model.name;
            //name2 = model.name2;
            org = model.org;
            // dte_travel_days = model.dte_travel_days;
            business_date = model.business_date;
            travel_date = model.travel_date;
            visa_fee = model.visa_fee;
            passport_expense = model.passport_expense;
            clothing_expense = model.clothing_expense;
            country_id = model.country_id;
            country = model.country;
            province = model.province;
            remark = model.remark;
            city = model.district;
        }

    }

    //employeeList
    public class employeeDoc2SubmitModel
    {
        public string? ref_id { get; set; } = string.Empty;
        public string? id { get; set; } = string.Empty;
        public string? employeeid { get; set; } = string.Empty;
        public string? name { get; set; } = string.Empty;
        public string? name2 { get; set; } = string.Empty;
        public string? org { get; set; } = string.Empty;
        public string? dte_travel_days { get; set; } = string.Empty;
        public string? business_date { get; set; } = string.Empty;
        public string? travel_date { get; set; } = string.Empty;
        public string? visa_fee { get; set; } = string.Empty;
        public string? passport_expense { get; set; } = string.Empty;
        public string? clothing_expense { get; set; } = string.Empty;
        public string? country_id { get; set; } = string.Empty;
        public string? country { get; set; } = string.Empty;
        public string? province { get; set; } = string.Empty;
        public string? remark { get; set; } = string.Empty;
        public string? city { get; set; } = string.Empty;
    }
    public class employeeDoc2VModel
    {
        public string? ref_id { get; set; } = string.Empty;
        public string? id { get; set; } = string.Empty;
        public string? employeeid { get; set; } = string.Empty;
        public string? name { get; set; } = string.Empty;
        public string? name2 { get; set; } = string.Empty;
        public string? org { get; set; } = string.Empty;
        public string? dte_travel_days { get; set; } = string.Empty;
        public string? business_date { get; set; } = string.Empty;
        public string? travel_date { get; set; } = string.Empty;
        public string? visa_fee { get; set; } = string.Empty;
        public string? passport_expense { get; set; } = string.Empty;
        public string? clothing_expense { get; set; } = string.Empty;
        public string? country_id { get; set; } = string.Empty;
        public string? country { get; set; } = string.Empty;
        public string? province { get; set; } = string.Empty;
        public string? remark { get; set; } = string.Empty;
        public string? city { get; set; } = string.Empty;
    }

    public class employeeDoc2V2Model
    {
        public string? ref_id { get; set; } = string.Empty;
        public string? employeeid { get; set; } = string.Empty;
        public string? name { get; set; } = string.Empty;
        public string? org { get; set; } = string.Empty;
        public string? dte_travel_days { get; set; } = string.Empty;
        public string? business_date { get; set; } = string.Empty;
        public string? travel_date { get; set; } = string.Empty;
        public string? visa_fee { get; set; } = string.Empty;
        public string? passport_expense { get; set; } = string.Empty;
        public string? clothing_expense { get; set; } = string.Empty;
        public string? country_id { get; set; } = string.Empty;
        public string? country { get; set; } = string.Empty;
        public string? province { get; set; } = string.Empty;
        public string? remark { get; set; } = string.Empty;
        public string district { get; set; } = string.Empty;
    }
    public class travelerDoc2Model
    {

        public string emp_id { get; set; } = string.Empty;
        public string emp_name { get; set; } = string.Empty;
        public string? emp_name2 { get; set; } = string.Empty;
        public string org { get; set; } = string.Empty;
        public string country_id { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public string province { get; set; } = string.Empty;
        public string business_date { get; set; } = string.Empty;
        public string travel_date { get; set; } = string.Empty;
        public string air_ticket { get; set; } = string.Empty;
        public string accommodation { get; set; } = string.Empty;
        
        public string allowance { get; set; } = string.Empty;

        public string allowance_day { get; set; } = string.Empty;
        public string allowance_night { get; set; } = string.Empty;
        public string clothing_valid { get; set; } = string.Empty;
        public string clothing_expense { get; set; } = string.Empty;
        public string passport_valid { get; set; } = string.Empty;
        public string passport_expense { get; set; } = string.Empty;
        public string visa_fee { get; set; } = string.Empty;
        public string travel_insurance { get; set; } = string.Empty;
        public string transportation { get; set; } = string.Empty;
        public string registration_fee { get; set; } = string.Empty;
        public string miscellaneous { get; set; } = string.Empty;
        public string total_expenses { get; set; } = string.Empty;
        public string ref_id { get; set; } = string.Empty;
        public string edit { get; set; } = string.Empty;
        public string delete { get; set; } = string.Empty;
        public string remark { get; set; } = string.Empty;

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
        //0 กับ 3 แก้ไขได้
        public string approve_status { get; set; } = string.Empty;
        public string approve_remark { get; set; } = string.Empty;

        //DevFix 20210719 0000 เพิ่ม field OPT
        public string approve_opt { get; set; } = string.Empty;
        public string remark_opt { get; set; } = string.Empty;
        public string remark_cap { get; set; } = string.Empty;


        //DevFix 20210817 0000 เพิ่ม field status_approve_line, status_approve_cap, remark_approve_line, remark_approve_cap 
        public string status_approve_line { get; set; } = string.Empty;
        public string status_approve_cap { get; set; } = string.Empty;
        public string remark_approve_line { get; set; } = string.Empty;
        public string remark_approve_cap { get; set; } = string.Empty;

        //DevFix 20211013 0000 เพิ่ม key เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
        public string traveler_ref_id { get; set; } = string.Empty;


        //DevFix 20250129 0000 เพิ่ม exchange_date,exchange_rate,exchange_currency
        public string? exchange_date { get; set; } = string.Empty;
        public string? exchange_rate { get; set; } = string.Empty;
        public string? exchange_currency { get; set; } = string.Empty;

        public travelerDoc2Model() { }
        public travelerDoc2Model(travelerDoc2TempModel model)
        {
            emp_id = model.emp_id;
            air_ticket = model.air_ticket;
            accommodation = model.accommodation;
            allowance_day = model.allowance_day;
            allowance_night = model.allowance_night;
            clothing_valid = model.clothing_valid;
            clothing_expense = model.clothing_expense;
            passport_valid = model.passport_valid;
            passport_expense = model.passport_expense;
            visa_fee = model.visa_fee;
            travel_insurance = model.travel_insurance;
            transportation = model.transportation;
            registration_fee = model.registration_fee;
            miscellaneous = model.miscellaneous;
            total_expenses = model.total_expenses;
            emp_name = model.emp_name;
            org = model.org;
            country_id = model.country_id;
            country = model.country;
            business_date = model.business_date;
            travel_date = model.travel_date;
            // allowance = model.allowance;
            province = model.province;
            ref_id = model.ref_id;
            edit = model.edit;
            delete = model.delete;
            remark = model.remark;
            approve_status = model.approve_status;
            approve_remark = model.approve_remark;
            approve_opt = model.approve_opt;
            remark_opt = model.remark_opt;
            remark_cap = model.remark_cap;
            traveler_ref_id = model.traveler_ref_id;

            //DevFix 20250129 0000 เพิ่ม exchange_date,exchange_rate,exchange_currency
            exchange_date = model.exchange_date;
            exchange_rate = model.exchange_rate;
            exchange_currency = model.exchange_currency;
        }
    }

    public class travelerDoc2TempModel
    {
        public string? emp_id { get; set; } = string.Empty;
        public string? air_ticket { get; set; } = string.Empty;
        public string? accommodation { get; set; } = string.Empty;
        public string? allowance_day { get; set; } = string.Empty;
        public string? allowance_night { get; set; } = string.Empty;
        public string? clothing_valid { get; set; } = string.Empty;
        public string? clothing_expense { get; set; } = string.Empty;
        public string? passport_valid { get; set; } = string.Empty;
        public string? passport_expense { get; set; } = string.Empty;
        public string? visa_fee { get; set; } = string.Empty;
        public string? travel_insurance { get; set; } = string.Empty;
        public string? transportation { get; set; } = string.Empty;
        public string? registration_fee { get; set; } = string.Empty;
        public string? miscellaneous { get; set; } = string.Empty;
        public string? total_expenses { get; set; } = string.Empty;
        public string? emp_name { get; set; } = string.Empty;
        public string? org { get; set; } = string.Empty;
        public string? country_id { get; set; } = string.Empty;
        public string? country { get; set; } = string.Empty;
        public string? business_date { get; set; } = string.Empty;
        public string? travel_date { get; set; } = string.Empty;
        public string? allowance { get; set; } = string.Empty;
        public string? province { get; set; } = string.Empty;
        public string? ref_id { get; set; } = string.Empty;
        public string? edit { get; set; } = string.Empty;
        public string? delete { get; set; } = string.Empty;
        public string? remark { get; set; } = string.Empty;
        public string? approve_status { get; set; } = string.Empty;
        public string? approve_remark { get; set; } = string.Empty;
        public string? approve_opt { get; set; } = string.Empty;
        public string? remark_opt { get; set; } = string.Empty;
        public string? remark_cap { get; set; } = string.Empty;
        public string? traveler_ref_id { get; set; } = string.Empty;

        //exchange_date,exchange_rate,exchange_currency
        public string? exchange_date { get; set; } = string.Empty;
        public string? exchange_rate { get; set; } = string.Empty;
        public string? exchange_currency { get; set; } = string.Empty;
    }

    public class doc2ApproverModel
    {
        public string? line_id { get; set; } = string.Empty;
        public string? type { get; set; } = string.Empty;
        public string? emp_id { get; set; } = string.Empty;
        public string? emp_name { get; set; } = string.Empty;
        public string? emp_org { get; set; } = string.Empty;
        public string? appr_id { get; set; } = string.Empty;
        public string? appr_name { get; set; } = string.Empty;
        public string? appr_org { get; set; } = string.Empty;
        public string? remark { get; set; } = string.Empty;

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
        //0 กับ 3 แก้ไขได้
        public string? approve_status { get; set; } = string.Empty;
        public string? approve_remark { get; set; } = string.Empty;

        //DevFix 20210810 0000 approve level ตามลำดับได้เลย เรียงตาม traverler id
        public string? approve_level { get; set; } = string.Empty;
    }
    public class approverModel
    {
        public string line_id { get; set; }
        public string type { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_org { get; set; }

        public List<approvertraveler> approver_traveler { get; set; } = new List<approvertraveler>();




    }
    public class approvertraveler
    {

        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_org { get; set; }


    }

    public class approveRemark
    {
        public string appr_emp_id { get; set; }
        public string travel_emp_id { get; set; }
        public string remark { get; set; }
    }

    //DevFix 20200827 2349 Exchange Rates as of -->ExchangeRatesModel
    public class ExchangeRatesModel
    {
        public string? ex_value1 { get; set; }
        public string? ex_value { get; set; }
        public string? ex_date { get; set; }
        public string? ex_cur { get; set; }
    }

}