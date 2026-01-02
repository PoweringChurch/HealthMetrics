using Microsoft.EntityFrameworkCore;

class PatientDetailsDb : DbContext
{
    public PatientDetailsDb(DbContextOptions<PatientDetailsDb> options) : base(options) {}
    public int patientIdNum = 0;
    public DbSet<PatientInfo> PatientDetails => Set<PatientInfo>();
}