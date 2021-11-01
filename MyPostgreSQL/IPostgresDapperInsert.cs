using System;

namespace MyPostgreSQL
{
    public interface IPostgresDapperInsert : IPostgresDapperBase
    {
        IPostgresDapperInsert SetInsertString(string insertString);
        IPostgresDapperInsert SetInsertModel(object insertModel);
    }
    
    public class PostgresInsertCommand : PostgresDapperBase, IPostgresDapperInsert
    {

        private readonly string _tableName;

        private string _insertString;
        public PostgresInsertCommand(IPostgresConnection connectionString, string tableName) : base(connectionString)
        {
            _tableName = connectionString.GenerateTableName(tableName);
        }

        protected override string GenerateBeforeWhere()
        {
            if (_insertString == null)
                throw new Exception("Please initialize insert model");

            return $"INSERT INTO {_tableName} {_insertString}";
        }

        public IPostgresDapperInsert SetInsertString(string insertString)
        {
            _insertString = insertString;
            return this;
        }

        public IPostgresDapperInsert SetInsertModel(object updateModel)
        {

            var (fields, _) = SqlOperation.Insert.GenerateFieldsString(updateModel);
            var values = SqlOperation.Insert.GenerateValues(updateModel);

            _insertString = $"({fields}) VALUES ({values})";

            return this;
        }
    }
}