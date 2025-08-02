using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using desco_report_server.Models;

namespace desco_report_server.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
    {

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<DailyConsumption> DailyConsumptions { get; set; } = null!;
        public DbSet<Bill> Bills { get; set; } = null!;
        public DbSet<DescoAccount> DescoAccounts { get; set; } = null!;
        public DbSet<DescoDailyConsumption> DescoDailyConsumptions { get; set; } = null!;
        public DbSet<DescoMonthlyConsumption> DescoMonthlyConsumptions { get; set; } = null!;
        public DbSet<DescoRechargeHistory> DescoRechargeHistories { get; set; } = null!;
        public DbSet<DescoLocation> DescoLocations { get; set; } = null!;
        public DbSet<DescoRecentEvent> DescoRecentEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure User entity - IdentityUser already handles Email and UserName
            builder.Entity<User>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
            });

            // Configure Account entity
            builder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.AccountNumber).IsUnique();
                entity.Property(a => a.AccountNumber).HasMaxLength(20).IsRequired();
                entity.Property(a => a.CustomerName).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Address).HasMaxLength(500);
                entity.Property(a => a.PhoneNumber).HasMaxLength(15);
                entity.Property(a => a.Email).HasMaxLength(256);
            });

            // Configure DailyConsumption entity
            builder.Entity<DailyConsumption>(entity =>
            {
                entity.HasKey(dc => dc.Id);
                entity.Property(dc => dc.ConsumptionValue).HasPrecision(10, 2);
                entity.Property(dc => dc.Unit).HasMaxLength(10);
            });

            // Configure Bill entity
            builder.Entity<Bill>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.BillAmount).HasPrecision(10, 2);
                entity.Property(b => b.PaidAmount).HasPrecision(10, 2);
                entity.Property(b => b.DueAmount).HasPrecision(10, 2);
            });

            // Configure DescoAccount entity (simplified - no navigation properties)
            builder.Entity<DescoAccount>(entity =>
            {
                entity.HasKey(da => da.Id);
                entity.HasIndex(da => da.AccountNumber).IsUnique();
                entity.Property(da => da.AccountNumber).HasMaxLength(50).IsRequired();
                entity.Property(da => da.MeterNumber).HasMaxLength(50).IsRequired();
                entity.Property(da => da.CustomerName).HasMaxLength(100);
                entity.Property(da => da.Address).HasMaxLength(500);
                entity.Property(da => da.PhoneNumber).HasMaxLength(20);
                entity.Property(da => da.Email).HasMaxLength(100);
                entity.Property(da => da.CurrentBalance).HasPrecision(10, 2);
                entity.Property(da => da.CurrentMonthConsumption).HasPrecision(10, 2);
            });

            // Configure DescoDailyConsumption entity (simplified - no navigation properties)
            builder.Entity<DescoDailyConsumption>(entity =>
            {
                entity.HasKey(dc => dc.Id);
                entity.Property(dc => dc.ConsumptionValue).HasPrecision(10, 2);
                entity.Property(dc => dc.Cost).HasPrecision(10, 2);
                entity.Property(dc => dc.Unit).HasMaxLength(10);
                entity.HasIndex(dc => new { dc.DescoAccountId, dc.Date }).IsUnique();
            });

            // Configure DescoMonthlyConsumption entity (simplified - no navigation properties)
            builder.Entity<DescoMonthlyConsumption>(entity =>
            {
                entity.HasKey(mc => mc.Id);
                entity.Property(mc => mc.ConsumptionValue).HasPrecision(10, 2);
                entity.Property(mc => mc.Cost).HasPrecision(10, 2);
                entity.Property(mc => mc.AverageDailyConsumption).HasPrecision(10, 2);
                entity.Property(mc => mc.Unit).HasMaxLength(10);
                entity.HasIndex(mc => new { mc.DescoAccountId, mc.Year, mc.Month }).IsUnique();
            });

            // Configure DescoRechargeHistory entity (simplified - no navigation properties)
            builder.Entity<DescoRechargeHistory>(entity =>
            {
                entity.HasKey(rh => rh.Id);
                entity.Property(rh => rh.Amount).HasPrecision(10, 2);
                entity.Property(rh => rh.TransactionId).HasMaxLength(50);
                entity.Property(rh => rh.PaymentMethod).HasMaxLength(50);
                entity.Property(rh => rh.Status).HasMaxLength(20);
                entity.Property(rh => rh.Notes).HasMaxLength(500);
            });

            // Configure DescoLocation entity (simplified - no navigation properties)
            builder.Entity<DescoLocation>(entity =>
            {
                entity.HasKey(dl => dl.Id);
                entity.Property(dl => dl.Division).HasMaxLength(100);
                entity.Property(dl => dl.District).HasMaxLength(100);
                entity.Property(dl => dl.Thana).HasMaxLength(100);
                entity.Property(dl => dl.Area).HasMaxLength(100);
                entity.Property(dl => dl.PostCode).HasMaxLength(20);
                entity.Property(dl => dl.FullAddress).HasMaxLength(500);
                entity.Property(dl => dl.Latitude).HasPrecision(10, 6);
                entity.Property(dl => dl.Longitude).HasPrecision(10, 6);
            });

            // Configure DescoRecentEvent entity (simplified - no navigation properties)
            builder.Entity<DescoRecentEvent>(entity =>
            {
                entity.HasKey(re => re.Id);
                entity.Property(re => re.EventType).HasMaxLength(100).IsRequired();
                entity.Property(re => re.Message).HasMaxLength(500).IsRequired();
                entity.Property(re => re.Category).HasMaxLength(50);
                entity.Property(re => re.Priority).HasMaxLength(20);
            });
        }
    }
}