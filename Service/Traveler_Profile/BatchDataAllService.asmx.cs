
// using System;
// using System.Data;
// using System.Web.Services;
// using System.Configuration;

// using Microsoft.Exchange.WebServices.Data;

// namespace top.ebiz.service.Service.Traveler_Profile 
// {
//     /// <summary>
//     /// Summary description for BatchDataAllService
//     /// </summary>
//     [WebService(Namespace = "http://tempuri.org/")]
//     [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
//     [System.ComponentModel.ToolboxItem(false)]
//     // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
//     // [System.Web.Script.Services.ScriptService]
//     public class BatchDataAllService : System.Web.Services.WebService
//     {
//         [WebMethod]
//         public string HelloWorld()
//         {
//             return "Hello World";
//         }

//         [WebMethod]
//         public string get_batch_data_all(string action_test)
//         {
//             var ret = ""; var sqlstr = "";

//             sqlstr = @" call bz_sp_insert_log(";
//             sqlstr += @" 'CONFIRMATIONLETTER Start'";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" )";
//             ret = SetDocService.execute_data_ex(sqlstr, false);

//             try
//             {
//                 //string sAtt = @"D:\ebiz\EBiz_Webservice\DocumentFile\Document Personal Car.pdf";
//                 //SendEmailService smail = new SendEmailService(); 
//                 //smail.send_mail("zkuluwat@thaioilgroup.com", "", "test mail", "test test", sAtt);
//                 CONFIRMATIONLETTER();
//             }
//             catch (Exception ex) { action_test += ex.Message.ToString(); }

//             sqlstr = @" call bz_sp_insert_log(";
//             sqlstr += @" 'CONFIRMATIONLETTER End:" + action_test + "'";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" ,''";
//             sqlstr += @" )";
//             ret = SetDocService.execute_data_ex(sqlstr, false);


//             return action_test;
//         }
//         public string CONFIRMATIONLETTER()
//         {
//             //CONFIRMATION LETTER	028_OB/LB/OT/LT : Business travel confirmation/information is now available!
//             //ตั้ง batch --> Local 3 day, Overse 5 day  เช่น  busses พฤ ให้ส่งวันจันทร์  
//             string msg = "";
//             string sqlstr = "";

//             //DevFix 20200910 0727 เพิ่มแนบ link Ebiz ด้วย Link ไปหน้า login 
//             //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/authen.aspx?page=/main/request/edit/###/i
//             String LinkLogin = configApp.GetStringFromAppSettings["LinkLogin"].ToString();

//             //DevFix 20211004 0000 เพิ่มแนบ link Ebiz Phase2  
//             //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/authen.aspx?page=/master/###
//             String LinkLoginTravelerhistory = top.ebiz.helper.AppEnvironment.GeteLinkLoginTravelerhistory().ToString();


//             string sDear = "";
//             string sDetail = "";
//             string sTitle = "";
//             string sBusinessDate = "";
//             string sLocation = "";
//             string sTravelerList = "";
//             string sOtherList = "";

//             string email_admin = "";


//             ws_conn.wsConnection conn = new ws_conn.wsConnection();
//             sendEmailService mail = new sendEmailService();
//             sendEmailModel dataMail = new sendEmailModel();

//             DataTable dtresult = new DataTable();
//             searchDocTravelerProfileServices _swd = new searchDocTravelerProfileServices();

//             _swd = new searchDocTravelerProfileServices();
//             dtresult = _swd.refsearch_emprole_list("pmsv_admin");
//             for (int i = 0; i < dtresult.Rows.Count; i++)
//             {
//                 email_admin += dtresult.Rows[i]["email"] + ";";
//             }
//             dtresult = new DataTable();
//             _swd = new searchDocTravelerProfileServices();
//             dtresult = _swd.refsearch_emprole_list("pmdv_admin");
//             for (int i = 0; i < dtresult.Rows.Count; i++)
//             {
//                 email_admin += dtresult.Rows[i]["email"] + ";";
//             }

