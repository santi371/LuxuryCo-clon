using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LuxuryCo.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "producto",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "inventario_sede",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ai_action_log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    SessionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PromptOriginal = table.Column<string>(type: "text", nullable: false),
                    SanitizedPrompt = table.Column<string>(type: "text", nullable: false),
                    IntentDetected = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModelUsed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActionExecuted = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BeforeState = table.Column<string>(type: "text", nullable: false),
                    AfterState = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    TraceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_action_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_action_log_usuario_UserId",
                        column: x => x.UserId,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "ai_image_generation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    PromptOriginal = table.Column<string>(type: "text", nullable: false),
                    OptimizedPrompt = table.Column<string>(type: "text", nullable: false),
                    NegativePrompt = table.Column<string>(type: "text", nullable: false),
                    Seed = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GenerationTimeMs = table.Column<double>(type: "double precision", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_image_generation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_image_generation_usuario_UserId",
                        column: x => x.UserId,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_action_log_UserId",
                table: "ai_action_log",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_image_generation_UserId",
                table: "ai_image_generation",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_action_log");

            migrationBuilder.DropTable(
                name: "ai_image_generation");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "inventario_sede");
        }
    }
}
