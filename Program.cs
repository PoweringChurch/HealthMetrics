using Microsoft.EntityFrameworkCore;
using HealthMetrics.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

Console.WriteLine($"Connection String: {builder.Configuration.GetConnectionString("PatientInfoDb")}");

builder.Services.AddDbContext<PatientInfoDb>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("HealthMetricsDb")));
    
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

PatientEndpoints.MapEndpoints(app);
MedicationEndpoints.MapEndpoints(app);

app.Run();