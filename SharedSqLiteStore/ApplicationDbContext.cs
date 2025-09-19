using System.Data.Entity;

public class ApplicationDbContext : DbContext
{
	public DbSet<SzShipmentDetail> SzShipmentDetails { get; set; }
    public DbSet<SzShipmentDetailComment> SzShipmentDetailComments { get; set; }
    public DbSet<SzShipmentDetailSpecialCharge> SzShipmentDetailSpecialCharges { get; set; }
    public DbSet<SzShipmentHeader> SzShipmentHeaders { get; set; }
    public DbSet<SzShipmentHeaderComment> SzShipmentHeaderComments { get; set; }
    public DbSet<SzShipmentHeaderSpecialCharge> SzShipmentHeaderSpecialCharges { get; set; }
    public DbSet<SzShipmentSerial> SzShipmentSerials { get; set; }
    public DbSet<SzItemMaster> SzItemMasters { get; set; }
    public DbSet<SzErrorMessage> SzErrorMessage { get; set; }
    public ApplicationDbContext() : base(":memory:")
    {
        // Ensure the database is created and the schema is applied
        Database.SetInitializer(new DropCreateDatabaseAlways<ApplicationDbContext>());        
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SzShipmentDetail>().ToTable("SzShipmentDetail");
		modelBuilder.Entity<SzShipmentDetailComment>().ToTable("SzShipmentDetailComments");
        modelBuilder.Entity<SzShipmentDetailSpecialCharge>().ToTable("SzShipmentDetailSpecialCharges");
        modelBuilder.Entity<SzShipmentHeader>().ToTable("SzShipmentHeader");
        modelBuilder.Entity<SzShipmentHeaderComment>().ToTable("SzShipmentHeaderComments");
        modelBuilder.Entity<SzShipmentHeaderSpecialCharge>().ToTable("SzShipmentHeaderSpecialCharges");
        modelBuilder.Entity<SzShipmentSerial>().ToTable("SzShipmentSerial");
        modelBuilder.Entity<SzItemMaster>().ToTable("SzItemMaster");
        modelBuilder.Entity<SzErrorMessage>().ToTable("SzErrorMessage");

        base.OnModelCreating(modelBuilder);
	}
}
