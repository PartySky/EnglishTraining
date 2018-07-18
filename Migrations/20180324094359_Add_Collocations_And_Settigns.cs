using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EnglishTraining.Migrations
{
    public partial class Add_Collocations_And_Settigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collocations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AudioUrl = table.Column<string>(nullable: true),
                    Lang = table.Column<string>(nullable: true),
                    NextRepeatDate = table.Column<DateTime>(nullable: false),
                    NotUsedToday = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyRepeatAmount = table.Column<int>(nullable: true),
                    DailyTimeAmount = table.Column<int>(nullable: true),
                    LearningLanguage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Collocations");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
