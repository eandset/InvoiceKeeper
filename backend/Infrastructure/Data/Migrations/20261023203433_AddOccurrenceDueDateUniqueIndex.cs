using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Keeper.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurrenceDueDateUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payment_occurrences_recurring_payment_id",
                table: "payment_occurrences");

            migrationBuilder.CreateIndex(
                name: "ux_occurrences_payment_due_date",
                table: "payment_occurrences",
                columns: new[] { "recurring_payment_id", "due_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_occurrences_payment_due_date",
                table: "payment_occurrences");

            migrationBuilder.CreateIndex(
                name: "IX_payment_occurrences_recurring_payment_id",
                table: "payment_occurrences",
                column: "recurring_payment_id");
        }
    }
}