//             string Tel_Services_Team = "";
//             string Tel_Call_Center = "";
//             string i_petty_cash = "";
//             sqlstr = @" SELECT key_value as tel_services_team from bz_config_data where lower(key_name) = lower('tel_services_team') and status = 1";
//             conn = new ws_conn.wsConnection();
//             dtresult = conn.adapter_data(sqlstr);
//             if (dtresult.Rows.Count == 0) { return "no data."; }
//             try { Tel_Services_Team = dtresult.Rows[0]["tel_services_team"].ToString(); } catch { }

//             sqlstr = @" SELECT key_value as tel_call_center from bz_config_data where lower(key_name) = lower('tel_call_center') and status = 1";
//             dtresult = conn.adapter_data(sqlstr);
//             if (dtresult.Rows.Count == 0) { return "no data."; }
//             try { Tel_Call_Center = dtresult.Rows[0]["tel_call_center"].ToString(); } catch { }

//             sqlstr = @" SELECT key_value as i_petty_cash from bz_config_data where lower(key_name) = lower('URL I-PETTY CASH') and status = 1";
//             dtresult = conn.adapter_data(sqlstr);
//             if (dtresult.Rows.Count == 0) { return "no data."; }
//             try { i_petty_cash = dtresult.Rows[0]["i_petty_cash"].ToString(); } catch { }

//             //หาใบงานที่ต้องแจ้งเตือน  
//             sqlstr = @"  select distinct h.dh_topic, h.dh_type
//                              , to_char(min(to_date(to_char(te.dte_bus_todate, 'DD MON YYYY'), 'DD MON YYYY')), 'DD MON YYYY') as date_min
//                              , to_char(max(to_date(to_char(te.dte_bus_todate, 'DD MON YYYY'), 'DD MON YYYY')), 'DD MON YYYY') as date_max
//                              , min(to_date(to_char(te.dte_bus_todate, 'DD MON YYYY'), 'DD MON YYYY')) as bus_date
//                              FROM bz_doc_traveler_expense te 
//                              inner join BZ_DOC_HEAD h on h.dh_code=te.dh_code  
//                              WHERE h.dh_doc_status = 50 and te.dte_status = 1 
//                              and (nvl(te.dte_appr_opt,'true') = 'true' and nvl(te.dte_cap_appr_opt,'true') = 'true') 
//                              group by h.dh_topic, h.dh_type";

//             sqlstr = @"select t.* from (" + sqlstr + ")t where t.bus_date = (sysdate- (case when t.dh_type like 'local%' then 3 else 5 end)) ";

//             conn = new ws_conn.wsConnection();
//             DataTable dtdoc = conn.adapter_data(sqlstr);
//             if (dtdoc.Rows.Count == 0) { return "no data."; }

//             for (int i = 0; i < dtdoc.Rows.Count; i++)
//             {
//                 string doc_id = dtdoc.Rows[i]["dh_code"].ToString();
//                 string doc_type = dtdoc.Rows[i]["dh_type"].ToString();//oversea,local
//                 string email_traverler = "";
                 

//                 //http://tbkc-dapps-05.thaioil.localnet/Ebiz2/authen.aspx?page=/master/###"/>
//                 //https://e-biztravel.thaioilgroup.com/Ebiz2/authen.aspx?page=/master/###
//                 //string url_air_ticket = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/airticket");
//                 //string url_accommodation = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/accommodation");
//                 //string url_allowance = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/allowance");
//                 //string url_travel_insurance = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/travelinsurance");
//                 //string url_isos = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/isos");
//                 //string url_transportation = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/transportation");
//                 //string url_reimbursement = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/reimbursement");
//                 //string url_feedback = (LinkLoginTravelerhistory).Replace("master/###/travelerhistory", "authen.aspx?page=master/" + doc_id + "/feedback");
//                 //string url_i_petty_cash = i_petty_cash;

