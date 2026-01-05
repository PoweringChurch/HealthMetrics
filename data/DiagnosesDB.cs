using Microsoft.EntityFrameworkCore;

class DiagnosesDb : DbContext
{
    public DiagnosesDb(DbContextOptions<DiagnosesDb> options) : base(options) {}
    public DbSet<Diagnosis> Diagnoses {get; set;}
}