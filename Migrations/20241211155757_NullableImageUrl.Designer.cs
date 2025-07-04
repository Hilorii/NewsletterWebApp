﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NewsletterWebApp.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NewsletterWebApp.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20241211155757_NullableImageUrl")]
    partial class NullableImageUrl
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseSerialColumns(modelBuilder);

            modelBuilder.Entity("NewsletterWebApp.Data.Click", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("EmailLogId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("EmailLogId");

                    b.ToTable("Clicks");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.Email", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("text");

                    b.Property<bool>("IsNewsletter")
                        .HasColumnType("boolean");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.EmailLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<int>("EmailId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("SentAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("EmailId");

                    b.ToTable("EmailLogs");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.EmailLogUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<int>("EmailLogId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("EmailLogId");

                    b.HasIndex("UserId");

                    b.ToTable("EmailLogUsers");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<bool>("Admin")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Subscribed")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.Click", b =>
                {
                    b.HasOne("NewsletterWebApp.Data.EmailLog", "EmailLog")
                        .WithMany("Clicks")
                        .HasForeignKey("EmailLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EmailLog");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.EmailLog", b =>
                {
                    b.HasOne("NewsletterWebApp.Data.Email", "Email")
                        .WithMany("EmailLogs")
                        .HasForeignKey("EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Email");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.EmailLogUser", b =>
                {
                    b.HasOne("NewsletterWebApp.Data.EmailLog", "EmailLog")
                        .WithMany("EmailLogUsers")
                        .HasForeignKey("EmailLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NewsletterWebApp.Data.User", "User")
                        .WithMany("EmailLogUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EmailLog");

                    b.Navigation("User");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.Email", b =>
                {
                    b.Navigation("EmailLogs");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.EmailLog", b =>
                {
                    b.Navigation("Clicks");

                    b.Navigation("EmailLogUsers");
                });

            modelBuilder.Entity("NewsletterWebApp.Data.User", b =>
                {
                    b.Navigation("EmailLogUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
