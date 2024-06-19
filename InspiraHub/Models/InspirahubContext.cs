using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InspiraHub.Models;

public partial class InspirahubContext : DbContext
{
    public InspirahubContext()
    {
    }

    public InspirahubContext(DbContextOptions<InspirahubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DATABASE_PORT");
        var dbName = Environment.GetEnvironmentVariable("DATABASE_NAME");
        var dbUsername = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
        var dbPassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

        var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUsername};Password={dbPassword}";

        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comments_pkey");

            entity.ToTable("comments");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('comment_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreateAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_at");
            entity.Property(e => e.UserComment).HasColumnName("user_comment");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Content).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comments_content_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comments_iduser_fkey");
        });

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("content_pkey");

            entity.ToTable("content");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('content_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ContentType)
                .HasMaxLength(20)
                .HasColumnName("content_type");
            entity.Property(e => e.CreateAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Preview).HasColumnName("preview");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Contents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("content_id_user_fkey");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("password_reset_tokens_pkey");

            entity.ToTable("password_reset_tokens");

            entity.Property(e => e.Id)
            .HasDefaultValueSql("nextval('password_reset_tokens_id_seq'::regclass)")
            .HasColumnName("id");
            entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(225)
            .HasColumnName("email");
            entity.Property(e => e.Token)
       .IsRequired()
       .HasMaxLength(255)
       .HasColumnName("token");
            entity.Property(e => e.CreatedAt)
               .HasColumnType("timestamp without time zone")
               .HasColumnName("created_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('user_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.DateBirth).HasColumnName("date_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
