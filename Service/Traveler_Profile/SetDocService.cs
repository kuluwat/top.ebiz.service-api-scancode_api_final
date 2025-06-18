
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.helper;
using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Create_Trip;
using _documentService = top.ebiz.service.Service.Create_Trip.documentService;
using Users = top.ebiz.service.Models.Traveler_Profile.Users;

namespace top.ebiz.service.Service.Traveler_Profile
{

    public class SetDocService
    {


        // Constructor รับ DocumentService เป็น dependency

        ClassConnectionDb _conn = new ClassConnectionDb();
        List<OracleParameter> parameters = new List<OracleParameter>();

        string sqlstr = "";
        string ret = "";
        DataTable dt;
         
        #region Function  


        public DataTable CheckPassport(string tablename, string doc_id)
        {
            dt = new DataTable();
            sqlstr = "select * from BZ_DATA_PASSPORT where 1=1 ";

            #region Execute
            parameters = new List<OracleParameter>();
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = _conn.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dt.AcceptChanges();
                }
                catch { }
                finally { _conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute 

            return dt;
        }
        public int GetMaxID(TableMaxId tlb)
        {
            string tablename = tlb switch
            {
                TableMaxId.BZ_DOC_ISOS_RECORD => "BZ_DOC_ISOS_RECORD",
                TableMaxId.BZ_DOC_IMG => "BZ_DOC_IMG",
                TableMaxId.BZ_DATA_CONTENT => "BZ_DATA_CONTENT",
                TableMaxId.BZ_DOC_AIRTICKET_BOOKING => "BZ_DOC_AIRTICKET_BOOKING",
                TableMaxId.BZ_DOC_AIRTICKET_BOOKING_DETAIL => "BZ_DOC_AIRTICKET_BOOKING_DETAIL",
                TableMaxId.BZ_DOC_ACCOMMODATION_BOOKING => "BZ_DOC_ACCOMMODATION_BOOKING",
                TableMaxId.BZ_DOC_ACCOMMODATION_DETAIL => "BZ_DOC_ACCOMMODATION_DETAIL",
                TableMaxId.BZ_DATA_VISA => "BZ_DATA_VISA",
                TableMaxId.BZ_DATA_PASSPORT => "BZ_DATA_PASSPORT",
                TableMaxId.BZ_DOC_ALLOWANCE => "BZ_DOC_ALLOWANCE",
                TableMaxId.BZ_DOC_ALLOWANCE_DETAIL => "BZ_DOC_ALLOWANCE_DETAIL",
                TableMaxId.BZ_DOC_ALLOWANCE_MAIL => "BZ_DOC_ALLOWANCE_MAIL",
                TableMaxId.BZ_DOC_REIMBURSEMENT => "BZ_DOC_REIMBURSEMENT",
                TableMaxId.BZ_DOC_REIMBURSEMENT_DETAIL => "BZ_DOC_REIMBURSEMENT_DETAIL",
                TableMaxId.BZ_DOC_TRAVELEXPENSE => "BZ_DOC_TRAVELEXPENSE",
                TableMaxId.BZ_DOC_TRAVELEXPENSE_DETAIL => "BZ_DOC_TRAVELEXPENSE_DETAIL",
                TableMaxId.BZ_DATA_MAIL => "BZ_DATA_MAIL",
                TableMaxId.BZ_DOC_INSURANCE => "BZ_DOC_INSURANCE",
                TableMaxId.BZ_DOC_FEEDBACK => "BZ_DOC_FEEDBACK",
                TableMaxId.BZ_CONFIG_DAILY_ALLOWANCE => "BZ_CONFIG_DAILY_ALLOWANCE",
                TableMaxId.BZ_MASTER_VISA_DOCUMENT => "BZ_MASTER_VISA_DOCUMENT",
                TableMaxId.BZ_MASTER_VISA_DOCOUNTRIES => "BZ_MASTER_VISA_DOCOUNTRIES",
                TableMaxId.BZ_MASTER_INSURANCE_COMPANY => "BZ_MASTER_INSURANCE_COMPANY",
                TableMaxId.BZ_DOC_PORTAL => "BZ_DOC_PORTAL",
                TableMaxId.BZ_DOC_AIRTICKET_DETAIL => "BZ_DOC_AIRTICKET_DETAIL",
                TableMaxId.BZ_DATA_FX_TYPE_M => "BZ_DATA_FX_TYPE_M",
                TableMaxId.BZ_DATA_KH_CODE => "BZ_DATA_KH_CODE",
                TableMaxId.BZ_DATA_MANAGE => "BZ_DATA_MANAGE",
                TableMaxId.BZ_MASTER_ALLOWANCE_TYPE => "BZ_MASTER_ALLOWANCE_TYPE",
                TableMaxId.BZ_MASTER_LIST_STATUS => "BZ_MASTER_LIST_STATUS",
                TableMaxId.BZ_MASTER_ALREADY_BOOKED_TYPE => "BZ_MASTER_ALREADY_BOOKED_TYPE",
                TableMaxId.BZ_MASTER_FEEDBACK_TYPE => "BZ_MASTER_FEEDBACK_TYPE",
                TableMaxId.BZ_MASTER_FEEDBACK_LIST => "BZ_MASTER_FEEDBACK_LIST",
                TableMaxId.BZ_MASTER_FEEDBACK_QUESTION => "BZ_MASTER_FEEDBACK_QUESTION",
                _ => throw new ArgumentOutOfRangeException(nameof(tlb), tlb, null)
            };
            //??? ต้องเปลี่ยนเป็น StoredProcedure
            dt = new DataTable();
            sqlstr = string.Format("select (nvl( max(to_number(id)),0)+1)as id from {0}", tablename);

            #region Execute 
            parameters = new List<OracleParameter>();
            //parameters.Add(new OracleParameter("xxxx", "xxx" )); 
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = new OracleCommand(sqlstr, _conn.conn);
                    //command.CommandType = CommandType.StoredProcedure;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dt.AcceptChanges();
                }
                catch { }
                finally { _conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute 

            //if (conn_ExecuteData(ref dt, sqlstr) == "")
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        if (dt.Rows[0]["id"].ToString() != "") { return Convert.ToInt32(dt.Rows[0]["id"].ToString()); }
                    }
                    catch { }
                }
            }
            return 1;
        }
        public int GetMaxIDYear(TableMaxId tbl)
        {
            string sql = tbl switch
            {
                TableMaxId.BZ_DOC_ISOS_RECORD => "select (nvl( max(to_number(id)),0)+1)as id from BZ_DOC_ISOS_RECORD where year = to_char(sysdate,'rrrr')",
                _ => throw new ArgumentOutOfRangeException(nameof(tbl), tbl, null)
            };
            dt = new DataTable();
            sqlstr = sql;

            #region Execute 
            parameters = new List<OracleParameter>();
            //parameters.Add(new OracleParameter("xxxx", "xxx" )); 
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = _conn.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dt.AcceptChanges();
                }
                catch { }
                finally { _conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute 

            //if (conn_ExecuteData(ref dt, sqlstr) == "")
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        if (dt.Rows[0]["id"].ToString() != "") { return Convert.ToInt32(dt.Rows[0]["id"].ToString()); }
                    }
                    catch { }
                }
            }
            return 1;
        }
        public string sqlEmpRole(string token_login, ref string user_id, ref string user_role, ref Boolean user_admin, string doc_id)
        {
            user_id = ""; user_role = ""; user_admin = false;
            dt = new DataTable();

            sqlstr = @" SELECT distinct a.USER_NAME, a.user_id, to_char(u.ROLE_ID) user_role ,a.TOKEN_CODE as token_code
                FROM bz_login_token a left join vw_bz_users u on a.user_login = u.userid
                left join bz_data_manage m on (m.pmsv_admin = 'true' or m.pmdv_admin = 'true' or m.super_admin = 'true') and m.emp_id = a.user_id
                WHERE a.TOKEN_CODE = :token_login  ";

            #region Execute 
            parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("token_login", token_login));
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = _conn.conn.CreateCommand();
                    command.CommandText = sqlstr;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    dt.AcceptChanges();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                finally { _conn.CloseConnection(); }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            #endregion Execute 

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow login_empid = dt.Rows[0];
                user_id = login_empid["user_id"].ToString() ?? "";
                user_role = login_empid["user_role"].ToString() ?? "";
            }

            // ตรวจสอบ user_role
            if (user_role == "1")
            {
                user_admin = true; // ถ้า user_role เป็น "1" ให้เป็น admin
            }
            else
            {
                sqlstr = " select emp_id from bz_data_manage where ( pmsv_admin = 'true' ) and emp_id = :user_id ";
                DataTable login_empid = new DataTable();

                #region Execute 
                parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("user_id", user_id));
                try
                {
                    _conn = new ClassConnectionDb();
                    _conn.OpenConnection();
                    try
                    {
                        var command = _conn.conn.CreateCommand();
                        command.CommandText = sqlstr;
                        foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                        login_empid = new DataTable();
                        login_empid = _conn.ExecuteAdapter(command).Tables[0];
                        login_empid.AcceptChanges();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                    finally { _conn.CloseConnection(); }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                #endregion Execute 

                // ตรวจสอบ login_empid
                if (login_empid != null && login_empid.Rows.Count > 0)
                {
                    user_admin = true; // ถ้ามีข้อมูลใน bz_data_manage ให้เป็น admin
                }
                else
                {
                    sqlstr = " SELECT DISTINCT user_id, '' AS user_name, email, role_type FROM (SELECT a.emp_id AS user_id, u.email, 'super_admin' AS role_type FROM bz_data_manage a INNER JOIN vw_bz_users u ON a.emp_id = u.employeeid WHERE a.super_admin = 'true')where user_id=:user_id ";
                    DataTable checksuperadmin = new DataTable();

                    #region Execute 
                    parameters = new List<OracleParameter>();
                    parameters.Add(new OracleParameter("user_id", user_id));
                    try
                    {
                        _conn = new ClassConnectionDb();
                        _conn.OpenConnection();
                        try
                        {
                            var command = _conn.conn.CreateCommand();
                            command.CommandText = sqlstr;
                            foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                            checksuperadmin = new DataTable();
                            checksuperadmin = _conn.ExecuteAdapter(command).Tables[0];
                            checksuperadmin.AcceptChanges();
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        finally { _conn.CloseConnection(); }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                    #endregion Execute 

                    // ตรวจสอบ login_empid
                    if (checksuperadmin != null && checksuperadmin.Rows.Count > 0)
                    {
                        user_admin = true; // ถ้ามีข้อมูลใน bz_data_manage ให้เป็น admin
                    }
                }
            }

            // ตรวจสอบ doc_id
            if (user_admin == false && !string.IsNullOrEmpty(doc_id) && doc_id.IndexOf("T") > -1)
            {
                DataTable login_empid = new DataTable();
                sqlstr = " select emp_id from bz_data_manage where ( pmdv_admin = 'true' ) and emp_id = :user_id ";

                #region Execute 
                parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("user_id", user_id));
                try
                {
                    _conn = new ClassConnectionDb();
                    _conn.OpenConnection();
                    try
                    {
                        var command = _conn.conn.CreateCommand();
                        command.CommandText = sqlstr;
                        foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                        login_empid = new DataTable();
                        login_empid = _conn.ExecuteAdapter(command).Tables[0];
                        login_empid.AcceptChanges();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                    finally { _conn.CloseConnection(); }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                #endregion Execute 

                // ตรวจสอบ login_empid
                if (login_empid != null && login_empid.Rows.Count > 0)
                {
                    user_admin = true; // ถ้ามีข้อมูลใน bz_data_manage ให้เป็น admin
                }
            }

            return "";
        }
        public string sqlEmpUserID(string token_login)
        {
            dt = new DataTable();
            sqlstr = @" SELECT a.USER_NAME, a.user_id, to_char(u.ROLE_ID) user_role ,a.TOKEN_CODE as token_code
                        FROM bz_login_token a left join vw_bz_users u on a.user_login = u.userid
                        WHERE a.TOKEN_CODE = :token_login ";

            #region Execute
            parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("token_login", token_login));
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = _conn.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dt.AcceptChanges();
                }
                catch { }
                finally { _conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute 

            //
            //if (conn_ExecuteData(ref dt, sqlstr) == "")
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    DataRow login_empid = dt.Rows[0];
                    return login_empid["user_id"].ToString() ?? "";
                }
            }
            return "";
        }
        public string sqlEmpUserName(string token_login)
        {
            dt = new DataTable();
            sqlstr = @" SELECT a.USER_NAME, a.user_id, to_char(u.ROLE_ID) user_role ,a.TOKEN_CODE as token_code
                        FROM bz_login_token a left join vw_bz_users u on a.user_login = u.userid
                        WHERE a.TOKEN_CODE = :token_login ";

            #region Execute
            parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("token_login", token_login));
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = _conn.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dt.AcceptChanges();
                }
                catch { }
                finally { _conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute 
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    DataRow login_empid = dt.Rows[0];
                    return login_empid["user_name"].ToString() ?? "";
                }
            }
            return "";
        }
        public string sqlEmpUserDispayName(string token_login)
        {
            dt = new DataTable();
            sqlstr = @" SELECT a.USER_NAME, a.user_id, to_char(u.ROLE_ID) user_role ,a.TOKEN_CODE as token_code
                        ,case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end userdisplay
                        FROM bz_login_token a left join vw_bz_users u on a.user_login = u.userid
                        WHERE a.TOKEN_CODE = :token_login ";

            #region Execute
            parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("token_login", token_login));
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = _conn.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dt.AcceptChanges();
                }
                catch { }
                finally { _conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute 
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    DataRow login_empid = dt.Rows[0];
                    return login_empid["userdisplay"].ToString() ?? "";
                }
            }
            return "";
        }
        public string sqlEmpUserMail(string token_login)
        {
            dt = new DataTable();
            sqlstr = @" SELECT a.USER_NAME, a.user_id, to_char(u.ROLE_ID) user_role ,a.TOKEN_CODE as token_code, u.email
                        ,case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end userdisplay
                        FROM bz_login_token a left join vw_bz_users u on a.user_login = u.userid
                        WHERE a.TOKEN_CODE = :token_login ";

            #region Execute
            parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("token_login", token_login));
            try
            {
                _conn = new ClassConnectionDb();
                _conn.OpenConnection();
                try
                {
                    var command = _conn.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    foreach (var parameter in parameters) { command.Parameters.Add(parameter); }
                    dt = new DataTable();
                    dt = _conn.ExecuteAdapter(command).Tables[0];
                    //dt.TableName = "Table1";
                    dt.AcceptChanges();
                }
                catch { }
                finally { _conn.CloseConnection(); }
            }
            catch { }
            #endregion Execute 
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    DataRow login_empid = dt.Rows[0];
                    return login_empid["email"].ToString() ?? "";
                }
            }
            return "";
        }

        #endregion Function  

        #region Function in Doc 
        public ImgList SetTravelerHistoryImgCheck(ImgList value)
        {
            var ret = SetTravelerHistoryImg(value);
            value.remark = (ret.ToLower() == "true" ? "" : ret);

            return value;
        }
        public string SetTravelerHistoryImg(ImgList value)
        {
            var data = value;
            var token_login = value.modified_by;
            var employeeid = data.emp_id ?? "";
            int imaxid = 1;

            string ret = "";
            string sqlstr = "";
            var parameters = new List<OracleParameter>();
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    sqlstr = @"select to_char(count(1)) as id_key from BZ_USER_PEOFILE where employeeid = :employeeid";
                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("employeeid", employeeid, "char"));
                    var resList = context.TempIdKeyModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList().FirstOrDefault();

                    Boolean bcheckInsert = false;
                    if (resList != null)
                    {
                        if (resList.id_key?.ToString() == "0") { bcheckInsert = true; }
                    }
                    if (bcheckInsert)
                    {
                        sqlstr = @"select to_char(nvl( max(to_number(id)),0)+1)as id_key from BZ_USER_PEOFILE ";
                        parameters = new List<OracleParameter>();
                        var resMax = context.TempIdKeyModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList().FirstOrDefault();

                        try
                        {
                            imaxid = Convert.ToInt32(resList.id_key?.ToString());
                        }
                        catch { }
                    }
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            if (bcheckInsert == true)
                            {
                                sqlstr = @"INSERT INTO BZ_USER_PEOFILE 
                                (ID, DOC_ID, EMPLOYEEID, IMGPATH, IMGPROFILENAME, CREATE_BY, CREATE_DATE, UPDATE_BY, UPDATE_DATE, TOKEN_UPDATE) 
                                VALUES 
                                (:id, :doc_id, :employeeid, :imgpath, :imgprofilename, :create_by, SYSDATE, NULL, NULL, :token_update)";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter(":id", imaxid, "char"));
                                parameters.Add(context.ConvertTypeParameter(":doc_id", "personal", "char"));
                                parameters.Add(context.ConvertTypeParameter(":employeeid", data.emp_id, "char"));
                                parameters.Add(context.ConvertTypeParameter(":imgpath", data.path, "char"));
                                parameters.Add(context.ConvertTypeParameter(":imgprofilename", data.filename, "char"));
                                parameters.Add(context.ConvertTypeParameter(":create_by", "system", "char"));
                                parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                            }
                            else
                            {
                                sqlstr = @"UPDATE BZ_USER_PEOFILE SET 
                                IMGPATH = :imgpath, 
                                IMGPROFILENAME = :imgprofilename, 
                                UPDATE_BY = :update_by, 
                                UPDATE_DATE = SYSDATE 
                                WHERE EMPLOYEEID = :employeeid";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter(":imgpath", data.path, "char"));
                                parameters.Add(context.ConvertTypeParameter(":imgprofilename", data.filename, "char"));
                                parameters.Add(context.ConvertTypeParameter(":update_by", token_login, "char"));
                                parameters.Add(context.ConvertTypeParameter(":employeeid", data.emp_id, "char"));
                            }

                            try
                            {
                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                if (iret > -1) { ret = "true"; } else { ret = "false"; }
                                ;
                            }
                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }

                            if (ret == "true")
                            {
                                context.SaveChanges();
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

            data.remark = ret;

            return ret;
        }

        public ImgList SetImgByPage(ImgList value, string action_type)
        {
            var data = value;
            var token_login = value.modified_by;//ส่งมาเป็น emp ??? เดียวค่อยเครียร์ว่าจะใช้อะไรกันแน่

            #region set data 
            int imaxid = GetMaxID(TableMaxId.BZ_DOC_IMG);

            using (ClassConnectionDb transaction = new ClassConnectionDb())
            {
                transaction.OpenConnection();
                transaction.BeginTransaction();


                if (action_type == "insert" || action_type == "delete")
                {
                    ///sql query string 
                    if (action_type == "insert")
                    {
                        //กรณีที่มีข้อมูลเก่า id เดียวกันให้ ลบก่อน
                        sqlstr = "UPDATE BZ_DOC_IMG SET STATUS = 0 " +
         "WHERE EMP_ID = :data_emp_id " +
         "AND DOC_ID = :data_doc_id " +
         "AND ID = :data_id";


                        parameters = new List<OracleParameter>();
                        parameters.Add(new OracleParameter("data_emp_id", data.emp_id));
                        parameters.Add(new OracleParameter("data_doc_id", data.doc_id));
                        parameters.Add(new OracleParameter("data_doc_id", data.id));

                        #region ExecuteNonQuerySQL Data

                        var cmd = transaction.conn.CreateCommand();
                        //command.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = sqlstr;
                        if (parameters != null && parameters?.Count > 0)
                        {
                            foreach (var _param in parameters)
                            {
                                if (_param != null && !cmd.Parameters.Contains(_param.ParameterName))
                                {
                                    cmd.Parameters.Add(_param);
                                }
                            }
                            cmd.Parameters.AddRange(parameters?.ToArray());
                        }
                        ret = transaction.ExecuteNonQuerySQL(cmd);
                        //if (ret != "true") break;

                        #endregion  ExecuteNonQuerySQL Data

                    }


                    parameters = new List<OracleParameter>();

                    if (action_type == "insert")
                    {
                        //กรณีที่มีข้อมูลเก่า id เดียวกันให้ ลบก่อน
                        var id = "";
                        if (data.id.ToString() == "") { id = imaxid.ToString(); imaxid++; }
                        sqlstr = "INSERT INTO BZ_DOC_IMG (ID, DOC_ID, EMP_ID, PATH, FILE_NAME, PAGE_NAME, ACTION_NAME, STATUS, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) " +
         "VALUES (:id, :doc_id, :emp_id, :path, :filename, :page_name, :action_name, :status, :create_by, sysdate, :token_update)";

                        parameters.Add(new OracleParameter("id", id));
                        parameters.Add(new OracleParameter("doc_id", data.doc_id));
                        parameters.Add(new OracleParameter("emp_id", data.emp_id));
                        parameters.Add(new OracleParameter("path", data.path));
                        parameters.Add(new OracleParameter("filename", data.filename));
                        parameters.Add(new OracleParameter("page_name", data.pagename));
                        parameters.Add(new OracleParameter("action_name", data.actionname));
                        parameters.Add(new OracleParameter("status", "1")); // Assuming `STATUS` is an integer and the value is `1`
                        parameters.Add(new OracleParameter("create_by", data.modified_by));
                        parameters.Add(new OracleParameter("token_update", null));
                    }
                    else if (action_type == "delete")
                    {
                        sqlstr = "UPDATE BZ_DOC_IMG SET STATUS = 0 " +
         "WHERE EMP_ID = :emp_id " +
         "AND DOC_ID = :doc_id " +
         "AND ID = :id";

                        parameters.Add(new OracleParameter("emp_id", data.emp_id));
                        parameters.Add(new OracleParameter("doc_id", data.doc_id));
                        parameters.Add(new OracleParameter("id", data.id));
                    }

                    #region ExecuteNonQuerySQL Data

                    var command = transaction.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    if (parameters != null && parameters?.Count > 0)
                    {
                        foreach (var _param in parameters)
                        {
                            if (_param != null && !command.Parameters.Contains(_param.ParameterName))
                            {
                                command.Parameters.Add(_param);
                            }
                        }
                        command.Parameters.AddRange(parameters?.ToArray());
                    }
                    ret = transaction.ExecuteNonQuerySQL(command);
                    //if (ret != "true") break;

                    #endregion  ExecuteNonQuerySQL Data
                }


                if (ret == "true")
                {
                    if (ClassConnectionDb.IsAuthorizedRole())
                    {
                        // ตรวจสอบสิทธิ์ก่อนดำเนินการ 
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    transaction.Rollback();
                }
            }


            #endregion set data


            return data;
        }
        public string openfile_excel_check(string fullpath, string token_login, string emp_user_active)
        {
            string ret = "true";
            try
            {
                FileInfo? fileInfo = FileUtil.GetFileInfo(fullpath);
                ExcelPackage package = new ExcelPackage(fileInfo);
            }
            catch (Exception ex) { ret = ex.Message.ToString(); }

            return ret;
        }
        public string readfile_excel_check(string fullpath, string token_login, string emp_user_active)
        {
            string ret = "true";
            try
            {
                ImportDataKH_Code(fullpath, token_login);
            }
            catch (Exception ex) { ret = ex.Message.ToString(); }

            return ret;
        }

        public string ImportDataKH_Code(string _FullPathName, string token_login)
        {
            var info = FileUtil.GetFileInfo(_FullPathName);
            if (info is null)
            {
                throw new FileNotFoundException("File not found", _FullPathName);
            }
            try
            {
                string emp_name = sqlEmpUserName(token_login);
                // sw_WriteLine("ImEx0", "start import excel " + _FullPathName);
                ret = import_excel_kh_code(_FullPathName, token_login, emp_name);
                //sw_WriteLine("ImEx1", "end import excel " + sqlstr);
            }
            catch (Exception ex) { ret += ex.Message.ToString(); }
            if (ret.ToLower() != "true") { return ret; }

            // Open the stream and read it back.
            using (FileStream file = info.Open(FileMode.Open, FileAccess.Read))
            {
                // sw_WriteLine("Im1", "Open the stream and read it back" + token_login + " file " + _FullPathName);

                string emp_name = sqlEmpUserName(token_login);
                string file_name = "";
                string file_size = "";
                string path = "";
                // Get file size  
                long size = file.Length;
                file_name = file.Name.ToString();
                file_size = (file.Length).ToString();

                FileInfo fileInfo = info;
                string directoryFullPath = fileInfo.DirectoryName!;
                path = directoryFullPath + @"\";
                file_name = fileInfo.Name.ToString();


                string ret = "";
                var parameters = new List<OracleParameter>();

                try
                {
                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {

                                //นำข้อมูล update in table BZ_DATA_KH_CODE 
                                sqlstr = "delete from BZ_FILE_DATA  where page_name ='khcode'";
                                parameters = new List<OracleParameter>();
                                try
                                {
                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iret > -1) { ret = "true"; } else { ret = "false"; }
                                    ;
                                }
                                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }

                                if (ret == "true")
                                {
                                    sqlstr = @"INSERT INTO BZ_FILE_DATA 
                                    (id, page_name, file_name, file_size, path, create_by, create_date, token_update)
                                    VALUES 
                                    (:id, :page_name, :file_name, :file_size, :path, :create_by, sysdate, :token_update)";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("id", "0", "char"));
                                    parameters.Add(context.ConvertTypeParameter("page_name", "khcode", "char"));
                                    parameters.Add(context.ConvertTypeParameter("file_name", file_name, "char"));
                                    parameters.Add(context.ConvertTypeParameter("file_size", file_size, "char"));
                                    parameters.Add(context.ConvertTypeParameter("path", path, "char"));
                                    parameters.Add(context.ConvertTypeParameter("create_by", emp_name, "char"));
                                    parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));

                                    try
                                    {
                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                        if (iret > 0) { ret = "true"; } else { ret = "false"; }
                                        ;
                                    }
                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }

                                }

                                if (ret == "true")
                                {
                                    context.SaveChanges();
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
            }

            return ret;
        }
        public string import_excel_kh_code(string fullpath, string token_login, string emp_user_active)
        {
            var info = FileUtil.GetFileInfo(fullpath);
            if (info is null)
            {
                throw new FileNotFoundException("File not found", fullpath);
            }
            DataTable dtcol = new DataTable();
            dtcol.Columns.Add("emp_id");
            dtcol.Columns.Add("oversea_code");
            dtcol.Columns.Add("local_code");
            dtcol.AcceptChanges();

            var imsg_rows = 1;
            string ret = "";
            try
            {
                //sw_WriteLine("name11", " path to your excel file ");

                //// path to your excel file 
                //fullpath = @"D:\Ebiz2\EBiz_Webservice\DocumentFile\khcode\KH_QR_CODE.xlsx";
                imsg_rows = 2;
                ExcelPackage ExcelPkg = new ExcelPackage();
                imsg_rows = 3;
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");
                imsg_rows = 4;

                FileInfo fileInfo = info;
                imsg_rows = 5;

                ExcelPackage package = new ExcelPackage(fileInfo);
                imsg_rows = 6;

                ExcelWorksheet? worksheet = package.Workbook.Worksheets.FirstOrDefault();
                imsg_rows = 7;
                // get number of rows and columns in the sheet
                int rows = worksheet.Dimension.Rows; // 20
                int columns = worksheet.Dimension.Columns; // 7
                int irows = 0;
                imsg_rows = 8;

                //sw_WriteLine("name12", "loop through the worksheet rows " + rows);
                string emp_id = "";
                string oversea_code = "";
                string local_code = "";
                // loop through the worksheet rows and columns
                for (int i = 2; i <= rows; i++)
                {
                    //column 1: emp --> 913 ต้องเพิ่ม 000000000 ให้ครบสิบหลัก
                    //column 9: Overseas --> OA2
                    //column 11: Local Allo --> LA2 
                    try
                    {
                        emp_id = worksheet.Cells[i, 1].Text.ToString();
                        oversea_code = worksheet.Cells[i, 9].Text.ToString();
                        local_code = worksheet.Cells[i, 11].Text.ToString();
                        if (emp_id.ToString() == "") { break; }

                        dtcol.Rows.Add(dtcol.NewRow());
                        dtcol.AcceptChanges();
                        dtcol.Rows[irows]["emp_id"] = emp_id;
                        dtcol.Rows[irows]["oversea_code"] = oversea_code;
                        dtcol.Rows[irows]["local_code"] = local_code;
                        dtcol.AcceptChanges();

                        irows++;

                        //sw_WriteLine("namex" + irows, emp_id.ToString() + " ,oversea_code :" + oversea_code + " ,local_code:" + local_code); 

                    }
                    catch (Exception ex1)
                    {
                        // sw_WriteLine("name x" + irows, "rows error: " + irows);
                        //ret = "rows error: " + irows + " fullpath: " + fullpath + " --> open excel " + ex1.Message.ToString();
                        //sw_WriteLine("namex" + irows, "rows error: " + irows + " emp:"+emp_id.ToString() + " ,oversea_code :" + oversea_code + " ,local_code:" + local_code);
                        break;
                    }


                }


            }
            catch (Exception ex)
            {
                ret = "rows error: " + imsg_rows + " fullpath: " + fullpath + " --> open excel " + ex.Message.ToString();
                return ret;
            }

            Boolean bCheckStepUpdateData = false;
            try
            {
                SetDocService wss = new SetDocService();
                Boolean bNewData = false;
                int imaxid = wss.GetMaxID(TableMaxId.BZ_DATA_KH_CODE);
                var sqlstr_all = "";
                var bCheckQuery = false;
                ClassConnectionDb conn = new ClassConnectionDb();

                string sqlstr = "";



                DataTable dt = new DataTable();

                using (var context = new TOPEBizTravelerProfileEntitys())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {


                        try
                        {
                            for (int i = 0; i < dtcol.Rows.Count; i++)
                            {
                                var parameters = new List<OracleParameter>();
                                string user_id = "";
                                string emp_id = dtcol.Rows[i]["emp_id"].ToString();
                                string oversea_code = dtcol.Rows[i]["oversea_code"].ToString();
                                string local_code = dtcol.Rows[i]["local_code"].ToString();
                                if (emp_id.ToString() == "") { continue; }
                                var iresult = 0;
                                if (bNewData == false)
                                {
                                    #region copy ข้อมูลเดิมไว้กอน
                                    sqlstr = "delete from BZ_DATA_KH_CODE_BEFOR ";
                                    iresult = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());

                                    sqlstr = "insert into BZ_DATA_KH_CODE_BEFOR select * from  BZ_DATA_KH_CODE ";
                                    iresult = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    #endregion copy ข้อมูลเดิมไว้กอน

                                    bNewData = true;
                                    sqlstr = "delete from BZ_DATA_KH_CODE ";
                                    iresult = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iresult > -1) { ret = "true"; }
                                    bCheckStepUpdateData = true;
                                }

                                if (emp_id.Length < 8)
                                {
                                    emp_id = ("00000000" + emp_id).Substring(emp_id.Length, 8);
                                }


                                if (ret == "true")
                                {
                                    parameters = new List<OracleParameter>();
                                    sqlstr = @"INSERT INTO  BZ_DATA_KH_CODE
                                    (id, emp_id, user_id, oversea_code, local_code, create_by, create_date, token_update) 
                                    VALUES 
                                    (:id, :emp_id, :user_id, :oversea_code, :local_code, :create_by, sysdate, :token_update)";

                                    parameters.Add(new OracleParameter("id", imaxid));
                                    parameters.Add(new OracleParameter("emp_id", emp_id));
                                    parameters.Add(new OracleParameter("user_id", user_id));
                                    parameters.Add(new OracleParameter("oversea_code", oversea_code));
                                    parameters.Add(new OracleParameter("local_code", local_code));

                                    //parameters.Add(new OracleParameter("sap_flag", "1"));
                                    //parameters.Add(new OracleParameter("ebiz_flag", "0")); // sap = 1, ebiz = 0
                                    parameters.Add(new OracleParameter("create_by", emp_user_active)); // username login
                                    parameters.Add(new OracleParameter("token_update", token_login));


                                    iresult = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iresult > -1) { ret = "true"; } else { ret = "false"; break; }
                                    imaxid++;
                                }

                            }

                            if (ret == "true")
                            {
                                if (ClassConnectionDb.IsAuthorizedRole())
                                {
                                    // ตรวจสอบสิทธิ์ก่อนดำเนินการ 
                                    transaction.Commit();
                                }
                                else
                                {
                                    transaction.Rollback();
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                        }



                    }
                }

            Next_line_1:;

                if (ret.ToLower() == "true")
                {
                    //sqlstr = "update BZ_DATA_KH_CODE a set emp_id =  substr('00000000' || a.emp_id,length('00000000' || a.emp_id)-7) ";
                    //ret = SetDocService.execute_data_ex(sqlstr, bCheckQuery);
                    //sqlstr_all += sqlstr + "||"; 
                    //ret = SetDocService.execute_data_ex(sqlstr_all, bCheckQuery);
                }
                else
                {

                    #region copy ข้อมูลเดิมกลับ 
                    if (bCheckStepUpdateData == true)
                    {
                        sqlstr = "delete from BZ_DATA_KH_CODE ";
                        ret = SetDocService.execute_data_ex(sqlstr, bCheckQuery);
                        if (ret.ToLower() != "true") { goto Next_line_1; }

                        sqlstr = "insert into BZ_DATA_KH_CODE select * from  BZ_DATA_KH_CODE_BEFOR ";
                        ret = SetDocService.execute_data_ex(sqlstr, bCheckQuery);
                        if (ret.ToLower() != "true") { goto Next_line_1; }
                    }
                    #endregion copy copy ข้อมูลเดิมกลับ

                }
            }
            catch (Exception ex) { ret = "import excel to table " + ex.Message.ToString() + " จำนวนข้อมูล : " + dtcol.Rows.Count; }

            //sw_WriteLine("e1", ret);

            return ret;

        }

        private void sw_WriteLine(string name_x, string msg_ref)
        {
            //C:\Users\2bLove\source\repos\top.ebiz\top.ebiz.service\bin\Debug\net8.0\
            string ServerFolder = top.ebiz.helper.AppEnvironment.GeteServerFolder();
            string pathw = $@"{ServerFolder}\DocumentFile\khcode\MyTest{name_x}.txt";
            if (!File.Exists(pathw))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(pathw))
                {
                    sw.WriteLine(msg_ref);
                }
            }
        }

        public string SetImgList(List<ImgList> value, int imaxidImg, string emp_user_active, string token_login
            , TOPEBizCreateTripEntities context)
        {
            List<ImgList> dtlist = value;
            for (int i = 0; i < dtlist.Count; i++)
            {
                ret = "true"; sqlstr = "";
                parameters = new List<OracleParameter>();

                var action_type = dtlist[i].action_type.ToString();
                if (action_type == "") { continue; }
                else if (action_type != "delete")
                {
                    var action_change = dtlist[i].action_change + "";
                    if (action_change.ToLower() != "true") { continue; }
                }

                if (action_type == "insert")
                {
                    string doc_id_def = dtlist[i].doc_id;
                    try
                    {
                        if (!string.IsNullOrEmpty(dtlist[i].actionname))
                        {
                            if (dtlist[i].actionname.ToLower() == "visa_page")
                            {
                                doc_id_def = "personal";
                            }
                        }
                    }
                    catch { dtlist[i].actionname = ""; }

                    sqlstr = @"INSERT INTO BZ_DOC_IMG 
           (ID, ID_LEVEL_1, ID_LEVEL_2, DOC_ID, EMP_ID, PATH, FILE_NAME, PAGE_NAME, ACTION_NAME, STATUS, ACTIVE_TYPE, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
           VALUES 
           (:imaxidImg, :id_level_1, :id_level_2, :doc_id_def, :emp_id, :path, :filename, :pagename, :actionname, 1, :active_type, :emp_user_active, sysdate, :token_login)";

                    parameters.Add(new OracleParameter("imaxidImg", imaxidImg));
                    parameters.Add(new OracleParameter("id_level_1", dtlist[i].id_level_1));
                    parameters.Add(new OracleParameter("id_level_2", dtlist[i].id_level_2));
                    parameters.Add(new OracleParameter("doc_id_def", doc_id_def));
                    parameters.Add(new OracleParameter("emp_id", dtlist[i].emp_id));
                    parameters.Add(new OracleParameter("path", dtlist[i].path));
                    parameters.Add(new OracleParameter("filename", dtlist[i].filename));
                    parameters.Add(new OracleParameter("pagename", dtlist[i].pagename));
                    parameters.Add(new OracleParameter("actionname", dtlist[i].actionname));
                    parameters.Add(new OracleParameter("active_type", dtlist[i].active_type));
                    parameters.Add(new OracleParameter("emp_user_active", emp_user_active));
                    parameters.Add(new OracleParameter("token_login", token_login));

                    imaxidImg++;
                }
                else
                {

                    var img_status = 0;//delete
                    if (action_type == "update")
                    {
                        img_status = 1;
                    }
                    //กรณีที่มีข้อมูลเก่า id เดียวกันให้ ลบก่อน
                    sqlstr = @"UPDATE BZ_DOC_IMG SET 
                   STATUS = :img_status";

                    parameters.Add(new OracleParameter("img_status", img_status));
                    if (dtlist[i].pagename.ToString().ToLower() == "visa")
                    {
                        sqlstr += ", ACTIVE_TYPE = :active_type";
                        parameters.Add(new OracleParameter("active_type", dtlist[i].active_type));
                    }

                    if (dtlist[i].pagename.ToString().ToLower() == "passport")
                    {
                    }

                    if (action_type == "update")
                    {
                        sqlstr += ", PATH = :path, FILE_NAME = :file_name";
                        parameters.Add(new OracleParameter("path", dtlist[i].path.ToString()));
                        parameters.Add(new OracleParameter("file_name", dtlist[i].filename.ToString()));
                    }

                    sqlstr += @",
           UPDATE_BY = :emp_user_active,
           UPDATE_DATE = sysdate,
           TOKEN_UPDATE = :token_login
           WHERE ID = :id";

                    parameters.Add(new OracleParameter("emp_user_active", emp_user_active));
                    parameters.Add(new OracleParameter("token_login", token_login));
                    parameters.Add(new OracleParameter("id", dtlist[i].id));

                    if (dtlist[i].pagename.ToString().ToLower() == "passport" ||
                        dtlist[i].pagename.ToString().ToLower() == "visadocument" ||
                        dtlist[i].pagename.ToString().ToLower() == "visa")
                    {
                        if ((dtlist[i].id_level_1 + "").ToString() != "")
                        {
                            sqlstr += " AND ID_LEVEL_1 = :id_level_1";
                            parameters.Add(new OracleParameter("id_level_1", dtlist[i].id_level_1));
                        }
                        if ((dtlist[i].id_level_2 + "").ToString() != "")
                        {
                            sqlstr += " AND ID_LEVEL_2 = :id_level_2";
                            parameters.Add(new OracleParameter("id_level_2", dtlist[i].id_level_2));
                        }

                        if (dtlist[i].actionname.ToString().ToLower() == "visa_page")
                        {
                            sqlstr += " AND EMP_ID = :emp_id";
                            parameters.Add(new OracleParameter("emp_id", dtlist[i].emp_id));
                            sqlstr += " AND DOC_ID = 'personal'";
                        }
                        else if (dtlist[i].pagename.ToString().ToLower() == "visa")
                        {
                            sqlstr += " AND DOC_ID = :doc_id";
                            parameters.Add(new OracleParameter("doc_id", dtlist[i].doc_id));
                            sqlstr += " AND EMP_ID = :emp_id";
                            parameters.Add(new OracleParameter("emp_id", dtlist[i].emp_id));
                        }
                    }
                    else
                    {
                        if (dtlist[i].pagename.ToString().ToLower() == "isos" || dtlist[i].pagename.ToString().ToLower() == "mtvisacountries")
                        {
                        }
                        else
                        {
                            sqlstr += " AND DOC_ID = :doc_id";
                            parameters.Add(new OracleParameter("doc_id", dtlist[i].doc_id));
                            sqlstr += " AND EMP_ID = :emp_id";
                            parameters.Add(new OracleParameter("emp_id", dtlist[i].emp_id));
                        }
                    }

                }


                try
                {
                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                    if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                    ;
                }
                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

            }

            if (ret.ToLower() == "true")
            {
                //delete file  
            }

            return ret;
        }




        #endregion Function in Doc
        //2. BAPI se37/ZTHROMB005 RFC สำหรับ update ข้อมูลพนักงาน เพื่ออัพเดตข้อมูลการเคลมค่าพาสปอร์ตและกระเป๋าเดินทางเข้า SAP
        public string insertPassort(string emp_id)
        {
            Boolean StatusSuccess = false;
            string StatusMessage = "";
            string msg_error = "";
 
            StatusSuccess = true;
            msg_error = (StatusSuccess ? "true" : StatusMessage);


            return msg_error;
        }

        internal static string execute_data_ex(string xstring, Boolean type_check)
        {
            //data maximum 100 row
            ClassConnectionDb conn = new ClassConnectionDb();
            string ret = "";
            string sqlstr = "";
            string[] drdata = xstring.Split(';');


            DataTable dt = new DataTable();
            using (ClassConnectionDb transaction = new ClassConnectionDb())
            {
                transaction.OpenConnection();
                transaction.BeginTransaction();

                for (int i = 0; i < drdata.Length; i++)
                {
                    sqlstr = drdata[i].ToString();
                    if (sqlstr == "") { continue; }
                    #region ExecuteNonQuerySQL Data

                    var command = transaction.conn.CreateCommand();
                    //command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sqlstr;
                    var parameters = new List<OracleParameter>();

                    if (parameters != null && parameters?.Count > 0)
                    {
                        foreach (var _param in parameters)
                        {
                            if (_param != null && !command.Parameters.Contains(_param.ParameterName))
                            {
                                command.Parameters.Add(_param);
                            }
                        }
                        command.Parameters.AddRange(parameters?.ToArray());
                    }
                    ret = transaction.ExecuteNonQuerySQL(command);
                    if (ret != "true") break;

                    #endregion  ExecuteNonQuerySQL Data
                    //var cmd = new OracleCommand(sqlstr)
                    //{
                    //    CommandType = CommandType.Text,
                    //    CommandText = sqlstr
                    //};

                    //ret = transaction.ExecuteNonQuerySQL(cmd);
                    //if (ret.ToLower() != "true") { break; } else { ret = ret.ToLower();
                    //}
                }
                if (ret == "true")
                {
                    if (ClassConnectionDb.IsAuthorizedRole( ))
                    {
                        // ตรวจสอบสิทธิ์ก่อนดำเนินการ 
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    transaction.Rollback();
                }
            }



            return ret;
        }

        internal static string conn_ExecuteData(ref DataTable dtSelect, string sqlstr)
        {
            ClassConnectionDb conn = new ClassConnectionDb();
            dtSelect = new DataTable();

            conn.OpenConnection(); // Ensure your connection is open
            //conn.BeginTransaction(); // Begin a transaction if needed


            try
            {
                // Assuming ClassConnectionDb provides connection string and OpenConnection method

                // Create OracleCommand
                var cmd = new OracleCommand(sqlstr, conn.conn)
                {
                    CommandType = CommandType.Text,
                };
                using var reader = cmd.ExecuteReader();
                dtSelect.Load(reader);
                conn.CloseConnection();
                // Remove ReadOnly property for all columns
                foreach (DataColumn col in dtSelect.Columns)
                {
                    col.MaxLength = -1;
                    if (col.ReadOnly)
                        col.ReadOnly = false;
                }
                return string.Empty; // Return empty string for success
            }
            catch (Exception ex)
            {
                conn.CloseConnection();
                //conn.Rollback();
                return ex.Message; // Return error message if an exception occurs
            }


        }


        internal static string conn_ExecuteData(ref DataTable dtSelect, OracleCommand cmd)
        {
            ClassConnectionDb conn = new ClassConnectionDb();
            dtSelect = new DataTable();

            conn.OpenConnection(); // Ensure your connection is open
            //conn.BeginTransaction(); // Begin a transaction if needed


            try
            {
                // Assuming ClassConnectionDb provides connection string and OpenConnection method

                // Create OracleCommand
                cmd.Connection = conn.conn;
                cmd.CommandType = CommandType.Text;
                using var reader = cmd.ExecuteReader();
                dtSelect.Load(reader);
                conn.CloseConnection();
                cmd.Dispose();
                // Remove ReadOnly property for all columns
                foreach (DataColumn col in dtSelect.Columns)
                {
                    col.MaxLength = -1;
                    if (col.ReadOnly)
                        col.ReadOnly = false;
                }

                return string.Empty; // Return empty string for success
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                conn.CloseConnection();
                //conn.Rollback();
                return ex.Message; // Return error message if an exception occurs
            }


        }
        internal static string conn_ExecuteData(ref DataTable dtSelect, DbCommand cmd)
        {
            dtSelect = new DataTable();

            try
            {

                cmd.CommandType = CommandType.Text;
                using var reader = cmd.ExecuteReader();
                dtSelect.Load(reader);
                cmd.Dispose();
                foreach (DataColumn col in dtSelect.Columns)
                {
                    col.MaxLength = -1;
                    if (col.ReadOnly)
                        col.ReadOnly = false;
                }

                return string.Empty; // Return empty string for success
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                return ex.Message; // Return error message if an exception occurs
            }


        }

        internal static string conn_ExecuteData_Bind_Variables(ref DataTable dtSelect, string sqlStr, List<OracleParameter> parameters)
        {
            ClassConnectionDb conn = new ClassConnectionDb();
            dtSelect = new DataTable();

            try
            {
                conn.OpenConnection(); // เปิด connection
                using (OracleCommand cmd = new OracleCommand(sqlStr, conn.conn))
                {
                    cmd.BindByName = true; // ใช้ชื่อ parameter ไม่ใช่ลำดับ
                    cmd.CommandType = CommandType.Text;

                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dtSelect.Load(reader);
                    }

                    // Remove ReadOnly property from DataTable
                    foreach (DataColumn col in dtSelect.Columns)
                    {
                        col.MaxLength = -1;
                        if (col.ReadOnly)
                            col.ReadOnly = false;
                    }

                    return string.Empty; // success
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                conn.CloseConnection();
            }
        }


        #region set data in page

        public TravelerHistoryOutModel SetTravelerHistory(TravelerHistoryOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_id_active = value.emp_id;
            var token_login = data.token_login;

            #region set data 

            int imaxid = 1;

            string ret = "";
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    sqlstr = @"select to_char(nvl( max(to_number(id)),0)+1)as id_key from BZ_USER_PEOFILE";
                    var iMax = context.TempIdKeyModelList.FromSqlRaw(sqlstr, new OracleParameter()).ToList().FirstOrDefault();
                    try { imaxid = Convert.ToInt32(iMax.id_key?.ToString() ?? "0"); } catch { }


                    sqlstr = "select distinct to_char(employeeid) as id_key from  bz_user_peofile ";
                    var EmpList = context.TempIdKeyModelList.FromSqlRaw(sqlstr, new OracleParameter()).ToList();


                    if (data.traveler_emp.Count > 0)
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                List<travelerEmpList> dtlist = data.traveler_emp;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        //if (action_change.ToLower() != "true") { continue; }
                                    }

                                    var doc_id = dtlist[i].doc_id.ToString();
                                    var emp_id_select = dtlist[i].emp_id.ToString();

                                    action_type = "insert";


                                    var iChkUpdate = EmpList.Where(a => a.id_key == emp_id_select).ToList();
                                    if (iChkUpdate.Count > 0) { action_type = "update"; }

                                    parameters = new List<OracleParameter>();
                                    if (action_type == "insert")
                                    {
                                        sqlstr = @"INSERT INTO BZ_USER_PEOFILE 
                                            (ID, DOC_ID, EMPLOYEEID, USERID, MOBILE, TELEPHONE, IMGPATH, IMGPROFILENAME, 
                                             CREATE_BY, CREATE_DATE, UPDATE_BY, UPDATE_DATE, TOKEN_UPDATE)
                                            VALUES 
                                            (:imaxid, :doc_id, :emp_id, :user_name, :user_phone, :user_tel, :imgpath, :imgprofilename, 
                                             :create_by, sysdate, :update_by, sysdate, :token_login)";

                                        parameters.Add(context.ConvertTypeParameter("imaxid", imaxid, "int"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("user_name", dtlist[i].userName, "char"));
                                        parameters.Add(context.ConvertTypeParameter("user_phone", dtlist[i].userPhone, "char"));
                                        parameters.Add(context.ConvertTypeParameter("user_tel", dtlist[i].userTel, "char"));
                                        parameters.Add(context.ConvertTypeParameter("imgpath", dtlist[i].imgpath, "char"));
                                        parameters.Add(context.ConvertTypeParameter("imgprofilename", dtlist[i].imgprofilename, "char"));
                                        parameters.Add(context.ConvertTypeParameter("create_by", emp_id_active, "char"));
                                        parameters.Add(context.ConvertTypeParameter("update_by", token_login, "char"));
                                        parameters.Add(context.ConvertTypeParameter("token_login", token_login, "char"));
                                        imaxid++;

                                    }
                                    else if (action_type == "update")
                                    {
                                        sqlstr = @"UPDATE BZ_USER_PEOFILE 
                                     SET MOBILE = :user_phone,
                                         TELEPHONE = :user_tel,
                                         UPDATE_BY = :update_by,
                                         UPDATE_DATE = sysdate,
                                         TOKEN_UPDATE = :token_login
                                     WHERE EMPLOYEEID = :emp_id";

                                        // Add parameters
                                        parameters.Add(context.ConvertTypeParameter("user_phone", dtlist[i].userPhone, "char"));
                                        parameters.Add(context.ConvertTypeParameter("user_tel", dtlist[i].userTel, "char"));
                                        parameters.Add(context.ConvertTypeParameter("update_by", token_login, "char"));
                                        parameters.Add(context.ConvertTypeParameter("token_login", token_login, "char"));
                                        parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));

                                    }
                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());

                                    if (iret > -1)
                                    {
                                        ret = "true";
                                    }
                                    else
                                    {
                                        ret = "false";
                                        break;
                                    }
                                }
                                if (ret == "true")
                                {
                                    context.SaveChanges();
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
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }

            #endregion set data

            var msg_error = "";
            if (!(ret == "true"))
            {
                msg_error = ret + " --> query error :" + sqlstr;
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
        public AirTicketOutModel SetAirTicket(AirTicketOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id; 
            var token_login = data.token_login;

            Boolean already_booked_emp_select = false;

            #region set data  

            searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
            int imaxid = GetMaxID(TableMaxId.BZ_DOC_AIRTICKET_BOOKING);
            int imaxidSub = GetMaxID(TableMaxId.BZ_DOC_AIRTICKET_DETAIL);
            int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);


            string doc_id = value.doc_id;
            string ret = "";
            string sql = "";
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    if (data.airticket_booking.Count > 0)
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                if (data.airticket_booking.Count > 0)
                                {
                                    List<airticketbookList> dtlist = data.airticket_booking;
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

                                        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                        string doc_status = "";
                                        try
                                        {
                                            doc_status = dtlist[i].doc_status.ToString();
                                        }
                                        catch { }
                                        if (doc_status == "") { doc_status = "1"; }
                                        try
                                        {
                                            if (dtlist[i].booking_status.ToString() != "")
                                            {
                                                //Traveler หรือ Admin Action : เลือก  confirm + submit 
                                                if (doc_type == "submit" && dtlist[i].booking_status.ToString() == "2") { doc_status = "4"; }
                                                else if (dtlist[i].booking_status.ToString() == "1" || dtlist[i].booking_status.ToString() == "3"
                                                    || doc_type == "save")
                                                {
                                                    //Admin Action : เลือก Booked + Waiting List + กด Save / Submit  
                                                    if (data.user_admin == true)
                                                    {
                                                        doc_status = "3";
                                                    }
                                                    else
                                                    {
                                                        doc_status = "2";
                                                    }
                                                }
                                            }
                                        }
                                        catch { }


                                        if (action_type == "insert")
                                        {
                                            sqlstr = @"INSERT INTO BZ_DOC_AIRTICKET_BOOKING 
           (ID, DOC_ID, DOC_STATUS, EMP_ID, DATA_TYPE, ASK_BOOKING, SEARCH_AIR_TICKET, AS_COMPANY_RECOMMEND, 
            ALREADY_BOOKED, ALREADY_BOOKED_OTHER, ALREADY_BOOKED_ID, BOOKING_REF, BOOKING_STATUS, ADDITIONAL_REQUEST, 
            CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
           VALUES 
           (:imaxid, :doc_id, :doc_status, :emp_id, :data_type, :ask_booking, :search_air_ticket, :as_company_recommend, 
            :already_booked, :already_booked_other, :already_booked_id, :booking_ref, :booking_status, :additional_request, 
            :create_by, sysdate, :token_login)";

                                            // Add parameters
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("imaxid", imaxid, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("data_type", dtlist[i].data_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter("ask_booking", dtlist[i].ask_booking, "char"));
                                            parameters.Add(context.ConvertTypeParameter("search_air_ticket", dtlist[i].search_air_ticket, "char"));
                                            parameters.Add(context.ConvertTypeParameter("as_company_recommend", dtlist[i].as_company_recommend, "char"));
                                            parameters.Add(context.ConvertTypeParameter("already_booked", dtlist[i].already_booked, "char"));
                                            parameters.Add(context.ConvertTypeParameter("already_booked_other", dtlist[i].already_booked_other, "char"));
                                            parameters.Add(context.ConvertTypeParameter("already_booked_id", dtlist[i].already_booked_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("booking_ref", dtlist[i].booking_ref, "char"));
                                            parameters.Add(context.ConvertTypeParameter("booking_status", dtlist[i].booking_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("additional_request", dtlist[i].additional_request, "char"));
                                            parameters.Add(context.ConvertTypeParameter("create_by", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter("token_login", token_login, "char"));



                                            imaxid++;
                                        }
                                        else if (action_type == "update")
                                        {

                                            sqlstr = @"UPDATE BZ_DOC_AIRTICKET_BOOKING SET 
           ASK_BOOKING = :ask_booking, 
           SEARCH_AIR_TICKET = :search_air_ticket, 
           AS_COMPANY_RECOMMEND = :as_company_recommend, 
           ALREADY_BOOKED = :already_booked, 
           ALREADY_BOOKED_OTHER = :already_booked_other, 
           ALREADY_BOOKED_ID = :already_booked_id, 
           BOOKING_REF = :booking_ref, 
           BOOKING_STATUS = :booking_status, 
           ADDITIONAL_REQUEST = :additional_request, 
           DATA_TYPE = :data_type, 
           UPDATE_BY = :update_by, 
           UPDATE_DATE = sysdate, 
           TOKEN_UPDATE = :token_update, 
           DOC_STATUS = :doc_status
           WHERE ID = :id 
           AND DOC_ID = :doc_id 
           AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("ask_booking", dtlist[i].ask_booking, "char"));
                                            parameters.Add(context.ConvertTypeParameter("search_air_ticket", dtlist[i].search_air_ticket, "char"));
                                            parameters.Add(context.ConvertTypeParameter("as_company_recommend", dtlist[i].as_company_recommend, "char"));
                                            parameters.Add(context.ConvertTypeParameter("already_booked", dtlist[i].already_booked, "char"));
                                            parameters.Add(context.ConvertTypeParameter("already_booked_other", dtlist[i].already_booked_other, "char"));
                                            parameters.Add(context.ConvertTypeParameter("already_booked_id", dtlist[i].already_booked_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("booking_ref", dtlist[i].booking_ref, "char"));
                                            parameters.Add(context.ConvertTypeParameter("booking_status", dtlist[i].booking_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("additional_request", dtlist[i].additional_request, "char"));
                                            parameters.Add(context.ConvertTypeParameter("data_type", dtlist[i].data_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter("update_by", emp_user_active, "char"));  // user name login
                                            parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));


                                        }
                                        else if (action_type == "delete")
                                        {
                                            sqlstr = @"DELETE FROM BZ_DOC_AIRTICKET_BOOKING 
           WHERE ID = :id 
           AND DOC_ID = :doc_id 
           AND EMP_ID = :emp_id";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));

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
                                if (data.airticket_detail.Count > 0 && ret == "true")
                                {
                                    List<airticketList> dtlist = data.airticket_detail;
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
                                            sqlstr = @"INSERT INTO BZ_DOC_AIRTICKET_DETAIL
           (ID, DOC_ID, EMP_ID, AIRTICKET_DATE, AIRTICKET_ROUTE_FROM, AIRTICKET_ROUTE_TO, AIRTICKET_FLIGHT, 
            AIRTICKET_DEPARTURE_TIME, AIRTICKET_ARRIVAL_TIME, AIRTICKET_DEPARTURE_DATE, AIRTICKET_ARRIVAL_DATE, 
            CHECK_MY_TRIP, AIRTICKET_ROOT, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
           VALUES 
           (:id, :doc_id, :emp_id, :airticket_date, :airticket_route_from, :airticket_route_to, 
            :airticket_flight, :airticket_departure_time, :airticket_arrival_time, :airticket_departure_date, 
            :airticket_arrival_date, :check_my_trip, :airticket_root, :create_by, SYSDATE, :token_update)";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("id", imaxidSub, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_date", dtlist[i].airticket_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_route_from", dtlist[i].airticket_route_from, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_route_to", dtlist[i].airticket_route_to, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_flight", dtlist[i].airticket_flight, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_departure_time", dtlist[i].airticket_departure_time, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_arrival_time", dtlist[i].airticket_arrival_time, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_departure_date", dtlist[i].airticket_departure_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_arrival_date", dtlist[i].airticket_arrival_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter("check_my_trip", dtlist[i].check_my_trip, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_root", dtlist[i].airticket_root, "char"));
                                            parameters.Add(context.ConvertTypeParameter("create_by", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));

                                            imaxidSub++;
                                        }
                                        else if (action_type == "update")
                                        {
                                            sqlstr = @"UPDATE BZ_DOC_AIRTICKET_DETAIL SET
           AIRTICKET_DATE = :airticket_date,
           AIRTICKET_ROUTE_FROM = :airticket_route_from,
           AIRTICKET_ROUTE_TO = :airticket_route_to,
           AIRTICKET_FLIGHT = :airticket_flight,
           AIRTICKET_DEPARTURE_TIME = :airticket_departure_time,
           AIRTICKET_ARRIVAL_TIME = :airticket_arrival_time,
           AIRTICKET_DEPARTURE_DATE = :airticket_departure_date,
           AIRTICKET_ARRIVAL_DATE = :airticket_arrival_date,
           CHECK_MY_TRIP = :check_my_trip,
           AIRTICKET_ROOT = :airticket_root,
           UPDATE_BY = :update_by,
           UPDATE_DATE = SYSDATE,
           TOKEN_UPDATE = :token_update
           WHERE ID = :id
           AND DOC_ID = :doc_id
           AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("airticket_date", dtlist[i].airticket_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_route_from", dtlist[i].airticket_route_from, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_route_to", dtlist[i].airticket_route_to, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_flight", dtlist[i].airticket_flight, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_departure_time", dtlist[i].airticket_departure_time, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_arrival_time", dtlist[i].airticket_arrival_time, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_departure_date", dtlist[i].airticket_departure_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_arrival_date", dtlist[i].airticket_arrival_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter("check_my_trip", dtlist[i].check_my_trip, "char"));
                                            parameters.Add(context.ConvertTypeParameter("airticket_root", dtlist[i].airticket_root, "char"));
                                            parameters.Add(context.ConvertTypeParameter("update_by", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));
                                            parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));


                                        }
                                        else if (action_type == "delete")
                                        {
                                            sqlstr = @"DELETE FROM BZ_DOC_AIRTICKET_DETAIL 
           WHERE ID = :id
           AND DOC_ID = :doc_id
           AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
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
                                if (data.img_list.Count > 0 && ret == "true")
                                {
                                    ret = "true"; sqlstr = "";
                                    ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                                }

                                if (data.airticket_booking.Count > 0 && ret == "true")
                                {
                                    List<airticketbookList> dtlist = data.airticket_booking;
                                    for (int i = 0; i < dtlist.Count; i++)
                                    {
                                        var _data_type = "save";
                                        ret = "true"; sqlstr = "";
                                        var action_type = dtlist[i].action_type.ToString();
                                        if (action_type == "") { continue; }
                                        else if (action_type != "delete")
                                        {
                                            var action_change = dtlist[i].action_change + "";
                                            if (action_change.ToLower() != "true") { continue; }
                                        }

                                        _data_type = dtlist[i].data_type.ToString();

                                        if (_data_type == "submit")
                                        {
                                            #region delelte data กรณีที่เป็น submit
                                            sqlstr = @"DELETE FROM BZ_DOC_AIRTICKET_DETAIL_KEEP WHERE DOC_ID = :doc_id AND EMP_ID = :emp_id";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));


                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }



                                            #endregion delelte data กรณีที่เป็น submit

                                            #region save data to bz_doc_airticket_detail_keep  

                                            sqlstr = @"
                                            INSERT INTO BZ_DOC_AIRTICKET_DETAIL_KEEP
                                            SELECT a.* 
                                            FROM BZ_DOC_AIRTICKET_DETAIL a
                                            INNER JOIN bz_doc_airticket_booking b 
                                                ON a.emp_id = b.emp_id 
                                                AND a.doc_id = b.doc_id 
                                                AND b.data_type = 'submit'
                                            WHERE a.DOC_ID = :doc_id
                                            AND a.EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));


                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                            #endregion save data to bz_doc_airticket_detail_keep 

                                        }

                                    }
                                }

                                if (ret == "true")
                                {
                                    context.SaveChanges();
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
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }

            #endregion set data

            var msg_error = "";
            var msg_status = "Save data";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                if (doc_type == "submit")
                {
                    try
                    {

                        //// ต้องแยก function get ออกมาตรงนี้ แล้วค่อยไป เรียกใช้ตาม emp_id_select 
                        //_swd = new searchDocTravelerProfileServices();
                        //var estList = new List<EstExpOutModel>();

                        //if (data.airticket_booking.Count > 0)
                        //{
                        //    List<airticketbookList> dtlist = data.airticket_booking;
                        //    for (int i = 0; i < dtlist.Count; i++)
                        //    {
                        //        var doc_id_select = dtlist[i].doc_id.ToString();
                        //        var emp_id_select = dtlist[i].emp_id.ToString();
                        //        var est = _swd.EstimateExpense(doc_id_select, emp_id_select);
                        //        estList.Add(est);
                        //    }
                        //}

                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            sql = "select distinct emp_id from bz_doc_allowance where doc_id = :doc_id ";
                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            var dataAllowance = context.TempEmpIdModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                            if (dataAllowance.Count > 0)
                            {
                                using (var transaction = context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        if (data.airticket_booking.Count > 0)
                                        {
                                            List<airticketbookList> dtlist = data.airticket_booking;
                                            for (int i = 0; i < dtlist.Count; i++)
                                            {
                                                ret = "true"; sqlstr = "";
                                                var action_type = dtlist[i].action_type.ToString();
                                                var data_type = dtlist[i].data_type.ToString();
                                                var doc_id_select = dtlist[i].doc_id.ToString();
                                                var emp_id_select = dtlist[i].emp_id.ToString();
                                                if (action_type == "") { continue; }
                                                else if (action_type != "delete")
                                                {
                                                    var action_change = dtlist[i].action_change + "";
                                                    if (action_change.ToLower() != "true") { continue; }
                                                }
                                                if (data_type == "submit")
                                                {
                                                    //ตรวจสอบว่าเป็นการ submit ให้คำนวณค่า allowance ใหม่ เฉพาะที่มีข้อมูล allowance 
                                                    var drcheck = dataAllowance.Where(p => p.emp_id.Equals(emp_id_select)).ToList();
                                                    if (drcheck != null && drcheck.Count > 0)
                                                    {
                                                        sqlstr = " delete from bz_doc_allowance_detail where doc_id= :doc_id and emp_id= :emp_id ";
                                                        parameters = new List<OracleParameter>();
                                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id_select, "char"));
                                                        parameters.Add(context.ConvertTypeParameter("emp_id", emp_id_select, "char"));

                                                        try
                                                        {
                                                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                            if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                            ;
                                                        }
                                                        catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }


                                                        #region Auwat 20210826 0000 update ข้อมูล doc status ALLOWANCE

                                                        string passport = "";
                                                        string passport_date = "";
                                                        string luggage_clothing = "";
                                                        string luggage_clothing_date = "";

                                                        try
                                                        {
                                                            searchDocCreateServices _swdFlow = new searchDocCreateServices();
                                                            var est = _swdFlow.EstimateExpense(context, doc_id_select, emp_id_select);
                                                            if (est.PassportExpense.ToString() != "")
                                                            {
                                                                passport = est.PassportExpense.ToString();
                                                            }
                                                            if (est.PassportDate.ToString() != "")
                                                            {
                                                                passport_date = _swdFlow.convert_date_display(est.PassportDate.ToString());
                                                            }
                                                            if (est.CLExpense.ToString() != "")
                                                            {
                                                                luggage_clothing = est.CLExpense.ToString();
                                                            }
                                                            if (est.CLDate.ToString() != "")
                                                            {
                                                                luggage_clothing_date = _swdFlow.convert_date_display(est.CLDate.ToString());//2021-03-11
                                                            }
                                                        }
                                                        catch { }

                                                        sqlstr = @" UPDATE BZ_DOC_ALLOWANCE SET DOC_STATUS = :doc_status";

                                                        parameters.Add(new OracleParameter("doc_status", "1"));

                                                        if (passport.ToString() != "")
                                                        {
                                                        }
                                                        if (passport_date.ToString() != "")
                                                        {
                                                        }
                                                        if (luggage_clothing.ToString() != "")
                                                        {
                                                            sqlstr += @" ,LUGGAGE_CLOTHING = :luggage_clothing";
                                                            parameters.Add(new OracleParameter("luggage_clothing", luggage_clothing));
                                                        }
                                                        if (luggage_clothing_date.ToString() != "")
                                                        {
                                                            sqlstr += @" ,LUGGAGE_CLOTHING_DATE = :luggage_clothing_date";
                                                            parameters.Add(new OracleParameter("luggage_clothing_date", luggage_clothing_date));
                                                        }
                                                    }

                                                    sqlstr = @"
                                                    UPDATE BZ_DOC_ALLOWANCE
                                                    SET UPDATE_BY = :update_by,
                                                        UPDATE_DATE = sysdate,
                                                        TOKEN_UPDATE = :token_update
                                                    WHERE DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                    // Add parameters to the list
                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter("update_by", emp_user_active, "char"));
                                                    parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));
                                                    parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));

                                                    try
                                                    {
                                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                        if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                        ;
                                                    }
                                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                                    #endregion Auwat 20210826 0000 update ข้อมูล doc status ALLOWANCE 

                                                }
                                            }

                                        }

                                        if (ret == "true")
                                        {
                                            context.SaveChanges();
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
                    }
                    catch (Exception ex)
                    {
                        ret = ex.Message.ToString();
                    }



                    msg_status = "Submit Data";


                    var page_name = "airticket";
                    var module_name = doc_type;
                    var email_admin = "";
                    var email_user_in_doc = "";
                    var mail_cc_active = "";
                    var role_type = "pmsv_admin";
                    var emp_id_user_in_doc = "";
                    var email_user_display = "";
                    var email_attachments = "";

                    List<EmpListOutModel> emp_list = data.emp_list;
                    List<mailselectList> mail_list = new List<mailselectList>();

                    _swd = new searchDocTravelerProfileServices();
                    string dtemplistuser = _swd.getapprovedtraveleremails(doc_id);
                    DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                    if (value.doc_id.ToString().IndexOf("T") > -1)
                    {
                        _swd = new searchDocTravelerProfileServices();
                        dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                        for (int i = 0; i < dtemplist.Rows.Count; i++)
                        {
                            email_admin += dtemplist.Rows[i]["email"] + ";";
                        }
                    }

                    //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
                    mail_cc_active = sqlEmpUserMail(value.token_login);

                    List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                    if (drempcheck.Count > 0)
                    {
                        emp_id_user_in_doc = drempcheck[0].emp_id.ToString();
                        email_user_in_doc = drempcheck[0].userEmail.ToString();
                        email_user_display = drempcheck[0].userDisplay.ToString();
                    }

                    List<airticketbookList> company_rec = data.airticket_booking.Where(a => (a.emp_id == emp_id_user_in_doc)).ToList();
                    string module_name_select = "";
                    if (data.user_admin == true)
                    {
                        if (company_rec != null && company_rec.Count > 0)
                        {
                            if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "1")
                            {
                                module_name_select = "traveler_review";  //032 
                            }
                            else if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "2")
                            {
                                module_name_select = "admin_confirmed";//011 
                                try
                                {
                                    List<ImgList> drimgcheck = data.img_list.Where(a => (a.emp_id == emp_id_user_in_doc) && a.action_type != "delete").ToList();
                                    for (int i = 0; i < drimgcheck.Count; i++)
                                    {
                                        if (email_attachments != "") { email_attachments += ";"; }
                                        email_attachments += drimgcheck[i].fullname;
                                    }
                                }
                                catch { }
                            }
                            else if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "3")
                            {
                                module_name_select = "admin_not_confirmed";//010 
                            }

                            if (module_name_select == "admin_confirmed")
                            {
                                mail_list.Add(new mailselectList
                                {
                                    module = "Air Ticket",
                                    mail_to = email_user_in_doc,
                                    //  mail_to = dtemplistuser,
                                    mail_to_display = email_user_display,
                                    mail_cc = email_admin,
                                    mail_attachments = email_attachments,
                                    mail_body_in_form = "",
                                    mail_status = "true",
                                    action_change = "true",
                                    emp_id = emp_id_user_in_doc,
                                });
                            }
                            else
                            {
                                mail_list.Add(new mailselectList
                                {
                                    module = "Air Ticket",
                                    mail_to = email_admin,
                                    // mail_to = dtemplistuser,
                                    mail_to_display = email_user_display,
                                    mail_cc = email_user_in_doc,
                                    mail_attachments = email_attachments,
                                    mail_body_in_form = "",
                                    mail_status = "true",
                                    action_change = "true",
                                    emp_id = emp_id_user_in_doc,
                                });
                            }

                        }
                    }
                    else
                    {
                        if (company_rec != null && company_rec.Count > 0)
                        {
                            try
                            {
                                //20211108 1438 ของเดิมเช็ค As Company Recommend เปลี่ยนเป็นเช็คแค่ Ask Booking by Company 
                                if (company_rec[0].ask_booking.ToString() == "true")
                                {
                                    module_name_select = "traveler_request";//009 
                                }
                                else if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "1")
                                {
                                    //Already Booked, Booked  

                                    //TO: Admin - PMSV; Admin - PMDV(if any) ;
                                    //CC: Traveler 
                                    module_name_select = "traveler_review";//032 
                                }

                                mail_list.Add(new mailselectList
                                {
                                    module = "Air Ticket",
                                    mail_to = email_user_in_doc,
                                    // mail_to = dtemplistuser,
                                    mail_to_display = email_user_display,
                                    mail_cc = email_admin,
                                    mail_attachments = email_attachments,
                                    mail_body_in_form = "",
                                    mail_status = "true",
                                    action_change = "true",
                                    emp_id = emp_id_user_in_doc,
                                });
                            }
                            catch { }
                        }
                    }

                    if (module_name_select != "")
                    {
                        ret = "";
                        SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                        ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id, page_name, module_name_select);
                        if (ret.ToLower() != "true")
                        {
                            msg_error = ret;
                        }
                    }




                    searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                    AirTicketModel value_load = new AirTicketModel();
                    value_load.token_login = data.token_login;
                    value_load.doc_id = data.doc_id;
                    data = new AirTicketOutModel();
                    data = swd.SearchAirTicket(value_load);
                }
            }
            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? msg_status + " succesed." : msg_status + " failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        public AccommodationOutModel SetAccommodation(AccommodationOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;
            Boolean already_booked_emp_select = false;


            var msg_ex_def = "";
            ret = "";

            #region set data 
            var doc_id = value.doc_id ?? "";

            if (data.accommodation_booking.Count > 0)
            {
                int imaxid = GetMaxID(TableMaxId.BZ_DOC_ACCOMMODATION_BOOKING);
                int imaxidSub = GetMaxID(TableMaxId.BZ_DOC_ACCOMMODATION_DETAIL);
                int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

                string ret = "";

                try
                {
                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {

                        if (data.accommodation_booking.Count > 0)
                        {
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                try
                                {
                                    if (data.accommodation_booking.Count > 0)
                                    {
                                        List<accommodationbookList> dtlist = data.accommodation_booking;
                                        for (int i = 0; i < dtlist.Count; i++)
                                        {
                                            ret = "true";
                                            var action_type = dtlist[i].action_type.ToString();
                                            if (action_type == "") { continue; }
                                            else if (action_type != "delete")
                                            {
                                                var action_change = dtlist[i].action_change + "";
                                                if (action_change.ToLower() != "true") { continue; }
                                            }

                                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                            string doc_status = "";
                                            try
                                            {
                                                doc_status = dtlist[i].doc_status.ToString();
                                            }
                                            catch { }
                                            try
                                            {
                                                if (dtlist[i].booking_status.ToString() != "")
                                                {
                                                    //Traveler หรือ Admin Action : เลือก  confirm + submit 
                                                    if (doc_type == "submit" && dtlist[i].booking_status.ToString() == "2") { doc_status = "4"; }
                                                    else if (dtlist[i].booking_status.ToString() == "1" || dtlist[i].booking_status.ToString() == "3" || doc_type == "save")
                                                    {
                                                        //Admin Action : เลือก Booked + Waiting List + กด Save / Submit  
                                                        if (data.user_admin == true)
                                                        {
                                                            doc_status = "3";
                                                        }
                                                        else
                                                        {
                                                            doc_status = "2";
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }

                                            try
                                            {
                                                List<EmpListOutModel> drempcheck = data.emp_list.Where(a => ((a.mail_status == "true") && (a.emp_id == dtlist[i].emp_id.ToString()))).ToList();
                                                if (drempcheck.Count > 0)
                                                {
                                                    drempcheck[0].doc_status_id = doc_status.ToString();
                                                    if (dtlist[i].booking_status.ToString() == "1" || dtlist[i].booking_status.ToString() == "3")
                                                    { already_booked_emp_select = false; }
                                                    else { already_booked_emp_select = true; }
                                                    try
                                                    {
                                                        if (data.user_admin == true && dtlist[i].already_booked.ToString() == "false") { already_booked_emp_select = true; }
                                                    }
                                                    catch { }
                                                }
                                            }
                                            catch { }

                                            if (action_type == "insert")
                                            {
                                                sqlstr = @"
                                                INSERT INTO BZ_DOC_ACCOMMODATION_BOOKING
                                                (ID, DOC_ID, EMP_ID, BOOKING, SEARCH, RECOMMEND, ALREADY_BOOKED, ALREADY_BOOKED_OTHER, ALREADY_BOOKED_ID, 
                                                ADDITIONAL_REQUEST, BOOKING_STATUS, PLACE_NAME, MAP_URL, DOC_STATUS, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES (:id, :doc_id, :emp_id, :booking, :search, :recommend, :already_booked, :already_booked_other, 
                                                        :already_booked_id, :additional_request, :booking_status, :place_name, :map_url, :doc_status, :create_by, sysdate, :token_update)";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("id", imaxid, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("booking", dtlist[i].booking, "char"));
                                                parameters.Add(context.ConvertTypeParameter("search", dtlist[i].search, "char"));
                                                parameters.Add(context.ConvertTypeParameter("recommend", dtlist[i].recommend, "char"));
                                                parameters.Add(context.ConvertTypeParameter("already_booked", dtlist[i].already_booked, "char"));
                                                parameters.Add(context.ConvertTypeParameter("already_booked_other", dtlist[i].already_booked_other, "char"));
                                                parameters.Add(context.ConvertTypeParameter("already_booked_id", dtlist[i].already_booked_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("additional_request", dtlist[i].additional_request, "char"));
                                                parameters.Add(context.ConvertTypeParameter("booking_status", dtlist[i].booking_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter("place_name", dtlist[i].place_name, "char"));
                                                parameters.Add(context.ConvertTypeParameter("map_url", dtlist[i].map_url, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter("create_by", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));

                                                imaxid++; // Increment ID after the insertion
                                            }
                                            else if (action_type == "update")
                                            {
                                                sqlstr = @"
                                                UPDATE BZ_DOC_ACCOMMODATION_BOOKING
                                                SET BOOKING = :booking, SEARCH = :search, RECOMMEND = :recommend, ALREADY_BOOKED = :already_booked, 
                                                    ALREADY_BOOKED_OTHER = :already_booked_other, ALREADY_BOOKED_ID = :already_booked_id, 
                                                    ADDITIONAL_REQUEST = :additional_request, BOOKING_STATUS = :booking_status, 
                                                    PLACE_NAME = :place_name, MAP_URL = :map_url, DOC_STATUS = :doc_status, 
                                                    UPDATE_BY = :update_by, UPDATE_DATE = sysdate, TOKEN_UPDATE = :token_update
                                                WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("booking", dtlist[i].booking, "char"));
                                                parameters.Add(context.ConvertTypeParameter("search", dtlist[i].search, "char"));
                                                parameters.Add(context.ConvertTypeParameter("recommend", dtlist[i].recommend, "char"));
                                                parameters.Add(context.ConvertTypeParameter("already_booked", dtlist[i].already_booked, "char"));
                                                parameters.Add(context.ConvertTypeParameter("already_booked_other", dtlist[i].already_booked_other, "char"));
                                                parameters.Add(context.ConvertTypeParameter("already_booked_id", dtlist[i].already_booked_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("additional_request", dtlist[i].additional_request, "char"));
                                                parameters.Add(context.ConvertTypeParameter("booking_status", dtlist[i].booking_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter("place_name", dtlist[i].place_name, "char"));
                                                parameters.Add(context.ConvertTypeParameter("map_url", dtlist[i].map_url, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter("update_by", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));
                                                parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                            }
                                            else if (action_type == "delete")
                                            {
                                                sqlstr = @" DELETE FROM BZ_DOC_ACCOMMODATION_BOOKING
                                                         WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                            }

                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                        }
                                    }

                                    if (data.accommodation_detail.Count > 0 && ret == "true")
                                    {
                                        List<accommodationList> dtlist = data.accommodation_detail;
                                        for (int i = 0; i < dtlist.Count; i++)
                                        {
                                            ret = "true";
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
                                                INSERT INTO BZ_DOC_ACCOMMODATION_DETAIL
                                                (ID, DOC_ID, EMP_ID, COUNTRY, HOTEL_NAME, CHECK_IN, CHECK_OUT, ROOMTYPE, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES (:id, :doc_id, :emp_id, :country, :hotel_name, :check_in, :check_out, :roomtype, :create_by, sysdate, :token_update)";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("id", imaxidSub, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("country", dtlist[i].country, "char"));
                                                parameters.Add(context.ConvertTypeParameter("hotel_name", dtlist[i].hotel_name, "char"));
                                                parameters.Add(context.ConvertTypeParameter("check_in", dtlist[i].check_in, "char"));
                                                parameters.Add(context.ConvertTypeParameter("check_out", dtlist[i].check_out, "char"));
                                                parameters.Add(context.ConvertTypeParameter("roomtype", dtlist[i].roomtype, "char"));
                                                parameters.Add(context.ConvertTypeParameter("create_by", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));

                                                imaxidSub++;
                                            }
                                            else if (action_type == "update")
                                            {
                                                sqlstr = @"
                                                UPDATE BZ_DOC_ACCOMMODATION_DETAIL
                                                SET COUNTRY = :country, HOTEL_NAME = :hotel_name, CHECK_IN = :check_in, CHECK_OUT = :check_out, 
                                                    ROOMTYPE = :roomtype, UPDATE_BY = :update_by, UPDATE_DATE = sysdate, TOKEN_UPDATE = :token_update
                                                WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";


                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("country", dtlist[i].country, "char"));
                                                parameters.Add(context.ConvertTypeParameter("hotel_name", dtlist[i].hotel_name, "char"));
                                                parameters.Add(context.ConvertTypeParameter("check_in", dtlist[i].check_in, "char"));
                                                parameters.Add(context.ConvertTypeParameter("check_out", dtlist[i].check_out, "char"));
                                                parameters.Add(context.ConvertTypeParameter("roomtype", dtlist[i].roomtype, "char"));
                                                parameters.Add(context.ConvertTypeParameter("update_by", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));
                                                parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                            }
                                            else if (action_type == "delete")
                                            {
                                                sqlstr = @"
                                                DELETE FROM BZ_DOC_ACCOMMODATION_DETAIL
                                                WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                            }

                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                        }
                                    }
                                    if (data.img_list.Count > 0 && ret == "true")
                                    {
                                        ret = "true"; sqlstr = "";
                                        ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                                    }

                                    if (ret == "true")
                                    {
                                        context.SaveChanges();
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
                }
                catch (Exception ex_def) { msg_ex_def = ex_def.Message.ToString() + " Sql " + sqlstr; }

            }
            #endregion set data

            var msg_error = "";
            var msg_status = "Save data";
            if (ret.ToLower() != "")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            if (doc_type == "submit")
            {


                msg_status = "Submit Data";
                var page_name = "accommodation";
                var module_name = doc_type;
                var email_admin = "";
                var email_user_in_doc = "";
                var mail_cc_active = "";
                var role_type = "pmsv_admin";
                var emp_id_user_in_doc = "";
                var email_user_display = "";
                var email_attachments = "";

                List<EmpListOutModel> emp_list = data.emp_list;
                List<mailselectList> mail_list = new List<mailselectList>();

                searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
                if (value.doc_id.ToString().IndexOf("T") > -1)
                {
                    _swd = new searchDocTravelerProfileServices();
                    dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                }

                //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
                mail_cc_active = sqlEmpUserMail(value.token_login);

                List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                if (drempcheck.Count > 0)
                {
                    emp_id_user_in_doc = drempcheck[0].emp_id.ToString();
                    email_user_in_doc = drempcheck[0].userEmail.ToString();
                    email_user_display = drempcheck[0].userDisplay.ToString();
                }

                List<accommodationbookList> company_rec = data.accommodation_booking.Where(a => (a.emp_id == emp_id_user_in_doc)).ToList();
                string module_name_select = "";
                if (data.user_admin == true)
                {
                    if (company_rec != null && company_rec.Count > 0)
                    {
                        if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "1")
                        {
                            module_name_select = "traveler_review";  //032 
                        }
                        else if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "2")
                        {
                            module_name_select = "admin_confirmed";//011 
                            try
                            {
                                List<ImgList> drimgcheck = data.img_list.Where(a => (a.emp_id == emp_id_user_in_doc)).ToList();
                                for (int i = 0; i < drimgcheck.Count; i++)
                                {
                                    if (email_attachments != "") { email_attachments += ";"; }
                                    email_attachments += drimgcheck[i].fullname;
                                }
                            }
                            catch { }
                        }
                        else if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "3")
                        {
                            module_name_select = "admin_not_confirmed";//010 
                        }

                        if (module_name_select == "admin_confirmed")
                        {
                            mail_list.Add(new mailselectList
                            {
                                module = "Accommodation",
                                mail_to = email_user_in_doc,
                                mail_to_display = email_user_display,
                                mail_cc = email_admin,
                                mail_attachments = email_attachments,
                                mail_body_in_form = "",
                                mail_status = "true",
                                action_change = "true",
                                emp_id = emp_id_user_in_doc,
                            });
                        }
                        else
                        {
                            mail_list.Add(new mailselectList
                            {
                                module = "Accommodation",
                                mail_to = email_admin,
                                mail_to_display = email_user_display,
                                mail_cc = email_user_in_doc,
                                mail_attachments = email_attachments,
                                mail_body_in_form = "",
                                mail_status = "true",
                                action_change = "true",
                                emp_id = emp_id_user_in_doc,
                            });
                        }

                    }
                }
                else
                {
                    if (company_rec != null && company_rec.Count > 0)
                    {
                        try
                        {
                            //20211108 1438 ของเดิมเช็ค As Company Recommend เปลี่ยนเป็นเช็คแค่ Ask Booking by Company 
                            //if (company_rec[0].recommend.ToString() == "true")
                            if (company_rec[0].booking.ToString() == "true")
                            {
                                module_name_select = "traveler_request";//009 
                            }
                            else if (company_rec[0].already_booked.ToString() == "true" && company_rec[0].booking_status.ToString() == "1")
                            {
                                //Already Booked, Booked  
                                //TO: Admin - PMSV; Admin - PMDV(if any) ;
                                //CC: Traveler 
                                module_name_select = "traveler_review";//032 
                            }
                            mail_list.Add(new mailselectList
                            {
                                module = "Accommodation",
                                mail_to = email_admin,
                                mail_to_display = email_user_display,
                                mail_cc = email_user_in_doc,
                                mail_attachments = email_attachments,
                                mail_body_in_form = "",
                                mail_status = "true",
                                action_change = "true",
                                emp_id = emp_id_user_in_doc,
                            });
                        }
                        catch { }
                    }
                }

                if (module_name_select != "")
                {
                    ret = "";
                    SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                    ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id, page_name, module_name_select);
                    if (ret.ToLower() != "true")
                    {
                        msg_error = ret;
                    }
                }


                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                AccommodationModel value_load = new AccommodationModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new AccommodationOutModel();
                data = swd.SearchAccommodation(value_load);

            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? msg_status + " succesed." : msg_status + " failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public VisaOutModel SetVisa(VisaOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            #region set data 
            var doc_id = data.doc_id ?? "";
            Boolean user_admin = false;
            string user_id = "";
            string user_role = "";
            sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);
            emp_user_active = user_id;

            Boolean personal_type = (doc_id == "personal" ? true : false);

            string visa_card_id = "";
            string ret = "";
            try
            {
                int imaxid = GetMaxID(TableMaxId.BZ_DATA_VISA);
                int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            if (data.visa_detail.Count > 0)
                            {
                                List<visaList> dtlist = data.visa_detail;
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    Boolean bGotoUpdateDataVisa = true;
                                    Boolean bCheckDoc = false;
                                    DataTable dtdoc_check = new DataTable();
                                    if (!personal_type)
                                    {
                                        //var doc_id = dtlist[i].doc_id.ToString();
                                        var emp_id = dtlist[i].emp_id.ToString();

                                        sqlstr = @"select to_char(nvl( max(to_number(id)),0)+1)as id_key from BZ_DOC_VISA where doc_id = :doc_id and emp_id = :emp_id";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("emp_id", emp_id, "char"));

                                        var refMax = context.TempIdKeyModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList().FirstOrDefault();
                                        bCheckDoc = (refMax?.id_key?.ToString() ?? "0") == "0" ? true : false;
                                    }

                                    ret = "true";
                                    var id_def = "";
                                    var action_type = dtlist[i].action_type.ToString();
                                    if (action_type == "") { continue; }
                                    else if (action_type != "delete")
                                    {
                                        var action_change = dtlist[i].action_change + "";
                                        if (action_change.ToLower() != "true")
                                        {
                                            if (action_type == "update" && bCheckDoc == false)
                                            {
                                                action_type = "insert";
                                                id_def = imaxid.ToString();
                                                imaxid++;
                                                bGotoUpdateDataVisa = false;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    if (bGotoUpdateDataVisa)
                                    {
                                        if (action_type == "insert")
                                        {
                                            sqlstr = @"
                                                    INSERT INTO BZ_DATA_VISA
                                                    (ID, DOC_ID, EMP_ID, VISA_PLACE_ISSUE, VISA_VALID_FROM, VISA_VALID_TO, VISA_VALID_UNTIL, VISA_TYPE, 
                                                    VISA_CATEGORY, VISA_ENTRY, VISA_NAME, VISA_SURNAME, VISA_DATE_BIRTH, VISA_NATIONALITY, PASSPORT_NO, 
                                                    VISA_SEX, VISA_AUTHORIZED_SIGNATURE, VISA_REMARK, VISA_CARD_ID, VISA_SERIAL, DEFAULT_TYPE, 
                                                    CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                    VALUES (:id, :doc_id, :emp_id, :visa_place_issue, :visa_valid_from, :visa_valid_to, :visa_valid_until, 
                                                    :visa_type, :visa_category, :visa_entry, :visa_name, :visa_surname, :visa_date_birth, 
                                                    :visa_nationality, :passport_no, :visa_sex, :visa_authorized_signature, :visa_remark, 
                                                    :visa_card_id, :visa_serial, :default_type, :create_by, sysdate, :token_update)";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("id", imaxid, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", "", "char")); // No doc_id since it maps to emp_id for Visa
                                            parameters.Add(context.ConvertTypeParameter("emp_id", dtlist[i].emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_place_issue", dtlist[i].visa_place_issue, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_valid_from", dtlist[i].visa_valid_from, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_valid_to", dtlist[i].visa_valid_to, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_valid_until", dtlist[i].visa_valid_until, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_type", dtlist[i].visa_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_category", dtlist[i].visa_category, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_entry", dtlist[i].visa_entry, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_name", dtlist[i].visa_name, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_surname", dtlist[i].visa_surname, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_date_birth", dtlist[i].visa_date_birth, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_nationality", dtlist[i].visa_nationality, "char"));
                                            parameters.Add(context.ConvertTypeParameter("passport_no", dtlist[i].passport_no, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_sex", dtlist[i].visa_sex, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_authorized_signature", dtlist[i].visa_authorized_signature, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_remark", dtlist[i].visa_remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_card_id", dtlist[i].visa_card_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_serial", dtlist[i].visa_serial, "char"));
                                            parameters.Add(context.ConvertTypeParameter("default_type", dtlist[i].default_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter("create_by", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter("token_update", token_login, "char"));

                                            //กรณีที่เป็นข้อมูลใหม่ ให้ map id ใหม่ให้กับ Img ด้วย  
                                            if (data.img_list.Count > 0)
                                            {
                                                List<ImgList> drimg = data.img_list.Where(a => (a.id_level_1 == dtlist[i].id & a.emp_id == dtlist[i].emp_id)).ToList();
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
                                            sqlstr = @"UPDATE BZ_DATA_VISA
                                                    SET 
                                                        VISA_PLACE_ISSUE = :visa_place_issue, 
                                                        VISA_VALID_FROM = :visa_valid_from, 
                                                        VISA_VALID_TO = :visa_valid_to, 
                                                        VISA_VALID_UNTIL = :visa_valid_until, 
                                                        VISA_TYPE = :visa_type, 
                                                        VISA_CATEGORY = :visa_category, 
                                                        VISA_ENTRY = :visa_entry, 
                                                        VISA_NAME = :visa_name, 
                                                        VISA_SURNAME = :visa_surname, 
                                                        VISA_DATE_BIRTH = :visa_date_birth, 
                                                        VISA_NATIONALITY = :visa_nationality, 
                                                        PASSPORT_NO = :passport_no, 
                                                        VISA_SEX = :visa_sex, 
                                                        VISA_AUTHORIZED_SIGNATURE = :visa_authorized_signature, 
                                                        VISA_REMARK = :visa_remark, 
                                                        VISA_CARD_ID = :visa_card_id, 
                                                        VISA_SERIAL = :visa_serial, ";

                                            if ((dtlist[i].default_action_change + "").ToString() == "true")
                                            {
                                                sqlstr += @" ,DEFAULT_TYPE = :default_type";
                                            }
                                            sqlstr += @"UPDATE_BY = :update_by
                                                        , UPDATE_DATE = sysdate
                                                        , TOKEN_UPDATE = :token_update 
                                                        WHERE ID = :id AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("visa_place_issue", dtlist[i].visa_place_issue, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_valid_from", dtlist[i].visa_valid_from, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_valid_to", dtlist[i].visa_valid_to, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_valid_until", dtlist[i].visa_valid_until, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_type", dtlist[i].visa_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_category", dtlist[i].visa_category, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_entry", dtlist[i].visa_entry, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_name", dtlist[i].visa_name, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_surname", dtlist[i].visa_surname, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_date_birth", dtlist[i].visa_date_birth, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_nationality", dtlist[i].visa_nationality, "char"));
                                            parameters.Add(context.ConvertTypeParameter("passport_no", dtlist[i].passport_no, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_sex", dtlist[i].visa_sex, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_authorized_signature", dtlist[i].visa_authorized_signature, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_remark", dtlist[i].visa_remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_card_id", dtlist[i].visa_card_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("visa_serial", dtlist[i].visa_serial, "char"));

                                            if ((dtlist[i].default_action_change + "").ToString() == "true")
                                            {
                                                parameters.Add(context.ConvertTypeParameter(":default_type", dtlist[i].default_type, "char"));
                                            }

                                            parameters.Add(context.ConvertTypeParameter(":update_by", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                            id_def = dtlist[i].id.ToString();

                                        }
                                        else if (action_type == "delete")
                                        {
                                            sqlstr = @"DELETE FROM BZ_DATA_VISA WHERE ID = :id  AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                            id_def = dtlist[i].id.ToString();
                                        }

                                        try
                                        {
                                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                            if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                            ;
                                        }
                                        catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                        if (action_type == "delete")
                                        {
                                            sqlstr = @"DELETE FROM BZ_DOC_VISA   WHERE  ID = :id AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (personal_type)
                                                {
                                                    //กรณีนี้อาจจะมีหรือไม่มีก็ได้
                                                    if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                    ;
                                                }
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                        }
                                    }

                                    //เพิ่มข้อมูลใน bz_doc_visa 
                                    if (action_type == "insert" || action_type == "update")
                                    {
                                        if (!personal_type)
                                        {
                                            if ((dtlist[i].default_action_change + "").ToString() == "true" || bCheckDoc == false)
                                            {
                                                sqlstr = @"DELETE FROM BZ_DOC_VISA WHERE ID = :id_def AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":id_def", id_def, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                                try
                                                {
                                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                    if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                    ;
                                                }
                                                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }


                                                sqlstr = @"INSERT INTO BZ_DOC_VISA
                                                        (ID, DOC_ID, EMP_ID, VISA_PLACE_ISSUE, VISA_VALID_FROM, 
                                                        VISA_VALID_TO, VISA_VALID_UNTIL, VISA_TYPE, VISA_CATEGORY, 
                                                        VISA_ENTRY, VISA_NAME, VISA_SURNAME, VISA_DATE_BIRTH, 
                                                        VISA_NATIONALITY, PASSPORT_NO, VISA_SEX, VISA_AUTHORIZED_SIGNATURE, 
                                                        VISA_REMARK, VISA_CARD_ID, VISA_SERIAL, DEFAULT_TYPE, 
                                                        CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                        VALUES 
                                                        (:id_def, :doc_id, :emp_id, :visa_place_issue, :visa_valid_from, 
                                                        :visa_valid_to, :visa_valid_until, :visa_type, :visa_category, 
                                                        :visa_entry, :visa_name, :visa_surname, :visa_date_birth, 
                                                        :visa_nationality, :passport_no, :visa_sex, :visa_authorized_signature, 
                                                        :visa_remark, :visa_card_id, :visa_serial, :default_type, 
                                                        :create_by, sysdate, :token_update)";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":id_def", id_def, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_place_issue", dtlist[i].visa_place_issue, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_valid_from", dtlist[i].visa_valid_from, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_valid_to", dtlist[i].visa_valid_to, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_valid_until", dtlist[i].visa_valid_until, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_type", dtlist[i].visa_type, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_category", dtlist[i].visa_category, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_entry", dtlist[i].visa_entry, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_name", dtlist[i].visa_name, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_surname", dtlist[i].visa_surname, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_date_birth", dtlist[i].visa_date_birth, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_nationality", dtlist[i].visa_nationality, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":passport_no", dtlist[i].passport_no, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_sex", dtlist[i].visa_sex, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_authorized_signature", dtlist[i].visa_authorized_signature, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_remark", dtlist[i].visa_remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_card_id", dtlist[i].visa_card_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":visa_serial", dtlist[i].visa_serial, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":default_type", dtlist[i].default_type, "char"));

                                                parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));

                                                try
                                                {
                                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                    if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                    ;
                                                }
                                                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                            }
                                        }
                                        else
                                        {
                                            //ถ้าแก้ก็จะไปมีผลต่อใบงานใหม่ หรือใบงานที่ยังไม่เคย save ใบไหนเคย save แล้วไม่ต้องสนใจการแก้ไข 
                                        }
                                    }

                                }
                            }

                            if (data.img_list.Count > 0 && ret == "true")
                            {
                                ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                            }

                            if (ret == "true")
                            {
                                context.SaveChanges();
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

            if (ret == "true")
            {
                //update doc status //เนื่องจาก update ทีละ emp id อยุ่แล้ว 
                if (data.visa_detail.Count > 0)
                {
                    string emp_id_select = "";
                    string doc_status = "3";
                    List<EmpListOutModel> drempcheck = data.emp_list.Where(a => (a.mail_status == "true")).ToList();
                    for (int i = 0; i < drempcheck.Count; i++)
                    {
                        emp_id_select = drempcheck[i].emp_id.ToString();
                        try
                        {
                            doc_status = drempcheck[i].doc_status_id.ToString();
                        }
                        catch { }
                    }

                    if (doc_type == "submit" || doc_type == "sendmail_visa_requisition")
                    {
                        if (!user_admin == true)
                        {
                            doc_status = "";
                        }
                    }
                    else
                    {
                        Boolean bCheckStatus = false;
                        if (doc_status == "") { doc_status = "0"; }

                        List<visaList> dtlist = data.visa_detail.Where(a => (a.emp_id == emp_id_select && a.visa_active_in_doc == "true")).ToList();
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            sqlstr = @" select case when to_date(visa_valid_until,'dd Mon yyyy') >= sysdate then '1' else '0' end  id_key
                                 from BZ_DATA_VISA 
                                 where emp_id = :emp_id and visa_card_id in (:visa_card_id)";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("emp_id", emp_id_select, "char"));
                            parameters.Add(context.ConvertTypeParameter("visa_card_id", string.Join(",", dtlist.Select(v => v.visa_card_id)), "char"));

                            var resStatus = context.TempIdKeyModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList();
                            if (resStatus != null && resStatus?.Count > 0)
                            {
                                var drcheck = resStatus.Where(p => p.id_key == "0").ToList();
                                if (drcheck.Count == 0) { doc_status = "4"; bCheckStatus = true; }
                            }
                        }

                        if (bCheckStatus == false)
                        {
                            if (user_admin)
                            {
                                doc_status = "3";
                            }
                            else
                            {
                                doc_status = "2";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(doc_status))
                    {
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                try
                                {
                                    sqlstr = @"update BZ_DOC_VISA set DOC_STATUS = :doc_status where doc_id = :doc_id  and emp_id = :emp_id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("emp_id", emp_id_select, "char"));

                                    try
                                    {
                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                        if (personal_type)
                                        {
                                            //กรณีนี้อาจจะมีหรือไม่มีก็ได้
                                            if (iret > -1) { ret = "true"; } else { ret = "false"; }
                                            ;
                                        }
                                    }
                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }

                                    if (ret == "true")
                                    {
                                        context.SaveChanges();
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
                }
            }

            #endregion set data

            var msg_error = "";
            var msg_status = "Save data";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                if (doc_type == "submit")
                {
                    msg_status = "Submit Data";
                }
                else
                {
                    msg_status = "Save";
                }

                if ((!user_admin && doc_type == "save") || (user_admin && doc_type == "submit") || doc_type == "sendmail_visa_requisition")
                {
                    if (data.doc_id != "personal")
                    {
                        //Auwat 20210630 1200 แก้ไขเนื่องจาก font แจ้งมาว่าไม่ได้ใช้งานเส้นนี้ในการส่ง mail Visa Requisition จะใช้ SendMailVisa
                        //doc_type == "sendmail_visa_requisition" 
                        var page_name = "VISA";
                        var module_name = doc_type;
                        var email_admin = "";
                        var email_user_in_doc = "";
                        var mail_cc_active = "";
                        var role_type = "pmsv_admin";
                        var emp_id_user_in_doc = "";
                        var email_user_display = "";
                        var email_attachments = "";

                        if (doc_type == "save") { if (!user_admin) { module_name = "sendmail_visa_employee_letter"; } }
                        if (doc_type == "submit") { if (user_admin) { module_name = "sendmail_visa_requisition"; } }

                        List<EmpListOutModel> emp_list = data.emp_list;
                        List<mailselectList> mail_list = data.mail_list;
                        searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                        DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                        for (int i = 0; i < dtemplist.Rows.Count; i++)
                        {
                            email_admin += dtemplist.Rows[i]["email"] + ";";
                        }
                        if (value.doc_id.ToString().IndexOf("T") > -1)
                        {
                            _swd = new searchDocTravelerProfileServices();
                            dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                            for (int i = 0; i < dtemplist.Rows.Count; i++)
                            {
                                email_admin += dtemplist.Rows[i]["email"] + ";";
                            }
                        }

                        //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
                        mail_cc_active = sqlEmpUserMail(value.token_login);

                        List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                        if (drempcheck.Count > 0)
                        {
                            emp_id_user_in_doc = drempcheck[0].emp_id.ToString();
                            email_user_in_doc = drempcheck[0].userEmail.ToString();
                            email_user_display = drempcheck[0].userDisplay.ToString();
                        }

                        mail_list = new List<mailselectList>();
                        if (module_name == "sendmail_visa_requisition")
                        {
                            mail_list = data.mail_list.Where(a => ((a.emp_id.ToLower() == emp_id_user_in_doc))).ToList();
                            mail_list[0].module = "VISA Requisition";
                            mail_list[0].mail_to += email_admin;
                            mail_list[0].mail_to_display = email_user_display;
                            mail_list[0].mail_cc += email_user_in_doc;
                            mail_list[0].mail_body_in_form = "";
                            mail_list[0].mail_status = "true";
                            mail_list[0].action_change = "true";
                            mail_list[0].emp_id = emp_id_user_in_doc;

                        }
                        else if (module_name == "sendmail_visa_employee_letter")
                        {
                            mail_list = data.mail_list.Where(a => ((a.emp_id.ToLower() == emp_id_user_in_doc))).ToList();
                            mail_list[0].module = "VISA";
                            mail_list[0].mail_to += email_admin;
                            mail_list[0].mail_to_display = email_user_display;
                            mail_list[0].mail_cc += email_user_in_doc;
                            mail_list[0].mail_body_in_form = "";
                            mail_list[0].mail_status = "true";
                            mail_list[0].action_change = "true";
                            mail_list[0].emp_id = emp_id_user_in_doc;
                        }

                        ret = "";
                        SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                        ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id, page_name, module_name);
                        if (ret.ToLower() != "true")
                        {
                            msg_error = ret;
                        }
                        else
                        {
                            if (module_name == "sendmail_visa_requisition")
                            {
                                msg_status = "Send Visa Requisition Completed";
                            }
                            else
                            {
                                msg_status = "Send Completed";
                            }
                        }
                    }
                }

                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                VisaModel value_load = new VisaModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new VisaOutModel();
                data = swd.SearchVisa(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? msg_status + " succesed." : msg_status + " failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public PassportOutModel SetPassport(PassportOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;
            var semp_id = "";
            try
            {
                #region set data 
                string doc_id = data.doc_id ?? "";

                Boolean personal_type = (doc_id == "personal" ? true : false);

                if (data.passport_detail.Count > 0)
                {
                    int imaxid = GetMaxID(TableMaxId.BZ_DATA_PASSPORT);
                    int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

                    string ret = "";
                    try
                    {
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                try
                                {

                                    if (data.passport_detail.Count > 0)
                                    {
                                        List<passportList> dtlist = data.passport_detail;
                                        for (int i = 0; i < dtlist.Count; i++)
                                        {
                                            parameters = new List<OracleParameter>();

                                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                            string doc_status = "4";

                                            Boolean bCheckDoc = false;

                                            if (!personal_type)
                                            {
                                                sqlstr = "select to_char(count(1)) as id_key from  BZ_DOC_PASSPORT  where doc_id = :doc_id and emp_id = :emp_id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id.ToString(), "char"));

                                                var resList = context.TempIdKeyModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList().FirstOrDefault();
                                                if (resList != null)
                                                {
                                                    try
                                                    {
                                                        if (resList.id_key != "0")
                                                        {
                                                            bCheckDoc = true;
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }

                                            Boolean bCheckUpdateDataPassport = true;
                                            string id_def = "";

                                            var action_type = dtlist[i].action_type.ToString();
                                            if (action_type == "") { continue; }
                                            else if (action_type != "delete")
                                            {
                                                var action_change = dtlist[i].action_change + "";
                                                if (action_change.ToLower() != "true")
                                                {
                                                    if (action_type == "update" && bCheckDoc == false)
                                                    {
                                                        action_type = "insert";
                                                        id_def = imaxid.ToString();
                                                        imaxid++;
                                                        bCheckUpdateDataPassport = false;
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }

                                            }
                                            if (bCheckUpdateDataPassport)
                                            {
                                                if (action_type == "insert")
                                                {
                                                    sqlstr = @"
                                                INSERT INTO BZ_DATA_PASSPORT
                                                (ID, DOC_ID, EMP_ID, PASSPORT_NO, PASSPORT_DATE_ISSUE, 
                                                PASSPORT_DATE_EXPIRE, PASSPORT_TITLE, PASSPORT_NAME, PASSPORT_SURNAME, PASSPORT_DATE_BIRTH,
                                                ACCEPT_TYPE, DEFAULT_TYPE, SORT_BY, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES 
                                                (:id, :doc_id, :emp_id, :passport_no, :passport_date_issue, 
                                                :passport_date_expire, :passport_title, :passport_name, :passport_surname, :passport_date_birth,
                                                :accept_type, :default_type, :sort_by, :create_by, sysdate, :token_update)";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":id", imaxid, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":doc_id", "", "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                                    parameters.Add(context.ConvertTypeParameter(":passport_no", dtlist[i].passport_no, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_issue", dtlist[i].passport_date_issue, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_expire", dtlist[i].passport_date_expire, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_title", dtlist[i].passport_title, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_name", dtlist[i].passport_name, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_surname", dtlist[i].passport_surname, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_birth", dtlist[i].passport_date_birth, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":accept_type", dtlist[i].accept_type, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":default_type", dtlist[i].default_type, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":sort_by", dtlist[i].sort_by, "char")); // Add the sort_by parameter if needed
                                                    parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));

                                                    //กรณีที่เป็นข้อมูลใหม่ ให้ map id ใหม่ให้กับ Img ด้วย  
                                                    if (data.img_list.Count > 0)
                                                    {
                                                        List<ImgList> drimg = data.img_list.Where(a => (a.id_level_1 == dtlist[i].id & a.emp_id == dtlist[i].emp_id)).ToList();
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
                                                UPDATE BZ_DATA_PASSPORT
                                                SET
                                                    PASSPORT_NO = :passport_no,
                                                    PASSPORT_DATE_ISSUE = :passport_date_issue,
                                                    PASSPORT_DATE_EXPIRE = :passport_date_expire,
                                                    PASSPORT_TITLE = :passport_title,
                                                    PASSPORT_NAME = :passport_name,
                                                    PASSPORT_SURNAME = :passport_surname,
                                                    PASSPORT_DATE_BIRTH = :passport_date_birth,
                                                    ACCEPT_TYPE = :accept_type,
                                                    DEFAULT_TYPE = :default_type,
                                                    SORT_BY = :sort_by,
                                                    UPDATE_BY = :update_by,
                                                    UPDATE_DATE = sysdate,
                                                    TOKEN_UPDATE = :token_update
                                                WHERE ID = :id AND EMP_ID = :emp_id";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":passport_no", dtlist[i].passport_no, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_issue", dtlist[i].passport_date_issue, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_expire", dtlist[i].passport_date_expire, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_title", dtlist[i].passport_title, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_name", dtlist[i].passport_name, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_surname", dtlist[i].passport_surname, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_birth", dtlist[i].passport_date_birth, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":accept_type", string.Format("{0}", dtlist[i].accept_type), "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":default_type", dtlist[i].default_type, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":sort_by", dtlist[i].sort_by, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":update_by", emp_user_active, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));


                                                    id_def = dtlist[i].id.ToString();
                                                }
                                                else if (action_type == "delete")
                                                {
                                                    //ไม่ต้องเก็บข้อมูล doc id เนื่องจาก passport ให้ map กับ emp id 
                                                    sqlstr = @" DELETE FROM BZ_DATA_PASSPORT WHERE ID = :id AND EMP_ID = :emp_id";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                                    id_def = dtlist[i].id.ToString();
                                                }
                                                if (!string.IsNullOrEmpty(action_type))
                                                {
                                                    try
                                                    {
                                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                        if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                        ;
                                                    }
                                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }
                                                }

                                                if (action_type == "delete")
                                                {
                                                    sqlstr = @" DELETE FROM BZ_DOC_PASSPORT WHERE ID = :id AND EMP_ID = :emp_id";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                    try
                                                    {
                                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                        if (personal_type)
                                                        {
                                                            if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                            ;
                                                        }
                                                    }
                                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                                }
                                            }

                                            //เพิ่มข้อมูลใน bz_doc_passport  
                                            if (action_type == "insert" || action_type == "update")
                                            {
                                                if (dtlist[i].default_type.ToString() == "true")
                                                {
                                                    List<EmpListOutModel> drempcheck = data.emp_list.Where(a => (a.mail_status == "true")).ToList();
                                                    if (drempcheck.Count > 0)
                                                    {
                                                        semp_id = drempcheck[0].emp_id.ToString();
                                                    }
                                                }
                                                if (!personal_type)
                                                {

                                                    sqlstr = @" DELETE FROM BZ_DOC_PASSPORT WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":id", id_def, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                    try
                                                    {
                                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                        if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                        ;
                                                    }
                                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }



                                                    sqlstr = @"
                                                        INSERT INTO BZ_DOC_PASSPORT
                                                        (ID, DOC_ID, DOC_STATUS, EMP_ID, PASSPORT_NO, PASSPORT_DATE_ISSUE, PASSPORT_DATE_EXPIRE, PASSPORT_TITLE, 
                                                         PASSPORT_NAME, PASSPORT_SURNAME, PASSPORT_DATE_BIRTH, ACCEPT_TYPE, DEFAULT_TYPE, SORT_BY, CREATE_BY, 
                                                         CREATE_DATE, TOKEN_UPDATE)
                                                        VALUES 
                                                        (:id, :doc_id, :doc_status, :emp_id, :passport_no, :passport_date_issue, :passport_date_expire, :passport_title, 
                                                         :passport_name, :passport_surname, :passport_date_birth, :accept_type, :default_type, :sort_by, :create_by, 
                                                         SYSDATE, :token_update)";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":id", id_def, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":doc_status", doc_status, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_no", dtlist[i].passport_no, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_issue", dtlist[i].passport_date_issue, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_expire", dtlist[i].passport_date_expire, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_title", dtlist[i].passport_title, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_name", dtlist[i].passport_name, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_surname", dtlist[i].passport_surname, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":passport_date_birth", dtlist[i].passport_date_birth, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":accept_type", dtlist[i].accept_type, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":default_type", dtlist[i].default_type, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":sort_by", dtlist[i].sort_by, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));

                                                    try
                                                    {
                                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                        if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                        ;
                                                    }
                                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }



                                                    sqlstr = @" UPDATE BZ_DOC_PASSPORT SET default_type = :default_type WHERE EMP_ID = :emp_id";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":default_type", "false", "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                    try
                                                    {
                                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                        if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                                        ;
                                                    }
                                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }


                                                    sqlstr = @" UPDATE BZ_DOC_PASSPORT SET default_type = :default_type
                                                        WHERE ID = :id AND EMP_ID = :emp_id";

                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":default_type", dtlist[i].default_type, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

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
                                    }
                                    if (data.img_list?.Count > 0 && ret == "true")
                                    {
                                        ret = "true"; sqlstr = "";
                                        ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                                    }

                                    if (ret == "true")
                                    {
                                        context.SaveChanges();
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
                }
                #endregion set data

                var msg_error = "";
                if (ret.ToLower() != "true")
                {
                    msg_error = ret + " --> query error :" + sqlstr;
                }
                else
                {
                    searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                    PassportModel value_load = new PassportModel();
                    value_load.token_login = data.token_login;
                    value_load.doc_id = data.doc_id;
                    data = new PassportOutModel();
                    data = swd.SearchPassport(value_load);
                }

                data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
                data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
                data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succesed." : "Save data failed.";
                data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
                data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
                data.after_trip.opt3.status = "Error msg";
                data.after_trip.opt3.remark = msg_error;
            }
            catch (System.Exception ex)
            {
                LoggerFile.write(ex);

            }


            return data;
        }
        private void setDeleteImg(ref List<ImgList> data_img_list, string doc_id, string id, string emp_id)
        {
            //update status = 0 เพื่อแสดงว่าข้อมูล img ถูกลบ
            if (data_img_list.Count > 0)
            {
                List<ImgList> drimg = data_img_list.Where(a => (a.id == id & a.emp_id == emp_id)).ToList();
                if (drimg.Count > 0)
                {
                    drimg[0].status = "0";
                }
            }
        }
        public void wr(string msg)
        {
            try
            {
                string timeStampFile = DateTime.Now.ToString("yyyyMM");
                string path = System.IO.Directory.GetCurrentDirectory();
                string file_Log = @"D:\ebiz\EBiz_Webservice\Table\Log_Ebiz_2WS_" + timeStampFile + ".txt";
                string timeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff");
                string taskComplete = (timeStamp) + " " + msg;
                using (System.IO.StreamWriter w_Log = new System.IO.StreamWriter(file_Log, true))
                {
                    w_Log.WriteLine(taskComplete);
                    w_Log.Close();
                }
            }
            catch { }
        }
        public AllowanceOutModel SetAllowance(AllowanceOutModel value)
        {
            wr("SetAllowance");

            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            #region set data 
            searchDocTravelerProfileServices wssearch = new searchDocTravelerProfileServices();
            DataTable dtm_exchangerate = wssearch.ref_exchangerate();

            if (data.allowance_detail.Count > 0)
            {
                int imaxid = GetMaxID(TableMaxId.BZ_DOC_ALLOWANCE);
                int imaxidSub = GetMaxID(TableMaxId.BZ_DOC_ALLOWANCE_DETAIL);
                int imaxidMail = GetMaxID(TableMaxId.BZ_DOC_ALLOWANCE_MAIL);
                int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

                string ret = "";
                try
                {
                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        if (data.allowance_main.Count > 0)
                        {
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                try
                                {
                                    if (data.allowance_main.Count > 0)
                                    {
                                        List<allowanceList> dtlist = data.allowance_main;
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

                                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                            string doc_status = "";
                                            try
                                            {
                                                doc_status = dtlist[i].doc_status.ToString();
                                            }
                                            catch { }
                                            if (doc_status == "") { doc_status = "1"; }
                                            try
                                            {
                                                if (data.user_admin == true)
                                                {
                                                    if (data.data_type == "submit") { doc_status = "4"; }
                                                    else { doc_status = "3"; }
                                                }
                                            }
                                            catch { }

                                            if (action_type == "insert")
                                            {
                                                sqlstr = @"
                                                INSERT INTO BZ_DOC_ALLOWANCE
                                                (ID, DOC_ID, DOC_STATUS, EMP_ID, GRAND_TOTAL, LUGGAGE_CLOTHING, SENDMAIL_TO_TRAVELER, REMARK,
                                                FILE_TRAVEL_REPORT, FILE_REPORT, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES (:imaxid, :doc_id, :doc_status, :emp_id, :grand_total, :luggage_clothing, :sendmail_to_traveler, :remark,
                                                :file_travel_report, :file_report, :emp_user_active, sysdate, :token_login)";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":imaxid", imaxid, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_status", doc_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":grand_total", dtlist[i].grand_total, "number"));
                                                parameters.Add(context.ConvertTypeParameter(":luggage_clothing", dtlist[i].luggage_clothing, "number"));
                                                parameters.Add(context.ConvertTypeParameter(":sendmail_to_traveler", dtlist[i].sendmail_to_traveler, "number"));
                                                parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":file_travel_report", dtlist[i].file_travel_report, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":file_report", dtlist[i].file_report, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));

                                                imaxid++;
                                            }
                                            else if (action_type == "update")
                                            {
                                                sqlstr = @"
                                                UPDATE BZ_DOC_ALLOWANCE
                                                SET
                                                GRAND_TOTAL = :grand_total,
                                                LUGGAGE_CLOTHING = :luggage_clothing,
                                                SENDMAIL_TO_TRAVELER = :sendmail_to_traveler,
                                                REMARK = :remark,
                                                FILE_TRAVEL_REPORT = :file_travel_report,
                                                FILE_REPORT = :file_report,
                                                DOC_STATUS = :doc_status,
                                                UPDATE_BY = :emp_user_active,
                                                UPDATE_DATE = sysdate,
                                                TOKEN_UPDATE = :token_login
                                                WHERE ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":grand_total", dtlist[i].grand_total, "number"));
                                                parameters.Add(context.ConvertTypeParameter(":luggage_clothing", dtlist[i].luggage_clothing, "number"));
                                                parameters.Add(context.ConvertTypeParameter(":sendmail_to_traveler", dtlist[i].sendmail_to_traveler, "number"));
                                                parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":file_travel_report", dtlist[i].file_travel_report, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":file_report", dtlist[i].file_report, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_status", doc_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            }
                                            else if (action_type == "delete")
                                            {
                                                sqlstr = @"
                                                        DELETE FROM BZ_DOC_ALLOWANCE
                                                        WHERE ID = :id
                                                        AND DOC_ID = :doc_id
                                                        AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            }

                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                        }
                                    }
                                    if (data.allowance_detail.Count > 0)
                                    {
                                        List<allowancedetailList> dtlist = data.allowance_detail;
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

                                            if (action_type == "insert" || action_type == "update")
                                            {
                                                string allowance_date = dtlist[i].allowance_date.ToString();//30 Oct 2019
                                                string allowance_unit = dtlist[i].allowance_unit.ToString();
                                                if (allowance_date != "")
                                                {
                                                    if (allowance_unit == "") { allowance_unit = "USD"; }
                                                    //date_from = 05 JAN 2021
                                                    // DataRow[] drex = dtm_exchangerate.Select("date_from='" + allowance_date + "' and currency_id ='" + allowance_unit + "' ");$
                                                    DataRow[] drex = dtm_exchangerate.AsEnumerable().Where(s => s.Field<string>("date_from") == allowance_date && s.Field<string>("currency_id") == allowance_unit).ToArray();

                                                    if (drex.Length > 0)
                                                    {
                                                        dtlist[i].exchange_rate = drex[0]["exchange_rate"].ToString();
                                                    }
                                                }
                                            }

                                            if (action_type == "insert")
                                            {
                                                sqlstr = @"
                                                INSERT INTO BZ_DOC_ALLOWANCE_DETAIL
                                                (ID, DOC_ID, EMP_ID, ALLOWANCE_DATE, ALLOWANCE_DAYS, ALLOWANCE_LOW, ALLOWANCE_MID, ALLOWANCE_HIGHT,
                                                ALLOWANCE_TOTAL, ALLOWANCE_UNIT, ALLOWANCE_HRS, ALLOWANCE_TYPE_ID, ALLOWANCE_REMARK, ALLOWANCE_EXCHANGE_RATE,
                                                CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES
                                                (:imaxidSub, :doc_id, :emp_id, :allowance_date, :allowance_days, :allowance_low, :allowance_mid, :allowance_hight,
                                                :allowance_total, :allowance_unit, :allowance_hrs, :allowance_type_id, :allowance_remark, :exchange_rate,
                                                :emp_user_active, sysdate, :token_login)";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":imaxidSub", imaxidSub, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_date", dtlist[i].allowance_date, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_days", dtlist[i].allowance_days, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_low", dtlist[i].allowance_low, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_mid", dtlist[i].allowance_mid, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_hight", dtlist[i].allowance_hight, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_total", dtlist[i].allowance_total, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_unit", dtlist[i].allowance_unit, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_hrs", dtlist[i].allowance_hrs, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_type_id", dtlist[i].allowance_type_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_remark", dtlist[i].allowance_remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":exchange_rate", dtlist[i].exchange_rate, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));

                                                imaxidSub++;
                                            }
                                            else if (action_type == "update")
                                            {
                                                sqlstr = @"
                                                UPDATE BZ_DOC_ALLOWANCE_DETAIL
                                                SET
                                                ALLOWANCE_DATE = :allowance_date,
                                                ALLOWANCE_DAYS = :allowance_days,
                                                ALLOWANCE_LOW = :allowance_low,
                                                ALLOWANCE_MID = :allowance_mid,
                                                ALLOWANCE_HIGHT = :allowance_hight,
                                                ALLOWANCE_TOTAL = :allowance_total,
                                                ALLOWANCE_UNIT = :allowance_unit,
                                                ALLOWANCE_HRS = :allowance_hrs,
                                                ALLOWANCE_TYPE_ID = :allowance_type_id,
                                                ALLOWANCE_REMARK = :allowance_remark,
                                                ALLOWANCE_EXCHANGE_RATE = :exchange_rate,
                                                UPDATE_BY = :emp_user_active,
                                                UPDATE_DATE = sysdate,
                                                TOKEN_UPDATE = :token_login
                                                WHERE
                                                ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":allowance_date", dtlist[i].allowance_date, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_days", dtlist[i].allowance_days, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_low", dtlist[i].allowance_low, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_mid", dtlist[i].allowance_mid, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_hight", dtlist[i].allowance_hight, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_total", dtlist[i].allowance_total, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_unit", dtlist[i].allowance_unit, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_hrs", dtlist[i].allowance_hrs, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_type_id", dtlist[i].allowance_type_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":allowance_remark", dtlist[i].allowance_remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":exchange_rate", dtlist[i].exchange_rate, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            }

                                            else if (action_type == "delete")
                                            {
                                                sqlstr = @"
                                                DELETE FROM BZ_DOC_ALLOWANCE_DETAIL
                                                WHERE
                                                ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            }

                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }


                                        }
                                    }
                                    if (data.mail_list.Count > 0)
                                    {
                                        List<mailselectList> dtlist = data.mail_list;
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
                                                INSERT INTO BZ_DOC_ALLOWANCE_MAIL
                                                (ID, DOC_ID, EMP_ID, MAIL_TO, MAIL_CC, MAIL_BCC, MAIL_STATUS, MAIL_REMARK, MAIL_EMP_ID,
                                                CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES
                                                (:imaxidMail, :doc_id, :emp_id, :mail_to, :mail_cc, :mail_bcc, :mail_status, :mail_remark, :mail_emp_id,
                                                :emp_user_active, sysdate, :token_login)";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":imaxidMail", imaxidMail, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_to", dtlist[i].mail_to, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_cc", dtlist[i].mail_cc, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_bcc", dtlist[i].mail_bcc, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_status", dtlist[i].mail_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_remark", dtlist[i].mail_remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_emp_id", dtlist[i].mail_emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));

                                                imaxidMail++;
                                            }
                                            else if (action_type == "update")
                                            {
                                                sqlstr = @"
                                                UPDATE BZ_DOC_ALLOWANCE_MAIL
                                                SET
                                                MAIL_TO = :mail_to,
                                                MAIL_CC = :mail_cc,
                                                MAIL_BCC = :mail_bcc,
                                                MAIL_STATUS = :mail_status,
                                                MAIL_REMARK = :mail_remark,
                                                MAIL_EMP_ID = :mail_emp_id,
                                                UPDATE_BY = :emp_user_active,
                                                UPDATE_DATE = sysdate,
                                                TOKEN_UPDATE = :token_login
                                                WHERE
                                                ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":mail_to", dtlist[i].mail_to, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_cc", dtlist[i].mail_cc, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_bcc", dtlist[i].mail_bcc, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_status", dtlist[i].mail_status, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_remark", dtlist[i].mail_remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":mail_emp_id", dtlist[i].mail_emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            }
                                            else if (action_type == "delete")
                                            {
                                                sqlstr = @"
                                                DELETE FROM BZ_DOC_ALLOWANCE_MAIL
                                                WHERE
                                                ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            }


                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }


                                        }
                                    }
                                    if (data.img_list.Count > 0)
                                    {
                                        ret = "true"; sqlstr = "";
                                        ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                                    }


                                    if (ret == "true")
                                    {
                                        context.SaveChanges();
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
                }
                catch (Exception ex)
                {
                    ret = ex.Message.ToString();
                }

            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                //018_OB/LB/OT/LT : Please submit an i-Petty Cash in Allowance - [Title_Name of traveler]
                //ใช้ SendMailAllowance

                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                AllowanceModel value_load = new AllowanceModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new AllowanceOutModel();
                data = swd.SearchAllowance(value_load);
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
        public ReimbursementOutModel SetReimbursement(ReimbursementOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            #region set data  
            int imaxid = GetMaxID(TableMaxId.BZ_DOC_REIMBURSEMENT);
            int imaxidSub = GetMaxID(TableMaxId.BZ_DOC_REIMBURSEMENT_DETAIL);
            int imaxidMail = GetMaxID(TableMaxId.BZ_DATA_MAIL);
            int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

            var imaxid_def = imaxid;
            var imaxidSub_def = imaxidSub;
            var imaxidMail_def = imaxidMail;
            var imaxidImg_def = imaxidImg;
            if (data.reimbursement_main.Count > 0)
            {
                string ret = "";
                try
                {
                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {

                                if (data.reimbursement_main.Count > 0)
                                {
                                    List<reimbursementList> dtlist = data.reimbursement_main;
                                    for (int i = 0; i < dtlist.Count; i++)
                                    {
                                        ret = "true";
                                        var action_type = dtlist[i].action_type.ToString();
                                        if (action_type == "") { continue; }
                                        else if (action_type != "delete")
                                        {
                                            var action_change = dtlist[i].action_change + "";
                                            if (action_change.ToLower() != "true") { continue; }
                                        }


                                        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                        string doc_status = "";
                                        List<EmpListOutModel> dtemplist = data.emp_list.Where(a => (a.mail_status == "true" && a.emp_id == dtlist[i].emp_id.ToString())).ToList();
                                        for (int j = 0; j < dtemplist.Count; j++)
                                        {
                                            try
                                            {
                                                doc_status = dtemplist[j].doc_status_id.ToString();
                                            }
                                            catch { }
                                            if (doc_status == "") { doc_status = "1"; }
                                            if (data.user_admin == false) { doc_status = "2"; } else { doc_status = "3"; }
                                        }


                                        if (action_type == "insert")
                                        {
                                            sqlstr = @"
                                                INSERT INTO BZ_DOC_REIMBURSEMENT
                                                (ID, DOC_ID, EMP_ID, DOC_STATUS, SENDMAIL_TO_TRAVELER,
                                                FILE_TRAVEL_REPORT, FILE_REPORT, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES (:imaxid, :doc_id, :emp_id, :doc_status, :sendmail_to_traveler,
                                                :file_travel_report, :file_report, :emp_user_active, sysdate, :token_login)";

                                            // Add parameters to the command
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":imaxid", imaxid, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":sendmail_to_traveler", dtlist[i].sendmail_to_traveler, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":file_travel_report", dtlist[i].file_travel_report, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":file_report", dtlist[i].file_report, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));

                                            imaxid++;
                                        }

                                        else if (action_type == "update")
                                        {
                                            sqlstr = @"
                                                UPDATE BZ_DOC_REIMBURSEMENT SET
                                                SENDMAIL_TO_TRAVELER = :sendmail_to_traveler,
                                                FILE_TRAVEL_REPORT = :file_travel_report,
                                                FILE_REPORT = :file_report,
                                                DOC_STATUS = :doc_status,
                                                UPDATE_BY = :emp_user_active,
                                                UPDATE_DATE = sysdate,
                                                TOKEN_UPDATE = :token_login
                                                WHERE ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                            // Add parameters to the command
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":sendmail_to_traveler", dtlist[i].sendmail_to_traveler, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":file_travel_report", dtlist[i].file_travel_report, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":file_report", dtlist[i].file_report, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                        }

                                        else if (action_type == "delete")
                                        {
                                            sqlstr = @"
                                                DELETE FROM BZ_DOC_REIMBURSEMENT
                                                WHERE ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                            // Add parameters to the command
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                        }


                                        try
                                        {
                                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                            if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                            ;
                                        }
                                        catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }


                                    }
                                }

                                if (data.reimbursement_detail.Count > 0 && ret == "true")
                                {
                                    List<reimbursementdetailList> dtlist = data.reimbursement_detail;
                                    for (int i = 0; i < dtlist.Count; i++)
                                    {
                                        ret = "true";
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
                                                INSERT INTO BZ_DOC_REIMBURSEMENT_DETAIL
                                                (ID, DOC_ID, EMP_ID, REIMBURSEMENT_DATE, DETAILS, EXCHANGE_RATE, CURRENCY, AS_OF, TOTAL, GRAND_TOTAL, REMARK,
                                                CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES
                                                (:imaxidSub, :doc_id, :emp_id, :reimbursement_date, :details, :exchange_rate, :currency, :as_of, :total, :grand_total, :remark,
                                                :emp_user_active, sysdate, :token_login)";

                                            // Add parameters to the command
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":imaxidSub", imaxidSub, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":reimbursement_date", dtlist[i].reimbursement_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":details", dtlist[i].details, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":exchange_rate", dtlist[i].exchange_rate, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":currency", dtlist[i].currency, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":as_of", dtlist[i].as_of, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":total", dtlist[i].total, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":grand_total", dtlist[i].grand_total, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));

                                            imaxidSub++;
                                        }
                                        else if (action_type == "update")
                                        {
                                            sqlstr = @"
                                                UPDATE BZ_DOC_REIMBURSEMENT_DETAIL
                                                SET
                                                REIMBURSEMENT_DATE = :reimbursement_date,
                                                DETAILS = :details,
                                                EXCHANGE_RATE = :exchange_rate,
                                                CURRENCY = :currency,
                                                AS_OF = :as_of,
                                                TOTAL = :total,
                                                GRAND_TOTAL = :grand_total,
                                                REMARK = :remark,
                                                UPDATE_BY = :emp_user_active,
                                                UPDATE_DATE = sysdate,
                                                TOKEN_UPDATE = :token_login
                                                WHERE
                                                ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                            // Add parameters to the command
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":reimbursement_date", dtlist[i].reimbursement_date, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":details", dtlist[i].details, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":exchange_rate", dtlist[i].exchange_rate, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":currency", dtlist[i].currency, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":as_of", dtlist[i].as_of, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":total", dtlist[i].total, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":grand_total", dtlist[i].grand_total, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                        }
                                        else if (action_type == "delete")
                                        {
                                            sqlstr = @"
                                                DELETE FROM BZ_DOC_REIMBURSEMENT_DETAIL
                                                WHERE
                                                ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                            // Add parameters to the command
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                        }


                                        try
                                        {
                                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                            if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                            ;
                                        }
                                        catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }



                                    }
                                }

                                if (data.img_list.Count > 0 && ret == "true")
                                {
                                    ret = "true"; sqlstr = "";
                                    ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                                }

                                if (ret == "true")
                                {
                                    context.SaveChanges();
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
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                ReimbursementModel value_load = new ReimbursementModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new ReimbursementOutModel();
                data = swd.SearchReimbursement(value_load);
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
        public bool IsTravelExpenseDetailDuplicate(System.Data.Common.DbConnection connection, string docId, string expenseType, string empId)
        {
            string checkSql = @"
        SELECT COUNT(*)
        FROM BZ_DOC_TRAVELEXPENSE_DETAIL
        WHERE DOC_ID = :doc_id_param
        AND EXPENSE_TYPE = :expense_type_param
        AND EMP_ID = :emp_id_param";

            // ไม่ต้องใช้ 'using (var connection = ...)' ในฟังก์ชันนี้แล้ว
            // เพราะ connection ถูกส่งเข้ามาและคาดว่าถูกจัดการโดยเมธอดที่เรียก
            using (OracleCommand checkCmd = (OracleCommand)connection.CreateCommand())
            {
                checkCmd.CommandText = checkSql;
                checkCmd.Parameters.Add(new OracleParameter(":doc_id_param", docId));
                checkCmd.Parameters.Add(new OracleParameter(":expense_type_param", expenseType));
                checkCmd.Parameters.Add(new OracleParameter(":emp_id_param", empId));

                int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                return existingCount > 0;
            }
        }
        public TravelExpenseOutModel SetTravelExpense(TravelExpenseOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            #region set data  
            int imaxid = GetMaxID(TableMaxId.BZ_DOC_TRAVELEXPENSE);
            int imaxidSub = GetMaxID(TableMaxId.BZ_DOC_TRAVELEXPENSE_DETAIL);
            int imaxidMail = GetMaxID(TableMaxId.BZ_DATA_MAIL);
            int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

            var imaxid_def = imaxid;
            var imaxidSub_def = imaxidSub;
            var imaxidMail_def = imaxidMail;
            var imaxidImg_def = imaxidImg;

            if (doc_type == "cancelled")
            {
                ret = "True";
            }
            else
            {
                if (data.travelexpense_main.Count > 0)
                {

                    string ret = "";
                    try
                    {
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                var dbConnection = context.Database.GetDbConnection();
                                if (dbConnection.State != System.Data.ConnectionState.Open)
                                {
                                    dbConnection.Open();
                                }
                                try
                                {

                                    if (data.travelexpense_main.Count > 0)
                                    {
                                        List<travelexpenseList> dtlist = data.travelexpense_main;
                                        for (int i = 0; i < dtlist.Count; i++)
                                        {
                                            ret = "true";
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
                                                INSERT INTO BZ_DOC_TRAVELEXPENSE
                                                (ID, DOC_ID, EMP_ID, SEND_TO_SAP, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES
                                                (:imaxid, :doc_id, :emp_id, :send_to_sap, :emp_user_active, sysdate, :token_login)";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":imaxid", imaxid, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":send_to_sap", dtlist[i].send_to_sap, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));

                                                imaxid++;
                                            }
                                            else if (action_type == "update")
                                            {
                                                sqlstr = @"
                                                 UPDATE BZ_DOC_TRAVELEXPENSE
                                                 SET
                                                     SEND_TO_SAP = :send_to_sap,
                                                     UPDATE_BY = :emp_user_active,
                                                     UPDATE_DATE = sysdate,
                                                     TOKEN_UPDATE = :token_login
                                                 WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":send_to_sap", dtlist[i].send_to_sap, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_user_active", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_login", token_login, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                            }
                                            else if (action_type == "delete")
                                            {
                                                sqlstr = @" DELETE FROM BZ_DOC_TRAVELEXPENSE WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                            }

                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                        }
                                    }

                                    if (data.travelexpense_detail.Count > 0 && ret == "true")
                                    {
                                        List<travelexpensedetailList> dtlist = data.travelexpense_detail;
                                        for (int i = 0; i < dtlist.Count; i++)
                                        {
                                            ret = "true";
                                            var action_type = dtlist[i].action_type.ToString();
                                            if (action_type == "") { continue; }
                                            else if (action_type != "delete")
                                            {
                                                var action_change = dtlist[i].action_change + "";
                                                if (action_change.ToLower() != "true") { continue; }
                                            }

                                            if (action_type == "insert")
                                            {
                                                string docIdForDetail = dtlist[i].doc_id;
                                                string expenseTypeForCheck = dtlist[i].expense_type;
                                                string empidForDetail = dtlist[i].emp_id;

                                                if (IsTravelExpenseDetailDuplicate(dbConnection, docIdForDetail, expenseTypeForCheck, empidForDetail))
                                                {
                                                    // Console.WriteLine($"Skipping processing for BZ_DOC_TRAVELEXPENSE_DETAIL: Duplicate EXPENSE_TYPE '{expenseTypeForCheck}' for DOC_ID '{docIdForDetail}'.");
                                                    continue;
                                                }
                                                else
                                                {
                                                    sqlstr = @"
                                                INSERT INTO BZ_DOC_TRAVELEXPENSE_DETAIL
                                                (ID, DOC_ID, EMP_ID, EXPENSE_TYPE, DATA_DATE, STATUS, EXCHANGE_RATE, CURRENCY, AS_OF, TOTAL, GRAND_TOTAL, REMARK, STATUS_ACTIVE, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                                VALUES
                                                (:id, :doc_id, :emp_id, :expense_type, :data_date, :status, :exchange_rate, :currency, :as_of, :total, :grand_total, :remark, :status_active, :create_by, SYSDATE, :token_update)";

                                                    // Add parameters to the command
                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter(":id", imaxidSub, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":expense_type", dtlist[i].expense_type, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":data_date", dtlist[i].data_date, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":status", dtlist[i].status, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":exchange_rate", dtlist[i].exchange_rate, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":currency", dtlist[i].currency, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":as_of", dtlist[i].as_of, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":total", dtlist[i].total, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":grand_total", dtlist[i].grand_total, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":status_active", dtlist[i].status_active, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char"));
                                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));

                                                    imaxidSub++;
                                                }

                                            }
                                            else if (action_type == "update")
                                            {
                                                sqlstr = @"
                                                UPDATE BZ_DOC_TRAVELEXPENSE_DETAIL SET
                                                DATA_DATE = :data_date,
                                                STATUS = :status,
                                                EXCHANGE_RATE = :exchange_rate,
                                                CURRENCY = :currency,
                                                AS_OF = :as_of,
                                                TOTAL = :total,
                                                GRAND_TOTAL = :grand_total,
                                                REMARK = :remark,
                                                STATUS_ACTIVE = :status_active,
                                                UPDATE_BY = :update_by,
                                                UPDATE_DATE = SYSDATE,
                                                TOKEN_UPDATE = :token_update
                                                WHERE
                                                ID = :id
                                                AND DOC_ID = :doc_id
                                                AND EMP_ID = :emp_id";

                                                // Add parameters to the command
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":data_date", dtlist[i].data_date, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":status", dtlist[i].status, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":exchange_rate", dtlist[i].exchange_rate, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":currency", dtlist[i].currency, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":as_of", dtlist[i].as_of, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":total", dtlist[i].total, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":grand_total", dtlist[i].grand_total, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":status_active", dtlist[i].status_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":update_by", emp_user_active, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                            }
                                            else if (action_type == "delete")
                                            {
                                                sqlstr = @" delete from BZ_DOC_TRAVELEXPENSE_DETAIL WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            }

                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                        }
                                    }

                                    if (data.img_list.Count > 0)
                                    {
                                        ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                                    }

                                    if (ret == "true")
                                    {
                                        context.SaveChanges();
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

                }
            }
            #endregion set data

            var msg_error = "";
            var msg_text = "Save data";
            var msg_text2 = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                if (doc_type == "sendtosap" || doc_type == "cancelled")
                {
                    string page_name = "travelexpense";

                    var role_type = "pmsv_admin";
                    string email_admin = "";
                    var email_traverler = "";
                    var email_apprver = "";
                    var email_requester = "";
                    var email_user_in_doc = "";
                    var email_user_display = "";
                    var emp_id_user_in_doc = "";
                    searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                    DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                    if (value.doc_id.ToString().IndexOf("T") > -1)
                    {
                        _swd = new searchDocTravelerProfileServices();
                        dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                        for (int i = 0; i < dtemplist.Rows.Count; i++)
                        {
                            email_admin += dtemplist.Rows[i]["email"] + ";";
                        }
                    }
                    List<EmpListOutModel> emp_list = data.emp_list;
                    List<mailselectList> mail_list = new List<mailselectList>();
                    List<ImgList> img_list = data.img_list;

                    string module_name = "";
                    string sap_obj_id = "";

                    //DevFix 20250123 0000 ปิดในส่วนการส่ง SAP iRPA => ใช้บน web แทน
                    if (doc_type == "sendtosap" && false)
                    {
                        module_name = "sendmail_to_sap";
                        msg_text = "Send to SAP";

                        #region กรณีที่เป็นการส่งรายใบงาน 
                        string doc_id = "";
                        string id = "";
                        string emp_id = "";
                        string status_sap = "";
                        string sdate = "";
                        string edate = "";
                        string location = "";
                        if (data.travelexpense_detail.Count > 0)
                        {
                            for (int i = 0; i < data.travelexpense_detail.Count; i++)
                            {
                                var action_type = data.travelexpense_detail[i].action_type.ToString();
                                if (action_type == "") { continue; }
                                else if (action_type != "delete")
                                {
                                    var action_change = data.travelexpense_detail[i].action_change + "";
                                    if (action_change.ToLower() != "true") { continue; }
                                }
                                if (data.travelexpense_detail[i].status_active == "true")
                                {
                                    doc_id = data.travelexpense_detail[i].doc_id;
                                    id = data.travelexpense_detail[i].id;
                                    emp_id = data.travelexpense_detail[i].emp_id;
                                    status_sap = data.travelexpense_detail[i].status;
                                    break;
                                }
                            }
                        }
                        List<EmpListOutModel> dremplist = data.emp_list.Where(a => ((a.emp_id == emp_id) && (a.send_to_sap == "true"))).ToList();
                        if (dremplist.Count > 0)
                        {
                            email_user_in_doc = "";
                            for (int i = 0; i < data.travelexpense_detail.Count; i++)
                            {
                                status_sap = "6";
                                emp_id_user_in_doc = dremplist[0].emp_id.ToString();
                                email_user_in_doc = dremplist[0].userEmail.ToString();
                                email_user_display = dremplist[0].userDisplay.ToString();
                                if (i == 0)
                                {
                                    sdate = dremplist[0].sap_from_date.ToString();
                                    edate = dremplist[0].sap_to_date.ToString();
                                    location = dremplist[0].def_location_id.ToString();

                                    try
                                    {
                                        //WS_ZTHRTEB020.SAP_ZTHRTEB020 ws_sap = new WS_ZTHRTEB020.SAP_ZTHRTEB020();
                                        //ret = ws_sap.ZTHRTEB020_DOC(doc_id, "", sdate, edate, location, token_login);
                                        if (ret.ToLower() != "true")
                                        {
                                            msg_error = " SAP Error :" + ret;
                                        }
                                        else
                                        {
                                            sqlstr = @" select distinct a.doc_id, a.sap_obj_id, a.create_date, type_main
                                                             from BZ_DOC_TRAVELEXPENSE_SAP a 
                                                             where doc_id = '" + doc_id + "' order by to_number(a.sap_obj_id) ";
                                            DataTable dtsap = new DataTable();
                                            if (SetDocService.conn_ExecuteData(ref dtsap, sqlstr) == "")
                                            {
                                                try
                                                {
                                                    int isapcount = dtsap.Rows.Count;
                                                    for (int isap = 0; isap < isapcount; isap++)
                                                    {
                                                        sap_obj_id = dtsap.Rows[0]["sap_obj_id"].ToString();
                                                        if (isap == 0)
                                                        {
                                                            msg_text2 = @"Successfully sent to SAP <br>(ID : " + sap_obj_id + ")";
                                                            break;
                                                        }
                                                    }
                                                    if (isapcount > 1) { msg_text2 += ")"; }
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    catch (Exception ex_msg_sap) { msg_error = " SAP Error2 :" + ex_msg_sap; }
                                    break;
                                }
                            }
                        }

                        #endregion กรณีที่เป็นการส่งรายใบงาน

                        email_user_in_doc = "";
                        mail_list.Add(new mailselectList
                        {
                            module = "sendmail_to_sap",
                            mail_to = email_admin,
                            mail_body_in_form = "",
                            mail_cc = email_user_in_doc,
                            emp_id = emp_id_user_in_doc,
                            mail_status = "true",
                            action_change = "true",
                        });

                        ret = "";
                        SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                        ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, module_name);

                    }
                    else if (doc_type == "cancelled")
                    {
                        module_name = "tripcancelled";
                        #region cancelled
                        var doc_id = data.doc_id;
                        var cancel_reason = "";
                        try
                        {
                            cancel_reason = value.travelexpense_main[0].remark.ToString();
                        }
                        catch { }

                        #region กณีที่ไม่มีข้อมูลให้เพิ่มใหม่ เพื่อใช้ในการตรวจสอบ tracing
                        sqlstr = @"select count(1) as xcount from BZ_DOC_TRAVELEXPENSE where doc_id = '" + doc_id + "'";
                        dt = new DataTable();

                        if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
                        {
                            if (dt.Rows.Count > 0)
                            {
                                if (dt.Rows[0]["xcount"].ToString() == "0")
                                {
                                    if (data.travelexpense_main.Count > 0)
                                    {
                                        List<travelexpenseList> dtlist = data.travelexpense_main;
                                        for (int i = 0; i < dtlist.Count; i++)
                                        {
                                            //ret = "true";

                                            //sqlstr = @" insert into  BZ_DOC_TRAVELEXPENSE (ID,DOC_ID,EMP_ID,SEND_TO_SAP,CREATE_BY,CREATE_DATE,TOKEN_UPDATE) values ( ";

                                            //sqlstr += @" " + imaxid;
                                            //sqlstr += @" ," + ChkSqlStr(dtlist[i].doc_id, 300);
                                            //sqlstr += @" ," + ChkSqlStr(dtlist[i].emp_id, 300);

                                            //sqlstr += @" ," + ChkSqlStr(dtlist[i].send_to_sap, 300);

                                            //sqlstr += @" ," + ChkSqlStr(emp_user_active, 300);//user name login
                                            //sqlstr += @" ,sysdate";
                                            //sqlstr += @" ," + ChkSqlStr(token_login, 300);
                                            //sqlstr += @" )";

                                            //imaxid++;
                                            //ret = execute_data_ex(sqlstr, false);
                                            ret = InsertTravelExpense(
        imaxid++,
        dtlist[i].doc_id,
        dtlist[i].emp_id,
        dtlist[i].send_to_sap,
        emp_user_active,
        token_login
    );

                                            if (ret != "true")
                                            {
                                                // 处理错误
                                                break;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        #endregion กณีที่ไม่มีข้อมูลให้เพิ่มใหม่ เพื่อใช้ในการตรวจสอบ tracing

                        //sqlstr = " update BZ_DOC_TRAVELEXPENSE set STATUS_TRIP_CANCELLED = 'true' , REMARK =  " + ChkSqlStr(cancel_reason, 4000) + "  where doc_id = '" + doc_id + "' ";
                        //ret = execute_data_ex(sqlstr, false);
                        ret = UpdateCancellationStatus(doc_id, cancel_reason);




                        if (ret.ToLower() != "true")
                        {
                            msg_error = ret + " --> query error :" + sqlstr;
                        }
                        else
                        {
                            #region send mail   
                            dtemplist = _swd.refsearch_empapprer_list(doc_id);
                            for (int i = 0; i < dtemplist.Rows.Count; i++)
                            {
                                email_apprver += dtemplist.Rows[i]["email"] + ";";
                            }

                            dtemplist = _swd.refsearch_emprequester_list(doc_id);
                            for (int i = 0; i < dtemplist.Rows.Count; i++)
                            {
                                email_requester += dtemplist.Rows[i]["email"] + ";";
                            }

                            var emp_id_select = "";
                            List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                            if (drempcheck.Count > 0)
                            {
                                for (int i = 0; i < drempcheck.Count; i++)
                                {
                                    if (emp_id_select != "") { emp_id_select += ";"; }
                                    emp_id_select = drempcheck[i].emp_id;
                                    if (email_traverler != "") { email_traverler += ";"; }
                                    email_traverler += drempcheck[i].userEmail;
                                }
                            }

                            mail_list.Add(new mailselectList
                            {
                                emp_id = emp_id_select,
                                mail_to = email_traverler + email_apprver + email_requester,
                                mail_cc = email_admin,
                                mail_body_in_form = " Reason of Cancelled : " + cancel_reason,
                                module = "tripcancelled",
                            });

                            ret = "";
                            SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                            ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, module_name);

                            #endregion send mail 
                        }
                        #endregion cancelled

                    }
                }
                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                TravelExpenseModel value_load = new TravelExpenseModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new TravelExpenseOutModel();
                data = swd.SearchTravelExpense(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? (msg_text + " succesed." + msg_text2) : (msg_text + " data failed.");
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        // แก้ไขเมธอดให้คืนค่าเป็น string แทน bool
        private string InsertTravelExpense(int id, string doc_id, string emp_id, string send_to_sap,
                                    string create_by, string token_update)
        {
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    string sql = @"INSERT INTO BZ_DOC_TRAVELEXPENSE 
                         (ID, DOC_ID, EMP_ID, SEND_TO_SAP, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                         VALUES (:id, :doc_id, :emp_id, :send_to_sap, :create_by, SYSDATE, :token_update)";

                    List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter("id", OracleDbType.Int32, id, ParameterDirection.Input),
                new OracleParameter("doc_id", OracleDbType.Varchar2, doc_id, ParameterDirection.Input),
                new OracleParameter("emp_id", OracleDbType.Varchar2, emp_id, ParameterDirection.Input),
                new OracleParameter("send_to_sap", OracleDbType.Varchar2, send_to_sap, ParameterDirection.Input),
                new OracleParameter("create_by", OracleDbType.Varchar2, create_by, ParameterDirection.Input),
                new OracleParameter("token_update", OracleDbType.Varchar2, token_update, ParameterDirection.Input)
            };

                    int affectedRows = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                    return affectedRows > 0 ? "true" : "false";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private string UpdateCancellationStatus(string doc_id, string cancel_reason)
        {
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    string sql = @"UPDATE BZ_DOC_TRAVELEXPENSE 
                         SET STATUS_TRIP_CANCELLED = 'true', 
                             REMARK = :cancel_reason
                         WHERE doc_id = :doc_id";

                    List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter("cancel_reason", OracleDbType.Varchar2, cancel_reason, ParameterDirection.Input),
                new OracleParameter("doc_id", OracleDbType.Varchar2, doc_id, ParameterDirection.Input)
            };

                    int affectedRows = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                    return affectedRows > 0 ? "true" : "false";
                }
            }
            catch (Exception ex)
            {

                return ex.Message; // หรือ return "false" ก็ได้ตามต้องการ
            }
        }
        public TravelExpenseOutModel SetTravelExpenseMailToSap(TravelExpenseOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value; // This line is fine as 'data' is of type TravelExpenseOutModelToSap
            var emp_user_active = ""; //เอา token_login ไปหา
            var emp_id_active = ""; // value.emp_id;
            var token_login = data.token_login;

            int imaxid = GetMaxID(TableMaxId.BZ_DOC_TRAVELEXPENSE);
            int imaxidSub = GetMaxID(TableMaxId.BZ_DOC_TRAVELEXPENSE_DETAIL);
            int imaxidMail = GetMaxID(TableMaxId.BZ_DATA_MAIL);
            int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

            var imaxid_def = imaxid;
            var imaxidSub_def = imaxidSub;
            var imaxidMail_def = imaxidMail;
            var imaxidImg_def = imaxidImg;
            string msg_text = "";
            string msg_error = "";
            string msg_text2 = "";

            if (doc_type == "sendtosap")
            {
                string page_name = "travelexpense";

                var role_type = "pmsv_admin";
                string email_admin = "";
                var email_traverler = "";
                var email_apprver = "";
                var email_requester = "";
                var email_user_in_doc = "";
                var email_user_display = "";
                var emp_id_user_in_doc = "";
                searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
                if (value.doc_id.ToString().IndexOf("T") > -1)
                {
                    _swd = new searchDocTravelerProfileServices();
                    dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                }
                List<EmpListOutModel> emp_list = data.emp_list;
                List<mailselectList> mail_list = new List<mailselectList>();
                List<ImgList> img_list = data.img_list;

                string module_name = "";
                string sap_obj_id = "";

                //DevFix 20250123 0000 ปิดในส่วนการส่ง SAP iRPA => ใช้บน web แทน
                if (doc_type == "sendtosap")
                {
                    module_name = "sendmail_to_sap";
                    msg_text = "Send to Mail";

                    #region กรณีที่เป็นการส่งรายใบงาน 
                    string doc_id = "";
                    string id = "";
                    string emp_id = "";
                    string status_sap = "";
                    string sdate = "";
                    string edate = "";
                    string location = "";
                    if (data.travelexpense_detail.Count > 0)
                    {
                        for (int i = 0; i < data.travelexpense_detail.Count; i++)
                        {
                            var action_type = data.travelexpense_detail[i].action_type.ToString();
                            if (action_type == "") { continue; }
                            else if (action_type != "delete")
                            {
                                var action_change = data.travelexpense_detail[i].action_change + "";
                                if (action_change.ToLower() != "true") { continue; }
                            }
                            if (data.travelexpense_detail[i].status_active == "true")
                            {
                                doc_id = data.travelexpense_detail[i].doc_id;
                                id = data.travelexpense_detail[i].id;
                                emp_id = data.travelexpense_detail[i].emp_id;
                                status_sap = data.travelexpense_detail[i].status;
                                break;
                            }
                        }
                    }


                    #endregion กรณีที่เป็นการส่งรายใบงาน

                    email_user_in_doc = "";
                    mail_list.Add(new mailselectList
                    {
                        module = "sendmail_to_sap",
                        mail_to = email_admin,
                        mail_body_in_form = "",
                        mail_cc = email_user_in_doc,
                        emp_id = emp_id_user_in_doc,
                        mail_status = "true",
                        action_change = "true",
                    });

                    ret = "";
                    SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                    ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, module_name);

                }

            }


            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? (msg_text + " succesed." + msg_text2) : (msg_text + " data failed.");
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public TravelInsuranceOutModel SetTravelInsurance(TravelInsuranceOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;
            Boolean action_change_emp_select = false;//ใช้เพื่อเช็คในการส่ง mail   


            #region set data   
            var doc_id = value.doc_id ?? "";

            string ret = "";
            try
            {
                if (data.travelInsurance_detail.Count > 0)
                {
                    int imaxid = GetMaxID(TableMaxId.BZ_DOC_INSURANCE);
                    int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                if (data.travelInsurance_detail.Count > 0)
                                {
                                    List<travelinsuranceList> dtlist = data.travelInsurance_detail;
                                    for (int i = 0; i < dtlist.Count; i++)
                                    {
                                        ret = "true";
                                        var action_type = dtlist[i].action_type.ToString();
                                        if (action_type == "") { continue; }
                                        else if (action_type != "delete")
                                        {
                                            var action_change = dtlist[i].action_change + "";
                                            if (action_change.ToLower() != "true") { continue; }
                                        }

                                        //ใช้เพื่อเช็คในการส่ง mail 
                                        try
                                        {

                                            List<EmpListOutModel> drempcheck = data.emp_list.Where(a => ((a.mail_status == "true") && (a.emp_id == dtlist[i].emp_id + ""))).ToList();
                                            if (drempcheck.Count > 0)
                                            {
                                                if (dtlist[i].certificates_no.ToString() != "") { action_change_emp_select = true; }
                                            }
                                        }
                                        catch { }

                                        if (action_type == "insert")
                                        {
                                            sqlstr = @"INSERT INTO BZ_DOC_INSURANCE 
                                            (ID, DOC_ID, EMP_ID, INS_EMP_ID, INS_EMP_NAME, INS_EMP_ORG, INS_EMP_PASSPORT, INS_EMP_AGE,
                                            NAME_BENEFICIARY, RELATIONSHIP, PERIOD_INS_DEST, PERIOD_INS_FROM, PERIOD_INS_TO, DESTINATION, DATE_EXPIRE,
                                            DURATION, BILLING_CHARGE, CERTIFICATES_NO,
                                            INS_EMP_ADDRESS, INS_EMP_OCCUPATION, INS_EMP_TEL, INS_EMP_FAX, INS_PLAN,
                                            AGENT_TYPE, BROKER_TYPE, TRAVEL_AGENT_TYPE, INSURANCE_COMPANY,
                                            INS_BROKER, CERTIFICATES_TOTAL, REMARK, SORT_BY,
                                            CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                                            VALUES 
                                            (:id, :doc_id, :emp_id, :ins_emp_id, :ins_emp_name, :ins_emp_org, :ins_emp_passport, :ins_emp_age,
                                            :name_beneficiary, :relationship, :period_ins_dest, :period_ins_from, :period_ins_to, :destination, :date_expire,
                                            :duration, :billing_charge, :certificates_no,
                                            :ins_emp_address, :ins_emp_occupation, :ins_emp_tel, :ins_emp_fax, :ins_plan,
                                            :agent_type, :broker_type, :travel_agent_type, :insurance_company,
                                            :ins_broker, :certificates_total, :remark, :sort_by,
                                            :create_by, SYSDATE, :token_update)";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":id", imaxid, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_id", dtlist[i].ins_emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_name", dtlist[i].ins_emp_name, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_org", dtlist[i].ins_emp_org, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_passport", dtlist[i].ins_emp_passport, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_age", dtlist[i].ins_emp_age, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":name_beneficiary", dtlist[i].name_beneficiary, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":relationship", dtlist[i].relationship, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":period_ins_dest", dtlist[i].period_ins_dest, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":period_ins_from", dtlist[i].period_ins_from, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":period_ins_to", dtlist[i].period_ins_to, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":destination", dtlist[i].destination, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":date_expire", dtlist[i].date_expire, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":duration", dtlist[i].duration, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":billing_charge", dtlist[i].billing_charge, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":certificates_no", dtlist[i].certificates_no, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_address", dtlist[i].ins_emp_address, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_occupation", dtlist[i].ins_emp_occupation, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_tel", dtlist[i].ins_emp_tel, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_fax", dtlist[i].ins_emp_fax, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_plan", dtlist[i].ins_plan, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":agent_type", dtlist[i].agent_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":broker_type", dtlist[i].broker_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":travel_agent_type", dtlist[i].travel_agent_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":insurance_company", dtlist[i].insurance_company, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_broker", dtlist[i].ins_broker, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":certificates_total", dtlist[i].certificates_total, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":sort_by", dtlist[i].sort_by, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));

                                            imaxid++;
                                        }
                                        else if (action_type == "update")
                                        {
                                            sqlstr = @"UPDATE BZ_DOC_INSURANCE SET 
                                            INS_EMP_ID = :ins_emp_id, INS_EMP_NAME = :ins_emp_name, INS_EMP_ORG = :ins_emp_org, 
                                            INS_EMP_PASSPORT = :ins_emp_passport, INS_EMP_AGE = :ins_emp_age, 
                                            NAME_BENEFICIARY = :name_beneficiary, RELATIONSHIP = :relationship, 
                                            PERIOD_INS_DEST = :period_ins_dest, PERIOD_INS_FROM = :period_ins_from, 
                                            PERIOD_INS_TO = :period_ins_to, DESTINATION = :destination, 
                                            DATE_EXPIRE = :date_expire, DURATION = :duration, 
                                            BILLING_CHARGE = :billing_charge, CERTIFICATES_NO = :certificates_no, 
                                            INS_EMP_ADDRESS = :ins_emp_address, INS_EMP_OCCUPATION = :ins_emp_occupation, 
                                            INS_EMP_TEL = :ins_emp_tel, INS_EMP_FAX = :ins_emp_fax, INS_PLAN = :ins_plan, 
                                            AGENT_TYPE = :agent_type, BROKER_TYPE = :broker_type, 
                                            TRAVEL_AGENT_TYPE = :travel_agent_type, INSURANCE_COMPANY = :insurance_company, 
                                            INS_BROKER = :ins_broker, CERTIFICATES_TOTAL = :certificates_total, 
                                            REMARK = :remark, SORT_BY = :sort_by, 
                                            UPDATE_BY = :update_by, UPDATE_DATE = SYSDATE, TOKEN_UPDATE = :token_update 
                                            WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_id", dtlist[i].ins_emp_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_name", dtlist[i].ins_emp_name, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_org", dtlist[i].ins_emp_org, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_passport", dtlist[i].ins_emp_passport, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_age", dtlist[i].ins_emp_age, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":name_beneficiary", dtlist[i].name_beneficiary, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":relationship", dtlist[i].relationship, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":period_ins_dest", dtlist[i].period_ins_dest, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":period_ins_from", dtlist[i].period_ins_from, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":period_ins_to", dtlist[i].period_ins_to, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":destination", dtlist[i].destination, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":date_expire", dtlist[i].date_expire, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":duration", dtlist[i].duration, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":billing_charge", dtlist[i].billing_charge, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":certificates_no", dtlist[i].certificates_no, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_address", dtlist[i].ins_emp_address, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_occupation", dtlist[i].ins_emp_occupation, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_tel", dtlist[i].ins_emp_tel, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_emp_fax", dtlist[i].ins_emp_fax, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_plan", dtlist[i].ins_plan, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":agent_type", dtlist[i].agent_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":broker_type", dtlist[i].broker_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":travel_agent_type", dtlist[i].travel_agent_type, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":insurance_company", dtlist[i].insurance_company, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":ins_broker", dtlist[i].ins_broker, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":certificates_total", dtlist[i].certificates_total, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":remark", dtlist[i].remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":sort_by", dtlist[i].sort_by, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":update_by", emp_user_active, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                        }
                                        else if (action_type == "delete")
                                        {
                                            sqlstr = @"DELETE FROM BZ_DOC_INSURANCE WHERE ID = :id AND DOC_ID = :doc_id AND EMP_ID = :emp_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":doc_id", dtlist[i].doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                        }

                                        try
                                        {
                                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                            if (iret > 0) { ret = "true"; } else { ret = "false"; break; }
                                            ;
                                        }
                                        catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }

                                    }
                                }

                                if (data.img_list.Count > 0 && ret == "true")
                                {
                                    ret = SetImgList(data.img_list, imaxidImg, emp_user_active, token_login, context);
                                }

                                if (ret == "true")
                                {
                                    context.SaveChanges();
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
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }


            //update doc status //เนื่องจาก update ทีละ emp id อยุ่แล้ว 
            if (data.travelInsurance_detail.Count > 0)
            {
                try
                {
                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        sqlstr = @" select  a.emp_id
                                    from (select count(1) as total, doc_id, emp_id from BZ_DOC_INSURANCE  group by  doc_id, emp_id)  a
                                    inner join (select count(1) as total, doc_id, emp_id from BZ_DOC_INSURANCE where (certificates_no is not null )  group by  doc_id, emp_id )  b
                                    on a.doc_id = b.doc_id and a.emp_id = b.emp_id
                                    where (a.total - b.total) = 0  and  a.doc_id = :doc_id ";

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter(":doc_id", doc_id, "char"));
                        var resList = context.TempEmpIdModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList();

                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                List<EmpListOutModel> dtlist = data.emp_list.Where(a => (a.mail_status == "true")).ToList();
                                for (int i = 0; i < dtlist.Count; i++)
                                {
                                    //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                    string doc_status = "";
                                    string emp_id_select = dtlist[i].emp_id ?? "";
                                    try
                                    {
                                        doc_status = dtlist[i].doc_status_id?.ToString() ?? "";
                                    }
                                    catch { }
                                    if (doc_status == "") { doc_status = "1"; }
                                    if (data.user_admin == false) { doc_status = "2"; } else { doc_status = "3"; }


                                    Boolean bCheckStatus = false;
                                    if (doc_type == "submit")
                                    {
                                        var checkEmpActive = resList.Where(p => p.emp_id == emp_id_select).ToList().FirstOrDefault();
                                        if (checkEmpActive != null)
                                        {
                                            try
                                            {
                                                if (!string.IsNullOrEmpty(checkEmpActive?.emp_id?.ToString()))
                                                {
                                                    doc_status = "4"; bCheckStatus = true;
                                                }
                                            }
                                            catch { }
                                        }
                                    }



                                    if (bCheckStatus == true)
                                    {
                                        doc_status = "4";
                                    }

                                    sqlstr = @"update BZ_DOC_INSURANCE set DOC_STATUS = :doc_status where doc_id = :doc_id and emp_id = :emp_id ";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("emp_id", emp_id_select, "char"));

                                    try
                                    {
                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                        if (iret > -1) { ret = "true"; } else { ret = "false"; break; }
                                        ;
                                    }
                                    catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); break; }


                                    dtlist[i].doc_status_id = doc_status;

                                }

                                if (ret == "true")
                                {
                                    context.SaveChanges();
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
            }

            #endregion set data

            var msg_error = "";
            var msg_status = "";
            if (doc_type == "save") { msg_status = "Save data"; }
            if (doc_type == "submit") { msg_status = "Submit data"; }
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                //--> 022_OB/LB/OT/LT : Travel Insurance Certificate has been completed - [Title_Name of traveler]
                //doc_type = "save" และ status page = complated
                if (doc_type == "submit" && action_change_emp_select == true) { doc_type = "sendmail_to_been_completed"; }
                if (doc_type == "sendmail_to_insurance" || doc_type == "sendmail_to_traveler" || doc_type == "sendmail_to_been_completed")
                {
                    var page_name = "travelinsurance";
                    var module_name = doc_type;
                    var email_admin = "";
                    var email_user_in_doc = "";
                    var email_ins_broker = "";
                    var mail_cc_active = "";
                    var mail_body_in_form = "";
                    var file_outbound_name = "";
                    var file_outbound_path = "";
                    var mail_to_display = "";
                    var email_attachments = "";

                    var role_type = "pmsv_admin";
                    var emp_id_user_in_doc = "";
                    var module = "";

                    List<EmpListOutModel> emp_list = data.emp_list;
                    List<mailselectList> mail_list = data.mail_list;

                    searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                    DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                    if (value.doc_id.ToString().IndexOf("T") > -1)
                    {
                        _swd = new searchDocTravelerProfileServices();
                        dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                        for (int i = 0; i < dtemplist.Rows.Count; i++)
                        {
                            email_admin += dtemplist.Rows[i]["email"] + ";";
                        }
                    }
                    //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
                    mail_cc_active = sqlEmpUserMail(value.token_login);

                    List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                    if (drempcheck.Count > 0)
                    {
                        emp_id_user_in_doc = drempcheck[0].emp_id.ToString();
                        email_user_in_doc = drempcheck[0].userEmail.ToString();
                    }

                    List<travelinsuranceList> drlistcheck = data.travelInsurance_detail.Where(a => (a.emp_id == emp_id_user_in_doc)).ToList();
                    if (drlistcheck.Count > 0)
                    {
                        file_outbound_name = drlistcheck[0].file_outbound_name.ToString();
                        file_outbound_path = drlistcheck[0].file_outbound_path.ToString();
                    }

                    if (doc_type == "sendmail_to_insurance")
                    {
                        msg_status = "Send to Broker";
                        module = "Sendmail to Broker";

                        //send mail 
                        //to : บริษัทฯประกัน???  travelInsurance_detail.ins_broker 
                        //insurance Form ที่เป็นหน้า higlith สีเหลือง น้ำเงิน
                        //*** file ยังไม่แน่ใจว่าสามารถ gen doc tp word ได้หรือไม่ถ้าไม่ได้ ให้ส่ง url 
                        //*** เขียน service แยก ??

                        try
                        {
                            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                            {

                                sqlstr = @"select mb.email ,mb.name as ins_broker_name
                                    from  bz_doc_insurance a
                                    left join bz_master_insurance_company mb on a.ins_broker = mb.id
                                    where to_number(a.id) =  (select max(to_number(a2.id))as id from bz_doc_insurance a2 where a.doc_id = a2.doc_id and a.emp_id = a2.emp_id)
                                    and a.doc_id = :doc_id and a.emp_id = :emp_id ";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter(":doc_id", doc_id, "char"));
                                parameters.Add(context.ConvertTypeParameter(":emp_id", emp_id_user_in_doc, "char"));

                                var resList = context.TemptravelInsuranceModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList();

                                if (resList != null && resList?.Count > 0)
                                {
                                    foreach (var item in resList)
                                    {
                                        email_ins_broker += $"{item.email?.ToString()};";
                                    }
                                    if (resList?.Count > 1)
                                    {
                                        mail_to_display = "All";
                                    }
                                    else { mail_to_display = resList?[0].ins_broker_name?.ToString() ?? ""; }
                                }
                                else
                                {
                                    mail_body_in_form = "ไม่พบข้อมูล Broker ";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ret = ex.Message.ToString();
                        }
                        string _FolderMailAttachments = top.ebiz.helper.AppEnvironment.GeteServerFolder();
                        string mail_attachments = _FolderMailAttachments + @"\DocumentFile\temp\" + file_outbound_name;
                        mail_body_in_form += " <br>คำขอเอาประกันภัยการเดินทางต่างประเทศ : " + file_outbound_name + "(ถ้าแก้ไขเรื่องแนบไฟล์ให้เอาออก)";
                        mail_list = new List<mailselectList>();
                        mail_list.Add(new mailselectList
                        {
                            module = module,
                            mail_to = email_ins_broker,
                            mail_to_display = mail_to_display,
                            mail_body_in_form = mail_body_in_form,
                            mail_cc = email_admin,
                            mail_attachments = mail_attachments,
                            emp_id = emp_id_user_in_doc,
                            mail_status = "true",
                            action_change = "true",
                        });

                        //data.img_list
                    }
                    else if (doc_type == "sendmail_to_traveler")
                    {
                        msg_status = "Send to Traveler";
                        module = "Sendmail to Traveler";
                        //send mail 
                        //to : emp_list.mail_status = "true"
                        //cc : user int mail_list & emp ที่ emp_list.mail_status = "true"

                        mail_list = new List<mailselectList>();
                        mail_list.Add(new mailselectList
                        {
                            module = module,
                            mail_to = email_user_in_doc,
                            mail_body_in_form = mail_body_in_form,
                            mail_cc = email_admin,
                            emp_id = emp_id_user_in_doc,
                            mail_status = "true",
                            action_change = "true",
                        });
                    }
                    else if (doc_type == "sendmail_to_been_completed")
                    {
                        msg_status = "Send to Traveler";
                        module = "Sendmail to Traveler";

                        try
                        {
                            List<ImgList> drimgcheck = data.img_list.Where(a => (a.emp_id == emp_id_user_in_doc) && a.action_type != "delete").ToList();
                            for (int i = 0; i < drimgcheck.Count; i++)
                            {
                                if (email_attachments != "") { email_attachments += ";"; }

                                if ((drimgcheck[i].fullname + "" + "") == "")
                                {
                                    email_attachments += drimgcheck[i].path + "" + drimgcheck[i].filename;
                                }
                                else
                                {
                                    email_attachments += drimgcheck[i].fullname;
                                }
                            }
                        }
                        catch { }

                        //send mail 
                        //to : emp_list.mail_status = "true"
                        //cc : admin 
                        mail_list = new List<mailselectList>();
                        mail_list.Add(new mailselectList
                        {
                            module = module,
                            mail_to = email_user_in_doc,
                            mail_body_in_form = mail_body_in_form,
                            mail_cc = email_admin,
                            mail_attachments = email_attachments,
                            emp_id = emp_id_user_in_doc,
                            mail_status = "true",
                            action_change = "true",
                        });
                    }


                    logService.logModel mLog = new logService.logModel();

                    mLog.module = "SetTravelInsurance" + value.doc_id;
                    mLog.tevent = email_attachments.Length > 50 ? email_attachments.Substring(0, 50) : email_attachments;
                    mLog.ref_id = 0;
                    //mLog.data_log = JsonSerializer.Serialize(value);
                    //logService.insertLog(mLog);

                    ret = "";
                    SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                    ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id, page_name, module_name);
                    if (ret.ToLower() != "true")
                    {
                        msg_error = ret;
                    }
                }

                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                TravelInsuranceModel value_load = new TravelInsuranceModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new TravelInsuranceOutModel();
                data = swd.SearchTravelInsurance(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? msg_status + " succesed." : msg_status + " failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }

        //private string SetISOSRecord(List<isosList> dtlist, string emp_user_active, string token_login, int imaxid)
        //{
        //    string ret = "";
        //    try
        //    {
        //        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
        //        {
        //            using (var transaction = context.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    for (int i = 0; i < dtlist.Count; i++)
        //                    {
        //                        if (dtlist[i].send_mail_type.ToString() != "0") { continue; }

        //                        var sqlstr = @" INSERT INTO BZ_DOC_ISOS_RECORD
        //                    (ID, YEAR, DOC_ID, EMP_ID, ISOS_TYPE_OF_TRAVEL, ISOS_EMP_ID, ISOS_EMP_TITLE, ISOS_EMP_NAME, ISOS_EMP_SURNAME,
        //                     ISOS_EMP_SECTION, ISOS_EMP_DEPARTMENT, ISOS_EMP_FUNCTION, SEND_MAIL_TYPE, INSURANCE_COMPANY_ID, 
        //                     CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
        //                 VALUES
        //                    (:ID, TO_CHAR(SYSDATE,'RRRR'), :DOC_ID, :EMP_ID, :ISOS_TYPE_OF_TRAVEL, :ISOS_EMP_ID, :ISOS_EMP_TITLE, :ISOS_EMP_NAME,
        //                     :ISOS_EMP_SURNAME, :ISOS_EMP_SECTION, :ISOS_EMP_DEPARTMENT, :ISOS_EMP_FUNCTION, :SEND_MAIL_TYPE, 
        //                     :INSURANCE_COMPANY_ID, :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

        //                        var parameters = new List<OracleParameter>();

        //                        parameters.Add(context.ConvertTypeParameter("ID", imaxid, "int"));
        //                        parameters.Add(context.ConvertTypeParameter("DOC_ID", dtlist[i].doc_id, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("EMP_ID", dtlist[i].emp_id, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_TYPE_OF_TRAVEL", dtlist[i].isos_type_of_travel, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_EMP_ID", dtlist[i].isos_emp_id, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_EMP_TITLE", dtlist[i].isos_emp_title, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_EMP_NAME", dtlist[i].isos_emp_name, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_EMP_SURNAME", dtlist[i].isos_emp_surname, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_EMP_SECTION", dtlist[i].isos_emp_section, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_EMP_DEPARTMENT", dtlist[i].isos_emp_department, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("ISOS_EMP_FUNCTION", dtlist[i].isos_emp_function, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("SEND_MAIL_TYPE", dtlist[i].send_mail_type, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("INSURANCE_COMPANY_ID", dtlist[i].insurance_company_id, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("CREATE_BY", emp_user_active, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("TOKEN_UPDATE", token_login, "char"));

        //                        try
        //                        {
        //                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
        //                            if (iret > 0)
        //                            {
        //                                ret = "true";
        //                            }
        //                            else
        //                            {
        //                                ret = "false";
        //                                break;
        //                            }
        //                        }
        //                        catch (Exception ex_Exec)
        //                        {
        //                            ret = ex_Exec.Message;
        //                            break;
        //                        }

        //                        dtlist[i].id = imaxid.ToString();
        //                        imaxid++;
        //                    }

        //                    if (ret == "true")
        //                    {
        //                        context.SaveChanges();
        //                        transaction.Commit();
        //                    }
        //                    else
        //                    {
        //                        transaction.Rollback();
        //                    }
        //                }
        //                catch (Exception ex_tran)
        //                {
        //                    ret = ex_tran.Message.ToString();
        //                    transaction.Rollback();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ret = ex.Message.ToString();
        //    }
        //    return ret;
        //}

        public (bool, string) SetISOSRecord(List<isosList> dtlist, string emp_user_active, string token_login, int startMaxId)
        {
            bool ret = true;
            string msg = "";

            try
            {
                using (var context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        foreach (var item in dtlist)
                        {
                            var sql = @"
                        INSERT INTO BZ_DOC_ISOS_RECORD (
                            ID, DOC_ID, EMP_ID, SEND_MAIL_TYPE,
                            ISOS_TYPE_OF_TRAVEL, ISOS_EMP_ID, ISOS_EMP_TITLE,
                            ISOS_EMP_NAME, ISOS_EMP_SURNAME, ISOS_EMP_SECTION,
                            ISOS_EMP_DEPARTMENT, ISOS_EMP_FUNCTION,
                            INSURANCE_COMPANY_ID,
                            CREATE_BY, CREATE_DATE, TOKEN_UPDATE
                        )
                        VALUES (
                            :ID, :DOC_ID, :EMP_ID, :SEND_MAIL_TYPE,
                            :ISOS_TYPE_OF_TRAVEL, :ISOS_EMP_ID, :ISOS_EMP_TITLE,
                            :ISOS_EMP_NAME, :ISOS_EMP_SURNAME, :ISOS_EMP_SECTION,
                            :ISOS_EMP_DEPARTMENT, :ISOS_EMP_FUNCTION,
                            :INSURANCE_COMPANY_ID,
                            :CREATE_BY, SYSDATE, :TOKEN_UPDATE
                        )
                    ";

                            var parameters = new List<OracleParameter>
                    {
                        context.ConvertTypeParameter("ID", startMaxId++, "number"),
                        context.ConvertTypeParameter("DOC_ID", item.doc_id ?? "", "char"),
                        context.ConvertTypeParameter("EMP_ID", item.emp_id ?? "", "char"),
                        context.ConvertTypeParameter("SEND_MAIL_TYPE", item.send_mail_type ?? "0", "number"),
                        context.ConvertTypeParameter("ISOS_TYPE_OF_TRAVEL", item.isos_type_of_travel ?? "", "char"),
                        context.ConvertTypeParameter("ISOS_EMP_ID", item.isos_emp_id ?? "", "char"),
                        context.ConvertTypeParameter("ISOS_EMP_TITLE", item.isos_emp_title ?? "", "char"),
                        context.ConvertTypeParameter("ISOS_EMP_NAME", item.isos_emp_name ?? "", "char"),
                        context.ConvertTypeParameter("ISOS_EMP_SURNAME", item.isos_emp_surname ?? "", "char"),
                        context.ConvertTypeParameter("ISOS_EMP_SECTION", item.isos_emp_section ?? "", "char"),
                        context.ConvertTypeParameter("ISOS_EMP_DEPARTMENT", item.isos_emp_department ?? "", "char"),
                        context.ConvertTypeParameter("ISOS_EMP_FUNCTION", item.isos_emp_function ?? "", "char"),
                        context.ConvertTypeParameter("INSURANCE_COMPANY_ID", item.insurance_company_id ?? "", "char"),
                        context.ConvertTypeParameter("CREATE_BY", emp_user_active, "char"),
                        context.ConvertTypeParameter("TOKEN_UPDATE", token_login, "char")
                    };

                            var result = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                            if (result <= 0)
                            {
                                ret = false;
                                msg = "Insert failed";
                                transaction.Rollback();
                                break;
                            }
                        }

                        if (ret)
                        {
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return (ret, msg);
        }


        public ISOSOutModel SetISOSMain(ISOSOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var data = value;
            var token_login = data.token_login;
            var doc_type = data.data_type;
            var doc_id = data.doc_id;
            var page_name = "isos";
            var emp_user_active = "";//เอา token_login ไปหา 

            string msg_error = "";
            SetContentHTML(token_login, doc_id, emp_user_active, page_name, value.html_content, value.img_list, value.emp_list, ref msg_error, ref ret, false);

            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                ISOSModel value_load = new ISOSModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new ISOSOutModel();
                data = swd.SearchISOS(value_load);
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

        public void SetContentHTML(
            string token_login, string doc_id, string emp_user_active
            , string page_name, string html_content
            , List<ImgList> img_list
            , List<EmpListOutModel> emp_list
            , ref string msg_error
            , ref string ret
            , Boolean transportation_type)
        {
            #region set data  

            #region genarate file to server
            string content_path = $"DocumentFile/{page_name.ToLower()}";
            string content_name = "data.txt";
            string data_isos = html_content;
            //ข้อมูลเป็นเเบบ Base64 เก็บไว้ใน db ไม่ได้ จึงเก็บไว้ในรูปแบบไฟล์ .txt ที่ \\10.224.43.14\EBiz2\EBiz_Webservice\ExportFile\OB20120006\isos\data.txt
            try
            {
            }
            catch { }

            //C:\inetpub\wwwroot\ebiz_service\ebiz.webservice\ebiz.webservice\DocumentFile\ISOS  
            var file_Log = FileUtil.GetDirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}/wwwroot/{content_path}/{content_name}")?.FullName ?? ""; //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", content_path, content_name);

            using (StreamWriter w_File_Data = new StreamWriter(file_Log, false))
            {
                w_File_Data.WriteLine(data_isos);
                w_File_Data.Close();
            }
            #endregion genarate file to server


            // data.html_content, data.img_list
            #region Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
            string doc_status = "";
            var emp_id_select = "";
            if (transportation_type)
            {
                List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                if (drempcheck.Count > 0)
                {
                    emp_id_select = drempcheck[0].emp_id;
                    try
                    {
                        doc_status = drempcheck[0].doc_status_id?.ToString() ?? "";
                    }
                    catch { }
                }
            }
            #endregion Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed


            int imaxid = GetMaxID(TableMaxId.BZ_DATA_CONTENT);
            int imaxidImg = GetMaxID(TableMaxId.BZ_DOC_IMG);

            ret = "";
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            sqlstr = "delete from BZ_DATA_CONTENT where page_name = :page_name";
                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("page_name", page_name, "char"));
                            try
                            {
                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                if (iret > -1) { ret = "true"; } else { ret = "false"; }
                                ;
                            }
                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }

                            if (ret == "true")
                            {
                                sqlstr = @"INSERT INTO BZ_DATA_CONTENT 
                                                    (ID, PAGE_NAME, CONTENT_PATH, CONTENT_NAME, REMARK, CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                                                    VALUES 
                                                    (:id, :page_name, :content_path, :content_name, :remark, :create_by, SYSDATE, :token_update)";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter(":id", imaxid, "char"));
                                parameters.Add(context.ConvertTypeParameter(":page_name", page_name, "char"));
                                parameters.Add(context.ConvertTypeParameter(":content_path", content_path, "char"));
                                parameters.Add(context.ConvertTypeParameter(":content_name", content_name, "char"));
                                parameters.Add(context.ConvertTypeParameter(":remark", "", "char")); // ใส่ค่าว่าง
                                parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char")); // user name login
                                parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));

                                try
                                {
                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iret > 0) { ret = "true"; } else { ret = "false"; }
                                    ;
                                }
                                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }
                            }

                            if (img_list.Count > 0 && ret == "true")
                            {
                                for (int i = 0; i < img_list.Count; i++)
                                {
                                    img_list[i].pagename = "isos";
                                }
                                ret = SetImgList(img_list, imaxidImg, emp_user_active, token_login, context);
                            }


                            if (transportation_type && ret == "true")
                            {
                                // เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed

                                sqlstr = @"delete from BZ_DATA_CONTENT_EMP where doc_id = :doc_id and emp_id = :emp_id_select ";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                parameters.Add(context.ConvertTypeParameter("emp_id_select", emp_id_select, "char"));
                                try
                                {
                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iret > -1) { ret = "true"; } else { ret = "false"; }
                                    ;
                                }
                                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }

                                sqlstr = @"insert into  BZ_DATA_CONTENT_EMP ( doc_id, emp_id, doc_status) values ( :doc_id, :emp_id_select, :doc_status )";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                parameters.Add(context.ConvertTypeParameter("emp_id_select", emp_id_select, "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                try
                                {
                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iret > 0) { ret = "true"; } else { ret = "false"; }

                                }
                                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }

                            }


                            if (ret == "true")
                            {
                                context.SaveChanges();
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

            msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
        }

        public TransportationOutModel SetTransportation(TransportationOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var data = value;
            var token_login = data.token_login;
            var doc_type = data.data_type;
            var doc_id = data.doc_id;
            var page_name = "transportation";
            var emp_user_active = "";//เอา token_login ไปหา

            string msg_error = "";
            SetContentHTML(token_login, doc_id, emp_user_active, page_name, value.html_content, value.img_list, value.emp_list, ref msg_error, ref ret, true);

            msg_error = "";
            var msg_status = "Save data";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                //doc_type = submit
                if (doc_type == "submit")
                {
                    //ส่ง mail พร้อมแนบไฟล์/ลิ้ง ส่งให้พนักงาน อาจจะมีพนักงงานหลายคน  
                    string url_personal_car_document = data.url_personal_car_document;

                    msg_status = "Submit Data";
                    var module_name = doc_type;
                    var email_admin = "";
                    var email_user_in_doc = "";
                    var mail_cc_active = "";
                    var role_type = "pmsv_admin";
                    var emp_id_user_in_doc = "";

                    List<EmpListOutModel> emp_list = data.emp_list;
                    List<mailselectList> mail_list = new List<mailselectList>();

                    searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                    DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                    if (value.doc_id.ToString().IndexOf("T") > -1)
                    {
                        _swd = new searchDocTravelerProfileServices();
                        dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                        for (int i = 0; i < dtemplist.Rows.Count; i++)
                        {
                            email_admin += dtemplist.Rows[i]["email"] + ";";
                        }
                    }
                    //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
                    mail_cc_active = sqlEmpUserMail(value.token_login);

                    List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                    if (drempcheck.Count > 0)
                    {
                        for (int i = 0; i < drempcheck.Count; i++)
                        {
                            emp_id_user_in_doc += drempcheck[i].emp_id.ToString() + ";";
                            email_user_in_doc += drempcheck[i].userEmail.ToString() + ";";
                        }
                    }
                    // to travler  cc admin  ตาม mail_status = 'true'
                    mail_list.Add(new mailselectList
                    {
                        module = "Transportation",
                        mail_to = email_user_in_doc,
                        mail_cc = email_admin,
                        mail_attachments = Regex.Replace(url_personal_car_document ?? "", @"(?<!:)/{2,}(DocumentFile/)", "/$1"),
                        mail_body_in_form = "",
                        mail_status = "true",
                        action_change = "true",
                        emp_id = emp_id_user_in_doc,
                    });

                    ret = "";
                    SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                    ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id, page_name, module_name);
                    if (ret.ToLower() != "true")
                    {
                        msg_error = ret;
                    }
                }

                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                TransportationModel value_load = new TransportationModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new TransportationOutModel();
                data = swd.SearchTransportation(value_load);

            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? msg_status + " succesed." : msg_status + " failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        public FeedbackOutModel SetFeedback(FeedbackOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;
            var doc_id = value.doc_id;

            //emp_user_active = get_emp_user();

            #region set data  
            if (data.feedback_detail.Count > 0)
            {
                List<feedbackList> dtlist = data.feedback_detail;
                int imaxid = GetMaxID(TableMaxId.BZ_DOC_FEEDBACK);

                if (data.feedback_detail.Count > 0)
                {
                    #region Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                    string doc_status = "";
                    var emp_id_select = "";
                    List<EmpListOutModel> drempcheck = data.emp_list.Where(a => (a.mail_status == "true")).ToList();
                    if (drempcheck.Count > 0)
                    {
                        emp_id_select = drempcheck[0].emp_id;
                        try
                        {
                            doc_status = drempcheck[0].doc_status_id.ToString();
                        }
                        catch { }
                        if (value.user_admin == true)
                        {
                            //doc_status = "3";
                        }
                        else
                        {
                            doc_status = "2";
                            //doc_type = submit
                            if (doc_type == "submit")
                            {
                                doc_status = "4";
                            }
                        }

                    }
                    #endregion Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed

                    try
                    {
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            for (int i = 0; i < dtlist.Count; i++)
                            {
                                ret = "true";
                                var action_type = dtlist[i].action_type.ToString();
                                if (string.IsNullOrEmpty(action_type)) { continue; }
                                if (action_type != "delete")
                                {
                                    var action_change = dtlist[i].action_change + "";
                                    if (!action_change.Equals("true", StringComparison.OrdinalIgnoreCase)) { continue; }
                                }

                                var sqlstr = "";
                                var parameters = new List<OracleParameter>();

                                if (action_type == "insert")
                                {
                                    sqlstr = @" INSERT INTO BZ_DOC_FEEDBACK
                                (ID, DOC_ID, EMP_ID, DOC_STATUS, 
                                 FEEDBACK_TYPE_ID, FEEDBACK_LIST_ID, FEEDBACK_QUESTION_ID, QUESTION_OTHER, NO, QUESTION, DESCRIPTION, ANSWER, 
                                 CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                             VALUES
                                (:ID, :DOC_ID, :EMP_ID, :DOC_STATUS, 
                                 :FEEDBACK_TYPE_ID, :FEEDBACK_LIST_ID, :FEEDBACK_QUESTION_ID, :QUESTION_OTHER, :NO, :QUESTION, :DESCRIPTION, :ANSWER, 
                                 :CREATE_BY, SYSDATE, :TOKEN_UPDATE)";

                                    parameters.Add(context.ConvertTypeParameter("ID", imaxid, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DOC_ID", dtlist[i].doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("EMP_ID", dtlist[i].emp_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("DOC_STATUS", doc_status, "char"));
                                    parameters.Add(context.ConvertTypeParameter("FEEDBACK_TYPE_ID", dtlist[i].feedback_type_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("FEEDBACK_LIST_ID", dtlist[i].feedback_list_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("FEEDBACK_QUESTION_ID", dtlist[i].feedback_question_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("QUESTION_OTHER", dtlist[i].question_other, "char"));
                                    parameters.Add(context.ConvertTypeParameter("NO", dtlist[i].no, "char"));
                                    parameters.Add(context.ConvertTypeParameter("QUESTION", dtlist[i].question, "char"));
                                    parameters.Add(context.ConvertTypeParameter("DESCRIPTION", dtlist[i].description, "char"));
                                    parameters.Add(context.ConvertTypeParameter("ANSWER", dtlist[i].answer, "char"));
                                    parameters.Add(context.ConvertTypeParameter("CREATE_BY", emp_user_active, "char"));
                                    parameters.Add(context.ConvertTypeParameter("TOKEN_UPDATE", token_login, "char"));

                                    imaxid++;
                                }
                                else if (action_type == "update")
                                {
                                    sqlstr = @" UPDATE BZ_DOC_FEEDBACK SET 
                                 FEEDBACK_TYPE_ID = :FEEDBACK_TYPE_ID, 
                                 FEEDBACK_LIST_ID = :FEEDBACK_LIST_ID, 
                                 FEEDBACK_QUESTION_ID = :FEEDBACK_QUESTION_ID, 
                                 QUESTION_OTHER = :QUESTION_OTHER, 
                                 NO = :NO, 
                                 QUESTION = :QUESTION, 
                                 DESCRIPTION = :DESCRIPTION, 
                                 ANSWER = :ANSWER, 
                                 DOC_STATUS = :DOC_STATUS, 
                                 UPDATE_BY = :UPDATE_BY, 
                                 UPDATE_DATE = SYSDATE, 
                                 TOKEN_UPDATE = :TOKEN_UPDATE 
                             WHERE 
                                 ID = :ID AND DOC_ID = :DOC_ID AND EMP_ID = :EMP_ID";

                                    parameters.Add(context.ConvertTypeParameter("FEEDBACK_TYPE_ID", dtlist[i].feedback_type_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("FEEDBACK_LIST_ID", dtlist[i].feedback_list_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("FEEDBACK_QUESTION_ID", dtlist[i].feedback_question_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("QUESTION_OTHER", dtlist[i].question_other, "char"));
                                    parameters.Add(context.ConvertTypeParameter("NO", dtlist[i].no, "char"));
                                    parameters.Add(context.ConvertTypeParameter("QUESTION", dtlist[i].question, "char"));
                                    parameters.Add(context.ConvertTypeParameter("DESCRIPTION", dtlist[i].description, "char"));
                                    parameters.Add(context.ConvertTypeParameter("ANSWER", dtlist[i].answer, "char"));
                                    parameters.Add(context.ConvertTypeParameter("DOC_STATUS", doc_status, "char"));
                                    parameters.Add(context.ConvertTypeParameter("UPDATE_BY", emp_user_active, "char"));
                                    parameters.Add(context.ConvertTypeParameter("TOKEN_UPDATE", token_login, "char"));
                                    parameters.Add(context.ConvertTypeParameter("ID", dtlist[i].id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("DOC_ID", dtlist[i].doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("EMP_ID", dtlist[i].emp_id, "char"));
                                }
                                else if (action_type == "delete")
                                {
                                    sqlstr = @" DELETE FROM BZ_DOC_FEEDBACK 
                             WHERE ID = :ID AND DOC_ID = :DOC_ID AND EMP_ID = :EMP_ID";

                                    parameters.Add(context.ConvertTypeParameter("ID", dtlist[i].id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("DOC_ID", dtlist[i].doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("EMP_ID", dtlist[i].emp_id, "char"));
                                }

                                try
                                {
                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iret <= 0)
                                    {
                                        ret = "false";
                                        break;
                                    }
                                }
                                catch (Exception ex_Exec)
                                {
                                    ret = ex_Exec.Message;
                                    break;
                                }

                                if (ret == "true" && action_type != "delete")
                                {
                                    sqlstr = @" UPDATE BZ_DOC_FEEDBACK SET DOC_STATUS = :DOC_STATUS 
                                 WHERE DOC_ID = :DOC_ID AND EMP_ID = :EMP_ID";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("DOC_STATUS", doc_status, "char"));
                                    parameters.Add(context.ConvertTypeParameter("DOC_ID", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("EMP_ID", emp_id_select, "char"));

                                    try
                                    {
                                        var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                        if (iret <= 0)
                                        {
                                            ret = "false";
                                            break;
                                        }
                                    }
                                    catch (Exception ex_Exec)
                                    {
                                        ret = ex_Exec.Message;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ret = ex.Message;
                    }


                }
            }
            #endregion set data


            var msg_error = "";
            var msg_status = "Save data";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                //doc_type = submit
                if (doc_type == "submit")
                {
                    msg_status = "Submit Data";
                    var page_name = "feedback";
                    var module_name = doc_type;
                    var email_admin = "";
                    var email_user_in_doc = "";
                    var mail_cc_active = "";
                    var role_type = "pmsv_admin";
                    var emp_id_user_in_doc = "";

                    List<EmpListOutModel> emp_list = data.emp_list;
                    List<mailselectList> mail_list = new List<mailselectList>();
                    List<ImgList> img_list = new List<ImgList>();

                    searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                    DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                    if (value.doc_id.ToString().IndexOf("T") > -1)
                    {
                        _swd = new searchDocTravelerProfileServices();
                        dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                        for (int i = 0; i < dtemplist.Rows.Count; i++)
                        {
                            email_admin += dtemplist.Rows[i]["email"] + ";";
                        }
                    }
                    //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
                    mail_cc_active = sqlEmpUserMail(value.token_login);

                    List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                    if (drempcheck.Count > 0)
                    {
                        emp_id_user_in_doc = drempcheck[0].emp_id.ToString();
                        email_user_in_doc = drempcheck[0].userEmail.ToString();
                    }
                    // to travler  cc admin  ตาม mail_status = 'true'
                    mail_list.Add(new mailselectList
                    {
                        module = "Feedback",
                        mail_to = email_user_in_doc,
                        mail_cc = email_admin,
                        emp_id = emp_id_user_in_doc,
                        mail_status = "true",
                        action_change = "true",
                        mail_body_in_form = "",
                    });


                    ret = "";
                    SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                    ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, module_name);
                    if (ret.ToLower() != "true")
                    {
                        msg_error = ret;
                    }
                }

                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                FeedbackModel value_load = new FeedbackModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new FeedbackOutModel();
                data = swd.SearchFeedback(value_load);

            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? msg_status + " succesed." : msg_status + " failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }

        public PortalOutModel SetPortal(PortalOutModel value)
        {
            //กรณีนี้ข้อมูลไม่มี type ที่เป็น insert และ delete เนื่องจากมีข้อมูลเพียงชุดเดียว
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            #region set data  
            if (true)
            {
                //img_header,img_personal_profile,img_banner_1,img_banner_2,img_banner_3,img_practice_areas,get_in_touch
                string action_change_imgname = data.action_change_imgname;
                if (action_change_imgname != "")
                {

                    using var context = new TOPEBizCreateTripEntities();
                    using var transaction = context.Database.BeginTransaction();
                    var p = new List<OracleParameter>();
                    sqlstr = @" update BZ_DOC_PORTAL set CREATE_BY = CREATE_BY ";
                    if (action_change_imgname.ToLower() == "img_header")
                    {
                        sqlstr += @" ,IMG_HEADER = :IMG_HEADER";
                        p.Add(context.ConvertTypeParameter("IMG_HEADER", data.img_list[0].img_header, "char"));
                    }
                    else if (action_change_imgname.ToLower() == "img_personal_profile")
                    {
                        sqlstr += @" ,IMG_PERSONAL_PROFILE = :IMG_PERSONAL_PROFILE";
                        p.Add(context.ConvertTypeParameter("IMG_PERSONAL_PROFILE", data.img_list[0].img_personal_profile, "char"));
                    }
                    else if (action_change_imgname.ToLower() == "img_banner_1")
                    {
                        sqlstr += @" ,IMG_BANNER_1 = :IMG_BANNER_1";
                        p.Add(context.ConvertTypeParameter("IMG_BANNER_1", data.img_list[0].img_banner_1, "char"));
                        sqlstr += @" ,URL_BANNER_1 = :URL_BANNER_1";
                        p.Add(context.ConvertTypeParameter("URL_BANNER_1", data.img_list[0].url_banner_1, "char"));
                    }
                    else if (action_change_imgname.ToLower() == "img_banner_2")
                    {

                        sqlstr += @" ,IMG_BANNER_2 = :IMG_BANNER_2";
                        p.Add(context.ConvertTypeParameter("IMG_BANNER_2", data.img_list[0].img_banner_2, "char"));
                        sqlstr += @" ,URL_BANNER_2 = :URL_BANNER_2";
                        p.Add(context.ConvertTypeParameter("URL_BANNER_2", data.img_list[0].url_banner_2, "char"));
                    }
                    else if (action_change_imgname.ToLower() == "img_banner_3")
                    {

                        sqlstr += @" ,IMG_BANNER_3 = :IMG_BANNER_3";
                        p.Add(context.ConvertTypeParameter("IMG_BANNER_3", data.img_list[0].img_banner_3, "char"));
                        sqlstr += @" ,URL_BANNER_3 = :URL_BANNER_3";
                        p.Add(context.ConvertTypeParameter("URL_BANNER_3", data.img_list[0].url_banner_3, "char"));
                    }
                    else if (action_change_imgname.ToLower() == "img_practice_areas")
                    {
                        sqlstr += @" ,IMG_PRACTICE_AREAS = :IMG_PRACTICE_AREAS";
                        p.Add(context.ConvertTypeParameter("IMG_PRACTICE_AREAS", data.img_list[0].img_practice_areas, "char"));
                    }
                    else if (action_change_imgname.ToLower() == "title")
                    {
                        sqlstr += @" ,TEXT_TITLE = :TEXT_TITLE";
                        p.Add(context.ConvertTypeParameter("TEXT_TITLE", data.text_title, "char"));
                        sqlstr += @" ,TEXT_DESC = :TEXT_DESC";
                        p.Add(context.ConvertTypeParameter("TEXT_DESC", data.text_desc, "char"));
                    }
                    else if (action_change_imgname.ToLower() == "get_in_touch")
                    {

                        sqlstr += @" ,TEXT_CONTACT_TITLE = :TEXT_CONTACT_TITLE";
                        p.Add(context.ConvertTypeParameter("TEXT_CONTACT_TITLE", data.text_contact_title, "char"));
                        sqlstr += @" ,TEXT_CONTACT_EMAIL = :TEXT_CONTACT_EMAIL";
                        p.Add(context.ConvertTypeParameter("TEXT_CONTACT_EMAIL", data.text_contact_email, "char"));
                        sqlstr += @" ,TEXT_CONTACT_TEL = :TEXT_CONTACT_TEL";
                        p.Add(context.ConvertTypeParameter("TEXT_CONTACT_TEL", data.text_contact_tel, "char"));
                    }

                    sqlstr += @" ,URL_EMPLOYEE_PRIVACY_CENTER = :URL_EMPLOYEE_PRIVACY_CENTER";
                    sqlstr += @" ,UPDATE_BY = :UPDATE_BY";
                    sqlstr += @" ,UPDATE_DATE = sysdate";
                    sqlstr += @" ,TOKEN_UPDATE = :TOKEN_UPDATE";
                    sqlstr += @" where ID = :ID";

                    p.Add(context.ConvertTypeParameter("URL_EMPLOYEE_PRIVACY_CENTER", data.url_employee_privacy_center, "char"));
                    p.Add(context.ConvertTypeParameter("UPDATE_BY", emp_user_active, "char"));
                    p.Add(context.ConvertTypeParameter("TOKEN_UPDATE", token_login, "char"));
                    p.Add(context.ConvertTypeParameter("ID", data.id, "char"));

                    try
                    {
                        context.Database.ExecuteSqlRaw(sqlstr, p.ToArray());
                        transaction.Commit();
                        ret = "true";
                    }
                    catch (System.Exception ex)
                    {
                        transaction.Rollback();
                    }


                }

            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
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
         

        public ManageRoleOutModel SetManageRole(ManageRoleOutModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = ""; // เอา token_login ไปหา
            var emp_id_active = ""; // value.emp_id;
            var token_login = data.token_login;
            string ret = ""; // สถานะการบันทึกข้อมูล
            #region set data
            if (data.admin_list.Count > 0)
            {
                int imaxid = GetMaxID(TableMaxId.BZ_DATA_MANAGE);
                value.after_add_user = new List<userNewList>();

                List<roleList> drlistCheck = data.admin_list.Where(a => ((a.emp_id == "") && a.action_type != "delete")).ToList();
                for (int i = 0; i < drlistCheck.Count; i++)
                {
                    string userid = drlistCheck[i].username.ToString();
                    string emp_id = "";
                    string _msg = "";
                    // userid ค้นหาใน ad 
                    
                }

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            List<roleList> dtlist = data.admin_list;

                            foreach (var item in dtlist)
                            {
                                var action_type = item.action_type.ToString();
                                if (string.IsNullOrEmpty(action_type)) continue;

                                string sqlstr = "";
                                List<OracleParameter> parameters = new List<OracleParameter>();

                                if (action_type == "insert")
                                {
                                    sqlstr = @"INSERT INTO BZ_DATA_MANAGE
                                (ID, EMP_ID, USER_ID, SUPER_ADMIN, PMSV_ADMIN, PMDV_ADMIN, CONTACT_ADMIN,
                                SORT_BY, STATUS, CREATE_BY, CREATE_DATE, TOKEN_UPDATE)
                                VALUES (:id, :emp_id, :user_id, :super_admin, :pmsv_admin, :pmdv_admin, :contact_admin,
                                :sort_by, :status, :create_by, SYSDATE, :token_update)";
                                    parameters.Add(context.ConvertTypeParameter(":id", imaxid, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":emp_id", item.emp_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":user_id", item.username, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":super_admin", item.super_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":pmsv_admin", item.pmsv_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":pmdv_admin", item.pmdv_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":contact_admin", item.contact_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":sort_by", item.sort_by, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":status", item.status, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                                    imaxid++;
                                }
                                else if (action_type == "update")
                                {
                                    sqlstr = @"UPDATE BZ_DATA_MANAGE SET
                                EMP_ID = :emp_id,
                                USER_ID = :user_id,
                                SUPER_ADMIN = :super_admin,
                                PMSV_ADMIN = :pmsv_admin,
                                PMDV_ADMIN = :pmdv_admin,
                                CONTACT_ADMIN = :contact_admin,
                                SORT_BY = :sort_by,
                                STATUS = :status,
                                UPDATE_BY = :update_by,
                                UPDATE_DATE = SYSDATE,
                                TOKEN_UPDATE = :token_update
                                WHERE ID = :id";
                                    parameters.Add(context.ConvertTypeParameter(":id", item.id, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":emp_id", item.emp_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":user_id", item.username, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":super_admin", item.super_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":pmsv_admin", item.pmsv_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":pmdv_admin", item.pmdv_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":contact_admin", item.contact_admin, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":sort_by", item.sort_by, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":status", item.status, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":update_by", emp_user_active, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                                }
                                else if (action_type == "delete")
                                {
                                    sqlstr = @"DELETE FROM BZ_DATA_MANAGE WHERE ID = :id";
                                    parameters.Add(context.ConvertTypeParameter(":id", item.id, "char"));
                                }

                                // Execute SQL query
                                try
                                {
                                    int iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    ret = (iret > -1) ? "true" : "false";
                                }
                                catch (Exception exExec)
                                {
                                    ret = exExec.Message.ToString();
                                    break;
                                }

                                if (ret.ToLower() != "true") break;
                            }

                            // Commit or rollback transaction based on success
                            if (ret.ToLower() == "true")
                            {
                                // Update roles in another table (vw_bz_users)
                                sqlstr = "UPDATE bz_users a SET a.role_id = (SELECT CASE WHEN b.SUPER_ADMIN = 'true' THEN 1 ELSE 0 END FROM bz_data_manage b WHERE a.userid = b.user_id)";
                                parameters = new List<OracleParameter>();
                                try
                                {
                                    int iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    ret = (iret > 0) ? "true" : "false";
                                }
                                catch (Exception exExec)
                                {
                                    ret = exExec.Message.ToString();
                                }

                                if (ret.ToLower() == "true")
                                {
                                    transaction.Commit();
                                }
                                else
                                {
                                    transaction.Rollback();
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                        catch (Exception exTran)
                        {
                            ret = exTran.Message.ToString();
                            transaction.Rollback();
                        }
                    }
                }
            }
            #endregion set data

            // Prepare return data
            var msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error: " + sqlstr;
            }
            else
            {
                // Load data after saving
                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                ManageRoleModel value_load = new ManageRoleModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = swd.SearchManageRole(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Save data succeeded." : "Save data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            try
            {
                data.after_add_user = value.after_add_user;
            }
            catch { }

            return data;
        }

        public ResendEmailOutModel SetResendEmail(ResendEmailOutModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            #region set data  
            //one item only
            if (data.emailList.Count > 0)
            {
                try
                {
                    var emailList = data.emailList.FirstOrDefault();
                    string id = emailList?.id ?? "";
                    string statussend = "";
                    string errorsend = "";

                    Models.Create_Trip.sendEmailModel dataMail = new Models.Create_Trip.sendEmailModel();

                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        dataMail.doc_id = emailList?.doc_id ?? "";
                        dataMail.step_flow = "resend email" ?? "";
                        dataMail.mail_from = emailList?.fromemail ?? "";
                        dataMail.mail_to = emailList?.torecipients ?? "";
                        dataMail.mail_cc = emailList?.ccrecipients ?? "";
                        dataMail.mail_subject = emailList?.subject ?? "";
                        dataMail.mail_body = emailList?.body ?? "";
                        dataMail.mail_attachments = emailList?.attachments ?? "";

                        // ตรวจสอบสิทธฺ์ที่สามารส่งเมลได้
                        if (ClassConnectionDb.IsAuthorizedRole())
                        {
                            var sm = new SendEmailServiceTravelerProfile();
                            errorsend = sm.SendMail23FlowTrip(dataMail, false);
                        }
                        statussend = (errorsend == "" ? "true" : "false");


                        //STATUSSEND, ERRORSEND, DATESEND
                        sqlstr = @" UPDATE BZ_EMAIL_DETAILS SET
                                            STATUSSEND = :STATUSSEND, 
                                            ERRORSEND = :ERRORSEND, 
                                            DATESEND = SYSDATE  
                                            WHERE ID = :ID";

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("STATUSSEND", statussend, "char"));
                        parameters.Add(context.ConvertTypeParameter("ERRORSEND", errorsend, "char"));
                        parameters.Add(context.ConvertTypeParameter("ID", id, "char"));

                        try
                        {
                            int iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                            ret = (iret > 0) ? "true" : "false";
                        }
                        catch (Exception exExec)
                        {
                            ret = exExec.Message.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ret = ex.Message;
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                ResendEmailModel value_load = new ResendEmailModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new ResendEmailOutModel();
                data = swd.SearchResendEmail(value_load);
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
        // ปรับปรุงเมธอด SetResendEmail เพื่อรองรับ Payload จากหน้าบ้าน
        public ResendEmailOutModel SetResendEmailV2(ResendEmailOutModel value)
        {
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = "";//เอา token_login ไปหา
            var emp_id_active = "";// value.emp_id;
            var token_login = data.token_login;

            string super_admin_mail = "";
            string pmsv_admin_mail = "";
            string pmdv_admin_mail = "";
            string requester_mail = "";
            string initiator_mail = "";
            string on_behalf_of_mail = "";
            string traveler_mail = "";
            string line_approver_mail = "";
            string cap_approver_mail = "";
            string requester_name = "";

            #region set data  
            //one item only
            if (data.emailList.Count > 0)
            {
                try
                {
                    var emailList = data.emailList.FirstOrDefault();
                    string id = emailList?.id ?? "";
                    string doc_id = emailList?.doc_id ?? ""; // ใช้ doc_id จาก emailList แทน
                    string subject = emailList?.subject ?? "";
                    string stepflow = emailList?.stepflow ?? ""; // ดึงค่า stepflow จาก payload
                    string statussend = "";
                    string errorsend = "";

                    Models.Create_Trip.sendEmailModel dataMail = new Models.Create_Trip.sendEmailModel();

                    // ตรวจสอบว่ามีข้อมูลผู้รับในอีเมลหรือไม่
                    string torecipients = emailList?.torecipients ?? "";
                    string ccrecipients = emailList?.ccrecipients ?? "";
                    string bccrecipients = emailList?.bccrecipients ?? "";

                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        if (doc_id != "")
                        {
                            try
                            {
                                // ดึงข้อมูล traveler
                                const string sqlTravel = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                            , b.employeeid as name3, b.orgname as name4
                            from BZ_DOC_TRAVELER_EXPENSE a left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid 
                            left join (select min(dte_id) as id, dh_code, dte_emp_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                            on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id 
                            where a.dh_code = :doc_id and nvl(a.dte_status,0) <> 0 order by s.id";

                                var parametersTravel = new List<OracleParameter>();
                                parametersTravel.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                var tempTravel = context.TempModelList.FromSqlRaw(sqlTravel, parametersTravel.ToArray()).ToList();
                                if (tempTravel != null)
                                {
                                    foreach (var item in tempTravel)
                                    {
                                        traveler_mail += ";" + item.name2;
                                    }
                                }

                                // ดึงข้อมูล initiator
                                string sql = @"SELECT u.EMAIL as initial_mail
                            FROM BZ_DOC_HEAD d
                            JOIN vw_bz_users u ON d.DH_INITIATOR_EMPID = u.EMPLOYEEID
                            WHERE d.DH_CODE = :doc_id";

                                var parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                                var result = context.Database.SqlQueryRaw<EmailResult>(sql, parameters.ToArray()).ToList();

                                if (result != null && result.Count > 0)
                                {
                                    initiator_mail = result[0].initial_mail;
                                }

                                // ดึงข้อมูล Line Approver
                                string sqlLineApprover = @"SELECT u.EMAIL as approver_mail
                            FROM BZ_DOC_HEAD d
                            JOIN vw_bz_users u ON d.DH_LINE_APPROVER_EMPID = u.EMPLOYEEID
                            WHERE d.DH_CODE = :doc_id";

                                var lineParameters = new List<OracleParameter>();
                                lineParameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                var lineResult = context.Database.SqlQueryRaw<EmailResult>(sqlLineApprover, lineParameters.ToArray()).ToList();
                                if (lineResult != null && lineResult.Count > 0)
                                {
                                    line_approver_mail = lineResult[0].approver_mail;
                                }

                                // ดึงข้อมูล CAP Approver
                                string sqlCapApprover = @"SELECT u.EMAIL as approver_mail
                            FROM BZ_DOC_HEAD d
                            JOIN vw_bz_users u ON d.DH_CAP_APPROVER_EMPID = u.EMPLOYEEID
                            WHERE d.DH_CODE = :doc_id";

                                var capParameters = new List<OracleParameter>();
                                capParameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                var capResult = context.Database.SqlQueryRaw<EmailResult>(sqlCapApprover, capParameters.ToArray()).ToList();
                                if (capResult != null && capResult.Count > 0)
                                {
                                    cap_approver_mail = capResult[0].approver_mail;
                                }
                            }
                            catch (Exception ex)
                            {
                                // จัดการข้อผิดพลาด
                            }
                        }

                        // สร้างอินสแตนซ์ของ documentService
                        _documentService documentServiceInstance = new _documentService();

                        // ดึงข้อมูลอีเมลของ admin
                        //  super_admin_mail = documentServiceInstance.get_mail_group_admin(context);
                        super_admin_mail = documentServiceInstance.mail_group_admin(context, "super_admin");
                        pmsv_admin_mail = documentServiceInstance.mail_group_admin(context, "pmsv_admin");
                        if (doc_id.IndexOf("T") > -1)
                        {
                            pmdv_admin_mail += documentServiceInstance.mail_group_admin(context, "pmdv_admin");
                        }

                        // ดึงข้อมูลอีเมลของ requester และ on behalf of
                        documentServiceInstance.get_mail_requester_in_doc(context, doc_id, ref requester_name, ref requester_mail, ref on_behalf_of_mail);

                        // กำหนด flow จาก stepflow ที่ได้รับจาก payload
                        string currentFlow = DetermineFlowFromStepflow(stepflow);
                        bool isRejected = stepflow.Contains("REJECT") || stepflow.Contains("DECLINED") || stepflow.Contains("DENIED");

                        // สร้าง resMailShowCase ตาม flow ที่กำหนด
                        string mailTo = "";
                        string mailCc = "";
                        var resMailShowCase = "<div>";

                        switch (currentFlow)
                        {
                            case "submitFlow1":
                                if (stepflow.Contains("Initiator") || stepflow.Contains("INITIATOR"))
                                {
                                    // ส่งไปยัง Initiator
                                    mailTo = initiator_mail;
                                    mailCc = super_admin_mail + pmsv_admin_mail + pmdv_admin_mail + requester_mail + on_behalf_of_mail + traveler_mail;

                                    resMailShowCase += "To: ";
                                    resMailShowCase += $"{initiator_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Initiator)</span>";
                                    resMailShowCase += "<br>Cc: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span> ";
                                    resMailShowCase += $"{pmdv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMDV Admin)</span> ";
                                    if (!string.IsNullOrEmpty(requester_mail))
                                    {
                                        resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                    {
                                        resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(traveler_mail))
                                    {
                                        resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span> ";
                                    }
                                }
                                else
                                {
                                    // ส่งไปยัง Admin
                                    mailTo = super_admin_mail + pmsv_admin_mail;
                                    mailCc = requester_mail + on_behalf_of_mail;

                                    resMailShowCase += "To: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span>";
                                    resMailShowCase += "<br>Cc: ";
                                    if (!string.IsNullOrEmpty(requester_mail))
                                    {
                                        resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                    {
                                        resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                    }
                                }
                                break;

                            case "submitFlow2":
                                mailTo = line_approver_mail;
                                mailCc = requester_mail + on_behalf_of_mail + traveler_mail + super_admin_mail + pmsv_admin_mail + pmdv_admin_mail;

                                resMailShowCase += "To: ";
                                resMailShowCase += $"{line_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Line Approval)</span>";
                                resMailShowCase += "<br>Cc: ";
                                if (!string.IsNullOrEmpty(requester_mail))
                                {
                                    resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                }
                                if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                {
                                    resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                }
                                if (!string.IsNullOrEmpty(traveler_mail))
                                {
                                    resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span> ";
                                }
                                resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span> ";
                                resMailShowCase += $"{pmdv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMDV Admin)</span> ";
                                break;

                            case "submitFlow3":
                                if (!isRejected) // ส่งไปยัง CAP Approver
                                {
                                    mailTo = cap_approver_mail;
                                    mailCc = line_approver_mail + super_admin_mail + pmsv_admin_mail + pmdv_admin_mail + requester_mail + on_behalf_of_mail + traveler_mail;

                                    resMailShowCase += "To: ";
                                    resMailShowCase += $"{cap_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(CAP Approver)</span>";
                                    resMailShowCase += "<br>Cc: ";
                                    resMailShowCase += $"{line_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Line Approver)</span> ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span> ";
                                    resMailShowCase += $"{pmdv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMDV Admin)</span> ";
                                    if (!string.IsNullOrEmpty(requester_mail))
                                    {
                                        resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                    {
                                        resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(traveler_mail))
                                    {
                                        resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span> ";
                                    }
                                }
                                else // กรณีปฏิเสธ
                                {
                                    mailTo = requester_mail + traveler_mail;
                                    mailCc = super_admin_mail + pmsv_admin_mail + pmdv_admin_mail + line_approver_mail + on_behalf_of_mail;

                                    resMailShowCase += "To: ";
                                    if (!string.IsNullOrEmpty(requester_mail))
                                    {
                                        resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(traveler_mail))
                                    {
                                        resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span> ";
                                    }
                                    resMailShowCase += "<br>Cc: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span> ";
                                    resMailShowCase += $"{pmdv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMDV Admin)</span> ";
                                    resMailShowCase += $"{line_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Line Approval)</span> ";
                                    if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                    {
                                        resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                    }
                                }
                                break;

                            case "submitFlow4":
                                if (!isRejected) // กรณีอนุมัติ
                                {
                                    mailTo = super_admin_mail + pmsv_admin_mail + pmdv_admin_mail + traveler_mail + line_approver_mail + cap_approver_mail + requester_mail + on_behalf_of_mail;
                                    mailCc = "";

                                    resMailShowCase += "To: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span> ";
                                    resMailShowCase += $"{pmdv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMDV Admin)</span> ";
                                    if (!string.IsNullOrEmpty(traveler_mail))
                                    {
                                        resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span> ";
                                    }
                                    resMailShowCase += $"{line_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Line Approver)</span> ";
                                    resMailShowCase += $"{cap_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(CAP Approver)</span> ";
                                    if (!string.IsNullOrEmpty(requester_mail))
                                    {
                                        resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                    {
                                        resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                    }
                                }
                                else // กรณีปฏิเสธ
                                {
                                    mailTo = super_admin_mail + pmsv_admin_mail + pmdv_admin_mail + traveler_mail;
                                    mailCc = cap_approver_mail + line_approver_mail + requester_mail + on_behalf_of_mail;

                                    resMailShowCase += "To: ";
                                    resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Super Admin)</span> ";
                                    resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMSV Admin)</span> ";
                                    resMailShowCase += $"{pmdv_admin_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(PMDV Admin)</span> ";
                                    if (!string.IsNullOrEmpty(traveler_mail))
                                    {
                                        resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Traveler)</span> ";
                                    }
                                    resMailShowCase += "<br>Cc: ";
                                    resMailShowCase += $"{cap_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(CAP Approver)</span> ";
                                    resMailShowCase += $"{line_approver_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Line Approver)</span> ";
                                    if (!string.IsNullOrEmpty(requester_mail))
                                    {
                                        resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(Requester)</span> ";
                                    }
                                    if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                    {
                                        resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} <span style='color:#666;'>(On Behalf Of)</span> ";
                                    }
                                }
                                break;

                            default:
                                // ใช้ข้อมูลจาก payload โดยตรง
                                mailTo = torecipients;
                                mailCc = ccrecipients;

                                // แยกอีเมลและแสดงผลตามรูปแบบที่ต้องการ
                                resMailShowCase += "To: ";
                                var toEmails = torecipients.Split(';').Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
                                foreach (var email in toEmails)
                                {
                                    string role = DetermineEmailRole(email, super_admin_mail, pmsv_admin_mail, pmdv_admin_mail,
                                        requester_mail, initiator_mail, on_behalf_of_mail, traveler_mail, line_approver_mail, cap_approver_mail);
                                    resMailShowCase += $"{email.ToLower()} <span style='color:#666;'>({role})</span> ";
                                }

                                resMailShowCase += "<br>Cc: ";
                                var ccEmails = ccrecipients.Split(';').Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
                                foreach (var email in ccEmails)
                                {
                                    string role = DetermineEmailRole(email, super_admin_mail, pmsv_admin_mail, pmdv_admin_mail,
                                        requester_mail, initiator_mail, on_behalf_of_mail, traveler_mail, line_approver_mail, cap_approver_mail);
                                    resMailShowCase += $"{email.ToLower()} <span style='color:#666;'>({role})</span> ";
                                }
                                break;
                        }

                        resMailShowCase += "</div>";

                        // กำหนดค่า mail_to และ mail_cc ตามที่กำหนดไว้ในแต่ละ flow
                        // ถ้ามีการกำหนดค่าใน payload ให้ใช้ค่าจาก payload
                        dataMail.mail_to = string.IsNullOrEmpty(torecipients) ? mailTo : torecipients;
                        dataMail.mail_cc = string.IsNullOrEmpty(ccrecipients) ? mailCc : ccrecipients;
                        dataMail.mail_show_case = resMailShowCase;

                        // กำหนดค่าอื่นๆ จาก payload
                        dataMail.doc_id = emailList?.doc_id ?? "";
                        dataMail.step_flow = "resend email" ?? "";
                        dataMail.mail_from = emailList?.fromemail ?? "";
                        dataMail.mail_subject = emailList?.subject ?? "";
                        dataMail.mail_body = emailList?.body ?? "";
                        dataMail.mail_attachments = emailList?.attachments ?? "";

                        // ตรวจสอบสิทธฺ์ที่สามารส่งเมลได้
                        if (ClassConnectionDb.IsAuthorizedRole( ))
                        {
                            var swmail = new SendEmailServiceTravelerProfile();
                            errorsend = swmail.SendMail23FlowTrip(dataMail, false);
                        }
                        statussend = (errorsend == "" ? "true" : "false");

                        //STATUSSEND, ERRORSEND, DATESEND
                        sqlstr = @" UPDATE BZ_EMAIL_DETAILS SET
                                  STATUSSEND = :STATUSSEND, 
                                  ERRORSEND = :ERRORSEND, 
                                  DATESEND = SYSDATE  
                                  WHERE ID = :ID";

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("STATUSSEND", statussend, "char"));
                        parameters.Add(context.ConvertTypeParameter("ERRORSEND", errorsend, "char"));
                        parameters.Add(context.ConvertTypeParameter("ID", id, "char"));

                        try
                        {
                            int iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                            ret = (iret > 0) ? "true" : "false";
                        }
                        catch (Exception exExec)
                        {
                            ret = exExec.Message.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ret = ex.Message;
                }
            }
            #endregion set data

            var msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                ResendEmailModel value_load = new ResendEmailModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new ResendEmailOutModel();
                data = swd.SearchResendEmail(value_load);
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

        // เมธอดสำหรับกำหนด flow จาก stepflow
        private string DetermineFlowFromStepflow(string stepflow)
        {
            if (string.IsNullOrEmpty(stepflow))
                return "";

            stepflow = stepflow.ToUpper();

            if (stepflow.Contains("PENDING BY SUPER ADMIN") ||
                stepflow.Contains("DRAFT") ||
                stepflow.Contains("INITIATOR"))
            {
                return "submitFlow1";
            }
            else if (stepflow.Contains("ESTIMATE") ||
                     stepflow.Contains("SUBMIT AN ESTIMATE"))
            {
                return "submitFlow2";
            }
            else if (stepflow.Contains("LINE APPROVAL") ||
                     stepflow.Contains("WAITING LINE"))
            {
                return "submitFlow3";
            }
            else if (stepflow.Contains("CAP APPROVAL") ||
                     stepflow.Contains("WAITING CAP") ||
                     stepflow.Contains("APPROVED"))
            {
                return "submitFlow4";
            }

            return "";
        }

        // เมธอดสำหรับกำหนดบทบาทของอีเมล
        private string DetermineEmailRole(string email, string super_admin_mail, string pmsv_admin_mail, string pmdv_admin_mail,
            string requester_mail, string initiator_mail, string on_behalf_of_mail, string traveler_mail,
            string line_approver_mail, string cap_approver_mail)
        {
            if (string.IsNullOrEmpty(email))
                return "Unknown";

            email = email.ToLower().Trim();

            if (!string.IsNullOrEmpty(super_admin_mail) && super_admin_mail.ToLower().Contains(email))
                return "Super Admin";
            else if (!string.IsNullOrEmpty(pmsv_admin_mail) && pmsv_admin_mail.ToLower().Contains(email))
                return "PMSV Admin";
            else if (!string.IsNullOrEmpty(pmdv_admin_mail) && pmdv_admin_mail.ToLower().Contains(email))
                return "PMDV Admin";
            else if (!string.IsNullOrEmpty(requester_mail) && requester_mail.ToLower().Contains(email))
                return "Requester";
            else if (!string.IsNullOrEmpty(initiator_mail) && initiator_mail.ToLower().Contains(email))
                return "Initiator";
            else if (!string.IsNullOrEmpty(on_behalf_of_mail) && on_behalf_of_mail.ToLower().Contains(email))
                return "On Behalf Of";
            else if (!string.IsNullOrEmpty(traveler_mail) && traveler_mail.ToLower().Contains(email))
                return "Traveler";
            else if (!string.IsNullOrEmpty(line_approver_mail) && line_approver_mail.ToLower().Contains(email))
                return "Line Approver";
            else if (!string.IsNullOrEmpty(cap_approver_mail) && cap_approver_mail.ToLower().Contains(email))
                return "CAP Approver";

            return "Other";
        }

        // คลาสสำหรับเก็บผลลัพธ์จากการ query
        public class DocStatusResult
        {
            public string DH_STATUS { get; set; }
            public string DH_FLOW_STEP { get; set; }
        }

        public class EmailResult
        {
            public string approver_mail { get; set; }
            public string initial_mail { get; set; }
        }
        public KHCodeOutModel SetKHCode(KHCodeOutModel value)
        {
            // Initialize variables
            var doc_type = value.data_type;
            var data = value;
            var emp_user_active = ""; // User active token
            var token_login = data.token_login;

            #region Set Data
            Boolean user_admin = false;
            string user_id = "";
            string user_role = "";
            sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, "");
            emp_user_active = user_id;

            string ret = "true";
            string sqlstr = "";

            if (data.khcode_list.Count > 0)
            {
                int imaxid = GetMaxID(TableMaxId.BZ_DATA_KH_CODE);
                List<khcodeList> dtlist = data.khcode_list;

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            for (int i = 0; i < dtlist.Count; i++)
                            {
                                var id_def = "";
                                var action_type = dtlist[i].action_type.ToString();

                                if (string.IsNullOrEmpty(action_type)) continue;

                                if (action_type != "delete")
                                {
                                    var action_change = dtlist[i].action_change + "";
                                    if (action_change.ToLower() != "true") continue;
                                }

                                parameters = new List<OracleParameter>();

                                if (action_type == "insert")
                                {
                                    sqlstr = @"INSERT INTO BZ_DATA_KH_CODE
                                       (ID, EMP_ID, USER_ID, OVERSEA_CODE, LOCAL_CODE, STATUS, DATA_FOR_SAP, 
                                        CREATE_BY, CREATE_DATE, TOKEN_UPDATE) 
                                       VALUES 
                                       (:id, :emp_id, :user_id, :oversea_code, :local_code, :status, :data_for_sap, 
                                        :create_by, SYSDATE, :token_update)";

                                    parameters.Add(context.ConvertTypeParameter(":id", imaxid, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":user_id", dtlist[i].user_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":oversea_code", dtlist[i].oversea_code, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":local_code", dtlist[i].local_code, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":status", dtlist[i].status, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":data_for_sap", dtlist[i].data_for_sap, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":create_by", emp_user_active, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));

                                    id_def = imaxid.ToString();
                                    imaxid++;
                                }
                                else if (action_type == "update")
                                {
                                    sqlstr = @"UPDATE BZ_DATA_KH_CODE SET
                                       OVERSEA_CODE = :oversea_code, 
                                       LOCAL_CODE = :local_code, 
                                       STATUS = :status, 
                                       UPDATE_BY = :update_by, 
                                       UPDATE_DATE = SYSDATE, 
                                       TOKEN_UPDATE = :token_update
                                       WHERE ID = :id AND EMP_ID = :emp_id";

                                    parameters.Add(context.ConvertTypeParameter(":oversea_code", dtlist[i].oversea_code, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":local_code", dtlist[i].local_code, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":status", dtlist[i].status, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":update_by", emp_user_active, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":token_update", token_login, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                    id_def = dtlist[i].id.ToString();
                                }
                                else if (action_type == "delete")
                                {
                                    sqlstr = @"DELETE FROM BZ_DATA_KH_CODE WHERE ID = :id AND EMP_ID = :emp_id";

                                    parameters.Add(context.ConvertTypeParameter(":id", dtlist[i].id, "char"));
                                    parameters.Add(context.ConvertTypeParameter(":emp_id", dtlist[i].emp_id, "char"));

                                    id_def = dtlist[i].id.ToString();
                                }

                                try
                                {
                                    int iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    ret = (iret > 0) ? "true" : "false";
                                }
                                catch (Exception exExec)
                                {
                                    ret = exExec.Message.ToString();
                                    transaction.Rollback();
                                    break;
                                }

                                if (ret.ToLower() != "true") break;
                            }

                            if (ret.ToLower() == "true")
                            {
                                transaction.Commit();
                            }
                        }
                        catch (Exception exTran)
                        {
                            ret = exTran.Message.ToString();
                            transaction.Rollback();
                        }
                    }
                }
            }

            #endregion Set Data

            var msg_error = "";
            var msg_status = "Save data";

            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {
                if (doc_type == "submit")
                {
                    msg_status = "Submit Data";
                }
                else
                {
                    msg_status = "Save";
                }

                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                KHCodeModel value_load = new KHCodeModel
                {
                    token_login = data.token_login
                };

                data = swd.SearchKHCode(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() == "true") ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel
            {
                status = (ret.ToLower() == "true") ? msg_status + " succeeded." : msg_status + " failed.",
                remark = (ret.ToLower() == "true") ? "" : msg_error
            };
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel
            {
                status = "Error msg",
                remark = msg_error
            };

            return data;
        }

        #endregion set data in page

        #region set send mail in page 

        public AllowanceOutModel SendMailAllowance(AllowanceOutModel value)
        {
            var msg_error = "";
            var doc_type = value.data_type;
            var page_name = "allowance";
            var data = value;

            var email_admin = "";
            var role_type = "pmsv_admin";
            searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
            DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
            for (int i = 0; i < dtemplist.Rows.Count; i++)
            {
                email_admin += dtemplist.Rows[i]["email"] + ";";
            }
            if (value.doc_id.ToString().IndexOf("T") > -1)
            {
                _swd = new searchDocTravelerProfileServices();
                dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
            }


            List<allowanceList> allowance_main = data.allowance_main;
            List<mailselectList> mail_list = data.mail_list;
            List<EmpListOutModel> emp_list = data.emp_list;
            List<ImgList> img_list = data.img_list;

            try
            {
                //msg_error += "line:1";
                //mail_list.mail_attachments = full path
                List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                //msg_error += "line:11";
                if (drempcheck.Count > 0)
                {
                    //"http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/ExportFile/OB20090026/allowance/00001109/Allowance_Payment_Test.xlsx"
                    var emp_id_select = drempcheck[0].emp_id.ToString();
                    if (emp_id_select != "")
                    {
                        mail_list = new List<mailselectList>();
                        mail_list = data.mail_list.Where(a => (a.emp_id == emp_id_select)).ToList();

                        //msg_error += "line:12";
                        List<allowanceList> drcheck = value.allowance_main.Where(a => (a.emp_id == emp_id_select)).ToList();
                        if (drcheck.Count > 0)
                        {
                            //mail_list[0].mail_attachments = drcheck[0].file_report + ";" + drcheck[0].file_travel_report; 
                            for (int i = 0; i < mail_list.Count; i++)
                            {
                                mail_list[i].mail_attachments = drcheck[0].file_report + ";" + drcheck[0].file_travel_report;
                            }
                        }

                        //Allowance Payment Form

                        //เพิ่ม to admin ,cc : traverler 
                        var email_user_in_doc = drempcheck[0].userEmail.ToString();
                        var email_user_display = drempcheck[0].userDisplay.ToString();
                        mail_list[0].mail_to += email_admin;
                        mail_list[0].mail_to_display = email_user_display;
                        mail_list[0].mail_cc += email_user_in_doc;
                    }

                }

                //msg_error += "line:21";
                ret = "";
                SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, "");
                if (ret == "true")
                {
                    //msg_error += "line:22";
                    if (value.user_admin == true)
                    {
                        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                        List<EmpListOutModel> drempcheck2 = emp_list.Where(a => (a.mail_status == "true")).ToList();
                        if (drempcheck2.Count > 0)
                        {

                            try
                            {
                                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                                {
                                    using (var transaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            sqlstr = @" update bz_doc_allowance set doc_status = '4' 
                                        where doc_id = :doc_id and emp_id = :emp_id ";
                                            parameters = new List<OracleParameter>();

                                            parameters.Add(context.ConvertTypeParameter("doc_id", value.doc_id.ToString(), "char"));
                                            parameters.Add(context.ConvertTypeParameter("emp_id", drempcheck2[0]?.emp_id?.ToString(), "char"));
                                            try
                                            {
                                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                                if (iret > -1) { ret = "true"; } else { ret = "false"; }
                                                ;
                                            }
                                            catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }



                                            if (ret == "true")
                                            {
                                                context.SaveChanges();
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




                        }

                    }
                }
                //msg_error += "line:31";
                msg_error += ret;
            }
            catch (Exception ex) { msg_error += ex.Message.ToString(); ret = "false"; }
            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public ReimbursementOutModel SendMailReimbursement(ReimbursementOutModel value)
        {
            //send mail 
            //to : user int mail_list
            //cc : user int mail_list
            //subject : E-Biz : Reimbursement
            //body :  
            //body url : 
            //attachments :  

            var msg_error = "";
            var doc_type = value.data_type;
            var page_name = "reimbursement";
            var data = value;

            var email_admin = "";
            var role_type = "pmsv_admin";
            searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
            DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
            for (int i = 0; i < dtemplist.Rows.Count; i++)
            {
                email_admin += dtemplist.Rows[i]["email"] + ";";
            }
            if (value.doc_id.ToString().IndexOf("T") > -1)
            {
                _swd = new searchDocTravelerProfileServices();
                dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
            }

            List<mailselectList> mail_list = data.mail_list;
            List<EmpListOutModel> emp_list = data.emp_list;
            List<ImgList> img_list = data.img_list;

            try
            {
                //msg_error += "line:1";
                //mail_list.mail_attachments = full path
                List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                //msg_error += "line:11";
                if (drempcheck.Count > 0)
                {
                    //"http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/ExportFile/OB20090026/allowance/00001109/Allowance_Payment_Test.xlsx"
                    var emp_id_select = drempcheck[0].emp_id.ToString();
                    if (emp_id_select != "")
                    {
                        //ตัดให้เหลือรายการเดียว 
                        mail_list = new List<mailselectList>();
                        mail_list = data.mail_list.Where(a => (a.emp_id == emp_id_select)).ToList();

                        //msg_error += "line:12";
                        List<reimbursementList> drcheck = value.reimbursement_main.Where(a => (a.emp_id == emp_id_select)).ToList();
                        if (drcheck.Count > 0)
                        {
                            // msg_error += "line:13";
                            List<mailselectList> drmailcheck = mail_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                            drmailcheck[0].mail_attachments = drcheck[0].file_report + ";" + drcheck[0].file_travel_report;
                            // msg_error += "line:14";


                            //เพิ่ม to admin ,cc : traverler 
                            var email_user_in_doc = drempcheck[0].userEmail.ToString();
                            var email_user_display = drempcheck[0].userDisplay.ToString();
                            mail_list[0].mail_to += email_admin;
                            mail_list[0].mail_to_display = email_user_display;
                            mail_list[0].mail_cc += email_user_in_doc;

                        }
                    }
                    //เพิ่มไฟล์แนบจากหน้า web
                    if (img_list.Count > 0)
                    {
                        List<mailselectList> drmailcheck = mail_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                        List<ImgList> drimg_list = data.img_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                        for (int i = 0; i < drimg_list.Count; i++)
                        {
                            if (drimg_list[i].fullname.ToString() != "")
                            {
                                if (drmailcheck[0].mail_attachments != "") { drmailcheck[0].mail_attachments += ";"; }
                                drmailcheck[0].mail_attachments += drimg_list[i].fullname;
                            }
                        }
                    }
                }


                //msg_error += "line:21";
                ret = "";
                SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, "");

                // msg_error += "line:31";
                msg_error = "";

                if (ret == "true")
                {
                    if (value.user_admin == true)
                    {
                        try
                        {
                            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                            {
                                using (var transaction = context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        List<EmpListOutModel> drempcheck2 = emp_list.Where(a => (a.mail_status == "true")).ToList();
                                        sqlstr = @" update BZ_DOC_REIMBURSEMENT set doc_status = '4' 
where doc_id = :doc_id and emp_id =:emp_id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", value.doc_id.ToString(), "char"));
                                        parameters.Add(context.ConvertTypeParameter("emp_id", drempcheck2[0].emp_id.ToString(), "char"));
                                        try
                                        {
                                            var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                            if ((iret >= 0)) { ret = "true"; } else { ret = "false"; }
                                            ;
                                        }
                                        catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }



                                        if (ret == "true")
                                        {
                                            if (ClassConnectionDb.IsAuthorizedRole())
                                            {
                                                // ตรวจสอบสิทธิ์ก่อนดำเนินการ 
                                                context.SaveChanges();
                                                transaction.Commit();
                                            }
                                            else
                                            {
                                                transaction.Rollback();
                                            }
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
                        //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                        //List<EmpListOutModel> drempcheck2 = emp_list.Where(a => (a.mail_status == "true")).ToList();
                        //if (drempcheck2.Count > 0)
                        //{
                        //    sqlstr = @" update BZ_DOC_REIMBURSEMENT set doc_status = '4' 
                        //                where doc_id = '" + value.doc_id.ToString() + "' and emp_id = '" + drempcheck2[0].emp_id.ToString() + "' ";
                        //    ret = execute_data_ex(sqlstr, false);
                        //}
                    }
                }

            }
            catch (Exception ex) { msg_error = ex.Message.ToString(); ret = "false"; }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public TravelInsuranceOutModel SendMailTravelinsurance(TravelInsuranceOutModel value)
        {
            //send mail 
            //to : user int mail_list
            //cc : user int mail_list
            //subject : E-Biz : Send mail to Traveler 
            //body :  
            //body url : 
            //attachments :  

            var msg_error = "";
            var doc_type = value.data_type;
            var page_name = "travelinsurance";
            var data = value;
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            List<mailselectList> mail_list = data.mail_list;
            List<EmpListOutModel> emp_list = data.emp_list;
            List<ImgList> img_list = data.img_list;

            ret = "";
            SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
            ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, "");

            if (ret == "true")
            {
                if (value.user_admin == true)
                {
                    //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                    List<EmpListOutModel> drempcheck2 = emp_list.Where(a => (a.mail_status == "true")).ToList();
                    if (drempcheck2.Count > 0)
                    {
                        sqlstr = @" update bz_doc_insurance set doc_status = '3' 
                                        where doc_id = :doc_id and emp_id = :emp_id ";
                        p.Add(new("doc_id", value.doc_id.ToString()));
                        p.Add(new("emp_id", drempcheck2[0].emp_id.ToString()));
                        using TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities();
                        ret = context.Database.ExecuteSqlRaw(sqlstr, p.ToArray()) == 1 ? "true" : "false"; //execute_data_ex(sqlstr, false);
                    }
                }
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public TravelInsuranceOutModel SendMailTravelinsuranceClaim(TravelInsuranceOutModel value)
        {
            //send mail 
            //to : user int mail_list
            //cc : user int mail_list
            //subject : E-Biz : Send mail to Traveler 
            //body :  
            //body url : 
            //attachments :  

            var msg_error = "";
            var doc_type = value.data_type;
            var page_name = "travelinsurance";
            var data = value;

            List<mailselectList> mail_list = data.mail_list;
            List<EmpListOutModel> emp_list = data.emp_list;
            List<ImgList> img_list = data.img_list;
            //ตรวจสอบว่ามีการ active เพื่อส่ง mail หรือไม่จาก mail_status = 'true'
            List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
            if (drempcheck.Count > 0)
            {
                string emp_id_def = drempcheck[0].emp_id.ToString() + "";
                string email_emp_def = "";
                string email_admin_def = "";
                mail_list = new List<mailselectList>();
                for (int i = 0; i < drempcheck.Count; i++)
                {
                    email_emp_def += drempcheck[i].userEmail.ToString() + ";";
                }
                var role_type = "pmsv_admin";
                searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin_def += dtemplist.Rows[i]["email"] + ";";
                }
                if (value.doc_id.ToString().IndexOf("T") > -1)
                {
                    _swd = new searchDocTravelerProfileServices();
                    dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin_def += dtemplist.Rows[i]["email"] + ";";
                    }
                }
                mail_list.Add(new mailselectList
                {
                    module = "claim form requisition",
                    emp_id = emp_id_def,
                    mail_to = email_emp_def,
                    mail_body_in_form = "",
                    mail_cc = email_admin_def,
                    mail_status = "true",
                    action_change = "true",
                }); ;
            }

            ret = "";
            SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
            ret = swmail.SendMailInPage(ref mail_list, emp_list, img_list, data.doc_id, page_name, "claim form requisition");

            msg_error = (ret.ToLower() ?? "") == "true" ? "" : ret;

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public TransportationOutModel SendMailTransportation(TransportationOutModel value)
        {
            //ส่ง mail พร้อมแนบไฟล์/ลิ้ง ส่งให้พนักงาน อาจจะมีพนักงงานหลายคน 
            searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
            string url_personal_car_document = swd.refdata_TransportationURL(value.token_login, value.doc_id, "");

            var doc_type = value.data_type;
            var doc_id = value.doc_id;
            var ret = "";
            var data = value;

            int icount_emp = 0;
            string slist_emp_mail_to = "";
            string s_mail_to_emp_name = "";

            string s_mail_to = ""; // emp_list.mail_staus = 'true'
            string s_mail_cc = "";
            string s_subject = "";
            string s_mail_body = "";
            string s_mail_attachments = "";

            for (int i = 0; i < data.emp_list.Count; i++)
            {
                if ((data.emp_list[i].mail_status + "").ToString() == "true")
                {
                    icount_emp += 1;
                    slist_emp_mail_to += data.emp_list[i].userEmail + ";";

                    s_mail_to_emp_name = data.emp_list[i].userDisplay;
                }
            }
            var role_type = "pmsv_admin";
            var email_admin = "";
            List<EmpListOutModel> emp_list = data.emp_list;
            List<mailselectList> mail_list = new List<mailselectList>();

            searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
            DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
            for (int i = 0; i < dtemplist.Rows.Count; i++)
            {
                email_admin += dtemplist.Rows[i]["email"] + ";";
            }
            if (value.doc_id.ToString().IndexOf("T") > -1)
            {
                _swd = new searchDocTravelerProfileServices();
                dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
            }

            s_mail_to = slist_emp_mail_to + email_admin;
            s_mail_cc = "";

            s_subject = "E-Biz : Transportation Form";
            s_mail_body = @"Dear " + s_mail_to_emp_name + "";
            if (icount_emp > 1)
            {
                s_mail_body = "Dear All";
            }
            s_mail_body += @"";
            s_mail_body += @"Regards, 
                                <br>Thiti Noydee (Yo)
                                <br>System Administration Officer
                                <br>
                                <br>Tel : 038-359000  Ext 20104";


            SendEmailServiceTravelerProfile sm = new SendEmailServiceTravelerProfile();
            sm.send_mail(doc_id, "SendMailTransportation", s_mail_to, s_mail_cc, s_subject, s_mail_body, s_mail_attachments);

            for (int i = 0; i < data.emp_list.Count; i++)
            {
                if ((data.emp_list[i].mail_status + "").ToString() == "true")
                {
                    if (ret.ToLower().Replace("true", "") == "")
                    {
                        data.emp_list[i].mail_remark = "Send mail Success.";
                    }
                    else
                    {
                        data.emp_list[i].mail_remark = "Send mail Error." + ret;
                    }
                }
            }

            return data;
        }
        public VisaOutModel SendMailVisa(VisaOutModel value)
        {
            //ส่ง mail พร้อมแนบไฟล์/ลิ้ง ส่งให้พนักงาน อาจจะมีพนักงงานหลายคน 

            var msg_error = "";
            var doc_type = value.data_type;
            var page_name = "visa";
            var data = value;


            List<mailselectList> mail_list;//= data.mail_list;
            List<EmpListOutModel> emp_list = data.emp_list;
            List<ImgList> img_list = data.img_list;

            List<visaList> visa_detail = data.visa_detail;
            string country_in_doc = "";
            string email_admin = "";
            string email_user_in_doc = "";
            string emp_id_user_in_doc = "";
            string email_user_display = "";
            try
            {
                //Auwat 20210630 1200 แก้ไขเนื่องจาก font แจ้งมาว่าไม่ได้ใช้งานเส้นนี้ในการส่ง mail Visa Requisition จะใช้ SendMailVisa
                var role_type = "pmsv_admin";
                searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
                DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
                if (value.doc_id.ToString().IndexOf("T") > -1)
                {
                    _swd = new searchDocTravelerProfileServices();
                    dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                    for (int i = 0; i < dtemplist.Rows.Count; i++)
                    {
                        email_admin += dtemplist.Rows[i]["email"] + ";";
                    }
                }
                var emp_id_select = "";
                List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
                if (drempcheck.Count > 0)
                {
                    emp_id_select = drempcheck[0].emp_id;
                    emp_id_user_in_doc = drempcheck[0].emp_id;
                    email_user_in_doc = drempcheck[0].userEmail;
                    email_user_display = drempcheck[0].userDisplay;
                }

                List<MasterCountryModel> drcountrycheck = data.country_doc.Where(a => (a.action_change == "true")).ToList();
                if (drcountrycheck.Count > 0)
                {
                    country_in_doc = "";
                    for (int i = 0; i < drcountrycheck.Count; i++)
                    {
                        if (country_in_doc != "") { country_in_doc += ","; }
                        country_in_doc += drcountrycheck[i].country_id;
                    }
                }

            }
            catch { }

            string module_name = "sendmail_visa_requisition";
            mail_list = new List<mailselectList>();

            if (module_name == "sendmail_visa_requisition")
            {
                List<mailselectList> mail_list_def = new List<mailselectList>();

                mail_list_def = data.mail_list.Where(a => ((a.emp_id.ToLower() == emp_id_user_in_doc))).ToList();
                if (mail_list_def.Count > 0)
                {
                    mail_list.Add(new mailselectList
                    {
                        module = "VISA Requisition",
                        mail_to = mail_list_def[0].mail_to + email_user_in_doc,
                        mail_to_display = email_user_display,
                        mail_cc = mail_list_def[0].mail_cc + email_admin,
                        mail_body_in_form = "",
                        mail_status = "true",
                        action_change = "true",
                        emp_id = emp_id_user_in_doc,
                        country_in_doc = country_in_doc,
                    });
                }
            }

            ret = "";
            SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
            ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id, page_name, module_name);


            if (ret == "")
            {
                if (value.user_admin == true)
                {
                    //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                    List<EmpListOutModel> drempcheck2 = emp_list.Where(a => (a.mail_status == "true")).ToList();
                    if (drempcheck2.Count > 0)
                    {
                        using var context = new TOPEBizCreateTripEntities();
                        string emp_id_select = drempcheck2[0].emp_id;
                        string visa_card_id = "";
                        List<visaList> dtlist = data.visa_detail.Where(a => (a.emp_id == emp_id_select && a.visa_active_in_doc == "true")).ToList();
                        for (int i = 0; i < dtlist.Count; i++)
                        {
                            if (visa_card_id != "") { visa_card_id += ","; }
                            visa_card_id += "'" + dtlist[i].visa_card_id.ToString() + "'";
                        }
                        sqlstr = @" update bz_doc_visa set doc_status = '3' 
                                        where emp_id = :emp_id and doc_id = :doc_id ";
                        var parameters = new List<OracleParameter>();
                        parameters.Add(new("emp_id", drempcheck2[0].emp_id.ToString()));
                        parameters.Add(new("doc_id", value.doc_id));
                        ret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray()) > -1 ? "true" : "false";

                    }
                }
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        public ISOSOutModel SendMailISOS(ISOSOutModel value)
        {
            //ส่ง mail  ส่งให้พนักงานที่อยู่ในใบงาน 
            //mail isos : to all user ในใบงานนั้นๆ, cc pmsv group
            var data = value;
            var msg_error = "";
            var page_name = "isos";
            var module_name = value.data_type;
            var email_admin = "";
            var email_user_in_doc = "";
            var mail_cc_active = "";
            var role_type = "pmsv_admin";
            var emp_id_user_in_doc = "";
            var email_user_display = "";

            List<EmpListOutModel> emp_list = data.emp_list;
            List<mailselectList> mail_list = new List<mailselectList>();

            searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
            DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
            for (int i = 0; i < dtemplist.Rows.Count; i++)
            {
                email_admin += dtemplist.Rows[i]["email"] + ";";
            }
            if (value.doc_id.ToString().IndexOf("T") > -1)
            {
                _swd = new searchDocTravelerProfileServices();
                dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
            }
            //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
            mail_cc_active = sqlEmpUserMail(value.token_login);

            List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
            if (drempcheck.Count > 0)
            {
                for (int i = 0; i < drempcheck.Count; i++)
                {
                    emp_id_user_in_doc += drempcheck[i].emp_id.ToString() + ";";//ไม่ได้ใช้งาน แต่เก็บไว้ให้ครบ
                    email_user_in_doc += drempcheck[i].userEmail.ToString() + ";";
                    email_user_display += drempcheck[i].userDisplay.ToString() + ";";
                }
            }
            ///???
            module_name = "sendmail_isos_to_traveler";
            mail_list.Add(new mailselectList
            {
                module = "ISOS",
                mail_to = email_user_in_doc,
                mail_to_display = email_user_display,
                mail_body_in_form = "",
                mail_cc = email_admin,
                emp_id = emp_id_user_in_doc,
                mail_status = "true",
                action_change = "true",
            });

            ret = "";
            SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
            ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id, page_name, module_name);
            if (ret.ToLower() != "true")
            {
                msg_error = ret;
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }
        public ISOSOutModel SendMailISOSRecord(ISOSOutModel value)
        {
            //ส่ง mail  ส่งให้พนักงานที่อยู่ในใบงาน 
            //mail isos : to broker ในใบงานนั้นๆ, cc pmsv group
            var data = value;

            string token_login = value.token_login.ToString();
            string doc_id = value.doc_id.ToString();
            string emp_user_active = token_login; //User take action
            string ret = "";
            var msg_error = "";
            var page_name = "isos";
            var module_name = value.data_type;
            var email_admin = "";
            var email_user_in_doc = "";
            var mail_cc_active = "";
            var role_type = "pmsv_admin";
            var emp_id_user_in_doc = "";
            var email_broker = "";
            var email_broker_id = "";
            var email_broker_name = "";

            for (int i = 0; i < value.m_broker.Count; i++)
            {
                if (value.m_broker[i].status.ToString() == "true")
                {
                    email_broker += value.m_broker[i].email + ";";

                    if (email_broker_id != "") { email_broker_id += ","; }
                    email_broker_id += value.m_broker[i].id;
                    if (email_broker_name != "") { email_broker_name += ","; }
                    email_broker_name += value.m_broker[i].name;
                }
            }

            emp_user_active = sqlEmpUserName(token_login);


            List<EmpListOutModel> emp_list = data.emp_list;
            List<mailselectList> mail_list = new List<mailselectList>();

            searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
            DataTable dtemplist = _swd.refsearch_emprole_list(role_type);
            for (int i = 0; i < dtemplist.Rows.Count; i++)
            {
                email_admin += dtemplist.Rows[i]["email"] + ";";
            }

            //auwat 20221003 1424 น้องเมล์ เมล์ที่ออกมา ไม่ต้อง CC พนักงานก็ได้ค่ะ แต่ช่วย CC  PMDV team ด้วยค่ะ
            //if (value.doc_id.ToString().IndexOf("T") > -1)
            {
                _swd = new searchDocTravelerProfileServices();
                dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
                for (int i = 0; i < dtemplist.Rows.Count; i++)
                {
                    email_admin += dtemplist.Rows[i]["email"] + ";";
                }
            }
            //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
            mail_cc_active = sqlEmpUserMail(token_login);

            List<EmpListOutModel> drempcheck = emp_list.Where(a => (a.mail_status == "true")).ToList();
            if (drempcheck.Count > 0)
            {
                sqlstr = "";
                parameters = new List<OracleParameter>();

                int imaxid = GetMaxIDYear(TableMaxId.BZ_DOC_ISOS_RECORD);

                Boolean bActiveSendMail = false;
                try
                {
                    //emp_id_user_in_doc = "";
                    //for (int i = 0; i < drempcheck.Count; i++)
                    //{
                    //    if (i > 0) { emp_id_user_in_doc += ","; }
                    //    emp_id_user_in_doc += drempcheck[i].emp_id?.ToString() ;
                    //}
                    //string[] emps = emp_id_user_in_doc.Split(","); 

                    //var EmpList = new List<Models.Create_Trip.tempISOSMailModel>();
                    //using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    //{
                    //    //ตรวจสอบว่าข้อมูลของพนักงาน เคยส่งไปให้ Broker แล้วหรือไม่ ถ้ามีไม่ต้องส่งไปใหม่ 
                    //    //broker เดียว เเต่ E-Mail อาจจะมีหลายเมลล์ 
                    //    //travel type ต่างกันต้อง run number ชุดเดียวค่ะ คุ้มครองพนักงานบิรษัท   
                    //    sqlstr = @" select u.employeeid as emp_id, nvl(b.send_mail_type,0) as send_mail_type 
                    //             , u.entitle as title, u.enfirstname as name, u.enlastname as surname, u.sections as section, u.department, u.function 
                    //             from vw_bz_users u 
                    //             left join bz_doc_isos_record b on u.employeeid = b.emp_id and b.year = to_char(sysdate,'rrrr')
                    //             where nvl(b.send_mail_type,0) = 0 and (";

                    //    if (!string.IsNullOrEmpty(emps))
                    //    {
                    //        string placeholders = string.Join(",", emps.Select((_, i) => $":emplist{i}"));
                    //        sqlstr += $" WHERE upper(u.employeeid) IN ({placeholders})";

                    //        for (int i = 0; i < emps.Length; i++)
                    //        {
                    //            parameters.Add(new OracleParameter($"emplist{i}", emps[i]));
                    //        }
                    //        }
                    //        sqlstr += $" ) ";

                    //    EmpList = context.TempISOSMailModelList.FromSqlRaw(sqlstr, parameters.ToArray()).ToList();
                    //}

                    emp_id_user_in_doc = string.Join(",", drempcheck.Select(e => e.emp_id?.ToString()));
                    string[] emps = emp_id_user_in_doc.Split(",", StringSplitOptions.RemoveEmptyEntries);

                    List<Models.Create_Trip.tempISOSMailModel> EmpList;

                    using (var context = new TOPEBizCreateTripEntities())
                    {
                        string sqlstr = @"
                                        select u.employeeid as emp_id, nvl(b.send_mail_type,0) as send_mail_type, 
                                               u.entitle as title, u.enfirstname as name, u.enlastname as surname, 
                                               u.sections as section, u.department, u.function 
                                        from vw_bz_users u
                                        left join bz_doc_isos_record b 
                                            on u.employeeid = b.emp_id and b.year = to_char(sysdate,'rrrr')
                                        where nvl(b.send_mail_type,0) = 0 and upper(u.employeeid) IN (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids))) ";

                        var paramList = new List<OracleParameter>();
                        var whereConditions = new List<string>();

                        for (int i = 0; i < emps.Length; i++)
                        {
                            // whereConditions.Add($":emplist{i}");
                            // paramList.Add(context.ConvertTypeParameter($"emplist{i}", emps[i], "char"));
                            whereConditions.Add(emps[i]);

                        }

                        // sqlstr = string.Format(sqlstr, string.Join(",", whereConditions));
                        paramList.Add(context.ConvertTypeParameter($"empids", string.Join(",", whereConditions), "char"));
                        EmpList = context.TempISOSMailModelList
                            .FromSqlRaw(sqlstr, paramList.ToArray())
                            .ToList();
                    }

                    if (EmpList?.Count > 0)
                    {
                        ret = "true";

                        string type_of_travel = "Business Trip";
                        if (doc_id.ToLower().IndexOf("ot") > -1 || doc_id.ToLower().IndexOf("lt") > -1)
                        {
                            type_of_travel = "Training Trip";
                        }

                        List<isosList> dtlist = new List<isosList>();
                        for (int j = 0; j < EmpList.Count; j++)
                        {
                            if (EmpList[j].send_mail_type?.ToString() == "1") { continue; }

                            isosList deflist = new isosList();
                            deflist.id = (j + 1).ToString();
                            deflist.doc_id = doc_id;
                            deflist.emp_id = EmpList[j].emp_id ?? "";// dt.Rows[j]["emp_id"].ToString();
                            deflist.send_mail_type = "0";

                            deflist.isos_type_of_travel = type_of_travel;
                            deflist.isos_emp_id = EmpList[j].emp_id ?? "";
                            deflist.isos_emp_title = EmpList[j].title ?? "";
                            deflist.isos_emp_name = EmpList[j].name ?? "";
                            deflist.isos_emp_surname = EmpList[j].surname ?? "";
                            deflist.isos_emp_section = EmpList[j].section ?? "";
                            deflist.isos_emp_department = EmpList[j].department ?? "";
                            deflist.isos_emp_function = EmpList[j].function ?? "";

                            deflist.insurance_company_id = email_broker_id;
                            dtlist.Add(deflist);

                        }
                        if (dtlist.Count > 0)
                        {
                            (bool bret, ret) = SetISOSRecord(dtlist, emp_user_active, token_login, imaxid);
                            if (ret.ToLower() == "true")
                            {
                                bActiveSendMail = true;
                            }
                        }
                        else
                        {
                            ret = "false";
                            msg_error = "ไม่มีรายการที่ต้องส่ง";
                        }

                    }

                    if (bActiveSendMail && ret.ToLower() == "true")
                    {
                        Console.WriteLine("bActiveSendMail: " + bActiveSendMail);
                        Console.WriteLine("Before SendMailInPage ret: " + ret);
                        module_name = "sendmail_isos_to_broker";
                        mail_list = new List<mailselectList>();
                        mail_list.Add(new mailselectList
                        {
                            module = "International SOS Record",
                            mail_to = email_broker,
                            mail_to_display = email_broker_name,
                            //mail_cc = email_user_in_doc + email_admin,
                            mail_cc = email_admin,
                            mail_body_in_form = "",
                            mail_status = "true",
                            action_change = "true",
                            emp_id = emp_id_user_in_doc,
                        });


                        SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
                        ret = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id
                            , page_name, module_name);
                        if (ret.ToLower() != "true")
                        {
                            msg_error = ret;
                        }
                        else
                        {
                            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                            {
                                //update log status send mail
                                //sqlstr = @" UPDATE BZ_DOC_ISOS_RECORD SET SEND_MAIL_TYPE = 1";
                                //sqlstr += @" ,UPDATE_BY = " + ChkSqlStr(emp_user_active, 300);
                                //sqlstr += @" ,UPDATE_DATE = sysdate";
                                //sqlstr += @" ,TOKEN_UPDATE = " + ChkSqlStr(token_login, 300);
                                //sqlstr += @" where ";
                                //sqlstr += @" DOC_ID = " + ChkSqlStr(doc_id, 300);
                                sqlstr = @" UPDATE BZ_DOC_ISOS_RECORD SET SEND_MAIL_TYPE = 1";
                                sqlstr += @" ,UPDATE_BY = :emp_user_active ";
                                sqlstr += @" ,UPDATE_DATE = sysdate";
                                sqlstr += @" ,TOKEN_UPDATE = :token_login";
                                sqlstr += @" where DOC_ID = :doc_id";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("emp_user_active", emp_user_active, "char"));
                                parameters.Add(context.ConvertTypeParameter("token_login", token_login, "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                                try
                                {
                                    var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                    if (iret > 0) { ret = "true"; } else { ret = "false"; }
                                    ;

                                }
                                catch (Exception ex_Exec) { ret = ex_Exec.Message.ToString(); }
                            }
                            ret = "true";
                        }


                    }
                    ret = "true";
                }
                catch (Exception ex)
                {
                    ret = ex.Message.ToString();
                }

                if ((ret.ToLower() ?? "") == "true")
                {
                    searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                    ISOSModel value_load = new ISOSModel();
                    value_load.token_login = data.token_login;
                    value_load.doc_id = data.doc_id;
                    data = new ISOSOutModel();
                    data = swd.SearchISOS(value_load);
                    msg_error += "Load Data";

                }

            }

            drempcheck = new List<EmpListOutModel>();
            drempcheck = emp_list.Where(a => a.mail_status == "true").ToList();

            if (drempcheck.Count > 0)
            {
                bool isSuccess = false;
                msg_error = "";
                emp_id_user_in_doc = string.Join(",", drempcheck.Select(e => e.emp_id?.ToString()));
                string[] emps = emp_id_user_in_doc.Split(",", StringSplitOptions.RemoveEmptyEntries);

                List<Models.Create_Trip.tempISOSMailModel> EmpList;

                using (var context = new TOPEBizCreateTripEntities())
                {
                    string sqlstr = @"
            select u.employeeid as emp_id, nvl(b.send_mail_type,0) as send_mail_type, 
                   u.entitle as title, u.enfirstname as name, u.enlastname as surname, 
                   u.sections as section, u.department, u.function 
            from vw_bz_users u
            left join bz_doc_isos_record b 
                on u.employeeid = b.emp_id and b.year = to_char(sysdate,'rrrr')
            where nvl(b.send_mail_type,0) = 0 and upper(u.employeeid) IN  (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids)))
        ";

                    var paramList = new List<OracleParameter>();
                    var placeholders = new List<string>();

                    for (int i = 0; i < emps.Length; i++)
                    {
                        // placeholders.Add($":emplist{i}");
                        // paramList.Add(context.ConvertTypeParameter($"emplist{i}", emps[i], "char"));
                        placeholders.Add(emps[i]);
                    }

                    // sqlstr = string.Format(sqlstr, string.Join(",", placeholders));

                    paramList.Add(context.ConvertTypeParameter("empids", string.Join(",", placeholders)));
                    EmpList = context.TempISOSMailModelList
                        .FromSqlRaw(sqlstr, paramList.ToArray())
                        .ToList();
                }

                if (EmpList.Count > 0)
                {
                    string type_of_travel = doc_id.ToLower().Contains("ot") || doc_id.ToLower().Contains("lt")
                        ? "Training Trip"
                        : "Business Trip";

                    var dtlist = new List<isosList>();
                    for (int j = 0; j < EmpList.Count; j++)
                    {
                        if (EmpList[j].send_mail_type?.ToString() == "1") continue;

                        dtlist.Add(new isosList
                        {
                            id = (j + 1).ToString(),
                            doc_id = doc_id,
                            emp_id = EmpList[j].emp_id ?? "",
                            send_mail_type = "0",
                            isos_type_of_travel = type_of_travel,
                            isos_emp_id = EmpList[j].emp_id ?? "",
                            isos_emp_title = EmpList[j].title ?? "",
                            isos_emp_name = EmpList[j].name ?? "",
                            isos_emp_surname = EmpList[j].surname ?? "",
                            isos_emp_section = EmpList[j].section ?? "",
                            isos_emp_department = EmpList[j].department ?? "",
                            isos_emp_function = EmpList[j].function ?? "",
                            insurance_company_id = email_broker_id
                        });
                    }

                    if (dtlist.Count == 0)
                    {
                        msg_error = "ไม่มีรายการที่ต้องส่ง";
                    }
                    else
                    {
                        int imaxid = GetMaxIDYear(TableMaxId.BZ_DOC_ISOS_RECORD);

                        using (var context = new TOPEBizCreateTripEntities())
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {

                                (isSuccess, msg_error) = SetISOSRecord(dtlist, emp_user_active, token_login, imaxid);
                                if (!isSuccess)
                                {
                                    msg_error = "SetISOSRecord failed.";

                                }
                                else
                                {

                                    mail_list = new List<mailselectList>
                    {
                        new mailselectList
                        {
                            module = "International SOS Record",
                            mail_to = email_broker,
                            mail_to_display = email_broker_name,
                            mail_cc = email_admin,
                            mail_body_in_form = "",
                            mail_status = "true",
                            action_change = "true",
                            emp_id = emp_id_user_in_doc
                        }
                    };

                                    SendEmailServiceTravelerProfile swmail = new();
                                    isSuccess = swmail.SendMailInPage(ref mail_list, data.emp_list, data.img_list, data.doc_id,
                                                                      page_name, "sendmail_isos_to_broker") == "true";

                                    if (!isSuccess)
                                    {
                                        msg_error = "SendMailInPage failed.";

                                    }
                                    else
                                    {

                                        string sqlUpdate = @"
                                        UPDATE BZ_DOC_ISOS_RECORD 
                                        SET SEND_MAIL_TYPE = 1,
                                            UPDATE_BY = :emp_user_active,
                                            UPDATE_DATE = sysdate,
                                            TOKEN_UPDATE = :token_login
                                        WHERE DOC_ID = :doc_id";

                                        var parameters = new List<OracleParameter>
                    {
                        context.ConvertTypeParameter("emp_user_active", emp_user_active, "char"),
                        context.ConvertTypeParameter("token_login", token_login, "char"),
                        context.ConvertTypeParameter("doc_id", doc_id, "char")
                    };

                                        int updateResult = context.Database.ExecuteSqlRaw(sqlUpdate, parameters.ToArray());
                                        isSuccess = updateResult > 0;
                                        if (isSuccess)
                                        {
                                            transaction.Commit();
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                msg_error = ex.Message;
                                isSuccess = false;
                            }
                        }
                    }
                }

                if (isSuccess)
                {
                    searchDocTravelerProfileServices swd = new();
                    ISOSModel value_load = new()
                    {
                        token_login = data.token_login,
                        doc_id = data.doc_id
                    };
                    data = swd.SearchISOS(value_load);
                    msg_error += " Load Data";
                }
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;

        }
        public PortalOutModel SendMailContact(PortalOutModel value)
        {
            //get in touch เมื่อ submit ให้ส่ง mail หา admin & CONTACT US

            var data = value;
            var msg_error = "";
            var role_type = "pmsv_admin";
            var mail_body_in_form = value.text_name + "<br>" + value.text_subject + "<br>" + value.text_message;

            searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
            DataTable dtemplist = swd.refsearch_emprole_list(role_type);
            string email_admin = "";
            for (int i = 0; i < dtemplist.Rows.Count; i++)
            {
                email_admin += dtemplist.Rows[i]["email"] + ";";
            }

            searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();
            dtemplist = _swd.refsearch_emprole_list("pmdv_admin");
            for (int i = 0; i < dtemplist.Rows.Count; i++)
            {
                email_admin += dtemplist.Rows[i]["email"] + ";";
            }

            //ให้ cc หาคนที่แจ้งปัญหาด้วย จับจาก token login 
            var mail_cc_active = "";
            mail_cc_active = sqlEmpUserMail(value.token_login);

            List<mailselectList> mail_list = new List<mailselectList>();
            mail_list.Add(new mailselectList
            {
                module = "Contact As",
                mail_to = value.text_contact_email + ";" + email_admin,
                mail_body_in_form = mail_body_in_form,
                mail_cc = mail_cc_active,
                mail_status = "true",
                action_change = "true",
            });

            ret = "";
            SendEmailServiceTravelerProfile swmail = new SendEmailServiceTravelerProfile();
            ret = swmail.SendMailInContact(ref mail_list);

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }

        #endregion set send mail in page 


        #region SAP
        public TravelExpenseOutModel SendTravelExpenseToSAP(TravelExpenseOutModel value)
        {
            var msg = "";
            var sqlstr_all = "";
            var page_name = "travelexpense";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new TravelExpenseOutModel();
            data = value;
            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;

            //ทดสอบ update status = Send to SAP ทั้งหมดใน list ที่ส่งไป SAP ก่อน 
            for (int i = 0; i < data.travelexpense_detail.Count; i++)
            {

                string emp_user_active = "";
                string id = data.travelexpense_detail[i].id;
                string emp_id = data.travelexpense_detail[i].emp_id;
                string status_sap = "";

                List<EmpListOutModel> dremplist = data.emp_list.Where(a => ((a.emp_id == emp_id) && (a.send_to_sap == "true"))).ToList();
                if (dremplist.Count > 0) { status_sap = "6"; } else { continue; }
                if (data.travelexpense_detail[i].status_active == "true") { status_sap = "6"; } else { continue; }

                data.travelexpense_detail[i].status = status_sap;

                sqlstr = @" update BZ_DOC_TRAVELEXPENSE_DETAIL set";

                sqlstr += @" STATUS = " + ChkSqlStr(status_sap, 4000);

                sqlstr += @" ,UPDATE_BY = " + ChkSqlStr(emp_user_active, 300);//user name login
                sqlstr += @" ,UPDATE_DATE = sysdate";
                sqlstr += @" ,TOKEN_UPDATE = " + ChkSqlStr(token_login, 300);
                sqlstr += @" where ";
                sqlstr += @" ID = " + ChkSqlStr(id, 300);
                sqlstr += @" and DOC_ID = " + ChkSqlStr(doc_id, 300);
                sqlstr += @" and EMP_ID = " + ChkSqlStr(emp_id, 300);

                ret = execute_data_ex(sqlstr, true);
                sqlstr_all += sqlstr + "||";

                if (ret.ToLower() != "true") { goto Next_line_1; }
            }
        Next_line_1:;

            if (ret.ToLower() == "true")
            {
                ret = execute_data_ex(sqlstr_all, false); sqlstr_all = "";
            }

            var msg_error = "";
            if (ret.ToLower() != "true")
            {
                msg_error = ret + " --> query error :" + sqlstr;
            }
            else
            {

                searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
                TravelExpenseModel value_load = new TravelExpenseModel();
                value_load.token_login = data.token_login;
                value_load.doc_id = data.doc_id;
                data = new TravelExpenseOutModel();
                data = swd.SearchTravelExpense(value_load);
            }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send data to SAP succesed." : "Send data to SAP failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;
            return data;
        }

        public string ChkSqlStr(object Str, int Length)
        {
            //วิธีที่ 1 --> แทนที่ ' ด้วย ช่องว่าง 1 ช่อง --> " " ทำให้ ' ใน base หายไป
            //วิธีที่ 2 --> แทนที่ ' ด้วย ''         --> Chr(39) & Chr(39) ทำให้ ' ใน base ยังอยู่ 

            //Str = "เลี้ยงตอบแทน' บ.Cyberouis, XX'XX'xxx'xxx"

            string Str1;

            if (Str == null || Convert.IsDBNull(Str))
            {
                return "null";
            }

            if (Str.ToString().ToLower() == "null")
            {
                return "null";
            }

            if (Str.ToString().Trim() == "")
            {
                return "null";
            }

            Str1 = Str.ToString();

            //วิธีที่ 1
            //Str1 = Replace(Str1, Chr(39), " ")

            //วิธีที่ 2
            //Str1 = Replace(Str1, Chr(39), Chr(39) & Chr(39))
            Str1 = Str1.Replace("'", "''");

            if (Str1.ToString().Length >= Length)
            {
                return "'" + Str1.ToString().Substring(0, Length) + "'";
            }
            else
            {
                return "'" + Str1.ToString().Trim() + "'";
            }
        }

        #endregion SAP

        #region car service
        //CarServiceOutModel
        public TransportationOutModel OpenWebCarService(TransportationOutModel value)
        {
            //ส่ง mail  ส่งให้พนักงานที่อยู่ในใบงาน 
            //mail isos : to all user ในใบงานนั้นๆ, cc pmsv group
            var data = value;
            var msg_error = "";
            var page_name = "transportation";
            var module_name = "";
            var doc_id = value.doc_id.ToString();

            #region Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
            string doc_status = "";
            var emp_id_select = "";
            List<EmpListOutModel> drempcheck = data.emp_list.Where(a => (a.mail_status == "true")).ToList();
            if (drempcheck.Count > 0)
            {
                emp_id_select = drempcheck[0].emp_id;
                if (value.user_admin == true)
                {
                    doc_status = "3";
                }
                else
                {
                    doc_status = "2";
                }
                try
                {
                    if (drempcheck[0].doc_status_id.ToString() == "4") { doc_status = "4"; }
                }
                catch { }

                drempcheck[0].doc_status_id = doc_status;

            }
            try
            {
                var parameter = new List<OracleParameter>();
                var iret = 0;

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            sqlstr = @"delete from BZ_DATA_CONTENT_EMP where doc_id = :doc_id and emp_id = :emp_id ";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id?.ToString() ?? "", "char"));
                            parameters.Add(context.ConvertTypeParameter("emp_id", emp_id_select?.ToString() ?? "", "char"));

                            iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                            if (iret > -1)
                            {
                                sqlstr = @"insert into  BZ_DATA_CONTENT_EMP ( doc_id, emp_id, doc_status) values ( :doc_id , :emp_id, :doc_status )";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id?.ToString() ?? "", "char"));
                                parameters.Add(context.ConvertTypeParameter("emp_id", emp_id_select?.ToString() ?? "", "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status?.ToString() ?? "", "char"));

                                iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                if (iret > -1) { ret = "true"; } else { ret = "false"; }
                            }

                            if (ret == "true")
                            {
                                context.SaveChanges();
                                transaction.Commit();
                            }
                            else { transaction.Rollback(); }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
            catch { }
            #endregion Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed


            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Send mail succesed." : "Send mail failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        #endregion car service

        #region Insert User Contract
        //??? comment for compare old version and new version
        //public string AddUser(string user, ref string _msg)
        //{
        //    List<Users> userList = new List<Users>();
        //    string xuser_ad = user.Replace("@thaioilgroup.com", "");
        //    string xpass_ad = "admin";
        //    string employee_id = "";

        //    Boolean bUserInThaioilGroup = false;

        //    //กรณีที่มาจาก AD หรือมาจากหน้า Login ที่เป็นการสวมสิทธิ์ 
        //    userAuthenService userAuthen = new userAuthenService();
        //    userList = userAuthen.GetADUsersFilter(xuser_ad, xpass_ad, ref _msg);
        //    try
        //    {
        //        if (_msg == "")
        //        {
        //            if (userList[0].UserName.ToString().ToLower() == xuser_ad.ToLower())
        //            {
        //                bUserInThaioilGroup = true;
        //            }
        //            else { _msg += "invalid username(1) :" + xuser_ad; }
        //        }
        //    }
        //    catch { _msg += "invalid username(2):" + xuser_ad + " group : " + bUserInThaioilGroup; }

        //    if (bUserInThaioilGroup == true)
        //    {
        //        DataTable dt = new DataTable();
        //        sqlstr = @" select EMPLOYEEID
        //                ,case when USERTYPE = 2 then 'Y' else 'N' end  check_user_z 
        //                from VW_BZ_USERS where upper(userid) =  upper('" + user + "')";
        //        if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
        //        {
        //            string action_type = "";
        //            string token_login = Guid.NewGuid().ToString();
        //            if (dt.Rows.Count > 0)
        //            {
        //                DataRow login_empid = dt.Rows[0];
        //                employee_id = login_empid["EMPLOYEEID"].ToString() ?? "";

        //                if (login_empid["EMPLOYEEID"].ToString() == "Y")
        //                {
        //                    //อัฟเดตข้อมูลในตารางกรณีที่เป็น user z
        //                    action_type = "update";
        //                }
        //            }
        //            else
        //            {
        //                //กรณีที่ไม่มีข้อมูลจะเป็น user z ทั้งหมด กรณีที่เป็น user thaioilจะต้องมีใน table อยู่แล้ว?? มี batch ดึงข้อมูล
        //                action_type = "insert";//new empid 
        //            }

        //            if (userList != null)
        //            {
        //                if (userList.Count > 0)
        //                {
        //                    //เพิ่ม/update ข้อมูลในตาราง BZ_USERS  
        //                    sqlstr = @" call bz_sp_add_user_z ( ";
        //                    sqlstr += " '" + token_login + "'";
        //                    sqlstr += ",'" + user.ToString().ToUpper() + "'";
        //                    sqlstr += ",'" + userList[0].DisplayName.ToString() + "'";
        //                    sqlstr += ",'" + userList[0].Email.ToString() + "'";
        //                    sqlstr += ",'" + action_type + "'";
        //                    sqlstr += ")";
        //                    if (SetDocService.conn_ExecuteNonQuery(sqlstr, false) == "true")
        //                    {
        //                        sqlstr = "select * from  VW_BZ_USERS where upper(USERID) =  '" + user.ToString().ToUpper() + "'";
        //                        if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
        //                        {
        //                            if (dt.Rows.Count > 0)
        //                            {
        //                                employee_id = dt.Rows[0]["EMPLOYEEID"].ToString() ?? "";
        //                                _msg = "";
        //                                return employee_id;
        //                            }
        //                        }
        //                    }

        //                }
        //            }

        //        }

        //    }

        //    return employee_id;
        //}

        //public string AddUser(string user, ref string _msg)
        //{
        //    List<Users> userList = new List<Users>();
        //    string xuser_ad = user.Replace("@thaioilgroup.com", "");
        //    string xpass_ad = "admin";
        //    string employee_id = "";

        //    Boolean bUserInThaioilGroup = false;

        //    //กรณีที่มาจาก AD หรือมาจากหน้า Login ที่เป็นการสวมสิทธิ์ 
        //    userAuthenService userAuthen = new userAuthenService();
        //    userList = userAuthen.GetADUsersFilter(xuser_ad, xpass_ad, ref _msg);
        //    try
        //    {
        //        if (_msg == "")
        //        {
        //            if (userList[0].UserName.ToString().ToLower() == xuser_ad.ToLower())
        //            {
        //                bUserInThaioilGroup = true;
        //            }
        //            else { _msg += "invalid username(1) :" + xuser_ad; }
        //        }
        //    }
        //    catch
        //    {
        //        _msg += "invalid username(2):" + xuser_ad + " group : " + bUserInThaioilGroup;
        //    }

        //    if (bUserInThaioilGroup == true)

        //        using (var context = new TOPEBizTravelerProfileEntitys())
        //        {
        //            var sql = @" select EMPLOYEEID
        //                ,case when USERTYPE = 2 then 'Y' else 'N' end  check_user_z 
        //                from VW_BZ_USERS where upper(userid) =  upper(:userid)";

        //            var parameters = new List<OracleParameter>();
        //            parameters.Add(new OracleParameter("userid", user));

        //            var checkUser = context.VW_BZ_USERS.FromSqlRaw(sql, parameters.ToArray()).ToList();

        //            if (checkUser.Count > 0)
        //            {
        //                employee_id = checkUser[0].EMPLOYEEID.ToString() ?? "";
        //                var action_type = "";

        //                if (employee_id == "Y")
        //                {
        //                    //อัฟเดตข้อมูลในตารางกรณีที่เป็น user z
        //                    action_type = "update";
        //                }
        //                else
        //                {
        //                    //กรณีที่ไม่มีข้อมูลจะเป็น user z ทั้งหมด กรณีที่เป็น user thaioilจะต้องมีใน table อยู่แล้ว?? มี batch ดึงข้อมูล
        //                    action_type = "insert";//new empid 
        //                }

        //            }
        //            if (userList != null)
        //            {

        //                if (userList.Count > 0)
        //                {


        //                }
        //            }
        //        }

        //    return employee_id;
        //}

 

        // ฟังก์ชันสำหรับบันทึกข้อผิดพลาด
        private void LogError(Exception ex)
        {
            // ตัวอย่างการบันทึก log ข้อผิดพลาด
            // คุณสามารถใช้ library เช่น NLog, Serilog, หรือเขียนลงไฟล์ log เอง
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        #endregion Insert User Contract
    }



}