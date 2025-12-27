using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo1Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEndTimeToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Invoices",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Invoices");
        }
    }
}
