using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace top.ebiz.service.Models.Create_Trip
{
    public class CountryModel
    {
        public string token_login { get; set; }

        
        public List<continentID> continent { get; set; }
    }

    public class continentID
    {
        public string id { get; set; }
    }
    public class CountryResultModel
    {
        public string country_id { get; set; }
        public string country { get; set; }
        public string continent_id { get; set; }
        public string continent { get; set; }
    }
}