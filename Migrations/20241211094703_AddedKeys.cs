using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmailLogUsers_EmailLogId",
                table: "EmailLogUsers",
                column: "EmailLogId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogUsers_UserId",
                table: "EmailLogUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_EmailId",
                table: "EmailLogs",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_Clicks_EmailLogId",
                table: "Clicks",
                column: "EmailLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clicks_EmailLogs_EmailLogId",
                table: "Clicks",
                column: "EmailLogId",
                principalTable: "EmailLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailLogs_Emails_EmailId",
                table: "EmailLogs",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailLogUsers_EmailLogs_EmailLogId",
                table: "EmailLogUsers",
                column: "EmailLogId",
                principalTable: "EmailLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailLogUsers_Users_UserId",
                table: "EmailLogUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clicks_EmailLogs_EmailLogId",
                table: "Clicks");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailLogs_Emails_EmailId",
                table: "EmailLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailLogUsers_EmailLogs_EmailLogId",
                table: "EmailLogUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailLogUsers_Users_UserId",
                table: "EmailLogUsers");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogUsers_EmailLogId",
                table: "EmailLogUsers");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogUsers_UserId",
                table: "EmailLogUsers");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_EmailId",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_Clicks_EmailLogId",
                table: "Clicks");
        }
    }
}
