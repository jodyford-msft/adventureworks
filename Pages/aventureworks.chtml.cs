using Microsoft.AspNetCore.Mvc.RazorPages;  
using System;  
using System.Collections.Generic;  
using System.Data.SqlClient;  

public class AdventureWorksModel : PageModel  
{  
    public List<Person> People { get; set; } = new List<Person>();  

    public void OnGet()  
    {  
        string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");  

        if (string.IsNullOrEmpty(connectionString))  
        {  
            throw new Exception("Connection string not found in environment variables.");  
        }  

        try  
        {  
            using (SqlConnection connection = new SqlConnection(connectionString))  
            {  
                connection.Open();  
                string query = "SELECT TOP 10 FirstName, LastName FROM Person.Person";  

                using (SqlCommand command = new SqlCommand(query, connection))  
                using (SqlDataReader reader = command.ExecuteReader())  
                {  
                    while (reader.Read())  
                    {  
                        People.Add(new Person  
                        {  
                            FirstName = reader["FirstName"].ToString(),  
                            LastName = reader["LastName"].ToString()  
                        });  
                    }  
                }  
            }  
        }  
        catch (Exception ex)  
        {  
            throw new Exception($"An error occurred: {ex.Message}");  
        }  
    }  
}  

public class Person  
{  
    public string FirstName { get; set; }  
    public string LastName { get; set; }  
}  
