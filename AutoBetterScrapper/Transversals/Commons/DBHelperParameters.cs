using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using static AutoBetterScrapper.Transversals.Commons.EnumHelper;
namespace AutoBetterScrapper.Transversals.Commons
{
    public class DBHelperParameters
    {
        public static SqlParameter NewParameter(string ParameterName, object value, SqlDbType type)
        {
            return new SqlParameter
            {
                ParameterName = ParameterName,
                SqlValue = value ?? DBNull.Value,
                SqlDbType = type
            };
        }
        public static string BuilderFunction(EnumSchemas squema, [System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return squema switch
                {
                    EnumSchemas.DBO => $"{GetEnumDescription(EnumSchemas.DBO)}.{name}",
                    EnumSchemas.SETTING => $"{GetEnumDescription(EnumSchemas.SETTING)}.{name}",
                    EnumSchemas.SECURITY => $"{GetEnumDescription(EnumSchemas.SECURITY)}.{name}",

                    _ => name,
                };
            }
            else
                return name;
        }
        public enum EnumSchemas
        {
            [Description("dbo")]
            DBO,

            [Description("setting")]
            SETTING,

            [Description("security")]
            SECURITY
        }
    }
}