//                 string url_air_ticket = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/airticket");
//                 string url_accommodation = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/accommodation");
//                 string url_allowance = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/allowance");
//                 string url_travel_insurance = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/travelinsurance");
//                 string url_isos = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/isos");
//                 string url_transportation = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/transportation");
//                 string url_reimbursement = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/reimbursement");
//                 string url_feedback = (LinkLoginTravelerhistory + "/").Replace("authen.aspx?page=/master/###/", "authen.aspx?page=master/" + doc_id + "/feedback");
//                 string url_i_petty_cash = i_petty_cash;

//                 #region traverler in doc
//                 sqlstr = @" select distinct nvl(b.ENTITLE,'')||' '||b.ENFIRSTNAME||' '||b.ENLASTNAME as emp_name, b.email as email  
//                              , b.employeeid as emp_id, b.orgname  
//                              from bz_doc_traveler_approver a
//                              inner join BZ_DOC_HEAD h on h.dh_code=a.dh_code
//                              inner join vw_bz_users b on a.dta_travel_empid = b.employeeid 
//                              where h.dh_doc_status = 50 
//                              and a.dh_code ='" + doc_id + "' and a.dta_type = 2 and a.dta_action_status in (3) and a.dta_doc_status = 42 ";
//                 conn = new ws_conn.wsConnection();
//                 DataTable dtTravel = conn.adapter_data(sqlstr);
//                 if (dtTravel.Rows.Count == 0) { continue; ; }
//                 for (int j = 0; j < dtTravel.Rows.Count; j++)
//                 {
//                     if (email_traverler != "") { email_traverler += ";"; }
//                     email_traverler += dtTravel.Rows[j]["email"].ToString();
//                 }
//                 #endregion traverler in doc

//                 sqlstr = @"   select distinct h.dh_topic
//                              , to_char(min(to_date(to_char(te.dte_bus_todate, 'DD MON YYYY'), 'DD MON YYYY')), 'DD MON YYYY') as date_min
//                              , to_char(max(to_date(to_char(te.dte_bus_todate, 'DD MON YYYY'), 'DD MON YYYY')), 'DD MON YYYY') as date_max
//                              FROM bz_doc_traveler_expense te 
//                              inner join BZ_DOC_HEAD h on h.dh_code=te.dh_code  
//                              WHERE h.dh_doc_status = 50 and te.dte_status = 1 
//                              and (nvl(te.dte_appr_opt,'true') = 'true' and nvl(te.dte_cap_appr_opt,'true') = 'true') 
//                              and h.dh_code in ('" + doc_id + "') group by h.dh_topic";
//                 conn = new ws_conn.wsConnection();
//                 DataTable dtbusdate = conn.adapter_data(sqlstr);
//                 if (dtbusdate != null && dtbusdate.Rows.Count > 0)
//                 {
//                     sTitle = "Title : " + dtbusdate.Rows[0]["DH_TOPIC"].ToString() ?? "";
//                     sBusinessDate = "Business Date : " + dtbusdate.Rows[0]["date_min"].ToString() + "-" + dtbusdate.Rows[0]["date_max"].ToString();
//                 }

//                 sqlstr = @"  select distinct h.dh_topic
//                              , case when substr(te.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end name1
//                              , te.city_text as name2
//                              FROM bz_doc_traveler_expense te 
//                              inner join BZ_DOC_HEAD h on h.dh_code=te.dh_code
//                              inner join BZ_USERS U on te.DTE_Emp_Id = u.employeeid
//                              left join bz_master_country c on te.ct_id = c.ct_id
//                              left join BZ_MASTER_PROVINCE p on te.PV_ID = p.PV_ID  
//                              WHERE h.dh_doc_status = 50 and te.dte_status = 1 
//                              and (nvl(te.dte_appr_opt,'true') = 'true' and nvl(te.dte_cap_appr_opt,'true') = 'true') 
//                              and h.dh_code in ('" + doc_id + "') " +
//                              " order by case when substr(te.dh_code,0,1) = 'L' then p.pv_name else c.ct_name end , te.city_text";
//                 conn = new ws_conn.wsConnection();
//                 DataTable dtLocation = conn.adapter_data(sqlstr);
//                 if (dtLocation != null && dtLocation.Rows.Count > 0)
//                 {
//                     if (dtLocation.Rows.Count == 1)
//                     {
//                         sLocation = "Location : " + dtLocation.Rows[0]["name1"] + "/" + dtLocation.Rows[0]["name2"];
//                     }
//                     else
//                     {
//                         sLocation = "";
//                         for (int l = 0; l < dtLocation.Rows.Count; l++)
//                         {
//                             if (sLocation == "") { sLocation = "Location : "; } else { sLocation += ","; }
//                             sLocation += dtLocation.Rows[l]["name1"] + "/" + dtLocation.Rows[l]["name2"];
//                         }
//                     }
//                 }

