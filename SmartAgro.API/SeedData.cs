using Microsoft.AspNetCore.Identity;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            SmartAgroDbContext context,
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ✅ CREAR ROLES
            string[] roles = { "Admin", "Cliente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ✅ CREAR ADMIN POR DEFECTO
            var adminEmail = "admin@smartagro.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nombre = "Administrador",
                    Apellidos = "Sistema",
                    EmailConfirmed = true,
                    Activo = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ✅ CREAR DATOS DE PRUEBA SI NO EXISTEN
            if (!context.Proveedores.Any())
            {
                var proveedores = new List<Proveedor>
                {
                    new Proveedor
                    {
                        Nombre = "TecnoAgro SA",
                        RazonSocial = "Tecnologías Agrícolas SA de CV",
                        RFC = "TAG950101XXX",
                        Direccion = "Av. Tecnología 123, León, Gto",
                        Telefono = "4771234567",
                        Email = "ventas@tecnoagro.com",
                        ContactoPrincipal = "Juan Pérez"
                    },
                    new Proveedor
                    {
                        Nombre = "Sensores México",
                        RazonSocial = "Sensores y Equipos de México SA",
                        RFC = "SEM980505YYY",
                        Direccion = "Blvd. Industrial 456, León, Gto",
                        Telefono = "4779876543",
                        Email = "contacto@sensoresmx.com",
                        ContactoPrincipal = "María González"
                    }
                };

                context.Proveedores.AddRange(proveedores);
                await context.SaveChangesAsync();
            }

            if (!context.MateriasPrimas.Any())
            {
                var proveedor1 = context.Proveedores.First();
                var proveedor2 = context.Proveedores.Skip(1).First();

                var materiasPrimas = new List<MateriaPrima>
                {
                    new MateriaPrima
                    {
                        Nombre = "Sensor de Humedad DHT22",
                        Descripcion = "Sensor digital de temperatura y humedad",
                        UnidadMedida = "Pieza",
                        CostoUnitario = 450.00m,
                        Stock = 50,
                        StockMinimo = 10,
                        ProveedorId = proveedor1.Id
                    },
                    new MateriaPrima
                    {
                        Nombre = "Válvula Solenoide 12V",
                        Descripcion = "Válvula electromagnética para control de riego",
                        UnidadMedida = "Pieza",
                        CostoUnitario = 1200.00m,
                        Stock = 30,
                        StockMinimo = 5,
                        ProveedorId = proveedor1.Id
                    },
                    new MateriaPrima
                    {
                        Nombre = "Microcontrolador ESP32",
                        Descripcion = "Microcontrolador con WiFi y Bluetooth integrado",
                        UnidadMedida = "Pieza",
                        CostoUnitario = 350.00m,
                        Stock = 40,
                        StockMinimo = 8,
                        ProveedorId = proveedor2.Id
                    },
                    new MateriaPrima
                    {
                        Nombre = "Bomba de Agua 12V",
                        Descripcion = "Bomba sumergible de 12V para sistemas de riego",
                        UnidadMedida = "Pieza",
                        CostoUnitario = 800.00m,
                        Stock = 20,
                        StockMinimo = 3,
                        ProveedorId = proveedor2.Id
                    }
                };

                context.MateriasPrimas.AddRange(materiasPrimas);
                await context.SaveChangesAsync();
            }

            if (!context.Productos.Any())
            {
                var productos = new List<Producto>
                {
                    new Producto
                    {
                        Nombre = "Sistema de Riego Inteligente SmartAgro Pro",
                        Descripcion = "Sistema completo de riego automatizado con sensores IoT y control remoto",
                        DescripcionDetallada = "Sistema integral de riego inteligente que incluye sensores de humedad, temperatura, válvulas automáticas y control remoto vía aplicación móvil. Perfecto para cultivos de hasta 100m².",
                        PrecioBase = 8500.00m,
                        PorcentajeGanancia = 35.00m,
                        PrecioVenta = 11475.00m,
                        ImagenPrincipal = "https://example.com/smartagro-pro.jpg",
                        ImagenesSecundarias = "[\"https://example.com/smartagro-pro-2.jpg\", \"https://example.com/smartagro-pro-3.jpg\"]",
                        VideoDemo = "https://youtube.com/watch?v=demo1",
                        Caracteristicas = "[\"Sensores de humedad y temperatura\", \"Control remoto vía WiFi\", \"Aplicación móvil incluida\", \"Válvulas automáticas\", \"Bomba de agua incluida\", \"Instalación fácil\"]",
                        Beneficios = "[\"Ahorro de agua hasta 40%\", \"Control total desde tu móvil\", \"Riego automático 24/7\", \"Mejora el rendimiento de cultivos\", \"Fácil instalación y uso\"]"
                    },
                    new Producto
                    {
                        Nombre = "Kit Básico de Sensores SmartAgro",
                        Descripcion = "Kit básico con sensores de humedad y temperatura para monitoreo de cultivos",
                        DescripcionDetallada = "Kit de inicio perfecto para quienes quieren comenzar con agricultura inteligente. Incluye sensores básicos y conectividad IoT.",
                        PrecioBase = 2500.00m,
                        PorcentajeGanancia = 40.00m,
                        PrecioVenta = 3500.00m,
                        ImagenPrincipal = "https://example.com/kit-basico.jpg",
                        Caracteristicas = "[\"Sensor de humedad DHT22\", \"Microcontrolador ESP32\", \"Conectividad WiFi\", \"App móvil básica\"]",
                        Beneficios = "[\"Monitoreo en tiempo real\", \"Alertas automáticas\", \"Fácil configuración\", \"Ideal para principiantes\"]"
                    }
                };

                context.Productos.AddRange(productos);
                await context.SaveChangesAsync();

                // ✅ CREAR RECETAS DE PRODUCTOS
                var producto1 = context.Productos.First();
                var producto2 = context.Productos.Skip(1).First();
                var materias = context.MateriasPrimas.ToList();

                var recetas = new List<ProductoMateriaPrima>
                {
                    // Producto 1 - Sistema Completo
                    new ProductoMateriaPrima
                    {
                        ProductoId = producto1.Id,
                        MateriaPrimaId = materias[0].Id, // Sensor DHT22
                        CantidadRequerida = 3,
                        CostoUnitario = materias[0].CostoUnitario,
                        CostoTotal = 3 * materias[0].CostoUnitario,
                        Notas = "Sensores para diferentes zonas del cultivo"
                    },
                    new ProductoMateriaPrima
                    {
                        ProductoId = producto1.Id,
                        MateriaPrimaId = materias[1].Id, // Válvula
                        CantidadRequerida = 2,
                        CostoUnitario = materias[1].CostoUnitario,
                        CostoTotal = 2 * materias[1].CostoUnitario,
                        Notas = "Válvulas para control de sectores"
                    },
                    new ProductoMateriaPrima
                    {
                        ProductoId = producto1.Id,
                        MateriaPrimaId = materias[2].Id, // ESP32
                        CantidadRequerida = 1,
                        CostoUnitario = materias[2].CostoUnitario,
                        CostoTotal = 1 * materias[2].CostoUnitario,
                        Notas = "Controlador principal del sistema"
                    },
                    new ProductoMateriaPrima
                    {
                        ProductoId = producto1.Id,
                        MateriaPrimaId = materias[3].Id, // Bomba
                        CantidadRequerida = 1,
                        CostoUnitario = materias[3].CostoUnitario,
                        CostoTotal = 1 * materias[3].CostoUnitario,
                        Notas = "Bomba principal para el sistema"
                    },
                    // Producto 2 - Kit Básico
                    new ProductoMateriaPrima
                    {
                        ProductoId = producto2.Id,
                        MateriaPrimaId = materias[0].Id, // Sensor DHT22
                        CantidadRequerida = 1,
                        CostoUnitario = materias[0].CostoUnitario,
                        CostoTotal = 1 * materias[0].CostoUnitario,
                        Notas = "Sensor principal"
                    },
                    new ProductoMateriaPrima
                    {
                        ProductoId = producto2.Id,
                        MateriaPrimaId = materias[2].Id, // ESP32
                        CantidadRequerida = 1,
                        CostoUnitario = materias[2].CostoUnitario,
                        CostoTotal = 1 * materias[2].CostoUnitario,
                        Notas = "Controlador del kit"
                    }
                };

                context.ProductoMateriasPrimas.AddRange(recetas);
                await context.SaveChangesAsync();
            }

            Console.WriteLine("✅ Inicialización de datos completada");
            Console.WriteLine("🔑 ENDPOINTS DISPONIBLES:");
            Console.WriteLine("   🔐 POST /api/auth/login - Iniciar sesión");
            Console.WriteLine("   🔄 POST /api/auth/refresh-token - Refrescar token");
            Console.WriteLine("   🚪 POST /api/auth/logout - Cerrar sesión");
            Console.WriteLine("   👥 GET/POST/PUT/DELETE /api/users - Gestión de usuarios (Solo Admin)");
            Console.WriteLine("   ❌ REMOVIDO: POST /api/auth/register - Ya no disponible");
            Console.WriteLine("   📧 Los clientes reciben credenciales por email cuando el admin los crea");
            Console.WriteLine("🌟 SmartAgro API iniciada correctamente");
            Console.WriteLine($"📖 Swagger UI: http://localhost:5194/swagger");
            Console.WriteLine($"🌐 API Base URL: http://localhost:5194/api");
            Console.WriteLine($"👤 Admin por defecto: admin@smartagro.com / Admin123!");
            Console.WriteLine("🔄 FLUJO DE REGISTRO: Solo admins pueden crear clientes");
        }
    }
}