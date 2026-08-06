using Microsoft.EntityFrameworkCore;
using LuxuryCo.Database.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MassTransit;
using Hangfire;
using Hangfire.PostgreSql;

// Enable Legacy Timestamp Behavior to prevent DateTime Kind errors when saving to PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<LuxuryCo.Back.Services.IAuthService, LuxuryCo.Back.Services.AuthService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IProductoService, LuxuryCo.Back.Services.ProductoService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IUsuarioService, LuxuryCo.Back.Services.UsuarioService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IEmailService, LuxuryCo.Back.Services.EmailService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ICarritoService, LuxuryCo.Back.Services.CarritoService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IMarcaService, LuxuryCo.Back.Services.MarcaService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ISedeService, LuxuryCo.Back.Services.SedeService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IProveedorService, LuxuryCo.Back.Services.ProveedorService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IInventarioService, LuxuryCo.Back.Services.InventarioService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IPaymentGatewayService, LuxuryCo.Back.Services.PaymentGatewayService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ICheckoutService, LuxuryCo.Back.Services.CheckoutService>();
builder.Services.AddSingleton<LuxuryCo.Back.Services.PromptSecurityService>();
builder.Services.AddSingleton<LuxuryCo.Back.Services.ColombianDialectParserService>();
builder.Services.AddSingleton<LuxuryCo.Back.Services.ConfirmationService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.PermissionEngine>();
builder.Services.AddScoped<LuxuryCo.Back.Services.BusinessRulesService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ToolExecutorService>();

// Register AI Providers
builder.Services.AddHttpClient<LuxuryCo.Back.Services.GroqProvider>();
builder.Services.AddHttpClient<LuxuryCo.Back.Services.GeminiProvider>();
builder.Services.AddHttpClient<LuxuryCo.Back.Services.GeminiImageProvider>();
builder.Services.AddHttpClient<LuxuryCo.Back.Services.WhisperProvider>();
builder.Services.AddHttpClient<LuxuryCo.Back.Services.OpenRouterProvider>();
builder.Services.AddSingleton<LuxuryCo.Back.Services.PollinationsProvider>(); // URL-based, no HttpClient needed
builder.Services.AddHttpClient<LuxuryCo.Back.Services.StabilityProvider>();
builder.Services.AddScoped<LuxuryCo.Back.Services.IntentParserService>();

// Register Image Generation Services
builder.Services.AddSingleton<LuxuryCo.Back.Services.ImageCacheService>();
builder.Services.AddSingleton<LuxuryCo.Back.Services.ImageModerationService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ImagePromptOptimizerService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ImageStorageService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ImageProviderRouter>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ImageMetadataService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.ImageGenerationService>();
builder.Services.AddHttpClient<LuxuryCo.Back.Services.VirtualTryOnService>();

// Register Document and Storage Services
builder.Services.AddSingleton<LuxuryCo.Back.Services.SecureFileStorageService>();
builder.Services.AddScoped<LuxuryCo.Back.Services.DocumentGenerationService>();

// Core Orchestrator
builder.Services.AddScoped<LuxuryCo.Back.Services.IAiService, LuxuryCo.Back.Services.MultiModelAiService>();

// Enterprise Architecture Configurations
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LuxuryCo.Back.Services.ITenantProvider, LuxuryCo.Back.Services.TenantProvider>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Distributed Cache: Usa MemoryCache en entornos ligeros (Render Free Tier) para evitar
// dependencia externa de Redis. En producción enterprise, cambiar a AddStackExchangeRedisCache.
builder.Services.AddDistributedMemoryCache();

// MassTransit: Desactivado temporalmente en Render Free Tier por límites de RAM.
// El código de Domain Events y Consumers permanece en el proyecto listo para activarse
// en un entorno con más recursos (ej. Render Pro, Azure, AWS).
// Para activar: descomentar y agregar el paquete MassTransit.InMemory.
// Hangfire (PostgreSQL) - Envuelto en try-catch para degradación elegante
var dbConn = builder.Configuration.GetConnectionString("LuxuryCoDbConnection") ?? throw new InvalidOperationException("DB connection string not found.");
try
{
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(dbConn)));
    builder.Services.AddHangfireServer();
}
catch (Exception ex)
{
    Console.WriteLine($"Hangfire no disponible, degradación elegante: {ex.Message}");
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// DbContext configuration
builder.Services.AddDbContext<LuxuryCoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LuxuryCoDbConnection")));

// Supabase configuration (supports both : and __ separators for env vars)
var supabaseUrl = builder.Configuration["Supabase:Url"] ?? builder.Configuration["Supabase__Url"] ?? string.Empty;
var supabaseKey = builder.Configuration["Supabase:Key"] ?? builder.Configuration["Supabase__Key"] ?? string.Empty;
var options = new Supabase.SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true
};
builder.Services.AddSingleton(provider => new Supabase.Client(supabaseUrl, supabaseKey, options));