//                 //TO: Traveler
//                 //CC : Admin - PMSV; Admin - PMDV(if any) ;
//                 //Subj: OB / LBYYMMXXXX : Business / Training Travel Confirmation Letter

//                 dataMail.mail_to = email_traverler;
//                 dataMail.mail_cc = email_admin;
//                 dataMail.mail_subject = doc_id + " : Travel arrangements for a business trip has been ready!";

//                 sDear = @"Dear All,";

//                 sDetail = "To get ready to travel, Business travel confirmation/information is now available;";
//                 sDetail += "<br>";
//                 sDetail += "To view the approval details, click ";
//                 sDetail += "<a href='" + (LinkLogin.Replace("/i", "/cap")).Replace("###", doc_id) + "'>" + doc_id + "</a>";
//                 sDetail += "<br>";
//                 sDetail += "To view travel details, click ";
//                 sDetail += "<a href='" + (LinkLoginTravelerhistory).Replace("###", doc_id) + "'>travel details.</a>";

//                 var iNo = 1;
//                 sTravelerList = "<table>";
//                 for (int j = 0; j < dtTravel.Rows.Count; j++)
//                 {
//                     sTravelerList += " <tr>";
//                     sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 20pt;font-size:15pt;'>" + iNo + ") " + dtTravel.Rows[j]["emp_name"].ToString() + "</span></font></td>";//1) [Title_Name of traveler] 
//                     sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + dtTravel.Rows[j]["emp_id"].ToString() + "</span></font></td>";//Emp. ID
//                     sTravelerList += " <td><font face='Browallia New,sans-serif' size='4'><span style='margin:0 0 0 36pt;font-size:15pt;'>" + dtTravel.Rows[j]["orgname"].ToString() + "</span></font></td>";//SEC./DEP./FUNC. 
//                     sTravelerList += " </tr>";
//                     iNo++;
//                 }
//                 sTravelerList += "</table>";

//                 sOtherList = @"<table width='1042' border='1' cellspacing='0' cellpadding='0' style='border-collapse:collapse;width:625.5pt;margin-left:35.7pt;border-style:none;'>";
//                 sOtherList += @"<tbody>";
//                 sOtherList += @"<tr height='48' style='height:28.8pt;'>
//                                         <td width='187' style='width:112.5pt;height:28.8pt;background-color:#1F4E78;padding:0 5.4pt;border-style:solid dotted none solid;border-top-width:1pt;border-right-width:1pt;border-left-width:1pt;border-top-color:windowtext;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                         <span style='background-color:#1F4E78;'>
//                                         <div align='center' style='text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='Browallia New,sans-serif' size='4' color='white'><span style='font-size:15pt;'><b>Items</b></span></font></span></font></div>
//                                         </span></td><td width='855' style='width:513pt;height:28.8pt;background-color:#1F4E79;padding:0 5.4pt;border-style:solid solid none none;border-top-width:1pt;border-right-width:1pt;border-top-color:windowtext;border-right-color:windowtext;'>
//                                         <span style='background-color:#1F4E79;'>
//                                         <div align='center' style='text-indent:14.5pt;text-align:center;margin:0;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'><font face='DB Heavent' size='4' color='white'><span style='font-size:14pt;'><b>Details</b></span></font></span></font></div>
//                                         </span></td></tr>";

