using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthMetrics.Endpoints;

public static class DiagnosesEndpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app) 
    {
        var group = app.MapGroup("/diagnosis");
        group.MapGet("/{patientId:int}",GetPatientDiagnoses);
        group.MapPost("/{patientId:int}",AddPatientDiagnosis);
        group.MapDelete("/{diagnosisId:int}",RemoveDiagnosis);
        group.MapPatch("/{diagnosisId:int}",UpdatePartialDiagnosis);
    }
    static async Task<IResult> GetPatientDiagnoses(int patientId,
        [FromServices] HealthMetricsDb healthMetricsDb,
        string? sortBy = "name",
        bool ascending = true,
        int page = 1,
        int pageSize = 10)
    {
        if (await healthMetricsDb.PatientInfos.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        
        IQueryable<Diagnosis> query = healthMetricsDb.Diagnoses
            .Where(d => d.PatientId == patientId);
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(p => p.DiagnosisDate) : query.OrderByDescending(p => p.DiagnosisDate),
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();
        var diagnoses = await query
            .Skip((page-1)*pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new 
        { 
            Data = diagnoses,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
    static async Task<IResult> AddPatientDiagnosis(int patientId,
        DiagnosisDTO diagnosisDTO,
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        if (await healthMetricsDb.PatientInfos.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        
        if (diagnosisDTO is null) return TypedResults.BadRequest("DiagnosisDTO is null");
        if (diagnosisDTO.DiagnosisDate is null)
            return TypedResults.BadRequest("Must provide a diagnosis date");
        if (diagnosisDTO.DiagnosisDate > DateTime.Now)
            return TypedResults.BadRequest("Diagnosis date cannot be in the future");
        if (diagnosisDTO.ResolvedDate.HasValue && diagnosisDTO.ResolvedDate > DateTime.Now)
            return TypedResults.BadRequest("Resolve date cannot be in the future");
        Diagnosis diagnosis = new()
        {
            Name = diagnosisDTO.Name,
            PatientId = patientId,
            DiagnosisDate = diagnosisDTO.DiagnosisDate.Value,
            Status = diagnosisDTO.Status
        };
        Helpers.MapParameters(diagnosisDTO,diagnosis);
        healthMetricsDb.Diagnoses.Add(diagnosis);
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(diagnosis);
    }
    static async Task<IResult> UpdatePartialDiagnosis(int diagnosisId,
        DiagnosisDTO updates,
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        if (updates is null) return TypedResults.BadRequest("PatientInfoDTO is null");
        if (updates.DiagnosisDate.HasValue && updates.DiagnosisDate.Value > DateTime.Now)
            return TypedResults.BadRequest("Diagnosis date cannot be in the future");
    
        if (updates.ResolvedDate.HasValue && updates.ResolvedDate.Value > DateTime.Now)
            return TypedResults.BadRequest("Resolve date cannot be in the future");

        var diagnosis = await healthMetricsDb.Diagnoses.FindAsync(diagnosisId);
        if (diagnosis is null) return TypedResults.NotFound();

        Helpers.MapParameters(updates,diagnosis);

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(diagnosis);
    }
    static async Task<IResult> RemoveDiagnosis(int diagnosisId,
    [FromServices] HealthMetricsDb healthMetricsDb)
    {
        var diagnosis = await healthMetricsDb.Diagnoses.FindAsync(diagnosisId);
        if (diagnosis is null)
            return TypedResults.NotFound();
        
        diagnosis.DeletedAt = DateTime.UtcNow;

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
