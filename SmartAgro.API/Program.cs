using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartAgro.API.Services;
using SmartAgro.Data;
using SmartAgro.Models.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ? CONFIGURAR JSON PARA EVITAR CICLOS DE REFERENCIA
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ? CONFIGURACIÓN CORS CRÍTICA - DEBE IR ANTES DE CUALQUIER OTRA CONFIGURACIÓN
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// Agregar DbContext
builder.Services.AddDbContext<SmartAgroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SmartAgroDbContext>()
.AddDefaultTokenProviders();

// Configurar JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Solo para desarrollo
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ? REGISTRAR TODOS LOS SERVICIOS
// ?? AuthService actualizado SIN registro público
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICotizacionService, CotizacionService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IMateriaPrimaService, MateriaPrimaService>();
builder.Services.AddScoped<ICompraProveedorService, CompraProveedorService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IComentarioService, ComentarioService>();
builder.Services.AddScoped<ICosteoFifoService, CosteoFifoService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SmartAgro API", Version = "v1" });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartAgro API v1");
        c.RoutePrefix = "swagger";
    });
}

// ? ORDEN CRÍTICO DE MIDDLEWARES
app.UseCors("AllowAngularDev"); // CORS DEBE IR PRIMERO, ANTES DE AUTHENTICATION

// Middleware de logging para debugging
app.Use(async (context, next) =>
{
    Console.WriteLine($"?? {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"?? Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");

    await next();

    Console.WriteLine($"?? Response: {context.Response.StatusCode}");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Inicializar datos por defecto
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SmartAgroDbContext>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Crear la base de datos si no existe
        await context.Database.EnsureCreatedAsync();

        // Crear roles por defecto
        string[] roles = { "Admin", "Cliente" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"? Rol '{role}' creado");
            }
        }

        // Crear usuario administrador por defecto
        var adminEmail = "cortezgonzalezjuandiego3@gmail.com";
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
                Activo = true,
                FechaRegistro = DateTime.Now // ? Agregado para consistencia
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123@");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"? Usuario administrador creado: {adminEmail} / Admin123!");
            }
            else
            {
                Console.WriteLine($"? Error al crear usuario admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // ? Crear producto por defecto si no existe
        if (!await context.Productos.AnyAsync())
        {
            var productoDefault = new SmartAgro.Models.Entities.Producto
            {
                Nombre = "Sistema de Riego Inteligente SmartAgro",
                Descripcion = "Sistema de riego automático con IoT para agricultura de precisión",
                DescripcionDetallada = "Sistema completo de riego automatizado con sensores IoT, control remoto y monitoreo en tiempo real.",
                PrecioBase = 12000.00m,
                PorcentajeGanancia = 25.00m,
                PrecioVenta = 15000.00m,
                ImagenPrincipal = "/assets/images/sistema-riego-default.jpg",
                Caracteristicas = "[\"Control automático\", \"Sensores IoT\", \"Monitoreo remoto\", \"Ahorro de agua\"]",
                Beneficios = "[\"Hasta 40% ahorro de agua\", \"Control desde smartphone\", \"Datos en tiempo real\"]",
                Activo = true
            };

            context.Productos.Add(productoDefault);
            await context.SaveChangesAsync();
            Console.WriteLine("? Producto por defecto creado");
        }

        Console.WriteLine("?? Inicialización de datos completada");
        Console.WriteLine("?? ENDPOINTS DISPONIBLES:");
        Console.WriteLine("   ?? POST /api/auth/login - Iniciar sesión");
        Console.WriteLine("   ?? POST /api/auth/refresh-token - Refrescar token");
        Console.WriteLine("   ?? POST /api/auth/logout - Cerrar sesión");
        Console.WriteLine("   ?? GET/POST/PUT/DELETE /api/users - Gestión de usuarios (Solo Admin)");
        Console.WriteLine("   ? REMOVIDO: POST /api/auth/register - Ya no disponible");
        Console.WriteLine("   ?? Los clientes reciben credenciales por email cuando el admin los crea");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error durante la inicialización: {ex.Message}");
    }
}

Console.WriteLine("?? SmartAgro API iniciada correctamente");
Console.WriteLine($"?? Swagger UI: http://localhost:5194/swagger");
Console.WriteLine($"?? API Base URL: http://localhost:5194/api");
Console.WriteLine($"?? Admin por defecto: admin@smartagro.com / Admin123!");
Console.WriteLine($"?? FLUJO DE REGISTRO: Solo admins pueden crear clientes");

app.Run();