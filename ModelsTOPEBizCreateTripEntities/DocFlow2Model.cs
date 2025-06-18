using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using top.ebiz.service.Service;

namespace top.ebiz.service.Models.Create_Trip
{
    public class DocFlow2Model
    {
        public string? token_login { get; set; }
        public string? doc_id { get; set; }
        public string? type { get; set; }
        public actionModel? action { get; set; }
        public docFlow2_data? oversea { get; set; }
        public docFlow2_data? local { get; set; }
        public bool? checkbox_1 { get; set; }
        public bool? checkbox_2 { get; set; }
        public string? remark { get; set; }

        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string? type_flow { get; set; }

    }

    public class docFlow2_data
    { 
        public List<docFlow2_travel>? traveler { get; set; }
        public List<docFlow2_approve>? approver { get; set; }
        public bool? checkbox_1 { get; set; }
        public bool? checkbox_2 { get; set; }
        public string? remark { get; set; }
    }
    public class docFlow2_travel
    {
        public string? emp_id { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? air_ticket { get; set; }

        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? accommodation { get; set; }

        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? allowance { get; set; }

        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? allowance_day { get; set; }

        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? allowance_night { get; set; }
         
        public string? clothing_valid { get; set; }

        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? clothing_expense { get; set; }

 
        public string? passport_valid { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? passport_expense { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? visa_fee { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? travel_insurance { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? transportation { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? registration_fee { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? miscellaneous { get; set; }


        [JsonConverter(typeof(DecimalToStringConverter))]
        public string? total_expenses { get; set; }


        //DevFix 20250129 0000 เพิ่ม exchange_date,exchange_rate,exchange_currency
        public string? exchange_date { get; set; }
        public string? exchange_rate { get; set; }
        public string? exchange_currency { get; set; }


        public string? upd_status { get; set; }
        public string? ref_id { get; set; }
        public string? edit { get; set; }
        public string? delete { get; set; }
        public string? remark { get; set; }

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
        public string? approve_status { get; set; }
        public string? approve_remark { get; set; }
    }

    public class docFlow2_approve
    {
        public string? line_id { get; set; }
        public string? type { get; set; }
        public string? emp_id { get; set; }
        public string? emp_name { get; set; }
        public string? emp_org { get; set; }
        public string? appr_id { get; set; }
        public string? appr_name { get; set; }
        public string? appr_org { get; set; }
        public string? remark { get; set; }
        public string? doc_status { get; set; }
        public string? appr_status { get; set; }
        public string? appr_remark { get; set; }
        public string? token_update { get; set; }
        public string? record_status { get; set; }

        //public string line_id { get; set; }
        //public string type { get; set; }
        //public string emp_id { get; set; }
        //public List<docFlow2_appr_traveler> traveler { get; set; } = new List<docFlow2_appr_traveler>();

        //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
        public string? approve_status { get; set; }
        public string? approve_remark { get; set; }

        //DevFix 20210810 0000 approve level ตามลำดับได้เลย เรียงตาม traverler id
        public string? approve_level { get; set; }
    }

    public class docFlow2_appr_traveler
    {
        public string? emp_id { get; set; }
    } 
}