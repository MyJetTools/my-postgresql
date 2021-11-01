using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace MyPostgreSQL
{
    public interface IPostgresDapperBase
    {
        IPostgresDapperBase SetWhereCondition(string whereCondition, object whereModel = null);
        IPostgresDapperBase SetWhereConditionModel(object whereModel);
        Task<int> ExecuteAsync();
        string GetSqlRequest();
    }

    public abstract class PostgresDapperBase : IPostgresDapperBase
    {
        private readonly IPostgresConnection _connectionString;
        private object _whereModel;
        private string _whereConditionData;


        protected abstract string GenerateBeforeWhere();

        protected PostgresDapperBase(IPostgresConnection connectionString)
        {
            _connectionString = connectionString;
        }

        public IPostgresDapperBase SetWhereCondition(string whereCondition, object whereModel = null)
        {
            _whereConditionData = whereCondition;
            _whereModel = whereModel;
            return this;
        }


        public IPostgresDapperBase SetWhereConditionModel(object whereModel)
        {
            _whereConditionData = whereModel.GenerateAndCondition();
            return this;
        }

        private string GetWhereCondition()
        {
            return _whereConditionData == null ? string.Empty : " WHERE " + _whereConditionData;
        }


        public string GetSqlRequest()
        {
            return GenerateBeforeWhere() + GetWhereCondition();
        }

        public async Task<int> ExecuteAsync()
        {
            if (_connectionString == null)
                throw new Exception("Connection string is not Set");

            await using var dbConnection = new NpgsqlConnection(_connectionString.ConnectionString);
            
            var sql = GetSqlRequest();
            try
            {
                await dbConnection.OpenAsync();

                if (_whereModel == null)
                    return await dbConnection.ExecuteAsync(sql);

                return await dbConnection.ExecuteAsync(sql, _whereModel);

            }
            catch (Exception e)
            {
                Console.WriteLine(sql + ": " + e.Message);
                throw;
            }
        }
    }
}