using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Keeper.Infrastructure.Data.Migrations
{
    public partial class AddOccurrenceStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_occurrences_is_paid",
                table: "payment_occurrences");

            migrationBuilder.DropColumn(
                name: "is_paid",
                table: "payment_occurrences");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "payment_occurrences",
                type: "text",
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.CreateIndex(
                name: "idx_occurrences_status",
                table: "payment_occurrences",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_occurrences_status",
                table: "payment_occurrences");

            migrationBuilder.DropColumn(
                name: "status",
                table: "payment_occurrences");

            migrationBuilder.AddColumn<bool>(
                name: "is_paid",
                table: "payment_occurrences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_occurrences_is_paid",
                table: "payment_occurrences",
                column: "is_paid");
        }
    }
}

