using Microsoft.EntityFrameworkCore;
using HealthMetrics.Endpoints;

//builder setup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<HealthMetricsDb>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("HealthMetricsDb")));
    
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "HealthcareAPI";
    config.Title = "HealthcareAPI v1";
    config.Version = "v1";
});
//create app
var app = builder.Build();
//swagger setup
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
//use http redirects
app.UseHttpsRedirection();
//map endpoints
PatientEndpoints.MapEndpoints(app);
MedicationEndpoints.MapEndpoints(app);
DiagnosesEndpoints.MapEndpoints(app);
VitalsEndpoints.MapEndpoints(app);

app.Run();