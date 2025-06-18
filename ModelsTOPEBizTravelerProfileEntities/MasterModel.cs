using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    #region get master
    public class CurrencyList
    {
        public string id { get; set; }
        public string name { get; set; }
        public string sort_by { get; set; }
    }
    public class ExchangeRateList
    {
        public string id { get; set; }

        public string currency_id { get; set; }
        public string exchange_rate { get; set; }
        public string date_from { get; set; }
        public string date_to { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }

    }
    public class MStatusModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string sort_by { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class MFeedbackTypeModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string sort_by { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class MFeedbackListModel
    {
        public string feedback_type_id { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string sort_by { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }
    public class MMasterNomalModel
    {
        public string id { get; set; }
        public string main_id { get; set; }
        public string sub_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string sort_by { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }

    public class MMaintainDataModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public Boolean user_admin { get; set; }

        public string year { get; set; }
        public string page_name { get; set; }
        public string module_name { get; set; }

        public List<MasterNormalModel> airticket_type { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel> already_booked { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel> list_status { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel> allowance_type { get; set; } = new List<MasterNormalModel>();

        public List<MasterNormalModel> feedback_type { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel> feedback_list { get; set; } = new List<MasterNormalModel>();
        public List<MasterNormalModel> feedback_question { get; set; } = new List<MasterNormalModel>();

        public List<MasterAllowance_ListModel> allowance_list { get; set; } = new List<MasterAllowance_ListModel>();

        public List<MMasterNomalModel> master_zone { get; set; } = new List<MMasterNomalModel>();
        public List<MMasterNomalModel> master_country { get; set; } = new List<MMasterNomalModel>();
        public List<MMasterNomalModel> master_province { get; set; } = new List<MMasterNomalModel>();
        public List<MMasterNomalModel> master_visa_doc { get; set; } = new List<MMasterNomalModel>();
        public List<MMasterNomalModel> master_currency { get; set; } = new List<MMasterNomalModel>();
        public List<MasterAirportModel> master_airport { get; set; } = new List<MasterAirportModel>();
        public List<MMasterInsurancebrokerModel> master_insurancebroker { get; set; } = new List<MMasterInsurancebrokerModel>();
         
        public List<MasterVISADocument_ListModel> visa_document { get; set; } = new List<MasterVISADocument_ListModel>();
        public List<MasterVISADocountries_ListModel> visa_docountries { get; set; } = new List<MasterVISADocountries_ListModel>();

        public List<ImgList> img_list { get; set; } = new List<ImgList>();

        [NotMapped]
        public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }
    public class MasterNormalModel
    {
        public string? token_login { get; set; }
        public string? data_type { get; set; }
        public bool? user_admin { get; set; }

        public string? id_main { get; set; }
        public string? id_sub { get; set; }
        public string? id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? status { get; set; }
        public string? sort_by { get; set; }
        public string? page_name { get; set; }
        public string? module_name { get; set; }

        public string? question_other { get; set; }

        public string? action_type { get; set; }
        public string? action_change { get; set; }

        public string? sub_data { get; set; }
    }
    public class MasterCountryModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public Boolean user_admin { get; set; }
         
        public string emp_id { get; set; }
        public string country_id { get; set; }
        public string country_name { get; set; }
        public string description { get; set; } 

        public string action_type { get; set; }
        public string action_change { get; set; }

        public string sub_data { get; set; }
    }
    public class MasterAirportModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public Boolean user_admin { get; set; }

        public string id { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string airport_code { get; set; }
        public string county_name { get; set; }
        public string city_name { get; set; }

        public string status { get; set; }
        public string sort_by { get; set; }
    }
    public class MasterSectionModel
    {
        public string section  { get; set; }
        public string department { get; set; }
        public string function { get; set; }
        public string sort_by { get; set; }
    }

    #endregion get master



    #region maintain master
    //public class MMenuModel
    //{
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public string url { get; set; }

    //}
    public class MMenuModel
    {
        public string token_login { get; set; }

        public List<MMenuListModel> menuList = new List<MMenuListModel>();
       [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }

    public class MMenuListModel
    {
        public string token_login { get; set; }

        public string pagename { get; set; }
        public string menuname { get; set; }
        public string url { get; set; }
        public Boolean display { get; set; }

    }
    public class MasterAllowance_ListModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public Boolean user_admin { get; set; }

        public string id { get; set; }
        public string travel_category { get; set; }
        public string overnight_type { get; set; }
        public string kh_code { get; set; }
        public string workplace { get; set; }
        public string workplace_type_country { get; set; }
        public string allowance_rate { get; set; }
        public string currency { get; set; }

        public string status { get; set; }
        public string sort_by { get; set; }
        public string remark { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }

    //visadocument
    public class MasterVISADocument_ListModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public Boolean user_admin { get; set; }

        public string id { get; set; }
        public string name { get; set; }//field : description
        public string preparing_by { get; set; }
        public string description { get; set; }

        public string status { get; set; }
        public string sort_by { get; set; }
        public string remark { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }

        public string sub_data { get; set; }
    }

    //visadocountries
    public class MasterVISADocountries_ListModel
    {
        public string token_login { get; set; }
        public string data_type { get; set; }
        public Boolean user_admin { get; set; }

        public string id { get; set; }
        public string continent_id { get; set; }//field : master_zone.id 
        public string country_id { get; set; }//field : master_country.id 
        public string visa_doc_id { get; set; }//field : visa_document.id
        public string name { get; set; }//field : description
        public string preparing_by { get; set; }
        public string description { get; set; }

        public string status { get; set; }
        public string sort_by { get; set; }
        public string remark { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }


    public class MMasterInsurancebrokerModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string travelcompany_type { get; set; }

        public string status { get; set; }
        public string sort_by { get; set; }

        //status เพื่อให้ทราบว่า master broker ตัวไหนเอาไปใช่กับ isos 
        public string status_isos { get; set; }
        //status เพื่อให้ทราบว่า master broker ตัวไหนเอาไปใช่กับ insurance 
        public string status_insurance { get; set; }

        public string action_type { get; set; }
        public string action_change { get; set; }
    }

    #endregion maintain master

}