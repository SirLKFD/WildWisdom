using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReDoDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationPolicies");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "OrgBooks");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "OrgBooks");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "OrgBooks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "OrgBooks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BorrowingPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    BorrowLimitPerUser = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BorrowDaysDefault = table.Column<int>(type: "int", nullable: false, defaultValue: 14),
                    MaxActiveTicketsPerUser = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowingPolicy_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ClaimWindowHours = table.Column<int>(type: "int", nullable: false, defaultValue: 48),
                    AllowOverlappingReservations = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimPolicy_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    FinePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 5.00m),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinePolicy_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RenewalPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    AllowRenewals = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxRenewals = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewalPolicy_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrgBooks_CategoryId",
                table: "OrgBooks",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgBooks_PublisherId",
                table: "OrgBooks",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "UQ__BorrowingPolicy__OrganizationId",
                table: "BorrowingPolicies",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__ClaimPolicy__OrganizationId",
                table: "ClaimPolicies",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__FinePolicy__OrganizationId",
                table: "FinePolicies",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__RenewalPolicy__OrganizationId",
                table: "RenewalPolicies",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrgBooks_Category",
                table: "OrgBooks",
                column: "CategoryId",
                principalTable: "BookCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrgBooks_Publisher",
                table: "OrgBooks",
                column: "PublisherId",
                principalTable: "Publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrgBooks_Category",
                table: "OrgBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgBooks_Publisher",
                table: "OrgBooks");

            migrationBuilder.DropTable(
                name: "BookCategories");

            migrationBuilder.DropTable(
                name: "BorrowingPolicies");

            migrationBuilder.DropTable(
                name: "ClaimPolicies");

            migrationBuilder.DropTable(
                name: "FinePolicies");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "RenewalPolicies");

            migrationBuilder.DropIndex(
                name: "IX_OrgBooks_CategoryId",
                table: "OrgBooks");

            migrationBuilder.DropIndex(
                name: "IX_OrgBooks_PublisherId",
                table: "OrgBooks");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "OrgBooks");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "OrgBooks");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "OrgBooks",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "OrgBooks",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    AllowOverlappingReservations = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowRenewals = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    BorrowDaysDefault = table.Column<int>(type: "int", nullable: false, defaultValue: 14),
                    BorrowLimitPerUser = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    ClaimWindowHours = table.Column<int>(type: "int", nullable: false, defaultValue: 48),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    FinePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxActiveTicketsPerUser = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    MaxRenewals = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
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

            migrationBuilder.CreateIndex(
                name: "UQ__OrganizationPolicy__OrganizationId",
                table: "OrganizationPolicies",
                column: "OrganizationId",
                unique: true);
        }
    }
}
