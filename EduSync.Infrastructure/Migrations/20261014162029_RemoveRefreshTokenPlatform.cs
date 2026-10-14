using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRefreshTokenPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.check_constraints
                    WHERE parent_object_id = OBJECT_ID(N'dbo.RefreshTokens')
                        AND name = N'CK_RefreshTokens_Platform')
                    ALTER TABLE dbo.RefreshTokens DROP CONSTRAINT CK_RefreshTokens_Platform;
                """);

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "RefreshTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "RefreshTokens",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Web");
        }
    }
}
