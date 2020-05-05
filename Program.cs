using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace SwiftKick.DapperDemos
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var sqlConnectionString = "Server=.;Database=GriffCoUnlimited;Trusted_Connection=True;";

            // DEMO 1: QueryAsync<> 
            //await DemoWithQueryAsync(sqlConnectionString);

            // DEMO 2: QueryAsync<> with Parameters
            //await DemoWithQueryAsyncWithParameters(sqlConnectionString);
            await DemoWithQueryAsyncWithDynamicParameters(sqlConnectionString);

            // DEMO 3: QueryFirst / QueryFirstOrDefault
            //await DemoWithQueryFirst(sqlConnectionString);

            // DEMO 4: INSERT/UPDATE/DELETE with ExecuteAsync
            //await DemoExecuteAsyncWithInsert(sqlConnectionString);
            //await DemoExecuteAsyncWithUpdate(sqlConnectionString);
            //await DemoExecuteAsyncWithDelete(sqlConnectionString);

            // DEMO 5: Executing stored procedures
            //await DemoStoredProcedure(sqlConnectionString);

            // DEMO 6: Working with Transactions
            //await DemoTransactions(sqlConnectionString);

            // DEMO 7: Dynamic results
            //await DemoDynamicResults(sqlConnectionString);

            // DEMO 8: Mapping 
            // DEMO 9: Multi-mapping
            // DEMO 10: Multi-results
        }

        #region Helper methods
        private static void WriteList(IEnumerable<User> result)
        {
            foreach (var r in result)
            {
                Console.WriteLine($"{r.FirstName}\t\t{r.LastName}\t\t{r.DateOfBirth}");
            }
        }

        private static void WriteList(IEnumerable<dynamic> result)
        {
            foreach (var r in result)
            {
                Console.WriteLine($"{r.FirstName}\t\t{r.LastName}\t\t{r.DateOfBirth}");
            }
        }

        private static void WriteList(User result)
        {
            Console.WriteLine($"{result.FirstName}\t\t{result.LastName}\t\t{result.DateOfBirth}");
        }
        #endregion

        private static async Task DemoWithQueryAsync(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"SELECT [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[PhoneNumber]
                              ,[UserName]
                              ,[DateOfBirth]
                              ,[City]
                              ,[State]
                              ,[ZipCode]
                          FROM [dbo].[MassiveUserList]";
            var result = await connection.QueryAsync<User>(sql);
            WriteList(result);
        }

        private static void DemoWithQueryAsyncNotBuffered(string sqlConnectionString)
        {
            using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"SELECT [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[PhoneNumber]
                              ,[UserName]
                              ,[DateOfBirth]
                              ,[City]
                              ,[State]
                              ,[ZipCode]
                          FROM [dbo].[MassiveUserList]";
            var result = connection.Query<User>(sql, buffered: false);
            WriteList(result);
        }

        private static async Task DemoWithQueryAsyncWithParameters(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"SELECT [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[PhoneNumber]
                              ,[UserName]
                              ,[DateOfBirth]
                              ,[City]
                              ,[State]
                              ,[ZipCode]
                          FROM [dbo].[MassiveUserList]
                          WHERE DateOfBirth > @dateOfBirth";

            var result = await connection.QueryAsync<User>(sql,
                new { dateOfBirth = new DateTime(2005, 10, 01) });
            WriteList(result);
        }

        private static async Task DemoWithQueryAsyncWithDynamicParameters(string sqlConnectionString)
        {
            // criteria
            var searchFirstName = "Harry";
            var searchLastName = "Pennington";

            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"SELECT [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[PhoneNumber]
                              ,[UserName]
                              ,[DateOfBirth]
                              ,[City]
                              ,[State]
                              ,[ZipCode]
                          FROM [dbo].[MassiveUserList] ";

            var whereClauses = new List<string>();
            var dynamicParam = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchFirstName))
            {
                whereClauses.Add("FirstName = @firstName");
                dynamicParam.Add("firstName", searchFirstName);
            }

            if (!string.IsNullOrWhiteSpace(searchLastName))
            {
                whereClauses.Add("LastName = @lastName");
                dynamicParam.Add("lastName", searchLastName);
            }

            if (whereClauses.Any())
                sql += " WHERE " + string.Join(" AND ", whereClauses);

            var result = await connection.QueryAsync<User>(sql, dynamicParam);

            WriteList(result);
        }

        private static async Task DemoWithQueryFirst(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"SELECT TOP 1 [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[PhoneNumber]
                              ,[UserName]
                              ,[DateOfBirth]
                              ,[City]
                              ,[State]
                              ,[ZipCode]
                          FROM [dbo].[MassiveUserList]";

            var result = await connection.QueryFirstAsync<User>(sql);
            WriteList(result);
        }

        private static async Task DemoExecuteAsyncWithInsert(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"INSERT INTO [dbo].[MassiveUserList]
                               ([FirstName]
                               ,[LastName]
                               ,[PhoneNumber]
                               ,[UserName]
                               ,[DateOfBirth]
                               ,[City]
                               ,[State]
                               ,[ZipCode])
                         VALUES
                               (@firstName, @lastName, @phoneNumber, 
                                @userName, @dateOfBirth,
                                @city, @state, @zipCode)";

            var newUser = new User()
            {
                FirstName = "Jim",
                LastName = "Bob",
                City = "Chesapeake",
                State = "Virginia",
                ZipCode = "23320",
                DateOfBirth = DateTime.Parse("1999-01-01"),
                PhoneNumber = "757-867-5309",
                UserName = "COMPUTER/JimBob"
            };

            var result = await connection.ExecuteAsync(sql, newUser);
        }

        private static async Task DemoExecuteAsyncWithUpdate(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"UPDATE [dbo].[MassiveUserList]
                        SET DateOfBirth = @dateOfBirth
                        WHERE FirstName = @firstName";

            var result = await connection.ExecuteAsync(sql,
                new { dateOfBirth = DateTime.Parse("2009-01-01"), firstName = "Jim" });
        }

        private static async Task DemoExecuteAsyncWithDelete(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"DELETE FROM [dbo].[MassiveUserList]                        
                        WHERE LastName = @lastName";

            var result = await connection.ExecuteAsync(sql, new { lastName = "Bob" });
        }


        private static async Task DemoStoredProcedure(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = "dbo.LastNameList";

            var results = await connection.QueryAsync<string>(sql, commandType: CommandType.StoredProcedure);
        }

        private static async Task DemoTransactions(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();
            await using var transaction = connection.BeginTransaction();

            var sql = @"INSERT INTO [dbo].[MassiveUserList]
                               ([FirstName]
                               ,[LastName]
                               ,[PhoneNumber]
                               ,[UserName]
                               ,[DateOfBirth]
                               ,[City]
                               ,[State]
                               ,[ZipCode])
                         VALUES
                               (@firstName, @lastName, @phoneNumber, 
                                @userName, @dateOfBirth,
                                @city, @state, @zipCode)";

            var newUser = new User()
            {
                FirstName = "Jim",
                LastName = "Bob",
                City = "Chesapeake",
                State = "Virginia",
                ZipCode = "23320",
                DateOfBirth = DateTime.Parse("1999-01-01"),
                PhoneNumber = "757-867-5309",
                UserName = "COMPUTER/JimBob"
            };

            for (var x = 0; x < 10000; x++)
            {
                var result = await connection.ExecuteAsync(sql, newUser, transaction);
                newUser.DateOfBirth = newUser.DateOfBirth.AddDays(1);
            }

            transaction.Commit();
        }

        private static async Task DemoDynamicResults(string sqlConnectionString)
        {
            await using var connection = new SqlConnection(sqlConnectionString);
            var sql = @"SELECT [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[PhoneNumber]
                              ,[UserName]
                              ,[DateOfBirth]
                              ,[City]
                              ,[State]
                              ,[ZipCode]
                          FROM [dbo].[MassiveUserList]";

            var result = await connection.QueryAsync(sql);
            WriteList(result);
        }
    }
}
