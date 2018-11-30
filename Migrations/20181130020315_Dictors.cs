using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EnglishTraining.Migrations
{
    public partial class Dictors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DictorSex",
                table: "Settings",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dictors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Country = table.Column<string>(nullable: true),
                    Langname = table.Column<string>(nullable: true),
                    Sex = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dictors", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dictors");

            migrationBuilder.DropColumn(
                name: "DictorSex",
                table: "Settings");
        }
    }
}
