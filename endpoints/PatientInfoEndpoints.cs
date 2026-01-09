using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthMetrics.Endpoints;
public static class PatientEndpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/patients");

        group.MapGet("/",GetAllPatients);
        group.MapGet("/{patientId:int}", GetPatientById);
        group.MapPatch("/{patientId:int}",UpdatePartialPatient); 
        group.MapPost("",AddPatient);
        group.MapDelete("/{patientId:int}",RemovePatient);
    }
    private static async Task<IResult> GetAllPatients(
        HealthMetricsDb healthMetricsDb,
        string? sortBy = "name",
        bool ascending = true,
        int page = 1,
        int pageSize = 10)
    {
        IQueryable<PatientInfo> query = healthMetricsDb.PatientInfos;

        query = sortBy?.ToLower() switch
        {
            "age" => ascending ? query.OrderBy(p => p.Age) : query.OrderByDescending(p => p.Age),
            "name" => ascending ? query.OrderBy(p => p.FirstName) : query.OrderByDescending(p => p.FirstName),
            _ => query.OrderBy(p => p.FirstName)
        };

        var totalCount = await query.CountAsync();
        var patients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new 
        { 
            Data = patients,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
    static async Task<IResult> GetPatientById(int patientId, [FromServices] HealthMetricsDb healthMetricsDb)
    {
        return await healthMetricsDb.PatientInfos.FindAsync(patientId)
            is PatientInfo patientInfo                          //is patient or null?
                ? TypedResults.Ok(patientInfo)                  //if patient
                : TypedResults.NotFound();    //if null
    }
    static async Task<IResult> AddPatient(PatientInfoDTO infoDTO, [FromServices] HealthMetricsDb healthMetricsDb)
    {
        if (infoDTO.DOB.HasValue && infoDTO.DOB.Value > DateTime.Now)
            return TypedResults.BadRequest("Date of birth cannot be in the future");
        if (infoDTO.DOB.HasValue && infoDTO.DOB.Value < new DateTime(1850, 1, 1))
            return TypedResults.BadRequest("Date of birth seems unrealistic");
        
        PatientInfo info = new();
        Helpers.MapParameters(infoDTO,info);
        healthMetricsDb.PatientInfos.Add(info);
        await healthMetricsDb.SaveChangesAsync();

        return TypedResults.Created(info);
    }
    static async Task<IResult> UpdatePartialPatient(int patientId, PatientInfoDTO updates, [FromServices] HealthMetricsDb healthMetricsDb)
    {
        if (updates is null) return TypedResults.BadRequest("PatientInfoDTO is null");
        if (updates.DOB.HasValue && updates.DOB.Value > DateTime.Now)
        return TypedResults.BadRequest("Date of birth cannot be in the future");
    
        if (updates.DOB.HasValue && updates.DOB.Value < new DateTime(1850, 1, 1))
            return TypedResults.BadRequest("Date of birth seems unrealistic");

        var info = await healthMetricsDb.PatientInfos.FindAsync(patientId);
        if (info is null) return TypedResults.NotFound();

        Helpers.MapParameters(updates,info);

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(info);
    }
    static async Task<IResult> RemovePatient(int patientId, 
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        var info = await healthMetricsDb.PatientInfos.FindAsync(patientId);
        if (info is null)
            return TypedResults.NotFound();
        
        info.DeletedAt = DateTime.UtcNow;

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}