//                 if (doc_type == "oversea")
//                 {
//                     //สีขาว
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                 <td width='187' valign='top' style='width:112.5pt;height:14.4pt;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                 <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                 <a href='" + url_air_ticket + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'>AirTicket</span></font></a>" +
//                                   @"</span></font></div></td>
//                                 <td width='855' style='width:513pt;height:14.4pt;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                 <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                 The Itinerary have been confirm, to check information" +
//                                   @" <a href='" + url_air_ticket + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" here and any incorrect, please contact to Business Travel  Services Team urgently.</font></div></td></tr>";

//                     //สีฟ้า
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                 <td width='187' valign='top' style='width:112.5pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                 <span style='background-color:#DDEBF7;'>
//                                 <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                 <a href='" + url_accommodation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='black'>" +
//                                   @"<span style='font-size:15pt;'>Accommodation</span></font></a></span></font></div></span></td>
//                                 <td width='855' valign='top' style='width:513pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                 <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                 The Itinerary have been confirm, to check information" +
//                                   @" <a href='" + url_accommodation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" and any incorrect, please contact to Business Travel  Services Team urgently.</font></div></td></tr>";

//                     //สีขาว
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                 <td width='187' valign='top' style='width:112.5pt;height:14.4pt;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                 <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                 <a href='" + url_allowance + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'>" +
//                                   @"<span style='font-size:15pt;'>Allowance</span></font></a></span></font></div></td>
//                                 <td width='855' style='width:513pt;height:14.4pt;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                 <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                 Individual allowance request to i-Petty cash to check document status" +
//                                   @" <a href='" + url_i_petty_cash + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" </font></div></td></tr>";

//                     //สีฟ้า
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                 <td width='187' valign='top' style='width:112.5pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                 <span style='background-color:#DDEBF7;'>
//                                 <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                 <a href='" + url_travel_insurance + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='black'>" +
//                                   @"<span style='font-size:15pt;'>Travel Insurance</span></font></a></span></font></div></span></td>
//                                     <td width='855' valign='top' style='width:513pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     Please read carefully in Travel Insurance coverage" +
//                                   @" <a href='" + url_travel_insurance + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" and ISOS to medical assistance,
//                                 <br>international healthcare and security assistance" +
//                                   @" <a href='" + url_isos + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" before your travel date.</font></div></td></tr>";

//                     //สีขาว
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                 <td width='187' valign='top' style='width:112.5pt;height:14.4pt;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                 <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                 <a href='" + url_transportation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'>" +
//                                   @"<span style='font-size:15pt;'>Transportation</span></font></a></span></font></div></td>
//                                     <td width='855' style='width:513pt;height:14.4pt;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     To check/request your transportation for travel in Thailand or request a Private Car Requisition" +
//                                   @" <a href='" + url_transportation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here.</i></span></font></a>" +
//                                   @" </font></div></td></tr>";

//                     //สีฟ้า
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                 <td width='187' valign='top' style='width:112.5pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                 <span style='background-color:#DDEBF7;'>
//                                 <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                 <a href='" + url_reimbursement + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='black'>" +
//                                   @"<span style='font-size:15pt;'>Reimbursement</span></font></a></span></font></div></span></td>
//                                     <td width='855' valign='top' style='width:513pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     After approval/traveling you able to request reimbursement (actual payment claims) with receipt i.e. passport,
//                                     <br> taxi by" +
//                                   @" <a href='" + url_reimbursement + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here.</i></span></font></a>" +
//                                   @" to get a formula form attached via E-Business system to i-Petty
//                                     <br> cash or requested to i-Petty cash directly with attaches ; approval form receipts, currency refer (if any).
//                                     </font></div></td></tr>";

