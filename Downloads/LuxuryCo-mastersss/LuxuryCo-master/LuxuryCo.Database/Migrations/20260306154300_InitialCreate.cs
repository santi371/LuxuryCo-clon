using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuxuryCo.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    id_departamento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamentos", x => x.id_departamento);
                });

            migrationBuilder.CreateTable(
                name: "EstadosEnvio",
                columns: table => new
                {
                    id_estado_envio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosEnvio", x => x.id_estado_envio);
                });

            migrationBuilder.CreateTable(
                name: "EstadosPedido",
                columns: table => new
                {
                    id_estado_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosPedido", x => x.id_estado_pedido);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    id_marca = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.id_marca);
                });

            migrationBuilder.CreateTable(
                name: "MetodosPago",
                columns: table => new
                {
                    id_metodo_pago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.id_metodo_pago);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "Ciudades",
                columns: table => new
                {
                    id_ciudad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_departamento = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ciudades", x => x.id_ciudad);
                    table.ForeignKey(
                        name: "FK_Ciudades_Departamentos_id_departamento",
                        column: x => x.id_departamento,
                        principalTable: "Departamentos",
                        principalColumn: "id_departamento");
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: true),
                    id_marca = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "Categorias",
                        principalColumn: "id_categoria");
                    table.ForeignKey(
                        name: "FK_Productos_Marcas_id_marca",
                        column: x => x.id_marca,
                        principalTable: "Marcas",
                        principalColumn: "id_marca");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    id_rol = table.Column<int>(type: "integer", nullable: true),
                    foto_perfil_url = table.Column<string>(type: "text", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_secret = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_id_rol",
                        column: x => x.id_rol,
                        principalTable: "Roles",
                        principalColumn: "id_rol");
                });

            migrationBuilder.CreateTable(
                name: "ImagenesProducto",
                columns: table => new
                {
                    id_imagen = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: true),
                    url_imagen = table.Column<string>(type: "text", nullable: false),
                    principal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagenesProducto", x => x.id_imagen);
                    table.ForeignKey(
                        name: "FK_ImagenesProducto_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto");
                });

            migrationBuilder.CreateTable(
                name: "Carritos",
                columns: table => new
                {
                    id_carrito = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carritos", x => x.id_carrito);
                    table.ForeignKey(
                        name: "FK_Carritos_Usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "DireccionesUsuario",
                columns: table => new
                {
                    id_direccion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    id_ciudad = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DireccionesUsuario", x => x.id_direccion);
                    table.ForeignKey(
                        name: "FK_DireccionesUsuario_Ciudades_id_ciudad",
                        column: x => x.id_ciudad,
                        principalTable: "Ciudades",
                        principalColumn: "id_ciudad");
                    table.ForeignKey(
                        name: "FK_DireccionesUsuario_Usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "Resenas",
                columns: table => new
                {
                    id_resena = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    id_producto = table.Column<int>(type: "integer", nullable: true),
                    calificacion = table.Column<int>(type: "integer", nullable: false),
                    comentario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resenas", x => x.id_resena);
                    table.ForeignKey(
                        name: "FK_Resenas_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto");
                    table.ForeignKey(
                        name: "FK_Resenas_Usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "DetallesCarrito",
                columns: table => new
                {
                    id_detalle_carrito = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_carrito = table.Column<int>(type: "integer", nullable: true),
                    id_producto = table.Column<int>(type: "integer", nullable: true),
                    cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCarrito", x => x.id_detalle_carrito);
                    table.ForeignKey(
                        name: "FK_DetallesCarrito_Carritos_id_carrito",
                        column: x => x.id_carrito,
                        principalTable: "Carritos",
                        principalColumn: "id_carrito");
                    table.ForeignKey(
                        name: "FK_DetallesCarrito_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto");
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    id_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    id_direccion = table.Column<int>(type: "integer", nullable: true),
                    fecha_pedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    id_estado_pedido = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.id_pedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_DireccionesUsuario_id_direccion",
                        column: x => x.id_direccion,
                        principalTable: "DireccionesUsuario",
                        principalColumn: "id_direccion");
                    table.ForeignKey(
                        name: "FK_Pedidos_EstadosPedido_id_estado_pedido",
                        column: x => x.id_estado_pedido,
                        principalTable: "EstadosPedido",
                        principalColumn: "id_estado_pedido");
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "DetallesPedido",
                columns: table => new
                {
                    id_detalle_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_pedido = table.Column<int>(type: "integer", nullable: true),
                    id_producto = table.Column<int>(type: "integer", nullable: true),
                    cantidad = table.Column<int>(type: "integer", nullable: true),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedido", x => x.id_detalle_pedido);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Pedidos_id_pedido",
                        column: x => x.id_pedido,
                        principalTable: "Pedidos",
                        principalColumn: "id_pedido");
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto");
                });

            migrationBuilder.CreateTable(
                name: "Envios",
                columns: table => new
                {
                    id_envio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_pedido = table.Column<int>(type: "integer", nullable: true),
                    fecha_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    numero_guia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_estado_envio = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envios", x => x.id_envio);
                    table.ForeignKey(
                        name: "FK_Envios_EstadosEnvio_id_estado_envio",
                        column: x => x.id_estado_envio,
                        principalTable: "EstadosEnvio",
                        principalColumn: "id_estado_envio");
                    table.ForeignKey(
                        name: "FK_Envios_Pedidos_id_pedido",
                        column: x => x.id_pedido,
                        principalTable: "Pedidos",
                        principalColumn: "id_pedido");
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    id_factura = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_pedido = table.Column<int>(type: "integer", nullable: true),
                    fecha_factura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    id_metodo_pago = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.id_factura);
                    table.ForeignKey(
                        name: "FK_Facturas_MetodosPago_id_metodo_pago",
                        column: x => x.id_metodo_pago,
                        principalTable: "MetodosPago",
                        principalColumn: "id_metodo_pago");
                    table.ForeignKey(
                        name: "FK_Facturas_Pedidos_id_pedido",
                        column: x => x.id_pedido,
                        principalTable: "Pedidos",
                        principalColumn: "id_pedido");
                });

            migrationBuilder.InsertData(
                table: "EstadosEnvio",
                columns: new[] { "id_estado_envio", "nombre_estado" },
                values: new object[,]
                {
                    { 1, "Preparando" },
                    { 2, "En camino" },
                    { 3, "Entregado" }
                });

            migrationBuilder.InsertData(
                table: "EstadosPedido",
                columns: new[] { "id_estado_pedido", "nombre_estado" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "Pagado" },
                    { 3, "Enviado" },
                    { 4, "Cancelado" }
                });

            migrationBuilder.InsertData(
                table: "MetodosPago",
                columns: new[] { "id_metodo_pago", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "Pago con tarjeta", "Tarjeta" },
                    { 2, "Pago por PSE", "PSE" },
                    { 3, "Pago por Nequi", "Nequi" },
                    { 4, "Pago contra entrega", "Contra entrega" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "id_rol", "descripcion", "nombre_rol" },
                values: new object[,]
                {
                    { 1, "Administrador del sistema", "ADMIN" },
                    { 2, "Cliente regular", "CLIENTE" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "id_usuario", "activo", "apellido", "email", "fecha_registro", "foto_perfil_url", "id_rol", "nombre", "password_hash", "telefono", "two_factor_enabled", "two_factor_secret" },
                values: new object[] { 1, true, "LuxuryCo", "admin@luxuryco.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", 1, "Admin", "TODO_HASH_THIS_Password123*", "123456789", false, "" });

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_id_usuario",
                table: "Carritos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Ciudades_id_departamento",
                table: "Ciudades",
                column: "id_departamento");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCarrito_id_carrito",
                table: "DetallesCarrito",
                column: "id_carrito");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCarrito_id_producto",
                table: "DetallesCarrito",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_id_pedido",
                table: "DetallesPedido",
                column: "id_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_id_producto",
                table: "DetallesPedido",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_DireccionesUsuario_id_ciudad",
                table: "DireccionesUsuario",
                column: "id_ciudad");

            migrationBuilder.CreateIndex(
                name: "IX_DireccionesUsuario_id_usuario",
                table: "DireccionesUsuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_id_estado_envio",
                table: "Envios",
                column: "id_estado_envio");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_id_pedido",
                table: "Envios",
                column: "id_pedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_id_metodo_pago",
                table: "Facturas",
                column: "id_metodo_pago");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_id_pedido",
                table: "Facturas",
                column: "id_pedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesProducto_id_producto",
                table: "ImagenesProducto",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_direccion",
                table: "Pedidos",
                column: "id_direccion");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_estado_pedido",
                table: "Pedidos",
                column: "id_estado_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_usuario",
                table: "Pedidos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_categoria",
                table: "Productos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_marca",
                table: "Productos",
                column: "id_marca");

            migrationBuilder.CreateIndex(
                name: "IX_Resenas_id_producto",
                table: "Resenas",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Resenas_id_usuario",
                table: "Resenas",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_id_rol",
                table: "Usuarios",
                column: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesCarrito");

            migrationBuilder.DropTable(
                name: "DetallesPedido");

            migrationBuilder.DropTable(
                name: "Envios");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "ImagenesProducto");

            migrationBuilder.DropTable(
                name: "Resenas");

            migrationBuilder.DropTable(
                name: "Carritos");

            migrationBuilder.DropTable(
                name: "EstadosEnvio");

            migrationBuilder.DropTable(
                name: "MetodosPago");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "DireccionesUsuario");

            migrationBuilder.DropTable(
                name: "EstadosPedido");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "Ciudades");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Departamentos");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
