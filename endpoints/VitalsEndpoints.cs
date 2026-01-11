using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthMetrics.Endpoints;

public static class VitalsEndpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app) 
    {
        var group = app.MapGroup("/vitals");
        group.MapGet("/{patientId:int}",GetPatientVitals);
        group.MapPost("/{patientId:int}",AddPatientVitals);
        group.MapDelete("/{vitalsId:int}",RemoveVitalsEntry);
        group.MapPatch("/{vitalsId:int}",UpdatePartialVitals);
    }
    static ValidationError? ValidateVitals(VitalsEntryDTO vitalsDTO)
    {
        if (vitalsDTO.DateTaken is null)
            return new ValidationError("Must provide a date taken", "Date");
        if (vitalsDTO.DateTaken > DateTime.Now)
            return new ValidationError("Date taken cannot be in the future", "Date");
        return null;
    }
    static async Task<IResult> GetPatientVitals(int patientId,
        [FromServices] HealthMetricsDb healthMetricsDb,
        string? sortBy = "date",
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
        IQueryable<VitalsEntry> query = healthMetricsDb.VitalsEntries
            .Where(v => v.PatientId == patientId);
        //sort
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(p => p.DateTaken) : query.OrderByDescending(p => p.DateTaken),
            _ => query.OrderBy(p => p.DateTaken)
        };
        //paginate
        var totalCount = await query.CountAsync();
        var entries = await query
            .Skip((page-1)*pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new 
        { 
            Data = entries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
    static async Task<IResult> AddPatientVitals(int patientId,
        List<VitalsEntryDTO> vitalsDTOs,
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //limit dtos sent
        if (vitalsDTOs.Count <= 0) return TypedResults.BadRequest("Must provide at least one DTO");
        if (vitalsDTOs.Count > 100) return TypedResults.BadRequest("Only provide up to 10 DTOs at once");
        //check if patient exists
        if (await healthMetricsDb.Patients.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        //loop through dtos sent
        foreach (VitalsEntryDTO vitalsDTO in vitalsDTOs)
        {
            //validate
            if (ValidateVitals(vitalsDTO) is ValidationError err)
                return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
            //create
            VitalsEntry entry = new()
            {
                PatientId = patientId,
                DateTaken = vitalsDTO.DateTaken
            };
            Helpers.MapParameters(vitalsDTO,entry);
            healthMetricsDb.VitalsEntries.Add(entry);
        }
        
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok();
    }
    static async Task<IResult> UpdatePartialVitals(int vitalsId,
        VitalsEntryDTO updates,
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //validate
        if (ValidateVitals(updates) is ValidationError err)
            return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
        //check if entry exists
        VitalsEntry? entry = await healthMetricsDb.VitalsEntries.FindAsync(vitalsId);
        if (entry is null) return
            TypedResults.NotFound();
        //update
        Helpers.MapParameters(updates,entry);

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(entry);
    }
    static async Task<IResult> RemoveVitalsEntry(int vitalsId,
    [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //check if entry exists
        VitalsEntry? entry = await healthMetricsDb.VitalsEntries.FindAsync(vitalsId);
        if (entry is null)
            return TypedResults.NotFound();
        //mark as deleted
        entry.DeletedAt = DateTime.UtcNow;

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
