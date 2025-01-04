using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterWebApp.Migrations
{
    /// <inheritdoc />
    public partial class EmailAndMailingListRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailMailingLists",
                columns: table => new
                {
                    EmailId = table.Column<int>(type: "integer", nullable: false),
                    MailingListId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMailingLists", x => new { x.EmailId, x.MailingListId });
                    table.ForeignKey(
                        name: "FK_EmailMailingLists_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailMailingLists_MailingLists_MailingListId",
                        column: x => x.MailingListId,
                        principalTable: "MailingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMailingLists_MailingListId",
                table: "EmailMailingLists",
                column: "MailingListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailMailingLists");
        }
    }
}
