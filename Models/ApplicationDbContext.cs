using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =======================
        // DbSets
        // =======================
        public DbSet<Claim> Claims { get; set; }
        public DbSet<LecturerProfile> LecturerProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =======================
            // Claim → LecturerProfile (optional)
            // =======================
            builder.Entity<Claim>()
                .HasOne(c => c.LecturerProfile)
                .WithMany(lp => lp.Claims)
                .HasForeignKey(c => c.LecturerProfileId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Claim → ApplicationUser (Lecturer) (required)
            // =======================
            builder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.LecturerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // =======================
            // Claim Configuration
            // =======================
            builder.Entity<Claim>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                      .IsRequired()
                      .HasMaxLength(36); // GUID string

                entity.Property(c => c.Title)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.Status)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(c => c.Description)
                      .HasMaxLength(500);

                entity.Property(c => c.FilePath)
                      .HasMaxLength(200);

                entity.Property(c => c.HoursWorked)
                      .HasColumnType("decimal(5,2)")
                      .IsRequired();

                entity.Property(c => c.HourlyRate)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Ignore(c => c.Amount); // Computed, not stored

                entity.Property(c => c.DateSubmitted)
                      .IsRequired();

                entity.Property(c => c.ModuleName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.ModuleCode)
                      .IsRequired()
                      .HasMaxLength(20);

                // Optional indexes
                entity.HasIndex(c => c.Status);
                entity.HasIndex(c => c.DateSubmitted);
            });

            // =======================
            // LecturerProfile Configuration
            // =======================
            builder.Entity<LecturerProfile>(entity =>
            {
                entity.HasKey(lp => lp.Id);
                entity.Property(lp => lp.Id)
                      .IsRequired()
                      .HasMaxLength(36); // Match ApplicationUser.Id

                entity.Property(lp => lp.FullName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(lp => lp.EmployeeID)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(lp => lp.QualificationName)
                      .HasMaxLength(100);

                entity.Property(lp => lp.QualificationCode)
                      .HasMaxLength(50);

                entity.Property(lp => lp.Faculty)
                      .HasMaxLength(100);

                entity.Property(lp => lp.YearLevel)
                      .HasMaxLength(50);

                // Index for faster lookup
                entity.HasIndex(lp => lp.EmployeeID).IsUnique();
            });
        }
    }
}
