using Ismedu.Api.DTOs;
using Ismedu.Api.Models;
using Ismedu.Api.Repositories;

namespace Ismedu.Api.Data;

// Adds 3 sample students when the Students table is empty (development only).
public static class SampleStudentSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var students = scope.ServiceProvider.GetRequiredService<IStudentRepository>();

        var (_, totalCount) = await students.GetPagedAsync(new StudentQuery { Page = 1, PageSize = 1 });
        if (totalCount > 0)
        {
            return;
        }

        var samples = new[]
        {
            new Student
            {
                FirstName = "Rahul", LastName = "Sharma", DateOfBirth = new DateTime(2003, 5, 14),
                Email = "rahul.sharma@example.com", Mobile = "9876543210",
                CountryId = 1, StateId = 1, CityId = 1, Address = "12 Marine Drive, Colaba, Mumbai"
            },
            new Student
            {
                FirstName = "Priya", LastName = "Nair", DateOfBirth = new DateTime(2004, 11, 2),
                Email = "priya.nair@example.com", Mobile = "9123456780",
                CountryId = 1, StateId = 2, CityId = 3, Address = "45 MG Road, Indiranagar, Bengaluru"
            },
            new Student
            {
                FirstName = "Aditya", LastName = "D'Souza", DateOfBirth = new DateTime(2002, 8, 23),
                Email = "aditya.dsouza@example.com", Mobile = "8765432109",
                CountryId = 1, StateId = 1, CityId = 2, Address = "7 FC Road, Shivajinagar, Pune"
            }
        };

        foreach (var student in samples)
        {
            await students.CreateAsync(student);
        }

        logger.LogInformation("Added {Count} sample students.", samples.Length);
    }
}