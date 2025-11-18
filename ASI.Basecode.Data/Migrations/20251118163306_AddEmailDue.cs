using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailDue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceiveOverdueEmails",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "OverdueReminderLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BorrowTicketId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    NextSendAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReminderCount = table.Column<int>(type: "int", nullable: false),
                    LastFineAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdueReminderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverdueReminderLog_BorrowTicket",
                        column: x => x.BorrowTicketId,
                        principalTable: "BorrowTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OverdueReminderLog_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OverdueReminderLogs_BorrowTicketId",
                table: "OverdueReminderLogs",
                column: "BorrowTicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OverdueReminderLogs_UserId",
                table: "OverdueReminderLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OverdueReminderLogs");

            migrationBuilder.DropColumn(
                name: "ReceiveOverdueEmails",
                table: "Users");
        }
    }
}
