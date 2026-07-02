using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlAtaaClinic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Departments_BranchId",
                schema: "core",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                schema: "core",
                table: "Departments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "core",
                table: "Departments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "core",
                table: "Departments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BranchId_ArabicName",
                schema: "core",
                table: "Departments",
                columns: new[] { "BranchId", "ArabicName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Departments_BranchId_ArabicName",
                schema: "core",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "core",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "core",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                schema: "core",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BranchId",
                schema: "core",
                table: "Departments",
                column: "BranchId");
        }
    }
}
