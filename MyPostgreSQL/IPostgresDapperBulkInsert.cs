using System;
using System.Collections.Generic;
using System.Text;

namespace MyPostgreSQL
{
public interface IPostgresDapperBulkInsert<in T> : IPostgresDapperBase
    {
        IPostgresDapperBulkInsert<T> SetInsertString(string insertString);
        IPostgresDapperBulkInsert<T> ClearTableBeforeInsert();
        IPostgresDapperBulkInsert<T> SetInsertModel(IEnumerable<T> insertModel);
    }
    
    public class PostgresBulkInsertCommand<T> : PostgresDapperBase, IPostgresDapperBulkInsert<T>
    {

        private readonly string _tableName;

        private string _insertString;

        private bool _clearTableBeforeInsert;
        public PostgresBulkInsertCommand(IPostgresConnection connectionString, string tableName) : base(connectionString)
        {
            _tableName = tableName;
        }

        protected override string GenerateBeforeWhere()
        {
            if (_insertString == null)
                throw new Exception("Please initialize insert model");

            var openTransaction = _clearTableBeforeInsert ? $"BEGIN; DELETE FROM {_tableName};" : "";
            var commitTransaction = _clearTableBeforeInsert ? "COMMIT;" : "";

            return $"{openTransaction}INSERT INTO {_tableName} {_insertString}{commitTransaction}";
        }

        public IPostgresDapperBulkInsert<T> SetInsertString(string insertString)
        {
            _insertString = insertString;
            return this;
        }

        public IPostgresDapperBulkInsert<T> ClearTableBeforeInsert()
        {
            _clearTableBeforeInsert = true;
            return this;
        }

        public IPostgresDapperBulkInsert<T> SetInsertModel(IEnumerable<T> insertModel)
        {


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

            insertValues.Append(';');



            _insertString = $"({fields}) VALUES {insertValues}";

            return this;
        }

    }
}