using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace top.ebiz.service.Models.Create_Trip
{
    public class DocDetail4Model
    {
        public string token { get; set; }
        public string id_doc { get; set; }
    }

    public class DocDetail4OutModel
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
        public List<DocList4TravelerListModel> traveler_list { get; set; } = new List<DocList4TravelerListModel>();
        public List<DocList4TravelerSummaryListModel> traveler_summary { get; set; } = new List<DocList4TravelerSummaryListModel>();
    }
    public class DocDetail4HeadModel
    {
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string emp_org { get; set; }
        public string appr_emp_id { get; set; }
        public string appr_emp_name { get; set; }
        public string appr_emp_org { get; set; }
        public string continent { get; set; }
        public string country { get; set; }
        public string province { get; set; }
        public string city_text { get; set; }
        public string bus_date { get; set; }
        public string travel_date { get; set; }
        public string action_status { get; set; }
        public string ref_id { get; set; }
    }
    public class DocList4Model
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
    }
    public class DocList4TravelerListModel
    {
        //public string no { get; set; }
        //public string emp_code { get; set; }
        //public string emp_name { get; set; }
        //public string emp_unit { get; set; }
        public string text { get; set; }
    }

    public class DocList4TravelerSummaryListModel
    {
        public string ref_id { get; set; }
        public string no { get; set; }
        public string emp_name { get; set; }
        public string emp_unit { get; set; }
        public string country { get; set; }
        public string province { get; set; }
        public string business_date { get; set; }
        public string traveler_date { get; set; }
        public string total_expenses { get; set; }
        public string appr_name { get; set; }
        public string appr_status { get; set; }
        public string appr_remark { get; set; }
    }

}