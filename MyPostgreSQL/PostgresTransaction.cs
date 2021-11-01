using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace MyPostgreSQL
{
    public class PostgresTransaction
    {
        private readonly IPostgresConnection _postgresConnection;

        public PostgresTransaction(IPostgresConnection postgresConnection)
        {
            _postgresConnection = postgresConnection;
        }

        private readonly List<IPostgresDapperBase> _commands = new List<IPostgresDapperBase>();

        public PostgresTransaction Add(IPostgresDapperBase postgresDapperBase)
        {

            _commands.Add(postgresDapperBase);
            return this;
        }

        public async Task ExecuteAsync()
        {

            if (_commands.Count == 0)
                throw new Exception("Transaction does not have commands to execute");

            var sql = new StringBuilder();
            sql.Append("BEGIN;");

            foreach (var command in _commands)
                sql.Append(command.GetSqlRequest() + ';');

            sql.Append("COMMIT;");


            await using var dbConnection = new NpgsqlConnection(_postgresConnection.ConnectionString);

            try
            {
                await dbConnection.OpenAsync();
                await dbConnection.ExecuteAsync(sql.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(sql + ": " + e.Message);
                throw;
            }
        }
    }


    public static class PostgresTransactionHelper
    {
        public static PostgresTransaction StartTransaction(this IPostgresConnection postgresConnection)
        {
            return new PostgresTransaction(postgresConnection);
        }
    }

}