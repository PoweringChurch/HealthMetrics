using Microsoft.EntityFrameworkCore;

class PatientDetailsDb : DbContext
{
    public PatientDetailsDb(DbContextOptions<PatientDetailsDb> options) : base(options) {}
    public DbSet<PatientInfo> PatientDetails => Set<PatientInfo>();
}