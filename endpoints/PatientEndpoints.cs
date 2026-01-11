using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YamlDotNet.Core.Tokens;
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
    static ValidationError? ValidatePatientDTO(PatientDTO patientDTO)
    {
        if (patientDTO.DOB.HasValue && patientDTO.DOB.Value > DateTime.Now)
            return new ValidationError("Date of birth cannot be in the future", "DOB");
        if (patientDTO.DOB.HasValue && patientDTO.DOB.Value < new DateTime(1850, 1, 1))
            return new ValidationError("Date of birth seems unrealistic", "DOB");

        return null;
    }
    private static async Task<IResult> GetAllPatients(
        HealthMetricsDb healthMetricsDb,
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
        //get patients
        IQueryable<Patient> query = healthMetricsDb.Patients;
        //sort
        query = sortBy?.ToLower() switch
        {
            "age" => ascending ? query.OrderBy(p => p.Age) : query.OrderByDescending(p => p.Age),
            "name" => ascending ? query.OrderBy(p => p.FirstName) : query.OrderByDescending(p => p.FirstName),
            _ => query.OrderBy(p => p.FirstName)
        };
        //pagination
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
        return await healthMetricsDb.Patients.FindAsync(patientId)
            is Patient patient                //is patient or null?
                ? TypedResults.Ok(patient)    //if patient ok
                : TypedResults.NotFound();    //if null not found
    }
    static async Task<IResult> AddPatient(List<PatientDTO> patientDTOs, [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //limit dtos sent
        if (patientDTOs.Count <= 0) return TypedResults.BadRequest("Must provide at least one DTO");
        if (patientDTOs.Count > 100) return TypedResults.BadRequest("Only provide up to 10 DTOs at once");
        //loop through sent dtos
        foreach (PatientDTO patientDTO in patientDTOs)
        {
            //validate
            if (ValidatePatientDTO(patientDTO) is ValidationError err)
                return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
            //create
            Patient patient = new();
            Helpers.MapParameters(patientDTO,patient);
            healthMetricsDb.Patients.Add(patient);
        }
        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Created();
    }
    static async Task<IResult> UpdatePartialPatient(int patientId, PatientDTO updates, [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //validate
        if (ValidatePatientDTO(updates) is ValidationError err)
            return TypedResults.BadRequest(new {message = err.Message, field = err.Field});
        //locate
        var patient = await healthMetricsDb.Patients.FindAsync(patientId);
        if (patient is null) return TypedResults.NotFound();
        //update
        Helpers.MapParameters(updates,patient);

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.Ok(patient);
    }
    static async Task<IResult> RemovePatient(int patientId, 
        [FromServices] HealthMetricsDb healthMetricsDb)
    {
        //check if patient exists
        var patient = await healthMetricsDb.Patients.FindAsync(patientId);
        if (patient is null)
            return TypedResults.NotFound();
        //mark as deleted
        patient.DeletedAt = DateTime.UtcNow;

        await healthMetricsDb.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}