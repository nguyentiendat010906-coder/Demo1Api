using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo1Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierNameToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CashierName",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashierName",
                table: "Invoices");
        }
    }
}
