using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace top.ebiz.service.Service
{
    public class ClassConnectionDb : IDisposable
    {
        static public string ConnectionString()
        {
            return top.ebiz.helper.AppEnvironment.GeteConnectionString();
        }
        public OracleConnection conn;
        public OracleTransaction trans;
        public OracleCommand cmd;
        public void OpenConnection()
        {
            if (conn == null)
            {
                conn = new OracleConnection(ConnectionString());
            }

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }
        public void CloseConnection()
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        public void BeginTransaction()
        {
            if (trans == null)
            {
                trans = conn.BeginTransaction();
            }
        }

        public void Commit()
        {
            if (trans != null)
            {
                trans.Commit();
            }
        }
        public void Rollback()
        {
            if (trans != null)
            {
                trans.Rollback();
            }
        }
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    trans?.Dispose();
                    conn?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        static public bool IsAuthorizedRole()
        {
            return true;
        }

        public DataSet ExecuteAdapter(OracleCommand cmd)
        {
            if (cmd.CommandType != CommandType.StoredProcedure)
            {
                cmd.CommandType = CommandType.Text;
            }
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null)
            {
                if (ds.Tables.Count > 0)
                {
                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        column.ColumnName = column.ColumnName.ToLower();
                    }
                }
            }
            return ds;
        }

        public string ExecuteNonQuerySQL(OracleCommand sqlCommand)
        {

            string ret = "";
            string query = sqlCommand.CommandText;
            try
            {
                if (sqlCommand.CommandType != CommandType.StoredProcedure)
                {
                    sqlCommand.CommandType = CommandType.Text;
                }

                sqlCommand.ExecuteNonQuery();

                ret = "true";

            }
            catch (Exception ex)
            {
                ret = ex.ToString();
                foreach (OracleParameter p in sqlCommand.Parameters)
                {
                    string value = p.Value == null ? "NULL" : p.Value?.ToString() ?? "";
                    if (p.OracleDbType == OracleDbType.Varchar2 || p.OracleDbType == OracleDbType.Char)
                    {
                        value = $"'{value}'";
                    }
                    query = query.Replace(p.ParameterName, value);
                }
            }
            return ret;
        }
        static public OracleParameter ConvertTypeParameter(string paramName, object value, string type = "char", int defLength = 4000)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(paramName));

            var param = new OracleParameter { ParameterName = paramName };

            switch (type.ToLowerInvariant())
            {
                case "char":
                    param.OracleDbType = OracleDbType.Char;
                    param.Size = value?.ToString()?.Length ?? defLength;
                    param.Value = value ?? DBNull.Value;
                    break;

                case "int":
                    param.OracleDbType = OracleDbType.Int32;
                    param.Value = TryParseInt(value);
                    break;

                case "number":
                    param.OracleDbType = OracleDbType.Decimal;
                    param.Value = TryParseDecimal(value);
                    break;

                case "date":
                    param.OracleDbType = OracleDbType.Date;
                    param.Value = TryParseDateTime(value);
                    break;

                default:
                    param.OracleDbType = OracleDbType.Varchar2;
                    param.Size = value?.ToString()?.Length ?? defLength;
                    param.Value = value ?? DBNull.Value;
                    break;
            }

            return param;
        }

        private static object TryParseInt(object value) =>
            (value != null && int.TryParse(value.ToString(), out int result)) ? result : DBNull.Value;

        private static object TryParseDecimal(object value) =>
            (value != null && decimal.TryParse(value.ToString(), out decimal result)) ? result : DBNull.Value;

        private static object TryParseDateTime(object value) =>
            (value != null && DateTime.TryParse(value.ToString(), out DateTime result)) ? result : DBNull.Value;



    }

    public class DecimalToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // ถ้าเป็นตัวเลข ให้แปลงเป็น string
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var decimalValue))
            {
                return decimalValue.ToString();
            }
            // ถ้าเป็น string ให้คืนค่าเดิม
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            throw new JsonException("Invalid JSON value for string.");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
