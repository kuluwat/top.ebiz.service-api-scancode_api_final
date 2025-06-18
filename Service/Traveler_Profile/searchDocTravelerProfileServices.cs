using System.Data;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;

using Microsoft.EntityFrameworkCore;
using top.ebiz.helper;

using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Models.Create_Trip;
using System.Globalization;
using System.Collections;
using System.Text;

namespace top.ebiz.service.Service.Traveler_Profile
{
    public class searchDocTravelerProfileServices
    {
        string case_multi_passport = "true";

        //format e-mail
        //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/authen.aspx?page=/master/OB20120006/travelerhistory -->Phase2
        //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/authen.aspx?page=/main/request/edit/OB20120008/i -->Phase1

        //Home--> send mail contact as  ข้อมูลจากหน้าจอ
        //http://localhost:4200/ebizhome



        string sqlstr;
        SetDocService sw;
        //ClassConnectionDb conn;
        DataTable dt;
        #region  new arr
        public EmpListOutModel new_emp_list(string token_login, Boolean user_admin)
        {
            EmpListOutModel emplist = new EmpListOutModel();
            emplist.token_login = token_login;
            emplist.doc_id = "";
            emplist.emp_id = "";
            emplist.id = "";
            emplist.user_admin = user_admin;
            emplist.show_button = false;

            emplist.titlename = "";
            emplist.firstname = "";
            emplist.lastname = "";
            emplist.age = "";
            emplist.org_unit = "";

            emplist.userDisplay = "";
            emplist.userName = "";
            emplist.division = "";
            emplist.idNum = "";
            emplist.userEmail = "";
            emplist.userPhone = "";
            emplist.userTel = "";

            emplist.userCompany = "";
            emplist.userPosition = "";
            emplist.userGender = "";
            emplist.userJoinDate = "";
            emplist.dateOfDeparture = "";

            emplist.isEdit = false;
            emplist.imgpath = "";
            emplist.imgprofilename = "";

            emplist.passportno = "";
            emplist.dateofissue = "";
            emplist.dateofbirth = "";
            emplist.dateofexpire = "";
            emplist.passport_img = "";
            emplist.passport_img_name = "";

            emplist.travel_topic = "";
            emplist.travel_topic_sub = "";
            emplist.business_date = "";
            emplist.travel_date = "";
            emplist.country_city = "";

            emplist.continent_id = "";
            emplist.country_id = "";

            emplist.gl_account = "";
            emplist.cost_center = "";
            emplist.io_wbs = "";

            emplist.remark = "";

            emplist.mail_status = "";
            emplist.mail_remark = "";

            emplist.send_to_sap = "";

            //public List<travelerEmpList> traveler_emp { get; set; } = new List<Models.travelerEmpList>();
            //public List<travelerHistoryList> arrTraveler { get; set; } = new List<Models.travelerHistoryList>();

            return emplist;
        }
        #endregion  new arr

        #region master data  
        public class ExchangeRatesModel
        {
            public string ex_value1 { get; set; }
            public string ex_date { get; set; }
            public string ex_cur { get; set; }
        }
        public ExchangeRatesModel ExchangeRates()
        {
            ExchangeRatesModel ex_rate = new ExchangeRatesModel();
            DataTable dt = new DataTable();
            sqlstr = @" select t_fxb_cur as ex_cur, t_fxb_value1 as ex_value1, t_fxb_valdate as ex_date 
                        from VW_FX_TYPE_M 
                        where  t_fxb_cur = 'USD' 
                        and t_fxb_valdate in (select  max(t_fxb_valdate) from VW_FX_TYPE_M where  t_fxb_cur = 'USD' )";

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    ex_rate = new ExchangeRatesModel();
                    ex_rate.ex_value1 = dt.Rows[0]["ex_value1"].ToString();
                    ex_rate.ex_cur = dt.Rows[0]["ex_cur"].ToString();
                    ex_rate.ex_date = dt.Rows[0]["ex_date"].ToString();
                }

            }
            return ex_rate;
        }
        private DataTable refdata_user_all(string page_name)
        {
            var dt = new DataTable();
            page_name = "all";

            //sqlstr = @" select a.employeeid as emp_id
            //            ,a.entitle || ' ' || a.enfirstname || ' ' || a.enlastname as emp_name
            //            ,a.email 
            //            ,a.usertype as emp_type
            //            ,a.units as emp_unit
            //            from vw_bz_users a ";
            StringBuilder sqlBuilder = new StringBuilder();
            var whereClause = new List<string>();

            List<OracleParameter> parameters = new List<OracleParameter>();


            sqlBuilder.AppendLine(@" select a.employeeid as emp_id
                        ,a.entitle || ' ' || a.enfirstname || ' ' || a.enlastname as emp_name
                        ,a.email 
                        ,a.usertype as emp_type
                        ,a.units as emp_unit
                        from vw_bz_users a ");



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
          
            //if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            //{
            //}

            return dt;
        }
        private DataTable refdata_user_all(string page_name, string token_login)
        {
            var dt = new DataTable();
            page_name = "all";

            //sqlstr = @" select a.employeeid as emp_id
            //            ,a.entitle || ' ' || a.enfirstname || ' ' || a.enlastname as emp_name
            //            ,a.email 
            //            ,a.usertype as emp_type
            //            ,a.units as emp_unit
            //            ,(case when a.usertype = 2 then a.enfirstname else nvl(a.entitle, '')|| ' ' || a.enfirstname || ' ' || a.enlastname end ) as emp_display
            //            from vw_bz_users a where 1=1 ";

            //if (token_login != "")
            //{
            //    sqlstr += @" and  a.employeeid in (select distinct a.user_id  from bz_login_token a where a.token_code =  :token_code) ";
            //    p.Add(new OracleParameter("token_code", token_login));
            //}
            List<OracleParameter> parameters = new List<OracleParameter>();
            StringBuilder sqlBuilder = new StringBuilder();
            var whereClause = new List<string>();




            sqlBuilder.AppendLine(@"  select a.employeeid as emp_id
                        ,a.entitle || ' ' || a.enfirstname || ' ' || a.enlastname as emp_name
                        ,a.email 
                        ,a.usertype as emp_type
                        ,a.units as emp_unit
                        ,case when usertype = 2 THEN enfirstname else nvl(entitle, '') || ' ' || enfirstname || ' ' || enlastname end as emp_display
                        from vw_bz_users a  ");


            if (token_login != "")
            {
                whereClause.Add(@" and  a.employeeid in (select distinct a.user_id  from bz_login_token a where a.token_code =  :token_code) ");

                parameters.Add(ClassConnectionDb.ConvertTypeParameter("token_code", token_login, "char", 4000));


            }
            if (whereClause.Count > 0)
            {
                sqlBuilder.AppendLine("WHERE");
                sqlBuilder.AppendLine(string.Join(" AND ", whereClause));
            }


            string sqlstr = sqlBuilder.ToString();

            //dt = new DataTable();
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
            //dt = new DataTable();
            //var cmd = new OracleCommand(sqlstr);
            //cmd.Parameters.AddRange(p.ToArray());
            //if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            //{
            //}

            return dt;
        }
        public List<ImgList> refdata_img_list_master(string token_login, string doc_id, string page_name, string action_name, string modified_by)
        {

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_IMG);

            var img_list = new List<ImgList>();
            var dt = new DataTable();

            //path,file_name,page_name 
            //sqlstr = @"  select distinct null as doc_id  ,null as emp_id
            //            , a.id 
            //            , a.id_level_1, a.id_level_2
            //            , a.path
            //            , a.file_name as filename
            //            , a.page_name as pagename
            //            , a.action_name as actionname
            //            , a.path || a.file_name as fullname
            //            , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) as modified_by
            //            , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
            //            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
            //            , nvl(active_type,'false') as active_type
            //            from bz_doc_img a
            //            left join vw_bz_users u on (case when a.update_by is null then a.create_by else a.update_by end) = u.employeeid 
            //            where a.status = 1 and lower(a.page_name) = lower('" + page_name + "') ";
            //if (action_name != "") { sqlstr += @" and lower(a.action_name) = lower('" + action_name + "')  "; }
            //sqlstr += " order by a.id ";
            StringBuilder sqlBuilder = new StringBuilder();


            List<OracleParameter> parameters = new List<OracleParameter>();

            sqlBuilder.AppendLine(@"  select distinct null as doc_id  ,null as emp_id
                        , a.id 
                        , a.id_level_1, a.id_level_2
                        , a.path
                        , a.file_name as filename
                        , a.page_name as pagename
                        , a.action_name as actionname
                        , a.path || a.file_name as fullname
                        , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) as modified_by
                        , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
                        , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                        , nvl(active_type,'false') as active_type
                        from bz_doc_img a
                        left join vw_bz_users u on (case when a.update_by is null then a.create_by else a.update_by end) = u.employeeid 
                        ");
            var whereClause1 = new List<string>();
            if (page_name != "")
            {
                whereClause1.Add(@" a.status = 1 and lower(a.page_name) = lower(: page_name )");

                parameters.Add(ClassConnectionDb.ConvertTypeParameter("page_name", page_name, "char", 4000));

            }
            if (whereClause1.Count > 0)
            {
                sqlBuilder.AppendLine("WHERE");
                sqlBuilder.AppendLine(string.Join(" AND ", whereClause1));
            }
            var whereClause2 = new List<string>();
            if (action_name != "")
            {
                whereClause2.Add(@" lower(a.action_name) = lower( :action_name ) ");

                parameters.Add(ClassConnectionDb.ConvertTypeParameter("action_name", action_name, "char", 4000));

            }
            if (whereClause2.Count > 0)
            {
                sqlBuilder.AppendLine(" AND ");
                sqlBuilder.AppendLine(string.Join(" AND ", whereClause2));
            }

            sqlBuilder.AppendLine(" order by a.id");
            string sqlstr = sqlBuilder.ToString();

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
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"SQL Execution Error: {ex.Message}");
            }
            #endregion Execute
            dt = new DataTable();

            if (dt?.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }

                    img_list.Add(new ImgList
                    {
                        doc_id = doc_id,
                        emp_id = dt.Rows[i]["emp_id"].ToString(),
                        id = id,

                        id_level_1 = dt.Rows[i]["id_level_1"].ToString() + "",
                        id_level_2 = dt.Rows[i]["id_level_2"].ToString() + "",
                        path = dt.Rows[i]["path"].ToString() + "",
                        filename = dt.Rows[i]["filename"].ToString() + "",
                        fullname = dt.Rows[i]["fullname"].ToString() + "",
                        pagename = (dt.Rows[i]["pagename"].ToString() ?? "") == "" ? page_name : dt.Rows[i]["pagename"].ToString(),
                        actionname = dt.Rows[i]["actionname"].ToString() + "",
                        modified_date = dt.Rows[i]["modified_date"].ToString() + "",
                        modified_by = dt.Rows[i]["modified_by"].ToString() + "",
                        action_type = dt.Rows[i]["action_type"].ToString() + "",
                        action_change = "false",
                        active_type = (dt.Rows[i]["active_type"].ToString() ?? "") == "" ? "false" : dt.Rows[i]["active_type"].ToString(),
                    });
                }

            }
            if (img_list.Count == 0)
            {
                modified_by = "";
                DataTable dtemp = refdata_user_all("", token_login);
                if (dtemp.Rows.Count > 0) { modified_by = dtemp.Rows[0]["emp_display"].ToString(); }
                img_list.Add(new ImgList { doc_id = doc_id, pagename = page_name, modified_by = modified_by, action_type = "insert", action_change = "false", id = imaxid.ToString() });
            }
            return img_list;
        }

        public List<ImgList> refdata_img_list_personal(string token_login, string doc_id, string page_name, string action_name, string modified_by)
        {

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_IMG);

            var img_list = new List<ImgList>();
            var dt = new DataTable();

            //path,file_name,page_name 
            //sqlstr = @"    select distinct null as doc_id  ,a.emp_id
            //            , a.id 
            //            , a.id_level_1, a.id_level_2
            //            , a.path
            //            , a.file_name as filename
            //            , a.page_name as pagename
            //            , a.action_name as actionname
            //            , a.path || a.file_name as fullname
            //            , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) as modified_by
            //            , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
            //            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
            //            , nvl(active_type,'false') as active_type
            //            from bz_doc_img a
            //            left join vw_bz_users u on (case when a.update_by is null then a.create_by else a.update_by end) = u.employeeid 
            //            where a.status = 1 and lower(a.page_name) = lower('" + page_name + "')  ";//visadocument
            //if (action_name != "") { sqlstr += @" and lower(a.action_name) = lower('" + action_name + "')  "; }
            //sqlstr += " order by a.id ";

            StringBuilder sqlBuilder = new StringBuilder();
            var whereClause = new List<string>();

            List<OracleParameter> parameters = new List<OracleParameter>();

            sqlBuilder.AppendLine(@"   select distinct null as doc_id  ,a.emp_id
           , a.id 
           , a.id_level_1, a.id_level_2
           , a.path
           , a.file_name as filename
           , a.page_name as pagename
           , a.action_name as actionname
           , a.path || a.file_name as fullname
           , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) as modified_by
           , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
           , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
           , nvl(active_type,'false') as active_type
           from bz_doc_img a
           left join vw_bz_users u on (case when a.update_by is null then a.create_by else a.update_by end) = u.employeeid  ");

      
            var whereClause1 = new List<string>();
            if (!string.IsNullOrEmpty(page_name))
            {
                whereClause1.Add(@" a.status = 1 and lower(a.page_name) = lower(:page_name)");
                parameters.Add(ClassConnectionDb.ConvertTypeParameter("page_name", page_name, "char", 4000));
            }

            if (whereClause1.Count > 0)
            {
                sqlBuilder.AppendLine("WHERE");
                sqlBuilder.AppendLine(string.Join(" AND ", whereClause1));
            }

          
            var whereClause2 = new List<string>();
            if (!string.IsNullOrEmpty(action_name))
            {
                Console.WriteLine("Adding action_name condition to whereClause2");
                whereClause2.Add(@" lower(a.action_name) = lower(:action_name)");
                parameters.Add(ClassConnectionDb.ConvertTypeParameter("action_name", action_name, "char", 4000));
            }

            Console.WriteLine($"whereClause2 count: {whereClause2.Count}");

            if (whereClause2.Count > 0)
            {
                sqlBuilder.AppendLine(" AND ");
                sqlBuilder.AppendLine(string.Join(" AND ", whereClause2));
                Console.WriteLine("Added whereClause2 to SQL");
            }

            sqlBuilder.AppendLine("order by a.id");

          
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

            if (dt?.Rows.Count>0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }

                    img_list.Add(new ImgList
                    {
                        doc_id = doc_id,
                        emp_id = dt.Rows[i]["emp_id"].ToString(),
                        id = id,

                        id_level_1 = dt.Rows[i]["id_level_1"].ToString() + "",
                        id_level_2 = dt.Rows[i]["id_level_2"].ToString() + "",
                        path = dt.Rows[i]["path"].ToString() + "",
                        filename = dt.Rows[i]["filename"].ToString() + "",
                        fullname = dt.Rows[i]["fullname"].ToString() + "",
                        pagename = (dt.Rows[i]["pagename"].ToString() ?? "") == "" ? page_name : dt.Rows[i]["pagename"].ToString(),
                        actionname = dt.Rows[i]["actionname"].ToString() + "",
                        modified_date = dt.Rows[i]["modified_date"].ToString() + "",
                        modified_by = dt.Rows[i]["modified_by"].ToString() + "",
                        action_type = dt.Rows[i]["action_type"].ToString() + "",
                        action_change = "false",
                        active_type = (dt.Rows[i]["active_type"].ToString() ?? "") == "" ? "false" : dt.Rows[i]["active_type"].ToString(),
                    });
                }

            }
            if (img_list.Count == 0)
            {
                modified_by = "";
                DataTable dtemp = refdata_user_all("", token_login);
                if (dtemp.Rows.Count > 0) { modified_by = dtemp.Rows[0]["emp_display"].ToString(); }
                img_list.Add(new ImgList { doc_id = doc_id, pagename = page_name, modified_by = modified_by, action_type = "insert", action_change = "false", id = imaxid.ToString() });
            }
            return img_list;
        }
        public List<ImgList> refdata_img_list_visa(string token_login, string doc_id, string page_name, string action_name, string modified_by)
        {
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_IMG);

            var img_list = new List<ImgList>();
            var dt = new DataTable();

            //path,file_name,page_name 
            sqlstr = @"    select distinct null as doc_id  ,a.emp_id
                        , a.id 
                        , a.id_level_1, a.id_level_2
                        , a.path
                        , a.file_name as filename
                        , a.page_name as pagename
                        , a.action_name as actionname
                        , a.path || a.file_name as fullname
                        , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) as modified_by
                        , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
                        , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                        , nvl(active_type,'false') as active_type
                        from bz_doc_img a
                        left join vw_bz_users u on (case when a.update_by is null then a.create_by else a.update_by end) = u.employeeid 
                        where a.status = 1 and lower(a.page_name) = lower('" + page_name + "')  ";//visadocument
            if (action_name != "") { sqlstr += @" and lower(a.action_name) = lower('" + action_name + "')  "; }
            sqlstr += @" and a.emp_id in (select distinct ex.dte_emp_id from bz_doc_head h 
                        inner join bz_doc_traveler_expense ex on h.dh_code = ex.dh_code where h.dh_code = '" + doc_id + "'   ) ";
            sqlstr += " order by a.action_name, a.emp_id, a.id ";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }

                    img_list.Add(new ImgList
                    {
                        doc_id = doc_id,
                        emp_id = dt.Rows[i]["emp_id"].ToString(),
                        id = id,

                        id_level_1 = dt.Rows[i]["id_level_1"].ToString() + "",
                        id_level_2 = dt.Rows[i]["id_level_2"].ToString() + "",
                        path = dt.Rows[i]["path"].ToString() + "",
                        filename = dt.Rows[i]["filename"].ToString() + "",
                        fullname = dt.Rows[i]["fullname"].ToString() + "",
                        pagename = (dt.Rows[i]["pagename"].ToString() ?? "") == "" ? page_name : dt.Rows[i]["pagename"].ToString(),
                        actionname = dt.Rows[i]["actionname"].ToString() + "",
                        modified_date = dt.Rows[i]["modified_date"].ToString() + "",
                        modified_by = dt.Rows[i]["modified_by"].ToString() + "",
                        action_type = dt.Rows[i]["action_type"].ToString() + "",
                        action_change = "false",
                        active_type = (dt.Rows[i]["active_type"].ToString() ?? "") == "" ? "false" : dt.Rows[i]["active_type"].ToString(),
                    });
                }
            }

            Boolean submit_expense_phase1 = false;
            sqlstr = @" select count(1) as xcount
                            from bz_doc_head a 
                            where dh_doc_status in (31,32,41,42,50) and dh_code ='" + doc_id + "' ";

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["xcount"].ToString() != "0")
                    {
                        if (submit_expense_phase1 == true)
                        {
                            //ตรวจสอบกรณีที่ใบงานใน phase1 มีการ submit ใน step 2 ถึงจะ gen file name ตั้งต้น EMPLOYEE_LETTER.docx ให้  
                            img_list.Add(new ImgList
                            {
                                doc_id = doc_id,
                                emp_id = "",
                                id = imaxid.ToString(),

                                id_level_1 = "",
                                id_level_2 = "",

                                path = "path",
                                filename = "EMPLOYEE_LETTER.docx",
                                fullname = "",
                                pagename = page_name,
                                actionname = "",
                                modified_date = "",
                                modified_by = "Auto Generate",
                                action_type = "insert",
                                action_change = "false",
                                active_type = "false",
                            });
                        }
                    }
                }
            }

            if (img_list.Count == 0)
            {
                modified_by = "";
                DataTable dtemp = refdata_user_all("", token_login);
                if (dtemp.Rows.Count > 0) { modified_by = dtemp.Rows[0]["emp_display"].ToString(); }
                img_list.Add(new ImgList { doc_id = doc_id, pagename = page_name, modified_by = modified_by, action_type = "insert", action_change = "false", id = imaxid.ToString() });
            }
            return img_list;
        }
        public List<ImgList> refdata_img_list(string doc_id, string page_name, string action_name, string modified_by)
        {
            Boolean submit_expense_phase1 = false;
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_IMG);

            var img_list = new List<ImgList>();
            var dt = new DataTable();
            if (page_name == "visa")
            {
                //ตรวจสอบกรณีที่ใบงานใน phase1 มีการ submit ใน step 2 ถึงจะ gen file name ตั้งต้น EMPLOYEE_LETTER.docx ให้  
                //31  Pending by Line Approver
                //32  Approve by Line Approver
                //41  Pending by CAP
                //42  Approved
                //50  Completed
                sqlstr = @" select count(1) as xcount
                            from bz_doc_head a 
                            where dh_doc_status in (31,32,41,42,50) and dh_code =:doc_id";
                p.Add(new OracleParameter("doc_id", doc_id));
                cmd = new(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0]["xcount"].ToString() != "0")
                        {
                            submit_expense_phase1 = true;
                        }
                    }
                }
            }
            p.Clear();
            //path,file_name,page_name
            sqlstr = @" select distinct ";
            if (doc_id == "personal")
            {
                sqlstr += @" personal as doc_id";
            }
            else
            {
                sqlstr += @" h.dh_code as doc_id";
            }
            sqlstr += @" ,ex.dte_emp_id as emp_id
                    , a.id 
                    , a.id_level_1, a.id_level_2
                    , a.path
                    , a.file_name as filename
                    , a.page_name as pagename
                    , a.action_name as actionname
                    , a.path || a.file_name as fullname
                    , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) as modified_by
                    , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                    , nvl(active_type,'false') as active_type
                    from bz_doc_head h
                    inner join  (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_img a on a.emp_id  = ex.dte_emp_id  and a.status = 1
                    and lower(a.page_name) = lower(:page_name) ";
            p.Add(new OracleParameter("page_name", page_name));

            if (page_name == "passport")
            {

            }
            else
            {
                if (page_name == "visa")
                {
                    //sqlstr += " and h.dh_code = a.doc_id ";
                }
                else { sqlstr += " and h.dh_code = a.doc_id "; }
            }
            sqlstr += "  left join vw_bz_users u on(case when a.update_by is null then a.create_by else a.update_by end) = u.employeeid ";
            if (doc_id == "personal")
            {
                sqlstr += " where h.dh_code = :doc_id ";
                p.Add(new OracleParameter("doc_id", doc_id));

            }
            else
            {
                sqlstr += " where h.dh_code = :doc_id ";
                p.Add(new OracleParameter("doc_id", doc_id));

            }
            if (action_name != "") { sqlstr += @" and lower(a.action_name) = lower(:action_name)  "; p.Add(new OracleParameter("action_name", action_name)); }
            sqlstr += " order by ex.dte_emp_id,a.id ";

            if (page_name == "isos")
            {
                sqlstr = @" select '' as doc_id,'' as emp_id
                            , a.id 
                            , a.id_level_1, a.id_level_2
                            , a.path
                            , a.file_name as filename
                            , a.page_name as pagename
                            , a.action_name as actionname
                            , a.path || a.file_name as fullname 
                            , (case when a.update_by is null then a.create_by else a.update_by end) modified_by
                            , to_char(case when a.update_date is null then a.create_date else a.update_date end,'dd MON rrrr') as modified_date
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                            , nvl(active_type,'false') as active_type
                           from  bz_doc_img a where lower(a.page_name) = lower(:page_name) and a.status = 1 ";
                p.Add(new OracleParameter("page_name", page_name));

            }

            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                p.Clear();
                var token_login = modified_by;
                DataTable dtcountry = new DataTable();
                if (page_name == "visa")
                {
                    cmd = SqlstrDataEmpDetailWithCommand(token_login, doc_id, "");
                    sqlstr = cmd.CommandText;
                    var sqlstr_country = @" select distinct country_name as country_text, doc_id, emp_id from (" + sqlstr + ")t order by doc_id, emp_id ";
                    if (doc_id.ToUpper().IndexOf("L") > -1)
                    {
                        sqlstr_country = @" select distinct province as country_text, doc_id, emp_id from (" + sqlstr + ")t order by doc_id, emp_id ";
                    }
                    // cmd.CommandText = sqlstr_country;
                    OracleCommand cmd2 = new(sqlstr_country);
                    cmd2.Parameters.Add("token_login", token_login);
                    cmd2.Parameters.Add("doc_id", doc_id);

                    dtcountry = new DataTable();
                    SetDocService.conn_ExecuteData(ref dtcountry, cmd2);
                }

                if (img_list.Count == 0)
                {
                    if (page_name == "visa")
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var emp_id_select = dt.Rows[i]["emp_id"].ToString();
                            submit_expense_phase1 = false;
                            var id = dt.Rows[i]["id"].ToString();
                            if (dt.Rows[i]["action_type"].ToString() == "insert")
                            {
                                id = imaxid.ToString();
                                imaxid++;
                            }

                            string country_text = "";
                            if (dtcountry.Rows.Count > 0)
                            {
                                // DataRow[] drcountry = dtcountry.Select("emp_id = '" + emp_id_select + "'");
                                DataRow[] drcountry = dtcountry.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                                for (int c = 0; c < drcountry.Length; c++)
                                {
                                    country_text = drcountry[c]["country_text"].ToString();
                                    List<ImgList> drselect = img_list.Where(a => (a.emp_id == emp_id_select & a.filename == "EMPLOYEE_LETTER_" + country_text.ToUpper() + ".docx")).ToList();
                                    if (drselect.Count == 0)
                                    {
                                        submit_expense_phase1 = true;
                                    }
                                    if (submit_expense_phase1 == true)
                                    {
                                        // DataRow[] drcheck = dt.Select("filename='EMPLOYEE_LETTER_" + country_text.ToUpper() + ".docx'");
                                        string fileName = $"EMPLOYEE_LETTER_{country_text?.ToUpper()}.docx";
                                        DataRow[] drcheck = dt.AsEnumerable().Where(s => s.Field<string>("filename") == fileName).ToArray();

                                        if (drcheck.Length == 0)
                                        {
                                            submit_expense_phase1 = true;
                                        }
                                    }

                                    if (submit_expense_phase1 == true)
                                    {
                                        //ตรวจสอบกรณีที่ใบงานใน phase1 มีการ submit ใน step 2 ถึงจะ gen file name ตั้งต้น EMPLOYEE_LETTER.docx ให้  
                                        img_list.Add(new ImgList
                                        {
                                            doc_id = doc_id,
                                            emp_id = emp_id_select,
                                            id = id.ToString(),
                                            id_level_1 = dt.Rows[i]["id_level_1"].ToString() + "",
                                            id_level_2 = dt.Rows[i]["id_level_2"].ToString() + "",

                                            path = "path",
                                            filename = "EMPLOYEE_LETTER_" + country_text.ToUpper() + ".docx",
                                            fullname = "",
                                            pagename = page_name,
                                            actionname = "",
                                            modified_date = "",
                                            modified_by = "Auto Generate",
                                            action_type = "insert",
                                            action_change = "false",
                                            active_type = "false",
                                        });
                                    }

                                }
                            }

                        }
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var id = dt.Rows[i]["id"].ToString();
                        if (dt.Rows[i]["action_type"].ToString() == "insert")
                        {
                            if (page_name == "visa")
                            {
                                continue;
                            }
                            else
                            {
                                id = imaxid.ToString();
                                imaxid++;
                            }
                        }
                        img_list.Add(new ImgList
                        {
                            doc_id = doc_id,
                            emp_id = dt.Rows[i]["emp_id"].ToString(),
                            id = id,

                            id_level_1 = dt.Rows[i]["id_level_1"].ToString() + "",
                            id_level_2 = dt.Rows[i]["id_level_2"].ToString() + "",
                            path = dt.Rows[i]["path"].ToString() + "",
                            filename = dt.Rows[i]["filename"].ToString() + "",
                            fullname = dt.Rows[i]["fullname"].ToString() + "",
                            pagename = (dt.Rows[i]["pagename"].ToString() ?? "") == "" ? page_name : dt.Rows[i]["pagename"].ToString(),
                            actionname = dt.Rows[i]["actionname"].ToString() + "",
                            modified_date = dt.Rows[i]["modified_date"].ToString() + "",
                            modified_by = dt.Rows[i]["modified_by"].ToString() + "",
                            action_type = dt.Rows[i]["action_type"].ToString() + "",
                            action_change = "false",
                            active_type = (dt.Rows[i]["active_type"].ToString() ?? "") == "" ? "false" : dt.Rows[i]["active_type"].ToString(),
                        });
                    }
                }
            }

            if (img_list.Count == 0 && submit_expense_phase1 == true)
            {
                if (page_name == "visa" & submit_expense_phase1 == true)
                {
                    //ตรวจสอบกรณีที่ใบงานใน phase1 มีการ submit ใน step 2 ถึงจะ gen file name ตั้งต้น EMPLOYEE_LETTER.docx ให้  
                    img_list.Add(new ImgList
                    {
                        doc_id = doc_id,
                        emp_id = "",
                        id = imaxid.ToString(),

                        id_level_1 = "",
                        id_level_2 = "",

                        path = "path",
                        filename = "EMPLOYEE_LETTER.docx",
                        fullname = "",
                        pagename = page_name,
                        actionname = "",
                        modified_date = "",
                        modified_by = "Auto Generate",
                        action_type = "insert",
                        action_change = "false",
                        active_type = "false",
                    });
                }
                else
                {
                    img_list.Add(new ImgList { doc_id = doc_id, pagename = page_name, modified_by = "", action_type = "insert", action_change = "false", id = imaxid.ToString() });
                }
            }
            return img_list;
        }
        private DataTable refdata_book_status(string page_name)
        {
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            var dt = new DataTable();
            if (page_name == "airticket" || page_name == "accommodation")
            {
                page_name = "bookingstatus";
            }
            else
            {
                page_name = "all";
            }

            sqlstr = @" select  id, name, sort_by
                        from bz_master_list_status 
                        where status =1  and lower(page_name) = lower(:page_name) order by sort_by";
            p.Add(new OracleParameter("page_name", page_name));
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            dt = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
            }
            return dt;
        }
        private DataTable refdata_book_type()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,sort_by
                        from BZ_MASTER_ALREADY_BOOKED_TYPE
                        where status =1  order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable refdata_room_type()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,sort_by
                        from BZ_MASTER_ROOM_TYPE
                        where status =1  order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable refdata_m_allowance_type()
        {
            var dt = new DataTable();

            sqlstr = @" select id, name, sort_by
                        from bz_master_allowance_type
                        where status =1  order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }

        private DataTable refdata_m_feedback_type()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,sort_by
                        from bz_master_feedback_type
                        where status =1  order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            { }
            return dt;
        }
        private DataTable refdata_m_feedback_list()
        {
            var dt = new DataTable();
            sqlstr = @" select id,question as name,sort_by,feedback_type_id
                        from bz_master_feedback_list
                        where status =1  order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            { }
            return dt;
        }
        private DataTable refdata_kh_code()
        {
            var dt = new DataTable();
            sqlstr = @" select id, emp_id, user_id, oversea_code, local_code, status, nvl(data_for_sap,0) as data_for_sap
                        , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                        from bz_data_kh_code a
                        where status =1  order by id";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }

        public DataTable ref_exchangerate_all_type()
        {
            dt = new DataTable();
            sqlstr = @"  select id, t_fxb_cur as currency_id,  round(t_fxb_value1,4) as exchange_rate
                        ,date_from
                        ,date_to 
                        from VW_BZ_FX_TYPE_M 
                        where (t_fxb_valdate,t_fxb_cur) in (select  max(t_fxb_valdate),t_fxb_cur from VW_BZ_FX_TYPE_M group by  t_fxb_cur )
                        order by t_fxb_cur ";

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        public DataTable ref_exchangerate()
        {
            //substr(t_fxb_valdate,7,2) || ' ' || to_char(to_date(substr(t_fxb_valdate,5,2) || '012020','MMDDYYYY'),'MON') || ' ' || substr(t_fxb_valdate,0,4) as 
            dt = new DataTable();
            sqlstr = @" select id, t_fxb_cur as currency_id, round(t_fxb_value1,4) as exchange_rate
                        ,date_from
                        ,date_to 
                        from VW_BZ_FX_TYPE_M 
                        where  t_fxb_cur = 'USD' 
                        and t_fxb_valdate in (select  max(t_fxb_valdate) from VW_BZ_FX_TYPE_M where  t_fxb_cur = 'USD' )    order by t_fxb_cur ";
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }

            return dt;
        }
        public DataTable ref_exchangerateadddefdate()
        {
            dt = new DataTable();
            sqlstr = @"SELECT 
                  id, 
                  t_fxb_cur AS currency_id, 
                  ROUND(t_fxb_value1,4) AS exchange_rate,
                  TO_CHAR(t_fxb_valdate, 'YYYYMMDD') AS def_date,  -- เพิ่มคอลัมน์นี้
                  date_from,
                  date_to 
               FROM VW_BZ_FX_TYPE_M 
               WHERE t_fxb_cur = 'USD' 
               AND t_fxb_valdate IN (
                   SELECT MAX(t_fxb_valdate) 
                   FROM VW_BZ_FX_TYPE_M 
                   WHERE t_fxb_cur = 'USD'
               ) 
               ORDER BY t_fxb_cur";

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                // ตรวจสอบว่ามีคอลัมน์ที่ต้องการหรือไม่
                if (!dt.Columns.Contains("def_date"))
                {
                    // ถ้าไม่สามารถเพิ่มใน SQL ได้ ให้เพิ่มแบบ Manual
                    dt.Columns.Add("def_date", typeof(string));
                    foreach (DataRow row in dt.Rows)
                    {
                        row["def_date"] = row["date_from"]; // หรือ logic อื่นที่เหมาะสม
                    }
                }
            }
            return dt;
        }
        public DataTable ref_exchangerate_by_doc(string doc_id)
        {
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            dt = new DataTable();
            sqlstr = @"   select id, t_fxb_cur as currency_id,  round(t_fxb_value1,4) as exchange_rate
                        ,date_from
                        ,date_to 
                        from VW_BZ_FX_TYPE_M 
                        where  t_fxb_cur = 'USD'  
                        and (
                        to_number(t_fxb_valdate)  >= ( 
                            select min(to_number(to_char(to_date(allowance_date,'dd Mon rrrr'),'rrrrMMdd'))) as x 
                            from  bz_doc_allowance_detail where doc_id =:doc_id1  ) ";
            p.Add(new OracleParameter("doc_id1", doc_id));
            sqlstr += @" and 
                        to_number(t_fxb_valdate)  <= ( 
                            select  max(to_number(to_char(to_date(allowance_date,'dd Mon rrrr'),'rrrrMMdd')))  as  x
                            from  bz_doc_allowance_detail where doc_id =:doc_id2  ) ";
            p.Add(new OracleParameter("doc_id2", doc_id));
            sqlstr += @" )";
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                if (dt.Rows.Count == 0)
                {
                    dt = new DataTable();
                    dt = ref_exchangerate();
                }
            }

            return dt;
        }
        private DataTable ref_currency()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,sort_by
                        from VW_BZ_CURRENCY
                        order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable ref_m_insurance_plan()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,sort_by
                        from bz_master_insurance_plan
                        order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable ref_m_broker()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,sort_by
                        from bz_master_insurance_company
                        order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable ref_m_broker_isos()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,email,sort_by
                        from bz_master_insurance_company
                        where status_isos = 'true'
                        order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable ref_expense_type()
        {
            var dt = new DataTable();

            sqlstr = @" select id,name,sort_by
                        from BZ_MASTER_EXPENSE_TYPE
                        where status = 1 
                        order by to_number(sort_by)";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable ref_expense_type(string filleter_travelrecord)
        {
            var dt = new DataTable();

            sqlstr = @" select *
                        from BZ_MASTER_EXPENSE_TYPE
                        where status = 1  ";
            if (filleter_travelrecord != "") { sqlstr += @" and field_ws is not null"; }

            sqlstr += @" order by sort_by";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }
            return dt;
        }
        private DataTable ref_status(string page_name)
        {
            var dt = new DataTable();
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            sqlstr = @" select id,name,sort_by
                        from BZ_MASTER_LIST_STATUS
                        where status = 1  ";
            if (page_name != "")
            {
                sqlstr += @" and lower(page_name) = lower(:page_name) ";
                p.Add(new("page_name", page_name));
            }
            sqlstr += @" order by sort_by ";

            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
            }
            return dt;
        }

        private DataTable ref_m_continent()
        {
            var dt = new DataTable();

            sqlstr = @"  select ctn_id as id,ctn_name as name,0 as sort_by
                         from bz_master_continent
                         order by ctn_name";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["sort_by"] = (i + 1).ToString();
                    dt.AcceptChanges();
                }
            }
            return dt;
        }
        private DataTable ref_m_country()
        {
            var dt = new DataTable();

            sqlstr = @" select ct_id as id,ctn_id as id_main,ct_name as name,0 as sort_by
                         from bz_master_country
                         order by ctn_id,ct_name";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["sort_by"] = (i + 1).ToString();
                    dt.AcceptChanges();
                }
            }
            return dt;
        }
        private DataTable ref_m_province()
        {
            var dt = new DataTable();

            sqlstr = @" select pv_id as id,ct_id as id_main,pv_name as name,0 as sort_by
                         from bz_master_province
                         order by pv_id,pv_name";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["sort_by"] = (i + 1).ToString();
                    dt.AcceptChanges();
                }
            }
            return dt;
        }
        private DataTable ref_m_section()
        {
            var dt = new DataTable();

            sqlstr = @" select distinct b.sections as section, b.department, b.function ,0 as sort_by
                         from vw_bz_users b 
                         order by b.function, b.sections, b.department ";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["sort_by"] = (i + 1).ToString();
                    dt.AcceptChanges();
                }
            }
            return dt;
        }
        #endregion master data 

        #region  ref data

        public Boolean refdata_role_review(string token_login, string doc_id)
        {
            var ret = false;
            //var p = new List<OracleParameter>();
            //using var context = new TOPEBizCreateTripEntities();
            //  using var connection = context.Database.GetDbConnection();

            var parameters = new List<OracleParameter>();
            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            sqlstr = @"select count(1) as xcount from (
                     select dh_code as doc_id, a.dh_behalf_emp_id as emp_id
                     from  bz_doc_head a
                     union 
                     select dh_code as doc_id, a.dh_initiator_empid as emp_id
                     from  bz_doc_head a
                     union 
                     select dh_code as doc_id, a.dh_create_by as emp_id
                     from  bz_doc_head a
                     )t inner join  (select distinct user_id, token_code from bz_login_token) a on  t.emp_id = a.user_id
                     where t.doc_id = :doc_id and a.token_code = :token_code ";
                            parameters = new List<OracleParameter>();

                            parameters.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));
                            parameters.Add(context.ConvertTypeParameter("token_code", token_login, "char"));
                            try
                            {
                                var iret = context.Database.ExecuteSqlRaw(sqlstr, parameters.ToArray());
                                if ((iret >= -1)) { ret = true; } else { ret = false; }
                                ;
                            }
                            catch (Exception ex_Exec) { }



                            if (ret == true)
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

                            transaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            //try
            //{

            //    p.Add(context.ConvertTypeParameter("doc_id", doc_id, "v"));
            //    p.Add(context.ConvertTypeParameter("token_code", token_login, "v"));
            //    sqlstr = @"select count(1) as xcount from (
            //         select dh_code as doc_id, a.dh_behalf_emp_id as emp_id
            //         from  bz_doc_head a
            //         union 
            //         select dh_code as doc_id, a.dh_initiator_empid as emp_id
            //         from  bz_doc_head a
            //         union 
            //         select dh_code as doc_id, a.dh_create_by as emp_id
            //         from  bz_doc_head a
            //         )t inner join  (select distinct user_id, token_code from bz_login_token) a on  t.emp_id = a.user_id
            //         where t.doc_id = :doc_id and a.token_code = :token_code ";
            //    dt = new DataTable();

            //    DbCommand cmd = connection.CreateCommand();
            //    cmd.CommandText = sqlstr;
            //    cmd.Parameters.AddRange(p.ToArray());
            //    if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            //    {

            //        if (dt.Rows[0]["xcount"].ToString() == "0") { return false; } else { return true; }

            //    }
            //    context.Database.CloseConnection();
            //}
            //catch (System.Exception ex)
            //{

            //    if (context.Database.CanConnect())
            //    {
            //        context.Database.CloseConnection();
            //    }
            //    LoggerFile.write(ex);
            //}

            return ret;
        }
        public string refdata_pdpa_wording()
        {
            DataTable dtdetail = new DataTable();
            sqlstr = @" select key_value from BZ_CONFIG_DATA where status = 1 and lower(key_name) = lower('PASSPORT PDPA WORDING')";
            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                try { return dt.Rows[0]["key_value"].ToString(); } catch { }

            }
            return "";
        }
        public DataTable refdata_emp_doc(ref string emplist)
        {
            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();
            string[] emps = emplist.Split(",");
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            sqlstr = @"select to_char(b.employeeid) as emp_id
                    , nvl(b.entitle, '') as titlename
                    , nvl(b.enfirstname, '') as firstname
                    , nvl(b.enlastname, '') as lastname
                    , nvl(b.entitle, '')|| ' ' || b.enfirstname || ' ' || b.enlastname as userdisplay
                    , nvl(b.userid, '') as username
                    , nvl(b.orgname, '') as division
                    , nvl(b.email, '') as useremail
                    , 28 as age
                    , up.mobile as userphone
                    , up.telephone as usertel
                    , up.passportno as passportno
                    , up.dateofissue as dateofissue
                    , up.dateofbirth as dateofbirth
                    , up.dateofexpire as dateofexpire
                    , up.id  
                    , case when to_number(b.employeeid) < 1000000   then 
                        (case when up.imgpath is null then (replace(replace(b.imgurl,'.jpg', ''),b.employeeid, '')) else up.imgpath end)
                      else 
                        (case when up.imgpath is null then replace((replace(replace(b.imgurl,'.jpg', ''),b.employeeid, '')),'/TOP/','/TES/') else up.imgpath end)
                      end imgpath
                    , case when to_number(b.employeeid) < 1000000   then 
                        (case when up.imgprofilename is null then to_char(to_number(b.employeeid)) || '.jpg' else up.imgprofilename end)
                        else 
                        (case when up.imgprofilename is null then to_char((b.employeeid)) || '.jpg' else up.imgprofilename end)
                      end imgprofilename
                      , b.sections, b.department, b.function
                    from vw_bz_users b  
                    left join bz_user_peofile up on b.employeeid = up.employeeid ";
            if (emplist != "")
            {
                string placeholders = string.Join(",", emps.Select((_, i) => $":emplist{i}"));
                sqlstr += $" WHERE upper(b.employeeid) IN ({placeholders})";

                for (int i = 0; i < emps.Length; i++)
                {
                    p.Add(new OracleParameter($"emplist{i}", emps[i]));
                }

            }
            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
            }
            emplist = sqlstr;
            return dt;
        }
        public DataTable refdata_emp_detail(string token_login, string doc_id, string emp_id, ref Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";
            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();
            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            // sqlstr = sqlstr_data_emp_detail(token_login, doc_id, "");
            var cmd = SqlstrDataEmpDetailWithCommand(token_login, doc_id, "");
            sqlstr = cmd.CommandText;
            var oldparam = DbUtil.CopyOracleCommandParameters(cmd);
            List<OracleParameter> p = new();
            dt = new DataTable();

            //ws_conn.wsConnection conn = new ws_conn.wsConnection(); 

            DataTable dtcopy = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                var sqlstr_bus = @"select to_char(t2.date_min, 'DD MON YYYY') as date_min ,to_char(t2.date_max, 'DD MON YYYY') as date_max ,emp_id from (
                                    select min(DTE_BUS_FROMDATE) as date_min, max(DTE_BUS_TODATE) as date_max, doc_id, emp_id from (" + sqlstr + ")t group by doc_id, emp_id ";
                sqlstr_bus += "  )t2";


                var sqlstr_travel = @"select 
    to_char(
        case 
            when lower(t2.country_name) in ('canada', 'usa') then t2.date_min - 1
            else t2.date_min
        end, 'DD MON YYYY') as date_min,
    to_char(
        case 
            when lower(t2.country_name) in ('canada', 'usa') then t2.date_max + 1
            else t2.date_max
        end, 'DD MON YYYY') as date_max,
    emp_id,
    to_char(
        case 
            when lower(t2.country_name) in ('canada', 'usa') then t2.date_min - 1
            else t2.date_min
        end, 'YYYYMMDD') as sap_from_date,
    to_char(
        case 
            when lower(t2.country_name) in ('canada', 'usa') then t2.date_max + 1
            else t2.date_max
        end, 'YYYYMMDD') as sap_to_date
            from (
                select 
                    min(DTE_TRAVEL_FROMDATE) as date_min, 
                    max(DTE_TRAVEL_TODATE) as date_max, 
                    doc_id, 
                    emp_id, 
                    min(country_name) as country_name 
                from (" + sqlstr + @")t 
                group by doc_id, emp_id ";
                sqlstr_travel += "  )t2";


                var sqlstr_country = @" select distinct row_id, def_location_id, country_name as country_text, doc_id, emp_id from (" + sqlstr + ")t order by row_id, doc_id, emp_id ";
                if (doc_id.ToUpper().IndexOf("L") > -1)
                {
                    sqlstr_country = @" select distinct row_id, def_location_id, province as country_text, doc_id, emp_id from (" + sqlstr + ")t order by row_id, doc_id, emp_id ";
                }
                var sqlstr_city_text = @" select distinct row_id, country_name as country_text, city as city_text, doc_id, emp_id from (" + sqlstr + ")t order by row_id, doc_id, emp_id ";
                if (doc_id.ToUpper().IndexOf("L") > -1)
                {
                    sqlstr_city_text = @" select distinct row_id, province as country_text, city as city_text, doc_id, emp_id from (" + sqlstr + ")t order by row_id, doc_id, emp_id ";
                }

                var sqlstrcancelled = @"select distinct nvl(status_trip_cancelled,'false') as status_trip_cancelled, doc_id from bz_doc_travelexpense  where 1=1 ";
                if (doc_id == "personal")
                { sqlstrcancelled += @" and emp_id in ( select distinct a.user_id  from bz_login_token a where a.token_code = :token_login)"; p.Add(new("token_login", token_login)); }
                else
                {
                    sqlstrcancelled += @" and doc_id = :doc_id ";
                    p.Add(new("doc_id", doc_id));
                }

                DataTable dtbusdate = new DataTable();
                DataTable dttraveldate = new DataTable();
                DataTable dtcountry = new DataTable();
                DataTable dtcity = new DataTable();
                DataTable dttripcancelled = new DataTable();
                cmd = new(sqlstr_bus);
                cmd.Parameters.AddRange(oldparam.ToArray());
                SetDocService.conn_ExecuteData(ref dtbusdate, cmd);
                cmd = new(sqlstr_travel);
                cmd.Parameters.AddRange(oldparam.ToArray());
                SetDocService.conn_ExecuteData(ref dttraveldate, cmd);
                cmd = new(sqlstr_country);
                cmd.Parameters.AddRange(oldparam.ToArray());
                SetDocService.conn_ExecuteData(ref dtcountry, cmd);
                cmd = new(sqlstr_city_text);
                cmd.Parameters.AddRange(oldparam.ToArray());
                SetDocService.conn_ExecuteData(ref dtcity, cmd);
                cmd = new(sqlstrcancelled);
                cmd.Parameters.AddRange(p.ToArray());
                SetDocService.conn_ExecuteData(ref dttripcancelled, cmd);

                dtcopy = dt.Clone(); dtcopy.AcceptChanges();
                //dtcopy = dt.Copy(); dtcopy.AcceptChanges();
                //กรณีที่ traverler 1 คนมีมากกว่า 1 รายการให้ ตัดเหลือ 1รายการ 
                //และแก้หาช่วงวัน business_date, travel_date 
                //เอาข้อมูล มาต่อกัน มี country_id, country_name, continent, city, country, country_city 


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string doc_id_def = dt.Rows[i]["doc_id"].ToString();
                    string emp_id_def = dt.Rows[i]["emp_id"].ToString();
                    // DataRow[] dr = dtcopy.Select("emp_id = '" + emp_id_def + "'");
                    DataRow[] dr = dtcopy.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_def).ToArray();

                    if (dr.Length > 0)
                    {
                    }
                    else
                    {
                        try
                        {
                            // dr = dttripcancelled.Select("doc_id='" + doc_id_def + "'");
                            dr = dttripcancelled.AsEnumerable().Where(s => s.Field<string>("doc_id") == doc_id_def).ToArray();
                            if (dr.Length > 0)
                            {
                                dt.Rows[i]["status_trip_cancelled"] = dr[0]["status_trip_cancelled"].ToString();
                            }
                        }
                        catch { }

                        string sap_from_date = "";
                        string sap_to_date = "";

                        string business_date = dt.Rows[i]["business_date"].ToString();
                        string travel_date = dt.Rows[i]["travel_date"].ToString();
                        string country_city = dt.Rows[i]["country_city"].ToString();
                        if (dtbusdate.Rows.Count > 0)
                        {
                            // DataRow[] drselect = dtbusdate.Select("emp_id = '" + emp_id_def + "'");
                            DataRow[] drselect = dtbusdate.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_def).ToArray();

                            if (drselect.Length > 0) { business_date = drselect[0]["date_min"].ToString() + "-" + drselect[0]["date_max"].ToString(); }
                        }
                        if (dttraveldate.Rows.Count > 0)
                        {
                            // DataRow[] drselect = dttraveldate.Select("emp_id = '" + emp_id_def + "'");
                            DataRow[] drselect = dttraveldate.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_def).ToArray();

                            if (drselect.Length > 0)
                            {
                                travel_date = drselect[0]["date_min"].ToString() + "-" + drselect[0]["date_max"].ToString();

                                //travelexpen_date
                                sap_from_date = drselect[0]["sap_from_date"].ToString();
                                sap_to_date = drselect[0]["sap_to_date"].ToString();
                            }
                        }
                        var country_text = "";
                        var location_text = "";//ใช้ใน SAP
                        var city_text = "";
                        if (dtcountry.Rows.Count > 0)
                        {
                            // DataRow[] drcountry = dtcountry.Select("emp_id = '" + emp_id_def + "'");
                            DataRow[] drcountry = dtcountry.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_def).ToArray();

                            for (int c = 0; c < drcountry.Length; c++)
                            {
                                city_text = "";
                                if (country_text != "") { country_text += ", "; }
                                // DataRow[] drcity = dtcity.Select("emp_id = '" + emp_id_def + "' and country_text = '" + drcountry[c]["country_text"].ToString() + "' ");
                                DataRow[] drcity = dtcity.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_def && s.Field<string>("country_text") == drcountry[c]["country_text"].ToString()).ToArray();

                                if (drcity.Length > 0)
                                {
                                    for (int ct = 0; ct < drcity.Length; ct++)
                                    {
                                        if (city_text != "") { city_text += ","; }
                                        city_text += drcity[ct]["city_text"].ToString() + "";
                                    }
                                    if (city_text != "") { country_text += drcountry[c]["country_text"].ToString() + " / " + city_text; }
                                }
                                if (location_text != "") { location_text += ","; }
                                location_text += drcountry[c]["def_location_id"].ToString();
                            }
                            country_city = country_text;
                        }
                        dt.Rows[i]["business_date"] = business_date;
                        dt.Rows[i]["travel_date"] = travel_date;
                        dt.Rows[i]["dateOfDeparture"] = travel_date;
                        dt.Rows[i]["country_city"] = country_city;
                        dt.Rows[i]["def_location_id"] = location_text;

                        dt.Rows[i]["sap_from_date"] = sap_from_date;
                        dt.Rows[i]["sap_to_date"] = sap_to_date;

                        dtcopy.ImportRow(dt.Rows[i]);
                        dtcopy.AcceptChanges();
                    }
                }
                dt = new DataTable();
                dt = dtcopy.Copy(); dt.AcceptChanges();
            }
            return dt;
        }
        public string sqlstr_data_emp_detail(string token_login, string doc_id, string other_case)
        {
            //ข้อมูลส่วนบุคคลไม่ต้องเก็บ doc_id --> bz_user_peofile.doc_id
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            if (doc_id == "personal")
            {
                sqlstr = @"  select 'personal' as doc_id, 'false' as status_trip_cancelled
                             , null as travel_topic
                             , null as travel_topic_sub
                             , null as business_date
                             , null as travel_date
                             , null as emp_name  
                             , null as emp_organization
                             , null as continent_id
                             , null as country_id
                             , null as continent
                             , null as country_name 
                             , null as city  
                             , null as country
                             , null as country_city  
                            , 'plane' as icon
                            , null as datefrom
                            , null as dateto 
                            , null as DTE_BUS_FROMDATE, null as DTE_BUS_TODATE, null as DTE_TRAVEL_FROMDATE, null as DTE_TRAVEL_TODATE
                            , null as gl_account,null as cost_center,null as io_wbs
                            , nvl(case when b.orgname is null then  b.units else b.orgname end , '') as org_unit
                            , case when  udp.emp_id is null then nvl(b.entitle, '') else udp.passport_title end titlename
                            , case when  udp.emp_id is null then nvl(b.enfirstname, '') else udp.passport_name end firstname
                            , case when  udp.emp_id is null then nvl(b.enlastname, '') else udp.passport_surname end lastname
                            , nvl(b.entitle, '')|| ' ' || b.enfirstname || ' ' || b.enlastname as userdisplay
                            , nvl(b.userid, '') as username
                            , nvl(b.orgname, '') as division
                            , nvl(b.email, '') as useremail 
                            , 28 as age
                            , up.mobile as userphone
                            , up.telephone as usertel
                            , udp.passport_no as passportno
                            , udp.passport_date_issue as dateofissue
                            , udp.passport_date_birth as dateofbirth
                            , udp.passport_date_expire as dateofexpire
                            , up.id   
                            , case when to_number(b.employeeid) < 1000000   then 
                                (case when up.imgpath is null then replace(replace(replace(b.imgurl,'.jpg', ''),b.employeeid, ''),to_number(b.employeeid),'') else up.imgpath end)
                              else 
                                (case when up.imgpath is null then replace((replace(replace(b.imgurl,'.jpg', ''),b.employeeid, '')),'/TOP/','/TES/') else up.imgpath end)
                              end imgpath
                            , case when to_number(b.employeeid) < 1000000   then 
                                (case when up.imgprofilename is null then to_char(to_number(b.employeeid)) || '.jpg' else up.imgprofilename end)
                                else 
                                (case when up.imgprofilename is null then to_char((b.employeeid)) || '.jpg' else up.imgprofilename end)
                              end imgprofilename
                            , p.passport_img 
                            , p.passport_img_name 
                            , b.email
                            , b.sections, b.department, b.function
                            , b.companyname as company, b.objenfullname as position, case when b.gender is null then (case when b.entitle in ('Mr.') then 'Male' else 'Female' end) else b.gender end gender
                            , b.joindate, null as dateOfDeparture
                            , b.employeeid as emp_id
                            , '' as province
                            , '' as sap_from_date, '' as sap_to_date
                            , '' as def_location_id
                            from vw_bz_users b 
                            left join bz_user_peofile up on b.employeeid = up.employeeid 
                            left join bz_data_passport udp on b.employeeid = udp.emp_id and udp.default_type = 'true' 
                            left join ( 
                            select doc_id,emp_id as employeeid,path || file_name as passport_img,file_name as passport_img_name
                            from  bz_doc_img where page_name = 'passport' and status = 1
                            and (emp_id,id_level_1) in (select emp_id,id from bz_data_passport where default_type = 'true' )  
                            ) p on b.employeeid = p.employeeid  
                            where b.employeeid in ( select distinct a.user_id  from bz_login_token a where a.token_code = '" + token_login + "') ";

                sqlstr += @" order by b.employeeid ";
            }
            else
            {
                sqlstr = @"
                     select te.DTE_ID as row_id, h.dh_code as doc_id, 'false' as status_trip_cancelled
                     , h.dh_topic as travel_topic
                     , h.dh_topic as travel_topic_sub
                     , case when te.DTE_BUS_FROMDATE is null then '' else to_char(te.DTE_BUS_FROMDATE, 'DD MON YYYY') end ||' - '|| case when te.DTE_BUS_TODATE is null then '' else to_char(te.DTE_BUS_TODATE, 'DD MON YYYY') end business_date
                     , case when te.DTE_TRAVEL_FROMDATE is null then '' else to_char(te.DTE_TRAVEL_FROMDATE, 'DD MON YYYY') end ||' - '|| case when te.DTE_TRAVEL_TODATE is null then '' else to_char(te.DTE_TRAVEL_TODATE, 'DD MON YYYY') end travel_date
                     , te.DTE_EMP_ID emp_id, nvl(b.ENTITLE, '')|| ' ' || b.ENFIRSTNAME || ' ' || b.ENLASTNAME emp_name  
                    , b.ORGNAME emp_organization
                    , te.CTN_ID as continent_id
                    , te.CT_ID as country_id
                    , c.CTN_NAME continent
                    , d.CT_NAME country_name 
                    , te.CITY_TEXT city  
                    , d.CT_NAME country
                    , case when te.CITY_TEXT  = null then d.CT_NAME else (case when d.CT_NAME  = null then te.CITY_TEXT else d.CT_NAME || (case when te.CITY_TEXT is not null then '/'|| te.CITY_TEXT end ) end ) end country_city 

                    , 'plane' as icon
                    , to_char(te.DTE_TRAVEL_FROMDATE, 'DD MON YYYY') as datefrom
                    , to_char(te.DTE_TRAVEL_TODATE, 'DD MON YYYY') as dateto
                    , te.DTE_BUS_FROMDATE, te.DTE_BUS_TODATE, te.DTE_TRAVEL_FROMDATE, te.DTE_TRAVEL_TODATE

                    , nvl(case when b.orgname is null then  b.units else b.orgname end , '') as org_unit
                            , case when  udp.emp_id is null then nvl(b.entitle, '') else udp.passport_title end titlename
                            , case when  udp.emp_id is null then nvl(b.enfirstname, '') else udp.passport_name end firstname
                            , case when  udp.emp_id is null then nvl(b.enlastname, '') else udp.passport_surname end lastname
                    , nvl(b.entitle, '')|| ' ' || b.enfirstname || ' ' || b.enlastname as userdisplay
                    , nvl(b.userid, '') as username
                    , nvl(b.orgname, '') as division
                    , nvl(b.email, '') as useremail
                    , 28 as age
                    , up.mobile as userphone
                    , up.telephone as usertel
                    , udp.passport_no as passportno
                    , udp.passport_date_issue as dateofissue
                    , udp.passport_date_birth as dateofbirth
                    , udp.passport_date_expire as dateofexpire
                    , up.id  
                    , case when to_number(b.employeeid) < 1000000   then 
                        (case when up.imgpath is null then replace(replace(replace(b.imgurl,'.jpg', ''),b.employeeid, ''),to_number(b.employeeid),'') else up.imgpath end)
                      else 
                        (case when up.imgpath is null then replace((replace(replace(b.imgurl,'.jpg', ''),b.employeeid, '')),'/TOP/','/TES/') else up.imgpath end)
                      end imgpath
                    , case when to_number(b.employeeid) < 1000000   then 
                        (case when up.imgprofilename is null then to_char(to_number(b.employeeid)) || '.jpg' else up.imgprofilename end)
                        else 
                        (case when up.imgprofilename is null then to_char((b.employeeid)) || '.jpg' else up.imgprofilename end)
                      end imgprofilename
                    , p.passport_img 
                    , p.passport_img_name 

                    ,te.dte_gl_account as gl_account,te.dte_cost_center as cost_center,te.dte_order_wbs as io_wbs
                    ,b.email
                    , b.sections, b.department, b.function
                    , b.companyname as company, b.objenfullname as position, case when b.gender is null then (case when b.entitle in ('Mr.') then 'Male' else 'Female' end) else b.gender end gender
                    , b.joindate, null as dateOfDeparture
                    , pv.pv_name as province
                    , '' as sap_from_date, '' as sap_to_date
                    , case when h.dh_type = 'local' then to_char(pv.pv_id) else to_char(te.CT_ID) end def_location_id

                    from bz_doc_head h
                    inner join bz_doc_traveler_expense te on h.dh_code = te.dh_code  
                    inner join vw_bz_users b on te.DTE_EMP_ID = b.employeeid  
                    left join bz_user_peofile up on te.dte_emp_id = up.employeeid 
                    left join bz_data_passport udp on te.dte_emp_id = udp.emp_id and udp.default_type = 'true'
                    
                    left join (

                    select doc_id,emp_id as employeeid,path || file_name as passport_img,file_name as passport_img_name
                    from  bz_doc_img where page_name = 'passport' and status = 1
                    and (emp_id,id_level_1) in (select emp_id,id from bz_data_passport where default_type = 'true' ) 
                    
                    ) p on te.dte_emp_id = p.employeeid and (h.dh_code = p.doc_id  or 1=1)
                    left join BZ_MASTER_CONTINENT c on te.CTN_ID = c.CTN_ID  
                    left join BZ_MASTER_COUNTRY d on te.CT_ID = d.CT_ID  
                    left join bz_master_province pv on te.pv_id = pv.pv_id ";

                if (other_case == "ListEmployeeTravelInsurance" || other_case == "ListEmployeeISOSMemberList")
                {
                    //ข้อมูลที่จะส่งไป แจ้งเตือนพนักงานเพื่อให้กรอกคำขอเอาประกันภัยการเดินทางต่างประเทศ
                    sqlstr += @" inner join (
                                         select distinct ta.dh_code, ta.dta_appr_empid as approverid, ta.dta_travel_empid as employeeid
                                         from  bz_doc_traveler_approver ta 
                                         where lower(ta.dta_remark) = lower('CAP') and  ta.dta_doc_status = 42 and  lower(ta.dh_code) like lower('OB%') 
                              )ta   on te.dte_emp_id = ta.employeeid and h.dh_code = ta.dh_code  ";
                }
                sqlstr += @" where te.DH_CODE = '" + doc_id + "' ";
                //แสดงเฉพาะคนที่ไม่ถูก reject
                sqlstr += @" and (te.DTE_EMP_ID, te.DH_CODE) in ( 
                             select dte_emp_id, dh_code from bz_doc_traveler_expense
                             where (nvl(dte_appr_status,1) not in (30) or nvl(dte_cap_appr_status,1) not in (40))
                             and nvl(dte_cap_appr_status,1) not in (40)
                             and (nvl(dte_appr_opt,'true') = 'true' and nvl(dte_cap_appr_opt,'true') = 'true'))";

                sqlstr += @" order by te.DTE_ID";
            }

            return sqlstr;
        }
        public OracleCommand SqlstrDataEmpDetailWithCommand(string token_login, string doc_id, string other_case)
        {
            OracleCommand cmd;
            List<OracleParameter> p = new();
            //ข้อมูลส่วนบุคคลไม่ต้องเก็บ doc_id --> bz_user_peofile.doc_id
            if (doc_id == "personal")
            {
                sqlstr = @"  select 'personal' as doc_id, 'false' as status_trip_cancelled
                             , null as travel_topic
                             , null as travel_topic_sub
                             , null as business_date
                             , null as travel_date
                             , null as emp_name  
                             , null as emp_organization
                             , null as continent_id
                             , null as country_id
                             , null as continent
                             , null as country_name 
                             , null as city  
                             , null as country
                             , null as country_city  
                            , 'plane' as icon
                            , null as datefrom
                            , null as dateto 
                            , null as DTE_BUS_FROMDATE, null as DTE_BUS_TODATE, null as DTE_TRAVEL_FROMDATE, null as DTE_TRAVEL_TODATE
                            , null as gl_account,null as cost_center,null as io_wbs
                            , nvl(case when b.orgname is null then  b.units else b.orgname end , '') as org_unit
                            , case when  udp.emp_id is null then nvl(b.entitle, '') else udp.passport_title end titlename
                            , case when  udp.emp_id is null then nvl(b.enfirstname, '') else udp.passport_name end firstname
                            , case when  udp.emp_id is null then nvl(b.enlastname, '') else udp.passport_surname end lastname
                            , nvl(b.entitle, '')|| ' ' || b.enfirstname || ' ' || b.enlastname as userdisplay
                            , nvl(b.userid, '') as username
                            , nvl(b.orgname, '') as division
                            , nvl(b.email, '') as useremail 
                            , 28 as age
                            , up.mobile as userphone
                            , up.telephone as usertel
                            , udp.passport_no as passportno
                            , udp.passport_date_issue as dateofissue
                            , udp.passport_date_birth as dateofbirth
                            , udp.passport_date_expire as dateofexpire
                            , up.id   
                            , case when to_number(b.employeeid) < 1000000   then 
                                (case when up.imgpath is null then replace(replace(replace(b.imgurl,'.jpg', ''),b.employeeid, ''),to_number(b.employeeid),'') else up.imgpath end)
                              else 
                                (case when up.imgpath is null then replace((replace(replace(b.imgurl,'.jpg', ''),b.employeeid, '')),'/TOP/','/TES/') else up.imgpath end)
                              end imgpath
                            , case when to_number(b.employeeid) < 1000000   then 
                                (case when up.imgprofilename is null then to_char(to_number(b.employeeid)) || '.jpg' else up.imgprofilename end)
                                else 
                                (case when up.imgprofilename is null then to_char((b.employeeid)) || '.jpg' else up.imgprofilename end)
                              end imgprofilename
                            , p.passport_img 
                            , p.passport_img_name 
                            , b.email
                            , b.sections, b.department, b.function
                            , b.companyname as company, b.objenfullname as position, case when b.gender is null then (case when b.entitle in ('Mr.') then 'Male' else 'Female' end) else b.gender end gender
                            , b.joindate, null as dateOfDeparture
                            , b.employeeid as emp_id
                            , '' as province
                            , '' as sap_from_date, '' as sap_to_date
                            , '' as def_location_id
                            from vw_bz_users b 
                            left join bz_user_peofile up on b.employeeid = up.employeeid 
                            left join bz_data_passport udp on b.employeeid = udp.emp_id and udp.default_type = 'true' 
                            left join ( 
                            select doc_id,emp_id as employeeid,path || file_name as passport_img,file_name as passport_img_name
                            from  bz_doc_img where page_name = 'passport' and status = 1
                            and (emp_id,id_level_1) in (select emp_id,id from bz_data_passport where default_type = 'true' )  
                            ) p on b.employeeid = p.employeeid  
                            where b.employeeid in ( select distinct a.user_id  from bz_login_token a where a.token_code = :token_login) ";
                p.Add(new("token_login", token_login));
                sqlstr += @" order by b.employeeid ";
            }
            else
            {
                sqlstr = @"
                     select te.DTE_ID as row_id, h.dh_code as doc_id, 'false' as status_trip_cancelled
                     , h.dh_topic as travel_topic
                     , h.dh_topic as travel_topic_sub
                     , case when te.DTE_BUS_FROMDATE is null then '' else to_char(te.DTE_BUS_FROMDATE, 'DD MON YYYY') end ||' - '|| case when te.DTE_BUS_TODATE is null then '' else to_char(te.DTE_BUS_TODATE, 'DD MON YYYY') end business_date
                     , case when te.DTE_TRAVEL_FROMDATE is null then '' else to_char(te.DTE_TRAVEL_FROMDATE, 'DD MON YYYY') end ||' - '|| case when te.DTE_TRAVEL_TODATE is null then '' else to_char(te.DTE_TRAVEL_TODATE, 'DD MON YYYY') end travel_date
                     , te.DTE_EMP_ID emp_id, nvl(b.ENTITLE, '')|| ' ' || b.ENFIRSTNAME || ' ' || b.ENLASTNAME emp_name  
                    , b.ORGNAME emp_organization
                    , te.CTN_ID as continent_id
                    , te.CT_ID as country_id
                    , c.CTN_NAME continent
                    , d.CT_NAME country_name 
                    , te.CITY_TEXT city  
                    , d.CT_NAME country
                    , case when te.CITY_TEXT  = null then d.CT_NAME else (case when d.CT_NAME  = null then te.CITY_TEXT else d.CT_NAME || (case when te.CITY_TEXT is not null then '/'|| te.CITY_TEXT end ) end ) end country_city 

                    , 'plane' as icon
                    , to_char(te.DTE_TRAVEL_FROMDATE, 'DD MON YYYY') as datefrom
                    , to_char(te.DTE_TRAVEL_TODATE, 'DD MON YYYY') as dateto
                    , te.DTE_BUS_FROMDATE, te.DTE_BUS_TODATE, te.DTE_TRAVEL_FROMDATE, te.DTE_TRAVEL_TODATE

                    , nvl(case when b.orgname is null then  b.units else b.orgname end , '') as org_unit
                            , case when  udp.emp_id is null then nvl(b.entitle, '') else udp.passport_title end titlename
                            , case when  udp.emp_id is null then nvl(b.enfirstname, '') else udp.passport_name end firstname
                            , case when  udp.emp_id is null then nvl(b.enlastname, '') else udp.passport_surname end lastname
                    , nvl(b.entitle, '')|| ' ' || b.enfirstname || ' ' || b.enlastname as userdisplay
                    , nvl(b.userid, '') as username
                    , nvl(b.orgname, '') as division
                    , nvl(b.email, '') as useremail
                    , 28 as age
                    , up.mobile as userphone
                    , up.telephone as usertel
                    , udp.passport_no as passportno
                    , udp.passport_date_issue as dateofissue
                    , udp.passport_date_birth as dateofbirth
                    , udp.passport_date_expire as dateofexpire
                    , up.id  
                    , case when to_number(b.employeeid) < 1000000   then 
                        (case when up.imgpath is null then replace(replace(replace(b.imgurl,'.jpg', ''),b.employeeid, ''),to_number(b.employeeid),'') else up.imgpath end)
                      else 
                        (case when up.imgpath is null then replace((replace(replace(b.imgurl,'.jpg', ''),b.employeeid, '')),'/TOP/','/TES/') else up.imgpath end)
                      end imgpath
                    , case when to_number(b.employeeid) < 1000000   then 
                        (case when up.imgprofilename is null then to_char(to_number(b.employeeid)) || '.jpg' else up.imgprofilename end)
                        else 
                        (case when up.imgprofilename is null then to_char((b.employeeid)) || '.jpg' else up.imgprofilename end)
                      end imgprofilename
                    , p.passport_img 
                    , p.passport_img_name 

                    ,te.dte_gl_account as gl_account,te.dte_cost_center as cost_center,te.dte_order_wbs as io_wbs
                    ,b.email
                    , b.sections, b.department, b.function
                    , b.companyname as company, b.objenfullname as position, case when b.gender is null then (case when b.entitle in ('Mr.') then 'Male' else 'Female' end) else b.gender end gender
                    , b.joindate, null as dateOfDeparture
                    , pv.pv_name as province
                    , '' as sap_from_date, '' as sap_to_date
                    , case when h.dh_type = 'local' then to_char(pv.pv_id) else to_char(te.CT_ID) end def_location_id

                    from bz_doc_head h
                    inner join bz_doc_traveler_expense te on h.dh_code = te.dh_code  
                    inner join vw_bz_users b on te.DTE_EMP_ID = b.employeeid  
                    left join bz_user_peofile up on te.dte_emp_id = up.employeeid 
                    left join bz_data_passport udp on te.dte_emp_id = udp.emp_id and udp.default_type = 'true'
                    
                    left join (

                    select doc_id,emp_id as employeeid,path || file_name as passport_img,file_name as passport_img_name
                    from  bz_doc_img where page_name = 'passport' and status = 1
                    and (emp_id,id_level_1) in (select emp_id,id from bz_data_passport where default_type = 'true' ) 
                    
                    ) p on te.dte_emp_id = p.employeeid and (h.dh_code = p.doc_id  or 1=1)
                    left join BZ_MASTER_CONTINENT c on te.CTN_ID = c.CTN_ID  
                    left join BZ_MASTER_COUNTRY d on te.CT_ID = d.CT_ID  
                    left join bz_master_province pv on te.pv_id = pv.pv_id ";

                if (other_case == "ListEmployeeTravelInsurance" || other_case == "ListEmployeeISOSMemberList")
                {
                    //ข้อมูลที่จะส่งไป แจ้งเตือนพนักงานเพื่อให้กรอกคำขอเอาประกันภัยการเดินทางต่างประเทศ
                    sqlstr += @" inner join (
                                         select distinct ta.dh_code, ta.dta_appr_empid as approverid, ta.dta_travel_empid as employeeid
                                         from  bz_doc_traveler_approver ta 
                                         where lower(ta.dta_remark) = lower('CAP') and  ta.dta_doc_status = 42 and  lower(ta.dh_code) like lower('OB%') 
                              )ta   on te.dte_emp_id = ta.employeeid and h.dh_code = ta.dh_code  ";
                }
                sqlstr += @" where te.DH_CODE = :doc_id ";
                p.Add(new("doc_id", doc_id));

                //แสดงเฉพาะคนที่ไม่ถูก reject
                sqlstr += @" and (te.DTE_EMP_ID, te.DH_CODE) in ( 
                             select dte_emp_id, dh_code from bz_doc_traveler_expense
                             where (nvl(dte_appr_status,1) not in (30) or nvl(dte_cap_appr_status,1) not in (40))
                             and nvl(dte_cap_appr_status,1) not in (40)
                             and (nvl(dte_appr_opt,'true') = 'true' and nvl(dte_cap_appr_opt,'true') = 'true'))";

                sqlstr += @" order by te.DTE_ID";
            }
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            return cmd;
        }


        public DataTable refdata_emp_visa_traveler(string token_login, string doc_id)
        {
            OracleCommand cmd;
            List<OracleParameter> p = new();
            if (doc_id == "personal")
            {
                sqlstr = @"select distinct v.id, ct.ct_name as nationality, v.visa_valid_until as valid_unit, v.visa_type, v.visa_entry
                          , vimg.full_path, vimg.file_name, v.emp_id as employeeid , v.emp_id
                          from bz_data_visa v
                          left join  ( 
                                select doc_id,emp_id,id_level_1,path || file_name as full_path,file_name 
                                from  bz_doc_img where page_name = 'visa' and status = 1 and action_name = 'visa_page' and nvl(active_type,'true') = 'true'
                                and (emp_id,id_level_1) in (select emp_id,id from bz_data_visa )
                            )vimg on v.emp_id = vimg.emp_id and v.id = vimg.id_level_1
                        left join  bz_master_country ct on v.visa_nationality = ct.ct_id
                            where   v.emp_id in (select distinct a.user_id  from bz_login_token a where a.token_code =  :token_login) ";
                p.Add(new("token_login", token_login));

                sqlstr += @"order by v.id";
            }
            else
            {
                sqlstr = @"select distinct v.id, ct.ct_name as nationality, v.visa_valid_until as valid_unit, v.visa_type, v.visa_entry
                      , vimg.full_path, vimg.file_name, v.emp_id as employeeid , v.emp_id
                      from bz_data_visa v
                      left join  ( 
                            select doc_id,emp_id,id_level_1,path || file_name as full_path,file_name 
                            from  bz_doc_img where page_name = 'visa' and status = 1 and action_name = 'visa_page'
                            and (emp_id,id_level_1) in (select emp_id,id from bz_data_visa )
                        )vimg on v.emp_id = vimg.emp_id and v.id = vimg.id_level_1
                        left join  bz_master_country ct on v.visa_nationality = ct.ct_id
                        where v.emp_id in (select distinct dte_emp_id from  bz_doc_traveler_expense te";

                sqlstr += @"  where te.dh_code =:doc_id   ";
                p.Add(new("doc_id", doc_id));

                sqlstr += @" ) ";

                sqlstr += @"order by v.id";
            }
            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
            }

            return dt;
        }
        public DataTable refdata_visa_docountries(string continent_id, string country_id, ref DataTable dtdesc)
        {
            string sqlstr_def = "";
            OracleCommand cmd;
            List<OracleParameter> p = new();
            sqlstr = @"select distinct a.id 
                        , a.id_level_1 as continent_id, a.id_level_2 as country_id
                        , a.path
                        , a.file_name as filename
                        , a.page_name as pagename 
                        , a.path || a.file_name as fullname   
                        , b.name as docountries_name
                         from bz_doc_img a
                         inner join bz_master_visa_docountries b on a.id_level_1 = b.continent_id and  a.id_level_2 = b.country_id
                         where a.status = 1
                         and lower(a.page_name) = lower('mtvisacountries')
                          and a.id_level_1 = :continent_id and a.id_level_2 = :country_id  ";
            p.Add(new("continent_id", continent_id));
            p.Add(new("country_id", country_id));

            sqlstr += @"  order by a.id ";

            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                p.Clear();
                if (dt.Rows.Count == 0)
                {

                    sqlstr = @"select distinct a.id 
                        , a.id_level_1 as continent_id, a.id_level_2 as country_id
                        , a.path
                        , a.file_name as filename
                        , a.page_name as pagename 
                        , a.path || a.file_name as fullname   
                        , b.name as docountries_name
                         from bz_doc_img a
                         inner join bz_master_visa_docountries b  on a.id_level_1 = b.continent_id and b.country_id is null
                         where a.status = 1
                         and lower(a.page_name) = lower('mtvisacountries')
                          and a.id_level_1 = :continent_id and a.id_level_2 is null  ";
                    p.Add(new("continent_id", continent_id));

                    sqlstr += @"  order by a.id ";

                    dt = new DataTable();

                    cmd = new(sqlstr);
                    cmd.Parameters.AddRange(p.ToArray());
                    if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
                    {
                        if (dt.Rows.Count == 0)
                        {
                        }
                    }
                }
            }
            p.Clear();
            sqlstr = @" select distinct c.id, c.description as docountries_name, c.preparing_by
                         from bz_master_visa_docountries b  
                         inner join bz_master_visa_document c on b.visa_doc_id = c.id
                         where b.continent_id = :continent_id and b.country_id = :country_id order by c.id";
            p.Add(new("continent_id", continent_id));
            p.Add(new("country_id", country_id));

            dtdesc = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dtdesc, cmd) == "")
            {
                p.Clear();

                if (dtdesc.Rows.Count == 0)
                {
                    sqlstr = @" select distinct c.id, c.description as docountries_name, c.preparing_by
                         from bz_master_visa_docountries b  
                         inner join bz_master_visa_document c on b.visa_doc_id = c.id
                         where b.continent_id = :continent_id and b.country_id is null order by c.id";
                    p.Add(new("continent_id", continent_id));

                    dtdesc = new DataTable();
                    cmd = new(sqlstr);
                    cmd.Parameters.AddRange(p.ToArray());
                    if (SetDocService.conn_ExecuteData(ref dtdesc, cmd) == "")
                    {
                    }

                }
            }

            return dt;
        }

        public DataTable refdata_content_html(string token_login, string doc_id, string emp_id, string page_name, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";
            OracleCommand cmd;
            List<OracleParameter> p = new();
            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_CONTENT);

            sqlstr = @"
                    select a.id
                    , a.content_path
                    , a.content_name
                    , a.content_path || a.content_name as fullname
                    , '' as html_content
                    , '' as video_url
                    , a.remark  
                    , a.page_name
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                    , nvl(a.doc_status,1) as doc_status
                    
                    from bz_data_content a
                    where lower(page_name) = lower(:page_name) order by a.id";
            p.Add(new("page_name", page_name));

            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                p.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }

        private DataTable refdata_Transportation(string token_login, string doc_id, string emp_id)
        {
            var dt = new DataTable();

            //using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
            //{
            string sqlstrCar = "";
            string sqlstrDef = "";

            //กรณีที่ connection db carservice
            sqlstrCar = $@" SELECT DISTINCT A.ID
                        , A.ITEM_NO
                        , WF.DOC_NO AS CARS_DOC_NO
                        , CASE WHEN B.ROUTE IS NULL THEN 0 ELSE B.ROUTE END AS ROUTE
                        , D.NAME AS CAR_TYPE
                        , A.BUSINESS_DOC_NO
                        , E.UMS_ID
                        , E.UMS_USERNAME
                        , F.TEL_NO
                        , TO_CHAR(A.USE_DATE_FROM,'DD/MM/YYYY') AS USE_DATE_FROM
                        , TO_CHAR(A.USE_DATE_TO,'DD/MM/YYYY') AS USE_DATE_TO
                        , A.USE_TIME_FROM
                        , A.USE_TIME_TO
                        , TO_CHAR(A.USE_DATE_FROM, 'DD Mon YYYY') || ' - ' || TO_CHAR(A.USE_DATE_TO, 'DD Mon YYYY') AS USE_DATE_STR
                        , A.LOCATION_FROM
                        , A.LOCATON_FROM_URL as LOCATION_FROM_URL
                        , A.LOCATON_TO as LOCATION_TO
                        , A.LOCATON_TO_URL as LOCATION_TO_URL
                        , CASE A.TRANSPORT_TYPE
                            WHEN 1 THEN 'ส่งอย่างเดียว'
                            WHEN 2 THEN 'รับอย่างเดียว'
                            WHEN 3 THEN 'ส่งและรอรับกลับ'
                          END AS TRANSPORT_TYPE 
                        , A.TRANSPORT_DETAIL
                        , A.ORDER_BY
                        , H.NAME AS CAR_COME_NAME
                        , G.DRIVER_NAME
                        , G.CAR_MODEL
                        , G.CAR_COLOR
                        , G.VEHICLE_REGISTRATION
                        , G.TEL_NUMBER AS DRIVER_TEL  
                        FROM CARS_DETAIL_BOOKING@TRANSPTPRD  A
                        LEFT JOIN CARS_ROUTE@TRANSPTPRD  B ON A.ID = B.ID AND A.ITEM_NO = B.ITEM_NO
                        LEFT JOIN CARS_DETAIL_TRAVELER@TRANSPTPRD  C ON C.ID = B.ID AND B.ITEM_NO = C.ITEM_NO_BOOKING
                        LEFT JOIN CARS_MT_CARTYPE@TRANSPTPRD  D ON A.CAR_TYPE = D.ID
                        LEFT JOIN CARS_DETAIL_TRAVELER@TRANSPTPRD  E ON A.ID = E.ID AND A.ITEM_NO = E.ITEM_NO_BOOKING
                        LEFT JOIN CARS_USER_PROFILE@TRANSPTPRD  F ON UPPER(E.UMS_USERNAME) = UPPER(F.USERNAME)
                        LEFT JOIN CARS_MT_DRIVER@TRANSPTPRD  G ON B.DRIVER_CODE = G.ID 
                        LEFT JOIN CARS_MT_CARCOMP@TRANSPTPRD  H ON G.CARCOMP_CODE = H.ID
                        RIGHT JOIN CARS_WF_PROCESS@TRANSPTPRD  WF ON WF.ID = A.ID
                        WHERE A.BUSINESS_DOC_NO = '{doc_id}' AND WF.STEP_FLOW != 4 ORDER BY A.ORDER_BY ";
            if (!(SetDocService.conn_ExecuteData(ref dt, sqlstrCar) == ""))
            {
                //กรณีที่ connection db ebiz
                sqlstrDef = $@" SELECT DISTINCT A.ID
                        , A.ITEM_NO
                        , WF.DOC_NO AS CARS_DOC_NO
                        , CASE WHEN B.ROUTE IS NULL THEN 0 ELSE B.ROUTE END AS ROUTE
                        , D.NAME AS CAR_TYPE
                        , A.BUSINESS_DOC_NO
                        , E.UMS_ID
                        , E.UMS_USERNAME
                        , F.TEL_NO
                        , TO_CHAR(A.USE_DATE_FROM,'DD/MM/YYYY') AS USE_DATE_FROM
                        , TO_CHAR(A.USE_DATE_TO,'DD/MM/YYYY') AS USE_DATE_TO
                        , A.USE_TIME_FROM
                        , A.USE_TIME_TO
                        , TO_CHAR(A.USE_DATE_FROM, 'DD Mon YYYY') || ' - ' || TO_CHAR(A.USE_DATE_TO, 'DD Mon YYYY') AS USE_DATE_STR
                        , A.LOCATION_FROM
                        , A.LOCATON_FROM_URL as LOCATION_FROM_URL
                        , A.LOCATON_TO as LOCATION_TO
                        , A.LOCATON_TO_URL as LOCATION_TO_URL
                        , CASE A.TRANSPORT_TYPE
                            WHEN 1 THEN 'ส่งอย่างเดียว'
                            WHEN 2 THEN 'รับอย่างเดียว'
                            WHEN 3 THEN 'ส่งและรอรับกลับ'
                          END AS TRANSPORT_TYPE 
                        , A.TRANSPORT_DETAIL
                        , A.ORDER_BY
                        , H.NAME AS CAR_COME_NAME
                        , G.DRIVER_NAME
                        , G.CAR_MODEL
                        , G.CAR_COLOR
                        , G.VEHICLE_REGISTRATION
                        , G.TEL_NUMBER AS DRIVER_TEL  
                        FROM CARS_DETAIL_BOOKING A
                        LEFT JOIN CARS_ROUTE B ON A.ID = B.ID AND A.ITEM_NO = B.ITEM_NO
                        LEFT JOIN CARS_DETAIL_TRAVELER C ON C.ID = B.ID AND B.ITEM_NO = C.ITEM_NO_BOOKING
                        LEFT JOIN CARS_MT_CARTYPE D ON A.CAR_TYPE = D.ID
                        LEFT JOIN CARS_DETAIL_TRAVELER E ON A.ID = E.ID AND A.ITEM_NO = E.ITEM_NO_BOOKING
                        LEFT JOIN CARS_USER_PROFILE F ON UPPER(E.UMS_USERNAME) = UPPER(F.USERNAME)
                        LEFT JOIN CARS_MT_DRIVER G ON B.DRIVER_CODE = G.ID 
                        LEFT JOIN CARS_MT_CARCOMP H ON G.CARCOMP_CODE = H.ID
                        RIGHT JOIN CARS_WF_PROCESS WF ON WF.ID = A.ID
                        WHERE A.BUSINESS_DOC_NO = '{doc_id}'  AND WF.STEP_FLOW != 4 ORDER BY A.ORDER_BY ";
                if (SetDocService.conn_ExecuteData(ref dt, sqlstrDef) == "")
                {
                }
            }
            //}

            return dt;
        }
        public string refdata_TransportationURL(string token_login, string doc_id, string emp_id)
        {
            var dt = new DataTable();
            sqlstr = @" select key_value 
                        from  bz_config_data 
                        where lower(key_name) = lower('URL Document Personal Car') and status = 1 ";

            dt = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    //http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws  + DocumentFile/Document Personal Car.pdf
                    string _Server_path_service = top.ebiz.helper.AppEnvironment.GeteServerPathAPI()?.ToString() ?? "";
                    string key_value = dt?.Rows[0]["key_value"]?.ToString() ?? "";
                    return $"{_Server_path_service}/{key_value}";//DocumentFile/แบบขอใช้รถยนต์ส่วนตัวเพื่อการเดินทางธุรกิจบริษัท.docx
                }
            }
            return "";
        }

        public DataTable refdata_air_book(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_AIRTICKET_BOOKING);

            sqlstr = @"select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , a.ask_booking
                    , a.search_air_ticket
                    , a.as_company_recommend
                    , a.already_booked 
                    , a.already_booked_other 
                    , a.already_booked_id 
                    , a.booking_ref
                    , a.booking_status 
                    , a.additional_request  
                    , nvl(a.data_type,'save') as data_type
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change
                    , case when (select count(1) from bz_doc_allowance aw where aw.doc_id = a.doc_id and aw.emp_id = a.emp_id ) > 0 then 'true' else 'false' end data_type_allowance    
                    , nvl(a.doc_status,1) as doc_status
                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_airticket_booking a on h.dh_code = a.doc_id and a.emp_id  = ex.dte_emp_id 
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id ";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_air_book_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_AIRTICKET_DETAIL);

            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , a.airticket_date
                    , a.airticket_route_from
                    , a.airticket_route_to
                    , a.airticket_flight
                    , a.airticket_departure_time 
                    , a.airticket_arrival_time
                    , a.airticket_departure_date 
                    , a.airticket_arrival_date
                    , a.check_my_trip  
                    , a.airticket_root  
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                    , case when a.airticket_date is not null then to_char(to_date(a.airticket_date,'dd Mon rrrr')+1,'dd Mon rrrr') end airticket_date_next
                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_airticket_detail a on h.dh_code = a.doc_id  and a.emp_id  = ex.dte_emp_id   
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id, a.id ";



            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_accom_book(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_ACCOMMODATION_BOOKING);
            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sqlstr = @"select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , a.booking
                    , a.search
                    , a.recommend 
                    , a.already_booked
                    , a.already_booked_other
                    , a.already_booked_id 
                    , a.additional_request 
                    , a.booking_status  
                    , a.place_name  
                    , a.map_url    
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                    , nvl(a.doc_status,1) as doc_status
                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_accommodation_booking a on h.dh_code = a.doc_id and a.emp_id  = ex.dte_emp_id   
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_accom_book_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_ACCOMMODATION_DETAIL); ;

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sqlstr = @"select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , a.country
                    , a.hotel_name
                    , a.check_in
                    , a.check_out
                    , a.roomtype  
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change
 
                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code from  bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_accommodation_detail a on h.dh_code = a.doc_id and a.emp_id  = ex.dte_emp_id  
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }

        public EmpListOutModel data_traveler_history(string token_login, string doc_id, string id, DataRow dremp, DataTable dtvisa)
        {
            EmpListOutModel emp_list = new EmpListOutModel();
            var travellist = new List<travelerHistoryList>();

            string emp_id = dremp["emp_id"].ToString();

            // DataRow[] drvisa = dtvisa.Select("emp_id = '" + emp_id + "'");
            DataRow[] drvisa = dtvisa.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id).ToArray();
            travellist = new List<travelerHistoryList>();
            if (drvisa.Length > 0)
            {
                //doc_id,emp_id,id,country,icon,datefrom,dateto
                for (int j = 0; j < drvisa.Length; j++)
                {
                    travellist.Add(new travelerHistoryList
                    {
                        doc_id = doc_id,
                        emp_id = emp_id,
                        id = (drvisa[j]["id"].ToString() ?? "") == "" ? j.ToString() : drvisa[j]["id"].ToString(),
                        seq = (j + 1).ToString(),
                        country = drvisa[j]["country"].ToString(),
                        icon = drvisa[j]["icon"].ToString(),
                        datefrom = drvisa[j]["datefrom"].ToString(),
                        dateto = drvisa[j]["dateto"].ToString(),
                    });
                }
            }
            else
            {
                travellist.Add(new travelerHistoryList
                {
                    doc_id = doc_id,
                    emp_id = emp_id,
                });
            }

            emp_list.token_login = token_login;
            emp_list.doc_id = doc_id;
            emp_list.emp_id = emp_id;
            emp_list.id = id;

            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
            emp_list.doc_status_id = "1";
            emp_list.doc_status_text = "Not Start";

            emp_list.status_trip_cancelled = dremp["status_trip_cancelled"].ToString();

            emp_list.show_button = true;

            emp_list.titlename = dremp["titlename"].ToString();
            emp_list.firstname = dremp["firstname"].ToString();
            emp_list.lastname = dremp["lastname"].ToString();
            emp_list.userDisplay = dremp["userdisplay"].ToString();
            emp_list.userName = dremp["username"].ToString();
            emp_list.division = dremp["division"].ToString();
            emp_list.idNum = dremp["emp_id"].ToString();
            emp_list.userEmail = dremp["useremail"].ToString();
            emp_list.userPhone = dremp["userphone"].ToString();
            emp_list.userTel = dremp["usertel"].ToString();
            emp_list.isEdit = false;
            emp_list.traveler_emp = null;
            emp_list.arrTraveler = travellist;
            emp_list.imgpath = dremp["imgpath"].ToString();
            emp_list.imgprofilename = dremp["imgprofilename"].ToString();

            emp_list.passportno = dremp["passportno"].ToString();
            emp_list.dateofissue = dremp["dateofissue"].ToString();
            emp_list.dateofbirth = dremp["dateofbirth"].ToString();
            emp_list.dateofexpire = dremp["dateofexpire"].ToString();
            emp_list.passport_img = dremp["passport_img"].ToString();
            emp_list.passport_img_name = dremp["passport_img_name"].ToString();

            emp_list.travel_topic = dremp["travel_topic"].ToString();
            emp_list.travel_topic_sub = dremp["travel_topic_sub"].ToString();
            emp_list.business_date = dremp["business_date"].ToString();
            emp_list.travel_date = dremp["travel_date"].ToString();
            emp_list.country_city = dremp["country_city"].ToString();

            emp_list.continent_id = dremp["continent_id"].ToString();
            emp_list.country_id = dremp["country_id"].ToString();

            emp_list.gl_account = dremp["gl_account"].ToString();
            emp_list.cost_center = dremp["cost_center"].ToString();
            emp_list.io_wbs = dremp["io_wbs"].ToString();

            //userCompany,userPosition,userGender,userJoinDate,dateOfDeparture
            //, null as company, null as position, null as gender, null as joindate, null as dateOfDeparture
            emp_list.userCompany = dremp["company"].ToString();
            emp_list.userPosition = dremp["position"].ToString();
            emp_list.userGender = dremp["gender"].ToString();
            emp_list.userJoinDate = dremp["joindate"].ToString();
            emp_list.dateOfDeparture = dremp["dateOfDeparture"].ToString();
            try
            {
                emp_list.sap_from_date = dremp["sap_from_date"].ToString();
                emp_list.sap_to_date = dremp["sap_to_date"].ToString();
            }
            catch { }
            try
            {
                emp_list.def_location_id = dremp["def_location_id"].ToString();
            }
            catch { }

            if (id == "0")
            {
                emp_list.mail_status = "true";
            }
            else
            {
                emp_list.mail_status = "false";
            }
            emp_list.send_to_sap = "false";

            return emp_list;
        }
        public travelerEmpList data_traveler_emp_list(string token_login, string doc_id, string id, DataRow dremp, Boolean user_admin)
        {
            travelerEmpList emp_list = new travelerEmpList();
            try
            {
                emp_list.token_login = token_login;
                emp_list.doc_id = doc_id;
                emp_list.emp_id = dremp["emp_id"].ToString();
                emp_list.id = id;
                emp_list.user_admin = true;
                emp_list.show_button = true;

                emp_list.age = dremp["age"].ToString();
                emp_list.org_unit = dremp["org_unit"].ToString();

                emp_list.titlename = dremp["titlename"].ToString();
                emp_list.firstname = dremp["firstname"].ToString();
                emp_list.lastname = dremp["lastname"].ToString();
                emp_list.userDisplay = dremp["userdisplay"].ToString();
                emp_list.userName = dremp["username"].ToString();
                emp_list.division = dremp["division"].ToString();
                emp_list.idNum = dremp["emp_id"].ToString();
                emp_list.userEmail = dremp["useremail"].ToString();
                emp_list.userPhone = dremp["userphone"].ToString();
                emp_list.userTel = dremp["usertel"].ToString();
                emp_list.isEdit = false;
                emp_list.imgpath = dremp["imgpath"].ToString();
                emp_list.imgprofilename = dremp["imgprofilename"].ToString();

                emp_list.passportno = dremp["passportno"].ToString();
                emp_list.dateofissue = dremp["dateofissue"].ToString();
                emp_list.dateofbirth = dremp["dateofbirth"].ToString();
                emp_list.dateofexpire = dremp["dateofexpire"].ToString();
                emp_list.passport_img = dremp["passport_img"].ToString();
                emp_list.passport_img_name = dremp["passport_img_name"].ToString();

                emp_list.travel_topic = dremp["travel_topic"].ToString();
                emp_list.travel_topic_sub = dremp["travel_topic_sub"].ToString(); ;

                emp_list.business_date = dremp["business_date"].ToString();
                emp_list.travel_date = dremp["travel_date"].ToString();
                emp_list.country_city = dremp["country_city"].ToString();

                emp_list.action_type = "update";

            }
            catch (Exception ex) { emp_list.token_login = ex.Message.ToString(); }

            return emp_list;
        }
        public DataTable refdata_passport_list(string doc_id)
        {
            var p = new List<OracleParameter>();
            sqlstr = @"select a.id, a.passport_no, ex.dte_emp_id
                    from bz_doc_head h
                    inner join  (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid   
                    left join bz_data_passport a on a.emp_id  = ex.dte_emp_id  
                    where a.passport_no is not null and h.dh_code = :doc_id order by ex.dte_emp_id,a.id  ";
            p.Add(new OracleParameter("doc_id", doc_id));
            var cmd = new OracleCommand(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
            }

            return dt;
        }
        public DataTable refdata_visa(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            DataTable dtdetail = new DataTable();
            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_VISA); ;
            int imaxid_data = sw.GetMaxID(TableMaxId.BZ_DATA_VISA); ;

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);
            if (doc_id == "personal")
            {
                sqlstr = @" select distinct 'personal' as doc_id, b.employeeid as emp_id
                        , a.id
                        , a.visa_place_issue 
                        , a.visa_valid_from 
                        , a.visa_valid_to 
                        , a.visa_valid_until 
                        , a.visa_type 
                        , a.visa_category 
                        , a.visa_entry 
                        , a.visa_name 
                        , a.visa_surname 
                        , a.visa_date_birth 
                        , a.visa_nationality 
                        , a.passport_no 
                        , a.visa_sex 
                        , a.visa_authorized_signature 
                        , a.visa_remark 
                        , a.visa_card_id 
                        , a.visa_serial 
                        , a.visa_in_traveler
                        , 'true' as visa_active_in_doc
                        , case when nvl( (select ad.default_type  from bz_doc_visa ad where ad.doc_id = a.doc_id
                            and a.emp_id  = ad.emp_id  and ad.default_type is not null  and  ad.visa_card_id = a.visa_card_id) ,'false' )
                            = 'true' then 'true' else 'false' end default_type
                        , 'false' as default_action_change
                        , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
 
                        from vw_bz_users b 
                        left join bz_data_visa a on b.employeeid = a.emp_id 
                        where b.employeeid in (select distinct a.user_id  from bz_login_token a where a.token_code = '" + token_login + "')  order by b.employeeid,a.id";
                p.Add(new OracleParameter("token_code", token_login));

            }
            else
            {
                sqlstr = @" select h.dh_code as doc_id ,ex.dte_emp_id as emp_id
                    , a.id
                    , a.visa_place_issue 
                    , a.visa_valid_from 
                    , a.visa_valid_to 
                    , a.visa_valid_until 
                    , a.visa_type 
                    , a.visa_category 
                    , a.visa_entry 
                    , a.visa_name 
                    , a.visa_surname 
                    , a.visa_date_birth 
                    , a.visa_nationality 
                    , a.passport_no 
                    , a.visa_sex 
                    , a.visa_authorized_signature 
                    , a.visa_remark 
                    , a.visa_card_id 
                    , a.visa_serial 
                    , a.visa_in_traveler
                    , 'false' as visa_active_in_doc
 
                    , case when nvl( (select ad.default_type  from bz_doc_visa ad where ad.doc_id = h.dh_code
                        and a.emp_id  = ad.emp_id  and ad.default_type is not null  and  ad.visa_card_id = a.visa_card_id) ,'false' )
                        = 'true' then 'true' else 'false' end default_type
                    , 'false' as default_action_change
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
 
                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid    
                    left join bz_data_visa a on a.emp_id  = ex.dte_emp_id    
                    where h.dh_code = :doc_id";
                p.Add(new OracleParameter("doc_id", doc_id));

                if (emp_id != "") { sqlstr += @" and ex.dte_emp_id =  :emp_id "; p.Add(new OracleParameter("emp_id", emp_id)); }
                sqlstr += @"   order by ex.dte_emp_id,a.id ";
            }


            dt = new DataTable();
            cmd = new OracleCommand(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                if (doc_id != "personal")
                {
                    sqlstr = @"  select distinct te.dte_emp_id as emp_id, te.ct_id as country_id, ct.ct_name as country    
                                 from bz_doc_traveler_expense te
                                 inner join  bz_master_country ct on te.ct_id = ct.ct_id
                                 where te.dh_code = :doc_id";

                    if (emp_id != "") { sqlstr += @" and te.dte_emp_id =  :emp_id "; }
                }
                cmd = new OracleCommand(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                DataTable dtcountry = new DataTable();
                SetDocService.conn_ExecuteData(ref dtcountry, cmd);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
                    if (doc_id != "personal")
                    {
                        if (dtcountry != null)
                        {
                            string emp_id_select = dt.Rows[i]["emp_id"].ToString();
                            string country_id_select = dt.Rows[i]["visa_nationality"].ToString();
                            if (dtcountry.Rows.Count > 0)
                            {
                                try
                                {
                                    // DataRow[] drcheck_ct = dtcountry.Select("emp_id='" + emp_id_select + "' and country_id ='" + country_id_select + "' ");
                                    DataRow[] drcheck_ct = dtcountry.AsEnumerable().Where(r => r.Field<string>("emp_id") == emp_id_select && r.Field<string>("country_id") == country_id_select).ToArray();
                                    if (drcheck_ct.Length > 0) { dt.Rows[i]["visa_active_in_doc"] = "true"; }
                                }
                                catch
                                {
                                    //กรณที่ไม่มีข้อมูล visa nationality
                                }
                            }
                        }
                    }

                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }

            }

            return dt;
        }

        public DataTable refdata_visa_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            DataTable dtdetail = new DataTable();
            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_VISA); ;
            int imaxid_data = sw.GetMaxID(TableMaxId.BZ_DATA_VISA); ;

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);
            if (doc_id == "personal")
            {
                sqlstr = @" select distinct 'personal' as doc_id, b.employeeid as emp_id
                        , a.id
                        , a.visa_place_issue 
                        , a.visa_valid_from 
                        , a.visa_valid_to 
                        , a.visa_valid_until 
                        , a.visa_type 
                        , a.visa_category 
                        , a.visa_entry 
                        , a.visa_name 
                        , a.visa_surname 
                        , a.visa_date_birth 
                        , a.visa_nationality 
                        , a.passport_no 
                        , a.visa_sex 
                        , a.visa_authorized_signature 
                        , a.visa_remark 
                        , a.visa_card_id 
                        , a.visa_serial 
                        , a.visa_in_traveler
                        , 'true' as visa_active_in_doc
                        , case when nvl( (select ad.default_type  from bz_doc_visa ad where ad.doc_id = a.doc_id
                            and a.emp_id  = ad.emp_id  and ad.default_type is not null  and  ad.visa_card_id = a.visa_card_id) ,'false' )
                            = 'true' then 'true' else 'false' end default_type
                        , 'false' as default_action_change
                        , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                        , nvl(a.doc_status,1) as doc_status
                        , 1 as visa_valid_until_check
                        from vw_bz_users b 
                        left join bz_data_visa a on b.employeeid = a.emp_id 
                        where b.employeeid in (select distinct a.user_id  from bz_login_token a where a.token_code = :token_login)  order by b.employeeid,a.id";
                p.Add(new("token_login", token_login));
            }
            else
            {
                sqlstr = @" select h.dh_code as doc_id ,ex.dte_emp_id as emp_id
                    , a.id
                    , a.visa_place_issue 
                    , a.visa_valid_from 
                    , a.visa_valid_to 
                    , a.visa_valid_until 
                    , a.visa_type 
                    , a.visa_category 
                    , a.visa_entry 
                    , a.visa_name 
                    , a.visa_surname 
                    , a.visa_date_birth 
                    , a.visa_nationality 
                    , a.passport_no 
                    , a.visa_sex 
                    , a.visa_authorized_signature 
                    , a.visa_remark 
                    , a.visa_card_id 
                    , a.visa_serial 
                    , a.visa_in_traveler
                    , 'false' as visa_active_in_doc
 
                    , case when nvl( (select ad.default_type  from bz_doc_visa ad where ad.doc_id = h.dh_code
                        and a.emp_id  = ad.emp_id  and ad.default_type is not null  and  ad.visa_card_id = a.visa_card_id and ROWNUM = 1) ,'false' )
                        = 'true' then 'true' else 'false' end default_type
                    , 'false' as default_action_change
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                    , nvl(a.doc_status,1) as doc_status
                    , case when nvl(a.visa_valid_until,'')='' then 0 else (case when to_date('22 Sep 2029','dd Mon yyyy') > sysdate then 0 else 1 end ) end visa_valid_until_check
                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code,ct_id from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid    
                    left join bz_data_visa a on a.emp_id  = ex.dte_emp_id and a.visa_nationality = ex.ct_id   
                    where h.dh_code = :doc_id and  a.id is not null";
                p.Add(new("doc_id", doc_id));

                if (emp_id != "") { sqlstr += @" and ex.dte_emp_id =  :emp_id "; p.Add(new("emp_id", emp_id)); }
                sqlstr += @"   order by ex.dte_emp_id,a.id ";
            }


            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                p.Clear();
                if (doc_id == "personal")
                {
                    //กรณีที่ไม่มีข้อมูล emp_id ส่งมาให้หาข้อมูลตาม token login???หน้า personal 
                    if (emp_id == "") { emp_id = user_id; }
                }

                sqlstr = @"  select distinct te.dte_emp_id as emp_id, te.ct_id as country_id, ct.ct_name as country    
                                 from bz_doc_traveler_expense te
                                 inner join  bz_master_country ct on te.ct_id = ct.ct_id
                                 where 1=1 ";
                if (doc_id != "") { sqlstr += @" and te.dh_code = :doc_id "; p.Add(new("doc_id", doc_id)); }
                if (emp_id != "") { sqlstr += @" and te.dte_emp_id =  :emp_id "; p.Add(new("emp_id", emp_id)); }
                DataTable dtcountry = new DataTable();
                cmd = new(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                SetDocService.conn_ExecuteData(ref dtcountry, cmd);
                p.Clear();

                //ดึงข้อมูล doc_status จาก doc_status
                sqlstr = @"select distinct emp_id, nvl(doc_status,'1') as doc_status from BZ_DOC_VISA where doc_id = :doc_id";
                p.Add(new("doc_id", doc_id));
                //
                DataTable dtdoc_status = new DataTable();
                cmd = new(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                SetDocService.conn_ExecuteData(ref dtdoc_status, cmd);
                p.Clear();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string emp_id_select = dt.Rows[i]["emp_id"].ToString();

                    //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
                    if (doc_id != "personal")
                    {
                        if (dtcountry != null)
                        {
                            string country_id_select = dt.Rows[i]["visa_nationality"].ToString();
                            if (dtcountry.Rows.Count > 0)
                            {
                                try
                                {
                                    // DataRow[] drcheck_ct = dtcountry.Select("emp_id='" + emp_id_select + "' and country_id ='" + country_id_select + "' ");
                                    // DataRow[] drcheck_ct = dtcountry.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("country_id") == country_id_select).ToArray();
                                    DataRow[] drcheck_ct = dtcountry.AsEnumerable().Where(s =>
              s["emp_id"] != DBNull.Value && s["emp_id"].ToString() == emp_id_select &&
              s["country_id"] != DBNull.Value && s["country_id"].ToString() == country_id_select
          ).ToArray();
                                    if (drcheck_ct.Length > 0) { dt.Rows[i]["visa_active_in_doc"] = "true"; }

                                }
                                catch
                                {
                                    //กรณที่ไม่มีข้อมูล visa nationality
                                }
                            }
                        }

                        if (dtdoc_status != null)
                        {
                            if (dtdoc_status.Rows.Count > 0)
                            {
                                try
                                {
                                    // DataRow[] drcheck_doc_status = dtdoc_status.Select("emp_id='" + emp_id_select + "' ");
                                    DataRow[] drcheck_doc_status = dtdoc_status.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                                    if (drcheck_doc_status.Length > 0) { dt.Rows[i]["doc_status"] = drcheck_doc_status[0]["doc_status"].ToString(); }
                                }
                                catch
                                {
                                    //กรณที่ไม่มีข้อมูล
                                }
                            }
                        }

                    }

                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }

            }

            return dt;
        }
        public DataTable refdata_passport_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            DataTable dtdetail = new DataTable();
            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_PASSPORT); ;

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);
            var p = new List<OracleParameter>();
            if (doc_id == "personal")
            {
                sqlstr = @" select ad.default_type,ad.passport_no as passportno,nvl(update_date,create_date) 
                             from bz_data_passport ad where  (ad.default_type is not null and ad.default_type = 'true')   ";
                sqlstr += @" and ad.emp_id in (select distinct a.user_id  from bz_login_token a where a.token_code = :token_login)";
                sqlstr += @" order by  nvl(update_date,create_date) ";
                p.Add(new OracleParameter("token_login", token_login));
            }
            else
            {
                //Auwat 20210903 0000 แก้ไข concept ให้ passport เป็นข้อมูลชุดกันของแต่ traverler
                //sqlstr = @"  select ad.passport_no as passportno, max( nvl(update_date,create_date)) as x
                //             from bz_doc_passport ad where  (ad.default_type is not null and ad.default_type = 'true')
                //             and doc_id = '" + doc_id + "' group by  ad.passport_no"; 
                sqlstr = @" select ad.default_type,ad.passport_no as passportno,nvl(update_date,create_date) 
                             from bz_data_passport ad where  (ad.default_type is not null and ad.default_type = 'true')   
            and ad.emp_id in (select distinct dte_emp_id from bz_doc_traveler_expense where dh_code = :doc_id)
   order by  nvl(update_date,create_date) ";
                p.Add(new OracleParameter("doc_id", doc_id));

            }

            var cmd = new OracleCommand(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            DataTable dtdefault_type = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dtdefault_type, cmd) == "")
            {

            }
            p.Clear();
            if (doc_id == "personal")
            {

                sqlstr = @" select distinct 'personal'  as doc_id, b.employeeid as emp_id
                            , a.id
                            , a.passport_no as passportno
                            , a.passport_date_issue as dateofissue
                            , a.passport_date_expire as dateofexpire
                    
                            , case when a.passport_title is null then b.entitle else a.passport_title end titlename
                            , case when a.passport_name is null then b.enfirstname else a.passport_name end firstname
                            , case when a.passport_surname is null then b.enlastname else a.passport_surname end lastname
                            , a.passport_date_birth as dateofbirth 
                            , a.accept_type, a.sort_by
                            , a.default_type as default_type
                            , 'false' as default_action_change
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            , '1' as doc_status, '' as ref_id, '' as sap_status
                            from vw_bz_users b 
                            left join bz_data_passport a on b.employeeid = a.emp_id  
                            where b.employeeid in (select distinct a.user_id  from bz_login_token a where a.token_code =  :token_login)";
                sqlstr += "  order by b.employeeid,a.id";
                p.Add(new OracleParameter("token_login", token_login));
            }
            else
            {
                //Auwat 20210903 0000 แก้ไข concept ให้ passport เป็นข้อมูลชุดกันของแต่ traverler 
                sqlstr = @" select distinct 'personal'  as doc_id, b.employeeid as emp_id
                            , a.id
                            , a.passport_no as passportno
                            , a.passport_date_issue as dateofissue
                            , a.passport_date_expire as dateofexpire
                    
                            , case when a.passport_title is null then b.entitle else a.passport_title end titlename
                            , case when a.passport_name is null then b.enfirstname else a.passport_name end firstname
                            , case when a.passport_surname is null then b.enlastname else a.passport_surname end lastname
                            , a.passport_date_birth as dateofbirth 
                            , a.accept_type, a.sort_by
                            , a.default_type as default_type
                            , 'false' as default_action_change
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            , '1' as doc_status, '' as ref_id, '' as sap_status
                            from vw_bz_users b 
                            left join bz_data_passport a on b.employeeid = a.emp_id  
                            where b.employeeid in (select distinct dte_emp_id from bz_doc_traveler_expense where dh_code = :doc_id)";
                sqlstr += "  order by b.employeeid,a.id";
                p.Add(new OracleParameter("doc_id", doc_id));

            }

            dt = new DataTable();
            cmd = new OracleCommand(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());

            //
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                p.Clear();
                if (doc_id == "personal")
                {
                    //กรณีที่ไม่มีข้อมูล emp_id ส่งมาให้หาข้อมูลตาม token login???หน้า personal 
                    if (emp_id == "") { emp_id = user_id; }
                }
                sqlstr = @"  select distinct te.dte_emp_id as emp_id, te.ct_id as country_id, ct.ct_name as country    
                                 from bz_doc_traveler_expense te
                                 inner join  bz_master_country ct on te.ct_id = ct.ct_id
                                 where 1=1 ";
                if (doc_id != "") { sqlstr += @" and te.dh_code = :doc_id "; p.Add(new OracleParameter("doc_id", doc_id)); }
                if (emp_id != "") { sqlstr += @" and te.dte_emp_id =  :emp_id "; p.Add(new OracleParameter("emp_id", emp_id)); }
                DataTable dtcountry = new DataTable();
                cmd = new OracleCommand(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());

                SetDocService.conn_ExecuteData(ref dtcountry, cmd);
                p.Clear();
                #region ดึงข้อมูล doc_status จาก doc_status
                //Auwat 20210903 0000 แก้ไข concept ให้ passport เป็นข้อมูลชุดกันของแต่ traverler
                sqlstr = @"select distinct emp_id, nvl(doc_status,'1') as doc_status from BZ_DOC_PASSPORT where doc_id =:doc_id";
                p.Add(new OracleParameter("doc_id", doc_id));
                DataTable dtdoc_status = new DataTable();
                cmd = new OracleCommand(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                SetDocService.conn_ExecuteData(ref dtdoc_status, cmd);
                p.Clear();
                #endregion ดึงข้อมูล doc_status จาก doc_status

                //DevFix 20250122 0000 ดึง status send to SAP iRPA มาแสดง 
                sqlstr = $@"select distinct a.id, a.emp_id, a.passport_no, a.ref_id, a.sap_status 
                            from  BZ_PASSPORT a where 1=1  ";
                if (emp_id != "") { sqlstr += $@" and a.emp_id = :emp_id "; p.Add(new OracleParameter("emp_id", emp_id)); }
                sqlstr += $@" order by a.id desc";

                DataTable dtsap_status = new DataTable();
                cmd = new OracleCommand(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                SetDocService.conn_ExecuteData(ref dtsap_status, cmd);
                p.Clear();

                Boolean bdefault_type = false;
                if (doc_id == "personal") { bdefault_type = true; }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string emp_id_select = dt.Rows[i]["emp_id"]?.ToString() ?? "";
                    string passport_no_select = dt.Rows[i]["passportno"]?.ToString() ?? "";

                    try
                    {
                        if (bdefault_type == false)
                        {

                            dt.Rows[i]["default_type"] = "false";
                            if (dtdefault_type.Rows.Count > 0)
                            {
                                // DataRow[] drcheckaction = dtdefault_type.Select("passportno='" + dt.Rows[i]["passportno"].ToString() + "'");
                                DataRow[] drcheckaction = dtdefault_type.AsEnumerable().Where(s => s.Field<string>("passportno") == dt.Rows[i]["passportno"].ToString()).ToArray();

                                if (drcheckaction.Length > 0)
                                {
                                    dt.Rows[i]["default_type"] = "true";
                                }
                            }
                        }
                    }
                    catch { }

                    if (doc_id != "personal")
                    {
                        if (dtdoc_status != null)
                        {
                            if (dtdoc_status.Rows.Count > 0)
                            {
                                try
                                {
                                    // DataRow[] drcheck_doc_status = dtdoc_status.Select("emp_id='" + emp_id_select + "' ");
                                    DataRow[] drcheck_doc_status = dtdoc_status.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                                    if (drcheck_doc_status.Length > 0) { dt.Rows[i]["doc_status"] = drcheck_doc_status[0]["doc_status"].ToString(); }
                                }
                                catch
                                {
                                    //กรณที่ไม่มีข้อมูล
                                }
                            }
                        }
                    }

                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {

                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }


                    //DevFix 20250122 0000 ดึง status send to SAP iRPA มาแสดง
                    if (dtsap_status != null && dtsap_status?.Rows.Count > 0)
                    {
                        try
                        {
                            // DataRow[] drSAP = dtsap_status.Select($"passport_no = '{passport_no_select}' and emp_id = '{emp_id_select}'");
                            DataRow[] drSAP = dtsap_status.AsEnumerable().Where(s => s.Field<string>("passport_no") == passport_no_select && s.Field<string>("emp_id") == emp_id_select).ToArray();
                            if (drSAP.Length > 0)
                            {
                                dt.Rows[i]["ref_id"] = drSAP[0]["ref_id"]?.ToString() ?? "";
                                dt.Rows[i]["sap_status"] = drSAP[0]["sap_status"]?.ToString() ?? "";
                            }
                        }
                        catch { }
                    }
                }

            }

            return dt;
        }
        public DataTable refdata_allowance(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_ALLOWANCE); ;

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);


            //Auwat 20210705 เพิ่ม file Passport, Passport Valid Until, Luggage Clothing Valid Until
            //passport, passport_date, luggage_clothing_date 
            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id 
                    , a.grand_total   
                    , case when a.luggage_clothing  is null then  to_char(ex.dte_cl_expense) else to_char(a.luggage_clothing)  end luggage_clothing 
                    , to_char(ex.dte_cl_expense) as luggage_clothing_def
                    , case when a.luggage_clothing_date is null then to_char(ex.dte_cl_valid-1,'dd Mon rrrr') else a.luggage_clothing_date end luggage_clothing_date 
                    , ex.dte_passport_expense as passport
                    , to_char(ex.dte_passport_valid-1,'dd Mon rrrr') as passport_date
                    , a.remark 
                    , file_travel_report, file_report
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                    , case when (select count(1) from bz_doc_allowance aw where aw.doc_id = a.doc_id and aw.emp_id = a.emp_id ) > 0 then 'true' else 'false' end data_type_allowance
                    , nvl(a.doc_status,1) as doc_status
                    from bz_doc_head h
                    inner join (select distinct  null as dte_passport_expense, null as dte_passport_valid
                    ,dte_emp_id, dh_code, null as dte_cl_valid, sum(dte_cl_expense) as dte_cl_expense
                    from bz_doc_traveler_expense group by dte_emp_id, dh_code, dte_cl_valid
                    ) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_allowance a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id  and ex.dte_emp_id = a.emp_id        
                    where 1=1    ";

            if (doc_id.ToLower().IndexOf("ob") > -1 || doc_id.ToLower().IndexOf("ot") > -1)
            {
                sqlstr += @" and h.dh_code in (select distinct doc_id from  bz_doc_airticket_booking where data_type ='submit')";
            }
            sqlstr += @" and h.dh_code = :doc_id ";
            p.Add(new OracleParameter("doc_id", doc_id));
            sqlstr += @" order by ex.dte_emp_id,a.id";

            dt = new DataTable();
            cmd = new OracleCommand(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());

            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                p.Clear();
                sqlstr = @" select distinct  ex.dte_emp_id as emp_id, ex.dh_code as doc_id
                             , ex.dte_passport_expense as passport, to_char(ex.dte_passport_valid-1,'dd Mon rrrr') as passport_date
                             , to_char(ex.dte_cl_valid-1,'dd Mon rrrr') as luggage_clothing_date
                             , ex.dte_passport_valid
                             from bz_doc_traveler_expense ex 
                             where ex.dte_passport_expense is not null and ex.dh_code = :doc_id order by ex.dte_passport_valid";
                DataTable dtpassport_phase1 = new DataTable();
                p.Add(new OracleParameter("doc_id", doc_id));
                cmd = new OracleCommand(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                if (SetDocService.conn_ExecuteData(ref dtpassport_phase1, cmd) == "")
                {
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.Rows[i]["action_change"] = "true";
                        dt.AcceptChanges();
                        imaxid++;

                        if (dtpassport_phase1 != null)
                        {
                            if (dtpassport_phase1.Rows.Count > 0)
                            {
                                // DataRow[] drcheck = dtpassport_phase1.Select("emp_id = '" + dt.Rows[i]["emp_id"].ToString() + "'");
                                DataRow[] drcheck = dtpassport_phase1.AsEnumerable().Where(s => s.Field<string>("emp_id") == dt.Rows[i]["emp_id"].ToString()).ToArray();
                                if (drcheck.Length > 0)
                                {
                                    dt.Rows[i]["passport"] = drcheck[0]["passport"].ToString();
                                    dt.Rows[i]["passport_date"] = drcheck[0]["passport_date"].ToString();
                                    if (dt.Rows[i]["luggage_clothing_date"].ToString() == "") { dt.Rows[i]["luggage_clothing_date"] = drcheck[0]["luggage_clothing_date"].ToString(); }
                                }
                            }
                        }
                    }
                }
            }

            return dt;
        }

        public DataTable refdata_allowance_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            //LB21030082
            //OB21030506 --> ขาไป 1 เที่ยวบิน
            //OB21030505 oversea specel case --> ขาไป 2 เที่ยวบิน ภายในวันเดียวกัน

            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_ALLOWANCE_DETAIL); ;
            string doc_type = refdoc_type(doc_id);

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            DataTable dtt = new DataTable();
            DataTable dtdef = new DataTable();
            DataTable dtrate = new DataTable();
            DataTable dtzone = new DataTable();
            DataTable dtair = new DataTable();
            DataTable dtm_exchangerate = ref_exchangerate();//rate USD 


            #region หาจำนวนวันตั้งแต่เริมจนถึงวันสุดท้ายที่กลับ
            if (doc_type == "local")
            {
                //ดึงตาม traverler
                sqlstr = @" select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                        , to_char(ex.dte_bus_fromdate,'rrrr') as date_from_y
                        , to_char(ex.dte_bus_fromdate,'mm') as date_from_m
                        , to_char(ex.dte_bus_fromdate,'dd') as date_from_d
                        , to_char(ex.dte_bus_todate,'rrrr') as date_to_y
                        , to_char(ex.dte_bus_todate,'mm') as date_to_m
                        , to_char(ex.dte_bus_todate,'dd') as date_to_d
                        , to_char(ex.dte_bus_fromdate,'rrrrmmdd') as date_from 
                        , to_char(ex.dte_bus_todate,'rrrrmmdd') as date_to 
                        from bz_doc_head h
                        inner join (select distinct dte_emp_id, dh_code, min(dte_bus_fromdate) as dte_bus_fromdate
                        , max(dte_bus_todate) as dte_bus_todate
                        from bz_doc_traveler_expense 
                        where  dte_bus_fromdate is not null and dte_bus_fromdate is not null 
                        group by dte_emp_id, dh_code) ex on h.dh_code = ex.dh_code 
                        inner join vw_bz_users b on ex.dte_emp_id = b.employeeid      
                        where  h.dh_code = :doc_id ";

                sqlstr += @" order by ex.dte_emp_id";
                p.Add(new OracleParameter("doc_id", doc_id));
            }
            else
            {
                string xfrom_date = "airticket_arrival_date";
                sqlstr = @"select count(1) as xcount from  bz_doc_airticket_detail_keep t where t.doc_id = :doc_id ";
                p.Add(new OracleParameter("doc_id", doc_id));
                cmd = new(sqlstr);
                cmd.Parameters.AddRange(p.ToArray());
                dt = new DataTable();
                //
                if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
                {
                    if (dt.Rows.Count > 0) { if (dt.Rows[0]["xcount"].ToString() == "1") { xfrom_date = "airticket_arrival_date"; } }
                }
                p.Clear();
                //Auwat 20210823 0000 แก้ไข concept ให้ passport เป็นข้อมูลชุดกันของแต่ traverler
                sqlstr = @"select t.doc_id, t.emp_id
                    , to_char(t.val_date,'rrrr') as date_from_y
                    , to_char(t.val_date,'mm') as date_from_m
                    , to_char(t.val_date,'dd') as date_from_d
                    , to_char(t2.val_date,'rrrr') as date_to_y
                    , to_char(t2.val_date,'mm') as date_to_m
                    , to_char(t2.val_date,'dd') as date_to_d
                    , to_char(t.val_date,'rrrrmmdd') as date_from 
                    , to_char(t2.val_date,'rrrrmmdd') as date_to   
                    from (
                    select min(to_date(a.airticket_departure_date,'dd MON rrrr')) as val_date
                    , a.doc_id, a.emp_id, 1 as ref_type
                    from  bz_doc_airticket_detail_keep a  
                    group by a.emp_id, a.doc_id
                    )t 
                    left join (
                    select max(to_date(a." + xfrom_date + ",'dd MON rrrr')) as val_date " +
                 @" , a.doc_id, a.emp_id, 0 as ref_type
                    from  bz_doc_airticket_detail_keep a  
                    group by a.emp_id, a.doc_id
                    )t2 on t.doc_id = t2.doc_id and t.emp_id = t2.emp_id
                    where t.doc_id = :doc_id  order by t.emp_id ";
                p.Add(new OracleParameter("doc_id", doc_id));
            }
            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                dtt = dt; dtt.AcceptChanges();
            }
            p.Clear();
            #endregion หาจำนวนวันตั้งแต่เริมจนถึงวันสุดท้ายที่กลับ

            #region master 
            sqlstr = @" select a.kh_code, a.allowance_rate, a.currency
                        , a.workplace_type_country 
                        , a.workplace  
                        , case when a.workplace_type_country = 0 then a.workplace else  
                        (select to_char(z.ctn_id) from bz_master_country z where to_char(z.ct_id) =  to_char(a.workplace ) )  
                        end zone
                        , case when a.workplace_type_country = 1 then a.workplace end country
                        , a.overnight_type
                        from  bz_config_daily_allowance a 
                        where a.status = 1 
                        and lower(a.travel_category) like lower(:doc_type)  ";
            p.Add(new OracleParameter("doc_type", doc_type));

            dtrate = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dtrate, cmd) == "")
            {
            }
            p.Clear();
            sqlstr = @"  select a.ctn_id, a.ct_id, b.ap_id,b.*
                        from  bz_master_country a
                        inner join bz_master_airport b on a.ct_id = b.ct_id 
                        where ap_id  in 
                        (select  distinct  airticket_route_from from bz_doc_airticket_detail_keep  where doc_id = :doc_id1  union select distinct airticket_route_to from bz_doc_airticket_detail_keep  where doc_id = :doc_id2    )";
            p.Add(new OracleParameter("doc_id1", doc_id));
            p.Add(new OracleParameter("doc_id2", doc_id));
            dtzone = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dtzone, cmd) == "")
            {
            }
            p.Clear();
            #endregion master

            #region data airticket จำนวนวันที่เดินทางในแต่ละวัน

            sqlstr = @" select distinct
                        a.id,a.emp_id
                        , to_char(to_date(a.airticket_date,'dd MON rrrr'),'rrrrMMdd') as def_date 
                        , to_char(to_date(a.airticket_departure_date,'dd MON rrrr'),'rrrrMMdd') as airticket_departure_date
                        , to_char(to_date(a.airticket_arrival_date,'dd MON rrrr'),'rrrrMMdd') as airticket_arrival_date
                        , a.airticket_route_from, bf.ctn_id as zone_from, bf.ct_id as country_from   
                        , a.airticket_route_to, bt.ctn_id as zone_to, bt.ct_id as country_to 
   
                        , (24*60) as s_full_time
                        , (to_number(substr(a.airticket_departure_time,0,2))*60)+to_number(substr(a.airticket_departure_time,4,2)) as total_time  
                        , (to_number(substr(a.airticket_departure_time,0,2))*60)+to_number(substr(a.airticket_departure_time,4,2)) as s_departure_time 
                        , (to_number(substr(a.airticket_arrival_time,0,2))*60)+to_number(substr(a.airticket_arrival_time,4,2)) as s_arrival_time
                        , (24*60) - (to_number(substr(a.airticket_arrival_time,0,2))*60)+to_number(substr(a.airticket_arrival_time,4,2)) as s_next_time 

                        , 24-(to_number(substr(a.airticket_departure_time,0,2)) + (case when to_number(substr(a.airticket_departure_time,4,2)) > 0 then 1 else 0 end ) ) as diff_time_from 
                        , (to_number(substr(a.airticket_arrival_time,0,2)) + (case when to_number(substr(a.airticket_arrival_time,4,2)) > 0 then 1 else 0 end ) ) as diff_time_to 
                        , ROUND(
                         to_number(((to_number(substr(a.airticket_arrival_time,0,2))*60)+to_number(substr(a.airticket_arrival_time,4,2)) - (to_number(substr(a.airticket_departure_time,0,2))*60)+to_number(substr(a.airticket_departure_time,4,2)))/60)
                        ,4) as diff_time_inday
                        , 'false' as last_rows
                        , to_date(a.airticket_arrival_date,'dd MON rrrr') - to_date(a.airticket_date,'dd MON rrrr') as diff_date
                        from  bz_doc_airticket_detail_keep a 
                        inner join (
                        select a.ctn_id, a.ct_id, b.ap_id
                        from  bz_master_country a
                        inner join bz_master_airport b on a.ct_id = b.ct_id  )bf on to_char(a.airticket_route_from) = to_char(bf.ap_id)
                        inner join (
                        select a.ctn_id, a.ct_id, b.ap_id
                        from  bz_master_country a
                        inner join bz_master_airport b on a.ct_id = b.ct_id  )bt on to_char(a.airticket_route_to) = to_char(bt.ap_id) 
                        where a.doc_id = :doc_id ";
            p.Add(new OracleParameter("doc_id", doc_id));

            if (doc_type == "local")
            {
            }
            else
            {
                sqlstr += @" and (a.doc_id,a.emp_id) in (select distinct doc_id,emp_id from bz_doc_airticket_detail_keep)  ";
            }
            sqlstr += @" order by a.emp_id, to_number(to_char(to_date(a.airticket_date,'dd MON rrrr'),'rrrrMMdd')),a.id";


            //if (doc_type == "local")
            //{
            //    sqlstr = @" select * from VW_BZ_DATA_AIR_LOCAL a ";
            //}
            //else
            //{
            //    sqlstr = @" select * from VW_BZ_DATA_AIR_OVERSEA a ";
            //}
            //sqlstr += @" where a.doc_id = '" + doc_id + "'  ";
            //sqlstr += @" order by a.emp_id, to_number(def_date),a.id ";
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            dtair = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dtair, cmd) == "")
            {
                if (dtair.Rows.Count > 0)
                {
                    string emp_id_bf = "";
                    string emp_id_af = "";
                    for (int i = 0; i < dtair.Rows.Count; i++)
                    {
                        emp_id_bf = dtair.Rows[i]["emp_id"].ToString();
                        if (emp_id_bf != emp_id_af)
                        {
                            emp_id_af = emp_id_bf;
                            if (i > 0)
                            {
                                dtair.Rows[i - 1]["last_rows"] = "true";
                                dtair.AcceptChanges();
                            }
                        }
                    }
                    dtair.Rows[dtair.Rows.Count - 1]["last_rows"] = "true";
                    dtair.AcceptChanges();
                }
            }
            p.Clear();
            #endregion data airticket  จำนวนวันที่เดินทางในแต่ละวัน

            #region data allowance 
            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , a.allowance_date
                    , a.allowance_days 
                    , a.allowance_low
                    , a.allowance_mid  
                    , a.allowance_hight 
                    , a.allowance_total 
                    , a.allowance_unit   
                    , a.allowance_hrs   
                    , null as exchange_rate   
                    , nvl(a.allowance_type_id,2) as allowance_type_id 
                    , a.allowance_remark   

                    , h2.grand_total  
                    , case when  h2.luggage_clothing  is null then  to_char(ex.dte_cl_expense) else to_char(h2.luggage_clothing)  end luggage_clothing
                    , h2.remark  

                    , case when h.dh_type like 'oversea%' then kh.oversea_code else kh.local_code end kh_code
                    , to_char(to_date(a.allowance_date,'dd MON rrrr'),'rrrrMMdd') as def_date
                    , null as allowance_values_def

                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change
                   
                    from bz_doc_head h
                    inner join  (select distinct dte_emp_id, dh_code, dte_cl_expense from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_data_kh_code kh on b.employeeid = kh.emp_id
                    left join bz_doc_allowance h2 on h.dh_code =  h2.doc_id and ex.dh_code =  h2.doc_id  and ex.dte_emp_id = h2.emp_id 
                    left join bz_doc_allowance_detail a on h.dh_code = a.doc_id  and h2.emp_id = a.emp_id and a.emp_id  = ex.dte_emp_id         
                    where h.dh_code = :doc_id ";
            p.Add(new OracleParameter("doc_id", doc_id));

            if (doc_type == "local")
            {
            }
            else
            {
                sqlstr += @" and (h.dh_code,ex.dte_emp_id) in (select distinct doc_id,emp_id from  bz_doc_airticket_detail_keep ) ";
            }

            sqlstr += @" order by ex.dte_emp_id,a.id";
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            dtdef = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dtdef, cmd) == "")
            {

            }
            p.Clear();
            #endregion data allowance

            dt = new DataTable();
            if (dtt.Rows.Count > 0)
            {
                //คำนวน day & date airticket
                int irows = 0;

                DataTable dtnew = new DataTable();
                dtnew = dtdef.Clone(); dtnew.AcceptChanges();
                for (int i = 0; i < dtt.Rows.Count; i++)
                {
                    string emp_id_select = dtt.Rows[i]["emp_id"].ToString();

                    // DataRow[] drkh_code = dtdef.Select("emp_id='" + emp_id_select + "'");
                    DataRow[] drkh_code = dtdef.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                    string kh_code_select = "";
                    if (drkh_code.Length > 0) { kh_code_select = drkh_code[0]["kh_code"].ToString(); }

                    // DataRow[] drDate = dtt.Select("emp_id= '" + emp_id_select + "'");
                    //DataRow[] drDate = dtdef.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                    DataRow[] drDate = dtt.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                    if (drDate.Length > 0)
                    {
                        if (drDate[0]["date_from"].ToString() == "") { continue; }
                        if (drDate[0]["date_to"].ToString() == "") { continue; }

                        int ifrom = Convert.ToInt32(drDate[0]["date_from"].ToString());
                        int ito = Convert.ToInt32(drDate[0]["date_to"].ToString());

                        DateTime dNowsFrom = new DateTime(Convert.ToInt32(drDate[0]["date_from_y"].ToString())
                            , Convert.ToInt32(drDate[0]["date_from_m"].ToString())
                            , Convert.ToInt32(drDate[0]["date_from_d"].ToString()));
                        DateTime dNowsTo = new DateTime(Convert.ToInt32(drDate[0]["date_to_y"].ToString())
                            , Convert.ToInt32(drDate[0]["date_to_m"].ToString())
                            , Convert.ToInt32(drDate[0]["date_to_d"].ToString()));

                        DateTime dNowsFromNext = dNowsFrom.AddDays(1);

                        // หา ประเทศตาม สนามบิน   
                        string zone_def = "";//id
                        string country_def = "";//id  
                        string zone_def_bf = "";
                        string country_def_bf = "";
                        int irunning_rows = 1;


                        Boolean btransit = false;
                        Boolean blast_rows = false;

                        for (int j = 0; j < 999; j++)
                        {
                            DateTime dNows = dNowsFrom.AddDays(j);
                            DateTime dBefor = dNowsFrom.AddDays(j - 1);
                            DateTime dAfter = dNowsFrom.AddDays(j + 1);
                            if (dNows > dNowsTo) { break; }

                            DateTime dNowsNext = dNowsFromNext.AddDays(j);
                            //วันที่เดินทาง
                            string def_date = dNows.ToString("yyyyMMdd");

                            //ตรวจสอบและดึงข้อมูลจาก table ที่เคยบันทึกไว้มาแสดง
                            Boolean bCheckDateNewData = true;
                            string action_type = "insert";
                            string action_change = "true";
                            // DataRow[] drold = dtdef.Select("emp_id= '" + emp_id_select + "' and def_date ='" + def_date + "' ");
                            DataRow[] drold = dtdef.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("def_date") == def_date).ToArray();

                            if (drold.Length > 0)
                            {
                                bCheckDateNewData = false;
                                dtnew.ImportRow(drold[0]);
                                dtnew.AcceptChanges();
                            }
                            else
                            {
                                drold = dtdef.Select($"emp_id= '{emp_id_select}' and action_type = 'update' ");
                                if (drold.Length > 0)
                                {
                                    bCheckDateNewData = false;
                                    dtnew.ImportRow(drold[0]);
                                    dtnew.AcceptChanges();
                                }
                                else
                                {
                                    DataRow newRow = dtnew.NewRow();
                                    newRow["DOC_ID"] = doc_id;
                                    dtnew.Rows.Add(newRow);
                                    //dtnew.Rows.Add(dtnew.NewRow());
                                    dtnew.AcceptChanges();

                                    drold = dtdef.Select($"emp_id= '{emp_id_select}' ");
                                }
                            }

                            dtnew.Rows[irows]["allowance_days"] = (j + 1).ToString();
                            dtnew.Rows[irows]["allowance_date"] = dNows.ToString("dd MMM yyyy");

                            if (bCheckDateNewData == true)
                            {
                                #region calculate travel hr
                                double dHr = 0;
                                double dAllowance = 0;
                                double dAllowanceRate = 0;
                                double ex_rate = 1;
                                string allowance_values = "";
                                Double dTimeCheck_def = 0;
                                double allowance_low = 0;
                                double allowance_mid = 0;
                                double allowance_hight = 0;
                                string allowance_type_id = "";
                                string as_of = "";
                                string currency = "";

                                // clear data
                                dtnew.Rows[irows]["emp_id"] = emp_id_select;
                                dtnew.Rows[irows]["allowance_low"] = Convert.DBNull;
                                dtnew.Rows[irows]["allowance_mid"] = Convert.DBNull;
                                dtnew.Rows[irows]["allowance_hight"] = Convert.DBNull;

                                if (doc_type == "local")
                                {
                                    // allowance_low -->ไม่ค้างคืน -->0
                                    // allowance_mid -->ค้างคืน -->1
                                    string overnight_type = "0";

                                    // กรณีที่ไปวันเดียว ให้เป็น ไม่ค้างคืน เช็คแค่รายการแรก
                                    if ((dNowsFrom == dNowsTo) && j == 0) { overnight_type = "1"; }
                                    else if ((dNows == dNowsTo)) { overnight_type = "1"; }

                                    //Auwat 20210622 0000 แก้ไข ไม่ค้างคืน ให้คิด rate จากจำนวนเต็มแล้วหารครึ่ง
                                    //DataRow[] drrate = dtrate.Select("overnight_type='" + overnight_type + "'" +
                                    //    " and kh_code ='" + kh_code_select + "'"); 

                                    //Auwat 20210712 0000 แก้ไขเอา rate ค้างคืน/ไม่ค้างคืน ตามที่ maintain ไว้
                                    //DataRow[] drrate = dtrate.Select("kh_code ='" + kh_code_select + "' ");
                                    // DataRow[] drrate = dtrate.Select("kh_code ='" + kh_code_select + "' and overnight_type = '" + overnight_type + "' ");
                                    // DataRow[] drrate = dtrate.AsEnumerable().Where(s => s.Field<string>("kh_code") == kh_code_select && s.Field<string>("overnight_type") == overnight_type).ToArray();
                                    DataRow[] drrate = dtrate.AsEnumerable()
     .Where(s => s.Field<string>("kh_code") == kh_code_select &&
                 s.Field<decimal>("overnight_type") == Convert.ToDecimal(overnight_type))
     .ToArray();
                                    if (drrate.Length > 0)
                                    {
                                        try
                                        {
                                            dAllowanceRate = Convert.ToDouble(drrate[0]["allowance_rate"].ToString());
                                        }
                                        catch { }

                                        // rate in local เป็นเงิน thb เสมอ
                                        ex_rate = 1;
                                    }

                                    //Auwat 20210712 0000 แก้ไขเอา rate ค้างคืน/ไม่ค้างคืน ตามที่ maintain ไว้
                                    //if (overnight_type == "0") { dAllowanceRate = dAllowanceRate / 2; }


                                    dAllowance = (dAllowanceRate * ex_rate);

                                    if (bCheckDateNewData == true)
                                    {
                                        action_type = "insert";
                                        action_change = "true";

                                        if (dNows >= dNowsTo)
                                        {
                                            //กรณีที่ไม่มีวัน ถัดๆไปตีเป็นไม่ค้างคืน
                                            dtnew.Rows[irows]["allowance_low"] = dAllowance;
                                        }
                                        else
                                        {
                                            dtnew.Rows[irows]["allowance_hight"] = dAllowance;
                                        }
                                        dtnew.Rows[irows]["allowance_type_id"] = Convert.DBNull;

                                    }
                                    else
                                    {
                                        action_type = "update";
                                        action_change = "false";

                                        dtnew.Rows[irows]["allowance_low"] = drold[0]["allowance_low"];
                                        dtnew.Rows[irows]["allowance_hight"] = drold[0]["allowance_hight"];
                                        try
                                        {
                                            dtnew.Rows[irows]["allowance_type_id"] = drold[0]["allowance_type_id"];
                                        }
                                        catch { }
                                    }
                                    //เก็บค่าที่คำนวณจาก ระบบ
                                    dtnew.Rows[irows]["allowance_values_def"] = allowance_values;
                                    dtnew.Rows[irows]["action_type"] = action_type;
                                    dtnew.Rows[irows]["action_change"] = action_change;

                                }
                                else
                                {
                                    try { allowance_low = Convert.ToDouble(drold[0]["allowance_low"]); } catch { }
                                    try { allowance_mid = Convert.ToDouble(drold[0]["allowance_mid"]); } catch { }
                                    try { allowance_hight = Convert.ToDouble(drold[0]["allowance_hight"]); } catch { }
                                    try { allowance_type_id = drold[0]["allowance_type_id"].ToString(); } catch { }
                                    try { currency = drold[0]["allowance_unit"].ToString(); } catch { }


                                    if (true == true)
                                    {
                                        // ดูวันเวลาจาก Air ticket
                                        // ขาไปนับเวลาจาก departure time ไปถึง 00.00 ของวันเดินทางว่าตกที่ < 6 , > 6 < 12, >= 12
                                        // ถ้ามีหลายประเทศในวันนึงให้ดูว่าอยู่ประเทศไหนนานกว่ากัน ให้คิดเรทตามประเทศที่อยู่นานกว่า 
                                        // กรณีที่ให้ดูว่าอยู่ประเทศไหนนานกว่ากัน??? ต้องคำนวณจากเครื่องบินเที่ยวแรกลงจอด จนเครื่องเที่ยวปัจจุบันลงจอดอีกรอบ ถ้าเป็นเที่ยวไปนับจากเที่ยงคืนจนถึงเวลาที่บินออก
                                        // นับตามระยะเวลาเครื่องออก-ลง + รอขึ้นเครื่อง 
                                        // ขากลับนับจาก arrival time ย้อนกลับไปถึง 00.00 ของวันนั้นๆ 

                                        //s_next_time --> 24 - airticket_arrival_date time  
                                        DataRow[] drair_chk;//จะมีแค่รายการเดียวเสมอ
                                        // DataRow[] drair = dtair.Select("emp_id= '" + emp_id_select + "' and airticket_departure_date ='" + def_date + "' ");//จะมีแค่รายการเดียวเสมอ
                                        DataRow[] drair = dtair.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("airticket_departure_date") == def_date).ToArray(); //จะมีแค่รายการเดียวเสมอ
                                        if (irunning_rows == 1)
                                        {
                                            //วันแรก --> ขากลับนับจาก departure time ไปถึง 00.00 ของวันเดินทาง --> 13:00 => 24 - 13 = 11
                                            dTimeCheck_def = Convert.ToDouble(drair[0]["s_full_time"].ToString()) - Convert.ToDouble(drair[0]["s_departure_time"].ToString());

                                            zone_def = drair[0]["zone_to"].ToString();
                                            country_def = drair[0]["country_to"].ToString();

                                            zone_def_bf = zone_def;
                                            country_def_bf = country_def;
                                        }
                                        else if (dNows == dNowsTo)
                                        {
                                            // drair = dtair.Select("emp_id= '" + emp_id_select + "' and airticket_arrival_date ='" + def_date + "' ");//จะมีแค่รายการเดียวเสมอ
                                            drair = dtair.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("airticket_arrival_date") == def_date).ToArray();

                                            //วันสุดท้าย --> ขากลับนับจาก arrival time ย้อนกลับไปถึง 00.00 ของวันนั้นๆ  --> 13:00 => 13 = 13
                                            dTimeCheck_def = Convert.ToDouble(drair[0]["s_arrival_time"].ToString());

                                            zone_def = drair[0]["zone_from"].ToString();
                                            country_def = drair[0]["country_from"].ToString();
                                        }
                                        else
                                        {
                                            dTimeCheck_def = Convert.ToDouble("1440");

                                            Boolean bCheckCall = false;
                                            //กรณีที่มีข้อมูล departure time ตรงกัน
                                            if (drair.Length > 0)
                                            {
                                                bCheckCall = true;

                                                //กรณีที่ departure กับ arrival วันเดียวกัน เช่น 10 Nov 2021 - 10 Nov 2021 
                                                // drair_chk = dtair.Select("emp_id= '" + emp_id_select + "' and airticket_departure_date ='" + def_date + "' and airticket_arrival_date ='" + def_date + "' ");
                                                drair_chk = dtair.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("airticket_departure_date") == def_date && s.Field<string>("airticket_arrival_date") == def_date).ToArray();

                                                if (drair_chk.Length > 0)
                                                {
                                                    #region ใช้ในกรณีที่ วันเดียวกัน วันนัดไป ให้ตรวจสอบว่า อยู่ประเทศไหนนานกว่า 
                                                    //ขาไป นับจาก 0.00 น. ถึง departure time 
                                                    //ขากลับ นับจาก arrival ถึง 24.00 น.
                                                    // drair_chk = dtair.Select("emp_id= '" + emp_id_select + "' and airticket_departure_date ='" + def_date + "' and airticket_arrival_date ='" + def_date + "' ");
                                                    drair_chk = dtair.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("airticket_departure_date") == def_date && s.Field<string>("airticket_arrival_date") == def_date).ToArray();

                                                    if (Convert.ToDouble(drair_chk[0]["s_departure_time"].ToString()) > Convert.ToDouble(drair_chk[0]["s_next_time"].ToString()))
                                                    {
                                                        zone_def = drair_chk[0]["zone_from"].ToString();
                                                        country_def = drair_chk[0]["country_from"].ToString();
                                                    }
                                                    else
                                                    {
                                                        zone_def = drair_chk[0]["zone_to"].ToString();
                                                        country_def = drair_chk[0]["country_to"].ToString();
                                                    }
                                                    //นำไปใช้ลำดับถัดไป
                                                    zone_def_bf = drair_chk[0]["zone_to"].ToString();
                                                    country_def_bf = drair_chk[0]["country_to"].ToString();
                                                    #endregion ใช้ในกรณีที่ วันเดียวกัน วันนัดไป ให้ตรวจสอบว่า อยู่ประเทศไหนนานกว่า 
                                                }
                                                else
                                                {
                                                    #region ใช้ในกรณีที่ วันปัจุบัน ตรงกับ วัน departure ให้ตรวจสอบว่า อยู่ประเทศไหนนานกว่า  
                                                    //กรณีที่ departure กับ arrival คนละวัน  เช่น 11 Nov 2021 - 12 Nov 2021 or 11 Nov 2021 - 13 Nov 2021 
                                                    // drair_chk = dtair.Select("emp_id= '" + emp_id_select + "' and airticket_departure_date ='" + def_date + "'  ");
                                                    drair_chk = dtair.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("airticket_departure_date") == def_date).ToArray();

                                                    if (Convert.ToDouble(drair_chk[0]["diff_date"].ToString()) == 1)
                                                    {
                                                        //กรณีที่เป็นข้อมูลที่ transit เครื่องบินเกิน 1 วัน เช่น 11 Nov 2021 - 13 Nov 2021 
                                                        btransit = true;
                                                        zone_def = drair_chk[0]["zone_from"].ToString();
                                                        country_def = drair_chk[0]["country_from"].ToString();

                                                        zone_def_bf = zone_def.ToString();
                                                        country_def_bf = country_def.ToString();
                                                    }
                                                    #endregion ใช้ในกรณีที่ วันปัจุบัน ตรงกับ วัน departure ให้ตรวจสอบว่า อยู่ประเทศไหนนานกว่า 
                                                }


                                                if (drair_chk[0]["last_rows"].ToString() == "true") { blast_rows = true; }

                                            }
                                            else
                                            {
                                                #region ใช้ในกรณีที่ วันปัจุบัน ตรงกับ วัน arrival ให้ตรวจสอบว่า อยู่ประเทศไหนนานกว่า 
                                                //นับจาก arrival ถึง 24.00 น.
                                                // drair_chk = dtair.Select("emp_id= '" + emp_id_select + "' and airticket_arrival_date ='" + def_date + "'  ");
                                                drair_chk = dtair.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select && s.Field<string>("airticket_arrival_date") == def_date).ToArray();
                                                if (drair_chk.Length > 0)
                                                {
                                                    bCheckCall = true;
                                                    zone_def = drair_chk[0]["zone_to"].ToString();
                                                    country_def = drair_chk[0]["country_to"].ToString();

                                                    zone_def_bf = zone_def.ToString();
                                                    country_def_bf = country_def.ToString();
                                                }
                                                #endregion ใช้ในกรณีที่ วันปัจุบัน ตรงกับ วัน departure ให้ตรวจสอบว่า อยู่ประเทศไหนนานกว่า 

                                            }

                                            if (bCheckCall == false || (bCheckCall == false && btransit == true))
                                            {
                                                //กรณีที่เป็นวันที่อยู่ระหว่าง รายการ ไม่มีใน dtair และ กรณีที่เป็นวันที่อยู่ระหว่าง departure --> arrival
                                                dTimeCheck_def = Convert.ToDouble("1440");
                                                zone_def = zone_def_bf;
                                                country_def = country_def_bf;
                                            }
                                        }
                                        dTimeCheck_def = (dTimeCheck_def / 60);
                                    }

                                    //country + kh_code
                                    DataRow[] drrate = dtrate.AsEnumerable().Where(s => s.Field<string>("country") == country_def && s.Field<string>("kh_code") == kh_code_select).ToArray();
                                    if (drrate.Length == 0)
                                    {
                                        //zone + kh_code
                                        // drrate = dtrate.Select("zone='" + zone_def + "' and kh_code ='" + kh_code_select + "'");
                                        //drrate = dtrate.AsEnumerable().Where(s => s.Field<string>("zone") == zone_def && s.Field<string>("kh_code") == kh_code_select).ToArray();
                                        drrate = dtrate.AsEnumerable()
        .Where(s => s.Field<string>("zone") == zone_def
                && s.Field<string>("kh_code") == kh_code_select
                && string.IsNullOrEmpty(s.Field<string>("country")))
        .ToArray();
                                    }
                                    if (drrate.Length > 0)
                                    {
                                        string def_currency = drrate[0]["currency"].ToString();
                                        if (bCheckDateNewData == true) { currency = def_currency; }
                                        try
                                        {
                                            dAllowanceRate = Convert.ToDouble(drrate[0]["allowance_rate"].ToString());
                                        }
                                        catch { }
                                        if (def_currency.ToString() != "USD")
                                        {
                                            //กรณีที่เป็น THB ต้อง convert to USD
                                            // DataRow[] drex_rate = dtm_exchangerate.Select("def_date ='" + def_date + "'");
                                            DataRow[] drex_rate = dtm_exchangerate.AsEnumerable().Where(s => s.Field<string>("def_date") == def_date).ToArray();

                                            if (drex_rate.Length == 0)
                                            {
                                                drex_rate = dtm_exchangerate.Select("def_date > '" + def_date + "'");
                                            }
                                            if (drex_rate.Length > 0)
                                            {
                                                try
                                                {
                                                    ex_rate = Convert.ToDouble(drex_rate[0]["exchange_rate"].ToString());
                                                    currency = drex_rate[0]["currency_id"].ToString();
                                                    as_of = drex_rate[0]["def_date"].ToString();
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                //กรณีที่ไม่มีให้เอา ex rate ล่าสุดใน table
                                                try
                                                {
                                                    var value = dtm_exchangerate.Rows[0]["exchange_rate"].ToString();
                                                    Console.WriteLine(value); // ดูว่าค่าที่ได้คืออะไรจริงๆ
                                                    ex_rate = Convert.ToDouble(dtm_exchangerate.Rows[0]["exchange_rate"].ToString());
                                                }
                                                catch { }
                                            }
                                        }
                                    }


                                    dAllowance = (dAllowanceRate * ex_rate);

                                    if (bCheckDateNewData == true)
                                    {
                                        action_type = "insert";
                                        action_change = "true";

                                        //วันที่ไปวันแรก  
                                        //วันที่กลับวันสุดท้าย 
                                        if (dTimeCheck_def < 6)
                                        {
                                            allowance_low = dAllowance;
                                            allowance_low = 0;//กรณีที่เป็น ขาไป  
                                        }
                                        else if (dTimeCheck_def >= 12)
                                        {
                                            allowance_hight = dAllowance;
                                        }
                                        else
                                        {
                                            //กรณีนี้ให้ครึ่งเดียว
                                            allowance_mid = (dAllowance / 2);
                                        }
                                    }
                                    else
                                    {
                                        action_type = "update";
                                        action_change = "false";
                                    }

                                    dtnew.Rows[irows]["emp_id"] = emp_id_select;
                                    dtnew.Rows[irows]["exchange_rate"] = ex_rate;
                                    //dtnew.Rows[irows]["as_of"] = as_of;
                                    dtnew.Rows[irows]["allowance_unit"] = currency;

                                    dtnew.Rows[irows]["allowance_low"] = allowance_low;
                                    dtnew.Rows[irows]["allowance_mid"] = allowance_mid;
                                    dtnew.Rows[irows]["allowance_hight"] = allowance_hight;
                                    dtnew.Rows[irows]["allowance_unit"] = currency;
                                    dtnew.Rows[irows]["allowance_type_id"] = allowance_type_id;

                                    //เก็บค่าที่คำนวณจาก ระบบ
                                    dtnew.Rows[irows]["allowance_values_def"] = allowance_values;

                                    dtnew.Rows[irows]["action_type"] = action_type;
                                    dtnew.Rows[irows]["action_change"] = action_change;

                                    irunning_rows++;
                                }
                                #endregion calculate travel hr

                            }

                            if (dtnew.Rows[irows]["action_type"].ToString() == "insert")
                            {
                                dtnew.Rows[irows]["id"] = imaxid.ToString();
                                dtnew.AcceptChanges();
                                imaxid++;
                            }
                            irows++;
                        }

                    }
                }

                dt = new DataTable();
                dt = dtnew.Copy(); dt.AcceptChanges();
            }


            return dt;
        }
        public DataTable refdata_allowance_mail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_ALLOWANCE_MAIL); ;

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , a.mail_emp_id
                    , a.mail_to
                    , a.mail_cc 
                    , a.mail_bcc
                    , a.mail_status  
                    , a.mail_remark  
                     
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  

                    from bz_doc_head h
                    inner join  (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_allowance_mail a on h.dh_code = a.doc_id and ex.dte_emp_id = a.emp_id  
                    where h.dh_code in (select distinct doc_id from  bz_doc_airticket_booking where data_type ='submit')  
                    and h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_reimbursement(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_REIMBURSEMENT); ;

            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id 
                    , a.sendmail_to_traveler 
                    , null as remark
                    , file_travel_report, file_report
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                    , nvl(a.doc_status,1) as doc_status
                    from bz_doc_head h 
                    inner join  (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_reimbursement a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id  and ex.dte_emp_id = a.emp_id   
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_reimbursement_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_REIMBURSEMENT_DETAIL); ;

            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id 
                    , a.reimbursement_date
                    , a.details 
                    , a.exchange_rate
                    , a.currency  
                    , a.as_of 
                    , a.total 
                    , a.grand_total  
                    , a.remark
                    
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change

                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid 
                    left join bz_doc_reimbursement h2 on h.dh_code =  h2.doc_id and ex.dte_emp_id = h2.emp_id 
                    left join bz_doc_reimbursement_detail a on h.dh_code = a.doc_id  and h2.emp_id = a.emp_id  and ex.dte_emp_id = a.emp_id   
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_travelexpense(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            DataTable dtdetail = new DataTable();

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_TRAVELEXPENSE); ;


            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id 
                    , a.send_to_sap 
                    , a.remark as remark
                    
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 
                    , '1' as doc_status
                    , nvl(status_trip_cancelled,'false') as status_trip_cancelled

                    from bz_doc_head h 
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_travelexpense a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id  and ex.dte_emp_id = a.emp_id   
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_travelexpense_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();
            DataTable dtSAP = new DataTable();

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_TRAVELEXPENSE_DETAIL); ;


            sqlstr = @" select nvl(sap_obj_id,'') as sap_obj_id, nvl(ct_id,'') as ct_id, nvl(pv_id,'') as pv_id, emp_id
                         from BZ_DOC_TRAVELEXPENSE_SAP 
                         where type_main = 'true' and doc_id = '" + doc_id + "'";
            dtSAP = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dtSAP, sqlstr) == "")
            { }

            //allowan day เอามาจาก page allowance
            //ถ้า overea allowan day
            //ถ้า Local allowan day ,night
            sqlstr = @" select a.doc_id, a.emp_id, nvl(a.luggage_clothing,0) as luggage_clothing
                        , sum(case when h.dh_type like 'oversea%' then to_number(nvl(b.allowance_total,0)) else to_number(nvl(b.allowance_low,0)) end) as allowance_day
                        , sum(case when h.dh_type like 'oversea%' then 0 else to_number(nvl(b.allowance_hight,0)) end)  as allowance_night
                        from bz_doc_allowance a
                        inner join bz_doc_head h on a.doc_id = h.dh_code
                        left join bz_doc_allowance_detail b on a.doc_id = b.doc_id and a.emp_id = b.emp_id
                        where a.doc_id = '" + doc_id + "'" +
                        " group by a.doc_id, a.emp_id, nvl(a.luggage_clothing,0) ";
            DataTable dtallowan = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dtallowan, sqlstr) == "")
            { }

            //TRAVEL INSURANCE เอามาจาก page TRAVEL INSURANCE
            sqlstr = @" select a.doc_id, a.emp_id, nvl(a.certificates_total,0) as certificates_total
                        from bz_doc_insurance a 
                        where a.doc_id = '" + doc_id + "' " +
                        " group by  a.doc_id, a.emp_id, nvl(a.certificates_total, 0) ";
            DataTable dtinsurance = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dtinsurance, sqlstr) == "")
            { }
            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id 
                    , a.data_date
                    , a.status 
                    , a.exchange_rate
                    , a.currency  
                    , a.as_of 
                    , a.total 
                    , a.grand_total  
                    , a.remark
                    , met.id as expense_type
                    , met.name as expense_type_text
                    , nvl(a.status_active,'false') as status_active
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change
                    , a.ref_id, a.sap_obj_id as sap_status
                    from bz_doc_head h
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_doc_travelexpense h2 on h.dh_code = h2.doc_id and ex.dte_emp_id = h2.emp_id   
                    left join bz_master_expense_type met on (case when h.dh_type like 'oversea%' then met.status_active_oversea else met.status_active_local end ) = 'true'
                    left join bz_doc_travelexpense_detail a on h.dh_code = a.doc_id  and h2.emp_id = a.emp_id  and ex.dte_emp_id = a.emp_id   
                    and met.id = a.expense_type 
                    where 1=1";
            if (doc_id != "") { sqlstr += @" and h.dh_code = '" + doc_id + "' "; }
            sqlstr += @" order by ex.dte_emp_id,to_number(met.sort_by),to_number(a.id)";

            dt = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        string emp_id_select = dt.Rows[i]["emp_id"].ToString();
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;

                        DataRow[] dr;
                        try
                        {
                            //3   ALLOW_D
                            //4   ALLOW_N
                            //5   CLTH&LUG
                            if (dt.Rows[i]["expense_type"].ToString() == "3" ||
                                dt.Rows[i]["expense_type"].ToString() == "4" ||
                                dt.Rows[i]["expense_type"].ToString() == "5")
                            {
                                // dr = dtallowan.Select("emp_id ='" + emp_id_select + "'");
                                dr = dtallowan.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                                if (dr.Length > 0)
                                {
                                    if (dt.Rows[i]["expense_type"].ToString() == "3")
                                    {
                                        dt.Rows[i]["total"] = dr[0]["allowance_day"].ToString();
                                    }
                                    else if (dt.Rows[i]["expense_type"].ToString() == "4")
                                    {
                                        dt.Rows[i]["total"] = dr[0]["allowance_night"].ToString();
                                    }
                                    else if (dt.Rows[i]["expense_type"].ToString() == "5")
                                    {
                                        dt.Rows[i]["total"] = dr[0]["luggage_clothing"].ToString();
                                    }
                                }
                            }
                        }
                        catch { }

                        try
                        {
                            //8   T_INSUR
                            if (dt.Rows[i]["expense_type"].ToString() == "8")
                            {
                                // dr = dtinsurance.Select("emp_id ='" + emp_id_select + "'");
                                dr = dtinsurance.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                                if (dr.Length > 0)
                                {
                                    dt.Rows[i]["total"] = dr[0]["certificates_total"].ToString();
                                }
                            }
                        }
                        catch { }

                    }

                }
            }
            return dt;
        }

        public DataTable refdata_data_mail(string token_login, string doc_id, string page_name, string module, string emp_id, Boolean user_admin, ref int imaxid_mail)
        {
            string user_id = "";
            string user_role = "";

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_MAIL); ;

            if (page_name == "x")
            {
                sqlstr = @"
                        select a.doc_id,a.emp_id
                        , a.id 
                        , a.page_name 
                        , a.module  
                        , a.mail_emp_id
                        , a.mail_to
                        , a.mail_cc 
                        , a.mail_bcc
                        , a.mail_status  
                        , a.mail_remark  
                        , nvl(a.mail_type_many,'false') as mail_type_many
                        
                        , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 

                        from bz_data_mail a 
                        where lower(a.page_name) = lower('" + page_name + "') ";

                sqlstr += @" order by a.id";
            }
            else
            {
                sqlstr = @"
                            select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                            , a.id 
                            , a.page_name 
                            , a.module  
                            , a.mail_emp_id
                            , a.mail_to
                            , a.mail_cc 
                            , a.mail_bcc
                            , a.mail_status  
                            , a.mail_remark  
                            , nvl(a.mail_type_many,'false') as mail_type_many
                    
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change 

                            from bz_doc_head h
                            inner join  (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                            left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                            left join bz_data_mail a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id  and ex.dte_emp_id = a.emp_id   
                            where h.dh_code = '" + doc_id + "' ";
                if (page_name != "") { sqlstr += @" and lower(a.page_name) = lower('" + page_name + "') "; }
                if (module != "") { sqlstr += @" and lower(a.page_name) = lower('" + module + "') "; }
                sqlstr += @" order by ex.dte_emp_id,a.id";
            }


            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["action_type"].ToString() == "insert")
                        {
                            dt.Rows[i]["id"] = imaxid.ToString();
                            dt.AcceptChanges();
                            imaxid++;
                        }
                    }
                }
                else
                {
                    imaxid_mail = imaxid;
                }
            }

            return dt;
        }

        public DataTable refdata_Insurance_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();
            DataTable dtDef = new DataTable();

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_INSURANCE); ;

            sqlstr = @" select  name_beneficiary, relationship, emp_id
                        from bz_doc_insurance where (doc_id,emp_id,id) in (
                            select doc_id, emp_id, max(id) as id
                            from bz_doc_insurance where (update_date,emp_id) in (select max(update_date),emp_id from bz_doc_insurance group by emp_id  ) 
                            group by doc_id, emp_id
                        )  order by emp_id ";
            dtDef = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dtDef, sqlstr) == "")
            {
            }

            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , case when  a.ins_emp_id is not null then a.ins_emp_id else 
                        case when p.emp_id is null then b.employeeid else p.emp_id end 
                      end ins_emp_id
                    , case when  a.ins_emp_id is not null then a.ins_emp_name else 
                        case when p.emp_id is null then b.userdisplay else ( p.passport_title || ' ' || p.passport_name || ' ' || p.passport_surname ) end 
                      end ins_emp_name
                    , case when a.ins_emp_id is not null then a.ins_emp_org else b.orgname end ins_emp_org
                    , case when a.ins_emp_id is not null then a.ins_emp_passport else p.passport_no end ins_emp_passport_def
                    , nvl(p.passport_no,'') as ins_emp_passport 
                    , case when a.ins_emp_id is not null then a.ins_emp_age else 
                        case when  p.passport_date_birth is not null then to_char(to_number(to_char(sysdate,'rrrr'))  - to_number(to_char(to_date( p.passport_date_birth,'dd MON rrrr'),'rrrr'))) end
                      end ins_emp_age_def   
                    , to_char(to_number(to_char(sysdate,'rrrr'))  - to_number(to_char(to_date( p.passport_date_birth,'dd MON rrrr'),'rrrr')))  as ins_emp_age
                    
                    , case when a.ins_emp_id is not null then a.insurance_company else cc.key_value end insurance_company
                    , case when a.ins_emp_id is not null then a.ins_emp_address else ca.key_value end ins_emp_address 
                    
                    , nvl(a.ins_emp_occupation,'Employee') as ins_emp_occupation
                    , a.ins_emp_tel, a.ins_emp_fax
                    , a.ins_plan, a.ins_broker
 
                    , a.name_beneficiary 
                    , a.relationship  
                    , a.period_ins_dest
                    , a.period_ins_from  
                    , a.period_ins_to 
                    , a.destination 
                    , a.date_expire  
                    , case when a.duration is null then to_number(nvl((case when a.period_ins_to is not null and to_date(a.period_ins_from,'dd MON rrrr') is not null then  to_date(a.period_ins_to,'dd MON rrrr') - to_date(a.period_ins_from,'dd MON rrrr') end),0))
                      else  to_number(nvl(a.duration,0)) end duration

                    , a.billing_charge
                    , a.certificates_no
                    , a.certificates_total
                    , a.remark  
                    , a.sort_by  
 
                    , a.agent_type, a.broker_type, a.travel_agent_type
                     
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                    , nvl(a.doc_status,1) as doc_status

                    from bz_doc_head h  
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_data_passport p on ex.dte_emp_id = p.emp_id and p.default_type ='true' 
                    left join bz_doc_insurance a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id  and ex.dte_emp_id = a.emp_id 
                    left join bz_config_data cc on cc.status = 1 and cc.key_name ='Company Name'  and 'TOP'  = cc.key_filter
                    left join bz_config_data ca on ca.status = 1 and ca.key_name ='Company Address'  and 'TOP' = ca.key_filter 
                    where h.dh_code = '" + doc_id + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;

                        //กรณีที่เป็นข้อมูลใหม่ Name of Beneficiary, Relationship เป็นข้อมูลล่าสุดของคนนั้นที่เคยกรอกไป 
                        if (dtDef.Rows.Count > 0)
                        {
                            // DataRow[] dr = dtDef.Select("emp_id ='" + dt.Rows[i]["emp_id"].ToString() + "'");
                            DataRow[] dr = dtDef.AsEnumerable().Where(s => s.Field<string>("emp_id") == dt.Rows[i]["emp_id"].ToString()).ToArray();
                            if (dr.Length > 0)
                            {
                                dt.Rows[i]["name_beneficiary"] = dr[0]["name_beneficiary"].ToString();
                                dt.Rows[i]["relationship"] = dr[0]["relationship"].ToString();
                            }
                        }

                    }
                }
            }

            return dt;
        }
        public DataTable refdata_Insurance_detail_year(string token_login, string year)
        {
            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();
            DataTable dtDef = new DataTable();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_INSURANCE); ;

            sqlstr = @" select  name_beneficiary, relationship, emp_id
                        from bz_doc_insurance where (doc_id,emp_id,id) in (
                            select doc_id, emp_id, max(id) as id
                            from bz_doc_insurance where (update_date,emp_id) in (select max(update_date),emp_id from bz_doc_insurance group by emp_id  ) 
                            group by doc_id, emp_id
                        )  order by emp_id ";
            dtDef = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dtDef, sqlstr) == "")
            {
            }

            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , case when  a.ins_emp_id is not null then a.ins_emp_id else 
                        case when p.emp_id is null then b.employeeid else p.emp_id end 
                      end ins_emp_id
                    , case when  a.ins_emp_id is not null then a.ins_emp_name else 
                        case when p.emp_id is null then b.userdisplay else ( p.passport_title || ' ' || p.passport_name || ' ' || p.passport_surname ) end 
                      end ins_emp_name
                    , case when a.ins_emp_id is not null then a.ins_emp_org else b.orgname end ins_emp_org
                    , case when a.ins_emp_id is not null then a.ins_emp_passport else p.passport_no end ins_emp_passport_def
                    , nvl(p.passport_no,'') as ins_emp_passport 
                    , case when a.ins_emp_id is not null then a.ins_emp_age else 
                        case when  p.passport_date_birth is not null then to_char(to_number(to_char(sysdate,'rrrr'))  - to_number(to_char(to_date( p.passport_date_birth,'dd MON rrrr'),'rrrr'))) end
                      end ins_emp_age_def   
                    , to_char(to_number(to_char(sysdate,'rrrr'))  - to_number(to_char(to_date( p.passport_date_birth,'dd MON rrrr'),'rrrr')))  as ins_emp_age
                    
                    , case when a.ins_emp_id is not null then a.insurance_company else cc.key_value end insurance_company
                    , case when a.ins_emp_id is not null then a.ins_emp_address else ca.key_value end ins_emp_address 
                    
                    , nvl(a.ins_emp_occupation,'Employee') as ins_emp_occupation
                    , a.ins_emp_tel, a.ins_emp_fax
                    , a.ins_plan, a.ins_broker
 
                    , a.name_beneficiary 
                    , a.relationship  
                    , a.period_ins_dest
                    , a.period_ins_from  
                    , a.period_ins_to 
                    , a.destination 
                    , a.date_expire  
                    , case when a.duration is null then to_number(nvl((case when a.period_ins_to is not null and to_date(a.period_ins_from,'dd MON rrrr') is not null then  to_date(a.period_ins_to,'dd MON rrrr') - to_date(a.period_ins_from,'dd MON rrrr') end),0))
                      else  to_number(nvl(a.duration,0)) end duration

                    , a.billing_charge
                    , a.certificates_no
                    , a.certificates_total
                    , a.remark  
                    , a.sort_by  
 
                    , a.agent_type, a.broker_type, a.travel_agent_type
                     
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                     
                    from bz_doc_head h  
                    inner join  (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense)  ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_data_passport p on ex.dte_emp_id = p.emp_id and p.default_type ='true' 
                    left join bz_doc_insurance a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id  and ex.dte_emp_id = a.emp_id 
                    left join bz_config_data cc on cc.status = 1 and cc.key_name ='Company Name'  and b.companyname = cc.key_filter
                    left join bz_config_data ca on ca.status = 1 and ca.key_name ='Company Address'  and b.companyname = ca.key_filter 
                    where h.dh_code = '" + year + "' order by ex.dte_emp_id,a.id";

            dt = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;

                        //กรณีที่เป็นข้อมูลใหม่ Name of Beneficiary, Relationship เป็นข้อมูลล่าสุดของคนนั้นที่เคยกรอกไป 
                        if (dtDef.Rows.Count > 0)
                        {
                            // DataRow[] dr = dtDef.Select("emp_id ='" + dt.Rows[i]["emp_id"].ToString() + "'");
                            DataRow[] dr = dtDef.AsEnumerable().Where(s => s.Field<string>("emp_id") == dt.Rows[i]["emp_id"].ToString()).ToArray();
                            if (dr.Length > 0)
                            {
                                dt.Rows[i]["name_beneficiary"] = dr[0]["name_beneficiary"].ToString();
                                dt.Rows[i]["relationship"] = dr[0]["relationship"].ToString();
                            }
                        }

                    }
                }
            }

            return dt;
        }

        public DataTable refdata_ISOS_record(string doc_id)
        {
            sqlstr = " select a.isos_emp_id as emp_id from bz_doc_isos_record a where substr(a.year,3,2) = substr('" + doc_id + "',3,2) order by to_number(a.id) ";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
            }

            return dt;
        }

        public DataTable refdata_Feedback_detail(string token_login, string doc_id, string emp_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";

            var data = new TravelerHistoryOutModel();
            DataTable dtdetail = new DataTable();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_FEEDBACK); ;

            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, doc_id);

            sqlstr = @"
                    select h.dh_code as doc_id,ex.dte_emp_id as emp_id
                    , a.id
                    , t.id as feedback_type_id
                    , t.name as feedback_type_name
                    , l.id as feedback_list_id
                    , l.name as feedback_list_name
                    , q.id as feedback_question_id
                    , l.question_other
                    
                    , l.id as topic_id 
                    , l.name as topic_name
                    , null as subjective
                    , case when a.no is null then q.no else a.no end no
                    , case when a.question is null then q.question else a.question end question
                    , case when a.description is null then q.description else a.description end description
                    , a.answer
                    
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change   
                    , nvl(a.doc_status,1) as doc_status
                    
                    from bz_doc_head h 
                    inner join (select distinct dte_emp_id, dh_code from bz_doc_traveler_expense) ex on h.dh_code = ex.dh_code 
                    left join vw_bz_users b on ex.dte_emp_id = b.employeeid
                    left join bz_master_feedback_type t on 1=1
                    left join bz_master_feedback_list l on t.id = l.feedback_type_id  
                    left join bz_master_feedback_question q on  t.id = q.feedback_type_id and l.id = q.feedback_list_id    
                    
                    left join bz_doc_feedback a on h.dh_code =  a.doc_id and ex.dh_code =  a.doc_id  and ex.dte_emp_id = a.emp_id     
                    and t.id = a.feedback_type_id 
                    and l.id = a.feedback_list_id
                    and q.id = a.feedback_question_id 
                    where h.dh_code = '" + doc_id + "' " +
                  " order by ex.dte_emp_id, to_number(t.id), to_number(l.id) ,to_number(q.id) ,to_number(a.no) ";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        dt.Rows[i]["id"] = imaxid.ToString();
                        dt.AcceptChanges();
                        imaxid++;
                    }
                }
            }

            return dt;
        }
        public DataTable refdata_Portal(string token_login, ref Boolean user_admin, ref string id)
        {
            string user_id = "";
            string user_role = "";
            var data = new PortalOutModel();
            DataTable dtdetail = new DataTable();
            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, "");

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DOC_PORTAL); ;

            sqlstr = @" select id   
                        ,replace(img_header,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as img_header
                        ,replace(img_personal_profile,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as img_personal_profile
                        ,replace(img_banner_1,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as img_banner_1
                        ,replace(url_banner_1,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as url_banner_1
                        ,replace(img_banner_2,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as img_banner_2
                        ,replace(url_banner_2,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as url_banner_2
                        ,replace(img_banner_3,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as img_banner_3
                        ,replace(url_banner_3 ,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as url_banner_3
                        ,replace(img_practice_areas,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as img_practice_areas
                        ,replace(url_employee_privacy_center ,'http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/','') as url_employee_privacy_center
    
                        ,text_title
                        ,text_desc 
                        ,text_contact_title
                        ,text_contact_email
                        ,text_contact_tel 
                        ,'false' as action_change_text  
                        ,'update' as action_type
                        ,'false' as action_change
                        from  bz_doc_portal a ";
            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count == 0)
                {
                    //insert row ให้เลย 
                    var sqlstrinsert = "insert into bz_doc_portal (id) values ('" + imaxid + "')";
                    //SetDocService.conn_ExecuteNonQuery(sqlstrinsert, false);
                    if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
                    {
                        id = imaxid.ToString();
                    }
                }
                else
                {
                    id = dt.Rows[0]["id"].ToString();
                }
            }
            return dt;
        }
        public DataTable refdata_practice_areas(ref string url_approval, ref string url_employee_payment, ref string url_transportation, ref string url_others)
        {
            sqlstr = @"select key_value, key_filter from bz_config_data where status = 1 and key_name = 'URL PRACTICE AREAS' ";
            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["key_filter"].ToString() == "APPROVAL") { url_approval = dt.Rows[i]["key_value"].ToString(); }
                    if (dt.Rows[i]["key_filter"].ToString() == "EMPLOYEE PAYMENT") { url_employee_payment = dt.Rows[i]["key_value"].ToString(); }
                    if (dt.Rows[i]["key_filter"].ToString() == "TRANSPORTATION") { url_transportation = dt.Rows[i]["key_value"].ToString(); }
                    if (dt.Rows[i]["key_filter"].ToString() == "OTHERS") { url_others = dt.Rows[i]["key_value"].ToString(); }
                }
            }
            return dt;
        }

        public DataTable refdata_Up_coming_plan(string token_login, string doc_id, Boolean user_admin)
        {
            string user_id = "";
            string user_role = "";
            var data = new PortalOutModel();
            DataTable dtdetail = new DataTable();
            sw = new SetDocService();
            sw.sqlEmpRole(token_login, ref user_id, ref user_role, ref user_admin, "");


            Boolean pmdv_admin = false;
            string sql = " select emp_id from bz_data_manage where pmdv_admin = 'true' and emp_id = '" + user_id + "'";
            DataTable login_empid = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (login_empid != null && login_empid.Rows.Count > 0)
                {
                    pmdv_admin = true;
                }
            }

            sqlstr = @" select distinct to_char(a.business_date,'dd/MM/rrrr') as business_date_cn
                        , a.title as topic_of_traveling
                        , a.place as country
                        , a.business as business_date
                        , a.button_status
                        , a.doc_id
                        , '1' as tab_no, to_number(business_date_sort), to_number(business_date_to_sort), to_number(no)
                        from  vw_bz_plan_doc_list a
                        where 1=1 ";
            if (user_admin == false)
            {
                sqlstr += " and a.user_id = '" + user_id + "'";
            }
            if (pmdv_admin == true)
            {
                sqlstr += " and a.doc_id like '%T%'";
            }
            if ((doc_id + "") != "") { sqlstr += " and a.doc_id = '" + doc_id + "'"; }

            sqlstr += " order by to_number(business_date_sort) asc,to_number(business_date_to_sort) desc, a.doc_id desc, to_number(no)";

            sqlstr = "select * from(" + sqlstr + ")t where rownum <=50";
            dt = new DataTable();
            //
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    //เนื่องจากมี ข้อมูล location มากกว่า 1 ต้องรวมก่อน 
                    DataTable dtnew = new DataTable();
                    dtnew = dt.Clone(); dtnew.AcceptChanges();
                    string doc_id_bef = ""; string doc_id_aff = "";
                    int irows = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        doc_id_bef = dt.Rows[i]["doc_id"].ToString();
                        if (doc_id_bef != doc_id_aff)
                        {
                            doc_id_aff = doc_id_bef;
                            string country_def = "";
                            // DataRow[] dr = dt.Select("doc_id='" + doc_id_bef + "'"); $
                            DataRow[] dr = dt.AsEnumerable().Where(s => s.Field<string>("doc_id") == doc_id_bef).ToArray();
                            for (int k = 0; k < dr.Length; k++)
                            {
                                if (k > 0) { country_def += ","; }
                                country_def += dr[k]["country"].ToString();
                            }
                            dt.Rows[i]["country"] = country_def;

                            dtnew.ImportRow(dt.Rows[i]); dtnew.AcceptChanges();
                            irows++;
                            if (irows == 3) { break; }
                        }
                    }
                    dt = new DataTable();
                    dt = dtnew.Copy(); dt.AcceptChanges();
                }


                // Defualt tab
                sqlstr = @" select distinct h.dh_code as doc_id
                             , case when user_action.doc_code is null then to_char(s.ts_default_tabno) else to_char(user_action.tab_no) end tab_no
                             from BZ_DOC_HEAD h left join bz_master_status s on h.DH_DOC_STATUS = s.TS_ID 
                             left join (select dh_code doc_code, TAB_NO from bz_doc_action where action_status = 1   ";
                if (user_admin == false)
                {
                    sqlstr += " and emp_id = '" + user_id + "'";
                }
                if (doc_id != "") { sqlstr += " and dh_code = '" + doc_id + "'"; }

                sqlstr += " ) user_action on h.dh_code=user_action.doc_code   where (h.dh_doc_status not in ( 0, 10, 20) )  ";
                sqlstr += " order by  h.dh_code desc";
                DataTable dtDefTab = new DataTable();
                //
                if (SetDocService.conn_ExecuteData(ref dtDefTab, sqlstr) == "")
                {
                    if (dtDefTab.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string doc_id_select = dt.Rows[i]["doc_id"].ToString();
                            // DataRow[] dr = dtDefTab.Select("doc_id = '" + doc_id_select + "' ");$
                            DataRow[] dr = dtDefTab.AsEnumerable().Where(s => s.Field<string>("doc_id") == doc_id_select).ToArray();
                            if (dr.Length > 0)
                            {
                                dt.Rows[i]["tab_no"] = dr[0]["tab_no"].ToString();
                            }
                        }
                    }
                }


            }
            return dt;
        }

        public DataTable refdata_Role()
        {
            SetDocService sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_MANAGE); ;

            sqlstr = @"select a.id,u.employeeid as  emp_id
                    , (case when u.userid is null then a.user_id else u.userid end) as username
                    , case when  a.emp_id is null then a.user_id else (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) end displayname
                    , u.email as email
                    , u.orgname as idicator
                    , a.super_admin
                    , a.pmsv_admin
                    , a.pmdv_admin
                    , a.contact_admin 
                    , a.sort_by
                    , case when u.employeeid is null then 'false' else 'true' end user_in_ad
                    , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change   
                    from bz_data_manage a 
                    left join vw_bz_users u on (case when a.user_id is null then a.emp_id  else  a.user_id end) = (case when a.user_id is null then u.employeeid else u.userid end) 
                    order by to_number(a.id)";

            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {

            }

            return dt;
        }

        public DataTable refdata_resendemailv1(string doc_id)
        {
            SetDocService sw = new SetDocService();

            // สร้างคำสั่ง SQL พื้นฐาน
            // string sqlBase = @"select id, doc_id, stepflow, fromemail, torecipients, bccrecipients, ccrecipients, 
            //           subject, body, attachments, statussend, errorsend, datesend, 
            //           to_char(a.datesend,'dd Mon rrrr HH24:MI:SS') as datesendtext,
            //           activetype, 
            //           (case when a.id is null then 'insert' else 'update' end) action_type, 
            //           'false' as action_change   
            //           from BZ_EMAIL_DETAILS a";
            StringBuilder sqlBuilder = new StringBuilder(@"
       select id, doc_id, stepflow, fromemail, torecipients, bccrecipients, ccrecipients, 
                      subject, body, attachments, statussend, errorsend, datesend, 
                      to_char(a.datesend,'dd Mon rrrr HH24:MI:SS') as datesendtext,
                      activetype, 
                      (case when a.id is null then 'insert' else 'update' end) action_type, 
                      'false' as action_change   
                      from BZ_EMAIL_DETAILS a where 1=1
                                                 
                                                   ");
            List<OracleParameter> parameters = new();
            // สร้าง OracleCommand และเพิ่ม parameter อย่างปลอดภัย


            // เพิ่มเงื่อนไข WHERE ถ้ามี doc_id
            if (!string.IsNullOrEmpty(doc_id))
            {
                // cmd.CommandText += " where lower(a.doc_id) like lower(:doc_id)";
                sqlBuilder.AppendLine(" and lower(a.doc_id) like lower(:doc_id) ");
                // cmd.Parameters.Add("doc_id", OracleDbType.Varchar2).Value = "%" + doc_id + "%";
                parameters.Add(new("doc_id", "%" + doc_id + "%"));

            }

            // เรียงลำดับและจำกัดผลลัพธ์
            // cmd.CommandText += " order by datesend desc";
            sqlBuilder.AppendLine("order by datesend desc");
            //// จำกัดผลลัพธ์เฉพาะเมื่อไม่มี doc_id
            //if (string.IsNullOrEmpty(doc_id))
            //{
            //    cmd.CommandText += " fetch first 10000 rows only";
            //    // หรือใช้ syntax ของ Oracle รุ่นเก่า:
            //    // cmd.CommandText = $"select * from ({cmd.CommandText}) where rownum <= 100";
            //}
            OracleCommand cmd = new OracleCommand(sqlBuilder.ToString());
            cmd.Parameters.AddRange(parameters.ToArray());
            dt = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                // การประมวลผลเพิ่มเติมถ้ามี
            }

            return dt;
        }
        public DataTable refdata_resendemail(string doc_id, int pageIndex, int pageSize, out int totalCount)
        {
            totalCount = 0;
            SetDocService sw = new SetDocService();
            DataTable dt = new DataTable();

            // string baseSql = @"select id, doc_id, stepflow, fromemail, torecipients, bccrecipients, ccrecipients, 
            //    subject, body, attachments, statussend, errorsend, datesend, 
            //    to_char(a.datesend,'dd Mon rrrr HH24:MI:SS') as datesendtext,
            //    activetype, 
            //    (case when a.id is null then 'insert' else 'update' end) action_type, 
            //    'false' as action_change   
            //    from BZ_EMAIL_DETAILS a";
            StringBuilder sqlBuilder = new StringBuilder(@"
              select id, doc_id, stepflow, fromemail, torecipients, bccrecipients, ccrecipients, 
               subject, body, attachments, statussend, errorsend, datesend, 
               to_char(a.datesend,'dd Mon rrrr HH24:MI:SS') as datesendtext,
               activetype, 
               (case when a.id is null then 'insert' else 'update' end) action_type, 
               'false' as action_change   
               from BZ_EMAIL_DETAILS a where 1=1
              ");
            StringBuilder whereClause = new StringBuilder();
            List<OracleParameter> parameters = new();
            // string whereClause = "";
            // string fullSql = baseSql;

            bool isSearch = !string.IsNullOrEmpty(doc_id);

            if (isSearch)
            {
                // whereClause = " where lower(a.doc_id) like lower(:doc_id)";
                whereClause.AppendLine(" and lower(a.doc_id) like lower(:doc_id)  ");
                parameters.Add(new("doc_id", "%" + doc_id + "%"));
                // fullSql += whereClause;
            }
            sqlBuilder.AppendLine(whereClause.ToString());
            sqlBuilder.AppendLine(" order by datesend desc ");

            // --- ถ้า Search → ใช้ Pagination ---
            if (isSearch)
            {
                int offset = pageIndex * pageSize;
                // fullSql += " offset :offset rows fetch next :pagesize rows only";
                // cmd.Parameters.Add("offset", OracleDbType.Int32).Value = offset;
                // cmd.Parameters.Add("pagesize", OracleDbType.Int32).Value = pageSize;
                sqlBuilder.AppendLine(" offset :offset rows fetch next :pagesize rows only  ");
                parameters.Add(new("offset", offset));
                parameters.Add(new("pagesize", pageSize));
            }
            else
            {
                // --- ถ้าไม่ได้ Search → Limit 100 แถวแรก ---
                // fullSql += " fetch first 100 rows only";
                sqlBuilder.AppendLine(" fetch first 100 rows only  ");
            }


            // --- นับ totalCount ---
            if (isSearch)
            {
                StringBuilder countSql = new("select count(*) from BZ_EMAIL_DETAILS a where 1=1"); //"select count(*) from BZ_EMAIL_DETAILS a" + whereClause;
                countSql.AppendLine(whereClause.ToString());
                OracleCommand countCmd = new OracleCommand(countSql.ToString());
                countCmd.Parameters.Add("doc_id", OracleDbType.Varchar2).Value = "%" + doc_id + "%";

                DataTable dtCount = new DataTable();
                SetDocService.conn_ExecuteData(ref dtCount, countCmd);
                totalCount = Convert.ToInt32(dtCount.Rows[0][0]);
            }
            else
            {
                // --- ถ้าไม่ Search → totalCount = 100 ---
                totalCount = 100;
            }
            OracleCommand cmd = new OracleCommand(sqlBuilder.ToString());
            cmd.Parameters.AddRange(parameters.ToArray());

            SetDocService.conn_ExecuteData(ref dt, cmd);
            return dt;
        }

        #endregion  ref data

        #region  search data  
        private string refdoc_type(string filter_value)
        {
            string sqlstr = @"select distinct 
                     case when dh_type like 'local%' then 'local' else 'oversea' end doc_type 
                     from bz_doc_head h 
                     where h.dh_code = :filter_value";

            OracleCommand cmd = new OracleCommand(sqlstr);
            cmd.Parameters.Add("filter_value", OracleDbType.Varchar2).Value = filter_value;
            dt = new DataTable();

            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                try
                {
                    if (dt.Rows.Count > 0)
                    {
                        return dt.Rows[0]["doc_type"].ToString();
                    }
                }
                catch { }
            }
            return "";
        }
        private DataTable refsearch_emp_list(string filter_value)
        {
            filter_value = filter_value + "";

            var dt = new DataTable();

            sqlstr = @" select 0 as id,a.employeeid as emp_id,userid as username
                        , a.entitle as titlename, a.enfirstname as firstname, a.enlastname as lastname 
                        , case when usertype = 2 then a.enfirstname else a.entitle || ' ' || a.enfirstname || ' ' || a.enlastname end displayname
                        , a.email  
                        , a.orgname as idicator
                        , '' as category, a.sections, a.department, a.function
                        from vw_bz_users a where 1=1 ";
            if (filter_value != "")
            {
                sqlstr += @" and ( lower(a.entitle || ' ' || a.enfirstname || ' ' || a.enlastname) like lower('%" + filter_value + "%') ";
                sqlstr += @" or lower(a.email) like lower('%" + filter_value + "%') ";
                sqlstr += @" or lower(a.orgname) like lower('%" + filter_value + "%') )";
            }
            sqlstr += @" order by (a.enfirstname || ' ' || a.enlastname) ";

            dt = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["id"] = (i + 1).ToString();
                    }
                    dt.AcceptChanges();
                }
            }

            return dt;
        }
        public string getapprovedtraveleremails(string doc_id)
        {
            string traveler_mail = "";

            try
            {
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var sqlTravel = @"SELECT DISTINCT 
                                TO_CHAR(s.id) AS id, 
                                NVL(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME name1, 
                                b.email AS name2,  
                                b.employeeid AS name3, 
                                b.orgname AS name4,
                                a.dte_cap_appr_opt
                             FROM BZ_DOC_TRAVELER_EXPENSE a 
                             LEFT JOIN vw_bz_users b ON a.DTE_EMP_ID = b.employeeid 
                             LEFT JOIN (
                                 SELECT MIN(dte_id) AS id, dh_code, dte_emp_id 
                                 FROM BZ_DOC_TRAVELER_EXPENSE 
                                 GROUP BY dh_code, dte_emp_id
                             ) s ON a.dh_code = s.dh_code AND a.dte_emp_id = s.dte_emp_id 
                             WHERE a.dh_code = :doc_id 
                             AND a.dte_cap_appr_opt = 'true'
                             AND NVL(a.dte_status,0) <> 0 
                             ORDER BY s.id";

                    var parametersTravel = new List<OracleParameter>();
                    parametersTravel.Add(context.ConvertTypeParameter("doc_id", doc_id, "char"));

                    var tempTravel = context.TempModelList.FromSqlRaw(sqlTravel, parametersTravel.ToArray()).ToList();

                    if (tempTravel != null && tempTravel.Count > 0)
                    {
                        foreach (var item in tempTravel)
                        {
                            if (!string.IsNullOrEmpty(item.name2))
                            {
                                traveler_mail += (traveler_mail == "" ? "" : ";") + item.name2;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine("Error in GetApprovedTravelerEmails: " + ex.Message);
            }

            return traveler_mail;
        }
        public DataTable refsearch_empapprer_list(string doc_id)
        {
            var bGetLine = false;
            var bGetCAP = false;
            var dt = new DataTable();
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            //ต้องหา approver ตาม status ใบงาน
            #region approver mail in doc 
            string doc_status = "";
            sqlstr = @"SELECT a.dh_doc_status FROM BZ_DOC_HEAD a where a.DH_CODE = :doc_id";
            p.Add(new("doc_id", doc_id));
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            dt = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                //doc_status    
                //21  Pending by Super Admin  
                //23  Revise by Super Admin 
                //31  Pending by Line Approver
                //32  Approve by Line Approver 
                //41  Pending by CAP
                //42  Approved
                //50  Completed
                doc_status = dt.Rows[0]["dh_doc_status"].ToString();
            }
            p.Clear();
            if (doc_status == "21")
            {
                //ยังไม่ต้องแจ้ง approver เนื่องจากยังไม่ถึง step approver
            }
            else if (doc_status == "31")
            {
                //Pending by Line Approver
                bGetLine = true;
            }
            else if (doc_status == "41")
            {
                //Pending by Line Approver
                bGetLine = true;
                //Pending by CAP Approver
                bGetCAP = true;
            }
            else if (doc_status == "50")
            {
                //Pending by Line Approver
                bGetLine = true;
                //Pending by CAP Approver
                bGetCAP = true;
            }

            //DevFix 20210714 0000 เพิ่มสถานะที่ Line/CAP --> 1:Draft , 2:Pendding , 3:Approve , 4:Revise , 5:Reject , 6:Not Active 
            if (bGetLine == true)
            {
                //เอาเฉพาะที่ไม่ถูก reject
                sqlstr = @" select distinct '' as id, b.email                       
                        from bz_doc_traveler_approver a
                        inner join vw_bz_users b on a.dta_appr_empid = b.employeeid 
                        where a.dta_type = 1 and a.dta_action_status not in (5)  
                        and a.dh_code = :doc_id ";
                p.Add(new("doc_id", doc_id));

            }
            if (bGetCAP == true)
            {
                if (sqlstr != "") { sqlstr += " union "; }
                //เอาเฉพาะที่ไม่ถูก reject 
                sqlstr += @" select distinct '' as id, b.email                       
                        from bz_doc_traveler_approver a
                        inner join vw_bz_users b on a.dta_appr_empid = b.employeeid 
                        where a.dta_type = 2 and a.dta_action_status not in (5)  
                        and a.dh_code = :doc_id ";
                p.Add(new("doc_id", doc_id));

            }

            if (bGetLine == false && bGetCAP == false) { dt = new DataTable(); return dt; }
            sqlstr = "select distinct t.* from (" + sqlstr + ")t ";
            #endregion approver mail in doc

            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["id"] = (i + 1).ToString();
                    }
                    dt.AcceptChanges();
                }
            }
            p.Clear();
            return dt;
        }

        public DataTable refsearch_emprequester_list(string doc_id)
        {
            var dt = new DataTable();
            var p = new List<OracleParameter>();
            OracleCommand cmd;
            #region DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin

            sqlstr = @" SELECT '' as ID, EMPLOYEEID as user_id, EMAIL email FROM VW_BZ_USERS b
                                 WHERE EMPLOYEEID IN ( SELECT DH_CREATE_BY FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";
            p.Add(new("doc_id", doc_id));
            sqlstr += " union ";
            sqlstr += @" SELECT '' as ID, EMPLOYEEID user_id, EMAIL email FROM VW_BZ_USERS 
                                 WHERE EMPLOYEEID IN ( SELECT DH_BEHALF_EMP_ID FROM  BZ_DOC_HEAD WHERE DH_CODE = :doc_id)";
            p.Add(new("doc_id", doc_id));
            sqlstr += " union ";
            sqlstr += @"SELECT '' as ID, EMPLOYEEID user_id, EMAIL email FROM VW_BZ_USERS
                                     WHERE EMPLOYEEID in (select dh_initiator_empid from bz_doc_head where dh_code =:doc_id)  ";
            p.Add(new("doc_id", doc_id));

            sqlstr = "select distinct t.* from (" + sqlstr + ")t ";
            #endregion DevFix 20210729 0000 ส่งเมลแจ้งคนที่ Requester & On behalf of  &  cc initiator & admin 


            dt = new DataTable();
            cmd = new(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            if (SetDocService.conn_ExecuteData(ref dt, cmd) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["id"] = (i + 1).ToString();
                    }
                    dt.AcceptChanges();
                }
            }

            return dt;
        }


        public DataTable refsearch_emprole_list(string role_type)
        {
            role_type = role_type + "";

            var dt = new DataTable();
            //id,emp_id,username,name_th,name_en,email,telephone,mobile,imgurl
            sqlstr = @" select 0 as id,a.employeeid as emp_id,a.userid as username
                         ,trim(a.thtitle) ||' '|| trim(a.thfirstname) ||' '|| trim(a.thlastname) as name_th
                         ,case when usertype = 2 then a.enfirstname else a.entitle || ' ' || a.enfirstname || ' ' || a.enlastname end name_en
                         ,upper(a.email) as email
                         ,b.telephone
                         ,b.mobile 
                         ,case when b.imgurl is null then a.imgurl else b.imgurl end imgurl 
                         ,a.orgname as idicator
                         from vw_bz_users a
                         left join (select  b.userid, b.telephone, b.mobile, b.imgpath || b.imgprofilename  as imgurl from bz_user_peofile b
                         where (b.userid,nvl(b.update_date ,b.create_date )) in (
                         select userid,max(nvl(update_date ,create_date ))as  last_date  from bz_user_peofile  group by  userid)
                         ) b on lower(a.userid) = lower(b.userid)
                         where 1=1  ";
            if (role_type.ToLower() != "")
            {
                if (role_type.ToLower() == "pmsv_admin")
                {
                    sqlstr += @" and  lower(a.userid) in (select lower(user_id) as userid from bz_data_manage where pmsv_admin = 'true' or super_admin = 'true')";

                }
                else
                {
                    var sqlCondition = role_type switch
                    {
                        "pmsv_admin" => " and  lower(a.userid) in (select lower(user_id) as userid from bz_data_manage where pmsv_admin = 'true') ",
                        "pmdv_admin" => " and  lower(a.userid) in (select lower(user_id) as userid from bz_data_manage where pmdv_admin = 'true')",
                        "contact_admin" => " and lower(a.userid) in (select lower(user_id) as userid from bz_data_manage where contact_admin = 'true')",
                        _ => throw new Exception("not allow column")
                    };
                    // sqlstr += @" and  lower(a.userid) in (select lower(user_id) as userid from bz_data_manage where " + role_type.ToLower() + " = 'true')";
                    sqlstr += sqlCondition;
                }
            }
            sqlstr += @" order by (a.enfirstname || ' ' || a.enlastname) ";

            dt = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["id"] = (i + 1).ToString();
                    }
                    dt.AcceptChanges();
                }
            }

            return dt;
        }
        public DataTable refsearch_travelrecordv1(string token_login, string doc_id, string country, string date_from, string date_to, string travel_type, string emp_id, string section, string department
            , string function, List<traveltypeList> travel_list)
        {
            //แบบ dynamic --> adb pm แจ้งให้ hole ไว้ก่อน
            //DataTable dtm_expense_type = ref_expense_type("true"); 
            //DataTable dttravelexpense = refdata_travelexpense_detail(token_login, "", "", false);
            Boolean bCheckFilter = false;
            var dt = new DataTable();
            sqlstr = @"select 0 as no, b.employeeid as emp_id, h.dh_code as doc_id 
                            , nvl(b.entitle, '') as emp_title 
                            , b.enfirstname || ' ' || b.enlastname as emp_name
                            , '' as category, b.sections as section, b.department, b.function 
                          
                            , ms.ts_name as travel_status 
                            , '' as in_house
                            , h.dh_topic as travel_topic 
                            , mc.CT_NAME as country
                            , case when h.dh_type like 'oversea%' then  h.dh_city else mp.pv_name end city_province
                            , to_char(te.dte_bus_fromdate,'dd Mon rrrr') as date_from
                            , to_char(te.dte_bus_todate,'dd Mon rrrr') as date_to
                            , to_number(nvl(( case when te.dte_bus_todate is not null and te.dte_bus_fromdate is not null then te.dte_bus_todate - te.dte_bus_fromdate end ),0))  as duration 
                            
                            , te.dte_total_expense as estimate_expense
                            , te.dte_gl_account as gl_account
                            , te.dte_cost_center as cost_center
                            , te.dte_order_wbs as order_wbs
                            
                            , case when a1.grand_total is not null then replace(to_char(to_number(a1.grand_total),'9,999,999.99'),' ') else '' end accommodation
                            , case when a2.grand_total is not null then replace(to_char(to_number(a2.grand_total),'9,999,999.99'),' ') else '' end air_ticket
                            , case when a3.grand_total is not null then replace(to_char(to_number(a3.grand_total),'9,999,999.99'),' ') else '' end allowance_day
                            , case when a4.grand_total is not null then replace(to_char(to_number(a4.grand_total),'9,999,999.99'),' ') else '' end allowance_night
                            , case when a5.grand_total is not null then replace(to_char(to_number(a5.grand_total),'9,999,999.99'),' ') else '' end clothing_luggage
                            , case when a6.grand_total is not null then replace(to_char(to_number(a6.grand_total),'9,999,999.99'),' ') else '' end course_fee
                            , case when a7.grand_total is not null then replace(to_char(to_number(a7.grand_total),'9,999,999.99'),' ') else '' end instruction_fee
                            , case when a8.grand_total is not null then replace(to_char(to_number(a8.grand_total),'9,999,999.99'),' ') else '' end miscellaneous
                            , case when a9.grand_total is not null then replace(to_char(to_number(a9.grand_total),'9,999,999.99'),' ') else '' end passport
                            , case when a10.grand_total is not null then replace(to_char(to_number(a10.grand_total),'9,999,999.99'),' ') else '' end transportation
                            , case when a11.grand_total is not null then replace(to_char(to_number(a11.grand_total),'9,999,999.99'),' ') else '' end visa_fee  
                            , case when (  nvl(a1.grand_total,0) + nvl(a2.grand_total,0) + nvl(a3.grand_total,0) + nvl(a4.grand_total,0) + nvl(a5.grand_total,0)
                             + nvl(a6.grand_total,0) + nvl(a7.grand_total,0) + nvl(a8.grand_total,0) + nvl(a9.grand_total,0) + nvl(a10.grand_total,0) 
                             + nvl(a11.grand_total,0)) <> 0 then replace(to_char(to_number(
                               nvl(a1.grand_total,0) + nvl(a2.grand_total,0) + nvl(a3.grand_total,0) + nvl(a4.grand_total,0) + nvl(a5.grand_total,0)
                             + nvl(a6.grand_total,0) + nvl(a7.grand_total,0) + nvl(a8.grand_total,0) + nvl(a9.grand_total,0) + nvl(a10.grand_total,0) 
                             + nvl(a11.grand_total,0)
                            ),'9,999,999.99'),' ') else '' end total   

                            , h.dh_code as doc_id
                            , te.ct_id as countryid
                            , lower(substr(h.dh_code,0,2)) as travel_type
                            from vw_bz_users b
                            inner join bz_doc_traveler_expense te on b.employeeid = te.dte_emp_id 
                            inner join bz_doc_head h on h.dh_code = te.dh_code 
                            inner join bz_master_status ms on h.dh_doc_status = ms.ts_id
                            left join bz_master_country mc on te.ct_id = mc.ct_id 
                            left join bz_master_province mp on te.pv_id = mp.pv_id
                            left join bz_doc_travelexpense_detail a1 on b.employeeid = a1.emp_id  and te.dte_emp_id = a1.emp_id  and h.dh_code = a1.doc_id and a1.expense_type = 1
                            left join bz_doc_travelexpense_detail a2 on b.employeeid = a2.emp_id  and te.dte_emp_id = a2.emp_id  and h.dh_code = a2.doc_id and a2.expense_type = 2
                            left join bz_doc_travelexpense_detail a3 on b.employeeid = a3.emp_id  and te.dte_emp_id = a3.emp_id  and h.dh_code = a3.doc_id and a3.expense_type = 3
                            left join bz_doc_travelexpense_detail a4 on b.employeeid = a4.emp_id  and te.dte_emp_id = a4.emp_id  and h.dh_code = a4.doc_id and a4.expense_type = 4
                            left join bz_doc_travelexpense_detail a5 on b.employeeid = a5.emp_id  and te.dte_emp_id = a5.emp_id  and h.dh_code = a5.doc_id and a5.expense_type = 5
                            left join bz_doc_travelexpense_detail a6 on b.employeeid = a6.emp_id  and te.dte_emp_id = a6.emp_id  and h.dh_code = a6.doc_id and a6.expense_type = 6
                            left join bz_doc_travelexpense_detail a7 on b.employeeid = a7.emp_id  and te.dte_emp_id = a7.emp_id  and h.dh_code = a7.doc_id and a7.expense_type = 7
                            left join bz_doc_travelexpense_detail a8 on b.employeeid = a8.emp_id  and te.dte_emp_id = a8.emp_id  and h.dh_code = a8.doc_id and a8.expense_type = 8
                            left join bz_doc_travelexpense_detail a9 on b.employeeid = a9.emp_id  and te.dte_emp_id = a9.emp_id  and h.dh_code = a9.doc_id and a9.expense_type = 9
                            left join bz_doc_travelexpense_detail a10 on b.employeeid = a10.emp_id  and te.dte_emp_id = a10.emp_id  and h.dh_code = a10.doc_id and a10.expense_type = 10
                            left join bz_doc_travelexpense_detail a11 on b.employeeid = a11.emp_id  and te.dte_emp_id = a11.emp_id  and h.dh_code = a11.doc_id and a11.expense_type = 11 ";

            sqlstr += @" where 1=1 ";

            // ไม่เอา doc ที่ถูก cancle
            sqlstr += @" and h.dh_code not in (select doc_id from BZ_DOC_TRAVELEXPENSE where STATUS_TRIP_CANCELLED = 'true') ";

            #region Filter
            if (doc_id.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and  lower(h.dh_code) =  lower('" + doc_id + "')";
            }
            if (country.ToLower() != "")
            {
                bCheckFilter = true;
                //ต้องดักเพิ่มกรณีที่เป็น แบบ local ต้องไปเช็คที่ pv_id 
                sqlstr += @" and  (( h.dh_type like 'oversea%' and lower(te.ct_id) =  lower('" + country + "')  )  or  ( h.dh_type like 'local%' and te.ct_id = 19 and lower(te.pv_id) =  lower('" + country + "')  ) )";
            }
            if (date_from.ToLower() != "" && date_to.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and ( ";
                sqlstr += @" to_number(to_char(te.dte_bus_fromdate,'rrrrMMdd')) >= to_number(to_char(to_date('" + date_from + "','dd Mon rrrr'),'rrrrmmdd')) ";
                sqlstr += @" and  ";
                sqlstr += @" to_number(to_char(te.dte_bus_todate,'rrrrMMdd')) <= to_number(to_char(to_date('" + date_to + "','dd Mon rrrr'),'rrrrmmdd')) ";
                sqlstr += @" )  ";
            }
            else if (date_from.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and  to_number(to_char(te.dte_bus_fromdate,'rrrrMMdd')) = to_number(to_char(to_date('" + date_from + "','dd Mon rrrr'),'rrrrmmdd')) ";
            }
            else if (date_to.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and  to_number(to_char(te.dte_bus_todate,'rrrrMMdd')) = to_number(to_char(to_date('" + date_to + "','dd Mon rrrr'),'rrrrmmdd')) ";
            }

            string xtravel_list = "";
            for (int ii = 0; ii < travel_list.Count; ii++)
            {
                if (travel_list[ii].id.ToString() != "")
                {
                    if (xtravel_list != "") { xtravel_list += ","; }
                    xtravel_list += "'" + travel_list[ii].id.ToString() + "'";
                }
            }
            if (xtravel_list == "")
            {
                if (travel_type.ToLower() != "")
                {
                    bCheckFilter = true;
                    sqlstr += @" and lower(substr(h.dh_code,0,2)) =  lower('" + travel_type + "')";
                }
            }
            else
            {
                bCheckFilter = true;
                sqlstr += @" and lower(substr(h.dh_code,0,2)) in (" + xtravel_list.ToLower() + ")";
            }

            if (emp_id.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and b.employeeid =  '" + emp_id + "' ";
            }
            if (section.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and lower(b.sections) =  lower('" + section + "')";
            }
            if (department.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and lower(b.department) =  lower('" + department + "')";
            }
            if (function.ToLower() != "")
            {
                bCheckFilter = true;
                sqlstr += @" and lower(b.function) =  lower('" + function + "')";
            }

            #endregion Filter 
            if (bCheckFilter == false)
            {
                sqlstr += @" and ROWNUM <= 100 ";
            }

            sqlstr += @" order by b.employeeid, h.dh_code ";

            dt = new DataTable();
            string ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);
            if (ret == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["no"] = (i + 1).ToString();
                }
                dt.AcceptChanges();


                //เพิ่มกรองข้อมูลตาม User โดยเช็คจาก token_login
            }

            return dt;
        }
        public DataTable refsearch_travelrecordV2(string token_login, string doc_id, string country, string date_from, string date_to, string travel_type, string emp_id, string section, string department, string function, List<traveltypeList> travel_list)
        {
            Boolean bCheckFilter = false;
            var dt = new DataTable();

            // ใช้ SQL query ที่แก้ไขแล้ว
            sqlstr = @"select 0 as no, b.employeeid as emp_id, h.dh_code as doc_id 
                , nvl(b.entitle, '') as emp_title 
                , b.enfirstname || ' ' || b.enlastname as emp_name
                , '' as category, b.sections as section, b.department, b.function 
              
                , ms.ts_name as travel_status 
                , '' as in_house
                , h.dh_topic as travel_topic 
                , mc.CT_NAME as country
                , case when h.dh_type like 'oversea%' then h.dh_city else mp.pv_name end city_province
                , to_char(te.dte_bus_fromdate,'dd Mon rrrr') as date_from
                , to_char(te.dte_bus_todate,'dd Mon rrrr') as date_to
                , to_number(nvl((case when te.dte_bus_todate is not null and te.dte_bus_fromdate is not null then te.dte_bus_todate - te.dte_bus_fromdate end),0)) as duration 
                
                , te.dte_total_expense as estimate_expense
                , te.dte_gl_account as gl_account
                , te.dte_cost_center as cost_center
                , te.dte_order_wbs as order_wbs
                
                , case when a1.grand_total is not null then replace(to_char(to_number(replace(nvl(a1.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end accommodation
                , case when a2.grand_total is not null then replace(to_char(to_number(replace(nvl(a2.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end air_ticket
                , case when a3.grand_total is not null then replace(to_char(to_number(replace(nvl(a3.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end allowance_day
                , case when a4.grand_total is not null then replace(to_char(to_number(replace(nvl(a4.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end allowance_night
                , case when a5.grand_total is not null then replace(to_char(to_number(replace(nvl(a5.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end clothing_luggage
                , case when a6.grand_total is not null then replace(to_char(to_number(replace(nvl(a6.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end course_fee
                , case when a7.grand_total is not null then replace(to_char(to_number(replace(nvl(a7.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end instruction_fee
                , case when a8.grand_total is not null then replace(to_char(to_number(replace(nvl(a8.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end miscellaneous
                , case when a9.grand_total is not null then replace(to_char(to_number(replace(nvl(a9.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end passport
                , case when a10.grand_total is not null then replace(to_char(to_number(replace(nvl(a10.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end transportation
                , case when a11.grand_total is not null then replace(to_char(to_number(replace(nvl(a11.grand_total,'0'), ',', '')),'9,999,999.99'),' ') else '' end visa_fee  
                , case when (
                    nvl(to_number(replace(a1.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a2.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a3.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a4.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a5.grand_total, ',', '')),0) +
                    nvl(to_number(replace(a6.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a7.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a8.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a9.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a10.grand_total, ',', '')),0) +
                    nvl(to_number(replace(a11.grand_total, ',', '')),0)
                  ) <> 0 then replace(to_char(
                    nvl(to_number(replace(a1.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a2.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a3.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a4.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a5.grand_total, ',', '')),0) +
                    nvl(to_number(replace(a6.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a7.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a8.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a9.grand_total, ',', '')),0) + 
                    nvl(to_number(replace(a10.grand_total, ',', '')),0) +
                    nvl(to_number(replace(a11.grand_total, ',', '')),0)
                  ,'9,999,999.99'),' ') else '' end total   

                , h.dh_code as doc_id
                , te.ct_id as countryid
                , lower(substr(h.dh_code,0,2)) as travel_type
                from vw_bz_users b
                inner join bz_doc_traveler_expense te on b.employeeid = te.dte_emp_id 
                inner join bz_doc_head h on h.dh_code = te.dh_code 
                inner join bz_master_status ms on h.dh_doc_status = ms.ts_id
                left join bz_master_country mc on te.ct_id = mc.ct_id 
                left join bz_master_province mp on te.pv_id = mp.pv_id
                left join bz_doc_travelexpense_detail a1 on b.employeeid = a1.emp_id and te.dte_emp_id = a1.emp_id and h.dh_code = a1.doc_id and a1.expense_type = 1
                left join bz_doc_travelexpense_detail a2 on b.employeeid = a2.emp_id and te.dte_emp_id = a2.emp_id and h.dh_code = a2.doc_id and a2.expense_type = 2
                left join bz_doc_travelexpense_detail a3 on b.employeeid = a3.emp_id and te.dte_emp_id = a3.emp_id and h.dh_code = a3.doc_id and a3.expense_type = 3
                left join bz_doc_travelexpense_detail a4 on b.employeeid = a4.emp_id and te.dte_emp_id = a4.emp_id and h.dh_code = a4.doc_id and a4.expense_type = 4
                left join bz_doc_travelexpense_detail a5 on b.employeeid = a5.emp_id and te.dte_emp_id = a5.emp_id and h.dh_code = a5.doc_id and a5.expense_type = 5
                left join bz_doc_travelexpense_detail a6 on b.employeeid = a6.emp_id and te.dte_emp_id = a6.emp_id and h.dh_code = a6.doc_id and a6.expense_type = 6
                left join bz_doc_travelexpense_detail a7 on b.employeeid = a7.emp_id and te.dte_emp_id = a7.emp_id and h.dh_code = a7.doc_id and a7.expense_type = 7
                left join bz_doc_travelexpense_detail a8 on b.employeeid = a8.emp_id and te.dte_emp_id = a8.emp_id and h.dh_code = a8.doc_id and a8.expense_type = 8
                left join bz_doc_travelexpense_detail a9 on b.employeeid = a9.emp_id and te.dte_emp_id = a9.emp_id and h.dh_code = a9.doc_id and a9.expense_type = 9
                left join bz_doc_travelexpense_detail a10 on b.employeeid = a10.emp_id and te.dte_emp_id = a10.emp_id and h.dh_code = a10.doc_id and a10.expense_type = 10
                left join bz_doc_travelexpense_detail a11 on b.employeeid = a11.emp_id and te.dte_emp_id = a11.emp_id and h.dh_code = a11.doc_id and a11.expense_type = 11 ";

            sqlstr += @" where 1=1 ";

            // ไม่เอา doc ที่ถูก cancel
            sqlstr += @" and h.dh_code not in (select doc_id from BZ_DOC_TRAVELEXPENSE where STATUS_TRIP_CANCELLED = 'true') ";

            #region Filter
            if (!string.IsNullOrEmpty(doc_id))
            {
                bCheckFilter = true;
                sqlstr += @" and lower(h.dh_code) = lower('" + doc_id.Replace("'", "''") + "')";
            }
            if (!string.IsNullOrEmpty(country))
            {
                bCheckFilter = true;
                sqlstr += @" and ((h.dh_type like 'oversea%' and lower(te.ct_id) = lower('" + country.Replace("'", "''") + "')) or (h.dh_type like 'local%' and te.ct_id = 19 and lower(te.pv_id) = lower('" + country.Replace("'", "''") + "'))";
            }
            if (!string.IsNullOrEmpty(date_from) && !string.IsNullOrEmpty(date_to))
            {
                bCheckFilter = true;
                sqlstr += @" and (";
                sqlstr += @" to_number(to_char(te.dte_bus_fromdate,'rrrrMMdd')) >= to_number(to_char(to_date('" + date_from.Replace("'", "''") + "','dd Mon rrrr'),'rrrrmmdd')) ";
                sqlstr += @" and ";
                sqlstr += @" to_number(to_char(te.dte_bus_todate,'rrrrMMdd')) <= to_number(to_char(to_date('" + date_to.Replace("'", "''") + "','dd Mon rrrr'),'rrrrmmdd')) ";
                sqlstr += @") ";
            }
            else if (!string.IsNullOrEmpty(date_from))
            {
                bCheckFilter = true;
                sqlstr += @" and to_number(to_char(te.dte_bus_fromdate,'rrrrMMdd')) = to_number(to_char(to_date('" + date_from.Replace("'", "''") + "','dd Mon rrrr'),'rrrrmmdd')) ";
            }
            else if (!string.IsNullOrEmpty(date_to))
            {
                bCheckFilter = true;
                sqlstr += @" and to_number(to_char(te.dte_bus_todate,'rrrrMMdd')) = to_number(to_char(to_date('" + date_to.Replace("'", "''") + "','dd Mon rrrr'),'rrrrmmdd')) ";
            }

            string xtravel_list = "";
            for (int ii = 0; ii < travel_list.Count; ii++)
            {
                if (!string.IsNullOrEmpty(travel_list[ii].id.ToString()))
                {
                    if (xtravel_list != "") { xtravel_list += ","; }
                    xtravel_list += "'" + travel_list[ii].id.ToString().Replace("'", "''") + "'";
                }
            }
            if (string.IsNullOrEmpty(xtravel_list))
            {
                if (!string.IsNullOrEmpty(travel_type))
                {
                    bCheckFilter = true;
                    sqlstr += @" and lower(substr(h.dh_code,0,2)) = lower('" + travel_type.Replace("'", "''") + "')";
                }
            }
            else
            {
                bCheckFilter = true;
                sqlstr += @" and lower(substr(h.dh_code,0,2)) in (" + xtravel_list.ToLower() + ")";
            }

            if (!string.IsNullOrEmpty(emp_id))
            {
                bCheckFilter = true;
                sqlstr += @" and b.employeeid = '" + emp_id.Replace("'", "''") + "' ";
            }
            if (!string.IsNullOrEmpty(section))
            {
                bCheckFilter = true;
                sqlstr += @" and lower(b.sections) = lower('" + section.Replace("'", "''") + "')";
            }
            if (!string.IsNullOrEmpty(department))
            {
                bCheckFilter = true;
                sqlstr += @" and lower(b.department) = lower('" + department.Replace("'", "''") + "')";
            }
            if (!string.IsNullOrEmpty(function))
            {
                bCheckFilter = true;
                sqlstr += @" and lower(b.function) = lower('" + function.Replace("'", "''") + "')";
            }
            #endregion Filter 

            if (bCheckFilter == false)
            {
                sqlstr += @" and ROWNUM <= 100 ";
            }

            sqlstr += @" order by b.employeeid, h.dh_code ";

            dt = new DataTable();
            string ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);
            if (ret == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["no"] = (i + 1).ToString();
                }
                dt.AcceptChanges();
            }

            return dt;
        }
        public DataTable refsearch_travelrecord(
    string token_login,
    string doc_id,
    string country,
    string date_from,
    string date_to,
    string travel_type,
    string emp_id,
    string section,
    string department,
    string function_,
    List<traveltypeList> travel_list)
        {
            var dt = new DataTable();
            var paramList = new List<OracleParameter>();

            var sqlstr = @"
    SELECT 
    0 as no, 
    b.employeeid as emp_id, 
    h.dh_code as doc_id,
    NVL(b.entitle, '') as emp_title,
    b.enfirstname || ' ' || b.enlastname as emp_name,
    '' as category, 
    b.sections as section, 
    b.department, 
    b.function,
    ms.ts_name as travel_status,
    '' as in_house,
    h.dh_topic as travel_topic,
    mc.CT_NAME as country,
    CASE WHEN h.dh_type like 'oversea%' THEN h.dh_city ELSE mp.pv_name END city_province,
    TO_CHAR(te.dte_bus_fromdate, 'dd Mon yyyy') as date_from,
    TO_CHAR(te.dte_bus_todate, 'dd Mon yyyy') as date_to,
    CASE 
        WHEN te.dte_bus_todate IS NOT NULL AND te.dte_bus_fromdate IS NOT NULL 
        THEN te.dte_bus_todate - te.dte_bus_fromdate 
        ELSE 0 
    END as duration,
    te.dte_total_expense as estimate_expense,
    te.dte_gl_account as gl_account,
    te.dte_cost_center as cost_center,
    te.dte_order_wbs as order_wbs,
    CASE 
        WHEN TRIM(a1.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a1.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN TO_NUMBER(REPLACE(a1.grand_total, ',', '')) 
        ELSE 0 
    END AS air_ticket,
    CASE 
        WHEN TRIM(a2.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a2.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN TO_NUMBER(REPLACE(a2.grand_total, ',', '')) 
        ELSE 0 
    END AS accommodation,
    CASE 
        WHEN TRIM(a3.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a3.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a3.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END allowance_day,
    CASE 
        WHEN TRIM(a4.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a4.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a4.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END allowance_night,
    CASE 
        WHEN TRIM(a5.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a5.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a5.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END clothing_luggage,
    CASE 
        WHEN TRIM(a6.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a6.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a6.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END passport,
    CASE 
        WHEN TRIM(a7.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a7.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a7.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END visa_fee,
    CASE 
        WHEN TRIM(a8.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a8.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a8.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END travel_insurance,
    CASE 
        WHEN TRIM(a9.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a9.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a9.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END transportation,
    CASE 
        WHEN TRIM(a10.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a10.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a10.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END course_fee,
    CASE 
        WHEN TRIM(a11.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a11.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a11.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END instruction_fee,
    CASE 
        WHEN TRIM(a12.grand_total) IS NOT NULL AND REGEXP_LIKE(TRIM(REPLACE(a12.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') 
        THEN REPLACE(TO_CHAR(TO_NUMBER(REPLACE(a12.grand_total, ',', '')),'9,999,999.99'), ' ') 
        ELSE '' 
    END miscellaneous,
    CASE 
        WHEN (
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a1.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a1.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a2.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a2.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a3.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a3.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a4.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a4.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a5.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a5.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a6.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a6.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a7.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a7.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a8.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a8.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a9.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a9.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a10.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a10.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a11.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a11.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a12.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a12.grand_total, ',', '')) ELSE 0 END, 0)
        ) <> 0 
        THEN REPLACE(TO_CHAR(
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a1.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a1.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a2.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a2.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a3.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a3.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a4.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a4.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a5.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a5.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a6.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a6.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a7.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a7.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a8.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a8.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a9.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a9.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a10.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a10.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a11.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a11.grand_total, ',', '')) ELSE 0 END, 0) +
            NVL(CASE WHEN REGEXP_LIKE(TRIM(REPLACE(a12.grand_total, ',', '')), '^[0-9]+(\.[0-9]*)?$') THEN TO_NUMBER(REPLACE(a12.grand_total, ',', '')) ELSE 0 END, 0)
        , '9,999,999.99'), ' ') 
        ELSE '' 
    END total,
    h.dh_code as doc_id,
    te.ct_id as countryid,
    LOWER(SUBSTR(h.dh_code, 0, 2)) as travel_type
FROM vw_bz_users b
INNER JOIN bz_doc_traveler_expense te ON b.employeeid = te.dte_emp_id 
INNER JOIN bz_doc_head h ON h.dh_code = te.dh_code 
INNER JOIN bz_master_status ms ON h.dh_doc_status = ms.ts_id
LEFT JOIN bz_master_country mc ON te.ct_id = mc.ct_id 
LEFT JOIN bz_master_province mp ON te.pv_id = mp.pv_id
LEFT JOIN bz_doc_travelexpense_detail a1 ON b.employeeid = a1.emp_id AND h.dh_code = a1.doc_id AND a1.expense_type = 1
LEFT JOIN bz_doc_travelexpense_detail a2 ON b.employeeid = a2.emp_id AND h.dh_code = a2.doc_id AND a2.expense_type = 2
LEFT JOIN bz_doc_travelexpense_detail a3 ON b.employeeid = a3.emp_id AND h.dh_code = a3.doc_id AND a3.expense_type = 3
LEFT JOIN bz_doc_travelexpense_detail a4 ON b.employeeid = a4.emp_id AND h.dh_code = a4.doc_id AND a4.expense_type = 4
LEFT JOIN bz_doc_travelexpense_detail a5 ON b.employeeid = a5.emp_id AND h.dh_code = a5.doc_id AND a5.expense_type = 5
LEFT JOIN bz_doc_travelexpense_detail a6 ON b.employeeid = a6.emp_id AND h.dh_code = a6.doc_id AND a6.expense_type = 6
LEFT JOIN bz_doc_travelexpense_detail a7 ON b.employeeid = a7.emp_id AND h.dh_code = a7.doc_id AND a7.expense_type = 7
LEFT JOIN bz_doc_travelexpense_detail a8 ON b.employeeid = a8.emp_id AND h.dh_code = a8.doc_id AND a8.expense_type = 8
LEFT JOIN bz_doc_travelexpense_detail a9 ON b.employeeid = a9.emp_id AND h.dh_code = a9.doc_id AND a9.expense_type = 9
LEFT JOIN bz_doc_travelexpense_detail a10 ON b.employeeid = a10.emp_id AND h.dh_code = a10.doc_id AND a10.expense_type = 10
LEFT JOIN bz_doc_travelexpense_detail a11 ON b.employeeid = a11.emp_id AND h.dh_code = a11.doc_id AND a11.expense_type = 11
LEFT JOIN bz_doc_travelexpense_detail a12 ON b.employeeid = a12.emp_id AND h.dh_code = a12.doc_id AND a12.expense_type = 12
WHERE 1=1
";

            bool bCheckFilter = false;

            if (!string.IsNullOrEmpty(doc_id))
            {
                bCheckFilter = true;
                sqlstr += @" AND LOWER(h.dh_code) = :doc_id";
                paramList.Add(new OracleParameter(":doc_id", OracleDbType.Varchar2) { Value = doc_id.ToLower() });
            }

            if (!string.IsNullOrEmpty(country))
            {
                bCheckFilter = true;
                sqlstr += @" AND ((h.dh_type LIKE 'oversea%' AND LOWER(te.ct_id) = :country)
                        OR (h.dh_type LIKE 'local%' AND te.ct_id = 19 AND LOWER(te.pv_id) = :country))";
                paramList.Add(new OracleParameter(":country", OracleDbType.Varchar2) { Value = country.ToLower() });
            }

            if (!string.IsNullOrEmpty(date_from) && !string.IsNullOrEmpty(date_to))
            {
                bCheckFilter = true;
                sqlstr += @" AND TO_NUMBER(TO_CHAR(te.dte_bus_fromdate, 'yyyyMMdd')) >= TO_NUMBER(TO_CHAR(TO_DATE(:date_from, 'dd Mon yyyy'), 'yyyyMMdd'))
                    AND TO_NUMBER(TO_CHAR(te.dte_bus_todate, 'yyyyMMdd')) <= TO_NUMBER(TO_CHAR(TO_DATE(:date_to, 'dd Mon yyyy'), 'yyyyMMdd'))";
                paramList.Add(new OracleParameter(":date_from", OracleDbType.Varchar2) { Value = date_from });
                paramList.Add(new OracleParameter(":date_to", OracleDbType.Varchar2) { Value = date_to });
            }
            else if (!string.IsNullOrEmpty(date_from))
            {
                bCheckFilter = true;
                sqlstr += @" AND TO_NUMBER(TO_CHAR(te.dte_bus_fromdate, 'yyyyMMdd')) = TO_NUMBER(TO_CHAR(TO_DATE(:date_from, 'dd Mon yyyy'), 'yyyyMMdd'))";
                paramList.Add(new OracleParameter(":date_from", OracleDbType.Varchar2) { Value = date_from });
            }
            else if (!string.IsNullOrEmpty(date_to))
            {
                bCheckFilter = true;
                sqlstr += @" AND TO_NUMBER(TO_CHAR(te.dte_bus_todate, 'yyyyMMdd')) = TO_NUMBER(TO_CHAR(TO_DATE(:date_to, 'dd Mon yyyy'), 'yyyyMMdd'))";
                paramList.Add(new OracleParameter(":date_to", OracleDbType.Varchar2) { Value = date_to });
            }

            string xtravel_list = "";
            foreach (var travel in travel_list)
            {
                if (!string.IsNullOrEmpty(travel.id.ToString()))
                {
                    if (xtravel_list != "") xtravel_list += ",";
                    xtravel_list += "'" + travel.id.ToString() + "'";
                }
            }

            if (string.IsNullOrEmpty(xtravel_list))
            {
                if (!string.IsNullOrEmpty(travel_type))
                {
                    bCheckFilter = true;
                    sqlstr += @" AND LOWER(SUBSTR(h.dh_code, 0, 2)) = :travel_type";
                    paramList.Add(new OracleParameter(":travel_type", OracleDbType.Varchar2) { Value = travel_type.ToLower() });
                }
            }
            else
            {
                bCheckFilter = true;
                sqlstr += @" AND LOWER(SUBSTR(h.dh_code, 0, 2)) IN (" + xtravel_list.ToLower() + ")";
            }

            if (!string.IsNullOrEmpty(emp_id))
            {
                bCheckFilter = true;
                sqlstr += @" AND (:emp_id IS NULL OR b.employeeid = :emp_id)";
                paramList.Add(new OracleParameter(":emp_id", OracleDbType.Varchar2) { Value = emp_id });
            }

            if (!string.IsNullOrEmpty(section))
            {
                bCheckFilter = true;
                sqlstr += @" AND (:section IS NULL OR LOWER(b.sections) = LOWER(:section))";
                paramList.Add(new OracleParameter(":section", OracleDbType.Varchar2) { Value = section.ToLower() });
            }

            if (!string.IsNullOrEmpty(department))
            {
                bCheckFilter = true;
                sqlstr += @" AND (:department IS NULL OR LOWER(TRIM(b.department)) = LOWER(TRIM(:department)))";
                paramList.Add(new OracleParameter(":department", OracleDbType.Varchar2) { Value = department.ToLower() });
            }

            if (!string.IsNullOrEmpty(function_))
            {
                bCheckFilter = true;
                sqlstr += @" AND (:function IS NULL OR LOWER(TRIM(b.function)) = LOWER(TRIM(:function)))";
                paramList.Add(new OracleParameter(":function", OracleDbType.Varchar2) { Value = function_.ToLower() });
            }

            // If no filter is applied, limit the result set
            if (!bCheckFilter)
            {
                sqlstr += @" AND ROWNUM <= 100";
            }

            sqlstr += @" ORDER BY TO_CHAR(b.employeeid), h.dh_code";

            // Execute the query and return results
            SetDocService.conn_ExecuteData_Bind_Variables(ref dt, sqlstr, paramList);

            // Set row numbers
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["no"] = i + 1;
            }

            return dt;
        }

        public DataTable refsearch_approval_from(string token_login, string doc_id)
        {
            var dt = new DataTable();

            try
            {
                // 1. สร้าง SQL Query หลัก
                string sqlstr = $@"SELECT
                                0 as no,
                                h.dh_code as doc_id,
                                (CASE WHEN TO_NUMBER(uc.employeeid) > 90000000 THEN '' ELSE uc.employeeid || ' ' END) ||
                                (NVL(uc.ENTITLE, '') || ' ' || uc.ENFIRSTNAME || ' ' || uc.ENLASTNAME) as requested_by,
                                uc.ORGNAME as org_unit_req,
                                uo.employeeid || ' ' || (NVL(uo.ENTITLE, '') || ' ' || uo.ENFIRSTNAME || ' ' || uo.ENLASTNAME) as on_behalf_of,
                                uo.ORGNAME as org_unit_on_behalf,
                                TO_CHAR(h.dh_create_date, 'dd/MM/rrrr') as date_to_requested,
                                h.dh_code as document_number,
                                ts.ts_name as document_status,
                                mc.com_name as company,
                                '' as travel_type,
                                CASE WHEN h.dh_travel = 1 THEN 'Single Travel' ELSE 'Multiple Travel' END travel_with,
                                h.dh_topic as travel_topic,
                                '' as continent,
                                '' as country,
                                '' as city,
                                '' as province,
                                CASE WHEN h.dh_type LIKE 'local%' THEN h.dh_city END location,
                                CASE
                                    WHEN TO_CHAR(h.dh_bus_fromdate, 'MMrrrr') = TO_CHAR(h.dh_bus_todate, 'MMrrrr') THEN
                                        TO_CHAR(h.dh_bus_fromdate, 'dd') || ' - ' || TO_CHAR(h.dh_bus_todate, 'dd Mon rrrr')
                                    ELSE
                                        CASE
                                            WHEN TO_CHAR(h.dh_bus_fromdate, 'rrrr') = TO_CHAR(h.dh_bus_todate, 'rrrr') THEN
                                                TO_CHAR(h.dh_bus_fromdate, 'dd Mon') || ' - ' || TO_CHAR(h.dh_bus_todate, 'dd Mon rrrr')
                                            ELSE
                                                TO_CHAR(h.dh_bus_fromdate, 'dd Mon rrrr') || ' - ' || TO_CHAR(h.dh_bus_todate, 'dd Mon rrrr')
                                        END
                                END as business_date,
                                CASE
                                    WHEN TO_CHAR(h.dh_travel_fromdate, 'MMrrrr') = TO_CHAR(h.dh_travel_todate, 'MMrrrr') THEN
                                        TO_CHAR(h.dh_travel_fromdate, 'dd') || ' - ' || TO_CHAR(h.dh_travel_todate, 'dd Mon rrrr')
                                    ELSE
                                        CASE
                                            WHEN TO_CHAR(h.dh_travel_fromdate, 'rrrr') = TO_CHAR(h.dh_travel_todate, 'rrrr') THEN
                                                TO_CHAR(h.dh_travel_fromdate, 'dd Mon') || ' - ' || TO_CHAR(h.dh_travel_todate, 'dd Mon rrrr')
                                            ELSE
                                                TO_CHAR(h.dh_travel_fromdate, 'dd Mon rrrr') || ' - ' || TO_CHAR(h.dh_travel_todate, 'dd Mon rrrr')
                                        END
                                END as travel_date,
                                (h.dh_travel_todate - h.dh_travel_fromdate) + 1 as travel_duration,
                                h.dh_travel_object as traveling_objective,
                                h.dh_after_trip_opt1 as to_submit,
                                h.dh_after_trip_opt2 as to_share,
                                h.dh_after_trip_opt2_remark as to_share_remark,
                                h.dh_after_trip_opt3 as other,
                                h.dh_after_trip_opt3_remark as other_remark,
                                0 as exchange_rates_as_of,
                                0 as grand_total_expenses,
                                h.dh_expense_opt1 as the_budget,
                                h.dh_expense_opt2 as shall_seek,
                                h.dh_remark as remark,
                                h.dh_bus_fromdate as raw_bus_fromdate,  -- เพิ่มฟิลด์นี้สำหรับกรณี fallback
                                h.dh_bus_todate as raw_bus_todate        -- เพิ่มฟิลด์นี้สำหรับกรณี fallback
                            FROM bz_doc_head h
                            INNER JOIN vw_bz_users uc ON h.dh_create_by = uc.employeeid
                            LEFT JOIN vw_bz_users uo ON h.dh_behalf_emp_id = uo.employeeid
                            INNER JOIN bz_master_company mc ON h.dh_com_code = mc.com_code
                            INNER JOIN bz_master_status ts ON h.dh_doc_status = ts.ts_id
                            WHERE h.dh_code =:doc_id";

                // 2. ดึงข้อมูลหลัก
                var parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("doc_id", doc_id));
                var cmd = new OracleCommand(sqlstr);
                cmd.Parameters.AddRange(parameters.ToArray());
                string ret = SetDocService.conn_ExecuteData(ref dt, cmd);

                if (ret == "" && dt.Rows.Count > 0)
                {
                    // 3. ดึงข้อมูลอัตราแลกเปลี่ยน
                    decimal exchangeRate = 0;
                    sqlstr = @"SELECT exchange_rate
                            FROM BZ_DOC_TRAVELER_EXPENSE a
                            INNER JOIN (
                                SELECT MAX(TO_DATE(exchange_date, 'dd Mon rrrr')) as exchange_date, dh_code
                                FROM BZ_DOC_TRAVELER_EXPENSE
                                GROUP BY dh_code
                            ) b ON a.dh_code = b.dh_code AND TO_DATE(a.exchange_date, 'dd Mon rrrr') = b.exchange_date
                            WHERE a.dh_code = :doc_id";


                    DataTable dtm_exchangerate = new DataTable();
                    cmd = new OracleCommand(sqlstr);
                    cmd.Parameters.AddRange(parameters.ToArray());
                    SetDocService.conn_ExecuteData(ref dtm_exchangerate, cmd);

                    if (dtm_exchangerate.Rows.Count > 0)
                    {
                        decimal.TryParse(dtm_exchangerate.Rows[0]["exchange_rate"].ToString(), out exchangeRate);
                    }

                    // 4. ดึงข้อมูลประเภทการเดินทาง
                    string travelType = "";
                    DataTable dttype = new DataTable();

                    sqlstr = @"SELECT a.dtt_id as id, a.dtt_note as remark
                            FROM bz_doc_travel_type a
                            WHERE a.dh_code= :doc_id";
                    cmd = new OracleCommand(sqlstr);
                    cmd.Parameters.AddRange(parameters.ToArray());

                    SetDocService.conn_ExecuteData(ref dttype, cmd);

                    foreach (DataRow row in dttype.Rows)
                    {
                        if (!string.IsNullOrEmpty(travelType))
                        {
                            travelType += "/";
                        }

                        switch (row["id"].ToString())
                        {
                            case "1": travelType += "Meeting"; break;
                            case "2": travelType += "Site visit"; break;
                            case "3": travelType += "Workshop"; break;
                            case "4": travelType += "Roadshow"; break;
                            case "5": travelType += "Conference"; break;
                            case "6": travelType += row["remark"].ToString(); break;
                            case "7": travelType += "Training"; break;
                        }
                    }

                    // 5. ปรับปรุงข้อมูลใน DataTable
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        // ตรวจสอบและแก้ไข business_date ถ้าเป็น null
                        if (dt.Rows[i]["business_date"] == DBNull.Value || string.IsNullOrEmpty(dt.Rows[i]["business_date"].ToString()))
                        {
                            DateTime fromDate = Convert.ToDateTime(dt.Rows[i]["raw_bus_fromdate"]);
                            DateTime toDate = Convert.ToDateTime(dt.Rows[i]["raw_bus_todate"]);
                            if (fromDate.Month == toDate.Month && fromDate.Year == toDate.Year)
                            {
                                dt.Rows[i]["business_date"] = $"{fromDate:dd} - {toDate:dd MMM yyyy}";
                            }
                            else if (fromDate.Year == toDate.Year)
                            {
                                dt.Rows[i]["business_date"] = $"{fromDate:dd MMM} - {toDate:dd MMM yyyy}";
                            }
                            else
                            {
                                dt.Rows[i]["business_date"] = $"{fromDate:dd MMM yyyy} - {toDate:dd MMM yyyy}";
                            }
                        }

                        dt.Rows[i]["travel_type"] = travelType;
                        dt.Rows[i]["exchange_rates_as_of"] = exchangeRate;
                        dt.Rows[i]["no"] = (i + 1).ToString();
                    }

                    dt.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                // Log ข้อผิดพลาด
                Console.WriteLine($"Error in refsearch_approval_from: {ex.Message}");
                throw; // หรือ return new DataTable(); ตามความเหมาะสม
            }

            return dt;
        }

        //public DataTable refsearch_approval_from(string token_login, string doc_id)
        //{

        //    var dt = new DataTable();
        //    sqlstr = $@"select 0 as no
        //             , h.dh_code as doc_id
        //             , (case when to_number(uc.employeeid) > 90000000 then '' else uc.employeeid || ' '  end) || (nvl(uc.ENTITLE, '')|| ' ' || uc.ENFIRSTNAME || ' ' || uc.ENLASTNAME) as requested_by
        //             , uc.ORGNAME as org_unit_req
        //             , uo.employeeid || ' ' ||  (nvl(uo.ENTITLE, '')|| ' ' || uo.ENFIRSTNAME || ' ' || uo.ENLASTNAME) as on_behalf_of  
        //             , uo.ORGNAME as org_unit_on_behalf
        //             , to_char(h.dh_create_date,'dd/MM/rrrr') as date_to_requested
        //             , h.dh_code as document_number
        //             , ts.ts_name as document_status
        //             , mc.com_name as company
        //             , '' as travel_type
        //             , case when h.dh_travel = 1 then 'Single Travel' else 'Multiple Travel' end travel_with 

        //             , h.dh_topic as travel_topic
        //             , '' as continent
        //             , '' as country
        //             , '' as city
        //             , '' as province
        //             , case when h.dh_type like 'local%' then h.dh_city end location
        //             , case when to_char(h.dh_bus_fromdate,'MMrrrr') = to_char(h.dh_bus_todate,'MMrrrr') then
        //                  to_char(h.dh_bus_fromdate,'dd') ||' - '|| to_char(h.dh_bus_todate,'dd Mon rrrr')
        //                else
        //                  case when to_char(h.dh_bus_fromdate,'rrrr') = to_char(h.dh_bus_todate,'rrrr') then
        //                        to_char(h.dh_bus_fromdate,'dd Mon') ||' - '|| to_char(h.dh_bus_todate,'dd Mon rrrr')
        //                    else
        //                        to_char(h.dh_bus_fromdate,'dd Mon rrrr') ||' - '|| to_char(h.dh_bus_todate,'dd Mon rrrr')
        //                    end
        //               end business_date
        //             , case when to_char(h.dh_travel_fromdate,'MMrrrr') = to_char(h.dh_travel_todate,'MMrrrr') then
        //                  to_char(h.dh_travel_fromdate,'dd') ||' - '|| to_char(h.dh_travel_todate,'dd Mon rrrr')
        //                else
        //                  case when to_char(h.dh_travel_fromdate,'rrrr') = to_char(h.dh_travel_todate,'rrrr') then
        //                        to_char(h.dh_travel_fromdate,'dd Mon') ||' - '|| to_char(h.dh_travel_todate,'dd Mon rrrr')
        //                    else
        //                        to_char(h.dh_travel_fromdate,'dd Mon rrrr') ||' - '|| to_char(h.dh_travel_todate,'dd Mon rrrr')
        //                  end
        //               end  travel_date
        //             , (h.dh_travel_todate - h.dh_travel_fromdate)+1 as travel_duration
        //             , h.dh_travel_object as traveling_objective
        //             , h.dh_after_trip_opt1 as to_submit 
        //             , h.dh_after_trip_opt2 as to_share 
        //             , h.dh_after_trip_opt2_remark as to_share_remark 
        //             , h.dh_after_trip_opt3 as other 
        //             , h.dh_after_trip_opt3_remark as other_remark 

        //             , 0 as exchange_rates_as_of
        //             , 0 as grand_total_expenses

        //             , h.dh_expense_opt1 as the_budget
        //             , h.dh_expense_opt2 as shall_seek 
        //             , h.dh_remark as remark 

        //            from bz_doc_head h
        //            inner join vw_bz_users uc on h.dh_create_by = uc.employeeid 
        //            left join vw_bz_users uo on h.dh_behalf_emp_id = uo.employeeid
        //            inner join bz_master_company mc on h.dh_com_code = mc.com_code
        //            inner join bz_master_status ts on h.dh_doc_status = ts.ts_id
        //            where h.dh_code = '{doc_id}' ";

        //    dt = new DataTable();
        //    string ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);
        //    if (ret == "")
        //    {
        //        string travel_type = "";
        //        string exchange_rates_as_of = "";

        //        //DevFix 20250129 ปรับให้ใช้ข้อมูลที่อยู่ในตาราง 
        //        //DataTable dtm_exchangerate = ref_exchangerate(); 
        //        sqlstr = $@"select exchange_rate
        //                    from  BZ_DOC_TRAVELER_EXPENSE a 
        //                    inner join (
        //                    select max(to_date(exchange_date,'dd Mon rrrr')) as exchange_date, dh_code --, exchange_rate, exchange_currency
        //                    from BZ_DOC_TRAVELER_EXPENSE a group by dh_code 
        //                    )b on a.dh_code = b.dh_code and to_date(a.exchange_date,'dd Mon rrrr') = b.exchange_date
        //                    where a.dh_code = '{doc_id}'";

        //        DataTable dtm_exchangerate = new DataTable();
        //        SetDocService.conn_ExecuteData(ref dtm_exchangerate, sqlstr);
        //        if (dtm_exchangerate.Rows.Count > 0)
        //        {
        //            exchange_rates_as_of = dtm_exchangerate.Rows[0]["exchange_rate"].ToString();
        //        }

        //        #region Travel Type  
        //        DataTable dttype = new DataTable();
        //        sqlstr = @"  select a.dtt_id as id, a.dtt_note as remark from bz_doc_travel_type a  where a.dh_code = '" + doc_id + "'";
        //        ret = SetDocService.conn_ExecuteData(ref dttype, sqlstr);
        //        for (int i = 0; i < dttype.Rows.Count; i++)
        //        {
        //            if (travel_type != "") { travel_type += "/"; }
        //            if (dttype.Rows[i]["id"].ToString() == "1") { travel_type += "Meeting"; }
        //            else if (dttype.Rows[i]["id"].ToString() == "2") { travel_type += "Site visit"; }
        //            else if (dttype.Rows[i]["id"].ToString() == "3") { travel_type += "Workshop"; }
        //            else if (dttype.Rows[i]["id"].ToString() == "4") { travel_type += "Roadshow"; }
        //            else if (dttype.Rows[i]["id"].ToString() == "5") { travel_type += "Conference"; }
        //            else if (dttype.Rows[i]["id"].ToString() == "6")
        //            {
        //                //เป็น other ให้แสดง text ของ other 
        //                travel_type += dttype.Rows[i]["remark"].ToString() + "";
        //            }
        //            //DevFix 20220805 --> after go-live เพิ่ม Tick box = Training 
        //            else if (dttype.Rows[i]["id"].ToString() == "7") { travel_type += "Training"; }
        //        }
        //        #endregion Travel Type  


        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            dt.Rows[i]["travel_type"] = travel_type;
        //            try { dt.Rows[i]["exchange_rates_as_of"] = exchange_rates_as_of; } catch { }
        //            dt.Rows[i]["no"] = (i + 1).ToString();



        //        }
        //        dt.AcceptChanges();

        //    }


        //    return dt;
        //}
        private string sql_approval_from_traveler_summary(string doc_id, Boolean bdetail
            , ref string sqlstr_from)
        {
            sqlstr = @" select distinct 0 as no 
                     , uc.employeeid as emp_id
                     , (nvl(uc.ENTITLE, '')|| ' ' || uc.ENFIRSTNAME || ' ' || uc.ENLASTNAME)  as emp_name
                     , uc.ORGNAME as org_unit
                     , case when h.dh_type like 'oversea%' then mct.ct_name || '/' || te.city_text else te.city_text end country_city
                     , mpv.pv_name as province
                     , case when h.dh_type like 'local%' then te.city_text end location
                     , case when to_char(te.dte_bus_fromdate,'MMrrrr') = to_char(te.dte_bus_todate,'MMrrrr') then
                          to_char(te.dte_bus_fromdate,'dd') ||' - '|| to_char(te.dte_bus_todate,'dd Mon rrrr')
                        else
                          case when to_char(te.dte_bus_fromdate,'rrrr') = to_char(te.dte_bus_todate,'rrrr') then
                                to_char(te.dte_bus_fromdate,'dd Mon') ||' - '|| to_char(te.dte_bus_todate,'dd Mon rrrr')
                            else
                                to_char(te.dte_bus_fromdate,'dd Mon rrrr') ||' - '|| to_char(te.dte_bus_todate,'dd Mon rrrr')
                          end
                       end business_date 
                     , case when to_char(te.dte_travel_fromdate,'MMrrrr') = to_char(te.dte_travel_todate,'MMrrrr') then
                          to_char(te.dte_travel_fromdate,'dd') ||' - '|| to_char(te.dte_travel_todate,'dd Mon rrrr')
                        else
                          case when to_char(te.dte_travel_fromdate,'rrrr') = to_char(te.dte_travel_todate,'rrrr') then
                                to_char(te.dte_travel_fromdate,'dd Mon') ||' - '|| to_char(te.dte_travel_todate,'dd Mon rrrr')
                            else
                                to_char(te.dte_travel_fromdate,'dd Mon rrrr') ||' - '|| to_char(te.dte_travel_todate,'dd Mon rrrr')
                          end
                       end travel_date
                      , '' as budget_account 
                      , nvl(te.dte_gl_account,'') as dte_gl_account ,  nvl(te.dte_cost_center,'') as dte_cost_center, nvl(te.dte_order_wbs,'') as dte_order_wbs";
            if (bdetail == true)
            {
                sqlstr += @" 
                     , case when te.dte_air_tecket is not null then replace(to_char(to_number(te.dte_air_tecket),'9,999,999.99'),' ') else '' end air_ticket
                     , case when te.dte_accommodatic is not null then replace(to_char(to_number(te.dte_accommodatic),'9,999,999.99'),' ') else '' end accommodation
                     , case when h.dh_type like 'oversea%' then (case when te.dte_allowance is not null then replace(to_char(to_number(te.dte_allowance),'9,999,999.99'),' ') else '' end) else (case when te.dte_allowance_day is not null then replace(to_char(to_number(te.dte_allowance_day),'9,999,999.99'),' ') else '' end) end allowance 
                     , case when te.dte_transport is not null then replace(to_char(to_number(te.dte_transport),'9,999,999.99'),' ') else '' end transportation 
                     , case when te.dte_passport_expense is not null then replace(to_char(to_number(te.dte_passport_expense),'9,999,999.99'),' ') else '' end passport 
                     , to_char(te.dte_passport_valid,'dd Month rrrr') as passport_valid
                     , case when te.dte_visa_free is not null then replace(to_char(to_number(te.dte_visa_free),'9,999,999.99'),' ') else '' end visa_fee 
                     , case when (nvl(te.dte_regis_free,0) + nvl(te.dte_miscellaneous,0) + nvl(te.dte_air_tecket,0)) is not null then replace(to_char(to_number(nvl(te.dte_regis_free,0) + nvl(te.dte_miscellaneous,0) + nvl(te.dte_air_tecket,0)),'9,999,999.99'),' ') else '' end others
                     , case when te.dte_cl_expense is not null then replace(to_char(to_number(te.dte_cl_expense),'9,999,999.99'),' ') else '' end luggage_clothing  
                     , to_char(te.dte_cl_valid,'dd Month rrrr') as luggage_clothing_valid  
                     , case when te.dte_travel_ins is not null then replace(to_char(to_number(te.dte_travel_ins),'9,999,999.99'),' ') else '' end insurance 
                     , case when te.dte_total_expense is not null then replace(to_char(to_number(te.dte_total_expense),'9,999,999.99'),' ') else '' end total_expenses  
                     , te.dte_traveler_remark as remark   ";
            }

            sqlstr_from = "";
            sqlstr_from += @" from bz_doc_head h
                                inner join bz_doc_traveler_expense te on h.dh_code = te.dh_code
                                inner join vw_bz_users uc on te.dte_emp_id = uc.employeeid 
                      
                                left join bz_master_continent mctn on to_char(te.ctn_id) = to_char(mctn.ctn_id)  
                                left join bz_master_country mct on to_char(te.ct_id) = to_char(mct.ct_id) and to_char(te.ctn_id) = to_char(mct.ctn_id) 
                                left join bz_master_province mpv on to_char(te.pv_id) = to_char(mpv.pv_id) and to_char(te.ct_id) = '19'  ";
            sqlstr_from += @"   where h.dh_code = '" + doc_id + "' ";

            ////แสดงเฉพาะคนที่ไม่ถูก reject 
            //sqlstr_from += @" and (te.DTE_EMP_ID, te.DH_CODE) in ( 
            //                 select dte_emp_id, dh_code from bz_doc_traveler_expense
            //                 where (nvl(dte_appr_status,1) not in (30) or nvl(dte_cap_appr_status,1) not in (40))
            //                 and nvl(dte_cap_appr_status,1) not in (40)
            //                 and (nvl(dte_appr_opt,'true') = 'true' and nvl(dte_cap_appr_opt,'true') = 'true'))";

            return sqlstr + sqlstr_from;

            //replace(replace(replace(
            //           replace(replace(replace(replace('A0FC' || nvl(te.dte_gl_account, '') || ' | ' || nvl(te.dte_cost_center, '') || ' | ' || nvl(te.dte_order_wbs, ''), ' ') || 'C0HK', '||', ''), '|C0HK', ''), 'C0HK', '')
            //           , 'A0FC|', ''), 'A0FC', ''), '|', ' | ') as budget_account
        }
        public void refsearch_approval_from_traveler_location(string token_login, string doc_id, ref DataTable dtref)
        {
            if (dtref.Rows.Count == 0) { return; }
            string sqlstr_from = "";
            var dt = new DataTable();
            sqlstr = sql_approval_from_traveler_summary(doc_id, false, ref sqlstr_from);

            sqlstr = @"select distinct 
                         mctn.ctn_name as continent
                         , mct.ct_name as country
                         , te.city_text as city 
                         , mpv.pv_name as province ";
            sqlstr += sqlstr_from;
            DataTable dtdisplay = new DataTable();
            SetDocService.conn_ExecuteData(ref dtdisplay, sqlstr);

            sqlstr = @"select min(te.dte_travel_fromdate) as fromdate , max(te.dte_travel_todate) as todate ";
            sqlstr += sqlstr_from;
            DataTable dtdate = new DataTable();
            SetDocService.conn_ExecuteData(ref dtdate, sqlstr);

            var continent = "";
            var country = "";
            var city = "";
            var province = "";
            for (int i = 0; i < dtdisplay.Rows.Count; i++)
            {
                if (continent != "") { continent += "/"; }
                if (country != "") { country += "/"; }
                if (city != "") { city += "/"; }
                if (province != "") { province += "/"; }

                continent += dtdisplay.Rows[i]["continent"].ToString() + "";
                country += dtdisplay.Rows[i]["country"].ToString() + "";
                city += dtdisplay.Rows[i]["city"].ToString() + "";
                province += dtdisplay.Rows[i]["province"].ToString() + "";

                try
                {
                    string budget_account = "";
                    budget_account = dtdisplay.Rows[i]["dte_gl_account"].ToString();

                    if (dtdisplay.Rows[i]["dte_cost_center"].ToString() != "")
                    {
                        if (budget_account != "") { budget_account += " | "; }
                        budget_account += dtdisplay.Rows[i]["dte_cost_center"].ToString();
                    }
                    if (dtdisplay.Rows[i]["dte_order_wbs"].ToString() != "")
                    {
                        if (budget_account != "") { budget_account += " | "; }
                        budget_account += dtdisplay.Rows[i]["dte_order_wbs"].ToString();
                    }
                    dtdisplay.Rows[i]["budget_account"] = budget_account;
                }
                catch { }
            }

            dtref.Rows[0]["continent"] = continent + "";
            dtref.Rows[0]["country"] = country + "";
            dtref.Rows[0]["city"] = city + "";
            dtref.Rows[0]["province"] = province + "";
            dtref.AcceptChanges();


        }
        public DataTable refsearch_approval_from_traveler_summary(string token_login, string doc_id)
        {
            string sqlstr_from = "";
            var dt = new DataTable();
            sqlstr = sql_approval_from_traveler_summary(doc_id, false, ref sqlstr_from);

            dt = new DataTable();
            string ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);
            if (ret == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["no"] = (i + 1).ToString();

                    try
                    {
                        string budget_account = "";
                        budget_account = dt.Rows[i]["dte_gl_account"].ToString();

                        if (dt.Rows[i]["dte_cost_center"].ToString() != "")
                        {
                            if (budget_account != "") { budget_account += " | "; }
                            budget_account += dt.Rows[i]["dte_cost_center"].ToString();
                        }
                        if (dt.Rows[i]["dte_order_wbs"].ToString() != "")
                        {
                            if (budget_account != "") { budget_account += " | "; }
                            budget_account += dt.Rows[i]["dte_order_wbs"].ToString();
                        }
                        dt.Rows[i]["budget_account"] = budget_account;
                    }
                    catch { }

                }
                dt.AcceptChanges();

            }

            return dt;
        }
        public DataTable refsearch_approval_from_estimate_expense(string token_login, string doc_id)
        {
            string sqlstr_from = "";
            var dt = new DataTable();
            sqlstr = sql_approval_from_traveler_summary(doc_id, true, ref sqlstr_from);

            dt = new DataTable();
            string ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);
            if (ret == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["no"] = (i + 1).ToString();

                    try
                    {
                        string budget_account = "";
                        budget_account = dt.Rows[i]["dte_gl_account"].ToString();

                        if (dt.Rows[i]["dte_cost_center"].ToString() != "")
                        {
                            if (budget_account != "") { budget_account += " | "; }
                            budget_account += dt.Rows[i]["dte_cost_center"].ToString();
                        }
                        if (dt.Rows[i]["dte_order_wbs"].ToString() != "")
                        {
                            if (budget_account != "") { budget_account += " | "; }
                            budget_account += dt.Rows[i]["dte_order_wbs"].ToString();
                        }
                        dt.Rows[i]["budget_account"] = budget_account;
                    }
                    catch { }
                }
                dt.AcceptChanges();

            }

            return dt;
        }
        //public DataTable refsearch_approval_from_approvaldetails(string token_login, string doc_id)
        //{
        //    var dt = new DataTable();
        //    sqlstr = @"SELECT DISTINCT
        //            0 AS no,
        //            uc.employeeid AS emp_id,
        //            (NVL(uc.ENTITLE, '') || ' ' || uc.ENFIRSTNAME || ' ' || uc.ENLASTNAME) AS emp_name,
        //            uc.ORGNAME AS org_unit,

        //            -- Line Approver (dta_type = 1)
        //            (SELECT (NVL(u.ENTITLE, '') || ' ' || u.ENFIRSTNAME || ' ' || u.ENLASTNAME)
        //             FROM bz_doc_traveler_approver t
        //             JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
        //             WHERE t.dh_code = h.dh_code
        //             AND t.dta_travel_empid = uc.employeeid
        //             AND t.dta_type = 1
        //             AND t.dta_status = 1
        //             AND t.dta_appr_level = te.dta_appr_level) AS line_approval,

        //            -- Line Approver Org Unit
        //            (SELECT u.ORGNAME
        //             FROM bz_doc_traveler_approver t
        //             JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
        //             WHERE t.dh_code = h.dh_code
        //             AND t.dta_travel_empid = uc.employeeid
        //             AND t.dta_type = 1
        //             AND t.dta_status = 1
        //             AND t.dta_appr_level = te.dta_appr_level) AS org_unit_line,

        //            -- Cap Approver (dta_type = 2)
        //            (SELECT (NVL(u.ENTITLE, '') || ' ' || u.ENFIRSTNAME || ' ' || u.ENLASTNAME)
        //             FROM bz_doc_traveler_approver t
        //             JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
        //             WHERE t.dh_code = h.dh_code
        //             AND t.dta_travel_empid = uc.employeeid
        //             AND t.dta_type = 2
        //             AND t.dta_status = 1
        //             AND t.dta_appr_level = te.dta_appr_level) AS cap_approval,

        //            -- Cap Approver Org Unit
        //            (SELECT u.ORGNAME
        //             FROM bz_doc_traveler_approver t
        //             JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
        //             WHERE t.dh_code = h.dh_code
        //             AND t.dta_travel_empid = uc.employeeid
        //             AND t.dta_type = 2
        //             AND t.dta_status = 1
        //             AND t.dta_appr_level = te.dta_appr_level) AS org_unit_cap,

        //            -- Line Approval Date
        //            (SELECT 
        //                CASE
        //                    WHEN t.dta_update_date IS NULL THEN 'Pending'
        //                    WHEN t.dta_appr_status != 'true' THEN 'Rejected : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
        //                    ELSE 'Approved : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
        //                END
        //             FROM bz_doc_traveler_approver t
        //             WHERE t.dh_code = h.dh_code
        //             AND t.dta_travel_empid = uc.employeeid
        //             AND t.dta_type = 1
        //             AND t.dta_status = 1
        //             AND t.dta_appr_level = te.dta_appr_level) AS approved_date_line,

        //            -- Cap Approval Date
        //            (SELECT 
        //                CASE
        //                    WHEN t.dta_update_date IS NULL THEN 'Pending'
        //                    WHEN t.dta_appr_status != 'true' THEN 'Rejected : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
        //                    ELSE 'Approved : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
        //                END
        //             FROM bz_doc_traveler_approver t
        //             WHERE t.dh_code = h.dh_code
        //             AND t.dta_travel_empid = uc.employeeid
        //             AND t.dta_type = 2
        //             AND t.dta_status = 1
        //             AND t.dta_appr_level = te.dta_appr_level) AS approved_date_cap,

        //            uc.employeeid AS dta_travel_empid,
        //            te.dta_appr_level
        //        FROM 
        //            bz_doc_head h
        //            INNER JOIN bz_doc_traveler_approver te ON h.dh_code = te.dh_code
        //            INNER JOIN vw_bz_users uc ON te.dta_travel_empid = uc.employeeid
        //        WHERE 
        //            te.dta_status = 1 
        //            AND h.dh_code = :doc_id    ORDER BY     uc.employeeid,  te.dta_appr_level";

        //    var parameters = new List<OracleParameter>();
        //    parameters.Add(new OracleParameter("doc_id", doc_id));

        //    dt = new DataTable();
        //    string ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);
        //    if (ret == "")
        //    {
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            dt.Rows[i]["no"] = (i + 1).ToString();

        //        }
        //        dt.AcceptChanges();

        //    }

        //    return dt;
        //}

        public DataTable refsearch_approval_from_approvaldetails(string token_login, string doc_id)
        {
            var dt = new DataTable();
            sqlstr = @"SELECT DISTINCT
    0 AS no,
    uc.employeeid AS emp_id,
    (NVL(uc.ENTITLE, '') || ' ' || uc.ENFIRSTNAME || ' ' || uc.ENLASTNAME) AS emp_name,
    uc.ORGNAME AS org_unit,

    -- Line Approver (dta_type = 1)
    (SELECT (NVL(u.ENTITLE, '') || ' ' || u.ENFIRSTNAME || ' ' || u.ENLASTNAME)
     FROM bz_doc_traveler_approver t
     JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
     WHERE t.dh_code = h.dh_code
     AND t.dta_travel_empid = uc.employeeid
     AND t.dta_type = 1
     AND t.dta_status = 1
     AND t.dta_appr_level = te.dta_appr_level) AS line_approval,
 
    -- Line Approver Org Unit
    (SELECT u.ORGNAME
     FROM bz_doc_traveler_approver t
     JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
     WHERE t.dh_code = h.dh_code
     AND t.dta_travel_empid = uc.employeeid
     AND t.dta_type = 1
     AND t.dta_status = 1
     AND t.dta_appr_level = te.dta_appr_level) AS org_unit_line,
 
    -- Cap Approver (dta_type = 2)
    (SELECT (NVL(u.ENTITLE, '') || ' ' || u.ENFIRSTNAME || ' ' || u.ENLASTNAME)
     FROM bz_doc_traveler_approver t
     JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
     WHERE t.dh_code = h.dh_code
     AND t.dta_travel_empid = uc.employeeid
     AND t.dta_type = 2
     AND t.dta_status = 1
     AND t.dta_appr_level = te.dta_appr_level) AS cap_approval,
 
    -- Cap Approver Org Unit
    (SELECT u.ORGNAME
     FROM bz_doc_traveler_approver t
     JOIN vw_bz_users u ON t.dta_appr_empid = u.employeeid
     WHERE t.dh_code = h.dh_code
     AND t.dta_travel_empid = uc.employeeid
     AND t.dta_type = 2
     AND t.dta_status = 1
     AND t.dta_appr_level = te.dta_appr_level) AS org_unit_cap,
 
    -- Line Approval Date (Updated with Revised by)
    (SELECT 
        CASE
    WHEN t.dta_action_status = 2 THEN 'Pending'
    WHEN t.dta_action_status = 5 AND t.dta_appr_status != 'true' THEN 'Rejected : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
    WHEN t.dta_action_status = 3 THEN 'Approved : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
    ELSE 
        'Revised by ' || (SELECT (NVL(u.ENTITLE, '') || ' ' || u.ENFIRSTNAME || ' ' || u.ENLASTNAME)
                         FROM vw_bz_users u 
                         WHERE u.employeeid = t.dta_appr_empid)
END
     FROM bz_doc_traveler_approver t
     WHERE t.dh_code = h.dh_code
     AND t.dta_travel_empid = uc.employeeid
     AND t.dta_type = 1
     AND t.dta_status = 1
     AND t.dta_appr_level = te.dta_appr_level) AS approved_date_line,
 
    -- Cap Approval Date (Updated with Revised by)
    (SELECT 
        CASE
    WHEN t.dta_action_status = 2 THEN 'Pending'
    WHEN t.dta_action_status = 5 AND t.dta_appr_status != 'true' THEN 'Rejected : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
    WHEN t.dta_action_status = 3 THEN 'Approved : ' || TO_CHAR(t.dta_update_date, 'dd Mon rrrr')
    ELSE 
        'Revised by ' || (SELECT (NVL(u.ENTITLE, '') || ' ' || u.ENFIRSTNAME || ' ' || u.ENLASTNAME)
                         FROM vw_bz_users u 
                         WHERE u.employeeid = t.dta_appr_empid)
END
     FROM bz_doc_traveler_approver t
     WHERE t.dh_code = h.dh_code
     AND t.dta_travel_empid = uc.employeeid
     AND t.dta_type = 2
     AND t.dta_status = 1
     AND t.dta_appr_level = te.dta_appr_level) AS approved_date_cap,
 
    uc.employeeid AS dta_travel_empid,
    te.dta_appr_level
FROM 
    bz_doc_head h
    INNER JOIN bz_doc_traveler_approver te ON h.dh_code = te.dh_code
    INNER JOIN vw_bz_users uc ON te.dta_travel_empid = uc.employeeid
WHERE 
    te.dta_status = 1 
    AND h.dh_code = '" + doc_id + "'  ORDER BY  uc.employeeid,  te.dta_appr_level";

            dt = new DataTable();
            string ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);
            if (ret == "")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["no"] = (i + 1).ToString();

                }
                dt.AcceptChanges();

            }

            return dt;
        }


        #endregion  search data 

        #region page
        public void refDocByToken(ref string doc_id)
        {
            dt = new DataTable();
            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    doc_id = dt.Rows[0]["doc_id"].ToString();
                }
            }
        }
        public TravelerHistoryOutModel SearchTravelerHistory(TravelerHistoryModel value)
        {
            //กรณีที่ข้อมูล personal แสดงว่ามาจากการกดเข้าหน้า Traveler History ตรงๆ
            value.doc_id = value.doc_id + "";

            var msg = "";
            var page_name = "travelerhistory";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new TravelerHistoryOutModel();

            #region emp in travel
            data.token_login = token_login;
            data.doc_id = doc_id;
            data.pdpa_wording = refdata_pdpa_wording();


            try
            {
                Boolean user_admin = false;
                DataTable dtdata = refdata_emp_detail(value.token_login, value.doc_id, "", ref user_admin);
                DataTable dtvisa_traveler = refdata_emp_visa_traveler(value.token_login, value.doc_id);
                if (dtdata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtdata.Rows.Count; i++)
                    {
                        DataRow dremp = dtdata.Rows[i];
                        var emp_id_select = dremp["emp_id"].ToString();
                        var id = (i + 1).ToString(); ;
                        if (i == 0)
                        {
                            data.emp_id = dremp["emp_id"].ToString();
                            data.id = id;

                            data.user_admin = true;
                            data.user_request = refdata_role_review(token_login, doc_id);

                            data.travel_topic = dremp["travel_topic"].ToString(); ;
                            data.travel_topic_sub = dremp["travel_topic_sub"].ToString(); ;
                            data.business_date = dremp["business_date"].ToString(); ;
                            data.travel_date = dremp["travel_date"].ToString(); ;
                            data.country_city = dremp["country_city"].ToString();

                            data.show_button = true;
                        }
                        try
                        {
                            List<travelerHistoryList> drselect = data.arrTraveler.Where(a => (a.emp_id == emp_id_select)).ToList();
                            if (drselect.Count == 0)
                            {
                                data.traveler_emp.Add(data_traveler_emp_list(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], data.user_admin));
                            }
                        }
                        catch (Exception ex) { msg += i.ToString(); msg += ex.Message.ToString(); }

                        #region กรณีที่เป็น history การเดินทางใน trip  
                        var nationality = "";
                        var visa_type = "";
                        var valid_unit = "";
                        var visa_entry = "";
                        var full_path = "";
                        var file_name = "";
                        try
                        {
                            // DataRow[] drselectvisa = dtvisa_traveler.Select("employeeid='" + emp_id_select + "'");$
                            DataRow[] drselectvisa = dtvisa_traveler.AsEnumerable().Where(s => s.Field<string>("employeeid") == emp_id_select).ToArray();
                            if (drselectvisa.Length > 0)
                            {
                                for (int k = 0; k < drselectvisa.Length; k++)
                                {

                                    nationality = drselectvisa[k]["nationality"].ToString() + "";
                                    visa_type = drselectvisa[k]["visa_type"].ToString() + "";
                                    valid_unit = drselectvisa[k]["valid_unit"].ToString() + "";
                                    visa_entry = drselectvisa[k]["visa_entry"].ToString() + "";
                                    full_path = drselectvisa[k]["full_path"].ToString() + "";
                                    file_name = drselectvisa[k]["file_name"].ToString() + "";

                                    data.arrTraveler.Add(new travelerHistoryList
                                    {
                                        doc_id = dremp["doc_id"].ToString(),
                                        emp_id = dremp["emp_id"].ToString(),
                                        id = k.ToString(),
                                        seq = k.ToString(),
                                        country = dremp["country"].ToString(),
                                        icon = dremp["icon"].ToString(),
                                        datefrom = dremp["datefrom"].ToString(),
                                        dateto = dremp["dateto"].ToString(),

                                        nationality = nationality,
                                        valid_unit = valid_unit,
                                        visa_type = visa_type,
                                        visa_entry = visa_entry,
                                        full_path = full_path,//url full path
                                        file_name = file_name,


                                    });
                                }
                            }
                            else
                            {
                                data.arrTraveler.Add(new travelerHistoryList
                                {
                                    doc_id = dremp["doc_id"].ToString(),
                                    emp_id = dremp["emp_id"].ToString(),
                                    id = (dremp["id"].ToString() ?? "") == "" ? id.ToString() : dremp["id"].ToString(),
                                    seq = "1",
                                    country = dremp["country"].ToString(),
                                    icon = dremp["icon"].ToString(),
                                    datefrom = dremp["datefrom"].ToString(),
                                    dateto = dremp["dateto"].ToString(),

                                    nationality = nationality,
                                    valid_unit = valid_unit,
                                    visa_type = visa_type,
                                    visa_entry = visa_entry,
                                    full_path = full_path,//url full path
                                    file_name = file_name,


                                });
                            }
                        }
                        catch (Exception ex) { msg += " --> กรณีที่เป็น history การเดินทางใน trip : " + i.ToString(); msg += ex.Message.ToString(); }

                        #endregion กรณีที่เป็น history การเดินทางใน trip

                        //กรณี personal ให้ดึงข้อมูลไปแสดงแค่ชุดเดียวก็พอ doc id ไหนก็ได้
                        if (doc_id == "personal") { break; }
                    }
                }
                else
                {
                    data.traveler_emp.Add(new travelerEmpList
                    {
                        token_login = token_login,
                        doc_id = doc_id,
                        emp_id = "",
                        id = "",
                        user_admin = user_admin,
                        show_button = false,

                        age = "",
                        org_unit = "",
                        titlename = "",
                        firstname = "",
                        lastname = "",
                        userDisplay = "",
                        userName = "",
                        division = "",
                        idNum = "",
                        userEmail = "",
                        userPhone = "",
                        userTel = "",
                        isEdit = false,
                        imgpath = "",
                        imgprofilename = "",

                        passportno = "",
                        dateofissue = "",
                        dateofbirth = "",
                        dateofexpire = "",
                        passport_img = "",
                        passport_img_name = "",

                        travel_topic = "",
                        travel_topic_sub = "",
                        business_date = "",
                        travel_date = "",
                        country_city = "",

                        remark = "",

                        action_type = "",
                        action_change = "",
                    });
                    data.arrTraveler.Add(new travelerHistoryList
                    {
                        doc_id = doc_id,
                        emp_id = "",
                        id = "",

                        seq = "",
                        country = "",
                        icon = "",
                        datefrom = "",
                        dateto = "",

                        nationality = "",
                        valid_unit = "",
                        visa_type = "",
                        visa_entry = "",
                        full_path = "",
                        file_name = "",
                    });
                }
            }
            catch (Exception ex) { LoggerFile.write(ex); data.token_login = ex.Message.ToString() + msg; }

            data.remark = msg;
            #endregion emp in travel


            return data;
        }

        public AirTicketOutModel SearchAirTicket(AirTicketModel value)
        {
            var msg = "";
            var page_name = "airticket";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new AirTicketOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            try
            {
                #region master data
                DataTable dtbookstatus = refdata_book_status(page_name);
                DataTable dtbooktype = refdata_book_type();

                if (dtbookstatus.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtbookstatus.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_book_status.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }
                if (dtbooktype.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtbooktype.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_book_type.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }

                #endregion master data

                #region emp in travel
                try
                {
                    Boolean user_admin = true;
                    DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtbook = refdata_air_book(token_login, doc_id, "", user_admin);
                    DataTable dtbookdetail = refdata_air_book_detail(token_login, doc_id, "", user_admin);

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            //DevFix 20210820 0000 แก้ไขกรณีที่เป็น case ที่ traverler มีการเดินทาง 2 รายการ
                            //ตอนนี้กรอกแค่รายการเดียวก่อน ???
                            var drcheck_emp = data.emp_list.Where(a => (a.emp_id == dtdata.Rows[i]["emp_id"].ToString())).ToList();
                            if (drcheck_emp.Count > 0)
                            {
                                ////กรณีที่ traverler 1 คนมีมากกว่า 1 รายการให้ ตัดเหลือ 1รายการ 
                                ////และแก้หาช่วงวัน business_date, travel_date 
                                ////เอาข้อมูล มาต่อกัน มี country_id, country_name, continent, city, country, country_city
                                //drcheck_emp[0].business_date = "";
                                //drcheck_emp[0].travel_date = "";
                                //drcheck_emp[0].business_date = ""; 
                                //continue;
                            }
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }

                            // DataRow[] drbook = dtbook.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drbook = dtbook.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                            for (int k = 0; k < drbook.Length; k++)
                            {
                                //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                                if (dremp_list != null && dremp_list.Count > 0)
                                {
                                    dremp_list[0].doc_status_id = drbook[k]["doc_status"].ToString();
                                }

                                //ตามจำนวน employee
                                data.airticket_booking.Add(new airticketbookList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    doc_status = data.emp_list[i].doc_status_id.ToString(),

                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    id = drbook[k]["id"].ToString(),
                                    data_type = drbook[k]["data_type"].ToString(),

                                    ask_booking = drbook[k]["ask_booking"].ToString(),
                                    search_air_ticket = drbook[k]["search_air_ticket"].ToString(),
                                    as_company_recommend = drbook[k]["as_company_recommend"].ToString(),
                                    already_booked = drbook[k]["already_booked"].ToString(),
                                    already_booked_other = drbook[k]["already_booked_other"].ToString(),
                                    already_booked_id = drbook[k]["already_booked_id"].ToString(),

                                    booking_ref = drbook[k]["booking_ref"].ToString(),
                                    booking_status = drbook[k]["booking_status"].ToString(),
                                    additional_request = drbook[k]["additional_request"].ToString(),

                                    data_type_allowance = drbook[k]["data_type_allowance"].ToString(),

                                    action_type = drbook[k]["action_type"].ToString(),
                                    action_change = "false",


                                });
                            }
                            // DataRow[] drdetail = dtbookdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drdetail = dtbookdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                            for (int k = 0; k < drdetail.Length; k++)
                            {
                                //จำวนวนรายการ detail ตามจำนวน employee
                                data.airticket_detail.Add(new airticketList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    id = drdetail[k]["id"].ToString(),

                                    airticket_date = drdetail[k]["airticket_date"].ToString(),
                                    airticket_route_from = drdetail[k]["airticket_route_from"].ToString(),
                                    airticket_route_to = drdetail[k]["airticket_route_to"].ToString(),
                                    airticket_flight = drdetail[k]["airticket_flight"].ToString(),
                                    airticket_departure_time = drdetail[k]["airticket_departure_time"].ToString(),
                                    airticket_arrival_time = drdetail[k]["airticket_arrival_time"].ToString(),

                                    airticket_departure_date = drdetail[k]["airticket_departure_date"].ToString(),
                                    airticket_arrival_date = drdetail[k]["airticket_arrival_date"].ToString(),

                                    check_my_trip = drdetail[k]["check_my_trip"].ToString(),
                                    airticket_root = drdetail[k]["airticket_root"].ToString(),

                                    action_type = drdetail[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }

                        }
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;
                        data.img_list = imgList;
                    }

                    #region check data and new row default
                    if (data.airticket_detail.Count == 0) { data.airticket_detail.Add(new airticketList { }); }
                    if (data.airticket_booking.Count == 0) { data.airticket_booking.Add(new airticketbookList { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.m_book_status.Count == 0) { data.m_book_status.Add(new MStatusModel { }); }
                    if (data.m_book_type.Count == 0) { data.m_book_type.Add(new MStatusModel { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }

                #endregion emp in travel
            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            if (data.airticket_booking == null)
            {
                data.airticket_booking.Add(new airticketbookList
                {
                    doc_id = doc_id,
                    emp_id = "",
                    id = "",

                    action_type = "",
                    action_change = "false",
                });
            }
            if (data.airticket_detail == null)
            {
                data.airticket_detail.Add(new airticketList
                {
                    doc_id = doc_id,
                    emp_id = "",
                    id = "",
                    action_type = "",
                    action_change = "false",
                });
            }

            return data;
        }
        public AccommodationOutModel SearchAccommodation(AccommodationModel value)
        {
            var msg = "";
            var page_name = "accommodation";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new AccommodationOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_request = refdata_role_review(token_login, doc_id);

            try
            {
                DataTable dtbookstatus = refdata_book_status(page_name);
                if (dtbookstatus.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtbookstatus.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_book_status.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }

                DataTable dtbooktype = refdata_book_type();
                if (dtbooktype.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtbooktype.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_book_type.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }
                DataTable dtroomtype = refdata_room_type();
                if (dtroomtype.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtroomtype.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_room_type.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }

                #region emp in travel
                try
                {
                    Boolean user_admin = true;
                    DataTable dtdata = refdata_emp_detail(value.token_login, value.doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtbook = refdata_accom_book(value.token_login, value.doc_id, "", user_admin);
                    DataTable dtbookdetail = refdata_accom_book_detail(value.token_login, value.doc_id, "", user_admin);

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = data.emp_list[i].emp_id.ToString();
                            if (emp_id_select == "") { continue; }

                            // DataRow[] drbook = dtbook.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drbook = dtbook.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                            for (int k = 0; k < drbook.Length; k++)
                            {
                                //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                                if (dremp_list != null && dremp_list.Count > 0)
                                {
                                    dremp_list[0].doc_status_id = drbook[k]["doc_status"].ToString();
                                }

                                //ตามจำนวน employee
                                data.accommodation_booking.Add(new accommodationbookList
                                {
                                    doc_id = doc_id,
                                    emp_id = emp_id_select,
                                    id = drbook[k]["id"].ToString(),
                                    doc_status = drbook[k]["doc_status"].ToString(),

                                    booking = drbook[k]["booking"].ToString(),
                                    search = drbook[k]["search"].ToString(),
                                    recommend = drbook[k]["recommend"].ToString(),
                                    already_booked = drbook[k]["already_booked"].ToString(),
                                    already_booked_other = drbook[k]["already_booked_other"].ToString(),

                                    additional_request = drbook[k]["additional_request"].ToString(),
                                    already_booked_id = drbook[k]["already_booked_id"].ToString(),

                                    booking_status = drbook[k]["booking_status"].ToString(),
                                    place_name = drbook[k]["place_name"].ToString(),
                                    map_url = drbook[k]["map_url"].ToString(),

                                    action_type = drbook[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }

                            // DataRow[] drdetail = dtbookdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drdetail = dtbookdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                            for (int k = 0; k < drdetail.Length; k++)
                            {
                                //จำวนวนรายการ detail ตามจำนวน employee  
                                data.accommodation_detail.Add(new accommodationList
                                {
                                    doc_id = doc_id,
                                    emp_id = emp_id_select,
                                    id = drdetail[k]["id"].ToString(),

                                    country = drdetail[k]["country"].ToString(),
                                    hotel_name = drdetail[k]["hotel_name"].ToString(),
                                    check_in = drdetail[k]["check_in"].ToString(),
                                    check_out = drdetail[k]["check_out"].ToString(),
                                    roomtype = drdetail[k]["roomtype"].ToString(),

                                    action_type = drdetail[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }

                        }
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;

                        data.img_list = imgList;
                    }

                    #region check data and new row default
                    if (data.accommodation_detail.Count == 0) { data.accommodation_detail.Add(new accommodationList { }); }
                    if (data.accommodation_booking.Count == 0) { data.accommodation_booking.Add(new accommodationbookList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }
                #endregion emp in travel

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }


            return data;
        }

        public VisaOutModel SearchVisa(VisaModel value)
        {
            //กรณีที่ข้อมูล personal แสดงว่ามาจากการกดเข้าหน้าหน้า Visa ตรงๆ
            value.doc_id = value.doc_id + "";

            var msg = "";
            var page_name = "visa";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new VisaOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            data.pdpa_wording = refdata_pdpa_wording();

            if (true)
            {
                DataTable dtm_continent = ref_m_continent();
                DataTable dtm_country = ref_m_country();

                sqlstr = @"  select distinct te.dte_emp_id as emp_id, te.ct_id as country_id, ct.ct_name as country_name    
                                 from bz_doc_traveler_expense te
                                 inner join  bz_master_country ct on te.ct_id = ct.ct_id
                                 where 1=1 ";
                if (doc_id != "personal")
                {
                    if (doc_id != "") { sqlstr += @" and te.dh_code = '" + doc_id + "' "; }
                }
                DataTable dtcountry = new DataTable();
                SetDocService.conn_ExecuteData(ref dtcountry, sqlstr);
                if (dtcountry.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtcountry.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.country_doc.Add(new MasterCountryModel
                        {
                            emp_id = dt.Rows[i]["emp_id"].ToString(),
                            country_id = dt.Rows[i]["country_id"].ToString(),
                            country_name = dt.Rows[i]["country_name"].ToString(),
                        });
                    }
                }

                if (dtm_continent.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_continent.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_continent.Add(new MasterNormalModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }
                if (dtm_country.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_country.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Boolean country_active_in_doc = false;
                        string country_id_select = dt.Rows[i]["id"].ToString();
                        if (doc_id != "personal")
                        {
                            try
                            {
                                // DataRow[] drcheck_ct = dtcountry.Select("country_id ='" + country_id_select + "' ");$
                                DataRow[] drcheck_ct = dtcountry.AsEnumerable().Where(s => s.Field<string>("country_id") == country_id_select).ToArray();
                                if (drcheck_ct.Length > 0) { country_active_in_doc = true; } else { continue; }
                            }
                            catch
                            { }
                        }
                        data.m_country.Add(new MasterNormalModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            id_main = dt.Rows[i]["id_main"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }
            }

            #region emp in travel
            try
            {
                try
                {
                    Boolean user_admin = true;
                    DataTable dtdata = refdata_emp_detail(value.token_login, value.doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList;
                    if (doc_id == "personal")
                    {
                        imgList = refdata_img_list_personal(token_login, doc_id, page_name, "", token_login);
                    }
                    else
                    {
                        imgList = refdata_img_list(doc_id, page_name, "", token_login);
                        //imgList = refdata_img_list_visa(token_login, doc_id, page_name, "", token_login);
                    }

                    DataTable dtdetail = refdata_visa_detail(value.token_login, value.doc_id, "", user_admin);

                    int imaxid_mail = 0;
                    DataTable dtmail = refdata_data_mail(token_login, doc_id, page_name, "", "", user_admin, ref imaxid_mail);


                    int imaxid = sw.GetMaxID(TableMaxId.BZ_DATA_VISA); ;

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }
                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
                            Boolean bactive_in_doc = false;

                            // DataRow[] drdetail = dtdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drdetail = dtdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                            if (drdetail.Length > 0)
                            {
                                //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                string doc_status = "1";
                                try { doc_status = drdetail[0]["doc_status"].ToString(); } catch { }

                                for (int k = 0; k < drdetail.Length; k++)
                                {
                                    //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
                                    if (drdetail[k]["visa_active_in_doc"].ToString() == "true")
                                    {
                                        bactive_in_doc = true;
                                        doc_status = drdetail[k]["doc_status"].ToString();
                                    }

                                    //จำวนวนรายการ detail ตามจำนวน employee
                                    data.visa_detail.Add(new visaList
                                    {
                                        doc_id = data.emp_list[i].doc_id.ToString(),
                                        emp_id = data.emp_list[i].emp_id.ToString(),
                                        id = drdetail[k]["id"].ToString(),

                                        //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
                                        visa_active_in_doc = drdetail[k]["visa_active_in_doc"].ToString(),

                                        visa_place_issue = drdetail[k]["visa_place_issue"].ToString(),
                                        visa_valid_from = drdetail[k]["visa_valid_from"].ToString(),
                                        visa_valid_to = drdetail[k]["visa_valid_to"].ToString(),
                                        visa_valid_until = drdetail[k]["visa_valid_until"].ToString(),
                                        visa_type = drdetail[k]["visa_type"].ToString(),
                                        visa_category = drdetail[k]["visa_category"].ToString(),
                                        visa_entry = drdetail[k]["visa_entry"].ToString(),
                                        visa_name = drdetail[k]["visa_name"].ToString(),
                                        visa_surname = drdetail[k]["visa_surname"].ToString(),
                                        visa_date_birth = drdetail[k]["visa_date_birth"].ToString(),
                                        visa_nationality = drdetail[k]["visa_nationality"].ToString(),
                                        passport_no = drdetail[k]["passport_no"].ToString(),
                                        visa_sex = drdetail[k]["visa_sex"].ToString(),
                                        visa_authorized_signature = drdetail[k]["visa_authorized_signature"].ToString(),
                                        visa_remark = drdetail[k]["visa_remark"].ToString(),
                                        visa_card_id = drdetail[k]["visa_card_id"].ToString(),
                                        visa_serial = drdetail[k]["visa_serial"].ToString(),

                                        default_type = drdetail[k]["default_type"].ToString(),
                                        default_action_change = "false",

                                        action_type = drdetail[k]["action_type"].ToString(),
                                        action_change = "false",

                                    });

                                }

                                List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                                if (dremp_list != null && dremp_list.Count > 0)
                                {
                                    dremp_list[0].doc_status_id = doc_status;
                                }



                            }
                            else
                            {
                                //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
                                bactive_in_doc = true;

                                data.visa_detail.Add(new visaList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    id = imaxid.ToString(),

                                    //Auwat 20210823 0000 เพิ่มข้อมูล status ของ visa ที่ตรง location ในใบงาน
                                    visa_active_in_doc = "true",

                                    doc_status = "1",

                                    default_type = "false",
                                    default_action_change = "false",

                                    action_type = "insert",
                                    action_change = "false",

                                });

                                imaxid++;
                            }

                            // DataRow[] drmail = dtmail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drmail = dtmail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                            for (int k = 0; k < drmail.Length; k++)
                            {
                                //จำวนวนรายการ detail ตามจำนวน employee
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    page_name = page_name,
                                    module = "",

                                    id = drmail[k]["id"].ToString(),
                                    mail_emp_id = drmail[k]["mail_emp_id"].ToString(),
                                    mail_to = drmail[k]["mail_to"].ToString(),
                                    mail_status = drmail[k]["mail_status"].ToString(),

                                    action_type = drmail[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }
                            if (drmail.Length == 0)
                            {
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    page_name = page_name,
                                    module = "",

                                    id = imaxid_mail.ToString(),

                                    action_type = "insert",
                                    action_change = "false",
                                });
                                imaxid_mail++;
                            }
                        }
                    }

                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;

                        data.img_list = imgList;

                    }

                    #region check data and new row default
                    if (data.visa_detail.Count == 0) { data.visa_detail.Add(new visaList { }); }
                    if (data.passport_list.Count == 0) { data.passport_list.Add(new passportList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    if (data.mail_list.Count == 0) { data.mail_list.Add(new mailselectList { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }
            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            data.remark = msg;

            #endregion emp in travel


            return data;
        }
        public KHCodeOutModel SearchKHCode(KHCodeModel value)
        {
            //กรณีที่ข้อมูล personal แสดงว่ามาจากการกดเข้าหน้าหน้า Visa ตรงๆ
            value.doc_id = value.doc_id + "";

            var msg = "";
            var page_name = "KHCode";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new KHCodeOutModel();

            data.token_login = token_login;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            DataTable dtkhcode = refdata_kh_code();

            if (dtkhcode.Rows.Count > 0)
            {
                dt = new DataTable();
                dt = dtkhcode.Copy(); dt.AcceptChanges();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.khcode_list.Add(new khcodeList
                    {
                        id = dt.Rows[i]["id"].ToString(),
                        emp_id = dt.Rows[i]["emp_id"].ToString(),
                        user_id = dt.Rows[i]["user_id"].ToString(),
                        oversea_code = dt.Rows[i]["oversea_code"].ToString(),
                        local_code = dt.Rows[i]["local_code"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        data_for_sap = dt.Rows[i]["data_for_sap"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),

                    });
                }
            }

            return data;
        }
        public TemplateKHCodeOutModel SearchTemplateKHCode(KHCodeModel value)
        {
            //กรณีที่ข้อมูล personal แสดงว่ามาจากการกดเข้าหน้าหน้า Visa ตรงๆ
            value.doc_id = value.doc_id + "";

            var msg = "";
            var page_name = "KHCode";
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new TemplateKHCodeOutModel();
            data.token_login = token_login;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            string file_page = page_name;
            string _foler_name = "DocumentFile";
            string _Folder = "/" + _foler_name + "/" + file_page + "/";
            //string _Path = System.Web.HttpContext.Current.Server.MapPath("~" + _Folder);

            string ServerPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI()?.ToString() ?? "";
            string url = $@"{ServerPathAPI}DocumentFile/khcode/";

            sqlstr = @" select a.id, a.file_name, a.path ||  a.file_name as file_path, a.file_size
                        , to_char((case when a.update_date is null then a.create_date else a.update_date end ),'dd Mon rrrr') as modified_date
                        , (case when u.usertype = 2 then u.enfirstname else nvl(u.entitle, '')|| ' ' || u.enfirstname || ' ' || u.enlastname  end ) modified_by 
                        from BZ_FILE_DATA a 
                        left join vw_bz_users u on (case when a.update_by is null then a.create_by else a.update_by end ) = u.userid 
                        where a.page_name = 'khcode'";

            if (SetDocService.conn_ExecuteData(ref dt, sqlstr) == "")
            {
                if (dt.Rows.Count > 0)
                {
                    data.fileName = dt.Rows[0]["file_name"].ToString();
                    data.fileSize = dt.Rows[0]["file_size"].ToString();
                    data.lastUploadDate = dt.Rows[0]["modified_date"].ToString();
                    data.uploadBy = dt.Rows[0]["modified_by"].ToString();
                    //\\10.224.43.14\EBiz2\EBiz_Webservice\DocumentFile\khcode
                    data.url = url + dt.Rows[0]["file_name"].ToString(); //dt.Rows[0]["file_path"].ToString();
                }
            }
            return data;
        }

        public PassportOutModel SearchPassport(PassportModel value)
        {
            //กรณีที่ข้อมูล personal แสดงว่ามาจากการกดเข้าหน้าหน้า Passport ตรงๆ
            value.doc_id = value.doc_id + "";

            var msg = "";
            var page_name = "passport";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new PassportOutModel();


            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            data.pdpa_wording = refdata_pdpa_wording();

            #region emp in travel
            try
            {
                try
                {
                    Boolean user_admin = true;
                    DataTable dtdata = refdata_emp_detail(value.token_login, value.doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList;
                    if (doc_id == "personal")
                    {
                        imgList = refdata_img_list_personal(token_login, doc_id, page_name, "", token_login);
                    }
                    else
                    {
                        imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    }


                    DataTable dtbookdetail = refdata_passport_detail(value.token_login, value.doc_id, "", user_admin);
                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }

                            // DataRow[] drdetail = dtbookdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drdetail = dtbookdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < drdetail.Length; k++)
                            {
                                //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                                if (dremp_list != null && dremp_list.Count > 0)
                                {
                                    dremp_list[0].doc_status_id = drdetail[0]["doc_status"].ToString();
                                }

                                //จำวนวนรายการ detail ตามจำนวน employee
                                data.passport_detail.Add(new passportList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    id = drdetail[k]["id"].ToString(),
                                    doc_status = drdetail[k]["doc_status"].ToString(),

                                    passport_no = drdetail[k]["passportno"].ToString(),
                                    passport_date_issue = drdetail[k]["dateofissue"].ToString(),
                                    passport_date_expire = drdetail[k]["dateofexpire"].ToString(),
                                    passport_title = drdetail[k]["titlename"].ToString(),
                                    passport_name = drdetail[k]["firstname"].ToString(),
                                    passport_surname = drdetail[k]["lastname"].ToString(),
                                    passport_date_birth = drdetail[k]["dateofbirth"].ToString(),
                                    accept_type = (drdetail[k]["accept_type"].ToString() == "true" ? true : false),

                                    action_type = drdetail[k]["action_type"].ToString(),
                                    default_type = drdetail[k]["default_type"].ToString(),
                                    default_action_change = "false",
                                    action_change = "false",
                                    ref_id = drdetail[k]["ref_id"].ToString(),
                                    sap_status = drdetail[k]["sap_status"].ToString(),

                                    sort_by = (drdetail[k]["default_type"].ToString() == "true" ? "0" : (drdetail[k]["sort_by"].ToString() == "" ? drdetail[k]["id"].ToString() : drdetail[k]["sort_by"].ToString())),

                                });
                            }

                            //กรณี personal ให้ดึงข้อมูลไปแสดงแค่ชุดเดียวก็พอ doc id ไหนก็ได้
                            if (doc_id == "personal") { break; }
                        }
                    }

                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.img_list = imgList;
                    }

                    #region check data and new row default
                    if (data.passport_detail.Count == 0) { data.passport_detail.Add(new passportList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }
            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            data.remark = msg;

            #endregion emp in travel


            return data;
        }
        public AllowanceOutModel SearchAllowance(AllowanceModel value)
        {
            var msg = "";
            var page_name = "allowance";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new AllowanceOutModel();

            Boolean user_admin = true;
            DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            try
            {
                DataTable dtm_exchangerate = ref_exchangerate();
                DataTable dtm_currency = ref_currency();
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
                if (dtm_currency.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_currency.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_currency.Add(new CurrencyList
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }

                DataTable dtbooktype = refdata_m_allowance_type();
                if (dtbooktype.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtbooktype.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_allowance_type.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }
                DataTable dtuserall = refdata_user_all(page_name);
                if (dtuserall.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtuserall.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_empmail_list.Add(new emailList
                        {
                            emp_id = dt.Rows[i]["emp_id"].ToString(),
                            emp_name = dt.Rows[i]["emp_name"].ToString(),
                            emp_unit = dt.Rows[i]["emp_unit"].ToString(),
                            emp_type = dt.Rows[i]["emp_type"].ToString(),
                            email = dt.Rows[i]["email"].ToString()
                        });

                    }
                }

                #region emp in travel
                try
                {
                    //Boolean user_admin = true; 
                    //DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtallowance = refdata_allowance(token_login, doc_id, "", user_admin);
                    DataTable dtallowancedetail = refdata_allowance_detail(token_login, doc_id, "", user_admin);
                    DataTable dtallowancemail = refdata_allowance_mail(token_login, doc_id, "", user_admin);


                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(token_login, doc_id, i.ToString(), dtdata.Rows[i], dtvisa));
                        }
                    }
                    else
                    {
                        data.emp_list.Add(new_emp_list(token_login, user_admin));
                    }
                    if (dtallowance.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtallowance.Rows.Count; i++)
                        {
                            DataRow dr = dtallowance.Rows[i];

                            var emp_id_select = dr["emp_id"].ToString();
                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                            List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                            if (dremp_list != null && dremp_list.Count > 0)
                            {
                                dremp_list[0].doc_status_id = dr["doc_status"].ToString();
                            }

                            string passport = dr["passport"].ToString();
                            string passport_date = dr["passport_date"].ToString();
                            string luggage_clothing = dr["luggage_clothing"].ToString();
                            string luggage_clothing_date = dr["luggage_clothing_date"].ToString();
                            if (true)
                            {
                                try
                                {
                                    Create_Trip.searchDocCreateServices clsCreate = new Create_Trip.searchDocCreateServices();
                                    using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                                    {
                                        var est = clsCreate.EstimateExpense(context, doc_id, emp_id_select);
                                        if (est.PassportExpense.ToString() != "")
                                        {
                                            passport = est.PassportExpense.ToString();
                                        }
                                        if (est.PassportDate.ToString() != "")
                                        {
                                            passport_date = clsCreate.convert_date_display(est.PassportDate.ToString());
                                        }
                                        if (dr["action_type"].ToString() == "insert")
                                        {
                                            if (est.CLExpense.ToString() != "")
                                            {
                                                luggage_clothing = est.CLExpense.ToString();
                                            }
                                            if (est.CLDate.ToString() != "")
                                            {
                                                luggage_clothing_date = clsCreate.convert_date_display(est.CLDate.ToString());//2021-03-11
                                            }
                                        }

                                    }
                                }
                                catch { }
                            }

                            data.allowance_main.Add(new allowanceList
                            {
                                doc_id = doc_id,
                                emp_id = dr["emp_id"].ToString(),
                                id = dr["id"].ToString(),
                                doc_status = dr["doc_status"].ToString(),

                                grand_total = dr["grand_total"].ToString(),
                                remark = dr["remark"].ToString(),

                                passport = passport.ToString(),
                                passport_date = passport_date.ToString(),
                                luggage_clothing = luggage_clothing.ToString(),
                                luggage_clothing_date = luggage_clothing_date.ToString(),

                                file_travel_report = dr["file_travel_report"].ToString(),
                                file_report = dr["file_report"].ToString(),

                                action_type = dr["action_type"].ToString(),
                                action_change = dr["action_change"].ToString(),//ต่างจาก module อื่น เนื่อวจากส่งมาจากตอน get data
                            });

                        }
                    }
                    else
                    {
                        data.allowance_main.Add(new allowanceList
                        {
                            doc_id = doc_id,
                            emp_id = "",
                            id = "",
                            doc_status = "1",

                            action_type = "",
                            action_change = "",
                        });
                    }
                    if (dtallowancedetail.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtallowancedetail.Rows.Count; i++)
                        {
                            DataRow dr = dtallowancedetail.Rows[i];
                            data.allowance_detail.Add(new allowancedetailList
                            {
                                doc_id = doc_id,
                                emp_id = dr["emp_id"].ToString(),
                                id = dr["id"].ToString(),

                                allowance_days = dr["allowance_days"].ToString(),
                                allowance_date = dr["allowance_date"].ToString(),
                                allowance_low = dr["allowance_low"].ToString(),
                                allowance_mid = dr["allowance_mid"].ToString(),
                                allowance_hight = dr["allowance_hight"].ToString(),
                                allowance_total = dr["allowance_total"].ToString(),
                                allowance_unit = dr["allowance_unit"].ToString(),
                                allowance_hrs = dr["allowance_hrs"].ToString(),
                                exchange_rate = dr["exchange_rate"].ToString(),
                                allowance_remark = dr["allowance_remark"].ToString(),
                                allowance_type_id = dr["allowance_type_id"].ToString(),
                                allowance_values_def = dr["allowance_values_def"].ToString(),

                                action_type = dr["action_type"].ToString(),
                                action_change = dr["action_change"].ToString(),//ต่างจาก module อื่น เนื่อวจากส่งมาจากตอน get data

                            });
                        }
                    }
                    else
                    {
                        data.allowance_detail.Add(new allowancedetailList
                        {
                            doc_id = doc_id,
                            emp_id = "",
                            id = "1",
                            allowance_type_id = "2",

                            action_type = "insert",
                            action_change = "false",

                        });
                    }
                    if (dtallowancemail.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtallowancemail.Rows.Count; i++)
                        {
                            DataRow dr = dtallowancemail.Rows[i];
                            data.mail_list.Add(new mailselectList
                            {
                                doc_id = doc_id,
                                emp_id = dr["emp_id"].ToString(),
                                id = dr["id"].ToString(),

                                mail_emp_id = dr["mail_emp_id"].ToString(),
                                mail_to = dr["mail_to"].ToString(),
                                mail_status = dr["mail_status"].ToString(),

                                action_type = dr["action_type"].ToString(),
                                action_change = "false",
                            });
                        }
                    }
                    else
                    {
                        data.mail_list.Add(new mailselectList
                        {
                            doc_id = doc_id,
                            emp_id = "",
                            id = "1",
                            action_type = "insert",
                            action_change = "false",
                        });
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;

                        data.gl_account = "5820100";
                        data.cost_center = "0000113067";
                        data.io_wbs = "R-1118444-AD-00060-00034";

                        data.img_list = imgList;
                    }

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }

                #endregion emp in travel

                #region check data and new row default
                if (data.allowance_main.Count == 0) { data.allowance_main.Add(new allowanceList { }); }
                if (data.allowance_detail.Count == 0) { data.allowance_detail.Add(new allowancedetailList { }); }

                if (data.m_allowance_type.Count == 0) { data.m_allowance_type.Add(new MStatusModel { }); }

                if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                if (data.mail_list.Count == 0) { data.mail_list.Add(new mailselectList { }); }
                #endregion check data and new row default  

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }


            return data;
        }
        public AllowanceMasterOutModel SearchAllowanceMaster(AllowanceModel value)
        {
            var msg = "";
            var page_name = "allowance";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new AllowanceMasterOutModel();

            data.token_login = token_login;

            try
            {
                DataTable dtm_exchangerate = ref_exchangerate();
                DataTable dtm_currency = ref_currency();
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
                if (dtm_currency.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_currency.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_currency.Add(new CurrencyList
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }

                DataTable dtbooktype = refdata_m_allowance_type();
                if (dtbooktype.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtbooktype.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_allowance_type.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }
                DataTable dtuserall = refdata_user_all(page_name);
                if (dtuserall.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtuserall.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_empmail_list.Add(new emailList
                        {
                            emp_id = dt.Rows[i]["emp_id"].ToString(),
                            emp_name = dt.Rows[i]["emp_name"].ToString(),
                            emp_unit = dt.Rows[i]["emp_unit"].ToString(),
                            emp_type = dt.Rows[i]["emp_type"].ToString(),
                            email = dt.Rows[i]["email"].ToString()
                        });

                    }
                }

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }


            return data;
        }
        public ReimbursementOutModel SearchReimbursement(ReimbursementModel value)
        {
            var msg = "";
            var page_name = "reimbursement";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new ReimbursementOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            try
            {
                DataTable dtm_exchangerate = ref_exchangerate();
                dtm_exchangerate = ref_exchangerate_all_type();
                DataTable dtm_currency = ref_currency();
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
                if (dtm_currency.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_currency.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_currency.Add(new CurrencyList
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }

                #region emp in travel 
                try
                {
                    Boolean user_admin = true;
                    DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtmain = refdata_reimbursement(token_login, doc_id, "", user_admin);
                    DataTable dtdetail = refdata_reimbursement_detail(token_login, doc_id, "", user_admin);

                    int imaxid_mail = 0;
                    DataTable dtmail = refdata_data_mail(token_login, doc_id, page_name, "", "", user_admin, ref imaxid_mail);

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }

                            // DataRow[] dr = dtmain.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] dr = dtmain.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < dr.Length; k++)
                            {
                                //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                                if (dremp_list != null && dremp_list.Count > 0)
                                {
                                    dremp_list[0].doc_status_id = dr[k]["doc_status"].ToString();
                                }

                                //ตามจำนวน employee
                                data.reimbursement_main.Add(new reimbursementList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    id = dr[k]["id"].ToString(),
                                    sendmail_to_traveler = dr[k]["sendmail_to_traveler"].ToString(),

                                    remark = dr[k]["remark"].ToString(),

                                    file_travel_report = dr[k]["file_travel_report"].ToString(),
                                    file_report = dr[k]["file_report"].ToString(),

                                    action_type = dr[k]["action_type"].ToString(),
                                    action_change = "false",

                                });
                            }

                            // DataRow[] drdetail = dtdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drdetail = dtdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < drdetail.Length; k++)
                            {
                                //ตามจำนวน employee
                                data.reimbursement_detail.Add(new reimbursementdetailList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    id = drdetail[k]["id"].ToString(),
                                    reimbursement_date = drdetail[k]["reimbursement_date"].ToString(),
                                    details = drdetail[k]["details"].ToString(),
                                    exchange_rate = drdetail[k]["exchange_rate"].ToString(),
                                    currency = drdetail[k]["currency"].ToString(),
                                    as_of = drdetail[k]["as_of"].ToString(),
                                    total = drdetail[k]["total"].ToString(),
                                    grand_total = drdetail[k]["grand_total"].ToString(),
                                    remark = drdetail[k]["remark"].ToString(),
                                    action_type = drdetail[k]["action_type"].ToString(),
                                    action_change = "false",

                                });

                                List<ExchangeRateList> drexrate = data.m_exchangerate.Where(a =>
                                    (a.currency_id == drdetail[k]["currency"].ToString()
                                    & a.date_from == drdetail[k]["as_of"].ToString())
                                    ).ToList();
                                if (drexrate.Count == 0)
                                {
                                    if (drdetail[k]["exchange_rate"].ToString() != "" && drdetail[k]["exchange_rate"].ToString() != "0")
                                    {
                                        data.m_exchangerate.Add(new ExchangeRateList
                                        {
                                            id = "",
                                            currency_id = drdetail[k]["currency"].ToString(),
                                            exchange_rate = drdetail[k]["exchange_rate"].ToString(),
                                            date_from = drdetail[k]["as_of"].ToString(),
                                            date_to = drdetail[k]["as_of"].ToString(),
                                        });
                                    }
                                }
                                else
                                {
                                    if (drdetail[k]["exchange_rate"].ToString() != "" && drdetail[k]["exchange_rate"].ToString() != "0")
                                    {
                                        drexrate[0].currency_id = drdetail[k]["currency"].ToString();
                                        drexrate[0].exchange_rate = drdetail[k]["exchange_rate"].ToString();
                                    }
                                }

                            }


                            // DataRow[] drmail = dtmail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drmail = dtmail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < drmail.Length; k++)
                            {
                                //จำวนวนรายการ detail ตามจำนวน employee
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    page_name = page_name,
                                    module = "",

                                    id = drmail[k]["id"].ToString(),
                                    mail_emp_id = drmail[k]["mail_emp_id"].ToString(),
                                    mail_to = drmail[k]["mail_to"].ToString(),
                                    mail_status = drmail[k]["mail_status"].ToString(),

                                    action_type = drmail[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }
                            if (drmail.Length == 0)
                            {
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    page_name = page_name,
                                    module = "",

                                    id = imaxid_mail.ToString(),

                                    action_type = "insert",
                                    action_change = "false",
                                });
                                imaxid_mail++;
                            }


                        }
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;
                        data.img_list = imgList;
                    }

                    #region check data and new row default
                    if (data.reimbursement_main.Count == 0) { data.reimbursement_main.Add(new reimbursementList { }); }
                    if (data.reimbursement_detail.Count == 0) { data.reimbursement_detail.Add(new reimbursementdetailList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }

                #endregion emp in travel

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }



            return data;
        }
        public TravelInsuranceOutModel SearchTravelInsurance(TravelInsuranceModel value)
        {
            //Family Relationships
            //https://atitaya11022522.wordpress.com/2013/10/11/family-members/ 

            //TravelInsurance 
            var msg = "";
            var page_name = "travelinsurance";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new TravelInsuranceOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            data.pdpa_wording = refdata_pdpa_wording();

            try
            {
                #region emp in travel 
                try
                {
                    DataTable dtm_insurance_plan = ref_m_insurance_plan();
                    if (dtm_insurance_plan.Rows.Count > 0)
                    {
                        dt = new DataTable();
                        dt = dtm_insurance_plan.Copy(); dt.AcceptChanges();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data.m_insurance_plan.Add(new MStatusModel
                            {
                                id = dt.Rows[i]["id"].ToString(),
                                name = dt.Rows[i]["name"].ToString(),
                                sort_by = dt.Rows[i]["sort_by"].ToString(),
                            });
                        }
                    }
                    DataTable dtm_broker = ref_m_broker();
                    if (dtm_broker.Rows.Count > 0)
                    {
                        dt = new DataTable();
                        dt = dtm_broker.Copy(); dt.AcceptChanges();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data.m_broker.Add(new MStatusModel
                            {
                                id = dt.Rows[i]["id"].ToString(),
                                name = dt.Rows[i]["name"].ToString(),
                                sort_by = dt.Rows[i]["sort_by"].ToString(),
                            });
                        }
                    }


                    Boolean user_admin = false;
                    DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    List<ImgList> imgListCertificate = refdata_img_list(doc_id, page_name, "certificate", token_login);

                    DataTable dtdetail = refdata_Insurance_detail(token_login, doc_id, "", user_admin);

                    int imaxid_mail = 1;
                    DataTable dtmail = refdata_data_mail(token_login, doc_id, page_name, "", "", user_admin, ref imaxid_mail);

                    string ServerPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI()?.ToString() ?? "";
                    //Insurance Coverage.pdf 
                    //"path": "http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/Image/OB21080156/travelinsurance/00001333/",
                    //"filename": "Img_001.png",
                    //string file_coverage_path = @"http://TBKC-DAPPS-05.thaioil.localnet/ebiz_ws/DocumentFile/Travelinsurance/";
                    string file_coverage_path = $@"{ServerPathAPI}DocumentFile/Travelinsurance/";
                    string file_coverage_name = "FPG Wording Travel Insurance.pdf";

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }

                            // DataRow[] dr = dtdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] dr = dtdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < dr.Length; k++)
                            {

                                //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                                List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                                if (dremp_list != null && dremp_list.Count > 0)
                                {
                                    dremp_list[0].doc_status_id = dr[k]["doc_status"].ToString();
                                }

                                //ตามจำนวน employee
                                data.travelInsurance_detail.Add(new travelinsuranceList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    id = dr[k]["id"].ToString(),
                                    doc_status = dr[k]["doc_status"].ToString(),

                                    ins_emp_id = dr[k]["ins_emp_id"].ToString(),
                                    ins_emp_name = dr[k]["ins_emp_name"].ToString(),
                                    ins_emp_org = dr[k]["ins_emp_org"].ToString(),
                                    ins_emp_passport = dr[k]["ins_emp_passport"].ToString(),

                                    billing_charge = dr[k]["billing_charge"].ToString(),//Company Name

                                    ins_emp_address = dr[k]["ins_emp_address"].ToString(),
                                    ins_emp_occupation = dr[k]["ins_emp_occupation"].ToString(),
                                    ins_emp_age = dr[k]["ins_emp_age"].ToString(),
                                    ins_emp_tel = dr[k]["ins_emp_tel"].ToString(),
                                    ins_emp_fax = dr[k]["ins_emp_fax"].ToString(),

                                    name_beneficiary = dr[k]["name_beneficiary"].ToString(),
                                    relationship = dr[k]["relationship"].ToString(),
                                    period_ins_dest = dr[k]["period_ins_dest"].ToString(),
                                    period_ins_from = dr[k]["period_ins_from"].ToString(),
                                    period_ins_to = dr[k]["period_ins_to"].ToString(),

                                    ins_plan = dr[k]["ins_plan"].ToString(),
                                    ins_broker = dr[k]["ins_broker"].ToString(),

                                    destination = (dr[k]["action_type"].ToString() == "insert" ? data.emp_list[i].country_city.ToString() : dr[k]["destination"].ToString()),

                                    date_expire = dr[k]["date_expire"].ToString(),
                                    duration = dr[k]["duration"].ToString(),

                                    agent_type = dr[k]["agent_type"].ToString(),
                                    broker_type = dr[k]["broker_type"].ToString(),
                                    travel_agent_type = dr[k]["travel_agent_type"].ToString(),
                                    insurance_company = dr[k]["insurance_company"].ToString(),

                                    certificates_no = dr[k]["certificates_no"].ToString(),
                                    certificates_total = dr[k]["certificates_total"].ToString(),
                                    remark = dr[k]["remark"].ToString(),
                                    sort_by = (dr[k]["sort_by"].ToString() == "" ? dr[k]["id"].ToString() : dr[k]["sort_by"].ToString()),

                                    file_outbound_path = "",//path doc
                                    file_outbound_name = "",//file doc

                                    //file_coverage_path, file_coverage_name
                                    file_coverage_path = file_coverage_path,//path doc
                                    file_coverage_name = file_coverage_name,//file doc

                                    action_type = dr[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }

                            // DataRow[] drmail = dtmail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drmail = dtmail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < drmail.Length; k++)
                            {
                                //จำวนวนรายการ detail ตามจำนวน employee
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    page_name = page_name,
                                    module = "",

                                    id = drmail[k]["id"].ToString(),
                                    mail_emp_id = drmail[k]["mail_emp_id"].ToString(),
                                    mail_to = drmail[k]["mail_to"].ToString(),
                                    mail_status = drmail[k]["mail_status"].ToString(),

                                    action_type = drmail[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }
                            if (drmail.Length == 0)
                            {
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    page_name = page_name,
                                    module = "",

                                    id = imaxid_mail.ToString(),

                                    action_type = "insert",
                                    action_change = "false",
                                });
                                imaxid_mail++;
                            }


                        }
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;
                        data.img_list = imgList;
                        data.img_list_cert = imgListCertificate;
                    }


                    #region check data and new row default
                    if (data.travelInsurance_detail.Count == 0) { data.travelInsurance_detail.Add(new travelinsuranceList { }); }
                    if (data.img_list_cert.Count == 0) { data.img_list_cert.Add(new ImgList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    if (data.mail_list.Count == 0) { data.mail_list.Add(new mailselectList { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { LoggerFile.write(ex); data.token_login = ex.Message.ToString(); }

                #endregion emp in travel

            }
            catch (Exception ex)
            {
                LoggerFile.write(ex);
                data.token_login = ex.Message.ToString() + msg;
            }

            return data;
        }
        public ISOSOutModel SearchISOS(ISOSModel value)
        {
            //ISOS 
            var msg = "";
            var page_name = "isos";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new ISOSOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);
            try
            {
                DataTable dtm_broker = ref_m_broker_isos();
                if (dtm_broker.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_broker.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_broker.Add(new MMasterInsurancebrokerModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            email = dt.Rows[i]["email"].ToString(),
                            status = "false",
                            action_change = "false",
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }

                #region emp in travel 
                try
                {
                    Boolean user_admin = false;
                    DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtcontent = refdata_content_html(token_login, doc_id, "", page_name, user_admin);

                    string doc_status = "";
                    DataTable dtins_status = refdata_Insurance_detail(token_login, doc_id, "", user_admin);
                    if (dtins_status != null && dtins_status.Rows.Count > 0)
                    {
                        doc_status = dtins_status.Rows[0]["doc_status"].ToString();
                    }

                    DataTable dtrecord = refdata_ISOS_record(doc_id);

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));
                            data.emp_list[i].type_send_to_broker = "false";
                            try
                            {
                                if (dtrecord.Rows.Count > 0)
                                {
                                    // DataRow[] drselect = dtrecord.Select("emp_id='" + data.emp_list[i].emp_id.ToString() + "'");$
                                    DataRow[] drselect = dtrecord.AsEnumerable().Where(s => s.Field<string>("emp_id") == data.emp_list[i].emp_id.ToString()).ToArray();

                                    if (drselect.Length > 0) { data.emp_list[i].type_send_to_broker = "true"; }
                                }
                            }
                            catch { }

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }

                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                            List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                            if (dremp_list != null && dremp_list.Count > 0)
                            {
                                //dremp_list[0].doc_status_id = doc_status.ToString();
                                try
                                {
                                    if (dtins_status.Rows.Count > 0)
                                    {
                                        // DataRow[] drselect = dtins_status.Select("emp_id='" + emp_id_select + "'");$
                                        DataRow[] drselect = dtins_status.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                                        if (drselect.Length > 0)
                                        {
                                            dremp_list[0].doc_status_id = drselect[0]["doc_status"].ToString();
                                        }
                                    }
                                }
                                catch { }
                            }


                        }
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;
                        data.img_list = imgList;


                    }

                    //เป็นข้อมูลของทั้งองค์กร 
                    if (dtcontent.Rows.Count > 0)
                    {
                        string html_content = "";
                        //read file fullname 
                        string xfile_Log = "";
                        try
                        {
                            var content_path = dtcontent.Rows[0]["content_path"].ToString() ?? "";
                            var content_name = dtcontent.Rows[0]["content_name"].ToString() ?? "";
                            if (content_path != "")
                            {
                                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", content_path.Trim('/'), content_name.Trim('/'));

                                using (StreamReader sr = File.OpenText(fullPath))
                                {
                                    string _data = null;
                                    while ((_data = sr.ReadLine()) != null)
                                    {
                                        html_content += _data;
                                    }
                                }
                            }
                        }
                        catch (Exception ex_msg) { html_content = ex_msg.Message.ToString() + " localPath " + xfile_Log + " isos path " + dtcontent.Rows[0]["content_path"].ToString(); }

                        data.html_content = html_content;
                        if (imgList.Count > 0)
                        {
                            data.video_url = imgList[0].fullname;
                        }
                    }
                    else
                    {
                        data.html_content = "";
                        data.video_url = "";
                    }

                    #region check data and new row default
                    if (data.business_type_list.Count == 0) { data.business_type_list.Add(new MMasterNomalModel { }); }
                    if (data.isos_detail.Count == 0) { data.isos_detail.Add(new isosList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }

                #endregion emp in travel

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            return data;
        }
        public TransportationOutModel SearchTransportation(TransportationModel value)
        {
            var msg = "";
            var page_name = "transportation";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;
            var transport_id = "";
            var driver_status = false;


            var data = new TransportationOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.transportation_type = "B";//T=Training ;B=Bussiness ?? ยังไม่ทราบว่าข้อมูลมาจากส่วนไหน
            data.t_car_id = "";

            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            try
            {
                #region emp in travel
                try
                {
                    var parameters = new List<OracleParameter>();
                    var dtdoc_status = new List<EmpIdCheckModel>();

                    using (var context = new TOPEBizTravelerProfileEntitys())
                    {
                        sqlstr = @"select EMP_ID as emp_id, DOC_STATUS as check_type from BZ_DATA_CONTENT_EMP where doc_id = :doc_id ";
                        parameters = new List<OracleParameter>();
                        parameters.Add(new OracleParameter("doc_id", doc_id));
                        dtdoc_status = context.EmpIdCheckModels.FromSqlRaw(sqlstr, parameters.ToArray()).ToList();
                    }

                    Boolean user_admin = true;
                    DataTable dtdata = refdata_emp_detail(value.token_login, value.doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtcar = refdata_Transportation(value.token_login, value.doc_id, "");
                    string url_personal_car_document = refdata_TransportationURL(value.token_login, value.doc_id, "");
                    data.url_personal_car_document = url_personal_car_document.ToString();
                    DataTable dtcontent = refdata_content_html(token_login, doc_id, "", page_name, user_admin);
                    string doc_status = "1";
                    try
                    {
                        doc_status = dtcontent.Rows[0]["doc_status"].ToString();
                    }
                    catch { }

                    var emp_user_select = "";
                    if (dtcar.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtcar.Rows.Count; i++)
                        {
                            if (emp_user_select != "") { emp_user_select += ","; }
                            emp_user_select += "'" + dtcar.Rows[i]["ums_id"].ToString().ToUpper() + "'";

                            if (dtcar.Rows[i]["driver_name"].ToString() != "") { driver_status = true; }

                        }
                        transport_id = dtcar.Rows[0]["id"].ToString();
                    }
                    DataTable dtempdoc = refdata_emp_doc(ref emp_user_select);


                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                            List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                            if (dremp_list != null && dremp_list.Count > 0)
                            {
                                dremp_list[0].doc_status_id = doc_status.ToString();
                                try
                                {
                                    if (transport_id != "")
                                    {
                                        if (driver_status == true)
                                        {
                                            dremp_list[0].doc_status_id = "4";
                                        }
                                        else { if (data.user_admin == true) { dremp_list[0].doc_status_id = "3"; } else { dremp_list[0].doc_status_id = "2"; } }
                                    }
                                    else
                                    {
                                        transport_id = "2";
                                        try
                                        {
                                            if (dremp_list[0].doc_status_id.ToString() == "") { dremp_list[0].doc_status_id = "1"; }
                                        }
                                        catch { }
                                    }
                                }
                                catch { }
                            }

                            doc_status = "1";

                            var drdocstatus = dtdoc_status.Where(p => p.emp_id == emp_id_select).ToList().FirstOrDefault();
                            if (!string.IsNullOrEmpty(drdocstatus?.check_type))
                            {
                                dremp_list[0].doc_status_id = drdocstatus?.check_type?.ToString();
                            }

                        }
                    }
                    if (dtcar.Rows.Count > 0)
                    {
                        data.t_car_id = dtcar.Rows[0]["id"].ToString();
                        data.cars_doc_no = dtcar.Rows[0]["cars_doc_no"].ToString();


                        var vehicle_registration_befor = "";
                        var vehicle_registration_aftter = "";//vehicle_registration
                        var it_car_id = 0;
                        for (int i = 0; i < dtcar.Rows.Count; i++)
                        {
                            var emp_id_select = dtcar.Rows[i]["ums_id"].ToString();
                            vehicle_registration_befor = dtcar.Rows[i]["vehicle_registration"].ToString();
                            if (vehicle_registration_aftter != vehicle_registration_befor)
                            {
                                it_car_id++;
                                vehicle_registration_aftter = vehicle_registration_befor;
                                data.transportation_car.Add(new transportationCarList
                                {
                                    doc_id = doc_id,
                                    emp_id = emp_id_select,
                                    id = it_car_id.ToString(),
                                    t_car_id = it_car_id.ToString(),
                                    company_name = dtcar.Rows[i]["car_come_name"].ToString(),
                                    driver_name = dtcar.Rows[i]["driver_name"].ToString(),
                                    telephone_no = dtcar.Rows[i]["driver_tel"].ToString(),
                                    car_model = dtcar.Rows[i]["car_model"].ToString(),
                                    car_color = dtcar.Rows[i]["car_color"].ToString(),
                                    car_registration_no = dtcar.Rows[i]["vehicle_registration"].ToString(),
                                    remark = "",
                                    action_type = "update",
                                });
                            }

                            // DataRow[] dremp = dtempdoc.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] dremp = dtempdoc.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            var semp_id = emp_id_select;
                            var semp_name = "";
                            var semp_sname = "";// dtempdoc.Rows.Count.ToString();
                            var semp_display = "";//dremp.Length.ToString();
                            var semp_tel = "";// emp_user_select;
                            if (dremp.Length > 0)
                            {
                                semp_id = dremp[0]["emp_id"].ToString();
                                semp_name = dremp[0]["firstname"].ToString();
                                semp_sname = dremp[0]["lastname"].ToString();
                                semp_display = dremp[0]["userDisplay"].ToString();
                                semp_tel = dremp[0]["userTel"].ToString();
                            }
                            data.transportation_detail.Add(new transportationList
                            {
                                doc_id = doc_id,
                                id = it_car_id.ToString(),
                                t_car_id = it_car_id.ToString(),

                                //emp_id = data.emp_list[ino].emp_id,
                                //emp_name = data.emp_list[ino].firstname,
                                //emp_sname = data.emp_list[ino].lastname,
                                //emp_display = data.emp_list[ino].userDisplay,
                                //emp_tel = data.emp_list[ino].userTel,


                                emp_id = semp_id,
                                emp_name = semp_name,
                                emp_sname = semp_sname,
                                emp_display = semp_display,
                                emp_tel = dtcar.Rows[i]["tel_no"].ToString(),

                                travel_date = dtcar.Rows[i]["use_date_str"].ToString(),
                                travel_time = dtcar.Rows[i]["use_time_from"].ToString() + "-" + dtcar.Rows[i]["use_time_to"].ToString(),
                                place_from = dtcar.Rows[i]["location_from"].ToString(),
                                place_to = dtcar.Rows[i]["location_to"].ToString(),
                                place_from_url = dtcar.Rows[i]["location_from_url"].ToString(),
                                place_to_url = dtcar.Rows[i]["location_to_url"].ToString(),
                                place_type = dtcar.Rows[i]["transport_type"].ToString(),
                                car_type = dtcar.Rows[i]["car_type"].ToString(),
                                transport_desc = dtcar.Rows[i]["TRANSPORT_DETAIL"].ToString(),
                                remark = "",
                                action_type = "update"
                            });
                        }
                    }

                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.img_list = imgList;
                    }

                    //เป็นข้อมูลของทั้งองค์กร
                    if (dtcontent.Rows.Count > 0)
                    {
                        string html_content = "";
                        //read file fullname 
                        string xfile_Log = "";
                        try
                        {
                            var content_path = dtcontent.Rows[0]["content_path"].ToString() ?? "";
                            var content_name = dtcontent.Rows[0]["content_name"].ToString() ?? "";
                            if (content_path != "")
                            {
                                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", content_path.Trim('/'), content_name.Trim('/'));

                                using (StreamReader sr = File.OpenText(fullPath))
                                {
                                    string _data = null;
                                    while ((_data = sr.ReadLine()) != null)
                                    {
                                        html_content += _data;
                                    }
                                }
                            }
                        }
                        catch (Exception ex_msg) { html_content = ex_msg.Message.ToString() + " localPath " + xfile_Log + " isos path " + dtcontent.Rows[0]["content_path"].ToString(); }

                        data.html_content = html_content;
                        if (imgList.Count > 0)
                        {
                            data.video_url = imgList[0].fullname;
                        }
                    }
                    else
                    {
                        data.html_content = "";
                        data.video_url = "";
                    }

                    #region check data and new row default
                    if (data.transportation_car.Count == 0) { data.transportation_car.Add(new transportationCarList { }); }
                    if (data.transportation_detail.Count == 0) { data.transportation_detail.Add(new transportationList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { LoggerFile.write(ex); data.token_login = ex.Message.ToString(); }

                #endregion emp in travel

            }
            catch (Exception ex) { LoggerFile.write(ex); data.token_login = ex.Message.ToString() + msg; }


            return data;
        }


        public FeedbackOutModel SearchFeedback(FeedbackModel value)
        {
            //Feedback 
            var msg = "";
            var page_name = "feedback";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new FeedbackOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = false;
            data.user_request = refdata_role_review(token_login, doc_id);

            data.feedback_for = "1";//1 : Bussine Trip, 2 : Traing
            try
            {
                if (doc_id.ToLower().IndexOf("t") > -1) { data.feedback_for = "2"; }

            }
            catch { }


            try
            {
                DataTable dttype = refdata_m_feedback_type();
                DataTable dtlist = refdata_m_feedback_list();

                if (dttype.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dttype.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.feedback_type_master.Add(new MFeedbackTypeModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }
                if (dtlist.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtlist.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.feedback_topic_list.Add(new MFeedbackListModel
                        {
                            feedback_type_id = dt.Rows[i]["feedback_type_id"].ToString(),
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString()
                        });
                    }
                }


                #region emp in travel 
                try
                {
                    Boolean user_admin = false;
                    DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtdetail = refdata_Feedback_detail(token_login, doc_id, "", user_admin);

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }

                            // DataRow[] dr = dtdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] dr = dtdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < dr.Length; k++)
                            {
                                string doc_status = "1";
                                try { doc_status = dr[i]["doc_status"].ToString(); } catch { }
                                List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                                if (dremp_list != null && dremp_list.Count > 0)
                                {
                                    dremp_list[0].doc_status_id = doc_status.ToString();
                                }

                                //ตามจำนวน employee
                                data.feedback_detail.Add(new feedbackList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    id = dr[k]["id"].ToString(),

                                    feedback_type_id = dr[k]["feedback_type_id"].ToString(),
                                    feedback_type_name = dr[k]["feedback_type_name"].ToString(),
                                    feedback_question_id = dr[k]["feedback_question_id"].ToString(),

                                    feedback_list_id = dr[k]["feedback_list_id"].ToString(),
                                    feedback_list_name = dr[k]["feedback_list_name"].ToString(),

                                    question_other = dr[k]["question_other"].ToString(),

                                    topic_id = dr[k]["topic_id"].ToString(),
                                    topic_name = dr[k]["topic_name"].ToString(),

                                    no = dr[k]["no"].ToString(),
                                    question = dr[k]["question"].ToString(),
                                    description = dr[k]["description"].ToString(),
                                    answer = dr[k]["answer"].ToString(),

                                    action_type = dr[k]["action_type"].ToString(),
                                    action_change = "false",

                                });
                            }


                        }
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                    }

                    #region check data and new row default
                    if (data.feedback_detail.Count == 0) { data.feedback_detail.Add(new feedbackList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    #endregion check data and new row default  

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }

                #endregion emp in travel

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            return data;
        }
        public EmployeeListOutModel SearchEmployeeList(EmployeeListModel value)
        {
            //Feedback 
            var msg = "";
            var data = new EmployeeListOutModel();
            data.token_login = value.token_login;

            var filter_value = value.filter_value;
            var msg_error = "";
            var ret = "";
            try
            {
                List<emplistModel> emp_list = new List<emplistModel>();
                ref_emp_list(ref emp_list, filter_value);
                data.emp_list = emp_list;
                ret = "true";

            }
            catch (Exception ex) { ret = "false"; msg_error = ex.Message.ToString() + msg; }


            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Search data succesed." : "Search data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        public void ref_emp_list(ref List<emplistModel> emp_list, string filter_value)
        {
            DataTable dtlist = refsearch_emp_list(filter_value);
            if (dtlist.Rows.Count > 0)
            {
                dt = new DataTable();
                dt = dtlist.Copy(); dt.AcceptChanges();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    emp_list.Add(new emplistModel
                    {
                        id = dt.Rows[i]["id"].ToString(),
                        emp_id = dt.Rows[i]["emp_id"].ToString(),
                        username = dt.Rows[i]["username"].ToString(),

                        titlename = dt.Rows[i]["titlename"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        lastname = dt.Rows[i]["lastname"].ToString(),
                        email = dt.Rows[i]["email"].ToString(),
                        displayname = dt.Rows[i]["displayname"].ToString(),
                        idicator = dt.Rows[i]["idicator"].ToString(),

                        category = dt.Rows[i]["category"].ToString(),
                        sections = dt.Rows[i]["sections"].ToString(),
                        department = dt.Rows[i]["department"].ToString(),
                        function = dt.Rows[i]["function"].ToString(),

                    });
                }
            }
        }
        public EmpRoleListOutModel SearchEmpRoleList(EmpRoleListModel value)
        {
            //Feedback 
            var msg = "";
            var data = new EmpRoleListOutModel();
            data.token_login = value.token_login;

            var filter_value = value.filter_value;
            var msg_error = "";
            var ret = "";
            try
            {
                DataTable dtlist = refsearch_emprole_list(filter_value);
                if (dtlist.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtlist.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        //id,emp_id,username,name_th,name_en,email,tel,mobile,img
                        data.emprole_list.Add(new emprolelistModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            emp_id = dt.Rows[i]["emp_id"].ToString(),
                            username = dt.Rows[i]["username"].ToString(),

                            name_th = dt.Rows[i]["name_th"].ToString(),
                            name_en = dt.Rows[i]["name_en"].ToString(),
                            email = dt.Rows[i]["email"].ToString(),

                            //--> กรณีที่มีข้อมูล travelerhistory  ให้ใช้ข้อมูล tel,mobile และ img  
                            tel = dt.Rows[i]["telephone"].ToString(),
                            mobile = dt.Rows[i]["mobile"].ToString(),
                            img = dt.Rows[i]["imgurl"].ToString(),
                            idicator = dt.Rows[i]["idicator"].ToString(),
                            role_type = filter_value,

                        });
                        if (filter_value.ToLower() == "contact_admin")
                        {
                            data.contactus = data.emprole_list;
                        }
                    }
                }
                ret = "true";

            }
            catch (Exception ex) { ret = "false"; msg_error = ex.Message.ToString() + msg; }


            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Search data succesed." : "Search data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;


            return data;
        }

        public TravelExpenseOutModel SearchTravelExpense(TravelExpenseModel value)
        {
            var msg = "";
            var page_name = "travelexpense";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            var data = new TravelExpenseOutModel();

            data.token_login = token_login;
            data.doc_id = doc_id;
            data.id = "1";
            data.user_admin = true;
            data.user_request = refdata_role_review(token_login, doc_id);

            try
            {
                //DataTable dtm_exchangerate = ref_exchangerate();
                DataTable dtm_exchangerate = ref_exchangerate_all_type();
                DataTable dtm_currency = ref_currency();
                DataTable dtm_expense_type = ref_expense_type();
                DataTable dtm_status = ref_status(page_name);
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
                if (dtm_currency.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_currency.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.m_currency.Add(new CurrencyList
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }
                if (dtm_expense_type.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_expense_type.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.dtm_expense_type.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }
                if (dtm_status.Rows.Count > 0)
                {
                    dt = new DataTable();
                    dt = dtm_status.Copy(); dt.AcceptChanges();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data.dtm_status.Add(new MStatusModel
                        {
                            id = dt.Rows[i]["id"].ToString(),
                            name = dt.Rows[i]["name"].ToString(),
                            sort_by = dt.Rows[i]["sort_by"].ToString(),
                        });
                    }
                }

                #region emp in travel 
                try
                {
                    Boolean user_admin = true;
                    DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
                    DataTable dtvisa = dtdata.Copy(); dtvisa.AcceptChanges();

                    List<ImgList> imgList = refdata_img_list(doc_id, page_name, "", token_login);
                    DataTable dtmain = refdata_travelexpense(token_login, doc_id, "", user_admin);
                    DataTable dtdetail = refdata_travelexpense_detail(token_login, doc_id, "", user_admin);

                    int imaxid_mail = 0;
                    DataTable dtmail = refdata_data_mail(token_login, doc_id, page_name, "", "", user_admin, ref imaxid_mail);

                    string doc_status = "";
                    DataTable dtins_status = refdata_Insurance_detail(token_login, doc_id, "", user_admin);
                    if (dtins_status != null && dtins_status.Rows.Count > 0)
                    {
                        doc_status = dtins_status.Rows[0]["doc_status"].ToString();
                    }

                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emp_list.Add(data_traveler_history(value.token_login, value.doc_id, i.ToString(), dtdata.Rows[i], dtvisa));

                            var emp_id_select = dtdata.Rows[i]["emp_id"].ToString();
                            if (emp_id_select == "") { continue; }

                            //Auwat 20210823 0000 เพิ่มข้อมูล status ของใบงาน --> 1: Not Start, 2: Traveler, 3: Business Team, 4: Completed
                            List<EmpListOutModel> dremp_list = data.emp_list.Where(a => (a.emp_id == emp_id_select)).ToList();
                            if (dremp_list != null && dremp_list.Count > 0)
                            {
                                //dremp_list[0].doc_status_id = doc_status.ToString();
                                try
                                {
                                    if (dtins_status.Rows.Count > 0)
                                    {
                                        // DataRow[] drselect = dtins_status.Select("emp_id='" + emp_id_select + "'");$
                                        DataRow[] drselect = dtins_status.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                                        if (drselect.Length > 0)
                                        {
                                            dremp_list[0].doc_status_id = drselect[0]["doc_status"].ToString();
                                        }
                                    }
                                }
                                catch { }
                            }


                            // DataRow[] dr = dtmain.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] dr = dtmain.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();
                            for (int k = 0; k < dr.Length; k++)
                            {
                                //ตามจำนวน employee
                                data.travelexpense_main.Add(new travelexpenseList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    id = dr[k]["id"].ToString(),
                                    send_to_sap = dr[k]["send_to_sap"].ToString(),
                                    status_trip_cancelled = dr[k]["status_trip_cancelled"].ToString(),

                                    remark = dr[k]["remark"].ToString(),
                                    action_type = dr[k]["action_type"].ToString(),
                                    action_change = "false",

                                });
                            }
                            // DataRow[] drdetail = dtdetail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drdetail = dtdetail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < drdetail.Length; k++)
                            {
                                //ตามจำนวน employee
                                data.travelexpense_detail.Add(new travelexpensedetailList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),


                                    id = drdetail[k]["id"].ToString(),
                                    expense_type = drdetail[k]["expense_type"].ToString(),
                                    data_date = drdetail[k]["data_date"].ToString(),
                                    status = drdetail[k]["status"].ToString(),

                                    exchange_rate = drdetail[k]["exchange_rate"].ToString(),
                                    currency = drdetail[k]["currency"].ToString(),
                                    as_of = drdetail[k]["as_of"].ToString(),

                                    total = drdetail[k]["total"].ToString(),
                                    grand_total = drdetail[k]["grand_total"].ToString(),
                                    remark = drdetail[k]["remark"].ToString(),

                                    status_active = (drdetail[k]["status"].ToString().ToLower() ?? "") == "6" ? "true" : "false",
                                    action_type = drdetail[k]["action_type"].ToString(),
                                    action_change = "false",

                                    ref_id = drdetail[k]["ref_id"].ToString(),
                                    sap_status = drdetail[k]["sap_status"].ToString(),

                                });

                                List<ExchangeRateList> drexrate = data.m_exchangerate.Where(a =>
                                    (a.currency_id == drdetail[k]["currency"].ToString()
                                    & a.date_from == drdetail[k]["as_of"].ToString())
                                    ).ToList();
                                if (drexrate.Count == 0)
                                {
                                    if (drdetail[k]["exchange_rate"].ToString() != "" && drdetail[k]["exchange_rate"].ToString() != "0")
                                    {
                                        data.m_exchangerate.Add(new ExchangeRateList
                                        {
                                            id = "",
                                            currency_id = drdetail[k]["currency"].ToString(),
                                            exchange_rate = drdetail[k]["exchange_rate"].ToString(),
                                            date_from = drdetail[k]["as_of"].ToString(),
                                            date_to = drdetail[k]["as_of"].ToString(),
                                        });
                                    }
                                }
                                else
                                {
                                    if (drdetail[k]["exchange_rate"].ToString() != "" && drdetail[k]["exchange_rate"].ToString() != "0")
                                    {
                                        drexrate[0].currency_id = drdetail[k]["currency"].ToString();
                                        drexrate[0].exchange_rate = drdetail[k]["exchange_rate"].ToString();
                                    }
                                }

                            }


                            // DataRow[] drmail = dtmail.Select("emp_id='" + emp_id_select + "'");$
                            DataRow[] drmail = dtmail.AsEnumerable().Where(s => s.Field<string>("emp_id") == emp_id_select).ToArray();

                            for (int k = 0; k < drmail.Length; k++)
                            {
                                //จำวนวนรายการ detail ตามจำนวน employee
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),

                                    page_name = page_name,
                                    module = "",

                                    id = drmail[k]["id"].ToString(),
                                    mail_emp_id = drmail[k]["mail_emp_id"].ToString(),
                                    mail_to = drmail[k]["mail_to"].ToString(),
                                    mail_status = drmail[k]["mail_status"].ToString(),

                                    action_type = drmail[k]["action_type"].ToString(),
                                    action_change = "false",
                                });
                            }

                            if (drmail.Length == 0)
                            {
                                data.mail_list.Add(new mailselectList
                                {
                                    doc_id = data.emp_list[i].doc_id.ToString(),
                                    emp_id = data.emp_list[i].emp_id.ToString(),
                                    page_name = page_name,
                                    module = "",

                                    id = imaxid_mail.ToString(),

                                    action_type = "insert",
                                    action_change = "false",
                                });
                                imaxid_mail++;
                            }


                        }
                    }
                    if (data.emp_list.Count > 0)
                    {
                        data.user_admin = user_admin;
                        data.user_display = data.emp_list[0].userDisplay;
                        data.travel_topic = data.emp_list[0].travel_topic;
                        data.business_date = data.emp_list[0].business_date;
                        data.travel_date = data.emp_list[0].travel_date;
                        data.country_city = data.emp_list[0].country_city;
                        data.img_list = imgList;
                    }

                    #region check data and new row default
                    if (data.travelexpense_main.Count == 0) { data.travelexpense_main.Add(new travelexpenseList { }); }
                    if (data.travelexpense_detail.Count == 0) { data.travelexpense_detail.Add(new travelexpensedetailList { }); }

                    if (data.emp_list.Count == 0) { data.emp_list.Add(new EmpListOutModel { }); }
                    if (data.img_list.Count == 0) { data.img_list.Add(new ImgList { }); }
                    if (data.mail_list.Count == 0) { data.mail_list.Add(new mailselectList { }); }
                    #endregion check data and new row default  
                }
                catch (Exception ex) { LoggerFile.write(ex); data.token_login = ex.Message.ToString(); }

                #endregion emp in travel

            }
            catch (Exception ex) { LoggerFile.write(ex); data.token_login = ex.Message.ToString() + msg; }



            return data;
        }
        public OpenDocOutModel SearchOpenDoc(PortalModel value)
        {
            var msg = "";
            var page_name = "portal";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;
            var id = "";
            Boolean user_admin = false;

            var data = new OpenDocOutModel();
            data.token_login = token_login;

            try
            {
                #region emp in travel  
                try
                {
                    DataTable dtplan = refdata_Up_coming_plan(token_login, doc_id, user_admin);

                    if (dtplan.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtplan.Rows.Count; i++)
                        {
                            data.up_coming_plan.Add(new upcomingplanList
                            {
                                doc_id = dtplan.Rows[i]["doc_id"].ToString(),
                                date = dtplan.Rows[i]["business_date_cn"].ToString(),
                                topic_of_traveling = dtplan.Rows[i]["topic_of_traveling"].ToString(),
                                country = dtplan.Rows[i]["country"].ToString(),
                                business_date = dtplan.Rows[i]["business_date"].ToString(),
                                button_status = dtplan.Rows[i]["button_status"].ToString(),

                                tab_no = dtplan.Rows[i]["tab_no"].ToString(),

                                action_type = "update",
                                action_change = "false"
                            });
                        }
                    }
                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }
                #endregion emp in travel 
            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            data.id = id;
            data.user_admin = user_admin;

            if (data.up_coming_plan.Count == 0) { data.up_coming_plan.Add(new upcomingplanList { }); }

            return data;
        }
        public PortalOutModel SearchPortal(PortalModel value)
        {
            var msg = "";
            var page_name = "portal";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = "";// value.doc_id;
            var id = "";
            Boolean user_admin = false;

            var data = new PortalOutModel();
            data.token_login = token_login;

            try
            {
                #region emp in travel  
                try
                {
                    string url_approval = "";// "https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/1%20APPROVAL";
                    string url_employee_payment = "";//"https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/2%20EMPLOYEE%20PAYMENT";
                    string url_transportation = "";// "https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/3%20TRANSPORTATION";
                    string url_others = "";//"https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/4%20OTHERS";

                    DataTable dtdata = refdata_Portal(token_login, ref user_admin, ref id);
                    DataTable dtplan = refdata_Up_coming_plan(token_login, doc_id, user_admin);
                    DataTable dtpractice_areas = refdata_practice_areas(ref url_approval, ref url_employee_payment, ref url_transportation, ref url_others);
                    if (dtdata.Rows.Count > 0)
                    {
                        data.practice_areas.Add(new practice_areasList
                        {
                            url_approval = url_approval,
                            url_employee_payment = url_employee_payment,
                            url_transportation = url_transportation,
                            url_others = url_others,
                        });

                        //1. APPROVAL 
                        //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/1%20APPROVAL

                        //2. EMPLOYEE PAYMENT
                        //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/2%20EMPLOYEE%20PAYMENT

                        //3. TRANSPORTATION
                        //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/3%20TRANSPORTATION

                        //4. OTHERS
                        //- https://thaioilintranet.thaioilgroup.com/departments/S013/Services/Business%20Trip/4%20OTHERS

                        string ServerPathAPI = top.ebiz.helper.AppEnvironment.GeteServerPathAPI() ?? "";
                        data.img_list.Add(new imgportalList
                        {
                            img_header = $"{ServerPathAPI}{dtdata.Rows[0]["img_header"].ToString()}",
                            img_personal_profile = $"{dtdata.Rows[0]["img_personal_profile"].ToString()}",

                            img_banner_1 = $"{dtdata.Rows[0]["img_banner_1"].ToString()}",
                            url_banner_1 = $"{dtdata.Rows[0]["url_banner_1"].ToString()}",

                            img_banner_2 = $"{dtdata.Rows[0]["img_banner_2"].ToString()}",
                            url_banner_2 = $"{dtdata.Rows[0]["url_banner_2"].ToString()}",

                            img_banner_3 = $"{dtdata.Rows[0]["img_banner_3"].ToString()}",
                            url_banner_3 = $"{dtdata.Rows[0]["url_banner_3"].ToString()}",

                            img_practice_areas = $"{dtdata.Rows[0]["img_practice_areas"].ToString()}",

                        });

                        if (dtplan.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtplan.Rows.Count; i++)
                            {
                                data.up_coming_plan.Add(new upcomingplanList
                                {
                                    doc_id = dtplan.Rows[i]["doc_id"].ToString(),
                                    date = dtplan.Rows[i]["business_date_cn"].ToString(),
                                    topic_of_traveling = dtplan.Rows[i]["topic_of_traveling"].ToString(),
                                    country = dtplan.Rows[i]["country"].ToString(),
                                    business_date = dtplan.Rows[i]["business_date"].ToString(),
                                    button_status = dtplan.Rows[i]["button_status"].ToString(),

                                    tab_no = dtplan.Rows[i]["tab_no"].ToString(),

                                    action_type = "update",
                                    action_change = "false"
                                });
                            }
                        }

                        //title
                        data.text_title = dtdata.Rows[0]["text_title"].ToString();
                        data.text_desc = dtdata.Rows[0]["text_desc"].ToString();

                        //get in touch
                        data.text_contact_title = dtdata.Rows[0]["text_contact_title"].ToString();
                        data.text_contact_email = dtdata.Rows[0]["text_contact_email"].ToString();
                        data.text_contact_tel = dtdata.Rows[0]["text_contact_tel"].ToString();

                        //url นโยบายการคุ้มครอง​ข้อมูลส่วนบุคคล (Privacy Policy)
                        data.url_employee_privacy_center = dtdata.Rows[0]["url_employee_privacy_center"].ToString();

                        data.action_type = "update";
                        data.action_change = "false";
                    }
                    else
                    {
                        //กรณีนี้ไม่มีเนื่องจาก insert ข้อมูลให้ auto กรณีที่เป็นข้อมูลใหม่
                    }
                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }
                #endregion emp in travel 
            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            data.id = id;
            data.user_admin = user_admin;
            data.action_change_imgname = "";//img_header,img_personal_profile,img_banner_1,img_banner_2,img_banner_3,img_practice_areas,title,get_in_touch

            return data;
        }
        public ManageRoleOutModel SearchManageRole(ManageRoleModel value)
        {
            var msg = "";
            var page_name = "managerole";
            var token_login = value.token_login;
            var id = "1";
            Boolean user_admin = false;

            var data = new ManageRoleOutModel();
            data.token_login = token_login;

            try
            {
                #region emp in travel  
                try
                {

                    DataTable dtdata = refdata_Role();
                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(dtdata.Rows[i]["emp_id"]?.ToString()))
                            {
                                data.admin_list.Add(new roleList
                                {
                                    id = dtdata.Rows[i]["id"].ToString(),
                                    emp_id = dtdata.Rows[i]["emp_id"].ToString(),

                                    username = dtdata.Rows[i]["username"].ToString(),
                                    displayname = dtdata.Rows[i]["displayname"].ToString(),
                                    email = dtdata.Rows[i]["email"].ToString(),
                                    idicator = dtdata.Rows[i]["idicator"].ToString(),

                                    super_admin = dtdata.Rows[i]["super_admin"].ToString(),
                                    pmsv_admin = dtdata.Rows[i]["pmsv_admin"].ToString(),
                                    pmdv_admin = dtdata.Rows[i]["pmdv_admin"].ToString(),
                                    contact_admin = dtdata.Rows[i]["contact_admin"].ToString(),

                                    sort_by = dtdata.Rows[i]["sort_by"].ToString(),

                                    action_type = "update",
                                    action_change = "false"
                                });
                            }
                        }
                    }
                    else
                    {
                        //กรณีนี้ไม่มีเนื่องจาก insert ข้อมูลให้ auto กรณีที่เป็นข้อมูลใหม่
                    }
                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }
                #endregion emp in travel 
            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            data.id = id;
            data.user_admin = user_admin;

            return data;
        }
        public ResendEmailOutModel SearchResendEmailv1(ResendEmailModel value)
        {
            var msg = "";
            var page_name = "resendemail";
            var token_login = value.token_login;
            var doc_id = value.doc_id ?? "";
            var id = "1";
            Boolean user_admin = false;

            DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);

            var data = new ResendEmailOutModel();
            data.token_login = token_login;
            data.id = id;
            data.doc_id = doc_id;
            data.user_admin = user_admin;

            try
            {
                try
                {
                    dtdata = new DataTable();
                    dtdata = refdata_resendemailv1(doc_id);
                    if (dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            data.emailList.Add(new listResendEmailModel
                            {
                                id = dtdata.Rows[i]["id"].ToString(),
                                doc_id = dtdata.Rows[i]["doc_id"].ToString(),
                                stepflow = dtdata.Rows[i]["stepflow"].ToString(),
                                fromemail = dtdata.Rows[i]["fromemail"].ToString(),
                                torecipients = dtdata.Rows[i]["torecipients"].ToString(),
                                ccrecipients = dtdata.Rows[i]["ccrecipients"].ToString(),
                                bccrecipients = dtdata.Rows[i]["bccrecipients"].ToString(),
                                subject = dtdata.Rows[i]["subject"].ToString(),
                                body = dtdata.Rows[i]["body"].ToString(),
                                attachments = dtdata.Rows[i]["attachments"].ToString(),
                                statussend = dtdata.Rows[i]["statussend"].ToString(),
                                errorsend = dtdata.Rows[i]["errorsend"].ToString(),
                                datesendtext = dtdata.Rows[i]["datesendtext"].ToString(),
                                activetype = dtdata.Rows[i]["activetype"].ToString(),

                                action_type = "update",
                                action_change = "false"
                            });
                        }
                    }
                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            return data;
        }
        public ResendEmailOutModel SearchResendEmail(ResendEmailModel value)
        {
            var data = new ResendEmailOutModel
            {
                token_login = value.token_login,
                id = "1",
                doc_id = value.doc_id,
                user_admin = false
            };

            try
            {
                var dtdata = refdata_resendemail(value.doc_id, value.page_index, value.page_size, out int totalCount);
                data.totalCount = totalCount;

                if (dtdata.Rows.Count > 0)
                {
                    foreach (DataRow row in dtdata.Rows)
                    {
                        data.emailList.Add(new listResendEmailModel
                        {
                            id = row["id"].ToString(),
                            doc_id = row["doc_id"].ToString(),
                            stepflow = row["stepflow"].ToString(),
                            fromemail = row["fromemail"].ToString(),
                            torecipients = row["torecipients"].ToString(),
                            ccrecipients = row["ccrecipients"].ToString(),
                            bccrecipients = row["bccrecipients"].ToString(),
                            subject = row["subject"].ToString(),
                            body = row["body"].ToString(),
                            attachments = row["attachments"].ToString(),
                            statussend = row["statussend"].ToString(),
                            errorsend = row["errorsend"].ToString(),
                            datesendtext = row["datesendtext"].ToString(),
                            activetype = row["activetype"].ToString(),
                            action_type = "update",
                            action_change = "false"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                data.token_login = ex.Message;
            }

            return data;
        }

        public TravelRecordFilterOutModel SearchTravelRecordFilter(TravelRecordFilterModel value)
        {
            var msg = "";
            var page_name = "travelrecord";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = "personal";
            Boolean user_admin = false;

            var data = new TravelRecordFilterOutModel();
            data.token_login = token_login;

            DataTable dtdata = refdata_emp_detail(token_login, doc_id, "", ref user_admin);
            data.user_admin = user_admin;
            try
            {
                try
                {
                    #region traveltype 
                    //local,oversea,training
                    List<MStatusModel> m_traveltype = new List<MStatusModel>();
                    m_traveltype.Add(new MStatusModel
                    {
                        id = "ob",
                        name = "Oversea business",
                        sort_by = "1",
                    });
                    m_traveltype.Add(new MStatusModel
                    {
                        id = "lb",
                        name = "Local business",
                        sort_by = "2",
                    });
                    m_traveltype.Add(new MStatusModel
                    {
                        id = "ot",
                        name = "Oversea training",
                        sort_by = "3",
                    });
                    m_traveltype.Add(new MStatusModel
                    {
                        id = "lt",
                        name = "Local training",
                        sort_by = "4",
                    });

                    data.m_traveltype = m_traveltype;
                    #endregion traveltype

                    DataTable dtm_continent = ref_m_continent();
                    DataTable dtm_country = ref_m_country();
                    DataTable dtm_province = ref_m_province();
                    DataTable dtm_sections = ref_m_section();

                    if (dtm_continent.Rows.Count > 0)
                    {
                        dt = new DataTable();
                        dt = dtm_continent.Copy(); dt.AcceptChanges();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data.m_continent.Add(new MasterNormalModel
                            {
                                id = dt.Rows[i]["id"].ToString(),
                                name = dt.Rows[i]["name"].ToString(),
                                sort_by = dt.Rows[i]["sort_by"].ToString(),
                            });
                        }
                    }
                    if (dtm_country.Rows.Count > 0)
                    {
                        dt = new DataTable();
                        dt = dtm_country.Copy(); dt.AcceptChanges();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data.m_country.Add(new MasterNormalModel
                            {
                                id = dt.Rows[i]["id"].ToString(),
                                id_main = dt.Rows[i]["id_main"].ToString(),
                                name = dt.Rows[i]["name"].ToString(),
                                sort_by = dt.Rows[i]["sort_by"].ToString(),
                            });
                        }
                    }
                    if (dtm_province.Rows.Count > 0)
                    {
                        dt = new DataTable();
                        dt = dtm_province.Copy(); dt.AcceptChanges();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data.m_province.Add(new MasterNormalModel
                            {
                                id = dt.Rows[i]["id"].ToString(),
                                id_main = dt.Rows[i]["id_main"].ToString(),
                                name = dt.Rows[i]["name"].ToString(),
                                sort_by = dt.Rows[i]["sort_by"].ToString(),
                            });
                        }
                    }
                    if (dtm_sections.Rows.Count > 0)
                    {
                        dt = new DataTable();
                        dt = dtm_sections.Copy(); dt.AcceptChanges();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data.m_section.Add(new MasterSectionModel
                            {
                                section = dt.Rows[i]["section"].ToString(),
                                department = dt.Rows[i]["department"].ToString(),
                                function = dt.Rows[i]["function"].ToString(),
                                sort_by = dt.Rows[i]["sort_by"].ToString(),
                            });
                        }
                    }

                    List<emplistModel> emp_list = new List<emplistModel>();
                    ref_emp_list(ref emp_list, "");
                    data.emp_list = emp_list;

                }
                catch (Exception ex) { data.token_login = ex.Message.ToString(); }

            }
            catch (Exception ex) { data.token_login = ex.Message.ToString() + msg; }

            return data;
        }
        public TravelRecordOutModel SearchTravelRecord(TravelRecordModel value)
        {
            var msg = "";
            var page_name = "travelrecord";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;

            Boolean user_admin = false;

            var data = new TravelRecordOutModel();
            data.token_login = token_login;

            var msg_error = "";
            var ret = "";
            try
            {

                string doc_id = value.doc_id ?? "";
                string country = value.country ?? "";
                string date_from = value.date_from ?? "";
                string date_to = value.date_to ?? "";

                string travel_type = value.travel_type ?? "";
                string emp_id = value.emp_id ?? "";

                string section = value.section ?? "";
                string department = value.department ?? "";
                string function = value.function ?? "";

                List<traveltypeList> travel_list = value.travel_list;

                DataTable dtdata = refdata_emp_detail(token_login, "personal", "", ref user_admin);
                data.user_admin = user_admin;

                DataTable dtresults = refsearch_travelrecord(token_login, doc_id, country, date_from, date_to, travel_type, emp_id, section, department, function, travel_list);

                List<travelrecordList> travelrecord = new List<travelrecordList>();
                if (dtresults.Rows.Count > 0)
                {
                    for (int i = 0; i < dtresults.Rows.Count; i++)
                    {
                        data.travelrecord.Add(new travelrecordList
                        {
                            no = dtresults.Rows[i]["no"].ToString(),
                            emp_id = dtresults.Rows[i]["emp_id"].ToString(),
                            emp_title = dtresults.Rows[i]["emp_title"].ToString(),
                            emp_name = dtresults.Rows[i]["emp_name"].ToString(),
                            category = dtresults.Rows[i]["category"].ToString(),
                            section = dtresults.Rows[i]["section"].ToString(),
                            department = dtresults.Rows[i]["department"].ToString(),
                            function = dtresults.Rows[i]["function"].ToString(),

                            travel_status = dtresults.Rows[i]["travel_status"].ToString(),
                            in_house = dtresults.Rows[i]["in_house"].ToString(),
                            travel_topic = dtresults.Rows[i]["travel_topic"].ToString(),
                            country = dtresults.Rows[i]["country"].ToString(),
                            city_province = dtresults.Rows[i]["city_province"].ToString(),
                            date_from = dtresults.Rows[i]["date_from"].ToString(),
                            date_to = dtresults.Rows[i]["date_to"].ToString(),
                            duration = dtresults.Rows[i]["duration"].ToString(),
                            estimate_expense = dtresults.Rows[i]["estimate_expense"].ToString(),
                            gl_account = dtresults.Rows[i]["gl_account"].ToString(),
                            cost_center = dtresults.Rows[i]["cost_center"].ToString(),
                            order_wbs = dtresults.Rows[i]["order_wbs"].ToString(),

                            accommodation = dtresults.Rows[i]["accommodation"].ToString(),
                            air_ticket = dtresults.Rows[i]["air_ticket"].ToString(),
                            allowance_day = dtresults.Rows[i]["allowance_day"].ToString(),
                            allowance_night = dtresults.Rows[i]["allowance_night"].ToString(),
                            clothing_luggage = dtresults.Rows[i]["clothing_luggage"].ToString(),
                            course_fee = dtresults.Rows[i]["course_fee"].ToString(),
                            instruction_fee = dtresults.Rows[i]["instruction_fee"].ToString(),
                            miscellaneous = dtresults.Rows[i]["miscellaneous"].ToString(),
                            passport = dtresults.Rows[i]["passport"].ToString(),
                            transportation = dtresults.Rows[i]["transportation"].ToString(),
                            visa_fee = dtresults.Rows[i]["visa_fee"].ToString(),
                            total = dtresults.Rows[i]["total"].ToString(),

                            travel_insurance = dtresults.Rows[i]["travel_insurance"].ToString(),
                            doc_id = dtresults.Rows[i]["doc_id"].ToString(),
                            countryid = dtresults.Rows[i]["countryid"].ToString(),
                            travel_type = dtresults.Rows[i]["travel_type"].ToString(),
                        });
                    }
                }
                ret = "true";
            }
            catch (Exception ex) { ret = "false"; msg_error = ex.Message.ToString() + msg; }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Search data succesed." : "Search data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        private void ClearItemInReportWithStatus(string doc_id, ref Boolean bClearLineApproved, ref Boolean bClearCAPApproved, ref string document_status_now)
        {


            //DevFix 20250129 0000 เพิ่ม Clear Status, Line, CAP Status
            bClearLineApproved = false;
            bClearCAPApproved = false;
            document_status_now = "";
            sqlstr = $@" select distinct h.dh_code, h.dh_doc_status, a.doc_status, ts2.ts_name as document_status_now,  a.action_status, a.tab_no
                             ,case when substr(h.dh_doc_status,0,1) = substr(a.doc_status,0,1) then 1 else 0 end check_mat
                             ,case when substr(h.dh_doc_status,0,1) = 2 then '1' else (case when substr(h.dh_doc_status,0,1) = 3 then '2'  else 
                             case when substr(h.dh_doc_status,0,1) = 4 then '3' else (case when substr(h.dh_doc_status,0,1) = 5 then '4'  else '' end) end
                             end) end tab_no
                             from bz_doc_head h inner join  bz_doc_action a on   h.dh_code =  a.dh_code
                             inner join bz_master_status ts on h.dh_doc_status = ts.ts_id
                             inner join bz_master_status ts2 on a.doc_status = ts2.ts_id
                             where a.action_status = 1  and  h.dh_code ='{doc_id}'
                             order by a.doc_status asc ";

            DataTable dtStatuNow = new DataTable();
            SetDocService.conn_ExecuteData(ref dtStatuNow, sqlstr);
            if (dtStatuNow?.Rows.Count > 0)
            {
                document_status_now = dtStatuNow?.Rows[0]["document_status_now"]?.ToString() ?? "";
                if (dtStatuNow?.Rows[0]["check_mat"]?.ToString() == "0")
                {
                    string tab_no = dtStatuNow?.Rows[0]["tab_no"]?.ToString() ?? "";
                    if (tab_no == "1" || tab_no == "2")
                    {
                        bClearLineApproved = true;
                        bClearCAPApproved = true;
                    }
                    else if (tab_no == "3")
                    {
                        bClearCAPApproved = true;
                    }
                }
            }
        }
        public ApprovalFormOutModel SearchApprovalForm(ApprovalFormModel value)
        {
            var msg = "";
            var page_name = "approvalform";
            var imglist = new List<ImgList>();
            var token_login = value.token_login;
            var doc_id = value.doc_id;

            Boolean user_admin = false;

            var data = new ApprovalFormOutModel();
            data.token_login = token_login;
            data.id = "1";
            data.data_type = "search";
            data.user_admin = user_admin;

            var msg_error = "";
            var ret = "";
            try
            {
                //refsearch_approval_from,refsearch_approval_from_traveler_summary,refsearch_approval_from_estimate_expense,refsearch_approval_from_approvaldetails
                DataTable dtresults = refsearch_approval_from(token_login, doc_id);
                DataTable dtpart1 = refsearch_approval_from_traveler_summary(token_login, doc_id);
                DataTable dtpart2 = refsearch_approval_from_estimate_expense(token_login, doc_id);
                DataTable dtpart3 = refsearch_approval_from_approvaldetails(token_login, doc_id);

                Boolean bClearLineApproved = false; Boolean bClearCAPApproved = false; string document_status_now = "";
                ClearItemInReportWithStatus(doc_id, ref bClearLineApproved, ref bClearCAPApproved, ref document_status_now);

                refsearch_approval_from_traveler_location(token_login, doc_id, ref dtresults);

                if (dtresults.Rows.Count > 0)
                {
                    data.requested_by = dtresults.Rows[0]["requested_by"].ToString();
                    data.org_unit_req = dtresults.Rows[0]["org_unit_req"].ToString();
                    data.on_behalf_of = dtresults.Rows[0]["on_behalf_of"].ToString();
                    data.org_unit_on_behalf = dtresults.Rows[0]["org_unit_on_behalf"].ToString();
                    data.date_to_requested = dtresults.Rows[0]["date_to_requested"].ToString();
                    data.document_number = dtresults.Rows[0]["document_number"].ToString();
                    data.document_status = (document_status_now == "" ? dtresults.Rows[0]["document_status"].ToString() : document_status_now);
                    data.company = dtresults.Rows[0]["company"].ToString();
                    data.travel_type = dtresults.Rows[0]["travel_type"].ToString();
                    data.travel_with = dtresults.Rows[0]["travel_with"].ToString();

                    if (dtpart1.Rows.Count > 0)
                    {
                        data.travel_details.Add(new traveldetailsList
                        {
                            no = dtresults.Rows[0]["no"].ToString(),
                            travel_topic = dtresults.Rows[0]["travel_topic"].ToString(),
                            continent = dtresults.Rows[0]["continent"].ToString(),
                            country = dtresults.Rows[0]["country"].ToString(),
                            city = dtresults.Rows[0]["city"].ToString(),
                            province = dtresults.Rows[0]["province"].ToString(),
                            location = dtresults.Rows[0]["location"].ToString(),
                            business_date = dtresults.Rows[0]["business_date"].ToString(),
                            travel_date = dtresults.Rows[0]["travel_date"].ToString(),
                            travel_duration = dtresults.Rows[0]["travel_duration"].ToString(),
                            traveling_objective = dtresults.Rows[0]["traveling_objective"].ToString(),

                            to_submit = dtresults.Rows[0]["to_submit"].ToString(),
                            to_share = dtresults.Rows[0]["to_share"].ToString(),
                            to_share_remark = dtresults.Rows[0]["to_share_remark"].ToString(),
                            other = dtresults.Rows[0]["other"].ToString(),
                            other_remark = dtresults.Rows[0]["other_remark"].ToString(),
                        });

                        for (int i = 0; i < dtpart1.Rows.Count; i++)
                        {
                            data.traveler_summary.Add(new travelersummaryList
                            {
                                no = dtpart1.Rows[i]["no"].ToString(),
                                emp_id = dtpart1.Rows[i]["emp_id"].ToString(),
                                emp_name = dtpart1.Rows[i]["emp_name"].ToString(),
                                org_unit = dtpart1.Rows[i]["org_unit"].ToString(),
                                country_city = dtpart1.Rows[i]["country_city"].ToString(),
                                province = dtpart1.Rows[i]["province"].ToString(),
                                location = dtpart1.Rows[i]["location"].ToString(),
                                business_date = dtpart1.Rows[i]["business_date"].ToString(),
                                travel_date = dtpart1.Rows[i]["travel_date"].ToString(),
                                budget_account = dtpart1.Rows[i]["budget_account"].ToString(),
                            });
                        }
                    }

                    if (dtpart2.Rows.Count > 0)
                    {
                        decimal dExRate = 0;
                        decimal dGrandTotal = 0;

                        try
                        {
                            dExRate = Convert.ToDecimal(dtresults.Rows[0]["exchange_rates_as_of"]);
                            dExRate = Math.Truncate(dExRate * 100) / 100;
                        }
                        catch { }

                        for (int i = 0; i < dtpart2.Rows.Count; i++)
                        {
                            data.estimate_expense_details.Add(new estimateexpensedetailsList
                            {
                                no = dtpart2.Rows[i]["no"].ToString(),
                                emp_id = dtpart2.Rows[i]["emp_id"].ToString(),
                                emp_name = dtpart2.Rows[i]["emp_name"].ToString(),
                                org_unit = dtpart2.Rows[i]["org_unit"].ToString(),
                                country_city = dtpart2.Rows[i]["country_city"].ToString(),
                                province = dtpart2.Rows[i]["province"].ToString(),
                                location = dtpart2.Rows[i]["location"].ToString(),
                                business_date = dtpart2.Rows[i]["business_date"].ToString(),
                                travel_date = dtpart2.Rows[i]["travel_date"].ToString(),
                                budget_account = dtpart2.Rows[i]["budget_account"].ToString(),

                                air_ticket = dtpart2.Rows[i]["air_ticket"].ToString(),
                                accommodation = dtpart2.Rows[i]["accommodation"].ToString(),
                                allowance = dtpart2.Rows[i]["allowance"].ToString(),
                                transportation = dtpart2.Rows[i]["transportation"].ToString(),
                                passport = dtpart2.Rows[i]["passport"].ToString(),
                                passport_valid = dtpart2.Rows[i]["passport_valid"].ToString(),
                                visa_fee = dtpart2.Rows[i]["visa_fee"].ToString(),
                                others = dtpart2.Rows[i]["others"].ToString(),
                                luggage_clothing = dtpart2.Rows[i]["luggage_clothing"].ToString(),
                                luggage_clothing_valid = dtpart2.Rows[i]["luggage_clothing_valid"].ToString(),
                                insurance = dtpart2.Rows[i]["insurance"].ToString(),
                                total_expenses = dtpart2.Rows[i]["total_expenses"].ToString(),

                                remark = dtpart2.Rows[i]["remark"].ToString(),
                            });

                            try
                            {
                                dGrandTotal += Convert.ToDecimal((dtpart2.Rows[i]["total_expenses"].ToString()));
                            }
                            catch { }
                        }
                        data.estimate_expense.Add(new estimateexpenseList
                        {
                            exchange_rates_as_of = dExRate.ToString("#,##0.00"),
                            grand_total_expenses = dGrandTotal.ToString("#,##0.00"),
                        });
                    }

                    if (dtpart3.Rows.Count > 0)
                    {
                        data.approval_by.Add(new approvalbyList
                        {
                            the_budget = dtresults.Rows[0]["the_budget"].ToString(),
                            shall_seek = dtresults.Rows[0]["shall_seek"].ToString(),
                            remark = dtresults.Rows[0]["remark"].ToString(),
                        });
                        for (int i = 0; i < dtpart3.Rows.Count; i++)
                        {
                            data.approval_details.Add(new approvaldetailsList
                            {
                                no = dtpart3.Rows[i]["no"].ToString(),
                                emp_id = dtpart3.Rows[i]["emp_id"].ToString(),
                                emp_name = dtpart3.Rows[i]["emp_name"].ToString(),
                                org_unit = dtpart3.Rows[i]["org_unit"].ToString(),
                                line_approval = dtpart3.Rows[i]["line_approval"].ToString(),
                                org_unit_line = dtpart3.Rows[i]["org_unit_line"].ToString(),
                                approved_date_line = dtpart3.Rows[i]["approved_date_line"].ToString(),
                                cap_approval = dtpart3.Rows[i]["cap_approval"].ToString(),
                                org_unit_cap = dtpart3.Rows[i]["org_unit_cap"].ToString(),
                                approved_date_cap = dtpart3.Rows[i]["approved_date_cap"].ToString(),
                            });
                        }
                    }

                }

                #region check data and new row default
                if (data.travel_details.Count == 0) { data.travel_details.Add(new traveldetailsList { no = null, }); }
                if (data.traveler_summary.Count == 0) { data.traveler_summary.Add(new travelersummaryList { no = null, }); }
                if (data.estimate_expense.Count == 0) { data.estimate_expense.Add(new estimateexpenseList { }); }
                if (data.estimate_expense_details.Count == 0) { data.estimate_expense_details.Add(new estimateexpensedetailsList { no = null, }); }
                if (data.approval_by.Count == 0) { data.approval_by.Add(new approvalbyList { }); }
                if (data.approval_details.Count == 0) { data.approval_details.Add(new approvaldetailsList { no = null, }); }

                #endregion check data and new row default

                ret = "true";
            }
            catch (Exception ex) { ret = "false"; msg_error = ex.Message.ToString() + msg; }

            data.after_trip.opt1 = (ret.ToLower() ?? "") == "true" ? "true" : "false";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = (ret.ToLower() ?? "") == "true" ? "Search data succesed." : "Search data failed.";
            data.after_trip.opt2.remark = (ret.ToLower() ?? "") == "true" ? "" : msg_error;
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "Error msg";
            data.after_trip.opt3.remark = msg_error;

            return data;
        }
        #endregion page


        #region function
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

            }
            catch (Exception ex)
            {

            }
            return date;
        }

        private string retCheckValue(string value)
        {
            string ret = "N";
            try
            {
                if (value == "true")
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

        private decimal toDecimal(string value)
        {
            decimal ret = 0;
            try
            {
                ret = string.IsNullOrEmpty(value) ? ret : Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        public class logModel
        {
            public string module { get; set; }
            public string tevent { get; set; }
            public string data_log { get; set; }
            public int ref_id { get; set; }
            public string ref_code { get; set; }
            public string user_log { get; set; }
            public string user_token { get; set; }
        }
        public static int insertLog(logModel value)
        {
            int iResult = -1;

            //using (TOPEBizEntities context = new TOPEBizEntities())
            //{
            //    using (context.Database.Connection)
            //    {
            //        context.Database.Connection.Open();
            //        DbCommand cmd = context.Database.Connection.CreateCommand();
            //        cmd.CommandText = "bz_sp_insert_log";
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add(new OracleParameter("p_module", value.module));
            //        cmd.Parameters.Add(new OracleParameter("p_event", value.tevent));
            //        cmd.Parameters.Add(new OracleParameter("p_data_log", value.data_log));
            //        cmd.Parameters.Add(new OracleParameter("p_ref_id", value.ref_id));
            //        cmd.Parameters.Add(new OracleParameter("p_ref_code", value.ref_code));
            //        cmd.Parameters.Add(new OracleParameter("p_user_log", value.user_log));
            //        cmd.Parameters.Add(new OracleParameter("p_user_token", value.user_token));
            //        iResult = cmd.ExecuteNonQuery();

            //    }
            //}

            return iResult;
        }


        #endregion function


    }

}