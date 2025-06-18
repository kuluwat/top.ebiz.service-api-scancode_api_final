using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

using top.ebiz.service.Models.Create_Trip;
using top.ebiz.helper;
using top.ebiz.service.Service.Traveler_Profile;

using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Report;
using static top.ebiz.service.Service.Report.ClassReportModel;
using System.Drawing.Imaging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using Microsoft.Graph.Models.TermStore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Exchange.WebServices.Data;
using System.Diagnostics;
using static top.ebiz.service.Service.Traveler_Profile.SetDocService;
using System.Text;
namespace top.ebiz.service.Service.Create_Trip
{
    public class documentService
    {
        //DevFix 20200910 0727 เพิ่มแนบ link Ebiz ด้วย Link ไปหน้า login 
        //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/authen.aspx?page=/main/request/edit/###/i 
        string LinkLogin = top.ebiz.helper.AppEnvironment.GeteLinkLogin() ?? "";

        //DevFix 20211004 0000 เพิ่มแนบ link Ebiz Phase2  
        //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/master/###/travelerhistory 
        string LinkLoginTravelerhistory = top.ebiz.helper.AppEnvironment.GeteLinkLoginTravelerhistory() ?? "";

        #region auwat 20221026 1435 เพิ่มเก็บ log การส่ง mail => เนื่องจากมีกรณที่กดปุ่มแล้ว mail ไม่ไป
        private void write_log_mail(string step, string data_log)
        {
            try
            {
                logCreateModel mLog = new logCreateModel();
                mLog.module = "E-MAIL";
                mLog.tevent = step;//step
                mLog.ref_id = 0;
                mLog.data_log = data_log;
                //logService.insertLog(mLog);
            }
            catch (Exception ex_write)
            {
                logCreateModel mLog = new logCreateModel();
                mLog.module = "E-MAIL";
                mLog.tevent = "write log Doc Service error";//step
                mLog.ref_id = 0;
                mLog.data_log = ex_write.Message.ToString();
                //logService.insertLog(mLog);
            }
        }
        #endregion auwat 20221026 1435 เพิ่มเก็บ log การส่ง mail => เนื่องจากมีกรณที่กดปุ่มแล้ว mail ไม่ไป

