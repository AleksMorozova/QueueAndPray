using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueAndPray.Infrastructure.Jobs.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedAtUtcToJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "Jobs");
        }
    }
}
