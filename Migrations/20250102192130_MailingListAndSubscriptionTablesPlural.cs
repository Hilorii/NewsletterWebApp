using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterWebApp.Migrations
{
    /// <inheritdoc />
    public partial class MailingListAndSubscriptionTablesPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_MailingList_MailingListId",
                table: "Subscription");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_Users_UserId",
                table: "Subscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscription",
                table: "Subscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MailingList",
                table: "MailingList");

            migrationBuilder.RenameTable(
                name: "Subscription",
                newName: "Subscriptions");

            migrationBuilder.RenameTable(
                name: "MailingList",
                newName: "MailingLists");

            migrationBuilder.RenameIndex(
                name: "IX_Subscription_UserId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                columns: new[] { "MailingListId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MailingLists",
                table: "MailingLists",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_MailingLists_MailingListId",
                table: "Subscriptions",
                column: "MailingListId",
                principalTable: "MailingLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_MailingLists_MailingListId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MailingLists",
                table: "MailingLists");

            migrationBuilder.RenameTable(
                name: "Subscriptions",
                newName: "Subscription");

            migrationBuilder.RenameTable(
                name: "MailingLists",
                newName: "MailingList");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscription",
                newName: "IX_Subscription_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscription",
                table: "Subscription",
                columns: new[] { "MailingListId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MailingList",
                table: "MailingList",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_MailingList_MailingListId",
                table: "Subscription",
                column: "MailingListId",
                principalTable: "MailingList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_Users_UserId",
                table: "Subscription",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