//                     //สีขาว
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                 <td width='187' valign='top' style='width:112.5pt;height:14.4pt;padding:0 5.4pt;border-style:none dotted solid solid;border-right-width:1pt;border-bottom-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-bottom-color:windowtext;border-left-color:windowtext;'>
//                                 <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                 <a href='" + url_feedback + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'>" +
//                                   @"<span style='font-size:15pt;'>Feedback</span></font></a></span></font></div></td>
//                                     <td width='855' style='width:513pt;height:14.4pt;padding:0 5.4pt;border-style:none solid solid none;border-right-width:1pt;border-bottom-width:1pt;border-right-color:windowtext;border-bottom-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     Please complete feedback survey after travel as sending by E-Mail or" +
//                                   @" <a href='" + url_feedback + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" for feedback our service.
//                                     </font></div></td></tr>";
//                 }
//                 else
//                 {
//                     //สีขาว
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                     <td width='187' valign='top' style='width:112.5pt;height:14.4pt;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                     <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                     <a href='" + url_air_ticket + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'>AirTicket</span></font></a>" +
//                                   @"</span></font></div></td>
//                                     <td width='855' style='width:513pt;height:14.4pt;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     The Itinerary have been confirm, to check information" +
//                                   @" <a href='" + url_air_ticket + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" here and any incorrect, please contact to Business Travel  Services Team urgently.</font></div></td></tr>";

//                     //สีฟ้า
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                     <td width='187' valign='top' style='width:112.5pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                     <span style='background-color:#DDEBF7;'>
//                                     <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                     <a href='" + url_accommodation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='black'>" +
//                                   @"<span style='font-size:15pt;'>Accommodation</span></font></a></span></font></div></span></td>
//                                     <td width='855' valign='top' style='width:513pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     The Itinerary have been confirm, to check information" +
//                                   @" <a href='" + url_accommodation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" and any incorrect, please contact to Business Travel  Services Team urgently.</font></div></td></tr>";

//                     //สีขาว
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                     <td width='187' valign='top' style='width:112.5pt;height:14.4pt;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                     <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                     <a href='" + url_allowance + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'>" +
//                                   @"<span style='font-size:15pt;'>Allowance</span></font></a></span></font></div></td>
//                                     <td width='855' style='width:513pt;height:14.4pt;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     Individual allowance request to i-Petty cash to check document status" +
//                                   @" <a href='" + url_i_petty_cash + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here</i></span></font></a>" +
//                                   @" </font></div></td></tr>";

//                     //สีฟ้า
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                     <td width='187' valign='top' style='width:112.5pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                     <span style='background-color:#DDEBF7;'>
//                                     <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                     <a href='" + url_transportation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='black'>" +
//                                   @"<span style='font-size:15pt;'>Transportation</span></font></a></span></font></div></span></td>
//                                     <td width='855' valign='top' style='width:513pt;height:14.4pt;background-color:#DDEBF7;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     To check/request your transportation for travel in Thailand or request a Private Car Requisition" +
//                                   @" <a href='" + url_transportation + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here.</i></span></font></a>" +
//                                   @" </font></div></td></tr>";

//                     //สีขาว
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                     <td width='187' valign='top' style='width:112.5pt;height:14.4pt;padding:0 5.4pt;border-style:none dotted none solid;border-right-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-left-color:windowtext;'>
//                                     <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                     <a href='" + url_reimbursement + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'>" +
//                                   @"<span style='font-size:15pt;'>Reimbursement</span></font></a></span></font></div></td>
//                                     <td width='855' style='width:513pt;height:14.4pt;padding:0 5.4pt;border-style:none solid none none;border-right-width:1pt;border-right-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     After approval/traveling you able to request reimbursement (actual payment claims) with receipt i.e." +
//                                   @" <br> taxi by" +
//                                   @" <a href='" + url_reimbursement + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here.</i></span></font></a>" +
//                                   @" to get a formula form attached via Business Travel Services to i-Petty cash or requested to i-Petty" +
//                                   @" <br>cash directly with attaches ; approval form receipts, currency refer (if any).</font></div></td></tr>";

