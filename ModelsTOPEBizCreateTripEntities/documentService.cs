
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.helper;

 using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Service.Traveler_Profile;

namespace top.ebiz.service.Service.Create_Trip
{
    public class documentService
    {
        //DevFix 20200910 0727 เพิ่มแนบ link Ebiz ด้วย Link ไปหน้า login 
        //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/authen.aspx?page=/main/request/edit/###/i
        //String LinkLogin = System.Configuration.ConfigurationManager.AppSettings["LinkLogin"].ToString();
        string LinkLogin = configApp.GetStringFromAppSettings("LinkLogin") ?? "";

        //DevFix 20211004 0000 เพิ่มแนบ link Ebiz Phase2  
        //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/master/###/travelerhistory
        //String LinkLoginTravelerhistory = System.Configuration.ConfigurationManager.AppSettings["LinkLoginTravelerhistory"].ToString();
        string LinkLoginTravelerhistory = configApp.GetStringFromAppSettings("LinkLoginTravelerhistory") ?? "";

        #region auwat 20221026 1435 เพิ่มเก็บ log การส่ง mail => เนื่องจากมีกรณที่กดปุ่มแล้ว mail ไม่ไป
        private void write_log_mail(string step, string data_log)
        {
            try
            {
                logModel mLog = new logModel();
                mLog.module = "E-MAIL";
                mLog.tevent = step;//step
                mLog.ref_id = 0;
                mLog.data_log = data_log;
                logService.insertLog(mLog);
            }
            catch (Exception ex_write)
            {
                logModel mLog = new logModel();
                mLog.module = "E-MAIL";
                mLog.tevent = "write log Doc Service error";//step
                mLog.ref_id = 0;
                mLog.data_log = ex_write.Message.ToString();
                logService.insertLog(mLog);
            }
        }
        #endregion auwat 20221026 1435 เพิ่มเก็บ log การส่ง mail => เนื่องจากมีกรณที่กดปุ่มแล้ว mail ไม่ไป

        public string mail_group_admin(TOPEBizCreateTripEntities context, string role_type_name)
        {
            //??? น่าจะต้องทำเป็น stroe
            var admin_mail = "";
            var sql = @" select distinct  user_id, '' as user_name, email, role_type
                        from (
                        select a.emp_id as user_id, u.email , 'super_admin'  as role_type 
                         from bz_data_manage a inner join vw_bz_users u on a.emp_id = u.employeeid
                         where a.super_admin  = 'true' 
                         union
                         select a.emp_id as user_id, u.email , 'pmsv_admin'  as role_type 
                         from bz_data_manage a inner join vw_bz_users u on a.emp_id = u.employeeid
                         where  pmsv_admin =   'true'  
                         union
                         select a.emp_id as user_id, u.email , 'pmdv_admin'  as role_type 
                         from bz_data_manage a inner join vw_bz_users u on a.emp_id = u.employeeid
                         where  pmdv_admin =  'true'
                         union
                         select a.emp_id as user_id, u.email , 'contact_admin'  as role_type 
                         from bz_data_manage a inner join vw_bz_users u on a.emp_id = u.employeeid
                         where   contact_admin  =  'true'
                         )t where t.role_type = :role_type_name";

            var parameters = new List<OracleParameter>();
            parameters.Add(context.ConvertTypeParameter("role_type_name", role_type_name, "char"));
            var adminlistall = context.SearchUserEmailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

            if (adminlistall != null && adminlistall?.Count > 0)
            {
                admin_mail = string.Join(";", adminlistall.Select(a => a.email));
            }
            return admin_mail;
        }

        public void getTelServicesTeamCallCenter(ref string tel_services_team, ref string tel_call_center)
        {
            tel_services_team = "";
            tel_call_center = "";
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {

                    var sql = @" SELECT key_value as tel_services_team, '' as tel_call_center from bz_config_data where lower(key_name) = lower('tel_services_team') and status = 1";
                    List<OracleParameter> parameters = new List<OracleParameter>();
                    var tellist = context.TelephoneModelList.FromSqlRaw(sql).ToList();
                    if (tellist != null && tellist?.Count > 0)
                    {
                        try { tel_services_team = tellist[0].tel_services_team; } catch { }
                    }

                    sql = @" SELECT '' as tel_services_team, key_value as tel_call_center from bz_config_data where lower(key_name) = lower('tel_call_center') and status = 1";
                    parameters = new List<OracleParameter>();
                    tellist = context.TelephoneModelList.FromSqlRaw(sql).ToList();
                    if (tellist != null && tellist?.Count > 0)
                    {
                        try { tel_call_center = tellist[0].tel_call_center; } catch { }
                    }
                }
            }
            catch { }

        }
        public string get_mail_group_admin(TOPEBizCreateTripEntities context)
        {
            string admin_mail = "";
            var sql = "";
            var parameters = new List<OracleParameter>();

            try
            {
                sql = " SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email, role_id as role_type  FROM bz_users WHERE role_id = 1 ";
                parameters = new List<OracleParameter>();
                var adminList = context.SearchUserEmailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                if (adminList != null)
                {
                    foreach (var item in adminList)
                    {
                        admin_mail += ";" + item.email ?? "";
                    }
                    if (admin_mail != "") { admin_mail = ";" + admin_mail.Substring(1); }
                }
            }
            catch { }

            return admin_mail;
        }
        public void get_mail_requester_in_doc(TOPEBizCreateTripEntities context, string doc_id
            , ref string requester_name, ref string requester_mail, ref string on_behalf_of_mail)
        {
            requester_mail = "";
            requester_name = "";
            on_behalf_of_mail = "";

            var sql = "";
            var parameters = new List<OracleParameter>();

            string query_pis_emp_details = @" 
                        SELECT EMPLOYEEID as user_id
                        , nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name
                        , EMAIL email FROM BZ_USERS b";

            try
            {
                sql = $" {query_pis_emp_details} WHERE EMPLOYEEID IN ( SELECT DH_CREATE_BY FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id )";

                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                var requesterList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                if (requesterList != null)
                {
                    if (requesterList.Count > 0)
                    {
                        requester_mail = ";" + requesterList[0].email;
                        requester_name = requesterList[0].user_name;
                    }
                }



                sql = $" {query_pis_emp_details} WHERE EMPLOYEEID IN ( SELECT DH_BEHALF_EMP_ID FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";
                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                var behalfList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                if (behalfList != null)
                {
                    if (behalfList.Count > 0)
                    {
                        on_behalf_of_mail = ";" + behalfList[0].email;
                    }
                }


                sql = $" {query_pis_emp_details} WHERE EMPLOYEEID IN (SELECT DH_INITIATOR_EMPID FROM BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";
                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                var initial = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                if (initial != null && initial.Count() > 0)
                {
                    on_behalf_of_mail += ";" + initial[0].email;
                }
            }
            catch { }

        }

