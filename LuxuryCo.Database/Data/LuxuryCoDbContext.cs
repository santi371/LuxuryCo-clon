using Microsoft.EntityFrameworkCore;
using LuxuryCo.Database.Models;

namespace LuxuryCo.Database.Data
{
    public class LuxuryCoDbContext : DbContext
    {
        public LuxuryCoDbContext(DbContextOptions<LuxuryCoDbContext> options) : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<DireccionUsuario> DireccionesUsuario { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ImagenProducto> ImagenesProducto { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<DetalleCarrito> DetallesCarrito { get; set; }
        public DbSet<EstadoPedido> EstadosPedido { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<EstadoEnvio> EstadosEnvio { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Resena> Resenas { get; set; }

        // Nuevos modelos ERP
        public DbSet<Sede> Sedes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<ProductoProveedor> ProductosProveedores { get; set; }
        public DbSet<InventarioSede> InventariosSede { get; set; }
        public DbSet<HistorialAbastecimiento> HistorialesAbastecimiento { get; set; }
        public DbSet<TransferenciaStock> TransferenciasStock { get; set; }
        public DbSet<HistorialChatAi> HistorialesChatAi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuramos los nombres exactos de las tablas en Supabase
            modelBuilder.Entity<Rol>().ToTable("rol");
            modelBuilder.Entity<Usuario>().ToTable("usuario");
            modelBuilder.Entity<Departamento>().ToTable("departamento");
            modelBuilder.Entity<Ciudad>().ToTable("ciudad");
            modelBuilder.Entity<DireccionUsuario>().ToTable("direccion_usuario");
            modelBuilder.Entity<Categoria>().ToTable("categoria");
            modelBuilder.Entity<Marca>().ToTable("marca");
            modelBuilder.Entity<Producto>().ToTable("producto");
            modelBuilder.Entity<ImagenProducto>().ToTable("imagen_producto");
            modelBuilder.Entity<Carrito>().ToTable("carrito");
            modelBuilder.Entity<DetalleCarrito>().ToTable("detalle_carrito");
            modelBuilder.Entity<EstadoPedido>().ToTable("estado_pedido");
            modelBuilder.Entity<Pedido>().ToTable("pedido");
            modelBuilder.Entity<DetallePedido>().ToTable("detalle_pedido");
            modelBuilder.Entity<EstadoEnvio>().ToTable("estado_envio");
            modelBuilder.Entity<Envio>().ToTable("envio");
            modelBuilder.Entity<MetodoPago>().ToTable("metodo_pago");
            modelBuilder.Entity<Factura>().ToTable("factura");
            modelBuilder.Entity<Resena>().ToTable("resena");

            // Mapeos Tablas ERP
            modelBuilder.Entity<Sede>().ToTable("sede");
            modelBuilder.Entity<Proveedor>().ToTable("proveedor");
            modelBuilder.Entity<ProductoProveedor>().ToTable("producto_proveedor");
            modelBuilder.Entity<InventarioSede>().ToTable("inventario_sede");
            modelBuilder.Entity<HistorialAbastecimiento>().ToTable("historial_abastecimiento");
            modelBuilder.Entity<TransferenciaStock>().ToTable("transferencia_stock");

            // Llaves compuestas
            modelBuilder.Entity<ProductoProveedor>()
                .HasKey(pp => new { pp.id_producto, pp.id_proveedor });

            // Data semilla
            modelBuilder.Entity<Rol>().HasData(
                new Rol { id_rol = 1, nombre_rol = "ADMIN", descripcion = "Administrador del sistema" },
                new Rol { id_rol = 2, nombre_rol = "CLIENTE", descripcion = "Cliente regular" },
                new Rol { id_rol = 3, nombre_rol = "VENDEDOR", descripcion = "Atención en sede y ventas" },
                new Rol { id_rol = 4, nombre_rol = "SUPERVISOR", descripcion = "Administrador de inventario por sede" }
            );

            // Semilla Categorías
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { id_categoria = 1, nombre = "Hombre", descripcion = "Productos para hombre" },
                new Categoria { id_categoria = 2, nombre = "Mujer", descripcion = "Productos para mujer" },
                new Categoria { id_categoria = 3, nombre = "Accesorios", descripcion = "Accesorios en general" }
            );

            // Semilla Marcas
            modelBuilder.Entity<Marca>().HasData(
                new Marca { id_marca = 1, nombre = "MONEY MAKERS", logo_url = "" },
                new Marca { id_marca = 2, nombre = "Y/OUT", logo_url = "" },
                new Marca { id_marca = 3, nombre = "BLOW UP", logo_url = "" },
                new Marca { id_marca = 4, nombre = "CULTUREWISE", logo_url = "" }
            );

            modelBuilder.Entity<EstadoPedido>().HasData(
                new EstadoPedido { id_estado_pedido = 1, nombre_estado = "Pendiente" },
                new EstadoPedido { id_estado_pedido = 2, nombre_estado = "Pagado" },
                new EstadoPedido { id_estado_pedido = 3, nombre_estado = "Enviado" },
                new EstadoPedido { id_estado_pedido = 4, nombre_estado = "Cancelado" }
            );

            modelBuilder.Entity<EstadoEnvio>().HasData(
                new EstadoEnvio { id_estado_envio = 1, nombre_estado = "Preparando" },
                new EstadoEnvio { id_estado_envio = 2, nombre_estado = "En camino" },
                new EstadoEnvio { id_estado_envio = 3, nombre_estado = "Entregado" }
            );

            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { id_metodo_pago = 1, nombre = "Tarjeta", descripcion = "Pago con tarjeta" },
                new MetodoPago { id_metodo_pago = 2, nombre = "PSE", descripcion = "Pago por PSE" },
                new MetodoPago { id_metodo_pago = 3, nombre = "Nequi", descripcion = "Pago por Nequi" },
                new MetodoPago { id_metodo_pago = 4, nombre = "Contra entrega", descripcion = "Pago contra entrega" }
            );

            // Create default admin user (using a simple pass hash or letting it be hashed later)
            // But we should place seed data for Admin
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    id_usuario = 1,
                    nombre = "Admin",
                    apellido = "LuxuryCo",
                    email = "admin@luxuryco.com",
                    password_hash = "TODO_HASH_THIS_Password123*", // It needs hash! Later handled by identity/crypto
                    telefono = "123456789",
                    fecha_registro = new System.DateTime(2026, 1, 1),
                    activo = true,
                    id_rol = 1,
                    foto_perfil_url = "",
                    two_factor_secret = "",
                    two_factor_enabled = false
                }
            );

            // Additional configurations and restrictions can be placed here
        }
    }
}
