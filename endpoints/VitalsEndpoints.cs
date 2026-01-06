using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthMetrics.Endpoints;

//nts you need to make this not just be a copy paste of diagnoses, get to watch bebop lol
public static class VitalsEndpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app) 
    {
        var group = app.MapGroup("/vitals");
        group.MapGet("/fromvitals/{vitalsId:int}",GetVitalsFromId);
        group.MapGet("/frompatient/{patientId:int}",GetPatientVitals);
        group.MapPost("/{patientId:int}",AddPatientVitals);
        group.MapDelete("/{vitalsId:int}",RemoveDiagnosis);
        group.MapPatch("/{vitalsId:int}",UpdatePartialVitals);
    }
    static async Task<IResult> GetVitalsFromId(int vitalsId,
        [FromServices] VitalsDb vitalsDb)
    {
        VitalsEntry? entry = await vitalsDb.VitalsEntries.FindAsync(vitalsId);
        if (entry is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(entry);
    }
    static async Task<IResult> GetPatientVitals(int patientId,
        [FromServices] PatientInfoDb patientDb,
        [FromServices] VitalsDb vitalsDb,
        string? sortBy = "date",
        bool ascending = true,
        int page = 1,
        int pageSize = 10)
    {
        if (await patientDb.PatientInfos.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        
        IQueryable<VitalsEntry> query = vitalsDb.VitalsEntries
            .Where(m => m.PatientId == patientId);
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
        [FromServices] PatientInfoDb patientDb,
        [FromServices] VitalsDb vitalsDb)
    {
        if (await patientDb.PatientInfos.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        
        if (vitalsDTO is null) return TypedResults.BadRequest("DiagnosisDTO is null");
        if (vitalsDTO.DateTaken is null)
            return TypedResults.BadRequest("Must provide a date");
        if (vitalsDTO.DateTaken > DateTime.Now)
            return TypedResults.BadRequest("Diagnosis date cannot be in the future");

        VitalsEntry entry = new()
        {
            PatientId = patientId,
            DateTaken = vitalsDTO.DateTaken.Value
        };
        Helpers.MapParameters(vitalsDTO,entry);
        vitalsDb.VitalsEntries.Add(entry);
        await vitalsDb.SaveChangesAsync();
        return TypedResults.Ok(entry);
    }
    static async Task<IResult> UpdatePartialVitals(int vitalsId,
        VitalsEntryDTO updates,
        [FromServices] VitalsDb vitalsDb)
    {
        if (updates is null) return TypedResults.BadRequest("PatientInfoDTO is null");
        if (updates.DateTaken.HasValue && updates.DateTaken.Value > DateTime.Now)
            return TypedResults.BadRequest("Taken date cannot be in the future");

        VitalsEntry? entry = await vitalsDb.VitalsEntries.FindAsync(vitalsId);
        if (entry is null) return
            TypedResults.NotFound();

        Helpers.MapParameters(updates,entry);

        await vitalsDb.SaveChangesAsync();
        return TypedResults.Ok(entry);
    }
    static async Task<IResult> RemoveDiagnosis(int vitalsId,
    [FromServices] VitalsDb vitalsDb)
    {
        VitalsEntry? entry = await vitalsDb.VitalsEntries.FindAsync(vitalsId);
        if (entry is null)
            return TypedResults.NotFound();
        vitalsDb.VitalsEntries.Remove(entry);
        await vitalsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
