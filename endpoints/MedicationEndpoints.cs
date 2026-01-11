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
        group.MapPatch("/{medicationId:int}",UpdatePartialMedication);
    }
    static ValidationError? ValidateMedication(MedicationDTO medicationDTO)
    {
        if (medicationDTO.StartDate.HasValue && medicationDTO.StartDate.Value > DateTime.Now)
            return new ValidationError("Start date cannot be in the future", "Date");
        if (medicationDTO.EndDate.HasValue && medicationDTO.EndDate.Value > DateTime.Now)
            return new ValidationError("End date cannot be in the future", "Date");
        return null;
    }
    static async Task<IResult> GetPatientMedications(int patientId,
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
        IQueryable<Medication> query = healthMetricsDb.Medications
            .Where(m => m.PatientId == patientId);
        //sort
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };
        //pagination
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
    
    static async Task<IResult> AddPatientMedication(int patientId, List<MedicationDTO> medicationDTOs,[FromServices] HealthMetricsDb healthMetricsDb)
    {
        //limit dtos sent
        if (medicationDTOs.Count <= 0) return TypedResults.BadRequest("Must provide at least one DTO");
        if (medicationDTOs.Count > 100) return TypedResults.BadRequest("Only provide up to 10 DTOs at once");
        //check if patient exists
        if (await healthMetricsDb.Patients.FindAsync(patientId) is null)
            return TypedResults.NotFound();
        //loop through medicationDTOs sent
        foreach (MedicationDTO medicationDTO in medicationDTOs)
        {
            //validate
            if (ValidateMedication(medicationDTO) is ValidationError err)
                return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
            //create
            Medication medication = new()
            {
                Name = medicationDTO.Name,
                Dosage = medicationDTO.Dosage,
                Frequency = medicationDTO.Frequency,
                StartDate = medicationDTO.StartDate,
                PatientId = patientId
            };
            Helpers.MapParameters(medicationDTO,medication);

            healthMetricsDb.Medications.Add(medication);
        }
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok();
    }
    static async Task<IResult> UpdatePartialMedication(int medicationId, MedicationDTO updates, [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //validate
        if (ValidateMedication(updates) is ValidationError err)
            return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
        //locate
        var medication = await healthMetricsDb.Medications.FindAsync(medicationId);
        if (medication is null) 
            return TypedResults.NotFound();
        //update
        Helpers.MapParameters(updates,medication);

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(medication);
    }
    static async Task<IResult> RemoveMedication(int medicationId,
    [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //locate
        var medication = await healthMetricsDb.Medications.FindAsync(medicationId);
        if (medication is null)
            return TypedResults.NotFound();
        //mark as deleted
        medication.DeletedAt = DateTime.UtcNow;

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}