using Microsoft.EntityFrameworkCore;

class MedicationDb : DbContext
{
    public MedicationDb(DbContextOptions<MedicationDb> options) : base(options) {}
    public DbSet<Medication> Medications {get; set;}
}
