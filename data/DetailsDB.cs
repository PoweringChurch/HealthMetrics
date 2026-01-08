using Microsoft.EntityFrameworkCore;

class HealthMetricsDb : DbContext
{
    public HealthMetricsDb(DbContextOptions<HealthMetricsDb> options) : base(options) {}
    public DbSet<PatientInfo> PatientInfos {get; set;}
    public DbSet<Diagnosis> Diagnoses {get; set;}
    public DbSet<Medication> Medications {get; set;}
    public DbSet<VitalsEntry> VitalsEntries {get; set;}
}