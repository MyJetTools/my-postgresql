using System;
using System.Collections.Generic;
using System.Text;

namespace MyPostgreSQL
{
    public interface IPostgresInsertIfNotExistsBulkCommand : IPostgresDapperBase
    {
        IPostgresInsertIfNotExistsBulkCommand SetInsertModels<T>(IEnumerable<T> insertModel, string primaryKeyName);
    }

    public class PostgresInsertIfNotExistsBulkCommand : PostgresDapperBase, IPostgresInsertIfNotExistsBulkCommand
    {
        private readonly string _tableName;
        private string _insertString;
        private string _pkName;

        public PostgresInsertIfNotExistsBulkCommand(IPostgresConnection connectionString, string tableName) : base(connectionString)
        {
            _tableName = tableName;
        }

        protected override string GenerateBeforeWhere()
        {
            if (_insertString == null)
                throw new Exception("Please initialize insert model");

            return $@"INSERT INTO {_tableName} {_insertString} ON CONFLICT ON CONSTRAINT {_pkName} DO NOTHING";
        }

        public IPostgresInsertIfNotExistsBulkCommand SetInsertModels<T>(IEnumerable<T> insertModel, string primaryKeyName)
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

            return this;
        }
    }

}