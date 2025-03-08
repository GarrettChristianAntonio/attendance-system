using AttendanceSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<EmployeeSchedule> EmployeeSchedules => Set<EmployeeSchedule>();
    public DbSet<AbsenceRecord> AbsenceRecords => Set<AbsenceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KeyHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(11);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhotoPath).IsRequired();
            entity.Property(e => e.FaceDescriptor).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId);
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("OnTime");
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
            entity.HasOne(e => e.Shift).WithMany().HasForeignKey(e => e.ShiftId);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId);
        });

        modelBuilder.Entity<EmployeeSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
            entity.HasOne(e => e.Shift).WithMany().HasForeignKey(e => e.ShiftId);
        });

        modelBuilder.Entity<AbsenceRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
            entity.HasOne(e => e.Shift).WithMany().HasForeignKey(e => e.ShiftId);
        });
    }
}
