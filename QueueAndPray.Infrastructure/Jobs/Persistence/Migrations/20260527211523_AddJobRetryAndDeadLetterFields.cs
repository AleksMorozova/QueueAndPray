using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueAndPray.Infrastructure.Jobs.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobRetryAndDeadLetterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeadLetteredAtUtc",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstFailedAtUtc",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadLetteredAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "FirstFailedAtUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Jobs");
        }
    }
}
