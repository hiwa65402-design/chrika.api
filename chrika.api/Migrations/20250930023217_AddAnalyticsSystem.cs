using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chrika.api.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AdCampaigns",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AdInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AdCampaignId = table.Column<int>(type: "int", nullable: false),
                    InteractingUserId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdInteractions_AdCampaigns_AdCampaignId",
                        column: x => x.AdCampaignId,
                        principalTable: "AdCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdInteractions_Users_InteractingUserId",
                        column: x => x.InteractingUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AdInteractions_AdCampaignId",
                table: "AdInteractions",
                column: "AdCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_AdInteractions_InteractingUserId",
                table: "AdInteractions",
                column: "InteractingUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdInteractions");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AdCampaigns",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
