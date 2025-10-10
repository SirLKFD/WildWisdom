using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialLibrarySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    BorrowLimitPerUser = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BorrowDaysDefault = table.Column<int>(type: "int", nullable: false, defaultValue: 14),
                    FinePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClaimWindowHours = table.Column<int>(type: "int", nullable: false, defaultValue: 48),
                    MaxActiveTicketsPerUser = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    AllowRenewals = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxRenewals = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    AllowOverlappingReservations = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationPolicy_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrgBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ISBN = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Publisher = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    YearPublished = table.Column<int>(type: "int", nullable: true),
                    Edition = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    CopiesTotal = table.Column<int>(type: "int", nullable: false),
                    CopiesAvailable = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrgBooks_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BorrowTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    OrgBookId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DueAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ClaimedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    CheckedOutByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ReturnedToEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    BorrowDays = table.Column<int>(type: "int", nullable: false),
                    FinePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxRenewals = table.Column<int>(type: "int", nullable: false),
                    RenewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AccruedFine = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsFinePaid = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowTicket_CheckedOutByEmployee",
                        column: x => x.CheckedOutByEmployeeId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowTicket_ClaimedByEmployee",
                        column: x => x.ClaimedByEmployeeId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowTicket_OrgBook",
                        column: x => x.OrgBookId,
                        principalTable: "OrgBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowTicket_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowTicket_ReturnedToEmployee",
                        column: x => x.ReturnedToEmployeeId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowTicket_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTickets_CheckedOutByEmployeeId",
                table: "BorrowTickets",
                column: "CheckedOutByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTickets_ClaimedByEmployeeId",
                table: "BorrowTickets",
                column: "ClaimedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTickets_OrganizationId",
                table: "BorrowTickets",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTickets_OrgBookId",
                table: "BorrowTickets",
                column: "OrgBookId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTickets_ReturnedToEmployeeId",
                table: "BorrowTickets",
                column: "ReturnedToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTickets_UserId",
                table: "BorrowTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__BorrowTicket__ReferenceCode",
                table: "BorrowTickets",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__OrganizationPolicy__OrganizationId",
                table: "OrganizationPolicies",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgBooks_OrganizationId",
                table: "OrgBooks",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowTickets");

            migrationBuilder.DropTable(
                name: "OrganizationPolicies");

            migrationBuilder.DropTable(
                name: "OrgBooks");
        }
    }
}
