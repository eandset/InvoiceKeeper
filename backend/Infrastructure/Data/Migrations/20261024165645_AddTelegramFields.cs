using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Keeper.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "telegram_chat_id",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "telegram_id",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_telegram_chat_id",
                table: "users",
                column: "telegram_chat_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_telegram_id",
                table: "users",
                column: "telegram_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_users_telegram_chat_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ux_users_telegram_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telegram_chat_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telegram_id",
                table: "users");
        }
    }
}
