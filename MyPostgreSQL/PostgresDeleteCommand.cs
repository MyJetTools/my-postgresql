namespace MyPostgreSQL
{
    public class PostgresDeleteCommand : PostgresDapperBase
    {
        private readonly string _tableName;

        public PostgresDeleteCommand(IPostgresConnection connectionString, string tableName) : base(connectionString)
        {
            _tableName = connectionString.GenerateTableName(tableName);
        }

        protected override string GenerateBeforeWhere()
        {
            return $"DELETE FROM {_tableName}";
        }
    }
}