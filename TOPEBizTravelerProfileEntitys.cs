using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client; 
using top.ebiz.service.Models.Traveler_Profile;

namespace top.ebiz.service
{
    public class TOPEBizTravelerProfileEntitys : DbContext
    {
        #region function main
        // สร้าง constructor ที่ไม่มีพารามิเตอร์และดึง connection string จาก appsettings.json
        public TOPEBizTravelerProfileEntitys()
            : base(GetOptions(GetConnectionStringFromAppSettings()))
        {
        }

        // Constructor ที่รับ connection string เป็นพารามิเตอร์
        public TOPEBizTravelerProfileEntitys(string connectionString)
            : base(GetOptions(connectionString))
        {
        }

        // Constructor ที่รับ DbContextOptions สำหรับการเชื่อมต่อฐานข้อมูล (ใช้กับ DI)
        public TOPEBizTravelerProfileEntitys(DbContextOptions<TOPEBizTravelerProfileEntitys> options)
            : base(options)
        {
        }
         


        // ใช้ IConfiguration เพื่อดึง connection string จาก appsettings.json
        private static string GetConnectionStringFromAppSettings()
        {
            // สร้าง IConfiguration เพื่ออ่านไฟล์ appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // ดึง connection string จาก appsettings.json
            return config.GetConnectionString("eBizConnection") ?? "";
        }

        // กำหนด options สำหรับการสร้าง context ด้วย connection string สำหรับ Oracle
        private static DbContextOptions GetOptions(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TOPEBizTravelerProfileEntitys>();
            optionsBuilder.UseOracle(connectionString);
            return optionsBuilder.Options;
        }
        // เพิ่มฟังก์ชัน ConvertTypeParameter ในคลาสนี้
        public OracleParameter ConvertTypeParameter(string paramName, object value, string type = "char", int defLength = 4000)
        {
            OracleParameter param = new OracleParameter();
            param.ParameterName = paramName;

            switch (type.ToLower())
            {
                case "char":
                    param.OracleDbType = OracleDbType.Char;
                    param.Size = value != null ? value.ToString().Length : defLength; // กำหนดขนาด
                    param.Value = value ?? DBNull.Value; // ถ้าไม่มีค่าให้เป็น DBNull
                    break;

                case "int":
                    param.OracleDbType = OracleDbType.Int32;
                    param.Value = value != null && int.TryParse(value.ToString(), out int intValue) ? intValue : DBNull.Value;
                    break;

                case "number":
                    param.OracleDbType = OracleDbType.Decimal;
                    param.Value = value != null && decimal.TryParse(value.ToString(), out decimal decimalValue) ? decimalValue : DBNull.Value;
                    break;

                case "date":
                    param.OracleDbType = OracleDbType.Date;
                    param.Value = value != null && DateTime.TryParse(value.ToString(), out DateTime dateValue) ? dateValue : DBNull.Value;
                    break;

                default:
                    param.OracleDbType = OracleDbType.Varchar2; // Default เป็น string
                    param.Value = value ?? DBNull.Value;
                    break;
            }

            return param;
        }

        #endregion function main

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //DbSet => Insert Update Delete 
            // โมเดลที่ต้องระบุว่าไม่มี Primary Key 
            modelBuilder.Entity<BZ_USERS>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_TRAVEL_TYPE>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_CONTIENT>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_COUNTRY>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_PROVINCE>().HasNoKey();
            modelBuilder.Entity<BZ_MASTER_COUNTRY>().HasNoKey();
            modelBuilder.Entity<BZ_MASTER_CONTINENT>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_RUNNING>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_HEAD>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_TRAVELER_APPROVER>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_TRAVELER_EXPENSE>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_TRAVELER>().HasNoKey();

            modelBuilder.Entity<BZ_DOC_ACTION>().HasNoKey();
            modelBuilder.Entity<VW_BZ_USERS>().HasNoKey();

