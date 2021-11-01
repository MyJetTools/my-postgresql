using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace MyPostgreSQL
{
    public static class DapperHelpers
    {
        public static async Task<int> InsertEntityAsync(this DbConnection dbConnection, string tableName, object entity)
        {
            var (fields, _) = SqlOperation.Insert.GenerateFieldsString(entity);
            var sql =
                $"INSERT INTO {tableName} ({fields}) VALUES ({SqlOperation.Insert.GenerateValues(entity)})";

            try
            {
                return await dbConnection.ExecuteAsync(sql);

            }
            catch (Exception e)
            {
                Console.WriteLine(sql + ": " + e.Message);
                throw;
            }
        }

        public static IPostgresDapperUpdate Update(this IPostgresConnection connString, string tableName)
        {
            return new PostgresUpdateCommand(connString, tableName);
        }

        public static IPostgresDapperInsert Insert(this IPostgresConnection connString, string tableName)
        {
            return new PostgresInsertCommand(connString, tableName);
        }

        public static IPostgresDapperBulkInsert<T> BulkInsertPostgresQueryAsync<T>(this IPostgresConnection connString,
            string tableName)
        {
            return new PostgresBulkInsertCommand<T>(connString, tableName);
        }

        public static IPostgresDapperBase Delete(this IPostgresConnection connString, string tableName)
        {
            return new PostgresDeleteCommand(connString, tableName);
        }

        public static async Task ExecAsync(this IPostgresConnection postgresConnection, string sql,
            object model = null)
        {
            var jsonResult = string.Empty;
            try
            {
                sql = postgresConnection.ApplyScheme(sql);
                await using var conn = new NpgsqlConnection(postgresConnection.ConnectionString);
                await conn.OpenAsync();

                if (model == null)
                {
                    await conn.ExecuteScalarAsync<string>(sql);
                    return;
                }

                await conn.ExecuteScalarAsync<string>(sql, model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ": " + sql + "; JsonResult:" + jsonResult);
                throw;
            }
        }

        public static async Task<IEnumerable<T>> GetRecordsAsync<T>(this IPostgresConnection postgresConnection, string sql,
            object model = null)
        {
            try
            {
                sql = postgresConnection.ApplyScheme(sql);
                
                await using var conn = new NpgsqlConnection(postgresConnection.ConnectionString);
                
                await conn.OpenAsync();
                if (model == null)
                    return await conn.QueryAsync<T>(sql);

                return await conn.QueryAsync<T>(sql, model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ": " + sql);
                throw;
            }
        }

        public static async Task<T> GetFirstRecordOrNullAsync<T>(this IPostgresConnection postgresConnection, string sql,
            object model = null)
        {
            sql = postgresConnection.ApplyScheme(sql);
            
            await using var conn = new NpgsqlConnection(postgresConnection.ConnectionString);
            
            await conn.OpenAsync();
            if (model == null)
                return await conn.QueryFirstOrDefaultAsync<T>(sql);

            return await conn.QueryFirstOrDefaultAsync<T>(sql, model);
        }

        public static async Task<int> DoPostgresExecuteAsync(this IPostgresConnection postgresConnection, string sql,
            object model = null)
        {
            
            sql = postgresConnection.ApplyScheme(sql);
            await using var conn = new NpgsqlConnection(postgresConnection.ConnectionString);
            
            await conn.OpenAsync();
            if (model == null)
                return await conn.ExecuteAsync(sql);

            return await conn.ExecuteAsync(sql, model);
        }

        public static async Task<int> GetPostgresCountAsync(this IPostgresConnection postgresConnection, string sql,
            object model = null)
        {
            try
            {
                sql = postgresConnection.ApplyScheme(sql);
                await using var conn = new NpgsqlConnection(postgresConnection.ConnectionString);
                await conn.OpenAsync();
                
                if (model == null)
                    return await conn.ExecuteScalarAsync<int>(sql);

                return await conn.ExecuteScalarAsync<int>(sql, model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ": " + sql);
                throw;
            }
        }

        public static async Task<T> GetSingleRecordSingleValueAsync<T>(this IPostgresConnection postgresConnection,
            string sql, object model = null)
        {
            try
            {
                sql = postgresConnection.ApplyScheme(sql);
                await using var conn = new NpgsqlConnection(postgresConnection.ConnectionString);
                await conn.OpenAsync();
                return model == null
                    ? await conn.ExecuteScalarAsync<T>(sql)
                    : await conn.ExecuteScalarAsync<T>(sql, model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ": " + sql);
                throw;
            }
        }
        
        public static IPostgresInsertIfNotExistsBulkCommand BulkInsertIfNotExists(this IPostgresConnection connectionString, string tableName)
        {
            return new PostgresInsertIfNotExistsBulkCommand(connectionString, tableName);
        }
        
        public static IPostgresDapperBulkInsertOrUpdate BulkInsertOrUpdate(this IPostgresConnection connectionString, string tableName)
        {
            return new PostgresBulkInsertOrUpdateCommand(connectionString, tableName);
        }

        public static IPostgresDapperInsertOrUpdate InsertOrUpdate(this IPostgresConnection connectionString,
            string tableName)
        {
            return new PostgresInsertOrUpdateCommand(connectionString, tableName);
        }
    }
    
}