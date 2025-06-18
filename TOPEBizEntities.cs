using Microsoft.EntityFrameworkCore; 
using Oracle.ManagedDataAccess.Client; 

namespace top.ebiz.service
{ 
    public partial class TOPEBizEntities : DbContext
    {

        // สร้าง constructor ที่ไม่มีพารามิเตอร์และดึง connection string จาก appsettings.json
        public TOPEBizEntities()
            : base(GetOptionsx(GetConnectionStringFromAppSettingsx()))
        {
        }

        // Constructor ที่รับ connection string เป็นพารามิเตอร์
        public TOPEBizEntities(string connectionString)
            : base(GetOptionsx(connectionString))
        {
        }

        // Constructor ที่รับ DbContextOptions สำหรับการเชื่อมต่อฐานข้อมูล (ใช้กับ DI)
        public TOPEBizEntities(DbContextOptions<TOPEBizEntities> options)
            : base(options)
        {
        }
        private static DbContextOptions GetOptionsx(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TOPEBizEntities>();
            optionsBuilder.UseOracle(connectionString);
            return optionsBuilder.Options;
        }

        private static string GetConnectionStringFromAppSettingsx()
        {
            // สร้าง IConfiguration เพื่ออ่านไฟล์ appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // ดึง connection string จาก appsettings.json
            return config.GetConnectionString("eBizConnection") ?? "";
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NormalModel>().HasNoKey();

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
        //********** all 
        public DbSet<NormalModel> NormalModelList { get; set; }
        public class NormalModel
        {
            public string id { get; set; }
            public string name { get; set; }
            public string text { get; set; }
        }


    }

}
