using Microsoft.EntityFrameworkCore;

class HealthMetricsDb : DbContext
{
    public HealthMetricsDb(DbContextOptions<HealthMetricsDb> options) : base(options) {}
    public DbSet<PatientInfo> PatientInfos {get; set;}
    public DbSet<Diagnosis> Diagnoses {get; set;}
    public DbSet<Medication> Medications {get; set;}
    public DbSet<VitalsEntry> VitalsEntries {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientInfo>().HasQueryFilter(p => p.DeletedAt == null);
        modelBuilder.Entity<VitalsEntry>().HasQueryFilter(v => v.DeletedAt == null);
        modelBuilder.Entity<Medication>().HasQueryFilter(m => m.DeletedAt == null);
        modelBuilder.Entity<Diagnosis>().HasQueryFilter(d=>d.DeletedAt == null);
    }
}