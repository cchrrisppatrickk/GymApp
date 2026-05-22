using GymApp.Data;
using GymApp.Repositories;
using GymApp.Services;
using GymApp.Configuration;
// 1. AGREGAR ESTE NAMESPACE
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuración de BD (Ya la tienes)
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyección de Dependencias (Tus repositorios y servicios actuales)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPlaneRepository, PlaneRepository>();
builder.Services.AddScoped<IPlaneService, PlaneService>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<ICongelamientoRepository, CongelamientoRepository>();
builder.Services.AddScoped<ICongelamientoService, CongelamientoService>();
builder.Services.AddScoped<IMembresiaRepository, MembresiaRepository>();
builder.Services.AddScoped<IMembresiaService, MembresiaService>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
// Agrega el de Usuarios si falta
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IVentaService, VentaService>();

builder.Services.AddScoped<IPaseDiarioRepository, PaseDiarioRepository>();
builder.Services.AddScoped<IPaseDiarioService, PaseDiarioService>();

// En Program.cs
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IConfiguracionAlertaRepository, ConfiguracionAlertaRepository>();

// Jobs
builder.Services.AddScoped<NotificacionProgramadaJob>();
builder.Services.AddScoped<AlertaVencimientoJob>();

// ============================================================
// HANGFIRE
// ============================================================
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Webhook / n8n
builder.Services.AddHttpClient();
builder.Services.Configure<N8nSettings>(builder.Configuration.GetSection("N8nConfig"));
builder.Services.AddScoped<IWebhookService, WebhookService>();
// ============================================================
// 2. CONFIGURAR AUTENTICACIÓN (COOKIES)
// ============================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Si el usuario no tiene acceso, lo mandamos aquí
        options.LoginPath = "/Auth/Login";
        // Si el usuario intenta entrar a algo que su rol no permite
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        // Tiempo de vida de la cookie
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });
// ============================================================
// 4. CONFIGURAR AUTORIZACIÓN (POLÍTICAS Y CLAIMS)
// ============================================================
builder.Services.AddAuthorization(options => {
   // --- PAGOS ---
   options.AddPolicy("RequiereVerPagos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Pagos.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereAnularPagos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Pagos.Anular") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   // --- MEMBRESÍAS ---
   options.AddPolicy("RequiereVerMembresias", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Membresias.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarMembresias", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Membresias.Eliminar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   options.AddPolicy("RequiereEditarMembresias", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Membresias.Editar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- VENTAS ---
   options.AddPolicy("RequiereVerVentas", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Ventas.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarVentas", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Ventas.Anular") || context.User.HasClaim("Permiso", "Ventas.Eliminar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- PRODUCTOS ---
   options.AddPolicy("RequiereVerProductos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Productos.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarProductos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Productos.Eliminar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- PLANES ---
   options.AddPolicy("RequiereVerPlanes", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Planes.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarPlanes", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Planes.Eliminar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- TURNOS ---
   options.AddPolicy("RequiereVerTurnos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Turnos.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarTurnos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Turnos.Eliminar") || context.User.HasClaim("Permiso", "Turnos.Anular") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- CONGELAMIENTOS ---
   options.AddPolicy("RequiereVerCongelamientos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Congelamientos.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarCongelamientos", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Congelamientos.Eliminar") || context.User.HasClaim("Permiso", "Congelamientos.Anular") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- ACCESO ---
   options.AddPolicy("RequiereVerAcceso", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Acceso.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarAcceso", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Acceso.Eliminar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- PASES DIARIOS ---
   options.AddPolicy("RequiereVerPasesDiarios", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "PasesDiarios.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarPasesDiarios", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "PasesDiarios.Eliminar") || context.User.HasClaim("Permiso", "PasesDiarios.Anular") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- USUARIOS ---
   options.AddPolicy("RequiereVerUsuarios", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Usuarios.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarUsuarios", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Usuarios.Eliminar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- ROLES ---
   options.AddPolicy("RequiereVerRoles", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Roles.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
       
   options.AddPolicy("RequiereEliminarRoles", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Roles.Eliminar") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));

   // --- DASHBOARD ---
   options.AddPolicy("RequiereVerDashboard", policy => 
       policy.RequireAssertion(context => context.User.HasClaim("Permiso", "Dashboard.Ver") || context.User.HasClaim("Permiso", "AdminAccesoTotal") || context.User.IsInRole("Admin")));
});
// ============================================================

var app = builder.Build();

// ============================================================
// INICIO DEL DATA SEEDING (SEMBRADO DE DATOS)
// ============================================================
// Esto se ejecuta una sola vez al levantar la app. 
// Verifica si la BD está vacía y la llena.
await DbSeeder.Seed(app);
// ============================================================


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ============================================================
// HANGFIRE DASHBOARD Y RECURRING JOB
// ============================================================
app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<NotificacionProgramadaJob>(
    "notificacion-programada",
    job => job.EjecutarRevisionAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<AlertaVencimientoJob>(
    "AvisoVencimientos",
    job => job.EjecutarAlertasAsync(),
    Cron.Hourly);

// ============================================================
// 3. ACTIVAR EL MIDDLEWARE (¡IMPORTANTE EL ORDEN!)
// ============================================================
app.UseAuthentication(); // <--- Debe ir ANTES de Authorization
app.UseAuthorization();
// ============================================================

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rutas convencionales (MVC)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Rutas de Atributos (Web API)
app.MapControllers();

app.Run();



//Scaffold - DbContext "Server=DESKTOP-6U3IQMJ\SQLEXPRESS;Database=GymDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer - OutputDir Models - ContextDir Data - Context GymDbContext - Force