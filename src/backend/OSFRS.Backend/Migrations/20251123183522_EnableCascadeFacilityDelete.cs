using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSFRS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeFacilityDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Facilities_FacilityId1",
                table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Facilities_FacilityId1",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageRecords_Facilities_FacilityId",
                table: "UsageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageRecords_Users_UserId",
                table: "UsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_FacilityId1",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_FacilityId1",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "FacilityId1",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "FacilityId1",
                table: "MaintenanceRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_UsageRecords_Facilities_FacilityId",
                table: "UsageRecords",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageRecords_Users_UserId",
                table: "UsageRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsageRecords_Facilities_FacilityId",
                table: "UsageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageRecords_Users_UserId",
                table: "UsageRecords");

            migrationBuilder.AddColumn<int>(
                name: "FacilityId1",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityId1",
                table: "MaintenanceRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_FacilityId1",
                table: "Reservations",
                column: "FacilityId1");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_FacilityId1",
                table: "MaintenanceRecords",
                column: "FacilityId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Facilities_FacilityId1",
                table: "MaintenanceRecords",
                column: "FacilityId1",
                principalTable: "Facilities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Facilities_FacilityId1",
                table: "Reservations",
                column: "FacilityId1",
                principalTable: "Facilities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsageRecords_Facilities_FacilityId",
                table: "UsageRecords",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageRecords_Users_UserId",
                table: "UsageRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
