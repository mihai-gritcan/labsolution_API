﻿// <auto-generated />
using System;
using LabSolution.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LabSolution.Migrations
{
    [DbContext(typeof(LabSolutionContext))]
    [Migration("20211101181451_AddColumn_IsSuperUser")]
    partial class AddColumn_IsSuperUser
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:Collation", "Latin1_General_CI_AS")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LabSolution.Models.AppUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsSuperUser")
                        .HasColumnType("bit");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("AppUser");
                });

            modelBuilder.Entity("LabSolution.Models.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("Email")
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Passport")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PersonalNumber")
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.Property<string>("Phone")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Customer");
                });

            modelBuilder.Entity("LabSolution.Models.CustomerOrder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<int?>("ParentId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PlacedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Scheduled")
                        .HasColumnType("datetime2");

                    b.Property<short>("TestLanguage")
                        .HasColumnType("smallint");

                    b.Property<short>("TestType")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CustomerId" }, "IX_CustomerOrder_CustomerId");

                    b.ToTable("CustomerOrder");
                });

            modelBuilder.Entity("LabSolution.Models.ProcessedOrder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CheckedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CustomerOrderId")
                        .HasColumnType("int");

                    b.Property<string>("PdfName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PrintCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("ProcessedAt")
                        .HasColumnType("datetime");

                    b.Property<string>("ProcessedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TestResult")
                        .HasColumnType("int");

                    b.Property<string>("ValidatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CustomerOrderId" }, "IX_ProcessedOrder_CustomerOrderId")
                        .IsUnique();

                    b.ToTable("ProcessedOrder");
                });

            modelBuilder.Entity("LabSolution.Models.CustomerOrder", b =>
                {
                    b.HasOne("LabSolution.Models.Customer", "Customer")
                        .WithMany("CustomerOrders")
                        .HasForeignKey("CustomerId")
                        .HasConstraintName("FK_CustomerOrder_Customer")
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("LabSolution.Models.ProcessedOrder", b =>
                {
                    b.HasOne("LabSolution.Models.CustomerOrder", "CustomerOrder")
                        .WithOne("ProcessedOrder")
                        .HasForeignKey("LabSolution.Models.ProcessedOrder", "CustomerOrderId")
                        .HasConstraintName("FK_ProcessedOrder_CustomerOrder")
                        .IsRequired();

                    b.Navigation("CustomerOrder");
                });

            modelBuilder.Entity("LabSolution.Models.Customer", b =>
                {
                    b.Navigation("CustomerOrders");
                });

            modelBuilder.Entity("LabSolution.Models.CustomerOrder", b =>
                {
                    b.Navigation("ProcessedOrder");
                });
#pragma warning restore 612, 618
        }
    }
}
