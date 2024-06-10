﻿// <auto-generated />
using System;
using InspiraHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InspiraHub.Migrations
{
    [DbContext(typeof(InspirahubContext))]
    partial class InspirahubContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "pgcrypto");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence("main_sequence");

            modelBuilder.HasSequence("my_first_sequence")
                .StartsAt(5L);

            modelBuilder.Entity("InspiraHub.Models.Comment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasDefaultValueSql("nextval('main_sequence'::regclass)");

                    b.Property<long>("ContentId")
                        .HasColumnType("bigint")
                        .HasColumnName("content_id");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("create_at");

                    b.Property<string>("UserComment")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_comment");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("user_name");

                    b.HasKey("Id")
                        .HasName("comments_pkey");

                    b.HasIndex("ContentId");

                    b.HasIndex("UserId");

                    b.ToTable("comments", (string)null);
                });

            modelBuilder.Entity("InspiraHub.Models.Content", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasDefaultValueSql("nextval('main_sequence'::regclass)");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("content_type");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("create_at");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Preview")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("preview");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("title");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("content_pkey");

                    b.HasIndex("UserId");

                    b.ToTable("contents", (string)null);
                });

            modelBuilder.Entity("InspiraHub.Models.PasswordResetToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("PasswordResetTokens");
                });

            modelBuilder.Entity("InspiraHub.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasDefaultValueSql("nextval('my_first_sequence'::regclass)");

                    b.Property<DateOnly>("DateBirth")
                        .HasColumnType("date")
                        .HasColumnName("date_birth");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("email");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("last_name");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("password");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("users_pkey");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("InspiraHub.Models.Comment", b =>
                {
                    b.HasOne("InspiraHub.Models.Content", "Content")
                        .WithMany("Comments")
                        .HasForeignKey("ContentId")
                        .IsRequired()
                        .HasConstraintName("comments_content_id_fkey");

                    b.HasOne("InspiraHub.Models.User", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("comments_iduser_fkey");

                    b.Navigation("Content");

                    b.Navigation("User");
                });

            modelBuilder.Entity("InspiraHub.Models.Content", b =>
                {
                    b.HasOne("InspiraHub.Models.User", "User")
                        .WithMany("Contents")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("content_iduser_fkey");

                    b.Navigation("User");
                });

            modelBuilder.Entity("InspiraHub.Models.Content", b =>
                {
                    b.Navigation("Comments");
                });

            modelBuilder.Entity("InspiraHub.Models.User", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Contents");
                });
#pragma warning restore 612, 618
        }
    }
}
