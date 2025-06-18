using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace top.ebiz.service.Models.Create_Trip
{
    public class sendEmailModel2
    {
        public string user_log { get; set; }
    }


    public class docEmailModel
    {
        public string action { get; set; }
        public string user_token { get; set; }
        public string doc_no { get; set; }
        public string doc_link_url { get; set; }
        public string mail_from { get; set; }
        public string mail_to { get; set; }
        public string mail_cc { get; set; }
        public string mail_subject { get; set; }
        public string mail_dear { get; set; }
        public string mail_detail { get; set; }
        public string mail_revise_reason { get; set; }

        public string mail_business_title { get; set; }
        public string mail_business_date { get; set; }
        public string mail_business_location { get; set; }
        public bool test_send_email { get; set; }
        public List<TravelerMail> mail_business_traveller { get; set; }

    }

    public class sendEmailModel
    {
        public string? doc_id { get; set; }
        public string? step_flow { get; set; }
        public string? mail_from { get; set; }
        public string? mail_to { get; set; }
        public string? mail_cc { get; set; }
        public string? mail_subject { get; set; }
        public string? mail_body { get; set; }
        public string? mail_attachments { get; set; }
        public string? mail_show_case { get; set; } 
    }

    public class TravelerMail
    {
        public string name { get; set; }
    }

    ////type,to_date,from_date,to_date_date
    //public class estimateExpenseModel
    //{
    //    public string? type { get; set; } 
    //    public string? to_date { get; set; } 
    //    public string? from_date { get; set; } 
    //    public string? to_date_date { get; set; } 
    //}
    public class tempEmpIdModel
    {
        public string? emp_id { get; set; } 
    }
    public class tempEmpSpecialModel
    {
        public string? emp_id { get; set; } 
        public string? email { get; set; } 
        public string? displayname { get; set; } 
    }
    public class temptravelInsuranceModel
    {
        //  temptravelInsuranceModel -> email,ins_broker_name
        public string? email { get; set; } 
        public string? ins_broker_name { get; set; } 
    }
    public class tempIdKeyModel
    {
        public string? id_key { get; set; } 
    }
    public class tempStatusModel 
    {
        public string? approve_status { get; set; } 
    }
    public class tempISOSMailModel
    { 
        //emp_id,send_mail_type,title,name,surname,section,department,function
        public string? emp_id { get; set; } 
        public string? send_mail_type { get; set; } 
        public string? title { get; set; } 
        public string? name { get; set; } 
        public string? surname { get; set; } 
        public string? section { get; set; } 
        public string? department { get; set; } 
        public string? function { get; set; }  
    }
    public class tempEMailModel
    {
        public string? email { get; set; } 
    }
    public class tempEmployeeProfileModel
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? emp_id { get; set; } 
        public string? position { get; set; }
    } 
    public class tempModel 
    {
        public string? id { get; set; }
        public string? name1 { get; set; }
        public string? name2 { get; set; }
        public string? name3 { get; set; } 
        public string? name4 { get; set; }
    }
    
    public class tempPassportModel 
    {
        //passport_no,passport_date_issue,passport_date_expire,sdate,edate
        public string? passport_no { get; set; }
        public string? passport_date_issue { get; set; }
        public string? passport_date_expire { get; set; }
        public string? sdate { get; set; } 
        public string? edate { get; set; }
    }
     
    public class docEmailDetailsOutModel
    {
        public List<BZ_EMAIL_DETAILS> email_details_list { get; set; } = new List<BZ_EMAIL_DETAILS>();
        public string msg_remark { get; set; }

    }
}