            // โมเดลที่เป็น View หรือ Stored Procedure (ไม่มี Primary Key)
            modelBuilder.Entity<AccommodationModel>().HasNoKey();
            modelBuilder.Entity<accommodationList>().HasNoKey();
            modelBuilder.Entity<accommodationbookList>().HasNoKey();
            modelBuilder.Entity<AccommodationOutModel>().HasNoKey();
            modelBuilder.Entity<AirTicketModel>().HasNoKey();
            modelBuilder.Entity<airticketList>().HasNoKey();
            modelBuilder.Entity<airticketbookList>().HasNoKey();
            modelBuilder.Entity<AllowanceModel>().HasNoKey();
            modelBuilder.Entity<allowancedetailList>().HasNoKey();
            modelBuilder.Entity<allowanceList>().HasNoKey();
            modelBuilder.Entity<allowancemailList>().HasNoKey();
            modelBuilder.Entity<ApprovalFormModel>().HasNoKey();

            modelBuilder.Entity<approvalbyList>().HasNoKey();
            
            modelBuilder.Entity<approvaldetailsList>().HasNoKey();
            modelBuilder.Entity<TravelerHistoryModel>().HasNoKey();
            modelBuilder.Entity<CarServiceModel>().HasNoKey();
            modelBuilder.Entity<CarServiceOutModel>().HasNoKey();
            modelBuilder.Entity<EmployeeListModel>().HasNoKey();
            modelBuilder.Entity<EmployeeListOutModel>().HasNoKey();
            modelBuilder.Entity<emplistModel>().HasNoKey();
            modelBuilder.Entity<EmpRoleListModel>().HasNoKey();
            modelBuilder.Entity<EmpRoleListOutModel>().HasNoKey();
            modelBuilder.Entity<emprolelistModel>().HasNoKey();
            modelBuilder.Entity<EstimateExpenseModel>().HasNoKey();
            modelBuilder.Entity<EstExpInputModel>().HasNoKey();
            modelBuilder.Entity<EstExpProfileOutModel>().HasNoKey();
            modelBuilder.Entity<EstExpTravelDateModel>().HasNoKey();
            modelBuilder.Entity<EstExpSAPModel>().HasNoKey();
            modelBuilder.Entity<FeedbackModel>().HasNoKey();
            modelBuilder.Entity<FeedbackOutModel>().HasNoKey();
            modelBuilder.Entity<feedbackList>().HasNoKey();
            modelBuilder.Entity<ISOSModel>().HasNoKey();
            modelBuilder.Entity<ISOSOutModel>().HasNoKey();
            modelBuilder.Entity<isosList>().HasNoKey();
            modelBuilder.Entity<KHCodeModel>().HasNoKey();
            modelBuilder.Entity<KHCodeOutModel>().HasNoKey();
            modelBuilder.Entity<khcodeList>().HasNoKey();
            modelBuilder.Entity<TemplateKHCodeOutModel>().HasNoKey();
            modelBuilder.Entity<loginProfileTravelModel>().HasNoKey();
            modelBuilder.Entity<loginAutoModel>().HasNoKey();
            modelBuilder.Entity<loginAutoResultModel>().HasNoKey();
            modelBuilder.Entity<loginResultModel>().HasNoKey();
            modelBuilder.Entity<loginWebResultModel>().HasNoKey();
            modelBuilder.Entity<loginClientModel>().HasNoKey();
            modelBuilder.Entity<logoutModel>().HasNoKey();
            modelBuilder.Entity<loginUserResultModel>().HasNoKey();
            modelBuilder.Entity<Users>().HasNoKey();
            modelBuilder.Entity<loginProfileModel>().HasNoKey();
            modelBuilder.Entity<loginProfileResultModel>().HasNoKey();
            modelBuilder.Entity<logModel>().HasNoKey();
            modelBuilder.Entity<ManageRoleModel>().HasNoKey();
            modelBuilder.Entity<ManageRoleOutModel>().HasNoKey();
            modelBuilder.Entity<ResendEmailModel>().HasNoKey();
            modelBuilder.Entity<ResendEmailOutModel>().HasNoKey();
            modelBuilder.Entity<roleList>().HasNoKey();
            modelBuilder.Entity<userNewList>().HasNoKey();
            modelBuilder.Entity<CurrencyList>().HasNoKey();
            modelBuilder.Entity<ExchangeRateList>().HasNoKey();
            modelBuilder.Entity<MStatusModel>().HasNoKey();
            modelBuilder.Entity<MFeedbackTypeModel>().HasNoKey();
            modelBuilder.Entity<MFeedbackListModel>().HasNoKey();
            modelBuilder.Entity<MMasterNomalModel>().HasNoKey();
            modelBuilder.Entity<MMaintainDataModel>().HasNoKey();
            modelBuilder.Entity<MasterNormalModel>().HasNoKey();
            modelBuilder.Entity<MasterCountryModel>().HasNoKey();
            modelBuilder.Entity<MasterAirportModel>().HasNoKey();
            modelBuilder.Entity<MasterSectionModel>().HasNoKey();
            modelBuilder.Entity<MMenuModel>().HasNoKey();
            modelBuilder.Entity<MMenuListModel>().HasNoKey();
            modelBuilder.Entity<MasterAllowance_ListModel>().HasNoKey();
            modelBuilder.Entity<MasterVISADocument_ListModel>().HasNoKey();
            modelBuilder.Entity<MasterVISADocountries_ListModel>().HasNoKey();
            modelBuilder.Entity<MMasterInsurancebrokerModel>().HasNoKey();
            modelBuilder.Entity<PassportModel>().HasNoKey();
            modelBuilder.Entity<PassportOutModel>().HasNoKey();
            modelBuilder.Entity<passportList>().HasNoKey();
            modelBuilder.Entity<PortalModel>().HasNoKey();
            modelBuilder.Entity<OpenDocOutModel>().HasNoKey();
            modelBuilder.Entity<PortalOutModel>().HasNoKey();
            modelBuilder.Entity<upcomingplanList>().HasNoKey();
            modelBuilder.Entity<imgportalList>().HasNoKey();
            modelBuilder.Entity<practice_areasList>().HasNoKey();
            modelBuilder.Entity<ReimbursementModel>().HasNoKey();
            modelBuilder.Entity<ReimbursementOutModel>().HasNoKey();
            modelBuilder.Entity<reimbursementList>().HasNoKey();
            modelBuilder.Entity<reimbursementdetailList>().HasNoKey();
            modelBuilder.Entity<SAPModel>().HasNoKey();
            modelBuilder.Entity<SAPPasportModel>().HasNoKey();
            modelBuilder.Entity<actionModel>().HasNoKey();
            modelBuilder.Entity<SendEmailModel>().HasNoKey();
            modelBuilder.Entity<EmailModel>().HasNoKey();
            modelBuilder.Entity<emailList>().HasNoKey();
            modelBuilder.Entity<mailselectList>().HasNoKey();
            modelBuilder.Entity<TrackingModel>().HasNoKey();
            modelBuilder.Entity<TransportationOutModel>().HasNoKey();
            modelBuilder.Entity<transportationCarList>().HasNoKey();
            modelBuilder.Entity<transportationList>().HasNoKey();
            modelBuilder.Entity<TravelerHistoryModel>().HasNoKey();
            modelBuilder.Entity<TravelerOutModel>().HasNoKey();
            modelBuilder.Entity<travelerEmpList>().HasNoKey();
            modelBuilder.Entity<travelerVisaList>().HasNoKey();
            modelBuilder.Entity<travelerHistoryList>().HasNoKey();
            modelBuilder.Entity<TravelerHistoryOutModel>().HasNoKey();

