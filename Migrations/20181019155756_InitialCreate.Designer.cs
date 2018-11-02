﻿// <auto-generated />
using EnglishTraining;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace EnglishTraining.Migrations
{
    [DbContext(typeof(WordContext))]
    [Migration("20181019155756_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("EnglishTraining.DailyReapeatCount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<int>("Value");

                    b.Property<int?>("WordId");

                    b.HasKey("Id");

                    b.HasIndex("WordId");

                    b.ToTable("DailyReapeatCount");
                });

            modelBuilder.Entity("EnglishTraining.FourDaysLearnPhase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<bool>("Value");

                    b.Property<int?>("WordId");

                    b.HasKey("Id");

                    b.HasIndex("WordId");

                    b.ToTable("FourDaysLearnPhase");
                });

            modelBuilder.Entity("EnglishTraining.LearnDay", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<int>("Value");

                    b.Property<int?>("WordId");

                    b.HasKey("Id");

                    b.HasIndex("WordId");

                    b.ToTable("LearnDay");
                });

            modelBuilder.Entity("EnglishTraining.models.VmSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("DailyRepeatAmount");

                    b.Property<int?>("DailyTimeAmount");

                    b.Property<string>("LearningLanguage");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("EnglishTraining.NextRepeatDate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<DateTime>("Value");

                    b.Property<int?>("WordId");

                    b.HasKey("Id");

                    b.HasIndex("WordId");

                    b.ToTable("NextRepeatDate");
                });

            modelBuilder.Entity("EnglishTraining.RepeatIterationNum", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<int>("Value");

                    b.Property<int?>("WordId");

                    b.HasKey("Id");

                    b.HasIndex("WordId");

                    b.ToTable("RepeatIterationNum");
                });

            modelBuilder.Entity("EnglishTraining.VmCollocation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AudioUrl");

                    b.Property<string>("Lang");

                    b.Property<DateTime>("NextRepeatDate");

                    b.Property<bool>("NotUsedToday");

                    b.HasKey("Id");

                    b.ToTable("Collocations");
                });

            modelBuilder.Entity("EnglishTraining.VmWord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DailyReapeatCountForEngOld");

                    b.Property<int>("DailyReapeatCountForRusOld");

                    b.Property<bool>("FourDaysLearnPhaseOld");

                    b.Property<int>("LearnDayOld");

                    b.Property<int?>("LocalizationId");

                    b.Property<string>("Name_en");

                    b.Property<string>("Name_ru");

                    b.Property<DateTime>("NextRepeatDateOld");

                    b.Property<int>("RepeatIterationNumOld");

                    b.HasKey("Id");

                    b.HasIndex("LocalizationId")
                        .IsUnique();

                    b.ToTable("Words");
                });

            modelBuilder.Entity("EnglishTraining.VmWordLocalization", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name_en");

                    b.Property<string>("Name_pl");

                    b.Property<string>("Name_ru");

                    b.HasKey("Id");

                    b.ToTable("WordLocalization");
                });

            modelBuilder.Entity("EnglishTraining.DailyReapeatCount", b =>
                {
                    b.HasOne("EnglishTraining.VmWord", "Word")
                        .WithMany("DailyReapeatCount")
                        .HasForeignKey("WordId");
                });

            modelBuilder.Entity("EnglishTraining.FourDaysLearnPhase", b =>
                {
                    b.HasOne("EnglishTraining.VmWord", "Word")
                        .WithMany("FourDaysLearnPhase")
                        .HasForeignKey("WordId");
                });

            modelBuilder.Entity("EnglishTraining.LearnDay", b =>
                {
                    b.HasOne("EnglishTraining.VmWord", "Word")
                        .WithMany("LearnDay")
                        .HasForeignKey("WordId");
                });

            modelBuilder.Entity("EnglishTraining.NextRepeatDate", b =>
                {
                    b.HasOne("EnglishTraining.VmWord", "Word")
                        .WithMany("NextRepeatDate")
                        .HasForeignKey("WordId");
                });

            modelBuilder.Entity("EnglishTraining.RepeatIterationNum", b =>
                {
                    b.HasOne("EnglishTraining.VmWord", "Word")
                        .WithMany("RepeatIterationNum")
                        .HasForeignKey("WordId");
                });

            modelBuilder.Entity("EnglishTraining.VmWord", b =>
                {
                    b.HasOne("EnglishTraining.VmWordLocalization", "Localization")
                        .WithOne("word")
                        .HasForeignKey("EnglishTraining.VmWord", "LocalizationId");
                });
#pragma warning restore 612, 618
        }
    }
}