        public string mail_group_admin(TOPEBizCreateTripEntities context, string role_type_name)
        {
            //??? น่าจะต้องทำเป็น stroe
            var admin_mail = "";
            var sql = @" SELECT DISTINCT user_id, '' AS user_name, email, role_type
FROM (
-- Super Admin (มีลำดับความสำคัญสูงสุด)
SELECT a.emp_id AS user_id, u.email, 'super_admin' AS role_type 
FROM bz_data_manage a 
INNER JOIN vw_bz_users u ON a.emp_id = u.employeeid
WHERE a.super_admin = 'true'
    
UNION
    
-- PMSV Admin (เฉพาะที่ไม่ใช่ super_admin)
SELECT a.emp_id AS user_id, u.email, 'pmsv_admin' AS role_type 
FROM bz_data_manage a 
INNER JOIN vw_bz_users u ON a.emp_id = u.employeeid
WHERE a.pmsv_admin = 'true' 
AND a.super_admin <> 'true'AND a.pmdv_admin <> 'true' AND a.contact_admin <> 'true'
    
UNION
    
-- PMDV Admin (เฉพาะที่ไม่ใช่ super_admin)
SELECT a.emp_id AS user_id, u.email, 'pmdv_admin' AS role_type 
FROM bz_data_manage a 
INNER JOIN vw_bz_users u ON a.emp_id = u.employeeid
WHERE a.pmdv_admin = 'true'
AND a.super_admin <> 'true' AND a.pmsv_admin <> 'true' AND a.contact_admin <> 'true'
    
UNION
    
-- Contact Admin (เฉพาะที่ไม่ใช่ super_admin)
SELECT a.emp_id AS user_id, u.email, 'contact_admin' AS role_type 
FROM bz_data_manage a 
INNER JOIN vw_bz_users u ON a.emp_id = u.employeeid
WHERE a.contact_admin = 'true'
AND a.super_admin <> 'true' AND a.pmsv_admin <> 'true' AND a.pmdv_admin <> 'true'
) t where t.role_type = :role_type_name";

            var parameters = new List<OracleParameter>();
            parameters.Add(context.ConvertTypeParameter("role_type_name", role_type_name, "char"));
            var adminlistall = context.SearchUserEmailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

            if (adminlistall != null && adminlistall?.Count > 0)
            {
                admin_mail = $";{string.Join(";", adminlistall.Select(a => a.email))}";
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
                sql = " SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email, role_id as role_type  FROM vw_bz_users WHERE role_id = 1 ";
                parameters = new List<OracleParameter>();
                var adminList = context.SearchUserEmailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                if (adminList != null)
                {
                    foreach (var item in adminList)
                    {
                        admin_mail += item.email + ";" ?? "";
                    }
                    if (admin_mail != "") { admin_mail = admin_mail.Substring(1) + ";"; }
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

            // string query_pis_emp_details = @" 
            //         SELECT EMPLOYEEID as user_id
            //         , nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name
            //         , EMAIL email FROM VW_BZ_USERS b";

            try
            {
                sql = @"SELECT EMPLOYEEID as user_id
                    , nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name
                    , EMAIL email FROM VW_BZ_USERS b WHERE EMPLOYEEID IN ( SELECT DH_CREATE_BY FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id )";

                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                var requesterList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                if (requesterList != null)
                {
                    if (requesterList.Count > 0)
                    {
                        requester_mail = requesterList[0].email + ";";
                        requester_name = requesterList[0].user_name;
                    }
                }



                sql = @"SELECT EMPLOYEEID as user_id
                    , nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name
                    , EMAIL email FROM VW_BZ_USERS b WHERE EMPLOYEEID IN ( SELECT DH_BEHALF_EMP_ID FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";
                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                var behalfList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                if (behalfList != null)
                {
                    if (behalfList.Count > 0)
                    {
                        on_behalf_of_mail = behalfList[0].email + ";";
                    }
                }


                sql = @"SELECT EMPLOYEEID as user_id
                    , nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name
                    , EMAIL email FROM VW_BZ_USERS b WHERE EMPLOYEEID IN (SELECT DH_INITIATOR_EMPID FROM BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";
                parameters = new List<OracleParameter>();
                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                var initial = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                if (initial != null && initial.Count() > 0)
                {
                    on_behalf_of_mail += initial[0].email + ";";
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
                    FROM bz_login_token a left join vw_bz_users u on a.user_id=u.employeeid
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
                    //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject  
                    var query = context.AllApproveModelList.FromSqlRaw(
                        @"SELECT COUNT(1) AS status_value, '' AS emp_id, '' AS doc_status
                    FROM bz_doc_traveler_approver a
                    WHERE a.dta_action_status IN (1, 2, 4)
                    AND a.dta_type = 1
                    AND a.dh_code = :doc_id",
                        context.ConvertTypeParameter("doc_id", docId, "char")
                    ).ToList().FirstOrDefault();

                    // มีรายการที่ยังไม่ได้ Action หรือ Query ไม่คืนค่า
                    if (query == null) { return false; }
                    if (query.status_value > 0) { return false; }

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
                                    @"SELECT DISTINCT dta_appr_empid AS appr_empid,dta_travel_empid AS traverler_empid
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
SET DTA_DOC_STATUS = CASE 
                    WHEN DTA_DOC_STATUS = '42' THEN '42'
                    ELSE '41'
                    END,
DTA_ACTION_STATUS = CASE 
                    WHEN DTA_DOC_STATUS = '42' THEN '3'
                    ELSE '2'
                    END
WHERE dh_code = :doc_id 
AND DTA_TYPE = '2'
AND DTA_DOC_STATUS != '40'";

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

        private string AttachmentsInDocNo(TOPEBizCreateTripEntities context, string doc_id)
        {
            var attachments = "";
            var docFileList = context.BZ_DOC_FILE_ATTACHMENT.Where(p => p.DH_CODE.Equals(doc_id)).ToList();
            if (docFileList != null && docFileList.Count > 0)
            {
                string ServerFolder = top.ebiz.helper.AppEnvironment.GeteServerFolder();
                var attachmentList = new List<string>();
                foreach (var item in docFileList)
                {
                    var fullName = item.DF_FULLNAME ?? "";
                    string[] xsplit = fullName.Split($"/AttachmentFile/");
                    var pathName = $"AttachmentFile/{xsplit?[1]}";

                    var _val = (Path.Combine(ServerFolder, pathName))?.Replace("/", "\\");
                    if (!string.IsNullOrEmpty(_val)) { attachmentList.Add(_val); }
                }
                attachments = string.Join("|", attachmentList);
            }
            return attachments;
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

                        string requester_name = "";

                        #region DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin  
                        //PMSV Admin	PMDV Admin
                        string super_admin_mail = "";
                        string pmsv_admin_mail = "";
                        string pmdv_admin_mail = "";
                        string requester_mail = "";
                        string initiator_mail = "";
                        string on_behalf_of_mail = "";
                        string traveler_mail = "";
                        string line_approver_mail = "";
                        string cap_approver_mail = "";

                        //  super_admin_mail = get_mail_group_admin(context);
                        super_admin_mail = mail_group_admin(context, "super_admin");

                        pmsv_admin_mail = mail_group_admin(context, "pmsv_admin");

                        //กรณีที่เป็น pmdv admin, pmsv_admin
                        pmsv_admin_mail = mail_group_admin(context, "pmsv_admin");
                        if (doc_id.IndexOf("T") > -1)
                        {
                            pmdv_admin_mail += mail_group_admin(context, "pmdv_admin");
                        }

                        //ต้องหาใหม่เนื่องจากจะมีกรณีที่เป็นใบงานใหม่ด้วย
                        get_mail_requester_in_doc(context, doc_id, ref requester_name, ref requester_mail, ref on_behalf_of_mail);
                        #endregion DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin 



                        sql = @"SELECT io, COST_CENTER_RESP cc FROM VW_BZ_MASTER_IO where io is not null  and COST_CENTER_RESP is not null  ";
                        var ccio = context.CostcenterIOList.FromSqlRaw(sql).ToList();

                        #region DevFix 20200909 1606 กรณที่กรอกข้อมูล GL Account ใหม่ที่ไม่ได้อยู่ใน master ให้เพิ่มเข้าระบบ 
                        sql = " select GL_NO from VW_BZ_MASTER_GL where GL_NO is not null order by GL_NO ";
                        //var ccgl_account = context.Database.SqlQuery<gl_account>(sql).ToList();
                        var ccgl_account = context.GLAccountList.FromSqlRaw(sql, new OracleParameter()).ToList();
                        #endregion DevFix 20200909 1606 กรณที่กรอกข้อมูล GL Account ใหม่ที่ไม่ได้อยู่ใน master ให้เพิ่มเข้าระบบ


                        #region DevFix 20200911 0000 ส่งเมลแจ้งคนที่ On behalf of  

                        var param_list = new OracleParameter();
                        sql = @"SELECT EMPLOYEEID user_id, EMAIL email FROM vw_bz_users ";
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

                        on_behalf_of_mail = "";
                        if (behalfList != null)
                        {
                            if (behalfList.Count > 0)
                            {
                                on_behalf_of_mail = behalfList[0].email + ";";
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
                                    parameters = new List<OracleParameter>();
                                    sql = @" INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 1, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                }
                                if ((value.type_of_travel.siteVisite ?? "") == "true")
                                {
                                    parameters = new List<OracleParameter>();
                                    sql = @"
                                    INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 2, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.workshop ?? "") == "true")
                                {
                                    parameters = new List<OracleParameter>();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 3, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.roadshow ?? "") == "true")
                                {
                                    parameters = new List<OracleParameter>();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 4, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.conference ?? "") == "true")
                                {
                                    parameters = new List<OracleParameter>();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 5, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", "", "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                if ((value.type_of_travel.other ?? "") == "true")
                                {
                                    parameters = new List<OracleParameter>();
                                    sql = @"INSERT INTO BZ_DOC_TRAVEL_TYPE (DH_CODE, DTT_ID, DTT_NOTE) VALUES(:DH_CODE, :DTT_ID, :DTT_NOTE)";
                                    parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "v"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_ID", 6, "int"));
                                    parameters.Add(context.ConvertTypeParameter("DTT_NOTE", value.type_of_travel.other_detail, "v"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                //DevFix 20220805 --> after go-live เพิ่ม Tick box = Training 
                                if ((value.type_of_travel.training ?? "") == "true")
                                {
                                    parameters = new List<OracleParameter>();
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
                                        parameters = new List<OracleParameter>();
                                        sql = @"INSERT INTO BZ_DOC_CONTIENT (DH_CODE, CTN_ID) VALUES(:DH_CODE, :CTN_ID) ";
                                        parameters.Add(context.ConvertTypeParameter("DH_CODE", value.id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("CTN_ID", Convert.ToDecimal("1"), "int"));
                                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }
                                    else
                                    {
                                        foreach (var ic in value.continent)
                                        {
                                            parameters = new List<OracleParameter>();
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
                                    parameters = new List<OracleParameter>();
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

                                        parameters = new List<OracleParameter>();
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
                                    parameters = new List<OracleParameter>();
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
                                        gl_account_def = gl_account_def.Trim().ToUpper();

                                        var gl_accountcheck = ccgl_account.Where(p => p.gl_no.Trim().ToUpper().Equals(gl_account_def))?.ToList();
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

                                #region "#### BZ_DOC_FILE ####"

                                var doc_file = context.BZ_DOC_FILE_ATTACHMENT.Where(p => p.DH_CODE.Equals(value.id)).ToList();
                                var doc_file_list = value.doc_file_table;

                                if (doc_file != null || doc_file_list != null)
                                {
                                    foreach (var d in doc_file_list)
                                    {
                                        if (d.action_change == "true")
                                        {
                                            var ActionChangeStatus = false;
                                            BZ_DOC_FILE_ATTACHMENT row_change = doc_file.Where(p => p.DF_ID.Equals(d.id)).ToList()?.FirstOrDefault();
                                            if (row_change != null && row_change.DH_CODE != null) { ActionChangeStatus = true; }

                                            if (!ActionChangeStatus)
                                            {
                                                if (d.action_type == "insert")
                                                {
                                                    context.BZ_DOC_FILE_ATTACHMENT.Add(new BZ_DOC_FILE_ATTACHMENT
                                                    {
                                                        DH_CODE = value.id,
                                                        DF_ID = d.id,
                                                        DF_PATH = d.path,
                                                        DF_NAME = d.filename,
                                                        DF_FULLNAME = d.fullname,
                                                        UPDATED_DATE = DateTime.Now,
                                                        TOKEN_UPDATE = token_login
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                if (row_change != null)
                                                {
                                                    //เผื่อ d.action_type == "insert" ??? แต่ควรไม่มีเหตุการนี้
                                                    if (d.action_type == "update" || d.action_type == "insert")
                                                    {
                                                        row_change.DH_CODE = d.doc_id;
                                                        row_change.DF_ID = d.id;
                                                        row_change.DF_PATH = d.path;
                                                        row_change.DF_NAME = d.filename;
                                                        row_change.DF_FULLNAME = d.fullname;
                                                        row_change.UPDATED_DATE = DateTime.Now;
                                                        row_change.TOKEN_UPDATE = token_login;
                                                    }
                                                    else if (d.action_type == "delete")
                                                    {
                                                        var entitiesToDelete = context.BZ_DOC_FILE_ATTACHMENT.Where(x => x.DH_CODE == d.doc_id && x.DF_ID == d.id).ToList();
                                                        // ลบข้อมูลทั้งหมด
                                                        if (entitiesToDelete.Any())
                                                        {
                                                            context.BZ_DOC_FILE_ATTACHMENT.RemoveRange(entitiesToDelete);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else { continue; }

                                    }

                                }
                                #endregion "#### BZ_DOC_FILE ####"

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

                                #endregion "#### [SUBMIT] >> BZ_DOC_ACTION, BZ_DOC_TRAVELER_APPROVER ####"

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
                                            //DevFix 20210813 0000 หลังจาก Requester กด Submit แล้ว E-mail วิ่งไปหา Initiator แต่ไม่ CC: Requester & Traveler  
                                            sql = @" SELECT distinct EMPLOYEEID as user_id,  nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email
        FROM VW_BZ_USERS b 
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
                                                    requester_mail = requesterList[0].email + ";";
                                                    requester_name = requesterList[0].user_name ?? "";
                                                }
                                            }

                                            //DevFix 20210813 0000 เพิ่ม email เพื่อนำไปใช้ตอน cc 
                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
            , b.employeeid as name3, b.orgname as name4
            from BZ_DOC_TRAVELER_EXPENSE a left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid 
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
                                                    traveler_mail += item.name2 + ";";
                                                }
                                            }

                                            SendEmailServiceTravelerProfile mail = new SendEmailServiceTravelerProfile();
                                            sendEmailModel dataMail = new sendEmailModel();
                                            if (doc_status == 21) // admin
                                            {
                                                #region DevFix 20200916 2219 เพิ่ม cc initiator  
                                                var initial_mail = "";
                                                try
                                                {
                                                    sql = "SELECT EMPLOYEEID user_id, '' as user_name, EMAIL email ";
                                                    sql += "FROM vw_bz_users WHERE EMPLOYEEID = :initiator_emp_id_select ";
                                                    var initiator_emp_id_select = value.initiator.emp_id ?? "";
                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter("initiator_emp_id_select", initiator_emp_id_select, "char"));
                                                    var initial = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                    if (initial != null && initial.Count() > 0)
                                                    {
                                                        initial_mail = initial[0].email + ";";
                                                    }
                                                }
                                                catch (Exception ex) { }
                                                #endregion DevFix 20200916 2219 เพิ่ม cc initiator 

                                                //TO: Admin(PMSV)
                                                //CC: Requester; Initiator
                                                dataMail.mail_to = (super_admin_mail + pmsv_admin_mail);
                                                dataMail.mail_cc = requester_mail + on_behalf_of_mail + initial_mail + traveler_mail;

                                                var resMailShowCase = $"To: ";
                                                resMailShowCase += $"{super_admin_mail?.TrimStart(';').ToLower()} (Super Admin) ";
                                                resMailShowCase += $"{pmsv_admin_mail?.TrimStart(';').ToLower()} (PMSV Admin) ";
                                                resMailShowCase += $"<br>Cc: ";
                                                if (!string.IsNullOrEmpty(requester_mail))
                                                {
                                                    resMailShowCase += $"{requester_mail?.TrimStart(';').ToLower()} (Requester) ";
                                                }
                                                if (!string.IsNullOrEmpty(on_behalf_of_mail))
                                                {
                                                    resMailShowCase += $"{on_behalf_of_mail?.TrimStart(';').ToLower()} (On Behalf Of) ";
                                                }
                                                if (!string.IsNullOrEmpty(initial_mail))
                                                {
                                                    resMailShowCase += $"{initial_mail?.TrimStart(';').ToLower()} (Initiator) ";
                                                }
                                                if (!string.IsNullOrEmpty(traveler_mail))
                                                {
                                                    resMailShowCase += $"{traveler_mail?.TrimStart(';').ToLower()} (Traveler) ";
                                                }
                                                resMailShowCase += "</div>";

                                                dataMail.mail_show_case = resMailShowCase;

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
                                                sql += " FROM vw_bz_users WHERE EMPLOYEEID = :user_initiator_emp_id ";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("user_initiator_emp_id", user_initiator_emp_id, "char"));
                                                var initial = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (initial != null && initial.Count() > 0)
                                                {
                                                    initiator_mail = initial[0].email ?? "";
                                                    user_initiator_display = initial[0].user_name ?? "";
                                                }
                                                //DevFix 20210813 0000 หลังจาก Requester กด Submit แล้ว E-mail วิ่งไปหา Initiator 
                                                //to : Initiator
                                                //cc : Super admin, Requester, On Behalf Of, Traverler
                                                dataMail.mail_to = initiator_mail;
                                                dataMail.mail_cc = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) + (requester_mail + on_behalf_of_mail) + traveler_mail;

                                                var resMailShowCase = $"To: {initiator_mail?.ToLower()} (Initiator)";
                                                resMailShowCase += $"<br>Cc: {super_admin_mail?.ToLower()} (Super Admin)";
                                                resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                                resMailShowCase += $", {requester_mail?.ToLower()} (Requester)";
                                                resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";

                                                dataMail.mail_show_case = resMailShowCase;

                                                // OB/LBYYMMXXXX : Please initiate a request for business travel.
                                                dataMail.mail_subject = value.id + " : Please initiate a request for business travel.";

                                                sDear = "Dear " + user_initiator_display + ",";

                                                sDetail = "Please initiate a request for business travel. To view the details, click ";
                                                sDetail += "<a href='" + (LinkLogin + "").Replace("###", value.id) + "'>" + value.id + "</a>";
                                            }

                                            //Title: [Name of Title]
                                            //Business Date : [Date from to]
                                            //Location : [OB: Country, City, Location], [LB: Province, Location] 
                                            sTitle = value.topic_of_travel ?? "";
                                            sBusinessDate = dateFromTo(value.business_date.start, value.business_date.stop) ?? "";
                                            if (value.type == "local" || value.type == "localtraining")
                                            {
                                                //DevFix 20210330 1502 แก้ไข Location   
                                                sql = @" select distinct to_char(s.id) as id, e.PV_NAME as name1, a.CITY_TEXT as name2, '' as name3, '' as name4   
                from BZ_DOC_TRAVELER_EXPENSE a 
                left join ( select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text
                ) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
                and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text
                left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid  
                left join BZ_MASTER_CONTINENT c on a.CTN_ID = c.CTN_ID   
                left join BZ_MASTER_PROVINCE e on a.PV_ID = e.PV_ID 
                where a.DH_CODE = :doc_id and a.dte_status = 1 order by s.id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (temp != null && temp.Count() > 0)
                                                {
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = ""; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
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
                                                    if (temp.Count == 1)
                                                    {
                                                        sLocation = temp[0].name1 + "/" + temp[0].name2;
                                                    }
                                                    else
                                                    {
                                                        sLocation = "";
                                                        foreach (var item in temp)
                                                        {
                                                            if (sLocation == "") { sLocation = " "; } else { sLocation += ","; }
                                                            sLocation += item.name1 + "/" + item.name2;
                                                        }
                                                    }
                                                }
                                            }

                                            var iNo = 1;


                                            #region set mail
                                            //#region Build Mail Body
                                            //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                            //dataMail.mail_body += "    </div>";

                                            //// Add info box with title, date, location
                                            //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                            //dataMail.mail_body += "    </div>";

                                            //// สร้างตาราง traveler list
                                            //if (tempTravel != null && tempTravel.Count > 0)
                                            //{
                                            //    dataMail.mail_body += "<div style='margin-bottom: 20px;'>";
                                            //    dataMail.mail_body += "<p style = 'font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p> ";
                                            //    dataMail.mail_body += "    <table style='width: auto ; border-collapse: collapse; font-family: Aptos; font-size: 14px;'>";
                                            //    dataMail.mail_body += "        <thead>";
                                            //    dataMail.mail_body += "            <tr style='background-color:  #A7D0F0;'>";
                                            //    dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                            //    dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                            //    dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                            //    dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                            //    dataMail.mail_body += "            </tr>";
                                            //    dataMail.mail_body += "        </thead>";
                                            //    dataMail.mail_body += "        <tbody>";

                                            //    for (int j = 0; j < tempTravel.Count; j++)
                                            //    {
                                            //        var item = tempTravel[j];
                                            //        dataMail.mail_body += "        <tr>";
                                            //        dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{j + 1}</td>";
                                            //        dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                            //        dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                            //        dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                            //        dataMail.mail_body += "        </tr>";
                                            //    }

                                            //    dataMail.mail_body += "        </tbody>";
                                            //    dataMail.mail_body += "    </table>";
                                            //    dataMail.mail_body += "</div>";
                                            //}

                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                            //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                            //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "</div>";
                                            //#endregion
                                            dataMail.mail_body = $@"<head>  <meta charset='UTF-8'></head><table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

            <table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                            if (tempTravel != null && tempTravel.Count > 0)
                                            {
                                                dataMail.mail_body += @"
            <table width='auto' cellpadding='0' cellspacing='0' border='0' style='margin-bottom: 20px;'>
                <tr><td style='font-weight: bold; font-size: 14px; padding-bottom: 10px;'>Traveler List:</td></tr>
                <tr><td>
                    <table cellpadding='0' cellspacing='0' border='1' style='border-collapse: collapse; font-size: 14px; width: 100%;'>
                        <tr style='background-color: #A7D0F0;'>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>
                        </tr>";

                                                for (int j = 0; j < tempTravel.Count; j++)
                                                {
                                                    var item = tempTravel[j];
                                                    dataMail.mail_body += $@"
                        <tr>
                            <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{j + 1}</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                        </tr>";
                                                }

                                                dataMail.mail_body += @"
                    </table>
                </td></tr>
            </table>";
                                            }

                                            dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID == doc_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;
                                            #endregion set mail


                                            var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                            if (!string.IsNullOrEmpty(attachments_doc))
                                            {
                                                dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                            }

                                            data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
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
                data.message = ex.StackTrace;
                data.message += "Message:" + ex.Message;

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
            decimal? next_action_status = 31;
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


                    sql = " select to_char(dh_doc_status) as id, '' as name1, '' as name2, '' as name3, '' as name4 from bz_doc_head h where h.dh_code = :doc_id ";
                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var docHeadStatus = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                    var pf_doc_id = docHeadStatus[0].id.Substring(0, 1);

                    string requester_name = "";

                    #region DevFix 20200911 0000 
                    var Tel_Services_Team = "";
                    var Tel_Call_Center = "";

                    getTelServicesTeamCallCenter(ref Tel_Services_Team, ref Tel_Call_Center);

                    #endregion DevFix 20200911 0000 

                    #region DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin  
                    //PMSV Admin	PMDV Admin
                    string super_admin_mail = "";
                    string pmsv_admin_mail = "";
                    string pmdv_admin_mail = "";
                    string requester_mail = "";
                    string on_behalf_of_mail = "";
                    string traveler_mail = "";
                    string initiator_mail = "";
                    string line_approver_mail = "";
                    string cap_approver_mail = "";

                    //  super_admin_mail = get_mail_group_admin(context);
                    super_admin_mail = mail_group_admin(context, "super_admin");

                    //กรณีที่เป็น pmdv admin, pmsv_admin
                    pmsv_admin_mail = mail_group_admin(context, "pmsv_admin");
                    if (doc_id.IndexOf("T") > -1)
                    {
                        pmdv_admin_mail += mail_group_admin(context, "pmdv_admin");
                    }
                    get_mail_requester_in_doc(context, doc_id, ref requester_name, ref requester_mail, ref on_behalf_of_mail);
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

                        // Update approver statuses
                        sql = "update BZ_DOC_TRAVELER_APPROVER set ";
                        sql += " DTA_DOC_STATUS = CASE WHEN DTA_TYPE = '1' THEN 31 ELSE 41 END, "; // Line=31, CAP=41
                        sql += " DTA_APPR_STATUS = null, DTA_APPR_REMARK = null ";
                        sql += " WHERE DH_CODE = :doc_id AND DTA_STATUS = 1";

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                        //var employeeList = new List<employeeDoc2>();
                        #region employee

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
                        sql += " inner join VW_BZ_USERS U on tv.DTE_Emp_Id = u.employeeid ";
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
                    query = @"SELECT '' as COST_CENTER, B.DTE_EMP_ID as ORG_ID, '' as OTYPE, '' as COM_CODE, A.SH,A.VP,A.AEP,A.EVP,A.SEVP,A.CEO
                                    FROM VW_BZ_MASTER_COSTCENTER_ORG a
                                    INNER JOIN BZ_DOC_TRAVELER_EXPENSE B ON  A.COST_CENTER = B.DTE_COST_CENTER
                                    WHERE B.DH_CODE = :doc_id ";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var masterCostCenterList1 = context.MasterCostCenterList.FromSqlRaw(query, parameters.ToArray()).ToList();


                    //กรณีที่ตรวจสอบระดับ SEVP แต่ไม่มี Cost center ให้ไปหา CEO
                    //query = "select null as SEVP, employeeid as CEO from  vw_bz_users a  where  POSCAT = 'MD' and department is null and sections is null ";
                    //var masterCostCenterList2 = context.Database.SqlQuery<MasterCostCenter>(query).ToList();


                    query = "select '' as COST_CENTER, '' as ORG_ID, '' as OTYPE, '' as COM_CODE, '' as SH,'' as VP,'' as AEP,'' as EVP,  null as SEVP, employeeid as CEO from  vw_bz_users a  where  POSCAT = 'MD' and department is null and sections is null ";

                    parameters = new List<OracleParameter>();
                    //parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var masterCostCenterList2 = context.MasterCostCenterList.FromSqlRaw(query, parameters.ToArray()).ToList();


                    #endregion DevFix 20200827 ตรวจสอบ position approver  


                    //#region DevFix 20210810 เพิ่มรายชื่อ userเพื่อใช้ในเงื่อนไขลำดับแค่ CAP ของ Local   
                    //query = "SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGNAME, MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO FROM VW_BZ_USERS";
                    //parameters = new List<OracleParameter>();
                    //var usersList = context.TravelerUsersCAPModelList.FromSqlRaw(query, parameters.ToArray()).ToList();
                    //#endregion DevFix 20210810 เพิ่มรายชื่อ userเพื่อใช้ในเงื่อนไขลำดับแค่ CAP ของ Local  

                    using (var transaction = context.Database.BeginTransaction())
                    {
                        sql = "SELECT   a.user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, b.email email ";
                        sql += " FROM bz_login_token a left join vw_bz_users b on a.user_id=b.employeeid ";
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
                            //Devfix 20250327 case revise => cleaer def BZ_DOC_TRAVELER_APPROVER 
                            doc_head_search.DH_VERSION = null;

                            old_action_status = doc_head_search.DH_DOC_STATUS;
                            var _old_action_status = old_action_status?.ToString() ?? "";
                            if (_old_action_status?.ToString().Length > 1)
                            {
                                prefix_old_doctype = _old_action_status?.Substring(0, 1)?.ToString() ?? "";
                            }

                            //if (prefix_old_doctype == "2" || prefix_old_doctype == "3")
                            //{
                            next_topno = 3;
                            next_action_status = 31; // Pending by Line Approver
                                                     //}
                                                     //else
                                                     //{
                                                     //    next_topno = 4;
                                                     //    next_action_status = 41; // Pending by CAP Approver 
                                                     //}


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
                                    //if (string.IsNullOrEmpty(doc_head_search.DH_INITIATOR_EMPID))
                                    //    expense_status = 11;
                                    //else
                                    //    expense_status = 22;

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
                                //if (doc_head_search.DH_DOC_STATUS >= 21 && doc_head_search.DH_DOC_STATUS <= 22)
                                //{
                                doc_head_search.DH_DOC_STATUS = 31; //Pending for Line Approver

                                //}
                                //else
                                //{
                                //    // เคยส่งไป tab 3 แล้ว  
                                //}


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
                                        DTE_EXPENSE_CONFIRM = 1,";

                                //DevFix 20250129 0000 เพิ่ม exchange_date,exchange_rate,exchange_currency
                                sql += @" exchange_date = :exchange_date,";
                                sql += @" exchange_rate = :exchange_rate,";
                                sql += @" exchange_currency = :exchange_currency";

                                // ตรวจสอบเงื่อนไขสำหรับการอัปเดตเพิ่มเติม
                                if (value.action.type == "5")
                                {
                                    //if (prefix_old_doctype == "3")
                                    //{
                                    //    sql += @"
                                    //         , DTE_APPR_STATUS = CASE WHEN DTE_APPR_STATUS = 23 THEN 31 ELSE DTE_APPR_STATUS END
                                    //         , DTE_APPR_OPT = CASE WHEN DTE_APPR_STATUS = 23 THEN '' ELSE DTE_APPR_OPT END
                                    //         , DTE_APPR_REMARK = CASE WHEN DTE_APPR_STATUS = 23 THEN '' ELSE DTE_APPR_REMARK END  ";
                                    //}
                                    //else if (prefix_old_doctype == "4")
                                    //{
                                    //    sql += @"
                                    //        , DTE_CAP_APPR_STATUS = CASE WHEN DTE_CAP_APPR_STATUS = 23 THEN 41 ELSE DTE_CAP_APPR_STATUS END
                                    //        , DTE_CAP_APPR_OPT = CASE WHEN DTE_CAP_APPR_STATUS = 23 THEN '' ELSE DTE_CAP_APPR_OPT END 
                                    //        , DTE_CAP_APPR_REMARK = CASE WHEN DTE_CAP_APPR_STATUS = 23 THEN '' ELSE DTE_CAP_APPR_REMARK END  ";
                                    //}
                                    //else
                                    //{
                                    //    sql += " , DTE_APPR_STATUS = :expense_status";
                                    //} 

                                    sql += " , DTE_APPR_STATUS = 31, DTE_APPR_OPT = '' , DTE_APPR_REMARK = '' ";
                                    sql += " , DTE_CAP_APPR_STATUS = 41, DTE_CAP_APPR_OPT = '' , DTE_CAP_APPR_REMARK = '' ";
                                }
                                sql += " WHERE DTE_TOKEN = :ref_id";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("expen_upd_token", expen_upd_token, "char"));
                                parameters.Add(context.ConvertTypeParameter("inx", inx, "int"));
                                parameters.Add(context.ConvertTypeParameter("air_ticket", item.air_ticket, "char"));
                                parameters.Add(context.ConvertTypeParameter("accommodation", item.accommodation, "char"));
                                parameters.Add(context.ConvertTypeParameter("allowance", item.allowance, "char"));
                                parameters.Add(context.ConvertTypeParameter("allowance_day", item.allowance_day, "number"));
                                parameters.Add(context.ConvertTypeParameter("allowance_night", item.allowance_night, "number"));
                                parameters.Add(context.ConvertTypeParameter("clothing_expense", item.clothing_expense, "char"));
                                parameters.Add(context.ConvertTypeParameter("passport_expense", item.passport_expense, "char"));
                                parameters.Add(context.ConvertTypeParameter("visa_fee", item.visa_fee, "char"));
                                parameters.Add(context.ConvertTypeParameter("travel_insurance", item.travel_insurance, "char"));
                                parameters.Add(context.ConvertTypeParameter("transportation", item.transportation, "char"));
                                parameters.Add(context.ConvertTypeParameter("miscellaneous", item.miscellaneous, "char"));
                                parameters.Add(context.ConvertTypeParameter("total_expenses", item.total_expenses, "number"));
                                parameters.Add(context.ConvertTypeParameter("registration_fee", item.registration_fee, "char"));
                                parameters.Add(context.ConvertTypeParameter("clothing_valid", chkDateSQL_All(item.clothing_valid ?? ""), "date"));
                                parameters.Add(context.ConvertTypeParameter("passport_valid", chkDateSQL_All(item.passport_valid ?? ""), "date"));

                                //DevFix 20250129 0000 เพิ่ม exchange_date,exchange_rate,exchange_currency
                                parameters.Add(context.ConvertTypeParameter("exchange_date", item.exchange_date, "char"));
                                parameters.Add(context.ConvertTypeParameter("exchange_rate", item.exchange_rate, "char"));
                                parameters.Add(context.ConvertTypeParameter("exchange_currency", item.exchange_currency, "char"));

                                //if (value.action.type == "5")
                                //{
                                //    if (!(prefix_old_doctype == "3" || prefix_old_doctype == "4"))
                                //    {
                                //        parameters.Add(context.ConvertTypeParameter("expense_status", expense_status, "char"));
                                //    }
                                //}
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

                            //if (prefix_old_doctype == "2" || prefix_old_doctype == "3")
                            dta_type_check = "1"; // line approver
                                                  //else
                                                  //    dta_type_check = "2"; // cap

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

                                    sql = "delete BZ_DOC_TRAVELER_APPROVER where DH_CODE= :doc_id  ";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
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
                                if (listFind != null && listFind.Count() > 0 && !bstep_approver)
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

                                    sql = "update BZ_DOC_TRAVELER_APPROVER set DTA_UPDATE_TOKEN= :token_update ";
                                    parameters.Add(context.ConvertTypeParameter("token_update", item.token_update, "char"));

                                    if (listFind[0].DTA_DOC_STATUS != null && (listFind[0].DTA_DOC_STATUS == 32 || listFind[0].DTA_DOC_STATUS == 42))
                                    {

                                    }
                                    else
                                    {
                                        sql += " , DTA_ID= :line_id";
                                        parameters.Add(context.ConvertTypeParameter("line_id", item.line_id, "int"));


                                        //DevFix 20250129 0000 ยกเลิกขั้นตอนนี้ เนื่องจากต้องผ่านการ approve ใหม่ทั้งหมด
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
                                                            //check_add_action = false;
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
                                    sql += " ,DTA_APPR_LEVEL = :appr_level ";
                                    parameters.Add(context.ConvertTypeParameter("appr_level", appr_level, "char"));

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
                                //                                else
                                //                                {
                                //                                    // แก้ไขการสร้าง line_id ให้ไม่ซ้ำ โดยตรวจสอบจากค่าสูงสุดที่มีอยู่แล้ว

                                //                                    string maxIdSql = "SELECT NVL(MAX(DTA_ID), 0) AS DTA_ID FROM BZ_DOC_TRAVELER_APPROVER WHERE DH_CODE = :doc_id";
                                //                                    var maxIdParams = new List<OracleParameter>
                                //{
                                //   context.ConvertTypeParameter("doc_id", doc_id, "char")
                                //};

                                //                                    // Execute SQL and get the result
                                //                                    var result = context.Database.ExecuteSqlRaw(maxIdSql, maxIdParams.ToArray());

                                //                                    // หลังจากนี้ถ้าใช้ ExecuteSqlRaw() จะไม่ได้รับค่าผลลัพธ์
                                //                                    // ดังนั้นเราจึงต้องใช้ ExecuteScalar() แทนเพื่อรับค่ากลับจากคำสั่ง SELECT
                                //                                    decimal maxId = context.BZ_DOC_TRAVELER_APPROVER
                                //    .FromSqlRaw(maxIdSql, maxIdParams.ToArray())
                                //    .Select(x => x.DTA_ID) // เลือกเฉพาะฟิลด์ DTA_ID จากผลลัพธ์
                                //    .FirstOrDefault() ?? 0;

                                //                                    inx = maxId + 1;
                                //                                    item.line_id = inx.ToString();
                                //                                    item.record_status = "insert";


                                //                                    string s_next_status = next_action_status.ToString();

                                //                                    var action_status_type = item.type == "2" ? 2 : 1;
                                //                                    if (prefix_old_doctype == "2" || prefix_old_doctype == "3" || prefix_old_doctype == "4")
                                //                                    {
                                //                                        if (item.remark.IndexOf("Endorsed") > -1)
                                //                                        {
                                //                                            action_status_type = 1;
                                //                                        }
                                //                                        else
                                //                                        {
                                //                                            action_status_type = next_action_status == 41 ? 1 : 2;
                                //                                        }

                                //                                        if (item.remark.IndexOf("CAP", StringComparison.OrdinalIgnoreCase) > -1)
                                //                                        {
                                //                                            s_next_status = "41";
                                //                                        }
                                //                                    }

                                //                                    // เพิ่มตรวจสอบก่อนเพิ่ม tempEmpForAction
                                //                                    //if (!tempEmpForAction.Any(p => p.EMP_ID.Equals(item.appr_id)))
                                //                                    //{
                                //                                        tempEmpForAction.Add(new BZ_DOC_ACTION
                                //                                        {
                                //                                            FROM_EMP_ID = traveler_id.ToString(),
                                //                                            ACTION_STATUS = action_status_type,
                                //                                            DA_TOKEN = tokenUpdate,
                                //                                            EMP_ID = item.appr_id
                                //                                        });
                                //                                    //}

                                //                                    var appr_level = item.approve_level ?? "0";

                                //                                    // ใช้ MERGE statement แทน INSERT เพื่อป้องกันการซ้ำ
                                //                                    // Corrected MERGE statement
                                //                                    if (appr_level == null) { appr_level = "0"; }
                                //                                    sql = @"
                                //                INSERT INTO BZ_DOC_TRAVELER_APPROVER 
                                //                (DH_CODE, DTA_ID, DTA_TYPE, DTA_APPR_EMPID, DTA_TRAVEL_EMPID, 
                                //                 DTA_REMARK, DTA_DOC_STATUS, DTA_UPDATE_TOKEN, DTA_APPR_LEVEL,
                                //                 DTA_UPDATE_DATE, DTA_ACTION_STATUS)
                                //                VALUES (:doc_id, :line_id, :type, :appr_id, :emp_id, 
                                //                        :remark, :s_next_status, :token_update, :appr_level,
                                //                        SYSDATE, :action_status)";

                                //                                    parameters = new List<OracleParameter>
                                //            {
                                //                context.ConvertTypeParameter("doc_id", doc_id, "char"),
                                //                context.ConvertTypeParameter("line_id", item.line_id, "int"),
                                //                context.ConvertTypeParameter("type", item.type, "char"),
                                //                context.ConvertTypeParameter("appr_id", item.appr_id, "char"),
                                //                context.ConvertTypeParameter("emp_id", item.emp_id, "char"),
                                //                context.ConvertTypeParameter("remark", item.remark, "char"),
                                //                context.ConvertTypeParameter("s_next_status", s_next_status, "int"),
                                //                context.ConvertTypeParameter("token_update", item.token_update, "char"),
                                //                context.ConvertTypeParameter("appr_level", appr_level, "char"),
                                //                context.ConvertTypeParameter("action_status", action_status_type.ToString(), "char")
                                //            };

                                //                                    try
                                //                                    {
                                //                                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                //                                    }
                                //                                    catch (OracleException ex)
                                //                                    {
                                //                                        Console.WriteLine($"Oracle Error: {ex.Message}");
                                //                                    }
                                //                                }
                                //else
                                //{
                                //    inx++;
                                //    item.line_id = inx.ToString();
                                //    item.record_status = "insert";

                                //    string s_next_status = "null";
                                //    s_next_status = next_action_status.ToString();

                                //    //DevFix 20200828 2157  เฉพาะที่เป็น CAP ให้ update status =2  
                                //    var action_status_type = item.type == "2" ? 2 : 1;
                                //    if (prefix_old_doctype == "2" ||prefix_old_doctype == "3" || prefix_old_doctype == "4")
                                //    {
                                //        if (item.remark.IndexOf("Endorsed") > -1)
                                //        {
                                //            action_status_type = 1;
                                //        }

                                //        else
                                //        {
                                //            if (next_action_status == 41)
                                //            {
                                //                action_status_type = 1;
                                //            }
                                //            else
                                //            {
                                //                //ส่งหา Line
                                //                action_status_type = 2;
                                //            }
                                //        }
                                //        if (item.remark.IndexOf("CAP", StringComparison.OrdinalIgnoreCase) > -1)
                                //        {
                                //            s_next_status = "41";
                                //        }

                                //    }

                                //    var check = tempEmpForAction.Where(p => p.EMP_ID.Equals(item.appr_id));
                                //    if (check != null && check.Count() <= 0)
                                //    {
                                //        tempEmpForAction.Add(new BZ_DOC_ACTION
                                //        {
                                //            //DevFix 20210718 0000 เพิ่มเก็บ ข้อมูล Traveler ID
                                //            FROM_EMP_ID = traveler_id.ToString(),

                                //            //DevFix 20200828 2157  เฉพาะที่เป็น CAP ให้ update status =2 
                                //            //ACTION_STATUS = item.type == "2" ? 2 : 1,
                                //            ACTION_STATUS = action_status_type,
                                //            DA_TOKEN = tokenUpdate,
                                //            EMP_ID = item.appr_id // คนอนุมัติ
                                //        });

                                //    }


                                //    #region  DevFix 20200914 1200 เพิ่ม position ของ apprver เช่น EVP = 1, SEVP = 2, CEO = 3 ??? เหลือกรณีที่ Cost Center  
                                //    var appr_level = "0";
                                //    var bcheck_non_sevp = false;
                                //    var masterCostCenterList = masterCostCenterList1;

                                //    appr_level = item.approve_level;

                                //    if (item.type == "2")
                                //    {
                                //    }
                                //    #endregion DevFix 20200914 1200 เพิ่ม position ของ apprver เช่น EVP = 1, SEVP = 2, CEO = 3

                                //    if (appr_level == null) { appr_level = "0"; }
                                //    sql = "insert into BZ_DOC_TRAVELER_APPROVER (DH_CODE, DTA_ID, DTA_TYPE, DTA_APPR_EMPID, DTA_TRAVEL_EMPID, DTA_REMARK, DTA_DOC_STATUS, DTA_UPDATE_TOKEN,DTA_APPR_LEVEL) ";
                                //    sql += " values (:doc_id, :line_id, :type, :appr_id, :emp_id, :remark, :s_next_status, :token_update, :appr_level ) ";

                                //    parameters = new List<OracleParameter>();
                                //    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("line_id", item.line_id, "int"));
                                //    parameters.Add(context.ConvertTypeParameter("type", item.type, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("appr_id", item.appr_id, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("emp_id", item.emp_id, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("remark", item.remark, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("s_next_status", s_next_status, "int"));
                                //    parameters.Add(context.ConvertTypeParameter("token_update", item.token_update, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("appr_level", appr_level, "char"));
                                //    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                //}

                            }

                            if (value.action.type == "5")
                            {
                                if (value.action.type == "3") { }
                                else
                                {
                                    // update inaction ในกรณีที่เป็นรายการที่อยู่บนหน้าจอ 
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
                                    sql = "update BZ_DOC_TRAVELER_APPROVER set DTA_ACTION_STATUS = 2 where DTA_ACTION_STATUS in (1,4) and DH_CODE= :doc_id ";
                                    //if (dta_type_check == "2")
                                    //{
                                    //    sql += " and DTA_TYPE = 2";
                                    //}
                                    //else
                                    //{
                                    //    sql += " and DTA_TYPE = 1";
                                    //}
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                            }
                            #endregion

                            #region "#### SUBMIT type 5 && BZ_DOC_ACTION ####" 
                            if (value.action.type == "5") // submit
                            {
                                ////DevFix 20250129 0000 ยกเลิกขั้นตอนนี้ เนื่องจากต้องผ่านการ approve ใหม่ทั้งหมด
                                //if (prefix_old_doctype == "2")
                                //{
                                //    // ???
                                //    //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = '" + value.doc_id + "'");
                                //    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = :doc_id";

                                //    parameters = new List<OracleParameter>();
                                //    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                //    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                //}
                                //else if (prefix_old_doctype == "3")
                                //{
                                //    //update กรณีที่มีการ revise กลับมา action_status ให้เป็น 2 เนื่องจากอาจจะมีกรณีที่ revise แล้ว ได้ line approve ใหม่ --> line approve เก่าไม่มีการส่งไป update ??? 
                                //    //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = '" + value.doc_id + "'");
                                //    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = :doc_id ";

                                //    parameters = new List<OracleParameter>();
                                //    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                //    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                //}
                                //else if (prefix_old_doctype == "4")
                                //{
                                //    //update กรณีที่มีการ revise กลับมา action_status ให้เป็น 2 เนื่องจากอาจจะมีกรณีที่ revise แล้ว ได้ line approve ใหม่ --> line approve เก่าไม่มีการส่งไป update ??? 
                                //    //context.Database.ExecuteSqlCommand("update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = '" + value.doc_id + "'");
                                //    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE doc_status < 30 and action_status = 1 and DH_CODE = :doc_id";

                                //    parameters = new List<OracleParameter>();
                                //    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                //    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                //}

                                if (prefix_old_doctype == "2" || prefix_old_doctype == "3" || prefix_old_doctype == "4")
                                {
                                    sql = @"update BZ_DOC_ACTION set action_status = 2 WHERE action_status = 1 and DH_CODE = :doc_id";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                    sql = @"update BZ_DOC_TRAVELER_APPROVER set DTA_APPR_STATUS = null, DTA_APPR_REMARK = null ,DTA_DOC_STATUS = CASE WHEN DTA_TYPE = 1 THEN 31 ELSE 41 END,
                            DTA_STATUS = 1,  
                            DTA_APPR_POS = null,
                            DTA_UPDATE_DATE = null WHERE DH_CODE = :doc_id";
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
                                        sTitle = doc_head_search.DH_TOPIC ?? "";

                                        if (doc_head_search.DH_BUS_FROMDATE != null)
                                        {
                                            sBusinessDate = dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                        }

                                        if (value.action.type == "3") { next_action_status = 23; }

                                        sql = @"  select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1 ,a.city_text as name2, '' as name3, '' as name4    
                                                    FROM bz_doc_traveler_expense a 
                                                    inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
                                                    inner join VW_BZ_USERS U on a.DTE_Emp_Id = u.employeeid
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
                                                sLocation = temp[0].name1 + "/" + temp[0].name2;
                                            }
                                            else
                                            {
                                                sLocation = "";
                                                foreach (var item in temp)
                                                {
                                                    if (sLocation == "") { sLocation = ""; } else { sLocation += ","; }
                                                    sLocation += item.name1 + "/" + item.name2;
                                                }
                                            }
                                        }

                                        sendEmailModel dataMail = new sendEmailModel();
                                        if (value.action.type == "5") // submit
                                        {
                                            #region "#### SUBMIT ####"
                                            // ดึงข้อมูลผู้ใช้ทั้งหมด
                                            sql = "SELECT EMPLOYEEID user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, b.email email FROM VW_BZ_USERS b";
                                            var userList = context.SearchUserNameList.FromSqlRaw(sql).ToList();

                                            #region DevFix 20210806 0000 กรณีที่ไม่ใช่ admin ให้ตรวจสอบ emp ว่าเป็น traveler   
                                            sql = @"SELECT DISTINCT to_char(t.user_type) as role_type, t.emp_id as user_id, '' as user_name, '' as email
                                                        FROM ( SELECT dh_code as doc_id, 2 as user_type, a.dta_appr_empid as emp_id
                                                               FROM bz_doc_traveler_approver a WHERE a.dta_type = 1 ) t 
                                                        INNER JOIN ( SELECT dh_code as doc_id, 3 as user_type, a.dta_appr_empid as emp_id
                                                                     FROM bz_doc_traveler_approver a WHERE a.dta_type = 2 ) t1 
                                                        ON t.doc_id = t1.doc_id AND t.emp_id = t1.emp_id
                                                        WHERE t.doc_id = :doc_id 
                                                        ORDER BY role_type DESC";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var emp_type = context.SearchUserEmailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                            #endregion

                                            // ดึงเฉพาะ line approver ระดับ 1 ที่ไม่ใช่ CAP ด้วย
                                            // ดึง approver level 1 (action_status = 2) ที่ไม่อยู่ใน level 2
                                            var level2EmpIds = context.BZ_DOC_TRAVELER_APPROVER
                                                .Where(x => x.DH_CODE == doc_id && x.DTA_TYPE == "1" && x.DTA_APPR_LEVEL == 2)
                                                .Select(x => x.DTA_APPR_EMPID)
                                                .ToHashSet(); // ใช้ HashSet เพื่อความเร็วในการเช็ค

                                            var validApproverEmpIds = context.BZ_DOC_TRAVELER_APPROVER
                                                .Where(x =>
                                                    x.DH_CODE == doc_id &&
                                                    x.DTA_TYPE == "1" &&
                                                    x.DTA_APPR_LEVEL == 1 &&
                                                    x.DTA_DOC_STATUS == 31 &&
                                                    !level2EmpIds.Contains(x.DTA_APPR_EMPID)
                                                )
                                                .Select(x => x.DTA_APPR_EMPID)
                                                .Distinct()
                                                .ToHashSet(); // ใช้ HashSet เพื่อ lookup เร็วขึ้น

                                            // กรอง tempEmpForAction ให้เหลือเฉพาะคนที่ตรงตามเงื่อนไข
                                            tempEmpForAction = tempEmpForAction
                                                .Where(x => x.ACTION_STATUS == 1 && validApproverEmpIds.Contains(x.EMP_ID))
                                                .ToList();

                                            var tempEmpForAction_def = new List<BZ_DOC_ACTION>();
                                            for (int i = 0; i < tempEmpForAction.Count; i++)
                                            {
                                                string action_status = tempEmpForAction[i].ACTION_STATUS.ToString();
                                                string emp_select = tempEmpForAction[i].EMP_ID.ToString();

                                                if (action_status == "2") { continue; }
                                                var dta_type = 1;

                                                // ตรวจสอบว่าเป็น approver level 1 เท่านั้น
                                                //                                            var approverList = context.BZ_DOC_TRAVELER_APPROVER
                                                //.Where(a => a.DH_CODE == doc_id && a.DTA_APPR_EMPID == emp_select).ToList();

                                                //                                            var isLevel1Approver = approverList.Any(a => a.DTA_TYPE == "1" && a.DTA_APPR_LEVEL == 1);

                                                //                                            if (!isLevel1Approver)
                                                //                                            {
                                                //                                                continue;
                                                //                                            }

                                                //sql = @"SELECT DISTINCT a.dta_appr_empid AS user_id, NVL(b.ENTITLE,'') || ' ' || b.ENFIRSTNAME || ' ' || b.ENLASTNAME AS user_name, b.email AS email
                                                //            FROM bz_doc_traveler_approver a 
                                                //            LEFT JOIN vw_bz_users b ON a.dta_appr_empid = b.employeeid
                                                //            WHERE a.dh_code = :doc_id AND a.dta_type = 1 AND a.dta_action_status = 2
                                                //            AND a.dta_appr_level = 1 AND a.dta_appr_empid NOT IN (
                                                //            SELECT dta_appr_empid
                                                //            FROM bz_doc_traveler_approver
                                                //            WHERE dh_code = :doc_id AND dta_type = 1 AND dta_appr_level = 2 ) ";
                                                //            //AND a.dta_appr_empid = :emp_id";

                                                //parameters = new List<OracleParameter>();
                                                //parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                //parameters.Add(context.ConvertTypeParameter("emp_id", tempEmpForAction[i].EMP_ID, "char")); // <- ใส่ EMP_ID แต่ละคน

                                                //var empapp = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                //if (empapp.Count == 0) continue;

                                                //string emp_select = tempEmpForAction[i].EMP_ID ?? "";

                                                sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
                 , b.employeeid as name3, b.orgname as name4
                 from BZ_DOC_TRAVELER_EXPENSE a 
                 left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid
                 left join (select min(dte_id) as id, dh_code, dte_emp_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id) s 
                 on a.dh_code = s.dh_code and a.dte_emp_id = s.dte_emp_id  
                 where a.dh_code = :doc_id 
                 and exists (
                     select 1 from BZ_DOC_TRAVELER_APPROVER 
                     where dh_code = a.dh_code 
                     and dta_travel_empid = a.DTE_EMP_ID
                     and dta_type = 1 
                     and dta_appr_level = 1 
                     and dta_appr_empid = :emp_select
                 )
                 order by s.id";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("emp_select", emp_select, "char"));
                                                var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                traveler_mail = "";
                                                if (tempTravel != null)
                                                {
                                                    foreach (var item in tempTravel)
                                                    {

                                                        if (!string.IsNullOrEmpty(item.name2))
                                                        {
                                                            traveler_mail += item.name2 + ";";
                                                        }

                                                    }
                                                }



                                                var empapp = userList.Where(p => p.user_id == emp_select).ToList();
                                                if (empapp.Count == 0) continue;

                                                dataMail.mail_body = "";
                                                line_approver_mail = empapp[0].email ?? "";

                                                var appr_id = tempEmpForAction[i].EMP_ID.ToString();
                                                var traveler_id = tempEmpForAction[i].FROM_EMP_ID.ToString();

                                                if (tempEmpForAction_def.Any(p => p.EMP_ID.Equals(appr_id))) continue;

                                                tempEmpForAction_def.Add(new BZ_DOC_ACTION
                                                {
                                                    EMP_ID = appr_id
                                                });

                                                #region ตรวจสอบ approver role
                                                var approver2role = false;
                                                if (emp_type != null && emp_type.Count() > 0)
                                                {
                                                    var check_approver2role_def = emp_type.Where(p => p.user_id.Equals(appr_id) && p.role_type == "2");
                                                    if (check_approver2role_def != null && check_approver2role_def.Count() > 0)
                                                    {
                                                        approver2role = true;
                                                    }
                                                }
                                                #endregion

                                                // ตั้งค่าเมลล์
                                                dataMail.mail_to = line_approver_mail;
                                                dataMail.mail_cc = (requester_mail + on_behalf_of_mail) + traveler_mail + (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail);

                                                var resMailShowCase = $"To: {line_approver_mail} (Line Approval)";
                                                resMailShowCase += $"<br>Cc: {requester_mail?.TrimStart(';').ToLower()} (Requester)";
                                                resMailShowCase += $", {on_behalf_of_mail?.TrimStart(';').ToLower()} (On Behalf Of)";
                                                if (!string.IsNullOrEmpty(traveler_mail))
                                                {
                                                    resMailShowCase += $", {traveler_mail?.TrimStart(';').ToLower()} (Traveller)";
                                                }
                                                resMailShowCase += $", {super_admin_mail?.TrimStart(';').ToLower()} (Super Admin)";
                                                resMailShowCase += $", {pmsv_admin_mail?.TrimStart(';').ToLower()} (PMSV Admin)";
                                                dataMail.mail_show_case = resMailShowCase;

                                                dataMail.mail_subject = value.doc_id + (approver2role ? " : Please endorse business travel request as line manager / CAP."
                                                                                                   : " : Please endorse business travel request as line manager.");
                                                sDear = "Dear " + empapp[0].user_name + ",";
                                                sDetail = "Please endorse business travel " + (approver2role ? "as line manager / CAP." : "as line manager.") +
                                                         " To view the details, click ";
                                                sDetail += "<a href='" + (LinkLogin + "ii").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                                dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

            <table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                                if (tempTravel != null && tempTravel.Count > 0)
                                                {
                                                    dataMail.mail_body += @"
            <table width='auto' cellpadding='0' cellspacing='0' border='0' style='margin-bottom: 20px;'>
                <tr><td style='font-weight: bold; font-size: 14px; padding-bottom: 10px;'>Traveler List:</td></tr>
                <tr><td>
                    <table cellpadding='0' cellspacing='0' border='1' style='border-collapse: collapse; font-size: 14px; width: 100%;'>
                        <tr style='background-color: #A7D0F0;'>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>
                        </tr>";

                                                    for (int j = 0; j < tempTravel.Count; j++)
                                                    {
                                                        var item = tempTravel[j];
                                                        dataMail.mail_body += $@"
                        <tr>
                            <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{j + 1}</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                        </tr>";
                                                    }

                                                    dataMail.mail_body += @"
                    </table>
                </td></tr>
            </table>";
                                                }

                                                dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";




                                                // #region Build Mail Body
                                                // dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                                // dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                // dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                // dataMail.mail_body += "    </div>";
                                                // dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                // dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                // dataMail.mail_body += "    </div>";

                                                // // Add info box with title, date, location
                                                // dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                                // dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                                // dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                                // dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                                // dataMail.mail_body += "    </div>";

                                                // // สร้างตาราง traveler list
                                                // if (tempTravel != null && tempTravel.Count > 0)
                                                // {
                                                //     dataMail.mail_body += "<div style='margin-bottom: 20px;'>";
                                                //     dataMail.mail_body += "<p style = 'font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p> ";
                                                //     dataMail.mail_body += "    <table style='width: auto ; border-collapse: collapse; font-family: Aptos; font-size: 14px;'>";
                                                //     dataMail.mail_body += "        <thead>";
                                                //     dataMail.mail_body += "            <tr style='background-color: #A7D0F0;'>";
                                                //     dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                //     dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                //     dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                //     dataMail.mail_body += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                //     dataMail.mail_body += "            </tr>";
                                                //     dataMail.mail_body += "        </thead>";
                                                //     dataMail.mail_body += "        <tbody>";

                                                //     for (int j = 0; j < tempTravel.Count; j++)
                                                //     {
                                                //         var item = tempTravel[j];
                                                //         dataMail.mail_body += "        <tr>";
                                                //         dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc; text-align: center'>{j + 1}</td>";
                                                //         dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                                //         dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                                //         dataMail.mail_body += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                                //         dataMail.mail_body += "        </tr>";
                                                //     }

                                                //     dataMail.mail_body += "        </tbody>";
                                                //     dataMail.mail_body += "    </table>";
                                                //     dataMail.mail_body += "</div>";
                                                // }

                                                // dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                                // dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                // dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                // dataMail.mail_body += "    </div>";
                                                // dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                                // dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                                // dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                // dataMail.mail_body += "    </div>";
                                                // dataMail.mail_body += "</div>";
                                                // #endregion

                                                var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID == next_action_status).FirstOrDefaultAsync();
                                                dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                dataMail.doc_id = doc_id;

                                                var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                                if (!string.IsNullOrEmpty(attachments_doc))
                                                {
                                                    dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                                }

                                                data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                            }
                                            #endregion
                                        }
                                        else if (value.action.type == "2") // reject
                                        {
                                            #region "#### REJECT ####"  
                                            sql = @"SELECT u.EMAIL as initial_mail
                            FROM BZ_DOC_HEAD d
                            JOIN vw_bz_users u ON d.DH_INITIATOR_EMPID = u.EMPLOYEEID
                            WHERE d.DH_CODE = :doc_id";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var resultinitiator = context.TempEMailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (resultinitiator != null && resultinitiator.Count > 0)
                                            {
                                                initiator_mail = resultinitiator[0].email;
                                            }
                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2  
, b.employeeid as name3, b.orgname as name4
from BZ_DOC_TRAVELER_EXPENSE a left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid 
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
                                                    traveler_mail += item.name2 + ";";
                                                }
                                            }
                                            //to : Requester
                                            //cc : On Behalf Of, Super admin    
                                            dataMail.mail_to = requester_mail + traveler_mail + initiator_mail;
                                            dataMail.mail_cc = on_behalf_of_mail + (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail);

                                            var resMailShowCase = $"To :{requester_mail?.ToLower()} (Requester)";
                                            resMailShowCase += $",{traveler_mail?.ToLower()} (Traveler)";
                                            if (!string.IsNullOrEmpty(initiator_mail))
                                            {
                                                resMailShowCase += $",{initiator_mail?.ToLower()} (Initiator)";
                                            }

                                            resMailShowCase += $"<br>Cc :{on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                            resMailShowCase += $",{super_admin_mail?.ToLower()} (Super Admin)";
                                            resMailShowCase += $",{pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                            //resMailShowCase += $",{pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                            dataMail.mail_show_case = resMailShowCase;

                                            dataMail.mail_subject = value.doc_id + " :  The request for business travel has been rejected.";// + User[0].user_name + "";

                                            //sDear = "Dear " + requester_name + ",";
                                            sDear = "Dear All,";
                                            sDetail = "Your business travel request has been reject by " + User[0].user_name + ". To view the details, click ";
                                            sDetail += "<a href='" + (LinkLogin + "i").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                            var iNo = 1;
                                            if (tempTravel != null && tempTravel.Count > 0)
                                            {
                                                sTravelerList = "<div style='margin-bottom: 20px;'>";
                                                sTravelerList += "<p style = 'font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'> Traveler List:</p> ";
                                                sTravelerList += "    <table style='width: auto ; border-collapse: collapse; font-family: Aptos; font-size: 14px;'>";
                                                sTravelerList += "        <thead>";
                                                sTravelerList += "            <tr style='background-color: #A7D0F0;'>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>";
                                                sTravelerList += "            </tr>";
                                                sTravelerList += "        </thead>";
                                                sTravelerList += "        <tbody>";

                                                for (int j = 0; j < tempTravel.Count; j++)
                                                {
                                                    var item = tempTravel[j];
                                                    sTravelerList += "        <tr>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{j + 1}</td>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                                    sTravelerList += "        </tr>";
                                                }

                                                sTravelerList += "        </tbody>";
                                                sTravelerList += "    </table>";
                                                sTravelerList += "</div>";
                                            }

                                            #endregion
                                        }
                                        else if (value.action.type == "3") // revise
                                        {
                                            #region "#### REVISE ####"
                                            //DevFix 20210813 0000 เพิ่ม email เพื่อนำไปใช้ตอน cc   
                                            sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                                    , b.employeeid as name3, b.orgname as name4  
                                                    from BZ_DOC_TRAVELER_EXPENSE a left join vw_bz_users b on a.DTE_EMP_ID = b.employeeid 
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
                                                    traveler_mail += item.name2 + ";";
                                                }
                                            }

                                            sql = "SELECT  employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                            sql += "FROM vw_bz_users b WHERE employeeid= :create_by_emp_id_select ";

                                            var create_by_emp_id_select = doc_head_search.DH_CREATE_BY ?? "";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("create_by_emp_id_select", create_by_emp_id_select, "char"));
                                            var requestor = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            if (requestor != null && requestor?.Count > 0)
                                            {
                                                requester_mail = requestor[0].email ?? "";
                                            }

                                            //to : Requester, Traverler
                                            //cc : On Behalf Of, Super admin 
                                            dataMail.mail_to = requester_mail;
                                            dataMail.mail_cc = on_behalf_of_mail + (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail);

                                            var resMailShowCase = $"To :{requester_mail?.ToLower()} (Requester)";
                                            resMailShowCase += $"<br>Cc :{on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                            resMailShowCase += $",{super_admin_mail?.ToLower()} (Super Admin)";
                                            resMailShowCase += $",{pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                            //resMailShowCase += $",{pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                            resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                            dataMail.mail_show_case = resMailShowCase;


                                            dataMail.mail_subject = value.doc_id + " :  Please revise your request for business travel.";// + User[0].user_name + "";

                                            //sDear = "Dear " + requestor[0].user_name + ",";
                                            sDear = "Dear All,";

                                            sDetail = "Your business travel request has been revise by " + User[0].user_name + ". To view the details, click ";
                                            sDetail += "<a href='" + LinkLogin.Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                            var iNo = 1;
                                            if (tempTravel != null && tempTravel.Count > 0)
                                            {
                                                sTravelerList = "<div style='margin-bottom: 20px;'>";
                                                sTravelerList += "<p style = 'font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'> Traveler List:</p> ";
                                                sTravelerList += "    <table style='width: auto ; border-collapse: collapse; font-family: Aptos; font-size: 14px;'>";
                                                sTravelerList += "        <thead>";
                                                sTravelerList += "            <tr style='background-color: #A7D0F0;'>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                sTravelerList += "                <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>";
                                                sTravelerList += "            </tr>";
                                                sTravelerList += "        </thead>";
                                                sTravelerList += "        <tbody>";

                                                for (int j = 0; j < tempTravel.Count; j++)
                                                {
                                                    var item = tempTravel[j];
                                                    sTravelerList += "        <tr>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{j + 1}</td>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                                    sTravelerList += $"           <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                                    sTravelerList += "        </tr>";
                                                }

                                                sTravelerList += "        </tbody>";
                                                sTravelerList += "    </table>";
                                                sTravelerList += "</div>";
                                            }

                                            #endregion
                                        }


                                        if (value.action.type == "2" || value.action.type == "3")
                                        {
                                            #region set mail
                                            try
                                            {
                                                sReasonRejected = "";
                                                if (value.action.type == "2")
                                                {
                                                    sReasonRejected = "<strong>Reason for Rejection:</strong> " + value.action.remark;
                                                }
                                                else if (value.action.type == "3")
                                                {
                                                    sReasonRejected = "<strong>Reason for Revision:</strong> " + value.action.remark;
                                                }
                                            }
                                            catch { }

                                            #region Build Email Body
                                            dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                            if (!string.IsNullOrEmpty(sReasonRejected))
                                            {
                                                dataMail.mail_body += $@"
            <table width='60%' cellpadding='10' cellspacing='0' border='0' style='background-color: #fff8f8; border-left: 4px solid #ffcccc; margin-bottom: 10px;'>
                <tr><td style='font-size: 14px; color: #d9534f;'>{sReasonRejected}</td></tr>
            </table>";
                                            }

                                            // Traveler List (already generated before as HTML)
                                            dataMail.mail_body += $"<table width='auto' cellpadding='0' cellspacing='0' border='0' style='margin-bottom: 20px;'>{sTravelerList}</table>";

                                            dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";
                                            #endregion

                                            // set status
                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID == next_action_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;

                                            // แนบไฟล์ถ้ามี
                                            var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                            if (!string.IsNullOrEmpty(attachments_doc))
                                            {
                                                dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                            }

                                            // ตรวจสอบสิทธฺ์ที่สามารส่งเมลได้
                                            var sw = new SendEmailServiceTravelerProfile();
                                            data.message = sw.SendMail23FlowTrip(dataMail);
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
            bool newDocNo = false;
            string doc_status = "";
            string user_name = "";
            string user_id = "";
            string user_role = "";
            bool isRoleAdmin = false;
            string sql = "";
            var data = new ResultModel();

            if (value.action == null || string.IsNullOrEmpty(value.action.type))
            {
                data.status = "E";
                data.message = "Action is null!";
                return data;
            }

            try
            {
                var Tel_Services_Team = "";
                var Tel_Call_Center = "";
                getTelServicesTeamCallCenter(ref Tel_Services_Team, ref Tel_Call_Center);

                var iret = 0;
                var doc_id = value.doc_id ?? "";
                var token_login = value.token_login ?? "";

                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var doc_head_search = context.BZ_DOC_HEAD.Find(value.doc_id);
                    if (doc_head_search == null)
                    {
                        data.status = "E";
                        data.message = "Document not found!";
                        return data;
                    }

                    string doc_type = doc_head_search.DH_TYPE;
                    bool notiPassport = (doc_type == "oversea" || doc_type == "overseatraining");

                    string requester_name = "";
                    string super_admin_mail = "";
                    string pmsv_admin_mail = "";
                    string pmdv_admin_mail = "";
                    string requester_mail = "";
                    string on_behalf_of_mail = "";
                    string traveler_mail = "";
                    string line_approver_mail = "";
                    string cap_approver_mail = "";

                    // Get admin emails
                    super_admin_mail = mail_group_admin(context, "super_admin");

                    //  super_admin_mail = get_mail_group_admin(context);
                    pmsv_admin_mail = mail_group_admin(context, "pmsv_admin");
                    if (doc_id.IndexOf("T") > -1)
                    {
                        pmdv_admin_mail += mail_group_admin(context, "pmdv_admin");
                    }
                    get_mail_requester_in_doc(context, doc_id, ref requester_name, ref requester_mail, ref on_behalf_of_mail);

                    // Get document flow type
                    bool type_flow = true;
                    sql = @"SELECT a.DH_TYPE_FLOW as id_key FROM BZ_DOC_HEAD a where a.DH_CODE = :doc_id";
                    var parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                    var docHead = context.TempIdKeyModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                    if (docHead != null && (docHead[0].id_key ?? "1") != "1")
                    {
                        type_flow = false;
                    }

                    // Get user role from token
                    get_user_role_in_token_login(context, token_login, ref user_name, ref user_id, ref user_role);
                    isRoleAdmin = (user_role == "1");

                    // Check if all line approvers have approved
                    var lineApproveList = context.BZ_DOC_TRAVELER_APPROVER
                        .Where(p => p.DH_CODE.Equals(value.doc_id) &&
                                p.DTA_TYPE == "1" &&
                                !(p.DTA_APPR_STATUS.ToLower() == "true") &&
                                !(p.DTA_APPR_EMPID == user_id))
                        .Count();


                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            if (doc_id.IndexOf("T") > -1)
                            {
                                user_role = get_role_admin_in_manage(context, user_id, user_role ?? "");
                            }

                            // Update document header
                            doc_head_search.DH_AFTER_TRIP_OPT1 = retCheckValue(value.after_trip.opt1 ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT2 = retCheckValue(value.after_trip.opt2.status ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT3 = retCheckValue(value.after_trip.opt3.status ?? "");
                            doc_head_search.DH_AFTER_TRIP_OPT2_REMARK = value.after_trip.opt2.remark ?? "";
                            doc_head_search.DH_AFTER_TRIP_OPT3_REMARK = value.after_trip.opt3.remark ?? "";
                            doc_head_search.DH_EXPENSE_OPT1 = retCheckValue(value.checkbox_1.ToString() ?? "");
                            doc_head_search.DH_EXPENSE_OPT2 = retCheckValue(value.checkbox_2.ToString() ?? "");
                            doc_head_search.DH_UPDATE_BY = user_id;
                            doc_head_search.DH_UPDATE_DATE = DateTime.Now;

                            // Determine document status based on action
                            if (value.action.type == "2") // reject
                            {
                                doc_status = "30";
                            }
                            else if (value.action.type == "3") // revise
                            {
                                doc_status = "23";
                            }
                            else if ((value.action.type == "4" || value.action.type == "5")) // approve
                            {
                                doc_status = "32";
                            }

                            var traveler_expen = context.BZ_DOC_TRAVELER_EXPENSE.Where(p => p.DH_CODE.Equals(value.doc_id)).ToList();
                            var traveler_approver_List = context.BZ_DOC_TRAVELER_APPROVER
                                .Where(p => (p.DTA_TYPE == "1" || p.DTA_TYPE == "2") &&
                                        p.DH_CODE.Equals(value.doc_id) &&
                                        p.DTA_STATUS == 1)
                                .ToList();

                            var approverList = new List<SearchUserAllModel>();

                            foreach (var item in value.traveler_summary)
                            {
                                if (item.take_action != "true") continue;

                                string item_ref_id = item.ref_id ?? "";
                                string item_traverler_id = item.traverler_id ?? "";
                                string item_appr_id = item.appr_id ?? "";
                                string action_remark = chkString(value.action.remark);

                                if (doc_status == "30") { item.appr_status = "false"; }
                                string appr_remark = chkString(item.appr_remark) ?? action_remark;

                                // Check if all approvers for this traveler have approved
                                bool bTravelerApproverApproveAll = false;
                                var lineApproveTravelerList = context.BZ_DOC_TRAVELER_APPROVER
                                    .Where(p => p.DH_CODE.Equals(value.doc_id) &&
                                            (p.DTA_TYPE == "1" || p.DTA_TYPE == "2") &&
                                            p.DTA_TRAVEL_EMPID == item_traverler_id &&
                                            !(p.DTA_APPR_STATUS.ToLower() == "true") &&
                                            !(p.DTA_APPR_EMPID == user_id))
                                    .Count();

                                // ตรวจสอบว่าผู้ใช้เป็น admin หรือไม่
                                bTravelerApproverApproveAll = (isRoleAdmin ? true : (lineApproveTravelerList > 0 ? false : true));

                                // Update expense approval status
                                // สร้างคำสั่ง SQL พื้นฐาน
                                sql = "UPDATE BZ_DOC_TRAVELER_EXPENSE SET DTE_APPR_OPT = :item_appr_status, DTE_APPR_REMARK = :appr_remark";

                                // เพิ่มเงื่อนไขสำหรับ action_type = 2
                                if (bTravelerApproverApproveAll || value.action.type == "2")
                                {
                                    sql += ", DTE_CAP_APPR_OPT = :item_appr_status";
                                }

                                // เพิ่มเงื่อนไขการอัปเดตสถานะเอกสาร
                                if (!string.IsNullOrEmpty(doc_status))
                                {
                                    sql += ", DTE_APPR_STATUS = :doc_status";
                                }

                                // ส่วนเงื่อนไข WHERE
                                sql += " WHERE DTE_TOKEN = :item_ref_id";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("item_appr_status", item.appr_status, "char"));
                                parameters.Add(context.ConvertTypeParameter("appr_remark", appr_remark, "char"));
                                if (!string.IsNullOrEmpty(doc_status))
                                {
                                    parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                }
                                parameters.Add(context.ConvertTypeParameter("item_ref_id", item_ref_id, "char"));
                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());


                                var row_check = traveler_expen.FirstOrDefault(p => p.DTE_TOKEN.Equals(item.ref_id));
                                if (row_check != null && row_check.DTE_EMP_ID != null)
                                {
                                    string travel_emp_id_select = row_check.DTE_EMP_ID ?? "";

                                    if (value.action.type == "2") // reject
                                    {

                                        foreach (var traveler in value.traveler_summary.Where(t => t.take_action == "true"))
                                        {
                                            string traveler_id = traveler.traverler_id;
                                            //doc_status = "30";
                                            sql = @"UPDATE BZ_DOC_TRAVELER_APPROVER 
SET DTA_APPR_STATUS = 'false',
    DTA_APPR_REMARK = :action_remark,
    DTA_DOC_STATUS = CASE
                        WHEN DTA_TYPE = 1 THEN 30  -- Line Reject
                        WHEN DTA_TYPE = 2 THEN 40  -- CAP Reject
                     END,
    DTA_ACTION_STATUS = '5' -- Rejected
WHERE dh_code = :doc_id
    AND DTA_TYPE IN ('1', '2')
    AND DTA_STATUS = 1
    AND DTA_ACTION_STATUS = '2'
AND DTA_TRAVEL_EMPID = :travel_emp_id_select";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("travel_emp_id_select", traveler_id, "char"));

                                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                        }

                                    }
                                    else if (value.action.type == "3") // revise
                                    {
                                        string action_status = (item.appr_status == "true" ? "4" : "5");
                                        sql = @"UPDATE BZ_DOC_TRAVELER_APPROVER SET 
                                    DTA_APPR_STATUS = :item_appr_status,
                                    DTA_APPR_REMARK = :action_remark,
                                    DTA_DOC_STATUS = CASE 
                                                        WHEN DTA_TYPE = 1 THEN 31  -- Line Approver
                                                        WHEN DTA_TYPE = 2 THEN 41  -- CAP Approver
                                                        END,
                                    DTA_ACTION_STATUS = :action_status
                                    WHERE dh_code = :doc_id
                                    AND DTA_STATUS = 1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("item_appr_status", item.appr_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_status", action_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("travel_emp_id", travel_emp_id_select, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                    else if (value.action.type == "4" || value.action.type == "5") // approve/reject
                                    {
                                        // ดึงรายการ ID โดยไม่ต้องใส่เครื่องหมายคำพูด
                                        var approveIds = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "true")
                                            .Select(t => t.traverler_id)
                                            .ToList();

                                        var rejectIds = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "false")
                                            .Select(t => t.traverler_id)
                                            .ToList();

                                        if (approveIds.Any() || rejectIds.Any())
                                        {
                                            // สร้างฟังก์ชันช่วยสร้าง IN clause
                                            string BuildInClause(List<string> ids)
                                            {
                                                return string.Join(",", ids.Select(id => $"'{id.Replace("'", "''")}'"));
                                            }

                                            // Update APPROVED travelers
                                            if (approveIds.Any())
                                            {
                                                string inClause = BuildInClause(approveIds);

                                                if (isRoleAdmin)
                                                {
                                                    // Admin can ONLY update DTA_TYPE = '1'
                                                    sql = $@"
UPDATE BZ_DOC_TRAVELER_APPROVER 
SET 
    DTA_APPR_STATUS = 'true', 
    DTA_DOC_STATUS = 32,  -- Status for type 1
    DTA_ACTION_STATUS = '3',
    DTA_UPDATE_DATE = SYSDATE
WHERE 
    dh_code = :doc_id 
    AND DTA_STATUS = 1
    AND DTA_ACTION_STATUS = '2'
    AND DTA_TYPE = '1'  -- ใช้ string แทน number
    AND DTA_TRAVEL_EMPID IN ({inClause})";
                                                }
                                                else
                                                {
                                                    // Non-admin can update BOTH types but must be their approval
                                                    sql = $@"
UPDATE BZ_DOC_TRAVELER_APPROVER 
SET 
    DTA_APPR_STATUS = 'true', 
    DTA_DOC_STATUS = CASE 
        WHEN DTA_TYPE = '1' THEN 32
        WHEN DTA_TYPE = '2' THEN 42
        ELSE DTA_DOC_STATUS
    END,
    DTA_ACTION_STATUS = '3',
    DTA_UPDATE_DATE = SYSDATE
WHERE 
    dh_code = :doc_id 
    AND DTA_STATUS = 1
    AND DTA_ACTION_STATUS = '2'
    AND DTA_APPR_EMPID = :user_id
    AND DTA_TRAVEL_EMPID IN ({inClause})";
                                                }

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                                                if (!isRoleAdmin)
                                                {
                                                    parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                                }

                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            }

                                            // Update REJECTED travelers
                                            if (rejectIds.Any())
                                            {
                                                string inClause = BuildInClause(rejectIds);

                                                if (isRoleAdmin)
                                                {
                                                    sql = $@"
UPDATE BZ_DOC_TRAVELER_APPROVER
SET
    DTA_APPR_STATUS = 'false',
    DTA_DOC_STATUS = CASE
        WHEN DTA_TYPE = '1' THEN 30
        WHEN DTA_TYPE = '2' THEN 40
        ELSE DTA_DOC_STATUS
    END,
    DTA_ACTION_STATUS = '5',
    DTA_APPR_REMARK = :action_remark,
    DTA_UPDATE_DATE = SYSDATE
WHERE
    dh_code = :doc_id
    AND DTA_STATUS = 1
    AND DTA_ACTION_STATUS = '2'
    AND DTA_TRAVEL_EMPID IN ({inClause})";
                                                }
                                                else
                                                {
                                                    sql = $@"
UPDATE BZ_DOC_TRAVELER_APPROVER
SET
    DTA_APPR_STATUS = 'false',
    DTA_DOC_STATUS = CASE
        WHEN DTA_TYPE = '1' THEN 30
        WHEN DTA_TYPE = '2' THEN 40
        ELSE DTA_DOC_STATUS
    END,
    DTA_ACTION_STATUS = '5',
    DTA_APPR_REMARK = :action_remark,
    DTA_UPDATE_DATE = SYSDATE
WHERE
    dh_code = :doc_id
    AND DTA_STATUS = 1
    AND DTA_ACTION_STATUS = '2'
    AND DTA_APPR_EMPID = :user_id
    AND DTA_TRAVEL_EMPID IN ({inClause})";
                                                }

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("action_remark", value.action.remark, "char"));

                                                if (!isRoleAdmin)
                                                {
                                                    parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                                }

                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            }
                                        }
                                    }

                                    //                                    else if (value.action.type == "4" || value.action.type == "5") // approve/reject
                                    //                                    {
                                    //                                        var approveIds = value.traveler_summary
                                    //                                            .Where(t => t.take_action == "true" && t.appr_status == "true")
                                    //                                            .Select(t => $"'{t.traverler_id}'")
                                    //                                            .ToList();

                                    //                                        var rejectIds = value.traveler_summary
                                    //                                            .Where(t => t.take_action == "true" && t.appr_status == "false")
                                    //                                            .Select(t => $"'{t.traverler_id}'")
                                    //                                            .ToList();

                                    //                                        if (approveIds.Any() || rejectIds.Any())
                                    //                                        {
                                    //                                            // Update APPROVED travelers
                                    //                                            if (approveIds.Any())
                                    //                                            {
                                    //                                                if (isRoleAdmin)
                                    //                                                {
                                    //                                                    sql = $@"
                                    //UPDATE BZ_DOC_TRAVELER_APPROVER 
                                    //SET 
                                    //    DTA_APPR_STATUS = 'true', 
                                    //    DTA_DOC_STATUS = 32,
                                    //    DTA_ACTION_STATUS = '3'
                                    //WHERE 
                                    //    dh_code = :doc_id 
                                    //    AND DTA_STATUS = 1
                                    //    AND DTA_ACTION_STATUS = '2'
                                    //    AND DTA_TYPE = 1
                                    //    AND DTA_TRAVEL_EMPID IN (SELECT COLUMN_VALUE FROM TABLE(:approveIds))";
                                    //                                                }
                                    //                                                else
                                    //                                                {
                                    //                                                    sql = $@"
                                    //UPDATE BZ_DOC_TRAVELER_APPROVER 
                                    //SET 
                                    //    DTA_APPR_STATUS = 'true', 
                                    //    DTA_DOC_STATUS = CASE 
                                    //        WHEN DTA_TYPE = '1' THEN 32
                                    //        WHEN DTA_TYPE = '2' THEN 42
                                    //        ELSE DTA_DOC_STATUS
                                    //    END,
                                    //    DTA_ACTION_STATUS = '3'
                                    //WHERE 
                                    //    dh_code = :doc_id 
                                    //    AND DTA_STATUS = 1
                                    //    AND DTA_ACTION_STATUS = '2'
                                    //    AND DTA_APPR_EMPID = :user_id
                                    //    AND DTA_TRAVEL_EMPID IN (SELECT COLUMN_VALUE FROM TABLE(:approveIds))";
                                    //                                                }

                                    //                                                parameters = new List<OracleParameter>
                                    //    {
                                    //        context.ConvertTypeParameter("doc_id", doc_id, "char"),
                                    //        new OracleParameter
                                    //        {
                                    //            ParameterName = "approveIds",
                                    //            OracleDbType = OracleDbType.Varchar2,
                                    //            CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                                    //            Value = approveIds.ToArray(),
                                    //            Size = approveIds.Count
                                    //        }
                                    //    };

                                    //                                                if (!isRoleAdmin)
                                    //                                                {
                                    //                                                    parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                    //                                                }

                                    //                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    //                                            }


                                    //                                            // Update REJECTED travelers
                                    //                                            if (rejectIds.Any())
                                    //                                            {
                                    //                                                if (isRoleAdmin)
                                    //                                                {
                                    //                                                    // Admin can ONLY reject DTA_TYPE = '1'
                                    //                                                    sql = $@"UPDATE BZ_DOC_TRAVELER_APPROVER
                                    //SET
                                    //    DTA_APPR_STATUS = 'false',
                                    //    DTA_DOC_STATUS = CASE
                                    //        WHEN DTA_TYPE = '1' THEN 30
                                    //        WHEN DTA_TYPE = '2' THEN 40
                                    //        ELSE DTA_DOC_STATUS
                                    //    END,
                                    //    DTA_ACTION_STATUS = '5',
                                    //    DTA_APPR_REMARK = :action_remark
                                    //WHERE
                                    //    dh_code = :doc_id
                                    //    AND DTA_STATUS = 1
                                    //    AND DTA_ACTION_STATUS = '2'
                                    //    AND DTA_TRAVEL_EMPID IN (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:rejectIds)))";
                                    //                                                }
                                    //                                                else
                                    //                                                {
                                    //                                                    // Non-admin can reject BOTH types but must be their approval
                                    //                                                    sql = $@"UPDATE BZ_DOC_TRAVELER_APPROVER
                                    //SET
                                    //    DTA_APPR_STATUS = 'false',
                                    //    DTA_DOC_STATUS = CASE
                                    //        WHEN DTA_TYPE = '1' THEN 30
                                    //        WHEN DTA_TYPE = '2' THEN 40
                                    //        ELSE DTA_DOC_STATUS
                                    //    END,
                                    //    DTA_ACTION_STATUS = '5',
                                    //    DTA_APPR_REMARK = :action_remark
                                    //WHERE
                                    //    dh_code = :doc_id
                                    //    AND DTA_STATUS = 1
                                    //    AND DTA_ACTION_STATUS = '2'
                                    //    AND DTA_APPR_EMPID = :user_id
                                    //    AND DTA_TRAVEL_EMPID IN (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:rejectIds)))";
                                    //                                                }

                                    //                                                parameters = new List<OracleParameter>
                                    //    {
                                    //        context.ConvertTypeParameter("doc_id", doc_id, "char"),
                                    //        new OracleParameter
                                    //        {
                                    //            ParameterName = "rejectIds",
                                    //            OracleDbType = OracleDbType.Varchar2,
                                    //            CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                                    //            Value = rejectIds.ToArray(),
                                    //            Size = rejectIds.Count
                                    //        },
                                    //        context.ConvertTypeParameter("action_remark", value.action.remark ?? "", "char")
                                    //    };

                                    //                                                if (!isRoleAdmin)
                                    //                                                {
                                    //                                                    parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                    //                                                }

                                    //                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    //                                            }
                                    //                                        }
                                    //                                    }

                                    // Update approver update date
                                    sql = @"UPDATE BZ_DOC_TRAVELER_APPROVER 
            SET DTA_UPDATE_DATE = sysdate 
            WHERE dh_code = :doc_id 
            AND DTA_STATUS = 1 
            AND DTA_TRAVEL_EMPID = :travel_emp_id ";

                                    if (isRoleAdmin)
                                    {
                                        sql += "AND DTA_TYPE = '1' ";
                                    }
                                    else
                                    {
                                        sql += "AND (DTA_TYPE = '1' OR DTA_TYPE = '2') ";
                                        sql += "AND DTA_APPR_EMPID = :user_id ";
                                    }

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("travel_emp_id", travel_emp_id_select, "char"));
                                    if (!isRoleAdmin)
                                    {
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                    }
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                    // Add approver to notification list
                                    var findData = traveler_approver_List
                                        .Where(p => (p.DTA_TYPE == "1" || p.DTA_TYPE == "2") &&
                                                p.DTA_STATUS == 1 &&
                                                p.DTA_TRAVEL_EMPID.Equals(row_check.DTE_EMP_ID))
                                        .ToList();

                                    if (!isRoleAdmin)
                                    {
                                        findData = findData.Where(p => p.DTA_APPR_EMPID.Equals(user_id)).ToList();
                                    }

                                    foreach (var ifindData in findData)
                                    {
                                        if (!approverList.Any(a => a.user_id == ifindData.DTA_APPR_EMPID))
                                        {
                                            approverList.Add(new SearchUserAllModel { user_id = ifindData.DTA_APPR_EMPID ?? "" });
                                        }
                                    }
                                }
                            }

                            // Process document actions
                            if (!string.IsNullOrEmpty(user_id))
                            {
                                if (value.action.type == "2") // reject
                                {
                                    foreach (var item in approverList)
                                    {
                                        string item_user_id = item.user_id ?? "";
                                        sql = @"UPDATE BZ_DOC_ACTION 
                                   SET ACTION_DATE=sysdate, ACTION_BY=:user_id, ACTION_STATUS=2 
                                   WHERE dh_code=:doc_id AND EMP_ID=:item_user_id 
                                   AND DOC_STATUS=31 AND ACTION_STATUS=1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                    if (isRoleAdmin)
                                    {
                                        sql = @"UPDATE BZ_DOC_ACTION 
                                   SET ACTION_DATE=sysdate, ACTION_BY=:user_id, ACTION_STATUS=2 
                                   WHERE dh_code=:doc_id AND EMP_ID='admin' 
                                   AND DOC_STATUS=31 AND ACTION_STATUS=1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                    // Update CAP approver status if same as line approver
                                    foreach (var item in approverList)
                                    {
                                        string item_user_id = item.user_id ?? "";
                                        sql = @"UPDATE BZ_DOC_ACTION SET ACTION_STATUS=2 
                                   WHERE (SELECT CASE WHEN 
                                       (SELECT COUNT(1) FROM BZ_DOC_ACTION WHERE dh_code=:doc_id AND doc_status=31 AND emp_id=:item_user_id)>0
                                       AND
                                       (SELECT COUNT(1) FROM BZ_DOC_ACTION WHERE dh_code=:doc_id AND doc_status=41 AND emp_id=:item_user_id)>0
                                       THEN 1 ELSE 0 END FROM DUAL)>0
                                   AND dh_code=:doc_id AND EMP_ID=:item_user_id
                                   AND DOC_STATUS=41 AND ACTION_STATUS=1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }
                                }

                                else if (value.action.type == "3") // revise
                                {
                                    foreach (var item in approverList)
                                    {
                                        sql = @"UPDATE BZ_DOC_ACTION 
                                SET ACTION_DATE=sysdate, ACTION_BY=:user_id, ACTION_STATUS=2 , TAB_NO=2
                                WHERE dh_code=:doc_id AND (DOC_STATUS=31 OR DOC_STATUS=41) AND ACTION_STATUS=1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                    if (isRoleAdmin)
                                    {
                                        sql = @"UPDATE BZ_DOC_ACTION 
                                SET ACTION_DATE=sysdate, ACTION_BY=:user_id, ACTION_STATUS=2, TAB_NO=2 
                                WHERE dh_code=:doc_id AND EMP_ID='admin' 
                                AND (DOC_STATUS=31 OR DOC_STATUS=41) AND ACTION_STATUS=1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                    // Delete existing admin action
                                    sql = "DELETE FROM BZ_DOC_ACTION WHERE DH_CODE=:doc_id AND DOC_STATUS=23 AND EMP_ID='admin' AND ACTION_STATUS=1";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());

                                    // Create new admin action
                                    string token_action_new = Guid.NewGuid().ToString();
                                    string doc_head_type = doc_head_search.DH_TYPE ?? "";
                                    sql = @"INSERT INTO BZ_DOC_ACTION (DA_TOKEN, DH_CODE, DOC_TYPE, DOC_STATUS, EMP_ID, TAB_NO, CREATED_DATE, UPDATED_DATE)
                            VALUES (:token_action_new, :doc_id, :doc_head_type, :doc_status, 'admin', 2, sysdate, sysdate)";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("token_action_new", token_action_new, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_head_type", doc_head_type, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                }
                                else if (value.action.type == "4" || value.action.type == "5") // approve
                                {
                                    foreach (var item in approverList)
                                    {
                                        string item_user_id = item.user_id ?? "";
                                        sql = @"UPDATE BZ_DOC_ACTION 
                                   SET ACTION_DATE=sysdate, ACTION_BY=:user_id, ACTION_STATUS=2 
                                   WHERE dh_code=:doc_id AND EMP_ID=:item_user_id 
                                   AND (DOC_STATUS=31 OR DOC_STATUS=41) AND ACTION_STATUS=1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("item_user_id", item_user_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }

                                    if (isRoleAdmin)
                                    {
                                        sql = @"UPDATE BZ_DOC_ACTION 
                                   SET ACTION_DATE=sysdate, ACTION_BY=:user_id, ACTION_STATUS=2 
                                   WHERE dh_code=:doc_id AND EMP_ID='admin' 
                                   AND (DOC_STATUS=31 OR DOC_STATUS=41) AND ACTION_STATUS=1";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    }
                                }
                            }

                            context.SaveChanges();
                            transaction.Commit();
                            data.status = "S";
                            data.message = "";

                            // Check if all approvers have completed action (ทั้ง Line และ CAP)
                            sql = @"SELECT COUNT(1) as status_value, '' as emp_id, '' as doc_status
                        FROM BZ_DOC_TRAVELER_APPROVER a
                        WHERE (a.dta_type=1 OR a.dta_type=2) 
                        AND a.dta_action_status IN (1,2)
                        AND a.dh_code=:doc_id";

                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            var dataApporver_Def = context.AllApproveModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                            if (dataApporver_Def != null && dataApporver_Def.Count > 0 && dataApporver_Def[0].status_value == 0)
                            {
                                sql = @"UPDATE BZ_DOC_ACTION 
                            SET ACTION_DATE=sysdate, ACTION_BY=:user_id, ACTION_STATUS=2 
                            WHERE dh_code=:doc_id AND EMP_ID='admin' 
                            AND (DOC_STATUS=31 OR DOC_STATUS=41) AND ACTION_STATUS=1";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("user_id", user_id, "char"));
                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                context.SaveChanges();
                            }

                            // Check if all approvers have approved (ทั้ง Line และ CAP)
                            bool apprAllStatus = false;
                            bool apprAllLine = false;
                            bool apprAllCAP = false;
                            string ret_doc_status = doc_status;


                            if (value.action.type == "4" || value.action.type == "5" || value.action.type == "2")
                            {
                                apprAllStatus = AllApproveLineApprover(doc_id, ref ret_doc_status);
                                apprAllCAP = AllApproveCAPApprover(doc_id, ref doc_status);
                                if (apprAllStatus && apprAllCAP)
                                {
                                    sql = @"UPDATE BZ_DOC_TRAVELER_EXPENSE  
                                SET DTE_CAP_APPR_STATUS = 
                                CASE WHEN DTE_APPR_STATUS=32 AND DTE_APPR_OPT='true' THEN 42 ELSE 
                                    CASE WHEN DTE_APPR_STATUS=32 AND DTE_APPR_OPT='false' THEN 40 ELSE 
                                    CASE WHEN DTE_APPR_STATUS=30 THEN 40 END END END,
                                DTE_CAP_APPR_OPT = 
                                CASE WHEN DTE_APPR_STATUS=32 AND DTE_APPR_OPT='true' THEN NULL ELSE 
                                    CASE WHEN DTE_APPR_STATUS=32 AND DTE_APPR_OPT='false' THEN 'false' ELSE 
                                    CASE WHEN DTE_APPR_STATUS=30 THEN 'false' END END END
                                WHERE DH_CODE=:doc_id";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    context.SaveChanges();
                                }

                                //                            if(apprAllLine && apprAllCAP)
                                //                            {

                                //                                    sql = @"UPDATE BZ_DOC_TRAVELER_EXPENSE  
                                //SET DTE_CAP_APPR_STATUS = 
                                //CASE 
                                //    /* กรณี Line Approver และ CAP Approver เป็นคนเดียวกัน และอนุมัติ */
                                //    WHEN EXISTS (
                                //        SELECT 1 FROM bz_doc_traveler_approver 
                                //        WHERE dh_code = :doc_id 
                                //        AND dta_type = 1 
                                //        AND dta_appr_empid IN (
                                //            SELECT dta_appr_empid FROM bz_doc_traveler_approver 
                                //            WHERE dh_code = :doc_id AND dta_type = 2
                                //        )
                                //        AND DTE_APPR_STATUS = 32 
                                //        AND DTE_APPR_OPT = 'true'
                                //    ) THEN 42

                                //    /* กรณีปกติที่ Line Approver อนุมัติ */
                                //    WHEN DTE_APPR_STATUS = 32 AND DTE_APPR_OPT = 'true' THEN 42 

                                //    /* กรณี Line Approver ไม่อนุมัติ */
                                //    WHEN DTE_APPR_STATUS = 32 AND DTE_APPR_OPT = 'false' THEN 40 

                                //    /* กรณีถูกปฏิเสธโดย Line Approver */
                                //    WHEN DTE_APPR_STATUS = 30 THEN 40 
                                //END,

                                //DTE_CAP_APPR_OPT = 
                                //CASE 
                                //    /* กรณี Line Approver และ CAP Approver เป็นคนเดียวกัน และอนุมัติ */
                                //    WHEN EXISTS (
                                //        SELECT 1 FROM bz_doc_traveler_approver 
                                //        WHERE dh_code = :doc_id 
                                //        AND dta_type = 1 
                                //        AND dta_appr_empid IN (
                                //            SELECT dta_appr_empid FROM bz_doc_traveler_approver 
                                //            WHERE dh_code = :doc_id AND dta_type = 2
                                //        )
                                //        AND DTE_APPR_STATUS = 32 
                                //        AND DTE_APPR_OPT = 'true'
                                //    ) THEN NULL

                                //    /* กรณีปกติที่ Line Approver อนุมัติ */
                                //    WHEN DTE_APPR_STATUS = 32 AND DTE_APPR_OPT = 'true' THEN NULL 

                                //    /* กรณี Line Approver ไม่อนุมัติ */
                                //    WHEN DTE_APPR_STATUS = 32 AND DTE_APPR_OPT = 'false' THEN 'false' 

                                //    /* กรณีถูกปฏิเสธโดย Line Approver */
                                //    WHEN DTE_APPR_STATUS = 30 THEN 'false' 
                                //END
                                //WHERE DH_CODE = :doc_id";

                                //                                    parameters = new List<OracleParameter>();
                                //                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                //                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                //                                    context.SaveChanges();

                                //                            }



                                // Handle case where Line Approver and CAP Approver are the same person
                                foreach (var apprlist in traveler_approver_List)
                                {
                                    string item_travel_empid = apprlist.DTA_TRAVEL_EMPID ?? "";
                                    string item_approver_empid = apprlist.DTA_APPR_EMPID ?? "";

                                    sql = @"UPDATE BZ_DOC_TRAVELER_EXPENSE 
                                SET DTE_CAP_APPR_OPT=DTE_APPR_OPT, 
                                    DTE_CAP_APPR_STATUS=REPLACE(DTE_APPR_STATUS,3,4)  
                                WHERE (
                                SELECT TO_CHAR(COUNT(dta_type)) 
                                FROM (SELECT DISTINCT dta_type
                                FROM bz_doc_traveler_approver b
                                WHERE dh_code=:doc_id AND dta_travel_empid=:item_travel_empid AND dta_appr_empid=:item_approver_empid)t)>1
                                AND DH_CODE=:doc_id AND DTE_EMP_ID=:item_travel_empid";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_travel_empid", item_travel_empid, "char"));
                                    parameters.Add(context.ConvertTypeParameter("item_approver_empid", item_approver_empid, "char"));
                                    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    context.SaveChanges();
                                }
                            }

                            // Check if all approvers have completed action (ทั้ง Line และ CAP)


                            write_log_mail("0-email.start-submitFlow3", "type_flow :" + type_flow + " =>value.action.type :" + value.action.type + " =>apprAllStatus :" + apprAllStatus);

                            if (type_flow == true)
                            {
                                // Send email notifications based on action type
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
                                    sTitle = doc_head_search.DH_TOPIC ?? "";
                                    sBusinessDate = "";
                                    if (doc_head_search.DH_BUS_FROMDATE != null)
                                    {
                                        sBusinessDate = dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")),
                                            doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                    }

                                    sql = @"select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1, 
a.city_text as name2, '' as name3, '' as name4    
FROM bz_doc_traveler_expense a    
inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
inner join VW_BZ_USERS U on a.DTE_Emp_Id = u.employeeid
left join bz_master_country c on a.ct_id = c.ct_id
left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID
left join (select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text 
from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text) s 
on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id 
and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) 
and a.city_text = s.city_text
WHERE a.dh_code = :doc_id and a.dte_status = 1 ";
                                    sql += " order by s.id";

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                    if (temp != null && temp.Count() > 0)
                                    {
                                        sLocation = "";
                                        foreach (var item in temp)
                                        {
                                            if (!string.IsNullOrEmpty(sLocation)) sLocation += ", ";
                                            sLocation += item.name1 + "/" + item.name2;
                                        }
                                    }

                                    //reject 
                                    if (value.action.type == "2")
                                    {
                                        #region traveler mail in doc
                                        sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name, b.email as email    
                                             , b.employeeid as emp_id, b.orgname as position                   
                                             from bz_doc_traveler_approver a
                                             inner join vw_bz_users b on a.dta_travel_empid = b.employeeid
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
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id, "char"));
                                        }
                                        var tempTravel = context.TempEmployeeProfileModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                        if (tempTravel != null)
                                        {
                                            foreach (var item in tempTravel)
                                            {
                                                traveler_mail += item.email + ";";
                                            }
                                        }
                                        #endregion traveler mail in doc
                                        #region approver mail in doc 
                                        sql = @" select distinct b.employeeid as user_id, '' as user_name, b.email                       
                                                 from bz_doc_traveler_approver a
                                                 inner join vw_bz_users b on a.dta_appr_empid = b.employeeid 
                                                 where a.dta_type = 1 and a.dta_action_status in (5) and a.dta_doc_status = 40
                                                 and a.dh_code = :doc_id ";
                                        if (user_role != "1")
                                        {
                                            sql += " and a.dta_appr_empid = :user_id_def ";
                                        }

                                        line_approver_mail = "";
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        if (user_role != "1")
                                        {
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id, "char"));
                                        }
                                        var approvermailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        if (approvermailList != null)
                                        {
                                            if (approvermailList.Count > 0)
                                            {
                                                for (int m = 0; m < approvermailList.Count; m++)
                                                {
                                                    line_approver_mail += approvermailList[m].email + ";";
                                                }
                                            }
                                        }
                                        #endregion approver mail in doc

                                        #region "#### SEND REJECTION MAIL ####" 
                                        try
                                        {
                                            sendEmailModel dataMail = new sendEmailModel();

                                            var doc_head_create_by = doc_head_search.DH_CREATE_BY ?? "";
                                            sql = "SELECT b.employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                            sql += "FROM vw_bz_users b WHERE employeeid= :doc_head_create_by";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_head_create_by", doc_head_create_by, "char"));
                                            var requestor = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            dataMail.mail_to = requester_mail + traveler_mail;
                                            dataMail.mail_cc = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) + line_approver_mail + on_behalf_of_mail;

                                            var resMailShowCase = $"To: {requester_mail?.ToLower()} (Requester)";
                                            resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                            resMailShowCase += $"<br>Cc: {super_admin_mail?.ToLower()} (Super Admin)";
                                            resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                            //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                            resMailShowCase += $", {line_approver_mail?.ToLower()} (Line Approval)";
                                            resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                            dataMail.mail_show_case = resMailShowCase;

                                            dataMail.mail_subject = $"{doc_id}: The request for business travel has been rejected";

                                            sDear = "Dear All,";
                                            sDetail = $"Your business travel request has been rejected by {user_name}. To view the details, click ";
                                            sDetail += $"<a href='{(LinkLogin + "ii").Replace("###", doc_id)}'>{doc_id}</a>";

                                            #region Build Email Body
                                            dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

            <table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                            if (value.action.type == "2")
                                            {
                                                sReasonRejected = value.traveler_summary
                                                    .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                                    .Select(x => x.appr_remark)
                                                    .FirstOrDefault();

                                                if (!string.IsNullOrEmpty(sReasonRejected))
                                                {
                                                    dataMail.mail_body += $@"
            <table width='60%' cellpadding='10' cellspacing='0' border='0' style='background-color: #fff8f8; border-left: 4px solid #ffcccc; margin-bottom: 10px;'>
                <tr><td style='font-size: 14px; color: #d9534f;'><strong>Reason for Rejection:</strong> {sReasonRejected}</td>
                    </tr>
                </table>
            </td>
        </tr>";
                                                }
                                            }

                                            if (tempTravel != null && tempTravel?.Count > 0)
                                            {
                                                dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                var iNo = 1;
                                                foreach (var item in tempTravel)
                                                {
                                                    dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.emp_id}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.position}</td>
                    </tr>";
                                                    iNo++;
                                                }

                                                dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                            }

                                            dataMail.mail_body += $@"
    <tr>
        <td style='padding: 20px 0; font-size: 14px;'>
            <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</p>
                                                        <p style = 'margin: 0;' > For application assistance, please contact PTT Digital Call Center(Tel. {Tel_Call_Center}).</ p >
                                            
                                                    </ td >
                                            
                                                </ tr >
                                            
                                                < tr >
                                            
                                                    < td style = 'padding: 15px 0; border-top: 1px solid #eeeeee; font-size: 14px;' >


                                                        < p style = 'margin: 0;' > Best Regards,</ p >
                                            
                                                        < p style = 'margin: 0; font-weight: bold;' > Business Travel Services Team(PMSV) </ p >
                                            
                                                    </ td >
                                            
                                                </ tr >
                                            </ table > ";

                                            #endregion

                                            //#region Build Email Body
                                            //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                            //dataMail.mail_body += "    </div>";

                                            //if (value.action.type == "2")
                                            //{
                                            //    sReasonRejected = value.traveler_summary
                                            //        .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                            //        .Select(x => x.appr_remark)
                                            //        .FirstOrDefault();

                                            //    if (!string.IsNullOrEmpty(sReasonRejected))
                                            //    {
                                            //        dataMail.mail_body += "    <div style='font-family: Aptos, Arial, sans-serif; margin-bottom: 10px; margin-top: 10px; padding: 10px; background-color: #fff8f8; border-left: 4px solid #ffcccc;  width: 50%;'>";
                                            //        dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; color: #d9534f;'><strong>Reason for Rejection:</strong> " + sReasonRejected + "</p>";
                                            //        dataMail.mail_body += "    </div>";
                                            //    }
                                            //}

                                            //if (tempTravel != null && tempTravel?.Count > 0)
                                            //{
                                            //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //    dataMail.mail_body += "        <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                            //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif;'>";
                                            //    dataMail.mail_body += "            <thead>";
                                            //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                            //    dataMail.mail_body += "                </tr>";
                                            //    dataMail.mail_body += "            </thead>";
                                            //    dataMail.mail_body += "            <tbody>";

                                            //    var iNo = 1;
                                            //    foreach (var item in tempTravel)
                                            //    {
                                            //        dataMail.mail_body += "            <tr>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.emp_id}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.position}</td>";
                                            //        dataMail.mail_body += "            </tr>";
                                            //        iNo++;
                                            //    }

                                            //    dataMail.mail_body += "            </tbody>";
                                            //    dataMail.mail_body += "        </table>";
                                            //    dataMail.mail_body += "    </div>";
                                            //}

                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                            //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                            //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "</div>";

                                            //#endregion

                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;

                                            var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                            if (!string.IsNullOrEmpty(attachments_doc))
                                            {
                                                dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                            }

                                            data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                        }
                                        catch (Exception ex)
                                        {
                                            write_log_mail("88-email.message-submitFlow3", "error-reject" + ex.ToString());
                                        }
                                        #endregion
                                    }
                                    else if (value.action.type == "3") // revise
                                    {
                                        #region traveler mail in doc
                                        traveler_mail = "";
                                        sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 , b.employeeid as name3, b.orgname as name4
                from bz_doc_traveler_approver a
                inner join vw_bz_users b on a.dta_travel_empid = b.employeeid
                left join (select min(dte_id) as id, dh_code, dte_emp_id  from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s
                on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id  
                where a.dta_type = 1 and a.dta_action_status in (4) and a.dta_doc_status in (23,31)
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
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id, "char"));
                                        }
                                        var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        if (tempTravel != null)
                                        {
                                            foreach (var item in tempTravel)
                                            {
                                                traveler_mail += item.name2 + ";";
                                            }
                                        }
                                        #endregion traveler mail in doc

                                        #region approver mail in doc 
                                        sql = @" select distinct b.email                       
                from bz_doc_traveler_approver a
                inner join vw_bz_users b on a.dta_appr_empid = b.employeeid 
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
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id, "char"));
                                        }
                                        var approverLineMaillList = context.TempEMailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        line_approver_mail = "";
                                        if (approverLineMaillList != null)
                                        {
                                            if (approverLineMaillList.Count > 0)
                                            {
                                                for (int m = 0; m < approverLineMaillList.Count; m++)
                                                {
                                                    line_approver_mail += approverLineMaillList[m].email + ";";
                                                }
                                            }
                                        }
                                        #endregion approver mail in doc

                                        #region "#### SEND MAIL ####" 
                                        try
                                        {
                                            sTitle = doc_head_search.DH_TOPIC ?? "";
                                            sBusinessDate = "";
                                            if (doc_head_search.DH_BUS_FROMDATE != null)
                                            {
                                                sBusinessDate = dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")),
                                                                    doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                            }

                                            sendEmailModel dataMail = new sendEmailModel();

                                            sql = "SELECT b.employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                            sql += " FROM vw_bz_users b WHERE employeeid= :user_id_def ";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id, "char"));
                                            var user_active_doc = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            var revise_user_display_name = "";
                                            if (user_active_doc != null && user_active_doc?.Count > 0)
                                            {
                                                revise_user_display_name = user_active_doc[0].user_name ?? "";
                                            }

                                            //to : Super admin
                                            //cc : Requester, On Behalf Of   
                                            dataMail.mail_to = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail);
                                            dataMail.mail_cc = requester_mail + on_behalf_of_mail;

                                            var resMailShowCase = $"To: {super_admin_mail?.ToLower()} (Super Admin)";
                                            resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                            //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                            resMailShowCase += $"<br>Cc: {requester_mail?.ToLower()} (Requester)";
                                            resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                            resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                            dataMail.mail_show_case = resMailShowCase;

                                            dataMail.mail_subject = $"{value.doc_id}: The business travel request has been required to revise by {revise_user_display_name}";

                                            sDear = "Dear All,";
                                            sDetail = $"Your business travel request has been revised by {revise_user_display_name}. To view the details, click ";
                                            sDetail += $"<a href='{(LinkLogin + "i").Replace("###", value.doc_id)}'>{value.doc_id}</a>";

                                            #region Build Email Body
                                            dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                            if (value.action.type == "3")
                                            {

                                                sReasonRejected = value.action.remark;
                                                if (!string.IsNullOrEmpty(sReasonRejected))
                                                {
                                                    dataMail.mail_body += $@"
            <table width='60%' cellpadding='10' cellspacing='0' border='0' style='background-color: #fff8f8; border-left: 4px solid #ffcccc; margin-bottom: 10px;'>
                <tr><td style='font-size: 14px; color: #d9534f;'><strong>Reason for Revision:</strong> {sReasonRejected}</td>
                    </tr>
                </table>
            </td>
        </tr>";
                                                }
                                            }

                                            if (tempTravel != null && tempTravel?.Count > 0)
                                            {
                                                dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                var iNo = 1;
                                                foreach (var item in tempTravel)
                                                {
                                                    dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                    </tr>";
                                                    iNo++;
                                                }

                                                dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                            }

                                            dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                            #endregion
                                            //#region Build Email Body
                                            //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='margin: 0 0 10px 0;'>{sDear}</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='margin: 0 0 15px 0;'>{sDetail}</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                            //dataMail.mail_body += $"       <p style='margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                            //dataMail.mail_body += $"       <p style='margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                            //dataMail.mail_body += $"       <p style='margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                            //dataMail.mail_body += "    </div>";

                                            //if (value.action.type == "3")
                                            //{
                                            //    sReasonRejected = value.action.remark;
                                            //    if (!string.IsNullOrEmpty(sReasonRejected))
                                            //    {
                                            //        dataMail.mail_body += "    <div style='font-family: Aptos, Arial, sans-serif; margin-bottom: 10px; margin-top: 10px; padding: 10px; background-color: #fff8f8; border-left: 4px solid #ffcccc; display: inline-table; width: auto;'>";
                                            //        dataMail.mail_body += "        <p style='margin: 0; color: #d9534f;'><strong>Reason for Revision:</strong> " + sReasonRejected + "</p>";
                                            //        dataMail.mail_body += "    </div>";
                                            //    }
                                            //}


                                            //if (tempTravel != null && tempTravel?.Count > 0)
                                            //{
                                            //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //    dataMail.mail_body += "        <p style='margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                            //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-family: Aptos, Arial, sans-serif; font-size: 14px;'>";
                                            //    dataMail.mail_body += "            <thead>";
                                            //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                            //    dataMail.mail_body += "                </tr>";
                                            //    dataMail.mail_body += "            </thead>";
                                            //    dataMail.mail_body += "            <tbody>";

                                            //    var iNo = 1;
                                            //    foreach (var item in tempTravel)
                                            //    {
                                            //        dataMail.mail_body += "            <tr>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                            //        dataMail.mail_body += "            </tr>";
                                            //        iNo++;
                                            //    }

                                            //    dataMail.mail_body += "            </tbody>";
                                            //    dataMail.mail_body += "        </table>";
                                            //    dataMail.mail_body += "    </div>";
                                            //}

                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                            //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                            //dataMail.mail_body += "        <p style='margin: 0;'>Best Regards,</p>";
                                            //dataMail.mail_body += "        <p style='margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "</div>";
                                            //#endregion

                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;

                                            var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                            if (!string.IsNullOrEmpty(attachments_doc))
                                            {
                                                dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                            }

                                            data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                        }
                                        catch (Exception ex)
                                        {
                                            write_log_mail("88-email.message-submitFlow3", "error-revise" + ex.ToString());
                                        }
                                        #endregion
                                    }

                                    else if (value.action.type == "4" || value.action.type == "5")
                                    {

                                        // แยกกลุ่มผู้ถูกปฏิเสธและอนุมัติ
                                        var rejectedTravelers = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "false")
                                            .ToList();

                                        var approvedTravelers = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "true")
                                            .ToList();

