using Newtonsoft.Json.Linq;

namespace top.ebiz.service.Service.Report
{
    public class ClassReportModel
    {
        public class ParamTravelRecord
        {
            public string? token_login { get; set; }
            public string? doc_id { get; set; }
            public string? country { get; set; }
            public string? date_from { get; set; }
            public string? date_to { get; set; }
            public string? travel_type { get; set; }
            public string? emp_id { get; set; }
            public string? section { get; set; }
            public string? department { get; set; }
            public string? function { get; set; }
            public Travel_List[]? travel_list { get; set; }
        }
        public class Travel_List
        {
            public string? id { get; set; }
        }
        public class ReportParamModel
        {
            public string? method { get; set; }
            public string? param { get; set; }
        }
        public class ReportParamJsonModel
        {
            public string? method { get; set; }
            public string? param { get; set; }
            public string? jsondata { get; set; }
        }
        public class ReportParamxJsonModel
        {
            public string method { get; set; }
            public TravelRecordParam param { get; set; }
            public string jsondata { get; set; }
        }
        public class InsuranceRecordRequest
        {
            public string token_login { get; set; }
            public string year { get; set; }
        }
        public class TravelRecordParam
        {
            public string token_login { get; set; }
            public string doc_id { get; set; }
            public string country { get; set; }
            public string date_from { get; set; }
            public string date_to { get; set; }
            public string travel_type_name { get; set; }
            public string emp_id { get; set; }
            public string section { get; set; }
            public string department { get; set; }
            public string function { get; set; }
            public string travel_type { get; set; }
        }
    }
}
