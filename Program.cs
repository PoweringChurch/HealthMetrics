using Microsoft.EntityFrameworkCore;
using HealthMetrics.Endpoints;
using Serilog;
using Serilog.Sinks.PostgreSQL;

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

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.PostgreSQL(
            connectionString: "Host=localhost;Database=mydb;Username=user;Password=pass",
            tableName: "audit_logs",
            needAutoCreateTable: true,
            columnOptions: new Dictionary<string, ColumnWriterBase>
            {
                { "timestamp", new TimestampColumnWriter(NpgsqlTypes.NpgsqlDbType.TimestampTz) },
                { "message", new RenderedMessageColumnWriter(NpgsqlTypes.NpgsqlDbType.Text) },
                { "user_id", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Text) },
                { "ip_address", new SinglePropertyColumnWriter("IpAddress", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Text) },
                { "data_id", new SinglePropertyColumnWriter("DataId", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Text) },
                { "properties", new LogEventSerializedColumnWriter(NpgsqlTypes.NpgsqlDbType.Jsonb) }
            });
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