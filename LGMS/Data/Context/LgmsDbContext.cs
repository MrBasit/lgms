using LGMS.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LGMS.Data.Context
{
    public class LgmsDbContext:IdentityDbContext<IdentityUser>
    {
        public LgmsDbContext(DbContextOptions<LgmsDbContext> options):base(options)
        {
            
        }

        public DbSet<EmployeeStatus> EmployeeStatus { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<EquipmentStatus> EquipmentStatus { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<AttendanceId> AttendanceIds { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<AttendanceRecordStatus> AttendanceRecordStatuses { get; set; }
        public DbSet<SalarySlip> SalarySlips { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<QuotationPackageInformation> QuotationPackagesInformation { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<ContractStatus> ContractStatuses { get; set; }
        public DbSet<ContractType> ContractTypes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Expiration> Expiration { get; set; }
        public DbSet<ContractPackageInformation> ContractPackagesInformation { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<DomainDetails> DomainDetails { get; set; }
        public DbSet<SecurityDeposit> SecurityDeposits { get; set; }
        public DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
            builder.Entity<Department>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            builder.Entity<Designation>(entity =>
            {
                entity.HasIndex(e => e.Title).IsUnique();
            });

            builder.Entity<EmployeeStatus>(entity =>
            {
                entity.HasIndex(e => e.Title).IsUnique();
            });

            builder.Entity<Manufacturer>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            builder.Entity<Vendor>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            builder.Entity<EquipmentStatus>(entity =>
            {
                entity.HasIndex(e => e.Title).IsUnique();
            });

            builder.Entity<AttendanceId>(entity =>
            {
                entity.HasIndex(e => e.MachineId).IsUnique();
                entity.HasIndex(e => e.MachineName).IsUnique();
            });

        }

        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                //new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                //new IdentityRole() { Name = "HR", ConcurrencyStamp = "2", NormalizedName = "HR" },
                //new IdentityRole() { Name = "Sales", ConcurrencyStamp = "3", NormalizedName = "Sales" },
                //new IdentityRole() { Name = "Stores", ConcurrencyStamp = "4", NormalizedName = "Stores" },
                //new IdentityRole() { Name = "Employee", ConcurrencyStamp = "5", NormalizedName = "Employee" },
                //new IdentityRole() { Name = "Access", ConcurrencyStamp = "6", NormalizedName = "Access" },
                //new IdentityRole() { Name = "BD", ConcurrencyStamp = "7", NormalizedName = "BD" }
                );
        }
    }
}
