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
    static async Task<IResult> GetPatientVitals(int patientId,
        [FromServices] HealthMetrics healthMetricsDb,
        string? sortBy = "date",
        bool ascending = true,
        int page = 1,
        int pageSize = 10)
    {
        if (await healthMetricsDb.PatientInfos.AnyAsync(patientId) is null)
            return TypedResults.NotFound();
        
        IQueryable<VitalsEntry> query = healthMetricsDb.VitalsEntries
            .Where(v => v.PatientId == patientId);
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(p => p.DateTaken) : query.OrderByDescending(p => p.DateTaken),
            _ => query.OrderBy(p => p.DateTaken)
        };

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
        VitalsEntryDTO vitalsDTO,
        [FromServices] HealthMetrics healthMetricsDb)
    {
        if (await healthMetricsDb.PatientInfos.AnyAsync(patientId) is null)
            return TypedResults.NotFound();
        
        if (vitalsDTO is null) return TypedResults.BadRequest("VitalsDTO is null");
        if (vitalsDTO.DateTaken is null)
            return TypedResults.BadRequest("Must provide a date");
        if (vitalsDTO.DateTaken > DateTime.Now)
            return TypedResults.BadRequest("Date taken date cannot be in the future");

        VitalsEntry entry = new()
        {
            PatientId = patientId,
            DateTaken = vitalsDTO.DateTaken.Value
        };

        Helpers.MapParameters(vitalsDTO,entry);
        healthMetricsDb.VitalsEntries.Add(entry);
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(entry);
    }
    static async Task<IResult> UpdatePartialVitals(int vitalsId,
        VitalsEntryDTO updates,
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        if (updates is null) return TypedResults.BadRequest("VitalsDTO is null");
        if (updates.DateTaken.HasValue && updates.DateTaken.Value > DateTime.Now)
            return TypedResults.BadRequest("Taken date cannot be in the future");

        VitalsEntry? entry = await healthMetricsDb.VitalsEntries.AnyAsync(vitalsId);
        if (entry is null) return
            TypedResults.NotFound();

        Helpers.MapParameters(updates,entry);

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(entry);
    }
    static async Task<IResult> RemoveVitalsEntry(int vitalsId,
    [FromServices] HealthMetricsDb healthMetricsDb)
    {
        VitalsEntry? entry = await healthMetricsDb.VitalsEntries.AnyAsync(vitalsId);
        if (entry is null)
            return TypedResults.NotFound();
        
        entry.DeletedAt = DateTime.UtcNow;

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
