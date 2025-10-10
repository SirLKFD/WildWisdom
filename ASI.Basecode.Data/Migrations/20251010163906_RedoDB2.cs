using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class RedoDB2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrgBooks_Category",
                table: "OrgBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgBooks_Publisher",
                table: "OrgBooks");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "OrgBooks",
                newName: "BookCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_OrgBooks_CategoryId",
                table: "OrgBooks",
                newName: "IX_OrgBooks_BookCategoryId");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "OrgBooks",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genre",
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

            migrationBuilder.AddForeignKey(
                name: "FK_OrgBooks_BookCategories_BookCategoryId",
                table: "OrgBooks",
                column: "BookCategoryId",
                principalTable: "BookCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrgBooks_Publishers_PublisherId",
                table: "OrgBooks",
                column: "PublisherId",
                principalTable: "Publishers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrgBooks_BookCategories_BookCategoryId",
                table: "OrgBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgBooks_Publishers_PublisherId",
                table: "OrgBooks");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "OrgBooks");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "OrgBooks");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "OrgBooks");

            migrationBuilder.RenameColumn(
                name: "BookCategoryId",
                table: "OrgBooks",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_OrgBooks_BookCategoryId",
                table: "OrgBooks",
                newName: "IX_OrgBooks_CategoryId");

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
    }
}
