using Azure.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AdventureWorksModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly TokenCredential _tokenCredential;

    public AdventureWorksModel(IConfiguration configuration, TokenCredential tokenCredential)
    {
        _configuration = configuration;
        _tokenCredential = tokenCredential;
    }

    public List<Person> People { get; set; } = new List<Person>();

    public async Task OnGetAsync()
    {
        string? connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Connection string not found in configuration.");
        }

        try
        {
            var connection = new SqlConnection(connectionString)
            {
                AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/")
            };

            await connection.OpenAsync();
            string query = "SELECT TOP 10 FirstName, LastName FROM Person.Person";

            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    People.Add(new Person
                    {
                        FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                        LastName = reader["LastName"]?.ToString() ?? string.Empty
                    });
                }
            }
        }
        catch (SqlException ex)
        {
            // Log exception (ex) here
            throw new Exception("An error occurred while querying the database.", ex);
        }
    }
}

public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}