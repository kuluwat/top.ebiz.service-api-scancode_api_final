using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class SAPModel
    {
        public string? token_login { get; set; }

        public List<SAPPasportModel>? get_passport = new List<SAPPasportModel>();

        public string? date_from { get; set; }
        public string? date_to { get; set; }
        public actionModel? remark = new actionModel();
    }
     
    public class SAPPasportModel
    {
        public string? date_from { get; set; }
        public string? date_to { get; set; } 
        public string? remark { get; set; }
    }
    public class actionModel
    {
        public string? type { get; set; }
        public string? remark { get; set; }
    }



}