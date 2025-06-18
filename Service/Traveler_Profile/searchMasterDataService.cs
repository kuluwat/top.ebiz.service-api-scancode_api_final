
using System.Data;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.service.Models.Traveler_Profile;

namespace top.ebiz.service.Service.Traveler_Profile
{
    public class searchMasterDataService
    {
        //private void ref_master_visa_doc(ref MMaintainDataModel data)
        //{
        //    sw = new SetDocService();
        //    string sqlstr = @"select a.id,a.name,a.description,a.sort_by
        //                    from bz_master_visa_document a
        //                    where a.status = 1 
        //                    order by a.sort_by";

        //    dt = new DataTable();
        //    SetDocService.conn_ExecuteData(ref dt, sqlstr);
        //    if (dt.Rows.Count == 0)
        //    {
        //        data.master_visa_doc.Add(new MMasterNomalModel
        //        {
        //        });

        //    }
        //    else if (dt.Rows.Count > 0)
        //    {
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            data.master_visa_doc.Add(new MMasterNomalModel
        //            {
        //                id = dt.Rows[i]["id"].ToString(),
        //                name = dt.Rows[i]["name"].ToString(),
        //                description = dt.Rows[i]["description"].ToString(),
        //                sort_by = dt.Rows[i]["sort_by"].ToString(),
        //            });

        //        }
        //    }

        //}
        private void ref_master_continent(ref MMaintainDataModel data)
        {
            sw = new SetDocService();
            //string sqlstr = @"select ctn_id as id, ctn_name as name, null as description, null as sort_by
            //                    from bz_master_continent a
            //                    order by ctn_code ";
            StringBuilder sqlBuilder = new StringBuilder();
            var whereClause = new List<string>();

            List<OracleParameter> parameters = new List<OracleParameter>();
            var dt = new DataTable();

            sqlBuilder.AppendLine(@" select ctn_id as id, ctn_name as name, null as description, null as sort_by
                                from bz_master_continent a
                                order by ctn_code  ");
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
            //dt = new DataTable();
            //SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt?.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.master_zone.Add(new MMasterNomalModel
                    {
                        id = dt.Rows[i]["id"].ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        description = dt.Rows[i]["description"].ToString(),
                        sort_by = (i + 1).ToString(),
                    });

                }
            }

        }
        private void ref_master_country(ref MMaintainDataModel data)
        {
            sw = new SetDocService();
            //string sqlstr = @"select a.ct_id as id, a.ct_name as name, a.ct_thname as description, null as sort_by 
            //                ,b.ctn_id ,b.ctn_id as main_id
            //                from bz_master_country a
            //                left join BZ_MASTER_CONTINENT b on a.ctn_id = b.ctn_id
            //                order by ct_code";
            StringBuilder sqlBuilder = new StringBuilder();
            var whereClause = new List<string>();

            List<OracleParameter> parameters = new List<OracleParameter>();
            var dt = new DataTable();

            sqlBuilder.AppendLine(@" select a.ct_id as id, a.ct_name as name, a.ct_thname as description, null as sort_by 
                            ,b.ctn_id ,b.ctn_id as main_id
                            from bz_master_country a
                            left join BZ_MASTER_CONTINENT b on a.ctn_id = b.ctn_id
                            order by ct_code");



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
            //dt = new DataTable();
            //SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt?.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.master_country.Add(new MMasterNomalModel
                    {
                        id = dt.Rows[i]["id"].ToString(),
                        main_id = dt.Rows[i]["main_id"].ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        description = dt.Rows[i]["description"].ToString(),
                        sort_by = (i + 1).ToString(),
                    });

                }
            }

        }
        private void ref_master_province(ref MMaintainDataModel data)
        {
            sw = new SetDocService();
            string sqlstr = @"select ct_id as main_id,pv_id as id,pv_code as name,pv_name as description
                             from bz_master_province
                             order by pv_code";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.master_province.Add(new MMasterNomalModel
                    {
                        id = dt.Rows[i]["id"].ToString(),
                        main_id = dt.Rows[i]["main_id"].ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        description = dt.Rows[i]["description"].ToString(),
                        sort_by = (i + 1).ToString(),
                    });

                }
            }

        }
        private void ref_master_airport(ref MMaintainDataModel data)
        {
            sw = new SetDocService();
            string sqlstr = @" select ap_id as id,ap_name as name ,iata_code as airport_code 
                               ,ci_name as county_name, ct_name as city_name 
                               ,iata_code || ' - ' || ap_name || ' - ' || ci_name || ' ' || ct_name as display_name
                             from  bz_master_airport
                             where ct_id is not null 
                             order by ct_id";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.master_airport.Add(new MasterAirportModel
                    {
                        id = dt.Rows[i]["id"].ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        airport_code = dt.Rows[i]["airport_code"].ToString(),
                        display_name = dt.Rows[i]["display_name"].ToString(),
                        city_name = dt.Rows[i]["city_name"].ToString(),
                        county_name = dt.Rows[i]["county_name"].ToString(),
                        sort_by = (i + 1).ToString(),
                    });

                }
            }

        }
        private void ref_master_currency(ref MMaintainDataModel data)
        {
            sw = new SetDocService();
            string sqlstr = @"  select id, name, sort_by
                        from VW_BZ_CURRENCY
                        order by sort_by ";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.master_currency.Add(new MMasterNomalModel
                    {
                        id = dt.Rows[i]["id"].ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        sort_by = (i + 1).ToString(),
                    });

                }
            }

        }
        SetDocService sw;
        ClassConnectionDb conn;// cls_connection conn;
        string sqlstr = "";
        string sqlstr_all = "";
        string ret = "";
        DataTable dt;
        DataTable dtdata;
        public MMaintainDataModel SearchAirticketType(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_ALREADY_BOOKED_TYPE); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            from BZ_MASTER_ALREADY_BOOKED_TYPE a order by sort_by ";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count == 0)
            {
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    data.airticket_type.Add(new MasterNormalModel
                    {
                        page_name = page_name,
                        module_name = module_name,

                        id = id.ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }

        public MMaintainDataModel SearchAlreadyBooked(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_ALREADY_BOOKED_TYPE); ;

            //string sqlstr = @" select a.*
            //                , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
            //                from BZ_MASTER_ALREADY_BOOKED_TYPE a order by sort_by ";
            StringBuilder sqlBuilder = new StringBuilder();
            var whereClause = new List<string>();

            List<OracleParameter> parameters = new List<OracleParameter>();
            var dt = new DataTable();

            sqlBuilder.AppendLine(@" select a.*
                          , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                           from BZ_MASTER_ALREADY_BOOKED_TYPE a order by sort_by ");

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
            //dt = new DataTable();
            //SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt?.Rows.Count == 0)
            {
            }
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
                    data.already_booked.Add(new MasterNormalModel
                    {
                        page_name = page_name,
                        module_name = module_name,

                        id = id.ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchListStatus(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_LIST_STATUS); ;
            var p = new List<OracleParameter>();
            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            from bz_master_list_status a where 1=1 ";
            if (page_name != "")
            {
                // sqlstr += " and lower(a.page_name) = lower('" + page_name + "') ";
                sqlstr += " and lower(a.page_name) = lower(:page_name) ";
                p.Add(new OracleParameter("page_name", page_name));
            }
            sqlstr += " order by sort_by";

            dt = new DataTable();
            var cmd = new OracleCommand(sqlstr);
            cmd.Parameters.AddRange(p.ToArray());
            SetDocService.conn_ExecuteData(ref dt, cmd);

            if (dt.Rows.Count == 0)
            {
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    data.list_status.Add(new MasterNormalModel
                    {
                        page_name = page_name,
                        module_name = module_name,

                        id = id.ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }


        private void ref_allowance_type(ref MMaintainDataModel data)
        {
            var page_name = data.page_name.ToString();
            var module_name = data.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_ALLOWANCE_TYPE); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            from bz_master_allowance_type a where 1=1 ";

            sqlstr += " order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count == 0)
            {
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    data.allowance_type.Add(new MasterNormalModel
                    {
                        page_name = page_name,
                        module_name = module_name,

                        id = id.ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }

        }
        public MMaintainDataModel SearchAllowanceType(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            ref_allowance_type(ref data);

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchFeedbackType(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_FEEDBACK_TYPE); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change   
                            , case when (select count(1) from bz_master_feedback_list b where a.id = b.feedback_type_id) = 0 
                            then 'false' else 'true' end sub_data 
                            from bz_master_feedback_type a  
                            where 1=1 ";

            sqlstr += " order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count == 0)
            {
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    data.feedback_type.Add(new MasterNormalModel
                    {
                        page_name = page_name,
                        module_name = module_name,

                        id = id.ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),

                        sub_data = dt.Rows[i]["sub_data"].ToString().ToLower(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchFeedbackList(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_FEEDBACK_LIST); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            , case when (select count(1) from bz_master_feedback_question b where a.id = b.feedback_list_id and a.feedback_type_id = b.feedback_type_id) = 0
                            then 'false' else 'true' end sub_data 
                            from bz_master_feedback_list a where 1=1 ";

            sqlstr += " order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count == 0)
            {
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    data.feedback_list.Add(new MasterNormalModel
                    {
                        page_name = page_name,
                        module_name = module_name,

                        id = id.ToString(),

                        name = dt.Rows[i]["name"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        id_main = dt.Rows[i]["feedback_type_id"].ToString(),
                        question_other = dt.Rows[i]["question_other"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                        sub_data = dt.Rows[i]["sub_data"].ToString().ToLower(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchFeedbackQuestion(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_FEEDBACK_QUESTION); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            from bz_master_feedback_question a where 1=1 ";

            sqlstr += " order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count == 0)
            {
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    data.feedback_question.Add(new MasterNormalModel
                    {
                        page_name = page_name,
                        module_name = module_name,

                        id = id.ToString(),

                        name = dt.Rows[i]["question"].ToString(),
                        description = dt.Rows[i]["description"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        id_main = dt.Rows[i]["feedback_type_id"].ToString(),
                        id_sub = dt.Rows[i]["feedback_list_id"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }

        public MMaintainDataModel SearchConfigDailyAllowance(MMaintainDataModel value)
        {
            //ยังไม่ได้แก้ไขข้อมูล copy มาจาก SearchFeedbackQuestion
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            //bz_config_daily_allowance
            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_CONFIG_DAILY_ALLOWANCE); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            from bz_config_daily_allowance a where 1=1 ";

            sqlstr += " order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }

                    data.allowance_list.Add(new MasterAllowance_ListModel
                    {
                        token_login = token_login.ToString(),
                        data_type = "",
                        user_admin = false,
                        id = id.ToString(),

                        travel_category = dt.Rows[i]["travel_category"].ToString(),
                        kh_code = dt.Rows[i]["kh_code"].ToString(),
                        workplace = dt.Rows[i]["workplace"].ToString(),
                        workplace_type_country = dt.Rows[i]["workplace_type_country"].ToString(),
                        overnight_type = dt.Rows[i]["overnight_type"].ToString(),//case local ค้างคืน = 1 ,ไม่ค้างคืน = 2
                        allowance_rate = dt.Rows[i]["allowance_rate"].ToString(),
                        currency = dt.Rows[i]["currency"].ToString(),

                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }
            else
            {
                data.allowance_list.Add(new MasterAllowance_ListModel
                {
                    token_login = token_login.ToString(),
                    data_type = "",
                    user_admin = false,
                    id = imaxid.ToString(),
                    status = "1",
                    sort_by = "1",

                    action_type = "insert",
                    action_change = "false",
                });
            }

            ref_allowance_type(ref data);
            ref_master_country(ref data);
            ref_master_continent(ref data);
            ref_master_currency(ref data);

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }

        public MMenuModel SearchMenu(MMenuModel value)
        {
            var token_login = value.token_login;
            var data = value;


            string sqlstr = @" select  a.*
                               from  bz_menu_list a";
            sqlstr += " order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count == 0)
            {
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data.menuList.Add(new MMenuListModel
                    {
                        token_login = token_login,
                        pagename = dt.Rows[i]["pagename"].ToString(),
                        url = dt.Rows[i]["url"].ToString(),
                        display = (dt.Rows[i]["display"].ToString() ?? "") == "true" ? true : false,
                    });
                }
            }

            string status_msg = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = "";
            data.after_trip.opt3.remark = "";

            return data;
        }


        public MMaintainDataModel SearchVISADocument(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_VISA_DOCUMENT); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            , (select case when count(1) > 0 then 'true' else 'false' end from  bz_master_visa_docountries b where b.visa_doc_id = a.id) as sub_data
                            from bz_master_visa_document a
                            order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);

            if (dt.Rows.Count == 0)
            {
                data.visa_document.Add(new MasterVISADocument_ListModel
                {
                    id = imaxid.ToString(),
                    name = "",
                    description = "",
                    preparing_by = "",
                    status = "1",
                    sort_by = "1",

                    action_type = "insert",
                    action_change = "false",
                    sub_data = "false",
                });
            }
            else if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    data.visa_document.Add(new MasterVISADocument_ListModel
                    {
                        id = id.ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        description = dt.Rows[i]["description"].ToString(),
                        preparing_by = dt.Rows[i]["preparing_by"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                        sub_data = dt.Rows[i]["sub_data"].ToString(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }

        public MMaintainDataModel SearchVISADocountries(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            //"page_name": "mtvisacountries",
            //"module_name": "master visa docountries",
            searchDocTravelerProfileServices swd = new searchDocTravelerProfileServices();
            List<ImgList> imgList = swd.refdata_img_list_master(token_login, module_name, page_name, "", "");


            ref_master_country(ref data);

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_VISA_DOCOUNTRIES); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            from bz_master_visa_docountries a
                            order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);
            if (dt.Rows.Count == 0)
            {
                var visa_doc_id = "";
                try
                {
                    visa_doc_id = data.visa_document[0].id.ToString();
                }
                catch { }

                data.visa_docountries.Add(new MasterVISADocountries_ListModel
                {
                    id = imaxid.ToString(),
                    continent_id = data.master_country[0].main_id.ToString(),
                    country_id = data.master_country[0].id.ToString(),
                    visa_doc_id = visa_doc_id,
                    name = "",
                    description = "",
                    preparing_by = "",
                    status = "1",
                    sort_by = "1",

                    action_type = "insert",
                    action_change = "false",
                });

            }
            else if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }

                    data.visa_docountries.Add(new MasterVISADocountries_ListModel
                    {
                        id = id.ToString(),
                        continent_id = dt.Rows[i]["continent_id"].ToString(),
                        country_id = dt.Rows[i]["country_id"].ToString(),
                        visa_doc_id = dt.Rows[i]["visa_doc_id"].ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        description = dt.Rows[i]["description"].ToString(),
                        preparing_by = dt.Rows[i]["preparing_by"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }

            data.img_list = imgList;

            data.master_country = null;


            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchZoneCountry(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            ref_master_country(ref data);
            ref_master_continent(ref data);
            ref_master_province(ref data);

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchZoneCountryProvince(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            ref_master_country(ref data);
            ref_master_continent(ref data);
            ref_master_province(ref data);

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchAirport(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            ref_master_airport(ref data);

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
        public MMaintainDataModel SearchInsurancebroker(MMaintainDataModel value)
        {
            var token_login = value.token_login;
            var data = value;
            var doc_type = "search";
            var page_name = value.page_name.ToString();
            var module_name = value.module_name.ToString();

            sw = new SetDocService();
            int imaxid = sw.GetMaxID(TableMaxId.BZ_MASTER_INSURANCE_COMPANY); ;

            string sqlstr = @" select a.*
                            , (case when a.id is null then 'insert' else 'update' end) action_type, 'false' as action_change  
                            from bz_master_insurance_company a
                            order by sort_by";

            dt = new DataTable();
            SetDocService.conn_ExecuteData(ref dt, sqlstr);
            if (dt.Rows.Count == 0)
            {
                var visa_doc_id = "";
                try
                {
                    visa_doc_id = data.visa_document[0].id.ToString();
                }
                catch { }
                data.master_insurancebroker.Add(new MMasterInsurancebrokerModel
                {
                    id = imaxid.ToString(),
                    status = "1",
                    sort_by = "1",

                    action_type = "insert",
                    action_change = "false",
                });
                imaxid++;
            }
            else if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = dt.Rows[i]["id"].ToString();
                    if (dt.Rows[i]["action_type"].ToString() == "insert")
                    {
                        id = imaxid.ToString();
                        imaxid++;
                    }
                    if (dt.Rows[i]["status_isos"].ToString() == "") { dt.Rows[i]["status_isos"] = "false"; }
                    if (dt.Rows[i]["status_insurance"].ToString() == "") { dt.Rows[i]["status_insurance"] = "false"; }
                    data.master_insurancebroker.Add(new MMasterInsurancebrokerModel
                    {
                        id = id.ToString(),
                        name = dt.Rows[i]["name"].ToString(),
                        email = dt.Rows[i]["email"].ToString(),
                        travelcompany_type = dt.Rows[i]["travelcompany_type"].ToString(),

                        status = dt.Rows[i]["status"].ToString(),
                        sort_by = dt.Rows[i]["sort_by"].ToString(),

                        //status_isos เพื่อให้ทราบว่า master broker ตัวไหนเอาไปใช่กับ isos 
                        status_isos = dt.Rows[i]["status_isos"].ToString(),

                        //status_insurance เพื่อให้ทราบว่า master broker ตัวไหนเอาไปใช่กับ insurance 
                        status_insurance = dt.Rows[i]["status_insurance"].ToString(),

                        action_type = dt.Rows[i]["action_type"].ToString(),
                        action_change = dt.Rows[i]["action_change"].ToString(),
                    });

                }
            }

            string status_msg = "";
            data.data_type = "";

            data.after_trip.opt1 = "true";
            data.after_trip.opt2 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt2.status = status_msg;
            data.after_trip.opt2.remark = "";
            data.after_trip.opt3 = new Models.Create_Trip.subAfterTripModel();
            data.after_trip.opt3.status = doc_type;
            data.after_trip.opt3.remark = "";

            return data;
        }
    }
}