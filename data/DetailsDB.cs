using Microsoft.EntityFrameworkCore;

class PatientInfoDb : DbContext
{
    public PatientInfoDb(DbContextOptions<PatientInfoDb> options) : base(options) {}
    public DbSet<PatientInfo> PatientInfos {get; set;}
}