
using System.Data;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.service.Models.Create_Trip;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;
using System.Text;

namespace top.ebiz.service.Service.Create_Trip
{
    public class TravelerSummaryService
    {
        public List<TravelerSummaryResultModel> getResult(TravelerSummaryModel value)
        {
            return getResultV5(value);
        }
        public List<TravelerSummaryResultModel> getResultV2(TravelerSummaryModel value)
        {
            var bCheckByEmpStatus = false;
            var emp_id_active_list = new List<NormalModel>();
            var emp_id_inactive_list = new List<NormalModel>();

            var data = new List<TravelerSummaryResultModel>();

            if (value != null && value?.traveler_list.Count > 0)
            {
                try
                {
                    //var emp_id_active = "";
                    //var emp_id_inactive = "";
                    List<TravelerSummary> temp = new List<TravelerSummary>();
                    List<TravelerSummary> temp_active = new List<TravelerSummary>();
                    foreach (var item in value.traveler_list)
                    {
                        bCheckByEmpStatus = true;

                        //เนื่องจากหน้าบ้านยังไม่ได้ up ขึ้นไปใหม่ทำให้ส่งข้อมูล item.emp_status  = null
                        if (item.emp_status == null) { item.emp_status = "1"; }

                        //กรณีที่ไม่ใช่ emp ที่ active ไม่ต้องนำไปคำนวณใหม่ ให้ดึงของเดิมมาจาก table 
                        if (item.emp_status == "1")
                        {
                            emp_id_active_list.Add(new NormalModel { text = item.emp_id });

                            temp_active.Add(new TravelerSummary
                            {
                                emp_id = item.emp_id,
                            });
                        }
                        else
                        {
                            emp_id_inactive_list.Add(new NormalModel { text = item.emp_id });
                        }

                        string total_expense = string.IsNullOrEmpty(item.total_expen) ? "0" : item.total_expen;

                        var t = temp.Where(p => p.emp_id.Equals(item.emp_id)).ToList().FirstOrDefault();
                        if (t == null)
                        {
                            temp.Add(new TravelerSummary
                            {
                                emp_id = item.emp_id,
                                total_expen = total_expense,
                            });
                        }
                        else
                        {
                            t.total_expen = Convert.ToString(Convert.ToDecimal(t.total_expen) + Convert.ToDecimal(total_expense));
                        }
                    }

                    value.traveler_list = temp;

                    var doc_no = value.doc_no ?? "";
                    var sql = "";
                    var parameters = new List<OracleParameter>();

                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        var parameters_emp_list = new List<OracleParameter>();
                        var sqlWhere = new StringBuilder();
                        string sql_emp_list = "";
                        string[] emp_lists = [];
                        if (bCheckByEmpStatus == true)
                        {
                            if (emp_id_inactive_list?.Count > 0)
                            {
                                var index = 0;
                                // sql_emp_list += " and (";
                                foreach (var item in emp_id_inactive_list)
                                {
                                    var item_value = item.text ?? "";
                                    // var param_name = $"emp_id_inactive{index}";

                                    // sql_emp_list += $"{(index > 0 ? " or " : "")} a.dta_travel_empid = :{param_name} ";

                                    // parameters_emp_list.Add(context.ConvertTypeParameter(param_name, item_value, "char"));
                                    if (item_value != "")
                                    {
                                        emp_lists[index] = item_value;

                                    }
                                    index++;
                                }
                                // sql_emp_list += " ) ";
                                // sqlWhere.AppendLine(sql_emp_list);
                            }
                            else
                            {
                                if (emp_id_active_list?.Count > 0)
                                {
                                    var index = 0;
                                    // sql_emp_list += " and (";
                                    foreach (var item in emp_id_active_list)
                                    {
                                        var item_value = item.text ?? "";
                                        // var param_name = $"emp_id_active{index}";

                                        // sql_emp_list += $"{(index > 0 ? " and " : "")} a.dta_travel_empid <> :{param_name} ";
                                        // parameters_emp_list.Add(context.ConvertTypeParameter(param_name, item_value, "char"));
                                        if (item_value != "")
                                        {
                                            emp_lists[index] = item_value;

                                        }
                                        index++;
                                    }
                                    // sql_emp_list += " ) ";
                                    // sqlWhere.AppendLine(sql_emp_list);
                                }
                            }
                        }

                        #region query 
                        StringBuilder sqlBuilder = new(@"
                        select distinct dta_travel_empid as emp_id, dta_appr_empid as appr_id, dta_type as appr_type
                                , a.dta_action_status as approve_status
                                , a.dta_appr_remark as approve_remark 
                                , to_char(a.dta_appr_level) as approve_level
                                from BZ_DOC_TRAVELER_APPROVER a 
                                where dta_doc_status is not null 
                                and a.dh_code = :doc_no
                        
                        ");
                        if (bCheckByEmpStatus)
                        {
                            if (emp_id_inactive_list?.Count > 0)
                            {
                                sqlBuilder.AppendLine(" and a.dta_travel_empid in (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids)))");
                                parameters_emp_list.Add(new("empids", string.Join(",", emp_lists)));
                            }
                            else if (emp_id_active_list?.Count > 0)
                            {
                                sqlBuilder.AppendLine(" and a.dta_travel_empid not in (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids)))");
                                parameters_emp_list.Add(new("empids", string.Join(",", emp_lists)));
                            }

                        }

                        sqlBuilder.AppendLine(" order by a.dta_travel_empid, a.dta_appr_level ");
                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                        if (parameters_emp_list != null && parameters_emp_list?.Count > 0)
                        {
                            parameters.AddRange(parameters_emp_list);
                        }
                        var dataApporver_not_active = context.TravelerApproverSummaryConditionModelList.FromSqlRaw(sqlBuilder.ToString(), parameters.ToArray()).ToList();

                        StringBuilder sqlBuilderAc1 = new(@"
                        select distinct dta_travel_empid as emp_id, dta_appr_empid as appr_id, dta_type as appr_type
                                , a.dta_action_status as approve_status
                                , a.dta_appr_remark as approve_remark 
                                , to_char(a.dta_appr_level) as approve_level
                                from BZ_DOC_TRAVELER_APPROVER a 
                                where  dta_doc_status is not null and a.dta_type ='2' and a.dta_action_status <> '1' 
                                and a.dh_code = :doc_no
                        
                        ");
                        if (bCheckByEmpStatus)
                        {
                            if (emp_id_inactive_list?.Count > 0)
                            {
                                sqlBuilderAc1.AppendLine("and a.dta_travel_empid in (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids)))");
                            }
                            else if (emp_id_active_list?.Count > 0)
                            {
                                sqlBuilderAc1.AppendLine("and a.dta_travel_empid not in (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids)))");
                            }
                        }

                        // sqlBuilderAc1.AppendLine(sqlWhere.ToString());
                        sqlBuilderAc1.AppendLine(" order by a.dta_travel_empid, a.dta_appr_level ");

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                        if (parameters_emp_list != null && parameters_emp_list?.Count > 0)
                        {
                            parameters.AddRange(parameters_emp_list);
                        }
                        var dataApporver_in_active = context.TravelerApproverSummaryConditionModelList.FromSqlRaw(sqlBuilderAc1.ToString(), parameters.ToArray()).ToList();



                        StringBuilder sqlBuilderMaz = new(@"
                       select distinct a.dta_travel_empid as emp_id, to_char((max(nvl(a.dta_appr_level,0))+1)) as approve_level
                                 from BZ_DOC_TRAVELER_APPROVER a 
                                 where  dta_doc_status is not null and a.dta_type ='2' 
                                 and a.dh_code = :doc_no  ");
                        if (bCheckByEmpStatus)
                        {
                            if (emp_id_inactive_list?.Count > 0)
                            {
                                sqlBuilderMaz.AppendLine(" and a.dta_travel_empid in (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids))) ");
                            }
                            else if (emp_id_active_list?.Count > 0)
                            {
                                sqlBuilderMaz.AppendLine(" and a.dta_travel_empid not in (SELECT COLUMN_VALUE FROM TABLE(SYS.ODCIVARCHAR2LIST(:empids))) ");
                            }
                        }
                        // sqlBuilderMaz.AppendLine(sqlWhere.ToString());
                        sqlBuilderMaz.AppendLine(" group by a.dta_travel_empid ");

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                        if (parameters_emp_list != null && parameters_emp_list?.Count > 0)
                        {
                            parameters.AddRange(parameters_emp_list);
                        }
                        var dataMaxApprover_Level = context.TravelerApproverSummaryApproveLevelModelList.FromSqlRaw(sqlBuilderMaz.ToString(), parameters.ToArray()).ToList();


                        sql = $"SELECT DH_CODE, DH_TYPE FROM BZ_DOC_HEAD WHERE DH_CODE = :doc_no ";
                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                        var dataDoc = context.TravelerDocHeadList.FromSqlRaw(sql, parameters.ToArray()).ToList().FirstOrDefault();

                        string sqlemp = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGID, ORGNAME, reporttoid as MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO, COSTCENTER as COST_CENTER
                                           FROM VW_BZ_USERS WHERE EMPLOYEEID = :employeeid";

                        //DevFix 20210527 0000 แก้ไขกรณีที่หา CAP หาได้จาก  2 วิธี คือ หาตาม user หรือหาตาม costcenter 
                        sql = @"SELECT distinct to_char(a.DTE_EMP_ID) as EMPLOYEEID, null as ENTITLE, null as ENFIRSTNAME, null as ENLASTNAME, null as ORGID, null as ORGNAME, null as MANAGER_EMPID, b.SH, b.VP, b.AEP, b.EVP, b.SEVP, b.CEO
                             ,b.COST_CENTER
                             FROM BZ_DOC_TRAVELER_EXPENSE a
                             INNER JOIN VW_BZ_MASTER_COSTCENTER_ORG b on a.dte_cost_center = b.cost_center
                             where a.DH_CODE = :doc_no ";

                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                        var costcenterList = context.TravelerUsersModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                        foreach (var item in temp)
                        {
                            //DevFix 20210813 0000 เนื่องจากคำสั่งมีบัคแก้ไม่เป็น เลยเปลี่ยนวิธี
                            var costcenterListList = costcenterList.Where(a => a.EMPLOYEEID == item.emp_id).ToList();
                            if (costcenterListList != null & costcenterListList?.Count > 0)
                            {
                                item.cost_center = costcenterListList?[0].COST_CENTER ?? "";
                            }
                        }
                        value.traveler_list = temp;

                        //DevFix 20210527 0000 แก้ไขกรณีที่หา Line/CAP หาได้จาก  2 วิธี คือ หาตาม user หรือหาตาม costcenter 
                        bool RestLineByCostcenter = false;
                        bool RestCAPByCostcenter = false;

                        sql = @" select 
                              (select  KEY_VALUE as RestLineByCostcenter from BZ_CONFIG_DATA   where KEY_NAME in('RestLineByCostcenter') ) as RestLineByCostcenter
                              ,(select KEY_VALUE as RestCAPByCostcenter from BZ_CONFIG_DATA   where KEY_NAME in('RestCAPByCostcenter')) as RestCAPByCostcenter        
                              from dual  ";
                        parameters = new List<OracleParameter>();
                        var restApproverList = context.RestApproverListModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                        if (restApproverList.Count > 0)
                        {
                            if (restApproverList[0].RestLineByCostcenter == "true") { RestLineByCostcenter = true; }
                            if (restApproverList[0].RestCAPByCostcenter == "true") { RestCAPByCostcenter = true; }
                        }

                        //DevFix 20210607 0000 แก้เงื่อนไขเพิ่มเติม กรณีที่เป็น type training 
                        var doc_head_type = dataDoc?.DH_TYPE ?? "";
                        sql = "SELECT DOC_TYPE, APPR_TYPE, BUDGET_LIMIT, EMP_POSITION, APPROVER_L2 FROM BZ_APPROVER_CONDITION WHERE upper(doc_type) like upper(:doc_head_type || '%') order by budget_limit ";
                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_head_type", doc_head_type, "char"));
                        var conditionList = context.ApproverConditionModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                        #region DevFix ตรวจสอบ Position ของ user  
                        sql = @"  select a.employeeid as EMPLOYEEID, a.function as ORGNAME  
                                from  vw_bz_users a where department is null and sections is null 
                                and (select b.function from vw_bz_users b where b.department is null and b.sections is null and b.employeeid = a.reporttoid  ) = 'SEVP'
                                union
                                select a.employeeid as EMPLOYEEID, a.department as ORGNAME from  vw_bz_users a where function in ( 'SEVP') and department is not null and sections is null 
                                union 
                                select a.employeeid as EMPLOYEEID, a.function as ORGNAME from  vw_bz_users a  where  function = 'SEVP' and department is null and sections is null 
                                union 
                                select a.employeeid as EMPLOYEEID, 'CEO' as ORGNAME from  vw_bz_users a  where  function = 'MGT' and department is null and sections is null ";

                        parameters = new List<OracleParameter>();
                        var positionList = context.TravelerUsersOrgNameList.FromSqlRaw(sql, parameters.ToArray()).ToList();


                        #endregion DevFix ตรวจสอบ Position ของ user 


                        #endregion query


                        if (dataDoc != null)
                        {
                            //*** กรณีที่ Trip < 10000 จะถึงแค่ Section Head
                            Boolean iNeedLine2 = false;
                            double dTotalExpense = 0.00;
                            foreach (var item in value.traveler_list)
                            {
                                try
                                {
                                    if (item.total_expen != null)
                                    {
                                        dTotalExpense += Convert.ToDouble(item.total_expen);
                                    }
                                }
                                catch { }
                            }
                            iNeedLine2 = (dTotalExpense > 10000);



                            List<TravelerApproverConditionModel> apprListCAP = new List<TravelerApproverConditionModel>();
                            if (dataDoc.DH_TYPE.ToUpper() == "OVERSEA")
                            {
                                foreach (var item in value.traveler_list)
                                {
                                    //DevFix 20241225 0000 เปลี่ยนเป็น 
                                    //Traveler = Section Head
                                    //Line approval: Endrose 1 = Section Head
                                    //Line approval: Endrose 2 = VP 

                                    //Traveler = VP
                                    //Line approval : Endrose 1 = VP 
                                    //Traveler = EVP
                                    //Line approval : Endrose 1 = EVP
                                    //ADD LINE - Endorsed 1

                                    //รายละเอียดของ Traverler
                                    string traverlerEmpId = item.emp_id;

                                    Boolean bIsSH = false;
                                    string sqlempIsSH = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGID, ORGNAME, reporttoid as MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO, COSTCENTER as COST_CENTER
                                           FROM VW_BZ_USERS WHERE (EMPLOYEEID = SH or EMPLOYEEID = VP  or EMPLOYEEID = AEP  or EMPLOYEEID = EVP   or EMPLOYEEID = CEO) 
                                           and EMPLOYEEID = :employeeid ";
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("employeeid", traverlerEmpId, "char"));
                                    var usersIsSH = context.TravelerUsersModelList.FromSqlRaw(sqlempIsSH, parameters.ToArray()).ToList().FirstOrDefault();
                                    if (usersIsSH != null)
                                    {
                                        bIsSH = true;
                                    }


                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("employeeid", traverlerEmpId, "char"));
                                    var usersDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();
                                    if (usersDetail != null)
                                    {
                                        //รายละเอียดของ Head ของ Traverler  

                                        //ปรับให้ค้นหาตามตำแหน่ง SH,VP,AEP,EVP,SEVP,CEO 
                                        var managerEmpId = usersDetail.MANAGER_EMPID;
                                        var managerEmpId_VP = usersDetail.VP;

                                        if (bIsSH) { managerEmpId = traverlerEmpId; }
                                        else
                                        {
                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.SH) ? usersDetail.SH : !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersDetail.SEVP) ? usersDetail.SEVP : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                        }
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
                                        var usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                        if (usersApprDetail != null)
                                        {
                                            TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel();
                                            resultModel.line_id = "1";
                                            resultModel.type = "1";
                                            resultModel.emp_id = usersDetail.EMPLOYEEID;
                                            resultModel.emp_name = usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME;
                                            resultModel.emp_org = usersDetail.ORGNAME;
                                            resultModel.appr_id = usersApprDetail != null ? usersApprDetail.EMPLOYEEID : "";
                                            resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                            resultModel.appr_org = usersApprDetail != null ? usersApprDetail.ORGNAME : "";
                                            resultModel.remark = "Endorsed";// หน้าบ้านระบลำดับให้เองเช่น Endorsed1
                                            resultModel.approve_level = "1";// "0";
                                            data.Add(resultModel);

                                            //กรณีที่ไม่มี VP, อนุมานว่าระดับสูงกว่า 
                                            if (iNeedLine2 && !string.IsNullOrEmpty(managerEmpId_VP))
                                            {
                                                //ADD LINE - Endorsed 2
                                                //รายละเอียดของ Head ของ Traverler 
                                                //usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == managerEmpId);
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
                                                usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                                if (usersApprDetail != null)
                                                {
                                                    //รายละเอียดของ Head ของ Manager, เนื่องจาก LINE1 อาจจะสูงกว่า VP
                                                    managerEmpId = usersApprDetail.MANAGER_EMPID;

                                                    //usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == managerEmpId); 
                                                    parameters = new List<OracleParameter>();
                                                    parameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
                                                    usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();


                                                    resultModel = new TravelerSummaryResultModel();
                                                    resultModel.line_id = "2";
                                                    resultModel.type = "1";
                                                    resultModel.emp_id = usersDetail.EMPLOYEEID;
                                                    resultModel.emp_name = usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME;
                                                    resultModel.emp_org = usersDetail.ORGNAME;
                                                    resultModel.appr_id = usersApprDetail != null ? usersApprDetail.EMPLOYEEID : "";
                                                    resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                                    resultModel.appr_org = usersApprDetail != null ? usersApprDetail.ORGNAME : "";
                                                    resultModel.remark = "Endorsed";// หน้าบ้านระบลำดับให้เองเช่น Endorsed1
                                                    resultModel.approve_level = "2";//"0";
                                                    data.Add(resultModel);
                                                }
                                            }
                                        }

                                    }


                                    //ADD CAP

                                    #region  DevFix แก้ไขตามเงื่อนไขใหม่ 
                                    //กรณีที่น้อยกว่า 150,000 ให้ CAP EVP approve 
                                    //กรณีที่น้อยกว่า 150,000 แล้วไม่มี CAP EVP ให้ add manual
                                    //กรณีที่น้อยกว่า 300,000 ให้ CAP EVP approve 
                                    //กรณีที่น้อยกว่า 300,000 แล้วไม่มี CAP EVP ให้ add manual
                                    //กรณีที่น้อยกว่า 300,000 ให้ CAP SEVP approve 
                                    //กรณีที่น้อยกว่า 300,000 แล้วไม่มี CAP SEVP ให้ CEO approve

                                    //ทุกกรณีต้องผ่าน EVP
                                    //กรณีที่เป็น  EVPM,EVPE,QMVP ไม่ต้องให้ CAP EVP approve 
                                    //กรณีที่เป็น SEVP,CEO ไม่ต้อง CAP SEVP approve  

                                    #endregion  DevFix แก้ไขตามเงื่อนไขใหม่ 
                                    bool bBreakLine = false;
                                    bool sRoleUpEVP = false; //กรณีที่เป็น  EVPM,EVPE,QMVP ไม่ต้องให้ CAP EVP approve 
                                    string EMP_POSITION_DEF = "";//ตำแหน่งของพนักงาน 
                                    var check_position = positionList.SingleOrDefault(a => a.EMPLOYEEID == item.emp_id);
                                    if (check_position != null)
                                    {
                                        EMP_POSITION_DEF = check_position.ORGNAME;
                                        sRoleUpEVP = true;
                                    }

                                    var apprConditionList = conditionList.Where(a => a.DOC_TYPE == "oversea").ToList();
                                    var oldCap = new ApproverConditionModel();
                                    foreach (var cap in apprConditionList)
                                    {
                                        bBreakLine = false;
                                        //if (Convert.ToDecimal(item.total_expen) <= cap.BUDGET_LIMIT )
                                        {
                                            if (cap.BUDGET_LIMIT > Convert.ToDecimal(item.total_expen)) { bBreakLine = true; }

                                            if (cap.EMP_POSITION == "EVP"
                                            && (EMP_POSITION_DEF == "SEVP" || EMP_POSITION_DEF == "CEO" || sRoleUpEVP == true))
                                            {
                                                //กรณีที่เป็น  EVPM,EVPE,QMVP ไม่ต้องให้ CAP EVP approve 
                                                //กรณีที่เป็น SEVP,CEO ไม่ต้อง CAP SEVP approve 
                                                continue;
                                            }
                                            else
                                            {
                                                TravelerApproverConditionModel traveler = new TravelerApproverConditionModel();
                                                traveler = new TravelerApproverConditionModel();
                                                traveler.emp_id = item.emp_id;
                                                traveler.total_expen = item.total_expen;
                                                traveler.budget_limit = cap.BUDGET_LIMIT;
                                                traveler.appr_position = cap.EMP_POSITION;
                                                traveler.appr_type = cap.APPR_TYPE;
                                                traveler.doc_type = cap.DOC_TYPE;
                                                apprListCAP.Add(traveler);
                                            }
                                            if (bBreakLine == true) { break; }
                                        }
                                    }
                                }

                                sql = "SELECT DH_CODE, DTE_EMP_ID, DTE_COST_CENTER FROM BZ_DOC_TRAVELER_EXPENSE WHERE dte_status = 1 and DH_CODE = :doc_no ";

                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                                var dataTravelExpenseList = context.TravelerExpenseList.FromSqlRaw(sql, parameters.ToArray()).ToList();


                                var traverleremp_id_bef = "";
                                var traverler_id_aff = "";
                                var iapprove_level = 1;
                                foreach (var item in apprListCAP)
                                {
                                    traverler_id_aff = item.emp_id;
                                    if (traverler_id_aff != traverleremp_id_bef)
                                    {
                                        traverleremp_id_bef = traverler_id_aff;
                                        iapprove_level = 1;
                                    }


                                    TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel();
                                    //var travelExpen = dataTravelExpenseList.Where(a => a.DTE_EMP_ID == item.emp_id).ToList().FirstOrDefault();
                                    var travelExpenList = dataTravelExpenseList.Where(a => a.DTE_EMP_ID == item.emp_id).ToList();
                                    if (travelExpenList != null)
                                    {
                                        foreach (var travelExpen in travelExpenList)
                                        {
                                            var item_cost_center = travelExpen.DTE_COST_CENTER ?? "";
                                            item.cost_center = item_cost_center;

                                            sql = "SELECT COST_CENTER,ORG_ID,OTYPE,COM_CODE,SH,VP,AEP,EVP,SEVP,CEO FROM VW_BZ_MASTER_COSTCENTER_ORG WHERE COST_CENTER = :item_cost_center "; //TODO COST CENTER ID
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("item_cost_center", item_cost_center, "char"));
                                            var masterCostCenterList = context.MasterCostCenterList.FromSqlRaw(sql, parameters.ToArray()).ToList();
                                            //COST_CENTER,ORG_ID,OTYPE,COM_CODE,SH,VP,AEP,EVP,SEVP,CEO


                                            if (masterCostCenterList.Count == 0 && item.appr_position == "SEVP")
                                            {
                                                //กรณีที่ไม่มี Cost center ให้ไปหา CEO
                                                sql = @"SELECT distinct null as COST_CENTER,null as ORG_ID,null as OTYPE,null as COM_CODE,null as SH,VP,null as AEP,null as EVP, null  as SEVP, employeeid as CEO 
                                                    FROM vw_bz_users where  function = 'MGT' and department is null and sections is null ";
                                                parameters = new List<OracleParameter>(); ;
                                                masterCostCenterList = context.MasterCostCenterList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                            }
                                            if (masterCostCenterList.Count > 0)
                                            {
                                                foreach (var costDetail in masterCostCenterList)
                                                {
                                                    var appr_id = "";
                                                    //กรณีที่ไม่มี CAP Approver ให้ add manual 
                                                    if (item.appr_position == "SH")
                                                    {
                                                        appr_id = !string.IsNullOrEmpty(costDetail.SH) ? costDetail.SH : "";
                                                    }
                                                    else if (item.appr_position == "VP")
                                                    {
                                                        appr_id = !string.IsNullOrEmpty(costDetail.VP) ? costDetail.VP : "";
                                                    }
                                                    else if (item.appr_position == "AEP")
                                                    {
                                                        appr_id = !string.IsNullOrEmpty(costDetail.AEP) ? costDetail.AEP : "";
                                                    }
                                                    else if (item.appr_position == "EVP")
                                                    {
                                                        appr_id = !string.IsNullOrEmpty(costDetail.EVP) ? costDetail.EVP : "";
                                                    }
                                                    else if (item.appr_position == "SEVP")
                                                    {
                                                        appr_id = !string.IsNullOrEmpty(costDetail.SEVP) ? costDetail.SEVP : !string.IsNullOrEmpty(costDetail.CEO) ? costDetail.CEO : "";
                                                    }
                                                    else if (item.appr_position == "CEO")
                                                    {
                                                        appr_id = costDetail.CEO;
                                                    }
                                                    if (!string.IsNullOrEmpty(appr_id))
                                                    {
                                                        item.appr_id = appr_id;
                                                        break;
                                                    }
                                                }
                                            }

                                            //var usersDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.emp_id);
                                            //var usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.appr_id);
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("employeeid", item.emp_id, "char"));
                                            var usersDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("employeeid", item.appr_id, "char"));
                                            var usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();


                                            //DevFix 20200828 2140 กรณีที่เป็น Approve และ Approve type เดียวกัน ไม่ต้อง add ซ้ำ
                                            var check_data = data.SingleOrDefault(a => a.appr_id == item.appr_id && a.type == "2"
                                             && a.emp_id == item.emp_id);

                                            if (usersApprDetail != null && check_data == null)
                                            {
                                                resultModel.line_id = "1";
                                                resultModel.type = "2";
                                                resultModel.emp_id = usersDetail.EMPLOYEEID;
                                                resultModel.emp_name = usersDetail != null ? usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME : "";
                                                resultModel.emp_org = usersDetail != null ? usersDetail.ORGNAME : "";
                                                resultModel.appr_id = usersApprDetail.EMPLOYEEID;
                                                resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                                resultModel.appr_org = usersApprDetail.ORGNAME;
                                                resultModel.remark = "CAP";
                                                resultModel.approve_level = iapprove_level.ToString();
                                                data.Add(resultModel);
                                                iapprove_level += 1;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //DevFix 2020828 2341 เพิ่มเงื่อนไข a.DOC_TYPE == dataDoc.DH_TYPE 
                                //var dataListLINE = conditionList.Where(a => a.APPR_TYPE == "LINE" && a.DOC_TYPE == dataDoc.DH_TYPE).ToList().OrderBy(a => a.BUDGET_LIMIT);
                                //var dataListCAP = conditionList.Where(a => a.APPR_TYPE == "CAP" && a.DOC_TYPE == dataDoc.DH_TYPE).ToList().OrderBy(a => a.BUDGET_LIMIT); 
                                var dataListCAP = conditionList.Where(a => a.APPR_TYPE == "CAP" && a.DOC_TYPE == dataDoc.DH_TYPE).ToList();

                                apprListCAP = new List<TravelerApproverConditionModel>();
                                decimal totleExpens = 0;
                                foreach (var item in value.traveler_list)
                                {
                                    totleExpens += Convert.ToDecimal(item.total_expen);
                                }
                                foreach (var item in value.traveler_list)
                                {
                                    //ADD LINE
                                    //var usersDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.emp_id);
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("employeeid", item.emp_id, "char"));
                                    var usersDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();


                                    if (usersDetail != null)
                                    {
                                        //DevFix 20210527 0000 เนื่องจาก local MANAGER_EMPID ที่ได้ไม่ตรงกับข้อมูลจริงของ approver
                                        //ปรับให้ค้นหาตามตำแหน่ง SH,VP,AEP,EVP,SEVP,CEO 
                                        //var usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == usersDetail.MANAGER_EMPID);
                                        #region check line approver 
                                        var managerEmpId = usersDetail.MANAGER_EMPID;
                                        var managerEmpId_VP = usersDetail.VP;
                                        managerEmpId = !string.IsNullOrEmpty(usersDetail.SH) ? usersDetail.SH : !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersDetail.SEVP) ? usersDetail.SEVP : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : "";

                                        var line_id = ""; var line_id_cost = "";
                                        line_id = managerEmpId;

                                        //var usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == line_id); 
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("employeeid", line_id, "char"));
                                        var usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                        if (RestLineByCostcenter == true)
                                        {
                                            //DevFix 20210527 0000 แก้ไขกรณีที่หา CAP หาได้จาก  2 วิธี คือ หาตาม user หรือหาตาม costcenter 
                                            if (item.cost_center != "")
                                            {
                                                //DevFix 20210720 0000 เพิ่ม try กรณีที่ไม่มีข้อมูล 
                                                try
                                                {
                                                    usersApprDetail = costcenterList.Where(a => a.COST_CENTER == item.cost_center).ToList().FirstOrDefault();
                                                    line_id_cost = !string.IsNullOrEmpty(usersApprDetail.SH) ? usersApprDetail.SH : !string.IsNullOrEmpty(usersApprDetail.VP) ? usersApprDetail.VP : !string.IsNullOrEmpty(usersApprDetail.AEP) ? usersApprDetail.AEP : !string.IsNullOrEmpty(usersApprDetail.EVP) ? usersApprDetail.EVP : !string.IsNullOrEmpty(usersApprDetail.SEVP) ? usersApprDetail.SEVP : !string.IsNullOrEmpty(usersApprDetail.CEO) ? usersApprDetail.CEO : "";
                                                }
                                                catch { continue; }
                                            }
                                        }
                                        if (RestLineByCostcenter == true & line_id_cost != "")
                                        {
                                            line_id = line_id_cost;
                                        }
                                        usersApprDetail = new TravelerUsers();
                                        //usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == line_id); 
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("employeeid", line_id, "char"));
                                        usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();


                                        #endregion check line approver

                                        TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel();
                                        resultModel.line_id = "1";
                                        resultModel.type = "1";
                                        resultModel.emp_id = usersDetail.EMPLOYEEID;
                                        resultModel.emp_name = usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME;
                                        resultModel.emp_org = usersDetail.ORGNAME;
                                        resultModel.appr_id = usersApprDetail != null ? usersApprDetail.EMPLOYEEID : "";
                                        resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                        resultModel.appr_org = usersApprDetail != null ? usersApprDetail.ORGNAME : "";
                                        resultModel.remark = "Endorsed";
                                        resultModel.approve_level = "1";// "0";
                                        data.Add(resultModel);

                                        //กรณีที่ไม่มี VP, อนุมานว่าระดับสูงกว่า
                                        if (!string.IsNullOrEmpty(usersDetail.VP))
                                        {
                                            //ADD LINE - Endorsed 2
                                            //รายละเอียดของ Head ของ Traverler   
                                            //usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == managerEmpId);
                                            parameters = new List<OracleParameter>();
                                            parameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
                                            usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();


                                            if (usersApprDetail != null)
                                            {
                                                //รายละเอียดของ Head ของ Manager 
                                                managerEmpId = usersApprDetail.MANAGER_EMPID;
                                                //usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == managerEmpId); 
                                                parameters = new List<OracleParameter>();
                                                parameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
                                                usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                                resultModel = new TravelerSummaryResultModel();
                                                resultModel.line_id = "2";
                                                resultModel.type = "1";
                                                resultModel.emp_id = usersDetail.EMPLOYEEID;
                                                resultModel.emp_name = usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME;
                                                resultModel.emp_org = usersDetail.ORGNAME;
                                                resultModel.appr_id = usersApprDetail != null ? usersApprDetail.EMPLOYEEID : "";
                                                resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                                resultModel.appr_org = usersApprDetail != null ? usersApprDetail.ORGNAME : "";
                                                resultModel.remark = "Endorsed";
                                                resultModel.approve_level = "2";// "0";
                                                data.Add(resultModel);
                                            }
                                        }
                                    }

                                    //FIND CAP
                                    var oldCap = new ApproverConditionModel();
                                    foreach (var cap in dataListCAP)
                                    {
                                        bool bBreakLine = false;
                                        if (cap.BUDGET_LIMIT >= Convert.ToDecimal(totleExpens)) { bBreakLine = true; }
                                        if (bBreakLine == true)
                                        {
                                            TravelerApproverConditionModel traveler = new TravelerApproverConditionModel();

                                            traveler = new TravelerApproverConditionModel();
                                            traveler.emp_id = item.emp_id;
                                            traveler.total_expen = item.total_expen;
                                            traveler.budget_limit = cap.BUDGET_LIMIT;
                                            traveler.appr_position = cap.EMP_POSITION;
                                            traveler.appr_type = cap.APPR_TYPE;
                                            traveler.doc_type = cap.DOC_TYPE;

                                            //DevFix 20210527 0000 แก้ไขกรณีที่หา Line/CAP หาได้จาก  2 วิธี คือ หาตาม user หรือหาตาม costcenter 
                                            traveler.cost_center = item.cost_center;

                                            apprListCAP.Add(traveler);

                                            //DevFix 20200827 1103 add CEO???
                                            if (cap.APPROVER_L2 != null)
                                            {
                                                traveler = new TravelerApproverConditionModel();
                                                traveler.emp_id = item.emp_id;
                                                traveler.total_expen = item.total_expen;
                                                traveler.budget_limit = cap.BUDGET_LIMIT;
                                                traveler.appr_position = cap.APPROVER_L2;
                                                traveler.appr_type = cap.APPR_TYPE;
                                                traveler.doc_type = cap.DOC_TYPE;

                                                //DevFix 20210527 0000 แก้ไขกรณีที่หา Line/CAP หาได้จาก  2 วิธี คือ หาตาม user หรือหาตาม costcenter 
                                                traveler.cost_center = item.cost_center;

                                                apprListCAP.Add(traveler);
                                            }
                                            if (bBreakLine == true) { break; }

                                        }
                                        else
                                        {
                                            //DevFix 20210723 เพิ่มผู็อนุมัติลำดับก่อนหน้า เช่น ถ้ายอดเงินที่ต้องให้ EVP อนุมัติ ต้องให้ SH อนุมัติก่อน 
                                            // SH คือ line ไม่ต้องเพิ่มใน CAP
                                            if (cap.EMP_POSITION != "SH")
                                            {
                                                TravelerApproverConditionModel traveler = new TravelerApproverConditionModel();
                                                traveler = new TravelerApproverConditionModel();
                                                traveler.emp_id = item.emp_id;
                                                traveler.total_expen = item.total_expen;
                                                traveler.budget_limit = cap.BUDGET_LIMIT;
                                                traveler.appr_position = cap.EMP_POSITION;
                                                traveler.appr_type = cap.APPR_TYPE;
                                                traveler.doc_type = cap.DOC_TYPE;
                                                traveler.cost_center = item.cost_center;
                                                apprListCAP.Add(traveler);
                                            }
                                        }
                                    }
                                }

                                var traverleremp_id_bef = "";
                                var traverler_id_aff = "";
                                var iapprove_level = 1;
                                foreach (var item in apprListCAP)
                                {
                                    traverler_id_aff = item.emp_id;
                                    if (traverler_id_aff != traverleremp_id_bef)
                                    {
                                        traverleremp_id_bef = traverler_id_aff;
                                        iapprove_level = 1;
                                    }

                                    TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel();
                                    var appr_id = "";
                                    //var apprData = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.emp_id); 
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("employeeid", item.emp_id, "char"));
                                    var apprData = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                    try
                                    {
                                        if (item.appr_position == "SH")
                                        {
                                            appr_id = apprData.SH;
                                        }
                                        else if (item.appr_position == "VP")
                                        {
                                            appr_id = apprData.VP;
                                        }
                                        else if (item.appr_position == "AEP")
                                        {
                                            appr_id = apprData.AEP;
                                        }
                                        else if (item.appr_position == "EVP")
                                        {
                                            appr_id = apprData.EVP;
                                        }
                                        else if (item.appr_position == "SEVP")
                                        {
                                            appr_id = !string.IsNullOrEmpty(apprData.SEVP) ? apprData.SEVP : !string.IsNullOrEmpty(apprData.CEO) ? apprData.CEO : "";
                                        }
                                        else if (item.appr_position == "CEO")
                                        {
                                            appr_id = apprData.CEO;
                                        }
                                    }
                                    catch { continue; }
                                    item.appr_id = appr_id;

                                    //var usersDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.emp_id);
                                    //var usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.appr_id);

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("employeeid", item.emp_id, "char"));
                                    var usersDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("employeeid", item.appr_id, "char"));
                                    var usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                    //DevFix 20200828 2140 กรณีที่เป็น Approve และ Approve type เดียวกัน ไม่ต้อง add ซ้ำ
                                    var check_data = data.SingleOrDefault(a => a.appr_id == item.appr_id && a.type == (item.appr_type == "LINE" ? "1" : "2")
                                         && a.emp_id == item.emp_id);
                                    if (usersApprDetail != null && check_data == null)
                                    {
                                        resultModel.line_id = "1";
                                        resultModel.type = item.appr_type == "LINE" ? "1" : "2";
                                        resultModel.emp_id = usersDetail.EMPLOYEEID;
                                        resultModel.emp_name = usersDetail != null ? usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME : "";
                                        resultModel.emp_org = usersDetail != null ? usersDetail.ORGNAME : "";
                                        resultModel.appr_id = usersApprDetail.EMPLOYEEID;
                                        resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                        resultModel.appr_org = usersApprDetail.ORGNAME;
                                        resultModel.remark = item.appr_type == "LINE" ? "Endorsed" : "CAP";// หน้าบ้านระบลำดับให้เองเช่น Endorsed1, Endorsed2, CAP1, CAP2

                                        //DevFix 20210810 0000 approve level ตามลำดับได้เลย เรียงตาม traverler id
                                        resultModel.approve_level = iapprove_level.ToString();
                                        iapprove_level += 1;

                                        data.Add(resultModel);
                                    }

                                }

                            }

                        }

                        var data_def = new List<TravelerSummaryResultModel>();
                        if (true)
                        {
                            foreach (var item in dataApporver_not_active)
                            {
                                //add Approver ที่เคย Action ไปแล้ว แต่ไม่มีใน list ที่ต้องคำนวณใหม่ ให้ดึงข้อมูลใน db มาได้เลย
                                //var usersDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.emp_id); 
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("employeeid", item.emp_id, "char"));
                                var usersDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                if (usersDetail != null)
                                {
                                    //var usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.appr_id); 
                                    parameters = new List<OracleParameter>();
                                    parameters.Add(context.ConvertTypeParameter("employeeid", item.appr_id, "char"));
                                    var usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                    TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel();
                                    resultModel.line_id = item.appr_type == "1" ? "1" : "2";
                                    resultModel.type = item.appr_type;
                                    resultModel.emp_id = usersDetail.EMPLOYEEID;
                                    resultModel.emp_name = usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME;
                                    resultModel.emp_org = usersDetail.ORGNAME;
                                    resultModel.appr_id = usersApprDetail != null ? usersApprDetail.EMPLOYEEID : "";
                                    resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                    resultModel.appr_org = usersApprDetail != null ? usersApprDetail.ORGNAME : "";
                                    resultModel.remark = item.appr_type == "1" ? "Endorsed" : "CAP";

                                    resultModel.approve_status = item.approve_status;
                                    resultModel.approve_remark = item.approve_remark;
                                    resultModel.approve_action = "true";
                                    //DevFix 20210810 0000 approve level ตามลำดับได้เลย เรียงตาม traverler id
                                    resultModel.approve_level = item.approve_level;
                                    data_def.Add(resultModel);
                                }
                            }

                            //กรณีที่ไม่ใช่ emp ที่ active ไม่ต้องนำไปคำนวณใหม่ ให้ดึงของเดิมมาจาก table 
                            foreach (var itemData in data)
                            {
                                var check_active = temp_active.SingleOrDefault(a => a.emp_id == itemData.emp_id);
                                if (check_active == null)
                                {
                                    //ปรับให้มี LINE > SH, VP

                                    continue;
                                }
                                if (check_active.emp_id.ToString() != "")
                                {
                                    //กรณีที่เป็นการคำนวณใหม่และมีข้อมูลเดิมอยู่แล้วให้ยึด approver level จากของเดิมมาตั้งต้น
                                    var approve_level = itemData.approve_level;
                                    if (itemData.type == "2")
                                    {
                                        if (dataApporver_in_active != null)
                                        {
                                            try
                                            {
                                                var check_active_new = data_def.SingleOrDefault(a => a.emp_id == itemData.emp_id && a.appr_id == itemData.appr_id.ToString()
                                                && a.type == "2");
                                                if (check_active_new != null)
                                                {
                                                    if (check_active_new.emp_id.ToString() != "")
                                                    {
                                                        continue;//กรณีที่เป็นคนเดียวกันไม่ต้อง add เพิ่ม
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        if (approve_level == "")
                                        {
                                            var check_active_new = dataMaxApprover_Level.Where(a => a.emp_id == itemData.emp_id).ToList();
                                            if (check_active_new != null && check_active_new.Count > 0)
                                            {
                                                if (check_active_new[0].emp_id.ToString() != "")
                                                {
                                                    approve_level = check_active_new[0].approve_level;
                                                    check_active_new[0].approve_level = (Convert.ToInt32(approve_level) + 1).ToString();
                                                }
                                            }
                                            else
                                            {
                                                //กรณีที่ยังไม่เคยมีข้อมูลใน db
                                                approve_level = "1";
                                                dataMaxApprover_Level.Add(new TravelerApproverSummaryApproveLevelModel()
                                                {
                                                    emp_id = itemData.emp_id,
                                                    approve_level = approve_level,
                                                });
                                            }
                                        }
                                    }


                                    var item = itemData;

                                    //var itemList = data.Where(a => a.emp_id == itemData.emp_id).ToList();
                                    //if (itemList != null)
                                    //{
                                    //    foreach (var item in itemList)
                                    //    {
                                    TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel();
                                    resultModel.line_id = item.line_id;
                                    resultModel.type = item.type;
                                    resultModel.emp_id = item.emp_id;
                                    resultModel.emp_name = item.emp_name;
                                    resultModel.emp_org = item.emp_org;
                                    resultModel.appr_id = item.appr_id;
                                    resultModel.appr_name = item.appr_name;
                                    resultModel.appr_org = item.appr_org;
                                    resultModel.remark = item.remark;

                                    resultModel.approve_status = item.approve_status;
                                    resultModel.approve_remark = item.approve_remark;
                                    resultModel.approve_action = item.approve_action;

                                    //DevFix 20210810 0000 approve level ตามลำดับได้เลย เรียงตาม traverler id
                                    resultModel.approve_level = approve_level;
                                    data_def.Add(resultModel);
                                    //    }
                                    //}
                                }
                            }

                            if (dataApporver_in_active != null)
                            {
                                var iapprove_level = 1;
                                foreach (var item in dataApporver_in_active)
                                {
                                    //ไม่เอา approver cap ที่ได้ก่อนหน้านี้
                                    var check_active = data_def.SingleOrDefault(a => a.emp_id == item.emp_id && a.appr_id == item.appr_id.ToString() && a.type == "2");
                                    if (check_active == null)
                                    {
                                        var check_approve_level = data_def.Where(a => a.emp_id == item.emp_id && a.type == "2").ToList();
                                        if (check_approve_level != null && check_approve_level.Count > 0)
                                        {
                                            var i1 = 0;
                                            foreach (var j in check_approve_level)
                                            {
                                                if (Convert.ToInt32(j.approve_level) > i1)
                                                {
                                                    i1 = Convert.ToInt32(j.approve_level);
                                                }
                                            }
                                            iapprove_level = i1 + 1;
                                        }

                                        TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel();
                                        resultModel.line_id = "2";
                                        resultModel.type = item.appr_type;
                                        resultModel.emp_id = item.emp_id;

                                        //var usersDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.emp_id); 
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("employeeid", item.emp_id, "char"));
                                        var usersDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                        resultModel.emp_name = usersDetail != null ? usersDetail.ENTITLE + " " + usersDetail.ENFIRSTNAME + " " + usersDetail.ENLASTNAME : "";
                                        resultModel.emp_org = usersDetail != null ? usersDetail.ORGNAME : "";

                                        resultModel.appr_id = item.appr_id;

                                        //var usersApprDetail = usersList.SingleOrDefault(a => a.EMPLOYEEID == item.appr_id);  
                                        parameters = new List<OracleParameter>();
                                        parameters.Add(context.ConvertTypeParameter("employeeid", item.appr_id, "char"));
                                        var usersApprDetail = context.TravelerUsersModelList.FromSqlRaw(sqlemp, parameters.ToArray()).ToList().FirstOrDefault();

                                        resultModel.appr_name = usersApprDetail.ENTITLE + " " + usersApprDetail.ENFIRSTNAME + " " + usersApprDetail.ENLASTNAME;
                                        resultModel.appr_org = usersApprDetail.ORGNAME;
                                        resultModel.remark = "CAP";

                                        resultModel.approve_status = item.approve_status;
                                        resultModel.approve_remark = item.approve_remark;
                                        resultModel.approve_action = "true";// item.approve_status  == "1" ? "true" : "false";

                                        //DevFix 20210810 0000 approve level ตามลำดับได้เลย เรียงตาม traverler id
                                        resultModel.approve_level = iapprove_level.ToString();
                                        data_def.Add(resultModel);

                                    }
                                }
                            }

                            return data_def;
                        }

                    }


                }
                catch (Exception ex)
                {
                    var x = ex.Message.ToString();
                }
            }
            return data;
        }

        //public List<TravelerSummaryResultModel> getResultV4(TravelerSummaryModel value)
        //{
        //    var bCheckByEmpStatus = false;
        //    var emp_id_active_list = new List<NormalModel>();
        //    var emp_id_inactive_list = new List<NormalModel>();

        //    var data = new List<TravelerSummaryResultModel>();

        //    if (value != null && value?.traveler_list.Count > 0)
        //    {
        //        try
        //        {

        //            List<TravelerSummary> temp = new List<TravelerSummary>();
        //            List<TravelerSummary> temp_active = new List<TravelerSummary>();
        //            foreach (var item in value.traveler_list)
        //            {
        //                bCheckByEmpStatus = true;

        //                //เนื่องจากหน้าบ้านยังไม่ได้ up ขึ้นไปใหม่ทำให้ส่งข้อมูล item.emp_status  = null
        //                if (item.emp_status == null) { item.emp_status = "1"; }

        //                //กรณีที่ไม่ใช่ emp ที่ active ไม่ต้องนำไปคำนวณใหม่ ให้ดึงของเดิมมาจาก table 
        //                if (item.emp_status == "1")
        //                {
        //                    emp_id_active_list.Add(new NormalModel { text = item.emp_id });

        //                    temp_active.Add(new TravelerSummary
        //                    {
        //                        emp_id = item.emp_id,
        //                    });
        //                }
        //                else
        //                {
        //                    emp_id_inactive_list.Add(new NormalModel { text = item.emp_id });
        //                }

        //                string total_expense = string.IsNullOrEmpty(item.total_expen) ? "0" : item.total_expen;

        //                var t = temp.Where(p => p.emp_id.Equals(item.emp_id)).ToList().FirstOrDefault();
        //                if (t == null)
        //                {
        //                    temp.Add(new TravelerSummary
        //                    {
        //                        emp_id = item.emp_id,
        //                        total_expen = total_expense,
        //                    });
        //                }
        //                else
        //                {
        //                    t.total_expen = Convert.ToString(Convert.ToDecimal(t.total_expen) + Convert.ToDecimal(total_expense));
        //                }
        //            }

        //            value.traveler_list = temp;

        //            var doc_no = value.doc_no ?? "";
        //            var sql = "";
        //            var parameters = new List<OracleParameter>();

        //            using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
        //            {
        //                var parameters_emp_list = new List<OracleParameter>();

        //                sql = $"SELECT DH_CODE, DH_TYPE FROM BZ_DOC_HEAD WHERE DH_CODE = :doc_no ";
        //                parameters = new List<OracleParameter>();
        //                parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
        //                var dataDoc = context.TravelerDocHeadList.FromSqlRaw(sql, parameters.ToArray()).ToList().FirstOrDefault();


        //                string sqlemp = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGID, ORGNAME, reporttoid as MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO, COSTCENTER as COST_CENTER
        //                                   FROM VW_BZ_USERS WHERE EMPLOYEEID = :employeeid";

        //                var doc_head_type = (dataDoc?.DH_TYPE ?? "").ToUpper();
        //                var traveler_role = "";
        //                var traveler_function = "";
        //                //DevFix 20210527 0000 แก้ไขกรณีที่หา Line/CAP หาได้จาก  2 วิธี คือ หาตาม user หรือหาตาม costcenter 
        //                bool RestLineByCostcenter = false;
        //                bool RestCAPByCostcenter = false;

        //                if (dataDoc != null)
        //                {
        //                    //*** กรณีที่ Trip < 10000 จะถึงแค่ Section Head
        //                    Boolean iNeedLine2 = false;
        //                    double dTotalExpense = 0.00;
        //                    foreach (var item in value.traveler_list)
        //                    {
        //                        try
        //                        {
        //                            if (item.total_expen != null)
        //                            {
        //                                dTotalExpense += Convert.ToDouble(item.total_expen);
        //                            }
        //                        }
        //                        catch { }
        //                    }
        //                    iNeedLine2 = (dTotalExpense > 10000);

        //                    foreach (var item in value.traveler_list)
        //                    {
        //                        //check user role of travellerlist
        //                        double dtraverlerExpense = Convert.ToDouble(item.total_expen);
        //                        string traverlerEmpId = item.emp_id;

        //                        Boolean bIsSH = false;// ตรวจสอบว่าเป็น  Section Head หรือไม่
        //                        string sqltraveler = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGID, ORGNAME, POSCAT, FUNCTION,reporttoid as MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO, COSTCENTER as COST_CENTER
        //                                                FROM BZ_USERS WHERE EMPLOYEEID = :employeeid ";
        //                        parameters = new List<OracleParameter>();
        //                        parameters.Add(context.ConvertTypeParameter("employeeid", traverlerEmpId, "char"));
        //                        var usersDetail = context.TravelerUsersV2ModelList.FromSqlRaw(sqltraveler, parameters.ToArray()).ToList().FirstOrDefault();

        //                        if (usersDetail != null)
        //                        {
        //                            traveler_role = usersDetail.POSCAT;
        //                            traveler_function = usersDetail.FUNCTION;
        //                            bIsSH = usersDetail.POSCAT == "SH";

        //                            // ถ้า traverler_role เป็น JUNIOR, SENIOR, หรือ SUPERVISOR ให้กำหนดค่าเป็น Staff
        //                            if (usersDetail.POSCAT == "JUNIOR" || usersDetail.POSCAT == "SENIOR" || usersDetail.POSCAT == "SUPERVISOR")
        //                            {
        //                                traveler_role = "STAFF";
        //                            }
        //                            else if (usersDetail.POSCAT == "Dept. Head")
        //                            {
        //                                traveler_role = "VP";
        //                            }

        //                            // ถ้า traverler_function ไม่ใช่ EVPP, EVPE, หรือ EVPM ให้กำหนดค่าเป็น 'All'
        //                            if (usersDetail.FUNCTION != "EVPP" && usersDetail.FUNCTION != "EVPE" && usersDetail.FUNCTION != "EVPM")
        //                            {
        //                                traveler_function = "ALL";
        //                            }
        //                        }

        //                        sql = "select * from BZ_BUDGET_APPROVER_CONDITION where APPROVER_TYPE =:doc_head_type and SPECIAL_CONDITION_ROLE=:traverler_role and SPECIAL_CONDITION_FUNCTION LIKE :traverler_function order by seq";
        //                        parameters = new List<OracleParameter>();
        //                        parameters.Add(context.ConvertTypeParameter("doc_head_type", doc_head_type, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("traverler_role", traveler_role, "char"));
        //                        parameters.Add(context.ConvertTypeParameter("traverler_function", $"%{traveler_function}%", "char"));
        //                        var dataCondition = context.BzBudgetApproverConditionList.FromSqlRaw(sql, parameters.ToArray()).ToList();

        //                        if (dataCondition.Any())
        //                        {
        //                            foreach (var condition in dataCondition)
        //                            {
        //                                var bcontinue = false;
        //                                //var dataConditionAppr = dataCondition.FirstOrDefault();

        //                                // ตรวจสอบ Budget Symbol
        //                                //สัญลักษณ์ คำแทน(Text)
        //                                //< "LESS_THAN"
        //                                //<= "LESS_OR_EQUAL"
        //                                //> "GREATER_THAN"
        //                                //>= "GREATER_OR_EQUAL"
        //                                if (condition.BUDGET_SYMBOL == "<")
        //                                {
        //                                    //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense < Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
        //                                    if (!(dtraverlerExpense <= Convert.ToDouble(condition.BUDGET_LIMIT)))
        //                                    {
        //                                        continue;
        //                                    }
        //                                    else
        //                                    {
        //                                        bcontinue = true;
        //                                    }
        //                                }
        //                                else if (condition.BUDGET_SYMBOL == "<=")
        //                                {
        //                                    //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense <= Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
        //                                    if (!(dtraverlerExpense <= Convert.ToDouble(condition.BUDGET_LIMIT)))
        //                                    {
        //                                        continue;
        //                                    }
        //                                    else
        //                                    {
        //                                        bcontinue = true;
        //                                    }
        //                                }
        //                                else if (condition.BUDGET_SYMBOL == ">")
        //                                {
        //                                    //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense > Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
        //                                    if (!(dtraverlerExpense > Convert.ToDouble(condition.BUDGET_LIMIT)))
        //                                    {
        //                                        continue;
        //                                    }
        //                                    else
        //                                    {
        //                                        bcontinue = true;
        //                                    }
        //                                }
        //                                else if (condition.BUDGET_SYMBOL == ">=")
        //                                {
        //                                    //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense >= Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
        //                                    if (!(dtraverlerExpense >= Convert.ToDouble(condition.BUDGET_LIMIT)))
        //                                    {
        //                                        continue;
        //                                    }
        //                                    else
        //                                    {
        //                                        bcontinue = true;
        //                                    }
        //                                }


        //                                if (condition != null)
        //                                {
        //                                    // ตรวจสอบ LINE_LEVEL 1-2
        //                                    for (int level = 1; level <= 2; level++)
        //                                    {
        //                                        string lineLevelProperty = $"LINE_LEVEL{level}";

        //                                        var lineLevelValue = typeof(BZ_BUDGET_APPROVER_CONDITION)
        //                                            .GetProperty(lineLevelProperty)?
        //                                            .GetValue(condition, null) as string;

        //                                        if (!string.IsNullOrEmpty(lineLevelValue))
        //                                        {
        //                                            string managerEmpId = "";
        //                                            var roles = lineLevelValue.Split('/'); // แยก string ออกมาเป็น array เช่น ["EVP", "SEVP"]

        //                                            foreach (var role in roles)
        //                                            {
        //                                                if (role == "SH")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.SH) ? usersDetail.SH : !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "VP")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "AEP")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "EVP")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "SEVP")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "CEO")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else
        //                                                {
        //                                                    // กรณีที่ไม่มีชื่อตาม field ให้หาตาม function แทน 
        //                                                    string sqlempRoleSpecial = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, SH, VP, AEP, EVP, SEVP, CEO 
        //                                 ,r.ROLE as ORGID,a.FUNCTION as ORGNAME,COSTCENTER as COST_CENTER
        //                                 ,case when (EMPLOYEEID = SH or EMPLOYEEID = VP  or EMPLOYEEID = AEP  or EMPLOYEEID = EVP   or EMPLOYEEID = CEO) then 1 else 0 end MANAGER_EMPID 
        //                                 FROM VW_BZ_USERS a
        //                                 INNER JOIN BZ_EMPLOYEEMAPPINGROLE r on  lower(a.POSCAT) =lower( r.POSCAT) AND R.ACTIVE_TYPE = 1 
        //                                 and upper(a.FUNCTION)  = upper(:role) and a.Department is null and a.sections is null ";
        //                                                    parameters = new List<OracleParameter>();
        //                                                    parameters.Add(context.ConvertTypeParameter("role", role, "char"));
        //                                                    var usersRoleSpecial = context.TravelerUsersModelList.FromSqlRaw(sqlempRoleSpecial, parameters.ToArray()).ToList().FirstOrDefault();
        //                                                    if (usersRoleSpecial != null)
        //                                                    {
        //                                                        managerEmpId = usersRoleSpecial.EMPLOYEEID;
        //                                                    }
        //                                                }

        //                                                // ตรวจสอบว่า managerEmpId นี้มีอยู่ใน data แล้วหรือไม่ (เฉพาะในกลุ่ม LINE_LEVEL)
        //                                                if (!string.IsNullOrEmpty(managerEmpId) && !data.Any(x => x.type == "1" && x.appr_id == managerEmpId))
        //                                                {
        //                                                    var managerParameters = new List<OracleParameter>();
        //                                                    managerParameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
        //                                                    var usersApprDetail = context.TravelerUsersModelList
        //                                                        .FromSqlRaw(sqltraveler, managerParameters.ToArray()).ToList().FirstOrDefault();

        //                                                    if (usersApprDetail != null)
        //                                                    {
        //                                                        TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel
        //                                                        {
        //                                                            line_id = level.ToString(),
        //                                                            type = "1", // line = 1, CAP =2
        //                                                            emp_id = usersDetail.EMPLOYEEID,
        //                                                            emp_name = $"{usersDetail.ENTITLE} {usersDetail.ENFIRSTNAME} {usersDetail.ENLASTNAME}",
        //                                                            emp_org = usersDetail.ORGNAME,
        //                                                            appr_id = usersApprDetail.EMPLOYEEID,
        //                                                            appr_name = $"{usersApprDetail.ENTITLE} {usersApprDetail.ENFIRSTNAME} {usersApprDetail.ENLASTNAME}",
        //                                                            appr_org = usersApprDetail.ORGNAME,
        //                                                            remark = "Endorsed",
        //                                                            approve_level = level.ToString()
        //                                                        };

        //                                                        data.Add(resultModel);
        //                                                        break; // หยุดการวนลูปเมื่อพบชื่อที่ไม่ซ้ำ
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }

        //                                    // ตรวจสอบ CAP_LEVEL 1-3 
        //                                    for (int level = 1; level <= 3; level++)
        //                                    {
        //                                        string lineLevelProperty = $"CAP_LEVEL{level}";

        //                                        var lineLevelValue = typeof(BZ_BUDGET_APPROVER_CONDITION)
        //                                            .GetProperty(lineLevelProperty)?
        //                                            .GetValue(condition, null) as string;

        //                                        if (!string.IsNullOrEmpty(lineLevelValue))
        //                                        {
        //                                            string managerEmpId = "";
        //                                            var roles = lineLevelValue.Split('/'); // แยก string ออกมาเป็น array เช่น ["EVP", "SEVP"]

        //                                            foreach (var role in roles)
        //                                            {
        //                                                if (role == "SH")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.SH) ? usersDetail.SH : !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "VP")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "EVP")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "SEVP")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else if (role == "CEO")
        //                                                {
        //                                                    bcontinue = true;
        //                                                    managerEmpId = !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
        //                                                }
        //                                                else
        //                                                {
        //                                                    // กรณีที่ไม่มีชื่อตาม field ให้หาตาม function แทน 
        //                                                    string sqlempRoleSpecial = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, SH, VP, AEP, EVP, SEVP, CEO 
        //                                 ,r.ROLE as ORGID,a.FUNCTION as ORGNAME,COSTCENTER as COST_CENTER
        //                                 ,case when (EMPLOYEEID = SH or EMPLOYEEID = VP  or EMPLOYEEID = AEP  or EMPLOYEEID = EVP   or EMPLOYEEID = CEO) then 1 else 0 end MANAGER_EMPID 
        //                                 FROM VW_BZ_USERS a
        //                                 INNER JOIN BZ_EMPLOYEEMAPPINGROLE r on  lower(a.POSCAT) =lower( r.POSCAT) AND R.ACTIVE_TYPE = 1 
        //                                 and upper(a.FUNCTION)  = upper(:role) and a.Department is null and a.sections is null ";
        //                                                    parameters = new List<OracleParameter>();
        //                                                    parameters.Add(context.ConvertTypeParameter("role", role, "char"));
        //                                                    var usersRoleSpecial = context.TravelerUsersModelList.FromSqlRaw(sqlempRoleSpecial, parameters.ToArray()).ToList().FirstOrDefault();
        //                                                    if (usersRoleSpecial != null)
        //                                                    {
        //                                                        managerEmpId = usersRoleSpecial.EMPLOYEEID;
        //                                                    }
        //                                                }

        //                                                // ตรวจสอบว่า managerEmpId นี้มีอยู่ใน data แล้วหรือไม่ (เฉพาะในกลุ่ม CAP_LEVEL)
        //                                                if (!string.IsNullOrEmpty(managerEmpId) && !data.Any(x => x.type == "2" && x.appr_id == managerEmpId))
        //                                                {
        //                                                    var managerParameters = new List<OracleParameter>();
        //                                                    managerParameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
        //                                                    var usersApprDetail = context.TravelerUsersModelList
        //                                                        .FromSqlRaw(sqltraveler, managerParameters.ToArray()).ToList().FirstOrDefault();

        //                                                    if (usersApprDetail != null)
        //                                                    {
        //                                                        TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel
        //                                                        {
        //                                                            line_id = level.ToString(),
        //                                                            type = "2", // line = 1, CAP =2
        //                                                            emp_id = usersDetail.EMPLOYEEID,
        //                                                            emp_name = $"{usersDetail.ENTITLE} {usersDetail.ENFIRSTNAME} {usersDetail.ENLASTNAME}",
        //                                                            emp_org = usersDetail.ORGNAME,
        //                                                            appr_id = usersApprDetail.EMPLOYEEID,
        //                                                            appr_name = $"{usersApprDetail.ENTITLE} {usersApprDetail.ENFIRSTNAME} {usersApprDetail.ENLASTNAME}",
        //                                                            appr_org = usersApprDetail.ORGNAME,
        //                                                            remark = "CAP",
        //                                                            approve_level = level.ToString(),
        //                                                            approve_action = "true" // item.approve_status  == "1" ? "true" : "false";
        //                                                        };

        //                                                        data.Add(resultModel);
        //                                                        break; // หยุดการวนลูปเมื่อพบชื่อที่ไม่ซ้ำ
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }


        //                                if (bcontinue) { break; }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            var x = ex.Message.ToString();
        //        }
        //    }
        //    return data;
        //}

        public List<TravelerSummaryResultModel> getResultV5(TravelerSummaryModel value)
        {
            var bCheckByEmpStatus = false;
            var emp_id_active_list = new List<NormalModel>();
            var emp_id_inactive_list = new List<NormalModel>();

            var data = new List<TravelerSummaryResultModel>();

            if (value != null && value?.traveler_list.Count > 0)
            {
                try
                {
                    List<TravelerSummary> temp = new List<TravelerSummary>();
                    List<TravelerSummary> temp_active = new List<TravelerSummary>();
                    foreach (var item in value.traveler_list)
                    {
                        bCheckByEmpStatus = true;

                        //เนื่องจากหน้าบ้านยังไม่ได้ up ขึ้นไปใหม่ทำให้ส่งข้อมูล item.emp_status  = null
                        if (item.emp_status == null) { item.emp_status = "1"; }

                        //กรณีที่ไม่ใช่ emp ที่ active ไม่ต้องนำไปคำนวณใหม่ ให้ดึงของเดิมมาจาก table 
                        if (item.emp_status == "1")
                        {
                            emp_id_active_list.Add(new NormalModel { text = item.emp_id });

                            temp_active.Add(new TravelerSummary
                            {
                                emp_id = item.emp_id,
                            });
                        }
                        else
                        {
                            emp_id_inactive_list.Add(new NormalModel { text = item.emp_id });
                        }

                        string total_expense = string.IsNullOrEmpty(item.total_expen) ? "0" : item.total_expen;

                        var t = temp.Where(p => p.emp_id.Equals(item.emp_id)).ToList().FirstOrDefault();
                        if (t == null)
                        {
                            temp.Add(new TravelerSummary
                            {
                                emp_id = item.emp_id,
                                total_expen = total_expense,
                            });
                        }
                        else
                        {
                            t.total_expen = Convert.ToString(Convert.ToDecimal(t.total_expen));
                        }
                    }

                    value.traveler_list = temp;

                    var doc_no = value.doc_no ?? "";
                    var sql = "";
                    var parameters = new List<OracleParameter>();

                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                    {
                        var parameters_emp_list = new List<OracleParameter>();

                        sql = $"SELECT DH_CODE, DH_TYPE FROM BZ_DOC_HEAD WHERE DH_CODE = :doc_no ";
                        parameters = new List<OracleParameter>();
                        parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                        var dataDoc = context.TravelerDocHeadList.FromSqlRaw(sql, parameters.ToArray()).ToList().FirstOrDefault();


                        string sqlemp = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGID, ORGNAME, reporttoid as MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO, COSTCENTER as COST_CENTER
                            FROM VW_BZ_USERS WHERE EMPLOYEEID = :employeeid";

                        var doc_head_type = (dataDoc?.DH_TYPE ?? "").ToUpper();
                        var traveler_role = "";
                        var traveler_function = "";
                        //DevFix 20210527 0000 แก้ไขกรณีที่หา Line/CAP หาได้จาก  2 วิธี คือ หาตาม user หรือหาตาม costcenter 
                        bool RestLineByCostcenter = false;
                        bool RestCAPByCostcenter = false;

                        if (dataDoc != null)
                        {
                            //*** กรณีที่ Trip < 10000 จะถึงแค่ Section Head
                            Boolean iNeedLine2 = false;
                            double dTotalExpense = 0.00;
                            foreach (var item in value.traveler_list)
                            {
                                try
                                {
                                    if (item.total_expen != null)
                                    {
                                        dTotalExpense += Convert.ToDouble(item.total_expen);
                                    }
                                }
                                catch { }
                            }
                            iNeedLine2 = (dTotalExpense > 10000);

                            foreach (var item in value.traveler_list)
                            {
                                //check user role of travellerlist
                                double dtraverlerExpense = Convert.ToDouble(item.total_expen);
                                string traverlerEmpId = item.emp_id;

                                Boolean bIsSH = false;// ตรวจสอบว่าเป็น  Section Head หรือไม่
                                string sqltraveler = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGID, ORGNAME, POSCAT, FUNCTION,reporttoid as MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO, COSTCENTER as COST_CENTER
                                         FROM BZ_USERS WHERE EMPLOYEEID = :employeeid ";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("employeeid", traverlerEmpId, "char"));
                                var usersDetail = context.TravelerUsersV2ModelList.FromSqlRaw(sqltraveler, parameters.ToArray()).ToList().FirstOrDefault();

                                string sqlsevp = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, ORGID, ORGNAME, POSCAT, FUNCTION,reporttoid as MANAGER_EMPID, SH, VP, AEP, EVP, SEVP, CEO, COSTCENTER as COST_CENTER
                                         FROM BZ_USERS WHERE POSCAT =  'SEVP'";
                                //parameters = new List<OracleParameter>();
                                //parameters.Add(context.ConvertTypeParameter("employeeid", "00000671", "char"));
                                var usersSEVP = context.TravelerUsersV2ModelList.FromSqlRaw(sqlsevp, parameters.ToArray()).ToList().FirstOrDefault();

                                if (usersDetail != null)
                                {
                                    traveler_role = usersDetail.POSCAT;
                                    traveler_function = usersDetail.FUNCTION;
                                    bIsSH = usersDetail.POSCAT == "SH";

                                    // ถ้า traverler_role เป็น JUNIOR, SENIOR, หรือ SUPERVISOR ให้กำหนดค่าเป็น Staff
                                    if (usersDetail.POSCAT == "JUNIOR" || usersDetail.POSCAT == "SENIOR" || usersDetail.POSCAT == "SUPERVISOR")
                                    {
                                        traveler_role = "STAFF";
                                    }
                                    else if (usersDetail.POSCAT == "Dept. Head")
                                    {
                                        traveler_role = "VP";
                                    }

                                    // ถ้า traverler_function ไม่ใช่ EVPP, EVPE, หรือ EVPM ให้กำหนดค่าเป็น 'All'
                                    if (usersDetail.FUNCTION != "EVPP" && usersDetail.FUNCTION != "EVPE" && usersDetail.FUNCTION != "EVPM")
                                    {
                                        traveler_function = "ALL";
                                    }
                                }

                                sql = "select * from BZ_BUDGET_APPROVER_CONDITION where APPROVER_TYPE =:doc_head_type and SPECIAL_CONDITION_ROLE=:traverler_role and SPECIAL_CONDITION_FUNCTION LIKE :traverler_function order by seq";
                                parameters = new List<OracleParameter>();
                                parameters.Add(context.ConvertTypeParameter("doc_head_type", doc_head_type, "char"));
                                parameters.Add(context.ConvertTypeParameter("traverler_role", traveler_role, "char"));
                                parameters.Add(context.ConvertTypeParameter("traverler_function", $"%{traveler_function}%", "char"));
                                var dataCondition = context.BzBudgetApproverConditionList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                                if (dataCondition.Any())
                                {
                                    foreach (var condition in dataCondition)
                                    {
                                        var bcontinue = false;
                                        //var dataConditionAppr = dataCondition.FirstOrDefault();

                                        // ตรวจสอบ Budget Symbol
                                        //สัญลักษณ์ คำแทน(Text)
                                        //< "LESS_THAN"
                                        //<= "LESS_OR_EQUAL"
                                        //> "GREATER_THAN"
                                        //>= "GREATER_OR_EQUAL"
                                        if (condition.BUDGET_SYMBOL == "<")
                                        {
                                            //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense < Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
                                            if (!(dtraverlerExpense <= Convert.ToDouble(condition.BUDGET_LIMIT)))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                bcontinue = true;
                                            }
                                        }
                                        else if (condition.BUDGET_SYMBOL == "<=")
                                        {
                                            //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense <= Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
                                            if (!(dtraverlerExpense <= Convert.ToDouble(condition.BUDGET_LIMIT)))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                bcontinue = true;
                                            }
                                        }
                                        else if (condition.BUDGET_SYMBOL == ">")
                                        {
                                            //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense > Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
                                            if (!(dtraverlerExpense > Convert.ToDouble(condition.BUDGET_LIMIT)))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                bcontinue = true;
                                            }
                                        }
                                        else if (condition.BUDGET_SYMBOL == ">=")
                                        {
                                            //dataConditionAppr = dataCondition.Where(a => dtraverlerExpense >= Convert.ToDouble(a.BUDGET_LIMIT)).FirstOrDefault();
                                            if (!(dtraverlerExpense >= Convert.ToDouble(condition.BUDGET_LIMIT)))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                bcontinue = true;
                                            }
                                        }


                                        if (condition != null)
                                        {
                                            // ตรวจสอบ LINE_LEVEL 1-2
                                            for (int level = 1; level <= 2; level++)
                                            {
                                                string lineLevelProperty = $"LINE_LEVEL{level}";

                                                var lineLevelValue = typeof(BZ_BUDGET_APPROVER_CONDITION)
                                                    .GetProperty(lineLevelProperty)?
                                                    .GetValue(condition, null) as string;

                                                if (!string.IsNullOrEmpty(lineLevelValue))
                                                {
                                                    string managerEmpId = "";
                                                    var roles = lineLevelValue.Split('/'); // แยก string ออกมาเป็น array เช่น ["EVP", "SEVP"]

                                                    foreach (var role in roles)
                                                    {
                                                        if (role == "SH")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.SH) ? usersDetail.SH : !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "VP")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "AEP")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "EVP")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "SEVP")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "CEO")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else
                                                        {
                                                            // กรณีที่ไม่มีชื่อตาม field ให้หาตาม function แทน 
                                                            string sqlempRoleSpecial = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, SH, VP, AEP, EVP, SEVP, CEO 
,r.ROLE as ORGID,a.FUNCTION as ORGNAME,COSTCENTER as COST_CENTER
,case when (EMPLOYEEID = SH or EMPLOYEEID = VP  or EMPLOYEEID = AEP  or EMPLOYEEID = EVP   or EMPLOYEEID = CEO) then 1 else 0 end MANAGER_EMPID 
FROM VW_BZ_USERS a
INNER JOIN BZ_EMPLOYEEMAPPINGROLE r on  lower(a.POSCAT) =lower( r.POSCAT) AND R.ACTIVE_TYPE = 1 
and upper(a.FUNCTION)  = upper(:role) and a.Department is null and a.sections is null ";
                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("role", role, "char"));
                                                            var usersRoleSpecial = context.TravelerUsersModelList.FromSqlRaw(sqlempRoleSpecial, parameters.ToArray()).ToList().FirstOrDefault();
                                                            if (usersRoleSpecial != null)
                                                            {
                                                                managerEmpId = usersRoleSpecial.EMPLOYEEID;
                                                            }
                                                        }

                                                        // เพิ่มผู้อนุมัติโดยไม่ตรวจสอบว่าซ้ำหรือไม่
                                                        if (!string.IsNullOrEmpty(managerEmpId) && !data.Any(x => x.type == "1" && x.appr_id == managerEmpId && x.emp_id == usersDetail.EMPLOYEEID))
                                                        {
                                                            var managerParameters = new List<OracleParameter>();
                                                            managerParameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
                                                            var usersApprDetail = context.TravelerUsersModelList
                                                                .FromSqlRaw(sqltraveler, managerParameters.ToArray()).ToList().FirstOrDefault();

                                                            if (usersApprDetail != null)
                                                            {
                                                                TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel
                                                                {
                                                                    line_id = level.ToString(),
                                                                    type = "1", // line = 1, CAP =2
                                                                    emp_id = usersDetail.EMPLOYEEID,
                                                                    emp_name = $"{usersDetail.ENTITLE} {usersDetail.ENFIRSTNAME} {usersDetail.ENLASTNAME}",
                                                                    emp_org = usersDetail.ORGNAME,
                                                                    appr_id = usersApprDetail.EMPLOYEEID,
                                                                    appr_name = $"{usersApprDetail.ENTITLE} {usersApprDetail.ENFIRSTNAME} {usersApprDetail.ENLASTNAME}",
                                                                    appr_org = usersApprDetail.ORGNAME,
                                                                    remark = "Endorsed",
                                                                    approve_level = level.ToString()
                                                                };

                                                                data.Add(resultModel);
                                                                break; // หยุดการวนลูปเมื่อพบผู้ที่ต้องการ
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // ตรวจสอบ CAP_LEVEL 1-3 
                                            for (int level = 1; level <= 3; level++)
                                            {
                                                string lineLevelProperty = $"CAP_LEVEL{level}";

                                                var lineLevelValue = typeof(BZ_BUDGET_APPROVER_CONDITION)
                                                    .GetProperty(lineLevelProperty)?
                                                    .GetValue(condition, null) as string;

                                                if (!string.IsNullOrEmpty(lineLevelValue))
                                                {
                                                    string managerEmpId = "";
                                                    var roles = lineLevelValue.Split('/'); // แยก string ออกมาเป็น array เช่น ["EVP", "SEVP"]

                                                    foreach (var role in roles)
                                                    {
                                                        if (role == "SH")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.SH) ? usersDetail.SH : !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "VP")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.VP) ? usersDetail.VP : !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "EVP")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.AEP) ? usersDetail.AEP : !string.IsNullOrEmpty(usersDetail.EVP) ? usersDetail.EVP : !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "SEVP")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersSEVP.EMPLOYEEID) ? usersSEVP.EMPLOYEEID : !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else if (role == "CEO")
                                                        {
                                                            bcontinue = true;
                                                            managerEmpId = !string.IsNullOrEmpty(usersDetail.CEO) ? usersDetail.CEO : usersDetail.MANAGER_EMPID;
                                                        }
                                                        else
                                                        {
                                                            // กรณีที่ไม่มีชื่อตาม field ให้หาตาม function แทน 
                                                            string sqlempRoleSpecial = $@"SELECT EMPLOYEEID, ENTITLE, ENFIRSTNAME, ENLASTNAME, SH, VP, AEP, EVP, SEVP, CEO 
,r.ROLE as ORGID,a.FUNCTION as ORGNAME,COSTCENTER as COST_CENTER
,case when (EMPLOYEEID = SH or EMPLOYEEID = VP  or EMPLOYEEID = AEP  or EMPLOYEEID = EVP   or EMPLOYEEID = CEO) then 1 else 0 end MANAGER_EMPID 
FROM VW_BZ_USERS a
INNER JOIN BZ_EMPLOYEEMAPPINGROLE r on  lower(a.POSCAT) =lower( r.POSCAT) AND R.ACTIVE_TYPE = 1 
and upper(a.FUNCTION)  = upper(:role) and a.Department is null and a.sections is null ";
                                                            parameters = new List<OracleParameter>();
                                                            parameters.Add(context.ConvertTypeParameter("role", role, "char"));
                                                            var usersRoleSpecial = context.TravelerUsersModelList.FromSqlRaw(sqlempRoleSpecial, parameters.ToArray()).ToList().FirstOrDefault();
                                                            if (usersRoleSpecial != null)
                                                            {
                                                                managerEmpId = usersRoleSpecial.EMPLOYEEID;
                                                            }
                                                        }

                                                        // เพิ่มผู้อนุมัติโดยไม่ตรวจสอบว่าซ้ำหรือไม่
                                                        if (!string.IsNullOrEmpty(managerEmpId) && !data.Any(x => x.type == "2" && x.appr_id == managerEmpId && x.emp_id == usersDetail.EMPLOYEEID))
                                                        {
                                                            var managerParameters = new List<OracleParameter>();
                                                            managerParameters.Add(context.ConvertTypeParameter("employeeid", managerEmpId, "char"));
                                                            var usersApprDetail = context.TravelerUsersModelList
                                                                .FromSqlRaw(sqltraveler, managerParameters.ToArray()).ToList().FirstOrDefault();

                                                            if (usersApprDetail != null)
                                                            {
                                                                int displayLevel = level;
                                                                if (level == 3 && !data.Any(x => x.type == "2" && x.approve_level == "2" && x.emp_id == usersDetail.EMPLOYEEID))
                                                                {
                                                                    displayLevel = 2;
                                                                }

                                                                TravelerSummaryResultModel resultModel = new TravelerSummaryResultModel
                                                                {
                                                                    line_id = level.ToString(),
                                                                    type = "2", // line = 1, CAP =2
                                                                    emp_id = usersDetail.EMPLOYEEID,
                                                                    emp_name = $"{usersDetail.ENTITLE} {usersDetail.ENFIRSTNAME} {usersDetail.ENLASTNAME}",
                                                                    emp_org = usersDetail.ORGNAME,
                                                                    appr_id = usersApprDetail.EMPLOYEEID,
                                                                    appr_name = $"{usersApprDetail.ENTITLE} {usersApprDetail.ENFIRSTNAME} {usersApprDetail.ENLASTNAME}",
                                                                    appr_org = usersApprDetail.ORGNAME,
                                                                    remark = "CAP",
                                                                    approve_level = displayLevel.ToString(),
                                                                    approve_action = "true" // item.approve_status  == "1" ? "true" : "false";
                                                                };

                                                                data.Add(resultModel);
                                                                break; // หยุดการวนลูปเมื่อพบผู้ที่ต้องการ
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (bcontinue) { break; }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var x = ex.Message.ToString();
                }
            }
            return data;
        }
    }
}