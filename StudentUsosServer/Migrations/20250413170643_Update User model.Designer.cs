﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentUsosServer.Database;

#nullable disable

namespace StudentUsosServer.Migrations
{
    [DbContext(typeof(MainDBContext))]
    [Migration("20250413170643_Update User model")]
    partial class UpdateUsermodel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("StudentUsosServer.Models.AppLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CallerLineNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CallerName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreationDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("CreationDateUnix")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExceptionMessage")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ExceptionSerialized")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LogLevel")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserInstallation")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserUsosId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("AppLogs");
                });

            modelBuilder.Entity("StudentUsosServer.Models.User", b =>
                {
                    b.Property<int>("InternalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessTokenSecret")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<long>("CreationDateUnix")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FcmTokensJson")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Installation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("InternalAccessToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("InternalAccessTokenSecret")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastActiveDate")
                        .HasColumnType("TEXT");

                    b.Property<long>("LastActiveDateUnix")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StudentNumber")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "student_number");

                    b.Property<string>("USOSId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    b.HasKey("InternalId");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