// Authentication & JWT
var keyParam = builder.Configuration["Jwt:Key"] ?? "ThisIsASecretKey1234567890OuchNeedMoreCharacters";
var key = Encoding.ASCII.GetBytes(keyParam);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(ExceptionHandlerApp =>
{
    ExceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var error = exceptionHandlerPathFeature?.Error;
        
        await context.Response.WriteAsJsonAsync(new { 
            message = "Error interno del servidor. Si el error es 'Tenant or user not found', tu base de datos Supabase se encuentra en pausa.", 
            details = error?.Message 
        });
    });
});

if (app.Environment.IsDevelopment())
{
}

app.UseStaticFiles(); // For serving images in wwwroot/uploads
app.UseCors("AllowAll");
// Render maneja HTTPS en su proxy, solo redirigir en desarrollo local
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Security: Content Security Policy (CSP)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' https://cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; img-src 'self' data: https:; connect-src 'self' ws: wss: https:;");
    await next();
});

// Hangfire Dashboard (solo si Hangfire se inicializó correctamente)
try
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // Authorization = new[] { new HangfireAuthorizationFilter() } // To be implemented
    });
}
catch { Console.WriteLine("Hangfire Dashboard no disponible."); }

app.MapControllers();
app.MapHub<LuxuryCo.Back.Hubs.AdminNotificationHub>("/hubs/adminNotifications");

// Configurar Tareas Recurrentes de Fondo (Hangfire) - Solo si está disponible
try
{
    using (var scope = app.Services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<Hangfire.IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<LuxuryCo.Back.Services.GdprCleanupJob>(
            "gdpr-cleanup",
            job => job.ProcessDataRetentionPoliciesAsync(),
            Hangfire.Cron.Weekly(System.DayOfWeek.Sunday)
        );
    }
}
catch { Console.WriteLine("Hangfire Jobs no configurados (degradación elegante)."); }

