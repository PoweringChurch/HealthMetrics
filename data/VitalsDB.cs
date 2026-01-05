using Microsoft.EntityFrameworkCore;

class VitalsDb : DbContext
{
    public VitalsDb(DbContextOptions<VitalsDb> options) : base(options) {}
    public DbSet<VitalsEntry> VitalsEntries {get; set;}
}