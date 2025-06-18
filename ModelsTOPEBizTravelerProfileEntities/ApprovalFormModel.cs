
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class ApprovalFormModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
    }

    public class ApprovalFormOutModel
    {
        public string token_login { get; set; }
        public string id { get; set; }
        public Boolean user_admin { get; set; }
        public string data_type { get; set; }

        public string requested_by { get; set; }
        public string on_behalf_of { get; set; }
        public string org_unit_req { get; set; }
        public string org_unit_on_behalf { get; set; }

        public string date_to_requested { get; set; }
        public string document_number { get; set; }
        public string document_status { get; set; }
        public string company { get; set; }
        public string travel_type { get; set; }
        public string travel_with { get; set; }
         
        public List<traveldetailsList> travel_details { get; set; } = new List<traveldetailsList>();
        public List<travelersummaryList> traveler_summary { get; set; } = new List<travelersummaryList>();

        public List<estimateexpenseList> estimate_expense { get; set; } = new List<estimateexpenseList>();
        public List<estimateexpensedetailsList> estimate_expense_details { get; set; } = new List<estimateexpensedetailsList>();

        public List<approvalbyList> approval_by { get; set; } = new List<approvalbyList>();
        public List<approvaldetailsList> approval_details { get; set; } = new List<approvaldetailsList>();


        [NotMapped] 
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class traveldetailsList
    {
        public string no { get; set; }
        public string travel_topic { get; set; }
        public string continent { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string location { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string travel_duration { get; set; }
        public string traveling_objective { get; set; }

        public string to_submit { get; set; }
        public string to_share { get; set; }
        public string to_share_remark { get; set; }
        public string other { get; set; }
        public string other_remark { get; set; }

    }
    public class travelersummaryList
    {
        public string no { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string org_unit { get; set; }
        public string country_city { get; set; }
        public string province { get; set; }
        public string location { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string budget_account { get; set; }
    }
    
    public class estimateexpenseList
    {
        public string exchange_rates_as_of { get; set; }
        public string grand_total_expenses { get; set; }
    }
    public class estimateexpensedetailsList
    {
        public string no { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string org_unit { get; set; }
        public string country_city { get; set; }
        public string province { get; set; }
        public string location { get; set; }
        public string business_date { get; set; }
        public string travel_date { get; set; }
        public string budget_account { get; set; }

        public string air_ticket { get; set; }
        public string accommodation { get; set; }
        public string allowance { get; set; }
        public string transportation { get; set; }
        public string passport { get; set; }
        public string passport_valid { get; set; }
        public string visa_fee { get; set; }
        public string others { get; set; }
        public string luggage_clothing { get; set; }
        public string luggage_clothing_valid { get; set; }
        public string insurance { get; set; }
        public string total_expenses { get; set; }

        public string remark { get; set; }
    }

    public class approvalbyList
    {
        public string the_budget { get; set; }
        public string shall_seek { get; set; }
        public string remark { get; set; }
    }
    public class approvaldetailsList
    {
        public string no { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string org_unit { get; set; }
        public string line_approval { get; set; }
        public string cap_approval { get; set; }
        public string org_unit_line { get; set; }
        public string org_unit_cap { get; set; }
        public string approved_date_line { get; set; }
        public string approved_date_cap { get; set; }
    }

}