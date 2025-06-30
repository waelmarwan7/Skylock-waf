

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
        public DbSet<UserCustomRule> UserCustomRules { get; set; }
        public DbSet<UserDomainName> UserDomainNames { get; set; } // New DbSet for domain names

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure UserCustomRule many-to-many relationship
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
                
            // Configure User to UserDomainName one-to-many relationship
            modelBuilder.Entity<UserDomainName>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.DomainNames)
                .HasForeignKey(ud => ud.UserId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=DashBoard2;Trusted_Connection=True;Integrated Security=True;TrustServerCertificate=True;");
            }
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
