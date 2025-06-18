
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class FeedbackModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class FeedbackOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string data_type { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public Boolean user_request { get; set; }

        public string feedback_for { get; set; }


        public List<feedbackList> feedback_detail { get; set; } = new List<feedbackList>();
        public List<EmpListOutModel> emp_list { get; set; } = new List<EmpListOutModel>();

        public List<MFeedbackTypeModel> feedback_type_master { get; set; } = new List<MFeedbackTypeModel>();
        public List<MFeedbackListModel> feedback_topic_list { get; set; } = new List<MFeedbackListModel>();

  
        [NotMapped] 
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class feedbackList
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string id { get; set; }

        public string feedback_type_id { get; set; }
        public string feedback_type_name { get; set; }

        public string feedback_list_id { get; set; }
        public string feedback_list_name { get; set; }
        public string question_other { get; set; }

        public string feedback_question_id { get; set; }

        public string topic_id { get; set; }
        public string topic_name { get; set; }

        public Boolean subjective { get; set; }

        public string no { get; set; }
        public string question { get; set; }
        public string description { get; set; }
        public string answer { get; set; }


        public string action_type { get; set; }
        public string action_change { get; set; }
    }


}