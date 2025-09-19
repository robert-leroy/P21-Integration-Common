/*
 * File: ApplicationDbContext.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible for defining the data model used during processing.
 * 
 * Dependencies:
 * - SqLiteDB for in-memory database operations
 * 
 * Notes:
 * - Ensure proper configuration of log4net before using this class.
 * 
 * Copyright 2025 Robert LeRoy, Vero Software, LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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
