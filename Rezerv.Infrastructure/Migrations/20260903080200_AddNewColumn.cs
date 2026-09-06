using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rezerv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPackage_Packages_PackageId",
                table: "UserPackage");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPackage_Users_UserId",
                table: "UserPackage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPackage",
                table: "UserPackage");

            migrationBuilder.RenameTable(
                name: "UserPackage",
                newName: "UserPackages");

            migrationBuilder.RenameIndex(
                name: "IX_UserPackage_UserId",
                table: "UserPackages",
                newName: "IX_UserPackages_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPackage_PackageId",
                table: "UserPackages",
                newName: "IX_UserPackages_PackageId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Packages",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPackages",
                table: "UserPackages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPackages_Packages_PackageId",
                table: "UserPackages",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPackages_Users_UserId",
                table: "UserPackages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPackages_Packages_PackageId",
                table: "UserPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPackages_Users_UserId",
                table: "UserPackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPackages",
                table: "UserPackages");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Packages");

            migrationBuilder.RenameTable(
                name: "UserPackages",
                newName: "UserPackage");

            migrationBuilder.RenameIndex(
                name: "IX_UserPackages_UserId",
                table: "UserPackage",
                newName: "IX_UserPackage_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPackages_PackageId",
                table: "UserPackage",
                newName: "IX_UserPackage_PackageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPackage",
                table: "UserPackage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPackage_Packages_PackageId",
                table: "UserPackage",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPackage_Users_UserId",
                table: "UserPackage",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
