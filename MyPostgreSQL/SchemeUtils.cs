
namespace MyPostgreSQL
{
    public static class SchemeUtils
    {
        
        
        public static string GenerateTableName(this IPostgresConnection postgresConnection, string tableName)
        {

            if (string.IsNullOrEmpty(postgresConnection.Scheme))
                return tableName;

            return postgresConnection.Scheme+'.'+tableName;

        }

        public static string ApplyScheme(this IPostgresConnection postgresConnection, string sql)
        {
            return string.IsNullOrEmpty(postgresConnection.Scheme) 
                ? sql 
                : $"SET search_path TO {postgresConnection.Scheme}; {sql}";
        }
    }
}