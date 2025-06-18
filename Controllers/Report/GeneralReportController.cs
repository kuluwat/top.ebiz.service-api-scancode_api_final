
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Report;
using top.ebiz.service.Service.Traveler_Profile;
using static top.ebiz.service.Service.Report.ClassReportModel;

namespace top.ebiz.service.Controllers.Report
{
    public class GeneralReportController : ControllerBase
    {
        [ValidateAntiForgeryToken]
        [HttpPost("Report", Name = "Report")]
        public string Report([FromBody] ReportParamModel value)
        {
            ClassReport cls = new ClassReport();
            var ret = cls.Report(value);
            return ret;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("TravelRecordX", Name = "TravelRecordX")]

        public string TravelRecordX([FromBody] ReportParamxJsonModel value)

        {
            ClassReport cls = new ClassReport();
            var ret = cls.TravelRecordX(value);
            return ret;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("TravelRecord", Name = "TravelRecord")]
        public string TravelRecord([FromBody] ReportParamJsonModel value)
        {
            ClassReport cls = new ClassReport();
            var ret = cls.TravelRecord(value);
            return ret;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("TravelReport", Name = "TravelReport")]
        public string TravelReport([FromBody] ReportParamJsonModel value)
        {
            ClassReport cls = new ClassReport();
            var ret = cls.TravelReport(value);
            return ret;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ApprovalReport", Name = "ApprovalReport")]
        public string ApprovalReport([FromBody] ReportParamJsonModel value)
        {
            ClassReport cls = new ClassReport();
            var ret = cls.ApprovalReport(value);
            return ret;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ReportISOSRecord", Name = "ReportISOSRecord")]
        public string ReportISOSRecord([FromBody] ReportParamJsonModel value)
        {
            ClassReport cls = new ClassReport();
            var ret = cls.ReportISOSRecord(value);
            return ret;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ReportISOSRecordExcel", Name = "ReportISOSRecordExcel")]
        public IActionResult ReportISOSRecordExcel([FromBody] ExportRecordModel value)
        {
            if (value == null) return null;


            ExportReportService service = new ExportReportService();
            HttpResponseMessage response = null;

            logService.logModel mLog = new logService.logModel();
            mLog.module = "Report";
            mLog.tevent = "report isos member list record";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);


            Object result = null;
            result = service.report_isos_member_list_record(value);

            string json = JsonSerializer.Serialize(result);

            return Ok(json);
        }


        [ValidateAntiForgeryToken]
        [HttpPost("ReportInsuranceRecordExcel", Name = "ReportInsuranceRecordExcel")]
        public string ReportInsuranceRecord([FromBody] ReportParamJsonModel value)
        {
            ClassReport cls = new ClassReport();
            var ret = cls.ReportInsuranceRecord(value);
            return ret;
        }


    }
}
