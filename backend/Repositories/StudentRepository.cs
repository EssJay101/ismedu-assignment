using System.Data;
using Dapper;
using Ismedu.Api.Data;
using Ismedu.Api.DTOs;
using Ismedu.Api.Models;

namespace Ismedu.Api.Repositories;

public interface IStudentRepository
{
    Task<(IEnumerable<Student> Items, int TotalCount)> GetPagedAsync(StudentQuery query);
    Task<Student?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<bool> MobileExistsAsync(string mobile, int? excludeId = null);
    Task<int> CreateAsync(Student student);
    Task<bool> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
}

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnectionFactory _db;

    public StudentRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<Student> Items, int TotalCount)> GetPagedAsync(StudentQuery query)
    {
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        using var connection = _db.CreateConnection();
        using var results = await connection.QueryMultipleAsync(
            "sp_Student_GetPaged",
            new
            {
                p_Search = search,
                p_CountryId = query.CountryId,
                p_StateId = query.StateId,
                p_CityId = query.CityId,
                p_Page = query.Page,
                p_PageSize = query.PageSize
            },
            commandType: CommandType.StoredProcedure);

        var totalCount = await results.ReadSingleAsync<int>();
        var items = (await results.ReadAsync<Student>()).ToList();

        return (items, totalCount);
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Student>(
            "sp_Student_GetById",
            new { p_StudentId = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            "sp_Student_EmailExists",
            new { p_Email = email, p_ExcludeId = excludeId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> MobileExistsAsync(string mobile, int? excludeId = null)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            "sp_Student_MobileExists",
            new { p_Mobile = mobile, p_ExcludeId = excludeId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(Student student)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "sp_Student_Insert",
            ToParameters(student, includeId: false),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_Student_Update",
            ToParameters(student, includeId: true),
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_Student_Delete",
            new { p_StudentId = id },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    // Insert and Update send the same 9 values; Update also sends the ID
    private static DynamicParameters ToParameters(Student student, bool includeId)
    {
        var parameters = new DynamicParameters();

        if (includeId)
        {
            parameters.Add("p_StudentId", student.StudentId);
        }

        parameters.Add("p_FirstName", student.FirstName);
        parameters.Add("p_LastName", student.LastName);
        parameters.Add("p_DateOfBirth", student.DateOfBirth.Date, DbType.Date);
        parameters.Add("p_Email", student.Email);
        parameters.Add("p_Mobile", student.Mobile);
        parameters.Add("p_CountryId", student.CountryId);
        parameters.Add("p_StateId", student.StateId);
        parameters.Add("p_CityId", student.CityId);
        parameters.Add("p_Address", student.Address);

        return parameters;
    }
}