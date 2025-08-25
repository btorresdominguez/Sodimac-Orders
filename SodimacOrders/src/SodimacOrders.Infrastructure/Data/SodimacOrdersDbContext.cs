using Microsoft.EntityFrameworkCore;
using SodimacOrders.Domain.Entities; // Ensure this namespace includes your entity classes
using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using System;

namespace SodimacOrders.Infrastructure.Data
{
    public class SodimacOrdersDbContext : DbContext
    {
        public SodimacOrdersDbContext(DbContextOptions<SodimacOrdersDbContext> options) : base(options)
        {
        }

        // ========== DbSets Autenticación ==========
        public DbSet<User> Usuarios => Set<User>();
        public DbSet<UserTokens> UsuarioTokens => Set<UserTokens>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRol> UsuarioRoles => Set<UserRol>();

        // ========== DbSets Pedidos ==========
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<DetallePedido> DetallesPedidos => Set<DetallePedido>();
        public DbSet<RutaEntrega> RutasEntrega { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Configuración Usuarios ===
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Ignore(u => u.Role);

            // Configuración de UsuarioRol con clave compuesta
            modelBuilder.Entity<UserRol>()
                .HasKey(ur => new { ur.IdUsuario, ur.IdRol });

            modelBuilder.Entity<UserRol>()
                .HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.IdUsuario);

            modelBuilder.Entity<UserRol>()
                .HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.IdRol);

            // Relación UsuarioTokens → Usuario
            modelBuilder.Entity<UserTokens>()
                .HasOne(ut => ut.Usuario)
                .WithMany(u => u.UsuarioTokens)
                .HasForeignKey(ut => ut.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // === Configuración para Pedido ===
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.estado_pedido)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Pendiente");

                entity.Property(e => e.valor_total)
                    .HasColumnType("decimal(12,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.fecha_pedido)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.fecha_actualizacion)
                    .HasDefaultValueSql("GETDATE()");

                // Relaciones
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Pedidos)
                    .HasForeignKey(e => e.cliente_id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RutaEntrega)
                    .WithMany(r => r.Pedidos)
                    .HasForeignKey(e => e.ruta_id)
                    .OnDelete(DeleteBehavior.SetNull);

                // Índices
                entity.HasIndex(e => e.cliente_id).HasDatabaseName("IX_Orders_Cliente");
                entity.HasIndex(e => e.ruta_id).HasDatabaseName("IX_Orders_Ruta");
                entity.HasIndex(e => e.estado_pedido).HasDatabaseName("IX_Orders_Estado");
                entity.HasIndex(e => e.fecha_entrega).HasDatabaseName("IX_Orders_FechaEntrega");
                entity.HasIndex(e => e.fecha_pedido).HasDatabaseName("IX_Orders_FechaPedido");