            modelBuilder.Entity<EmpListOutModel>().HasNoKey();
            modelBuilder.Entity<EmpIdCheckModel>().HasNoKey();

            modelBuilder.Entity<TravelExpenseOutModel>().HasNoKey();
            modelBuilder.Entity<travelexpenseList>().HasNoKey();
            modelBuilder.Entity<travelexpensedetailList>().HasNoKey();
            modelBuilder.Entity<TravelInsuranceModel>().HasNoKey();
            modelBuilder.Entity<TravelInsuranceOutModel>().HasNoKey();
            modelBuilder.Entity<TravelRecordFilterModel>().HasNoKey();
            modelBuilder.Entity<TravelRecordFilterOutModel>().HasNoKey();
            modelBuilder.Entity<TravelRecordModel>().HasNoKey();
            modelBuilder.Entity<TravelRecordOutModel>().HasNoKey();
            modelBuilder.Entity<travelrecordList>().HasNoKey();
            modelBuilder.Entity<traveltypeList>().HasNoKey();
            modelBuilder.Entity<UploadFileModel>().HasNoKey();
            modelBuilder.Entity<VisaModel>().HasNoKey();
            modelBuilder.Entity<VisaOutModel>().HasNoKey();
            modelBuilder.Entity<visaList>().HasNoKey(); 
            modelBuilder.Entity<DocHeadModel>().HasNoKey();
             
        }

