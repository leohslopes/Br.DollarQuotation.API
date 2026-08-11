using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Br.DollarQuotation.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quotation_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_pair = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    condition = table.Column<int>(type: "integer", nullable: false),
                    target_price = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    triggered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotation_alerts", x => x.id);
                    table.ForeignKey(
                        name: "FK_quotation_alerts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_quotation_alerts_currency_pair",
                table: "quotation_alerts",
                column: "currency_pair");

            migrationBuilder.CreateIndex(
                name: "ix_quotation_alerts_is_active",
                table: "quotation_alerts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_quotation_alerts_user_id",
                table: "quotation_alerts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotation_alerts_user_pair_active",
                table: "quotation_alerts",
                columns: new[] { "user_id", "currency_pair", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quotation_alerts");
        }
    }
}
