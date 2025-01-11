using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;

namespace MyORMLibrary;

public class TestORMContext<T> where T : class, new()
{
    private readonly string _connectionString;
    private readonly IDbConnection _dbConnection;

    public TestORMContext(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public T GetById(int id)
    {
        string query =
            $"SELECT \n" +
            $"m.id AS id,\n" +
            $"m.title AS title,\n" +
            $"m.year AS year,\n" +
            $"m.description AS description," +
            $"m.rating AS rating,\n" +
            $"m.duration AS duration,\n" +
            $"c.country AS country,\n" +
            $"s.director AS director,\n" +
            $"g.genre AS genre,\n" +
            $"m.poster_url AS poster_url\n" +
            $"FROM \n" +
            $"Films m\n" +
            $"LEFT JOIN \n" +
            $"Countries c ON m.country_id = c.id\n" +
            $"LEFT JOIN \n" +
            $"Director s ON m.director_id = s.id\n" +
            $"LEFT JOIN \n" +
            $"Genres g ON m.genre_id = g.id\n" +
            $"WHERE \n" +
            $"m.id = @id;";

        _dbConnection.Open();

        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = query;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@Id";
            parameter.Value = id;
            command.Parameters.Add(parameter);
            //command.Parameters.AddWithValue("@Id", id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Map(reader);
                }
            }
        }

        return null;
    }

    public IEnumerable<T> GetByAll()
    {
        var result = new List<T>();
        string query =
            $"SELECT \n" +
            $"m.id AS id,\n" +
            $"m.title AS title,\n" +
            $"m.year AS year,\n" +
            $"m.description AS description," +
            $"m.rating AS rating,\n" +
            $"m.duration AS duration,\n" +
            $"c.country AS country,\n" +
            $"s.director AS director,\n" +
            $"g.genre AS genre,\n" +
            $"m.poster_url AS poster_url\n" +
            $"FROM \n" +
            $"Films m\n" +
            $"LEFT JOIN \n" +
            $"Countries c ON m.country_id = c.id\n" +
            $"LEFT JOIN \n" +
            $"Director s ON m.director_id = s.id\n" +
            $"LEFT JOIN \n" +
            $"Genres g ON m.genre_id = g.id\n";

        _dbConnection.Open();

        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = query;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(Map(reader));
                }
            }
        }

        return result;
    }
    
    public void Update(int id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"UPDATE {typeof(T).Name}s SET Column1 = data WHERE Id = @id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@value1", "значение");

            command.ExecuteNonQuery();
        }
    }

    public void Delete(int id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"DELETE FROM {typeof(T).Name}s WHERE Id = @id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();
        }
    }

    private T Map(IDataReader reader)
    {
        var entity = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            if (reader[property.Name] != DBNull.Value)
            {
                property.SetValue(entity, reader[property.Name]);
            }
        }

        return entity;
    }
    
    private string ParseExpression(Expression expression)
    {
        if (expression is BinaryExpression binary)
        {
            // разбираем выражение на составляющие
            var left = ParseExpression(binary.Left); // Левая часть выражения
            var right = ParseExpression(binary.Right); // Правая часть выражения
            var op = GetSqlOperator(binary.NodeType); // Оператор (например, > или =)
            return $"({left} {op} {right})";
        }
        else if (expression is MemberExpression member)
        {
            return member.Member.Name; // Название свойства
        }
        else if (expression is ConstantExpression constant)
        {
            return FormatConstant(constant.Value); // Значение константы
        }

        // TODO: можно расширить для поддержки более сложных выражений (например, методов Contains, StartsWith и т.д.).
        // если не поддерживается то выбрасываем исключение
        throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
    }

    private string GetSqlOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Unsupported node type: {nodeType}")
        };
    }

    private string FormatConstant(object value)
    {
        return value is string ? $"'{value}'" : value.ToString();
    }

    private string BuildSqlQuery(Expression<Func<T, bool>> predicate, bool singleResult)
    {
        var tableName = typeof(T).Name + "s"; // Имя таблицы, основанное на имени класса
        var whereClause = ParseExpression(predicate.Body);
        var limitClause = singleResult ? "TOP 1" : string.Empty;

        return $"SELECT * FROM {tableName} WHERE {whereClause} {limitClause}".Trim(); // TODO
    }

    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        var sqlQuery = BuildSqlQuery(predicate, singleResult: true);
        return ExecuteQuerySingle(sqlQuery);
    }

    public IEnumerable<T> Where(string query)
    {
        string sqlQuery = "SELECT * FROM Users WHERE " + query;
        return ExecuteQueryMultiple(sqlQuery);
    }

    public T ExecuteQuerySingle(string query)
    {
        using (var command = _dbConnection.CreateCommand()) //_connection.CreateCommand())
        {
            command.CommandText = query;
            _dbConnection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Map(reader);
                }
            }

            _dbConnection.Close();
        }

        return null;
    }

    public IEnumerable<T> ExecuteQueryMultiple(string query)
    {
        var results = new List<T>();
        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = query;
            _dbConnection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(Map(reader));
                }
            }

            _dbConnection.Close();
        }

        return results;
    }

    public IEnumerable<T> Where2(string request)
    {
        List<T> list = new List<T>();
        string query =
            $"SELECT * FROM {typeof(T).Name}s WHERE {request}"; // Используем имя класса в качестве имени таблицы

        _dbConnection.Open();

        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = query;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    list.Add(Map(reader));
                }
            }
        }

        return list;
    }
}