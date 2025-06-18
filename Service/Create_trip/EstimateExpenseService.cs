
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.service.Models.Create_Trip;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace top.ebiz.service.Service.Create_Trip
{
    public class EstimateExpenseService
    {

        public EstExpOutModel EstimateExpense(EstExpInputModel value)
        {
            EstExpOutModel dataOutput = new EstExpOutModel();
            //dataOutput.CLDate = "N/A";
            //dataOutput.PassportDate = "N/A";

            try
            {
                decimal iTravelDate = 0;
                using (TOPEBizCreateTripEntities context = new TOPEBizCreateTripEntities())
                {
                    var doc_no = value.doc_no ?? "";
                    var emp_id = value.emp_id ?? "";

                    var sql = "";
                    sql = "SELECT DTE_TRAVEL_TODATE travelDate ";
                    sql += " FROM BZ_DOC_TRAVELER_EXPENSE where DH_CODE= :doc_no and DTE_EMP_ID= :emp_id ";

                    var parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("doc_no", doc_no, "char"));
                    parameters.Add(context.ConvertTypeParameter("emp_id", emp_id, "char"));
                    DateTime? travelDate = context.EstExpTravelDateModelList.FromSqlRaw(sql, parameters.ToArray()).ToList().FirstOrDefault()?.travelDate;


                    if (travelDate != null)
                    {
                        iTravelDate = Convert.ToDecimal(Convert.ToDateTime(travelDate).ToString("yyyyMMdd", new System.Globalization.CultureInfo("en-US")));
                    }

                    if (!string.IsNullOrEmpty(value.test_travel_date))
                        iTravelDate = Convert.ToDecimal(value.test_travel_date);

                    // ถ้าไม่มีวันที่ ไม่เอาไปเทียบ logic เลย
                    if (iTravelDate == 0)
                        return dataOutput;

                    sql = "select SUBTY type, TO_NUMBER(nvl(MNDAT,'0')) to_date, TO_NUMBER(nvl(TERMN,'0')) from_date, TO_DATE(MNDAT, 'YYYYMMDD') to_date_date ";
                    sql += " from BZ_ZESS_PA0019 ";
                    sql += " where SUBTY in ('AE', 'LC') and PERNR= :emp_id ";

                    parameters = new List<OracleParameter>();
                    parameters.Add(context.ConvertTypeParameter("emp_id", emp_id, "char"));
                    var sapList = context.EstExpSAPModelList.FromSqlRaw(sql, parameters.ToArray()).ToList();

                    bool inCase = false;

                    #region "Clothing & Luggage"

                    // case : 1 : ถ้าตรวจสอบข้อมุล แล้วไม่มีข้อมูล :: ช่อง Valid Untill จะ Blank // ช่องขวา จะขึ้นเงิน
                    var lcList = sapList.Where(p => p.type.Equals("LC")).ToList();
                    if (lcList != null)
                    { 
                        //DevFix 20250121 0000 แก้ไขตาม Issue Log No 12.แก้ไข Bug: ระบบไม่ควรข้ึนค่ากระเป๋าเสื้อผ้า เนื่องจากข้อมูลใน SAP ยังไม่ถึงก าหนดให้สามารถ
                        //เบิกค่ากระเป๋าเสื้อผ้าได้ สำหรับ Module Oversea 
                        if (lcList.Count() <= 0)
                        {
                            dataOutput.CLExpense = "11000"; 
                        }
                        else { inCase = true; }
                    }

                    // case :2 : ถ้าตรวจสอบข้อมูล แล้วมีข้อมูล :: จะขึ้น Valid Untill อยู่ในวันที่เดินทาง จะขึ้น // ช่องขวา จะไม่ขึ้นเงิน
                    if (inCase == false)
                    {
                        var lcList2 = sapList.Where(p => p.type.Equals("LC") && iTravelDate >= p.from_date && iTravelDate <= p.to_date).OrderByDescending(o => o.to_date).ToList().FirstOrDefault();
                        if (lcList2 != null)
                        {
                            inCase = true;
                            dataOutput.CLDate = Convert.ToDateTime(lcList2.to_date_date).ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                        }
                    }

                    // case : 3 : ถ้าตรวจสอบข้อมูล แล้วมีข้อมูล :: จะขึ้น Valid Untill หมดก่อนที่เดินทาง จะ ขึ้น // ช่องขวา จะขึ้นเงิน
                    if (inCase == false)
                    {
                        var lcList3 = sapList.Where(p => p.type.Equals("LC")).OrderByDescending(o => o.to_date).ToList().FirstOrDefault();
                        if (lcList3 != null)
                        {
                            inCase = true;
                            dataOutput.CLDate = Convert.ToDateTime(lcList3.to_date_date).ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                            dataOutput.CLExpense = "11000";
                        }
                    }

                    #endregion "Clothing & Luggage"

                    inCase = false;

                    #region "Passport"

                    // case : 1 : ถ้าตรวจสอบข้อมุล แล้วไม่มีข้อมูล :: ช่อง Valid Untill จะ Blank // ช่องขวา จะขึ้นเงิน
                    var aeList = sapList.Where(p => p.type.Equals("AE")).ToList();
                    if (aeList != null && aeList.Count() <= 0)
                    {
                        inCase = true;
                        dataOutput.PassportExpense = "1500";
                    }

                    // case :2 : ถ้าตรวจสอบข้อมูล แล้วมีข้อมูล :: จะขึ้น Valid Untill อยู่ในวันที่เดินทาง จะขึ้น // ช่องขวา จะไม่ขึ้นเงิน
                    if (inCase == false)
                    {
                        var aeList2 = sapList.Where(p => p.type.Equals("AE") && iTravelDate >= p.from_date && iTravelDate <= p.to_date).OrderByDescending(o => o.to_date).ToList().FirstOrDefault();
                        if (aeList2 != null)
                        {
                            // ย้อนหลัง 6 เดือน
                            decimal expireBeforeDate = getExpireDateBefore(aeList2.to_date_date);
                            // ยังไม่หมดอายุ
                            if (expireBeforeDate >= iTravelDate)
                            {
                                inCase = true;
                                //string sdate = expireBeforeDate.ToString();
                                //dataOutput.PassportDate = sdate.Substring(0, 4) + "-" + sdate.Substring(4, 2) + "-" + sdate.Substring(6, 2);
                                dataOutput.PassportDate = Convert.ToDateTime(aeList2.to_date_date).ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                            }
                            else
                            {
                                // หมดอายุ
                                inCase = true;
                                dataOutput.PassportExpense = "1500";
                                dataOutput.PassportDate = Convert.ToDateTime(aeList2.to_date_date).ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                            }
                        }

                    }

                    // case : 3 : ถ้าตรวจสอบข้อมูล แล้วมีข้อมูล :: จะขึ้น Valid Untill หมดก่อนที่เดินทาง จะ ขึ้น // ช่องขวา จะขึ้นเงิน
                    if (inCase == false)
                    {
                        var aeList3 = sapList.Where(p => p.type.Equals("AE")).OrderByDescending(o => o.to_date).ToList().FirstOrDefault();
                        if (aeList3 != null)
                        {
                            inCase = true;
                            dataOutput.PassportDate = Convert.ToDateTime(aeList3.to_date_date).ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                            dataOutput.PassportExpense = "1500";
                        }
                    }

                    #endregion "Passport"
                }
            }
            catch (Exception ex)
            {

            }
            return dataOutput;
        }

        public decimal getExpireDateBefore(DateTime? d, int beforeMonth = -6)
        {
            decimal ret = 0;
            try
            {
                DateTime dchk = (DateTime)d;
                DateTime result = dchk.AddMonths(beforeMonth);
                ret = Convert.ToDecimal(result.ToString("yyyyMMdd", new System.Globalization.CultureInfo("en-US")));
            }
            catch (Exception ex)
            {

            }

            return ret;
        }

    }
}