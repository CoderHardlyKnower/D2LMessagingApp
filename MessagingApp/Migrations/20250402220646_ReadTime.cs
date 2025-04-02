using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessagingApp.Migrations
{
    /// <inheritdoc />
    public partial class ReadTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReadTime",
                table: "Messages",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadTime",
                table: "Messages");
        }
    }
}
