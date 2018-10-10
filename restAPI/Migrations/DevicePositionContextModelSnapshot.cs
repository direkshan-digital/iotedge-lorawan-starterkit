﻿// <auto-generated />
using System;
using restAPI.DataContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace restAPI.Migrations
{
    [DbContext(typeof(DevicePositionContext))]
    partial class DevicePositionContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("DataContracts.Controllers.DeviceMapPoint", b =>
                {
                    b.Property<ulong>("EUI")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("eui");

                    b.Property<uint>("ID")
                        .HasColumnName("id");

                    b.Property<double>("Latitude")
                        .HasColumnName("Latitude");

                    b.Property<double>("Longitute")
                        .HasColumnName("Longitude");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnName("TimeStamp");

                    b.HasKey("EUI");

                    b.ToTable("DeviceMapPoints");
                });
#pragma warning restore 612, 618
        }
    }
}
