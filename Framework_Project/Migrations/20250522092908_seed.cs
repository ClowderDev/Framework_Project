using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework_Project.Migrations
{
    /// <inheritdoc />
    public partial class seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Contact",
                columns: new[] { "Id", "Description", "Email", "LogoImg", "Map", "Name", "Phone" },
                values: new object[] { 1, "Welcome to Framework Shop - Your one-stop destination for quality products.", "contact@frameworkshop.com", "eb7ec5ad-9a7c-4f0d-b744-c4edd6d7387e_logo.jpg", "<iframe src=\"https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d13178.937968367609!2d106.80472990520988!3d10.878347242885324!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x317527587e9ad5bf%3A0xafa66f9c8be3c91!2sUniversity%20of%20Information%20Technology%20-%20VNUHCM!5e0!3m2!1sen!2s!4v1747905663960!5m2!1sen!2s\" width=\"400\" height=\"300\" style=\"border:0;\" allowfullscreen=\"\" loading=\"lazy\" referrerpolicy=\"no-referrer-when-downgrade\"></iframe>", "Framework Shop", "+1 234 567 890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Contact",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