//                     //สีฟ้า
//                     sOtherList += @"<tr height='24' style='height:14.4pt;'>
//                                     <td width='187' valign='top' style='width:112.5pt;height:14.4pt;background-color:#DEEAF6;padding:0 5.4pt;border-style:none dotted solid solid;border-right-width:1pt;border-bottom-width:1pt;border-left-width:1pt;border-right-color:#1F4E78;border-bottom-color:windowtext;border-left-color:windowtext;'>
//                                     <span style='background-color:#DDEBF7;'>
//                                     <div style='margin:0 0 0 8.1pt;'><font face='Calibri,sans-serif' size='2'><span style='font-size:11pt;'>
//                                     <a href='" + url_feedback + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='black'>" +
//                                   @"<span style='font-size:15pt;'>Reimbursement</span></font></a></span></font></div></span></td>
//                                     <td width='855' valign='top' style='width:513pt;height:14.4pt;background-color:#DEEAF6;padding:0 5.4pt;border-style:none solid solid none;border-right-width:1pt;border-bottom-width:1pt;border-right-color:windowtext;border-bottom-color:windowtext;'>
//                                     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4' color='#222A35'><span style='font-size:15pt;'>
//                                     Please complete feedback survey after travel as sending by E-Mail or" +
//                                   @" <a href='" + url_feedback + "' target='_blank'><font face='Browallia New,sans-serif' size='4' color='windowtext'><span style='font-size:15pt;'><i>click here.</i></span></font></a>" +
//                                   @" for feedback our service.</font></div></td></tr>";

//                 }

//                 sOtherList += @" </tbody></table>";


//                 #region set mail
//                 dataMail.mail_body = @"<span lang='en-US'>";
//                 dataMail.mail_body += "<div>";
//                 dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
//                 dataMail.mail_body += "     " + sDear + "</span></font></div>";
//                 dataMail.mail_body += "     <br>";
//                 dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
//                 dataMail.mail_body += "     " + sDetail + "</span></font></div>";
//                 dataMail.mail_body += "     <br>";
//                 dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
//                 dataMail.mail_body += "     " + sTitle + "</span></font></div>";
//                 dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
//                 dataMail.mail_body += "     " + sBusinessDate + "</span></font></div>";
//                 dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
//                 dataMail.mail_body += "     " + sLocation + "</span></font></div>";
//                 dataMail.mail_body += "     <br>";
//                 dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'>";
//                 dataMail.mail_body += "     Traveler List : " + sTravelerList + "</font></div>";
//                 dataMail.mail_body += "     <br>";

//                 dataMail.mail_body += "     <span style='font-size:15pt;'>" + sOtherList + "</span></font></div>";
//                 dataMail.mail_body += "     <br>";

//                 dataMail.mail_body += "     <div style='margin:0 0 0 36pt;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
//                 dataMail.mail_body += "     If you have any question please contact Business Travel Services Team (Tel. " + Tel_Services_Team + ").";
//                 dataMail.mail_body += "     <br>";
//                 dataMail.mail_body += "     For the application assistance, please contact PTT Digital Call Center (Tel. " + Tel_Call_Center + ").";
//                 dataMail.mail_body += "     </span></font></div>";

//                 dataMail.mail_body += "     <div style='margin:0;'><font face='Browallia New,sans-serif' size='4'><span style='font-size:15pt;'>";
//                 dataMail.mail_body += "     <br>";
//                 dataMail.mail_body += "     Best Regards,";
//                 dataMail.mail_body += "     <br>";
//                 dataMail.mail_body += "     Business Travel Services Team (PMSV)";
//                 dataMail.mail_body += "     </span></font></div>";

//                 dataMail.mail_body += "</div>";
//                 dataMail.mail_body += "</span>";

//                 #endregion set mail
//                 mail.sendMail(dataMail);

//             }


//             return msg;
//         }
//         public class sendEmailService
//         {

//             public Microsoft.Exchange.WebServices.Autodiscover.AutodiscoverRedirectionUrlValidationCallback RedirectionUrlValidationCallback { get; set; }
//             public string sendMail(sendEmailModel value)
//             {

//                 String s_mail_to = value.mail_to + "";
//                 String s_mail_cc = value.mail_cc + "";
//                 String s_subject = value.mail_subject + "";
//                 String s_mail_body = value.mail_body + "";
//                 String s_mail_attachments = value.mail_attachments + "";

//                 String msg_mail = "";
//                 Boolean SendAndSaveCopy = false;

