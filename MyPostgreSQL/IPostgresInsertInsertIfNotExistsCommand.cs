using System;

namespace MyPostgreSQL
{
    public interface IPostgresInsertInsertIfNotExistsCommand : IPostgresDapperBase
    {
        IPostgresInsertInsertIfNotExistsCommand SetInsertModel(object insertModel, string primaryKeyName);
    }

    public class PostgresInsertInsertIfNotExistsCommand : PostgresDapperBase, IPostgresInsertInsertIfNotExistsCommand
    {
        private readonly string _tableName;
        private string _insertString;
        private string _pkName;

        public PostgresInsertInsertIfNotExistsCommand(IPostgresConnection connectionString, string tableName) : base(
            connectionString)
        {
            _tableName = connectionString.GenerateTableName(tableName);
        }

        protected override string GenerateBeforeWhere()
        {
            if (_insertString == null)
                throw new Exception("Please initialize insert model");

            return
                $"INSERT INTO {_tableName} {_insertString} ON CONFLICT ON CONSTRAINT {_pkName} DO NOTHING";
        }

        public IPostgresInsertInsertIfNotExistsCommand SetInsertModel(object updateModel, string primaryKeyName)
        {
            _pkName = primaryKeyName;

            var (fields, _) = SqlOperation.Insert.GenerateFieldsString(updateModel);
            var values = SqlOperation.Insert.GenerateValues(updateModel);

            _insertString = $"({fields}) VALUES ({values})";

            return this;
        }
    }

    public static class PostgresInsertIfNotExistsCommandHelpers
    {
        public static IPostgresInsertInsertIfNotExistsCommand InsertIfNotExists(this IPostgresConnection connectionString, string tableName)
        {
            return new PostgresInsertInsertIfNotExistsCommand(connectionString, tableName);
        }
    }
}