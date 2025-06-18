using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class TravelDayModel
    {
        public string token_login { get; set; }
        public string type { get; set; }
        public List<TravelDayCountryModel> country { get; set; }
        public string start_date { get; set; }
        public string stop_date { get; set; }
    }

    public class TravelDayCountryModel
    {
        public string id { get; set; }
    }
    public class TravelDayResultModel
    {
        public string day { get; set; }
    }

}