                // Constraints
                entity.HasCheckConstraint("CK_Orders_Estado",
                    "estado_pedido IN ('Pendiente', 'Confirmado', 'En_Preparacion', 'En_Transito', 'Entregado', 'Cancelado')");
                entity.HasCheckConstraint("CK_Orders_FechaEntrega",
                    "fecha_entrega >= fecha_pedido");
            });

            // === Configuración para Cliente ===
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clients");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.direccion)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.fecha_creacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.activo)
                    .HasDefaultValue(true);

                // Índices
                entity.HasIndex(e => e.email)
                    .IsUnique()
                    .HasDatabaseName("IX_Clients_Email");
                entity.HasIndex(e => e.activo).HasDatabaseName("IX_Clients_Activo");
            });

            // === Configuración para RutaEntrega ===
            modelBuilder.Entity<RutaEntrega>(entity =>
            {
                entity.ToTable("DeliveryRoutes");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.nombre_ruta)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.estado)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Activa");

                entity.Property(e => e.capacidad_maxima)
                    .HasDefaultValue(50);

                entity.Property(e => e.fecha_creacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.activo)
                    .HasDefaultValue(true);

                // Índices
                entity.HasIndex(e => e.estado).HasDatabaseName("IX_DeliveryRoutes_Estado");
                entity.HasIndex(e => e.activo).HasDatabaseName("IX_DeliveryRoutes_Activo");

                // Constraints
                entity.HasCheckConstraint("CK_DeliveryRoutes_Estado",
                    "estado IN ('Activa', 'Inactiva', 'En_Proceso', 'Completada')");
            });

            // === Configuración para Producto ===
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.nombre_producto)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.codigo_producto)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.precio_unitario)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.categoria)
                    .HasMaxLength(100);

                entity.Property(e => e.activo)
                    .HasDefaultValue(true);

                // Índices
                entity.HasIndex(e => e.codigo_producto)
                    .IsUnique()
                    .HasDatabaseName("IX_Products_Codigo");
                entity.HasIndex(e => e.categoria).HasDatabaseName("IX_Products_Categoria");
            });

            // === Configuración para DetallePedido ===
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Ensure Id is configured as identity (auto-increment)
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd() // This ensures auto-increment
                    .HasAnnotation("SqlServer:Identity", "1, 1");

                // Configure other properties
                entity.Property(e => e.PrecioUnitario)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Subtotal)
                    .HasColumnType("decimal(18,2)");

                // Configure relationships
                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.Productos) // Assuming Pedido.Productos is the navigation property
                    .HasForeignKey(d => d.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configurar datos semilla (Seed Data)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Clientes
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { Id = 1, nombre = "Juan Pérez García", direccion = "Carrera 15 #45-67, Bogotá", email = "juan.perez@email.com", fecha_creacion = DateTime.Now },
                new Cliente { Id = 2, nombre = "María López Rodríguez", direccion = "Calle 80 #12-34, Medellín", email = "maria.lopez@email.com", fecha_creacion = DateTime.Now },
                new Cliente { Id = 3, nombre = "Carlos Hernández Martín", direccion = "Avenida 6N #23-45, Cali", email = "carlos.hernandez@email.com", fecha_creacion = DateTime.Now },
                new Cliente { Id = 4, nombre = "Ana Sofía Ruiz", direccion = "Carrera 9 #67-89, Barranquilla", email = "ana.ruiz@email.com", fecha_creacion = DateTime.Now },
                new Cliente { Id = 5, nombre = "Pedro González Castro", direccion = "Calle 72 #11-22, Bucaramanga", email = "pedro.gonzalez@email.com", fecha_creacion = DateTime.Now }
            );

            // Seed Rutas de Entrega
            modelBuilder.Entity<RutaEntrega>().HasData(
                new RutaEntrega { Id = 1, nombre_ruta = "Ruta Norte Bogotá", estado = "Activa", capacidad_maxima = 30, fecha_creacion = DateTime.Now },
                new RutaEntrega { Id = 2, nombre_ruta = "Ruta Sur Bogotá", estado = "Activa", capacidad_maxima = 25, fecha_creacion = DateTime.Now },
                new RutaEntrega { Id = 3, nombre_ruta = "Ruta Medellín Centro", estado = "Activa", capacidad_maxima = 40, fecha_creacion = DateTime.Now },
                new RutaEntrega { Id = 4, nombre_ruta = "Ruta Cali Valle", estado = "En_Proceso", capacidad_maxima = 35, fecha_creacion = DateTime.Now },
                new RutaEntrega { Id = 5, nombre_ruta = "Ruta Costa Atlántica", estado = "Activa", capacidad_maxima = 20, fecha_creacion = DateTime.Now }
            );

            // Seed Productos
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, nombre_producto = "Taladro Percutor 18V", codigo_producto = "TD-001", precio_unitario = 189900.00m, categoria = "Herramientas Eléctricas" },
                new Producto { Id = 2, nombre_producto = "Martillo Carpintero 16oz", codigo_producto = "MT-002", precio_unitario = 25500.00m, categoria = "Herramientas Manuales" },
                new Producto { Id = 3, nombre_producto = "Pintura Látex Blanco 4L", codigo_producto = "PT-003", precio_unitario = 45200.00m, categoria = "Pinturas" },
                new Producto { Id = 4, nombre_producto = "Destornillador Set x12", codigo_producto = "DS-004", precio_unitario = 38700.00m, categoria = "Herramientas Manuales" },
                new Producto { Id = 5, nombre_producto = "Cemento Gris 50kg", codigo_producto = "CM-005", precio_unitario = 18900.00m, categoria = "Materiales Construcción" },
                new Producto { Id = 6, nombre_producto = "Bombillo LED 12W", codigo_producto = "BL-006", precio_unitario = 12300.00m, categoria = "Electricidad" },
                new Producto { Id = 7, nombre_producto = "Tubo PVC 4\" x 6m", codigo_producto = "TV-007", precio_unitario = 28600.00m, categoria = "Plomería" },
                new Producto { Id = 8, nombre_producto = "Llave Inglesa 12\"", codigo_producto = "LI-008", precio_unitario = 42100.00m, categoria = "Herramientas Manuales" }
            );
        }
    }
}