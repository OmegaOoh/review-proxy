using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace identity.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GitHubUsername = table.Column<string>(type: "text", nullable: false),
                    GitHubAvatarUrl = table.Column<string>(type: "text", nullable: true),
                    GitHubID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_GitHubID",
                table: "Repositories",
                column: "GitHubID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repositories");
        }
    }
}
