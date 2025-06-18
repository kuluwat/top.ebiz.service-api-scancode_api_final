
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class SendEmailModel
    {
        public string user_token { get; set; }
        public string doc_no { get; set; }
        public string page_name { get; set; }
        public string action_name { get; set; }

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
        public Boolean test_send_email { get; set; }

        [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }

    public class EmailModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string page_name { get; set; }
        public string action_name { get; set; }

        public List<Users> aduser_list { get; set; } = new List<Users>();
        public List<emailList> email_list { get; set; } = new List<emailList>();
        public List<ImgList> file_list { get; set; } = new List<ImgList>();


        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }

    public class emailList
    {
        public string id { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; } 
        public string emp_name { get; set; }
        public string email { get; set; }
        public string emp_unit { get; set; }
        public string emp_type { get; set; } 
        public string email_status { get; set; }
        public string email_msg { get; set; } 
    }

    public class mailselectList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }
        public string page_name { get; set; }
        public string module { get; set; }

        public string mail_emp_id { get; set; }
        public string mail_to { get; set; }
        public string mail_to_display { get; set; }
        public string mail_cc { get; set; }
        public string mail_bcc { get; set; }
        public string mail_body { get; set; }
        public string mail_body_in_form { get; set; }
        public string mail_attachments { get; set; }
        public string mail_status { get; set; }
        public string mail_remark { get; set; }

        public string remark_by_module { get; set; }

        public string country_in_doc { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }

}
