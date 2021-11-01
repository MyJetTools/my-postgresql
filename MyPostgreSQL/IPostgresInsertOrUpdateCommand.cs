using System;

namespace MyPostgreSQL
{
    public interface IPostgresDapperInsertOrUpdate : IPostgresDapperBase
    {
        IPostgresDapperInsertOrUpdate SetInsertModel<T>(T insertModel, string primaryKeyName);
        IPostgresDapperInsertOrUpdate SetUpdateModel<T>(T updateModel);

    }

    public class PostgresInsertOrUpdateCommand : PostgresDapperBase, IPostgresDapperInsertOrUpdate
    {

        private readonly string _tableName;

        private string _insertString;
        private string _updateString;

        private string _pkName;

        public PostgresInsertOrUpdateCommand(IPostgresConnection connectionString, string tableName) : base(
            connectionString)
        {
            _tableName = connectionString.GenerateTableName(tableName);
        }

        protected override string GenerateBeforeWhere()
        {
            if (_insertString == null)
                throw new Exception("Please initialize insert model");

            return
                $"INSERT INTO {_tableName} {_insertString} ON CONFLICT ON CONSTRAINT {_pkName} DO UPDATE SET {_updateString}";
        }

        public IPostgresDapperInsertOrUpdate SetInsertModel<T>(T updateModel, string primaryKeyName)
        {
            _pkName = primaryKeyName;

            var (fields, _) = SqlOperation.Insert.GenerateFieldsString(updateModel);
            var values = SqlOperation.Insert.GenerateValues(updateModel);

            _insertString = $"({fields}) VALUES ({values})";

            return this;
        }

        public IPostgresDapperInsertOrUpdate SetUpdateModel<T>(T updateModel)
        {
            var (fields, count) = SqlOperation.Update.GenerateFieldsString(updateModel);
            var values = SqlOperation.Update.GenerateValues(updateModel);

            _updateString = count == 1 
                ? fields+"="+values 
                : "(" + fields + ")=(" + values + ")";

            return this;
        }

    }
}