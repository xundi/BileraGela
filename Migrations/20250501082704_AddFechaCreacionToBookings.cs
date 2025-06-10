using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaCreacionToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                 name: "FechaCreacion",
                 table: "Bookings",
                 type: "datetime(6)",
                 nullable: false,
                 defaultValueSql: "CURRENT_TIMESTAMP");


            migrationBuilder.AddColumn<string>(
                name: "Sala",
                table: "Bookings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Usuario",
                table: "Bookings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Sala",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Usuario",
                table: "Bookings");
        }
    }
}
