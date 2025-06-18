using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service
{
    public partial class TOPEBizCreateTripEntities : DbContext
    {
        #region function main
        // สร้าง constructor ที่ไม่มีพารามิเตอร์และดึง connection string จาก appsettings.json
        public TOPEBizCreateTripEntities()
            : base(GetOptions(GetConnectionStringFromAppSettings()))
        {
        }

        // Constructor ที่รับ connection string เป็นพารามิเตอร์
        public TOPEBizCreateTripEntities(string connectionString)
            : base(GetOptions(connectionString))
        {
        }

        // Constructor ที่รับ DbContextOptions สำหรับการเชื่อมต่อฐานข้อมูล (ใช้กับ DI)
        public TOPEBizCreateTripEntities(DbContextOptions<TOPEBizCreateTripEntities> options)
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
            var optionsBuilder = new DbContextOptionsBuilder<TOPEBizCreateTripEntities>();
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
                case "timestamp":
                    param.OracleDbType = OracleDbType.TimeStamp;
                    param.Value = value != null && DateTime.TryParse(value.ToString(), out DateTime timestampValue) ? (object)timestampValue : DBNull.Value;
                    break;

                case "clob":
                    param.OracleDbType = OracleDbType.Clob;
                    param.Value = value ?? (object)DBNull.Value;
                    break;

                case "blob":
                    param.OracleDbType = OracleDbType.Blob;
                    if (value is byte[] byteValue)
                        param.Value = byteValue;
                    else
                        param.Value = DBNull.Value;
                    break;

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
            modelBuilder.Entity<BZ_MASTER_STATUS>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_RUNNING>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_HEAD>()
            .ToTable("BZ_DOC_HEAD")
            .HasKey(s => s.DH_CODE);

            modelBuilder.Entity<BZ_DOC_TRAVELER_APPROVER>()
              .ToTable("BZ_DOC_TRAVELER_APPROVER")
            .HasKey(s => new { s.DTA_ID, s.DTA_TRAVEL_EMPID, s.DTA_APPR_EMPID, s.DTA_TYPE, s.DTA_UPDATE_TOKEN });

            modelBuilder.Entity<BZ_DOC_TRAVELER_APPROVER_P3>()
            .HasNoKey();

            modelBuilder.Entity<BZ_DOC_TRAVELER_EXPENSE>()
                .ToTable("BZ_DOC_TRAVELER_EXPENSE")
            .HasKey(s => s.DTE_TOKEN);
            modelBuilder.Entity<BZ_DOC_TRAVELER>().HasNoKey();

            modelBuilder.Entity<BZ_DOC_ACTION>()
            .ToTable("BZ_DOC_ACTION")
            .HasKey(s => s.DA_TOKEN);

            //passport
            modelBuilder.Entity<BZ_DOC_FILE>()
            .ToTable("BZ_DOC_FILE")
            .HasKey(s => new { s.DF_ID, s.DH_CODE });

            modelBuilder.Entity<BZ_DOC_FILE_ATTACHMENT>()
            .ToTable("BZ_DOC_FILE_ATTACHMENT")
            .HasKey(s => new { s.DF_ID, s.DH_CODE });

            modelBuilder.Entity<BZ_EMAIL_DETAILS>().HasNoKey();
            modelBuilder.Entity<VW_BZ_USERS>().HasNoKey();

            // โมเดลที่เป็น View หรือ Stored Procedure (ไม่มี Primary Key)
            modelBuilder.Entity<DocHeadModel>().HasNoKey();
            modelBuilder.Entity<DocDetail3HeadModel>().HasNoKey();
            modelBuilder.Entity<DocList2Model>().HasNoKey();
            modelBuilder.Entity<DocList3Model>().HasNoKey();
            modelBuilder.Entity<docFlow2_travel>().HasNoKey();
            modelBuilder.Entity<doc2ApproverModel>().HasNoKey();
            modelBuilder.Entity<DocFileListInModel>().HasNoKey();
            modelBuilder.Entity<DocFileListOutModel>().HasNoKey();
            modelBuilder.Entity<DocFileListTravelerhistoryOutModel>().HasNoKey();
            modelBuilder.Entity<allApproveModel>().HasNoKey();
            modelBuilder.Entity<SearchCAP_TraverlerModel>().HasNoKey();
            modelBuilder.Entity<SearchUserAllModel>().HasNoKey();
            modelBuilder.Entity<SearchUserRoleTypeModel>().HasNoKey();
            modelBuilder.Entity<SearchUserEmailModel>().HasNoKey();
            modelBuilder.Entity<SearchUserNameModel>().HasNoKey();
            modelBuilder.Entity<SearchUserTypeModel>().HasNoKey();
            modelBuilder.Entity<SearchUserIdModel>().HasNoKey();
            modelBuilder.Entity<SearchUserActionModel>().HasNoKey();
            modelBuilder.Entity<SearchCAPModel>().HasNoKey();
            modelBuilder.Entity<CountryDocModel>().HasNoKey();
            modelBuilder.Entity<ProvinceDocModel>().HasNoKey();
            modelBuilder.Entity<TelephoneModel>().HasNoKey();
            modelBuilder.Entity<costcenter_io>().HasNoKey();
            modelBuilder.Entity<gl_account>().HasNoKey();
            modelBuilder.Entity<tempModel>().HasNoKey();
            modelBuilder.Entity<tempPassportModel>().HasNoKey();
            modelBuilder.Entity<ExchangeRatesModel>().HasNoKey();
            modelBuilder.Entity<ContinentDocModel>().HasNoKey();
            modelBuilder.Entity<employeeDoc2Model>().HasNoKey();
            modelBuilder.Entity<approverModel>().HasNoKey();
            modelBuilder.Entity<MasterCostCenter>().HasNoKey();
            modelBuilder.Entity<TravelerUsers>().HasNoKey();
            modelBuilder.Entity<TravelerUsersV2>().HasNoKey();
            modelBuilder.Entity<travelerDoc2Model>().HasNoKey();
            modelBuilder.Entity<BZ_DOC_TRAVELER_APPROVER_V2>().HasNoKey();
            modelBuilder.Entity<TravelerDocModel>().HasNoKey();
            modelBuilder.Entity<StatusDocModel>().HasNoKey();

            modelBuilder.Entity<BZ_BUDGET_APPROVER_CONDITION>().HasNoKey();

            // โมเดล master ที่เป็น View หรือ Stored Procedure
            modelBuilder.Entity<WBSOutModel>().HasNoKey();
            modelBuilder.Entity<CCOutModel>().HasNoKey();
            modelBuilder.Entity<GLOutModel>().HasNoKey();
            modelBuilder.Entity<CompanyResultModel>().HasNoKey();
            modelBuilder.Entity<CountryResultModel>().HasNoKey();

            // traveler summary service ที่เป็น View หรือ Stored Procedure
            //doc_type,emp_id,total_expen,budget_limit,appr_position,appr_type,cost_center,appr_id,approve_status,approve_remark,approve_opt,remark_opt,approve_level,traveler_ref_id
            modelBuilder.Entity<TravelerApproverConditionModel>().HasNoKey();
            modelBuilder.Entity<TravelerApproverLevelModel>().HasNoKey();
            modelBuilder.Entity<TravelerApproverSummaryConditionModel>().HasNoKey();
            modelBuilder.Entity<TravelerApproverSummaryApproveLevelModel>().HasNoKey();

            modelBuilder.Entity<TravelerDocHead>().HasNoKey();
            modelBuilder.Entity<RestApproverListModel>().HasNoKey();
            modelBuilder.Entity<ApproverConditionModel>().HasNoKey();
            modelBuilder.Entity<TravelerExpense>().HasNoKey();
            modelBuilder.Entity<TravelerUsersOrgName>().HasNoKey();

            // estimate expense service
            modelBuilder.Entity<EstExpTravelDateModel>().HasNoKey();
            modelBuilder.Entity<EstExpSAPModel>().HasNoKey();

            modelBuilder.Entity<NormalModel>().HasNoKey();

            modelBuilder.Entity<tempEMailModel>().HasNoKey();
            modelBuilder.Entity<tempIdKeyModel>().HasNoKey();
            modelBuilder.Entity<tempStatusModel>().HasNoKey();
            modelBuilder.Entity<tempEmployeeProfileModel>().HasNoKey();
            modelBuilder.Entity<temptravelInsuranceModel>().HasNoKey();
            modelBuilder.Entity<tempEmpSpecialModel>().HasNoKey();

            modelBuilder.Entity<TravelerUsersCAP>().HasNoKey();

            //Doc2 Submit
            modelBuilder.Entity<employeeDoc2SubmitModel>().HasNoKey();

            //Doc3 Search Data
            modelBuilder.Entity<tempEmpIdModel>().HasNoKey();
            modelBuilder.Entity<ExpenseTravelerConditionModel>().HasNoKey();
            modelBuilder.Entity<DocDetail3HeadTable1Model>().HasNoKey();

            //Doc3 Submit
            modelBuilder.Entity<capApproveModel>().HasNoKey();


            //Search
            modelBuilder.Entity<actionTypeModel>().HasNoKey();

            //Mail  
            modelBuilder.Entity<tempISOSMailModel>().HasNoKey();



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
        public DbSet<BZ_MASTER_STATUS> BZ_MASTER_STATUS { get; set; }
        public DbSet<BZ_DOC_RUNNING> BZ_DOC_RUNNING { get; set; }
        public DbSet<BZ_DOC_HEAD> BZ_DOC_HEAD { get; set; }
        public DbSet<BZ_DOC_ACTION> BZ_DOC_ACTION { get; set; }
        public DbSet<BZ_DOC_TRAVELER_APPROVER> BZ_DOC_TRAVELER_APPROVER { get; set; }
        public DbSet<BZ_DOC_TRAVELER_APPROVER_P3> BZ_DOC_TRAVELER_APPROVER_P3 { get; set; }
        public DbSet<BZ_DOC_TRAVELER_EXPENSE> BZ_DOC_TRAVELER_EXPENSE { get; set; }
        public DbSet<BZ_DOC_TRAVELER> BZ_DOC_TRAVELER { get; set; }
        public DbSet<BZ_DOC_FILE> BZ_DOC_FILE { get; set; }
        public DbSet<BZ_DOC_FILE_ATTACHMENT> BZ_DOC_FILE_ATTACHMENT { get; set; }

        public DbSet<VW_BZ_USERS> VW_BZ_USERS { get; set; }

        public DbSet<BZ_EMAIL_DETAILS> BZ_EMAIL_DETAILS { get; set; }


        //********** set doc service ???เป็น View / Stored Procedure 
        public DbSet<DocHeadModel> DocHeadModelList { get; set; }
        public DbSet<DocDetail3HeadModel> DocDetail3HeadModelList { get; set; }
        public DbSet<DocList2Model> DocList2ModelList { get; set; }
        public DbSet<DocList3Model> DocList3ModelList { get; set; }

        public DbSet<docFlow2_travel> DocFlow2TravelList { get; set; }
        public DbSet<doc2ApproverModel> Doc2ApproverModelList { get; set; }

        public DbSet<DocFileListOutModel> DocFileListOutModelList { get; set; }
        public DbSet<DocFileListTravelerhistoryOutModel> DocFileListTravelerhistoryOutModelList { get; set; }

        public DbSet<allApproveModel> AllApproveModelList { get; set; }
        public DbSet<capApproveModel> CAPApproveModelList { get; set; }
        public DbSet<SearchCAP_TraverlerModel> SearchCAP_TraverlerModelList { get; set; }
        public DbSet<SearchUserAllModel> SearchUserAllModelList { get; set; }
        public DbSet<SearchUserNameModel> SearchUserNameList { get; set; }
        public DbSet<SearchUserRoleTypeModel> SearchUserRoleTypeList { get; set; }
        public DbSet<SearchUserEmailModel> SearchUserEmailModelList { get; set; }
        public DbSet<SearchUserActionModel> SearchUserActionList { get; set; }
        public DbSet<SearchCAPModel> SearchCAPModelList { get; set; }

        public DbSet<CountryDocModel> CountryDocModelList { get; set; }
        public DbSet<ProvinceDocModel> ProvinceDocModelList { get; set; }
        public DbSet<TelephoneModel> TelephoneModelList { get; set; }
        public DbSet<costcenter_io> CostcenterIOList { get; set; }
        public DbSet<gl_account> GLAccountList { get; set; }

        public DbSet<tempEMailModel> TempEMailModelList { get; set; }
        public DbSet<tempIdKeyModel> TempIdKeyModelList { get; set; }
        public DbSet<temptravelInsuranceModel> TemptravelInsuranceModelList { get; set; }
        public DbSet<tempEmpSpecialModel> TempEmpSpecialModelList { get; set; }

        //public DbSet<estimateExpenseModel> EstimateExpenseModelList { get; set; }
        public DbSet<tempStatusModel> TempStatusModelList { get; set; }
        public DbSet<tempEmpIdModel> TempEmpIdModelList { get; set; }
        public DbSet<tempEmployeeProfileModel> TempEmployeeProfileModelList { get; set; }
        public DbSet<tempModel> TempModelList { get; set; }
        public DbSet<tempPassportModel> TempPassportModelList { get; set; }
        public DbSet<tempISOSMailModel> TempISOSMailModelList { get; set; }


        public DbSet<ExchangeRatesModel> ExchangeRatesModelList { get; set; }
        public DbSet<ContinentDocModel> ContinentDocModelList { get; set; }

        public DbSet<employeeDoc2Model> EmployeeDoc2ModelList { get; set; }
        public DbSet<employeeDoc2SubmitModel> EmployeeDoc2SubmitModelList { get; set; }
        public DbSet<approverModel> ApproverModelList { get; set; }
        public DbSet<MasterCostCenter> MasterCostCenterList { get; set; }
        public DbSet<TravelerUsers> TravelerUsersModelList { get; set; }

        public DbSet<TravelerUsersV2> TravelerUsersV2ModelList { get; set; }

        public DbSet<TravelerUsersCAP> TravelerUsersCAPModelList { get; set; }
        public DbSet<travelerDoc2Model> TravelerDoc2ModelList { get; set; }
        public DbSet<BZ_DOC_TRAVELER_APPROVER_V2> TravelApproveList { get; set; }

        public DbSet<TravelerDocModel> TravelerDocModelList { get; set; }
        public DbSet<StatusDocModel> StatusDocModelList { get; set; }


        //********** master ???เป็น View / Stored Procedure 
        public DbSet<WBSOutModel> WBSOutModelList { get; set; }
        public DbSet<CCOutModel> CCOutModelList { get; set; }
        public DbSet<GLOutModel> GLOutModelList { get; set; }
        public DbSet<CompanyResultModel> CompanyResultModelList { get; set; }
        public DbSet<CountryResultModel> CountryResultModelList { get; set; }


        //********** traveler summary service ???เป็น View / Stored Procedure  
        public DbSet<TravelerApproverConditionModel> TravelerApproverConditionModelList { get; set; }
        public DbSet<TravelerApproverLevelModel> TravelerApproverLevelModelList { get; set; }
        public DbSet<TravelerApproverSummaryConditionModel> TravelerApproverSummaryConditionModelList { get; set; }
        public DbSet<TravelerApproverSummaryApproveLevelModel> TravelerApproverSummaryApproveLevelModelList { get; set; }
        public DbSet<TravelerDocHead> TravelerDocHeadList { get; set; }
        public DbSet<RestApproverListModel> RestApproverListModelList { get; set; }
        public DbSet<ApproverConditionModel> ApproverConditionModelList { get; set; }
        public DbSet<TravelerExpense> TravelerExpenseList { get; set; }
        public DbSet<TravelerUsersOrgName> TravelerUsersOrgNameList { get; set; }

        public DbSet<BZ_BUDGET_APPROVER_CONDITION> BzBudgetApproverConditionList { get; set; }


        //********** estimate expense service
        public DbSet<EstExpTravelDateModel> EstExpTravelDateModelList { get; set; }
        public DbSet<EstExpSAPModel> EstExpSAPModelList { get; set; }


        //Search
        public DbSet<actionTypeModel> actionTypeModelList { get; set; }

        #endregion DbSet

    }


}
