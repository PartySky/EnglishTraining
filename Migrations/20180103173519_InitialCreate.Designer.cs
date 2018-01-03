﻿// <auto-generated />
using EnglishTraining;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace EnglishTraining.Migrations
{
    [DbContext(typeof(WordContext))]
    [Migration("20180103173519_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("EnglishTraining.VmWord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DailyReapeatCountForEng");

                    b.Property<int>("DailyReapeatCountForRus");

                    b.Property<bool>("FourDausLearnPhase");

                    b.Property<int>("LearnDay");

                    b.Property<string>("Name_en");

                    b.Property<string>("Name_ru");

                    b.Property<string>("NextRepeatDate");

                    b.Property<int>("RepeatIterationNum");

                    b.HasKey("Id");

                    b.ToTable("Words");
                });
#pragma warning restore 612, 618
        }
    }
}