// Seed Data para Administrador y Patches de DB procesado en Background para evitar bloquear Kestrel y el Puerto 7066
_ = Task.Run(async () =>
{
    // Esperamos 5 segundos para que la App inicie en paz antes de sembrar.
    await Task.Delay(5000);
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<LuxuryCo.Database.Data.LuxuryCoDbContext>();
        var supabase = scope.ServiceProvider.GetRequiredService<Supabase.Client>();
        
        try 
        {
            // 0. Auto-patch robusto para añadir columnas faltantes en Supabase
            try
            {
                await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, @"
                    CREATE TABLE IF NOT EXISTS ai_action_log (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""UserId"" INTEGER NULL,
                        ""SessionId"" VARCHAR(255) NOT NULL DEFAULT '',
                        ""PromptOriginal"" TEXT NOT NULL,
                        ""SanitizedPrompt"" TEXT NOT NULL,
                        ""IntentDetected"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""ModelUsed"" VARCHAR(50) NOT NULL DEFAULT '',
                        ""Confidence"" DOUBLE PRECISION NOT NULL,
                        ""RiskLevel"" VARCHAR(50) NOT NULL DEFAULT 'LOW',
                        ""ActionExecuted"" VARCHAR(255) NOT NULL DEFAULT '',
                        ""BeforeState"" TEXT NOT NULL DEFAULT '',
                        ""AfterState"" TEXT NOT NULL DEFAULT '',
                        ""Timestamp"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
                        ""IpAddress"" VARCHAR(45) NOT NULL DEFAULT '',
                        ""Success"" BOOLEAN NOT NULL,
                        ""ErrorMessage"" TEXT NOT NULL DEFAULT '',
                        ""TraceId"" VARCHAR(100) NULL,
                        CONSTRAINT fk_ai_action_log_usuario FOREIGN KEY (""UserId"") REFERENCES usuario(id_usuario) ON DELETE SET NULL
                    );
                    ALTER TABLE ai_action_log ADD COLUMN IF NOT EXISTS ""TraceId"" VARCHAR(100) NULL;
                    
                    CREATE TABLE IF NOT EXISTS ai_image_generation (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""UserId"" INTEGER NULL,
                        ""PromptOriginal"" TEXT NOT NULL,
                        ""OptimizedPrompt"" TEXT NOT NULL DEFAULT '',
                        ""NegativePrompt"" TEXT NOT NULL DEFAULT '',
                        ""Seed"" INTEGER NOT NULL DEFAULT 0,
                        ""Provider"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""GenerationTimeMs"" DOUBLE PRECISION NOT NULL DEFAULT 0,
                        ""ImageUrl"" VARCHAR(1000) NOT NULL DEFAULT '',
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
                        CONSTRAINT fk_ai_image_generation_usuario FOREIGN KEY (""UserId"") REFERENCES usuario(id_usuario) ON DELETE SET NULL
                    );
                ");
                await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE producto ADD COLUMN IF NOT EXISTS \"ConcurrencyToken\" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';");
                await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE inventario_sede ADD COLUMN IF NOT EXISTS \"ConcurrencyToken\" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al aplicar parches de IA/Concurrencia: " + ex.Message);
            }

            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE marca ADD COLUMN IF NOT EXISTS logo_url character varying(500) NULL;");
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE \"Resenas\" ADD COLUMN IF NOT EXISTS nombre_invitado character varying(100) NULL;"); } catch {}
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE resena ADD COLUMN IF NOT EXISTS nombre_invitado character varying(100) NULL;"); } catch {}
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE detalle_carrito ADD COLUMN IF NOT EXISTS talla character varying(10) NULL;"); } catch {}
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE \"DetallesCarrito\" ADD COLUMN IF NOT EXISTS talla character varying(10) NULL;"); } catch {}

            // Multi-Tenant: Agregar TenantId a todas las tablas que lo requieren
            // Esto soluciona el error "column u.TenantId does not exist"
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE usuario ADD COLUMN IF NOT EXISTS \"TenantId\" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';"); } catch (Exception ex) { Console.WriteLine("Patch TenantId usuario: " + ex.Message); }
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE producto ADD COLUMN IF NOT EXISTS \"TenantId\" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';"); } catch {}
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE pedido ADD COLUMN IF NOT EXISTS \"TenantId\" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';"); } catch {}

            // Asegurar que existe el rol ADMIN
            var adminRole = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Roles, r => r.nombre_rol == "ADMIN");
            if (adminRole == null)
            {
                adminRole = new LuxuryCo.Database.Models.Rol { nombre_rol = "ADMIN", descripcion = "Administrador del sistema" };
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
            }

            // Asegurar roles adicionales para el ERP
            var rolesAdicionales = new[] { "VENDEDOR", "SUPERVISOR", "CLIENTE" };
            foreach (var rolStr in rolesAdicionales)
            {
                var existeRol = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(context.Roles, r => r.nombre_rol == rolStr);
                if (!existeRol)
                {
                    context.Roles.Add(new LuxuryCo.Database.Models.Rol { nombre_rol = rolStr, descripcion = $"Permisos de {rolStr}" });
                }
            }
            await context.SaveChangesAsync();

            // Asegurar que existe el usuario admin@luxuryco.com
            var adminEmail = "admin@luxuryco.com";
            var adminExists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(context.Usuarios, u => u.email == adminEmail);
            
            if (!adminExists)
            {
                // 1. Registrar en Supabase
                var session = await supabase.Auth.SignUp(adminEmail, "Admin123!");
                
                // 2. Insertar en nuestra base de datos local
                var adminUser = new LuxuryCo.Database.Models.Usuario
                {
                    nombre = "Super",
                    apellido = "Admin",
                    email = adminEmail,
                    password_hash = "SUPABASE_MANAGED",
                    telefono = "0000000000",
                    id_rol = adminRole.id_rol,
                    activo = true,
                    fecha_registro = DateTime.UtcNow,
                    two_factor_enabled = false
                };
                context.Usuarios.Add(adminUser);
                await context.SaveChangesAsync();
                Console.WriteLine("Usuario Administrador ('admin@luxuryco.com' / 'Admin123!') creado con éxito.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al sembrar usuario Admin o aplicar parches: " + ex.Message);
        }
    }
});

app.Run();
