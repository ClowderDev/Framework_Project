using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "revenue",
                table: "Statisticals",
                newName: "Revenue");

            migrationBuilder.RenameColumn(
                name: "orders",
                table: "Statisticals",
                newName: "Orders");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Statisticals",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Statisticals",
                newName: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Revenue",
                table: "Statisticals",
                newName: "revenue");

            migrationBuilder.RenameColumn(
                name: "Orders",
                table: "Statisticals",
                newName: "orders");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Statisticals",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Statisticals",
                newName: "date");
        }
    }
}
