using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.SqlClient;


public class ORMContext
{
    private readonly string _connectionString;
    private readonly string tableName = "users";

    public ORMContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public T Create<T>(T entity, string tableName) where T : class
    {
        // Пример реализации метода Create
        // Параметризованный SQL-запрос для вставки данных
        throw new NotImplementedException();
    }

    public IEnumerable<T> ReadById<T>(int id) where T : class, new()
    {
        var result = new List<T>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string queryRequest = $"SELECT * FROM {tableName} WHERE id = @id";
            SqlCommand command = new SqlCommand(queryRequest, connection);
            command.Parameters.AddWithValue("@id", id);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(Map<T>(reader));
                }
            }
        }

        return result;
    }

    public IEnumerable<T> ReadByAll<T>() where T : class, new()
    {
        var result = new List<T>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string queryRequest = $"SELECT * FROM {tableName}";
            SqlCommand command = new SqlCommand(queryRequest, connection);
            //command.Parameters.AddWithValue("@id");

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(Map<T>(reader));
                }
            }
        }

        return result;
    }

    public IEnumerable<T> ReadByName<T>(string Name) where T : class, new()
    {
        var result = new List<T>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string queryRequest = $"SELECT * FROM {tableName} WHERE name = @Name ";
            SqlCommand command = new SqlCommand(queryRequest, connection);
            command.Parameters.AddWithValue("@name", Name);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    result.Add(Map<T>(reader));
                }
            }
        }

        return result;
    }

    public T ReadByEmail<T>(string email) where T : class, new()
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string queryRequest = $"SELECT * FROM {tableName} WHERE email = @email ";
            SqlCommand command = new SqlCommand(queryRequest, connection);
            command.Parameters.AddWithValue("@email", email);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Map<T>(reader);
                }
            }
        }

        return null;
    }


    public void Update<T>(int id, T entity, string tableName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"UPDATE {tableName} SET Column1 = data WHERE Id = @id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@value1", "значение");

            command.ExecuteNonQuery();
        }
    }

    public void Delete(int id, string tableName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"DELETE FROM {tableName} WHERE Id = @id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();
            Console.WriteLine("Deleted successfully");
        }
    }

    public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, new()
    {
        var query = BuildQuery(predicate);

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (var command = new SqlCommand(query.Item1, connection))
            {
                foreach (var parameter in query.Item2)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map<T>(reader);
                    }
                }
            }
        }

        return default(T);
    }

    private Tuple<string, Dictionary<string, object>> BuildQuery<T>(Expression<Func<T, bool>> predicate)
    {
        var tableName = typeof(T).Name + "s";
        var parameters = new Dictionary<string, object>();
        var whereClause = BuildWhereClause(predicate.Body, parameters);

        var query = $"SELECT TOP 1 * FROM {tableName} {whereClause}";
        return new Tuple<string, Dictionary<string, object>>(query, parameters);
    }

    private string BuildWhereClause(Expression expression, Dictionary<string, object> parameters)
    {
        if (expression is BinaryExpression binaryExpression)
        {
            var left = BuildWhereClause(binaryExpression.Left, parameters);
            var right = BuildWhereClause(binaryExpression.Right, parameters);
            var operatorString = binaryExpression.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Operator {binaryExpression.NodeType} is not supported")
            };
            return $"{left} {operatorString} {right}";
        }
        else if (expression is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (expression is ConstantExpression constantExpression)
        {
            var parameterName = $"@p{parameters.Count}";
            parameters.Add(parameterName, constantExpression.Value);
            return parameterName;
        }
        else
        {
            throw new NotSupportedException($"Expression type {expression.GetType().Name} is not supported");
        }
    }

    private T Map<T>(SqlDataReader reader) where T : class, new()
    {
        var entity = new T();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            var property = typeof(T).GetProperty(columnName);
            if (property != null && !reader.IsDBNull(i))
            {
                property.SetValue(entity, reader.GetValue(i));
            }
        }

        return entity;
    }
}