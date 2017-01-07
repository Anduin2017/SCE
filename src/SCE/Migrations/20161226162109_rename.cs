using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SCE.Migrations
{
    public partial class rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descriminator",
                table: "ProjectRecord");

            migrationBuilder.AddColumn<bool>(
                name: "Keyboarded",
                table: "ProjectRecord",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keyboarded",
                table: "ProjectRecord");

            migrationBuilder.AddColumn<string>(
                name: "Descriminator",
                table: "ProjectRecord",
                nullable: true);
        }
    }
}
