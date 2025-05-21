
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Grad_Project_Dashboard_1.Models;
using Microsoft.EntityFrameworkCore.Design;

namespace Grad_Project_Dashboard_1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<CustomRule> CustomRules { get; set; }
        public DbSet<UserCustomRule> userCustomRules { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserCustomRule>()
                .HasKey(ur => new { ur.UserId, ur.CustomRuleId });

            modelBuilder.Entity<UserCustomRule>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserCustomRules)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserCustomRule>()
                .HasOne(ur => ur.CustomRule)
                .WithMany(r => r.UserCustomRules)
                .HasForeignKey(ur => ur.CustomRuleId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=DashBoard2;Trusted_Connection=True;Integrated Security=True;TrustServerCertificate=True;");
            base.OnConfiguring(optionsBuilder);
        }


    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=DashBoard2;Trusted_Connection=True;Integrated Security=True;TrustServerCertificate=True;");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