                                        // ส่งอีเมลสำหรับผู้ถูกปฏิเสธ
                                        if (rejectedTravelers.Any())
                                        {

                                            #region traveler mail in doc (Rejected)
                                            sql = @"SELECT DISTINCT to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name,
            b.employeeid as emp_id, b.email as email, b.orgname as position   
            FROM bz_doc_traveler_approver a
            INNER JOIN vw_bz_users b ON a.dta_travel_empid = b.employeeid
            LEFT JOIN (SELECT min(dte_id) as id, dh_code, dte_emp_id FROM BZ_DOC_TRAVELER_EXPENSE GROUP BY dh_code, dte_emp_id) s 
            ON a.dh_code = s.dh_code AND a.dta_travel_empid = s.dte_emp_id
            WHERE a.dta_type = 1 AND a.dta_action_status IN (5) AND a.dta_doc_status = 30
            AND a.dh_code = :doc_id 
            AND a.dta_travel_empid IN (" + string.Join(",", rejectedTravelers.Select(t => $"'{t.traverler_id}'")) + ") ORDER BY s.id";

                                            traveler_mail = "";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempTravel = context.Database.SqlQueryRaw<tempEmployeeProfileModel>(sql, parameters.ToArray()).ToList();
                                            if (tempTravel != null)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    traveler_mail += item.email + ";";
                                                }
                                            }
                                            #endregion

