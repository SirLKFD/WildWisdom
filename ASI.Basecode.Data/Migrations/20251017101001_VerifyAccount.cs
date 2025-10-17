using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class VerifyAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Admins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PendingUserRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    VerificationToken = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    TokenExpiration = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    OrganizationName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    OrganizationType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingUserRegistrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__PendingUserRegistrations__EmailId",
                table: "PendingUserRegistrations",
                column: "EmailId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingUserRegistrations");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Admins");
        }
    }
}
