using System;

namespace MyPostgreSQL
{
    public interface IPostgresDapperUpdate : IPostgresDapperBase
    {
        IPostgresDapperBase SetUpdateString(string updateString);
        IPostgresDapperUpdate SetUpdateModel(object updateModel);
    }
    
    public class PostgresUpdateCommand : PostgresDapperBase, IPostgresDapperUpdate
    {
        private readonly string _tableName;

        private string _updateString;

        public PostgresUpdateCommand(IPostgresConnection connectionString, string tableName) : base(connectionString)
        {
            _tableName =  connectionString.GenerateTableName(tableName);
        }

        protected override string GenerateBeforeWhere()
        {
            if (_updateString == null)
                throw new Exception("Update string is null");

            return $"UPDATE {_tableName} SET {_updateString}";
        }

        public IPostgresDapperBase SetUpdateString(string updateString)
        {
            _updateString = updateString;
            return this;
        }

        public IPostgresDapperUpdate SetUpdateModel(object updateModel)
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