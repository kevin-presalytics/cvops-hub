﻿// <auto-generated />
using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using db_migrations;

#nullable disable

namespace db_migrations.Migrations
{
    [DbContext(typeof(MigrationsDbContext))]
    partial class MigrationsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.16")
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

                    b.Property<Guid?>("UserCreated")
                        .HasColumnType("uuid")
                        .HasColumnName("user_created");

                    b.Property<Guid?>("UserModified")
                        .HasColumnType("uuid")
                        .HasColumnName("user_modified");

                    b.Property<Guid>("WorkspaceId")
                        .HasColumnType("uuid")
                        .HasColumnName("workspace_id");

                    b.HasKey("Id")
                        .HasName("pk_device");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("WorkspaceId")
                        .HasDatabaseName("ix_device_workspace_id");

                    b.ToTable("device", (string)null);
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

                    b.Property<Guid>("DefaultWorkspaceId")
                        .HasColumnType("uuid")
                        .HasColumnName("default_workspace_id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<bool>("IsEmailVerified")
                        .HasColumnType("boolean")
                        .HasColumnName("is_email_verified");

                    b.Property<string>("JwtSubject")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("jwt_subject");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("modified_by");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<Guid?>("UserCreated")
                        .HasColumnType("uuid")
                        .HasColumnName("user_created");

                    b.Property<Guid?>("UserModified")
                        .HasColumnType("uuid")
                        .HasColumnName("user_modified");

                    b.HasKey("Id")
                        .HasName("pk_cvops_user");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("JwtSubject")
                        .IsUnique();

                    b.ToTable("cvops_user", (string)null);
                });

            modelBuilder.Entity("lib.models.db.Workspace", b =>
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
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

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

                    b.HasKey("Id")
                        .HasName("pk_workspace");

                    b.ToTable("workspace", (string)null);
                });

            modelBuilder.Entity("lib.models.db.WorkspaceUser", b =>
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

                    b.Property<Guid?>("UserCreated")
                        .HasColumnType("uuid")
                        .HasColumnName("user_created");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<Guid?>("UserModified")
                        .HasColumnType("uuid")
                        .HasColumnName("user_modified");

                    b.Property<Guid>("WorkspaceId")
                        .HasColumnType("uuid")
                        .HasColumnName("workspace_id");

                    b.Property<string>("WorkspaceUserRole")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("workspace_user_role");

                    b.HasKey("Id")
                        .HasName("pk_workspace_user");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_workspace_user_user_id");

                    b.HasIndex("WorkspaceId")
                        .HasDatabaseName("ix_workspace_user_workspace_id");

                    b.ToTable("workspace_user", (string)null);
                });

            modelBuilder.Entity("lib.models.db.Device", b =>
                {
                    b.HasOne("lib.models.db.Workspace", "Workspace")
                        .WithMany("Devices")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_device_workspace_workspace_id");

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("lib.models.db.WorkspaceUser", b =>
                {
                    b.HasOne("lib.models.db.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_workspace_user_cvops_user_user_id");

                    b.HasOne("lib.models.db.Workspace", "Workspace")
                        .WithMany("WorkspaceUsers")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_workspace_user_workspace_workspace_id");

                    b.Navigation("User");

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("lib.models.db.Workspace", b =>
                {
                    b.Navigation("Devices");

                    b.Navigation("WorkspaceUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