//                 string mail_server = ConfigurationManager.AppSettings("MailSMTPServer").ToString();
//                 string mail_from = ConfigurationManager.AppSettings("MailFrom").ToString();
//                 string mail_test = ConfigurationManager.AppSettings("MailTest").ToString();
//                 string mail_font = "";
//                 string mail_fontsize = "";

//                 string mail_user = ConfigurationManager.AppSettings("MailUser").ToString();
//                 string mail_password = ConfigurationManager.AppSettings("MailPassword").ToString();

//                 if (mail_test != "")
//                 {
//                     s_mail_body += "</br>ข้อมูล mail to: " + s_mail_to + "</br></br>ข้อมูล mail cc: " + s_mail_cc;

//                     s_mail_to = mail_test;
//                     s_mail_cc = mail_test;
//                 }

//                 ExchangeService service = new ExchangeService();
//                 service.Credentials = new WebCredentials(mail_user, mail_password);
//                 service.TraceEnabled = true;


//                 EmailMessage email = new EmailMessage(service);
//                 service.AutodiscoverUrl(mail_from, RedirectionUrlValidationCallback);
//                 email.From = new EmailAddress("Mail Display ใส่ไม่มีผล", mail_from);
//                 //email.From.Name = "Car Service TSR";
//                 //email.From.Address = MailFrom;

//                 var email_to = s_mail_to.Split(';');
//                 for (int i = 0; i < email_to.Length; i++)
//                 {
//                     if (email_to[i].ToString() != "")
//                     {
//                         // Mail To จะต้องใช้วิธี Loop และห้ามใส่ ; ต่อท้าย
//                         email.ToRecipients.Add(email_to[i]);
//                     }
//                 }
//                 var email_cc = s_mail_cc.Split(';');
//                 for (int i = 0; i < email_cc.Length; i++)
//                 {
//                     if (email_cc[i].ToString() != "")
//                     {
//                         //Mail CC จะต้องใช้วิธี Loop และห้ามใส่ ; ต่อท้าย
//                         email.CcRecipients.Add(email_cc[i]);
//                     }
//                 }

//                 //Subject
//                 email.Subject = s_subject;

//                 //Body
//                 //เพิ่ม กำหนด font  
//                 if (mail_font == "") { mail_font = "Cordia New"; }
//                 if (mail_fontsize == "") { mail_fontsize = "18"; }
//                 s_mail_body = "<html><div style='font-size:" + mail_fontsize + "px; font-family:" + mail_font + ";'>" + s_mail_body + "</div></html>";
//                 email.Body = new MessageBody(BodyType.HTML, s_mail_body);

//                 //Attachments
//                 //string filePath = Path.Combine(Server.MapPath("~/temp"), "EMPLOYEE LETTER_TES_Mr._Luck_Saraya_170521012548" + ".docx");
//                 string filePath = s_mail_attachments;
//                 if ((s_mail_attachments + "") != "")
//                 {
//                     string[] xsplit_attachments = s_mail_attachments.Split(new char[] { '|', '|' }, StringSplitOptions.RemoveEmptyEntries);
//                     for (int i = 0; i < xsplit_attachments.Length; i++)
//                     {
//                         string templateFile = xsplit_attachments[i];
//                         email.Attachments.AddFileAttachment(templateFile);
//                     }
//                 }

//                 try
//                 {
//                     if (SendAndSaveCopy == true)
//                     {
//                         //จะมีใน send box item
//                         email.SendAndSaveCopy();
//                     }
//                     else
//                     {
//                         //ไม่เก็บใน send box item
//                         email.Send();
//                     }
//                     msg_mail = "";
//                 }
//                 catch (Exception ex)
//                 {
//                     msg_mail = ex.ToString();
//                 }

//                 return msg_mail;
//             }

//         }
//         public class sendEmailModel
//         {
//             public string mail_from { get; set; }
//             public string mail_to { get; set; }
//             public string mail_cc { get; set; }
//             public string mail_subject { get; set; }
//             public string mail_body { get; set; }
//             public string mail_attachments { get; set; }

//         }

//     }
// }
