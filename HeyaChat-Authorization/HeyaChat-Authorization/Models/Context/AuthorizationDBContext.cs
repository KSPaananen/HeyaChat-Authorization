using System;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.Models.Context;

public partial class AuthorizationDBContext : DbContext
{
    private IConfigurationRepository _repository;

    public AuthorizationDBContext()
    {
        
    }

    public AuthorizationDBContext(IConfigurationRepository repository, DbContextOptions<AuthorizationDBContext> options) : base(options)
    {
        _repository = repository ?? throw new NullReferenceException(nameof(repository));
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BlockedCredential> BlockedCredentials { get; set; }

    public virtual DbSet<DeleteRequest> DeleteRequests { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Codes> Codess { get; set; }

    public virtual DbSet<Suspension> Suspensions { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_repository.GetConnectionString(), builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
        });
    }
        

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs");

            entity.HasIndex(e => e.DeviceId, "idx_audit_logs_device_id");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.PerformedAction)
                .HasMaxLength(100)
                .HasColumnName("performed_action");
            entity.Property(e => e.PerformedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("performed_at");

            entity.HasOne(d => d.Device).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("audit_logs_device_id_fkey");
        });

        modelBuilder.Entity<BlockedCredential>(entity =>
        {
            entity.HasKey(e => e.BlockId).HasName("blocked_credentials_pkey");

            entity.ToTable("blocked_credentials");

            entity.HasIndex(e => e.Email, "idx_blocked_credentials_email");

            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<DeleteRequest>(entity =>
        {
            entity.HasKey(e => e.DeleteId).HasName("delete_requests_pkey");

            entity.ToTable("delete_requests");

            entity.HasIndex(e => e.DateRequested, "idx_delete_requests_date_requested");

            entity.Property(e => e.DeleteId).HasColumnName("delete_id");
            entity.Property(e => e.DateRequested)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("date_requested");
            entity.Property(e => e.Fulfilled)
                .HasDefaultValue(false)
                .HasColumnName("fulfilled");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.DeleteRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("delete_requests_user_id_fkey");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("devices_pkey");

            entity.ToTable("devices");

            entity.HasIndex(e => e.UserId, "idx_devices_user_id");

            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.CountryTag)
                .HasMaxLength(3)
                .HasColumnName("country_tag");
            entity.Property(e => e.DeviceIdentifier).HasColumnName("device_identifier");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(50)
                .HasColumnName("device_name");
            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("used_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Devices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("devices_user_id_fkey");
        });

        modelBuilder.Entity<Codes>(entity =>
        {
            entity.HasKey(e => e.CodeId).HasName("codes_pkey");

            entity.ToTable("codes");

            entity.HasIndex(e => e.UserId, "idx_codes_user_id");

            entity.Property(e => e.CodeId).HasColumnName("code_id");
            entity.Property(e => e.Code)
                .HasMaxLength(8)
                .HasColumnName("code");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.Used)
                .HasDefaultValue(false)
                .HasColumnName("used");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.MfaCodes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("codes_user_id_fkey");
        });

        modelBuilder.Entity<Suspension>(entity =>
        {
            entity.HasKey(e => e.SuspensionId).HasName("suspensions_pkey");

            entity.ToTable("suspensions");

            entity.HasIndex(e => e.UserId, "idx_suspensions_user_id");

            entity.Property(e => e.SuspensionId).HasColumnName("suspension_id");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.LiftedAt).HasColumnName("lifted_at");
            entity.Property(e => e.Reason)
                .HasMaxLength(150)
                .HasColumnName("reason");
            entity.Property(e => e.SuspendedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("suspended_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Suspensions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("suspensions_user_id_fkey");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("tokens_pkey");

            entity.ToTable("tokens");

            entity.HasIndex(e => e.DeviceId, "idx_tokens_device_id");

            entity.HasIndex(e => e.Identifier, "tokens_identifier_key").IsUnique();

            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.Active)
                .HasDefaultValue(false)
                .HasColumnName("active");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.Identifier).HasColumnName("identifier");

            entity.HasOne(d => d.Device).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tokens_device_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.UserId, "idx_users_user_id");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "users_phone_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BiometricsKey).HasColumnName("biometrics_key");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt).HasColumnName("password_salt");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("user_details_pkey");

            entity.ToTable("user_details");

            entity.HasIndex(e => e.UserId, "idx_user_details_user_id");

            entity.Property(e => e.DetailId).HasColumnName("detail_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("email_verified");
            entity.Property(e => e.MfaEnabled)
                .HasDefaultValue(false)
                .HasColumnName("mfa_enabled");
            entity.Property(e => e.PhoneVerified)
                .HasDefaultValue(false)
                .HasColumnName("phone_verified");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
