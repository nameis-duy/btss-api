﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240410112505_InitFinalV1")]
    partial class InitFinalV1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "citext");
            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "pg_trgm");
            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");
            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "unaccent");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("AvatarPath")
                        .HasColumnType("text");

                    b.Property<Point>("Coordinate")
                        .HasColumnType("geography (point)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DeviceToken")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasColumnType("citext");

                    b.Property<decimal>("GcoinBalance")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsMale")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<int>("PrestigePoint")
                        .HasColumnType("integer");

                    b.Property<int?>("ProviderId")
                        .HasColumnType("integer");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Phone")
                        .IsUnique();

                    b.HasIndex("ProviderId")
                        .IsUnique();

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Domain.Entities.Announcement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("AccountId")
                        .HasColumnType("integer");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("text");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<int?>("OrderId")
                        .HasColumnType("integer");

                    b.Property<int?>("PlanId")
                        .HasColumnType("integer");

                    b.Property<int?>("ProviderId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("OrderId");

                    b.HasIndex("PlanId");

                    b.HasIndex("ProviderId");

                    b.ToTable("Announcement");
                });

            modelBuilder.Entity("Domain.Entities.Destination", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int[]>("Activities")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Point>("Coordinate")
                        .IsRequired()
                        .HasColumnType("geography (point)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<string>>("ImagePaths")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<NpgsqlTsVector>("NameVector")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "simple")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Name", "UnaccentName" });

                    b.Property<int>("ProvinceId")
                        .HasColumnType("integer");

                    b.Property<int?>("Rating")
                        .HasColumnType("integer");

                    b.Property<int[]>("Seasons")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<int>("Topographic")
                        .HasColumnType("integer");

                    b.Property<string>("UnaccentName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("NameVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("NameVector"), "GIN");

                    b.HasIndex("ProvinceId");

                    b.HasIndex("UnaccentName");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("UnaccentName"), "GIN");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("UnaccentName"), new[] { "gin_trgm_ops" });

                    b.ToTable("Destination");
                });

            modelBuilder.Entity("Domain.Entities.DestinationComment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DestinationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("DestinationId");

                    b.ToTable("DestinationComment");
                });

            modelBuilder.Entity("Domain.Entities.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("CurrentStatus")
                        .HasColumnType("integer");

                    b.Property<decimal>("Deposit")
                        .HasColumnType("numeric");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<int?>("Period")
                        .HasColumnType("integer");

                    b.Property<int>("PlanId")
                        .HasColumnType("integer");

                    b.Property<int>("ProviderId")
                        .HasColumnType("integer");

                    b.Property<int?>("Rating")
                        .HasColumnType("integer");

                    b.Property<List<DateOnly>>("ServeDates")
                        .IsRequired()
                        .HasColumnType("date[]");

                    b.Property<decimal>("Total")
                        .HasColumnType("numeric");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("PlanId");

                    b.HasIndex("ProviderId");

                    b.ToTable("Order");
                });

            modelBuilder.Entity("Domain.Entities.OrderDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<int>("OrderId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<int>("ProductId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Total")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderDetail");
                });

            modelBuilder.Entity("Domain.Entities.Plan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("integer");

                    b.Property<decimal>("ActualGcoinBudget")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Point>("Departure")
                        .IsRequired()
                        .HasColumnType("geography (point)");

                    b.Property<string>("DepartureAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DestinationId")
                        .HasColumnType("integer");

                    b.Property<decimal>("DisplayGcoinBudget")
                        .HasColumnType("numeric");

                    b.Property<DateOnly>("EndDate")
                        .HasColumnType("date");

                    b.Property<decimal>("GcoinBudgetPerCapita")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("boolean");

                    b.Property<int>("JoinMethod")
                        .HasColumnType("integer");

                    b.Property<int>("MaxMemberCount")
                        .HasColumnType("integer");

                    b.Property<int>("MaxMemberWeight")
                        .HasColumnType("integer");

                    b.Property<int>("MemberCount")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<TimeSpan>("Offset")
                        .HasColumnType("interval");

                    b.Property<int>("PeriodCount")
                        .HasColumnType("integer");

                    b.Property<string>("Schedule")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int?>("SourceId")
                        .HasColumnType("integer");

                    b.Property<DateOnly>("StartDate")
                        .HasColumnType("date");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("TravelDuration")
                        .HasColumnType("interval");

                    b.Property<DateTime>("UtcDepartAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("UtcEndAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("UtcRegCloseAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("UtcStartAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("DestinationId");

                    b.HasIndex("SourceId");

                    b.ToTable("Plan");
                });

            modelBuilder.Entity("Domain.Entities.PlanMember", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("integer");

                    b.Property<List<string>>("Companions")
                        .HasColumnType("text[]");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("PlanId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Weight")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("PlanId");

                    b.ToTable("PlanMember");
                });

            modelBuilder.Entity("Domain.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PartySize")
                        .HasColumnType("integer");

                    b.Property<int[]>("Periods")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<int>("ProviderId")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProviderId");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("Domain.Entities.Provider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric");

                    b.Property<Point>("Coordinate")
                        .IsRequired()
                        .HasColumnType("geography (point)");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("Standard")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Provider");
                });

            modelBuilder.Entity("Domain.Entities.Province", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Region")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Province");
                });

            modelBuilder.Entity("Domain.Entities.StatisticalData", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Key");

                    b.ToTable("StatisticalData");
                });

            modelBuilder.Entity("Domain.Entities.Surcharge", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("AlreadyDivided")
                        .HasColumnType("boolean");

                    b.Property<decimal>("GcoinAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("ImagePath")
                        .HasColumnType("text");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PlanId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PlanId");

                    b.ToTable("Surcharge");
                });

            modelBuilder.Entity("Domain.Entities.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("AccountId")
                        .HasColumnType("integer");

                    b.Property<string>("BankTransCode")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Gateway")
                        .HasColumnType("integer");

                    b.Property<decimal>("GcoinAmount")
                        .HasColumnType("numeric");

                    b.Property<int?>("OrderId")
                        .HasColumnType("integer");

                    b.Property<int?>("PlanMemberId")
                        .HasColumnType("integer");

                    b.Property<int?>("ProviderId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("OrderId");

                    b.HasIndex("PlanMemberId");

                    b.HasIndex("ProviderId");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("Domain.Entities.Account", b =>
                {
                    b.HasOne("Domain.Entities.Provider", "Provider")
                        .WithOne("Account")
                        .HasForeignKey("Domain.Entities.Account", "ProviderId");

                    b.Navigation("Provider");
                });

            modelBuilder.Entity("Domain.Entities.Announcement", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("Announcements")
                        .HasForeignKey("AccountId");

                    b.HasOne("Domain.Entities.Order", "Order")
                        .WithMany("Announcements")
                        .HasForeignKey("OrderId");

                    b.HasOne("Domain.Entities.Plan", "Plan")
                        .WithMany("Announcements")
                        .HasForeignKey("PlanId");

                    b.HasOne("Domain.Entities.Provider", "Provider")
                        .WithMany()
                        .HasForeignKey("ProviderId");

                    b.Navigation("Account");

                    b.Navigation("Order");

                    b.Navigation("Plan");

                    b.Navigation("Provider");
                });

            modelBuilder.Entity("Domain.Entities.Destination", b =>
                {
                    b.HasOne("Domain.Entities.Province", "Province")
                        .WithMany("Destinations")
                        .HasForeignKey("ProvinceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Province");
                });

            modelBuilder.Entity("Domain.Entities.DestinationComment", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("DestinationComments")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Destination", "Destination")
                        .WithMany("Comments")
                        .HasForeignKey("DestinationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Destination");
                });

            modelBuilder.Entity("Domain.Entities.Order", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("Orders")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Plan", "Plan")
                        .WithMany("Orders")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Provider", "Provider")
                        .WithMany("Orders")
                        .HasForeignKey("ProviderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Domain.JsonEntities.OrderTrace", "Traces", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<string>("Description")
                                .HasColumnType("text");

                            b1.Property<bool>("IsClientAction")
                                .HasColumnType("boolean");

                            b1.Property<DateTime>("ModifiedAt")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<int>("Status")
                                .HasColumnType("integer");

                            b1.HasKey("OrderId", "Id");

                            b1.ToTable("Order");

                            b1.ToJson("Traces");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");
                        });

                    b.Navigation("Account");

                    b.Navigation("Plan");

                    b.Navigation("Provider");

                    b.Navigation("Traces");
                });

            modelBuilder.Entity("Domain.Entities.OrderDetail", b =>
                {
                    b.HasOne("Domain.Entities.Order", "Order")
                        .WithMany("Details")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Product", "Product")
                        .WithMany("OrderDetails")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Domain.Entities.Plan", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("Plans")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Destination", "Destination")
                        .WithMany("Plans")
                        .HasForeignKey("DestinationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Plan", "Source")
                        .WithMany("Copies")
                        .HasForeignKey("SourceId");

                    b.OwnsMany("Domain.JsonEntities.Contact", "SavedContacts", b1 =>
                        {
                            b1.Property<int>("PlanId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<string>("Address")
                                .HasColumnType("text");

                            b1.Property<Point>("Coordinate")
                                .HasColumnType("geometry");

                            b1.Property<string>("ImagePath")
                                .HasColumnType("text");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Phone")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<int>("Type")
                                .HasColumnType("integer");

                            b1.HasKey("PlanId", "Id");

                            b1.ToTable("Plan");

                            b1.ToJson("SavedContacts");

                            b1.WithOwner()
                                .HasForeignKey("PlanId");
                        });

                    b.Navigation("Account");

                    b.Navigation("Destination");

                    b.Navigation("SavedContacts");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("Domain.Entities.PlanMember", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("PlanMembers")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Plan", "Plan")
                        .WithMany("Members")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Plan");
                });

            modelBuilder.Entity("Domain.Entities.Product", b =>
                {
                    b.HasOne("Domain.Entities.Provider", "Provider")
                        .WithMany("Products")
                        .HasForeignKey("ProviderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Provider");
                });

            modelBuilder.Entity("Domain.Entities.Surcharge", b =>
                {
                    b.HasOne("Domain.Entities.Plan", "Plan")
                        .WithMany("Surcharges")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plan");
                });

            modelBuilder.Entity("Domain.Entities.Transaction", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("Transactions")
                        .HasForeignKey("AccountId");

                    b.HasOne("Domain.Entities.Order", "Order")
                        .WithMany("Transactions")
                        .HasForeignKey("OrderId");

                    b.HasOne("Domain.Entities.PlanMember", "PlanMember")
                        .WithMany("Transactions")
                        .HasForeignKey("PlanMemberId");

                    b.HasOne("Domain.Entities.Provider", "Provider")
                        .WithMany("Transactions")
                        .HasForeignKey("ProviderId");

                    b.Navigation("Account");

                    b.Navigation("Order");

                    b.Navigation("PlanMember");

                    b.Navigation("Provider");
                });

            modelBuilder.Entity("Domain.Entities.Account", b =>
                {
                    b.Navigation("Announcements");

                    b.Navigation("DestinationComments");

                    b.Navigation("Orders");

                    b.Navigation("PlanMembers");

                    b.Navigation("Plans");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("Domain.Entities.Destination", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Plans");
                });

            modelBuilder.Entity("Domain.Entities.Order", b =>
                {
                    b.Navigation("Announcements");

                    b.Navigation("Details");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("Domain.Entities.Plan", b =>
                {
                    b.Navigation("Announcements");

                    b.Navigation("Copies");

                    b.Navigation("Members");

                    b.Navigation("Orders");

                    b.Navigation("Surcharges");
                });

            modelBuilder.Entity("Domain.Entities.PlanMember", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("Domain.Entities.Product", b =>
                {
                    b.Navigation("OrderDetails");
                });

            modelBuilder.Entity("Domain.Entities.Provider", b =>
                {
                    b.Navigation("Account");

                    b.Navigation("Orders");

                    b.Navigation("Products");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("Domain.Entities.Province", b =>
                {
                    b.Navigation("Destinations");
                });
#pragma warning restore 612, 618
        }
    }
}
