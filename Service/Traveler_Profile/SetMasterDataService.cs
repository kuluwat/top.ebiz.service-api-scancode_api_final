using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
//using Oracle.ManagedDataAccess.Client;
//using System.Data.Entity;

//using System.Data.OracleClient;
//using Newtonsoft.Json;
using System.Text.Json;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;
//using System.IO;
using top.ebiz.service.Models.Traveler_Profile;
using Oracle.ManagedDataAccess.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using System.Drawing;
using System.Text;

namespace top.ebiz.service.Service.Traveler_Profile
{

    public class SetMasterDataService
    {

        SetDocService sw;
        //cls_connection conn;
        ClassConnectionDb conn;
        string sqlstr = "";
        string sqlstr_all = "";
        string ret = "";
        DataTable dt;
        DataTable dtdata;
        public ReimbursementOutModel SetReimbursementExchageRate(ReimbursementOutModel value)
        {
            //ยังไม่ได้เขียนเพิ่ม ??? ตอนนี้ให้ส่งข้อมูลเข้ามาเพื่อ insert/update อย่างเดียวก่อน

            var doc_type = value.data_type;
            var data = value;
            var data_def = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_FX_TYPE_M); ;



            if (data.data_type.ToString() != "")
            {
                sqlstr_all = "";
                using var context = new TOPEBizCreateTripEntities();
                using var transaction = context.Database.BeginTransaction();
                if (data.m_exchangerate.Count > 0)
                {

                    List<ExchangeRateList> dtlist = data.m_exchangerate;
                    for (int i = 0; i < dtlist.Count; i++)
                    {
                        ret = "true"; sqlstr = "";
                        var action_type = dtlist[i].action_type.ToString();
                        if (action_type == "") { continue; }
                        else if (action_type != "delete")
                        {
                            var action_change = dtlist[i].action_change + "";
                            if (action_change.ToLower() != "true") { continue; }
                        }

                        if (action_type == "insert")
                        {
                            sqlstr = @"INSERT INTO BZ_DATA_FX_TYPE_M
                              (ID, T_FXB_CUR, T_FXB_VALUE1, T_FXB_VALDATE, STATUS_ACTIVE, REMARK, 
                               DATA_SOURCE_CPAI, CREATE_BY, CREATE_DATE, TOKEN_UPDAT) 
                              VALUES (:ID, :T_FXB_CUR, :T_FXB_VALUE1, :T_FXB_VALDATE, :STATUS_ACTIVE, 
                                      :REMARK, :DATA_SOURCE_CPAI, :CREATE_BY, SYSDATE, :TOKEN_UPDAT)";

                            parameters.Add(new OracleParameter("ID", imaxid));
                            parameters.Add(new OracleParameter("T_FXB_CUR", dtlist[i].currency_id));
                            parameters.Add(new OracleParameter("T_FXB_VALUE1", dtlist[i].exchange_rate));
                            parameters.Add(new OracleParameter("T_FXB_VALDATE", dtlist[i].date_from));
                            parameters.Add(new OracleParameter("STATUS_ACTIVE", "1"));
                            parameters.Add(new OracleParameter("REMARK", ""));
                            parameters.Add(new OracleParameter("DATA_SOURCE_CPAI", "0"));
                            parameters.Add(new OracleParameter("CREATE_BY", emp_user_active));
                            parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));

                            imaxid++;
                        }
                        else if (action_type == "update")
                        {
                            sqlstr = @"UPDATE BZ_DATA_FX_TYPE_M 
                             SET T_FXB_CUR = :T_FXB_CUR,
                                 T_FXB_VALUE1 = :T_FXB_VALUE1,
                                 T_FXB_VALDATE = :T_FXB_VALDATE,
                                 UPDATE_BY = :UPDATE_BY,
                                 UPDATE_DATE = SYSDATE,
                                 TOKEN_UPDATE = :TOKEN_UPDATE
                             WHERE ID = :ID";
                            parameters.Add(new OracleParameter("T_FXB_CUR", dtlist[i].currency_id));
                            parameters.Add(new OracleParameter("T_FXB_VALUE1", dtlist[i].exchange_rate));
                            parameters.Add(new OracleParameter("T_FXB_VALDATE", dtlist[i].date_from));
                            parameters.Add(new OracleParameter("UPDATE_BY", emp_user_active));
                            parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));
                            parameters.Add(new OracleParameter("ID", dtlist[i].id));
                        }
                        else if (action_type == "delete")
                        {
                            sqlstr = @"DELETE FROM BZ_DATA_FX_TYPE_M WHERE ID = :ID";
                            parameters.Add(new OracleParameter("ID", dtlist[i].id));
                        }

                        ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";

