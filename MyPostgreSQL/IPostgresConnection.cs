namespace MyPostgreSQL
{
    public interface IPostgresConnection
    {
        string ConnectionString { get; }
        string Scheme { get; }
    }

    public class PostgresConnection : IPostgresConnection
    {
        public PostgresConnection(string connectionString, string appName, string scheme = null)
        {
            BaseConnectionString = connectionString;
            Scheme = scheme;
            AppName = appName;
        }
        private string BaseConnectionString { get; }
        private string AppName { get; }

        public string ConnectionString => $"{BaseConnectionString}Application Name={AppName}";
        public string Scheme { get; }
    }
}