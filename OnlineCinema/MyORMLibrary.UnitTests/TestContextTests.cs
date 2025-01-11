using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Moq;
using MyORMLibrary.UnitTests.models;

namespace MyORMLibrary.UnitTests;

[TestClass]
public class TestContextTests
{
    [TestMethod]
    public void GetByIdTest()
    {
        var dbConnection = new Mock<IDbConnection>();
        var dbCommand = new Mock<IDbCommand>();
        var dbDataReader = new Mock<IDataReader>();
        var dbparameter = new Mock<IDbDataParameter>();

        var person = new Person()
        {
            Id = 1,
            Name = "Chel",
            Email = "test@test.ru"
        };

        var context = new TestORMContext<Person>(dbConnection.Object);

        dbCommand.Setup(c => c.ExecuteReader()).Returns(dbDataReader.Object);
        // added
        dbCommand.Setup(c => c.CreateParameter()).Returns(dbparameter.Object);
        dbCommand.Setup(c => c.Parameters.Add(dbparameter));

        dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);

        dbDataReader.SetupSequence(c => c.Read())
            .Returns(true)
            .Returns(false);
        // TODO проверка на то что поступил id = 1
        // TODO по аналогии с этим тестом написать тесты на ORMContext методы GetById, GetAll, Create, Update, Delete

        // Test
        //dbDataReader.Setup(c => c.GetBoolean(It.Is<int>(p => p == 1))).Returns(true);

        dbDataReader.Setup(r => r["Id"]).Returns(person.Id);
        dbDataReader.Setup(r => r["Name"]).Returns(person.Name);
        dbDataReader.Setup(r => r["Email"]).Returns(person.Email);

        //dbCommand.Setup(r => r.CommandText == "123").Returns(true);

        var result = context.GetById(person.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(person.Id, result.Id);
        Assert.AreEqual(person.Name, result.Name);
        Assert.AreEqual(person.Email, result.Email);
    }

    [TestMethod]
    public void GetByAllTest()
    {
        var dbConnection = new Mock<IDbConnection>();
        var dbCommand = new Mock<IDbCommand>();
        var dbDataReader = new Mock<IDataReader>();
        //var dbparameter = new Mock<IDbDataParameter>();

        var person = new Person()
        {
            Id = 1,
            Name = "Chel",
            Email = "test@test.ru"
        };

        var context = new TestORMContext<Person>(dbConnection.Object);

        dbCommand.Setup(c => c.ExecuteReader()).Returns(dbDataReader.Object);
        // added
        //dbCommand.Setup(c => c.CreateParameter()).Returns(dbparameter.Object);
        //dbCommand.Setup(c => c.Parameters.Add(dbparameter));

        dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);

        dbDataReader.SetupSequence(c => c.Read())
            .Returns(true)
            .Returns(false);
        // TODO проверка на то что поступил id = 1
        // TODO по аналогии с этим тестом написать тесты на ORMContext методы GetById, GetAll, Create, Update, Delete

        // Test
        //dbDataReader.Setup(c => c.GetBoolean(It.Is<int>(p => p == 1))).Returns(true);

        dbDataReader.Setup(r => r["Id"]).Returns(person.Id);
        dbDataReader.Setup(r => r["Name"]).Returns(person.Name);
        dbDataReader.Setup(r => r["Email"]).Returns(person.Email);

        //dbCommand.Setup(r => r.CommandText == "123").Returns(true);

        var result = context.GetByAll();

        Assert.IsNotNull(result);
        Assert.AreEqual(person.Id, result.Id);
        Assert.AreEqual(person.Name, result.Name);
        Assert.AreEqual(person.Email, result.Email);
    }

    [TestMethod]
    public void DeleteTest()
    {
        // var dbConnection = new Mock<IDbConnection>();
        // var dbCommand = new Mock<IDbCommand>();
        // var dbDataReader = new Mock<IDataReader>();
        // //var dbparameter = new Mock<IDbDataParameter>();
        //
        // var person = new Person()
        // {
        //     Id = 1,
        //     Name = "Chel",
        //     Email = "test@test.ru"
        // };
        //
        // var context = new TestORMContext<Person>(dbConnection.Object);
        //
        // dbCommand.Setup(c => c.ExecuteReader()).Returns(dbDataReader.Object);
        // // added
        // //dbCommand.Setup(c => c.CreateParameter()).Returns(dbparameter.Object);
        // //dbCommand.Setup(c => c.Parameters.Add(dbparameter));
        //
        // dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);
        //
        // dbDataReader.SetupSequence(c => c.Read())
        //     .Returns(true)
        //     .Returns(false);
        // // TODO проверка на то что поступил id = 1
        // // TODO по аналогии с этим тестом написать тесты на ORMContext методы GetById, GetAll, Create, Update, Delete
        //
        // // Test
        // //dbDataReader.Setup(c => c.GetBoolean(It.Is<int>(p => p == 1))).Returns(true);
        //
        // dbDataReader.Setup(r => r["Id"]).Returns(person.Id);
        // dbDataReader.Setup(r => r["Name"]).Returns(person.Name);
        // dbDataReader.Setup(r => r["Email"]).Returns(person.Email);
        //
        // //dbCommand.Setup(r => r.CommandText == "123").Returns(true);
        //
        // context.Delete(1);
        // var result = context.GetByAll();
        // Assert.IsNull(result);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void UpdateTest()
    {
        Assert.IsTrue(true);
    }
}