                        if (ret.ToLower() != "true") { context.Database.RollbackTransaction(); goto Next_line_1; }

                    }
                }

            Next_line_1:;
                if (ret.ToLower() == "true")
                {
                    context.Database.CommitTransaction();
                    // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;

                //กรณีที่มี error ให้คืนค่า id ของ exchange rate 
                data = data_def;
            }
            else
            {
                List<ExchangeRateList> dtlist = data.m_exchangerate;
                for (int i = 0; i < dtlist.Count; i++)
                {
                    dtlist[i].action_change = "false";
                    dtlist[i].action_type = "update";
                }
                data.m_exchangerate = dtlist;
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public MMaintainDataModel SetAirticketType(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            //sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_ALREADY_BOOKED_TYPE); ;


            if (data.data_type.ToString() != "")
            {
                sqlstr_all = "";
                using var context = new TOPEBizCreateTripEntities();
                using var transaction = context.Database.BeginTransaction();
                try
                {



                    if (data.airticket_type.Count > 0)
                    {
                        List<MasterNormalModel> dtlist = data.airticket_type;
                        for (int i = 0; i < dtlist.Count; i++)
                        {
                            ret = "true"; sqlstr = "";
                            var action_type = dtlist[i].action_type.ToString();
                            if (action_type == "") { continue; }
                            else if (action_type != "delete")
                            {
                                var action_change = dtlist[i].action_change + "";
                                if (action_change.ToLower() != "true") { continue; }
                            }


                            if (action_type == "insert")
                            {
                                //sqlstr = @" insert into  BZ_MASTER_ALREADY_BOOKED_TYPE
                                //        (ID,NAME,STATUS,SORT_BY,CREATE_BY,CREATE_DATE,TOKEN_UPDATE) values ( ";

                                //sqlstr += @" " + imaxid;
                                //sqlstr += @" ," + conn.ChkSqlStr(dtlist[i].name, 300);
                                //sqlstr += @" ," + conn.ChkSqlStr(dtlist[i].status, 300);
                                //sqlstr += @" ," + conn.ChkSqlStr(dtlist[i].sort_by, 300);
                                //sqlstr += @" ," + conn.ChkSqlStr(emp_user_active, 300);//user name login
                                //sqlstr += @" ,sysdate";
                                //sqlstr += @" ," + conn.ChkSqlStr(token_login, 300);
                                //sqlstr += @" )";

                                sqlstr = @"INSERT INTO BZ_MASTER_ALREADY_BOOKED_TYPE
                                (ID, NAME, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                                VALUES (:ID, :NAME, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                parameters.Add(new OracleParameter("ID", imaxid));
                                parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                parameters.Add(new OracleParameter("CREATE_BY", emp_user_active));
                                parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));

                                imaxid++;
                            }
                            else if (action_type == "update")
                            {
                                //sqlstr = @" update BZ_MASTER_ALREADY_BOOKED_TYPE set";

                                //sqlstr += @" NAME = " + conn.ChkSqlStr(dtlist[i].name, 300);
                                //sqlstr += @" ,STATUS = " + conn.ChkSqlStr(dtlist[i].status, 300);
                                //sqlstr += @" ,SORT_BY = " + conn.ChkSqlStr(dtlist[i].sort_by, 300);

                                //sqlstr += @" ,UPDATE_BY = " + conn.ChkSqlStr(emp_user_active, 300);//user name login
                                //sqlstr += @" ,UPDATE_DATE = sysdate";
                                //sqlstr += @" ,TOKEN_UPDATE = " + conn.ChkSqlStr(token_login, 300);
                                //sqlstr += @" where ";
                                //sqlstr += @" ID = " + conn.ChkSqlStr(dtlist[i].id, 300);

                                sqlstr = @"UPDATE BZ_MASTER_ALREADY_BOOKED_TYPE
                               SET NAME = :NAME, STATUS = :STATUS, SORT_BY = :SORT_BY, 
                                   UPDATE_BY = :UPDATE_BY, UPDATE_DATE = SYSDATE, TOKEN_UPDATE = :TOKEN_UPDATE
                               WHERE ID = :ID";


                                parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                parameters.Add(new OracleParameter("CREATE_BY", emp_user_active));
                                parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));
                                parameters.Add(new OracleParameter("ID", dtlist[i].id));
                            }
                            else if (action_type == "delete")
                            {
                                //sqlstr = @" delete from BZ_MASTER_ALREADY_BOOKED_TYPE ";
                                //sqlstr += @" where ";
                                //sqlstr += @" ID = " + .ChkSqlStr(dtlist[i].id, 300);

                                sqlstr = @"DELETE FROM BZ_MASTER_ALREADY_BOOKED_TYPE WHERE ID = :ID";

                                parameters.Add(new OracleParameter("ID", dtlist[i].id));
                            }
                            ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";

                            if (ret.ToLower() != "true") { context.Database.RollbackTransaction(); goto Next_line_1; }

                        }
                    }
                }
                catch
                {
                    context.Database.RollbackTransaction();
                }
                finally
                {

                }
            Next_line_1:;
                if (ret.ToLower() == "true")
                {
                    context.Database.CommitTransaction();
                    // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchAirticketType(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public MMaintainDataModel SetAlreadyBooked(MMaintainDataModel value)
        {

            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_ALREADY_BOOKED_TYPE); ;



            if (data.data_type.ToString() != "")
            {
                sqlstr_all = "";
                if (data.already_booked.Count > 0)
                {
                    using var context = new TOPEBizCreateTripEntities();
                    using var transaction = context.Database.BeginTransaction();
                    List<MasterNormalModel> dtlist = data.already_booked;

                    try
                    {

                        for (int i = 0; i < dtlist.Count; i++)
                        {
                            ret = "true"; sqlstr = "";
                            parameters.Clear();
                            var action_type = dtlist[i].action_type.ToString();
                            if (action_type == "") { continue; }
                            else if (action_type != "delete")
                            {
                                var action_change = dtlist[i].action_change + "";
                                if (action_change.ToLower() != "true") { continue; }
                            }

                            if (action_type == "insert")
                            {


                                sqlstr = @"INSERT INTO BZ_MASTER_ALREADY_BOOKED_TYPE
                                (ID, NAME, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                                VALUES (:ID, :NAME, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                parameters.Add(new OracleParameter("ID", imaxid));
                                parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                parameters.Add(new OracleParameter("CREATE_BY", emp_user_active));
                                parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));

                                imaxid++;
                            }
                            else if (action_type == "update")
                            {

                                sqlstr = @"UPDATE BZ_MASTER_ALREADY_BOOKED_TYPE
                               SET NAME = :NAME, STATUS = :STATUS, SORT_BY = :SORT_BY, 
                                   UPDATE_BY = :UPDATE_BY, UPDATE_DATE = SYSDATE, TOKEN_UPDATE = :TOKEN_UPDATE
                               WHERE ID = :ID";


                                parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                parameters.Add(new OracleParameter("UPDATE_BY", emp_user_active));
                                parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));
                                parameters.Add(new OracleParameter("ID", dtlist[i].id));

                            }
                            else if (action_type == "delete")
                            {

                                sqlstr = @"DELETE FROM BZ_MASTER_ALREADY_BOOKED_TYPE WHERE ID = :ID";

                                parameters.Add(new OracleParameter("ID", dtlist[i].id));
                            }
                            ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";

                            if (ret.ToLower() != "true") { goto Next_line_1; }

                        }
                        transaction.Commit();
                    }
                    catch (System.Exception e)
                    {
                        transaction.Rollback();
                        ret = e.Message;
                    }
                }

            Next_line_1:;
                if (ret.ToLower() == "true")
                {
                    // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchAlreadyBooked(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public MMaintainDataModel SetListStatus(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_LIST_STATUS); ;


            if (data.data_type.ToString() != "")
            {
                sqlstr_all = "";

                if (data.list_status.Count > 0)
                {
                    using var context = new TOPEBizCreateTripEntities();
                    using var transaction = context.Database.BeginTransaction();
                    List<MasterNormalModel> dtlist = data.list_status;
                    try
                    {
                        for (int i = 0; i < dtlist.Count; i++)
                        {
                            ret = "true"; sqlstr = "";
                            parameters.Clear();
                            var action_type = dtlist[i].action_type.ToString();
                            if (action_type == "") { continue; }
                            else if (action_type != "delete")
                            {
                                var action_change = dtlist[i].action_change + "";
                                if (action_change.ToLower() != "true") { continue; }
                            }

                            // dtlist[i].page_name = "all";

                            if (action_type == "insert")
                            {
                                sqlstr = @"INSERT INTO BZ_MASTER_LIST_STATUS
                                (ID, NAME, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE,PAGE_NAME) 
                                VALUES (:ID, :NAME, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE,:PAGE_NAME)";

                                parameters.Add(new OracleParameter("ID", imaxid));
                                parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                parameters.Add(new OracleParameter("CREATE_BY", emp_user_active));
                                parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));
                                parameters.Add(new OracleParameter("PAGE_NAME", dtlist[i].page_name));

                                imaxid++;
                            }
                            else if (action_type == "update")
                            {
                                sqlstr = @"UPDATE BZ_MASTER_LIST_STATUS
                               SET NAME = :NAME, STATUS = :STATUS, SORT_BY = :SORT_BY, 
                                   UPDATE_BY = :UPDATE_BY, UPDATE_DATE = SYSDATE, TOKEN_UPDATE = :TOKEN_UPDATE
                               WHERE ID = :ID AND PAGE_NAME = :PAGE_NAME";


                                parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                parameters.Add(new OracleParameter("UPDATE_BY", emp_user_active));
                                parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));
                                parameters.Add(new OracleParameter("ID", dtlist[i].id));
                                parameters.Add(new OracleParameter("PAGE_NAME", dtlist[i].page_name));

                            }
                            else if (action_type == "delete")
                            {
                                sqlstr = @"DELETE FROM BZ_MASTER_LIST_STATUS WHERE ID = :ID AND PAGE_NAME = :PAGE_NAME";

                                parameters.Add(new OracleParameter("ID", dtlist[i].id));
                                parameters.Add(new OracleParameter("PAGE_NAME", dtlist[i].page_name));
                            }
                            ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";

                            if (ret.ToLower() != "true") { goto Next_line_1; }

                        }
                        transaction.Commit();
                    }
                    catch (System.Exception e)
                    {
                        transaction.Rollback();
                        ret = e.Message;
                    }
                }

            Next_line_1:;
                if (ret.ToLower() == "true")
                {
                    // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;
                data = new MMaintainDataModel();
                data = swd.SearchListStatus(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public MMaintainDataModel SetAllowanceType(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var sql = "";
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();

            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_ALLOWANCE_TYPE); ;


            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.allowance_type.Count > 0)
                            {
                                List<MasterNormalModel> dtlist = data.allowance_type;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    ret = "true"; sqlstr = "";
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }

                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
                INSERT INTO BZ_MASTER_ALLOWANCE_TYPE 
                (ID, NAME, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                VALUES 
                (:ID, :NAME, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                        parameters.Add(new OracleParameter(":ID", imaxid));
                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":CREATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));

                                        imaxid++;

                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
                UPDATE BZ_MASTER_ALLOWANCE_TYPE 
                SET NAME = :NAME, 
                    STATUS = :STATUS, 
                    SORT_BY = :SORT_BY, 
                    UPDATE_BY = :UPDATE_BY, 
                    UPDATE_DATE = SYSDATE, 
                    TOKEN_UPDATE = :TOKEN_UPDATE 
                WHERE ID = :ID";

                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":UPDATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));

                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"DELETE FROM BZ_MASTER_ALLOWANCE_TYPE WHERE ID = :ID";
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));
                                    }
                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";

                                    if (ret.ToLower() != "true") { goto Next_line_1; }
                                }
                                transaction.Commit();
                            }

                        Next_line_1:;
                            if (ret.ToLower() == "true")
                            {
                                // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ret = ex.Message;
                    }

                    finally
                    {
                    }
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchAllowanceType(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }


        public MMaintainDataModel SetFeedbackType(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var sql = "";
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_FEEDBACK_TYPE); ;


            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.feedback_type.Count > 0)
                            {
                                List<MasterNormalModel> dtlist = data.feedback_type;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    ret = "true"; sqlstr = "";
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }


                                    //if (action_type == "insert")
                                    //{
                                    //    sqlstr = @" insert into  BZ_MASTER_FEEDBACK_TYPE
                                    //(ID,NAME,STATUS,SORT_BY,CREATE_BY,CREATE_DATE,TOKEN_UPDATE) values ( ";

                                    //    sqlstr += @" " + imaxid;
                                    //    sqlstr += @" ," + conn.ChkSqlStr(dtlist[i].name, 300);
                                    //    sqlstr += @" ," + conn.ChkSqlStr(dtlist[i].status, 300);
                                    //    sqlstr += @" ," + conn.ChkSqlStr(dtlist[i].sort_by, 300);
                                    //    sqlstr += @" ," + conn.ChkSqlStr(emp_user_active, 300);//user name login
                                    //    sqlstr += @" ,sysdate";
                                    //    sqlstr += @" ," + conn.ChkSqlStr(token_login, 300);
                                    //    sqlstr += @" )";

                                    //    imaxid++;
                                    //}
                                    //else if (action_type == "update")
                                    //{
                                    //    sqlstr = @" update BZ_MASTER_FEEDBACK_TYPE set";

                                    //    sqlstr += @" NAME = " + conn.ChkSqlStr(dtlist[i].name, 300);
                                    //    sqlstr += @" ,STATUS = " + conn.ChkSqlStr(dtlist[i].status, 300);
                                    //    sqlstr += @" ,SORT_BY = " + conn.ChkSqlStr(dtlist[i].sort_by, 300);

                                    //    sqlstr += @" ,UPDATE_BY = " + conn.ChkSqlStr(emp_user_active, 300);//user name login
                                    //    sqlstr += @" ,UPDATE_DATE = sysdate";
                                    //    sqlstr += @" ,TOKEN_UPDATE = " + conn.ChkSqlStr(token_login, 300);
                                    //    sqlstr += @" where ";
                                    //    sqlstr += @" ID = " + conn.ChkSqlStr(dtlist[i].id, 300);
                                    //}
                                    //else if (action_type == "delete")
                                    //{
                                    //    sqlstr = @" delete from BZ_MASTER_FEEDBACK_TYPE ";
                                    //    sqlstr += @" where ";
                                    //    sqlstr += @" ID = " + conn.ChkSqlStr(dtlist[i].id, 300);

                                    //}
                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
                INSERT INTO BZ_MASTER_FEEDBACK_TYPE 
                (ID, NAME, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                VALUES 
                (:ID, :NAME, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                        parameters.Add(new OracleParameter(":ID", imaxid));
                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":CREATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));

                                        imaxid++;

                                        // Execute the insert using ExecuteSqlRaw

                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
                UPDATE BZ_MASTER_FEEDBACK_TYPE 
                SET NAME = :NAME, 
                    STATUS = :STATUS, 
                    SORT_BY = :SORT_BY, 
                    UPDATE_BY = :UPDATE_BY, 
                    UPDATE_DATE = SYSDATE, 
                    TOKEN_UPDATE = :TOKEN_UPDATE 
                WHERE ID = :ID";

                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":UPDATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));

                                        // Execute the update using ExecuteSqlRaw
                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"DELETE FROM BZ_MASTER_FEEDBACK_TYPE WHERE ID = :ID";
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));

                                        // Execute the delete using ExecuteSqlRaw
                                    }
                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";

                                    if (ret.ToLower() != "true") { goto Next_line_1; }

                                    //if (action_type == "delete" && (dtlist[i].sub_data.ToString() == "true"))
                                    //{
                                    //    //delete BZ_MASTER_FEEDBACK_LIST  
                                    //    sqlstr = @" delete from BZ_MASTER_FEEDBACK_LIST ";
                                    //    sqlstr += @" where ";
                                    //    sqlstr += @" FEEDBACK_TYPE_ID = " + conn.ChkSqlStr(dtlist[i].id, 300);


                                    //    ret = SetDocService.execute_data_ex(sqlstr, true);
                                    //    sqlstr_all += sqlstr + "||";

                                    //    if (ret.ToLower() != "true") { goto Next_line_1; }
                                    //}

                                    if (action_type == "delete" && (dtlist[i]?.sub_data?.ToString() == "true"))
                                    {
                                        sqlstr = @"DELETE FROM BZ_MASTER_FEEDBACK_LIST WHERE FEEDBACK_TYPE_ID = :FEEDBACK_TYPE_ID";
                                        parameters = new List<OracleParameter>();  // Clear existing parameters
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id));

                                        ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";


                                        if (ret.ToLower() != "true") { goto Next_line_1; }
                                    }
                                }

                                transaction.Commit();

                            }

                        Next_line_1:;
                            if (ret.ToLower() == "true")
                            {
                                // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ret = $"Error: {ex.Message}";
                    }
                    finally
                    {
                    }
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchFeedbackType(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        public MMaintainDataModel SetFeedbackList(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var sql = "";
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_FEEDBACK_LIST); ;


            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.feedback_list.Count > 0)
                            {
                                List<MasterNormalModel> dtlist = data.feedback_list;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    ret = "true";
                                    var action_type = dtlist[i].action_type?.ToString() ?? "";
                                    if (string.IsNullOrWhiteSpace(action_type)) { continue; }

                                    if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }

                                    string sqlstr = "";
                                    parameters = new List<OracleParameter>(); // 💡 Reset parameter ใหม่ทุกรอบ

                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
            INSERT INTO BZ_MASTER_FEEDBACK_LIST 
                (ID, FEEDBACK_TYPE_ID, QUESTION_OTHER, NAME, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
            VALUES 
                (:ID, :FEEDBACK_TYPE_ID, :QUESTION_OTHER, :NAME, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)
        ";

                                        parameters.Add(new OracleParameter(":ID", imaxid));
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id_main));
                                        parameters.Add(new OracleParameter(":QUESTION_OTHER", dtlist[i].question_other));
                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":CREATE_BY", emp_user_active));
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));

                                        imaxid++;
                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
                                                UPDATE BZ_MASTER_FEEDBACK_LIST 
                                                SET NAME = :NAME, 
                                                    STATUS = :STATUS, 
                                                    SORT_BY = :SORT_BY, 
                                                    QUESTION_OTHER = :QUESTION_OTHER, 
                                                    UPDATE_BY = :UPDATE_BY, 
                                                    UPDATE_DATE = SYSDATE, 
                                                    TOKEN_UPDATE = :TOKEN_UPDATE 
                                                WHERE ID = :ID 
                                                  AND FEEDBACK_TYPE_ID = :FEEDBACK_TYPE_ID
        ";

                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":QUESTION_OTHER", dtlist[i].question_other));
                                        parameters.Add(new OracleParameter(":UPDATE_BY", emp_user_active));
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id_main));
                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"
                                        DELETE FROM BZ_MASTER_FEEDBACK_LIST 
                                        WHERE ID = :ID 
                                          AND FEEDBACK_TYPE_ID = :FEEDBACK_TYPE_ID
                                    ";

                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id_main));
                                    }

                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";
                                    if (ret.ToLower() != "true") { goto Next_line_1; }


                                    if (action_type == "delete" && (dtlist[i].sub_data.ToString() == "true"))
                                    {
                                        var sqlBuilderDelete = new StringBuilder(@"
                            DELETE FROM BZ_MASTER_FEEDBACK_QUESTION 
                       WHERE FEEDBACK_LIST_ID = :FEEDBACK_LIST_ID 
                         AND FEEDBACK_TYPE_ID = :FEEDBACK_TYPE_ID");

                                        parameters = new List<OracleParameter>();  // Clear previous parameters before reusing

                                        parameters.Add(new OracleParameter(":FEEDBACK_LIST_ID", dtlist[i].id));
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id_main));

                                        // Execute the delete using ExecuteSqlRaw
                                        ret = context.Database.ExecuteSqlRaw(sqlBuilderDelete.ToString(), parameters.ToArray()) > -1 ? "true" : "false";


                                        if (ret.ToLower() != "true") { goto Next_line_1; }
                                    }


                                }
                                transaction.Commit();
                            }

                        Next_line_1:;
                            if (ret.ToLower() == "true")
                            {
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ret = $"Error: {ex.Message}";
                    }
                    finally
                    {
                        // conn.CloseConnection();
                    }
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchFeedbackList(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public MMaintainDataModel SetFeedbackQuestion(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var sql = "";
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_FEEDBACK_QUESTION); ;


            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {

                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.feedback_question.Count > 0)
                            {
                                List<MasterNormalModel> dtlist = data.feedback_question;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    ret = "true"; sqlstr = "";
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }

                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
                INSERT INTO BZ_MASTER_FEEDBACK_QUESTION 
                (ID, FEEDBACK_TYPE_ID, FEEDBACK_LIST_ID, QUESTION, DESCRIPTION, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                VALUES 
                (:ID, :FEEDBACK_TYPE_ID, :FEEDBACK_LIST_ID, :QUESTION, :DESCRIPTION, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                        parameters.Add(new OracleParameter(":ID", imaxid));
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id_main));
                                        parameters.Add(new OracleParameter(":FEEDBACK_LIST_ID", dtlist[i].id_sub));
                                        parameters.Add(new OracleParameter(":QUESTION", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":DESCRIPTION", dtlist[i].description));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":CREATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));

                                        imaxid++;


                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
                UPDATE BZ_MASTER_FEEDBACK_QUESTION 
                SET QUESTION = :QUESTION, 
                    DESCRIPTION = :DESCRIPTION, 
                    STATUS = :STATUS, 
                    SORT_BY = :SORT_BY, 
                    UPDATE_BY = :UPDATE_BY, 
                    UPDATE_DATE = SYSDATE, 
                    TOKEN_UPDATE = :TOKEN_UPDATE 
                WHERE ID = :ID 
                  AND FEEDBACK_TYPE_ID = :FEEDBACK_TYPE_ID 
                  AND FEEDBACK_LIST_ID = :FEEDBACK_LIST_ID";

                                        parameters.Add(new OracleParameter(":QUESTION", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":DESCRIPTION", dtlist[i].description));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":UPDATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id_main));
                                        parameters.Add(new OracleParameter(":FEEDBACK_LIST_ID", dtlist[i].id_sub));


                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"DELETE FROM BZ_MASTER_FEEDBACK_QUESTION 
                       WHERE ID = :ID 
                         AND FEEDBACK_TYPE_ID = :FEEDBACK_TYPE_ID 
                         AND FEEDBACK_LIST_ID = :FEEDBACK_LIST_ID";

                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));
                                        parameters.Add(new OracleParameter(":FEEDBACK_TYPE_ID", dtlist[i].id_main));
                                        parameters.Add(new OracleParameter(":FEEDBACK_LIST_ID", dtlist[i].id_sub));


                                    }

                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";


                                    if (ret.ToLower() != "true") { goto Next_line_1; }

                                }
                                transaction.Commit();
                            }

                        Next_line_1:;
                            if (ret.ToLower() == "true")
                            {
                                // ret = SetDocService.execute_data_ex(sql, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ret = $"Error: {ex.Message}";
                    }
                    finally
                    {
                        // conn.CloseConnection();
                    }
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchFeedbackQuestion(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        public MMaintainDataModel SetConfigDailyAllowance(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var sql = "";
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_CONFIG_DAILY_ALLOWANCE); ;


            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.allowance_list.Count > 0)
                            {
                                List<MasterAllowance_ListModel> dtlist = data.allowance_list;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    ret = "true"; sqlstr = "";
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }


                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
                INSERT INTO BZ_CONFIG_DAILY_ALLOWANCE 
                (ID, TRAVEL_CATEGORY, OVERNIGHT_TYPE, KH_CODE, WORKPLACE, WORKPLACE_TYPE_COUNTRY, 
                ALLOWANCE_RATE, CURRENCY, STATUS, SORT_BY, REMARK, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                VALUES (:ID, :TRAVEL_CATEGORY, :OVERNIGHT_TYPE, :KH_CODE, :WORKPLACE, :WORKPLACE_TYPE_COUNTRY, 
                :ALLOWANCE_RATE, :CURRENCY, :STATUS, :SORT_BY, :REMARK, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                        parameters.Add(new OracleParameter(":ID", imaxid));
                                        parameters.Add(new OracleParameter(":TRAVEL_CATEGORY", dtlist[i].travel_category));
                                        parameters.Add(new OracleParameter(":OVERNIGHT_TYPE", dtlist[i].overnight_type));
                                        parameters.Add(new OracleParameter(":KH_CODE", dtlist[i].kh_code));
                                        parameters.Add(new OracleParameter(":WORKPLACE", dtlist[i].workplace));
                                        parameters.Add(new OracleParameter(":WORKPLACE_TYPE_COUNTRY", dtlist[i].workplace_type_country));
                                        parameters.Add(new OracleParameter(":ALLOWANCE_RATE", dtlist[i].allowance_rate));
                                        parameters.Add(new OracleParameter(":CURRENCY", dtlist[i].currency));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":REMARK", dtlist[i].remark));
                                        parameters.Add(new OracleParameter(":CREATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));

                                        imaxid++;

                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
                UPDATE BZ_CONFIG_DAILY_ALLOWANCE 
                SET TRAVEL_CATEGORY = :TRAVEL_CATEGORY, 
                    OVERNIGHT_TYPE = :OVERNIGHT_TYPE, 
                    KH_CODE = :KH_CODE, 
                    WORKPLACE = :WORKPLACE, 
                    WORKPLACE_TYPE_COUNTRY = :WORKPLACE_TYPE_COUNTRY, 
                    ALLOWANCE_RATE = :ALLOWANCE_RATE, 
                    CURRENCY = :CURRENCY, 
                    STATUS = :STATUS, 
                    SORT_BY = :SORT_BY, 
                    REMARK = :REMARK, 
                    UPDATE_BY = :UPDATE_BY, 
                    UPDATE_DATE = SYSDATE, 
                    TOKEN_UPDATE = :TOKEN_UPDATE 
                WHERE ID = :ID";

                                        parameters.Add(new OracleParameter(":TRAVEL_CATEGORY", dtlist[i].travel_category));
                                        parameters.Add(new OracleParameter(":OVERNIGHT_TYPE", dtlist[i].overnight_type));
                                        parameters.Add(new OracleParameter(":KH_CODE", dtlist[i].kh_code));
                                        parameters.Add(new OracleParameter(":WORKPLACE", dtlist[i].workplace));
                                        parameters.Add(new OracleParameter(":WORKPLACE_TYPE_COUNTRY", dtlist[i].workplace_type_country));
                                        parameters.Add(new OracleParameter(":ALLOWANCE_RATE", dtlist[i].allowance_rate));
                                        parameters.Add(new OracleParameter(":CURRENCY", dtlist[i].currency));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":REMARK", dtlist[i].remark));
                                        parameters.Add(new OracleParameter(":UPDATE_BY", emp_user_active));  // username login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));

                                        // Execute update using ExecuteSqlRaw
                                        sql = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()).ToString();
                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"DELETE FROM BZ_CONFIG_DAILY_ALLOWANCE WHERE ID = :ID";
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));


                                    }
                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";






                                    if (ret.ToLower() != "true") { goto Next_line_1; }

                                }
                                transaction.Commit();

                            }

                        Next_line_1:;
                            if (ret.ToLower() == "true")
                            {
                                // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ret = $"Error: {ex.Message}";
                    }
                    finally
                    {
                    }
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchConfigDailyAllowance(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        public MMaintainDataModel SetInsurancePlan(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var sql = "";
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_FEEDBACK_TYPE); ;


            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.feedback_type.Count > 0)
                            {
                                List<MasterNormalModel> dtlist = data.feedback_type;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    ret = "true"; sqlstr = "";
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }


                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
                INSERT INTO BZ_MASTER_FEEDBACK_TYPE 
                (ID, NAME, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                VALUES (:ID, :NAME, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                        parameters.Add(new OracleParameter(":ID", imaxid));
                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":CREATE_BY", emp_user_active));  // user name login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));

                                        imaxid++;

                                        // Execute insert using ExecuteSqlRaw
                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
                UPDATE BZ_MASTER_FEEDBACK_TYPE 
                SET NAME = :NAME, 
                    STATUS = :STATUS, 
                    SORT_BY = :SORT_BY, 
                    UPDATE_BY = :UPDATE_BY, 
                    UPDATE_DATE = SYSDATE, 
                    TOKEN_UPDATE = :TOKEN_UPDATE 
                WHERE ID = :ID";

                                        parameters.Add(new OracleParameter(":NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter(":STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter(":SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter(":UPDATE_BY", emp_user_active));  // user name login
                                        parameters.Add(new OracleParameter(":TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));

                                        // Execute update using ExecuteSqlRaw
                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"
                DELETE FROM BZ_MASTER_FEEDBACK_TYPE 
                WHERE ID = :ID";

                                        parameters.Add(new OracleParameter(":ID", dtlist[i].id));

                                        // Execute delete using ExecuteSqlRaw

                                    }
                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";



                                    // Check for "delete" action and perform cascading delete if necessary


                                    if (ret.ToLower() != "true") { goto Next_line_1; }

                                }


                            }

                        Next_line_1:;
                            if (ret.ToLower() == "true")
                            {
                                // Commit if no issues
                                transaction.Commit();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                    finally { }
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchFeedbackType(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public MMaintainDataModel SetVISADocument(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var sql = "";
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_VISA_DOCUMENT); ;


            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.visa_document.Count > 0)
                            {
                                List<MasterVISADocument_ListModel> dtlist = data.visa_document;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    ret = "true"; sqlstr = "";
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }


                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
        INSERT INTO BZ_MASTER_VISA_DOCUMENT 
        (ID, NAME, DESCRIPTION, PREPARING_BY, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
        VALUES (:ID, :NAME, :DESCRIPTION, :PREPARING_BY, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                        parameters = new List<OracleParameter>(); // Clear parameters before adding new ones
                                        parameters.Add(new OracleParameter("ID", imaxid));
                                        parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter("DESCRIPTION", dtlist[i].description));
                                        parameters.Add(new OracleParameter("PREPARING_BY", dtlist[i].preparing_by));
                                        parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter("CREATE_BY", emp_user_active));
                                        parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));

                                        imaxid++;

                                        // Execute using context.Database.ExecuteSqlRaw

                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
        UPDATE BZ_MASTER_VISA_DOCUMENT 
        SET NAME = :NAME, 
            DESCRIPTION = :DESCRIPTION, 
            PREPARING_BY = :PREPARING_BY, 
            STATUS = :STATUS, 
            SORT_BY = :SORT_BY, 
            UPDATE_BY = :UPDATE_BY, 
            UPDATE_DATE = SYSDATE, 
            TOKEN_UPDATE = :TOKEN_UPDATE 
        WHERE ID = :ID";

                                        parameters = new List<OracleParameter>(); // Clear parameters before adding new ones
                                        parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter("DESCRIPTION", dtlist[i].description));
                                        parameters.Add(new OracleParameter("PREPARING_BY", dtlist[i].preparing_by));
                                        parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter("UPDATE_BY", emp_user_active));
                                        parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter("ID", dtlist[i].id));

                                        // Execute using context.Database.ExecuteSqlRaw

                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"DELETE FROM BZ_MASTER_VISA_DOCUMENT WHERE ID = :ID";

                                        parameters = new List<OracleParameter>(); // Clear parameters before adding new ones
                                        parameters.Add(new OracleParameter("ID", dtlist[i].id));

                                        // Execute using context.Database.ExecuteSqlRaw

                                    }
                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";
                                    // ret = SetDocService.execute_data_ex(sql, true);  // Execute with SetDocService

                                    if (ret.ToLower() != "true") { goto Next_line_1; }

                                    if (action_type == "delete" && dtlist[i].sub_data.ToString() == "true")
                                    {
                                        sqlstr = @"DELETE FROM BZ_MASTER_VISA_DOCOUNTRIES WHERE VISA_DOC_ID = :VISA_DOC_ID";

                                        parameters = new List<OracleParameter>(); // Clear parameters before adding new ones
                                        parameters.Add(new OracleParameter("VISA_DOC_ID", dtlist[i].id));

                                        // Execute using context.Database.ExecuteSqlRaw

                                        ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";


                                        if (ret.ToLower() != "true") { goto Next_line_1; }
                                    }

                                }
                                transaction.Commit();
                            }

                        Next_line_1:;
                            if (ret.ToLower() == "true")
                            {
                                // ret = SetDocService.execute_data_ex(sql, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ret = ex.Message;
                    }
                    finally
                    {
                    }
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchVISADocument(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public MMaintainDataModel SetVISADocountries(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();

            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_VISA_DOCOUNTRIES); ;
            int imaxidImg = sw.GetMaxID(TableMaxId.BZ_DOC_IMG); ;

            string ret = "";
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {

                            if (data.data_type.ToString() != "")
                            {
                                if (data.visa_docountries.Count > 0)
                                {
                                    List<MasterVISADocountries_ListModel> dtlist = data.visa_docountries;

                                    for (int i = 0; i < dtlist.Count; i++)
                                    {
                                        ret = "true";
                                        sqlstr = "";
                                        var id_def = "";
                                        var action_type = dtlist[i].action_type.ToString();
                                        if (action_type == "") { continue; }
                                        else if (action_type != "delete")
                                        {
                                            var action_change = dtlist[i].action_change + "";
                                            if (action_change.ToLower() != "true") { continue; }
                                        }

                                        if (action_type == "insert")
                                        {
                                            sqlstr = @"
                                            INSERT INTO BZ_MASTER_VISA_DOCOUNTRIES 
                                            (ID, CONTINENT_ID, COUNTRY_ID, VISA_DOC_ID, NAME, DESCRIPTION, 
                                             PREPARING_BY, STATUS, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                                            VALUES (:ID, :CONTINENT_ID, :COUNTRY_ID, :VISA_DOC_ID, :NAME, :DESCRIPTION, 
                                            :PREPARING_BY, :STATUS, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("ID", imaxid, "char"));
                                            parameters.Add(context.ConvertTypeParameter("CONTINENT_ID", dtlist[i].continent_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("COUNTRY_ID", dtlist[i].country_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("VISA_DOC_ID", dtlist[i].visa_doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("NAME", dtlist[i].name, "char"));
                                            parameters.Add(context.ConvertTypeParameter("DESCRIPTION", dtlist[i].description, "char"));
                                            parameters.Add(context.ConvertTypeParameter("PREPARING_BY", dtlist[i].preparing_by, "char"));
                                            parameters.Add(context.ConvertTypeParameter("STATUS", dtlist[i].status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("SORT_BY", dtlist[i].sort_by, "char"));
                                            parameters.Add(context.ConvertTypeParameter("CREATE_BY", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter("TOKEN_UPDATE", token_login, "char"));

                                            //กรณีที่เป็นข้อมูลใหม่ ให้ map id ใหม่ให้กับ Img ด้วย  
                                            if (data.img_list.Count > 0)
                                            {
                                                List<ImgList> drimg = data.img_list.Where(a => (a.id_level_1 == dtlist[i].id & a.id == dtlist[i].id)).ToList();
                                                if (drimg.Count > 0)
                                                {
                                                    drimg[0].id_level_1 = imaxid.ToString();
                                                }
                                            }

                                            id_def = imaxid.ToString();
                                            imaxid++;
                                        }
                                        else if (action_type == "update")
                                        {
                                            sqlstr = @"
                                            UPDATE BZ_MASTER_VISA_DOCOUNTRIES 
                                            SET VISA_DOC_ID = :VISA_DOC_ID, 
                                                CONTINENT_ID = :CONTINENT_ID, 
                                                COUNTRY_ID = :COUNTRY_ID, 
                                                NAME = :NAME, 
                                                DESCRIPTION = :DESCRIPTION, 
                                                PREPARING_BY = :PREPARING_BY, 
                                                STATUS = :STATUS, 
                                                SORT_BY = :SORT_BY, 
                                                UPDATE_BY = :UPDATE_BY, 
                                                UPDATE_DATE = SYSDATE, 
                                                TOKEN_UPDATE = :TOKEN_UPDATE 
                                            WHERE ID = :ID";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("VISA_DOC_ID", dtlist[i].visa_doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("CONTINENT_ID", dtlist[i].continent_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("COUNTRY_ID", dtlist[i].country_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("NAME", dtlist[i].name, "char"));
                                            parameters.Add(context.ConvertTypeParameter("DESCRIPTION", dtlist[i].description, "char"));
                                            parameters.Add(context.ConvertTypeParameter("PREPARING_BY", dtlist[i].preparing_by, "char"));
                                            parameters.Add(context.ConvertTypeParameter("STATUS", dtlist[i].status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("SORT_BY", dtlist[i].sort_by, "char"));
                                            parameters.Add(context.ConvertTypeParameter("UPDATE_BY", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter("TOKEN_UPDATE", token_login, "char"));
                                            parameters.Add(context.ConvertTypeParameter("ID", dtlist[i].id, "char"));
                                        }
                                        else if (action_type == "delete")
                                        {
                                            sqlstr = @"DELETE FROM BZ_MASTER_VISA_DOCOUNTRIES WHERE ID = :ID";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("ID", dtlist[i].id, "char"));
                                        }

                                        try
                                        {
                                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                            if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                            ;
                                        }
                                        catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }
                                    }

                                }
                            }
                            else
                            {
                                ret = "true";
                            }

                            if (data.img_list.Count > 0 && ret == "true")
                            {
                                ret = sw.SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                            }

                            if (ret == "true")
                            {
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                        catch (Exception ex_tran)
                        {
                            ret = ex_tran.Message.ToString();
                            transaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }

            #endregion set data

            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchVISADocountries(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public MMaintainDataModel SetInsurancebroker(MMaintainDataModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            var parameters = new List<OracleParameter>();
            #region set data  
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_INSURANCE_COMPANY); ;



            using (var context = new TOPEBizTravelerProfileEntitys())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.data_type.ToString() != "")
                        {
                            sqlstr_all = "";
                            if (data.master_insurancebroker.Count > 0)
                            {
                                List<MMasterInsurancebrokerModel> dtlist = data.master_insurancebroker;

                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    parameters.Clear();
                                    ret = "true"; sqlstr = "";
                                    var action_type = dtlist[i].action_type.ToString();

                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true") { continue; }
                                    }


                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"
                        INSERT INTO BZ_MASTER_INSURANCE_COMPANY
                        (ID, NAME, EMAIL, TRAVELCOMPANY_TYPE, STATUS, STATUS_ISOS, 
                         STATUS_INSURANCE, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                        VALUES (:ID, :NAME, :EMAIL, :TRAVELCOMPANY_TYPE, :STATUS, :STATUS_ISOS, 
                                :STATUS_INSURANCE, :SORT_BY, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                        parameters.Add(new OracleParameter("ID", imaxid));
                                        parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter("EMAIL", dtlist[i].email));
                                        parameters.Add(new OracleParameter("TRAVELCOMPANY_TYPE", dtlist[i].travelcompany_type));
                                        parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter("STATUS_ISOS", dtlist[i].status_isos));
                                        parameters.Add(new OracleParameter("STATUS_INSURANCE", dtlist[i].status_insurance));
                                        parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter("CREATE_BY", emp_user_active));
                                        parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));

                                        imaxid++; // Increment for next insert
                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"
                        UPDATE BZ_MASTER_INSURANCE_COMPANY 
                        SET NAME = :NAME, EMAIL = :EMAIL, TRAVELCOMPANY_TYPE = :TRAVELCOMPANY_TYPE, 
                            STATUS = :STATUS, STATUS_ISOS = :STATUS_ISOS, STATUS_INSURANCE = :STATUS_INSURANCE, 
                            SORT_BY = :SORT_BY, UPDATE_BY = :UPDATE_BY, UPDATE_DATE = SYSDATE, 
                            TOKEN_UPDATE = :TOKEN_UPDATE
                        WHERE ID = :ID";

                                        parameters.Add(new OracleParameter("NAME", dtlist[i].name));
                                        parameters.Add(new OracleParameter("EMAIL", dtlist[i].email));
                                        parameters.Add(new OracleParameter("TRAVELCOMPANY_TYPE", dtlist[i].travelcompany_type));
                                        parameters.Add(new OracleParameter("STATUS", dtlist[i].status));
                                        parameters.Add(new OracleParameter("STATUS_ISOS", dtlist[i].status_isos));
                                        parameters.Add(new OracleParameter("STATUS_INSURANCE", dtlist[i].status_insurance));
                                        parameters.Add(new OracleParameter("SORT_BY", dtlist[i].sort_by));
                                        parameters.Add(new OracleParameter("UPDATE_BY", emp_user_active));
                                        parameters.Add(new OracleParameter("TOKEN_UPDATE", token_login));
                                        parameters.Add(new OracleParameter("ID", dtlist[i].id));
                                    }
                                    else if (action_type == "delete")
                                    {
                                        sqlstr = @"DELETE FROM BZ_MASTER_INSURANCE_COMPANY WHERE ID = :ID";

                                        parameters.Add(new OracleParameter("ID", dtlist[i].id));
                                    }

                                    // Execute SQL with parameters
                                    ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";


                                    if (ret.ToLower() != "true") { goto Next_line_1; }
                                }

                                // Commit the transaction if all operations succeed

                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback in case of an error
                        transaction.Rollback();
                        ret = $"Error: {ex.Message}";
                    }

                    finally
                    {
                    }
                //ret = SetDocService.execute_data_ex(sqlstr, true);
                //sqlstr_all += sqlstr + "||";

                Next_line_1:;
                    if (ret.ToLower() == "true")
                    {
                        // ret = SetDocService.execute_data_ex(sqlstr_all, false);
                    }
                }
            }
            #endregion set data


            var msg_error = "";
            if (ret.ToLower() == "")
            {
                msg_error = ret;
            }
            else if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchMasterDataService swd = new searchMasterDataService();
                MMaintainDataModel value_load = new MMaintainDataModel();
                value_load.token_login = data.token_login;
                value_load.page_name = data.page_name;
                value_load.module_name = data.module_name;

                data = new MMaintainDataModel();
                data = swd.SearchInsurancebroker(value_load);
            }


            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
    }
}