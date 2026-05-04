using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuxuryCo.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddHistorialChatAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_sede",
                table: "usuario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                table: "imagen_producto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HistorialesChatAi",
                columns: table => new
                {
                    id_historial = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    session_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialesChatAi", x => x.id_historial);
                    table.ForeignKey(
                        name: "FK_HistorialesChatAi_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "proveedor",
                columns: table => new
                {
                    id_proveedor = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contacto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedor", x => x.id_proveedor);
                });

            migrationBuilder.CreateTable(
                name: "sede",
                columns: table => new
                {
                    id_sede = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    direccion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sede", x => x.id_sede);
                });

            migrationBuilder.CreateTable(
                name: "producto_proveedor",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_proveedor = table.Column<int>(type: "integer", nullable: false),
                    precio_costo = table.Column<decimal>(type: "numeric", nullable: false),
                    tiempo_entrega_dias = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_proveedor", x => new { x.id_producto, x.id_proveedor });
                    table.ForeignKey(
                        name: "FK_producto_proveedor_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_producto_proveedor_proveedor_id_proveedor",
                        column: x => x.id_proveedor,
                        principalTable: "proveedor",
                        principalColumn: "id_proveedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historial_abastecimiento",
                columns: table => new
                {
                    id_abastecimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_proveedor = table.Column<int>(type: "integer", nullable: true),
                    id_sede = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_registra = table.Column<int>(type: "integer", nullable: true),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    costo_unitario = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    notas = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_abastecimiento", x => x.id_abastecimiento);
                    table.ForeignKey(
                        name: "FK_historial_abastecimiento_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historial_abastecimiento_proveedor_id_proveedor",
                        column: x => x.id_proveedor,
                        principalTable: "proveedor",
                        principalColumn: "id_proveedor");
                    table.ForeignKey(
                        name: "FK_historial_abastecimiento_sede_id_sede",
                        column: x => x.id_sede,
                        principalTable: "sede",
                        principalColumn: "id_sede",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historial_abastecimiento_usuario_id_usuario_registra",
                        column: x => x.id_usuario_registra,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "inventario_sede",
                columns: table => new
                {
                    id_inventario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_sede = table.Column<int>(type: "integer", nullable: false),
                    cantidad_disponible = table.Column<int>(type: "integer", nullable: false),
                    umbral_minimo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventario_sede", x => x.id_inventario);
                    table.ForeignKey(
                        name: "FK_inventario_sede_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inventario_sede_sede_id_sede",
                        column: x => x.id_sede,
                        principalTable: "sede",
                        principalColumn: "id_sede",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transferencia_stock",
                columns: table => new
                {
                    id_transferencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_sede_origen = table.Column<int>(type: "integer", nullable: true),
                    id_sede_destino = table.Column<int>(type: "integer", nullable: true),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_solicita = table.Column<int>(type: "integer", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fecha_completada = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencia_stock", x => x.id_transferencia);
                    table.ForeignKey(
                        name: "FK_transferencia_stock_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transferencia_stock_sede_id_sede_destino",
                        column: x => x.id_sede_destino,
                        principalTable: "sede",
                        principalColumn: "id_sede");
                    table.ForeignKey(
                        name: "FK_transferencia_stock_sede_id_sede_origen",
                        column: x => x.id_sede_origen,
                        principalTable: "sede",
                        principalColumn: "id_sede");
                    table.ForeignKey(
                        name: "FK_transferencia_stock_usuario_id_usuario_solicita",
                        column: x => x.id_usuario_solicita,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.InsertData(
                table: "rol",
                columns: new[] { "id_rol", "descripcion", "nombre_rol" },
                values: new object[,]
                {
                    { 3, "Atención en sede y ventas", "VENDEDOR" },
                    { 4, "Administrador de inventario por sede", "SUPERVISOR" }
                });

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 1,
                column: "id_sede",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_sede",
                table: "usuario",
                column: "id_sede");

            migrationBuilder.CreateIndex(
                name: "IX_historial_abastecimiento_id_producto",
                table: "historial_abastecimiento",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_historial_abastecimiento_id_proveedor",
                table: "historial_abastecimiento",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_historial_abastecimiento_id_sede",
                table: "historial_abastecimiento",
                column: "id_sede");

            migrationBuilder.CreateIndex(
                name: "IX_historial_abastecimiento_id_usuario_registra",
                table: "historial_abastecimiento",
                column: "id_usuario_registra");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesChatAi_id_usuario",
                table: "HistorialesChatAi",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_inventario_sede_id_producto",
                table: "inventario_sede",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_inventario_sede_id_sede",
                table: "inventario_sede",
                column: "id_sede");

            migrationBuilder.CreateIndex(
                name: "IX_producto_proveedor_id_proveedor",
                table: "producto_proveedor",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_stock_id_producto",
                table: "transferencia_stock",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_stock_id_sede_destino",
                table: "transferencia_stock",
                column: "id_sede_destino");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_stock_id_sede_origen",
                table: "transferencia_stock",
                column: "id_sede_origen");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_stock_id_usuario_solicita",
                table: "transferencia_stock",
                column: "id_usuario_solicita");

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_sede_id_sede",
                table: "usuario",
                column: "id_sede",
                principalTable: "sede",
                principalColumn: "id_sede");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuario_sede_id_sede",
                table: "usuario");

            migrationBuilder.DropTable(
                name: "historial_abastecimiento");

            migrationBuilder.DropTable(
                name: "HistorialesChatAi");

            migrationBuilder.DropTable(
                name: "inventario_sede");

            migrationBuilder.DropTable(
                name: "producto_proveedor");

            migrationBuilder.DropTable(
                name: "transferencia_stock");

            migrationBuilder.DropTable(
                name: "proveedor");

            migrationBuilder.DropTable(
                name: "sede");

            migrationBuilder.DropIndex(
                name: "IX_usuario_id_sede",
                table: "usuario");

            migrationBuilder.DeleteData(
                table: "rol",
                keyColumn: "id_rol",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "rol",
                keyColumn: "id_rol",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "id_sede",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "orden",
                table: "imagen_producto");
        }
    }
}
