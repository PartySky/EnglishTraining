using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EnglishTraining.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyReapeatCountForEng = table.Column<int>(nullable: false),
                    DailyReapeatCountForRus = table.Column<int>(nullable: false),
                    FourDausLearnPhase = table.Column<bool>(nullable: false),
                    LearnDay = table.Column<int>(nullable: false),
                    Name_en = table.Column<string>(nullable: true),
                    Name_ru = table.Column<string>(nullable: true),
                    NextRepeatDate = table.Column<string>(nullable: true),
                    RepeatIterationNum = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Words");
        }
    }
}
