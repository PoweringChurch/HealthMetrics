using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "HealthcareAPI";
    config.Title = "HealthcareAPI v1";
    config.Version = "v1";
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "HealthcareAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var patientDetails = app.MapGroup("/patientDetails");

patientDetails.MapGet("/",GetAllPatientDetails);
patientDetails.MapGet("/{id}", GetPatientInfo);

patientDetails.MapPost("/",AddPatientInfo);

patientDetails.MapPut("/",ReplacePatientInfo);
/*
patientDetails.MapPatch("/{id}",UpdatePartialPatientInfo);
*/
patientDetails.MapDelete("/{id}",RemovePatientInfo);

app.Run();

static async Task<IResult> GetAllPatientDetails(PatientDetailsDb db)
{
    return TypedResults.Ok(await db.PatientDetails.ToArrayAsync());
}

static async Task<IResult> GetPatientInfo(int patientId, PatientDetailsDb db)
{
    return await db.PatientDetails.FindAsync(patientId)
        is PatientInfo patientInfo                          //is patient or null?
            ? TypedResults.Ok(patientInfo)                  //if patient
            : TypedResults.NotFound("No patient found");    //if null
}

static async Task<IResult> AddPatientInfo(PatientInfo info,PatientDetailsDb db)
{
    db.PatientDetails.Add(info);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/patientDetails/{info.PatientId}", info);
}

static async Task<IResult> UpdatePartialPatientInfo(int id, JsonPatchDocument<PatientInfo> inputInfo, PatientDetailsDb db)
{
    if (inputInfo is null) return TypedResults.BadRequest();

    var info = await db.PatientDetails.FindAsync(id);
    if (info is null) return TypedResults.NotFound();

    var hasErrors = false;
    inputInfo.ApplyTo(info,error => hasErrors = true);

    if (hasErrors) return TypedResults.BadRequest("Failed to apply patch");

    await db.SaveChangesAsync();
    return TypedResults.Ok(info);
    
}

static async Task<IResult> ReplacePatientInfo(int id, PatientInfo inputInfo, PatientDetailsDb db)
{
    if (inputInfo is null) return TypedResults.BadRequest();

    var info = await db.PatientDetails.FindAsync(id);
    if (info is null) return TypedResults.NotFound();

    info.FirstName = inputInfo.FirstName;
    info.MiddleName = inputInfo.MiddleName;
    info.LastName = inputInfo.LastName;
    info.PreferredName = inputInfo.PreferredName;

    info.DOB = inputInfo.DOB;
    
    info.Sex = inputInfo.Sex;
    info.Gender = inputInfo.Gender;
    
    info.HomeAddress = inputInfo.HomeAddress;
    info.PhoneNumber = inputInfo.PhoneNumber;
    info.AdditionalInfo = inputInfo.AdditionalInfo;

    info.VitalsHistory = inputInfo.VitalsHistory;
    info.MedicationHistory = inputInfo.MedicationHistory;
    info.DiagnosesHistory = inputInfo.DiagnosesHistory;

    await db.SaveChangesAsync();

    return Results.NoContent();
}

static async Task<IResult> RemovePatientInfo(int patientId, PatientDetailsDb db)
{
    if (await db.PatientDetails.FindAsync(patientId) is PatientInfo info) //if we can find patient of id patientId
    {
        db.PatientDetails.Remove(info);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NotFound();
}