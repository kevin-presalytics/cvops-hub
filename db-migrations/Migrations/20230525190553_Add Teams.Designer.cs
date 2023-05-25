﻿// <auto-generated />
using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using db_migrations;

#nullable disable

namespace db_migrations.Migrations
{
    [DbContext(typeof(MigrationsDbContext))]
    [Migration("20230525190553_Add Teams")]
    partial class AddTeams
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("lib.models.db.Device", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_modified");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<JsonDocument>("DeviceInfo")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("device_info");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("hash");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("modified_by");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<byte[]>("Salt")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("salt");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uuid")
                        .HasColumnName("team_id");

                    b.Property<Guid?>("UserCreated")
                        .HasColumnType("uuid")
                        .HasColumnName("user_created");

                    b.Property<Guid?>("UserModified")
                        .HasColumnType("uuid")
                        .HasColumnName("user_modified");

                    b.HasKey("Id")
                        .HasName("pk_device");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("TeamId")
                        .HasDatabaseName("ix_device_team_id");

                    b.ToTable("device", (string)null);
                });

            modelBuilder.Entity("lib.models.db.Team", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_modified");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("modified_by");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<Guid?>("UserCreated")
                        .HasColumnType("uuid")
                        .HasColumnName("user_created");

                    b.Property<Guid?>("UserModified")
                        .HasColumnType("uuid")
                        .HasColumnName("user_modified");

                    b.Property<bool>("isDefaultTeam")
                        .HasColumnType("boolean")
                        .HasColumnName("is_default_team");

                    b.HasKey("Id")
                        .HasName("pk_team");

                    b.ToTable("team", (string)null);
                });

            modelBuilder.Entity("lib.models.db.TeamUserMap", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_modified");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("modified_by");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uuid")
                        .HasColumnName("team_id");

                    b.Property<Guid?>("UserCreated")
                        .HasColumnType("uuid")
                        .HasColumnName("user_created");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<Guid?>("UserModified")
                        .HasColumnType("uuid")
                        .HasColumnName("user_modified");

                    b.HasKey("Id")
                        .HasName("pk_team_user_map");

                    b.HasIndex("TeamId");

                    b.HasIndex("UserId");

                    b.ToTable("team_user_map", (string)null);
                });

            modelBuilder.Entity("lib.models.db.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_modified");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("JwtSubject")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("jwt_subject");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("modified_by");

                    b.Property<Guid?>("UserCreated")
                        .HasColumnType("uuid")
                        .HasColumnName("user_created");

                    b.Property<Guid?>("UserModified")
                        .HasColumnType("uuid")
                        .HasColumnName("user_modified");

                    b.HasKey("Id")
                        .HasName("pk_user");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("JwtSubject")
                        .IsUnique();

                    b.ToTable("user", (string)null);
                });

            modelBuilder.Entity("lib.models.db.Device", b =>
                {
                    b.HasOne("lib.models.db.Team", "Team")
                        .WithMany("Devices")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_device_team_team_id");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("lib.models.db.TeamUserMap", b =>
                {
                    b.HasOne("lib.models.db.Team", null)
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("lib.models.db.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("lib.models.db.Team", b =>
                {
                    b.Navigation("Devices");
                });
#pragma warning restore 612, 618
        }
    }
}
