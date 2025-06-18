
using System.Data;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;

using Microsoft.EntityFrameworkCore;
using top.ebiz.service.Models.Create_Trip;

using top.ebiz.service.Service.AzureAD;

namespace top.ebiz.service.Service.Create_Trip
{
    public class userAuthenService
    {
        public List<loginProfileResultModel> getProfileCreate(loginProfileModel value)
        {
            var data = new List<loginProfileResultModel>();

            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
            {
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();
                    DbCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "bz_sp_login_profile";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new OracleParameter("p_token", value.token_login));
                    DataTable dt = new DataTable();

                    OracleParameter oraP = new OracleParameter();
                    oraP.ParameterName = "mycursor";
                    oraP.OracleDbType = OracleDbType.RefCursor;
                    oraP.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(oraP);

                    using (var reader = cmd.ExecuteReader())
                    {
                        try
                        {
                            //var schema = reader.GetSchemaTable();
                            //data = reader.MapToList<loginProfileResultModel>() ?? new List<loginProfileResultModel>();
                            dt.Load(reader);
                            data = dt.ConvertDataTableToDynamicModels<loginProfileResultModel>() ?? new List<loginProfileResultModel>();
                        }
                        catch (Exception ex) { }
                    }

                }
            }

            return data;
        }

        public loginResultModel login(loginModel value)
        {
            var data = new loginResultModel();

            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
            {
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();
                    DbCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "bz_sp_login_token";
                    cmd.CommandType = CommandType.StoredProcedure;

                    string token_login = Guid.NewGuid().ToString();
                    cmd.Parameters.Add(new OracleParameter("p_token", token_login));
                    cmd.Parameters.Add(new OracleParameter("p_user_id", value.user_id));

                    try
                    {
                        cmd.ExecuteNonQuery();


                        data.msg_sts = "S";
                        data.msg_txt = "success";
                        data.token_login = token_login;
                    }
                    catch (Exception ex)
                    {
                        data.msg_sts = "E";
                        data.msg_txt = "Error";
                        data.token_login = ex.Message.ToString();
                    }


                }
            }

            return data;
        }

        public List<Users> GetADUsers(string UserName)
        {
            //DevFix 20241017 0000 ดึงข้อมูลจาก Table 
            List<Users> lstADUsers = new List<Users>();

            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var connection = context.Database.GetDbConnection())
                    {
                        connection.Open();

                        //DevFix 20241017 0000 ที่จริงต้องเอามาจาก AzureAD เนื่องจากจะเอาไปตรวจสอบว่า User ที่ Login เข้ามา ถ้าเป็น z user ให้ Add เพิ่ม
                        var dt = context.VW_BZ_USERS
                         .FromSqlRaw("select email, userdisplay,userid  from vw_bz_users h where lower(h.userid) = lower(:userid) "
                        , context.ConvertTypeParameter(":userid", UserName, "VARCHAR2")).Select(s => new { s.EMAIL, s.USERID, s.USERDISPLAY }).ToList();
                        if (dt != null)
                        {
                            if (dt?.Count > 0)
                            {
                                Users objSurveyUsers = new Users();
                                objSurveyUsers.Email = dt[0].EMAIL;
                                objSurveyUsers.UserName = dt[0].USERID;
                                objSurveyUsers.DisplayName = dt[0].USERDISPLAY;

                                lstADUsers.Add(objSurveyUsers);
                            }
                        }

                    }
                }
            }
            catch { }
            return lstADUsers;
        }


        public loginWebResultModel loginWeb(loginClientModel value)
        {
            value.user = (value.user ?? "") == "" ? "" : value.user ?? "".ToUpper();

            string xuser_ad = value.user; //email

            string token_login = Guid.NewGuid().ToString() ?? "";
            var data = new loginWebResultModel();
            var userList = GetADUsers(xuser_ad);
            if (userList == null)
            {
                if (value.pass == "admin")
                {
                    data.name = value.user ?? "";
                    data.token = token_login ?? "";
                }
                else
                {
                    data.token = "";
                    data.name = "invalid username or password! (ad)";
                    return data;
                }
            }
            else
            {
                if (value.pass == "admin")
                {
                    data.name = value.user ?? "";
                    data.token = token_login ?? "";
                }
            }

            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var connection = context.Database.GetDbConnection())
                    {
                        connection.Open();
                        var userid = value.user ?? "";

                        try
                        {
                            var user = context.BZ_USERS.Where(q => q.USERID == userid.ToUpper()).AsNoTracking().ToList();
                            if (user != null && user.Count() > 0)
                            {
                                DbCommand cmd = connection.CreateCommand();
                                cmd.CommandText = "bz_sp_login_token2";
                                cmd.CommandType = CommandType.StoredProcedure;
                                //string token_login = Guid.NewGuid().ToString();//DevFix 20201209 1100ยกไปส่วนบน
                                cmd.Parameters.Add(new OracleParameter("p_token", token_login));
                                cmd.Parameters.Add(new OracleParameter("p_user_id", user[0].EMPLOYEEID));
                                cmd.Parameters.Add(new OracleParameter("p_user_name", user[0].USERID));
                                try
                                {
                                    cmd.ExecuteNonQuery();

                                    data.token = token_login;
                                    data.name = userid;
                                }
                                catch (Exception ex)
                                {
                                    data.token = "error";
                                    data.name = ex.Message.ToString();
                                }
                            }
                            else
                            {
                                #region DevFix 20201209 1100 กรณีที่เป็น user z ที่ผ่านการ login ad แล้ว
                                bool bUserAD = false;


                                if (!string.IsNullOrEmpty(userid))
                                {
                                    string _Email = "";
                                    string _UserName = userid;
                                    string _DisplayName = "";
                                    try
                                    {
                                        var msg = GraphMicrosoftClient.InitializeGraph();

                                        //var AzureAdUserLists = GraphMicrosoftClient.GetUserDetails(userid?.ToLower()).GetAwaiter().GetResult();
                                        var AzureAdUserLists = GraphMicrosoftClient.GetUser((userid?.ToLower())).Result;
                                        if (AzureAdUserLists != null)
                                        {
                                            var itemList = AzureAdUserLists;
                                            if (itemList != null)
                                            {
                                                _Email = itemList.mail ?? "";
                                                _DisplayName = itemList.displayName ?? "";

                                                //p_email varchar2, action_type varchar2
                                                DbCommand cmd = connection.CreateCommand();
                                                cmd.CommandText = "BZ_SP_ADD_USER_Z";
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.Parameters.Add(new OracleParameter("p_token", token_login));
                                                cmd.Parameters.Add(new OracleParameter("p_user_id", _UserName?.ToUpper()));
                                                cmd.Parameters.Add(new OracleParameter("p_user_name", _DisplayName?.ToString()));
                                                cmd.Parameters.Add(new OracleParameter("p_email", _Email?.ToUpper()));
                                                cmd.Parameters.Add(new OracleParameter("action_type", "insert"));
                                                try
                                                {
                                                    cmd.ExecuteNonQuery();
                                                    bUserAD = true;
                                                }
                                                catch (Exception ex)
                                                {
                                                    bUserAD = false;
                                                    data.token = "error BZ_SP_ADD_USER_Z";
                                                    data.name = ex.Message.ToString();
                                                    data.msg_txt = ex.Message.ToString();
                                                    return data;
                                                }

                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        bUserAD = false;
                                        data.token = "error";
                                        data.name = ex.Message.ToString();
                                        data.msg_txt = "error AzureAdUserLists";
                                        return data;
                                    }
                                }

                                if (bUserAD == true)
                                {
                                    var userAd = context.BZ_USERS.Where(q => q.USERID == userid.ToUpper()).AsNoTracking().ToList();
                                    if (userAd != null && userAd.Count() > 0)
                                    {
                                        DbCommand cmd = connection.CreateCommand();
                                        cmd.CommandText = "bz_sp_login_token2";
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        //string token_login = Guid.NewGuid().ToString();//DevFix 20201209 1100ยกไปส่วนบน
                                        cmd.Parameters.Add(new OracleParameter("p_token", token_login));
                                        cmd.Parameters.Add(new OracleParameter("p_user_id", userAd[0].EMPLOYEEID));
                                        cmd.Parameters.Add(new OracleParameter("p_user_name", userAd[0].USERID));
                                        try
                                        {
                                            cmd.ExecuteNonQuery();

                                            data.token = token_login;
                                            data.name = userid;
                                        }
                                        catch (Exception ex)
                                        {
                                            data.token = "error";
                                            data.name = ex.Message.ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    data.token = "";
                                    data.name = "invalid username or password!";
                                    data.msg_txt = "username not equal then email thaioil";
                                }
                                #endregion DevFix 20201209 1100 กรณีที่เป็น user z ที่ผ่านการ login ad แล้ว
                            }

                        }
                        catch (Exception ex)
                        {

                            data.token = "error";
                            data.name = ex.Message.ToString();
                            data.msg_txt = "error user";
                            return data;
                        }


                    }
                }
            }
            catch (Exception ex)
            {

                data.token = "error";
                data.name = ex.Message.ToString();
                data.msg_txt = ex.Message.ToString();
                return data;
            }

            return data;
        }


        public loginResultModel logout(logoutModel value)
        {
            var data = new loginResultModel();

            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
            {
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();
                    DbCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "bz_sp_logout";
                    cmd.CommandType = CommandType.StoredProcedure;

                    //string token_login = Guid.NewGuid().ToString();
                    cmd.Parameters.Add(new OracleParameter("p_token", value.token));

                    try
                    {
                        cmd.ExecuteNonQuery();

                        data.msg_sts = "S";
                        data.msg_txt = "success";
                        data.token_login = "";
                    }
                    catch (Exception ex)
                    {
                        data.msg_sts = "E";
                        data.msg_txt = "Error";
                        data.token_login = "";
                    }


                }
            }

            return data;
        }

    }
}