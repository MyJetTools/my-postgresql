using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyPostgreSQL
{
    public interface IPostgresDapperBulkInsertOrUpdate : IPostgresDapperBase
    {
        IPostgresDapperBulkInsertOrUpdate SetModels<T>(IEnumerable<T> insertModel, string primaryKeyName);
    }

    public class PostgresBulkInsertOrUpdateCommand : PostgresDapperBase, IPostgresDapperBulkInsertOrUpdate
    {
        private readonly string _tableName;
        private string _insertString;
        private string _updateString;
        private string _pkName;

        public PostgresBulkInsertOrUpdateCommand(IPostgresConnection connectionString, string tableName) : base(connectionString)
        {
            _tableName = tableName;
        }

        protected override string GenerateBeforeWhere()
        {
            if (_insertString == null)
                throw new Exception("Please initialize insert model");

            return $@"INSERT INTO {_tableName} {_insertString} ON CONFLICT ON CONSTRAINT {_pkName} DO UPDATE SET {_updateString};";
        }

        public IPostgresDapperBulkInsertOrUpdate SetModels<T>(IEnumerable<T> insertModel, string primaryKeyName)
        {
            _pkName = primaryKeyName;

            string fields = null;
            var insertValues = new StringBuilder();

            foreach (var itm in insertModel)
            {
                if (fields == null)
                    (fields, _) = SqlOperation.Insert.GenerateFieldsString(itm);

                if (insertValues.Length > 0)
                    insertValues.Append(',');

                insertValues.Append('(');
                insertValues.Append(SqlOperation.Insert.GenerateValues(itm));
                insertValues.Append(')');
            }

            _insertString = $"({fields}) VALUES {insertValues}";

            _updateString = GenerateUpdateString(insertModel.First());

            return this;
        }

        private string GenerateUpdateString(object model)
        {
            var filedNames = SqlOperation.Update.GenerateFieldsList(model);
            var fieldNamesString = string.Join(",", filedNames);
            var updateStringValues = string.Join(",", filedNames.Select(fieldName => $"excluded.{fieldName.Replace("\"", string.Empty)}"));

            return $"({fieldNamesString})=({updateStringValues})";
        }
    }

}