using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartAgro.API.Services;
using SmartAgro.Data;
using SmartAgro.Models.Entities;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// CONFIGURACI�N DE SERVICIOS
// =====================================

// Configuraci�n de la base de datos
builder.Services.AddDbContext<SmartAgroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuraci�n de Identity
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    // Configuraci�n de contrase�as
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Configuraci�n de usuarios
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Configuraci�n de lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configuraci�n de sign in
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<SmartAgroDbContext>()
.AddDefaultTokenProviders();

// Configuraci�n de JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configuraci�n de autorizaci�n
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireClienteRole", policy => policy.RequireRole("Cliente"));
    options.AddPolicy("RequireAdminOrCliente", policy =>
        policy.RequireRole("Admin", "Cliente"));
});

// Configuraci�n de controladores y JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configuraci�n de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:3000",
                "https://localhost:3000",
                "http://smartagro.local",
                "https://smartagro.local"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true); // Para desarrollo
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// =====================================
// REGISTRO DE SERVICIOS DE NEGOCIO
// =====================================

// Servicios de comunicaci�n (PRIMERO - porque otros servicios lo necesitan)
builder.Services.AddScoped<IEmailService, EmailService>();

// Servicios de autenticaci�n y usuarios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Servicios de productos y cat�logo
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IComentarioService, ComentarioService>();

// Servicios de proveedores y materias primas
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IMateriaPrimaService, MateriaPrimaService>();
builder.Services.AddScoped<ICompraProveedorService, CompraProveedorService>();
builder.Services.AddScoped<ICosteoFifoService, CosteoFifoService>();
// Servicios de ventas y cotizaciones
builder.Services.AddScoped<ICotizacionService, CotizacionService>();
builder.Services.AddScoped<IVentaService, VentaService>();

// Configuraci�n de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SmartAgro IoT Solutions API",
        Description = "API para el sistema de gesti�n SmartAgro IoT Solutions",
        Contact = new OpenApiContact
        {
            Name = "SmartAgro Team",
            Email = "cortezdc254@gmail.com",
            Url = new Uri("https://smartagro.com")
        }
    });

    // Configuraci�n de autenticaci�n JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa 'Bearer' [espacio] y luego tu token JWT.\n\nEjemplo: \"Bearer 12345abcdef\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Incluir comentarios XML
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configuraci�n de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}

// Configuraci�n de HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Configuraci�n de cache
builder.Services.AddMemoryCache();

// Configuraci�n de health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// =====================================
// CONFIGURACI�N DEL PIPELINE
// =====================================

// Configurar el pipeline de HTTP request
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartAgro API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "SmartAgro API Documentation";
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });

    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Middleware de seguridad
app.UseHttpsRedirection();

// CORS debe ir antes de Authentication y Authorization
app.UseCors("AllowFrontend");

// Middleware de autenticaci�n y autorizaci�n
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// Health checks
app.MapHealthChecks("/health");

// Endpoint ra�z
app.MapGet("/", () => new
{
    application = "SmartAgro IoT Solutions API",
    version = "1.0.0",
    status = "Running",
    timestamp = DateTime.UtcNow,
    documentation = "/swagger"
});

// =====================================
// INICIALIZACI�N DE DATOS
// =====================================

// Inicializar roles y datos de prueba
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Inicializar base de datos
        var context = services.GetRequiredService<SmartAgroDbContext>();
        await context.Database.MigrateAsync();

        // Inicializar roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await InitializeRoles(roleManager, logger);

        // Crear usuario administrador por defecto
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        await CreateAdminUser(userManager, logger);

        logger.LogInformation("? Aplicaci�n inicializada correctamente");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "? Error durante la inicializaci�n de la aplicaci�n");
        throw;
    }
}

app.Run();

// =====================================
// M�TODOS DE INICIALIZACI�N
// =====================================

static async Task InitializeRoles(RoleManager<IdentityRole> roleManager, ILogger logger)
{
    var roles = new[] { "Admin", "Cliente" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                logger.LogInformation($"? Rol '{role}' creado exitosamente");
            }
            else
            {
                logger.LogError($"? Error al crear rol '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}

static async Task CreateAdminUser(UserManager<Usuario> userManager, ILogger logger)
{
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
            Activo = true,
            FechaRegistro = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation($"? Usuario administrador creado: {adminEmail}");
            logger.LogInformation($"?? Contrase�a por defecto: Admin123!");
            logger.LogInformation($"?? IMPORTANTE: Los clientes deben ser registrados por el administrador desde el panel de control");
            logger.LogInformation($"?? Las credenciales se enviar�n autom�ticamente por email a los clientes registrados");
        }
        else
        {
            logger.LogError($"? Error al crear usuario administrador: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        logger.LogInformation($"?? Usuario administrador ya existe: {adminEmail}");
        logger.LogInformation($"?? Recuerda: Los clientes se registran desde el panel de administraci�n");
    }
}