        public void get_user_role_in_token_login(TOPEBizCreateTripEntities context, string token_login
            , ref string user_name, ref string user_id, ref string user_role)
        {

            var sql = "";
            var parameters = new List<OracleParameter>();

            try
            {
                sql = @"SELECT a.user_id 
                        , nvl(u.ENTITLE,'')||' '||u.ENFIRSTNAME||' '||u.ENLASTNAME user_name 
                        , u.email email
                        , to_char(u.ROLE_ID) role_type
                        FROM bz_login_token a left join bz_users u on a.user_id=u.employeeid
                        WHERE a.TOKEN_CODE = :token_login ";

                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("token_login", token_login, "char"));
                var login_empid = context.SearchUserEmailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                if (login_empid != null && login_empid.Count() > 0)
                {
                    user_id = login_empid[0].user_id ?? "";
                    user_name = login_empid[0].user_name ?? "";
                    user_role = login_empid[0].role_type ?? "";
                }
            }
            catch { }
        }

        public string get_role_admin_in_manage(TOPEBizCreateTripEntities context, string user_id, string user_role)
        {
            var sql = "";
            var parameters = new List<OracleParameter>();
            var user_role_select = user_role ?? "";

            try
            {
                sql = @" select emp_id as user_id, '' as user_name , '' as email 
                         from bz_data_manage where (pmsv_admin = 'true' or pmdv_admin = 'true') and emp_id = :user_id ";
                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                var adminlist = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                if (adminlist != null)
                {
                    //if (adminlist.Count > 0) { user_role = "1"; }
                    if (adminlist.Count > 0) { user_role_select = "1"; }
                }
            }
            catch { }

            return user_role_select;
        }

        public ResultModel genDocNo(genDocNoModel value)
        {
            var data = new ResultModel();

            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
            {
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();

                    DbCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "bz_sp_gen_docno";
                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.Add(new OracleParameter("p_token", value.token_login));
                    cmd.Parameters.Add(new OracleParameter("p_doc_type", value.doc_type));

                    OracleParameter oraP = new OracleParameter();
                    oraP.ParameterName = "ret_docno";
                    oraP.Size = 20;
                    oraP.OracleDbType = OracleDbType.Varchar2;
                    oraP.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(oraP);

                    try
                    {
                        cmd.ExecuteNonQuery();

                        string docno = cmd.Parameters["ret_docno"].Value.ToString();

                        data.status = "S";
                        data.message = "";
                        data.value = docno;
                    }
                    catch (Exception ex)
                    {
                        data.status = "E";
                        data.message = ex.Message;
                    }
                }
            }

            return data;
        }

        public ResultModel copyDocNo(CopyDocModel value)
        {
            var data = new ResultModel();

            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
            {
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();
                    DbCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "bz_sp_copy_doc";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new OracleParameter("p_token", value.token));
                    cmd.Parameters.Add(new OracleParameter("p_doc_no", value.id_doc));

                    OracleParameter oraP = new OracleParameter();
                    oraP.ParameterName = "ret_docno";
                    oraP.Size = 20;
                    oraP.OracleDbType = OracleDbType.Varchar2;
                    oraP.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(oraP);
                    try
                    {
                        cmd.ExecuteNonQuery();

                        string docno = cmd.Parameters["ret_docno"].Value.ToString();

                        data.status = "S";
                        data.message = "";
                        data.value = docno;
                    }
                    catch (Exception ex)
                    {
                        data.status = "E";
                        data.message = ex.Message;
                    }
                }
            }

            return data;
        }

        private bool AllApproveCAPApprover(string doc_id, ref string ret_doc_status)
        {
            bool ret = false;
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var query = context.AllApproveModelList.FromSqlRaw(
                        @" select case when xcount_all=xcount_approve then (case when xcount_all=xcount_cancel then 2 else 0 end) else 1 end status_value, '' as emp_id, '' as doc_status   
                             from (
                               select  sum(case when a.dta_doc_status in(40,41,42)  then 1 else 0 end) xcount_all
                               , sum(case when a.dta_doc_status in(40,42) then 1 else 0 end) xcount_approve 
                               , sum(case when a.dta_doc_status in(40) then 1 else 0 end) xcount_cancel 
                               from bz_doc_traveler_approver a   
                               where a.dta_doc_status in(40,41,42) and a.dh_code = :doc_id )t",
                       context.ConvertTypeParameter("doc_id", doc_id, "char")).ToList().FirstOrDefault();

                    if (query == null)
                        return false;

                    decimal? doc_status = 50; // approve all  
                    if (query.status_value == 2)
                        doc_status = 40; // Cancel by CAP Approver

                    if (query.status_value == 1)
                    {

                        return false; // ยังมีรายการที่ยังไม่ได้ Approver
                    }

                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var doc_head_search = context.BZ_DOC_HEAD.Find(doc_id);
                            if (doc_head_search == null)
                            {
                                return false;
                            }

                            doc_head_search.DH_DOC_STATUS = doc_status;

                            //string sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate  ";
                            //sql += ", ACTION_STATUS=2 ";
                            //sql += " where dh_code='" + doc_id + "' and EMP_ID='admin' ";
                            //sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";
                            //context.Database.ExecuteSqlCommand(sql);
                            // คำสั่ง SQL สำหรับการอัปเดต
                            string sql = "UPDATE BZ_DOC_ACTION SET ACTION_DATE = sysdate, ACTION_STATUS = 2 " +
                                         "WHERE EMP_ID = 'admin' AND DOC_STATUS = 41 AND ACTION_STATUS = 1 AND dh_code = :doc_id ";

                            // ใช้ ExecuteSqlRaw เพื่อความปลอดภัยและรองรับ EF Core รุ่นใหม่
                            context.Database.ExecuteSqlRaw(sql, new OracleParameter("doc_id", doc_id));

                            context.SaveChanges();
                            transaction.Commit();

                            ret_doc_status = doc_status?.ToString() ?? "";
                            ret = true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ret = false;
            }
            return ret;
        }
         
        private bool AllApproveLineApprover(string docId, ref string retDocStatus)
        {
            bool isApproved = false;

            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    decimal? docStatus = 41; // ค่าเริ่มต้นสถานะ

                    // ตรวจสอบสถานะการอนุมัติ
                    var query = context.AllApproveModelList.FromSqlRaw(
                        @"SELECT COUNT(1) AS status_value, 
                         '' AS emp_id, 
                         '' AS doc_status
                  FROM bz_doc_traveler_approver a
                  WHERE a.dta_action_status IN (1, 2, 4)
                    AND a.dta_type = 1
                    AND a.dh_code = :doc_id",
                        context.ConvertTypeParameter("doc_id", docId, "char")
                    ).ToList().FirstOrDefault();

                    if (query == null || query.status_value > 0)
                    {
                        // มีรายการที่ยังไม่ได้ Action หรือ Query ไม่คืนค่า
                        return false;
                    }

                    // คำนวณสถานะการอนุมัติ
                    query = context.AllApproveModelList.FromSqlRaw(
                        @"SELECT (
                        (SELECT COUNT(1) 
                         FROM bz_doc_traveler_approver a 
                         WHERE a.dta_type = 1 AND a.dh_code = :doc_id) - 
                        (SELECT COUNT(1) 
                         FROM bz_doc_traveler_approver a 
                         WHERE a.dta_type = 1 AND a.dta_action_status IN (5) AND a.dh_code = :doc_id)
                     ) AS status_value, 
                     '' AS emp_id, 
                     '' AS doc_status
                  FROM dual",
                        context.ConvertTypeParameter("doc_id", docId, "char")
                    ).ToList().FirstOrDefault();

                    if (query != null && query.status_value == 0)
                    {
                        docStatus = 30; // Cancel by Line Approver
                    }

                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // ค้นหาเอกสาร
                            var docHead = context.BZ_DOC_HEAD.Find(docId);
                            if (docHead == null)
                            {
                                return false; // ไม่พบเอกสาร
                            }

                            // อัปเดตสถานะเอกสาร
                            docHead.DH_DOC_STATUS = docStatus;

                            if (query != null && query.status_value > 0)
                            {
                                // ค้นหา CAP Users
                                var capUsers = context.Database.SqlQueryRaw<capApproveModel>(
                                    @"SELECT DISTINCT dta_appr_empid AS appr_empid, 
                                                dta_travel_empid AS traverler_empid
                              FROM bz_doc_traveler_approver
                              WHERE dh_code = :doc_id AND dta_type = 2",
                                    context.ConvertTypeParameter("doc_id", docId, "char")
                                ).ToList();

                                if (!capUsers.Any() && docStatus == 41)
                                {
                                    // หากไม่มี CAP User และสถานะเป็น Pending
                                    docHead.DH_DOC_STATUS = 50; // Pending by CAP
                                }
                                else
                                {
                                    // เพิ่ม Action สำหรับ CAP Users
                                    foreach (var user in capUsers)
                                    {
                                        string sqlInsertCAP = @"
                                    INSERT INTO BZ_DOC_ACTION (DA_TOKEN, DH_CODE, DOC_TYPE, DOC_STATUS, EMP_ID, TAB_NO, FROM_EMP_ID, ACTION_STATUS)
                                    VALUES (:token, :doc_id, :doc_type, 41, :user_id, 4, :traveler_id, 1)";

                                        var parameters = new List<OracleParameter>
                                {
                                    context.ConvertTypeParameter("token", Guid.NewGuid().ToString(), "char"),
                                    context.ConvertTypeParameter("doc_id", docId, "char"),
                                    context.ConvertTypeParameter("doc_type", docHead.DH_TYPE, "char"),
                                    context.ConvertTypeParameter("user_id", user.appr_empid??"", "char"),
                                    context.ConvertTypeParameter("traveler_id", user.traverler_empid??"", "char")
                                };

                                        context.Database.ExecuteSqlRaw(sqlInsertCAP, parameters.ToArray());
                                    }

                                    // อัปเดตสถานะ Approver
                                    string sqlUpdateApprover = @"
                                UPDATE BZ_DOC_TRAVELER_APPROVER
                                SET DTA_DOC_STATUS = 41, DTA_ACTION_STATUS = 2
                                WHERE dh_code = :doc_id AND DTA_TYPE = 2";

                                    var updateApproverParams = new List<OracleParameter>
                            {
                                context.ConvertTypeParameter("doc_id", docId, "char")
                            };

                                    context.Database.ExecuteSqlRaw(sqlUpdateApprover, updateApproverParams.ToArray());

                                    // อัปเดต Action
                                    string sqlUpdateAction = @"
                                UPDATE BZ_DOC_ACTION
                                SET ACTION_DATE = SYSDATE, ACTION_STATUS = 2
                                WHERE dh_code = :doc_id AND TAB_NO = 3 AND ACTION_STATUS = 1";

                                    var updateActionParams = new List<OracleParameter>
                            {
                                context.ConvertTypeParameter("doc_id", docId, "char")
                            };

                                    context.Database.ExecuteSqlRaw(sqlUpdateAction, updateActionParams.ToArray());
                                }
                            }

                            context.SaveChanges();
                            transaction.Commit();

                            // ตรวจสอบสถานะสุดท้าย
                            if (query != null && query.status_value > 0)
                            {
                                isApproved = AllApproveCAPApprover(docId, ref retDocStatus);
                            }

                            isApproved = true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error: {ex.Message}"); // เพิ่ม Logging หรือปรับใช้ระบบ Log ตามที่ต้องการ
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // เพิ่ม Logging หรือปรับใช้ระบบ Log ตามที่ต้องการ
            }

            return isApproved;
        }

        public ResultModel submitFlow1(DocModel value)
        {
            var parameters = new List<OracleParameter>();
            DbCommand cmd = null;
            int iResult = -1;
            decimal? decimalNull = null;
            bool newDocNo = false;
            decimal? doc_status = 1;
            decimal? old_doc_status = 0;
            string user_id = "";
            string token_update = Guid.NewGuid().ToString();
            int tab_no = 1;

            var pf_doc_id = "";
            var doc_id = value.id ?? "";
            var token_login = value.token_login ?? "";

            //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
            bool type_flow = true;
            if ((value.type_flow ?? "1") != "1") { type_flow = false; }

            var data = new ResultModel();
            try
            {
                // save
                if (value.action.type == "1")
                {
                    doc_status = 11;
                }
                else if (value.action.type == "5") // submit
                {
                    if (value.initiator.status == "true")
                    {
                        doc_status = 22;
                        tab_no = 1;
                    }
                    else
                    {
                        doc_status = 21;
                        tab_no = 2;
                    }
                }
                else if (value.action.type == "6") // cancel
                {
                    doc_status = 10;
                }

                var Tel_Services_Team = "";
                var Tel_Call_Center = "";
                getTelServicesTeamCallCenter(ref Tel_Services_Team, ref Tel_Call_Center);

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    // SH,VP,AEP,EVP,SEVP,CEO 
                    var empList = from emp in context.BZ_USERS
                                  select new
                                  {
                                      emp.EMPLOYEEID,
                                      emp.ENTITLE,
                                      emp.ENFIRSTNAME,
                                      emp.ENLASTNAME,
                                      emp.REPORTTOID,
                                      emp.DEPARTMENT,
                                      emp.SH,
                                      emp.VP,
                                      emp.AEP,
                                      emp.EVP,
                                      emp.SEVP,
                                      emp.CEO,
                                      MANAGER_EMPID = (
                                      emp.SH != null ? emp.SH :
                                        emp.VP != null ? emp.VP :
                                        emp.AEP != null ? emp.AEP :
                                        emp.EVP != null ? emp.EVP :
                                        emp.SEVP != null ? emp.SEVP :
                                        emp.CEO != null ? emp.CEO :
                                        emp.MANAGER_EMPID
                                        )
                                  };

                    var doc_head_search = context.BZ_DOC_HEAD.Where(p => p.DH_CODE.Equals(value.id)).ToList().FirstOrDefault();

                    if (doc_head_search == null)
                        newDocNo = true;
                    else
                    {
                        if (value.action.type == "5") // submit
                        {
                            pf_doc_id = doc_head_search.DH_DOC_STATUS.ToString();

                            if (doc_head_search.DH_DOC_STATUS == 22 || doc_head_search.DH_DOC_STATUS == 31 || doc_head_search.DH_DOC_STATUS == 41)
                            {
                                doc_status = 21;
                                tab_no = 2;
                            }
                        }
                    }

                    using (var transaction = context.Database.BeginTransaction())
                    {
                        string sql = @"SELECT  user_id, user_name, '' as email FROM bz_login_token WHERE TOKEN_CODE = :token_login ";
                        var param_token_login = context.ConvertTypeParameter(":token_login", token_login, "char");
                        cmd = context.Database.GetDbConnection().CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Parameters.Add(param_token_login);
                        var User = context.Database.select<SearchUserNameModel>(cmd);
                        if (User != null && User.Count() > 0)
                        {
                            user_id = User[0].user_id ?? "";
                        }

                        sql = "SELECT  EMPLOYEEID as user_id, '' as user_name,email FROM bz_users WHERE role_id = 1 ";
                        var adminList = context.Database.SqlQueryRaw<SearchUserNameModel>($"{sql}").ToList();

                        sql = @"SELECT io, COST_CENTER_RESP cc FROM BZ_MASTER_IO ";
                        var ccio = context.CostcenterIOList.FromSqlRaw(sql).ToList();

                        #region DevFix 20200909 1606 กรณที่กรอกข้อมูล GL Account ใหม่ที่ไม่ได้อยู่ใน master ให้เพิ่มเข้าระบบ 
                        sql = " select GL_NO from BZ_MASTER_GL order by GL_NO ";
                        //var ccgl_account = context.Database.SqlQuery<gl_account>(sql).ToList();
                        var ccgl_account = context.GLAccountList.FromSqlRaw(sql, new OracleParameter()).ToList();
                        #endregion DevFix 20200909 1606 กรณที่กรอกข้อมูล GL Account ใหม่ที่ไม่ได้อยู่ใน master ให้เพิ่มเข้าระบบ


                        #region DevFix 20200911 0000 ส่งเมลแจ้งคนที่ On behalf of  

                        var param_list = new OracleParameter();
                        sql = @"SELECT EMPLOYEEID user_id, EMAIL email FROM bz_users ";
                        if (value.behalf.emp_id == "")
                        {
                            sql += " WHERE 1=2";
                        }
                        else
                        {
                            sql += " WHERE EMPLOYEEID = :emp_id";
                            param_list = context.ConvertTypeParameter("emp_id", value.behalf.emp_id, "char");
                        }
                        var behalfList = context.Database.SqlQueryRaw<SearchUserModelX>(sql, param_list).ToList();

                        string on_behalf_of_mail = "";
                        if (behalfList != null)
                        {
                            if (behalfList.Count > 0)
                            {
                                on_behalf_of_mail = ";" + behalfList[0].email;
                            }
                        }
                        #endregion DevFix 20200911 0000 ส่งเมลแจ้งคนที่ On behalf of 

                        try
                        {
                            #region data
                            if (true)
                            {

                                #region "### BZ_DOC_HEAD ####"

                                var pcount = 0;
                                if (value.summary_table != null)
                                {
                                    var temp = value.summary_table.GroupBy(g => g.emp_id).ToList();
                                    pcount = temp.Count();
                                }

                                //DevFix 20210806 0000 เพิ่มตรวจสอบข้อมูลถ้า status = flase ให้ emp id = null
                                if (value.initiator.status == "false")
                                {
                                    value.initiator.emp_id = "";
                                }

                                if (newDocNo)
                                {
                                    BZ_DOC_HEAD head = new BZ_DOC_HEAD();
                                    head.DH_CODE = value.id ?? "";
                                    head.DH_TYPE = value.type ?? "";
                                    head.DH_BEHALF_EMP_ID = value.behalf.emp_id ?? "";
                                    head.DH_COM_CODE = value.id_company ?? "";
                                    head.DH_TOPIC = value.topic_of_travel ?? "";
                                    head.DH_TRAVEL = value.travel ?? "";
                                    head.DH_CITY = value.city ?? "";

                                    //DevFix 20210816 0000 เพิ่มจำกัดข้อมูล Length 4000
                                    if (value.travel_objective_expected != null)
                                    {
                                        //try
                                        //{
                                        //    value.travel_objective_expected = (value.travel_objective_expected).ToString().Substring(0, 4000);
                                        //}
                                        //catch { }
                                        value.travel_objective_expected = value.travel_objective_expected.ToString().Replace("\n", Environment.NewLine);
                                    }
                                    head.DH_TRAVEL_OBJECT = value.travel_objective_expected ?? "";


                                    head.DH_BUS_FROMDATE = chkDate(value.business_date.start ?? "");
                                    head.DH_BUS_TODATE = chkDate(value.business_date.stop ?? "");
                                    head.DH_TRAVEL_FROMDATE = chkDate(value.travel_date.start ?? "");
                                    head.DH_TRAVEL_TODATE = chkDate(value.travel_date.stop ?? "");

                                    head.DH_INITIATOR_EMPID = value.initiator.emp_id ?? "";
                                    head.DH_INITIATOR_REMARK = value.initiator.remark ?? "";
                                    head.DH_AFTER_TRIP_OPT1 = retCheckValue(value.after_trip.opt1 ?? "");
                                    head.DH_AFTER_TRIP_OPT2 = retCheckValue(value.after_trip.opt2.status ?? "");
                                    head.DH_AFTER_TRIP_OPT3 = retCheckValue(value.after_trip.opt3.status ?? "");
                                    head.DH_AFTER_TRIP_OPT2_REMARK = value.after_trip.opt2.remark ?? "";
                                    head.DH_AFTER_TRIP_OPT3_REMARK = value.after_trip.opt3.remark ?? "";
                                    head.DH_REMARK = value.remark ?? "";
                                    head.DH_TOTAL_PERSON = pcount; // value.summary_table == null ? 0 : value.summary_table.Count();
                                    head.DH_CREATE_DATE = DateTime.Now;

                                    head.DH_DOC_STATUS = doc_status;

                                    head.DH_CREATE_BY = user_id;
                                    head.DH_UPDATE_BY = user_id;
                                    head.DH_UPDATE_DATE = DateTime.Now;

                                    ////DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW 
                                    context.BZ_DOC_HEAD.Add(head);
                                }
                                else
                                {
                                    old_doc_status = doc_head_search.DH_DOC_STATUS;

                                    doc_head_search.DH_TYPE = value.type ?? "";
                                    doc_head_search.DH_BEHALF_EMP_ID = value.behalf.emp_id ?? "";
                                    doc_head_search.DH_COM_CODE = value.id_company ?? "";
                                    doc_head_search.DH_TOPIC = value.topic_of_travel ?? "";
                                    doc_head_search.DH_TRAVEL = value.travel ?? "";
                                    doc_head_search.DH_CITY = value.city ?? "";

                                    //DevFix 20210816 0000 เพิ่มจำกัดข้อมูล Length 4000
                                    if (value.travel_objective_expected != null)
                                    {
                                        value.travel_objective_expected = value.travel_objective_expected.ToString().Replace("\n", Environment.NewLine);
                                    }
                                    doc_head_search.DH_TRAVEL_OBJECT = value.travel_objective_expected ?? "";
                                    doc_head_search.DH_BUS_FROMDATE = chkDate(value.business_date.start ?? "");
                                    doc_head_search.DH_BUS_TODATE = chkDate(value.business_date.stop ?? "");
                                    doc_head_search.DH_TRAVEL_FROMDATE = chkDate(value.travel_date.start ?? "");
                                    doc_head_search.DH_TRAVEL_TODATE = chkDate(value.travel_date.stop ?? "");
                                    doc_head_search.DH_INITIATOR_EMPID = value.initiator.emp_id ?? "";
                                    doc_head_search.DH_INITIATOR_REMARK = value.initiator.remark ?? "";
                                    doc_head_search.DH_AFTER_TRIP_OPT1 = retCheckValue(value.after_trip.opt1 ?? "");
                                    doc_head_search.DH_AFTER_TRIP_OPT2 = retCheckValue(value.after_trip.opt2.status ?? "");
                                    doc_head_search.DH_AFTER_TRIP_OPT3 = retCheckValue(value.after_trip.opt3.status ?? "");
                                    doc_head_search.DH_AFTER_TRIP_OPT2_REMARK = value.after_trip.opt2.remark ?? "";
                                    doc_head_search.DH_AFTER_TRIP_OPT3_REMARK = value.after_trip.opt3.remark ?? "";
                                    doc_head_search.DH_REMARK = value.remark ?? "";
                                    doc_head_search.DH_TOTAL_PERSON = pcount;// value.summary_table == null ? 0 : value.summary_table.Count();
                                    doc_head_search.DH_UPDATE_BY = user_id;
                                    doc_head_search.DH_UPDATE_DATE = DateTime.Now;
                                    if (old_doc_status < 30)
                                        doc_head_search.DH_DOC_STATUS = doc_status;

                                    if (doc_status == 10)
                                        doc_head_search.DH_REMARK_REJECT = value.action.remark ?? "";
                                    else
                                        doc_head_search.DH_REMARK_REJECT = "";


                                    //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW 
                                    if (value.action.type == "1" || value.action.type == "5")
                                    {
                                        //doc_head_search.DH_TYPE_FLOW = value.type_flow ?? ""; 
                                        var stype_flow = value.type_flow ?? "";

                                        //context.Database.ExecuteSqlCommand("UPDATE BZ_DOC_HEAD SET DH_TYPE_FLOW = " + (value.type_flow ?? "") + " WHERE DH_CODE = '" + value.id + "'");

                                        sql = @"UPDATE BZ_DOC_HEAD SET DH_TYPE_FLOW = :stype_flow WHERE DH_CODE = :doc_id ";
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("stype_flow", stype_flow, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                                        // Execute SQL โดยส่งพารามิเตอร์ที่แปลงแล้วเข้าไป
                                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());


                                    }

                                }

                                #endregion "### BZ_DOC_HEAD ####"

                                #region "#### BZ_DOC_TRAVEL_TYPE ####"

                                //context.Database.ExecuteSqlCommand("DELETE FROM BZ_DOC_TRAVEL_TYPE WHERE DH_CODE = '" + value.id + "'");
                                sql = @"DELETE FROM BZ_DOC_TRAVEL_TYPE WHERE DH_CODE = :doc_id";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                                // Execute SQL โดยส่งพารามิเตอร์ที่แปลงแล้วเข้าไป
                                context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                if ((value.type_of_travel.meeting ?? "") == "true")
                                {
                                    parameters.Clear();
                                    sql = @" INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 1, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                }
                                if ((value.type_of_travel.siteVisite ?? "") == "true")
                                {
                                    parameters.Clear();
                                    sql = @"
                                      INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 2, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.workshop ?? "") == "true")
                                {
                                    parameters.Clear();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 3, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.roadshow ?? "") == "true")
                                {
                                    parameters.Clear();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 4, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.conference ?? "") == "true")
                                {
                                    parameters.Clear();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 5, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.other ?? "") == "true")
                                {
                                    parameters.Clear();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 6, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                //DevFix 20220805 --> after go-live เพิ่ม Tick box = Training 
                                if ((value.type_of_travel.training ?? "") == "true")
                                {
                                    parameters.Clear();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 7, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }

                                #endregion "#### BZ_DOC_TRAVEL_TYPE ####"

                                #region "#### BZ_DOC_CONTIENT ####"

                                //context.Database.ExecuteSqlCommand("DELETE FROM BZ_DOC_CONTIENT WHERE DH_CODE = '" + value.id + "'");
                                sql = @"DELETE FROM BZ_DOC_CONTIENT WHERE DH_CODE = :doc_id";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                if (!string.IsNullOrEmpty(value.type))
                                {
                                    if (value.type.ToString() == "local")
                                    {
                                        parameters.Clear();
                                        sql = @"INSERT INTO BZ_DOC_CONTIENT (DH_CODE, CTN_ID) VALUES(:DH_CODE, :CTN_ID) ";
                                        parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("CTN_ID", Convert.ToDecimal("1"), "int"));
                                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }
                                    else
                                    {
                                        foreach (var ic in value.continent)
                                        {
                                            parameters.Clear();
                                            sql = @"INSERT INTO BZ_DOC_CONTIENT (DH_CODE, CTN_ID) VALUES(:DH_CODE, :CTN_ID)";
                                            parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("CTN_ID", Convert.ToDecimal(ic.id), "int"));
                                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        }
                                    }
                                }

                                #endregion "#### BZ_DOC_CONTIENT ####"

                                #region "### BZ_DOC_COUNTRY ####"

                                sql = @"DELETE FROM BZ_DOC_COUNTRY WHERE DH_CODE = :doc_id";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                if (value.type.ToString() == "local")
                                {
                                    parameters.Clear();
                                    sql = @"
                                    INSERT INTO BZ_DOC_COUNTRY (DH_CODE, CT_ID) VALUES(:DH_CODE, :CT_ID)
                                    ";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("CT_ID", 19, "int"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                else
                                {
                                    decimal no = 0;
                                    foreach (var c in value.country)
                                    {
                                        no++;

                                        parameters.Clear();
                                        sql = @"INSERT INTO BZ_DOC_COUNTRY (DH_CODE, CT_ID) VALUES(:DH_CODE, :CT_ID)";
                                        parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("CT_ID", string.IsNullOrEmpty(c.contry_id) ? 0 : Convert.ToDecimal(c.contry_id), "int"));
                                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }
                                }

                                #endregion

                                #region "#### BZ_DOC_PROVINCE ####"

                                sql = @"DELETE FROM BZ_DOC_PROVINCE WHERE DH_CODE = :doc_id";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                foreach (var c in value.province)
                                {
                                    parameters.Clear();
                                    sql = @"INSERT INTO BZ_DOC_PROVINCE (DH_CODE, PV_ID) VALUES(:DH_CODE, :PV_ID)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("PV_ID", string.IsNullOrEmpty(c.province_id) ? 0 : Convert.ToDecimal(c.province_id), "int"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }

                                #endregion "#### BZ_DOC_PROVINCE ####"

                                #region "#### BZ_DOC_TRAVELER_EXPENSE ####"

                                var mas_country = context.BZ_MASTER_COUNTRY.ToList();

                                int i = 0;
                                var traveler_expen = context.BZ_DOC_TRAVELER_EXPENSE.Where(p => p.DH_CODE.Equals(value.id) && p.DTE_STATUS != 0).ToList();

                                foreach (var c in value.summary_table)
                                {
                                    i++;
                                    // case type = local --> continent_id =1(asia) and country_id = 19(thai)
                                    string continent_id = "1"; // asia
                                    string country_id = "19";
                                    if (value.type == "oversea" || value.type == "overseatraining")
                                    {
                                        if (!string.IsNullOrEmpty(c.country_id))
                                        {
                                            var citem = mas_country.Where(p => p.CT_ID.Equals(Convert.ToDecimal(c.country_id))).ToList().FirstOrDefault();
                                            if (citem != null)
                                            {
                                                continent_id = citem.CTN_ID.ToString();
                                                country_id = c.country_id;
                                            }
                                        }
                                    }

                                    DateTime? business_date_start = null;
                                    DateTime? business_date_stop = null;
                                    DateTime? travel_date_start = null;
                                    DateTime? travel_date_stop = null;
                                    if (value.travel == "1") // single
                                    {
                                        business_date_start = chkDate(value.business_date.start ?? "");
                                        business_date_stop = chkDate(value.business_date.stop ?? "");
                                        travel_date_start = chkDate(value.travel_date.start ?? "");
                                        travel_date_stop = chkDate(value.travel_date.stop ?? "");
                                    }
                                    else
                                    {
                                        business_date_start = chkDate(c.business_date.start ?? "");
                                        business_date_stop = chkDate(c.business_date.stop ?? "");
                                        travel_date_start = chkDate(c.travel_date.start ?? "");
                                        travel_date_stop = chkDate(c.travel_date.stop ?? "");
                                    }

                                    BZ_DOC_TRAVELER_EXPENSE row_update = null;
                                    bool updateStatus = false;
                                    if (traveler_expen != null && traveler_expen.Count() > 0)
                                    {
                                        //เนื่องจากหน้าบ้านไม่ได้ส่งข้อมูล มาให้ function นี้เลยไม่มีผล ทำให้เป็นข้อมูลใหม่เสมอ  
                                        //DevFix 20210817 เพิ่ม key traveler_ref_id เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id 
                                        if (c.traveler_ref_id != null)
                                        {
                                            //DevFix 20210817 เพิ่ม key traveler_ref_id เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
                                            //เนื่องจากไม่สามารถ up dataset model ได้ให้ใช้ DTE_TOKEN แทน 
                                            row_update = traveler_expen.Where(p => p.DTE_TOKEN.Equals(c.traveler_ref_id)).ToList().FirstOrDefault();

                                            if (row_update != null && row_update.DH_CODE != null)
                                                updateStatus = true;
                                        }
                                    }


                                    if (updateStatus == false)
                                    {
                                        context.BZ_DOC_TRAVELER_EXPENSE.Add(new BZ_DOC_TRAVELER_EXPENSE
                                        {
                                            DH_CODE = value.id,
                                            DTE_ID = i,
                                            CTN_ID = retDecimal(continent_id),// retDecimal(c.continent_id),
                                            CT_ID = retDecimal(country_id), //retDecimal(c.country_id),
                                            PV_ID = retDecimal(c.province_id),
                                            CITY_TEXT = c.city ?? "",
                                            DTE_BUS_FROMDATE = business_date_start, //chkDate((c.business_date.start ?? "")),
                                            DTE_BUS_TODATE = business_date_stop, //chkDate((c.business_date.stop ?? "")),
                                            DTE_TRAVEL_FROMDATE = travel_date_start, //chkDate((c.travel_date.start ?? "")),
                                            DTE_TRAVEL_TODATE = travel_date_stop, //chkDate((c.travel_date.stop ?? "")),
                                            DTE_TRAVEL_DAYS = retDecimal(c.travel_duration),
                                            DTE_EMP_ID = c.emp_id ?? "",
                                            DTE_COST_CENTER = retText(c.cost ?? ""),
                                            DTE_GL_ACCOUNT = c.gl_account ?? "",
                                            DTE_ORDER_WBS = c.order ?? "",
                                            DTE_TRAVELER_REMARK = c.remark ?? "",
                                            DTE_TOKEN_UPDATE = token_update,

                                            //DevFix 20210817 เพิ่ม key traveler_ref_id เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
                                            //เนื่องจากไม่สามารถ up dataset model ได้ให้ใช้ DTE_TOKEN แทน
                                            //DTE_TRAVELER_REF_ID = Guid.NewGuid().ToString(),
                                            DTE_TOKEN = Guid.NewGuid().ToString(),


                                            DTE_STATUS = 1,
                                            //DTE_EXPENSE_CONFIRM = 1,//ค่าที่ต้อง update ใน step 2 เพื่อยืนยันว่ามีการกรอกข้อมูล Expense แล้ว


                                        });
                                    }
                                    else
                                    {
                                        row_update.DTE_ID = i;
                                        row_update.CTN_ID = retDecimal(continent_id);
                                        row_update.CT_ID = retDecimal(country_id);
                                        row_update.PV_ID = retDecimal(c.province_id);
                                        row_update.CITY_TEXT = c.city ?? "";
                                        row_update.DTE_BUS_FROMDATE = business_date_start;
                                        row_update.DTE_BUS_TODATE = business_date_stop;
                                        row_update.DTE_TRAVEL_FROMDATE = travel_date_start;
                                        row_update.DTE_TRAVEL_TODATE = travel_date_stop;
                                        row_update.DTE_TRAVEL_DAYS = retDecimal(c.travel_duration);
                                        row_update.DTE_EMP_ID = c.emp_id ?? "";
                                        row_update.DTE_COST_CENTER = retText(c.cost ?? "");
                                        row_update.DTE_GL_ACCOUNT = c.gl_account ?? "";
                                        row_update.DTE_ORDER_WBS = c.order ?? "";
                                        row_update.DTE_TRAVELER_REMARK = c.remark ?? "";
                                        row_update.DTE_TOKEN_UPDATE = token_update;
                                        row_update.DTE_STATUS = 1;
                                        //row_update.DTE_EXPENSE_CONFIRM = 1;//ค่าที่ต้อง update ใน step 2 เพื่อยืนยันว่ามีการกรอกข้อมูล Expense แล้ว

                                        //DevFix 20210817 เพิ่ม key traveler_ref_id เพื่อใช้ในการแยกข้อมูลออกแต่ละรายการ เนื่องจากเงื่อนไขเดิมข้อมูลซ้ำ --> เก็บค่าเป็น token id
                                        //เนื่องจากไม่สามารถ up dataset model ได้ให้ใช้ DTE_TOKEN แทน
                                        //row_update.DTE_TOKEN_UPDATE = c.traveler_ref_id;
                                        row_update.DTE_TOKEN = c.traveler_ref_id;

                                    }

                                    if (!string.IsNullOrEmpty(c.order))
                                    {
                                        var iocheck = ccio.Where(p => p.io.ToUpper().Equals(c.order.ToUpper().Trim()))?.ToList();
                                        if (iocheck == null || iocheck.Count() == 0)
                                        {
                                            // กำหนดค่าที่ต้องการให้กับฟิลด์
                                            var field_order = c.order.ToUpper().Trim();
                                            var field_cost = retText(c.cost ?? "");

                                            // สร้าง SQL ที่ใช้พารามิเตอร์ (Parameterized query)
                                            sql = @"INSERT INTO BZ_MASTER_IO (IO, COST_CENTER_RESP)  SELECT :field_order, :field_cost FROM dual WHERE NOT EXISTS (  SELECT * FROM BZ_MASTER_IO WHERE UPPER(IO) = :field_order )";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("field_order", field_order, "char"));
                                            parameters.Add(context.ConvertTypeParameter("field_cost", field_cost, "char"));

                                            // เรียกใช้คำสั่ง SQL ผ่าน FromSqlRaw หรือ ExecuteSqlRaw
                                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());


                                        }
                                    }

                                    #region DevFix 20200909 1606 กรณที่กรอกข้อมูล GL Account ใหม่ที่ไม่ได้อยู่ใน master ให้เพิ่มเข้าระบบ 
                                    if (!string.IsNullOrEmpty(c.gl_account))
                                    {
                                        var gl_account_def = (c.gl_account ?? "").Replace("'", "''").Replace("\t", "");
                                        gl_account_def = gl_account_def.Trim();

                                        var gl_accountcheck = ccgl_account.Where(p => p.gl_no.Trim().ToUpper().Equals(gl_account_def.ToUpper()))?.ToList();
                                        if (gl_accountcheck == null || gl_accountcheck.Count() == 0)
                                        {
                                            sql = "insert into BZ_MASTER_GL (GL_NO,USERSTATUS) ";
                                            sql += " select :gl_account_def, 1 AS USERSTATUS from dual ";
                                            sql += " where upper(:gl_account_def) not in (select upper(GL_NO) as GL_NO  from BZ_MASTER_GL where (upper(GL_NO) = :gl_account_def )) ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("gl_account_def", gl_account_def, "char"));

                                            // เรียกใช้คำสั่ง SQL ผ่าน FromSqlRaw หรือ ExecuteSqlRaw
                                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        }
                                    }
                                    #endregion DevFix 20200909 1606 กรณที่กรอกข้อมูล GL Account ใหม่ที่ไม่ได้อยู่ใน master ให้เพิ่มเข้าระบบ

                                }

                                var row_delete = traveler_expen?.Where(p => p.DTE_TOKEN_UPDATE != token_update).ToList();
                                if (row_delete != null && row_delete.Count() > 0)
                                {
                                    foreach (var d in row_delete)
                                    {
                                        d.DTE_STATUS = 0;
                                    }
                                }
                                #endregion "#### BZ_DOC_TRAVELER_EXPENSE ####"

                                #region "#### NEW EMPLOYEE ####"

                                var empNotIn = (from t in value.summary_table
                                                join e in empList on t.emp_id equals e.EMPLOYEEID into e2
                                                from f in e2.DefaultIfEmpty()
                                                select new
                                                {
                                                    t.emp_id,
                                                    t.emp_name,
                                                    chk = f?.ENLASTNAME ?? "-"
                                                }).Where(p => p.chk.Equals("-"));

                                if (empNotIn != null)
                                {
                                    foreach (var item in empNotIn)
                                    {
                                        context.BZ_USERS.Add(new BZ_USERS
                                        {
                                            EMPLOYEEID = item.emp_id,
                                            ENFIRSTNAME = item.emp_name,
                                            EMPSTATUS = "1"
                                        });
                                    }
                                }

                                #endregion "#### NEW EMPLOYEE ####"

                                #region "#### [SUBMIT] >> BZ_DOC_ACTION, BZ_DOC_TRAVELER_APPROVER ####"

                                if (value.action.type == "5") // submit
                                {
                                    #region "BZ_DOC_ACTION"

                                    //DevFix 20200916 1029 กรณีที่เป็นการ submit ไปให้ admin  ไม่ต้อง add action Initiator
                                    if (tab_no == 1)
                                    {
                                        if (!string.IsNullOrEmpty(value.initiator.emp_id))
                                        {
                                            context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                            {
                                                DA_TOKEN = Guid.NewGuid().ToString(),
                                                DH_CODE = value.id,
                                                DOC_TYPE = value.type,
                                                DOC_STATUS = doc_status,
                                                EMP_ID = value.initiator.emp_id,
                                                TAB_NO = tab_no,
                                                ACTION_STATUS = 1,
                                                CREATED_DATE = DateTime.Now,
                                                UPDATED_DATE = DateTime.Now,
                                            });
                                        }
                                    }

                                    context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                    {
                                        DA_TOKEN = Guid.NewGuid().ToString(),
                                        DH_CODE = value.id,
                                        DOC_TYPE = value.type,
                                        DOC_STATUS = doc_status,
                                        EMP_ID = "admin",
                                        TAB_NO = tab_no,
                                        ACTION_STATUS = 1,
                                        CREATED_DATE = DateTime.Now,
                                        UPDATED_DATE = DateTime.Now,
                                    });

                                    sql = @"update BZ_DOC_ACTION set ACTION_STATUS=2, UPDATED_DATE=sysdate where DH_CODE= :doc_id and TAB_NO=1 and ACTION_STATUS=1 ";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                                    // เรียกใช้คำสั่ง SQL ผ่าน FromSqlRaw หรือ ExecuteSqlRaw
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());


                                    #endregion "BZ_DOC_ACTION"

                                    #region "BZ_DOC_TRAVELER_APPROVER"

                                    // ถ้ายังไม่ถึงระดับ line approver
                                    // กรณีนี้จะมีหลาย traverler โดย 1 คน มี Line  2 ท่าน  
                                    //if (old_doc_status < 30 || old_doc_status < 50)
                                    //DevFix 20211116 0000 เครียร์ข้อมูล approver ใหม่ old_doc_status < 50
                                    if (old_doc_status < 30 || old_doc_status < 50)
                                    {
                                        sql = @"DELETE FROM BZ_DOC_TRAVELER_APPROVER WHERE DH_CODE = :doc_id";
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                        //รายละเอียดของ Traverler ของใบงานนั้น กรณีที่เป็นข้อมูลเก่าจากการ revise
                                        var managerEmpId = "";
                                        #region LINE - Endorsed 1
                                        if (value?.summary_table != null && empList != null)
                                        {
                                            string upd_token = Guid.NewGuid().ToString();

                                            var qApprove = from t in value.summary_table
                                                           join e in empList on t.emp_id equals e.EMPLOYEEID
                                                           join e2 in empList on e.MANAGER_EMPID equals e2.EMPLOYEEID
                                                           where t.emp_id == e.EMPLOYEEID && t.dh_code == doc_id
                                                           orderby e2.EMPLOYEEID
                                                           select new
                                                           {
                                                               type = "1",
                                                               appr_empid = e2.EMPLOYEEID,
                                                               appr_level = 1,
                                                               travel_empid = t.emp_id,
                                                               remark = "Endorsed",
                                                               department = e2.DEPARTMENT ?? ""
                                                           };

                                            qApprove = qApprove.Distinct();
                                            var qApproveList = qApprove.ToList();
                                             
                                            if (qApproveList.Count > 0)
                                            {
                                                decimal line = 1;
                                                foreach (var item in qApproveList)
                                                {
                                                    managerEmpId = item.appr_empid;

                                                    context.BZ_DOC_TRAVELER_APPROVER.Add(new BZ_DOC_TRAVELER_APPROVER
                                                    {
                                                        DH_CODE = value.id,
                                                        DTA_ID = line++,
                                                        DTA_TYPE = item.type,
                                                        DTA_APPR_EMPID = item.appr_empid,
                                                        DTA_APPR_LEVEL = item.appr_level,
                                                        DTA_TRAVEL_EMPID = item.travel_empid,
                                                        DTA_REMARK = item.remark,
                                                        DTA_STATUS = 1,
                                                        DTA_UPDATE_TOKEN = upd_token,
                                                    });

                                                    #region LINE - Endorsed 2
                                                    if (managerEmpId != null)
                                                    {
                                                        var qApproveManager = from t in value.summary_table
                                                                              join e in empList on t.emp_id equals e.EMPLOYEEID
                                                                              join e2 in empList on e.MANAGER_EMPID equals e2.EMPLOYEEID
                                                                              where t.emp_id == managerEmpId && t.dh_code == doc_id
                                                                              orderby e2.EMPLOYEEID
                                                                              select new
                                                                              {
                                                                                  type = "1",
                                                                                  appr_empid = e2.EMPLOYEEID,
                                                                                  appr_level = 2,
                                                                                  travel_empid = t.emp_id,
                                                                                  remark = "Endorsed",
                                                                                  department = e2.DEPARTMENT ?? ""
                                                                              };

                                                        qApproveManager = qApproveManager.Distinct();
                                                        var qApproveManagerList = qApproveManager.ToList();
                                                        if (qApproveManagerList.Count > 0)
                                                        {
                                                            foreach (var itemManager in qApproveManagerList)
                                                            {
                                                                context.BZ_DOC_TRAVELER_APPROVER.Add(new BZ_DOC_TRAVELER_APPROVER
                                                                {
                                                                    DH_CODE = value.id,
                                                                    DTA_ID = line++,
                                                                    DTA_TYPE = item.type,
                                                                    DTA_APPR_EMPID = itemManager.appr_empid,
                                                                    DTA_APPR_LEVEL = itemManager.appr_level,
                                                                    DTA_TRAVEL_EMPID = item.travel_empid,
                                                                    DTA_REMARK = item.remark,
                                                                    DTA_STATUS = 1,
                                                                    DTA_UPDATE_TOKEN = upd_token,

                                                                });
                                                            }
                                                        }
                                                    }
                                                    #endregion LINE - Endorsed 2
                                                }
                                            }
                                        }
                                        #endregion LINE - Endorsed 1



                                        if (old_doc_status > 30)
                                        {
                                            //DevFix 20211116 0000 เครียร์ข้อมูล approver ใหม่ 
                                            sql = "delete from BZ_DOC_TRAVELER_APPROVER where DH_CODE= :doc_id ";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());


                                            //DevFix 20211116 0000 เครียร์ข้อมูล action เดิมที่ค้างจาก tab 4 
                                            sql = @"update BZ_DOC_ACTION set ACTION_STATUS=2, UPDATED_DATE=sysdate where DH_CODE= :doc_id and TAB_NO in (3,4) and ACTION_STATUS=1 ";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                            //DevFix 20211116 0000 เครียร์ข้อมูล approver ใหม่ 
                                            sql = "update BZ_DOC_HEAD set DH_DOC_STATUS = '21' where DH_CODE= :doc_id ";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        }

                                    }

                                    #endregion "BZ_DOC_TRAVELER_APPROVER"

                                }
                                else if (value.action.type == "1") // save 
                                {
                                    //sql = "delete from BZ_DOC_ACTION where DH_CODE='" + value.id + "' and DOC_STATUS = 11 and ACTION_STATUS=1 ";
                                    //context.Database.ExecuteSqlCommand(sql);
                                    sql = "delete from BZ_DOC_ACTION where DH_CODE= :doc_id and DOC_STATUS = 11 and ACTION_STATUS=1 ";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                    context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                    {
                                        DA_TOKEN = Guid.NewGuid().ToString(),
                                        DH_CODE = value.id,
                                        DOC_TYPE = value.type,
                                        DOC_STATUS = 11,
                                        EMP_ID = newDocNo == true ? user_id : doc_head_search?.DH_CREATE_BY,
                                        TAB_NO = 1,
                                        ACTION_STATUS = 1,
                                        CREATED_DATE = DateTime.Now,
                                        UPDATED_DATE = DateTime.Now,
                                    });

                                    context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                    {
                                        DA_TOKEN = Guid.NewGuid().ToString(),
                                        DH_CODE = value.id,
                                        DOC_TYPE = value.type,
                                        DOC_STATUS = 11,
                                        EMP_ID = "admin",
                                        TAB_NO = 1,
                                        ACTION_STATUS = 1,
                                        CREATED_DATE = DateTime.Now,
                                        UPDATED_DATE = DateTime.Now,
                                    });
                                }

                                #endregion

                                try
                                {
                                    context.SaveChanges();
                                    transaction.Commit();

                                    data.status = "S";
                                    data.message = "";
                                }
                                catch (Exception e)
                                {
                                    string xmessage = e.Message.ToString();
                                    // แสดง InnerException (ถ้ามี)
                                    if (e.InnerException != null)
                                    {
                                        xmessage += ("Inner Exception: " + e.InnerException.Message);

                                        // ตรวจสอบ InnerException ซ้อน (ถ้ามี)
                                        if (e.InnerException.InnerException != null)
                                        {
                                            xmessage += ("Inner Inner Exception: " + e.InnerException.InnerException.Message);
                                        }
                                    }

                                    data.status = "E";
                                    data.message = xmessage;

                                }

                            }
                            #endregion data

                            #region "#### SEND MAIL ####" 
                            write_log_mail("0-email.start-submitFlow1", "status :" + data.status + " =>type_flow :" + type_flow + " =>value.action.type :" + value.action.type);

                            if (data.status == "S")
                            {
                                //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW 
                                if (type_flow == true)
                                {
                                    // submit
                                    if (value.action.type == "5")
                                    {
                                        //DevFix 20200910 0727 เพิ่มแนบ link Ebiz ด้วย Link ไปหน้า login  
                                        string url_login = LinkLogin;
                                        string sDear = "";
                                        string sDetail = "";
                                        string sTitle = "";
                                        string sBusinessDate = "";
                                        string sLocation = "";
                                        string sTravelerList = "";
                                        string sReasonRejected = "";

                                        try
                                        {
                                            string admin_mail = "";
                                            string requester_mail = "";
                                            string requester_name = "";
                                            string traveler_mail = "";

                                            if (adminList != null)
                                            {
                                                foreach (var item in adminList)
                                                {
                                                    admin_mail += ";" + item.email ?? "";
                                                }
                                                if (admin_mail != "")
                                                    admin_mail = ";" + admin_mail.Substring(1);
                                            }

                                            //กรณีที่เป็น pmdv admin, pmsv_admin
                                            admin_mail += mail_group_admin(context, "pmsv_admin");
                                            if (doc_id.IndexOf("T") > -1)
                                            {
                                                admin_mail += mail_group_admin(context, "pmdv_admin");
                                            }

                                            //DevFix 20210813 0000 หลังจาก Requester กด Submit แล้ว E-mail วิ่งไปหา Initiator แต่ไม่ CC: Requester & Traveler  
                                            sql = @" SELECT distinct EMPLOYEEID as user_id,  nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email
                                                    FROM BZ_USERS b 
                                                    INNER JOIN BZ_DOC_HEAD h on b.EMPLOYEEID = h.DH_CREATE_BY
                                                    WHERE DH_CODE = :doc_id";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var requesterList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                            if (requesterList != null)
                                            {
                                                if (requesterList.Count > 0)
                                                {
                                                    requester_mail = ";" + requesterList[0].email;
                                                    requester_name = requesterList[0].user_name ?? "";
                                                }
                                            }

                                            //DevFix 20210813 0000 เพิ่ม email เพื่อนำไปใช้ตอน cc 
                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                                                     , b.employeeid as name3, b.orgname as name4
                                                     from BZ_DOC_TRAVELER_EXPENSE a left join bz_users b on a.DTE_EMP_ID = b.employeeid 
                                                     left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                                     on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id 
                                                     where a.dh_code = :doc_id and nvl(a.dte_status,0) <> 0  order by s.id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                            if (tempTravel != null)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    traveler_mail += ";" + item.name2;
                                                }
                                            }

                                            sendEmailService mail = new sendEmailService();
                                            sendEmailModel dataMail = new sendEmailModel();
                                            if (doc_status == 21) // admin
                                            {
                                                #region DevFix 20200916 2219 เพิ่ม cc initiator  
                                                var initial_mail = "";
                                                try
                                                {
                                                    sql = "SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email ";
                                                    sql += "FROM bz_users WHERE EMPLOYEEID = :initiator_emp_id_select ";
                                                    var initiator_emp_id_select = value.initiator.emp_id ?? "";
                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter("initiator_emp_id_select", initiator_emp_id_select, "char"));
                                                    var initial = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                    if (initial != null && initial.Count() > 0)
                                                    {
                                                        initial_mail = ";" + initial[0].email;
                                                    }
                                                }
                                                catch (Exception ex) { }
                                                #endregion DevFix 20200916 2219 เพิ่ม cc initiator 

                                                //TO: Admin(PMSV)
                                                //CC: Requester; Initiator
                                                dataMail.mail_to = admin_mail;
                                                dataMail.mail_cc = requester_mail + on_behalf_of_mail + initial_mail + traveler_mail;

                                                //Subj :   OB / LBYYMMXXXX : Please submit an estimate of business travel expenses
                                                dataMail.mail_subject = value.id + " : Please submit an estimate of business travel expenses.";

                                                sDear = "Dear Business Travel Services Team,";

                                                sDetail = "Please submit an estimate of business travel expenses. To view the details, click ";
                                                sDetail += "<a href='" + (LinkLogin + "i").Replace("###", value.id) + "'>" + value.id + "</a>";
                                            }
                                            else
                                            {
                                                var user_initiator_emp_id = value.initiator.emp_id ?? "";
                                                var user_initiator_display = "";
                                                sql = @" SELECT  EMPLOYEEID user_id, nvl(ENTITLE, '') || ' ' || ENFIRSTNAME || ' ' || ENLASTNAME as user_name, EMAIL email ";
                                                sql += " FROM bz_users WHERE EMPLOYEEID = :user_initiator_emp_id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("user_initiator_emp_id", user_initiator_emp_id, "char"));
                                                var initial = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (initial != null && initial.Count() > 0)
                                                {
                                                    dataMail.mail_to = initial[0].email ?? "";
                                                    user_initiator_display = initial[0].user_name ?? "";
                                                }

                                                //DevFix 20210813 0000 หลังจาก Requester กด Submit แล้ว E-mail วิ่งไปหา Initiator 
                                                dataMail.mail_cc = admin_mail + requester_mail + on_behalf_of_mail + traveler_mail;
                                                // OB/LBYYMMXXXX : Please initiate a requested for business travel.
                                                dataMail.mail_subject = value.id + " : Please initiate a requested for business travel.";

                                                sDear = "Dear " + user_initiator_display + ",";

                                                sDetail = "Please initiate a requested for business travel. To view the details, click ";
                                                sDetail += "<a href='" + (LinkLogin + "").Replace("###", value.id) + "'>" + value.id + "</a>";

                                            }

                                            //Title: [Name of Title]
                                            //Business Date : [Date from to]
                                            //Location : [OB: Country, City, Location], [LB: Province, Location] 
                                            sTitle = "Title : " + value.topic_of_travel ?? "";
                                            sBusinessDate = "Date : " + dateFromTo(value.business_date.start, value.business_date.stop) ?? "";
                                            if (value.type == "local" || value.type == "localtraining")
                                            {
                                                //DevFix 20210330 1502 แก้ไข Location   
                                                sql = @" select distinct to_char(s.id) as id, e.PV_NAME as name1, a.CITY_TEXT as name2, '' as name3, '' as name4   
                                                         from BZ_DOC_TRAVELER_EXPENSE a 
                                                         left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                         ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                         and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                         left join bz_users b on a.DTE_EMP_ID = b.employeeid  
                                                         left join BZ_MASTER_CONTINENT c on a.CTN_ID = c.CTN_ID   
                                                         left join BZ_MASTER_PROVINCE e on a.PV_ID = e.PV_ID 
                                                         where a.DH_CODE = :doc_id and a.dte_status = 1 order by s.id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (temp != null && temp.Count() > 0)
                                                {
                                                    //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                    //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //DevFix 20210819 0000  แก้ไขเทส case เลือกไป 2 ประเทศ แต่ในอีเมล์แสดงแค่ประเทศเดียว ?ต้องแสดงทั้งหมด   
                                                sql = @" select distinct to_char(s.id) as id, b.ct_name name1, c.ctn_name name2, '' as name3, '' as name4 
                                                         from BZ_DOC_COUNTRY a 
                                                         left join (select min(dte_id) as id, dh_code, ct_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ct_id) s on a.dh_code = s.dh_code and a.ct_id = s.ct_id  
                                                         left join BZ_MASTER_COUNTRY b on a.ct_id = b.ct_id
                                                         left join bz_master_continent c on b.ctn_id = c.ctn_id 
                                                         where a.dh_code = :doc_id order by s.id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (temp != null && temp.Count() > 0)
                                                {
                                                    //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }
                                            }

                                            var iNo = 1;
                                            sTravelerList = "<table>";
                                            if (tempTravel != null && tempTravel.Count() > 0)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    sTravelerList += " <tr>";
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                    sTravelerList += " </tr>";
                                                    iNo++;
                                                }
                                            }
                                            else
                                            {
                                                sTravelerList += " <tr><td></td></tr>";
                                            }
                                            sTravelerList += "</table>";

                                            #region set mail
                                            dataMail.mail_body = @"<span lang='en-US'>";
                                            dataMail.mail_body += "<div>";
                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            //if (sReasonRejected != "")
                                            //{
                                            //    dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            //    dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                            //    dataMail.mail_body += "     <br>";
                                            //} 
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                            dataMail.mail_body += "     </span></font></div>";

                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     Best Regards,";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                            dataMail.mail_body += "     </span></font></div>";

                                            dataMail.mail_body += "</div>";
                                            dataMail.mail_body += "</span>";

                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID == doc_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;

                                            mail.sendMail(dataMail);
                                            #endregion set mail
                                        }
                                        catch (Exception ex)
                                        {

                                            data.status = "E";
                                            data.message = "SEND MAIL " + ex.ToString();

                                            write_log_mail("88-email.message-submitFlow1", "error" + ex.ToString());

                                        }

                                    }

                                }
                            }

                            write_log_mail("99-email.end-submitFlow1", "");
                            #endregion "#### SEND MAIL ####" 


                        }
                        catch (Exception ex)
                        {
                            LoggerFile.write(ex);
                            data.status = "E";
                            data.message = ex.ToString();
                        }

                    }

                }
                if (data.status == "S")
                {
                    if (newDocNo)
                    {
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW 
                                if (value.action.type == "1" || value.action.type == "5")
                                {
                                    //doc_head_search.DH_TYPE_FLOW = value.type_flow ?? ""; 
                                    var stype_flow = value.type_flow ?? "";
                                    //var sqlstr_update = "UPDATE BZ_DOC_HEAD SET DH_TYPE_FLOW = " + stype_flow + " WHERE DH_CODE = '" + sdh_code + "'";

                                    var sql = "UPDATE BZ_DOC_HEAD SET DH_TYPE_FLOW = :stype_flow WHERE DH_CODE = :doc_id ";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("stype_flow", stype_flow, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    var iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1)
                                    {
                                        context.SaveChanges();
                                        transaction.Commit();
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerFile.write(ex);
                data.status = "E";
                data.message = ex.Message;

            }


            return data;
        }

        public ResultModel submitFlow2(DocFlow2Model value)
        {
            //int iResult = -1;
            //Decimal? decimalNull = null;
            //Boolean newDocNo = false;
            //Decimal? doc_status = 1;
            decimal? old_action_status = 21;
            decimal? next_action_status = 21;
            string prefix_old_doctype = "";
            decimal? next_topno = 3;

            decimal? expense_status = 21; // Pending for Super Admin
            string sql = "";
            var data = new ResultModel();
            var tempEmpApprover = new List<BZ_DOC_ACTION>();

            if (value.action == null || string.IsNullOrEmpty(value.action.type))
            {
                data.status = "E";
                data.message = "Action is null !";
                return data;
            }

            //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
            bool type_flow = true;

            try
            {
                string expen_upd_token = Guid.NewGuid().ToString();
                var expenList = new List<docFlow2_travel>();
                var approveList = new List<docFlow2_approve>();
                var approveList_Def = new List<docFlow2_approve>();

                var doc_id = value.doc_id ?? "";
                var token_login = value.token_login ?? "";
                var parameters = new List<OracleParameter>();

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {

                    var doc_head_search = context.BZ_DOC_HEAD.Find(value.doc_id);
                    if (doc_head_search == null)
                    {
                        data.status = "E";
                        data.message = "not found data !";
                        return data;
                    }

                    #region DevFix 20221108 0000 เนื่องจากเจอเคสที่ข้อมูล token ของ item traveler ข้อมูลไม่ตรงกับในตาราง จึงเช็คเพิ่มเติม 
                    if (true)
                    {
                        var expenList_check = new List<docFlow2_travel>();
                        if ((doc_head_search.DH_TYPE ?? "") == "local" ||
                            (doc_head_search.DH_TYPE ?? "") == "localtraining"
                            )
                        {
                            expenList_check = value.local.traveler;
                        }
                        else
                        {
                            expenList_check = value.oversea.traveler;
                        }
                        //int irow_test = 0;
                        foreach (var item in expenList_check)
                        {
                            var dte_token = item.ref_id;
                            //sql = @" select distinct DTE_TOKEN as ref_id FROM bz_doc_traveler_expense a WHERE a.dh_code in ('" + value.doc_id + "') and DTE_TOKEN = '" + _DTE_TOKEN + "' order by DTE_TOKEN ";
                            //var temp_token_expen = context.Database.SqlQuery<docFlow2_travel>(sql).ToList();

                            sql = @" select distinct DTE_TOKEN as id_key FROM bz_doc_traveler_expense a WHERE a.dh_code = :doc_id and DTE_TOKEN = :dte_token order by DTE_TOKEN ";
                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            parameters.Add(context.ConvertTypeParameter("dte_token", dte_token, "char"));
                            var temp_token_expen = context.TempIdKeyModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                            if (temp_token_expen != null && temp_token_expen.Count() > 0)
                            { }
                            else
                            {

                                //หา id log ล่าสุดที่ส่งมา เพื่อให้ support check 
                                //sql = @" select to_char(id) as ref_id, data_log as remark from  BZ_TRANS_LOG where data_log like '%" + value.doc_id + "%' and event  = 'FLOW2' and module = 'DOCUMENT' order by to_number(id) desc";
                                //var temp_trans_log = context.Database.SqlQuery<docFlow2_travel>(sql).ToList();
                                sql = @" select to_char(id) as id, data_log as name1, '' as name2, '' as name3, '' as name4 from  BZ_TRANS_LOG where data_log like '%' || :doc_id || '%'  and event  = 'FLOW2' and module = 'DOCUMENT' order by to_number(id) desc";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                var temp_trans_log = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                var trans_log_id = "";
                                var trans_log_data_log = "";
                                try { trans_log_id = temp_trans_log[0].id.ToString(); } catch { }
                                try { trans_log_data_log = temp_trans_log[0].name1.ToString(); } catch { }

                                data.status = "E";
                                data.message = "error data traveler list!, tran_log no:" + trans_log_id;// + " =>data log:" + trans_log_data_log;
                                return data;
                            }

                        }
                    }
                    #endregion DevFix 20221108 0000 เนื่องจากเจอเคสที่ข้อมูล token ของ item traveler ข้อมูลไม่ตรงกับในตาราง จึงเช็คเพิ่มเติม


                    //var docHeadStatus = new List<DocHeadModel>();
                    //sql = " select to_char(dh_doc_status) as document_status from bz_doc_head h where h.dh_code = '" + value.doc_id + "' ";
                    //docHeadStatus = context.Database.SqlQuery<DocHeadModel>(sql).ToList();
                    sql = " select to_char(dh_doc_status) as id, '' as name1, '' as name2, '' as name3, '' as name4 from bz_doc_head h where h.dh_code = :doc_id ";
                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var docHeadStatus = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                    var pf_doc_id = docHeadStatus[0].id.Substring(0, 1);

                    string admin_mail = "";
                    string requester_mail = "";
                    string requester_name = "";
                    string on_behalf_of_mail = "";
                    string traveler_mail = "";
                    string approver_mail = "";

                    #region DevFix 20200911 0000 
                    var Tel_Services_Team = "";
                    var Tel_Call_Center = "";
                    ////sql = @" SELECT key_value as tel_services_team from bz_config_data where lower(key_name) = lower('tel_services_team') and status = 1";
                    ////var tellist = context.Database.SqlQuery<TelephoneModel>(sql).ToList();
                    //sql = @" SELECT key_value as tel_services_team from bz_config_data where lower(key_name) = lower('tel_services_team') and status = 1";
                    //parameters = new List<OracleParameter>();
                    //var tellist = context.TelephoneModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    //try { Tel_Services_Team = tellist[0].tel_services_team; } catch { }

                    //// sql = @" SELECT key_value as tel_call_center 
                    ////              from bz_config_data where lower(key_name) = lower('tel_call_center') and status = 1";
                    ////var  telcalllist = context.Database.SqlQuery<TelephoneModel>(sql).ToList();
                    //sql = @" SELECT key_value as tel_call_center 
                    //             from bz_config_data where lower(key_name) = lower('tel_call_center') and status = 1";
                    //parameters = new List<OracleParameter>();
                    //var telcalllist = context.TelephoneModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    //try { Tel_Call_Center = telcalllist[0].tel_call_center; } catch { }


                    getTelServicesTeamCallCenter(ref Tel_Services_Team, ref Tel_Call_Center);

                    #endregion DevFix 20200911 0000 


                    #region DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin 
                    sql = "SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email FROM bz_users WHERE role_id = :role_id ";
                    var role_id = "1";
                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("role_id", role_id, "char"));
                    var adminList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    if (adminList != null)
                    {
                        foreach (var item in adminList)
                        {
                            admin_mail += ";" + item.email ?? "";
                        }
                        if (admin_mail != "")
                            admin_mail = ";" + admin_mail.Substring(1);
                    }

                    //กรณีที่เป็น pmdv admin, pmsv_admin
                    admin_mail += mail_group_admin(context, "pmsv_admin");
                    if (value.doc_id.IndexOf("T") > -1)
                    {
                        admin_mail += mail_group_admin(context, "pmdv_admin");
                    }

                    sql = @" SELECT EMPLOYEEID as user_id,  nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email FROM BZ_USERS b
                                 WHERE EMPLOYEEID IN ( SELECT DH_CREATE_BY FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var requesterList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    if (requesterList != null)
                    {
                        if (requesterList.Count > 0)
                        {
                            requester_mail = ";" + requesterList[0].email;
                            requester_name = requesterList[0].user_name;
                        }
                    }


                    sql = @" SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email FROM BZ_USERS 
                                 WHERE EMPLOYEEID IN ( SELECT DH_BEHALF_EMP_ID FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var behalfList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    if (behalfList != null)
                    {
                        if (behalfList.Count > 0)
                        {
                            on_behalf_of_mail = ";" + behalfList[0].email;
                        }
                    }

                    sql = @"SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email FROM bz_users
                                     WHERE EMPLOYEEID in (select dh_initiator_empid from bz_doc_head where dh_code = :doc_id)  ";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var initial = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    if (initial != null && initial.Count() > 0)
                    {
                        on_behalf_of_mail += ";" + initial[0].email;
                    }
                    #endregion DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin 


                    //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW  
                    sql = @"SELECT a.DH_TYPE_FLOW as id, '' as name1, '' as name2, '' as name3, '' as name4 FROM BZ_DOC_HEAD a where a.DH_CODE = :doc_id";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var docHead = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    if (docHead != null)
                    {
                        if ((docHead[0].id ?? "1") != "1") { type_flow = false; }
                    }

                    #region DevFix 20200827 ตรวจสอบ approver list ต้องมี Endorsed และ CAP ของแต่ละ Requester
                    if (value.action.type == "5") // submit
                    {
                        //var employeeList = new List<employeeDoc2>();
                        #region employee

                        //sql = " SELECT DTE_TOKEN ref_id, DTE_EMP_ID id, U.Employeeid ";
                        //sql += " , nvl(U.ENTITLE, '') || ' ' || U.ENFIRSTNAME || ' ' || U.ENLASTNAME || case when h.DH_TRAVEL ='1' then '' else ' | ' || case when h.DH_TYPE ='local' then p.pv_name else c.ct_name end end name ";
                        //sql += " , nvl(U.ENTITLE, '') || ' ' || U.ENFIRSTNAME || ' ' || U.ENLASTNAME  name2 ";
                        //sql += " , U.ORGNAME org, DTE_TRAVEL_DAYS ";
                        //sql += " , case when tv.DTE_BUS_FROMDATE is null then '' else to_char(tv.DTE_BUS_FROMDATE, 'dd Mon rrrr') || ' - ' || to_char(tv.DTE_BUS_TODATE, 'dd Mon rrrr') end as business_date ";
                        //sql += " , case when DTE_TRAVEL_FROMDATE is null then '' else to_char(DTE_TRAVEL_FROMDATE, 'dd Mon rrrr') || ' - ' || to_char(DTE_TRAVEL_TODATE, 'dd Mon rrrr') end as travel_date ";
                        //sql += " , to_char('') visa_fee, '' passport_expense, '' clothing_expense ";
                        //sql += " , to_char(c.ct_id) country_id, c.ct_name country ";
                        //sql += " , p.pv_name province ";
                        //sql += " , tv.dte_traveler_remark remark ";

                        //sql += " FROM bz_doc_traveler_expense tv inner join BZ_DOC_HEAD h on h.dh_code=tv.dh_code ";
                        //sql += " inner join BZ_USERS U on tv.DTE_Emp_Id = u.employeeid ";
                        //sql += " left join bz_master_country c on tv.ct_id = c.ct_id ";
                        //sql += " left join BZ_MASTER_PROVINCE p on tv.PV_ID = p.PV_ID ";
                        //sql += " WHERE tv.dh_code = '" + value.doc_id + "' and tv.dte_status = 1 ";
                        //sql += " order by DTE_ID ";

                        //employeeList = context.Database.SqlQuery<employeeDoc2>(sql).ToList();

                        sql = " SELECT DTE_TOKEN ref_id, DTE_EMP_ID id, U.Employeeid ";
                        sql += " , nvl(U.ENTITLE, '') || ' ' || U.ENFIRSTNAME || ' ' || U.ENLASTNAME || case when h.DH_TRAVEL ='1' then '' else ' | ' || case when h.DH_TYPE ='local' then p.pv_name else c.ct_name end end name ";
                        sql += " , nvl(U.ENTITLE, '') || ' ' || U.ENFIRSTNAME || ' ' || U.ENLASTNAME  name2 ";
                        sql += " , U.ORGNAME org, DTE_TRAVEL_DAYS ";
                        sql += " , case when tv.DTE_BUS_FROMDATE is null then '' else to_char(tv.DTE_BUS_FROMDATE, 'dd Mon rrrr') || ' - ' || to_char(tv.DTE_BUS_TODATE, 'dd Mon rrrr') end as business_date ";
                        sql += " , case when DTE_TRAVEL_FROMDATE is null then '' else to_char(DTE_TRAVEL_FROMDATE, 'dd Mon rrrr') || ' - ' || to_char(DTE_TRAVEL_TODATE, 'dd Mon rrrr') end as travel_date ";
                        sql += " , to_char('') visa_fee, '' passport_expense, '' clothing_expense ";
                        sql += " , to_char(c.ct_id) country_id, c.ct_name country ";
                        sql += " , p.pv_name province ";
                        sql += " , tv.dte_traveler_remark remark ";
                        sql += " , '' as city ";

                        sql += " FROM bz_doc_traveler_expense tv inner join BZ_DOC_HEAD h on h.dh_code=tv.dh_code ";
                        sql += " inner join BZ_USERS U on tv.DTE_Emp_Id = u.employeeid ";
                        sql += " left join bz_master_country c on tv.ct_id = c.ct_id ";
                        sql += " left join BZ_MASTER_PROVINCE p on tv.PV_ID = p.PV_ID ";
                        sql += " WHERE tv.dh_code = :doc_id and tv.dte_status = 1 ";
                        sql += " order by DTE_ID ";

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                        var employeeList = context.EmployeeDoc2SubmitModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                        #endregion employee

                        if ((doc_head_search.DH_TYPE ?? "") == "local" ||
                            (doc_head_search.DH_TYPE ?? "") == "localtraining"
                            )
                        {
                            expenList = value.local.traveler;
                            approveList = value.local.approver;
                        }
                        else
                        {
                            expenList = value.oversea.traveler;
                            approveList = value.oversea.approver;
                        }
                        if (employeeList != null)
                        {
                            foreach (var iEmp in employeeList)
                            {
                                var emp_name = iEmp.name2.ToString();
                                var bCheckDataReq = false;
                                string msg_alert = "";
                                //ตรวจสอบว่ามีค่า Traveler List Summary ครบตามจำนวนคน request หรือไม่
                                if (expenList != null)
                                {
                                    var ilist = expenList.FindAll(x => x.emp_id == iEmp.id.ToString());
                                    if (ilist.Count > 0)
                                    {
                                        bCheckDataReq = true;
                                    }
                                    msg_alert = "Traveler data " + emp_name + " is not incomplete !";
                                }
                                else
                                {
                                    msg_alert = "Traveler data is not incomplete !";
                                }
                                if (bCheckDataReq == false)
                                {
                                    data.status = "E";
                                    //data.message = "Traveler data is not incomplete !";
                                    //data.message = "Traveler data " + emp_name + " is not incomplete !";
                                    data.message = msg_alert;
                                    return data;
                                }
                                //ตรวจสอบว่ามีค่า Approver List ครบตามจำนวนคน request หรือไม่
                                bCheckDataReq = false;
                                msg_alert = "";
                                if (approveList != null)
                                {
                                    if (approveList.Count > 0)
                                    {
                                        var ilist = approveList.FindAll(x => x.emp_id == iEmp.id.ToString());
                                        if (ilist.Count > 0)
                                        {
                                            var ilEndorsed = approveList.FindAll(x => x.emp_id == iEmp.id.ToString() && x.type == "1");//Endorsed
                                            var ilCAP = approveList.FindAll(x => x.emp_id == iEmp.id.ToString() && x.type == "2");//CAP
                                            if (ilEndorsed.Count > 0 && ilCAP.Count > 0) { bCheckDataReq = true; }

                                            if (ilEndorsed.Count == 0) { msg_alert = "ผู้อนุมัติสำหรับ Endorsed ไม่ครบ"; bCheckDataReq = false; }
                                            if (ilCAP.Count == 0)
                                            {
                                                if (msg_alert != "") { msg_alert += " และ "; }
                                                msg_alert += "ผู้อนุมัติสำหรับ CAP ไม่ครบ."; bCheckDataReq = false;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        msg_alert = "ไม่มีผู้อนุมัติสำหรับ CAP.";
                                    }
                                }
                                else
                                {
                                    msg_alert = "ไม่มีผู้อนุมัติสำหรับ CAP.";
                                }
                                if (bCheckDataReq == false)
                                {
                                    data.status = "E";
                                    //data.message = "Approver data is not incomplete !";
                                    data.message = msg_alert;
                                    return data;
                                }

                            }
                        }
                    }
                    #endregion DevFix 20200827 ตรวจสอบ approver list ต้องมี Endorsed และ CAP ของแต่ละ Requester

                    #region DevFix 20200827 ตรวจสอบ position approver  
                    var query = "";
                    //query = @"SELECT A.SH,A.VP,A.AEP,A.EVP,A.SEVP,A.CEO,B.DTE_EMP_ID AS ORG_ID
                    //                 FROM BZ_MASTER_COSTCENTER_ORG a
                    //                 INNER JOIN BZ_DOC_TRAVELER_EXPENSE B ON  A.COST_CENTER = B.DTE_COST_CENTER
                    //                 WHERE B.DH_CODE = '" + value.doc_id + "' ";
                    //var masterCostCenterList1 = context.Database.SqlQuery<MasterCostCenter>(query).ToList();
                    query = @"SELECT '' as COST_CENTER, B.DTE_EMP_ID as ORG_ID, '' as OTYPE, '' as COM_CODE, A.SH,A.VP,A.AEP,A.EVP,A.SEVP,A.CEO
                                     FROM BZ_MASTER_COSTCENTER_ORG a
                                     INNER JOIN BZ_DOC_TRAVELER_EXPENSE B ON  A.COST_CENTER = B.DTE_COST_CENTER
                                     WHERE B.DH_CODE = :doc_id ";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var masterCostCenterList1 = context.MasterCostCenterList.FromSqlRaw(query, parameters.ToArray()).ToList();


                    //กรณีที่ตรวจสอบระดับ SEVP แต่ไม่มี Cost center ให้ไปหา CEO
                    //query = "select null as SEVP, employeeid as CEO from  bz_users a  where  POSCAT = 'MD' and department is null and sections is null ";
                    //var masterCostCenterList2 = context.Database.SqlQuery<MasterCostCenter>(query).ToList();


                    query = "select '' as COST_CENTER, '' as ORG_ID, '' as OTYPE, '' as COM_CODE, '' as SH,'' as VP,'' as AEP,'' as EVP,  null as SEVP, employeeid as CEO from  bz_users a  where  POSCAT = 'MD' and department is null and sections is null ";

                    parameters = new List<OracleParameter>();
                    //parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var masterCostCenterList2 = context.MasterCostCenterList.FromSqlRaw(query, parameters.ToArray()).ToList();


                    #endregion DevFix 20200827 ตรวจสอบ position approver  


                    #region DevFix 20210810 เพิ่มรายชื่อ userเพื่อใช้ในเงื่อนไขลำดับแค่ CAP ของ Local  
                    //query = "SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGNAME, MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO";
                    //query += " FROM BZ_USERS";
                    //var usersList = context.Database.SqlQuery<TravelerUsers>(query).ToList();
                    query = "SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGNAME, MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO FROM BZ_USERS";

                    parameters = new List<OracleParameter>();
                    var usersList = context.TravelerUsersCAPModelList.FromSqlRaw(query, parameters.ToArray()).ToList();
                    #endregion DevFix 20210810 เพิ่มรายชื่อ userเพื่อใช้ในเงื่อนไขลำดับแค่ CAP ของ Local  

                    using (var transaction = context.Database.BeginTransaction())
                    {
                        sql = "SELECT   a.user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, b.email email ";
                        sql += " FROM bz_login_token a left join bz_users b on a.user_id=b.employeeid ";
                        sql += " WHERE a.TOKEN_CODE = :token_login ";

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("token_login", token_login, "char"));
                        var User = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                        if ((doc_head_search.DH_TYPE ?? "") == "local" ||
                            (doc_head_search.DH_TYPE ?? "") == "localtraining")
                        {
                            expenList = value.local.traveler;
                            approveList = value.local.approver;
                        }
                        else
                        {
                            expenList = value.oversea.traveler;
                            approveList = value.oversea.approver;
                        }

                        try
                        {
                            #region "#### BZ_DOC_HEAD ####"

                            if ((doc_head_search.DH_TYPE ?? "") == "local" ||
                                (doc_head_search.DH_TYPE ?? "") == "localtraining")
                            {
                                doc_head_search.DH_EXPENSE_OPT1 = retCheckValue(value.local.checkbox_1.ToString() ?? "");
                                doc_head_search.DH_EXPENSE_OPT2 = retCheckValue(value.local.checkbox_2.ToString() ?? "");
                                doc_head_search.DH_EXPENSE_REMARK = value.local.remark ?? "";
                            }
                            else
                            {
                                doc_head_search.DH_EXPENSE_OPT1 = retCheckValue(value.oversea.checkbox_1.ToString() ?? "");
                                doc_head_search.DH_EXPENSE_OPT2 = retCheckValue(value.oversea.checkbox_2.ToString() ?? "");
                                doc_head_search.DH_EXPENSE_REMARK = value.oversea.remark ?? "";
                            }
                            doc_head_search.DH_UPDATE_BY = User[0].user_id == null ? "" : User[0].user_id;
                            doc_head_search.DH_UPDATE_DATE = DateTime.Now;

                            old_action_status = doc_head_search.DH_DOC_STATUS;
                            var _old_action_status = old_action_status?.ToString() ?? "";
                            if (_old_action_status?.ToString().Length > 1)
                            {
                                prefix_old_doctype = _old_action_status?.Substring(0, 1)?.ToString() ?? "";
                            }
                            if (prefix_old_doctype == "2" || prefix_old_doctype == "3")
                            {
                                next_topno = 3;
                                next_action_status = 31; // Pending by Line Approver
                            }
                            else
                            {
                                next_topno = 4;
                                next_action_status = 41; // Pending by Line Approver 
                            }


                            #endregion;

                            #region  "#### BZ_DOC_ACTION, BZ_DOC_ACTION_TRAVELER ####"

                            if (value.action.type == "2") // reject
                            {
                                expense_status = 20;
                                doc_head_search.DH_DOC_STATUS = expense_status;
                                doc_head_search.DH_REMARK_REJECT = value.action.remark ?? "";
                                //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = '" + value.doc_id + "'");
                                sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = :doc_id ";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                            }
                            else if (value.action.type == "3") // revise
                            {
                                //int revise_doc_status = 11;
                                expense_status = 11;
                                doc_head_search.DH_REMARK_REJECT = value.action.remark ?? "";
                                if (doc_head_search.DH_DOC_STATUS.ToString().Substring(0, 1) == "2")
                                {
                                    if (string.IsNullOrEmpty(doc_head_search.DH_INITIATOR_EMPID))
                                        expense_status = 11;
                                    else
                                        expense_status = 22;

                                    doc_head_search.DH_DOC_STATUS = expense_status;
                                    //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = '" + value.doc_id + "'");
                                    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = :doc_id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                else
                                {
                                    // tab3, tab4
                                    // ดู record action ที่ doc_status ขึ้นต้นด้วย 2
                                    //sql = "update BZ_DOC_ACTION  set action_status = 2  where DH_CODE='" + value.doc_id + "' and (DOC_STATUS >= 21 and DOC_STATUS <=29) and ACTION_STATUS=1 ";
                                    //context.Database.ExecuteSqlCommand(sql);
                                    sql = "update BZ_DOC_ACTION  set action_status = 2  where DH_CODE= :doc_id and (DOC_STATUS >= 21 and DOC_STATUS <=29) and ACTION_STATUS=1 ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }

                                if (!string.IsNullOrEmpty(doc_head_search.DH_INITIATOR_EMPID))
                                {
                                    context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                    {
                                        DA_TOKEN = Guid.NewGuid().ToString(),
                                        DH_CODE = value.doc_id,
                                        DOC_TYPE = value.type,
                                        DOC_STATUS = expense_status,
                                        EMP_ID = doc_head_search.DH_INITIATOR_EMPID ?? "",
                                        TAB_NO = 1,
                                        ACTION_STATUS = 1,
                                        CREATED_DATE = DateTime.Now,
                                        UPDATED_DATE = DateTime.Now,
                                        REMARK = value.action.remark ?? ""
                                    });
                                }
                                else
                                {
                                    context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                    {
                                        DA_TOKEN = Guid.NewGuid().ToString(),
                                        DH_CODE = value.doc_id,
                                        DOC_TYPE = value.type,
                                        DOC_STATUS = expense_status,
                                        EMP_ID = doc_head_search.DH_CREATE_BY ?? "",
                                        TAB_NO = 1,
                                        ACTION_STATUS = 1,
                                        CREATED_DATE = DateTime.Now,
                                        UPDATED_DATE = DateTime.Now,
                                        REMARK = value.action.remark ?? ""
                                    });
                                }


                                context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                {
                                    DA_TOKEN = Guid.NewGuid().ToString(),
                                    DH_CODE = value.doc_id,
                                    DOC_TYPE = value.type,
                                    DOC_STATUS = expense_status,
                                    EMP_ID = "admin",
                                    TAB_NO = 1,
                                    ACTION_STATUS = 1,
                                    CREATED_DATE = DateTime.Now,
                                    UPDATED_DATE = DateTime.Now,
                                    REMARK = value.action.remark ?? ""
                                });


                            }
                            else if (value.action.type == "5") // submit
                            {
                                expense_status = 31;
                                // 21 : pending for admin
                                // 22 : pending for initialtor
                                // ถ้ายังไม่เคยส่งไป tab 3
                                if (doc_head_search.DH_DOC_STATUS >= 21 && doc_head_search.DH_DOC_STATUS <= 22)
                                {
                                    doc_head_search.DH_DOC_STATUS = 31; //Pending for Line Approver
                                }
                                else
                                {
                                    // เคยส่งไป tab 3 แล้ว  
                                }

                            } // end button submit

                            #endregion

                            #region "#### BZ_DOC_TRAVELER_EXPENSE, BZ_DOC_TRAVELER_EXPENSE_TEMP ####"

                            var expenTemp = new List<docFlow2_travel>();
                            var tempEmpForAction = new List<BZ_DOC_ACTION>();

                            //sql = @" select *
                            //         from BZ_DOC_TRAVELER_APPROVER a
                            //         where a.dh_code = '" + value.doc_id + "' and a.DTA_STATUS = 1";
                            //var travelApproveTemp = context.Database.SqlQuery<BZ_DOC_TRAVELER_APPROVER_V2>(sql).ToList();
                            sql = @" select * from BZ_DOC_TRAVELER_APPROVER a where a.dh_code = :doc_id and a.DTA_STATUS = 1";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            var travelApproveTemp = context.TravelApproveList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                            decimal inx = 0;
                            foreach (var item in expenList)
                            {
                                inx++;

                                #region "#### BZ_DOC_TRAVELER_EXPENSE ####"

                                //sql = " update BZ_DOC_TRAVELER_EXPENSE set ";
                                //sql += " DTE_TOKEN_UPD = '" + expen_upd_token + "' ";
                                //sql += " , DTE_ID= " + inx.ToString(); 
                                //sql += " , DTE_AIR_TECKET = '" + chkString(item.air_ticket) + "'";
                                //sql += " , DTE_ACCOMMODATIC = '" + chkString(item.accommodation) + "'";
                                //sql += " , DTE_ALLOWANCE = '" + chkString(item.allowance) + "' ";
                                //sql += " , DTE_ALLOWANCE_DAY = " + retDecimalSQL(item.allowance_day);
                                //sql += " , DTE_ALLOWANCE_NIGHT = " + retDecimalSQL(item.allowance_night); 
                                //sql += " , DTE_CL_EXPENSE = '" + chkString(item.clothing_expense) + "' "; 
                                //sql += " , DTE_PASSPORT_EXPENSE = '" + chkString(item.passport_expense) + "' ";
                                //sql += " , DTE_VISA_FREE = '" + chkString(item.visa_fee) + "' ";
                                //sql += " , DTE_TRAVEL_INS = '" + chkString(item.travel_insurance) + "' ";
                                //sql += " , DTE_TRANSPORT = '" + chkString(item.transportation) + "'";
                                //sql += " , DTE_MISCELLANEOUS = '" + chkString(item.miscellaneous) + "' ";
                                //sql += " , DTE_TOTAL_EXPENSE = " + retDecimalSQL(item.total_expenses);
                                //sql += " , DTE_REGIS_FREE = '" + chkString(item.registration_fee) + "' ";
                                //sql += " , DTE_CL_VALID = " + chkDateSQL_All(item.clothing_valid ?? "");
                                //sql += " , DTE_PASSPORT_VALID = " + chkDateSQL_All(item.passport_valid ?? "");

                                //if (value.action.type == "5")
                                //{
                                //    if (prefix_old_doctype == "3")
                                //    {
                                //        //DevFix 20200901 2357 เพิ่มเงื่อนไขให้ update เฉพาะรายการที่ยังไม่ถูก approve โดย Line ,CAP                                             sql += " , DTE_APPR_STATUS = case when DTE_APPR_STATUS = 23 then '31' else DTE_APPR_STATUS end  ";
                                //        sql += ", DTE_APPR_STATUS =case when DTE_APPR_STATUS = 23 then 31 else DTE_APPR_STATUS end  ";
                                //        sql += ", DTE_APPR_OPT=case when DTE_APPR_STATUS = 23 then '' else DTE_APPR_OPT end ";
                                //        sql += ", DTE_APPR_REMARK=case when DTE_APPR_STATUS = 23 then '' else DTE_APPR_REMARK end ";
                                //    }
                                //    else if (prefix_old_doctype == "4")
                                //    {
                                //        //DevFix 20200901 2357 เพิ่มเงื่อนไขให้ update เฉพาะรายการที่ยังไม่ถูก approve โดย Line ,CAP  
                                //        sql += ", DTE_CAP_APPR_STATUS=case when DTE_CAP_APPR_STATUS = 23 then 41 else DTE_CAP_APPR_STATUS end ";
                                //        sql += ", DTE_CAP_APPR_OPT=case when DTE_CAP_APPR_STATUS = 23 then '' else DTE_CAP_APPR_OPT end ";
                                //        sql += ", DTE_CAP_APPR_REMARK=case when DTE_CAP_APPR_STATUS = 23 then '' else DTE_CAP_APPR_REMARK end ";
                                //    }
                                //    else
                                //    {
                                //        sql += " , DTE_APPR_STATUS = " + expense_status;
                                //    }
                                //}
                                //sql += " , DTE_EXPENSE_CONFIRM=1 ";
                                //sql += " where DTE_TOKEN='" + item.ref_id + "'";

                                //context.Database.ExecuteSqlCommand(sql);

                                sql = @"UPDATE BZ_DOC_TRAVELER_EXPENSE SET 
                                            DTE_TOKEN_UPD = :expen_upd_token,
                                            DTE_ID = :inx,
                                            DTE_AIR_TECKET = :air_ticket,
                                            DTE_ACCOMMODATIC = :accommodation,
                                            DTE_ALLOWANCE = :allowance,
                                            DTE_ALLOWANCE_DAY = :allowance_day,
                                            DTE_ALLOWANCE_NIGHT = :allowance_night,
                                            DTE_CL_EXPENSE = :clothing_expense,
                                            DTE_PASSPORT_EXPENSE = :passport_expense,
                                            DTE_VISA_FREE = :visa_fee,
                                            DTE_TRAVEL_INS = :travel_insurance,
                                            DTE_TRANSPORT = :transportation,
                                            DTE_MISCELLANEOUS = :miscellaneous,
                                            DTE_TOTAL_EXPENSE = :total_expenses,
                                            DTE_REGIS_FREE = :registration_fee,
                                            DTE_CL_VALID = :clothing_valid,
                                            DTE_PASSPORT_VALID = :passport_valid,
                                            DTE_EXPENSE_CONFIRM = 1 ";

                                // ตรวจสอบเงื่อนไขสำหรับการอัปเดตเพิ่มเติม
                                if (value.action.type == "5")
                                {
                                    if (prefix_old_doctype == "3")
                                    {
                                        sql += @"
                                             , DTE_APPR_STATUS = CASE WHEN DTE_APPR_STATUS = 23 THEN 31 ELSE DTE_APPR_STATUS END
                                             , DTE_APPR_OPT = CASE WHEN DTE_APPR_STATUS = 23 THEN '' ELSE DTE_APPR_OPT END
                                             , DTE_APPR_REMARK = CASE WHEN DTE_APPR_STATUS = 23 THEN '' ELSE DTE_APPR_REMARK END  ";
                                    }
                                    else if (prefix_old_doctype == "4")
                                    {
                                        sql += @"
                                            , DTE_CAP_APPR_STATUS = CASE WHEN DTE_CAP_APPR_STATUS = 23 THEN 41 ELSE DTE_CAP_APPR_STATUS END
                                            , DTE_CAP_APPR_OPT = CASE WHEN DTE_CAP_APPR_STATUS = 23 THEN '' ELSE DTE_CAP_APPR_OPT END 
                                            , DTE_CAP_APPR_REMARK = CASE WHEN DTE_CAP_APPR_STATUS = 23 THEN '' ELSE DTE_CAP_APPR_REMARK END  ";
                                    }
                                    else
                                    {
                                        sql += " , DTE_APPR_STATUS = :expense_status";
                                    }
                                }
                                sql += " WHERE DTE_TOKEN = :ref_id";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("expen_upd_token", expen_upd_token, "char"));
                                parameters.Add(context.ConvertTypeParameter("inx", inx, "int"));
                                parameters.Add(context.ConvertTypeParameter("air_ticket", item.air_ticket, "char"));
                                parameters.Add(context.ConvertTypeParameter("accommodation", item.accommodation, "char"));
                                parameters.Add(context.ConvertTypeParameter("allowance", item.allowance, "char"));
                                parameters.Add(context.ConvertTypeParameter("allowance_day", item.allowance_day, "int"));
                                parameters.Add(context.ConvertTypeParameter("allowance_night", item.allowance_night, "int"));
                                parameters.Add(context.ConvertTypeParameter("clothing_expense", item.clothing_expense, "char"));
                                parameters.Add(context.ConvertTypeParameter("passport_expense", item.passport_expense, "char"));
                                parameters.Add(context.ConvertTypeParameter("visa_fee", item.visa_fee, "char"));
                                parameters.Add(context.ConvertTypeParameter("travel_insurance", item.travel_insurance, "char"));
                                parameters.Add(context.ConvertTypeParameter("transportation", item.transportation, "char"));
                                parameters.Add(context.ConvertTypeParameter("miscellaneous", item.miscellaneous, "char"));
                                parameters.Add(context.ConvertTypeParameter("total_expenses", item.total_expenses, "int"));
                                parameters.Add(context.ConvertTypeParameter("registration_fee", item.registration_fee, "char"));
                                parameters.Add(context.ConvertTypeParameter("clothing_valid", chkDateSQL_All(item.clothing_valid ?? ""), "date"));
                                parameters.Add(context.ConvertTypeParameter("passport_valid", chkDateSQL_All(item.passport_valid ?? ""), "date"));

                                if (value.action.type == "5")
                                {
                                    if (!(prefix_old_doctype == "3" || prefix_old_doctype == "4"))
                                    {
                                        parameters.Add(context.ConvertTypeParameter("expense_status", expense_status, "char"));
                                    }
                                }
                                parameters.Add(context.ConvertTypeParameter("ref_id", item.ref_id, "char"));

                                context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                #endregion "#### BZ_DOC_TRAVELER_EXPENSE ####"

                            } // end for expen

                            // update inaction ในกรณีที่เป็นรายการที่อยู่บนหน้าจอ
                            //sql = "update BZ_DOC_TRAVELER_EXPENSE set DTE_EXPENSE_CONFIRM=0 ";
                            //sql += " where DH_CODE='" + value.doc_id + "' ";
                            //sql += " and ( DTE_TOKEN_UPD != '" + expen_upd_token + "' or DTE_TOKEN_UPD is null) ";
                            //context.Database.ExecuteSqlCommand(sql);
                            sql = "update BZ_DOC_TRAVELER_EXPENSE set DTE_EXPENSE_CONFIRM=0 where DH_CODE= :doc_id  and ( DTE_TOKEN_UPD != :expen_upd_token or DTE_TOKEN_UPD is null) ";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            parameters.Add(context.ConvertTypeParameter("expen_upd_token", expen_upd_token, "char"));
                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                            #endregion "#### BZ_DOC_TRAVELER_EXPENSE, BZ_DOC_TRAVELER_EXPENSE_TEMP ####"

                            #region "#### Compare BZ_DOC_TRAVELER_APPROVER ####"

                            string tokenUpdate = Guid.NewGuid().ToString();

                            string dta_type_check = "0";

                            if (prefix_old_doctype == "2" || prefix_old_doctype == "3")
                                dta_type_check = "1"; // line approver
                            else
                                dta_type_check = "2"; // cap

                            //DevFix 20210811 0000 เนื่องจากมีการแก้ไขข้อมูล Approver จากหน้าบ้าน จึงลบข้อมูลเก่าออกก่อน
                            foreach (var item in travelApproveTemp)
                            {
                                // เช็คว่าเป็น record ที่มีอยู่แล้วหรือไม่
                                var listFind = approveList.Where(p => p.type.Equals(item.DTA_TYPE)
                                                                && p.emp_id.Equals(item.DTA_TRAVEL_EMPID)
                                                                && p.appr_id.Equals(item.DTA_APPR_EMPID)
                                                                ).ToList();
                                if (listFind != null && listFind.Count() > 0)
                                {
                                }
                                else
                                {
                                    if (value.action.type != "3") // revise)
                                    {
                                        ////ข้อมูลเก่าใน db ให้ลบทิ้ง
                                        //sql = "delete from BZ_DOC_TRAVELER_APPROVER ";
                                        //sql += " where dh_code = '" + value.doc_id + "' ";
                                        //sql += " and DTA_TYPE='" + item.DTA_TYPE + "' ";
                                        //sql += " and DTA_APPR_EMPID='" + item.DTA_APPR_EMPID + "' ";
                                        //sql += " and DTA_TRAVEL_EMPID='" + item.DTA_TRAVEL_EMPID + "' ";
                                        //sql += " and DTA_STATUS = 1 ";
                                        //if (value.action.type != "3") // revise)
                                        //{
                                        //    int result = context.Database.ExecuteSqlCommand(sql);
                                        //}
                                        //ข้อมูลเก่าใน db ให้ลบทิ้ง
                                        sql = "delete from BZ_DOC_TRAVELER_APPROVER ";
                                        sql += " where dh_code = :doc_id ";
                                        sql += " and DTA_TYPE= :DTA_TYPE ";
                                        sql += " and DTA_APPR_EMPID= :DTA_APPR_EMPID ";
                                        sql += " and DTA_TRAVEL_EMPID= :DTA_TRAVEL_EMPID ";
                                        sql += " and DTA_STATUS = 1 ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("DTA_TYPE", item.DTA_TYPE, "char"));
                                        parameters.Add(context.ConvertTypeParameter("DTA_APPR_EMPID", item.DTA_APPR_EMPID, "char"));
                                        parameters.Add(context.ConvertTypeParameter("DTA_TRAVEL_EMPID", item.DTA_TRAVEL_EMPID, "char"));
                                        int result = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }
                                }
                            }


                            //DevFix 20211109 0000 กรณีที่ Line/CAP Revise  
                            var bstep_approver = false;
                            var traveler_approver_List = context.BZ_DOC_TRAVELER_APPROVER.Where(p => p.DH_CODE.Equals(value.doc_id) && p.DTA_STATUS == 1).ToList();
                            if (value.action.type == "5")
                            {
                                if (prefix_old_doctype == "3" || prefix_old_doctype == "4")
                                {
                                    bstep_approver = true;
                                }
                            }

                            inx = 0;
                            // approveList ข้อมูลจากหน้าจอ
                            foreach (var item in approveList)
                            {
                                //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID
                                var traveler_id = item.emp_id;

                                item.token_update = tokenUpdate;

                                // เช็คว่าเป็น record ที่มีอยู่แล้วหรือไม่
                                var listFind = travelApproveTemp.Where(p => p.DTA_TYPE.Equals(item.type)
                                                                && p.DTA_TRAVEL_EMPID.Equals(item.emp_id)
                                                                && p.DTA_APPR_EMPID.Equals(item.appr_id)
                                                                ).ToList();

                                //DevFix 20211109 0000 กรณีที่ Line/CAP ที่มีรายชื่อในใบงาน แต่ status ไม่ใช่ revise ให้ update ข้อมูลอื้่่นๆ ยกเว้น DTA_DOC_STATUS
                                var approver_status_not_revise_old = false;
                                if (bstep_approver == true)
                                {

                                    //DevFix 20221121 0000 กรณีที่บางรายการแก้ไข Line/CAP เป็นคนใหม่ แต่มี Line/CAP อยู่ในรายการอื่นร่วมด้วยทำให้หลุด ให้เช็ค type เพิ่ม 1 = Line, 2 = CAP
                                    //var drList = traveler_approver_List.Where(p => p.DTA_APPR_EMPID.Equals(item.appr_id) 
                                    //&& p.DTA_TRAVEL_EMPID == item.emp_id
                                    //&& p.DTA_DOC_STATUS != 23).ToList();
                                    var drList = traveler_approver_List.Where(p => p.DTA_APPR_EMPID.Equals(item.appr_id)
                                    && p.DTA_TRAVEL_EMPID == item.emp_id
                                    && p.DTA_DOC_STATUS != 23
                                    && p.DTA_TYPE.Equals(item.type)).ToList();
                                    if (drList.Count > 0)
                                    {
                                        approver_status_not_revise_old = true;
                                    }
                                }

                                // ถ้ามีให้ update
                                if (listFind != null && listFind.Count() > 0)
                                {

                                    #region  DevFix 20200914 1200 เพิ่ม position ของ apprver เช่น EVP = 1, SEVP = 2, CEO = 3 ??? เหลือกรณีที่ Cost Center  
                                    var appr_level = "0";
                                    var masterCostCenterList = masterCostCenterList1;
                                    appr_level = item.approve_level;
                                    #endregion DevFix 20200914 1200 เพิ่ม position ของ apprver เช่น EVP = 1, SEVP = 2, CEO = 3

                                    inx++;
                                    item.line_id = inx.ToString();
                                    item.record_status = "update";
                                    item.doc_status = listFind[0].DTA_DOC_STATUS == null ? "" : listFind[0].DTA_DOC_STATUS.ToString();
                                    item.appr_status = listFind[0].DTA_APPR_STATUS ?? "";
                                    item.appr_remark = listFind[0].DTA_APPR_REMARK ?? "";


                                    parameters = new List<OracleParameter>();

                                    //sql = "update BZ_DOC_TRAVELER_APPROVER set ";
                                    //sql += " DTA_UPDATE_TOKEN='" + item.token_update + "' ";
                                    sql = "update BZ_DOC_TRAVELER_APPROVER set DTA_UPDATE_TOKEN= :token_update ";
                                    parameters.Add(context.ConvertTypeParameter("token_update", item.token_update, "char"));

                                    if (listFind[0].DTA_DOC_STATUS != null && (listFind[0].DTA_DOC_STATUS == 32 || listFind[0].DTA_DOC_STATUS == 42))
                                    {

                                    }
                                    else
                                    {
                                        //sql += " , DTA_ID=" + item.line_id;
                                        sql += " , DTA_ID= :line_id";
                                        parameters.Add(context.ConvertTypeParameter("line_id", item.line_id, "int"));

                                        //  sql += " , DTA_DOC_STATUS=" + next_action_status;
                                        if (approver_status_not_revise_old == false)
                                        {
                                            if (dta_type_check == "1" || dta_type_check == "2" && dta_type_check == item.type)
                                            {
                                                //DevFix 20210717 1200 เพิ่มให้ update status ตามเดิมกรณีที่เป็นการ reject / approve CAP ไม่ต้อง อนุมัติใหม่
                                                //sql += " , DTA_DOC_STATUS= " + next_action_status.ToString(); 
                                                var check_action_update = true;
                                                var check_add_action = true;

                                                //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID
                                                //var check = tempEmpForAction.Where(p => p.EMP_ID.Equals(item.appr_id));
                                                var check = tempEmpForAction.Where(p => p.EMP_ID.Equals(item.appr_id)
                                                                                   && p.FROM_EMP_ID.Equals(item.emp_id));

                                                if (check != null && check.Count() <= 0)
                                                {
                                                    //DevFix 20200828 2157  เฉพาะที่เป็น CAP ให้ update status =2 
                                                    var action_status_type = item.type == "2" ? 2 : 1;
                                                    if (prefix_old_doctype == "3" || prefix_old_doctype == "4")
                                                    {
                                                        if (listFind[0].DTA_ACTION_STATUS.ToString() != "1" &&
                                                            listFind[0].DTA_ACTION_STATUS.ToString() != "4")
                                                        {
                                                            check_add_action = false;
                                                        }

                                                        if (item.remark.IndexOf("Endorsed") > -1)
                                                        {
                                                            //ไม่ Update Actino กรณีที่ Line กด reject ไปแล้ว -->ไม่ให้ส่งไปแจ้ง Line
                                                            if (listFind[0].DTA_ACTION_STATUS.ToString() == "5")
                                                            {
                                                                action_status_type = 2;
                                                                check_action_update = false;
                                                            }
                                                            else
                                                            {
                                                                action_status_type = 1;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (next_action_status == 41)
                                                            {
                                                                //ส่งหา CAP
                                                                if (listFind[0].DTA_ACTION_STATUS.ToString() == "5")
                                                                {
                                                                    action_status_type = 2;

                                                                    //DevFix 20210717 1200 เพิ่มให้ update status ตามเดิมกรณีที่เป็นการ reject / approve CAP ไม่ต้อง อนุมัติใหม่
                                                                    check_action_update = false;//reject
                                                                }
                                                                else
                                                                {
                                                                    action_status_type = 1;
                                                                }

                                                            }
                                                            else
                                                            {
                                                                //ส่งหา Line
                                                                action_status_type = 2;
                                                            }
                                                        }
                                                    }

                                                    if (check_add_action == true)
                                                    {
                                                        tempEmpForAction.Add(new BZ_DOC_ACTION
                                                        {
                                                            //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID
                                                            FROM_EMP_ID = traveler_id.ToString(),

                                                            //DevFix 20200828 2157  เฉพาะที่เป็น CAP ให้ update status =2 
                                                            ACTION_STATUS = action_status_type,
                                                            DA_TOKEN = tokenUpdate,
                                                            EMP_ID = item.appr_id // คนอนุมัติ  

                                                        });
                                                    }

                                                }

                                                //DevFix 20210717 1200 เพิ่มให้ update status ตามเดิมกรณีที่เป็นการ reject / approve CAP ไม่ต้อง อนุมัติใหม่
                                                if (check_action_update == true)
                                                {
                                                    //sql += " , DTA_DOC_STATUS= " + next_action_status.ToString();
                                                    sql += " , DTA_DOC_STATUS= :next_action_status ";
                                                    parameters.Add(context.ConvertTypeParameter("next_action_status", next_action_status, "int"));
                                                }

                                            }
                                        }
                                    }

                                    if (appr_level == null) { appr_level = "0"; }
                                    //sql += " ,DTA_APPR_LEVEL = '" + appr_level + "' ";
                                    sql += " ,DTA_APPR_LEVEL = :appr_level ";
                                    parameters.Add(context.ConvertTypeParameter("appr_level", appr_level, "char"));

                                    //sql += " where dh_code = '" + value.doc_id + "' ";
                                    //sql += " and DTA_TYPE='" + item.type + "' ";
                                    //sql += " and DTA_APPR_EMPID='" + item.appr_id + "' ";
                                    //sql += " and DTA_TRAVEL_EMPID='" + item.emp_id + "' ";
                                    //sql += " and DTA_STATUS = 1 ";

                                    sql += " where dh_code = :doc_id ";
                                    sql += " and DTA_TYPE= :type ";
                                    sql += " and DTA_APPR_EMPID= :appr_id ";
                                    sql += " and DTA_TRAVEL_EMPID= :emp_id ";
                                    sql += " and DTA_STATUS = 1 ";

                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("type", item.type, "char"));
                                    parameters.Add(context.ConvertTypeParameter("appr_id", item.appr_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("emp_id", item.emp_id, "char"));

                                    //DevFix 20211109 0000 กรณีที่ Line/CAP Revise   
                                    if (bstep_approver == true && approver_status_not_revise_old == false)
                                    {
                                        sql += " and DTA_DOC_STATUS = 23 ";
                                    }

                                    if (value.action.type == "3") // revise)
                                    {
                                        //DevFix 20210718 0000 เพิ่มเงื่อนไขกรณีที่ Line Revise และ Admin Revise ไปหา Requester  
                                    }
                                    else
                                    {
                                        //int result = context.Database.ExecuteSqlCommand(sql);
                                        int result = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                }
                                else
                                {
                                    inx++;
                                    item.line_id = inx.ToString();
                                    item.record_status = "insert";

                                    string s_next_status = "null";
                                    s_next_status = next_action_status.ToString();

                                    //DevFix 20200828 2157  เฉพาะที่เป็น CAP ให้ update status =2  
                                    var action_status_type = item.type == "2" ? 2 : 1;
                                    if (prefix_old_doctype == "3" || prefix_old_doctype == "4")
                                    {
                                        if (item.remark.IndexOf("Endorsed") > -1)
                                        {
                                            action_status_type = 1;
                                        }
                                        else
                                        {
                                            if (next_action_status == 41)
                                            {
                                                action_status_type = 1;
                                            }
                                            else
                                            {
                                                //ส่งหา Line
                                                action_status_type = 2;
                                            }
                                        }
                                    }

                                    var check = tempEmpForAction.Where(p => p.EMP_ID.Equals(item.appr_id));
                                    if (check != null && check.Count() <= 0)
                                    {
                                        tempEmpForAction.Add(new BZ_DOC_ACTION
                                        {
                                            //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID
                                            FROM_EMP_ID = traveler_id.ToString(),

                                            //DevFix 20200828 2157  เฉพาะที่เป็น CAP ให้ update status =2 
                                            //ACTION_STATUS = item.type == "2" ? 2 : 1,
                                            ACTION_STATUS = action_status_type,
                                            DA_TOKEN = tokenUpdate,
                                            EMP_ID = item.appr_id // คนอนุมัติ
                                        });

                                    }


                                    #region  DevFix 20200914 1200 เพิ่ม position ของ apprver เช่น EVP = 1, SEVP = 2, CEO = 3 ??? เหลือกรณีที่ Cost Center  
                                    var appr_level = "0";
                                    var bcheck_non_sevp = false;
                                    var masterCostCenterList = masterCostCenterList1;

                                    appr_level = item.approve_level;

                                    if (item.type == "2")
                                    {
                                    }
                                    #endregion DevFix 20200914 1200 เพิ่ม position ของ apprver เช่น EVP = 1, SEVP = 2, CEO = 3

                                    if (appr_level == null) { appr_level = "0"; }
                                    //sql = "insert into BZ_DOC_TRAVELER_APPROVER (DH_CODE, DTA_ID, DTA_TYPE, DTA_APPR_EMPID ";
                                    //sql += " , DTA_TRAVEL_EMPID, DTA_REMARK, DTA_DOC_STATUS, DTA_UPDATE_TOKEN,DTA_APPR_LEVEL) ";
                                    //sql += " values (";
                                    //sql += " '" + value.doc_id + "', " + item.line_id + ", '" + item.type + "', '" + item.appr_id + "' ";
                                    //sql += ", '" + item.emp_id + "', '" + item.remark + "', " + s_next_status + ", '" + item.token_update + "' , '" + appr_level + "'  ";
                                    //sql += " ) ";
                                    //context.Database.ExecuteSqlCommand(sql);
                                    sql = "insert into BZ_DOC_TRAVELER_APPROVER (DH_CODE, DTA_ID, DTA_TYPE, DTA_APPR_EMPID, DTA_TRAVEL_EMPID, DTA_REMARK, DTA_DOC_STATUS, DTA_UPDATE_TOKEN,DTA_APPR_LEVEL) ";
                                    sql += " values (:doc_id, :line_id, :type, :appr_id, :emp_id, :remark, :s_next_status, :token_update, :appr_level ) ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("line_id", item.line_id, "int"));
                                    parameters.Add(context.ConvertTypeParameter("type", item.type, "char"));
                                    parameters.Add(context.ConvertTypeParameter("appr_id", item.appr_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("emp_id", item.emp_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("remark", item.remark, "char"));
                                    parameters.Add(context.ConvertTypeParameter("s_next_status", s_next_status, "int"));
                                    parameters.Add(context.ConvertTypeParameter("token_update", item.token_update, "char"));
                                    parameters.Add(context.ConvertTypeParameter("appr_level", appr_level, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                }

                            }

                            if (value.action.type == "5")
                            {
                                if (value.action.type == "3") { }
                                else
                                {
                                    // update inaction ในกรณีที่เป็นรายการที่อยู่บนหน้าจอ
                                    //sql = "update BZ_DOC_TRAVELER_APPROVER set DTA_STATUS=0 ";
                                    //sql += " where DH_CODE='" + value.doc_id + "' ";
                                    //sql += " and DTA_STATUS = 1 and  ( DTA_UPDATE_TOKEN != '" + tokenUpdate + "' or DTA_UPDATE_TOKEN is null) ";
                                    //// ทำเฉพาะที่ไปถึง level cap แล้ว
                                    //if (dta_type_check == "2")
                                    //{
                                    //    sql += " and DTA_TYPE = 2 ";
                                    //}
                                    //context.Database.ExecuteSqlCommand(sql);

                                    // ทำเฉพาะที่ไปถึง level cap แล้ว
                                    if (dta_type_check == "2")
                                    {
                                        sql = "update BZ_DOC_TRAVELER_APPROVER set DTA_STATUS=0 where DH_CODE= :doc_id and DTA_STATUS = 1 and  ( DTA_UPDATE_TOKEN != :tokenUpdate or DTA_UPDATE_TOKEN is null)  and DTA_TYPE = 2  ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("tokenUpdate", tokenUpdate, "char"));
                                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                }

                                if (value.action.type == "3") { }
                                else
                                {
                                    //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject 
                                    ////update DTA_ACTION_STATUS 1, 4 to 2 
                                    //sql = "update BZ_DOC_TRAVELER_APPROVER set DTA_ACTION_STATUS = 2 ";
                                    //sql += " where DTA_ACTION_STATUS in (1,4) and DH_CODE='" + value.doc_id + "' ";
                                    //if (dta_type_check == "2")
                                    //{
                                    //    sql += " and DTA_TYPE = 2";
                                    //}
                                    //else
                                    //{
                                    //    sql += " and DTA_TYPE = 1";
                                    //}
                                    //context.Database.ExecuteSqlCommand(sql);

                                    sql = "update BZ_DOC_TRAVELER_APPROVER set DTA_ACTION_STATUS = 2 where DTA_ACTION_STATUS in (1,4) and DH_CODE= :doc_id ";
                                    if (dta_type_check == "2")
                                    {
                                        sql += " and DTA_TYPE = 2";
                                    }
                                    else
                                    {
                                        sql += " and DTA_TYPE = 1";
                                    }
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                            }
                            #endregion

                            #region "#### SUBMIT type 5 && BZ_DOC_ACTION ####" 
                            if (value.action.type == "5") // submit
                            {
                                if (prefix_old_doctype == "2")
                                {
                                    // ???
                                    //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = '" + value.doc_id + "'");

                                    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = :doc_id";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                }
                                else if (prefix_old_doctype == "3")
                                {
                                    //update กรณีที่มีการ revise กลับมา action_status ให้เป็น 2 เนื่องจากอาจจะมีกรณีที่ revise แล้ว ได้ line approve ใหม่ --> line approve เก่าไม่มีการส่งไป update ??? 
                                    //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = '" + value.doc_id + "'");
                                    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = :doc_id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                else if (prefix_old_doctype == "4")
                                {
                                    //update กรณีที่มีการ revise กลับมา action_status ให้เป็น 2 เนื่องจากอาจจะมีกรณีที่ revise แล้ว ได้ line approve ใหม่ --> line approve เก่าไม่มีการส่งไป update ??? 
                                    //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = '" + value.doc_id + "'");
                                    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = :doc_id";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }

                                foreach (var item in tempEmpForAction)
                                {
                                    context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                    {
                                        DA_TOKEN = Guid.NewGuid().ToString(),
                                        DH_CODE = value.doc_id,
                                        DOC_TYPE = doc_head_search.DH_TYPE ?? "",
                                        DOC_STATUS = next_action_status,
                                        EMP_ID = item.EMP_ID,
                                        TAB_NO = next_topno,
                                        ACTION_STATUS = item.ACTION_STATUS,
                                        ACTION_DATE = DateTime.Now,
                                        CREATED_DATE = DateTime.Now,
                                        UPDATED_DATE = DateTime.Now,

                                        //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID
                                        FROM_EMP_ID = item.FROM_EMP_ID.ToString()
                                    });
                                }
                                context.BZ_DOC_ACTION.Add(new BZ_DOC_ACTION
                                {
                                    DA_TOKEN = Guid.NewGuid().ToString(),
                                    DH_CODE = value.doc_id,
                                    DOC_TYPE = doc_head_search.DH_TYPE ?? "",
                                    DOC_STATUS = next_action_status,
                                    EMP_ID = "admin",
                                    TAB_NO = next_topno,
                                    ACTION_STATUS = 1,
                                    ACTION_DATE = DateTime.Now,
                                    CREATED_DATE = DateTime.Now,
                                    UPDATED_DATE = DateTime.Now,

                                    //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID
                                    //FROM_EMP_ID = runningno_action.ToString()
                                });

                            }
                            #endregion

                            //DevFix 20210718 0000 ปิด code นี้ เนื่องจาก มีข้อมูลค้างจากการ genarate ครั้งแรก
                            //sql = "delete from BZ_DOC_TRAVELER_APPROVER  ";
                            //sql += " where DH_CODE='" + value.doc_id + "' and DTA_DOC_STATUS is null  ";
                            //context.Database.ExecuteSqlCommand(sql);
                            sql = "delete from BZ_DOC_TRAVELER_APPROVER where DH_CODE= :doc_id and DTA_DOC_STATUS is null  ";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                            context.SaveChanges();
                            transaction.Commit();

                            data.status = "S";
                            data.message = "";

                            #region "#### SEND MAIL ####"
                            write_log_mail("0-email.start-submitFlow2_v3", "type_flow :" + type_flow + " =>value.action.type :" + value.action.type);

                            //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
                            if (type_flow == true)
                            {
                                //DevFix 20200910 0727 เพิ่มแนบ link Ebiz ด้วย Link ไปหน้า login  
                                string url_login = LinkLogin;
                                string sDear = "";
                                string sDetail = "";
                                string sTitle = "";
                                string sBusinessDate = "";
                                string sLocation = "";
                                string sTravelerList = "";
                                string sReasonRejected = "";

                                if (value.action.type == "2" || value.action.type == "3" || value.action.type == "5")
                                {

                                    try
                                    {
                                        sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                        sBusinessDate = "Business Date : ";
                                        if (doc_head_search.DH_BUS_FROMDATE != null)
                                        {
                                            sBusinessDate = "Business Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                        }

                                        if (value.action.type == "3") { next_action_status = 23; }

                                        //sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2    
                                        //             FROM bz_doc_traveler_expense a 
                                        //             inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                        //             inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                        //             left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                        //             ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                        //             and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                        //             left join bz_master_country c on a.ct_id = c.ct_id
                                        //             left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                        //             WHERE a.dh_code in ('" + value.doc_id + "') and a.dte_status = 1 and a.dte_appr_status  = '" + next_action_status + "' " +
                                        //             " order by s.id";
                                        //var temp = context.Database.SqlQuery<tempModel>(sql).ToList();

                                        sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a 
                                                     inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     WHERE a.dh_code = :doc_id and a.dte_status = 1 and a.dte_appr_status  = :next_action_status
                                                     order by s.id";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("next_action_status", next_action_status, "char"));
                                        var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        if (temp != null && temp.Count() > 0)
                                        {
                                            //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                            if (temp.Count == 1)
                                            {
                                                sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            }
                                            else
                                            {
                                                sLocation = "";
                                                foreach (var item in temp)
                                                {
                                                    if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                    sLocation += item.name1 + "/" + item.name2;
                                                }
                                            }
                                        }

                                        sendEmailService mail = new sendEmailService();
                                        sendEmailModel dataMail = new sendEmailModel();

                                        if (value.action.type == "5") // submit
                                        {
                                            #region "#### SUBMIT ####"  
                                            sql = "select EMPLOYEEID user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, b.email email from BZ_USERS b ";
                                            parameters = new List<OracleParameter>();
                                            //parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char")); 
                                            var userList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            #region DevFix 20210806 0000 กรณีที่ไม่ใช่ admin ให้ตรวจสอบ emp ว่าเป็น traveler   
                                            sql = @"select distinct to_char(t.user_type ) as role_type, t.emp_id as user_id, '' as user_name, '' as email
                                                 from ( 
                                                 select dh_code as doc_id, 2 as user_type, a.dta_appr_empid as emp_id
                                                 from  bz_doc_traveler_approver a where a.dta_type = 1 
                                                 )t
                                                 inner join (  
                                                 select dh_code as doc_id, 3 as user_type, a.dta_appr_empid as emp_id
                                                 from  bz_doc_traveler_approver a  where a.dta_type = 2  
                                                 )t1 on t.doc_id = t1.doc_id and t.emp_id = t1.emp_id
                                                 where t.doc_id = :doc_id order by role_type desc ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var emp_type = context.SearchUserEmailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            #endregion DevFix 20210806 0000 กรณีที่ไม่ใช่ admin ให้ตรวจสอบ emp ว่าเป็น traveler 

                                            //DevFix 20210718 0000 เก็บค่าไว้กณีที่เป็น CAP ข้อมูลจะซ้ำกัน
                                            var tempEmpForAction_def = new List<BZ_DOC_ACTION>();
                                            for (int i = 0; i < tempEmpForAction.Count; i++)
                                            {
                                                string action_status = tempEmpForAction[i].ACTION_STATUS.ToString();
                                                string emp_select = tempEmpForAction[i].EMP_ID.ToString();
                                                //ใช้ action_status = 2 ไม่ได้เนื่องจากกรณีที่มีการ revise มาให้ admin ตอน submit ส่งไปให้ line ที่เป็นคน revise กับ cap ?ไม่ต้องส่งให้ cap
                                                if (action_status == "2") { continue; }
                                                var dta_type = 1;
                                                if (next_topno == 4) // line approver
                                                {
                                                    dta_type = 2;
                                                }

                                                //DevFix 20210813 0000 เพิ่ม email เพื่อนำไปใช้ตอน cc 
                                                sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                                                         , b.employeeid as name3, b.orgname as name4
                                                         from BZ_DOC_TRAVELER_EXPENSE a left join bz_users b on a.DTE_EMP_ID = b.employeeid
                                                         left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                                         on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id  
                                                         where a.dh_code = :doc_id 
                                                         and (a.dte_emp_id, a.dh_code) in (select distinct dta_travel_empid, dh_code from BZ_DOC_TRAVELER_APPROVER
                                                         where dta_type = :dta_type and dta_appr_empid = :emp_select ";
                                                if (next_topno == 4) { sql += @" and dte_cap_appr_status = 41 "; }
                                                sql += @" ) order by s.id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("dta_type", dta_type, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_select", emp_select, "char"));
                                                var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (tempTravel != null)
                                                {
                                                    foreach (var item in tempTravel)
                                                    {
                                                        traveler_mail += ";" + item.name2;
                                                    }
                                                }

                                                var empapp = userList.Where(p => p.user_id == emp_select).ToList();
                                                dataMail.mail_body = "";
                                                if (next_topno == 3) // line approver
                                                {
                                                    //DevFix 20210729 0000 เช็คค่ากณีที่เป็น Line ข้อมูลจะซ้ำกัน
                                                    var appr_id = tempEmpForAction[i].EMP_ID.ToString();
                                                    var traveler_id = tempEmpForAction[i].FROM_EMP_ID.ToString();
                                                    var check_def = tempEmpForAction_def.Where(p => p.EMP_ID.Equals(appr_id));
                                                    if (check_def != null && check_def.Count() > 0) { continue; }
                                                    else
                                                    {
                                                        tempEmpForAction_def.Add(new BZ_DOC_ACTION
                                                        {
                                                            //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID 
                                                            EMP_ID = appr_id // คนอนุมัติ  

                                                        });
                                                    }

                                                    #region DevFix 20210806 0000 กรณีที่ไม่ใช่ admin ให้ตรวจสอบ emp ว่าเป็น traveler
                                                    var approver2role = false;
                                                    if (emp_type != null && emp_type.Count() > 0)
                                                    {
                                                        var check_approver2role_def = emp_type.Where(p => p.user_id.Equals(appr_id));
                                                        if (check_approver2role_def != null && check_approver2role_def.Count() > 0)
                                                        {
                                                            approver2role = true;
                                                        }
                                                    }
                                                    #endregion DevFix 20210806 0000 กรณีที่ไม่ใช่ admin ให้ตรวจสอบ emp ว่าเป็น traveler 

                                                    //to : Line Approval	
                                                    //cc : Requester, Traveller, Super Admin 
                                                    dataMail.mail_to = empapp[0].email ?? "";
                                                    dataMail.mail_cc = requester_mail + on_behalf_of_mail + admin_mail;// + traveler_mail ;
                                                    if (approver2role == true)
                                                    {
                                                        dataMail.mail_subject = value.doc_id + " :  Please endorse business travel request as line manager.";
                                                    }
                                                    else { dataMail.mail_subject = value.doc_id + " :  Please endorse business travel request as line manager."; }

                                                    sDear = "Dear " + empapp[0].user_name + ",";

                                                    if (approver2role == true)
                                                    {
                                                        sDetail = "Please endorse business travel as line manager / CAP. To view the details, click ";
                                                    }
                                                    else
                                                    {
                                                        sDetail = "Please endorse business travel as line manager. To view the details, click ";
                                                    }
                                                    sDetail += "<a href='" + (LinkLogin + "ii").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                                }
                                                else // CAP
                                                {
                                                    //DevFix 20210718 0000 เช็คค่ากณีที่เป็น CAP ข้อมูลจะซ้ำกัน
                                                    var appr_id = tempEmpForAction[i].EMP_ID.ToString();
                                                    var traveler_id = tempEmpForAction[i].FROM_EMP_ID.ToString();
                                                    var check_def = tempEmpForAction_def.Where(p => p.EMP_ID.Equals(appr_id));
                                                    if (check_def != null && check_def.Count() > 0) { continue; }
                                                    else
                                                    {
                                                        tempEmpForAction_def.Add(new BZ_DOC_ACTION
                                                        {
                                                            //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID 
                                                            EMP_ID = appr_id // คนอนุมัติ  

                                                        });
                                                    }

                                                    //to : Line Approval	
                                                    //cc : Requester, Traveller, Super Admin 
                                                    dataMail.mail_to = empapp[0].email ?? "";
                                                    dataMail.mail_cc = requester_mail + on_behalf_of_mail + admin_mail;//+ traveler_mail ;

                                                    dataMail.mail_subject = value.doc_id + " : Please approve business travel request as CAP.";

                                                    sDear = "Dear " + empapp[0].user_name + ",";

                                                    sDetail = "Please approve business travel request as CAP. To view the details, click ";
                                                    sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                                }

                                                #region table

                                                var iNo = 1;
                                                sTravelerList = "<table>";
                                                foreach (var item in tempTravel)
                                                {
                                                    sTravelerList += " <tr>";
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                    sTravelerList += " </tr>";
                                                    iNo++;
                                                }
                                                sTravelerList += "</table>";


                                                dataMail.mail_body = @"<span lang='en-US'>";
                                                dataMail.mail_body += "<div>";
                                                dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                                dataMail.mail_body += "     </span></font></div>";

                                                dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     Best Regards,";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                                dataMail.mail_body += "     </span></font></div>";

                                                dataMail.mail_body += "</div>";
                                                dataMail.mail_body += "</span>";
                                                #endregion table

                                                var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID == next_action_status).FirstOrDefaultAsync();
                                                dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                dataMail.doc_id = doc_id;

                                                mail.sendMail(dataMail);
                                            }
                                            #endregion 

                                        }
                                        else if (value.action.type == "2") // reject
                                        {
                                            #region "#### REJECT ####"
                                            //DevFix 20210813 0000 เพิ่ม email เพื่อนำไปใช้ตอน cc  --> แต่ยังไม่เพิ่ม
                                            //sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                            //         , b.employeeid as name3, b.orgname as name4                           
                                            //     from BZ_DOC_TRAVELER_EXPENSE a
                                            //     inner join  bz_doc_traveler_approver c on a.dh_code = c.dh_code and  a.DTE_EMP_ID = c.dta_travel_empid and c.dta_type = 1 and c.dta_action_status in (5)
                                            //     inner join bz_users b on a.DTE_EMP_ID = b.employeeid
                                            //     left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                            //     on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id  
                                            //     where a.DTE_APPR_STATUS = 30 and a.dh_code = '" + value.doc_id + "' order by s.id ";

                                            //var tempTravel = context.Database.SqlQuery<tempModel>(sql).ToList();

                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                                     , b.employeeid as name3, b.orgname as name4                           
                                                 from BZ_DOC_TRAVELER_EXPENSE a
                                                 inner join  bz_doc_traveler_approver c on a.dh_code = c.dh_code and  a.DTE_EMP_ID = c.dta_travel_empid and c.dta_type = 1 and c.dta_action_status in (5)
                                                 inner join bz_users b on a.DTE_EMP_ID = b.employeeid
                                                 left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                                 on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id  
                                                 where a.DTE_APPR_STATUS = 30 and a.dh_code = :doc_id order by s.id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (tempTravel != null)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    traveler_mail += ";" + item.name2;
                                                }
                                            }

                                            dataMail.mail_to = requester_mail;
                                            dataMail.mail_cc = admin_mail + on_behalf_of_mail;
                                            dataMail.mail_subject = value.doc_id + " :  The request for business travel has been rejected.";// + User[0].user_name + "";

                                            //sDear = "Dear " + requester_name + ",";
                                            sDear = "Dear All,";
                                            sDetail = "Your business travel request has been reject by " + User[0].user_name + ". To view the details, click ";
                                            sDetail += "<a href='" + (LinkLogin + "i").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                            var iNo = 1;
                                            sTravelerList = "<table>";
                                            if (tempTravel != null)
                                            {
                                                if (tempTravel?.Count > 0)
                                                {
                                                    foreach (var item in tempTravel)
                                                    {
                                                        sTravelerList += " <tr>";
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                        sTravelerList += " </tr>";
                                                        iNo++;
                                                    }
                                                }
                                            }
                                            sTravelerList += "</table>";

                                            #endregion
                                        }
                                        else if (value.action.type == "3") // revise
                                        {
                                            #region "#### REVISE ####"
                                            //DevFix 20210813 0000 เพิ่ม email เพื่อนำไปใช้ตอน cc  
                                            //sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                            //         , b.employeeid as name3, b.orgname as name4  
                                            //         from BZ_DOC_TRAVELER_EXPENSE a left join bz_users b on a.DTE_EMP_ID = b.employeeid 
                                            //         left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                            //         on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id
                                            //         where a.dh_code = '" + value.doc_id + "'  ";
                                            //if (pf_doc_id == "4")
                                            //{
                                            //    sql += @" and DTE_CAP_APPR_STATUS = 23";
                                            //}
                                            //else if (pf_doc_id == "3")
                                            //{
                                            //    sql += @" and DTE_APPR_STATUS = 23";
                                            //}
                                            //sql += @" order by s.id";

                                            //var tempTravel = context.Database.SqlQuery<tempModel>(sql).ToList();

                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                                     , b.employeeid as name3, b.orgname as name4  
                                                     from BZ_DOC_TRAVELER_EXPENSE a left join bz_users b on a.DTE_EMP_ID = b.employeeid 
                                                     left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                                     on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id
                                                     where a.dh_code = :doc_id ";
                                            if (pf_doc_id == "4")
                                            {
                                                sql += @" and DTE_CAP_APPR_STATUS = 23";
                                            }
                                            else if (pf_doc_id == "3")
                                            {
                                                sql += @" and DTE_APPR_STATUS = 23";
                                            }
                                            sql += @" order by s.id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();


                                            if (tempTravel != null)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    traveler_mail += ";" + item.name2;
                                                }
                                            }

                                            sql = "SELECT  employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                            sql += "FROM bz_users b WHERE employeeid= :create_by_emp_id_select ";

                                            var create_by_emp_id_select = doc_head_search.DH_CREATE_BY ?? "";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("create_by_emp_id_select", create_by_emp_id_select, "char"));
                                            var requestor = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (requestor != null && requestor?.Count > 0)
                                            {
                                                dataMail.mail_to = requestor[0].email ?? "";
                                            }
                                            dataMail.mail_cc = admin_mail + on_behalf_of_mail;

                                            dataMail.mail_subject = value.doc_id + " :  Please revise your request for business travel.";// + User[0].user_name + "";

                                            //sDear = "Dear " + requestor[0].user_name + ",";
                                            sDear = "Dear All,";

                                            sDetail = "Your business travel request has been revise by " + User[0].user_name + ". To view the details, click ";
                                            sDetail += "<a href='" + LinkLogin.Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                            var iNo = 1;
                                            sTravelerList = "<table>";
                                            if (tempTravel != null && tempTravel?.Count > 0)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    sTravelerList += " <tr>";
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                    sTravelerList += " </tr>";
                                                    iNo++;
                                                }
                                            }
                                            sTravelerList += "</table>";

                                            #endregion
                                        }


                                        if (value.action.type == "2" || value.action.type == "3")
                                        {
                                            #region set mail
                                            try
                                            {
                                                sReasonRejected = "";
                                                if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                            }
                                            catch { }

                                            dataMail.mail_body = @"<span lang='en-US'>";
                                            dataMail.mail_body += "<div>";
                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            if (sReasonRejected != "")
                                            {
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                dataMail.mail_body += "     <br>";
                                            }
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                            dataMail.mail_body += "     </span></font></div>";

                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     Best Regards,";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                            dataMail.mail_body += "     </span></font></div>";

                                            dataMail.mail_body += "</div>";
                                            dataMail.mail_body += "</span>";


                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID == next_action_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;


                                            mail.sendMail(dataMail);
                                            #endregion set mail

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        write_log_mail("88-email.message-submitFlow2_v3", "error" + ex.ToString());
                                    }

                                }
                            }

                            write_log_mail("99-email.end-submitFlow2_v3", "");
                            #endregion "#### SEND MAIL ####"

                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.status = "E";
                            data.message = ex.ToString();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                data.status = "E";
                data.message = ex.Message;
            }


            return data;
        }

        public ResultModel submitFlow3(DocFlow3Model value)
        {
            int iResult = -1;
            decimal? decimalNull = null;
            bool newDocNo = false;
            string doc_status = "";

            string user_name = "";
            string user_id = "";
            string user_role = "";

            string sql = "";
            var data = new ResultModel();

            if (value.action == null || string.IsNullOrEmpty(value.action.type))
            {
                data.status = "E";
                data.message = "Action is null !";
                return data;
            }


            //หน้าบ้านไม่ได้ส่ง approver id มาให้ใน  data.traveler_summary ทำให้ตรวจสอบข้อมูลไม่ได้ว่าตาราง BZ_DOC_TRAVELER_APPROVER 
            //กรณีที่มีข้อมูล traveler 1 คน เดินทางมากกว่า 1 รายการ ควร update status ไหน เช่นกรณีที่เป็นการ approve 1 reject 1 ตอนนี้อัฟเดตรายการท้ายสุด
            //ยิ่งถ้าเป็น admin เข้ามาทำรายการแทนจะไม่ทราบว่า traverler นั้นอยู่ภายใต้ approver
            searchDocServices ws_search = new searchDocServices();
            DocDetail3Model value_load = new DocDetail3Model();
            value_load.token = value.token_login.ToString();
            value_load.id_doc = value.doc_id.ToString();
            var out_load = ws_search.SearchDetail3(value_load);
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var traveler_expen_load = context.BZ_DOC_TRAVELER_EXPENSE.Where(p => p.DH_CODE.Equals(value.doc_id)).ToList();
                    foreach (var item in value.traveler_summary)
                    {
                        // รายการที่มีสิทธิ์ approve หรือไม่
                        if (item.take_action != "true")
                            continue;

                        var row_check = out_load.traveler_summary.Where(p => p.ref_id.Equals(item.ref_id)).ToList().FirstOrDefault();
                        if (row_check != null && row_check.emp_id != null)
                        {
                            item.appr_id = row_check.appr_id;
                        }

                        var row_check2 = traveler_expen_load.Where(p => p.DTE_TOKEN.Equals(item.ref_id)).ToList().FirstOrDefault();
                        if (row_check2 != null && row_check2.DTE_EMP_ID != null)
                        {
                            item.traverler_id = row_check2.DTE_EMP_ID;
                        }
                    }
                }
            }
            catch { }


            //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
            bool type_flow = true;

            //oversea,overseatraining,localtraining,local 
            var doc_type = "oversea";
            Boolean notiPassport = false;

            try
            {
                var Tel_Services_Team = "";
                var Tel_Call_Center = "";
                getTelServicesTeamCallCenter(ref Tel_Services_Team, ref Tel_Call_Center);

                var iret = 0;
                var doc_id = value.doc_id ?? "";
                var token_login = value.token_login ?? "";

                var parameters = new List<OracleParameter>();
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var doc_head_search = context.BZ_DOC_HEAD.Find(value.doc_id);
                    if (doc_head_search == null)
                    {
                        data.status = "E";
                        data.message = "not found data !";
                        return data;
                    }
                    else
                    {
                        doc_type = doc_head_search.DH_TYPE;
                        notiPassport = ((doc_type == "oversea" || doc_type == "overseatraining") ? true : false);
                    }

                    string admin_mail = "";
                    string requester_mail = "";
                    string requester_name = "";
                    string on_behalf_of_mail = "";
                    string traveler_mail = "";
                    string approver_mail = "";

                    #region DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin 
                    admin_mail = get_mail_group_admin(context);

                    //กรณีที่เป็น pmdv admin, pmsv_admin
                    admin_mail += mail_group_admin(context, "pmsv_admin");
                    if (doc_id.IndexOf("T") > -1)
                    {
                        admin_mail += mail_group_admin(context, "pmdv_admin");
                    }

                    get_mail_requester_in_doc(context, doc_id, ref requester_name, ref requester_mail, ref on_behalf_of_mail);

                    #endregion DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin 

                    //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW 
                    //sql = @"SELECT a.DH_TYPE_FLOW FROM BZ_DOC_HEAD a where a.DH_CODE ='" + value.doc_id + "'";
                    //var docHead = new List<DocList3Model>();
                    //docHead = context.Database.SqlQuery<DocList3Model>(sql).ToList();

                    sql = @"SELECT a.DH_TYPE_FLOW as id_key FROM BZ_DOC_HEAD a where a.DH_CODE = :doc_id ";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var docHead = context.TempIdKeyModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    if (docHead != null)
                    {
                        if ((docHead[0].id_key ?? "1") != "1") { type_flow = false; }
                    }


                    //ตรวจสอบว่ามีการ Line Approver Approve All หรือยัง  ของ Traveler นั้นๆ   
                    var bLineApproverApproveAll = false;
                    var lineApproveList = context.BZ_DOC_TRAVELER_APPROVER
                    .Where(p =>
                        p.DH_CODE.Equals(value.doc_id) &&
                        p.DTA_TYPE == "1" &&
                        !(p.DTA_APPR_STATUS.ToLower() == "true") &&
                        p.DTA_APPR_EMPID != user_id
                    ).Count();

                    //กรณีที่มีข้อมูลยังไม่ line approve ยังไม่ครบ
                    bLineApproverApproveAll = (lineApproveList > 0 ? false : true);



                    using (var transaction = context.Database.BeginTransaction())
                    {
                        get_user_role_in_token_login(context, token_login, ref user_name, ref user_id, ref user_role);

                        if (doc_id.IndexOf("T") > -1)
                        {
                            var user_role_select = user_role ?? "";
                            user_role = get_role_admin_in_manage(context, user_id, user_role_select);
                        }

                        try
                        {
                            doc_head_search.DH_AFTER_TRIP_OPT1 = retCheckValue(value.after_trip.opt1 ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT2 = retCheckValue(value.after_trip.opt2.status ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT3 = retCheckValue(value.after_trip.opt3.status ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT2_REMARK = value.after_trip.opt2.remark ?? "";
                            doc_head_search.DH_AFTER_TRIP_OPT3_REMARK = value.after_trip.opt3.remark ?? "";
                            doc_head_search.DH_EXPENSE_OPT1 = retCheckValue(value.checkbox_1.ToString() ?? "");
                            doc_head_search.DH_EXPENSE_OPT2 = retCheckValue(value.checkbox_2.ToString() ?? "");

                            //DevFix 20210723 0000  ปิดในส่วน remark tab2 เนื่องจาก tab3 ไม่มี remark ต้อง update 
                            //doc_head_search.DH_EXPENSE_REMARK = value.remark ?? "";

                            doc_head_search.DH_UPDATE_BY = user_id;
                            doc_head_search.DH_UPDATE_DATE = DateTime.Now;

                            if (value.action.type == "2") // reject
                            {
                                doc_status = "30";
                            }
                            else if (value.action.type == "3") // revise
                            {
                                doc_status = "23";
                            }
                            else if ((value.action.type == "4" || value.action.type == "5") && bLineApproverApproveAll)// approve
                            {
                                doc_status = "32";
                            }

                            var traveler_expen = context.BZ_DOC_TRAVELER_EXPENSE.Where(p => p.DH_CODE.Equals(value.doc_id)).ToList();
                            var traveler_approver_List = context.BZ_DOC_TRAVELER_APPROVER.Where(p => p.DTA_TYPE == "1" && p.DH_CODE.Equals(value.doc_id) && p.DTA_STATUS == 1).ToList();

                            var approverList = new List<SearchUserAllModel>();

                            foreach (var item in value.traveler_summary)
                            {
                                // รายการที่มีสิทธิ์ approve หรือไม่
                                if (item.take_action != "true")
                                    continue;

                                //********************************************************
                                var item_ref_id = item.ref_id ?? "";
                                var item_traverler_id = item.traverler_id ?? "";
                                var item_appr_id = item.appr_id ?? "";

                                var action_remark = chkString(value.action.remark);

                                //DevFix 20211018 0000 กรณีที่กดปึ่ม reject ให้ opt = false
                                var item_appr_status = item.appr_status ?? "";
                                if (doc_status == "30") { item_appr_status = "false"; }

                                //DevFix 20210817 0000 update ข้อมูล  remark ที่เกิดจากการกดปุ่ม reject
                                var appr_remark = chkString(item.appr_remark) ?? action_remark;

                                //ตรวจสอบว่ามีการ Line Approver Approve All หรือยัง  ของ Traveler นั้นๆ   
                                var bTravelerApproverApproveAll = false;
                                var lineApproveTravelerList = context.BZ_DOC_TRAVELER_APPROVER
                                .Where(p =>
                                    p.DH_CODE.Equals(value.doc_id) &&
                                    p.DTA_TYPE == "1" &&
                                    p.DTA_TRAVEL_EMPID == item_traverler_id &&
                                    !(p.DTA_APPR_STATUS.ToLower() == "true") &&
                                    p.DTA_APPR_EMPID != user_id
                                ).Count();

                                //กรณีที่มีข้อมูลยังไม่ line approve ยังไม่ครับ
                                bTravelerApproverApproveAll = (lineApproveTravelerList > 0 ? false : true);

                                if (bTravelerApproverApproveAll)
                                {
                                    // Update ข้อมูล Expense กรณีที่มีการ approve คนสุดท้ายของ line approver ของ traverler นั้นๆ
                                    sql = "update BZ_DOC_TRAVELER_EXPENSE set DTE_APPR_OPT= :item_appr_status ";
                                    sql += ", DTE_APPR_REMARK= :appr_remark ";
                                    if (!string.IsNullOrEmpty(doc_status))
                                    {
                                        sql += " , DTE_APPR_STATUS = :doc_status ";
                                    }
                                    sql += " where DTE_TOKEN = :item_ref_id ";
                                    sql += " and DTE_TOKEN = :item_ref_id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                    parameters.Add(context.ConvertTypeParameter("appr_remark", appr_remark, "char"));
                                    if (!string.IsNullOrEmpty(doc_status))
                                    {
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                    }
                                    parameters.Add(context.ConvertTypeParameter("item_ref_id", item_ref_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }
                                }

                                var row_check = traveler_expen.Where(p => p.DTE_TOKEN.Equals(item.ref_id)).ToList().FirstOrDefault();
                                if (row_check != null && row_check.DTE_EMP_ID != null)
                                {
                                    var travel_emp_id_select = row_check.DTE_EMP_ID ?? "";

                                    if (value.action.type == "2") // reject
                                    {
                                        sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                        sql += " DTA_APPR_STATUS= :item_appr_status "; // true, false 
                                        sql += " , DTA_APPR_REMARK= :action_remark ";
                                        sql += " , DTA_DOC_STATUS= :doc_status";
                                        sql += " , DTA_ACTION_STATUS = '5' ";
                                        sql += " where dh_code = :doc_id ";
                                        sql += " and DTA_TYPE = '1' and DTA_STATUS = 1";
                                        sql += " and DTA_TRAVEL_EMPID= :travel_emp_id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("travel_emp_id", travel_emp_id_select, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }

                                    }
                                    else if (value.action.type == "3") // revise
                                    {
                                        doc_status = "23";

                                        var action_status = (item_appr_status == "true" ? "4" : "5");

                                        sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                        sql += " DTA_APPR_STATUS= :item_appr_status "; // true, false 
                                        sql += " , DTA_APPR_REMARK= :action_remark ";
                                        sql += " , DTA_DOC_STATUS= :doc_status ";
                                        sql += " , DTA_ACTION_STATUS = :action_status ";
                                        sql += " where dh_code = :doc_id ";
                                        sql += " and DTA_TYPE = '1' and DTA_STATUS = 1";
                                        sql += " and DTA_TRAVEL_EMPID= :travel_emp_id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_status", action_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("travel_emp_id", travel_emp_id_select, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }


                                    }
                                    else if ((value.action.type == "4" || value.action.type == "5")) // approve
                                    {
                                        string user_id_select = user_id ?? "";

                                        #region DevFix 20211012 0000  item.ref_id เทียบได้กับ emp_id แต่เนื่องจาก traverler 1 คนมีได้มากกว่า 1 location ทำให้ข้อมูลผิด

                                        var row_check_3Emp = value.traveler_summary.Where(p =>
                                        p.appr_id == item.appr_id
                                        && p.traverler_id == item.traverler_id && p.appr_status != "false").ToList();
                                        if (row_check_3Emp != null && row_check_3Emp.Count > 0)
                                        {
                                            sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                            sql += " DTA_APPR_STATUS='true' ";
                                            sql += " , DTA_DOC_STATUS= :doc_status ";
                                            sql += " , DTA_APPR_REMARK= ''";
                                            sql += " , DTA_ACTION_STATUS = '3' ";
                                            sql += " where dh_code = :doc_id ";
                                            sql += " and DTA_TYPE = '1' and DTA_STATUS = 1";
                                            sql += " and DTA_TRAVEL_EMPID= :item_traverler_id ";
                                            sql += " and DTA_APPR_EMPID= :item_appr_id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("item_traverler_id", item_traverler_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("item_appr_id", item_appr_id, "char"));
                                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            if (iret > -1) { }

                                        }
                                        else
                                        {

                                            var action_status = (item_appr_status == "true" ? "3" : "5");
                                            var approver_id_select = (user_role == "1" ? "" : user_id_select);

                                            sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                            sql += " DTA_APPR_STATUS= :item_appr_status "; // true, false 
                                            sql += " , DTA_DOC_STATUS= :doc_status";
                                            sql += " , DTA_APPR_REMARK= ''";
                                            sql += " , DTA_ACTION_STATUS = :action_status ";
                                            sql += " where dh_code = :doc_id ";
                                            sql += " and DTA_TYPE = '1' and DTA_STATUS = 1";
                                            sql += " and DTA_TRAVEL_EMPID= :travel_emp_id ";

                                            //DevFix 2020902 2336 เพิ่มเงื่อนไข approver
                                            if (user_role != "1")
                                            {
                                                sql += " and DTA_APPR_EMPID= :approver_id_select ";
                                            }

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("action_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("travel_emp_id", travel_emp_id_select, "char"));
                                            parameters.Add(context.ConvertTypeParameter("approver_id_select", approver_id_select, "char"));
                                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            if (iret > -1) { }

                                        }

                                        #endregion DevFix 20211012 0000  item.ref_id เทียบได้กับ emp_id แต่เนื่องจาก traverler 1 คนมีได้มากกว่า 1 location ทำให้ข้อมูลผิด 
                                    }

                                    //DevFix 20210618 0000 เพิ่มข้อมูล  dta_update_date
                                    sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                    sql += " DTA_UPDATE_DATE = sysdate ";
                                    sql += " where dh_code = :doc_id ";
                                    sql += " and DTA_TYPE = '1' and DTA_STATUS = 1";
                                    sql += " and DTA_TRAVEL_EMPID= :travel_emp_id ";

                                    //DevFix 2020902 2336 เพิ่มเงื่อนไข approver
                                    if (user_role != "1")
                                    {
                                        sql += " and DTA_APPR_EMPID= :approver_id_select ";
                                    }

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("travel_emp_id", travel_emp_id_select, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("approver_id_select", user_id, "char"));
                                    }
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }


                                    var findData = traveler_approver_List.Where(p => p.DTA_TYPE == "1" && p.DTA_STATUS == 1
                                                                            && p.DTA_TRAVEL_EMPID.Equals(row_check.DTE_EMP_ID)).ToList();
                                    //DevFix 2020902 2336 เพิ่มเงื่อนไข approver
                                    if (user_role != "1")
                                    {
                                        findData = traveler_approver_List.Where(p => p.DTA_TYPE == "1" && p.DTA_STATUS == 1
                                                                              && p.DTA_TRAVEL_EMPID.Equals(row_check.DTE_EMP_ID)
                                                                              && p.DTA_APPR_EMPID.Equals(user_id)
                                                                              ).ToList();
                                    }
                                    if (findData.Count() > 1)
                                    {
                                        foreach (var ifindData in findData)
                                        {
                                            var check_data = approverList.SingleOrDefault(a => a.user_id == ifindData.DTA_APPR_EMPID);
                                            if (check_data == null)
                                            {
                                                approverList.Add(new SearchUserAllModel
                                                {
                                                    user_id = ifindData.DTA_APPR_EMPID ?? ""
                                                });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (findData != null && findData.Count() > 0)
                                        {
                                            approverList.Add(new SearchUserAllModel
                                            {
                                                user_id = findData[0].DTA_APPR_EMPID ?? ""
                                            });
                                        }
                                    }

                                }

                            }

                            var user_id_def = user_id ?? "";
                            if (user_id_def != "")
                            {
                                if (value.action.type == "2") // reject
                                {
                                    foreach (var item in approverList)
                                    {
                                        var item_user_id = item.user_id ?? "";

                                        sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id , ACTION_STATUS=2 ";
                                        sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                        sql += " and DOC_STATUS = 31 and ACTION_STATUS=1 ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                        parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }

                                    }
                                    if (user_role == "1") // admin
                                    {
                                        sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id , ACTION_STATUS=2 ";
                                        sql += " where dh_code= :doc_id and EMP_ID='admin' ";
                                        sql += " and DOC_STATUS = 31 and ACTION_STATUS=1 ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }
                                    }

                                    #region DevFix 20210726 0000 กรณีที่ Line reject เป็นคนเดียวกันกับ CAP ให้ update ACTION_STATUS = 2 ในตาราง BZ_DOC_ACTION ของ CAP   

                                    //approverList ข้อมูล 
                                    foreach (var item in approverList)
                                    {
                                        var item_user_id = item.user_id ?? "";

                                        sql = " update BZ_DOC_ACTION set ACTION_STATUS = 2 where ( select case when  ";
                                        sql += " (select count(1) as x from BZ_DOC_ACTION where dh_code = :doc_id and doc_status = 31 and  emp_id = :item_user_id ) > 0";
                                        sql += " and";
                                        sql += " (select count(1) as x from BZ_DOC_ACTION where dh_code = :doc_id and doc_status = 41 and  emp_id = :item_user_id ) > 0 ";
                                        sql += " then 1 else 0 end check_2_level from dual ) > 0 ";
                                        sql += " and dh_code= :doc_id and EMP_ID= :item_user_id ";
                                        sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }
                                    }
                                    #endregion DevFix 20210726 0000 กรณีที่ Line reject เป็นคนเดียวกันกับ CAP ให้ update ACTION_STATUS = 2 ในตาราง BZ_DOC_ACTION ของ CAP   

                                }
                                else if (value.action.type == "3") // revise
                                {
                                    foreach (var item in approverList)
                                    {
                                        var item_user_id = item.user_id ?? "";

                                        sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id , ACTION_STATUS=2 ";
                                        sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                        sql += " and DOC_STATUS = 31 and ACTION_STATUS=1 ";


                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }
                                    }

                                    if (user_role == "1") // admin
                                    {
                                        sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id, ACTION_STATUS=2 ";
                                        sql += " where dh_code= :doc_id and EMP_ID='admin' ";
                                        sql += " and DOC_STATUS = 31 and ACTION_STATUS=1 ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }
                                    }

                                    sql = "delete from BZ_DOC_ACTION where DH_CODE = :doc_id and DOC_STATUS=23 and EMP_ID='admin' and ACTION_STATUS=1 ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }

                                    // เรื่องจะถูกส่งกลับไปที่ admin
                                    var token_action_new = Guid.NewGuid().ToString() ?? "";
                                    var doc_head_type = doc_head_search.DH_TYPE ?? "";
                                    sql = "insert into BZ_DOC_ACTION (DA_TOKEN, DH_CODE, DOC_TYPE, DOC_STATUS, EMP_ID, TAB_NO, CREATED_DATE, UPDATED_DATE) ";
                                    sql += " values (:token_action_new , :doc_id, :doc_head_type, :doc_status, 'admin', 2, sysdate, sysdate ) ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("token_action_new", token_action_new, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_head_type", doc_head_type, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }

                                }
                                else if (value.action.type == "4" || value.action.type == "5") // approve
                                {
                                    foreach (var item in approverList)
                                    {
                                        var item_user_id = item.user_id ?? "";

                                        sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id, ACTION_STATUS=2 ";
                                        sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                        sql += " and DOC_STATUS = 31 and ACTION_STATUS=1 ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }
                                    }
                                    if (user_role == "1") // ถ้าเป็น admin
                                    {
                                        sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id, ACTION_STATUS=2 ";
                                        sql += " where dh_code= :doc_id and EMP_ID='admin' ";
                                        sql += " and DOC_STATUS = 31 and ACTION_STATUS=1 ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { };
                                    }

                                    #region DevFix 20201021 0000 กรณีที่ Line approve เป็นคนเดียวกันกับ CAP ให้ update ACTION_STATUS = 2 ในตาราง BZ_DOC_ACTION ของ CAP   
                                    //กรณีที่เป็น Line Apporve
                                    if (doc_status == "32")
                                    {
                                        //approverList ข้อมูล 
                                        foreach (var item in approverList)
                                        {
                                            var item_user_id = item.user_id ?? "";

                                            sql = " update BZ_DOC_ACTION set ACTION_STATUS = 2 where ( select case when  ";
                                            sql += " (select count(1) as x from BZ_DOC_ACTION where dh_code = :doc_id and doc_status = 31 and  emp_id = :item_user_id ) > 0";
                                            sql += " and";
                                            sql += " (select count(1) as x from BZ_DOC_ACTION where dh_code = :doc_id and doc_status = 41 and  emp_id = :item_user_id ) > 0 ";
                                            sql += " then 1 else 0 end check_2_level from dual ) > 0 ";
                                            sql += " and dh_code= :doc_id and EMP_ID= :item_user_id ";
                                            sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            if (iret > -1) { };

                                        }
                                    }
                                    #endregion DevFix 20201021 0000 กรณีที่ Line approve เป็นคนเดียวกันกับ CAP ให้ update ACTION_STATUS = 2 ในตาราง BZ_DOC_ACTION ของ CAP   
                                }
                            }

                            context.SaveChanges();
                            transaction.Commit();
                            data.status = "S";
                            data.message = "";

                            #region DevFix 20210714 0000 กรณีที่ Line Action ครบทุกคนแล้วให้ update status ของ admin = 2  
                            sql = @" select count(1) as status_value, '' as emp_id, '' as doc_status
                                     from BZ_DOC_TRAVELER_APPROVER a
                                     where a.dta_type = 1 and a.dta_action_status in (1,2)
                                     and a.dh_code = :doc_id  ";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            var dataApporver_Def = context.AllApproveModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                            if (dataApporver_Def != null && dataApporver_Def?.Count > 0)
                            {
                                if (dataApporver_Def[0].status_value == 0)
                                {
                                    sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id , ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID='admin' ";
                                    sql += " and DOC_STATUS = 31 and ACTION_STATUS=1 ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { };

                                    context.SaveChanges();
                                }
                            }
                            #endregion DevFix 20210714 0000 กรณีที่ Line Action ครบทุกคนแล้วให้ update status ของ admin = 2


                            //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW 
                            bool apprAllStatus = false;
                            string ret_doc_status = doc_status;

                            if (value.action.type == "4" || value.action.type == "5" || value.action.type == "2")
                            {

                                apprAllStatus = AllApproveLineApprover(doc_id, ref ret_doc_status);
                                if (apprAllStatus)
                                {
                                    sql = @" update BZ_DOC_TRAVELER_EXPENSE  
                                             set DTE_CAP_APPR_STATUS = 
                                             case when DTE_APPR_STATUS = 32 and DTE_APPR_OPT = 'true' then 41 else 
                                                case when DTE_APPR_STATUS = 32 and DTE_APPR_OPT = 'false' then 40 else (case when DTE_APPR_STATUS = 30 then 40 end) end
                                             end
                                             ,DTE_CAP_APPR_OPT = 
                                             case when DTE_APPR_STATUS = 32 and DTE_APPR_OPT = 'true' then null else 
                                                case when DTE_APPR_STATUS = 32 and DTE_APPR_OPT = 'false' then 'false' else (case when DTE_APPR_STATUS = 30 then 'false' end) end
                                             end
                                             where DH_CODE = :doc_id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { };

                                    context.SaveChanges();
                                }


                                //กรณีที่ Line Approver กับ CAP Approver เป็นคนเดียวกันให้ Stamp Approve ได้เลย
                                foreach (var apprlist in traveler_approver_List)
                                {
                                    var item_travel_empid = apprlist.DTA_TRAVEL_EMPID ?? "";
                                    var item_approver_empid = apprlist.DTA_APPR_EMPID ?? "";

                                    sql = @" update BZ_DOC_TRAVELER_EXPENSE set 
                                                     DTE_CAP_APPR_OPT = DTE_APPR_OPT 
                                                     ,DTE_CAP_APPR_STATUS = replace(DTE_APPR_STATUS,3,4)  
                                                     where (
                                                     select to_char(count(dta_type)) as action_status 
                                                     from (select distinct dta_type
                                                     from bz_doc_traveler_approver b
                                                     where  dh_code = :doc_id and dta_travel_empid = :item_travel_empid  and dta_appr_empid = :item_approver_empid )t )>1 " +
                                         @" and  DH_CODE = :doc_id and DTE_EMP_ID = :item_travel_empid ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_travel_empid", item_travel_empid, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_approver_empid", item_approver_empid, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { };

                                    context.SaveChanges();

                                }


                            }


                            #region "#### SEND MAIL ####" 
                            write_log_mail("0-email.start-submitFlow3", "type_flow :" + type_flow + " =>value.action.type :" + value.action.type + " =>apprAllStatus :" + apprAllStatus);

                            if (type_flow == true)
                            {
                                //DevFix 20200910 0727 เพิ่มแนบ link Ebiz ด้วย Link ไปหน้า login  
                                string url_login = LinkLogin;
                                string sDear = "";
                                string sDetail = "";
                                string sTitle = "";
                                string sBusinessDate = "";
                                string sLocation = "";
                                string sTravelerList = "";
                                string sReasonRejected = "";

                                if (value.action.type == "2") // reject
                                {
                                    #region traveler mail in doc
                                    sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name, b.email as email    
                                             , b.employeeid as emp_id, b.orgname as orgname                   
                                             from bz_doc_traveler_approver a
                                             inner join bz_users b on a.dta_travel_empid = b.employeeid
                                             left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s 
                                             on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id
                                             where a.dta_type = 1 and a.dta_action_status in (5) and a.dta_doc_status = 30
                                             and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += @" and a.dta_appr_empid = :user_id_def ";
                                    }
                                    sql += @" order by s.id ";

                                    traveler_mail = "";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var tempTravel = context.TempEmployeeProfileModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                    if (tempTravel != null)
                                    {
                                        foreach (var item in tempTravel)
                                        {
                                            traveler_mail += ";" + item.name;
                                        }
                                    }
                                    #endregion traveler mail in doc
                                    #region approver mail in doc 
                                    sql = @" select distinct b.employeeid as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 1 and a.dta_action_status in (5) and a.dta_doc_status = 40
                                                 and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += " and a.dta_appr_empid = :user_id_def ";
                                    }
                                    approver_mail = "";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (approvermailList != null)
                                    {
                                        if (approvermailList.Count > 0)
                                        {
                                            //approver_mail = ";" + approvermailList[0].email; 
                                            for (int m = 0; m < approvermailList.Count; m++)
                                            {
                                                //if (approver_mail != "") { approver_mail += ";"; }
                                                approver_mail += ";" + approvermailList[m].email;
                                            }
                                        }
                                    }
                                    #endregion approver mail in doc

                                    #region "#### SEND MAIL ####" 
                                    try
                                    {
                                        sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                        sBusinessDate = "Business Date : ";
                                        if (doc_head_search.DH_BUS_FROMDATE != null)
                                        {
                                            sBusinessDate = "Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                        }

                                        sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a 
                                                     inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                    WHERE a.dh_code = :doc_id and a.dte_status = 1 ";
                                        sql += " and a.dte_appr_status  = :doc_status order by s.id";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));

                                        var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                        if (temp != null && temp.Count() > 0)
                                        {
                                            //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                            if (temp.Count == 1)
                                            {
                                                sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            }
                                            else
                                            {
                                                sLocation = "";
                                                foreach (var item in temp)
                                                {
                                                    if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                    sLocation += item.name1 + "/" + item.name2;
                                                }
                                            }
                                        }

                                        sendEmailService mail = new sendEmailService();
                                        sendEmailModel dataMail = new sendEmailModel();

                                        var doc_head_create_by = doc_head_search.DH_CREATE_BY ?? "";
                                        sql = "SELECT   b.employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                        sql += "FROM bz_users b WHERE employeeid= :doc_head_create_by";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_head_create_by", doc_id, "char"));
                                        var requestor = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        //to : Requester, Traverler
                                        //cc : Super admin, Line Approval   
                                        dataMail.mail_to = requester_mail + traveler_mail;
                                        dataMail.mail_cc = approver_mail + on_behalf_of_mail + admin_mail;


                                        dataMail.mail_subject = value.doc_id + " :  The request for business travel has been rejected.";// + login_empid[0].user_name + "";

                                        //sDear = "Dear " + requestor[0].user_name + ",";
                                        sDear = "Dear All,";

                                        sDetail = $"Your business travel request has been reject by {user_name}. To view the details, click ";
                                        sDetail += $"<a href='{(LinkLogin + "ii").Replace("###", value.doc_id)}'>{doc_id}</a>";

                                        var iNo = 1;
                                        if (tempTravel != null && tempTravel?.Count > 0)
                                        {
                                            sTravelerList = "<table>";
                                            foreach (var item in tempTravel)
                                            {
                                                sTravelerList += " <tr>";
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name + "</span></font></td>";//1) [Title_Name of traveler] 
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.emp_id + "</span></font></td>";//Emp. ID
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.position + "</span></font></td>";//SEC./DEP./FUNC. 
                                                sTravelerList += " </tr>";
                                                iNo++;
                                            }
                                            sTravelerList += "</table>";
                                        }

                                        #region set mail
                                        try
                                        {
                                            sReasonRejected = "";
                                            if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                            else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                        }
                                        catch { }

                                        dataMail.mail_body = @"<span lang='en-US'>";
                                        dataMail.mail_body += "<div>";
                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                        dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        if (sReasonRejected != "")
                                        {
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                            dataMail.mail_body += "     <br>";
                                        }
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Best Regards,";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "</div>";
                                        dataMail.mail_body += "</span>";

                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                        dataMail.doc_id = doc_id;

                                        mail.sendMail(dataMail);
                                        #endregion set mail

                                    }
                                    catch (Exception ex)
                                    {
                                        write_log_mail("88-email.message-submitFlow3", "error-reject" + ex.ToString());
                                    }
                                    #endregion
                                }
                                if (value.action.type == "4" || value.action.type == "5" || value.action.type == "2")
                                {
                                    if (apprAllStatus)
                                    {
                                        //DevFix 20210804 0000 กรณีที่ Line & CAP คนเดียวกัน และปิดใบงานเรียบร้อย
                                        //ตรวจสอบ Status 
                                        string status_next_step_def = "41";
                                        #region get status 
                                        try
                                        {
                                            sql = @" select '' as id, to_char(DH_DOC_STATUS) as name1, '' as name2, '' as name3, '' as name4 from BZ_DOC_HEAD a where a.DH_CODE = :doc_id  ";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempStatus_Def = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                            if (tempStatus_Def != null && tempStatus_Def.Count() > 0)
                                            {
                                                status_next_step_def = tempStatus_Def[0].name1.ToString();
                                            }
                                        }
                                        catch { }
                                        #endregion get status 

                                        if (status_next_step_def == "50")
                                        {
                                            //DevFix 20211021 0000 เพื่อนำไปใช้ในการแจ้งเตือน 028_OB/LB/OT/LT : Business Travel Confirmation Letter
                                            Set_Trip_Complated(context, value.token_login, value.doc_id);


                                            #region traveler mail in doc 
                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                                     , b.employeeid as name3, b.orgname as name4                      
                                                     from bz_doc_traveler_approver a
                                                     inner join bz_users b on a.dta_travel_empid = b.employeeid left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                                     on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id  
                                                     where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42
                                                     and a.dh_code = :doc_id order by s.id ";

                                            traveler_mail = "";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (tempTravel != null)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    traveler_mail += ";" + item.name2;
                                                }
                                            }
                                            #endregion traveler mail in doc


                                            #region approver mail in doc 
                                            approver_mail = "";

                                            sql = @" select distinct b.employeeid as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42
                                                 and a.dh_code = :doc_id ";
                                            sql += @" union ";
                                            sql += @" select distinct b.employeeid as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 1 and a.dta_action_status in (3) and a.dta_doc_status = 32
                                                 and a.dh_code = :doc_id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (approvermailList != null)
                                            {
                                                if (approvermailList.Count > 0)
                                                {
                                                    for (int m = 0; m < approvermailList.Count; m++)
                                                    {
                                                        //if (approver_mail != "") { approver_mail += ";"; }
                                                        approver_mail += ";" + approvermailList[m].email;
                                                    }
                                                }
                                            }
                                            #endregion approver mail in doc

                                            #region "#### SEND MAIL ####"  
                                            try
                                            {
                                                sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                                sBusinessDate = "Business Date : ";
                                                if (doc_head_search.DH_BUS_FROMDATE != null)
                                                {
                                                    sBusinessDate = "Business Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                                }

                                                sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a 
                                                     inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                      left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                    WHERE a.dh_code = :doc_id and a.dte_status = 1 ";
                                                sql += " and a.dte_appr_status  = :status_next_step_def order by s.id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("status_next_step_def", status_next_step_def, "char"));
                                                var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (temp != null && temp.Count() > 0)
                                                {
                                                    //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }

                                                sendEmailService mail = new sendEmailService();
                                                sendEmailModel dataMail = new sendEmailModel();

                                                //to : Super admin, Requester, Traveller 
                                                //cc : CAP Approval, Line Approval 
                                                dataMail.mail_to = admin_mail + traveler_mail + approver_mail + on_behalf_of_mail + on_behalf_of_mail;
                                                dataMail.mail_cc = "";// approver_mail;
                                                dataMail.mail_subject = value.doc_id + " : The request for business travel has been approved.";
                                                //Attached: Approval / Output form
                                                var file_Approval_Output_form = file_name_approval(value.doc_id, value.token_login);
                                                if (file_Approval_Output_form != "")
                                                {
                                                    string file_name = file_Approval_Output_form;// @"temp\APPROVAL_FORM_OT21060025_2021100410233333.xlsx";

                                                    //string _FolderMailAttachments = System.Configuration.ConfigurationManager.AppSettings["FilePathServerApp"].ToString();//d:\Ebiz2\Ebiz_App\
                                                    string _FolderMailAttachments = configApp.GetStringFromAppSettings("FilePathServerApp") ?? "";

                                                    string mail_attachments = _FolderMailAttachments + file_name;
                                                    dataMail.mail_attachments = mail_attachments;
                                                }

                                                sDear = "Dear All,";

                                                sDetail = "The request for business travel has been approved. To view the approval details, click ";
                                                sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";
                                                sDetail += "<br>";
                                                sDetail += "Any additional arrangements require to complete by the traveler. To view travel details, click ";
                                                sDetail += "<a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id) + "'>travel details.</a>";

                                                var iNo = 1;
                                                sTravelerList = "<table>";
                                                foreach (var item in tempTravel)
                                                {
                                                    sTravelerList += " <tr>";
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                    sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                    sTravelerList += " </tr>";
                                                    iNo++;
                                                }
                                                sTravelerList += "</table>";

                                                #region set mail
                                                try
                                                {
                                                    sReasonRejected = "";
                                                    //if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                    //else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                                }
                                                catch { }

                                                dataMail.mail_body = @"<span lang='en-US'>";
                                                dataMail.mail_body += "<div>";
                                                dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                if (sReasonRejected != "")
                                                {
                                                    dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                    dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                    dataMail.mail_body += "     <br>";
                                                }
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                                dataMail.mail_body += "     </span></font></div>";

                                                dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     Best Regards,";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                                dataMail.mail_body += "     </span></font></div>";

                                                dataMail.mail_body += "</div>";
                                                dataMail.mail_body += "</span>";

                                                var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                dataMail.doc_id = doc_id;

                                                mail.sendMail(dataMail);
                                                #endregion set mail


                                            }
                                            catch (Exception ex)
                                            {
                                                write_log_mail("88-email.message-submitFlow3", "error-sub" + ex.ToString());
                                            }

                                            #endregion

                                            if (notiPassport)
                                            {
                                                #region "#### SEND MAIL ####" 017_OB/LB/OT/LT : Please update Passport information - [Title_Name of traveler]
                                                try
                                                {
                                                    //ส่งตอน CAP Approve แล้วและตรวจสอบได้ว่าไม่มี valid passport เเละให้ส่ง E-Mail  
                                                    foreach (var itemTravel in tempTravel)
                                                    {
                                                        var bValidPassportExpire = true;
                                                        var traveler_id = itemTravel.name3;
                                                        var traveler_name = itemTravel.name1;
                                                        sql = @" select distinct emp_id as id, '' as name1, '' as name2, '' as name3, '' as name4  from bz_data_passport 
                                                     where default_type = 'true' and to_date(passport_date_expire,'dd Mon yyyy') >= sysdate
                                                     and emp_id = :traveler_id";

                                                        parameters = new List<OracleParameter>();
                                                        parameters.Add(context.ConvertTypeParameter("traveler_id", traveler_id, "char"));
                                                        var dataPassport = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                        if (dataPassport != null)
                                                        {
                                                            if (dataPassport.Count > 0) { bValidPassportExpire = false; }
                                                        }
                                                        if (bValidPassportExpire == false) { continue; }

                                                        sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4   
                                                     FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     WHERE a.dh_code = :doc_id and a.dte_status = 1 ";
                                                        sql += " and a.dte_cap_appr_status  = :doc_status ";
                                                        sql += " and a.DTE_Emp_Id = :traveler_id order by s.id ";

                                                        parameters = new List<OracleParameter>();
                                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                                        parameters.Add(context.ConvertTypeParameter("traveler_id", traveler_id, "char"));
                                                        var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();


                                                        if (temp != null && temp.Count() > 0)
                                                        {
                                                            //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                            //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                            if (temp.Count == 1)
                                                            {
                                                                sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                            }
                                                            else
                                                            {
                                                                sLocation = "";
                                                                foreach (var item in temp)
                                                                {
                                                                    if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                                    sLocation += item.name1 + "/" + item.name2;
                                                                }
                                                            }
                                                        }

                                                        sendEmailService mail = new sendEmailService();
                                                        sendEmailModel dataMail = new sendEmailModel();

                                                        //TO: Traveler
                                                        //CC : Admin - PMSV; Admin - PMDV(if any);
                                                        dataMail.mail_to = itemTravel.name2;
                                                        dataMail.mail_cc = admin_mail;// approver_mail + requester_mail + on_behalf_of_mail + traveler_mail_reject;
                                                        dataMail.mail_subject = value.doc_id + " : Please update Passport information - " + itemTravel.name1;

                                                        sDear = "Dear " + itemTravel.name1 + ",";

                                                        sDetail = "Your require to update passport information in order to make further arrangements. To view travel details, click ";
                                                        sDetail += "<a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id).Replace("travelerhistory", "passport") + "'>" + value.doc_id + "</a>";

                                                        var iNo = 1;
                                                        sTravelerList = "<table>";
                                                        //foreach (var item in tempTravel)
                                                        //{ 
                                                        sTravelerList += " <tr>";
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + itemTravel.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + itemTravel.name3 + "</span></font></td>";//Emp. ID
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + itemTravel.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                        sTravelerList += " </tr>";
                                                        iNo++;
                                                        //}
                                                        sTravelerList += "</table>";

                                                        #region set mail
                                                        try
                                                        {
                                                            sReasonRejected = "";
                                                            //if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                            //else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                                        }
                                                        catch { }
                                                        dataMail.mail_body = @"<span lang='en-US'>";
                                                        dataMail.mail_body += "<div>";
                                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                        dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        if (sReasonRejected != "")
                                                        {
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                            dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                            dataMail.mail_body += "     <br>";
                                                        }
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                                        dataMail.mail_body += "     </span></font></div>";

                                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     Best Regards,";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                                        dataMail.mail_body += "     </span></font></div>";

                                                        dataMail.mail_body += "</div>";
                                                        dataMail.mail_body += "</span>";

                                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                        dataMail.doc_id = doc_id;

                                                        mail.sendMail(dataMail);
                                                        #endregion set mail
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    write_log_mail("88-email.message-submitFlow3", "error-Travel" + ex.ToString());
                                                }
                                                #endregion "#### SEND MAIL ####" 017_OB/LB/OT/LT : Please update Passport information - [Title_Name of traveler]
                                            }
                                        }
                                        else
                                        {
                                            //to CAP

                                            #region "#### SEND MAIL ####"

                                            try
                                            {
                                                sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                                sBusinessDate = "Business Date : ";
                                                if (doc_head_search.DH_BUS_FROMDATE != null)
                                                {
                                                    sBusinessDate = "Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                                }

                                                sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a    
                                                     inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     WHERE a.dh_code = :doc_id and a.dte_status = 1 ";
                                                sql += " and a.dte_appr_status  = :doc_status order by s.id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                                var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (temp != null && temp.Count() > 0)
                                                {
                                                    //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }

                                                sendEmailService mail = new sendEmailService();
                                                sendEmailModel dataMail = new sendEmailModel();

                                                sql = @" select distinct a.dta_appr_empid as user_id
                                                         , nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name
                                                         , b.email email
                                                         from bz_doc_traveler_approver a left join bz_users b on a.dta_appr_empid = b.employeeid
                                                         where a.dh_code = :doc_id and a.dta_type = 2 and a.dta_action_status in (2) ";

                                                //DevFix 20210810 0000 กรณีที่ Line Approve แล้วให้ส่งไปยัง CAP ลำดับแรกของแต่ละ traveler
                                                sql += @" and (a.dta_appr_level, a.dta_travel_empid) in (
                                                         select min(dta_appr_level),dta_travel_empid from bz_doc_traveler_approver a1
                                                         where a1.dh_code = :doc_id and a1.dta_type = 2 and a1.dta_action_status in (2) group by dta_travel_empid )";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var empapp = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (empapp != null && empapp?.Count > 0)
                                                {

                                                    //20221025 =>test เนื่องจากกรณีที่ cap 1 รายการจะมีข้อมูล mail to 
                                                    var sql_check = sql;

                                                    foreach (var iemp in empapp)
                                                    {
                                                        sql_check += "=>user_id:" + iemp.user_id;
                                                        sql_check += "=>email:" + iemp.email;

                                                        #region approver mail in doc 
                                                        approver_mail = "";

                                                        sql = @" select distinct b.employeeid user_id, '' as user_name, b.email                       
                                                             from bz_doc_traveler_approver a
                                                             inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                             where a.dta_type = 1 and a.dta_action_status in (3) and a.dta_doc_status = 32
                                                             and a.dh_code = :doc_id ";

                                                        parameters = new List<OracleParameter>();
                                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                        var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                        if (approvermailList != null)
                                                        {
                                                            if (approvermailList.Count > 0)
                                                            {
                                                                for (int m = 0; m < approvermailList.Count; m++)
                                                                {
                                                                    //if (approver_mail != "") { approver_mail += ";"; }
                                                                    approver_mail += ";" + approvermailList[m].email;
                                                                }
                                                            }
                                                        }
                                                        #endregion approver mail in doc


                                                        #region traveler mail in doc
                                                        traveler_mail = "";

                                                        //DevFix 20210813 0000 กรณีที่ Line Approve แล้วให้ส่งไปยัง CAP 
                                                        //หา traveler ที่อยู่ ภายใต้ CAP 
                                                        sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 , '' as name3, '' as name4
                                                             , b.employeeid as name3, b.orgname as name4                      
                                                             from bz_doc_traveler_approver a
                                                             inner join bz_users b on a.dta_travel_empid = b.employeeid
                                                             left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                                             on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id 
                                                             where a.dta_type = 2 and (a.dta_doc_status in (41) and a.dta_action_status = 2)
                                                             and a.dh_code = :doc_id and a.dta_appr_empid = :item_approver_emp_id order by s.id";

                                                        var item_approver_emp_id = iemp.user_id ?? "";
                                                        parameters = new List<OracleParameter>();
                                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                        parameters.Add(context.ConvertTypeParameter("item_approver_emp_id", item_approver_emp_id, "char"));
                                                        var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                        if (tempTravel != null)
                                                        {
                                                            foreach (var item in tempTravel)
                                                            {
                                                                traveler_mail += ";" + item.name2;
                                                            }
                                                        }
                                                        #endregion traveler mail in doc
                                                        //to : CAP Approval
                                                        //cc : Line Approval, Super admin, Requester, Traveller

                                                        dataMail.mail_to = iemp.email ?? "";
                                                        dataMail.mail_cc = approver_mail + admin_mail + requester_mail + on_behalf_of_mail + traveler_mail;
                                                        dataMail.mail_subject = value.doc_id + " : Please approve business travel request as CAP.";

                                                        sDear = "Dear " + iemp.user_name + ",";

                                                        sDetail = "Please approve business travel request as CAP. To view the details, click ";
                                                        sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                                        var iNo = 1;
                                                        sTravelerList = "<table>";
                                                        foreach (var item in tempTravel)
                                                        {
                                                            sTravelerList += " <tr>";
                                                            sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                            sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                            sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                            sTravelerList += " </tr>";
                                                            iNo++;
                                                        }
                                                        sTravelerList += "</table>";

                                                        #region set mail
                                                        try
                                                        {
                                                            sReasonRejected = "";
                                                            //if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                            //else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                                        }
                                                        catch { }

                                                        dataMail.mail_body = @"<span lang='en-US'>";
                                                        dataMail.mail_body += "<div>";
                                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                        dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        if (sReasonRejected != "")
                                                        {
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                            dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                            dataMail.mail_body += "     <br>";
                                                        }
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                                        dataMail.mail_body += "     </span></font></div>";

                                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     Best Regards,";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                                        dataMail.mail_body += "     </span></font></div>";

                                                        dataMail.mail_body += "</div>";
                                                        dataMail.mail_body += "</span>";


                                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                        dataMail.doc_id = doc_id;

                                                        mail.sendMail(dataMail);
                                                        #endregion set mail

                                                    }
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                write_log_mail("88-email.message-submitFlow3", "error-approve to cap" + ex.ToString());
                                            }

                                            #endregion
                                        }

                                    }
                                }
                                else if (value.action.type == "3") // revise
                                {
                                    #region traveler mail in doc
                                    traveler_mail = "";
                                    sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2   
                                             , b.employeeid as name3, b.orgname as name4                    
                                             from bz_doc_traveler_approver a
                                             inner join bz_users b on a.dta_travel_empid = b.employeeid
                                             left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                             on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id  
                                             where a.dta_type = 1 and a.dta_action_status in (4) and a.dta_doc_status = 23
                                             and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += @" and a.dta_appr_empid = :user_id_def ";
                                    }
                                    sql += @" order by s.id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (tempTravel != null)
                                    {
                                        foreach (var item in tempTravel)
                                        {
                                            traveler_mail += ";" + item.name2;
                                        }
                                    }
                                    #endregion traveler mail in doc
                                    #region approver mail in doc 
                                    approver_mail = "";
                                    sql = @" select distinct b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 1 and a.dta_action_status in (4) and a.dta_doc_status = 23
                                                 and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += " and a.dta_appr_empid = :user_id_def ";
                                    }

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var approvermailList = context.TempEMailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (approvermailList != null)
                                    {
                                        if (approvermailList.Count > 0)
                                        {
                                            for (int m = 0; m < approvermailList.Count; m++)
                                            {
                                                approver_mail += ";" + approvermailList[m].email;
                                            }
                                        }
                                    }
                                    #endregion approver mail in doc

                                    #region "#### SEND MAIL ####" 
                                    try
                                    {
                                        sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                        sBusinessDate = "Business Date : ";
                                        if (doc_head_search.DH_BUS_FROMDATE != null)
                                        {
                                            sBusinessDate = "Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                        }
                                        if (doc_head_search.DH_TYPE == "local" ||
                                            doc_head_search.DH_TYPE == "localtraining")
                                        {
                                            //DevFix 20210713 0000 แก้ไขดึงข้อมูล Location จาก BZ_DOC_TRAVELER_EXPENSE.CITY_TEXT 
                                            sql = @" select distinct to_char(s.id) as id, a.CITY_TEXT name1, a.CITY_TEXT name2, '' as name3, '' as name4
                                                  from BZ_DOC_TRAVELER_EXPENSE a
                                                  left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                  ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                  and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                  where a.dh_code = :doc_id order by s.id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (temp != null && temp.Count() > 0)
                                            {
                                                //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                if (temp.Count == 1)
                                                {
                                                    sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                }
                                                else
                                                {
                                                    sLocation = "";
                                                    foreach (var item in temp)
                                                    {
                                                        if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                        sLocation += item.name1 + "/" + item.name2;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sql = @" select distinct to_char(s.id) as id, b.ct_name name1, c.ctn_name name2 , '' as name3, '' as name4
                                                         from BZ_DOC_COUNTRY a 
                                                         left join (select min(dte_id) as id, dh_code, ct_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ct_id) s on a.dh_code = s.dh_code and a.ct_id = s.ct_id  
                                                         left join BZ_MASTER_COUNTRY b on a.ct_id = b.ct_id
                                                         left join bz_master_continent c on b.ctn_id = c.ctn_id 
                                                         where a.dh_code = :doc_id and no = 1  order by s.id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (temp != null && temp.Count() > 0)
                                            {
                                                //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                if (temp.Count == 1)
                                                {
                                                    sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                }
                                                else
                                                {
                                                    sLocation = "";
                                                    foreach (var item in temp)
                                                    {
                                                        if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                        sLocation += item.name1 + "/" + item.name2;
                                                    }
                                                }
                                            }
                                        }

                                        sendEmailService mail = new sendEmailService();
                                        sendEmailModel dataMail = new sendEmailModel();

                                        sql = "SELECT b.employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                        sql += " FROM bz_users b WHERE employeeid= :user_id_def ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        var user_active_doc = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        var revise_user_display_name = "";
                                        if (user_active_doc != null && user_active_doc?.Count > 0)
                                        {
                                            revise_user_display_name = user_active_doc[0].user_name ?? "";
                                        }

                                        dataMail.mail_to = admin_mail ?? "";
                                        dataMail.mail_cc = requester_mail + on_behalf_of_mail;

                                        dataMail.mail_subject = value.doc_id + " :  The business travel request has been required to revise by " + revise_user_display_name + "";

                                        sDear = "Dear All,";

                                        sDetail = "Your business travel request has been revise by " + revise_user_display_name + ". To view the details, click ";
                                        sDetail += "<a href='" + (LinkLogin + "i").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                        var iNo = 1;
                                        if (tempTravel != null && tempTravel?.Count > 0)
                                        {
                                            sTravelerList = "<table>";
                                            foreach (var item in tempTravel)
                                            {
                                                sTravelerList += " <tr>";
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                sTravelerList += " </tr>";
                                                iNo++;
                                            }
                                            sTravelerList += "</table>";
                                        }
                                        #region set mail
                                        try
                                        {
                                            sReasonRejected = "";
                                            if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                            else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                        }
                                        catch { }

                                        dataMail.mail_body = @"<span lang='en-US'>";
                                        dataMail.mail_body += "<div>";
                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                        dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        if (sReasonRejected != "")
                                        {
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                            dataMail.mail_body += "     <br>";
                                        }
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Best Regards,";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "</div>";
                                        dataMail.mail_body += "</span>";

                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                        dataMail.doc_id = doc_id;

                                        mail.sendMail(dataMail);
                                        #endregion set mail


                                    }
                                    catch (Exception ex)
                                    {
                                        write_log_mail("88-email.message-submitFlow3", "error-revise" + ex.ToString());
                                    }
                                    #endregion
                                }
                            }

                            write_log_mail("99-email.end-submitFlow3", "");
                            #endregion "#### SEND MAIL ####" 

                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.status = "E";
                            data.message = ex.ToString();
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                data.status = "E";
                data.message = ex.Message;
            }

            return data;
        }

        public ResultModel submitFlow4(DocFlow3Model value)
        {
            int iResult = -1;
            decimal? decimalNull = null;
            bool newDocNo = false;
            string doc_status = "";

            string user_name = "";
            string user_id = "";
            string user_role = "";

            var data = new ResultModel();


            if (value.action == null || string.IsNullOrEmpty(value.action.type))
            {
                data.status = "E";
                data.message = "Action is null !";
                return data;
            }

            //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
            bool type_flow = true;

            //DevFix 2021105 0000 เพิ่มข้อมูล Any file attached in E-Biz system ใช้ใน mail ของ Trainging
            var sAny_file_attached = "";


            //oversea,overseatraining,localtraining,local 
            var doc_type = "oversea";
            Boolean notiPassport = false;


            try
            {
                var parameters = new List<OracleParameter>();
                var sql = "";
                var doc_id = value.doc_id ?? "";
                var token_login = value.token_login ?? "";
                var iret = 0;

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var doc_head_search = context.BZ_DOC_HEAD.Find(value.doc_id);
                    if (doc_head_search == null)
                    {
                        data.status = "E";
                        data.message = "not found data !";
                        return data;
                    }
                    else
                    {
                        doc_type = doc_head_search.DH_TYPE;
                        notiPassport = ((doc_type == "oversea" || doc_type == "overseatraining") ? true : false);
                    }

                    #region DevFix 20200911 0000 
                    var Tel_Services_Team = "";
                    var Tel_Call_Center = "";

                    getTelServicesTeamCallCenter(ref Tel_Services_Team, ref Tel_Call_Center);

                    #endregion DevFix 20200911 0000 

                    #region DevFix 20210527 0000 เพิ่มข้อมูลไฟล์แนบ

                    var imaxid_docfile = 1;
                    sql = @"SELECT to_char( nvl(max(DF_ID),0)+1) as DF_ID , null as DH_CODE, null as DF_NAME, null as DF_PATH, null as DF_REMARK  FROM  BZ_DOC_FILE ";
                    parameters = new List<OracleParameter>();
                    var maxid_docfile = context.DocFileListOutModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();


                    if (maxid_docfile != null && maxid_docfile.Count() > 0)
                    {
                        imaxid_docfile = Convert.ToInt32(maxid_docfile[0].DF_ID);
                    }
                    #endregion DevFix 20210527 0000 เพิ่มข้อมูลไฟล์แนบ

                    string admin_mail = "";
                    string requester_mail = "";
                    string requester_name = "";
                    string on_behalf_of_mail = "";
                    string traveler_mail = "";
                    string traveler_mail_reject = "";
                    string approver_mail = "";

                    #region DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin
                    admin_mail = get_mail_group_admin(context);

                    //กรณีที่เป็น pmdv admin, pmsv_admin
                    admin_mail += mail_group_admin(context, "pmsv_admin");
                    if (doc_id.IndexOf("T") > -1)
                    {
                        admin_mail += mail_group_admin(context, "pmdv_admin");
                    }
                    get_mail_requester_in_doc(context, doc_id, ref requester_name, ref requester_mail, ref on_behalf_of_mail);

                    #endregion DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin 

                    //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW 
                    if ((doc_head_search.DH_TYPE_FLOW ?? "1") != "1") { type_flow = false; }


                    using (var transaction = context.Database.BeginTransaction())
                    {
                        get_user_role_in_token_login(context, token_login, ref user_name, ref user_id, ref user_role);

                        if (doc_id.IndexOf("T") > -1)
                        {
                            var user_role_select = user_role ?? "";
                            user_role = get_role_admin_in_manage(context, user_id, user_role_select);
                        }

                        try
                        {
                            var user_id_def = user_id ?? "";

                            doc_head_search.DH_AFTER_TRIP_OPT1 = retCheckValue(value.after_trip.opt1 ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT2 = retCheckValue(value.after_trip.opt2.status ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT3 = retCheckValue(value.after_trip.opt3.status ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT2_REMARK = value.after_trip.opt2.remark ?? "";
                            doc_head_search.DH_AFTER_TRIP_OPT3_REMARK = value.after_trip.opt3.remark ?? "";
                            doc_head_search.DH_EXPENSE_OPT1 = retCheckValue(value.checkbox_1.ToString() ?? "");
                            doc_head_search.DH_EXPENSE_OPT2 = retCheckValue(value.checkbox_2.ToString() ?? "");

                            //DevFix 20210723 0000  ปิดในส่วน remark tab2 เนื่องจาก tab3 ไม่มี remark ต้อง update 
                            //doc_head_search.DH_EXPENSE_REMARK = value.remark ?? "";

                            doc_head_search.DH_UPDATE_BY = user_id;
                            doc_head_search.DH_UPDATE_DATE = DateTime.Now;

                            if (value.action.type == "2") // reject
                                doc_status = "40";
                            else if (value.action.type == "3") // revise
                                doc_status = "23";
                            else if (value.action.type == "4" || value.action.type == "5") // approve
                                doc_status = "42";

                            if (string.IsNullOrEmpty(doc_status)) { doc_status = "41"; }//save

                            var traveler_expen = context.BZ_DOC_TRAVELER_EXPENSE.Where(p => p.DH_CODE.Equals(value.doc_id)).ToList();
                            var traveler_approver_List = context.BZ_DOC_TRAVELER_APPROVER.Where(p => p.DH_CODE.Equals(value.doc_id) && p.DTA_STATUS == 1).ToList();
                            var approverList = new List<SearchUserAllModel>();

                            #region DevFix 20211012 0000  item.ref_id เทียบได้กับ emp_id แต่เนื่องจาก traverler 1 คนมีได้มากกว่า 1 location ทำให้ข้อมูลผิด 
                            var sbCheckApprovList = "false";

                            #endregion DevFix 20211012 0000  item.ref_id เทียบได้กับ emp_id แต่เนื่องจาก traverler 1 คนมีได้มากกว่า 1 location ทำให้ข้อมูลผิด


                            foreach (var item in value.traveler_summary)
                            {
                                sql = "";
                                if (item.take_action != "true")
                                    continue;

                                //ตรวจสอบว่า ถ้าเป็นการ approve traveler 1 คน แล้วมีการยกเลิก 1 รายการ อนุมัติ 1 รายการ ต้อง stamp เป็น อนุมัติ ในตาราง BZ_DOC_TRAVELER_APPROVER
                                //ตรวจสอบว่า เป็น traveler 1 คน แล้วมีมากกว่า 1 รายการหรือไม่
                                var bApprover_type = false; //กรณีที่เป็น approver คนเดียว
                                var traverler_check = traveler_expen.Where(p => p.DTE_TOKEN.Equals(item.ref_id)).ToList().FirstOrDefault();
                                if (traverler_check != null && traverler_check.DTE_EMP_ID != null)
                                {
                                    var traverler_list = traveler_expen.Where(p => p.DTE_EMP_ID.Equals(traverler_check.DTE_EMP_ID)).ToList();
                                    if (traverler_list != null && traverler_list.Count > 1)
                                    {
                                        bApprover_type = true;
                                        sbCheckApprovList = "true";
                                    }
                                }

                                foreach (var item2 in value.traveler_summary)
                                {    // รายการที่มีสิทธิ์ approve หรือไม่
                                    if (item.take_action != "true")
                                        continue;

                                }

                                var action_remark = chkString(value.action.remark) ?? "";

                                var item_ref_id = item.ref_id ?? "";
                                var item_appr_status = item.appr_status ?? "";
                                var item_appr_remark = chkString(item.appr_remark) ?? "";
                                var cap_approver_opt = (doc_status == "40" ? "false" : item_appr_status);
                                var cap_approver_remark = (action_remark == "" ? action_remark : item_appr_remark);

                                sql = "update BZ_DOC_TRAVELER_EXPENSE set ";
                                //DevFix 20211018 0000 กรณีที่กดปึ่ม reject ให้ opt = false 
                                sql += " DTE_CAP_APPR_OPT= :cap_approver_opt ";
                                //DevFix 20210817 0000 update ข้อมูล  remark ที่เกิดจากการกดปุ่ม reject
                                sql += ", DTE_CAP_APPR_REMARK= :cap_approver_remark ";
                                sql += " , DTE_CAP_APPR_STATUS = :doc_status ";
                                sql += " where DTE_TOKEN = :item_ref_id ";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("cap_approver_opt", cap_approver_opt, "char"));
                                parameters.Add(context.ConvertTypeParameter("cap_approver_remark", cap_approver_remark, "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                parameters.Add(context.ConvertTypeParameter("item_ref_id", item_ref_id, "char"));
                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                if (iret > -1) { }

                                var row_check = traveler_expen.Where(p => p.DTE_TOKEN.Equals(item.ref_id)).ToList().FirstOrDefault();
                                if (row_check != null && row_check.DTE_EMP_ID != null)
                                {
                                    var travel_emp_id_select = row_check.DTE_EMP_ID ?? "";
                                    if (value.action.type == "2") // reject
                                    {
                                        //doc_status = "30";
                                        sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                        sql += " DTA_APPR_STATUS= :item_appr_status "; // true, false 
                                        sql += " , DTA_APPR_REMARK= :action_remark ";
                                        sql += " , DTA_DOC_STATUS= :doc_status";
                                        sql += " , DTA_ACTION_STATUS = '5' ";
                                        sql += " where dh_code = :doc_id ";
                                        sql += " and DTA_TYPE = '2' and DTA_STATUS = 1";
                                        sql += " and DTA_TRAVEL_EMPID= :travel_emp_id_select ";

                                        //DevFix 20200827 1640 แก้ไขเพิ่มเงื่อนไข EMPID CAP ที่ action 
                                        if (user_role != "1")
                                        {
                                            sql += " and DTA_APPR_EMPID = :user_id_def ";
                                        }

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("travel_emp_id_select", travel_emp_id_select, "char"));
                                        if (user_role != "1")
                                        {
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        }
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }
                                    }
                                    else if (value.action.type == "3") // revise

                                    {
                                        doc_status = "23";
                                        var action_status_select = (item_appr_status == "true" ? "4" : "5");

                                        sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                        sql += " DTA_APPR_STATUS= :item_appr_status "; // true, false 
                                        sql += " , DTA_APPR_REMARK= :action_remark ";
                                        sql += " , DTA_DOC_STATUS= :doc_status ";
                                        sql += " , DTA_ACTION_STATUS = :action_status_select ";

                                        sql += " where dh_code = :doc_id ";
                                        sql += " and DTA_TYPE = '2' and DTA_STATUS = 1";
                                        sql += " and DTA_TRAVEL_EMPID= :travel_emp_id_select ";

                                        //DevFix 20200827 1640 แก้ไขเพิ่มเงื่อนไข EMPID CAP ที่ action 
                                        if (user_role != "1")
                                        {
                                            sql += " and DTA_APPR_EMPID = :user_id_def ";
                                        }

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_status_select", action_status_select, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("travel_emp_id_select", travel_emp_id_select, "char"));
                                        if (user_role != "1")
                                        {
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        }
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }
                                    }

                                    else if (value.action.type == "4" || value.action.type == "5") // approve
                                    {
                                        ////DevFix 20211012 0000  item.ref_id เทียบได้กับ emp_id แต่เนื่องจาก traverler 1 คนมีได้มากกว่า 1 location ทำให้ข้อมูลผิด
                                        //if (item.appr_status == sbCheckApprovList)
                                        //{
                                        var action_status_select = ((item.appr_status == "true" || sbCheckApprovList == "true") ? "3" : "5");

                                        sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                        sql += " DTA_APPR_STATUS= :item_appr_status "; // true, false 
                                        sql += " , DTA_APPR_REMARK= ''";
                                        sql += " , DTA_DOC_STATUS= :doc_status ";
                                        sql += " , DTA_ACTION_STATUS = :action_status_select ";
                                        sql += " where dh_code = :doc_id ";
                                        sql += " and DTA_TYPE = '2' and DTA_STATUS = 1";
                                        sql += " and DTA_TRAVEL_EMPID= :travel_emp_id_select ";

                                        //DevFix 20200827 1640 แก้ไขเพิ่มเงื่อนไข EMPID CAP ที่ action 
                                        if (user_role != "1")
                                        {
                                            sql += " and DTA_APPR_EMPID = :user_id_def ";
                                        }


                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_status_select", action_status_select, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("travel_emp_id_select", travel_emp_id_select, "char"));
                                        if (user_role != "1")
                                        {
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        }
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1) { }

                                        //}
                                    }

                                    //DevFix 20210618 0000 เพิ่มข้อมูล  dta_update_date 
                                    sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                    sql += " DTA_UPDATE_DATE = sysdate ";
                                    sql += " where dh_code = :doc_id ";
                                    sql += " and DTA_TYPE = '2' and DTA_STATUS = 1";
                                    sql += " and DTA_TRAVEL_EMPID= :travel_emp_id_select ";
                                    if (user_role != "1")
                                    {
                                        sql += " and DTA_APPR_EMPID = :user_id_def ";
                                    }

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("travel_emp_id_select", travel_emp_id_select, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }


                                    //DevFix 20200827 1640 แก้ไขเพิ่มเงื่อนไข EMPID CAP ที่ action 
                                    //var findData = traveler_approver_List.Where(p => p.DTA_TYPE == "2" && p.DTA_STATUS == 1
                                    //                                        && p.DTA_TRAVEL_EMPID.Equals(row_check.DTE_EMP_ID)).ToList();
                                    var findData = traveler_approver_List.Where(p => p.DTA_TYPE == "2" && p.DTA_STATUS == 1
                                                                         && p.DTA_TRAVEL_EMPID.Equals(row_check.DTE_EMP_ID)).ToList();

                                    if (user_role != "1")
                                    {
                                        findData = traveler_approver_List.Where(p => p.DTA_TYPE == "2" && p.DTA_STATUS == 1
                                                                            && p.DTA_TRAVEL_EMPID.Equals(row_check.DTE_EMP_ID)
                                                                            && p.DTA_APPR_EMPID == user_id).ToList();
                                    }

                                    if (findData != null && findData.Count() > 0)
                                    {
                                        //DevFix 20200828 2140 กรณีที่เป็น Approver เดียวกัน ไม่ต้อง add ซ้ำ
                                        if (findData.Count() > 1)
                                        {
                                            foreach (var ifindData in findData)
                                            {
                                                try
                                                {
                                                    var check_data = approverList.SingleOrDefault(a => a.user_id == ifindData.DTA_APPR_EMPID);
                                                    if (check_data == null)
                                                    {
                                                        approverList.Add(new SearchUserAllModel
                                                        {
                                                            user_id = ifindData.DTA_APPR_EMPID ?? ""
                                                        });
                                                    }
                                                }
                                                catch
                                                {
                                                    approverList.Add(new SearchUserAllModel
                                                    {
                                                        user_id = ifindData.DTA_APPR_EMPID ?? ""
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (findData != null && findData.Count() > 0)
                                            {
                                                approverList.Add(new SearchUserAllModel
                                                {
                                                    user_id = findData[0].DTA_APPR_EMPID ?? ""
                                                });
                                            }
                                        }
                                    }

                                }

                            }

                            if (value.action.type == "2") // reject
                            {
                                foreach (var item in approverList)
                                {

                                    sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                    sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                    var item_user_id = item.user_id ?? "";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }

                                }
                                if (user_role == "1")
                                {
                                    sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                    sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_user_id", "admin", "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }
                                }
                            }
                            else if (value.action.type == "3") // revise
                            {
                                foreach (var item in approverList)
                                {
                                    sql = " update BZ_DOC_ACTION set  ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                    sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                    var item_user_id = item.user_id ?? "";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }
                                }
                                if (user_role == "1")
                                {
                                    sql = " update BZ_DOC_ACTION set  ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID='admin' ";
                                    sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_user_id", "admin", "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }
                                }

                                sql = "delete from BZ_DOC_ACTION where DH_CODE = :doc_id and DOC_STATUS=23 and EMP_ID='admin' and ACTION_STATUS=1 ";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                if (iret > -1) { }

                                sql = "insert into BZ_DOC_ACTION (DA_TOKEN, DH_CODE, DOC_TYPE, DOC_STATUS, EMP_ID, TAB_NO, CREATED_DATE, UPDATED_DATE) ";
                                sql += " values (:item_token, :doc_id, :doc_head_type, :doc_status, 'admin', 2, sysdate, sysdate ) ";

                                var item_token = Guid.NewGuid().ToString();
                                var doc_head_type = doc_head_search.DH_TYPE;

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("item_token", item_token, "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_head_type", doc_head_type, "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                if (iret > -1) { }

                            }
                            else if (value.action.type == "4" || value.action.type == "5") // approve
                            {
                                foreach (var item in approverList)
                                {
                                    sql = " update BZ_DOC_ACTION set  ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                    sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                    var item_user_id = item.user_id ?? "";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }
                                }
                                if (user_role == "1")
                                {
                                    sql = " update BZ_DOC_ACTION set  ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID= :item_user_id ";
                                    sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_user_id", "admin", "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }
                                }
                            }


                            #region DevFix 20210527 0000 file
                            ////http://TBKC-DAPPS-05.thaioil.localnet/ebiz
                            //string ServerPathAPI = System.Configuration.ConfigurationManager.AppSettings["ServerPathAPI"].ToString();
                            ////http://TBKC-DAPPS-05.thaioil.localnet/ebiz/file/LB21050027/
                            //var file_path = ServerPathAPI + @"/file/" + value.doc_id + @"/";

                            ////  d:/EBiz2/EBiz_Api/temp/ 
                            //var _path_temp = System.Web.HttpContext.Current.Server.MapPath("~" + @"/temp/" + value.doc_id + @"/");
                            //var _path_file = System.Web.HttpContext.Current.Server.MapPath("~" + @"/file/" + value.doc_id + @"/");

                            // การสร้างเส้นทางไปยัง temp directory -->??? เดียวต้องเอามาจาก table เพื่อลดความเสี่ยงจาก scan
                            string ServerPathAPI = configApp.GetStringFromAppSettings("ServerPathAPI") ?? "";
                            var file_path = $"{ServerPathAPI}/file/{doc_id}/";
                            var _path_temp = Path.Combine(Directory.GetCurrentDirectory(), "temp", doc_id.ToString());
                            var _path_file = Path.Combine(Directory.GetCurrentDirectory(), "file", doc_id.ToString());

                            if (value.docfile.Count > 0)
                            {
                                //delete --> insert 
                                List<DocFileListInModel> docfileList = value.docfile;
                                sql = " delete from BZ_DOC_FILE where DH_CODE = :doc_id ";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                if (iret > -1) { }

                                foreach (var item in docfileList)
                                {
                                    sql = " insert into BZ_DOC_FILE (DH_CODE, DF_ID, DF_NAME, DF_PATH, DF_REMARK, CREATED_BY, CREATED_DATE)";
                                    sql += " values (:item_dh_code, :item_dh_name , :file_path , :item_dh_remark , :user_id_def, sysdate )";

                                    var item_dh_code = item.DH_CODE ?? "";
                                    var item_dh_name = item.DF_NAME ?? "";
                                    var item_dh_remark = item.DF_REMARK ?? "";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("item_dh_code", item_dh_code, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_dh_name", item_dh_name, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_dh_remark", item_dh_remark, "char"));
                                    parameters.Add(context.ConvertTypeParameter("file_path", file_path, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }

                                    imaxid_docfile += 1;
                                }
                            }
                            #endregion DevFix 20210527 0000 file


                            context.SaveChanges();
                            transaction.Commit();
                            data.status = "S";
                            data.message = "";


                            #region DevFix 20210527 0000 file --> delete folder temp by doc id 
                            bool bcopyfile = false;
                            if (value.docfile.Count > 0)
                            {
                                try
                                {
                                    //ใส่ try เนื่องจาก code ใช้งานได้ไม่ติดปัญหาแต่จะมี กรณีที่มีการลบ file ทั้งหมดออก folder file ตาม doc นั้น  
                                    DirectoryInfo di = Directory.CreateDirectory(_path_temp);
                                    //ลบจริงตอน save
                                    if (Directory.Exists(_path_temp))
                                    {
                                        //กรณีที่ไม่มีแสดงว่าไม่ได้ถูก upload ตอนนี้ ไม่ต้อง copy file 
                                        if (di.GetFiles().Length > 0) { bcopyfile = true; }
                                    }
                                    if (bcopyfile == true)
                                    {
                                        //ลบข้อมูล folder file  
                                        DirectoryInfo difile = Directory.CreateDirectory(_path_file);
                                        if (Directory.Exists(_path_file))
                                        {
                                            //all files and folders in a directory 
                                            foreach (FileInfo file in difile.GetFiles())
                                            {
                                                //var sname = file.Name.ToString();
                                                //var file_list = value.docfile.Where(p => p.DF_NAME == sname).ToList();
                                                //if (file_list.Count == 0)
                                                //{
                                                //delete 
                                                file.Delete();
                                                //}
                                            }
                                        }

                                        DirectoryInfo ditemp = Directory.CreateDirectory(_path_temp);
                                        //ใส่ try เนื่องจาก code ใช้งานได้ไม่ติดปัญหาแต่จะมี กรณีที่มีการลบ file ทั้งหมดออก folder temp ตาม doc นั้น  
                                        Directory.CreateDirectory(_path_file);
                                        //ลบจริงตอน save
                                        if (Directory.Exists(_path_temp))
                                        {
                                            //all files and folders in a directory 
                                            foreach (FileInfo file in ditemp.GetFiles())
                                            {
                                                // copy to fodel file    
                                                file.CopyTo(_path_file + file.Name.ToString());
                                                //delete 
                                                file.Delete();
                                            }
                                        }

                                    }

                                }
                                catch { }

                            }
                            #endregion DevFix 20210527 0000 file --> delete folder temp by doc id


                            //DevFix 20210527 0000 เพิ่มข้อมูล ประเภทใบงานเป็น 1:flow, 2:not flow, 3:training เก็บไว้ที่  BZ_DOC_HEAD.DH_TYPE_FLOW
                            bool apprAllStatus = false;
                            string ret_doc_status = doc_status;
                            string emp_id_cap_next_level = "";
                            if (value.action.type == "4" || value.action.type == "5" || value.action.type == "2")
                            {
                                //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject , 6:Not Active
                                //ตรวจสอบเพิ่มกรณีที่เป็น traverler 1 คนมีมากกว่า 1 cap ถ้า cap ลำดับแรก reject ไปแล้วไม่ต้องส่งให้คนต่อไป
                                //ให้ update status 6:Not Active
                                if (value.action.type == "2")
                                {
                                    foreach (var item in value.traveler_summary)
                                    {
                                        if (item.take_action != "true")
                                            continue;

                                        var row_check = traveler_expen.Where(p => p.DTE_TOKEN.Equals(item.ref_id)).ToList().FirstOrDefault();
                                        if (row_check != null && row_check.DTE_EMP_ID != null)
                                        {
                                            var traverler_id_def = row_check.DTE_EMP_ID;
                                            if (traverler_id_def != "")
                                            {
                                                //หาข้อมูลของ cap ที่ level ถัดไป
                                                sql = @" select a.dta_appr_empid as emp_id, dta_appr_level as status_value, '' as doc_status
                                                       from bz_doc_traveler_approver a   
                                                       where dta_type = 2 and dta_doc_status = 41
                                                       and a.dh_code = :doc_id and a.dta_travel_empid = :traverler_id_def ";
                                                sql += @" and a.dta_appr_level > ( select  max(a1.dta_appr_level) as dta_appr_level
                                                       from bz_doc_traveler_approver a1
                                                       where a1.dta_type = 2 and a1.dta_doc_status in ( 40,42)
                                                       and a1.dh_code = :doc_id  and a1.dta_travel_empid = :traverler_id_def )  ";

                                                sql = "select emp_id, status_value, doc_status from(" + sql + ")t order by to_number(status_value)";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var dataCAP_Def = context.AllApproveModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (dataCAP_Def != null)
                                                {
                                                    if (dataCAP_Def.Count > 0)
                                                    {
                                                        emp_id_cap_next_level = dataCAP_Def[0].emp_id.ToString();
                                                        foreach (var itemCAP in dataCAP_Def)
                                                        {
                                                            sql = "update BZ_DOC_TRAVELER_APPROVER set ";
                                                            sql += "DTA_DOC_STATUS ='40' ";
                                                            sql += ", DTA_ACTION_STATUS ='6' ";  //6:Not Active
                                                            sql += ", DTA_APPR_STATUS ='true' ";
                                                            sql += " where DTA_TYPE = 2 and DTA_APPR_EMPID = :item_emp_id ";
                                                            sql += " and  DH_CODE = :doc_id ";
                                                            sql += " and  DTA_TRAVEL_EMPID = :traverler_id_def ";

                                                            var item_emp_id = itemCAP.emp_id ?? "";

                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                            parameters.Add(context.ConvertTypeParameter("item_emp_id", item_emp_id, "char"));
                                                            parameters.Add(context.ConvertTypeParameter("traverler_id_def", traverler_id_def, "char"));
                                                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                                            if (iret > -1) { }

                                                            context.SaveChanges();
                                                            sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                                            sql += " where dh_code= :doc_id and EMP_ID= :item_emp_id ";
                                                            sql += " and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                                            parameters.Add(context.ConvertTypeParameter("item_emp_id", item_emp_id, "char"));
                                                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                                            if (iret > -1) { }

                                                            context.SaveChanges();

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var item in value.traveler_summary)
                                    {
                                        if (item.take_action != "true")
                                            continue;

                                        var row_check = traveler_expen.Where(p => p.DTE_TOKEN.Equals(item.ref_id)).ToList().FirstOrDefault();
                                        if (row_check != null && row_check.DTE_EMP_ID != null)
                                        {
                                            var traverler_id_def = row_check.DTE_EMP_ID;
                                            if (traverler_id_def != "")
                                            {
                                                //หาข้อมูลของ cap ที่ level ถัดไป
                                                sql = @" select a.dta_appr_empid as emp_id, dta_appr_level as status_value, '' as doc_status
                                                       from bz_doc_traveler_approver a   
                                                       where dta_type = 2 and dta_doc_status = 41
                                                       and a.dh_code = :doc_id and a.dta_travel_empid = :traverler_id_def ";
                                                sql += @" and a.dta_appr_level > ( select  max(a1.dta_appr_level) as dta_appr_level
                                                       from bz_doc_traveler_approver a1
                                                       where a1.dta_type = 2 and a1.dta_doc_status in (42)
                                                       and a1.dh_code = :doc_id  and a1.dta_travel_empid =  :traverler_id_def )  ";

                                                sql = "select status_value, emp_id, doc_status from(" + sql + ")t order by to_number(status_value)";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("traverler_id_def", traverler_id_def, "char"));
                                                var dataCAP_Def = context.AllApproveModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (dataCAP_Def != null)
                                                {
                                                    if (dataCAP_Def.Count > 0)
                                                    {
                                                        emp_id_cap_next_level = dataCAP_Def[0].emp_id.ToString();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                apprAllStatus = AllApproveCAPApprover(doc_id, ref ret_doc_status);
                            }

                            #region DevFix 20210714 0000 กรณีที่ CAP Action ครบทุกคนแล้วให้ update status ของ admin = 2  

                            sql = @" select count(1) as status_value, '' as emp_id, '' as doc_status from BZ_DOC_TRAVELER_APPROVER a where a.dta_type = 2 and a.dta_action_status in (1,2) and a.dh_code = :doc_id  ";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            var dataApporver_Def = context.AllApproveModelList.FromSqlRaw(sql, parameters.ToArray()).ToList().FirstOrDefault();
                            if (dataApporver_Def != null)
                            {
                                if (dataApporver_Def.status_value == 0)
                                {
                                    sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2 ";
                                    sql += " where dh_code= :doc_id and EMP_ID='admin' and DOC_STATUS = 41 and ACTION_STATUS=1 ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    if (iret > -1) { }

                                    context.SaveChanges();
                                }
                            }
                            #endregion DevFix 20210714 0000 กรณีที่ CAP Action ครบทุกคนแล้วให้ update status ของ admin = 2


                            #region "#### SEND MAIL ####" 

                            write_log_mail($"0-email.start-submitFlow4", "type_flow :{type_flow} =>value.action.type :{value.action.type} =>apprAllStatus :{apprAllStatus} =>emp_id_cap_next_level : {emp_id_cap_next_level}");

                            //DevFix 20200910 0727 เพิ่มแนบ link Ebiz ด้วย Link ไปหน้า login  
                            string url_login = LinkLogin;
                            string sDear = "";
                            string sDetail = "";
                            string sTitle = "";
                            string sBusinessDate = "";
                            string sLocation = "";
                            string sTravelerList = "";
                            string sReasonRejected = "";

                            if (type_flow == true)
                            {
                                if (value.action.type == "2") // reject
                                {
                                    //*** กรณีที่เป็นการ approver reject สุดท้ายต้อง ส่ง mail reject ก่อน 
                                    #region traveler mail in doc
                                    traveler_mail = "";

                                    sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                             , b.employeeid as name3, b.orgname as name4                      
                                             from bz_doc_traveler_approver a
                                             inner join bz_users b on a.dta_travel_empid = b.employeeid
                                             left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                             on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id 
                                             where a.dta_type = 2 and a.dta_action_status in (5) and a.dta_doc_status = 40
                                             and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += @" and a.dta_appr_empid = :user_id_def";
                                    }
                                    sql += @" order by s.id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (tempTravel != null)
                                    {
                                        foreach (var item in tempTravel)
                                        {
                                            traveler_mail += ";" + item.name2;
                                        }
                                    }
                                    #endregion traveler mail in doc
                                    #region approver mail in doc 
                                    approver_mail = "";

                                    sql = @" select distinct b.employeeid as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 2 and a.dta_action_status in (5) and a.dta_doc_status = 40
                                                 and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += " and a.dta_appr_empid = :user_id_def ";
                                    }
                                    sql += @" union ";
                                    sql += @" select distinct b.employeeid as user_id, '' as user_name, b.email                 
                                             from bz_doc_traveler_approver a
                                             inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                             where a.dta_type = 1 and a.dta_action_status in (3) and a.dta_doc_status = 32
                                             and ( a.dta_travel_empid, a.dh_code) in (
                                             select distinct a1.dta_travel_empid, a1.dh_code from bz_doc_traveler_approver a1
                                             where a1.dta_type = 2 and a1.dta_action_status in (5) and a1.dta_doc_status = 40 ";
                                    if (user_role != "1")
                                    {
                                        //หา line ที่อยู่ภายใต้ cap
                                        sql += " and a1.dta_appr_empid = :user_id_def ";
                                    }
                                    sql += " )";
                                    sql += " and a.dh_code = :doc_id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (approvermailList != null)
                                    {
                                        if (approvermailList.Count > 0)
                                        {
                                            for (int m = 0; m < approvermailList.Count; m++)
                                            {
                                                approver_mail += ";" + approvermailList[m].email;
                                            }
                                        }
                                    }
                                    #endregion approver mail in doc

                                    #region "#### SEND MAIL ####"

                                    try
                                    {
                                        sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                        sBusinessDate = "Business Date : ";
                                        if (doc_head_search.DH_BUS_FROMDATE != null)
                                        {
                                            sBusinessDate = "Business Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                        }
                                        sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     WHERE a.dh_code = :doc_id and a.dte_status = 1 ";
                                        sql += " and a.dte_cap_appr_status  = :doc_status  order by s.id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                        var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        if (temp != null && temp.Count() > 0)
                                        {
                                            //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                            if (temp.Count == 1)
                                            {
                                                sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            }
                                            else
                                            {
                                                sLocation = "";
                                                foreach (var item in temp)
                                                {
                                                    if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                    sLocation += item.name1 + "/" + item.name2;
                                                }
                                            }
                                        }

                                        sendEmailService mail = new sendEmailService();
                                        sendEmailModel dataMail = new sendEmailModel();

                                        //to : Super admin, Requester, Traveller	 
                                        //cc : CAP Approval, Line Approval
                                        dataMail.mail_to = admin_mail + traveler_mail;
                                        dataMail.mail_cc = approver_mail + requester_mail + on_behalf_of_mail;

                                        dataMail.mail_subject = value.doc_id + " : The request for business travel has been rejected.";// + login_empid[0].user_display;

                                        sDear = "Dear All,";

                                        sDetail = "Your business travel request has been reject by " + user_name + ". To view the details, click ";
                                        sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                        if (tempTravel != null && tempTravel?.Count > 0)
                                        {
                                            var iNo = 1;
                                            sTravelerList = "<table>";
                                            foreach (var item in tempTravel)
                                            {
                                                sTravelerList += " <tr>";
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                sTravelerList += " </tr>";
                                                iNo++;
                                            }
                                            sTravelerList += "</table>";
                                        }

                                        #region set mail
                                        try
                                        {
                                            sReasonRejected = "";
                                            if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                            else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                        }
                                        catch { }

                                        dataMail.mail_body = @"<span lang='en-US'>";
                                        dataMail.mail_body += "<div>";
                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                        dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        if (sReasonRejected != "")
                                        {
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                            dataMail.mail_body += "     <br>";
                                        }
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Best Regards,";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "</div>";
                                        dataMail.mail_body += "</span>";

                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                        dataMail.doc_id = doc_id;

                                        mail.sendMail(dataMail);
                                        #endregion set mail

                                    }
                                    catch (Exception ex)
                                    {
                                        write_log_mail("88-email.message-submitFlow4", "error reject " + ex.ToString());
                                    }

                                    #endregion 
                                }
                                if (value.action.type == "4" || value.action.type == "5" || value.action.type == "2")
                                {
                                    if (apprAllStatus)
                                    {
                                        //DevFix 20211021 0000 เพื่อนำไปใช้ในการแจ้งเตือน 028_OB/LB/OT/LT : Business Travel Confirmation Letter
                                        Set_Trip_Complated(context, value.token_login, value.doc_id);

                                        #region traveler mail in doc    
                                        traveler_mail_reject = "";

                                        //DevFix 20210813 0000 แก้ไขให้ส่ง mail ไปหา traverlerที่ reject ของ action submit
                                        sql = @" select distinct '' as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2, '' as name3, '' as name4                      
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_travel_empid = b.employeeid 
                                                 where a.dta_type = 2 and a.dta_action_status in (5) and a.dta_doc_status = 42
                                                 and a.dh_code = :doc_id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        var traveler_reject = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                        if (traveler_reject != null)
                                        {
                                            foreach (var item in traveler_reject)
                                            {
                                                traveler_mail_reject += ";" + item.name2;
                                            }
                                        }
                                        sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                                                 , b.employeeid as name3, b.orgname as name4                     
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_travel_empid = b.employeeid
                                                 left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                                 on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id 
                                                 where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42
                                                 and a.dh_code = :doc_id order by s.id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        traveler_mail = "";
                                        if (tempTravel != null)
                                        {
                                            foreach (var item in tempTravel)
                                            {
                                                traveler_mail += ";" + item.name2;
                                            }
                                        }
                                        #endregion traveler mail in doc
                                        #region approver mail in doc 
                                        approver_mail = "";

                                        sql = @" select distinct b.employeeid  as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42
                                                 and a.dh_code = :doc_id ";
                                        sql += @" union ";
                                        sql += @" select distinct b.employeeid  as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 1 and a.dta_action_status in (3) and a.dta_doc_status = 32
                                                 and a.dh_code = :doc_id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        if (approvermailList != null)
                                        {
                                            if (approvermailList.Count > 0)
                                            {
                                                for (int m = 0; m < approvermailList.Count; m++)
                                                {
                                                    approver_mail += ";" + approvermailList[m].email;
                                                }
                                            }
                                        }
                                        #endregion approver mail in doc 
                                        //กรณีที่ใบงานมี traverler ถูก reject ทั้งหมด mail approver จึงไม่ต้องส่ง
                                        if (traveler_mail != "")
                                        {
                                            #region "#### SEND MAIL ####"  
                                            try
                                            {
                                                sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                                sBusinessDate = "Business Date : ";
                                                if (doc_head_search.DH_BUS_FROMDATE != null)
                                                {
                                                    sBusinessDate = "Business Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                                }

                                                sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     WHERE a.dh_code = :doc_id and a.dte_status = 1 ";
                                                sql += " and a.dte_cap_appr_status  = :doc_status order by s.id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                                var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (temp != null && temp.Count() > 0)
                                                {
                                                    //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }

                                                sendEmailService mail = new sendEmailService();
                                                sendEmailModel dataMail = new sendEmailModel();

                                                //to : Super admin, Requester, Traveller 
                                                //cc : CAP Approval, Line Approval
                                                dataMail.mail_to = admin_mail + traveler_mail + approver_mail + requester_mail + on_behalf_of_mail + traveler_mail_reject;
                                                dataMail.mail_cc = "";// approver_mail + requester_mail + on_behalf_of_mail + traveler_mail_reject;
                                                dataMail.mail_subject = value.doc_id + " : The request for business travel has been approved.";
                                                //Attached: Approval / Output form
                                                var file_Approval_Output_form = file_name_approval(value.doc_id, value.token_login);
                                                if (file_Approval_Output_form != "")
                                                {
                                                    string file_name = file_Approval_Output_form;// @"temp\APPROVAL_FORM_OT21060025_2021100410233333.xlsx";
                                                    //string _FolderMailAttachments = System.Configuration.ConfigurationManager.AppSettings["FilePathServerApp"].ToString();//d:\Ebiz2\Ebiz_App\
                                                    string _FolderMailAttachments = configApp.GetStringFromAppSettings("FilePathServerApp") ?? "";
                                                    string mail_attachments = _FolderMailAttachments + file_name;
                                                    dataMail.mail_attachments = mail_attachments;
                                                }

                                                sDear = "Dear All,";

                                                sDetail = "The request for business travel has been approved. To view the approval details, click ";
                                                sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";
                                                sDetail += "<br>";
                                                sDetail += "Any additional arrangements require to complete by the traveler. To view travel details, click ";
                                                sDetail += "<a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id) + "'>travel details.</a>";

                                                if (tempTravel != null && tempTravel?.Count > 0)
                                                {
                                                    var iNo = 1;
                                                    sTravelerList = "<table>";
                                                    foreach (var item in tempTravel)
                                                    {
                                                        sTravelerList += " <tr>";
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                        sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                        sTravelerList += " </tr>";
                                                        iNo++;
                                                    }
                                                    sTravelerList += "</table>";
                                                }
                                                #region set mail
                                                try
                                                {
                                                    sReasonRejected = "";
                                                    //if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                    //else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                                }
                                                catch { }

                                                dataMail.mail_body = @"<span lang='en-US'>";
                                                dataMail.mail_body += "<div>";
                                                dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                                dataMail.mail_body += "     <br>";
                                                if (sReasonRejected != "")
                                                {
                                                    dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                    dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                    dataMail.mail_body += "     <br>";
                                                }
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                                dataMail.mail_body += "     </span></font></div>";

                                                dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     Best Regards,";
                                                dataMail.mail_body += "     <br>";
                                                dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                                dataMail.mail_body += "     </span></font></div>";

                                                dataMail.mail_body += "</div>";
                                                dataMail.mail_body += "</span>";

                                                var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                dataMail.doc_id = doc_id;

                                                mail.sendMail(dataMail);
                                                #endregion set mail

                                            }
                                            catch (Exception ex)
                                            {
                                                write_log_mail("88-email.message-submitFlow4", "error sub " + ex.ToString());
                                            }
                                            #endregion "#### SEND MAIL ####"  


                                            if (notiPassport)
                                            {
                                                #region "#### SEND MAIL ####" 017_OB/LB/OT/LT : Please update Passport information - [Title_Name of traveler]
                                                try
                                                {
                                                    //ส่งตอน CAP Approve แล้วและตรวจสอบได้ว่าไม่มี valid passport เเละให้ส่ง E-Mail  
                                                    if (tempTravel != null && tempTravel?.Count > 0)
                                                    {
                                                        foreach (var itemTravel in tempTravel)
                                                        {
                                                            var bValidPassportExpire = true;
                                                            var item_traveler_id = itemTravel.name3 ?? "";
                                                            var item_traveler_name = itemTravel.name1 ?? "";
                                                            sql = @" select distinct emp_id as id, '' as name1, '' as name2, '' as name3, '' as name4 from bz_data_passport 
                                                                 where default_type = 'true' and to_date(passport_date_expire,'dd Mon yyyy') >= sysdate
                                                                 and emp_id = :item_traveler_id ";

                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("item_traveler_id", item_traveler_id, "char"));
                                                            var dataPassport = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                            if (dataPassport != null)
                                                            {
                                                                if (dataPassport.Count > 0) { bValidPassportExpire = false; }
                                                            }
                                                            if (bValidPassportExpire == false) { continue; }

                                                            sql = @" select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                                FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                                inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                                left join bz_master_country c on a.ct_id = c.ct_id
                                                                left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                                left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                                ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                                and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                                WHERE a.dte_status = 1  
                                                                and a.dh_code = :doc_id and a.dte_cap_appr_status  = :doc_status and a.DTE_Emp_Id = :item_traveler_id  order by s.id";

                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                                            parameters.Add(context.ConvertTypeParameter("item_traveler_id", item_traveler_id, "char"));
                                                            var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                            if (temp != null && temp.Count() > 0)
                                                            {
                                                                //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                                //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                                if (temp.Count == 1)
                                                                {
                                                                    sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                                }
                                                                else
                                                                {
                                                                    sLocation = "";
                                                                    foreach (var item in temp)
                                                                    {
                                                                        if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                                        sLocation += item.name1 + "/" + item.name2;
                                                                    }
                                                                }
                                                            }

                                                            sendEmailService mail = new sendEmailService();
                                                            sendEmailModel dataMail = new sendEmailModel();

                                                            //TO: Traveler
                                                            //CC : Admin - PMSV; Admin - PMDV(if any);
                                                            dataMail.mail_to = itemTravel.name2;
                                                            dataMail.mail_cc = admin_mail;// approver_mail + requester_mail + on_behalf_of_mail + traveler_mail_reject;
                                                            dataMail.mail_subject = value.doc_id + " : Please update Passport information - " + itemTravel.name1;

                                                            sDear = "Dear " + itemTravel.name1 + ",";

                                                            sDetail = "Your require to update passport information in order to make further arrangements. To view travel details, click ";
                                                            sDetail += "<a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id).Replace("travelerhistory", "passport") + "'>" + value.doc_id + "</a>";

                                                            var iNo = 1;
                                                            sTravelerList = "<table>";
                                                            //foreach (var item in tempTravel)
                                                            //{ 
                                                            sTravelerList += " <tr>";
                                                            sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + itemTravel.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                            sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + itemTravel.name3 + "</span></font></td>";//Emp. ID
                                                            sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + itemTravel.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                            sTravelerList += " </tr>";
                                                            iNo++;
                                                            //}
                                                            sTravelerList += "</table>";

                                                            #region set mail
                                                            try
                                                            {
                                                                sReasonRejected = "";
                                                                //if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                                //else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                                            }
                                                            catch { }
                                                            dataMail.mail_body = @"<span lang='en-US'>";
                                                            dataMail.mail_body += "<div>";
                                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                            dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                                            dataMail.mail_body += "     <br>";
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                            dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                                            dataMail.mail_body += "     <br>";
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                            dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                            dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                            dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                                            dataMail.mail_body += "     <br>";
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                            dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                                            dataMail.mail_body += "     <br>";
                                                            if (sReasonRejected != "")
                                                            {
                                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                                dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                                dataMail.mail_body += "     <br>";
                                                            }
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                            dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                                            dataMail.mail_body += "     <br>";
                                                            dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                                            dataMail.mail_body += "     </span></font></div>";

                                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                            dataMail.mail_body += "     <br>";
                                                            dataMail.mail_body += "     Best Regards,";
                                                            dataMail.mail_body += "     <br>";
                                                            dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                                            dataMail.mail_body += "     </span></font></div>";

                                                            dataMail.mail_body += "</div>";
                                                            dataMail.mail_body += "</span>";

                                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                            dataMail.doc_id = doc_id;

                                                            mail.sendMail(dataMail);
                                                            #endregion set mail
                                                        }
                                                    }
                                                }
                                                catch (Exception ex) { write_log_mail("88-email.message-submitFlow4", "error Travel " + ex.ToString()); }
                                                #endregion "#### SEND MAIL ####" 017_OB/LB/OT/LT : Please update Passport information - [Title_Name of traveler]
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //CAP --> กรณีที่มี CAP มากกว่า 1 คน
                                        if (emp_id_cap_next_level != "")
                                        {
                                            #region traveler mail in doc
                                            traveler_mail = "";

                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                                                     , b.employeeid as name3, b.orgname as name4                     
                                                     from bz_doc_traveler_approver a
                                                     inner join bz_users b on a.dta_travel_empid = b.employeeid
                                                     left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                                     on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id  
                                                     where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42
                                                     and a.dh_code = :doc_id ";

                                            if (user_role != "1")
                                            {
                                                sql += @" and a.dta_appr_empid ='" + user_id_def + "' ";
                                            }
                                            sql += @" order by s.id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                            if (user_role != "1")
                                            {
                                                parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                            }
                                            var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (tempTravel != null)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    traveler_mail += ";" + item.name2;
                                                }
                                            }
                                            #endregion traveler mail in doc

                                            #region approver mail in doc 
                                            approver_mail = "";

                                            sql = @" select distinct b.employeeid  as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42
                                                 and a.dh_code = :doc_id ";
                                            if (user_role != "1")
                                            {
                                                sql += " and a.dta_appr_empid = :user_id_def";
                                            }
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            if (user_role != "1")
                                            {
                                                parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                            }
                                            var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (approvermailList != null)
                                            {
                                                if (approvermailList.Count > 0)
                                                {
                                                    //approver_mail = ";" + approvermailList[0].email; 
                                                    for (int m = 0; m < approvermailList.Count; m++)
                                                    {
                                                        //if (approver_mail != "") { approver_mail += ";"; }
                                                        approver_mail += ";" + approvermailList[m].email;
                                                    }
                                                }
                                            }
                                            #endregion approver mail in doc

                                            #region "#### SEND MAIL ####"

                                            try
                                            {
                                                sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                                sBusinessDate = "Business Date : ";
                                                if (doc_head_search.DH_BUS_FROMDATE != null)
                                                {
                                                    sBusinessDate = "Business Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                                }

                                                sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     WHERE a.dh_code :doc_id and a.dte_status = 1 ";
                                                sql += " and a.dte_cap_appr_status  = :doc_status  order by s.id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (temp != null && temp.Count() > 0)
                                                {
                                                    //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }

                                                sendEmailService mail = new sendEmailService();
                                                sendEmailModel dataMail = new sendEmailModel();

                                                sql = @" select distinct nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name
                                                         , b.email as email ,a.dta_appr_empid as user_id
                                                         from bz_doc_traveler_approver a left join bz_users b on a.dta_appr_empid = b.employeeid
                                                         where a.dh_code = :doc_id and a.dta_type = 2 and a.dta_action_status in (2) ";

                                                //DevFix 20210810 0000 กรณีที่ Line Approve แล้วให้ส่งไปยัง CAP ลำดับแรกของแต่ละ traveler
                                                sql += @" and (a.dta_appr_level, a.dta_travel_empid) in (
                                                         select min(dta_appr_level),dta_travel_empid from bz_doc_traveler_approver a1
                                                         where a1.dh_code = :doc_id and a1.dta_type = 2 and a1.dta_action_status in (2) group by dta_travel_empid )";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var empapp = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                if (empapp != null && empapp?.Count > 0)
                                                {
                                                    foreach (var iemp in empapp)
                                                    {
                                                        //to : CAP Approval
                                                        //cc : Line Approval, Super admin, Requester, Traveller

                                                        dataMail.mail_to = iemp.email ?? "";
                                                        dataMail.mail_cc = approver_mail + admin_mail + requester_mail + traveler_mail + on_behalf_of_mail;
                                                        dataMail.mail_subject = value.doc_id + " : Please approve business travel request as CAP.";

                                                        sDear = "Dear " + iemp.user_name + ",";

                                                        sDetail = "Please approve business travel request as CAP. To view the details, click ";
                                                        sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";


                                                        if (tempTravel != null && tempTravel?.Count > 0)
                                                        {
                                                            var iNo = 1;
                                                            sTravelerList = "<table>";
                                                            foreach (var item in tempTravel)
                                                            {
                                                                sTravelerList += " <tr>";
                                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                                sTravelerList += " </tr>";
                                                                iNo++;
                                                            }
                                                            sTravelerList += "</table>";
                                                        }

                                                        #region set mail
                                                        try
                                                        {
                                                            sReasonRejected = "";
                                                            //if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                            //else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                                        }
                                                        catch { }

                                                        dataMail.mail_body = @"<span lang='en-US'>";
                                                        dataMail.mail_body += "<div>";
                                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                        dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                                        dataMail.mail_body += "     <br>";
                                                        if (sReasonRejected != "")
                                                        {
                                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                            dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                            dataMail.mail_body += "     <br>";
                                                        }
                                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                                        dataMail.mail_body += "     </span></font></div>";

                                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     Best Regards,";
                                                        dataMail.mail_body += "     <br>";
                                                        dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                                        dataMail.mail_body += "     </span></font></div>";

                                                        dataMail.mail_body += "</div>";
                                                        dataMail.mail_body += "</span>";

                                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                        dataMail.doc_id = doc_id;

                                                        mail.sendMail(dataMail);
                                                        #endregion set mail
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                write_log_mail("88-email.message-submitFlow4", "error to cap " + ex.ToString());
                                            }

                                            #endregion
                                        }
                                    }
                                }
                                else if (value.action.type == "3") // revise
                                {

                                    #region traveler mail in doc 
                                    traveler_mail = "";

                                    sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                                 , b.employeeid as name3, b.orgname as name4                     
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_travel_empid = b.employeeid
                                                 left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                                 on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id 
                                                 where a.dta_type = 2 and a.dta_action_status in (4) and a.dta_doc_status = 23
                                                 and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += @" and a.dta_appr_empid = :user_id_def ";
                                    }
                                    sql += @" order by s.id ";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (tempTravel != null)
                                    {
                                        foreach (var item in tempTravel)
                                        {
                                            traveler_mail += ";" + item.name2;
                                        }
                                    }
                                    #endregion traveler mail in doc
                                    #region approver mail in doc 
                                    approver_mail = "";

                                    sql = @" select distinct b.employeeid  as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 2 and a.dta_action_status in (4) and a.dta_doc_status = 23
                                                 and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        sql += " and a.dta_appr_empid = :user_id_def ";
                                    }
                                    sql += @" union ";
                                    sql += @" select distinct b.employeeid  as user_id, '' as user_name, b.email                 
                                             from bz_doc_traveler_approver a
                                             inner join bz_users b on a.dta_appr_empid = b.employeeid 
                                             where a.dta_type = 1 and a.dta_action_status in (3) and a.dta_doc_status = 32
                                             and ( a.dta_travel_empid, a.dh_code) in ( select distinct a1.dta_travel_empid, a1.dh_code from bz_doc_traveler_approver a1 where a1.dta_type = 2 and a1.dta_action_status in (4) and a1.dta_doc_status = 23  )
                                             and a.dh_code = :doc_id ";
                                    if (user_role != "1")
                                    {
                                        //หา line ที่อยู่ภายใต้ cap
                                        sql += " and a.dta_appr_empid = :user_id_def ";
                                    }

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    if (user_role != "1")
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                    }
                                    var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (approvermailList != null)
                                    {
                                        if (approvermailList.Count > 0)
                                        {
                                            //approver_mail = ";" + approvermailList[0].email; 
                                            for (int m = 0; m < approvermailList.Count; m++)
                                            {
                                                approver_mail += ";" + approvermailList[m].email;
                                            }
                                        }
                                    }
                                    #endregion approver mail in doc


                                    #region "#### SEND MAIL ####" 
                                    try
                                    {
                                        sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                        sBusinessDate = "Business Date : ";
                                        if (doc_head_search.DH_BUS_FROMDATE != null)
                                        {
                                            sBusinessDate = "Business Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                        }
                                        sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     WHERE a.dh_code = :doc_id and a.dte_status = 1  and a.dte_cap_appr_status  = :doc_status  order by s.id ";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                        var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        if (temp != null && temp.Count() > 0)
                                        {
                                            //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                            if (temp.Count == 1)
                                            {
                                                sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                            }
                                            else
                                            {
                                                sLocation = "";
                                                foreach (var item in temp)
                                                {
                                                    if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                    sLocation += item.name1 + "/" + item.name2;
                                                }
                                            }
                                        }

                                        sendEmailService mail = new sendEmailService();
                                        sendEmailModel dataMail = new sendEmailModel();

                                        //to : Super admin
                                        //cc : CAP Approval, Line Approval, Requester, Traveller 
                                        dataMail.mail_to = admin_mail;
                                        dataMail.mail_cc = approver_mail + requester_mail + traveler_mail + on_behalf_of_mail;

                                        dataMail.mail_subject = value.doc_id + " : Please revise your request for business travel.";// + login_empid[0].user_display;

                                        sDear = "Dear All,";

                                        sDetail = "Your business travel request has been revise by " + user_name + ". To view the details, click ";
                                        sDetail += "<a href='" + (LinkLogin + "i").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                        if (tempTravel != null && tempTravel?.Count > 0)
                                        {
                                            var iNo = 1;
                                            sTravelerList = "<table>";
                                            foreach (var item in tempTravel)
                                            {
                                                sTravelerList += " <tr>";
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                sTravelerList += " </tr>";
                                                iNo++;
                                            }
                                            sTravelerList += "</table>";
                                        }

                                        #region set mail
                                        try
                                        {
                                            sReasonRejected = "";
                                            if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                            else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                        }
                                        catch { }

                                        dataMail.mail_body = @"<span lang='en-US'>";
                                        dataMail.mail_body += "<div>";
                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                        dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                        dataMail.mail_body += "     <br>";
                                        if (sReasonRejected != "")
                                        {
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                            dataMail.mail_body += "     <br>";
                                        }
                                        dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Best Regards,";
                                        dataMail.mail_body += "     <br>";
                                        dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                        dataMail.mail_body += "     </span></font></div>";

                                        dataMail.mail_body += "</div>";
                                        dataMail.mail_body += "</span>";

                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                        dataMail.doc_id = doc_id;

                                        mail.sendMail(dataMail);
                                        #endregion set mail

                                    }
                                    catch (Exception ex)
                                    {
                                        write_log_mail("88-email.message-submitFlow4", "error revise " + ex.ToString());
                                    }

                                    #endregion 
                                }

                            }
                            else
                            {
                                //DevFix 20210614 0000 เพิ่ม Email แจ้งเตือน ประเภทใบงานเป็น 2:not flow, 3:training
                                if (value.action.type == "4" || value.action.type == "5")
                                {
                                    if (apprAllStatus)
                                    {
                                        #region "#### SEND MAIL ####"
                                        //Business Trip
                                        //To: Traveler
                                        //cc : Super Admin +PMSV Admin

                                        //Training Trip
                                        //To : Traveler
                                        //cc : Super Admin +PMDV Admin
                                        try
                                        {
                                            sql = "SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email FROM bz_users WHERE role_id = :role_id ";
                                            var role_id = "1";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("role_id", role_id, "char"));
                                            var adminList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (adminList != null)
                                            {
                                                foreach (var item in adminList)
                                                {
                                                    admin_mail += ";" + item.email ?? "";
                                                }
                                                if (admin_mail != "")
                                                    admin_mail = ";" + admin_mail.Substring(1);
                                            }

                                            //กรณีที่เป็น pmdv admin, pmsv_admin
                                            admin_mail += mail_group_admin(context, "pmsv_admin");
                                            if (doc_id.IndexOf("T") > -1)
                                            {
                                                admin_mail += mail_group_admin(context, "pmdv_admin");
                                            }

                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                                                     , b.employeeid as name3, b.orgname as name4 
                                                     from BZ_DOC_TRAVELER_EXPENSE a left join bz_users b on a.DTE_EMP_ID = b.employeeid
                                                     left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                                                     on a.dh_code =s.dh_code and a.dte_emp_id = s.dte_emp_id 
                                                     where a.dh_code = :doc_id order by s.id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            sTitle = "Title : " + doc_head_search.DH_TOPIC ?? "";
                                            sBusinessDate = "Business Date : ";
                                            if (doc_head_search.DH_BUS_FROMDATE != null)
                                            {
                                                sBusinessDate = "Business Date : " + dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                            }

                                            sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                     FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                     inner join BZ_USERS U on a.DTE_Emp_Id = u.employeeid
                                                     left join bz_master_country c on a.ct_id = c.ct_id
                                                     left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
                                                     left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                                                     ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                                                     and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                                                     WHERE a.dh_code = :doc_id and a.dte_status = 1 and a.dte_cap_appr_status  = :doc_status order by s.id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                            var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (temp != null && temp.Count() > 0)
                                            {
                                                //dataMail.mail_body += "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                //DevFix 20210816 0000 กรณีที่มีมากกว่า 1 Location
                                                if (temp.Count == 1)
                                                {
                                                    sLocation = "Location : " + temp[0].name1 + "/" + temp[0].name2;
                                                }
                                                else
                                                {
                                                    sLocation = "";
                                                    foreach (var item in temp)
                                                    {
                                                        if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
                                                        sLocation += item.name1 + "/" + item.name2;
                                                    }
                                                }
                                            }

                                            sendEmailService mail = new sendEmailService();
                                            sendEmailModel dataMail = new sendEmailModel();

                                            sql = @" select distinct b.employeeid  as user_id, '' as user_name, u.email from bz_users u  
                                                     inner join (  
                                                      select dh_create_by user_id, dh_code from bz_doc_head 
                                                       union 
                                                       select dh_initiator_empid user_id, dh_code from bz_doc_head
                                                       union
                                                       select DTE_EMP_ID user_id, dh_code from BZ_DOC_TRAVELER_EXPENSE 
                                                       ) u2 on u.employeeid = u2.user_id  
                                                       where  u2.dh_code = :doc_id ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var empapp = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            foreach (var e in empapp)
                                            {
                                                dataMail.mail_to += e.email + ";";
                                            }
                                            dataMail.mail_cc = admin_mail + on_behalf_of_mail;

                                            dataMail.mail_subject = value.doc_id + " : The business travel has been created and approved.";

                                            string xerrror_file_Approval_Output_form = "";
                                            try
                                            {
                                                //Attached : Approval form >> ตั้งชื่อใบงานตามเลข Doc. เช่น Approval Form_LB21090059 and Any file attached in E-Biz system
                                                var file_Approval_Output_form = file_name_approval(value.doc_id, value.token_login);
                                                if (file_Approval_Output_form != "")
                                                {
                                                    if (file_Approval_Output_form == "The request failed with HTTP status 401: Unauthorized.")
                                                    {
                                                        xerrror_file_Approval_Output_form = file_Approval_Output_form;
                                                    }
                                                    else
                                                    {
                                                        string file_name = file_Approval_Output_form;// @"temp\APPROVAL_FORM_OT21060025_2021100410233333.xlsx";
                                                        //string _FolderMailAttachments = System.Configuration.ConfigurationManager.AppSettings["FilePathServerApp"].ToString();//d:\Ebiz2\Ebiz_App\
                                                        string _FolderMailAttachments = configApp.GetStringFromAppSettings("FilePathServerApp") ?? "";
                                                        string mail_attachments = _FolderMailAttachments + file_name;
                                                        dataMail.mail_attachments = mail_attachments;
                                                    }
                                                }
                                                //file แนบ Any file attached  
                                                if (value.docfile.Count > 0)
                                                {
                                                    List<DocFileListInModel> docfileList = value.docfile;
                                                    foreach (var item in docfileList)
                                                    {
                                                        string file_name = item.DF_NAME;// @"EMPLOYEE LETTER_TOP_Mr._Luck_Saraya_180521102605.docx";
                                                        string _FolderMailAttachments = _path_file;//d:\Ebiz2\EBiz_Api\file\OT21060025\
                                                        string mail_attachments = _FolderMailAttachments + file_name;

                                                        if (dataMail.mail_attachments != "") { dataMail.mail_attachments += "|"; }
                                                        dataMail.mail_attachments += mail_attachments;
                                                    }
                                                }
                                            }
                                            catch (Exception ex) { }

                                            sDear = "Dear All,";

                                            sDetail = "The request for business travel has been approved. To view the approval details, click ";
                                            sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";
                                            sDetail += "<br>";
                                            sDetail += "Any additional arrangements require to complete by the traveler. To view travel details, click ";
                                            sDetail += "<a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id) + "'>travel details.</a>";

                                            var iNo = 1;
                                            sTravelerList = "<table>";
                                            foreach (var item in tempTravel)
                                            {
                                                sTravelerList += " <tr>";
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + item.name1 + "</span></font></td>";//1) [Title_Name of traveler] 
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name3 + "</span></font></td>";//Emp. ID
                                                sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + item.name4 + "</span></font></td>";//SEC./DEP./FUNC. 
                                                sTravelerList += " </tr>";
                                                iNo++;
                                            }
                                            sTravelerList += "</table>";

                                            #region set mail
                                            try
                                            {
                                                sReasonRejected = "";
                                                //if (value.action.type == "2") { sReasonRejected = "Reason Rejected : " + value.action.remark; }
                                                //else if (value.action.type == "3") { sReasonRejected = "Reason Revised : " + value.action.remark; }
                                            }
                                            catch { }

                                            dataMail.mail_body = @"<span lang='en-US'>";
                                            dataMail.mail_body += "<div>";
                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sDear + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sDetail + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sTitle + "</span></font></div>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     " + sLocation + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                            dataMail.mail_body += "     <span style='font-size:15pt;'>Traveler List : " + sTravelerList + "</span></font></div>";
                                            dataMail.mail_body += "     <br>";
                                            if (sReasonRejected != "")
                                            {
                                                dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
                                                dataMail.mail_body += "     " + sReasonRejected + "</font></div>";
                                                dataMail.mail_body += "     <br>";
                                            }
                                            dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
                                            dataMail.mail_body += "     </span></font></div>";

                                            dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     Best Regards,";
                                            dataMail.mail_body += "     <br>";
                                            dataMail.mail_body += "     Business Travel Services Team (PMSV)";
                                            dataMail.mail_body += "     </span></font></div>";

                                            dataMail.mail_body += "</div>";
                                            dataMail.mail_body += "</span>";

                                            if (xerrror_file_Approval_Output_form != "")
                                            {
                                                dataMail.mail_body += " " + xerrror_file_Approval_Output_form;
                                            }

                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;

                                            data.message = mail.sendMail(dataMail);
                                            #endregion set mail
                                        }
                                        catch (Exception ex)
                                        {
                                            data.message = "SEND MAIL Error " + ex.ToString();

                                            write_log_mail("88-email.message-submitFlow4", "error " + ex.ToString());
                                        }

                                        #endregion
                                    }
                                }

                            }

                            write_log_mail("99-email.end-submitFlow4", "");

                            #endregion "#### SEND MAIL ####" 

                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.status = "E";
                            data.message = ex.ToString();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                data.status = "E";
                data.message = ex.Message;
            }

            return data;
        }

        private void Set_Trip_Complated(TOPEBizCreateTripEntities context, string token_login, string doc_id)
        {
            try
            {
                //select dh_code, dh_type from BZ_DOC_TRIP_COMPLETED 
                string sql = "insert into BZ_DOC_TRIP_COMPLETED select dh_code, dh_type from BZ_DOC_HEAD where dh_code = :doc_id ";

                var parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                var iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                if (iret > -1) { }

                context.SaveChanges();
            }
            catch { }
        }
        private bool checkExpenUpdate(docFlow2_travel data1, docFlow2_travel data2)
        {
            bool ret = false;
            try
            {
                string datechk1 = "";
                string datechk2 = "";

                if (chkStrCompare(data1.emp_id) != chkStrCompare(data2.emp_id))
                    return true;

                if (chkStrCompare(data1.air_ticket) != chkStrCompare(data2.air_ticket))
                    return true;

                if (chkStrCompare(data1.accommodation) != chkStrCompare(data2.accommodation))
                    return true;

                if (chkStrCompare(data1.allowance) != chkStrCompare(data2.allowance))
                    return true;

                if (chkDecCompare(data1.allowance_day) != chkDecCompare(data2.allowance_day))
                    return true;

                if (chkDecCompare(data1.allowance_night) != chkDecCompare(data2.allowance_night))
                    return true;

                if (chkStrCompare(data1.visa_fee) != chkStrCompare(data2.visa_fee))
                    return true;

                if (chkStrCompare(data1.travel_insurance) != chkStrCompare(data2.travel_insurance))
                    return true;

                if (chkStrCompare(data1.transportation) != chkStrCompare(data2.transportation))
                    return true;

                if ((data1.passport_valid ?? "").Length >= 10)
                    datechk1 = data1.passport_valid.Substring(0, 10);
                if ((data2.passport_valid ?? "").Length >= 10)
                    datechk2 = data2.passport_valid.Substring(0, 10);

                if (chkStrCompare(datechk1) != chkStrCompare(datechk2))
                    return true;

                if (chkStrCompare(data1.passport_expense) != chkStrCompare(data2.passport_expense))
                    return true;

                datechk1 = "";
                datechk2 = "";
                if ((data1.clothing_valid ?? "").Length >= 10)
                    datechk1 = data1.clothing_valid.Substring(0, 10);
                if ((data2.clothing_valid ?? "").Length >= 10)
                    datechk2 = data2.clothing_valid.Substring(0, 10);

                if (chkStrCompare(datechk1) != chkStrCompare(datechk2))
                    return true;

                if (chkStrCompare(data1.clothing_expense) != chkStrCompare(data2.clothing_expense))
                    return true;

                if (chkStrCompare(data1.registration_fee) != chkStrCompare(data2.registration_fee))
                    return true;

                if (chkStrCompare(data1.miscellaneous) != chkStrCompare(data2.miscellaneous))
                    return true;

                if (chkDecCompare(data1.total_expenses) != chkDecCompare(data2.total_expenses))
                    return true;

                //ret = true;

            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        private string retText(string sdata, int digit = 10)
        {
            if (string.IsNullOrEmpty(sdata))
                return "";

            if (sdata == "-")
                return sdata;

            sdata = sdata.Trim();

            sdata = "0000000000" + sdata;
            sdata = sdata.Substring(sdata.Length - 10, 10);

            return sdata;
        }

        private bool Flow1Mail(decimal? doc_status, string doc_no)
        {
            bool ret = false;
            try
            {
                sendEmailService mail = new sendEmailService();
                sendEmailModel data = new sendEmailModel();
                if (doc_status == 21) // admin
                {
                    data.mail_subject = doc_no + " : Please submit business travel document request.";
                }
                else
                {
                    data.mail_subject = doc_no + " : Please initiate business travel document workflow request.";
                }

                mail.sendMail(data);
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
            }

            return ret;
        }

        private string dateFromTo(string sDateFrom, string sDateto)
        {
            string ret = "";
            try
            {
                if (sDateFrom == "")
                    return ret;

                ret = DateTime.ParseExact(sDateFrom.Substring(0, 10), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
                ret += " to " + DateTime.ParseExact(sDateto.Substring(0, 10), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");

            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        private DateTime? chkDate(string value)
        {
            DateTime? date = null;
            try
            {
                if (value == null)
                    return date;

                if (value.Length < 10)
                    return date;

                date = DateTime.ParseExact(value.Substring(0, 10), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                //if (string.IsNullOrEmpty(value))
                //    return date;

                //date = DateTime.ParseExact(value, "yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture);

            }
            catch (Exception ex)
            {

            }
            return date;
        }

        private string chkDateSQL(string value)
        {
            string date = "null";
            try
            {
                if (value == null)
                    return date;

                if (value.Length < 10)
                    return date;

                date = "to_date('" + value.Substring(0, 10) + "','yyyy-mm-dd')";

                //if (string.IsNullOrEmpty(value))
                //    return date;

                //date = "to_date('" + value + "','yyyy-mm-dd')";

            }
            catch (Exception ex)
            {

            }
            return date;
        }

        private string chkString(string value)
        {
            string ret = null;
            try
            {
                if (value == null)
                    return ret;


                ret = value.Replace("'", "''");

            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        private string retCheckValue(string value)
        {
            string ret = "N";
            try
            {
                if (value.ToUpper() == "TRUE")
                    ret = "Y";
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        private decimal? retDecimal(string value)
        {
            decimal? ret = null;
            try
            {
                ret = string.IsNullOrEmpty(value) ? ret : Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        private string retDecimalSQL(string value)
        {
            string ret = "null";
            try
            {
                ret = Convert.ToDecimal(value).ToString();
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        private string chkStrCompare(string value)
        {
            string ret = "";
            try
            {
                ret = value ?? "";
                ret = ret.Trim().ToUpper();
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        private decimal chkDecCompare(string value)
        {
            decimal ret = 0;
            try
            {
                ret = Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        //DevFix 202008221 2244 แก้ไข format date ให้รองรับกับ format ที่ 25 NOV 2021 แปลงเป็น yyyy-mm-dd 
        private string chkDateSQL_All(string value)
        {
            string date = "null";
            try
            {
                if (value == null)
                    return date;

                if (value.Length < 10)
                    return date;


                try
                {
                    string[] xsplit = value.Substring(0, 10).Split('-');
                    if (xsplit.Length == 3)
                    {
                        if (Convert.ToInt32(xsplit[0]) > 543)
                        {
                            //input yyyy-mm-dd
                            DateTime dDate = new DateTime(Convert.ToInt32(xsplit[0]), Convert.ToInt32(xsplit[1]), Convert.ToInt32(xsplit[2]));
                            return "to_date('" + dDate.ToString("yyyy-MM-dd") + "','yyyy-mm-dd')";
                        }
                        else
                        {
                            //input dd-mm-yyyy
                            DateTime dDate = new DateTime(Convert.ToInt32(xsplit[2]), Convert.ToInt32(xsplit[1]), Convert.ToInt32(xsplit[0]));
                            return "to_date('" + dDate.ToString("yyyy-MM-dd") + "','yyyy-mm-dd')";
                        }
                    }

                    xsplit = value.Substring(0, 11).Split(' ');
                    if (xsplit.Length == 3)
                    {
                        //input dd MMM yyyy
                        int iMonth = 1;
                        for (int i = 1; i < 13; i++)
                        {
                            string fullMonthName = new DateTime(2015, i, 1).ToString("MMM");
                            if (fullMonthName.ToUpper() == xsplit[1].ToUpper())
                            {
                                iMonth = i;
                                break;
                            }
                        }
                        DateTime dDate = new DateTime(Convert.ToInt32(xsplit[2]), iMonth, Convert.ToInt32(xsplit[0]));
                        return "to_date('" + dDate.ToString("yyyy-MM-dd") + "','yyyy-mm-dd')";
                    }

                }
                catch
                {
                }

                //yyyy-mm-dd
                date = "to_date('" + value.Substring(0, 10) + "','yyyy-mm-dd')";


                //if (string.IsNullOrEmpty(value))
                //    return date;

                //date = "to_date('" + value + "','yyyy-mm-dd')";

            }
            catch (Exception ex)
            {

            }
            return date;
        }

        public DocFileListOutModel uploadfile_create_trip(IHttpContextAccessor _httpContextAccessor)
        {
            // สร้างวัตถุสำหรับผลลัพธ์
            var data = new DocFileListOutModel();
            string ret = "";

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                data.after_trip.opt1 = "false";
                data.after_trip.opt2.status = "Upload file failed.";
                data.after_trip.opt2.remark = "HttpContext is not available.";
            }
            else
            {
                // ตรวจสอบ Content-Type
                if (httpContext.Request.ContentType == null ||
                    (!httpContext.Request.ContentType.Contains("multipart/form-data") &&
                     !httpContext.Request.ContentType.Contains("application/x-www-form-urlencoded")))
                {
                    throw new InvalidOperationException("Invalid Content-Type. Expected multipart/form-data or application/x-www-form-urlencoded.");
                }


                var files = httpContext.Request.Form.Files;

                // ตรวจสอบไฟล์ที่อัปโหลด
                if (files == null || files.Count == 0)
                {
                    data.after_trip.opt1 = "false";
                    data.after_trip.opt2.status = "No file selected.";
                    data.after_trip.opt2.remark = "Please select a file to upload.";
                    return data;
                }

                try
                {
                    var httpRequest = httpContext.Request;
                    string file_doc = httpRequest.Form["file_doc"].ToString() ?? "";
                    string file_token_login = httpRequest.Form["file_token_login"].ToString() ?? "";

                    // เรียกใช้ฟังก์ชันคัดลอกไฟล์ไปยังเซิร์ฟเวอร์
                    ret = copy_file_data_to_server(ref data, files, false, 1, file_doc);

                    // ประมวลผลผลลัพธ์
                    if (string.IsNullOrEmpty(ret))
                    {
                        data.after_trip.opt1 = "true";
                        data.after_trip.opt2.status = "Upload file succeeded.";
                        data.after_trip.opt2.remark = "";
                    }
                    else
                    {
                        data.after_trip.opt1 = "false";
                        data.after_trip.opt2.status = "Upload file failed.";
                        data.after_trip.opt2.remark = ret;
                    }
                }
                catch (Exception ex)
                {
                    data.after_trip.opt1 = "false";
                    data.after_trip.opt2.status = "Upload file failed.";
                    data.after_trip.opt2.remark = $"An error occurred: {ex.Message}";
                }
            }
            return data;
        }

        public DocFileListTravelerhistoryOutModel uploadfile_travelerhistory(IHttpContextAccessor _httpContextAccessor)
        {
            SetDocService sw = new SetDocService();
            var data = new DocFileListTravelerhistoryOutModel();
            string ret = "";

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                data.after_trip.opt1 = "false";
                data.after_trip.opt2.status = "Upload file failed.";
                data.after_trip.opt2.remark = "HttpContext is not available.";
                return data;
            }

            if (httpContext.Request.ContentType == null ||
                (!httpContext.Request.ContentType.Contains("multipart/form-data") &&
                 !httpContext.Request.ContentType.Contains("application/x-www-form-urlencoded")))
            {
                throw new InvalidOperationException("Invalid Content-Type. Expected multipart/form-data or application/x-www-form-urlencoded.");
            }

            var files = httpContext.Request.Form.Files;

            if (files == null || files.Count == 0)
            {
                data.after_trip.opt1 = "false";
                data.after_trip.opt2.status = "No file selected.";
                data.after_trip.opt2.remark = "Please select a file to upload.";
                return data;
            }

            try
            {
                var httpRequest = httpContext.Request;
                string file_doc = httpRequest.Form["file_doc"].ToString() ?? "";
                string file_page = httpRequest.Form["file_page"].ToString()?.Trim() ?? "";
                string file_emp = httpRequest.Form["file_emp"].ToString() ?? "";
                string file_typename = httpRequest.Form["file_typename"].ToString() ?? "";
                string file_token_login = httpRequest.Form["file_token_login"].ToString() ?? "";

                #region config path
                try
                {
                    file_page = httpRequest.Form["file_page"].ToString().Trim();
                    if (file_page == "kh code") { file_page = "khcode"; }
                }
                catch { }

                string folderName = "Image";
                string folder_path = $"/{folderName}/{file_doc}/{file_page}/{file_emp}/";
                if (file_page == "isos" || file_page == "portal")
                {
                    folderName = "DocumentFile";
                    folder_path = $"/{folderName}/{file_page}/";
                    if (!string.IsNullOrEmpty(file_typename))
                    {
                        folder_path += $"{file_typename}/";
                    }
                }
                else if (file_page == "mtvisacountries")
                {
                    file_doc = "master visa docountries";
                    folder_path = $"/{folderName}/{file_doc}/{file_page}/";
                }
                else if (file_page == "allowance")
                {
                    if (file_typename == "auto_generate")
                    {
                        folderName = "ExportFile";
                        folder_path = $"/{folderName}/{file_doc}/{file_page}/{file_emp}/";
                    }
                }
                else if (file_page == "passport")
                {
                    folderName = "personal";
                    folder_path = $"/{folderName}/{file_doc}/{file_page}/{file_emp}/";
                }
                else if (file_page == "khcode")
                {
                    folderName = "DocumentFile";
                    folder_path = $"/{folderName}/{file_page}/";
                }
                #endregion config path

                // เรียกใช้ฟังก์ชันคัดลอกไฟล์ไปยังเซิร์ฟเวอร์
                ret = copy_file_travelerhistory_data_to_server(ref data, files, false, 1, file_doc, folder_path);

                if (ret == "true")
                {
                    string modified_by = "";
                    string modified_date = "";
                    try
                    {
                        if (file_token_login != "")
                        {
                            sw = new SetDocService();
                            modified_by = sw.sqlEmpUserDispayName(file_token_login);
                        }

                    }
                    catch { }
                    try
                    {
                        modified_date = DateTime.Now.ToString("dd MMM yyyy");
                    }
                    catch { }

                    data.img_list.doc_id = file_doc;
                    data.img_list.emp_id = file_emp;
                    data.img_list.id = "1";

                    //data.img_list.path = _Server_path + folder_path;
                    //data.img_list.filename = _FileName; 
                    //data.img_list.fullname = _Server_path + _Folder + _FileName;

                    data.img_list.pagename = file_page;
                    data.img_list.action_type = "insert";
                    data.img_list.action_change = "true";

                    data.img_list.modified_by = modified_by;
                    data.img_list.modified_date = modified_date;

                    data.img_list.remark = file_token_login;

                    // Update data
                    sw = new SetDocService();
                    if (file_page == "travelerhistory")
                    {
                        var img_list = data.img_list;
                        ret = sw.SetTravelerHistoryImg(img_list);
                        data.img_list.remark = (ret.ToLower() == "true" ? "" : ret);
                    }
                    else if (file_page == "khcode")
                    {
                        var full_path_name = data.img_list?.fullname ?? "";
                        ret = sw.ImportDataKH_Code(full_path_name, file_token_login);
                    }
                    if (ret.ToLower() == "true")
                    {
                        data.after_trip.opt1 = "true";
                        data.after_trip.opt2.status = "Upload file succeeded.";
                        data.after_trip.opt2.remark = "";
                    }
                    else
                    {
                        data.after_trip.opt1 = "false";
                        data.after_trip.opt2.status = "Upload file failed.";
                        data.after_trip.opt2.remark = $"An error occurred: {ret}";
                    }
                }
            }
            catch (Exception ex)
            {
                data.after_trip.opt1 = "false";
                data.after_trip.opt2.status = "Upload file failed.";
                data.after_trip.opt2.remark = $"An error occurred: {ex.Message}";
            }

            return data;
        }


        public string file_name_approval(string doc_id, string token_login)
        {
            //????? หา funtions ใน Service ไม่เจอ
            //try
            //{
            //    ws_generalService.generalService ws_Approval_Output_form = new ws_generalService.generalService();
            //    var doc_type = "local";
            //    if (doc_id.IndexOf("O") > -1) { doc_type = "oversea"; }
            //    var arr_token = "[{'token_login':'" + token_login + "','doc_id':'" + doc_id + "','state':'" + doc_type + "'}]";
            //    var sfileTravelReport = ws_Approval_Output_form.TravelReport("phase1report", arr_token, "");

            //    Boolean bCheckUrl = false;
            //    string[] xsplit = sfileTravelReport.Split(':');
            //    for (int m = 0; m < xsplit.Length; m++)
            //    {
            //        if (bCheckUrl == true)
            //        {
            //            string xlast1 = xsplit[m].ToString();
            //            string[] xsplit2 = xlast1.Split('"');
            //            for (int n = 0; n < xsplit2.Length; n++)
            //            {
            //                if (xsplit2[n].ToString().Trim() != "")
            //                {
            //                    return @"temp\" + xsplit2[n].ToString();
            //                }
            //            }
            //            break;
            //        }
            //        if (xsplit[m].ToString().IndexOf("file_outbound_name") > -1)
            //        {
            //            bCheckUrl = true;
            //        }
            //    }
            //}
            //catch (Exception ex) { return ex.Message.ToString(); }

            return "";
        }


        public static string copy_file_data_to_server(ref DocFileListOutModel dataDoc
            , IFormFileCollection files, bool folderCopyFile, int iTypeFile
            , string? file_doc = "", string? page_name = "", string? emp_id = "")
        {
            // ตรวจสอบไฟล์
            if (files == null || files.Count == 0) return "Invalid files.";
            if (iTypeFile >= 1 && string.IsNullOrEmpty(file_doc)) return "Invalid Doc No.";
            if (iTypeFile >= 2 && string.IsNullOrEmpty(page_name)) return "Invalid Page Name.";
            if (iTypeFile >= 3 && string.IsNullOrEmpty(emp_id)) return "Invalid Employee ID.";

            try
            {
                // เลือกไฟล์แรกที่อัปโหลด
                IFormFile file = files[0];
                string safeFileTemp = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

                // ตรวจสอบชื่อไฟล์และนามสกุล
                if (string.IsNullOrEmpty(safeFileTemp)) return "Invalid file name.";
                if (!safeFileTemp.All(c => char.IsLetterOrDigit(c) || "_-.".Contains(c))) return "Invalid characters in file name.";
                if (string.IsNullOrEmpty(extension)) return "File does not have a valid extension.";

                string[] allowedExtensions = { ".xlsx", ".xls", ".pdf", ".doc", ".docx", ".png", ".jpg", ".gif", ".eml", ".msg" };
                if (!allowedExtensions.Contains(extension)) return "Invalid file type.";

                // กำหนดเส้นทางไดเรกทอรี
                string rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "ExportFile", file_doc, page_name ?? "", emp_id ?? "");
                if (!Directory.Exists(rootDir)) Directory.CreateDirectory(rootDir);

                if (folderCopyFile)
                {
                    rootDir = Path.Combine(rootDir, "copy");
                    if (!Directory.Exists(rootDir)) Directory.CreateDirectory(rootDir);
                }

                // สร้างชื่อไฟล์ใหม่
                string datetime_run = DateTime.Now.ToString("yyyyMMddHHmmss");
                string newFileName = $"{safeFileTemp}-{datetime_run}{extension}";
                string finalFilePath = Path.Combine(rootDir, newFileName);

                // ตรวจสอบเส้นทางไฟล์
                finalFilePath = Path.GetFullPath(finalFilePath);
                if (!finalFilePath.StartsWith(rootDir, StringComparison.OrdinalIgnoreCase))
                    return "Attempt to access unauthorized path.";

                // บันทึกไฟล์
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }


                // ดึงค่า FilePathServerApp จาก ConfigurationManager
                string _FilePathServerApp = System.Configuration.ConfigurationManager.AppSettings["FilePathServerApp"] ?? "";

                // ตั้งค่าผลลัพธ์เป็น URL
                dataDoc.DF_NAME = newFileName; // ชื่อไฟล์ใหม่
                dataDoc.DF_PATH = rootDir; // เส้นทางปลายทางในเซิร์ฟเวอร์

                // สร้าง URL สำหรับไฟล์ที่อัปโหลด
                string relativePath = $"/ExportFile/{file_doc}/{(page_name ?? "")}/{(emp_id ?? "")}/{(folderCopyFile ? "copy/" : "")}{newFileName}"
                    .Replace("\\", "/");

                // ตรวจสอบว่ามีข้อมูลสำคัญครบถ้วน
                if (string.IsNullOrWhiteSpace(file_doc) || string.IsNullOrWhiteSpace(newFileName))
                {
                    return "Invalid file document or file name.";
                }

                // Escape ตัวอักษรพิเศษใน URL
                string safeUrl = Uri.EscapeUriString(relativePath);

                // สร้าง URL เต็ม
                string fullUrl = $"{_FilePathServerApp}{safeUrl}";

                // ตรวจสอบว่า URL ไม่ออกนอกโครงสร้างโฟลเดอร์ที่กำหนด
                if (!fullUrl.StartsWith(_FilePathServerApp, StringComparison.OrdinalIgnoreCase))
                {
                    return "Attempt to create an unauthorized URL.";
                }

                try
                {
                    // กำหนดค่าผลลัพธ์
                    dataDoc.after_trip.opt3.remark = fullUrl ?? "";
                }
                catch { }
            }
            catch (Exception ex)
            {
                return $"An error occurred while processing your request. {ex.Message}";
            }

            return "";
        }

        public static string copy_file_travelerhistory_data_to_server(ref DocFileListTravelerhistoryOutModel dataDoc
            , IFormFileCollection files, bool folderCopyFile, int iTypeFile
            , string? file_doc = "", string? folder_path = "", string? page_name = "", string? emp_id = "")
        {
            // ตรวจสอบไฟล์
            if (files == null || files.Count == 0) return "Invalid files.";
            if (iTypeFile >= 1 && string.IsNullOrEmpty(file_doc)) return "Invalid Doc No.";
            if (iTypeFile >= 2 && string.IsNullOrEmpty(folder_path)) return "Invalid Folder Path.";
            if (iTypeFile >= 3 && string.IsNullOrEmpty(page_name)) return "Invalid Page Name.";
            if (iTypeFile >= 4 && string.IsNullOrEmpty(emp_id)) return "Invalid Employee ID.";

            try
            {
                // เลือกไฟล์แรกที่อัปโหลด
                IFormFile file = files[0];
                string safeFileTemp = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

                // ตรวจสอบชื่อไฟล์และนามสกุล
                if (string.IsNullOrEmpty(safeFileTemp)) return "Invalid file name.";
                if (!safeFileTemp.All(c => char.IsLetterOrDigit(c) || "_-.".Contains(c))) return "Invalid characters in file name.";
                if (string.IsNullOrEmpty(extension)) return "File does not have a valid extension.";

                string[] allowedExtensions = { ".xlsx", ".xls", ".pdf", ".doc", ".docx", ".png", ".jpg", ".gif", ".eml", ".msg" };
                if (!allowedExtensions.Contains(extension)) return "Invalid file type.";

                // กำหนดเส้นทางไดเรกทอรี
                //string rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", folder_path);
                string folderPathClean = folder_path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (folder_path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    throw new ArgumentException("Invalid characters in folder path.");
                }
                string rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", folderPathClean);

                if (!Directory.Exists(rootDir)) Directory.CreateDirectory(rootDir);

                if (folderCopyFile)
                {
                    rootDir = Path.Combine(rootDir, "copy");
                    if (!Directory.Exists(rootDir)) Directory.CreateDirectory(rootDir);
                }

                // สร้างชื่อไฟล์ใหม่
                string datetime_run = DateTime.Now.ToString("yyyyMMddHHmmss");
                string newFileName = $"{safeFileTemp}-{datetime_run}{extension}";
                string finalFilePath = Path.Combine(rootDir, newFileName);

                // ตรวจสอบเส้นทางไฟล์ 
                finalFilePath = Path.GetFullPath(finalFilePath);
                rootDir = Path.GetFullPath(rootDir);

                // ตรวจสอบว่า finalFilePath อยู่ภายใต้ rootDir หรือไม่
                if (!finalFilePath.StartsWith(rootDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "Attempt to access unauthorized path.";
                }

                // บันทึกไฟล์
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }


                // ดึงค่า FilePathServerApp จาก ConfigurationManager
                string _FilePathServerApp = System.Configuration.ConfigurationManager.AppSettings["FilePathServerApp"] ?? "";

                // ตั้งค่าผลลัพธ์เป็น URL
                dataDoc.DF_NAME = newFileName; // ชื่อไฟล์ใหม่
                dataDoc.DF_PATH = rootDir; // เส้นทางปลายทางในเซิร์ฟเวอร์

                // สร้าง URL สำหรับไฟล์ที่อัปโหลด
                string relativePath = $"/ExportFile/{file_doc}/{(page_name ?? "")}/{(emp_id ?? "")}/{(folderCopyFile ? "copy/" : "")}{newFileName}"
                    .Replace("\\", "/");

                // ตรวจสอบว่ามีข้อมูลสำคัญครบถ้วน
                if (string.IsNullOrWhiteSpace(file_doc) || string.IsNullOrWhiteSpace(newFileName))
                {
                    return "Invalid file document or file name.";
                }

                // Escape ตัวอักษรพิเศษใน URL
                string safeUrl = Uri.EscapeUriString(relativePath);

                // สร้าง URL เต็ม
                string fullUrl = $"{_FilePathServerApp}{safeUrl}";

                // ตรวจสอบว่า URL ไม่ออกนอกโครงสร้างโฟลเดอร์ที่กำหนด
                if (!fullUrl.StartsWith(_FilePathServerApp, StringComparison.OrdinalIgnoreCase))
                {
                    return "Attempt to create an unauthorized URL.";
                }

                try
                {
                    // Update Imgage List
                    dataDoc.img_list.path = _FilePathServerApp;
                    dataDoc.img_list.filename = newFileName;
                    dataDoc.img_list.fullname = fullUrl;

                    // กำหนดค่าผลลัพธ์
                    dataDoc.after_trip.opt3.remark = fullUrl ?? "";
                }
                catch { }
            }
            catch (Exception ex)
            {
                return $"An error occurred while processing your request. {ex.Message}";
            }

            return "true";
        }

    }
}