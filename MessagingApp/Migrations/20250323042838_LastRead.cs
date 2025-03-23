using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessagingApp.Migrations
{
    /// <inheritdoc />
    public partial class LastRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastRead",
                table: "ConversationParticipants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRead",
                table: "ConversationParticipants");
        }
    }
}
