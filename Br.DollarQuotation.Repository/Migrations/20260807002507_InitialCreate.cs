using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Br.DollarQuotation.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currency_quotations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_pair = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bid_price = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    ask_price = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    high_price = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    low_price = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    variation = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    variation_percentage = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    quotation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_quotations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    photo_base64 = table.Column<string>(type: "text", nullable: true),
                    photo_content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_currency_quotations_currency_pair",
                table: "currency_quotations",
                column: "currency_pair");

            migrationBuilder.CreateIndex(
                name: "ix_currency_quotations_quotation_date",
                table: "currency_quotations",
                column: "quotation_date");

            migrationBuilder.CreateIndex(
                name: "ux_currency_quotations_pair_date",
                table: "currency_quotations",
                columns: new[] { "currency_pair", "quotation_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_quotations");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