        #region DbSet
        public DbSet<NormalModel> NormalModelList { get; set; }

        // รายการ DbSet สำหรับแต่ละ entity (ตารางในฐานข้อมูล ???ต้องเป็น field ทั้งหมดนะ เดียวเช็คอีกทีนะ)
        public DbSet<BZ_USERS> BZ_USERS { get; set; }
        public DbSet<BZ_DOC_TRAVEL_TYPE> BZ_DOC_TRAVEL_TYPE { get; set; }
        public DbSet<BZ_DOC_CONTIENT> BZ_DOC_CONTIENT { get; set; }
        public DbSet<BZ_DOC_COUNTRY> BZ_DOC_COUNTRY { get; set; }
        public DbSet<BZ_DOC_PROVINCE> BZ_DOC_PROVINCE { get; set; }
        public DbSet<BZ_MASTER_COUNTRY> BZ_MASTER_COUNTRY { get; set; }
        public DbSet<BZ_MASTER_CONTINENT> BZ_MASTER_CONTINENT { get; set; }
        public DbSet<BZ_DOC_RUNNING> BZ_DOC_RUNNING { get; set; }
        public DbSet<BZ_DOC_HEAD> BZ_DOC_HEAD { get; set; }
        public DbSet<BZ_DOC_TRAVELER_APPROVER> BZ_DOC_TRAVELER_APPROVER { get; set; }
        public DbSet<BZ_DOC_TRAVELER_EXPENSE> BZ_DOC_TRAVELER_EXPENSE { get; set; }
        public DbSet<BZ_DOC_TRAVELER> BZ_DOC_TRAVELER { get; set; }

        public DbSet<BZ_DOC_ACTION> BZ_DOC_ACTION { get; set; }
        public DbSet<VW_BZ_USERS> VW_BZ_USERS { get; set; }

        // Models with no primary key (e.g., Views or Stored Procedures)
        public DbSet<AccommodationModel> AccommodationModels { get; set; }
        public DbSet<accommodationList> AccommodationLists { get; set; }
        public DbSet<accommodationbookList> AccommodationbookLists { get; set; }
        public DbSet<AccommodationOutModel> AccommodationOutModels { get; set; }
        public DbSet<AirTicketModel> AirTicketModels { get; set; }
        public DbSet<airticketList> AirticketLists { get; set; }
        public DbSet<airticketbookList> AirticketbookLists { get; set; }
        public DbSet<AllowanceModel> AllowanceModels { get; set; }
        public DbSet<allowancedetailList> AllowancedetailLists { get; set; }
        public DbSet<allowanceList> AllowanceLists { get; set; }
        public DbSet<allowancemailList> AllowancemailLists { get; set; }
        public DbSet<ApprovalFormModel> ApprovalFormModels { get; set; }