                                            #region approver mail in doc (Rejected)
                                            sql = @"SELECT DISTINCT b.employeeid as user_id, '' as user_name, b.email                       
            FROM bz_doc_traveler_approver a
            INNER JOIN vw_bz_users b ON a.dta_appr_empid = b.employeeid 
            WHERE a.dta_type = 1 AND a.dta_action_status IN (5) AND a.dta_doc_status = 30
            AND a.dh_code = :doc_id";

                                            line_approver_mail = "";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var approvermail = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                            if (approvermail != null && approvermail.Count > 0)
                                            {
                                                for (int m = 0; m < approvermail.Count; m++)
                                                {
                                                    line_approver_mail += approvermail[m].email + ";";
                                                }
                                            }
                                            #endregion

                                            #region "#### SEND REJECTION MAIL ####" 
                                            try
                                            {
                                                sendEmailModel dataMail = new sendEmailModel();

                                                var doc_head_create_by = doc_head_search.DH_CREATE_BY ?? "";
                                                sql = "SELECT b.employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                                sql += "FROM vw_bz_users b WHERE employeeid= :doc_head_create_by";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_head_create_by", doc_head_create_by, "char"));
                                                var requestor = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                dataMail.mail_to = requester_mail + traveler_mail;
                                                dataMail.mail_cc = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) + line_approver_mail + on_behalf_of_mail;

                                                var resMailShowCase = $"To: {requester_mail?.ToLower()} (Requester)";
                                                resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                                resMailShowCase += $"<br>Cc: {super_admin_mail?.ToLower()} (Super Admin)";
                                                resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                resMailShowCase += $", {line_approver_mail?.ToLower()} (Line Approval)";
                                                resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                dataMail.mail_show_case = resMailShowCase;

                                                dataMail.mail_subject = $"{doc_id}: The request for business travel has been rejected";

                                                sDear = "Dear All,";
                                                sDetail = $"Your business travel request has been rejected by {user_name}. To view the details, click ";
                                                sDetail += $"<a href='{(LinkLogin + "ii").Replace("###", doc_id)}'>{doc_id}</a>";

                                                #region Build Email Body
                                                dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                                if (rejectedTravelers.Any())
                                                {
                                                    sReasonRejected = value.traveler_summary
                                                        .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                                        .Select(x => x.appr_remark)
                                                        .FirstOrDefault();

                                                    if (!string.IsNullOrEmpty(sReasonRejected))
                                                    {
                                                        dataMail.mail_body += $@"
            <table width='60%' cellpadding='10' cellspacing='0' border='0' style='background-color: #fff8f8; border-left: 4px solid #ffcccc; margin-bottom: 10px;'>
                <tr><td style='font-size: 14px; color: #d9534f;'><strong>Reason for Rejection:</strong> {sReasonRejected}</td>
                    </tr>
                </table>
            </td>
        </tr>";
                                                    }
                                                }

                                                if (tempTravel != null && tempTravel?.Count > 0)
                                                {
                                                    dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                    var iNo = 1;
                                                    foreach (var item in tempTravel)
                                                    {
                                                        dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.emp_id}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.position}</td>
                    </tr>";
                                                        iNo++;
                                                    }

                                                    dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                                }

                                                dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                                #endregion

                                                //#region Build Email Body
                                                //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                                //dataMail.mail_body += "    </div>";

                                                //if (rejectedTravelers.Any())
                                                //{
                                                //    sReasonRejected = value.traveler_summary
                                                //        .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                                //        .Select(x => x.appr_remark)
                                                //        .FirstOrDefault();

                                                //    if (!string.IsNullOrEmpty(sReasonRejected))
                                                //    {
                                                //        dataMail.mail_body += "    <div style='font-family: Aptos, Arial, sans-serif; margin-bottom: 10px; margin-top: 10px; padding: 10px; background-color: #fff8f8; border-left: 4px solid #ffcccc; display: inline-table; width: auto;'>";
                                                //        dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; color: #d9534f;'><strong>Reason for Rejection:</strong> " + sReasonRejected + "</p>";
                                                //        dataMail.mail_body += "    </div>";

                                                //    }
                                                //}

                                                //if (tempTravel != null && tempTravel?.Count > 0)
                                                //{
                                                //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //    dataMail.mail_body += "        <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                                //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif;'>";
                                                //    dataMail.mail_body += "            <thead>";
                                                //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                //    dataMail.mail_body += "                </tr>";
                                                //    dataMail.mail_body += "            </thead>";
                                                //    dataMail.mail_body += "            <tbody>";

                                                //    var iNo = 1;
                                                //    foreach (var item in tempTravel)
                                                //    {
                                                //        dataMail.mail_body += "            <tr>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.emp_id}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.position}</td>";
                                                //        dataMail.mail_body += "            </tr>";
                                                //        iNo++;
                                                //    }

                                                //    dataMail.mail_body += "            </tbody>";
                                                //    dataMail.mail_body += "        </table>";
                                                //    dataMail.mail_body += "    </div>";
                                                //}

                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                                //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "</div>";

                                                //#endregion



                                                data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                            }
                                            catch (Exception ex)
                                            {
                                                write_log_mail("88-email.message-submitFlow3", "error-reject" + ex.ToString());
                                            }
                                            #endregion
                                        }

                                        // ส่งอีเมลสำหรับผู้ถูกอนุมัติ
                                        if (approvedTravelers.Any())
                                        {
                                            // หาผู้อนุมัติ CAP ลำดับถัดไป (เฉพาะกรณีอนุมัติ)
                                            if (apprAllStatus && !apprAllCAP)
                                            {
                                                // ส่งเมลไปยัง CAP Approver
                                                #region "#### SEND MAIL NEXT CAP ####"
                                                try
                                                {
                                                    // หาผู้อนุมัติ CAP ลำดับถัดไป
                                                    sql = @"SELECT DISTINCT 
    a.dta_appr_empid AS user_id,
    NVL(b.ENTITLE, '') || ' ' || b.ENFIRSTNAME || ' ' || b.ENLASTNAME AS user_name,
    b.email AS email
FROM bz_doc_traveler_approver a
LEFT JOIN vw_bz_users b ON a.dta_appr_empid = b.employeeid
WHERE 
    a.dh_code = :doc_id
    AND a.dta_type = 2
    AND a.dta_action_status = '2'
    AND a.dta_appr_level = (
        SELECT MIN(a2.dta_appr_level)
        FROM bz_doc_traveler_approver a2
        WHERE a2.dh_code = a.dh_code
          AND a2.dta_type = 2
          AND a2.dta_action_status = '2'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM bz_doc_traveler_approver a3
        WHERE a3.dh_code = a.dh_code
          AND a3.dta_type = 2
          AND a3.dta_appr_empid = a.dta_appr_empid
          AND a3.dta_appr_level <> a.dta_appr_level
    )";



                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                    var empapp = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                    if (empapp != null && empapp?.Count > 0)
                                                    {
                                                        foreach (var iemp in empapp)
                                                        {

                                                            cap_approver_mail = iemp.email ?? "";

                                                            // หา Line Approver ที่เกี่ยวข้อง
                                                            sql = @"select distinct b.email from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_appr_empid = b.employeeid where a.dta_type = 1 and a.dta_action_status in (3) and a.dta_doc_status = 32
    and a.dh_code = :doc_id";

                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                            var approverLineMailList = context.TempEMailModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                            line_approver_mail = "";
                                                            if (approverLineMailList != null && approverLineMailList.Count > 0)
                                                            {
                                                                for (int m = 0; m < approverLineMailList.Count; m++)
                                                                {
                                                                    line_approver_mail += approverLineMailList[m].email + ";";
                                                                }
                                                            }

                                                            // หา Traveler ที่เกี่ยวข้องกับ CAP Approver นี้
                                                            sql = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, 
    b.email as name2, b.employeeid as name3, b.orgname as name4                      
    from bz_doc_traveler_approver a
    inner join vw_bz_users b on a.dta_travel_empid = b.employeeid 
    left join (select min(dte_id) as id, dh_code, dte_emp_id from BZ_DOC_TRAVELER_EXPENSE 
    group by dh_code, dte_emp_id) s
    on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id  
    where a.dta_type = 2 and (a.dta_doc_status in (41) and a.dta_action_status = 2)
    and a.dh_code = :doc_id and a.dta_appr_empid = :item_approver_emp_id 
    order by s.id";

                                                            var item_approver_emp_id = iemp.user_id ?? "";
                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                            parameters.Add(context.ConvertTypeParameter("item_approver_emp_id", item_approver_emp_id, "char"));
                                                            var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                            traveler_mail = "";
                                                            if (tempTravel != null)
                                                            {
                                                                foreach (var item in tempTravel)
                                                                {
                                                                    traveler_mail += item.name2 + ";";
                                                                }
                                                            }

                                                            sendEmailModel dataMail = new sendEmailModel();
                                                            //to : CAP Approval
                                                            //cc : Line Approval, Super admin, Requester, Traveller
                                                            dataMail.mail_to = cap_approver_mail;
                                                            dataMail.mail_cc = line_approver_mail + (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) +
                                                                                (requester_mail + on_behalf_of_mail) + traveler_mail;

                                                            var resMailShowCase = $"To: {cap_approver_mail?.ToLower()} (CAP Approver)";
                                                            resMailShowCase += $"<br>Cc: {line_approver_mail?.ToLower()} (Line Approver)";
                                                            resMailShowCase += $", {super_admin_mail?.ToLower()} (Super Admin)";
                                                            resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                            //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                                            resMailShowCase += $", {requester_mail?.ToLower()} (Requester)";
                                                            resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                            resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                                            dataMail.mail_show_case = resMailShowCase;

                                                            // Use different subject and message if this is a combined approver

                                                            dataMail.mail_subject = $"{value.doc_id}: Please approve business travel request as CAP";
                                                            sDear = $"Dear {iemp.user_name},";
                                                            sDetail = "Please approve business travel request as CAP. To view the details, click ";

                                                            sDetail += $"<a href='{LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id)}'>{value.doc_id}</a>";

                                                            #region Build Email Body
                                                            dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";
                                                            if (tempTravel != null && tempTravel?.Count > 0)
                                                            {
                                                                dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                                var iNo = 1;
                                                                foreach (var item in tempTravel)
                                                                {
                                                                    dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                    </tr>";
                                                                    iNo++;
                                                                }

                                                                dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                                            }

                                                            dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                                            #endregion
                                                            //#region Build Email Body
                                                            //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                            //dataMail.mail_body += "    </div>";
                                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                            //dataMail.mail_body += "    </div>";
                                                            //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                                            //dataMail.mail_body += "    </div>";

                                                            //if (tempTravel != null && tempTravel?.Count > 0)
                                                            //{
                                                            //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                            //    dataMail.mail_body += "        <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                                            //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-size: 14px;'>";
                                                            //    dataMail.mail_body += "            <thead>";
                                                            //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                            //    dataMail.mail_body += "                </tr>";
                                                            //    dataMail.mail_body += "            </thead>";
                                                            //    dataMail.mail_body += "            <tbody>";

                                                            //    var iNo = 1;
                                                            //    foreach (var item in tempTravel)
                                                            //    {
                                                            //        dataMail.mail_body += "            <tr>";
                                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                                            //        dataMail.mail_body += "            </tr>";
                                                            //        iNo++;
                                                            //    }

                                                            //    dataMail.mail_body += "            </tbody>";
                                                            //    dataMail.mail_body += "        </table>";
                                                            //    dataMail.mail_body += "    </div>";
                                                            //}

                                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                                            //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                            //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                            //dataMail.mail_body += "    </div>";
                                                            //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                            //dataMail.mail_body += "    </div>";
                                                            //dataMail.mail_body += "</div>";
                                                            //#endregion

                                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                            dataMail.doc_id = doc_id;

                                                            var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                                            if (!string.IsNullOrEmpty(attachments_doc))
                                                            {
                                                                dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                                            }

                                                            data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    write_log_mail("88-email.message-submitFlow3", "error-approve to cap" + ex.ToString());
                                                }
                                                #endregion "#### SEND MAIL NEXT CAP ####"
                                            }
                                            else if (apprAllStatus && apprAllCAP)
                                            {
                                                Set_Trip_Complated(context, value.token_login, value.doc_id);

                                                string traveler_reject_mail = "";
                                                sql = @"select distinct '' as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2, '' as name3, '' as name4 from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_travel_empid = b.employeeid where a.dta_type = 2 and a.dta_action_status in (5) and a.dta_doc_status = 42 and a.dh_code = :doc_id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var traveler_reject = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (traveler_reject != null) traveler_reject_mail = string.Join(";", traveler_reject.Select(item => item.name2));

                                                sql = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2, b.employeeid as name3, b.orgname as name4 
                                                from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_travel_empid = b.employeeid 
                                                left join (select min(dte_id) as id, dh_code, dte_emp_id 
                                                from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s on a.dh_code = s.dh_code and a.dta_travel_empid = s.dte_emp_id
                                                where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42 and a.dh_code = :doc_id and not exists ( 
                                                select 1 from bz_doc_traveler_approver sub where sub.dh_code = a.dh_code and sub.dta_travel_empid = a.dta_travel_empid and sub.dta_action_status = 5)  order by s.id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (tempTravel != null) traveler_mail = string.Join(";", tempTravel.Select(item => item.name2));


                                                sql = @"select distinct b.employeeid as user_id, b.employeeid as user_name, b.email from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_appr_empid = b.employeeid where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42 and a.dh_code = :doc_id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var approverCAPMailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (approverCAPMailList != null && approverCAPMailList.Count > 0) cap_approver_mail = string.Join(";", approverCAPMailList.Select(m => m.email));

                                                sendEmailModel dataMail = new sendEmailModel();
                                                dataMail.mail_to = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) + traveler_mail + (line_approver_mail + cap_approver_mail) + (requester_mail + on_behalf_of_mail) + traveler_reject_mail;

                                                var resMailShowCase = $"To: {super_admin_mail?.ToLower()} (Super Admin)";
                                                resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                                resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                                resMailShowCase += $", {line_approver_mail?.ToLower()} (Line Approver)";
                                                resMailShowCase += $", {cap_approver_mail?.ToLower()} (CAP Approver) ";
                                                resMailShowCase += $", {requester_mail?.ToLower()} (Requester)";
                                                resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                resMailShowCase += $", {traveler_reject_mail?.ToLower()} (Traveller Reject)";
                                                dataMail.mail_show_case = resMailShowCase;

                                                dataMail.mail_subject = value.doc_id + " : The request for business travel has been approved";
                                                sDear = "Dear All,";
                                                sDetail = "The request for business travel has been approved. To view the approval details, click ";
                                                sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";
                                                sDetail += "<br>Any additional arrangements require to complete by the traveler. To view travel details, click ";
                                                sDetail += "<a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id) + "'>travel details.</a>";

                                                //#region Build Mail Body
                                                //dataMail.mail_body = @"<table style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto; border: 0;' cellpadding='0' cellspacing='0'>";
                                                //dataMail.mail_body += "    <tr>";
                                                //dataMail.mail_body += "        <td style='padding: 20px 0;'>";
                                                //dataMail.mail_body += $"           <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                //dataMail.mail_body += "        </td>";
                                                //dataMail.mail_body += "    </tr>";
                                                //dataMail.mail_body += "    <tr>";
                                                //dataMail.mail_body += "        <td style='padding: 0 0 20px 0;'>";
                                                //dataMail.mail_body += $"           <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                //dataMail.mail_body += "        </td>";
                                                //dataMail.mail_body += "    </tr>";

                                                //dataMail.mail_body += "    <tr>";
                                                //dataMail.mail_body += "        <td style='padding: 0 0 20px 0;'>";
                                                //dataMail.mail_body += "            <table style='width:600; background-color: #F3DDFF; padding: 15px; border-radius: 4px; box-sizing: border-box; border: 0;' cellpadding='0' cellspacing='0'>";
                                                //dataMail.mail_body += "                <tr>";
                                                //dataMail.mail_body += $"                   <td style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</td>";
                                                //dataMail.mail_body += "                </tr>";
                                                //dataMail.mail_body += "                <tr>";
                                                //dataMail.mail_body += $"                   <td style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</td>";
                                                //dataMail.mail_body += "                </tr>";
                                                //dataMail.mail_body += "                <tr>";
                                                //dataMail.mail_body += $"                   <td style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</td>";
                                                //dataMail.mail_body += "                </tr>";
                                                //dataMail.mail_body += "            </table>";
                                                //dataMail.mail_body += "        </td>";
                                                //dataMail.mail_body += "    </tr>";

                                                //if (tempTravel != null && tempTravel?.Count > 0)
                                                //{
                                                //    dataMail.mail_body += "    <tr>";
                                                //    dataMail.mail_body += "        <td style='padding: 0 0 20px 0;'>";
                                                //    dataMail.mail_body += "            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                                //    dataMail.mail_body += "            <table style='width: auto; border-collapse: collapse; font-size: 14px; border: 1px solid #ccc;'>";
                                                //    dataMail.mail_body += "                <thead>";
                                                //    dataMail.mail_body += "                    <tr style='background-color: #A7D0F0;'>";
                                                //    dataMail.mail_body += "                        <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                //    dataMail.mail_body += "                        <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                //    dataMail.mail_body += "                        <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                //    dataMail.mail_body += "                        <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                //    dataMail.mail_body += "                    </tr>";
                                                //    dataMail.mail_body += "                </thead>";
                                                //    dataMail.mail_body += "                <tbody>";

                                                //    var iNo = 1;
                                                //    foreach (var items in tempTravel)
                                                //    {
                                                //        dataMail.mail_body += "                    <tr>";
                                                //        dataMail.mail_body += $"                       <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>";
                                                //        dataMail.mail_body += $"                       <td style='padding: 8px; border: 1px solid #ccc;'>{items.name1}</td>";
                                                //        dataMail.mail_body += $"                       <td style='padding: 8px; border: 1px solid #ccc;'>{items.name3}</td>";
                                                //        dataMail.mail_body += $"                       <td style='padding: 8px; border: 1px solid #ccc;'>{items.name4}</td>";
                                                //        dataMail.mail_body += "                    </tr>";
                                                //        iNo++;
                                                //    }

                                                //    dataMail.mail_body += "                </tbody>";
                                                //    dataMail.mail_body += "            </table>";
                                                //    dataMail.mail_body += "        </td>";
                                                //    dataMail.mail_body += "    </tr>";
                                                //}

                                                //dataMail.mail_body += "    <tr>";
                                                //dataMail.mail_body += "        <td style='padding: 20px 0; font-size: 14px;'>";
                                                //dataMail.mail_body += "            <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                //dataMail.mail_body += "            <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                //dataMail.mail_body += "        </td>";
                                                //dataMail.mail_body += "    </tr>";

                                                //dataMail.mail_body += "    <tr>";
                                                //dataMail.mail_body += "        <td style='padding: 15px 0; border-top: 1px solid #eeeeee; font-size: 14px;'>";
                                                //dataMail.mail_body += "            <p style='margin: 0;'>Best Regards,</p>";
                                                //dataMail.mail_body += "            <p style='margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                //dataMail.mail_body += "        </td>";
                                                //dataMail.mail_body += "    </tr>";
                                                //dataMail.mail_body += "</table>";
                                                //#endregion

                                                #region Build Email Body
                                                dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";
                                                if (tempTravel != null && tempTravel?.Count > 0)
                                                {
                                                    dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                    var iNo = 1;
                                                    foreach (var item in tempTravel)
                                                    {
                                                        dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                    </tr>";
                                                        iNo++;
                                                    }

                                                    dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                                }

                                                dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                                #endregion



                                                try
                                                {
                                                    var file_attachments = file_attached_approval_form(value.doc_id, value.token_login);
                                                    if (!string.IsNullOrEmpty(file_attachments)) dataMail.mail_attachments = file_attachments;
                                                }
                                                catch { }

                                                var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                                if (!string.IsNullOrEmpty(attachments_doc)) dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? dataMail.mail_attachments + "|" : "") + attachments_doc;
                                                var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                dataMail.doc_id = doc_id;
                                                data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);

                                                if (notiPassport)
                                                {
                                                    foreach (var item in tempTravel)
                                                    {
                                                        sql = @"select distinct emp_id as id, '' as name1, '' as name2, '' as name3, '' as name4 from bz_data_passport where default_type = 'true' and to_date(passport_date_expire,'dd Mon yyyy') >= sysdate and to_date(passport_date_expire, 'dd Mon yyyy') <= sysdate + 180 and emp_id = :item_traveler_id";
                                                        parameters = new List<OracleParameter>();
                                                        parameters.Add(context.ConvertTypeParameter("item_traveler_id", item.name3, "char"));
                                                        var dataPassport = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                        if (dataPassport == null || dataPassport.Count == 0)
                                                        {
                                                            sendEmailModel passportMail = new sendEmailModel();
                                                            passportMail.mail_to = item.name2;
                                                            passportMail.mail_cc = (super_admin_mail + pmsv_admin_mail);
                                                            passportMail.mail_subject = value.doc_id + " : Please update Passport information - " + item.name1;
                                                            var MailShowCase = $"To: {item.name2.ToLower()} (Traveller)";
                                                            MailShowCase += $"<br>Cc: {super_admin_mail?.ToLower()} (Super Admin)";
                                                            MailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                            passportMail.mail_show_case = resMailShowCase;
                                                            passportMail.mail_body = @"
<table cellpadding='0' cellspacing='0' width='100%' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; max-width: 800px;'>
    <tr>
        <td style='padding: 20px 0 0 0;'>
            <p style='margin: 0;'>Dear " + item.name1 + @",</p>
        </td>
    </tr>

    <tr>
        <td style='padding: 10px 0 10px 0;'>
            <p style='margin: 0 0 10px 0; '>
                You are required to update your passport information in order to proceed with travel arrangements.<br/>
                To view travel details, click 
                <a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id).Replace("travelerhistory", "passport") + @"' style='color: #1a0dab; text-decoration: underline;'>" + value.doc_id + @"</a>.
            </p>
        </td>
    </tr>

    <tr>
        <td style='padding: 20px 0 0 0;'>
            <table width='60%' cellpadding='3' cellspacing='0' style='background-color: #F3DDFF; font-family: Aptos, Arial, sans-serif; font-size: 14px;'>
                <tr><td><strong>Title:</strong> " + sTitle + @"</td></tr>
                <tr><td><strong>Business Date:</strong> " + sBusinessDate + @"</td></tr>
                <tr><td><strong>Location:</strong> " + sLocation + @"</td></tr>
            </table>
        </td>
    </tr>

    <tr>
        <td style='padding: 20px 0 0 0;'>
            <p style='margin: 0 0 5px 0; '>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + @").</p>
            <p style='margin: 0; '>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + @").</p>
        </td>
    </tr>

    <tr>
        <td style='padding: 15px 0 0 0;'>
            <p style='margin: 0; '>Best Regards,</p>
            <p style='margin: 0; font-weight: bold; '>Business Travel Services Team (PMSV)</p>
        </td>
    </tr>
</table>"
;

                                                            statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                            passportMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                            passportMail.doc_id = doc_id;
                                                            SendEmailServiceTravelerProfile.SendMailFlowTrip(passportMail);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {

                                                // ส่งเมลไปยัง Line Approver ลำดับถัดไป
                                                var tempEmpForAction_def = new List<BZ_DOC_ACTION>();
                                                foreach (var item in value.traveler_summary)
                                                {
                                                    if (string.IsNullOrEmpty(item.appr_id)) continue;
                                                    if (!(item.appr_id == user_id)) continue;

                                                    var row_check_3Emp = value.traveler_summary.Where(p =>
                                                        p.appr_id == item.appr_id
                                                        && p.traverler_id == item.traverler_id && p.appr_status != "false").ToList();

                                                    if (row_check_3Emp == null || row_check_3Emp.Count == 0) continue;

                                                    var dta_type = "1";
                                                    var appr_id = item.appr_id.ToString();
                                                    var traveler_id = item.traverler_id.ToString();

                                                    // ตรวจสอบว่าเคยดำเนินการกับผู้อนุมัตินี้ไปแล้วหรือไม่
                                                    if (tempEmpForAction_def.Any(p => p.EMP_ID.Equals(appr_id))) continue;

                                                    tempEmpForAction_def.Add(new BZ_DOC_ACTION { EMP_ID = appr_id });

                                                    // STEP 1: ดึงข้อมูล Approvers ทั้งหมด
                                                    var allApprovers = context.BZ_DOC_TRAVELER_APPROVER
                                                        .AsNoTracking()
                                                        .Where(x => x.DTA_TYPE == "1" && x.DH_CODE == doc_id)
                                                        .ToList();

                                                    // STEP 2: หาผู้อนุมัติถัดไป

                                                    var appr_id_next = "";
                                                    var list_appr_level_now = context.TempEmpIdModelList.FromSqlRaw(
                                                        @"SELECT NVL(MAX(dta_appr_level), 0) AS emp_id
        FROM BZ_DOC_TRAVELER_APPROVER
        WHERE dta_type = 1
        AND DH_CODE = :id_doc  
        AND DTA_ACTION_STATUS = 2
        AND DTA_TRAVEL_EMPID = :dta_travel_empid",
                                                        context.ConvertTypeParameter("dta_travel_empid", traveler_id, "char"),
                                                        context.ConvertTypeParameter("id_doc", doc_id, "char")).ToList();

                                                    if (list_appr_level_now.Count > 0)
                                                    {
                                                        var dta_appr_level_now = list_appr_level_now[0].emp_id;

                                                        var list_appr_level_next = context.TempEmpIdModelList.FromSqlRaw(
                                                            @"SELECT dta_appr_empid AS emp_id
            FROM BZ_DOC_TRAVELER_APPROVER
            WHERE dta_type = 1
            AND DH_CODE = :id_doc  
            AND DTA_ACTION_STATUS = 2
            AND DTA_TRAVEL_EMPID = :dta_travel_empid
            AND dta_appr_level = :dta_appr_level",
                                                            context.ConvertTypeParameter("dta_travel_empid", traveler_id, "char"),
                                                            context.ConvertTypeParameter("id_doc", doc_id, "char"),
                                                            context.ConvertTypeParameter("dta_appr_level", dta_appr_level_now, "char")).ToList();

                                                        if (list_appr_level_next.Count > 0)
                                                        {
                                                            appr_id_next = list_appr_level_next[0].emp_id ?? "";
                                                        }

                                                        if (!string.IsNullOrEmpty(appr_id_next))
                                                        {

                                                            // STEP 3: ตรวจสอบประเภท Approver
                                                            var approverDetails = allApprovers
                                                                .Where(x => x.DTA_APPR_EMPID == appr_id_next)
                                                                .ToList();

                                                            bool isMultiLevel = approverDetails.Select(x => x.DTA_APPR_LEVEL).Distinct().Count() > 1;
                                                            var travelersForApprover = approverDetails.Select(x => x.DTA_TRAVEL_EMPID).Distinct().ToList();
                                                            bool isSingleTraveler = travelersForApprover.Count == 1;

                                                            // STEP 4: ตรวจสอบเงื่อนไขการอนุมัติ
                                                            bool shouldSendEmail = true;
                                                            var pendingReasons = new List<string>();

                                                            if (isMultiLevel)
                                                            {
                                                                // กรณี Multi-Level Approver
                                                                foreach (var level in approverDetails.Select(x => x.DTA_APPR_LEVEL).Distinct().OrderBy(x => x))
                                                                {
                                                                    //var travelers = approverDetails
                                                                    //    .Where(x => x.DTA_APPR_LEVEL == 1)
                                                                    //    .Select(x => x.DTA_TRAVEL_EMPID)
                                                                    //    .Distinct();


                                                                    // หาผู้อนุมัติระดับก่อนหน้าทั้งหมด (ที่ไม่ใช่ผู้อนุมัติปัจจุบัน)
                                                                    var prevLevelApprovers = allApprovers
                                                                        .Where(x =>
                                                                                   x.DTA_APPR_LEVEL == decimal.Parse(dta_appr_level_now) - 1 &&
                                                                                   x.DTA_APPR_EMPID != appr_id_next)
                                                                        .ToList();

                                                                    // ตรวจสอบว่าผู้อนุมัติระดับก่อนหน้าทุกคน (ที่ไม่ใช่ตัวเอง) อนุมัติแล้ว
                                                                    bool allApproved = prevLevelApprovers.Count > 0 &&
                                                                                     prevLevelApprovers.All(x => x.DTA_DOC_STATUS != 31);

                                                                    if (!allApproved)
                                                                    {
                                                                        shouldSendEmail = false;

                                                                    }

                                                                }
                                                            }
                                                            else if (isSingleTraveler)
                                                            {
                                                                // กรณี Single-Level Approver และ Single Traveler
                                                                var tid = travelersForApprover.First();
                                                                var approverLevel = approverDetails.First().DTA_APPR_LEVEL;

                                                                var prevLevelApprover = allApprovers
                                                                    .FirstOrDefault(x => x.DTA_TRAVEL_EMPID == tid &&
                                                                                       x.DTA_APPR_LEVEL == approverLevel - 1);

                                                                if (prevLevelApprover == null || prevLevelApprover.DTA_DOC_STATUS == 31)
                                                                {
                                                                    shouldSendEmail = false;
                                                                    //pendingReasons.Add($"Traveler {tid} ยังไม่อนุมัติระดับ {approverLevel - 1}");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // กรณี Single-Level Approver แต่มีหลาย Traveler
                                                                foreach (var tid in travelersForApprover)
                                                                {
                                                                    var approverLevel = approverDetails
                                                                        .First(x => x.DTA_TRAVEL_EMPID == tid).DTA_APPR_LEVEL;

                                                                    var prevLevelApprover = allApprovers
                                                                        .FirstOrDefault(x => x.DTA_TRAVEL_EMPID == tid &&
                                                                                           x.DTA_APPR_LEVEL == approverLevel - 1);

                                                                    if (prevLevelApprover == null || prevLevelApprover.DTA_DOC_STATUS == 31)
                                                                    {
                                                                        shouldSendEmail = false;
                                                                        //pendingReasons.Add($"Traveler {tid} ยังไม่อนุมัติระดับ {approverLevel - 1}");
                                                                    }
                                                                }
                                                            }
                                                            // ==============================================
                                                            // STEP 5: ดำเนินการส่งอีเมล
                                                            // ==============================================
                                                            if (shouldSendEmail)
                                                            {
                                                                if (!string.IsNullOrEmpty(appr_id_next))
                                                                {
                                                                    // ตรวจสอบว่าเป็น Combined Approver หรือไม่
                                                                    var isCombinedApprover = context.TempEmpIdModelList.FromSqlRaw(
                                                                        @"SELECT dta_appr_empid AS emp_id
            FROM BZ_DOC_TRAVELER_APPROVER
            WHERE dta_type = 2
            AND DH_CODE = :id_doc  
            AND DTA_ACTION_STATUS = 2
            AND DTA_APPR_EMPID = :dta_appr_empid",
                                                                        context.ConvertTypeParameter("id_doc", doc_id, "char"),
                                                                        context.ConvertTypeParameter("dta_appr_empid", appr_id_next, "char")).ToList();

                                                                    var approverInfo = context.VW_BZ_USERS
                                                                        .Where(u => u.EMPLOYEEID == appr_id_next)
                                                                        .Select(u => new
                                                                        {
                                                                            u.EMPLOYEEID,
                                                                            UserName = u.ENTITLE + " " + u.ENFIRSTNAME + " " + u.ENLASTNAME,
                                                                            u.EMAIL
                                                                        })
                                                                        .FirstOrDefault();

                                                                    if (approverInfo != null)
                                                                    {
                                                                        line_approver_mail = approverInfo.EMAIL ?? "";

                                                                        // ดึงข้อมูล Traveler ทั้งหมดที่ผู้อนุมัตินี้ต้องอนุมัติ
                                                                        sql = @"select distinct to_char(s.id) as id, 
                    nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, 
                    b.email as name2, 
                    b.employeeid as name3, 
                    b.orgname as name4
                    from bz_doc_traveler_approver a
                    inner join vw_bz_users b on a.dta_travel_empid = b.employeeid
                    left join (
                        select min(dte_id) as id, dh_code, dte_emp_id 
                        from BZ_DOC_TRAVELER_EXPENSE 
                        group by dh_code, dte_emp_id
                    ) s on a.dh_code = s.dh_code and a.dta_travel_empid = s.dte_emp_id  
                    where a.dh_code = :doc_id and a.dta_appr_empid = :appr_id and a.dta_action_status = 2
                    order by s.id";

                                                                        parameters = new List<OracleParameter>();
                                                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                                        parameters.Add(context.ConvertTypeParameter("appr_id", approverInfo.EMPLOYEEID, "char"));
                                                                        var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                                        traveler_mail = "";
                                                                        if (tempTravel != null)
                                                                        {
                                                                            foreach (var itemt in tempTravel)
                                                                            {
                                                                                if (!string.IsNullOrEmpty(itemt.name2))
                                                                                {
                                                                                    traveler_mail += itemt.name2 + ";";
                                                                                }
                                                                            }
                                                                        }

                                                                        sendEmailModel dataMail = new sendEmailModel();
                                                                        //to : Line Approval	
                                                                        //cc : Requester, Traveller, Super Admin 
                                                                        dataMail.mail_to = line_approver_mail;
                                                                        dataMail.mail_cc = (requester_mail + on_behalf_of_mail) + traveler_mail +
                                                                                            (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail);

                                                                        var resMailShowCase = $"To: {line_approver_mail} (Line Approval)";
                                                                        resMailShowCase += $"<br>Cc: {requester_mail?.ToLower()} (Requester)";
                                                                        resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                                        resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                                                        resMailShowCase += $", {super_admin_mail?.ToLower()} (Super Admin)";
                                                                        resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                                        //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                                                        dataMail.mail_show_case = resMailShowCase;

                                                                        // ใช้ข้อความที่แตกต่างกันสำหรับผู้อนุมัติแบบ Combined หรือไม่
                                                                        if (isCombinedApprover.Any())
                                                                        {
                                                                            dataMail.mail_subject = $"{value.doc_id}: Please endorse business travel request as line manager / CAP";
                                                                            sDear = $"Dear {approverInfo.UserName},";
                                                                            sDetail = "Please endorse business travel as line manager / CAP. To view the details, click ";
                                                                        }
                                                                        else
                                                                        {
                                                                            dataMail.mail_subject = $"{value.doc_id}: Please endorse business travel request as line manager";
                                                                            sDear = $"Dear {approverInfo.UserName},";
                                                                            sDetail = "Please endorse business travel as line manager. To view the details, click ";
                                                                        }
                                                                        sDetail += $"<a href='{(LinkLogin + "ii").Replace("###", value.doc_id)}'>{value.doc_id}</a>";

                                                                        //#region Build Email Body
                                                                        //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                                                        //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                                        //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                                        //dataMail.mail_body += "    </div>";
                                                                        //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                                        //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                                        //dataMail.mail_body += "    </div>";
                                                                        //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                                                        //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                                                        //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                                                        //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                                                        //dataMail.mail_body += "    </div>";

                                                                        //if (tempTravel != null && tempTravel?.Count > 0)
                                                                        //{
                                                                        //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                                        //    dataMail.mail_body += "        <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                                                        //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-size: 14px;'>";
                                                                        //    dataMail.mail_body += "            <thead>";
                                                                        //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                                        //    dataMail.mail_body += "                </tr>";
                                                                        //    dataMail.mail_body += "            </thead>";
                                                                        //    dataMail.mail_body += "            <tbody>";

                                                                        //    var iNo = 1;
                                                                        //    foreach (var items in tempTravel)
                                                                        //    {
                                                                        //        dataMail.mail_body += "            <tr>";
                                                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{items.name1}</td>";
                                                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{items.name3}</td>";
                                                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{items.name4}</td>";
                                                                        //        dataMail.mail_body += "            </tr>";
                                                                        //        iNo++;
                                                                        //    }

                                                                        //    dataMail.mail_body += "            </tbody>";
                                                                        //    dataMail.mail_body += "        </table>";
                                                                        //    dataMail.mail_body += "    </div>";
                                                                        //}

                                                                        //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                                                        //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                                        //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                                        //dataMail.mail_body += "    </div>";
                                                                        //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                                                        //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                                                        //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                                        //dataMail.mail_body += "    </div>";
                                                                        //dataMail.mail_body += "</div>";
                                                                        //#endregion

                                                                        #region Build Email Body
                                                                        dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";
                                                                        if (tempTravel != null && tempTravel?.Count > 0)
                                                                        {
                                                                            dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                                            var iNo = 1;
                                                                            foreach (var items in tempTravel)
                                                                            {
                                                                                dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{items.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{items.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{items.name4}</td>
                    </tr>";
                                                                                iNo++;
                                                                            }

                                                                            dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                                                        }

                                                                        dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                                                        #endregion

                                                                        var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                                                        if (!string.IsNullOrEmpty(attachments_doc))
                                                                        {
                                                                            dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                                                        }
                                                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                                        dataMail.doc_id = doc_id;
                                                                        data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                catch (Exception ex)
                                {
                                    write_log_mail("88-email.message-submitFlow3", "error" + ex.ToString());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.status = "E";
                            data.message = ex.ToString();
                        }
                    }

                    if (data.status != "E" && value.action.type == "3") // revise
                    {
                        // Update document status to Pending by Super Admin when revised
                        sql = "UPDATE BZ_DOC_HEAD SET DH_DOC_STATUS=21, DH_VERSION=31 WHERE DH_CODE=:id_doc";
                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("id_doc", doc_id, "char"));
                        context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                        context.SaveChanges();
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

                    string requester_name = "";

                    #region DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin  
                    //PMSV Admin	PMDV Admin
                    string super_admin_mail = "";
                    string pmsv_admin_mail = "";
                    string pmdv_admin_mail = "";
                    string requester_mail = "";
                    string on_behalf_of_mail = "";
                    string traveler_mail = "";
                    string line_approver_mail = "";
                    string cap_approver_mail = "";

                    // super_admin_mail = get_mail_group_admin(context);
                    super_admin_mail = mail_group_admin(context, "super_admin");

                    //กรณีที่เป็น pmdv admin, pmsv_admin
                    pmsv_admin_mail = mail_group_admin(context, "pmsv_admin");
                    if (doc_id.IndexOf("T") > -1)
                    {
                        pmdv_admin_mail += mail_group_admin(context, "pmdv_admin");
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
                                    if (traverler_list != null && traverler_list.Count > 0)
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
                                        foreach (var traveler in value.traveler_summary.Where(t => t.take_action == "true"))
                                        {
                                            string traveler_id = traveler.traverler_id;
                                            //doc_status = "30";
                                            sql = @"UPDATE BZ_DOC_TRAVELER_APPROVER 
                                        SET DTA_APPR_STATUS = :item_appr_status,
                                            DTA_APPR_REMARK = :action_remark,
                                            DTA_DOC_STATUS = 40,
                                            DTA_ACTION_STATUS = 5 -- Rejected
                                        WHERE dh_code = :doc_id
                                        AND DTA_TYPE = 2
                                        AND DTA_STATUS = 1
                                        AND DTA_ACTION_STATUS = '2'  AND DTA_TRAVEL_EMPID = :travel_emp_id_select ";



                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            parameters.Add(context.ConvertTypeParameter("travel_emp_id_select", traveler_id, "char"));

                                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            if (iret > -1) { }
                                        }
                                    }

                                    else if (value.action.type == "3") // revise
                                    {
                                        var action_status_select = (item_appr_status == "true" ? "4" : "5");

                                        sql = @"UPDATE BZ_DOC_TRAVELER_APPROVER SET 
        DTA_APPR_STATUS = :item_appr_status,
        DTA_APPR_REMARK = :action_remark,
        DTA_DOC_STATUS = CASE 
                            WHEN DTA_TYPE = 1 THEN 31  -- Line Approver
                            WHEN DTA_TYPE = 2 THEN 41  -- CAP Approver
                            END,
        DTA_ACTION_STATUS = :action_status_select
        WHERE dh_code = :doc_id
        AND DTA_STATUS = 1";

                                        // ส่วนเงื่อนไขเพิ่มเติมสำหรับผู้ใช้เฉพาะ (ถ้ามี)
                                        //if (user_role != "1") // ถ้าไม่ใช่บทบาทที่ 1
                                        //{
                                        //    sql += " AND DTA_APPR_EMPID = :user_id_def";
                                        //}

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("item_appr_status", item_appr_status, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_remark", action_remark, "char"));
                                        parameters.Add(context.ConvertTypeParameter("action_status_select", action_status_select, "char"));
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        parameters.Add(context.ConvertTypeParameter("travel_emp_id_select", travel_emp_id_select, "char"));

                                        //if (user_role != "1")
                                        //{
                                        //    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        //}

                                        iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                        if (iret > -1)
                                        {
                                            // การดำเนินการสำเร็จ
                                        }
                                    }

                                    //                                    else if (value.action.type == "4" || value.action.type == "5") // approve/reject
                                    //                                    {
                                    //                                        var approveIds = value.traveler_summary
                                    //                                           .Where(t => t.take_action == "true" && t.appr_status == "true")
                                    //                                           .Select(t => $"'{t.traverler_id}'")
                                    //                                           .ToList();

                                    //                                        var rejectIds = value.traveler_summary
                                    //                                            .Where(t => t.take_action == "true" && t.appr_status == "false")
                                    //                                            .Select(t => $"'{t.traverler_id}'")
                                    //                                            .ToList();

                                    //                                        if (approveIds.Any() || rejectIds.Any())
                                    //                                        {
                                    //                                            // Update APPROVED travelers
                                    //                                            if (approveIds.Any())
                                    //                                            {
                                    //                                                if (user_role == "1")
                                    //                                                {
                                    //                                                    // Admin can ONLY update DTA_TYPE = '1'
                                    //                                                    sql = $@"
                                    //UPDATE BZ_DOC_TRAVELER_APPROVER 
                                    //SET 
                                    //DTA_APPR_STATUS = 'true', 
                                    //DTA_DOC_STATUS = 42,  -- Status for type 1
                                    //DTA_ACTION_STATUS = '3'
                                    //WHERE 
                                    //dh_code = :doc_id 
                                    //AND DTA_STATUS = 1
                                    //AND DTA_ACTION_STATUS = '2'
                                    //AND DTA_TYPE = 2  -- จำกัดเฉพาะ type 1
                                    //AND DTA_TRAVEL_EMPID IN  ( SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:approveIds)) ) ";
                                    //                                                }
                                    //                                                else
                                    //                                                {
                                    //                                                    // Non-admin can update BOTH types but must be their approval
                                    //                                                    sql = $@"
                                    //UPDATE BZ_DOC_TRAVELER_APPROVER 
                                    //SET 
                                    //DTA_APPR_STATUS = 'true', 
                                    //DTA_DOC_STATUS = 42 ,
                                    //DTA_ACTION_STATUS = '3'
                                    //WHERE 
                                    //dh_code = :doc_id 
                                    //AND DTA_STATUS = 1
                                    //AND DTA_ACTION_STATUS = '2'
                                    //AND DTA_TYPE = 2
                                    //AND DTA_TRAVEL_EMPID IN ( SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:approveIds)) )  
                                    //AND DTA_APPR_EMPID = :user_id";
                                    //                                                }

                                    //                                                parameters = new List<OracleParameter>();
                                    //                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    //                                                parameters.Add(context.ConvertTypeParameter("approveIds", string.Join(",", approveIds), "char"));

                                    //                                                if (user_role != "1")
                                    //                                                {
                                    //                                                    parameters.Add(context.ConvertTypeParameter("user_id", user_id_def, "char"));
                                    //                                                }

                                    //                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    //                                            }

                                    //                                            // Update REJECTED travelers
                                    //                                            if (rejectIds.Any())
                                    //                                            {

                                    //                                                // Non-admin can reject BOTH types but must be their approval
                                    //                                                sql = $@"UPDATE BZ_DOC_TRAVELER_APPROVER
                                    //SET
                                    //    DTA_APPR_STATUS = 'false',
                                    //    DTA_DOC_STATUS = 40,
                                    //    DTA_ACTION_STATUS = '5'
                                    //    DTA_APPR_REMARK = :action_remark
                                    //WHERE
                                    //    dh_code = :doc_id
                                    //AND DTA_TYPE = 2
                                    //    AND DTA_ACTION_STATUS = '2'
                                    //AND DTA_TRAVEL_EMPID IN ( SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:rejectIds)) ) ";


                                    //                                                parameters = new List<OracleParameter>();
                                    //                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    //                                                parameters.Add(context.ConvertTypeParameter("rejectIds", string.Join(",", rejectIds), "char"));


                                    //                                                parameters.Add(context.ConvertTypeParameter("action_remark", value.action.remark, "char"));


                                    //                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                    //                                            }
                                    //                                        }
                                    //                                    }

                                    else if (value.action.type == "4" || value.action.type == "5") // approve/reject
                                    {
                                        // ดึง ID โดยไม่ใส่เครื่องหมายคำพูด
                                        var approveIds = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "true")
                                            .Select(t => t.traverler_id)
                                            .ToList();

                                        var rejectIds = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "false")
                                            .Select(t => t.traverler_id)
                                            .ToList();

                                        if (approveIds.Any() || rejectIds.Any())
                                        {
                                            // สร้างฟังก์ชันช่วยสร้าง IN clause แบบปลอดภัย
                                            string BuildSafeInClause(List<string> ids)
                                            {
                                                // Escape single quotes และสร้างรายการ ID ที่ปลอดภัย
                                                var safeIds = ids.Select(id => $"'{id.Replace("'", "''")}'");
                                                return string.Join(",", safeIds);
                                            }

                                            // Update APPROVED travelers
                                            if (approveIds.Any())
                                            {
                                                string safeInClause = BuildSafeInClause(approveIds);

                                                if (user_role == "1")
                                                {
                                                    // Admin can ONLY update DTA_TYPE = '2'
                                                    sql = $@"
UPDATE BZ_DOC_TRAVELER_APPROVER 
SET 
    DTA_APPR_STATUS = 'true', 
    DTA_DOC_STATUS = 42,
    DTA_ACTION_STATUS = '3',
    DTA_UPDATE_DATE = SYSDATE
WHERE 
    dh_code = :doc_id 
    AND DTA_STATUS = 1
    AND DTA_ACTION_STATUS = '2'
    AND DTA_TYPE = 2
    AND DTA_TRAVEL_EMPID IN ({safeInClause})";
                                                }
                                                else
                                                {
                                                    // Non-admin can update but must be their approval
                                                    sql = $@"
UPDATE BZ_DOC_TRAVELER_APPROVER 
SET 
    DTA_APPR_STATUS = 'true', 
    DTA_DOC_STATUS = 42,
    DTA_ACTION_STATUS = '3',
    DTA_UPDATE_DATE = SYSDATE
WHERE 
    dh_code = :doc_id 
    AND DTA_STATUS = 1
    AND DTA_ACTION_STATUS = '2'
    AND DTA_TYPE = 2
    AND DTA_TRAVEL_EMPID IN ({safeInClause})
    AND DTA_APPR_EMPID = :user_id";
                                                }

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(new OracleParameter("doc_id", OracleDbType.Varchar2)
                                                {
                                                    Value = doc_id,
                                                    Direction = ParameterDirection.Input
                                                });

                                                if (user_role != "1")
                                                {
                                                    parameters.Add(new OracleParameter("user_id", OracleDbType.Varchar2)
                                                    {
                                                        Value = user_id_def,
                                                        Direction = ParameterDirection.Input
                                                    });
                                                }

                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            }

                                            // Update REJECTED travelers
                                            if (rejectIds.Any())
                                            {
                                                string safeInClause = BuildSafeInClause(rejectIds);

                                                sql = $@"
UPDATE BZ_DOC_TRAVELER_APPROVER
SET
    DTA_APPR_STATUS = 'false',
    DTA_DOC_STATUS = 40,
    DTA_ACTION_STATUS = '5',
    DTA_APPR_REMARK = :action_remark,
    DTA_UPDATE_DATE = SYSDATE
WHERE
    dh_code = :doc_id
    AND DTA_TYPE = 2
    AND DTA_ACTION_STATUS = '2'
    AND DTA_TRAVEL_EMPID IN ({safeInClause})";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(new OracleParameter("doc_id", OracleDbType.Varchar2)
                                                {
                                                    Value = doc_id,
                                                    Direction = ParameterDirection.Input
                                                });
                                                parameters.Add(new OracleParameter("action_remark", OracleDbType.Varchar2)
                                                {
                                                    Value = value.action.remark ?? string.Empty,
                                                    Direction = ParameterDirection.Input
                                                });

                                                iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                            }
                                        }
                                    }

                                    //DevFix 20210618 0000 เพิ่มข้อมูล  dta_update_date 
                                    sql = " update BZ_DOC_TRAVELER_APPROVER set ";
                                    sql += " DTA_UPDATE_DATE = sysdate ";
                                    sql += " where dh_code = :doc_id ";
                                    sql += " and DTA_TYPE = 2 and DTA_STATUS = 1";
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
                                    sql = " update BZ_DOC_ACTION set  ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2, TAB_NO=2 ";
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
                                    sql = " update BZ_DOC_ACTION set  ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2, TAB_NO=2 ";
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


                            #region "#### BZ_DOC_FILE ####" 

                            // การสร้างเส้นทางไปยัง temp directory -->??? เดียวต้องเอามาจาก table เพื่อลดความเสี่ยงจาก scan
                            //New => d:\Ebiz\Service\file\OT21060025\EMPLOYEE LETTER_TOP_Mr._Luck_Saraya_180521102605.docx => server
                            //New => d:\Ebiz\Service\wwwroot\file\OT21060025\EMPLOYEE LETTER_TOP_Mr._Luck_Saraya_180521102605.docx => local
                            //string ServerPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI() ?? "";
                            //var file_path = $"{ServerPathAPI}file/{doc_id}/";
                            var _path_tempName = Path.GetFileName($"{AppDomain.CurrentDomain.BaseDirectory}/wwwroot/temp/{doc_id.ToString()}");
                            var _path_temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "temp", _path_tempName);
                            var _path_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "file", _path_tempName);

                            if (value?.docfile?.Count > 0)
                            {
                                ////delete --> insert 
                                //List<DocFileListInModel> docfileList = value.docfile;
                                //sql = " delete from BZ_DOC_FILE where DH_CODE = :doc_id ";

                                //parameters = new List<OracleParameter>();
                                //parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                //iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                //if (iret > -1) { }

                                //foreach (var item in docfileList)
                                //{
                                //    sql = " insert into BZ_DOC_FILE (DH_CODE, DF_ID, DF_NAME, DF_PATH, DF_REMARK, CREATED_BY, CREATED_DATE)";
                                //    sql += " values (:item_dh_code, :item_dh_name , :file_path , :item_dh_remark , :user_id_def, sysdate )";

                                //    var item_dh_code = item.DH_CODE ?? "";
                                //    var item_dh_name = item.DF_NAME ?? "";
                                //    var item_dh_remark = item.DF_REMARK ?? "";

                                //    parameters = new List<OracleParameter>();
                                //    parameters.Add(context.ConvertTypeParameter("item_dh_code", item_dh_code, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("item_dh_name", item_dh_name, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("item_dh_remark", item_dh_remark, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("file_path", _path_file, "char"));
                                //    parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                //    iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                                //    if (iret > -1) { }

                                //    imaxid_docfile += 1;
                                //}

                                var entitiesToDelete = context.BZ_DOC_FILE.Where(p => p.DH_CODE.Equals(doc_id)).ToList();
                                var docfileList = value.docfile;

                                if (entitiesToDelete != null || docfileList != null)
                                {
                                    // ลบข้อมูลทั้งหมด
                                    if (entitiesToDelete.Any())
                                    {
                                        context.BZ_DOC_FILE.RemoveRange(entitiesToDelete);
                                    }
                                    foreach (var d in docfileList)
                                    {
                                        context.BZ_DOC_FILE.Add(new BZ_DOC_FILE
                                        {
                                            DH_CODE = doc_id,
                                            DF_ID = Convert.ToDecimal(imaxid_docfile),
                                            DF_PATH = _path_file,
                                            DF_NAME = d.DF_NAME,
                                            //DF_FULLNAME = d.fullname,
                                            DF_REMARK = d.DF_REMARK,
                                            UPDATED_DATE = DateTime.Now,
                                            TOKEN_UPDATE = token_login
                                        });
                                        imaxid_docfile += 1;
                                    }

                                }

                            }
                            #endregion "#### BZ_DOC_FILE ####"


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
                                                // sql = @" select a.dta_appr_empid as emp_id, dta_appr_level as status_value, '' as doc_status
                                                //     from bz_doc_traveler_approver a   
                                                //     where dta_type = 2 and dta_doc_status = 41
                                                //     and a.dh_code = :doc_id and a.dta_travel_empid = :traverler_id_def ";
                                                // sql += @" and a.dta_appr_level > ( select  max(a1.dta_appr_level) as dta_appr_level
                                                //     from bz_doc_traveler_approver a1
                                                //     where a1.dta_type = 2 and a1.dta_doc_status in ( 40,42)
                                                //     and a1.dh_code = :doc_id  and a1.dta_travel_empid = :traverler_id_def )  ";

                                                // sql = "select emp_id, status_value, doc_status from(" + sql + ")t order by to_number(status_value)";
                                                StringBuilder sqlBuilder = new StringBuilder(@"
                                                   WITH t AS (
                                                   select a.dta_appr_empid as emp_id, dta_appr_level as status_value, '' as doc_status
                                                    from bz_doc_traveler_approver a   
                                                    where dta_type = 2 and dta_doc_status = 41
                                                    and a.dh_code = :doc_id and a.dta_travel_empid = :traverler_id_def
                                                    and a.dta_appr_level > ( select  max(a1.dta_appr_level) as dta_appr_level
                                                    from bz_doc_traveler_approver a1
                                                    where a1.dta_type = 2 and a1.dta_doc_status in ( 40,42)
                                                    and a1.dh_code = :doc_id  and a1.dta_travel_empid = :traverler_id_def )
                                                    )
                                                   select emp_id, status_value, doc_status from t 
                                                   order by to_number(status_value);
                                                   ");
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("traverler_id_def", traverler_id_def, "char"));
                                                var dataCAP_Def = context.AllApproveModelList.FromSqlRaw(sqlBuilder.ToString(), parameters.ToArray()).ToList();
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
                                                // sql = @" select a.dta_appr_empid as emp_id, dta_appr_level as status_value, '' as doc_status
                                                //     from bz_doc_traveler_approver a   
                                                //     where dta_type = 2 and dta_doc_status = 41
                                                //     and a.dh_code = :doc_id and a.dta_travel_empid = :traverler_id_def ";
                                                // sql += @" and a.dta_appr_level > ( select  max(a1.dta_appr_level) as dta_appr_level
                                                //     from bz_doc_traveler_approver a1
                                                //     where a1.dta_type = 2 and a1.dta_doc_status in (42)
                                                //     and a1.dh_code = :doc_id  and a1.dta_travel_empid =  :traverler_id_def )  ";

                                                // sql = "select status_value, emp_id, doc_status from(" + sql + ")t order by to_number(status_value)";
                                                StringBuilder sqlBuilder = new StringBuilder(@"
                                                   WITH t AS (
                                                  select a.dta_appr_empid as emp_id, dta_appr_level as status_value, '' as doc_status
                                                    from bz_doc_traveler_approver a   
                                                    where dta_type = 2 and dta_doc_status = 41
                                                    and a.dh_code = :doc_id and a.dta_travel_empid = :traverler_id_def
                                                    and a.dta_appr_level > ( select  max(a1.dta_appr_level) as dta_appr_level
                                                    from bz_doc_traveler_approver a1
                                                    where a1.dta_type = 2 and a1.dta_doc_status in (42)
                                                    and a1.dh_code = :doc_id  and a1.dta_travel_empid =  :traverler_id_def )
                                                    )
                                                   select emp_id, status_value, doc_status from t 
                                                   order by to_number(status_value);
                                                   ");
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                parameters.Add(context.ConvertTypeParameter("traverler_id_def", traverler_id_def, "char"));
                                                var dataCAP_Def = context.AllApproveModelList.FromSqlRaw(sqlBuilder.ToString(), parameters.ToArray()).ToList();
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
                                    sql = " update BZ_DOC_ACTION set ACTION_DATE=sysdate, ACTION_BY= :user_id_def, ACTION_STATUS=2, TAB_NO=2 ";
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

                            if (type_flow == true)
                            {
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
                                    sTitle = (doc_head_search.DH_TOPIC ?? "");
                                    if (doc_head_search.DH_BUS_FROMDATE != null)
                                    {
                                        sBusinessDate = dateFromTo(doc_head_search.DH_BUS_FROMDATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")), doc_head_search.DH_BUS_TODATE?.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"))) ?? "";
                                    }
                                    sql = @"select distinct to_char(s.id) as id, case when substr(a.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1, a.city_text as name2, '' as name3, '' as name4 FROM bz_doc_traveler_expense a inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code inner join VW_BZ_USERS U on a.DTE_Emp_Id = u.employeeid left join bz_master_country c on a.ct_id = c.ct_id left join BZ_MASTER_PROVINCE p on a.PV_ID = p.PV_ID left join (select min(dte_id) as id, dh_code, ctn_id, pv_id, city_text from BZ_DOC_TRAVELER_EXPENSE group by dh_code, ctn_id, pv_id, city_text) s on a.dh_code = s.dh_code and a.ctn_id = s.ctn_id and (case when a.pv_id is null then 1 else a.pv_id end = case when a.pv_id is null then 1 else s.pv_id end) and a.city_text = s.city_text WHERE a.dh_code = :doc_id and a.dte_status = 1 and a.dte_cap_appr_status = :doc_status order by s.id";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    parameters.Add(context.ConvertTypeParameter("doc_status", doc_status, "char"));
                                    var temp = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                    if (temp != null && temp.Count() > 0)
                                    {
                                        if (temp.Count == 1) sLocation = temp[0].name1 + "/" + temp[0].name2;
                                        else sLocation = string.Join(",", temp.Select(item => item.name1 + "/" + item.name2));
                                    }

                                    sql = @"select distinct b.employeeid as user_id, '' as user_name, b.email from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_appr_empid = b.employeeid where a.dta_type = 1 and a.dh_code = :doc_id";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                    var approverLineMailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                    if (approverLineMailList != null && approverLineMailList.Count > 0) line_approver_mail = string.Join(";", approverLineMailList.Select(m => m.email));

                                    if (value.action.type == "2")
                                    {
                                        #region traveler mail in doc
                                        traveler_mail = "";

                                        sql = @" select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2 
                                             , b.employeeid as name3, b.orgname as name4                      
                                             from bz_doc_traveler_approver a
                                             inner join vw_bz_users b on a.dta_travel_empid = b.employeeid
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
                                                traveler_mail += item.name2 + ";";
                                            }
                                        }
                                        #endregion traveler mail in doc

                                        #region approver mail in doc

                                        // First query - Line Approver Mail
                                        sql = @"SELECT DISTINCT b.employeeid AS user_id, '' AS user_name, b.email
        FROM bz_doc_traveler_approver a
        INNER JOIN vw_bz_users b ON a.dta_appr_empid = b.employeeid
        WHERE a.dta_type = 2 AND a.dta_action_status IN (5) AND a.dta_doc_status = 40
        AND a.dh_code = :doc_id";

                                        if (user_role != "1")
                                        {
                                            sql += " AND a.dta_appr_empid = :user_id_def";
                                        }

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        if (user_role != "1")
                                        {
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        }

                                        var approverLineMail = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                        line_approver_mail = "";
                                        if (approverLineMail != null && approverLineMail.Count > 0)
                                        {
                                            line_approver_mail = string.Join(";", approverLineMail.Select(x => x.email));
                                        }

                                        // Second query - CAP Approver Mail
                                        sql = @"SELECT DISTINCT b.employeeid AS user_id, '' AS user_name, b.email
        FROM bz_doc_traveler_approver a
        INNER JOIN vw_bz_users b ON a.dta_appr_empid = b.employeeid
        WHERE a.dta_type = 1 AND a.dta_action_status IN (3) AND a.dta_doc_status = 32
        AND (a.dta_travel_empid, a.dh_code) IN (
            SELECT DISTINCT a1.dta_travel_empid, a1.dh_code 
            FROM bz_doc_traveler_approver a1
            WHERE a1.dta_type = 2 AND a1.dta_action_status IN (5) AND a1.dta_doc_status = 40";

                                        if (user_role != "1")
                                        {
                                            // Find line under cap
                                            sql += " AND a1.dta_appr_empid = :user_id_def";
                                        }

                                        sql += @")
        AND a.dh_code = :doc_id";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        if (user_role != "1")
                                        {
                                            parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        }

                                        var approverCAPMailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                        cap_approver_mail = "";
                                        if (approverCAPMailList != null)
                                        {
                                            if (approverCAPMailList.Count > 0)
                                            {
                                                for (int m = 0; m < approverCAPMailList.Count; m++)
                                                {
                                                    cap_approver_mail += approverCAPMailList[m].email + ";";
                                                }
                                            }
                                        }

                                        #endregion approver mail in doc

                                        #region "#### SEND REJECTION MAIL ####" 
                                        try
                                        {
                                            sendEmailModel dataMail = new sendEmailModel();

                                            var doc_head_create_by = doc_head_search.DH_CREATE_BY ?? "";
                                            sql = "SELECT b.employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                            sql += "FROM vw_bz_users b WHERE employeeid= :doc_head_create_by";

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_head_create_by", doc_head_create_by, "char"));
                                            var requestor = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            dataMail.mail_to = requester_mail + traveler_mail;
                                            dataMail.mail_cc = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) + line_approver_mail + on_behalf_of_mail;

                                            var resMailShowCase = $"To: {requester_mail?.ToLower()} (Requester)";
                                            resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                            resMailShowCase += $"<br>Cc: {super_admin_mail?.ToLower()} (Super Admin)";
                                            resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                            //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                            resMailShowCase += $", {line_approver_mail?.ToLower()} (Line Approval)";
                                            resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                            dataMail.mail_show_case = resMailShowCase;

                                            dataMail.mail_subject = $"{doc_id}: The request for business travel has been rejected";

                                            sDear = "Dear All,";
                                            sDetail = $"Your business travel request has been rejected by {user_name}. To view the details, click ";
                                            sDetail += $"<a href='{(LinkLogin + "ii").Replace("###", doc_id)}'>{doc_id}</a>";

                                            #region Build Email Body
                                            dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                            if (value.action.type == "2")
                                            {
                                                sReasonRejected = value.traveler_summary
                                                    .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                                    .Select(x => x.appr_remark)
                                                    .FirstOrDefault();

                                                if (!string.IsNullOrEmpty(sReasonRejected))
                                                {
                                                    dataMail.mail_body += $@"
            <table width='60%' cellpadding='10' cellspacing='0' border='0' style='background-color: #fff8f8; border-left: 4px solid #ffcccc; margin-bottom: 10px;'>
                <tr><td style='font-size: 14px; color: #d9534f;'><strong>Reason for Rejection:</strong> {sReasonRejected}</td>
                    </tr>
                </table>
            </td>
        </tr>";
                                                }
                                            }

                                            if (tempTravel != null && tempTravel?.Count > 0)
                                            {
                                                dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                var iNo = 1;
                                                foreach (var item in tempTravel)
                                                {
                                                    dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                    </tr>";
                                                    iNo++;
                                                }

                                                dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                            }

                                            dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                            #endregion

                                            //#region Build Email Body
                                            //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                            //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                            //dataMail.mail_body += "    </div>";

                                            //if (value.action.type == "2")
                                            //{
                                            //    sReasonRejected = value.traveler_summary
                                            //        .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                            //        .Select(x => x.appr_remark)
                                            //        .FirstOrDefault();

                                            //    if (!string.IsNullOrEmpty(sReasonRejected))
                                            //    {
                                            //        dataMail.mail_body += "    <div style='font-family: Aptos, Arial, sans-serif; margin-bottom: 10px; margin-top: 10px; padding: 10px; background-color: #fff8f8; border-left: 4px solid #ffcccc; display: inline-table; width: auto;'>";
                                            //        dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; color: #d9534f;'><strong>Reason for Rejection:</strong> " + sReasonRejected + "</p>";
                                            //        dataMail.mail_body += "    </div>";
                                            //    }
                                            //}

                                            //if (tempTravel != null && tempTravel?.Count > 0)
                                            //{
                                            //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                            //    dataMail.mail_body += "        <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                            //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif;'>";
                                            //    dataMail.mail_body += "            <thead>";
                                            //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                            //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                            //    dataMail.mail_body += "                </tr>";
                                            //    dataMail.mail_body += "            </thead>";
                                            //    dataMail.mail_body += "            <tbody>";

                                            //    var iNo = 1;
                                            //    foreach (var item in tempTravel)
                                            //    {
                                            //        dataMail.mail_body += "            <tr>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                            //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                            //        dataMail.mail_body += "            </tr>";
                                            //        iNo++;
                                            //    }

                                            //    dataMail.mail_body += "            </tbody>";
                                            //    dataMail.mail_body += "        </table>";
                                            //    dataMail.mail_body += "    </div>";
                                            //}

                                            //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                            //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                            //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                            //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                            //dataMail.mail_body += "    </div>";
                                            //dataMail.mail_body += "</div>";

                                            //#endregion


                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                            dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                            dataMail.doc_id = doc_id;

                                            var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                            if (!string.IsNullOrEmpty(attachments_doc))
                                            {
                                                dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? "|" : "") + attachments_doc;
                                            }

                                            data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                        }
                                        catch (Exception ex)
                                        {
                                            write_log_mail("88-email.message-submitFlow3", "error-reject" + ex.ToString());
                                        }
                                        #endregion
                                    }

                                    else if (value.action.type == "3")
                                    {
                                        sql = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2, b.employeeid as name3, b.orgname as name4 from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_travel_empid = b.employeeid left join (select min(dte_id) as id, dh_code, dte_emp_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id where a.dta_type = 2 and a.dta_action_status in (4) and a.dta_doc_status in (23, 41) and a.dh_code = :doc_id";
                                        if (user_role != "1") sql += " and a.dta_appr_empid = :user_id_def";
                                        sql += " order by s.id";
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        if (user_role != "1") parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                        if (tempTravel != null) traveler_mail = string.Join(";", tempTravel.Select(item => item.name2));

                                        //sql = @"select distinct b.employeeid as user_id, '' as user_name, b.email from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_appr_empid = b.employeeid where a.dta_type = 2 and a.dta_action_status in (4) and a.dta_doc_status = 23 and a.dh_code = :doc_id";
                                        //if (user_role != "1") sql += " and a.dta_appr_empid = :user_id_def";
                                        //parameters = new List<OracleParameter>();
                                        //parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        //if (user_role != "1") parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        //var approverLineMailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                        //if (approverLineMailList != null && approverLineMailList.Count > 0) line_approver_mail = string.Join(";", approverLineMailList.Select(m => m.email));

                                        sql = @"
select distinct b.employeeid as user_id, '' as user_name, b.email
from bz_doc_traveler_approver a
inner join vw_bz_users b on a.dta_appr_empid = b.employeeid
where a.dta_type = 1
and a.dta_action_status in (2,4)
and a.dta_doc_status in (23,41)
and (a.dta_travel_empid, a.dh_code) in (
    select distinct a1.dta_travel_empid, a1.dh_code
    from bz_doc_traveler_approver a1
    where a1.dta_type = 2
    and a1.dta_action_status = 4
    and a1.dta_doc_status = 23
";

                                        if (user_role != "1")
                                        {
                                            sql += " and a1.dta_appr_empid = :user_id_def";
                                        }

                                        sql += @"
)
and a.dh_code = :doc_id
";

                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                        if (user_role != "1") parameters.Add(context.ConvertTypeParameter("user_id_def", user_id_def, "char"));
                                        var approverCAPMailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                        if (approverCAPMailList != null && approverCAPMailList.Count > 0) cap_approver_mail = string.Join(";", approverCAPMailList.Select(m => m.email));



                                        sendEmailModel dataMail = new sendEmailModel();
                                        dataMail.mail_to = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail);
                                        dataMail.mail_cc = (cap_approver_mail + line_approver_mail) + (requester_mail + on_behalf_of_mail) + traveler_mail;

                                        var resMailShowCase = $"To: {super_admin_mail?.ToLower()} (Super Admin)";
                                        resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                        //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                        resMailShowCase += $"<br>Cc: {cap_approver_mail?.ToLower()} (CAP Approver) ";
                                        resMailShowCase += $", {line_approver_mail?.ToLower()} (Line Approver)";
                                        resMailShowCase += $", {requester_mail?.ToLower()} (Requester)";
                                        resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                        resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                        dataMail.mail_show_case = resMailShowCase;

                                        dataMail.mail_subject = value.doc_id + " : Please revise your request for business travel";
                                        sDear = "Dear All,";
                                        sDetail = "Your business travel request has been revise by " + user_name + ". To view the details, click ";
                                        sDetail += "<a href='" + (LinkLogin + "i").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";

                                        #region Build Email Body
                                        dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                        if (value.action.type == "3")
                                        {
                                            sReasonRejected = value.action.remark;

                                            if (!string.IsNullOrEmpty(sReasonRejected))
                                            {
                                                dataMail.mail_body += $@"
            <table width='60%' cellpadding='10' cellspacing='0' border='0' style='background-color: #fff8f8; border-left: 4px solid #ffcccc; margin-bottom: 10px;'>
                <tr><td style='font-size: 14px; color: #d9534f;'><strong>Reason for Revision:</strong> {sReasonRejected}</td>
                    </tr>
                </table>
            </td>
        </tr>";
                                            }
                                        }

                                        if (tempTravel != null && tempTravel?.Count > 0)
                                        {
                                            dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                            var iNo = 1;
                                            foreach (var item in tempTravel)
                                            {
                                                dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                    </tr>";
                                                iNo++;
                                            }

                                            dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                        }

                                        dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                        #endregion
                                        //#region Build Email Body
                                        //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                        //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                        //dataMail.mail_body += $"       <p style='margin: 0 0 10px 0;'>{sDear}</p>";
                                        //dataMail.mail_body += "    </div>";
                                        //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                        //dataMail.mail_body += $"       <p style='margin: 0 0 15px 0;'>{sDetail}</p>";
                                        //dataMail.mail_body += "    </div>";
                                        //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                        //dataMail.mail_body += $"       <p style='margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                        //dataMail.mail_body += $"       <p style='margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                        //dataMail.mail_body += $"       <p style='margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                        //dataMail.mail_body += "    </div>";

                                        //if (value.action.type == "3")
                                        //{
                                        //    sReasonRejected = value.action.remark;
                                        //    if (!string.IsNullOrEmpty(sReasonRejected))
                                        //    {
                                        //        dataMail.mail_body += "    <div style='font-family: Aptos, Arial, sans-serif; margin-bottom: 10px; margin-top: 10px; padding: 10px; background-color: #fff8f8; border-left: 4px solid #ffcccc; display: inline-table; width: auto;'>";
                                        //        dataMail.mail_body += "        <p style='margin: 0; color: #d9534f;'><strong>Reason for Revision:</strong> " + sReasonRejected + "</p>";
                                        //        dataMail.mail_body += "    </div>";
                                        //    }
                                        //}

                                        //if (tempTravel != null && tempTravel?.Count > 0)
                                        //{
                                        //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                        //    dataMail.mail_body += "        <p style='margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                        //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-family: Aptos, Arial, sans-serif; font-size: 14px;'>";
                                        //    dataMail.mail_body += "            <thead>";
                                        //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                        //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                        //    dataMail.mail_body += "                </tr>";
                                        //    dataMail.mail_body += "            </thead>";
                                        //    dataMail.mail_body += "            <tbody>";

                                        //    var iNo = 1;
                                        //    foreach (var item in tempTravel)
                                        //    {
                                        //        dataMail.mail_body += "            <tr>";
                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                        //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                        //        dataMail.mail_body += "            </tr>";
                                        //        iNo++;
                                        //    }

                                        //    dataMail.mail_body += "            </tbody>";
                                        //    dataMail.mail_body += "        </table>";
                                        //    dataMail.mail_body += "    </div>";
                                        //}

                                        //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                        //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                        //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                        //dataMail.mail_body += "    </div>";
                                        //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                        //dataMail.mail_body += "        <p style='margin: 0;'>Best Regards,</p>";
                                        //dataMail.mail_body += "        <p style='margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                        //dataMail.mail_body += "    </div>";
                                        //dataMail.mail_body += "</div>";
                                        //#endregion


                                        var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                        if (!string.IsNullOrEmpty(attachments_doc)) dataMail.mail_attachments = attachments_doc;
                                        var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                        dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                        dataMail.doc_id = doc_id;
                                        data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                    }
                                    else if (value.action.type == "4" || value.action.type == "5")
                                    {

                                        // แยกกลุ่มผู้ถูกปฏิเสธและอนุมัติ
                                        var rejectedTravelers = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "false")
                                            .ToList();

                                        var approvedTravelers = value.traveler_summary
                                            .Where(t => t.take_action == "true" && t.appr_status == "true")
                                            .ToList();

                                        // ส่งอีเมลสำหรับผู้ถูกปฏิเสธ
                                        if (rejectedTravelers.Any())
                                        {
                                            #region traveler mail in doc (Rejected)
                                            sql = @"SELECT DISTINCT to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name,
            b.employeeid as emp_id, b.email as email, b.orgname as position   
            FROM bz_doc_traveler_approver a
            INNER JOIN vw_bz_users b ON a.dta_travel_empid = b.employeeid
            LEFT JOIN (SELECT min(dte_id) as id, dh_code, dte_emp_id FROM BZ_DOC_TRAVELER_EXPENSE GROUP BY dh_code, dte_emp_id) s 
            ON a.dh_code = s.dh_code AND a.dta_travel_empid = s.dte_emp_id
            WHERE a.dta_type = 2 AND a.dta_action_status IN (5) AND a.dta_doc_status = 40
            AND a.dh_code = :doc_id 
            AND a.dta_travel_empid IN (" + string.Join(",", rejectedTravelers.Select(t => $"'{t.traverler_id}'")) + ") ORDER BY s.id";

                                            traveler_mail = "";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var tempTravel = context.Database.SqlQueryRaw<tempEmployeeProfileModel>(sql, parameters.ToArray()).ToList();
                                            if (tempTravel != null)
                                            {
                                                foreach (var item in tempTravel)
                                                {
                                                    traveler_mail += item.email + ";";
                                                }
                                            }
                                            #endregion

                                            #region approver mail in doc (Rejected)
                                            sql = @"SELECT DISTINCT b.employeeid as user_id, '' as user_name, b.email                       
            FROM bz_doc_traveler_approver a
            INNER JOIN vw_bz_users b ON a.dta_appr_empid = b.employeeid 
            WHERE a.dta_type = 2 AND a.dta_action_status IN (5) AND a.dta_doc_status = 40
            AND a.dh_code = :doc_id";

                                            line_approver_mail = "";
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                            var approvermail = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                            if (approvermail != null && approvermail.Count > 0)
                                            {
                                                for (int m = 0; m < approvermail.Count; m++)
                                                {
                                                    line_approver_mail += approvermail[m].email + ";";
                                                }
                                            }
                                            #endregion

                                            #region "#### SEND REJECTION MAIL ####" 
                                            try
                                            {
                                                sendEmailModel dataMail = new sendEmailModel();

                                                var doc_head_create_by = doc_head_search.DH_CREATE_BY ?? "";
                                                sql = "SELECT b.employeeid as user_id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME user_name, EMAIL email ";
                                                sql += "FROM vw_bz_users b WHERE employeeid= :doc_head_create_by";

                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_head_create_by", doc_head_create_by, "char"));
                                                var requestor = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                                dataMail.mail_to = requester_mail + traveler_mail;
                                                dataMail.mail_cc = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) + line_approver_mail + on_behalf_of_mail;

                                                var resMailShowCase = $"To: {requester_mail?.ToLower()} (Requester)";
                                                resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                                resMailShowCase += $"<br>Cc: {super_admin_mail?.ToLower()} (Super Admin)";
                                                resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                resMailShowCase += $", {line_approver_mail?.ToLower()} (Line Approval)";
                                                resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                dataMail.mail_show_case = resMailShowCase;

                                                dataMail.mail_subject = $"{doc_id}: The request for business travel has been rejected";

                                                sDear = "Dear All,";
                                                sDetail = $"Your business travel request has been rejected by {user_name}. To view the details, click ";
                                                sDetail += $"<a href='{(LinkLogin + "ii").Replace("###", doc_id)}'>{doc_id}</a>";

                                                //#region Build Email Body
                                                //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                                //dataMail.mail_body += "    </div>";

                                                //if (rejectedTravelers.Any())
                                                //{
                                                //    sReasonRejected = value.traveler_summary
                                                //        .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                                //        .Select(x => x.appr_remark)
                                                //        .FirstOrDefault();

                                                //    if (!string.IsNullOrEmpty(sReasonRejected))
                                                //    {
                                                //        dataMail.mail_body += "    <div style='font-family: Aptos, Arial, sans-serif; margin-bottom: 10px; margin-top: 10px; padding: 10px; background-color: #fff8f8; border-left: 4px solid #ffcccc;  width: 50%;'>";
                                                //        dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; color: #d9534f;'><strong>Reason for Rejection:</strong> " + sReasonRejected + "</p>";
                                                //        dataMail.mail_body += "    </div>";
                                                //    }
                                                //}

                                                //if (tempTravel != null && tempTravel?.Count > 0)
                                                //{
                                                //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //    dataMail.mail_body += "        <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                                //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif;'>";
                                                //    dataMail.mail_body += "            <thead>";
                                                //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                //    dataMail.mail_body += "                </tr>";
                                                //    dataMail.mail_body += "            </thead>";
                                                //    dataMail.mail_body += "            <tbody>";

                                                //    var iNo = 1;
                                                //    foreach (var item in tempTravel)
                                                //    {
                                                //        dataMail.mail_body += "            <tr>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.emp_id}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.position}</td>";
                                                //        dataMail.mail_body += "            </tr>";
                                                //        iNo++;
                                                //    }

                                                //    dataMail.mail_body += "            </tbody>";
                                                //    dataMail.mail_body += "        </table>";
                                                //    dataMail.mail_body += "    </div>";
                                                //}

                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                                //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "</div>";

                                                //#endregion


                                                #region Build Email Body
                                                dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";

                                                if (rejectedTravelers.Any())
                                                {
                                                    sReasonRejected = value.traveler_summary
                                                        .Where(x => x.appr_status == "false" && !string.IsNullOrEmpty(x.appr_remark))
                                                        .Select(x => x.appr_remark)
                                                        .FirstOrDefault();

                                                    if (!string.IsNullOrEmpty(sReasonRejected))
                                                    {
                                                        dataMail.mail_body += $@"
            <table width='60%' cellpadding='10' cellspacing='0' border='0' style='background-color: #fff8f8; border-left: 4px solid #ffcccc; margin-bottom: 10px;'>
                <tr><td style='font-size: 14px; color: #d9534f;'><strong>Reason for Rejection:</strong> {sReasonRejected}</td>
                    </tr>
                </table>
            </td>
        </tr>";
                                                    }
                                                }

                                                if (tempTravel != null && tempTravel?.Count > 0)
                                                {
                                                    dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                    var iNo = 1;
                                                    foreach (var item in tempTravel)
                                                    {
                                                        dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.emp_id}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.position}</td>
                    </tr>";
                                                        iNo++;
                                                    }

                                                    dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                                }

                                                dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                                #endregion

                                                data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                            }
                                            catch (Exception ex)
                                            {
                                                write_log_mail("88-email.message-submitFlow3", "error-reject" + ex.ToString());
                                            }
                                            #endregion
                                        }

                                        // ส่งอีเมลสำหรับผู้ถูกอนุมัติ
                                        if (approvedTravelers.Any())
                                        {
                                            // หาผู้อนุมัติ CAP ลำดับถัดไป (เฉพาะกรณีอนุมัติ)
                                            if (apprAllStatus)
                                            {
                                                Set_Trip_Complated(context, value.token_login, value.doc_id);

                                                string traveler_reject_mail = "";
                                                sql = @"select distinct '' as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2, '' as name3, '' as name4 from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_travel_empid = b.employeeid where a.dta_type = 2 and a.dta_action_status in (5) and a.dta_doc_status = 42 and a.dh_code = :doc_id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var traveler_reject = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (traveler_reject != null) traveler_reject_mail = string.Join(";", traveler_reject.Select(item => item.name2));

                                                sql = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2, b.employeeid as name3, b.orgname as name4 
                                                from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_travel_empid = b.employeeid 
                                                left join (select min(dte_id) as id, dh_code, dte_emp_id 
                                                from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s on a.dh_code = s.dh_code and a.dta_travel_empid = s.dte_emp_id
                                                where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42 and a.dh_code = :doc_id and not exists ( 
                                                select 1 from bz_doc_traveler_approver sub where sub.dh_code = a.dh_code and sub.dta_travel_empid = a.dta_travel_empid and sub.dta_action_status = 5)  order by s.id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (tempTravel != null) traveler_mail = string.Join(";", tempTravel.Select(item => item.name2));


                                                sql = @"select distinct b.employeeid as user_id, b.employeeid as user_name, b.email from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_appr_empid = b.employeeid where a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42 and a.dh_code = :doc_id";
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                var approverCAPMailList = context.SearchUserNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                if (approverCAPMailList != null && approverCAPMailList.Count > 0) cap_approver_mail = string.Join(";", approverCAPMailList.Select(m => m.email));



                                                sendEmailModel dataMail = new sendEmailModel();
                                                dataMail.mail_to = (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) + traveler_mail + (line_approver_mail + cap_approver_mail) + (requester_mail + on_behalf_of_mail) + traveler_reject_mail;

                                                var resMailShowCase = $"To: {super_admin_mail?.ToLower()} (Super Admin)";
                                                resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                                resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                                resMailShowCase += $", {line_approver_mail?.ToLower()} (Line Approver)";
                                                resMailShowCase += $", {cap_approver_mail?.ToLower()} (CAP Approver) ";
                                                resMailShowCase += $", {requester_mail?.ToLower()} (Requester)";
                                                resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                resMailShowCase += $", {traveler_reject_mail?.ToLower()} (Traveller Reject)";
                                                dataMail.mail_show_case = resMailShowCase;

                                                dataMail.mail_subject = value.doc_id + " : The request for business travel has been approved";
                                                sDear = "Dear All,";
                                                sDetail = "The request for business travel has been approved. To view the approval details, click ";
                                                sDetail += "<a href='" + LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id) + "'>" + value.doc_id + "</a>";
                                                sDetail += "<br>Any additional arrangements require to complete by the traveler. To view travel details, click ";
                                                sDetail += "<a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id) + "'>travel details.</a>";

                                                #region Build Email Body
                                                dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";
                                                if (tempTravel != null && tempTravel?.Count > 0)
                                                {
                                                    dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                    var iNo = 1;
                                                    foreach (var item in tempTravel)
                                                    {
                                                        dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>
                    </tr>";
                                                        iNo++;
                                                    }

                                                    dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                                }

                                                dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";

                                                #endregion
                                                //#region Build Email Body
                                                //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                                //dataMail.mail_body += "    </div>";

                                                //if (tempTravel != null && tempTravel?.Count > 0)
                                                //{
                                                //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                //    dataMail.mail_body += "        <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                                //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse; font-size: 14px;'>";
                                                //    dataMail.mail_body += "            <thead>";
                                                //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                //    dataMail.mail_body += "                </tr>";
                                                //    dataMail.mail_body += "            </thead>";
                                                //    dataMail.mail_body += "            <tbody>";

                                                //    var iNo = 1;
                                                //    foreach (var item in tempTravel)
                                                //    {
                                                //        dataMail.mail_body += "            <tr>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name1}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name3}</td>";
                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{item.name4}</td>";
                                                //        dataMail.mail_body += "            </tr>";
                                                //        iNo++;
                                                //    }

                                                //    dataMail.mail_body += "            </tbody>";
                                                //    dataMail.mail_body += "        </table>";
                                                //    dataMail.mail_body += "    </div>";
                                                //}

                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                                //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                //dataMail.mail_body += "    </div>";
                                                //dataMail.mail_body += "</div>";
                                                //#endregion
                                                try
                                                {
                                                    var file_attachments = file_attached_approval_form(value.doc_id, value.token_login);
                                                    if (!string.IsNullOrEmpty(file_attachments)) dataMail.mail_attachments = file_attachments;
                                                }
                                                catch { }

                                                var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                                if (!string.IsNullOrEmpty(attachments_doc)) dataMail.mail_attachments = (!string.IsNullOrEmpty(dataMail.mail_attachments) ? dataMail.mail_attachments + "|" : "") + attachments_doc;

                                                data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);

                                                if (notiPassport)
                                                {
                                                    foreach (var item in tempTravel)
                                                    {
                                                        sql = @"select distinct emp_id as id, '' as name1, '' as name2, '' as name3, '' as name4 from bz_data_passport where default_type = 'true' and to_date(passport_date_expire,'dd Mon yyyy') >= sysdate and to_date(passport_date_expire, 'dd Mon yyyy') <= sysdate + 180 and emp_id = :item_traveler_id";
                                                        parameters = new List<OracleParameter>();
                                                        parameters.Add(context.ConvertTypeParameter("item_traveler_id", item.name3, "char"));
                                                        var dataPassport = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                        if (dataPassport == null || dataPassport.Count == 0)
                                                        {
                                                            sendEmailModel passportMail = new sendEmailModel();
                                                            passportMail.mail_to = item.name2;
                                                            passportMail.mail_cc = (super_admin_mail + pmsv_admin_mail);
                                                            passportMail.mail_subject = value.doc_id + " : Please update Passport information - " + item.name1;
                                                            var MailShowCase = $"To: {item.name2.ToLower()} (Traveller)";
                                                            MailShowCase += $"<br>Cc: {super_admin_mail?.ToLower()} (Super Admin)";
                                                            MailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                            passportMail.mail_show_case = resMailShowCase;
                                                            passportMail.mail_body = @"
<table cellpadding='0' cellspacing='0' width='100%' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; max-width: 800px;'>
    <tr>
        <td style='padding: 20px 0 0 0;'>
            <p style='margin: 0;'>Dear " + item.name1 + @",</p>
        </td>
    </tr>

    <tr>
        <td style='padding: 10px 0 10px 0;'>
            <p style='margin: 0 0 10px 0; '>
                You are required to update your passport information in order to proceed with travel arrangements.<br/>
                To view travel details, click 
                <a href='" + LinkLoginTravelerhistory.Replace("###", value.doc_id).Replace("travelerhistory", "passport") + @"' style='color: #1a0dab; text-decoration: underline;'>" + value.doc_id + @"</a>.
            </p>
        </td>
    </tr>

    <tr>
        <td style='padding: 10px 0 0 0;'>
            <table width='60%' cellpadding='3' cellspacing='0' style='background-color: #F3DDFF; font-family: Aptos, Arial, sans-serif; font-size: 14px; '>
                <tr><td><strong>Title:</strong> " + sTitle + @"</td></tr>
                <tr><td><strong>Business Date:</strong> " + sBusinessDate + @"</td></tr>
                <tr><td><strong>Location:</strong> " + sLocation + @"</td></tr>
            </table>
        </td>
    </tr>

    <tr>
        <td style='padding: 20px 0 0 0;'>
            <p style='margin: 0 0 5px 0; '>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + @").</p>
            <p style='margin: 0; '>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + @").</p>
        </td>
    </tr>

    <tr>
        <td style='padding: 15px 0 0 0;'>
            <p style='margin: 0; '>Best Regards,</p>
            <p style='margin: 0; font-weight: bold; '>Business Travel Services Team (PMSV)</p>
        </td>
    </tr>
</table>";


                                                            var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                            passportMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                            passportMail.doc_id = doc_id;
                                                            SendEmailServiceTravelerProfile.SendMailFlowTrip(passportMail);
                                                        }
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                // Find the next approver for each traveler based on the current approver's level
                                                var tempEmpForAction_def = new List<BZ_DOC_ACTION>();
                                                foreach (var item in value.traveler_summary)
                                                {
                                                    if (string.IsNullOrEmpty(item.appr_id)) continue;
                                                    if (!(item.appr_id == user_id)) continue;

                                                    var row_check_3Emp = value.traveler_summary.Where(p =>
                                                        p.appr_id == item.appr_id
                                                        && p.traverler_id == item.traverler_id && p.appr_status != "false").ToList();

                                                    if (row_check_3Emp != null && row_check_3Emp.Count > 0)
                                                    {
                                                        var appr_id = item.appr_id.ToString();
                                                        var traveler_id = item.traverler_id.ToString();

                                                        // Check if the approver has already been processed
                                                        var check_def = tempEmpForAction_def.Where(p => p.EMP_ID.Equals(appr_id));
                                                        if (check_def != null && check_def.Count() > 0) continue;

                                                        tempEmpForAction_def.Add(new BZ_DOC_ACTION
                                                        {
                                                            EMP_ID = appr_id
                                                        });

                                                        // STEP 1: ดึงข้อมูล Approvers ทั้งหมด
                                                        var allApprovers = context.BZ_DOC_TRAVELER_APPROVER
                                                            .AsNoTracking()
                                                            .Where(x => x.DTA_TYPE == "2" && x.DH_CODE == doc_id)
                                                            .ToList();

                                                        // STEP 2: Find the current approver level for the traveler
                                                        var list_appr_level_now = context.TempEmpIdModelList.FromSqlRaw(
                                                            @"SELECT NVL((dta_appr_level), 0) AS emp_id
                                    FROM BZ_DOC_TRAVELER_APPROVER
                                    WHERE dta_type = 2
                                    AND DH_CODE = :id_doc
                                    AND DTA_ACTION_STATUS = '2'
                                    AND DTA_TRAVEL_EMPID = :dta_travel_empid",
                                                            context.ConvertTypeParameter("dta_travel_empid", traveler_id, "char"),
                                                            context.ConvertTypeParameter("id_doc", doc_id, "char")).ToList();

                                                        if (list_appr_level_now.Count() > 0)
                                                        {
                                                            var dta_appr_level_now = list_appr_level_now[0].emp_id;

                                                            // Find the next approver based on the current level
                                                            var list_appr_level_next = context.TempEmpIdModelList.FromSqlRaw(
                                                                 @"SELECT dta_appr_empid AS emp_id
                                        FROM BZ_DOC_TRAVELER_APPROVER
                                        WHERE dta_type = 2
                                        AND DH_CODE = :id_doc
                                        AND DTA_ACTION_STATUS = 2
                                        AND DTA_TRAVEL_EMPID = :dta_travel_empid
                                        AND dta_appr_level = :dta_appr_level", // Find the next level
                                                                 context.ConvertTypeParameter("dta_travel_empid", traveler_id, "char"),
                                                                 context.ConvertTypeParameter("id_doc", doc_id, "char"),
                                                                 context.ConvertTypeParameter("dta_appr_level", dta_appr_level_now, "char")).ToList();

                                                            if (list_appr_level_next.Count() > 0)
                                                            {
                                                                var appr_id_next = list_appr_level_next[0].emp_id ?? "";

                                                                if (!string.IsNullOrEmpty(appr_id_next))
                                                                {

                                                                    // STEP 3: ตรวจสอบประเภท Approver
                                                                    var approverDetails = allApprovers
                                                                        .Where(x => x.DTA_APPR_EMPID == appr_id_next)
                                                                        .ToList();

                                                                    bool isMultiLevel = approverDetails.Select(x => x.DTA_APPR_LEVEL).Distinct().Count() > 1;
                                                                    var travelersForApprover = approverDetails.Select(x => x.DTA_TRAVEL_EMPID).Distinct().ToList();
                                                                    bool isSingleTraveler = travelersForApprover.Count == 1;

                                                                    // STEP 4: ตรวจสอบเงื่อนไขการอนุมัติ
                                                                    bool shouldSendEmail = true;
                                                                    var pendingReasons = new List<string>();

                                                                    if (isMultiLevel)
                                                                    {
                                                                        // กรณี Multi-Level Approver
                                                                        foreach (var level in approverDetails.Select(x => x.DTA_APPR_LEVEL).Distinct().OrderBy(x => x))
                                                                        {
                                                                            //var travelers = approverDetails
                                                                            //    .Where(x => x.DTA_APPR_LEVEL == 1)
                                                                            //    .Select(x => x.DTA_TRAVEL_EMPID)
                                                                            //    .Distinct();


                                                                            // หาผู้อนุมัติระดับก่อนหน้าทั้งหมด (ที่ไม่ใช่ผู้อนุมัติปัจจุบัน)
                                                                            var prevLevelApprovers = allApprovers
                                                                                .Where(x =>
                                                                                           x.DTA_APPR_LEVEL == decimal.Parse(dta_appr_level_now) - 1 &&
                                                                                           x.DTA_APPR_EMPID != appr_id_next)
                                                                                .ToList();

                                                                            // ตรวจสอบว่าผู้อนุมัติระดับก่อนหน้าทุกคน (ที่ไม่ใช่ตัวเอง) อนุมัติแล้ว
                                                                            bool allApproved = prevLevelApprovers.Count > 0 &&
                                                                                             prevLevelApprovers.All(x => x.DTA_DOC_STATUS != 31);

                                                                            if (!allApproved)
                                                                            {
                                                                                shouldSendEmail = false;

                                                                            }

                                                                        }
                                                                    }
                                                                    else if (isSingleTraveler)
                                                                    {
                                                                        // กรณี Single-Level Approver และ Single Traveler
                                                                        var tid = travelersForApprover.First();
                                                                        var approverLevel = approverDetails.First().DTA_APPR_LEVEL;

                                                                        var prevLevelApprover = allApprovers
                                                                            .FirstOrDefault(x => x.DTA_TRAVEL_EMPID == tid &&
                                                                                               x.DTA_APPR_LEVEL == approverLevel - 1);

                                                                        if (prevLevelApprover == null || prevLevelApprover.DTA_DOC_STATUS == 31)
                                                                        {
                                                                            shouldSendEmail = false;
                                                                            //pendingReasons.Add($"Traveler {tid} ยังไม่อนุมัติระดับ {approverLevel - 1}");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // กรณี Single-Level Approver แต่มีหลาย Traveler
                                                                        foreach (var tid in travelersForApprover)
                                                                        {
                                                                            var approverLevel = approverDetails
                                                                                .First(x => x.DTA_TRAVEL_EMPID == tid).DTA_APPR_LEVEL;

                                                                            var prevLevelApprover = allApprovers
                                                                                .FirstOrDefault(x => x.DTA_TRAVEL_EMPID == tid &&
                                                                                                   x.DTA_APPR_LEVEL == approverLevel - 1);

                                                                            if (prevLevelApprover == null || prevLevelApprover.DTA_DOC_STATUS == 31)
                                                                            {
                                                                                shouldSendEmail = false;
                                                                                //pendingReasons.Add($"Traveler {tid} ยังไม่อนุมัติระดับ {approverLevel - 1}");
                                                                            }
                                                                        }
                                                                    }
                                                                    // ==============================================
                                                                    // STEP 5: ดำเนินการส่งอีเมล
                                                                    // ==============================================
                                                                    if (shouldSendEmail)
                                                                    {
                                                                        if (!string.IsNullOrEmpty(appr_id_next))
                                                                        {
                                                                            // Get the next approver's information
                                                                            var approverInfo = context.VW_BZ_USERS
                                                                                .Where(u => u.EMPLOYEEID == appr_id_next)
                                                                                .Select(u => new
                                                                                {
                                                                                    u.EMPLOYEEID,
                                                                                    UserName = u.ENTITLE + " " + u.ENFIRSTNAME + " " + u.ENLASTNAME,
                                                                                    u.EMAIL
                                                                                })
                                                                                .FirstOrDefault();

                                                                            if (approverInfo != null)
                                                                            {

                                                                                cap_approver_mail = approverInfo.EMAIL;

                                                                                //Find Traveler Emails
                                                                                sql = @"select distinct to_char(s.id) as id, nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, b.email as name2, b.employeeid as name3, b.orgname as name4 from bz_doc_traveler_approver a inner join vw_bz_users b on a.dta_travel_empid = b.employeeid left join (select min(dte_id) as id, dh_code, dte_emp_id from BZ_DOC_TRAVELER_EXPENSE group by dh_code, dte_emp_id ) s on a.dh_code =s.dh_code and a.dta_travel_empid = s.dte_emp_id where a.dh_code = :doc_id and a.dta_type = 2 and a.dta_appr_empid = :appr_id and a.dta_action_status = 2 order by s.id";
                                                                                parameters = new List<OracleParameter>();
                                                                                parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                                                                                parameters.Add(context.ConvertTypeParameter("appr_id", appr_id_next, "char"));
                                                                                var tempTravel = context.TempModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                                                                traveler_mail = tempTravel != null ? string.Join(";", tempTravel.Select(t => t.name2)) : "";

                                                                                //dataMail.mail_cc += traveler_mail;



                                                                                traveler_mail = "";
                                                                                if (tempTravel != null)
                                                                                {
                                                                                    foreach (var itemp in tempTravel)
                                                                                    {
                                                                                        traveler_mail += itemp.name2 + ";";
                                                                                    }
                                                                                }

                                                                                sendEmailModel dataMail = new sendEmailModel();
                                                                                //to : CAP Approval
                                                                                //cc : Line Approval, Super admin, Requester, Traveller
                                                                                dataMail.mail_to = cap_approver_mail;
                                                                                dataMail.mail_cc = line_approver_mail + (super_admin_mail + pmsv_admin_mail + pmdv_admin_mail) +
                                                                                                  (requester_mail + on_behalf_of_mail) + traveler_mail;

                                                                                var resMailShowCase = $"To: {cap_approver_mail?.ToLower()} (CAP Approver)";
                                                                                resMailShowCase += $"<br>Cc: {line_approver_mail?.ToLower()} (Line Approver)";
                                                                                resMailShowCase += $", {super_admin_mail?.ToLower()} (Super Admin)";
                                                                                resMailShowCase += $", {pmsv_admin_mail?.ToLower()} (PMSV Admin)";
                                                                                //resMailShowCase += $", {pmdv_admin_mail?.ToLower()} (PMDV Admin)";
                                                                                resMailShowCase += $", {requester_mail?.ToLower()} (Requester)";
                                                                                resMailShowCase += $", {on_behalf_of_mail?.ToLower()} (On Behalf Of)";
                                                                                resMailShowCase += $", {traveler_mail?.ToLower()} (Traveller)";
                                                                                dataMail.mail_show_case = resMailShowCase;

                                                                                // Use different subject and message if this is a combined approver

                                                                                dataMail.mail_subject = $"{value.doc_id}: Please approve business travel request as CAP";
                                                                                sDear = $"Dear {approverInfo.UserName},";
                                                                                sDetail = "Please approve business travel request as CAP. To view the details, click ";

                                                                                sDetail += $"<a href='{LinkLogin.Replace("/i", "/cap").Replace("###", value.doc_id)}'>{value.doc_id}</a>";

                                                                                #region Build Email Body
                                                                                dataMail.mail_body = $@"<table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-family: Aptos, Arial, sans-serif; font-size: 14px; color: #333333; line-height: 1.6;'>
<tr><td align='left'>
    <table width='800' cellpadding='0' cellspacing='0' border='0' style='width:800px;'>
        <tr><td style='padding: 20px 0;'>

             <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDear}</td></tr>
                <tr><td style='padding-bottom: 10px; font-size: 14px;'>{sDetail}</td></tr>
            </table>

<table width='60%' cellpadding='6' cellspacing='0' border='0' style='background-color: #F3DDFF; padding-bottom: 10px;'>
    <tr><td style='font-size: 14px;'><strong>Title:</strong> {sTitle}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Business Date:</strong> {sBusinessDate}</td></tr>
    <tr><td style='font-size: 14px;'><strong>Location:</strong> {sLocation}</td></tr>
</table>";
                                                                                if (tempTravel != null && tempTravel?.Count > 0)
                                                                                {
                                                                                    dataMail.mail_body += @"
    <tr>
        <td style='padding: 0 0 20px 0;'>
            <p style='font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>
            <table style='width: auto; border-collapse: collapse; font-size: 14px; font-family: Aptos, Arial, sans-serif; border: 1px solid #ccc;' cellpadding='0' cellspacing='0'>
                <thead>
                    <tr style='background-color: #A7D0F0;'>
                        
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>
                            <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>
                            <th style='padding: 8px; text-align: center; border: 1px solid #ccc;'>Organization</th>

                    </tr>
                </thead>
                <tbody>";

                                                                                    var iNo = 1;
                                                                                    foreach (var items in tempTravel)
                                                                                    {
                                                                                        dataMail.mail_body += $@"
                    <tr>
                        <td style='padding: 8px; text-align: center; border: 1px solid #ccc;'>{iNo}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{items.name1}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{items.name3}</td>
                        <td style='padding: 8px; border: 1px solid #ccc;'>{items.name4}</td>
                    </tr>";
                                                                                        iNo++;
                                                                                    }

                                                                                    dataMail.mail_body += @"
                </tbody>
            </table>
        </td>
    </tr>";
                                                                                }

                                                                                dataMail.mail_body += $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='font-size: 14px; margin-bottom: 20px;'>
                <tr><td style='padding-bottom: 8px;'>If you have any questions, please contact Business Travel Services Team (Tel. {Tel_Services_Team}).</td></tr>
                <tr><td>For application assistance, please contact PTT Digital Call Center (Tel. {Tel_Call_Center}).</td></tr>
            </table>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr><td style='border-top: 1px solid #eeeeee; padding-top: 15px; font-size: 14px;'>Best Regards,</td></tr>
                <tr><td style='font-weight: bold; font-size: 14px;'>Business Travel Services Team (PMSV)</td></tr>
            </table>

        </td></tr>
    </table>
</td></tr>
</table>";
                                                                                #endregion

                                                                                //#region Build Email Body
                                                                                //dataMail.mail_body = @"<div style='font-family: Aptos, Arial, sans-serif; color: #333333; line-height: 1.6; max-width: 800px; margin: 0 auto;'>";
                                                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 10px 0;'>{sDear}</p>";
                                                                                //dataMail.mail_body += "    </div>";
                                                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                                                //dataMail.mail_body += $"       <p style='font-size: 15px; margin: 0 0 15px 0;'>{sDetail}</p>";
                                                                                //dataMail.mail_body += "    </div>";
                                                                                //dataMail.mail_body += "    <div style=' width: 50%; background-color: #F3DDFF; padding: 15px; border-radius: 4px; margin-bottom: 20px; box-sizing: border-box;'>";
                                                                                //dataMail.mail_body += "        <p style='font-size: 15px; margin: 0 0 8px 0; font-weight: bold;'>Request Details:</p>";
                                                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Title:</strong> {sTitle}</p>";
                                                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Business Date:</strong> {sBusinessDate}</p>";
                                                                                //dataMail.mail_body += $"       <p style='font-size: 14px; margin: 0 0 5px 0;'><strong>Location:</strong> {sLocation}</p>";
                                                                                //dataMail.mail_body += "    </div>";

                                                                                //if (tempTravel != null && tempTravel?.Count > 0)
                                                                                //{
                                                                                //    dataMail.mail_body += "    <div style='margin-bottom: 20px;'>";
                                                                                //    dataMail.mail_body += "        <p style='font-size: 15px; margin: 0 0 10px 0; font-weight: bold;'>Traveler List:</p>";
                                                                                //    dataMail.mail_body += "        <table style='width: auto ; border-collapse: collapse;'>";
                                                                                //    dataMail.mail_body += "            <thead>";
                                                                                //    dataMail.mail_body += "                <tr style='background-color: #A7D0F0;'>";
                                                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>No.</th>";
                                                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Name</th>";
                                                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Employee ID</th>";
                                                                                //    dataMail.mail_body += "                    <th style='padding: 8px; text-align: left; border: 1px solid #ccc;'>Organization</th>";
                                                                                //    dataMail.mail_body += "                </tr>";
                                                                                //    dataMail.mail_body += "            </thead>";
                                                                                //    dataMail.mail_body += "            <tbody>";

                                                                                //    var iNo = 1;
                                                                                //    foreach (var itemTravel in tempTravel)
                                                                                //    {
                                                                                //        dataMail.mail_body += "            <tr>";
                                                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc; text-align: center;'>{iNo}</td>";
                                                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{itemTravel.name1}</td>";
                                                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{itemTravel.name3}</td>";
                                                                                //        dataMail.mail_body += $"               <td style='padding: 8px; border: 1px solid #ccc;'>{itemTravel.name4}</td>";
                                                                                //        dataMail.mail_body += "            </tr>";
                                                                                //        iNo++;
                                                                                //    }

                                                                                //    dataMail.mail_body += "            </tbody>";
                                                                                //    dataMail.mail_body += "        </table>";
                                                                                //    dataMail.mail_body += "    </div>";
                                                                                //}

                                                                                //dataMail.mail_body += "    <div style='margin-bottom: 20px; font-size: 14px;'>";
                                                                                //dataMail.mail_body += "        <p style='margin: 0 0 8px 0;'>If you have any questions, please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").</p>";
                                                                                //dataMail.mail_body += "        <p style='margin: 0;'>For application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").</p>";
                                                                                //dataMail.mail_body += "    </div>";
                                                                                //dataMail.mail_body += "    <div style='border-top: 1px solid #eeeeee; padding-top: 15px;'>";
                                                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0;'>Best Regards,</p>";
                                                                                //dataMail.mail_body += "        <p style='font-size: 14px; margin: 0; font-weight: bold;'>Business Travel Services Team (PMSV)</p>";
                                                                                //dataMail.mail_body += "    </div>";
                                                                                //dataMail.mail_body += "</div>";
                                                                                //#endregion

                                                                                var statusItem = context.BZ_MASTER_STATUS.Where(p => p.TS_ID.ToString() == doc_status).FirstOrDefaultAsync();
                                                                                dataMail.step_flow = statusItem?.Result.TS_NAME?.ToString() ?? "";
                                                                                dataMail.doc_id = doc_id;

                                                                                var attachments_doc = AttachmentsInDocNo(context, doc_id);
                                                                                if (!string.IsNullOrEmpty(attachments_doc)) dataMail.mail_attachments = attachments_doc;

                                                                                data.message = SendEmailServiceTravelerProfile.SendMailFlowTrip(dataMail);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }


                                            }

                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    write_log_mail("88-email.message-submitFlow4", "Error: " + ex.ToString());
                                }
                            }

                            #endregion

                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.status = "E";
                            data.message = ex.ToString();
                        }

                    }
                }


                if (!(data.status == "E"))
                {
                    if (value.action.type == "3") // revise
                    {
                        using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                        {
                            //DevFix 20250327 1200  กด Revise ไปหน้า expense status ต้องเป็น Pending by Super Admin   
                            sql = "UPDATE   BZ_DOC_HEAD SET DH_DOC_STATUS = 21, DH_VERSION = 31 WHERE DH_CODE = :id_doc";
                            parameters = new List<OracleParameter>();
                            parameters.Add(context.ConvertTypeParameter("id_doc", doc_id, "char"));
                            iret = context.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                            if (iret > -1) { }
                            ;
                            context.SaveChanges();
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

        //private bool Flow1Mail(decimal? doc_status, string doc_no)
        //{
        //    bool ret = false;
        //    try
        //    {
        //        sendEmailModel data = new sendEmailModel();
        //        if (doc_status == 21) // admin
        //        {
        //            data.mail_subject = doc_no + " : Please submit business travel document request.";
        //        }
        //        else
        //        {
        //            data.mail_subject = doc_no + " : Please initiate business travel document workflow request.";
        //        }
        //        if (!string.IsNullOrEmpty(doc_no))
        //        {
        //            SendEmailServiceTravelerProfile.SendMailFlowTrip(data);
        //        }
        //        ret = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ret = false;
        //    }

        //    return ret;
        //}

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

            if (httpContext.Request.ContentType == null || (!httpContext.Request.ContentType.Contains("multipart/form-data") &&
                    !httpContext.Request.ContentType.Contains("application/x-www-form-urlencoded"))
                    )
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

            foreach (var file in httpContext.Request.Form.Files)
            {
                long fileSizeInBytes = file.Length;
                double fileSizeInMB = fileSizeInBytes / (1024.0 * 1024.0); // แปลงเป็น MB
                if (string.IsNullOrWhiteSpace(file.FileName))
                {
                    data.after_trip.opt1 = "false";
                    data.after_trip.opt2.status = "Invalid file name.";
                    data.after_trip.opt2.remark = "File name cannot be empty.";
                    return data;
                }
                if (fileSizeInMB <= 10) // ตรวจสอบว่าไม่เกิน 10 MB
                {
                    // Console.WriteLine($"ไฟล์ {file.FileName} ขนาด {fileSizeInMB:F2} MB ผ่านเงื่อนไข");
                    // ทำการประมวลผลไฟล์ เช่น บันทึกลงเซิร์ฟเวอร์
                }
                else
                {
                    data.after_trip.opt1 = "false";
                    data.after_trip.opt2.status = "No file selected.";
                    data.after_trip.opt2.remark = $"File {file.FileName} with size {fileSizeInMB:F2} MB exceeds the 10 MB limit and cannot be uploaded.";
                    return data;
                }
            }


            try
            {
                var httpRequest = httpContext.Request;
                string file_doc = (httpRequest.Form["file_doc"].ToString() ?? "")?.Replace("undefined", string.Empty) ?? string.Empty;
                string file_page = (httpRequest.Form["file_page"].ToString()?.Trim() ?? "")?.Replace("undefined", string.Empty) ?? string.Empty;
                string file_emp = (httpRequest.Form["file_emp"].ToString() ?? "")?.Replace("undefined", string.Empty) ?? string.Empty;
                string file_typename = (httpRequest.Form["file_typename"].ToString() ?? "")?.Replace("undefined", string.Empty) ?? string.Empty;
                string file_token_login = (httpRequest.Form["file_token_login"].ToString() ?? "")?.Replace("undefined", string.Empty) ?? string.Empty;

                #region config path
                try
                {
                    file_page = httpRequest.Form["file_page"].ToString().Trim();
                    if (file_page == "kh code") { file_page = "khcode"; }
                }
                catch { }

                string folderName = "AttachmentFile";// "Image";
                string folder_path = $"/{folderName}/{file_doc}{(!string.IsNullOrEmpty(file_page) ? $"/{file_page}" : "")}{(!string.IsNullOrEmpty(file_emp) ? $"/{file_emp}" : "")}/";

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
                ret = CopyFileDataToServer(ref data, files, false, 1, file_doc, folder_path, file_page, file_emp);

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

                    // Update Imgage List มาจาก copy_file_travelerhistory_data_to_server
                    //data.img_list.path = _ServerPathAPI;
                    //data.img_list.filename = newFileName;
                    //data.img_list.fullname = fullUrl;

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
                        string full_path_name = Path.GetFullPath(Path.Combine(data.DF_PATH, data.DF_NAME));
                        if (File.Exists(full_path_name))
                        {
                            ret = sw.ImportDataKH_Code(data.DF_PATH + data.DF_NAME, file_token_login);
                        }
                        //var full_path_name = Path.GetFileName(data.img_list.fullname);

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
                else
                {
                    data.after_trip.opt1 = "false";
                    data.after_trip.opt2.status = $"Upload file failed.{ret}";
                    data.after_trip.opt2.remark = ret;
                }
            }
            catch (Exception ex)
            {
                data.after_trip.opt1 = "false";
                data.after_trip.opt2.status = $"Upload file failed.{ex.Message}";
                data.after_trip.opt2.remark = $"An error occurred: {ex.Message}";
            }

            return data;
        }

        public string file_attached_approval_form(string doc_id, string token_login)
        {
            //DevFix 20250120 0000 ปิดไว้ก่อน เนื่องจากหา funtions ใน Service ไม่เจอ
            try
            {
                //Attached : Approval form >> ตั้งชื่อใบงานตามเลข Doc. เช่น Approval Form_LB21090059 and Any file attached in E-Biz system

                //file_name_approval => Out = @"APPROVAL_FORM_OT21060025_2021100410233333.xlsx"; 
                var file_Approval_Output_form = file_name_approval(doc_id, token_login);
                if (file_Approval_Output_form != "")
                {
                    if (file_Approval_Output_form == "The request failed with HTTP status 401: Unauthorized.")
                    {
                    }
                    else
                    {
                        // @"APPROVAL_FORM_OT21060025_2021100410233333.xlsx";  
                        string file_name = file_Approval_Output_form;

                        //d:\Ebiz\Service\temp\APPROVAL_FORM_OT21060025_2021100410233333.xlsx  
                        string mail_attachments = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "temp", file_name);
                        return mail_attachments;
                    }
                }
            }
            catch (Exception ex) { }

            return "";
        }

        public string file_name_approval(string doc_id, string token_login)
        {
            try
            {
                ClassReport ws_Approval_Output_form = new ClassReport();
                var doc_type = "local";
                if (doc_id.IndexOf("O") > -1) { doc_type = "oversea"; }
                 var arr_token = $"[{{'token_login':'{token_login}','doc_id':'{doc_id}','state':'{doc_type}'}}]";

                ReportParamJsonModel value = new ReportParamJsonModel
                {
                    param = arr_token,
                    method = "phase1report",
                    jsondata = ""
                };

                var sfileTravelReport = ws_Approval_Output_form.TravelReport(value);

                Boolean bCheckUrl = false;
                string[] xsplit = sfileTravelReport.Split(':');
                for (int m = 0; m < xsplit.Length; m++)
                {
                    if (bCheckUrl == true)
                    {
                        string xlast1 = xsplit[m].ToString();
                        string[] xsplit2 = xlast1.Split('"');
                        for (int n = 0; n < xsplit2.Length; n++)
                        {
                            if (xsplit2[n].ToString().Trim() != "")
                            {
                                return xsplit2[n].ToString();
                            }
                        }
                        break;
                    }
                    if (xsplit[m].ToString().IndexOf("file_outbound_name") > -1)
                    {
                        bCheckUrl = true;
                    }
                }
            }
            catch (Exception ex) { return ex.Message.ToString(); }

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
                // if (string.IsNullOrEmpty(safeFileTemp)) return "Invalid file name.";
                // if (!safeFileTemp.All(c => char.IsLetterOrDigit(c) || "_-.".Contains(c))) return "Invalid characters in file name.";
                // if (string.IsNullOrEmpty(extension)) return "File does not have a valid extension.";

                // string[] allowedExtensions = { ".zip", ".xlsx", ".xls", ".pdf", ".doc", ".docx", ".png", ".jpg", ".gif", ".eml", ".msg" };
                // if (!allowedExtensions.Contains(extension)) return "Invalid file type.";

                // กำหนดเส้นทางไดเรกทอรี
                string rootDir = FileUtil.GetDirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot/ExportFile/{file_doc}/{page_name}/{emp_id}")?.FullName ?? "";// Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "ExportFile", file_doc, page_name ?? "", emp_id ?? "");
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


                // ดึงค่า ServerPathAPI จาก ConfigurationManager
                string _ServerPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI() ?? "";

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
                string safeUrl = Uri.UnescapeDataString(relativePath);

                // สร้าง URL เต็ม
                string fullUrl = $"{_ServerPathAPI}{safeUrl}";

                // ตรวจสอบว่า URL ไม่ออกนอกโครงสร้างโฟลเดอร์ที่กำหนด
                if (!fullUrl.StartsWith(_ServerPathAPI, StringComparison.OrdinalIgnoreCase))
                {
                    return "Attempt to create an unauthorized URL.";
                }

                try
                {
                    // กำหนดค่าผลลัพธ์
                    dataDoc.after_trip.opt3.remark = fullUrl ?? "";
                    dataDoc.DF_FULL_PATH = fullUrl ?? "";
                }
                catch { }
            }
            catch (Exception ex)
            {
                return $"An error occurred while processing your request. {ex.Message}";
            }

            return "";
        }

        public static string CopyFileDataToServer(ref DocFileListTravelerhistoryOutModel dataDoc,
        IFormFileCollection files, bool folderCopyFile, int iTypeFile,
        string? fileDoc = "", string? folderPath = "", string? pageName = "", string? empId = "")
        {
            if (files == null || files.Count == 0) return "Invalid files.";
            if (iTypeFile >= 1 && string.IsNullOrWhiteSpace(fileDoc)) return "Invalid Doc No.";
            if (iTypeFile >= 2 && string.IsNullOrWhiteSpace(folderPath)) return "Invalid Folder Path.";
            if (iTypeFile >= 3 && string.IsNullOrWhiteSpace(pageName)) return "Invalid Page Name.";
            if (iTypeFile >= 4 && string.IsNullOrWhiteSpace(empId)) return "Invalid Employee ID.";

            try
            {
                var allowedExtensions = new[] { ".zip", ".xlsx", ".xls", ".pdf", ".doc", ".docx", ".png", ".jpg", ".gif", ".eml", ".msg" };
                // Select the first file
                var file = files[0];
                var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

                // Validate file name and extension
                if (string.IsNullOrWhiteSpace(safeFileName)) return "Invalid file name.";
                //if (!safeFileName.All(c => char.IsLetterOrDigit(c) || "_-. ".Contains(c))) return "Invalid characters in file name.";
                if (!safeFileName.All(c => (char.IsLetterOrDigit(c) || "_-. ".Contains(c) || (c >= 0x0E00 && c <= 0x0E7F)))) // ช่วง Unicode ภาษาไทย 
                {
                    return "Invalid characters in file name.";
                }
                if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension)) return "Invalid file type.";

                // Validate folder path
                if (folderPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0) throw new ArgumentException("Invalid characters in folder path.");
                var rootDir = FileUtil.GetDirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot/{folderPath}")?.FullName ?? "";// Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", folderPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (!Directory.Exists(rootDir)) Directory.CreateDirectory(rootDir);

                if (folderCopyFile)
                {
                    rootDir = Path.Combine(rootDir, "copy");
                    if (!Directory.Exists(rootDir)) Directory.CreateDirectory(rootDir);
                }

                // Generate unique file name
                var datetimeRun = DateTime.Now.ToString("yyyyMMddHHmmss");
                //    var newFileName = $"{safeFileName}-{datetimeRun}{extension}";.
                var newFileName = $"{safeFileName}{extension}";
                var finalFilePath = Path.Combine(rootDir, newFileName);

                // Ensure path safety
                if (!finalFilePath.StartsWith(rootDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    return "Attempt to access unauthorized path.";
                }

                // Save the file
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Retrieve ServerPathAPI from configuration
                var serverPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI() ?? "";

                // Prepare result
                dataDoc.DF_NAME = newFileName; // New file name
                dataDoc.DF_PATH = rootDir; // Server path

                // Construct relative path and URL
                var relativeFullPath = $"{folderPath}".Replace("\\", "/");
                var relativePath = $"{folderPath}{newFileName}".Replace("\\", "/");
                var fullUrl = $"{serverPathAPI.TrimEnd('/')}{Uri.UnescapeDataString(relativePath)}";
                var fullPath = $"{serverPathAPI.TrimEnd('/')}{Uri.UnescapeDataString(relativeFullPath)}";

                // Validate URL integrity
                if (!fullUrl.StartsWith(serverPathAPI, StringComparison.OrdinalIgnoreCase))
                {
                    return "Attempt to create an unauthorized URL.";
                }

                // Populate dataDoc fields
                dataDoc.img_list = new ImgList
                {
                    //finalFilePath = finalFilePath,
                    path = fullPath,
                    filename = newFileName,
                    fullname = fullUrl
                };
                dataDoc.after_trip.opt3.remark = fullUrl;
                dataDoc.DF_FULL_PATH = fullUrl;
            }
            catch (Exception ex)
            {
                // Log error (implement logging mechanism here)
                return $"An error occurred while processing your request. Please try again later.";
            }

            return "true";
        }


    }
}