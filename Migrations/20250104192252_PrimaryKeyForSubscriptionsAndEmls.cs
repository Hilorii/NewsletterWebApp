using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NewsletterWebApp.Migrations
{
    /// <inheritdoc />
    public partial class PrimaryKeyForSubscriptionsAndEmls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailMailingLists",
                table: "EmailMailingLists");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "EmailMailingLists",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailMailingLists",
                table: "EmailMailingLists",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_MailingListId",
                table: "Subscriptions",
                column: "MailingListId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMailingLists_EmailId",
                table: "EmailMailingLists",
                column: "EmailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_MailingListId",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailMailingLists",
                table: "EmailMailingLists");

            migrationBuilder.DropIndex(
                name: "IX_EmailMailingLists_EmailId",
                table: "EmailMailingLists");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EmailMailingLists");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                columns: new[] { "MailingListId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailMailingLists",
                table: "EmailMailingLists",
                columns: new[] { "EmailId", "MailingListId" });
        }
    }
}
