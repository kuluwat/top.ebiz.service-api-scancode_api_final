
 
using Oracle.ManagedDataAccess.Client;
using System.Data; 
using top.ebiz.service.Models.Traveler_Profile;
namespace top.ebiz.service.Service.Traveler_Profile
{

    public class userAuthenService
    { 
        public List<loginProfileResultModel> getProfile(loginProfileModel value)
        {
            var data = new List<loginProfileResultModel>();
            var msg = "";
            Boolean bCheckDataAD = true;
            string ret = "";
            string sqlstr = "";


            Boolean user_admin = false;
            string token_login = value.token_login;

            DataTable dt = new DataTable();
            sqlstr = @"  select  distinct   u.EMPLOYEEID empId
                    , u.userdisplay empName
                    , u.COMPANYCODE deptName 
                      , case when to_number(u.employeeid) < 1000000   then 
                                (case when up.imgpath is null then replace(replace(replace(u.imgurl,'.jpg', ''),u.employeeid, ''),to_number(u.employeeid),'') else up.imgpath end)
                              else 
                                (case when up.imgpath is null then replace((replace(replace(u.imgurl,'.jpg', ''),u.employeeid, '')),'/TOP/','/TES/') else up.imgpath end)
                      end  
                      ||
                      case when to_number(u.employeeid) < 1000000   then 
                                (case when up.imgprofilename is null then to_char(to_number(u.employeeid)) || '.jpg' else up.imgprofilename end)
                                else 
                                (case when up.imgprofilename is null then to_char((u.employeeid)) || '.jpg' else up.imgprofilename end)
                     end imgUrl
                     , u.usertype as user_type
                     from      BZ_LOGIN_TOKEN t 
                     left join vw_bz_users u on t.user_name = u.userid
                     left join bz_user_peofile up on u.employeeid = up.employeeid
                     where     t.TOKEN_CODE = :token_login";

            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(ClassConnectionDb.ConvertTypeParameter("token_login", token_login, "char", 4000));

            dt = new DataTable();
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
                    msg = "";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            #endregion Execute

            if (string.IsNullOrEmpty(msg) && dt?.Rows.Count > 0)
            {
                object empidObj = dt.Rows[0]["empid"];
                if (empidObj != DBNull.Value && !string.IsNullOrEmpty(empidObj.ToString()))
                {
                    bCheckDataAD = false;
                }

                sqlstr = @" SELECT a.USER_NAME, a.user_id, to_char(u.ROLE_ID) user_role ,a.TOKEN_CODE as token_code
                        FROM bz_login_token a left join vw_bz_users u on a.user_name = u.userid
                        WHERE a.TOKEN_CODE =:token_login  ";

                DataTable dtrole = new DataTable();
                parameters = new List<OracleParameter>();
                parameters.Add(ClassConnectionDb.ConvertTypeParameter("token_login", token_login, "char", 4000));


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
                        dtrole = dscmd?.Tables.Count > 0 ? dscmd.Tables[0] : new DataTable();
                        msg = "";
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
                #endregion Execute

                if (string.IsNullOrEmpty(msg) &&  dtrole?.Rows.Count > 0)
                {
                    DataRow login_empid = dtrole.Rows[0];
                    if ((login_empid["user_role"].ToString() ?? "") == "1") { user_admin = true; } else { }
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.Add(new loginProfileResultModel
                    {
                        empId = dt.Rows[i]["empId"].ToString(),
                        empName = dt.Rows[i]["empName"].ToString(),
                        deptName = dt.Rows[i]["deptName"].ToString(),
                        imgUrl = dt.Rows[i]["imgUrl"].ToString(),
                        remark = bCheckDataAD.ToString(),
                        user_admin = user_admin,
                        token_login = token_login,

                        user_type = dt.Rows[i]["user_type"].ToString(),

                    });
                }
            }
            return data;
        }
 
    }


}