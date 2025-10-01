using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chrika.api.Migrations
{
    /// <inheritdoc />
    public partial class RenameGroupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupFollowers_Groups_GroupId",
                table: "GroupFollowers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupJoinRequests_Groups_GroupId",
                table: "GroupJoinRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Groups_GroupId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupPosts_Groups_GroupId",
                table: "GroupPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_OwnerId",
                table: "Groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "AppGroups");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_Username",
                table: "AppGroups",
                newName: "IX_AppGroups_Username");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_OwnerId",
                table: "AppGroups",
                newName: "IX_AppGroups_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppGroups",
                table: "AppGroups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppGroups_Users_OwnerId",
                table: "AppGroups",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFollowers_AppGroups_GroupId",
                table: "GroupFollowers",
                column: "GroupId",
                principalTable: "AppGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupJoinRequests_AppGroups_GroupId",
                table: "GroupJoinRequests",
                column: "GroupId",
                principalTable: "AppGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_AppGroups_GroupId",
                table: "GroupMembers",
                column: "GroupId",
                principalTable: "AppGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPosts_AppGroups_GroupId",
                table: "GroupPosts",
                column: "GroupId",
                principalTable: "AppGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppGroups_Users_OwnerId",
                table: "AppGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupFollowers_AppGroups_GroupId",
                table: "GroupFollowers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupJoinRequests_AppGroups_GroupId",
                table: "GroupJoinRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_AppGroups_GroupId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupPosts_AppGroups_GroupId",
                table: "GroupPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppGroups",
                table: "AppGroups");

            migrationBuilder.RenameTable(
                name: "AppGroups",
                newName: "Groups");

            migrationBuilder.RenameIndex(
                name: "IX_AppGroups_Username",
                table: "Groups",
                newName: "IX_Groups_Username");

            migrationBuilder.RenameIndex(
                name: "IX_AppGroups_OwnerId",
                table: "Groups",
                newName: "IX_Groups_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFollowers_Groups_GroupId",
                table: "GroupFollowers",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupJoinRequests_Groups_GroupId",
                table: "GroupJoinRequests",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Groups_GroupId",
                table: "GroupMembers",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPosts_Groups_GroupId",
                table: "GroupPosts",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_OwnerId",
                table: "Groups",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
