using Microsoft.EntityFrameworkCore;
using LuxuryCo.Database.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Enable Legacy Timestamp Behavior to prevent DateTime Kind errors when saving to PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// DbContext configuration
builder.Services.AddDbContext<LuxuryCoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LuxuryCoDbConnection")));

// Supabase configuration
var supabaseUrl = builder.Configuration["Supabase:Url"] ?? string.Empty;
var supabaseKey = builder.Configuration["Supabase:Key"] ?? string.Empty;
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
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Data para Administrador y Patches de DB
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LuxuryCo.Database.Data.LuxuryCoDbContext>();
    var supabase = scope.ServiceProvider.GetRequiredService<Supabase.Client>();
    
    try 
    {
        // 0. Auto-patch robusto para añadir columna logo_url si no existe en Supabase
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "ALTER TABLE marca ADD COLUMN IF NOT EXISTS logo_url character varying(500) NULL;");

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

app.Run();
