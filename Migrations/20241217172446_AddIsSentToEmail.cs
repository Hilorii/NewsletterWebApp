using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSentToEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSent",
                table: "Emails",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSent",
                table: "Emails");
        }
    }
}
