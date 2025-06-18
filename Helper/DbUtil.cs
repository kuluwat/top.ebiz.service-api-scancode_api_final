using Oracle.ManagedDataAccess.Client;

public class DbUtil
{
public static List<OracleParameter> CopyOracleCommandParameters(OracleCommand sourceCmd)
{
    List<OracleParameter> newCmd = new();

    foreach (OracleParameter param in sourceCmd.Parameters)
    {
        newCmd.Add(new OracleParameter(param.ParameterName, param.Value)
        {
            DbType = param.DbType,
            Direction = param.Direction,
            Size = param.Size,
            SourceColumn = param.SourceColumn,
            OracleDbType = param.OracleDbType
        });
    }

    return newCmd;
}
    
}