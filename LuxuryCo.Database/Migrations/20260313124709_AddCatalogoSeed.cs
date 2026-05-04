using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuxuryCo.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogoSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carritos_Usuarios_id_usuario",
                table: "Carritos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ciudades_Departamentos_id_departamento",
                table: "Ciudades");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCarrito_Carritos_id_carrito",
                table: "DetallesCarrito");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCarrito_Productos_id_producto",
                table: "DetallesCarrito");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_Pedidos_id_pedido",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_Productos_id_producto",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_DireccionesUsuario_Ciudades_id_ciudad",
                table: "DireccionesUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_DireccionesUsuario_Usuarios_id_usuario",
                table: "DireccionesUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Envios_EstadosEnvio_id_estado_envio",
                table: "Envios");

            migrationBuilder.DropForeignKey(
                name: "FK_Envios_Pedidos_id_pedido",
                table: "Envios");

            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_MetodosPago_id_metodo_pago",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Pedidos_id_pedido",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_ImagenesProducto_Productos_id_producto",
                table: "ImagenesProducto");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_DireccionesUsuario_id_direccion",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_EstadosPedido_id_estado_pedido",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Usuarios_id_usuario",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_id_categoria",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Marcas_id_marca",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Resenas_Productos_id_producto",
                table: "Resenas");

            migrationBuilder.DropForeignKey(
                name: "FK_Resenas_Usuarios_id_usuario",
                table: "Resenas");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Roles_id_rol",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Resenas",
                table: "Resenas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Productos",
                table: "Productos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pedidos",
                table: "Pedidos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetodosPago",
                table: "MetodosPago");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Marcas",
                table: "Marcas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImagenesProducto",
                table: "ImagenesProducto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Facturas",
                table: "Facturas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstadosPedido",
                table: "EstadosPedido");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstadosEnvio",
                table: "EstadosEnvio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Envios",
                table: "Envios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DireccionesUsuario",
                table: "DireccionesUsuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetallesPedido",
                table: "DetallesPedido");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetallesCarrito",
                table: "DetallesCarrito");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Departamentos",
                table: "Departamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ciudades",
                table: "Ciudades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categorias",
                table: "Categorias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Carritos",
                table: "Carritos");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "usuario");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "rol");

            migrationBuilder.RenameTable(
                name: "Resenas",
                newName: "resena");

            migrationBuilder.RenameTable(
                name: "Productos",
                newName: "producto");

            migrationBuilder.RenameTable(
                name: "Pedidos",
                newName: "pedido");

            migrationBuilder.RenameTable(
                name: "MetodosPago",
                newName: "metodo_pago");

            migrationBuilder.RenameTable(
                name: "Marcas",
                newName: "marca");

            migrationBuilder.RenameTable(
                name: "ImagenesProducto",
                newName: "imagen_producto");

            migrationBuilder.RenameTable(
                name: "Facturas",
                newName: "factura");

            migrationBuilder.RenameTable(
                name: "EstadosPedido",
                newName: "estado_pedido");

            migrationBuilder.RenameTable(
                name: "EstadosEnvio",
                newName: "estado_envio");

            migrationBuilder.RenameTable(
                name: "Envios",
                newName: "envio");

            migrationBuilder.RenameTable(
                name: "DireccionesUsuario",
                newName: "direccion_usuario");

            migrationBuilder.RenameTable(
                name: "DetallesPedido",
                newName: "detalle_pedido");

            migrationBuilder.RenameTable(
                name: "DetallesCarrito",
                newName: "detalle_carrito");

            migrationBuilder.RenameTable(
                name: "Departamentos",
                newName: "departamento");

            migrationBuilder.RenameTable(
                name: "Ciudades",
                newName: "ciudad");

            migrationBuilder.RenameTable(
                name: "Categorias",
                newName: "categoria");

            migrationBuilder.RenameTable(
                name: "Carritos",
                newName: "carrito");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_id_rol",
                table: "usuario",
                newName: "IX_usuario_id_rol");

            migrationBuilder.RenameIndex(
                name: "IX_Resenas_id_usuario",
                table: "resena",
                newName: "IX_resena_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_Resenas_id_producto",
                table: "resena",
                newName: "IX_resena_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_id_marca",
                table: "producto",
                newName: "IX_producto_id_marca");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_id_categoria",
                table: "producto",
                newName: "IX_producto_id_categoria");

            migrationBuilder.RenameIndex(
                name: "IX_Pedidos_id_usuario",
                table: "pedido",
                newName: "IX_pedido_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_Pedidos_id_estado_pedido",
                table: "pedido",
                newName: "IX_pedido_id_estado_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_Pedidos_id_direccion",
                table: "pedido",
                newName: "IX_pedido_id_direccion");

            migrationBuilder.RenameIndex(
                name: "IX_ImagenesProducto_id_producto",
                table: "imagen_producto",
                newName: "IX_imagen_producto_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_Facturas_id_pedido",
                table: "factura",
                newName: "IX_factura_id_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_Facturas_id_metodo_pago",
                table: "factura",
                newName: "IX_factura_id_metodo_pago");

            migrationBuilder.RenameIndex(
                name: "IX_Envios_id_pedido",
                table: "envio",
                newName: "IX_envio_id_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_Envios_id_estado_envio",
                table: "envio",
                newName: "IX_envio_id_estado_envio");

            migrationBuilder.RenameIndex(
                name: "IX_DireccionesUsuario_id_usuario",
                table: "direccion_usuario",
                newName: "IX_direccion_usuario_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_DireccionesUsuario_id_ciudad",
                table: "direccion_usuario",
                newName: "IX_direccion_usuario_id_ciudad");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesPedido_id_producto",
                table: "detalle_pedido",
                newName: "IX_detalle_pedido_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesPedido_id_pedido",
                table: "detalle_pedido",
                newName: "IX_detalle_pedido_id_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesCarrito_id_producto",
                table: "detalle_carrito",
                newName: "IX_detalle_carrito_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesCarrito_id_carrito",
                table: "detalle_carrito",
                newName: "IX_detalle_carrito_id_carrito");

            migrationBuilder.RenameIndex(
                name: "IX_Ciudades_id_departamento",
                table: "ciudad",
                newName: "IX_ciudad_id_departamento");

            migrationBuilder.RenameIndex(
                name: "IX_Carritos_id_usuario",
                table: "carrito",
                newName: "IX_carrito_id_usuario");

            migrationBuilder.AlterColumn<string>(
                name: "two_factor_secret",
                table: "usuario",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "telefono",
                table: "usuario",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "foto_perfil_url",
                table: "usuario",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_registro",
                table: "usuario",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "reset_password_code",
                table: "usuario",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reset_password_expires",
                table: "usuario",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "rol",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha",
                table: "resena",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "comentario",
                table: "resena",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_creacion",
                table: "producto",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "producto",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_pedido",
                table: "pedido",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "metodo_pago",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "metodo_pago",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "marca",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                table: "marca",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "url_imagen",
                table: "imagen_producto",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_factura",
                table: "factura",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_estado",
                table: "estado_pedido",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "nombre_estado",
                table: "estado_envio",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "numero_guia",
                table: "envio",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_envio",
                table: "envio",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "referencia",
                table: "direccion_usuario",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "categoria",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_creacion",
                table: "carrito",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuario",
                table: "usuario",
                column: "id_usuario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_rol",
                table: "rol",
                column: "id_rol");

            migrationBuilder.AddPrimaryKey(
                name: "PK_resena",
                table: "resena",
                column: "id_resena");

            migrationBuilder.AddPrimaryKey(
                name: "PK_producto",
                table: "producto",
                column: "id_producto");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pedido",
                table: "pedido",
                column: "id_pedido");

            migrationBuilder.AddPrimaryKey(
                name: "PK_metodo_pago",
                table: "metodo_pago",
                column: "id_metodo_pago");

            migrationBuilder.AddPrimaryKey(
                name: "PK_marca",
                table: "marca",
                column: "id_marca");

            migrationBuilder.AddPrimaryKey(
                name: "PK_imagen_producto",
                table: "imagen_producto",
                column: "id_imagen");

            migrationBuilder.AddPrimaryKey(
                name: "PK_factura",
                table: "factura",
                column: "id_factura");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estado_pedido",
                table: "estado_pedido",
                column: "id_estado_pedido");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estado_envio",
                table: "estado_envio",
                column: "id_estado_envio");

            migrationBuilder.AddPrimaryKey(
                name: "PK_envio",
                table: "envio",
                column: "id_envio");

            migrationBuilder.AddPrimaryKey(
                name: "PK_direccion_usuario",
                table: "direccion_usuario",
                column: "id_direccion");

            migrationBuilder.AddPrimaryKey(
                name: "PK_detalle_pedido",
                table: "detalle_pedido",
                column: "id_detalle_pedido");

            migrationBuilder.AddPrimaryKey(
                name: "PK_detalle_carrito",
                table: "detalle_carrito",
                column: "id_detalle_carrito");

            migrationBuilder.AddPrimaryKey(
                name: "PK_departamento",
                table: "departamento",
                column: "id_departamento");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ciudad",
                table: "ciudad",
                column: "id_ciudad");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categoria",
                table: "categoria",
                column: "id_categoria");

            migrationBuilder.AddPrimaryKey(
                name: "PK_carrito",
                table: "carrito",
                column: "id_carrito");

            migrationBuilder.InsertData(
                table: "categoria",
                columns: new[] { "id_categoria", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "Productos para hombre", "Hombre" },
                    { 2, "Productos para mujer", "Mujer" },
                    { 3, "Accesorios en general", "Accesorios" }
                });

            migrationBuilder.InsertData(
                table: "marca",
                columns: new[] { "id_marca", "descripcion", "logo_url", "nombre" },
                values: new object[,]
                {
                    { 1, null, "", "MONEY MAKERS" },
                    { 2, null, "", "Y/OUT" },
                    { 3, null, "", "BLOW UP" },
                    { 4, null, "", "CULTUREWISE" }
                });

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 1,
                columns: new[] { "reset_password_code", "reset_password_expires" },
                values: new object[] { null, null });

            migrationBuilder.AddForeignKey(
                name: "FK_carrito_usuario_id_usuario",
                table: "carrito",
                column: "id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_ciudad_departamento_id_departamento",
                table: "ciudad",
                column: "id_departamento",
                principalTable: "departamento",
                principalColumn: "id_departamento");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_carrito_carrito_id_carrito",
                table: "detalle_carrito",
                column: "id_carrito",
                principalTable: "carrito",
                principalColumn: "id_carrito");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_carrito_producto_id_producto",
                table: "detalle_carrito",
                column: "id_producto",
                principalTable: "producto",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_pedido_pedido_id_pedido",
                table: "detalle_pedido",
                column: "id_pedido",
                principalTable: "pedido",
                principalColumn: "id_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_pedido_producto_id_producto",
                table: "detalle_pedido",
                column: "id_producto",
                principalTable: "producto",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_direccion_usuario_ciudad_id_ciudad",
                table: "direccion_usuario",
                column: "id_ciudad",
                principalTable: "ciudad",
                principalColumn: "id_ciudad");

            migrationBuilder.AddForeignKey(
                name: "FK_direccion_usuario_usuario_id_usuario",
                table: "direccion_usuario",
                column: "id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_envio_estado_envio_id_estado_envio",
                table: "envio",
                column: "id_estado_envio",
                principalTable: "estado_envio",
                principalColumn: "id_estado_envio");

            migrationBuilder.AddForeignKey(
                name: "FK_envio_pedido_id_pedido",
                table: "envio",
                column: "id_pedido",
                principalTable: "pedido",
                principalColumn: "id_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_factura_metodo_pago_id_metodo_pago",
                table: "factura",
                column: "id_metodo_pago",
                principalTable: "metodo_pago",
                principalColumn: "id_metodo_pago");

            migrationBuilder.AddForeignKey(
                name: "FK_factura_pedido_id_pedido",
                table: "factura",
                column: "id_pedido",
                principalTable: "pedido",
                principalColumn: "id_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_imagen_producto_producto_id_producto",
                table: "imagen_producto",
                column: "id_producto",
                principalTable: "producto",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_pedido_direccion_usuario_id_direccion",
                table: "pedido",
                column: "id_direccion",
                principalTable: "direccion_usuario",
                principalColumn: "id_direccion");

            migrationBuilder.AddForeignKey(
                name: "FK_pedido_estado_pedido_id_estado_pedido",
                table: "pedido",
                column: "id_estado_pedido",
                principalTable: "estado_pedido",
                principalColumn: "id_estado_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_pedido_usuario_id_usuario",
                table: "pedido",
                column: "id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_producto_categoria_id_categoria",
                table: "producto",
                column: "id_categoria",
                principalTable: "categoria",
                principalColumn: "id_categoria");

            migrationBuilder.AddForeignKey(
                name: "FK_producto_marca_id_marca",
                table: "producto",
                column: "id_marca",
                principalTable: "marca",
                principalColumn: "id_marca");

            migrationBuilder.AddForeignKey(
                name: "FK_resena_producto_id_producto",
                table: "resena",
                column: "id_producto",
                principalTable: "producto",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_resena_usuario_id_usuario",
                table: "resena",
                column: "id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_rol_id_rol",
                table: "usuario",
                column: "id_rol",
                principalTable: "rol",
                principalColumn: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_carrito_usuario_id_usuario",
                table: "carrito");

            migrationBuilder.DropForeignKey(
                name: "FK_ciudad_departamento_id_departamento",
                table: "ciudad");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_carrito_carrito_id_carrito",
                table: "detalle_carrito");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_carrito_producto_id_producto",
                table: "detalle_carrito");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_pedido_pedido_id_pedido",
                table: "detalle_pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_pedido_producto_id_producto",
                table: "detalle_pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_direccion_usuario_ciudad_id_ciudad",
                table: "direccion_usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_direccion_usuario_usuario_id_usuario",
                table: "direccion_usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_envio_estado_envio_id_estado_envio",
                table: "envio");

            migrationBuilder.DropForeignKey(
                name: "FK_envio_pedido_id_pedido",
                table: "envio");

            migrationBuilder.DropForeignKey(
                name: "FK_factura_metodo_pago_id_metodo_pago",
                table: "factura");

            migrationBuilder.DropForeignKey(
                name: "FK_factura_pedido_id_pedido",
                table: "factura");

            migrationBuilder.DropForeignKey(
                name: "FK_imagen_producto_producto_id_producto",
                table: "imagen_producto");

            migrationBuilder.DropForeignKey(
                name: "FK_pedido_direccion_usuario_id_direccion",
                table: "pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_pedido_estado_pedido_id_estado_pedido",
                table: "pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_pedido_usuario_id_usuario",
                table: "pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_producto_categoria_id_categoria",
                table: "producto");

            migrationBuilder.DropForeignKey(
                name: "FK_producto_marca_id_marca",
                table: "producto");

            migrationBuilder.DropForeignKey(
                name: "FK_resena_producto_id_producto",
                table: "resena");

            migrationBuilder.DropForeignKey(
                name: "FK_resena_usuario_id_usuario",
                table: "resena");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_rol_id_rol",
                table: "usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuario",
                table: "usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_rol",
                table: "rol");

            migrationBuilder.DropPrimaryKey(
                name: "PK_resena",
                table: "resena");

            migrationBuilder.DropPrimaryKey(
                name: "PK_producto",
                table: "producto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pedido",
                table: "pedido");

            migrationBuilder.DropPrimaryKey(
                name: "PK_metodo_pago",
                table: "metodo_pago");

            migrationBuilder.DropPrimaryKey(
                name: "PK_marca",
                table: "marca");

            migrationBuilder.DropPrimaryKey(
                name: "PK_imagen_producto",
                table: "imagen_producto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_factura",
                table: "factura");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estado_pedido",
                table: "estado_pedido");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estado_envio",
                table: "estado_envio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_envio",
                table: "envio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_direccion_usuario",
                table: "direccion_usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_detalle_pedido",
                table: "detalle_pedido");

            migrationBuilder.DropPrimaryKey(
                name: "PK_detalle_carrito",
                table: "detalle_carrito");

            migrationBuilder.DropPrimaryKey(
                name: "PK_departamento",
                table: "departamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ciudad",
                table: "ciudad");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categoria",
                table: "categoria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_carrito",
                table: "carrito");

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id_categoria",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id_categoria",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id_categoria",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "marca",
                keyColumn: "id_marca",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "marca",
                keyColumn: "id_marca",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "marca",
                keyColumn: "id_marca",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "marca",
                keyColumn: "id_marca",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "reset_password_code",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "reset_password_expires",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "logo_url",
                table: "marca");

            migrationBuilder.RenameTable(
                name: "usuario",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "rol",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "resena",
                newName: "Resenas");

            migrationBuilder.RenameTable(
                name: "producto",
                newName: "Productos");

            migrationBuilder.RenameTable(
                name: "pedido",
                newName: "Pedidos");

            migrationBuilder.RenameTable(
                name: "metodo_pago",
                newName: "MetodosPago");

            migrationBuilder.RenameTable(
                name: "marca",
                newName: "Marcas");

            migrationBuilder.RenameTable(
                name: "imagen_producto",
                newName: "ImagenesProducto");

            migrationBuilder.RenameTable(
                name: "factura",
                newName: "Facturas");

            migrationBuilder.RenameTable(
                name: "estado_pedido",
                newName: "EstadosPedido");

            migrationBuilder.RenameTable(
                name: "estado_envio",
                newName: "EstadosEnvio");

            migrationBuilder.RenameTable(
                name: "envio",
                newName: "Envios");

            migrationBuilder.RenameTable(
                name: "direccion_usuario",
                newName: "DireccionesUsuario");

            migrationBuilder.RenameTable(
                name: "detalle_pedido",
                newName: "DetallesPedido");

            migrationBuilder.RenameTable(
                name: "detalle_carrito",
                newName: "DetallesCarrito");

            migrationBuilder.RenameTable(
                name: "departamento",
                newName: "Departamentos");

            migrationBuilder.RenameTable(
                name: "ciudad",
                newName: "Ciudades");

            migrationBuilder.RenameTable(
                name: "categoria",
                newName: "Categorias");

            migrationBuilder.RenameTable(
                name: "carrito",
                newName: "Carritos");

            migrationBuilder.RenameIndex(
                name: "IX_usuario_id_rol",
                table: "Usuarios",
                newName: "IX_Usuarios_id_rol");

            migrationBuilder.RenameIndex(
                name: "IX_resena_id_usuario",
                table: "Resenas",
                newName: "IX_Resenas_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_resena_id_producto",
                table: "Resenas",
                newName: "IX_Resenas_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_producto_id_marca",
                table: "Productos",
                newName: "IX_Productos_id_marca");

            migrationBuilder.RenameIndex(
                name: "IX_producto_id_categoria",
                table: "Productos",
                newName: "IX_Productos_id_categoria");

            migrationBuilder.RenameIndex(
                name: "IX_pedido_id_usuario",
                table: "Pedidos",
                newName: "IX_Pedidos_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_pedido_id_estado_pedido",
                table: "Pedidos",
                newName: "IX_Pedidos_id_estado_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_pedido_id_direccion",
                table: "Pedidos",
                newName: "IX_Pedidos_id_direccion");

            migrationBuilder.RenameIndex(
                name: "IX_imagen_producto_id_producto",
                table: "ImagenesProducto",
                newName: "IX_ImagenesProducto_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_factura_id_pedido",
                table: "Facturas",
                newName: "IX_Facturas_id_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_factura_id_metodo_pago",
                table: "Facturas",
                newName: "IX_Facturas_id_metodo_pago");

            migrationBuilder.RenameIndex(
                name: "IX_envio_id_pedido",
                table: "Envios",
                newName: "IX_Envios_id_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_envio_id_estado_envio",
                table: "Envios",
                newName: "IX_Envios_id_estado_envio");

            migrationBuilder.RenameIndex(
                name: "IX_direccion_usuario_id_usuario",
                table: "DireccionesUsuario",
                newName: "IX_DireccionesUsuario_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_direccion_usuario_id_ciudad",
                table: "DireccionesUsuario",
                newName: "IX_DireccionesUsuario_id_ciudad");

            migrationBuilder.RenameIndex(
                name: "IX_detalle_pedido_id_producto",
                table: "DetallesPedido",
                newName: "IX_DetallesPedido_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_detalle_pedido_id_pedido",
                table: "DetallesPedido",
                newName: "IX_DetallesPedido_id_pedido");

            migrationBuilder.RenameIndex(
                name: "IX_detalle_carrito_id_producto",
                table: "DetallesCarrito",
                newName: "IX_DetallesCarrito_id_producto");

            migrationBuilder.RenameIndex(
                name: "IX_detalle_carrito_id_carrito",
                table: "DetallesCarrito",
                newName: "IX_DetallesCarrito_id_carrito");

            migrationBuilder.RenameIndex(
                name: "IX_ciudad_id_departamento",
                table: "Ciudades",
                newName: "IX_Ciudades_id_departamento");

            migrationBuilder.RenameIndex(
                name: "IX_carrito_id_usuario",
                table: "Carritos",
                newName: "IX_Carritos_id_usuario");

            migrationBuilder.AlterColumn<string>(
                name: "two_factor_secret",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "telefono",
                table: "Usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "foto_perfil_url",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_registro",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Roles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha",
                table: "Resenas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "comentario",
                table: "Resenas",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_creacion",
                table: "Productos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Productos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_pedido",
                table: "Pedidos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "MetodosPago",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "MetodosPago",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Marcas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "url_imagen",
                table: "ImagenesProducto",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_factura",
                table: "Facturas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_estado",
                table: "EstadosPedido",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "nombre_estado",
                table: "EstadosEnvio",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "numero_guia",
                table: "Envios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_envio",
                table: "Envios",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "referencia",
                table: "DireccionesUsuario",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Categorias",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_creacion",
                table: "Carritos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "id_usuario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "id_rol");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Resenas",
                table: "Resenas",
                column: "id_resena");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Productos",
                table: "Productos",
                column: "id_producto");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pedidos",
                table: "Pedidos",
                column: "id_pedido");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetodosPago",
                table: "MetodosPago",
                column: "id_metodo_pago");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Marcas",
                table: "Marcas",
                column: "id_marca");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImagenesProducto",
                table: "ImagenesProducto",
                column: "id_imagen");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Facturas",
                table: "Facturas",
                column: "id_factura");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstadosPedido",
                table: "EstadosPedido",
                column: "id_estado_pedido");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstadosEnvio",
                table: "EstadosEnvio",
                column: "id_estado_envio");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Envios",
                table: "Envios",
                column: "id_envio");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DireccionesUsuario",
                table: "DireccionesUsuario",
                column: "id_direccion");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetallesPedido",
                table: "DetallesPedido",
                column: "id_detalle_pedido");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetallesCarrito",
                table: "DetallesCarrito",
                column: "id_detalle_carrito");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Departamentos",
                table: "Departamentos",
                column: "id_departamento");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ciudades",
                table: "Ciudades",
                column: "id_ciudad");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categorias",
                table: "Categorias",
                column: "id_categoria");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Carritos",
                table: "Carritos",
                column: "id_carrito");

            migrationBuilder.AddForeignKey(
                name: "FK_Carritos_Usuarios_id_usuario",
                table: "Carritos",
                column: "id_usuario",
                principalTable: "Usuarios",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Ciudades_Departamentos_id_departamento",
                table: "Ciudades",
                column: "id_departamento",
                principalTable: "Departamentos",
                principalColumn: "id_departamento");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCarrito_Carritos_id_carrito",
                table: "DetallesCarrito",
                column: "id_carrito",
                principalTable: "Carritos",
                principalColumn: "id_carrito");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCarrito_Productos_id_producto",
                table: "DetallesCarrito",
                column: "id_producto",
                principalTable: "Productos",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_Pedidos_id_pedido",
                table: "DetallesPedido",
                column: "id_pedido",
                principalTable: "Pedidos",
                principalColumn: "id_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_Productos_id_producto",
                table: "DetallesPedido",
                column: "id_producto",
                principalTable: "Productos",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_DireccionesUsuario_Ciudades_id_ciudad",
                table: "DireccionesUsuario",
                column: "id_ciudad",
                principalTable: "Ciudades",
                principalColumn: "id_ciudad");

            migrationBuilder.AddForeignKey(
                name: "FK_DireccionesUsuario_Usuarios_id_usuario",
                table: "DireccionesUsuario",
                column: "id_usuario",
                principalTable: "Usuarios",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Envios_EstadosEnvio_id_estado_envio",
                table: "Envios",
                column: "id_estado_envio",
                principalTable: "EstadosEnvio",
                principalColumn: "id_estado_envio");

            migrationBuilder.AddForeignKey(
                name: "FK_Envios_Pedidos_id_pedido",
                table: "Envios",
                column: "id_pedido",
                principalTable: "Pedidos",
                principalColumn: "id_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_MetodosPago_id_metodo_pago",
                table: "Facturas",
                column: "id_metodo_pago",
                principalTable: "MetodosPago",
                principalColumn: "id_metodo_pago");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Pedidos_id_pedido",
                table: "Facturas",
                column: "id_pedido",
                principalTable: "Pedidos",
                principalColumn: "id_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_ImagenesProducto_Productos_id_producto",
                table: "ImagenesProducto",
                column: "id_producto",
                principalTable: "Productos",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_DireccionesUsuario_id_direccion",
                table: "Pedidos",
                column: "id_direccion",
                principalTable: "DireccionesUsuario",
                principalColumn: "id_direccion");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_EstadosPedido_id_estado_pedido",
                table: "Pedidos",
                column: "id_estado_pedido",
                principalTable: "EstadosPedido",
                principalColumn: "id_estado_pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Usuarios_id_usuario",
                table: "Pedidos",
                column: "id_usuario",
                principalTable: "Usuarios",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_id_categoria",
                table: "Productos",
                column: "id_categoria",
                principalTable: "Categorias",
                principalColumn: "id_categoria");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Marcas_id_marca",
                table: "Productos",
                column: "id_marca",
                principalTable: "Marcas",
                principalColumn: "id_marca");

            migrationBuilder.AddForeignKey(
                name: "FK_Resenas_Productos_id_producto",
                table: "Resenas",
                column: "id_producto",
                principalTable: "Productos",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_Resenas_Usuarios_id_usuario",
                table: "Resenas",
                column: "id_usuario",
                principalTable: "Usuarios",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_id_rol",
                table: "Usuarios",
                column: "id_rol",
                principalTable: "Roles",
                principalColumn: "id_rol");
        }
    }
}
