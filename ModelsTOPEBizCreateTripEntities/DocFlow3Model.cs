using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace top.ebiz.service.Models.Create_Trip
{
    public class DocFlow3Model
    {
        public string? token_login { get; set; }
        public string? doc_id { get; set; }
        public actionModel? action { get; set; }
        public bool? checkbox_1 { get; set; }
        public bool? checkbox_2 { get; set; }
        public string? remark { get; set; }

        [NotMapped]
        public afterTripModel? after_trip { get; set; } = new afterTripModel();
        public List<DocFlow3TravelerSummary>? traveler_summary { get; set; }

        //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
        public string? type_flow { get; set; }

        //DevFix 20210527 0000 file
        public List<DocFileListInModel>? docfile { get; set; } = new List<DocFileListInModel>();

        //DevFix 20211004 0000 file 
        public string? file_approval_output_form { get; set; }


    }

    public class DocFlow3TravelerSummary
    {
        public string? ref_id { get; set; }
        public string? take_action { get; set; }
        public string? appr_status { get; set; }
        public string? appr_remark { get; set; }
        public string? traverler_id { get; set; }
        public string? appr_id { get; set; }
    }

    public class allApproveModel
    {
        public string? emp_id { get; set; }
        public decimal? status_value { get; set; }
        public string? doc_status { get; set; }
    }
    public class capApproveModel
    {
        public string? appr_empid { get; set; }
        public string? traverler_empid { get; set; }
    }


}