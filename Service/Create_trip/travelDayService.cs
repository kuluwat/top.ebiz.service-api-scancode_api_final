using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Service.Create_Trip
{
    public class travelDayService
    {
        public List<TravelDayResultModel> getTravelDay(TravelDayModel value)
        {
            var data = new List<TravelDayResultModel>();
            if (value == null || string.IsNullOrEmpty(value.start_date) || string.IsNullOrEmpty(value.stop_date))
            {
                data.Add(new TravelDayResultModel
                {
                    day = "0"
                });
                return data;
            }

            DateTime date1 = DateTime.ParseExact(value.start_date.Substring(0, 10), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime date2 = DateTime.ParseExact(value.stop_date.Substring(0, 10), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            int default_day = (date2 - date1).Days + 1;

            // default : local ไม่ต้องบวกเพิ่ม
            int adj_day = 0;
            //if (value.type.ToLower() == "oversea")
            //{
            //    adj_day = 2; // ไป1 กลับ1
            //}

            //if (value.country != null && value.country.Count() > 0)
            //{
            //    using (TOPEBizEntities context = new TOPEBizEntities())
            //    {
            //        string sql = "";
            //        sql = " select to_char(max(adjust_day)) day from BZ_CONFIG_TRAVEL_DATE  "; // where ct_id in (154, 166)
            //        string subsql = "";
            //        foreach (var item in value.country)
            //        {
            //            subsql += ", " + item.id ?? "";
            //        }

            //        if (subsql.Length > 0)
            //        {
            //            subsql = subsql.Substring(1);
            //        }

            //        sql += " where ct_id in (" + subsql + ") ";

            //        data = context.Database.SqlQuery<TravelDayResultModel>(sql).ToList();
            //    }

            //    if (data != null && data.Count() > 0 && !string.IsNullOrEmpty(data[0].day))
            //    {
            //        adj_day = Convert.ToInt32(data[0].day);
            //    }
            //}

            data = new List<TravelDayResultModel>();
            data.Add(new TravelDayResultModel
            {
                day = (default_day + adj_day).ToString()
            });

            return data;
        }


    }
}