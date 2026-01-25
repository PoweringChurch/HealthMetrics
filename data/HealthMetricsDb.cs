using Microsoft.EntityFrameworkCore;

class HealthMetricsDb(DbContextOptions<HealthMetricsDb> options) : DbContext(options)
{
    public DbSet<Patient> Patients {get; set;}
    public DbSet<Diagnosis> Diagnoses {get; set;}
    public DbSet<Medication> Medications {get; set;}
    public DbSet<VitalsEntry> VitalsEntries {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // filter out data that has been marked as deleted
        modelBuilder.Entity<Patient>().HasQueryFilter(p => p.DeletedAt == null);
        modelBuilder.Entity<VitalsEntry>().HasQueryFilter(v => v.DeletedAt == null);
        modelBuilder.Entity<Medication>().HasQueryFilter(m => m.DeletedAt == null);
        modelBuilder.Entity<Diagnosis>().HasQueryFilter(d=>d.DeletedAt == null);
    }
}