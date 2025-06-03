using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatisticalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "message",
                table: "Statisticals",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "message",
                table: "Statisticals");
        }
    }
}
