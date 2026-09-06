using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rezerv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBookingToUserPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Packages_PackageId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "PackageId",
                table: "Bookings",
                newName: "UserPackageId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_PackageId",
                table: "Bookings",
                newName: "IX_Bookings_UserPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_UserPackages_UserPackageId",
                table: "Bookings",
                column: "UserPackageId",
                principalTable: "UserPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_UserPackages_UserPackageId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "UserPackageId",
                table: "Bookings",
                newName: "PackageId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_UserPackageId",
                table: "Bookings",
                newName: "IX_Bookings_PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Packages_PackageId",
                table: "Bookings",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
