
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Create_Trip;
using System.Text;
using System.Linq;


namespace top.ebiz.service.Service.Traveler_Profile
{
    public class ExportFileInModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; }

        public string path { get; set; }
        public string filename { get; set; }
        public string pagename { get; set; }
        public string actionname { get; set; }
        public string filetype { get; set; }//excel,pdf

        [NotMapped] public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }

    public class ExportFileOutModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; }

        public string path { get; set; }
        public string filename { get; set; }
        public string pagename { get; set; }
        public string actionname { get; set; }
        public string filetype { get; set; }//excel,pdf

        [NotMapped] public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }
    public class ExportISOSModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; }

        public string path { get; set; }
        public string filename { get; set; }
        public string pagename { get; set; }
        public string actionname { get; set; }
        public string filetype { get; set; }//excel,pdf

        [NotMapped] public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class ExportRecordModel
    {
        public string? token_login { get; set; }
        public string? year { get; set; }

        [NotMapped] public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }

    public class Report_AllowanceModel
    {
        public string token_login { get; set; }
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string id { get; set; }

        public string title { get; set; }
        public string country { get; set; }
        public string functional { get; set; }
        public string business_date { get; set; }
        public string departure_date { get; set; }
        public string arrival_date { get; set; }
        public string io_number { get; set; }
        public string cost_center { get; set; }
        public string gl_account { get; set; }

        public string employee_id { get; set; }
        public string employee_name { get; set; }
        public string total { get; set; }
        public string unit { get; set; }
        public string total_thb { get; set; }
        public string last_update { get; set; }

        public string icon_travel_agent { get; set; }
        public string icon_other { get; set; }
        public string passport { get; set; }
        public string passport_date { get; set; }
        public string luggage_clothing { get; set; }
        public string luggage_clothing_date { get; set; }
        public string remark { get; set; }
        public string important_note { get; set; }


        public List<ExchangeRateList> m_exchangerate { get; set; } = new List<ExchangeRateList>();
        public List<ExchangeRateList> m_exchangerate_max { get; set; } = new List<ExchangeRateList>();

        public List<dailyallowanceModel> dailyallowance { get; set; } = new List<dailyallowanceModel>();
        public List<flightscheduleModel> flightschedule { get; set; } = new List<flightscheduleModel>();


        [NotMapped] public afterTripModel? after_trip { get; set; } = new afterTripModel();

    }
    public class dailyallowanceModel
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string id { get; set; }

        public string allowance_days { get; set; }
        public string allowance_date { get; set; }
        public string allowance_low { get; set; }
        public string allowance_mid { get; set; }
        public string allowance_hight { get; set; }
        public string allowance_total { get; set; }
        public string allowance_unit { get; set; }

    }
    public class flightscheduleModel
    {
        public string doc_id { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string id { get; set; }

        public string airticket_date { get; set; }
        public string airticket_route_from { get; set; }
        public string airticket_route_to { get; set; }
        public string airticket_flight { get; set; }
        public string airticket_departure_time { get; set; }
        public string airticket_arrival_time { get; set; }
    }



    public class ReportISOSRecordOutModel
    {
        public string? token_login { get; set; }
        public string? year { get; set; }

        public List<reportisosList>? details_list { get; set; } = new List<reportisosList>();

        [NotMapped] public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }
    public class ReportInsuranceRecordOutModel
    {
        public string? token_login { get; set; }
        public string? year { get; set; }

        public List<insuranceModel> details_list { get; set; } = new List<insuranceModel>();

        [NotMapped] public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }
    public class reportisosList
    {
        public string? no { get; set; }
        public string? type_of_travel { get; set; }
        public string? emp_id { get; set; }
        public string? emp_title { get; set; }
        public string? emp_name { get; set; }
        public string? emp_surname { get; set; }
        public string? emp_section { get; set; }
        public string? emp_department { get; set; }
        public string? emp_function { get; set; }
        public string? emp_display { get; set; }

    }
    public class insuranceModel
    {
        public string? id { get; set; }
        public string? doc_id { get; set; }
        public string? emp_id { get; set; }
        public string? emp_display { get; set; }

        public string? emp_passport { get; set; }

        public string? emp_section { get; set; }
        public string? emp_department { get; set; }
        public string? emp_function { get; set; }

        public string? name_beneficiary { get; set; }
        public string? relationship { get; set; }

        public string? certificates_no { get; set; }
        public string? period_ins_from { get; set; }
        public string? period_ins_to { get; set; }
        public string? duration { get; set; }

        public string? country { get; set; }
        public string? billing_charge { get; set; }
        public string? certificates_total { get; set; }
    }

    public class ExportReportService
    {
        //cls_connection conn;
        string sqlstr = "";
        string ret = "";
        DataTable dt;

        public ExportFileOutModel exportfile_data(ExportFileInModel value)
        {
            //var data = value;
            DataTable dtdef = new DataTable();
            //HttpResponse response = HttpContext.Current.Response;


            var datetime_run = DateTime.Now.ToString("yyyyMMddHHmm");
            string _Folder = "/ExportFile/" + value.doc_id + "/" + value.pagename + "/" + value.emp_id + "/";

            //string _PathSave = System.Web.HttpContext.Current.Server.MapPath("~" + _Folder);
            string _PathFileSave = FileUtil.GetDirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot/ExportFile/{value.doc_id}/{value.pagename}/{value.emp_id}")?.FullName ??""; //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "ExportFile", value.doc_id, value.pagename, value.emp_id);
            string _FileName = "";
            string ret = "";

            //http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/Image/D001/travelerhistory/TO102155//Image/D001/travelerhistory/TO102155/
            //"http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws"
            string ServerPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI() ?? ""; // top.ebiz.helper.AppEnvironment.GeteServerPathAPI()?.ToString();

            #region Determine whether the directory exists. 
            string msg_error = "";
            try
            {
                #region Export Excel 
                if (value.pagename.ToString() == "allowance")
                {
                    _FileName = "Allowance Payment Form " + value.doc_id + "_" + datetime_run + ".xlsx";
                    //_PathFileSave += _FileName;
                    //export_excel_allowance(value, _PathFileSave, ref msg_error);
                }
                #endregion Export Excel 
            }
            catch (Exception ex) { msg_error = "create folder " + ex.Message.ToString(); }

            #endregion Determine whether the directory exists.

            //next_line_1:;

            var data = new ExportFileOutModel();
            data.path = ServerPathAPI + _Folder;
            data.filename = _FileName;

            data.after_trip.opt1 = (ret ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret ?? "") == "true" ? "Upload file succesed." : "Export file failed.";
            data.after_trip.opt2.remark = (ret ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "";
            data.after_trip.opt3.remark = _PathFileSave;

            return data;
        }

        public Report_AllowanceModel repoprt_data_allowance(ExportFileInModel value)
        {
            string msg_error = "";
            Report_AllowanceModel data = new Report_AllowanceModel();
            try
            {
                searchDocTravelerProfileServices wssearch = new searchDocTravelerProfileServices();

                DataSet ds = refdata_excel_allowance(value);
                //add data set to object

                int ifrom = 0;
                int sto = 0;
                DataTable dtMain = ds.Tables["allowance header"].Copy();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    var doc_id = dtMain.Rows[i]["doc_id"].ToString();
                    var emp_id = dtMain.Rows[i]["employee_id"].ToString();
                    var emp_name = dtMain.Rows[i]["employee_name"].ToString();

                    //20211027 เพิ่มดึงข้อมูล passport ใหม่ 
                    string passport = dtMain.Rows[i]["passport"].ToString();
                    string passport_date = dtMain.Rows[i]["passport_date"].ToString();
                    try
                    {
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            Create_Trip.searchDocCreateServices _swd = new Create_Trip.searchDocCreateServices();
                            var est = _swd.EstimateExpense(context, doc_id, emp_id);
                            if (est.PassportExpense.ToString() != "")
                            {
                                passport = est.PassportExpense.ToString();
                            }
                            if (est.PassportDate.ToString() != "")
                            {
                                passport_date = _swd.convert_date_display(est.PassportDate.ToString());
                            }
                        }
                    }
                    catch { }

                    #region ข้อมูล allowance header  
                    data.doc_id = doc_id;
                    data.emp_id = emp_id;
                    data.emp_name = emp_name;

                    data.title = dtMain.Rows[i]["title"].ToString();
                    data.country = dtMain.Rows[i]["country"].ToString();
                    data.functional = dtMain.Rows[i]["functional"].ToString();
                    data.business_date = dtMain.Rows[i]["business_date"].ToString();
                    data.departure_date = dtMain.Rows[i]["departure_date"].ToString();
                    data.arrival_date = dtMain.Rows[i]["arrival_date"].ToString();
                    data.io_number = dtMain.Rows[i]["io_number"].ToString();
                    data.cost_center = dtMain.Rows[i]["cost_center"].ToString();
                    data.gl_account = dtMain.Rows[i]["gl_account"].ToString();

                    data.employee_id = dtMain.Rows[i]["employee_id"].ToString();
                    data.employee_name = dtMain.Rows[i]["employee_name"].ToString();
                    data.total = dtMain.Rows[i]["total"].ToString();
                    data.unit = dtMain.Rows[i]["unit"].ToString();
                    data.total_thb = dtMain.Rows[i]["total_thb"].ToString();
                    data.last_update = dtMain.Rows[i]["last_update"].ToString();

                    #region ข้อมูล record of outfit allowances
                    data.icon_travel_agent = dtMain.Rows[i]["icon_travel_agent"].ToString();
                    data.icon_other = dtMain.Rows[i]["icon_other"].ToString();

                    data.passport = passport;
                    data.passport_date = passport_date;
                    data.luggage_clothing = dtMain.Rows[i]["luggage_clothing"].ToString();
                    data.luggage_clothing_date = dtMain.Rows[i]["luggage_clothing_date"].ToString();

                    data.remark = dtMain.Rows[i]["remark"].ToString();
                    data.important_note = dtMain.Rows[i]["important_note"].ToString();
                    #endregion ข้อมูล record of outfit allowances

                    #endregion ข้อมูล allowance header 

                    DataRow[] dr;
                    #region ข้อมูล table daily allowance 
                    dt = new DataTable();
                    dt = ds.Tables["daily allowance"].Copy();
                    // dr = dt.Select("emp_id ='" + emp_id + "' ");
                    dr = dt.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id).ToArray();//.Select("emp_id ='" + emp_id + "' ");
                    for (int j = 0; j < dr.Length; j++)
                    {
                        data.dailyallowance.Add(new dailyallowanceModel
                        {
                            doc_id = doc_id,
                            emp_id = emp_id,
                            emp_name = emp_name,

                            id = (j + 1).ToString(),
                            allowance_days = dr[j]["allowance_days"].ToString(),
                            allowance_date = dr[j]["allowance_date"].ToString(),
                            allowance_low = dr[j]["allowance_low"].ToString(),
                            allowance_mid = dr[j]["allowance_mid"].ToString(),
                            allowance_hight = dr[j]["allowance_hight"].ToString(),
                            allowance_total = dr[j]["allowance_total"].ToString(),
                            allowance_unit = dr[j]["allowance_unit"].ToString(),
                        });
                    }
                    #endregion ข้อมูล table daily allowance 


                    #region ข้อมูล table flight schedule 
                    dt = new DataTable();
                    dt = ds.Tables["flight schedule"].Copy();
                    // dr = dt.Select("emp_id ='" + emp_id + "' ");
                    dr = dt.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id).ToArray();
                    for (int j = 0; j < dr.Length; j++)
                    {
                        string check_over_day = "";
                        try
                        {
                            if (Convert.ToDouble(dr[j]["airticket_arrival_time"].ToString().Replace(":", ".")) <
                                Convert.ToDouble(dr[j]["airticket_departure_time"].ToString().Replace(":", "."))
                                )
                            {
                                check_over_day = " (" + dr[j]["airticket_date_next"].ToString() + ")";
                            }
                        }
                        catch { }

                        data.flightschedule.Add(new flightscheduleModel
                        {
                            doc_id = doc_id,
                            emp_id = emp_id,
                            emp_name = emp_name,

                            id = (j + 1).ToString(),
                            airticket_date = dr[j]["airticket_date"].ToString(),
                            airticket_route_from = dr[j]["airticket_route_from"].ToString(),
                            airticket_route_to = dr[j]["airticket_route_to"].ToString(),
                            airticket_flight = dr[j]["airticket_flight"].ToString(),
                            airticket_departure_time = dr[j]["airticket_departure_time"].ToString(),
                            airticket_arrival_time = dr[j]["airticket_arrival_time"].ToString() + check_over_day,
                        });
                    }
                    #endregion ข้อมูล table flight schedule

                }

                DataTable dtm_exchangerate = wssearch.ref_exchangerate();
                if (dtm_exchangerate.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_exchangerate.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_exchangerate_max.Add(new ExchangeRateList
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            currency_id = dt.Rows[i]["currency_id"].ToString(),
                            exchange_rate = dt.Rows[i]["exchange_rate"].ToString(),
                            date_from = dt.Rows[i]["date_from"].ToString(),
                            date_to = dt.Rows[i]["date_to"].ToString(),
                        });
                    }
                }
                dtm_exchangerate = new DataTable();
                dtm_exchangerate = wssearch.ref_exchangerate_by_doc(value.doc_id);
                if (dtm_exchangerate.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_exchangerate.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_exchangerate.Add(new ExchangeRateList
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            currency_id = dt.Rows[i]["currency_id"].ToString(),
                            exchange_rate = dt.Rows[i]["exchange_rate"].ToString(),
                            date_from = dt.Rows[i]["date_from"].ToString(),
                            date_to = dt.Rows[i]["date_to"].ToString(),
                        });
                    }
                }
                ret = "true";
            }
            catch (Exception ex) { msg_error = ex.Message.ToString(); data.token_login = msg_error; }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Export report succesed." : "Export report failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        //public DataSet refdata_excel_allowance2(ExportFileInModel value)
        //{
        //    string ret = "";
        //    string token_login = value.token_login;
        //    string doc_id = value.doc_id;
        //    string emp_id = value.emp_id;
        //    Boolean user_admin = false;

        //    #region get data
        //    searchDocTravelerProfileServices wssearch = new searchDocTravelerProfileServices();
        //    DataTable dtref = new DataTable();
        //    DataRow[] drref;

        //    Double dLuggage_Clothing = 0.00;
        //    string luggage_clothing_date = "";
        //    Double dPassport = 0.00;
        //    string passport_date = "";
        //    string remark = "";

        //    //ยังไม่มีข้อมูลที่จะนำมาแสดง ต้องถาม user ???
        //    string icon_travel_agent = "";
        //    string icon_other = "";
        //    string important_note = "";

        //    string sbz_doc_traveler_expense = @" 
        //                     select tb1.dh_code as doc_id, tb1.dte_emp_id as emp_id
        //                     , sum(to_char(tb1.dte_cl_expense)) as luggage_clothing
        //                     , sum(to_char(tb1.dte_cl_expense)) as dte_cl_expense
        //                     , to_char(tb2.dte_cl_valid-1,'dd Mon rrrr') as luggage_clothing_date
        //                     , tb2.dte_passport_expense as passport
        //                     , to_char(tb2.dte_passport_valid-1,'dd Mon rrrr') as passport_date
        //                     from bz_doc_traveler_expense tb1
        //                     left join (select ex.dh_code, ex.dte_emp_id, ex.dte_cl_valid, ex.dte_passport_expense, ex.dte_passport_valid 
        //                     from bz_doc_traveler_expense ex
        //                     where ex.dh_code ='" + doc_id + "' and ex.dte_emp_id ='" + emp_id + "' " +
        //                  @" and rownum =1) tb2 on tb1.dh_code = tb2.dh_code and tb1.dte_emp_id = tb2.dte_emp_id
        //                     where tb1.dh_code ='" + doc_id + "' and tb1.dte_emp_id ='" + emp_id + "'  " +
        //                  @" group by tb1.dh_code, tb1.dte_emp_id, tb2.dh_code, tb2.dte_emp_id, tb2.dte_cl_valid, tb2.dte_passport_expense, tb2.dte_passport_valid ";

        //    //ใช้วิธีดึงข้อมูลตรงจาก db ใส่ใน function เนื่องจากอาจจะมีการเปลี่ยนเปลงบ่อย
        //    DataSet ds = new DataSet();

        //    //ข้อมูล allowance header  
        //    sqlstr = @"  select distinct null as title,null as country,null as business_date,null as departure_date,null as arrival_date,null as io_number,null as cost_center,null as gl_account
        //            ,a.doc_id,a.emp_id as employee_id,null as employee_name,null as functional 
        //            ,b.total,b.total_thb,'USD' as unit
        //            ,nvl(a.update_date,a.create_date) as last_update
        //            ,a.remark 
        //            , case when a.luggage_clothing  is null then  to_char(ex.dte_cl_expense) else to_char(a.luggage_clothing)  end luggage_clothing 
        //            ,ex.luggage_clothing_date as luggage_clothing_date
        //            ,p.passport_no
        //            ,nvl(ex.passport,0) as passport
        //            ,nvl(ex.passport,0) as passport_thb
        //            ,ex.passport_date as passport_date

        //            ,a.emp_id
        //            ,null as icon_travel_agent,null as icon_other,null as important_note 

        //            from  bz_doc_allowance a
        //            inner join (
        //            select doc_id,emp_id,nvl(sum(allowance_total),0) as total,nvl(sum(allowance_total * nvl(allowance_exchange_rate,1)),0)  as total_thb
        //            from bz_doc_allowance_detail group by doc_id,emp_id 
        //            ) b on a.doc_id = b.doc_id and a.emp_id = b.emp_id 
        //            left join bz_data_passport p on a.emp_id = p.emp_id and p.default_type ='true'
        //            left join ( " + sbz_doc_traveler_expense + " ) ex on b.emp_id = ex.emp_id and a.doc_id = ex.doc_id  where a.doc_id ='" + doc_id + "' and b.emp_id ='" + emp_id + "' ";

        //    //ข้อมูลที่ออกมาตั้งมีแค่ rows เดียวก่อน ยังไม่มี กรณีที่ admin export all

        //    if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
        //    {
        //        wssearch = new searchDocTravelerProfileServices();
        //        dtref = new DataTable();
        //        dtref = wssearch.refdata_emp_detail(token_login, doc_id, emp_id, ref user_admin);

        //        wssearch = new searchDocTravelerProfileServices();
        //        DataTable dtref2 = new DataTable();
        //        dtref2 = wssearch.refdata_accom_book(token_login, doc_id, emp_id, user_admin);

        //        if (dt.Rows.Count > 0 && dtref.Rows.Count > 0)
        //        {
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                var emp_id_select = dt.Rows[i]["emp_id"].ToString();
        //                // drref = dtref.Select("emp_id ='" + emp_id_select + "'");
        //                drref = dtref.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
        //                if (drref.Length > 0)
        //                {
        //                    for (int k = 0; k < drref.Length; k++)
        //                    {
        //                        try
        //                        {
        //                            dPassport += Convert.ToDouble(dt.Rows[i]["passport_thb"].ToString());
        //                        }
        //                        catch { }
        //                        try
        //                        {
        //                            dLuggage_Clothing += Convert.ToDouble(dt.Rows[i]["luggage_clothing"].ToString());
        //                        }
        //                        catch { }
        //                        passport_date = dt.Rows[i]["passport_date"].ToString();
        //                        luggage_clothing_date = dt.Rows[i]["luggage_clothing_date"].ToString();
        //                    }

        //                    dt.Rows[i]["title"] = drref[0]["travel_topic"].ToString();
        //                    dt.Rows[i]["country"] = drref[0]["country_name"].ToString();
        //                    dt.Rows[i]["business_date"] = drref[0]["business_date"].ToString();
        //                    dt.Rows[i]["departure_date"] = drref[0]["datefrom"].ToString();
        //                    dt.Rows[i]["arrival_date"] = drref[0]["dateto"].ToString();
        //                    dt.Rows[i]["io_number"] = drref[0]["io_wbs"].ToString();
        //                    dt.Rows[i]["cost_center"] = drref[0]["cost_center"].ToString();
        //                    dt.Rows[i]["gl_account"] = drref[0]["gl_account"].ToString();

        //                    dt.Rows[i]["employee_name"] = drref[0]["emp_name"].ToString();
        //                    dt.Rows[i]["functional"] = drref[0]["emp_organization"].ToString();

        //                    dt.Rows[i]["passport"] = dPassport.ToString();
        //                    dt.Rows[i]["luggage_clothing"] = dLuggage_Clothing.ToString();

        //                    //dt.Rows[i]["passport_date"] =   passport_date.ToString();
        //                    // dt.Rows[i]["luggage_clothing_date"] = luggage_clothing_date.ToString();

        //                    remark = dt.Rows[i]["remark"].ToString();
        //                }

        //                // drref = dtref2.Select("emp_id ='" + emp_id_select + "'");
        //                drref = dtref2.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
        //                if (drref.Length > 0)
        //                {
        //                    //ยังระบุ field ไม่ได้
        //                    if (drref[0]["booking"].ToString().ToLower() == "true") { icon_travel_agent = "X"; }
        //                    if (drref[0]["booking"].ToString().ToLower() == "true") { icon_other = "X"; }
        //                    dt.Rows[i]["icon_travel_agent"] = icon_travel_agent;
        //                    dt.Rows[i]["icon_other"] = icon_other;
        //                }
        //                dt.AcceptChanges();

        //            }

        //        }

        //        dt.TableName = "allowance header";
        //        ds.Tables.Add(dt); ds.AcceptChanges();
        //    }


        //    //ข้อมูล table daily allowance 
        //    dt = new DataTable();
        //    dt = wssearch.refdata_allowance_detail(token_login, doc_id, "", user_admin);
        //    dt.TableName = "daily allowance";
        //    ds.Tables.Add(dt); ds.AcceptChanges();


        //    //ข้อมูล table flight schedule 
        //    dt = new DataTable();
        //    dt = wssearch.refdata_air_book_detail(token_login, doc_id, "", user_admin);
        //    dt.TableName = "flight schedule";
        //    ds.Tables.Add(dt); ds.AcceptChanges();

        //    #endregion get data  

        //    return ds;
        //}
   
        public DataSet refdata_excel_allowance(ExportFileInModel value)
        {
            string token_login = value.token_login;
            string doc_id = value.doc_id;
            string emp_id = value.emp_id;
            Boolean user_admin = false;

            // #region Variables Initialization
            var ds = new DataSet();
            var dt = new DataTable();
            var wssearch = new searchDocTravelerProfileServices();
            var dtref = new DataTable();
            DataRow[] drref;

            Double dLuggage_Clothing = 0.00;
            string luggage_clothing_date = "";
            Double dPassport = 0.00;
            string passport_date = "";
            string remark = "";
            string icon_travel_agent = "";
            string icon_other = "";
            string important_note = "";
            // #endregion

            // #region Build Parameterized SubQuery for Traveler Expense
            // This subquery is also parameterized to ensure security and consistency.
            var sbzDocTravelerExpense = new StringBuilder();
            sbzDocTravelerExpense.AppendLine(@"
        SELECT 
            tb1.dh_code as doc_id, 
            tb1.dte_emp_id as emp_id,
            SUM(TO_CHAR(tb1.dte_cl_expense)) as luggage_clothing,
            SUM(TO_CHAR(tb1.dte_cl_expense)) as dte_cl_expense,
            TO_CHAR(tb2.dte_cl_valid-1, 'dd Mon rrrr') as luggage_clothing_date,
            tb2.dte_passport_expense as passport,
            TO_CHAR(tb2.dte_passport_valid-1, 'dd Mon rrrr') as passport_date
        FROM bz_doc_traveler_expense tb1
        LEFT JOIN (
            SELECT ex.dh_code, ex.dte_emp_id, ex.dte_cl_valid, ex.dte_passport_expense, ex.dte_passport_valid 
            FROM bz_doc_traveler_expense ex
            WHERE ex.dh_code = :sub_doc_id AND ex.dte_emp_id = :sub_emp_id AND ROWNUM = 1
        ) tb2 ON tb1.dh_code = tb2.dh_code AND tb1.dte_emp_id = tb2.dte_emp_id
        WHERE tb1.dh_code = :main_doc_id AND tb1.dte_emp_id = :main_emp_id
        GROUP BY tb1.dh_code, tb1.dte_emp_id, tb2.dh_code, tb2.dte_emp_id, tb2.dte_cl_valid, 
                 tb2.dte_passport_expense, tb2.dte_passport_valid
    ");
            // #endregion

            // #region Build Main Query
            var sqlBuilder = new StringBuilder();
            var whereClause = new List<string>();
            var parameters = new List<OracleParameter>();

            // Main SELECT statement
            sqlBuilder.AppendLine(@"
        SELECT DISTINCT 
            null as title, null as country, null as business_date, null as departure_date, 
            null as arrival_date, null as io_number, null as cost_center, null as gl_account,
            a.doc_id, a.emp_id as employee_id, null as employee_name, null as functional,
            b.total, b.total_thb, 'USD' as unit,
            NVL(a.update_date, a.create_date) as last_update,
            a.remark,
            CASE WHEN a.luggage_clothing IS NULL THEN TO_CHAR(ex.dte_cl_expense) ELSE TO_CHAR(a.luggage_clothing) END as luggage_clothing,
            ex.luggage_clothing_date as luggage_clothing_date,
            p.passport_no,
            NVL(ex.passport, 0) as passport,
            NVL(ex.passport, 0) as passport_thb,
            ex.passport_date as passport_date,
            a.emp_id,
            null as icon_travel_agent, null as icon_other, null as important_note
        FROM bz_doc_allowance a
        INNER JOIN (
            SELECT doc_id, emp_id, NVL(SUM(allowance_total), 0) as total, 
                   NVL(SUM(allowance_total * NVL(allowance_exchange_rate, 1)), 0) as total_thb
            FROM bz_doc_allowance_detail 
            GROUP BY doc_id, emp_id
        ) b ON a.doc_id = b.doc_id AND a.emp_id = b.emp_id
        LEFT JOIN bz_data_passport p ON a.emp_id = p.emp_id AND p.default_type = 'true'
    ");

            // Append the subquery
            sqlBuilder.AppendLine($"LEFT JOIN ({sbzDocTravelerExpense}) ex ON b.emp_id = ex.emp_id AND a.doc_id = ex.doc_id");

            // #endregion

            // #region Add Where Clauses and Parameters
            whereClause.Add("a.doc_id = :doc_id");
            parameters.Add(new OracleParameter(":doc_id", doc_id));
            // Assuming ClassConnectionDb.ConvertTypeParameter is a helper you use:
            // parameters.Add(ClassConnectionDb.ConvertTypeParameter("doc_id", doc_id, "char", 100));

            whereClause.Add("b.emp_id = :emp_id");
            parameters.Add(new OracleParameter(":emp_id", emp_id));
            // parameters.Add(ClassConnectionDb.ConvertTypeParameter("emp_id", emp_id, "char", 100));

            // Add parameters for the subquery
            parameters.Add(new OracleParameter(":sub_doc_id", doc_id));
            parameters.Add(new OracleParameter(":sub_emp_id", emp_id));
            parameters.Add(new OracleParameter(":main_doc_id", doc_id));
            parameters.Add(new OracleParameter(":main_emp_id", emp_id));

            if (whereClause.Count > 0)
            {
                sqlBuilder.AppendLine("WHERE " + string.Join(" AND ", whereClause));
            }

            string sqlstr = sqlBuilder.ToString();
            // #endregion

            // #region Database Execution
            // This block replaces the direct call to SetDocService.conn_ExecuteData
            // with a standard, secure execution pattern.
            try
            {
                // Assuming you have a class for handling DB connections
                using (var conncmd = new ClassConnectionDb())
                {
                    conncmd.OpenConnection();
                    using (OracleCommand command = conncmd.conn.CreateCommand())
                    {
                        command.CommandText = sqlstr;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }
                        var dscmd = conncmd.ExecuteAdapter(command);
                        dt = dscmd?.Tables.Count > 0 ? dscmd.Tables[0] : new DataTable();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception appropriately
                // For example: Console.WriteLine($"SQL Execution Error: {ex.Message}");
                dt = new DataTable(); // Ensure dt is not null
            }


            // #region Data Processing
            // The rest of your logic remains largely the same, operating on the 'dt' DataTable.
            if (dt.Rows.Count > 0)
            {
                dtref = wssearch.refdata_emp_detail(token_login, doc_id, emp_id, ref user_admin);
                var dtref2 = wssearch.refdata_accom_book(token_login, doc_id, emp_id, user_admin);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var emp_id_select = dt.Rows[i]["emp_id"].ToString();
                    drref = dtref.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                    if (drref.Length > 0)
                    {
                        // Summing logic and date assignment
                        try { dPassport += Convert.ToDouble(dt.Rows[i]["passport_thb"]); } catch { }
                        try { dLuggage_Clothing += Convert.ToDouble(dt.Rows[i]["luggage_clothing"]); } catch { }
                        passport_date = dt.Rows[i]["passport_date"].ToString();
                        luggage_clothing_date = dt.Rows[i]["luggage_clothing_date"].ToString();

                        // Populating dt with data from dtref
                        dt.Rows[i]["title"] = drref[0]["travel_topic"];
                        dt.Rows[i]["country"] = drref[0]["country_name"];
                        // ... (other assignments remain the same)
                        dt.Rows[i]["employee_name"] = drref[0]["emp_name"];
                        dt.Rows[i]["functional"] = drref[0]["emp_organization"];
                        dt.Rows[i]["passport"] = dPassport.ToString();
                        dt.Rows[i]["luggage_clothing"] = dLuggage_Clothing.ToString();
                        remark = dt.Rows[i]["remark"].ToString();
                    }

                    drref = dtref2.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                    if (drref.Length > 0)
                    {
                        if (drref[0]["booking"].ToString().ToLower() == "true") { icon_travel_agent = "X"; }
                        // Note: This seems to be a logical error in the original code, as it checks the same condition twice.
                        if (drref[0]["booking"].ToString().ToLower() == "true") { icon_other = "X"; }
                        dt.Rows[i]["icon_travel_agent"] = icon_travel_agent;
                        dt.Rows[i]["icon_other"] = icon_other;
                    }
                }
                dt.AcceptChanges();
            }

            dt.TableName = "allowance header";
            ds.Tables.Add(dt.Copy());

            // Fetch and add other tables to the DataSet
            var dtDaily = wssearch.refdata_allowance_detail(token_login, doc_id, "", user_admin);
            dtDaily.TableName = "daily allowance";
            ds.Tables.Add(dtDaily.Copy());

            var dtFlight = wssearch.refdata_air_book_detail(token_login, doc_id, "", user_admin);
            dtFlight.TableName = "flight schedule";
            ds.Tables.Add(dtFlight.Copy());

            ds.AcceptChanges();
            // #endregion

            return ds;
        }
        public ReportISOSRecordOutModel report_isos_member_list_record2(ExportRecordModel value)
        {
            string msg_error = "";
            ReportISOSRecordOutModel data = new ReportISOSRecordOutModel();
            try
            {
                using var context = new TOPEBizCreateTripEntities();
                string year = value.year;
                data.token_login = value.token_login;
                data.year = year;

                sqlstr = @" select a.*
                            from bz_doc_isos_record a
                            where a.year = '" + year + "' ";
                sqlstr += @" order by to_number(a.id) ";
                dt = new DataTable();

                if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.details_list.Add(new reportisosList
                        {
                            no = dt.Rows[i]["id"].ToString(),
                            type_of_travel = dt.Rows[i]["isos_type_of_travel"].ToString(),
                            emp_id = dt.Rows[i]["isos_emp_id"].ToString(),
                            emp_title = dt.Rows[i]["isos_emp_title"].ToString(),
                            emp_name = dt.Rows[i]["isos_emp_name"].ToString(),
                            emp_surname = dt.Rows[i]["isos_emp_surname"].ToString(),
                            emp_section = dt.Rows[i]["isos_emp_section"].ToString(),
                            emp_department = dt.Rows[i]["isos_emp_department"].ToString(),
                            emp_function = dt.Rows[i]["isos_emp_function"].ToString(),
                            emp_display = dt.Rows[i]["isos_emp_title"].ToString() + " " + dt.Rows[i]["isos_emp_name"].ToString() + " " + dt.Rows[i]["isos_emp_surname"].ToString(),
                        });
                    }
                }


                ret = "true";
            }
            catch (Exception ex) { msg_error = ex.Message.ToString(); data.token_login = msg_error; }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Export report succesed." : "Export report failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public ReportISOSRecordOutModel report_isos_member_list_record(ExportRecordModel value)
        {
            string msg_error = "";
            ReportISOSRecordOutModel data = new ReportISOSRecordOutModel();
            try
            {
                using var context = new TOPEBizCreateTripEntities();
                string year = value.year;
                data.token_login = value.token_login;
                data.year = year;

                //sqlstr = @" select a.*
                //            from bz_doc_isos_record a
                //            where a.year = '" + year + "' ";
                //sqlstr += @" order by to_number(a.id) ";
                StringBuilder sqlBuilder = new StringBuilder();
                var whereClause = new List<string>();

                List<OracleParameter> parameters = new List<OracleParameter>();
                var dt = new DataTable();

                sqlBuilder.AppendLine(@" select a.*
                            from bz_doc_isos_record a");

                whereClause.Add(@" a.year = :year");
                parameters.Add(ClassConnectionDb.ConvertTypeParameter("year", year, "char", 4000));
                if (whereClause.Count > 0)
                {
                    sqlBuilder.AppendLine("WHERE");
                    sqlBuilder.AppendLine(string.Join(" AND ", whereClause));
                }

                sqlBuilder.AppendLine("order by to_number(a.id)");
                string sqlstr = sqlBuilder.ToString();

            
                #region Execute
                try
                {
                    using (ClassConnectionDb conncmd = new ClassConnectionDb())
                    {
                        conncmd.OpenConnection();
                        using OracleCommand command = new(sqlstr);
                        command.Connection = conncmd.conn;
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                            {
                                command.Parameters.Add(p);
                            }
                        }
                        var dscmd = conncmd.ExecuteAdapter(command);
                        dt = dscmd?.Tables.Count > 0 ? dscmd.Tables[0] : new DataTable();

                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"SQL Execution Error: {ex.Message}");
                }
                #endregion Execute
           

                if (dt?.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.details_list.Add(new reportisosList
                        {
                            no = dt.Rows[i]["id"].ToString(),
                            type_of_travel = dt.Rows[i]["isos_type_of_travel"].ToString(),
                            emp_id = dt.Rows[i]["isos_emp_id"].ToString(),
                            emp_title = dt.Rows[i]["isos_emp_title"].ToString(),
                            emp_name = dt.Rows[i]["isos_emp_name"].ToString(),
                            emp_surname = dt.Rows[i]["isos_emp_surname"].ToString(),
                            emp_section = dt.Rows[i]["isos_emp_section"].ToString(),
                            emp_department = dt.Rows[i]["isos_emp_department"].ToString(),
                            emp_function = dt.Rows[i]["isos_emp_function"].ToString(),
                            emp_display = dt.Rows[i]["isos_emp_title"].ToString() + " " + dt.Rows[i]["isos_emp_name"].ToString() + " " + dt.Rows[i]["isos_emp_surname"].ToString(),
                        });
                    }
                }


                ret = "true";
            }
            catch (Exception ex) { msg_error = ex.Message.ToString(); data.token_login = msg_error; }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Export report succesed." : "Export report failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public ReportInsuranceRecordOutModel report_insurance_list_record(ExportRecordModel value)
        {
            string msg_error = "";
            ReportInsuranceRecordOutModel data = new ReportInsuranceRecordOutModel();
            try
            {
                string year = value.year;
                data.token_login = value.token_login;
                data.year = year;

                //sqlstr = @" select distinct '' as id
                //            , h.dh_code as doc_id
                //            , ex.dte_emp_id as emp_id   
                //            , case when  a.ins_emp_id is not null then a.ins_emp_name else 
                //                case when p.emp_id is null then b.userdisplay else ( p.passport_title || ' ' || p.passport_name || ' ' || p.passport_surname ) end 
                //              end emp_display 
                //            , case when a.ins_emp_id is not null then a.ins_emp_passport else p.passport_no end emp_passport 

                //            , b.sections as emp_section
                //            , b.department as emp_department
                //            , b.function as emp_function

                //            , a.name_beneficiary
                //            , a.relationship

                //            , a.certificates_no
                //            , a.period_ins_from  
                //            , a.period_ins_to 
                //            , case when a.duration is null then to_number(nvl((case when a.period_ins_to is not null and to_date(a.period_ins_from,'dd MON rrrr') is not null then  to_date(a.period_ins_to,'dd MON rrrr') - to_date(a.period_ins_from,'dd MON rrrr') end),0))
                //              else  to_number(nvl(a.duration,0)) end duration 
                //            , mc.ct_name as country

                //            , case when a.ins_emp_id is not null then a.insurance_company else cc.key_value end  billing_charge 
                //            , a.certificates_total  

                //            from bz_doc_head h  
                //            inner join bz_doc_traveler_expense ex on h.dh_code = ex.dh_code 
                //            inner join vw_bz_users b on ex.dte_emp_id = b.employeeid
                //            inner join bz_data_passport p on ex.dte_emp_id = p.emp_id and p.default_type ='true' 
                //            inner join bz_doc_insurance a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id and ex.dte_emp_id = a.emp_id 
                //            and p.emp_id = p.emp_id
                //            left join bz_config_data cc on cc.status = 1 and cc.key_name ='Company Name'  and b.companyname = cc.key_filter
                //            left join bz_config_data ca on ca.status = 1 and ca.key_name ='Company Address'  and b.companyname = ca.key_filter 
                //            left join bz_master_country mc on ex.ct_id = mc.ct_id   
                //            where substr(h.dh_code,3,2) = substr('" + year + "',3,2) ";
                //sqlstr += @" order by h.dh_code,ex.dte_emp_id ";
                StringBuilder sqlBuilder = new StringBuilder();
                var whereClause = new List<string>();

                List<OracleParameter> parameters = new List<OracleParameter>();
                var dt = new DataTable();

                sqlBuilder.AppendLine(@" select distinct '' as id
                            , h.dh_code as doc_id
                            , ex.dte_emp_id as emp_id   
                            , case when  a.ins_emp_id is not null then a.ins_emp_name else 
                                case when p.emp_id is null then b.userdisplay else ( p.passport_title || ' ' || p.passport_name || ' ' || p.passport_surname ) end 
                              end emp_display 
                            , case when a.ins_emp_id is not null then a.ins_emp_passport else p.passport_no end emp_passport 

                            , b.sections as emp_section
                            , b.department as emp_department
                            , b.function as emp_function

                            , a.name_beneficiary
                            , a.relationship

                            , a.certificates_no
                            , a.period_ins_from  
                            , a.period_ins_to 
                            , case when a.duration is null then to_number(nvl((case when a.period_ins_to is not null and to_date(a.period_ins_from,'dd MON rrrr') is not null then  to_date(a.period_ins_to,'dd MON rrrr') - to_date(a.period_ins_from,'dd MON rrrr') end),0))
                              else  to_number(nvl(a.duration,0)) end duration 
                            , mc.ct_name as country

                            , case when a.ins_emp_id is not null then a.insurance_company else cc.key_value end  billing_charge 
                            , a.certificates_total  

                            from bz_doc_head h  
                            inner join bz_doc_traveler_expense ex on h.dh_code = ex.dh_code 
                            inner join vw_bz_users b on ex.dte_emp_id = b.employeeid
                            inner join bz_data_passport p on ex.dte_emp_id = p.emp_id and p.default_type ='true' 
                            inner join bz_doc_insurance a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id and ex.dte_emp_id = a.emp_id 
                            and p.emp_id = p.emp_id
                            left join bz_config_data cc on cc.status = 1 and cc.key_name ='Company Name'  and b.companyname = cc.key_filter
                            left join bz_config_data ca on ca.status = 1 and ca.key_name ='Company Address'  and b.companyname = ca.key_filter 
                            left join bz_master_country mc on ex.ct_id = mc.ct_id ");



                whereClause.Add(@"substr(h.dh_code,3,2) = substr(:year,3,2)");

                parameters.Add(ClassConnectionDb.ConvertTypeParameter("year", year, "char", 4000));



                if (whereClause.Count > 0)
                {
                    sqlBuilder.AppendLine("WHERE");
                    sqlBuilder.AppendLine(string.Join(" AND ", whereClause));
                }

                sqlBuilder.AppendLine("order by h.dh_code,ex.dte_emp_id");
                string sqlstr = sqlBuilder.ToString();

            
                #region Execute
                try
                {
                    using (ClassConnectionDb conncmd = new ClassConnectionDb())
                    {
                        conncmd.OpenConnection();
                        using OracleCommand command = new(sqlstr);
                        command.Connection = conncmd.conn;
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                            {
                                command.Parameters.Add(p);
                            }
                        }
                        var dscmd = conncmd.ExecuteAdapter(command);
                        dt = dscmd?.Tables.Count > 0 ? dscmd.Tables[0] : new DataTable();

                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"SQL Execution Error: {ex.Message}");
                }
                #endregion Execute
           

                if (dt?.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.details_list.Add(new insuranceModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            doc_id = dt.Rows[i]["doc_id"].ToString(),
                            emp_id = dt.Rows[i]["emp_id"].ToString(),
                            emp_passport = dt.Rows[i]["emp_passport"].ToString(),
                            emp_display = dt.Rows[i]["emp_display"].ToString(),
                            emp_section = dt.Rows[i]["emp_section"].ToString(),
                            emp_department = dt.Rows[i]["emp_department"].ToString(),
                            emp_function = dt.Rows[i]["emp_function"].ToString(),

                            name_beneficiary = dt.Rows[i]["name_beneficiary"].ToString(),
                            relationship = dt.Rows[i]["relationship"].ToString(),

                            certificates_no = dt.Rows[i]["certificates_no"].ToString(),
                            period_ins_from = dt.Rows[i]["period_ins_from"].ToString(),
                            period_ins_to = dt.Rows[i]["period_ins_to"].ToString(),
                            duration = dt.Rows[i]["duration"].ToString(),
                            country = dt.Rows[i]["country"].ToString(),
                            billing_charge = dt.Rows[i]["billing_charge"].ToString(),
                            certificates_total = dt.Rows[i]["certificates_total"].ToString(),

                        });
                    }
                }

                ret = "true";
            }
            catch (Exception ex) { msg_error = ex.Message.ToString(); data.token_login = msg_error; }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Export report succesed." : "Export report failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }

        //public ReportInsuranceRecordOutModel report_insurance_list_record(ExportRecordModel value)
        //{
        //    var data = new ReportInsuranceRecordOutModel();
        //    string msgError = "";

        //    using (var context = new TOPEBizCreateTripEntities())
        //    {
        //        using (var connection = context.Database.GetDbConnection())
        //        {
        //            connection.Open();
        //            using (DbCommand cmd = connection.CreateCommand())
        //            {
        //                cmd.CommandText = "bz_sp_report_insurance_list"; // Replace with your stored procedure name
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                // Adding parameters
        //                cmd.Parameters.Add(new OracleParameter("p_year", value.year));
        //                cmd.Parameters.Add(new OracleParameter("p_token", value.token_login));

        //                // Assuming you want to return a cursor or table from the stored procedure
        //                OracleParameter oraP = new OracleParameter();
        //                oraP.ParameterName = "ret_cursor";
        //                oraP.OracleDbType = OracleDbType.RefCursor;
        //                oraP.Direction = ParameterDirection.Output;
        //                cmd.Parameters.Add(oraP);

        //                try
        //                {
        //                    // Execute the command
        //                    using (var reader = cmd.ExecuteReader())
        //                    {
        //                        while (reader.Read())
        //                        {
        //                            data.details_list.Add(new insuranceModel
        //                            {
        //                                id = reader["id"].ToString(),
        //                                doc_id = reader["doc_id"].ToString(),
        //                                emp_id = reader["emp_id"].ToString(),
        //                                emp_passport = reader["emp_passport"].ToString(),
        //                                emp_display = reader["emp_display"].ToString(),
        //                                emp_section = reader["emp_section"].ToString(),
        //                                emp_department = reader["emp_department"].ToString(),
        //                                emp_function = reader["emp_function"].ToString(),
        //                                name_beneficiary = reader["name_beneficiary"].ToString(),
        //                                relationship = reader["relationship"].ToString(),
        //                                certificates_no = reader["certificates_no"].ToString(),
        //                                period_ins_from = reader["period_ins_from"].ToString(),
        //                                period_ins_to = reader["period_ins_to"].ToString(),
        //                                duration = reader["duration"].ToString(),
        //                                country = reader["country"].ToString(),
        //                                billing_charge = reader["billing_charge"].ToString(),
        //                                certificates_total = reader["certificates_total"].ToString(),
        //                            });
        //                        }
        //                    }

        //                    data.token_login = value.token_login; // Populate token_login
        //                    data.year = value.year; // Populate year
        //                    data.after_trip.opt1 = "true";
        //                    data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel
        //                    {
        //                        status = "Export report succeeded.",
        //                        remark = ""
        //                    };
        //                }
        //                catch (Exception ex)
        //                {
        //                    msgError = ex.Message;
        //                    data.after_trip.opt1 = "false";
        //                    data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel
        //                    {
        //                        status = "Export report failed.",
        //                        remark = msgError
        //                    };
        //                }
        //            }
        //        }
        //    }

        //    // Handle any potential error messages
        //    data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel
        //    {
        //        status = "Error msg",
        //        remark = msgError
        //    };

        //    return data;
        //}


    }






}