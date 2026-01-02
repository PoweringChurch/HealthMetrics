using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<PatientDetailsDb>(options => 
    options.UseInMemoryDatabase("PatientDetails"));


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

patientDetails.MapGet("",GetAllPatientDetails);


var patient = patientDetails.MapGroup("/patient");
patient.MapGet("/{patientId:int}", GetPatientInfo);
patient.MapPatch("/{patientId:int}",UpdatePartialPatientInfo);
patient.MapPost("",AddPatientInfo);
patient.MapPut("",ReplacePatientInfo);
patient.MapDelete("/{patientId:int}",RemovePatientInfo);

app.Run();

static async Task<IResult> GetAllPatientDetails([FromServices] PatientDetailsDb db)
{
    return TypedResults.Ok(await db.PatientDetails.ToArrayAsync());
}

static async Task<IResult> GetPatientInfo(int patientId, [FromServices] PatientDetailsDb db)
{
    return await db.PatientDetails.FindAsync(patientId)
        is PatientInfo patientInfo                          //is patient or null?
            ? TypedResults.Ok(patientInfo)                  //if patient
            : TypedResults.NotFound("No patient found");    //if null
}
static async Task<IResult> AddPatientInfo(PatientInfo info, [FromServices] PatientDetailsDb db)
{
    info.Id = db.patientIdNum;
    db.patientIdNum++;
    db.PatientDetails.Add(info);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/patientDetails/{info.Id}", info);
}

static async Task<IResult> UpdatePartialPatientInfo(int id, JsonPatchDocument<PatientInfo> inputInfo, [FromServices] PatientDetailsDb db)
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

static async Task<IResult> ReplacePatientInfo(int id, PatientInfo inputInfo, [FromServices] PatientDetailsDb db)
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

static async Task<IResult> RemovePatientInfo(int patientId, [FromServices] PatientDetailsDb db)
{
    if (await db.PatientDetails.FindAsync(patientId) is PatientInfo info) //if we can find patient of id patientId
    {
        db.PatientDetails.Remove(info);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NotFound();
}