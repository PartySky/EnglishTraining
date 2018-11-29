using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EnglishTraining.Migrations
{
    public partial class Removed_old_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "DailyReapeatCountForEngOld",
            //     table: "Words");

            // migrationBuilder.DropColumn(
            //     name: "DailyReapeatCountForRusOld",
            //     table: "Words");

            // migrationBuilder.DropColumn(
            //     name: "FourDaysLearnPhaseOld",
            //     table: "Words");

            // migrationBuilder.DropColumn(
            //     name: "LearnDayOld",
            //     table: "Words");

            // migrationBuilder.DropColumn(
            //     name: "NextRepeatDateOld",
            //     table: "Words");

            // migrationBuilder.DropColumn(
            //     name: "RepeatIterationNumOld",
            //     table: "Words");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<int>(
            //     name: "DailyReapeatCountForEngOld",
            //     table: "Words",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.AddColumn<int>(
            //     name: "DailyReapeatCountForRusOld",
            //     table: "Words",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.AddColumn<bool>(
            //     name: "FourDaysLearnPhaseOld",
            //     table: "Words",
            //     nullable: false,
            //     defaultValue: false);

            // migrationBuilder.AddColumn<int>(
            //     name: "LearnDayOld",
            //     table: "Words",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "NextRepeatDateOld",
            //     table: "Words",
            //     nullable: false,
            //     defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // migrationBuilder.AddColumn<int>(
            //     name: "RepeatIterationNumOld",
            //     table: "Words",
            //     nullable: false,
            //     defaultValue: 0);
        }
    }
}
