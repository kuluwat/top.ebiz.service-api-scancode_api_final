using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Traveler_Profile
{ 
    public class TrackingModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        //public actionModel action { get; set; }
        public Boolean checkbox_1 { get; set; }
        public Boolean checkbox_2 { get; set; }
        public string remark { get; set; }
        public string action_change { get; set; }
    } 
}