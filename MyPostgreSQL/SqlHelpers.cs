using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MyPostgreSQL
{
    public class ReadOnlyDbField : Attribute
    {
    }

    public enum SqlOperation
    {
        Insert, Update
    }
    
    public static class SqlHelpers
    {
        
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> TypeInfos
            = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();

        private static IReadOnlyDictionary<string, PropertyInfo> GetPropertiesAsDictionary(this Type type)
        {
            if (TypeInfos.TryGetValue(type, out var value))
                return value;


            var result = type.GetProperties().ToDictionary(itm => itm.Name.ToLower());

            TypeInfos.TryAdd(type, result);

            return result;
        }

        public static string GenerateAndCondition(this object model)
        {
            var properties = model.GetType().GetPropertiesAsDictionary();

            var result = new StringBuilder();
            foreach (var property in properties)
            {
                if (result.Length > 0)
                    result.Append(" AND ");
                result.Append(property.Key + '=' + property.Value.GetValue(model).ToPostgresValueString());
            }

            return result.ToString();
        }
        
        internal static IEnumerable<PropertyInfo> GetEntityProperties(this object entityToInsert, SqlOperation sqlOperation)
        {
            var type = entityToInsert.GetType();
            
            foreach (var pi in type.GetPropertiesAsDictionary().Values.Where(itm => itm.CanWrite || sqlOperation == SqlOperation.Update))
            {                
                var attr = pi.GetCustomAttribute<ReadOnlyDbField>();
                if (attr == null)
                    yield return pi;
            }
        }

        internal static IEnumerable<string> GenerateFieldsList(this SqlOperation sqlOperation, object entityToInsert)
        {
            foreach(var property in GetEntityProperties(entityToInsert, sqlOperation))
            {
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                var name = columnAttr == null ? '"' + property.Name.ToLower() + '"' : columnAttr.Name;

                yield return name;
            }
        }

        internal static (string updateString, int fieldsCount) GenerateFieldsString(this SqlOperation sqlOperation, object entityToInsert)
        {
            var fileds = GenerateFieldsList(sqlOperation, entityToInsert);

            var result = string.Join(",", fileds);

            return (result, fileds.Count());
        }

        internal static (string updateString, int fieldsCount) GenerateInsertOnConflictUpdateString(this SqlOperation sqlOperation, object entityToInsert)
        {
            var result = new StringBuilder();

            var first = true;
            var count = 0;
            foreach (var pi in GetEntityProperties(entityToInsert, sqlOperation))
            {
                count++;

                if (first)
                    first = false;
                else
                    result.Append(',');

                var columnAttr = pi.GetCustomAttribute<ColumnAttribute>();
                var name = columnAttr == null ? '"' + pi.Name.ToLower() + '"' : columnAttr.Name;
                result.Append(name);
            }

            return (result.ToString(), count);
        }

        private static string EscapePostgresString(this string value)
        {
            return value.Replace("'", "''");
        }

        private const string DbNullValue = "NULL";


        public static string ToPostgresValueString(this object value)
        {

            var type = value.GetType();
            if (type == typeof(string) || type == typeof(char))
                return "'" + value.ToString().EscapePostgresString() + "'";

            if (type == typeof(DateTime))
            {
                var dt = (DateTime) value;
                return "'" + dt.ToString("O") + "'";
            }

            if (type == typeof(DateTime?))
            {
                var dt = (DateTime?) value;
                return "'" + dt.Value.ToString("O") + "'";
            }

            if (type == typeof(Guid))
            {
                var dt = value.ToString();
                return "'" + dt + "'::uuid";
            }

            return (string) Convert.ChangeType(value, TypeCode.String, CultureInfo.InvariantCulture);
            
        }

        private static string GetValueForPostgres(this PropertyInfo pi, object entityToInsert)
        {
            var value = pi.GetValue(entityToInsert);

            if (value == null)
                return DbNullValue;

            return value.ToPostgresValueString();

        }

        public static string GenerateValues(this SqlOperation sqlOperation, object entityToInsert)
        {
            var result = new StringBuilder();

            var first = true;
            foreach (var pi in entityToInsert.GetEntityProperties(sqlOperation))
            {
                if (first)
                    first = false;
                else
                    result.Append(',');

                result.Append(pi.GetValueForPostgres(entityToInsert));
            }

            return result.ToString();
        }
        
        public static string ToPostgresWhereArray(this IEnumerable<string> src)
        {
            var result = new StringBuilder();

            foreach (var itm in src)
            {
                if (result.Length > 0)
                    result.Append(",");
                result.Append("'" + itm + "'");
            }

            return "(" + result + ")";
        }
        
    }
}