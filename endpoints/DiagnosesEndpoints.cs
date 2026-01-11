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
    static ValidationError? ValidateDiagnosis(DiagnosisDTO diagnosisDTO)
    {
        if (diagnosisDTO.DiagnosisDate is null)
            return new ValidationError("Must provide a diagnosis date", "Date");
        if (diagnosisDTO.DiagnosisDate > DateTime.Now)
            return new ValidationError("Diagnosis date cannot be in the future", "Date");
        if (diagnosisDTO.ResolvedDate.HasValue && diagnosisDTO.ResolvedDate > DateTime.Now)
            return new ValidationError("Resolve date cannot be in the future", "Date");
        return null;
    }
    static async Task<IResult> GetPatientDiagnoses(int patientId,
        [FromServices] HealthMetricsDb healthMetricsDb,
        string? sortBy = "name",
        bool ascending = true,
        int page = 1,
        int pageSize = 10)
    {
        //limit pages
        if (pageSize > 25 || pageSize <= 0)
            return TypedResults.BadRequest("Page size must be 1-25");
        if (page <= 0)
            return TypedResults.BadRequest("Page must be >= 1");
        //check if patient exists
        if (await healthMetricsDb.Patients.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        //get associated
        IQueryable<Diagnosis> query = healthMetricsDb.Diagnoses
            .Where(d => d.PatientId == patientId);
        //sort
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(p => p.DiagnosisDate) : query.OrderByDescending(p => p.DiagnosisDate),
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };
        //pagination
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
        List<DiagnosisDTO> diagnosisDTOs,
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //limit dtos sent
        if (diagnosisDTOs.Count <= 0) return TypedResults.BadRequest("Must provide at least one DTO");
        if (diagnosisDTOs.Count > 100) return TypedResults.BadRequest("Only provide up to 10 DTOs at once");
        //check if patient exists
        if (await healthMetricsDb.Patients.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        //loop through sent dtos
        foreach (DiagnosisDTO diagnosisDTO in diagnosisDTOs) 
        {
            //validate
            if (ValidateDiagnosis(diagnosisDTO) is ValidationError err)
                return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
            //create diagnosis
            Diagnosis diagnosis = new()
            {
                Name = diagnosisDTO.Name,
                PatientId = patientId,
                DiagnosisDate = diagnosisDTO.DiagnosisDate,
                Status = diagnosisDTO.Status
            };
            Helpers.MapParameters(diagnosisDTO,diagnosis);
            healthMetricsDb.Diagnoses.Add(diagnosis);
        }
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok();
    }
    static async Task<IResult> UpdatePartialDiagnosis(int diagnosisId,
        DiagnosisDTO updates,
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //validation
        if (ValidateDiagnosis(updates) is ValidationError err)
            return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
        var diagnosis = await healthMetricsDb.Diagnoses.FindAsync(diagnosisId);
        if (diagnosis is null) return TypedResults.NotFound();
        //update
        Helpers.MapParameters(updates,diagnosis);
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(diagnosis);
    }
    static async Task<IResult> RemoveDiagnosis(int diagnosisId,
    [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //check if diagnosis exists
        var diagnosis = await healthMetricsDb.Diagnoses.FindAsync(diagnosisId);
        if (diagnosis is null)
            return TypedResults.NotFound();
        //mark as deleted
        diagnosis.DeletedAt = DateTime.UtcNow;
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
