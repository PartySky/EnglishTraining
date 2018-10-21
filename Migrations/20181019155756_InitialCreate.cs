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

            migrationBuilder.CreateTable(
                name: "WordLocalization",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name_en = table.Column<string>(nullable: true),
                    Name_pl = table.Column<string>(nullable: true),
                    Name_ru = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordLocalization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyReapeatCountForEngOld = table.Column<int>(nullable: false),
                    DailyReapeatCountForRusOld = table.Column<int>(nullable: false),
                    FourDaysLearnPhaseOld = table.Column<bool>(nullable: false),
                    LearnDayOld = table.Column<int>(nullable: false),
                    LocalizationId = table.Column<int>(nullable: true),
                    Name_en = table.Column<string>(nullable: true),
                    Name_ru = table.Column<string>(nullable: true),
                    NextRepeatDateOld = table.Column<DateTime>(nullable: false),
                    RepeatIterationNumOld = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Words_WordLocalization_LocalizationId",
                        column: x => x.LocalizationId,
                        principalTable: "WordLocalization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyReapeatCount",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    WordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReapeatCount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReapeatCount_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FourDaysLearnPhase",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<bool>(nullable: false),
                    WordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FourDaysLearnPhase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FourDaysLearnPhase_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearnDay",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    WordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnDay_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NextRepeatDate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<DateTime>(nullable: false),
                    WordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NextRepeatDate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NextRepeatDate_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RepeatIterationNum",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    WordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepeatIterationNum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepeatIterationNum_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReapeatCount_WordId",
                table: "DailyReapeatCount",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_FourDaysLearnPhase_WordId",
                table: "FourDaysLearnPhase",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnDay_WordId",
                table: "LearnDay",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_NextRepeatDate_WordId",
                table: "NextRepeatDate",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_RepeatIterationNum_WordId",
                table: "RepeatIterationNum",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_Words_LocalizationId",
                table: "Words",
                column: "LocalizationId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Collocations");

            migrationBuilder.DropTable(
                name: "DailyReapeatCount");

            migrationBuilder.DropTable(
                name: "FourDaysLearnPhase");

            migrationBuilder.DropTable(
                name: "LearnDay");

            migrationBuilder.DropTable(
                name: "NextRepeatDate");

            migrationBuilder.DropTable(
                name: "RepeatIterationNum");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "WordLocalization");
        }
    }
}
