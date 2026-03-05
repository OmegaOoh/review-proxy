using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace identity.Migrations
{
    /// <inheritdoc />
    public partial class fixusers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Repositories",
                table: "Repositories");

            migrationBuilder.RenameTable(
                name: "Repositories",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_Repositories_GitHubID",
                table: "Users",
                newName: "IX_Users_GitHubID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Repositories");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GitHubID",
                table: "Repositories",
                newName: "IX_Repositories_GitHubID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Repositories",
                table: "Repositories",
                column: "Id");
        }
    }
}
