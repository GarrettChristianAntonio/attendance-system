using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbedTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmbedTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SecretHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Scopes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbedTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmbedTokens_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmbedTokens_OrganizationId",
                table: "EmbedTokens",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmbedTokens");
        }
    }
}
