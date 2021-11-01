# MyPostgresSQL

MyPostgresSQL is a NuGet library, [Dapper](https://github.com/DapperLib/Dapper) wrapper with bulk operations support.


## Base usage
```c#
namespace BaseUsage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var postgresConnection = new PostgresConnection(PostgresConnectionString, AppName, Schema);
            var users = postgresConnection..GetRecordsAsync<T>("SELECT * FROM USERS WHERE Id > @Id", new {Id = "5"});
        }
    }
}
```

## Connection setup

PostgresConnectionString - required

AppName - required

Schema - optional, default: "public"
```c#
new PostgresConnection(PostgresConnectionString, AppName, Schema);
```

#Single operations

Get records
```c#
await _postgresConnection
    .GetRecordsAsync<T>("SELECT * FROM USERS WHERE Id > @Id", new {Id = "5"});
```

Get first
```c#
var count = await _postgresConnection
    .GetPostgresCountAsync("SELECT * FROM USERS WHERE Id > @Id", new {Id = "5"});
```

Get first or null
```c#
await _postgresConnection
    .GetFirstRecordOrNullAsync<T>("SELECT * FROM USERS WHERE Id > @Id", new {Id = "5"});
```

Base insert
```c#
await _postgresConnection
    .Insert(TableName)
    .SetInsertModel(entity)
    .ExecuteAsync();
```

Base update
```c#
await _postgresConnection
    .Update(TableName)
    .SetUpdateModel(entity)
    .SetWhereCondition("Id > @Id and Name = @Name", new {Id = "id", Name = "name"})
    .ExecuteAsync();
```

Insert or update
```c#
await _postgresConnection
    .InsertOrUpdate(TableName)
    .SetInsertModel<InsertModel>(InsertModel, PK)
    .SetUpdateModel<UpdateModel>(UpdateModel)
    .ExecuteAsync();
```

# Bulk operations
Bulk insert
```c#
await _postgresConnection
    .BulkInsertPostgresQueryAsync<T>(TableName)
    .SetInsertModel(IEnumerable<T> insertModel) //opotional
    .ExecuteAsync();
```

Bulk insert if not exists
```c#
await _postgresConnection
    .BulkInsertIfNotExists(TableName)
    .SetInsertModel(IEnumerable<T>, primaryKeyName)
    .ExecuteAsync();
```

Bulk insert or update
```c#
await _postgresConnection
    .BulkInsertOrUpdate(TableName)
    .SetInsertModel(IEnumerable<T>, primaryKeyName)
    .ExecuteAsync();
```

