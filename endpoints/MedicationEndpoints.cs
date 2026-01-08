using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthMetrics.Endpoints;

public static class MedicationEndpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/medication");
        group.MapGet("/{patientId:int}",GetPatientMedications);
        group.MapPost("/{patientId:int}",AddPatientMedication);
        group.MapDelete("/{medicationId:int}",RemoveMedication);
        group.MapPatch("/{medicationId:int}",UpdatePartialVitals);
    }
    static async Task<IResult> GetPatientMedications(int patientId,
        [FromServices] PatientInfoDb patientDb,
        [FromServices] MedicationDb medicationDb,
        string? sortBy = "name",
        bool ascending = true,
        int page = 1,
        int pageSize = 10)
    
    {
        if (pageSize > 25 || pageSize <= 0)
            return TypedResults.BadRequest("Page size must be 1-25");
        if (page <= 0)
            return TypedResults.BadRequest("Page must be >= 1");
        
        if (await patientDb.PatientInfos.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        
        IQueryable<Medication> query = medicationDb.Medications
            .Where(m => m.PatientId == patientId);
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();
        var medications = await query
            .Skip((page-1)*pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new 
        { 
            Data = medications,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
    static async Task<IResult> UpdatePartialMedication(int medicationId, MedicationDTO updates, [FromServices] MedicationDb medicationDb)
    {
        if (updates is null) return TypedResults.BadRequest("PatientInfoDTO is null");
        if (updates.StartDate.HasValue && updates.StartDate.Value > DateTime.Now)
            return TypedResults.BadRequest("Start date cannot be in the future");
        if (updates.EndDate.HasValue && updates.EndDate.Value > DateTime.Now)
            return TypedResults.BadRequest("End date cannot be in the future");
        if 

        var medication = await medicationDb.Medications.FindAsync(medicationId);
        if (medication is null) 
            return TypedResults.NotFound();

        Helpers.MapParameters(updates,medication);

        await medicationDb.SaveChangesAsync();
        return TypedResults.Ok(medication);
    }
    static async Task<IResult> AddPatientMedication(int patientId,
        MedicationDTO medicationDTO,
        [FromServices] PatientInfoDb patientDb,
        [FromServices] MedicationDb medicationDb)
    {
        if (await patientDb.PatientInfos.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        
        if (medicationDTO is null) return TypedResults.BadRequest("MedicationDTO is null");
        if (medicationDTO.StartDate > DateTime.Now)
            return TypedResults.BadRequest("Start date cannot be in the future");
        if (medicationDTO.EndDate.HasValue && medicationDTO.EndDate > DateTime.Now)
            return TypedResults.BadRequest("End date cannot be in the future");

        Medication medication = new()
        {
            Name = medicationDTO.Name,
            Dosage = medicationDTO.Dosage,
            Frequency = medicationDTO.Frequency,
            StartDate = medicationDTO.StartDate,
            PatientId = patientId
        };
        Helpers.MapParameters(medicationDTO,medication);
        medicationDb.Medications.Add(medication);
        await medicationDb.SaveChangesAsync();
        return TypedResults.Ok(medication);
    }
    static async Task<IResult> RemoveMedication(int medicationId,
    [FromServices] MedicationDb medicationDb)
    {
        var medication = await medicationDb.Medications.FindAsync(medicationId);
        if (medication is null)
            return TypedResults.NotFound();
        medicationDb.Medications.Remove(medication);
        await medicationDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}