        public DbSet<approvalbyList> ApprovalbyLists { get; set; }
        public DbSet<approvaldetailsList> ApprovaldetailsLists { get; set; }
        public DbSet<TravelerHistoryModel> TravelerHistoryModels { get; set; }
        public DbSet<CarServiceModel> CarServiceModels { get; set; }
        public DbSet<CarServiceOutModel> CarServiceOutModels { get; set; }
        public DbSet<EmployeeListModel> EmployeeListModels { get; set; }
        public DbSet<EmployeeListOutModel> EmployeeListOutModels { get; set; }
        public DbSet<emplistModel> EmplistModels { get; set; }
        public DbSet<EmpRoleListModel> EmpRoleListModels { get; set; }
        public DbSet<EmpRoleListOutModel> EmpRoleListOutModels { get; set; }
        public DbSet<emprolelistModel> EmprolelistModels { get; set; }
        public DbSet<EstimateExpenseModel> EstimateExpenseModels { get; set; }
        public DbSet<EstExpInputModel> EstExpInputModels { get; set; }
        public DbSet<EstExpProfileOutModel> EstExpOutModels { get; set; }
        public DbSet<EstExpTravelDateModel> EstExpTravelDateModels { get; set; }
        public DbSet<EstExpSAPModel> EstExpSAPModels { get; set; }
        public DbSet<FeedbackModel> FeedbackModels { get; set; }
        public DbSet<FeedbackOutModel> FeedbackOutModels { get; set; }
        public DbSet<feedbackList> FeedbackLists { get; set; }
        public DbSet<ISOSModel> ISOSModels { get; set; }
        public DbSet<ISOSOutModel> ISOSOutModels { get; set; }
        public DbSet<isosList> IsosLists { get; set; }
        public DbSet<KHCodeModel> KHCodeModels { get; set; }
        public DbSet<KHCodeOutModel> KHCodeOutModels { get; set; }
        public DbSet<khcodeList> KhcodeLists { get; set; }
        public DbSet<TemplateKHCodeOutModel> TemplateKHCodeOutModels { get; set; }
        public DbSet<loginProfileTravelModel> LoginModels { get; set; }
        public DbSet<loginAutoModel> LoginAutoModels { get; set; }
        public DbSet<loginAutoResultModel> LoginAutoResultModels { get; set; }
        public DbSet<loginResultModel> LoginResultModels { get; set; }
        public DbSet<loginWebResultModel> LoginWebResultModels { get; set; }
        public DbSet<loginClientModel> LoginClientModels { get; set; }
        public DbSet<logoutModel> LogoutModels { get; set; }
        public DbSet<loginUserResultModel> LoginUserResultModels { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<loginProfileModel> LoginProfileModels { get; set; }
        public DbSet<loginProfileResultModel> LoginProfileResultModels { get; set; }
        public DbSet<logModel> LogModels { get; set; }
        public DbSet<ManageRoleModel> ManageRoleModels { get; set; }
        public DbSet<ManageRoleOutModel> ManageRoleOutModels { get; set; }

        public DbSet<ResendEmailModel> ResendEmailModels { get; set; }
        public DbSet<ResendEmailOutModel> ResendEmailOutModels { get; set; }

        public DbSet<roleList> RoleLists { get; set; }
        public DbSet<userNewList> UserNewLists { get; set; }
        public DbSet<CurrencyList> CurrencyLists { get; set; }
        public DbSet<ExchangeRateList> ExchangeRateLists { get; set; }
        public DbSet<MStatusModel> MStatusModels { get; set; }
        public DbSet<MFeedbackTypeModel> MFeedbackTypeModels { get; set; }
        public DbSet<MFeedbackListModel> MFeedbackListModels { get; set; }
        public DbSet<MMasterNomalModel> MMasterNomalModels { get; set; }
        public DbSet<MMaintainDataModel> MMaintainDataModels { get; set; }
        public DbSet<MasterNormalModel> MasterNormalModels { get; set; }
        public DbSet<MasterCountryModel> MasterCountryModels { get; set; }
        public DbSet<MasterAirportModel> MasterAirportModels { get; set; }
        public DbSet<MasterSectionModel> MasterSectionModels { get; set; }
        public DbSet<MMenuModel> MMenuModels { get; set; }
        public DbSet<MMenuListModel> MMenuListModels { get; set; }
        public DbSet<MasterAllowance_ListModel> MasterAllowance_ListModels { get; set; }
        public DbSet<MasterVISADocument_ListModel> MasterVISADocument_ListModels { get; set; }
        public DbSet<MasterVISADocountries_ListModel> MasterVISADocountries_ListModels { get; set; }
        public DbSet<MMasterInsurancebrokerModel> MMasterInsurancebrokerModels { get; set; }
        public DbSet<PassportModel> PassportModels { get; set; }
        public DbSet<PassportOutModel> PassportOutModels { get; set; }
        public DbSet<passportList> PassportLists { get; set; }
        public DbSet<PortalModel> PortalModels { get; set; }
        public DbSet<OpenDocOutModel> OpenDocOutModels { get; set; }
        public DbSet<PortalOutModel> PortalOutModels { get; set; }
        public DbSet<upcomingplanList> UpcomingplanLists { get; set; }
        public DbSet<imgportalList> ImgportalLists { get; set; }
        public DbSet<practice_areasList> PracticeAreasLists { get; set; }
        public DbSet<ReimbursementModel> ReimbursementModels { get; set; }
        public DbSet<ReimbursementOutModel> ReimbursementOutModels { get; set; }
        public DbSet<reimbursementList> ReimbursementLists { get; set; }
        public DbSet<reimbursementdetailList> ReimbursementdetailLists { get; set; }
        public DbSet<SAPModel> SAPModels { get; set; }
        public DbSet<SAPPasportModel> SAPPasportModels { get; set; }
        public DbSet<actionModel> ActionModels { get; set; }
        public DbSet<SendEmailModel> SendEmailModels { get; set; }
        public DbSet<EmailModel> EmailModels { get; set; }
        public DbSet<emailList> EmailLists { get; set; }
        public DbSet<mailselectList> MailselectLists { get; set; }
        public DbSet<TrackingModel> TrackingModels { get; set; }
        public DbSet<TransportationOutModel> TransportationOutModels { get; set; }
        public DbSet<transportationCarList> TransportationCarLists { get; set; }
        public DbSet<transportationList> TransportationLists { get; set; }
        public DbSet<TravelerOutModel> TravelerOutModels { get; set; }
        public DbSet<travelerEmpList> TravelerEmpLists { get; set; }
        public DbSet<travelerVisaList> TravelerVisaLists { get; set; }
        public DbSet<travelerHistoryList> TravelerHistoryLists { get; set; }
        public DbSet<TravelerHistoryOutModel> TravelerHistoryOutModels { get; set; }
        public DbSet<EmpListOutModel> EmpListOutModels { get; set; }
        public DbSet<EmpIdCheckModel> EmpIdCheckModels { get; set; }
        public DbSet<TravelExpenseOutModel> TravelExpenseOutModels { get; set; }
        public DbSet<travelexpenseList> TravelexpenseLists { get; set; }
        public DbSet<travelexpensedetailList> TravelexpensedetailLists { get; set; }
        public DbSet<TravelInsuranceModel> TravelInsuranceModels { get; set; }
        public DbSet<TravelInsuranceOutModel> TravelInsuranceOutModels { get; set; }
        public DbSet<TravelRecordFilterModel> TravelRecordFilterModels { get; set; }
        public DbSet<TravelRecordFilterOutModel> TravelRecordFilterOutModels { get; set; }
        public DbSet<TravelRecordModel> TravelRecordModels { get; set; }
        public DbSet<TravelRecordOutModel> TravelRecordOutModels { get; set; }
        public DbSet<travelrecordList> TravelrecordLists { get; set; }
        public DbSet<traveltypeList> TraveltypeLists { get; set; }
        public DbSet<UploadFileModel> UploadFileModels { get; set; }
        public DbSet<VisaModel> VisaModels { get; set; }
        public DbSet<VisaOutModel> VisaOutModels { get; set; }
        public DbSet<visaList> VisaLists { get; set; }

        public DbSet<DocHeadModel> DocHeadModels { get; set; }


        #endregion DbSet 
    }
}
