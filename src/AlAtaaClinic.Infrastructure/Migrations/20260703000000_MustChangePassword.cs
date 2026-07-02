using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlAtaaClinic.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MustChangePassword : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "MustChangePassword",
            schema: "security",
            table: "UserAccounts",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MustChangePassword",
            schema: "security",
            table: "UserAccounts");
    }
}
