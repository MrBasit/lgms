using LGMS.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
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
        }
    }
}
