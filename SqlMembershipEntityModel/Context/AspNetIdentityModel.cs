#nullable disable

using SqlMembershipEntityModel.Models;
using Microsoft.EntityFrameworkCore;

namespace SqlMembershipEntityModel.Context;

public partial class AspNetIdentityModel : DbContext
{
    public AspNetIdentityModel(DbContextOptions<AspNetIdentityModel> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetApplication> AspNetApplications { get; set; }

    public virtual DbSet<AspNetMembership> AspNetMemberships { get; set; }

    public virtual DbSet<AspNetProfile> AspNetProfiles { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetSchemaVersion> AspNetSchemaVersions { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUsersInRole> AspNetUsersInRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId);
            entity.ToTable("aspnet_Applications");

            entity
                .HasIndex(e => e.LoweredApplicationName)
                .IsUnique();

            entity
                .HasIndex(e => e.ApplicationName)
                .IsUnique();

            entity
                .HasIndex(e => e.LoweredApplicationName)
                .IsClustered();

            entity
                .Property(e => e.ApplicationId)
                .HasDefaultValueSql("(newid())");

            entity
                .Property(e => e.ApplicationName)
                .IsRequired()
                .HasMaxLength(256);

            entity
                .Property(e => e.Description)
                .HasMaxLength(256);

            entity
                .Property(e => e.LoweredApplicationName)
                .IsRequired()
                .HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetMembership>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.ToTable("aspnet_Membership");

            entity
                .HasIndex(e => new { e.ApplicationId, e.LoweredEmail });

            entity
                .Property(e => e.UserId)
                .ValueGeneratedNever();

            entity
                .Property(e => e.Comment)
                .HasColumnType("ntext");

            entity
                .Property(e => e.CreateDate)
                .HasColumnType("datetime");

            entity
                .Property(e => e.Email)
                .HasMaxLength(256);

            entity
                .Property(e => e.FailedPasswordAnswerAttemptWindowStart)
                .HasColumnType("datetime");

            entity
                .Property(e => e.FailedPasswordAttemptWindowStart)
                .HasColumnType("datetime");

            entity
                .Property(e => e.LastLockoutDate)
                .HasColumnType("datetime");

            entity
                .Property(e => e.LastLoginDate)
                .HasColumnType("datetime");

            entity
                .Property(e => e.LastPasswordChangedDate)
                .HasColumnType("datetime");

            entity
                .Property(e => e.LoweredEmail)
                .HasMaxLength(256);

            entity
                .Property(e => e.MobilePIN)
                .HasMaxLength(16);

            entity
                .Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(128);

            entity
                .Property(e => e.PasswordAnswer)
                .HasMaxLength(128);

            entity
                .Property(e => e.PasswordQuestion)
                .HasMaxLength(256);

            entity
                .Property(e => e.PasswordSalt)
                .IsRequired()
                .HasMaxLength(128);

            entity
                .HasOne(d => d.Application)
                .WithMany(p => p.AspNetMemberships)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity
                .HasOne(d => d.User)
                .WithOne(p => p.AspNetMembership)
                .HasForeignKey<AspNetMembership>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AspNetProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.ToTable("aspnet_Profile");

            entity
                .Property(e => e.UserId)
                .ValueGeneratedNever();

            entity
                .Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime");

            entity
                .Property(e => e.PropertyNames)
                .IsRequired()
                .HasColumnType("ntext");

            entity
                .Property(e => e.PropertyValuesBinary)
                .IsRequired()
                .HasColumnType("image");

            entity
                .Property(e => e.PropertyValuesString)
                .IsRequired()
                .HasColumnType("ntext");

            entity
                .HasOne(d => d.User)
                .WithOne(p => p.AspNetProfile)
                .HasForeignKey<AspNetProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.ToTable("aspnet_Roles");

            entity
                .HasIndex(e => new { e.ApplicationId, e.LoweredRoleName })
                .IsUnique()
                .IsClustered();

            entity
                .Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())");

            entity
                .Property(e => e.Description)
                .HasMaxLength(256);

            entity
                .Property(e => e.LoweredRoleName)
                .IsRequired()
                .HasMaxLength(256);

            entity
                .Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(256);

            entity
                .HasOne(d => d.Application)
                .WithMany(p => p.AspNetRoles)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AspNetSchemaVersion>(entity =>
        {
            entity.HasKey(e => new { e.Feature, e.CompatibleSchemaVersion });
            entity.ToTable("aspnet_SchemaVersions");

            entity
                .Property(e => e.Feature)
                .HasMaxLength(128);

            entity
                .Property(e => e.CompatibleSchemaVersion)
                .HasMaxLength(128);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.ToTable("aspnet_Users");

            entity
                .HasIndex(e => new { e.ApplicationId, e.LoweredUserName })
                .IsUnique()
                .IsClustered(true);

            entity
                .HasIndex(e => new { e.ApplicationId, e.LastActivityDate });

            entity
                .Property(e => e.UserId)
                .HasDefaultValueSql("(newid())");

            entity
                .Property(e => e.LastActivityDate)
                .HasColumnType("datetime");

            entity
                .Property(e => e.LoweredUserName)
                .IsRequired()
                .HasMaxLength(256);

            entity
                .Property(e => e.MobileAlias)
                .HasMaxLength(16);

            entity
                .Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(256);

            entity
                .HasOne(d => d.Application)
                .WithMany(p => p.AspNetUsers)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AspNetUsersInRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.ToTable("aspnet_UsersInRoles");

            entity.HasIndex(e => e.RoleId);

            entity
                .HasOne(d => d.Role)
                .WithMany(p => p.AspNetUsersInRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.AspNetUsersInRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
    }
}